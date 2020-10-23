using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace MK94.SeeRaw
{
	public class Serializer
	{
		internal Dictionary<Type, ISerialize> serializers = new Dictionary<Type, ISerialize>();

		internal ArraySegment<byte> SerializeState(RenderRoot state, Dictionary<string, Delegate> callbacks)
		{
			var memStream = new MemoryStream();
			var writer = new Utf8JsonWriter(memStream);
			writer.WriteStartObject();

			Serialize(state, typeof(RenderRoot), false, writer, callbacks);

			writer.WriteEndObject();
			writer.Flush();

			return new ArraySegment<byte>(memStream.GetBuffer(), 0, (int)memStream.Position);
		}

		public void Serialize(object obj, Type type, bool serializeNulls, Utf8JsonWriter writer, Dictionary<string, Delegate> callbacks)
		{
			if (serializers.TryGetValue(type, out var globalSerializer))
			{
				globalSerializer.Serialize(obj, this, writer, callbacks, serializeNulls);
				return;
			}

			if (obj is ISerializeable serializeable)
			{
				serializeable.Serialize(this, writer, callbacks, serializeNulls);
				return;
			}

			if (obj == null && !serializeNulls)
			{
				writer.WriteString("type", "null");
				writer.WriteNull("target");
			}

			else if (type == typeof(bool))
			{
				writer.WriteString("type", "bool");
				writer.WriteBoolean("target", (bool)obj);
			}

			else if (IsNumericalType(type))
			{
				writer.WriteString("type", "number");

				var writeNumber = typeof(Utf8JsonWriter).GetMethod(nameof(Utf8JsonWriter.WriteNumber), new[] { typeof(string), type });
				writeNumber.Invoke(writer, new[] { "target", obj });
			}

			else if (type == typeof(string))
			{
				writer.WriteString("type", "string");
				writer.WriteString("target", (string)obj);
			}

			else if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                SerializeArrayLike(obj, serializeNulls, writer, callbacks);
            }

            else if (obj is RenderRoot root)
			{
				writer.WriteStartArray("targets");

				foreach (var target in root.Targets)
				{
					writer.WriteStartObject();
					Serialize(target, typeof(RenderTarget), false, writer, callbacks);
					writer.WriteEndObject();
				}

				writer.WriteEndArray();
			}

			else if (obj is RenderTarget target)
			{
				Serialize(target.Value, target.Value.GetType(), serializeNulls, writer, callbacks);
			}

			else
			{
				writer.WriteString("type", "object");
				writer.WriteStartObject("target");

				foreach (var prop in obj.GetType().GetProperties())
				{
					writer.WriteStartObject(prop.Name);

					Serialize(prop.GetValue(obj), prop.PropertyType, serializeNulls, writer, callbacks);
					writer.WriteEndObject();
				}

				writer.WriteEndObject();
			}
		}

        private void SerializeArrayLike(object obj, bool serializeNulls, Utf8JsonWriter writer, Dictionary<string, Delegate> callbacks)
        {
            writer.WriteString("type", "array");
            writer.WriteStartArray("target");

            foreach (var elm in (IEnumerable)obj)
            {
                writer.WriteStartObject();
                Serialize(elm, elm.GetType(), serializeNulls, writer, callbacks);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
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
		public void Serialize(Serializer serializer, Utf8JsonWriter writer, Dictionary<string, Delegate> callbacks, bool serializeNulls);
    }

	public interface ISerialize
    {
		/// <summary>
		/// Serializes the object into json for the client. Has to include a "type" property so the client knows which renderer to pick.
		/// </summary>
		/// <param name="instance">The object being serialized</param>
		/// <param name="serializer">The default serializer. Call <see cref="Serializer.Serialize(object, Type, bool, Utf8JsonWriter, Dictionary{string, Delegate})"/> to append the default json properties</param>
		/// <param name="writer">The json writer used to create the message</param>
		/// <param name="callbacks">Messages that the client can send back to us. <br /> The key is used to identify which callback the client wants to execute and should be a random guid in most cases.</param>
		/// <param name="serializeNulls">Useful for UI elements that need to know the data type even if its empty. <br />
		/// E.g. allows Actionable to render a form with parameter inputs. <br />
		/// For custom UI Renderers you can safely ignore this one</param>
		public void Serialize(object instance, Serializer serializer, Utf8JsonWriter writer, Dictionary<string, Delegate> callbacks, bool serializeNulls);
	}
}
