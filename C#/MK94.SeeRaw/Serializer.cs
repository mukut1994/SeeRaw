using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MK94.SeeRaw
{
	public interface ISerializerConfigure
	{
		ISerializerConfigure WithConverter(JsonConverter converter);
	}

	public class SerializerContext
	{
		public HashSet<Type> SeenTypes = new HashSet<Type>();
	}

	public class Serializer : ISerializerConfigure
	{
		private static string SanitizePath(string path)
        {
			return path.Split(new[] { '.', '[', ']' }, StringSplitOptions.RemoveEmptyEntries).Skip(1).Aggregate("$", (a, b) => $"{a}.{b}");
		}

		#region helper classes
		private class ContractResolver : DefaultContractResolver
		{
			private class PathHookConverter : JsonConverter
			{
				JsonConverter? actualConverter;
				ContractResolver cr;
				Type t;

				public PathHookConverter(Type t, JsonConverter? b, ContractResolver cr)
				{
					this.actualConverter = b;
					this.t = t;
					this.cr = cr;
				}

				public override bool CanConvert(Type objectType) => throw new NotImplementedException();
				public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) => throw new NotImplementedException();

				public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
				{
					if (actualConverter != null)
						actualConverter.WriteJson(writer, value, serializer);
					else
						writer.WriteValue(value);

					cr.tables.Add(SanitizePath(writer.Path), new ReferenceTable { Path = writer.Path, Type = t });
				}
			}

			public Dictionary<string, ReferenceTable> tables;
			public JsonWriter wr;

			Stack<string> path;
			public HashSet<Type> knownTypes = new HashSet<Type>();

			protected override JsonContract CreateContract(Type objectType)
			{
				var x = base.CreateContract(objectType);

				knownTypes.Add(objectType);

				if (x is JsonPrimitiveContract)
				{
					x.Converter = new PathHookConverter(objectType, x.Converter ?? x.InternalConverter, this);
				}

				x.OnSerializedCallbacks.Add((o, context) => { addRef(objectType); });

				return x;
			}

			void addRef(Type t)
			{
				var index = wr.Path.LastIndexOf('.');

				if (index > -1)
				{
					var a = wr.Path.Substring(0, index);
					if (tables.TryGetValue(a, out var x))
						x.Children.Add(wr.Path);
				}

				var path = SanitizePath(wr.Path);

				if (tables.TryGetValue(path, out var r))
					r.Type = t;
				else
					tables.Add(path, new ReferenceTable { Path = path, Type = t });
			}
		}

		private class Ref : IReferenceResolver
		{
			public JsonWriter wr;
			public ConditionalWeakTable<object, string> x = new ConditionalWeakTable<object, string>();
			int counter = 0;

			public Dictionary<string, ReferenceTable> tables;

			public void AddReference(object context, string reference, object value)
			{
				throw new NotImplementedException();
			}

			public string GetReference(object context, object value)
			{
				if (x.TryGetValue(value, out var v))
					return v;

				counter++;
				var r = new ReferenceTable { ID = counter.ToString(), Path = wr.Path };
				tables.Add(SanitizePath(wr.Path), r);
				x.Add(value, counter.ToString());
				return counter.ToString();
			}

			public bool IsReferenced(object context, object value)
			{
				return x.TryGetValue(value, out _);
			}

			public object ResolveReference(object context, string reference)
			{
				return null;
			}
		}

		private class ReferenceTable
		{
			public string Path;
			public string ID;
			public Type Type;

			public List<string> Children = new List<string>();
		}
        #endregion

        private static ContractResolver cr = new ContractResolver();

		public ISerializerConfigure WithConverter(JsonConverter converter)
		{
			// TODO
			return this;
		}

		public ISerializerConfigure WithMetadata(Type type, object x)
        {

			return this;
        }

		public void Serialize(object obj, Stream stream, RendererContext renderContext, SerializerContext context)
		{
			lock (cr)
			{
				var re = new Ref();
				var wr = new JsonTextWriter(new StreamWriter(stream));
				var t = new Dictionary<string, ReferenceTable>();

				var s = new JsonSerializerSettings
				{
					ContractResolver = cr,
					ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
					PreserveReferencesHandling = PreserveReferencesHandling.Objects,
					Formatting = Formatting.Indented,
					ReferenceResolverProvider = () => re,
				};

				cr.wr = wr;
				re.wr = wr;
				cr.tables = t;
				re.tables = t;

				wr.Formatting = Formatting.Indented;

				wr.WriteStartObject();

				wr.WritePropertyName("kind");
				wr.WriteValue(0);
				wr.WritePropertyName("id");
				wr.WriteValue("1");

				WriteValue(wr, s, obj);
				WriteMeta(wr, t, context);
				WriteLinks(wr, t);

				wr.WriteEndObject();
				wr.Flush();
			}
		}

		private void WriteValue(JsonWriter wr, JsonSerializerSettings s, object obj)
		{
			wr.WritePropertyName("value");

			JsonSerializer.Create(s).Serialize(wr, obj);
		}

		private void WriteMeta(JsonWriter wr, Dictionary<string, ReferenceTable> t, SerializerContext context)
		{
			wr.WritePropertyName("meta");

			wr.WriteStartObject();
			foreach (var path in t)
			{
				if (path.Value.Type == typeof(Guid)) 
					continue;
				if (path.Value.Type == typeof(DateTime))
					continue;


				if (context.SeenTypes.Contains(path.Value.Type))
					continue;

				context.SeenTypes.Add(path.Value.Type);

				wr.WritePropertyName(path.Value.Type.Name);

				wr.WriteStartObject();

				wr.WritePropertyName("type");
				wr.WriteValue(SimpleType(path.Value.Type));
				wr.WritePropertyName("extendedType");
				wr.WriteValue(path.Value.Type.Name);

				if (path.Value.Children.Any())
				{
					wr.WritePropertyName("children");
					wr.WriteStartObject();
					foreach (var c in path.Value.Children)
					{
						wr.WritePropertyName(c);
						wr.WriteValue(t[c].Type.Name);
					}
					wr.WriteEndObject();
				}
				else if(path.Value.Type.IsEnum)
                {
					wr.WritePropertyName("values");
					wr.WriteStartArray();
					foreach (var e in path.Value.Type.GetEnumNames())
					{
						wr.WriteValue(e);
					}
					wr.WriteEndArray();
				}

				wr.WriteEndObject();
			}
			wr.WriteEndObject();
		}

		private static Dictionary<Type, string> simpleTypesLookup = new Dictionary<Type, string>()
	{
		{ typeof(sbyte), "number" },
		{ typeof(byte), "number" },
		{ typeof(short), "number" },
		{ typeof(ushort), "number" },
		{ typeof(int), "number" },
		{ typeof(uint), "number" },
		{ typeof(long), "number" },
		{ typeof(ulong), "number" },
		{ typeof(float), "number" },
		{ typeof(double), "number" },
		{ typeof(decimal), "number" },

		{ typeof(string), "string" },
		{ typeof(bool), "bool" }
	};

		private string SimpleType(Type type)
		{
			if (simpleTypesLookup.TryGetValue(type, out var ret))
				return ret;

			if (type.IsEnum)
				return "enum";

			if (typeof(IEnumerable).IsAssignableFrom(type))
				return "array";

			return "object";
		}

		private void WriteLinks(JsonWriter wr, Dictionary<string, ReferenceTable> t)
		{
			wr.WritePropertyName("links");

			wr.WriteStartObject();
			// TODO potential optimization here
			// not every path needs to be linked, most props can be derived from the metadata (the only exception is when classes get extended)
			foreach (var path in t)
			{
				if (path.Value.ID != null)
					wr.WritePropertyName($"${path.Value.ID}");
				else
					wr.WritePropertyName(path.Key);

				wr.WriteValue(path.Value.Type.Name);
			}
			wr.WriteEndObject();
		}

    }
}
