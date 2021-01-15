using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace MK94.SeeRaw
{
	public class SerializerContext
    {
		public Dictionary<string, Delegate> Callbacks { get; } = new Dictionary<string, Delegate>();
		private List<INotifyPropertyChanged> PropertyChangedNotifiers { get; } = new List<INotifyPropertyChanged>();

		internal PropertyChangedEventHandler onPropertyChanged;

        public void AddPropertyChangedNotifier(INotifyPropertyChanged notifyPropertyChanged)
        {
			PropertyChangedNotifiers.Add(notifyPropertyChanged);
			notifyPropertyChanged.PropertyChanged += onPropertyChanged;
		}
		
		public void ClearPropertyChangedHandlers()
        {
			foreach(var p in PropertyChangedNotifiers)
            {
				p.PropertyChanged -= onPropertyChanged;
            }
        }
	}

	public class Serializer
	{
		internal Dictionary<Type, ISerialize> serializers = new Dictionary<Type, ISerialize>();

		public ArraySegment<byte> SerializeState(RenderRoot state, SerializerContext context, JsonWriterOptions options = default)
		{
			var memStream = new MemoryStream();
			var writer = new Utf8JsonWriter(memStream, options);
			writer.WriteStartObject();

			Serialize(state, typeof(RenderRoot), false, writer, context);

			writer.WriteEndObject();
			writer.Flush();

			return new ArraySegment<byte>(memStream.GetBuffer(), 0, (int)memStream.Position);
		}

		public void Serialize(object obj, Type type, bool serializeNulls, Utf8JsonWriter writer, SerializerContext context)
		{
			if (obj is INotifyPropertyChanged notify)
				context.AddPropertyChangedNotifier(notify);

			if (serializers.TryGetValue(type, out var globalSerializer))
				globalSerializer.Serialize(obj, this, writer, context, serializeNulls);

			else if (obj is ISerializeable serializeable)
				serializeable.Serialize(this, writer, context, serializeNulls);

			else if (obj == null && !serializeNulls)
			{
				writer.WriteString("$type", "null");
				writer.WriteNull("target");
			}

			else if (type == typeof(bool))
			{
				writer.WriteString("$type", "bool");
				writer.WriteBoolean("target", (bool)obj);
			}

			else if (IsNumericalType(type))
			{
				writer.WriteString("$type", "number");

				var writeNumber = typeof(Utf8JsonWriter).GetMethod(nameof(Utf8JsonWriter.WriteNumber), new[] { typeof(string), type });
				writeNumber.Invoke(writer, new[] { "target", obj });
			}

			else if (type == typeof(string))
			{
				writer.WriteString("$type", "string");
				writer.WriteString("target", (string)obj);
			}

			else if (type.IsEnum)
				SerializeEnum(obj, type, writer);

			else if (typeof(IEnumerable).IsAssignableFrom(type))
				SerializeArrayLike(obj, serializeNulls, writer, context);

			else if (obj is RenderRoot root)
			{
				writer.WriteStartArray("targets");

				foreach (var target in root.Targets)
				{
					writer.WriteStartObject();
					Serialize(target, typeof(RenderTarget), false, writer, context);
					writer.WriteEndObject();
				}

				writer.WriteEndArray();
			}

			else
			{
				var typeName = obj.GetType().GetCustomAttribute<SeeRawTypeAttribute>()?.Name ?? "object";

				writer.WriteString("$type", typeName);
				foreach (var prop in obj.GetType().GetProperties())
				{
					writer.WriteStartObject(prop.Name);

					var value = prop.GetValue(obj);

					Serialize(value, value?.GetType() ?? prop.PropertyType, serializeNulls, writer, context);
					writer.WriteEndObject();
				}
			}
		}

        private void SerializeArrayLike(object obj, bool serializeNulls, Utf8JsonWriter writer, SerializerContext context)
        {
            writer.WriteString("$type", "array");
            writer.WriteStartArray("target");

            foreach (var elm in (IEnumerable)obj)
            {
                writer.WriteStartObject();
                Serialize(elm, elm.GetType(), serializeNulls, writer, context);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }

		private void SerializeEnum(object obj, Type type, Utf8JsonWriter writer)
        {
			var enumNames = type.GetEnumNames().Aggregate((a, b) => $"{a}, {b}");
			
			writer.WriteString("$type", $"enum");
			writer.WriteString("enum-values", $"{enumNames}");
			writer.WriteString("target", obj?.ToString());
        }

        private bool IsNumericalType(Type t)
		{
			return t == typeof(sbyte) ||
					t == typeof(byte) ||
					t == typeof(short) ||
					t == typeof(ushort) ||
					t == typeof(int) ||
					t == typeof(uint) ||
					t == typeof(long) ||
					t == typeof(ulong) ||
					t == typeof(float) ||
					t == typeof(decimal);
		}
	}

	public interface ISerializeable
    {
		/// <summary>
		/// Serializes the object into json for the client. Has to include a "type" property so the client knows which renderer to pick.
		/// </summary>
		/// <param name="serializer">The default serializer. Call <see cref="Serializer.Serialize(object, Type, bool, Utf8JsonWriter, Dictionary{string, Delegate})"/> to append the default json properties</param>
		/// <param name="writer">The json writer used to create the message</param>
		/// <param name="callbacks">Messages that the client can send back to us. <br /> The key is used to identify which callback the client wants to execute and should be a random guid in most cases.</param>
		/// <param name="serializeNulls">Useful for UI elements that need to know the data type even if its empty. <br />
		/// E.g. allows Actionable to render a form with parameter inputs. <br />
		/// For custom UI Renderers you can safely ignore this one</param>
		public void Serialize(Serializer serializer, Utf8JsonWriter writer, SerializerContext context, bool serializeNulls);
    }

	public interface ISerialize
    {
		/// <summary>
		/// Serializes the object into json for the client. Has to include a "type" property so the client knows which renderer to pick.
		/// </summary>
		/// <param name="instance">The object being serialized</param>
		/// <param name="serializer">The default serializer. Call <see cref="Serializer.Serialize(object, Type, bool, Utf8JsonWriter, Dictionary{string, Delegate})"/> to append the default json properties</param>
		/// <param name="writer">The json writer used to create the message</param>
		/// <param name="notifyProperties">The list of objects that can notify</param>
		/// <param name="callbacks">Messages that the client can send back to us. <br /> The key is used to identify which callback the client wants to execute and should be a random guid in most cases.</param>
		/// <param name="serializeNulls">Useful for UI elements that need to know the data type even if its empty. <br />
		/// E.g. allows Actionable to render a form with parameter inputs. <br />
		/// For custom UI Renderers you can safely ignore this one</param>
		public void Serialize(object instance, Serializer serializer, Utf8JsonWriter writer, SerializerContext context, bool serializeNulls);
	}

    public class DateTimeSerializer : ISerialize
    {
        public void Serialize(object instance, Serializer serializer, Utf8JsonWriter writer, SerializerContext context, bool serializeNulls)
        {
			writer.WriteString("$type", "string");
			writer.WriteString("target", ((DateTime)instance).ToString());
        }
    }
}
