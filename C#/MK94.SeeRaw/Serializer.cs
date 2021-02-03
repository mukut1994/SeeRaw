using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MK94.SeeRaw
{
	public class SerializerContext
    {
		public Utf8JsonWriter Main;
    }
	
	public class MetadataSerializer
	{
		internal List<IMetadataConverter> converters = new List<IMetadataConverter>();
		private static MetadataConverter DefaultConverter = new MetadataConverter();

		public void Serialize(Utf8JsonWriter writer, object? value, IEnumerable<string> valuePath, RendererContext context)
		{
			try
			{
				var converter = GetConverter(value?.GetType() ?? typeof(object));

				converter.Write(this, writer, value, valuePath, context);
			}
			catch (Exception e)
			{
				throw new Exception($"Failed to serialize at path {MetadataConverter.GetFullPath(valuePath)}", e);
			}
		}

		private IMetadataConverter GetConverter(Type type)
		{
			var converter = converters.FirstOrDefault(x => x.CanConvert(type));

			if (converter != null)
				return converter;

			var attr = type.GetCustomAttribute<MetadataConverterAttribute>();

			if (attr != null)
				return (IMetadataConverter) Activator.CreateInstance(attr.ConverterType);

			return DefaultConverter;
		}
	}

	// TODO add property suppport
	[AttributeUsage(AttributeTargets.Class)]
	public class MetadataConverterAttribute : Attribute
	{
		public Type ConverterType { get; }

		public MetadataConverterAttribute(Type converterType)
		{
			if (!typeof(IMetadataConverter).IsAssignableFrom(converterType))
				throw new InvalidProgramException($"Argument {nameof(converterType)} must inherit from {nameof(IMetadataConverter)}");

			ConverterType = converterType;
		}
	}

	public interface IMetadataConverter
    {
		bool CanConvert(Type type);

		void Write(MetadataSerializer serializer, Utf8JsonWriter writer, object? value, IEnumerable<string> valuePath, RendererContext context);

	}

	public class MetadataConverter : IMetadataConverter
	{
		public bool CanConvert(Type type) => true;

		public virtual void Write(MetadataSerializer serializer, Utf8JsonWriter writer, object? value, IEnumerable<string> valuePath, RendererContext context)
		{
			if (value == null)
			{
				writer.WriteString("type", "null");
				return;
			}

			switch (value)
			{
				case sbyte:
				case byte:
				case short:
				case ushort:
				case int:
				case uint:
				case long:
				case ulong:
				case float:
				case double:
				case decimal:
					writer.WriteString("type", "number");
					writer.WriteString("extendedType", GetFullName(value.GetType()));
					return;

				case string:
					writer.WriteString("type", "string");
					return;

				case bool:
					writer.WriteString("type", "bool");
					return;

				case IEnumerable ie:
					writer.WriteString("type", "array");
					writer.WriteString("extendedType", GetFullName(value.GetType()));

					writer.WriteStartArray("children");

					foreach (var elem in ie)
					{
						writer.WriteStartObject();
						serializer.Serialize(writer, elem, valuePath, context);
						writer.WriteEndObject();
					}

					writer.WriteEndArray();
					return;

			}

			var type = value.GetType();

			if (type.IsEnum)
			{
				var enumNames = type.GetEnumNames();

				writer.WriteString("type", $"enum");
				writer.WriteStartArray("values");

				foreach (var enumName in enumNames)
					writer.WriteStringValue(enumName);

				writer.WriteEndArray();
				return;
			}

			writer.WriteString("type", "object");
			writer.WriteString("extendedType", GetFullName(value.GetType()));

			writer.WriteStartObject("children");
			foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
			{
				writer.WriteStartObject(prop.Name);
				serializer.Serialize(writer, prop.GetValue(value), valuePath, context);
				writer.WriteEndObject();
			}
			writer.WriteEndObject();
		}

		protected internal static string GetFullPath(IEnumerable<string> path)
		{
			StringBuilder s = new StringBuilder();

			foreach (var p in path)
				s.Append(p);

			return s.ToString();
		}

		public static string GetFullName(Type t)
		{
			if (!t.IsGenericType)
				return t.Name;

			StringBuilder sb = new StringBuilder();

			sb.Append(t.Name.Substring(0, t.Name.LastIndexOf("`")));
			sb.Append(t.GetGenericArguments().Aggregate("<",
				(string aggregate, Type type) => aggregate + (aggregate == "<" ? "" : ",") + GetFullName(type)
				));
			sb.Append(">");

			return sb.ToString();
		}
	}

	public interface ISerializerConfigure
    {
		Serializer WithConverter(IMetadataConverter converter);

		Serializer WithConverter(JsonConverter converter);

		Serializer WithConverter<T>(T converter) where T : JsonConverter, IMetadataConverter;
	}

	public static class SerializerConfigureExtensions
    {
		/// <summary>
		/// Workaround helper for <see href="https://github.com/dotnet/runtime/issues/30524"/><br />
		/// Serializes the key as string (via <see cref="object.ToString"/>) and the value using the regular serialization pipeline
		/// </summary>
		public static ISerializerConfigure WithDictionaryConverter<TKey, TValue>(this ISerializerConfigure configure)
        {
			configure.WithConverter(new CommonDictionaryConverter<TKey, TValue>());
			return configure;
        }
    }

	public class Serializer : ISerializerConfigure
	{
		enum Kind
		{
			Full = 0,
			RemoveTarget = 1,
			Delta = 2,
			Download = 3
		}

		private ConcurrentBag<(MemoryStream stream, Utf8JsonWriter writer)> pool = new ConcurrentBag<(MemoryStream stream, Utf8JsonWriter writer)>();

		private JsonSerializerOptions valueSerializationOptions = new JsonSerializerOptions
		{
#if DEBUG
			WriteIndented = true
#endif
		};
		private static MetadataSerializer metadataSerializer = new MetadataSerializer();

		public Serializer()
        {
			valueSerializationOptions.Converters.Add(new JsonStringEnumConverter());
			metadataSerializer.converters.Add(new DateTimeSerializer());
		}

		// TODO serialization should be done in 1 pass
		// Using multiple passes gives a different thread time to modify the data midway
		// Because of that the Value, Metadata and callbacks could be out of sync
		public ArraySegment<byte> Serialize(string targetId, object instance, RendererContext rendererContext)
		{
			var (stream, writer) = RequestFromPool();

			writer.Reset();

			writer.WriteStartObject();

			writer.WriteNumber("kind", (int)Kind.Full);
			writer.WriteString("id", targetId);

			writer.WritePropertyName("value");
			JsonSerializer.Serialize(writer, instance, valueSerializationOptions);
			writer.WriteStartObject("metadata");
			metadataSerializer.Serialize(writer, instance, new[] { "$" }, rendererContext);
			writer.WriteEndObject();

			writer.WriteEndObject();

			writer.Flush();
			// TODO return to pool
			return new ArraySegment<byte>(stream.GetBuffer(), 0, (int)writer.BytesCommitted);
		}

		private (MemoryStream stream, Utf8JsonWriter writer) RequestFromPool()
        {
			if (pool.TryTake(out var instance))
				return instance;

			var stream = new MemoryStream();
			var writer = new Utf8JsonWriter(stream
#if DEBUG
		, new JsonWriterOptions { Indented = true }
#endif
		);

			return (stream, writer);
        }

		private void ReturnToPool(MemoryStream stream, Utf8JsonWriter writer)
        {
			pool.Add((stream, writer));
        }

		public Serializer WithConverter(IMetadataConverter converter)
		{
			metadataSerializer.converters.Add(converter);

			return this;
		}

		public Serializer WithConverter<T>(T converter) where T : JsonConverter, IMetadataConverter
		{
			metadataSerializer.converters.Add(converter);
			valueSerializationOptions.Converters.Add(converter);

			return this;
		}

		public Serializer WithConverter(JsonConverter converter)
		{
			valueSerializationOptions.Converters.Add(converter);

			return this;
		}
	}

    /// <summary>
    /// Workaround helper for https://github.com/dotnet/runtime/issues/30524
    /// </summary>
    /// <typeparam name="TKey">The type of the key</typeparam>
    public class CommonDictionaryConverter<TKey, TValue> : JsonConverter<Dictionary<TKey, TValue>>, IMetadataConverter
	{ 
		public override Dictionary<TKey, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();

		public override void Write(Utf8JsonWriter writer, Dictionary<TKey, TValue> value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();

			foreach (KeyValuePair<TKey, TValue> kvp in value)
			{
				writer.WritePropertyName(kvp.Key.ToString());
				JsonSerializer.Serialize(writer, kvp.Value, options);
			}

			writer.WriteEndObject();
		}

        public void Write(MetadataSerializer serializer, Utf8JsonWriter writer, object value, IEnumerable<string> valuePath, RendererContext context)
        {
			writer.WriteString("type", "object");
			writer.WriteString("extendedType", MetadataConverter.GetFullName(value.GetType()));

			var dict = value as Dictionary<TKey, TValue>;

			writer.WriteStartObject("children");

			foreach (var kv in dict)
			{
				writer.WriteStartObject(kv.Key.ToString());
				serializer.Serialize(writer, kv.Value, valuePath, context);
				writer.WriteEndObject();
			}

			writer.WriteEndObject();
		}
    }

    public class DateTimeSerializer : IMetadataConverter
	{
		public bool CanConvert(Type type) => type == typeof(DateTime) || type == typeof(DateTime?);

		public void Write(MetadataSerializer serializer, Utf8JsonWriter writer, object value, IEnumerable<string> valuePath, RendererContext context)
		{ 
			writer.WriteString("type", "datetime");
        }
    }
}
