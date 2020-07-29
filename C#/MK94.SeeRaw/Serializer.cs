using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace MK94.SeeRaw
{
	static class Serializer
	{
		public static void Serialize(object obj, Type type, bool serializeNulls, Utf8JsonWriter writer, Dictionary<string, Delegate> callbacks)
		{
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

			else if (obj is Actionable l)
				AppendLink(l, writer, callbacks);

			else if (obj is Delegate d)
				AppendDelegate(d, writer, callbacks);

			else if (obj is Progress p)
				AppendProgress(p, writer, callbacks);

			else
			{
				writer.WriteString("type", "object");
				writer.WriteStartObject("target");

				foreach ((PropertyInfo prop, int i) in obj.GetType().GetProperties().Select((x, i) => (x, i)))
				{
					writer.WriteStartObject(prop.Name);

					Serialize(prop.GetValue(obj), prop.PropertyType, serializeNulls, writer, callbacks);
					writer.WriteEndObject();
				}

				writer.WriteEndObject();
			}
		}

		private static void AppendLink(Actionable link, Utf8JsonWriter writer, Dictionary<string, Delegate> callbacks)
        {
			writer.WriteString("text", link.Text);

			var id = Guid.NewGuid().ToString();

			callbacks.Add(id, link.Action);
			writer.WriteString("id", id);

			if (link.Action is Action)
			{
				writer.WriteString("type", "link");

				return;
            }

			AppendDelegate(link.Action, writer, callbacks);
        }

		private static void AppendDelegate(Delegate @delegate, Utf8JsonWriter writer, Dictionary<string, Delegate> callbacks)
		{
			writer.WriteString("type", "form");

			writer.WriteStartArray("inputs");
			foreach (var parameter in @delegate.GetMethodInfo().GetParameters())
			{
				if (!parameter.ParameterType.IsPrimitive && parameter.ParameterType != typeof(string))
					throw new InvalidOperationException($"Delegate can only have primitive arguments");

				writer.WriteStartObject();

				writer.WriteString("name", parameter.Name);
				Serialize(null, parameter.ParameterType, true, writer, callbacks);

				writer.WriteEndObject();
			}
			writer.WriteEndArray();
		}

		private static void AppendProgress(Progress progress, Utf8JsonWriter writer, Dictionary<string, Delegate> callbacks)
        {
			writer.WriteString("type", "progress");

			writer.WriteNumber("percent", progress.Percent);
			writer.WriteString("value", progress.Value);
			writer.WriteString("max", progress.Max);
			writer.WriteString("min", progress.Min);

			if (progress.Speed != null)
				writer.WriteString("speed", progress.Speed);
			else 
				writer.WriteNull("speed");

			if (progress.pauseToggle != null)
			{
				var id = Guid.NewGuid().ToString();

				callbacks.Add(id, progress.pauseToggle);
				writer.WriteString("pause", id);
				writer.WriteBoolean("paused", progress.paused);
			}
			else
				writer.WriteNull("pause");

			if (progress.setSpeed != null)
			{
				var id = Guid.NewGuid().ToString();

				callbacks.Add(id, progress.setSpeed);
				writer.WriteString("setSpeed", id);
			}
			else
				writer.WriteNull("setSpeed"); 
			
			if (progress.cancellationTokenSource != null)
			{
				var id = Guid.NewGuid().ToString();

				callbacks.Add(id, (Action) progress.cancellationTokenSource.Cancel);
				writer.WriteString("cancel", id);
			}
			else
				writer.WriteNull("speed");

		}

		private static bool IsNumericalType(Type t)
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

}
