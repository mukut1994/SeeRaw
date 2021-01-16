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
		internal Dictionary<Type, IMetadataConverter> converters = new Dictionary<Type, IMetadataConverter>();
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
			if (converters.TryGetValue(type, out var converter))
				return converter;

			var attr = type.GetCustomAttribute<MetadataConverterAttribute>();

			if (attr != null)
				return (MetadataConverter) Activator.CreateInstance(attr.ConverterType);

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
			if (typeof(MetadataConverter).IsAssignableFrom(converterType))
				throw new InvalidProgramException($"Argument {nameof(converterType)} must inherit from {nameof(MetadataConverter)}");

			ConverterType = converterType;
		}
	}

	public interface IMetadataConverter
    {
		void Write(MetadataSerializer serializer, Utf8JsonWriter writer, object? value, IEnumerable<string> valuePath, RendererContext context);

	}

	public class MetadataConverter : IMetadataConverter
	{
		public virtual void Write(MetadataSerializer serializer, Utf8JsonWriter writer, object? value, IEnumerable<string> valuePath, RendererContext context)
		{
			if (value == null)
			{
				writer.WriteString("Type", "null");
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
					writer.WriteString("Type", "number");
					writer.WriteString("ExtendedType", GetFullName(value.GetType()));
					return;

				case string:
					writer.WriteString("Type", "string");
					return;

				case bool:
					writer.WriteString("Type", "bool");
					return;

				case IEnumerable ie:
					writer.WriteString("Type", "array");
					writer.WriteString("ExtendedType", GetFullName(value.GetType()));

					writer.WriteStartArray("Children");

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

				writer.WriteString("Type", $"enum");
				writer.WriteStartArray("Values");

				foreach (var enumName in enumNames)
					writer.WriteStringValue(enumName);

				writer.WriteEndArray();
			}

			writer.WriteString((string)"$type", GetFullName(value.GetType()));

			foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
			{
				writer.WriteStartObject(prop.Name);
				serializer.Serialize(writer, prop.GetValue(value), valuePath, context);
				writer.WriteEndObject();
			}
		}

		protected internal static string GetFullPath(IEnumerable<string> path)
		{
			StringBuilder s = new StringBuilder();

			foreach (var p in path)
				s.Append(p);

			return s.ToString();
		}

		protected static string GetFullName(Type t)
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

	public class Serializer
	{
		private ConcurrentBag<(MemoryStream stream, Utf8JsonWriter writer)> pool = new ConcurrentBag<(MemoryStream stream, Utf8JsonWriter writer)>();

		private JsonSerializerOptions valueSerializationOptions = new JsonSerializerOptions
		{
#if DEBUG
			WriteIndented = true
#endif
		};
		private static MetadataSerializer metadataSerializer = new MetadataSerializer();

		// TODO serialization should be done in 1 pass
		// Using multiple passes gives a different thread time to modify the data midway
		// Because of that the Value, Metadata and callbacks could be out of sync
		public ArraySegment<byte> Serialize(object instance, RendererContext rendererContext)
		{
			var (stream, writer) = RequestFromPool();

			writer.Reset();

			writer.WriteStartObject();

			writer.WriteString("Kind", "Full");

			writer.WritePropertyName("Value");
			JsonSerializer.Serialize(writer, instance, valueSerializationOptions);
			writer.WriteStartObject("Metadata");
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

		public Serializer WithMetadataConverter<T>(IMetadataConverter converter)
		{
			metadataSerializer.converters.Add(typeof(T), converter);

			return this;
		}

		public Serializer WithValueConverter(JsonConverter converter)
		{
			valueSerializationOptions.Converters.Add(converter);

			return this;
		}
	}

	/*
	public class DateTimeSerializer : ISerialize
    {
        public void Serialize(object instance, Serializer serializer, Utf8JsonWriter writer, RendererContext context, bool serializeNulls)
        {
			writer.WriteString("type", "string");
			writer.WriteString("target", ((DateTime)instance).ToString());
        }
    }*/
}
