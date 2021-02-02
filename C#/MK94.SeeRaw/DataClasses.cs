using MK94.SeeRaw.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace MK94.SeeRaw
{
    public class RenderRoot : INotifyPropertyChanged
	{
		public List<RenderTarget> Targets { get; } = new List<RenderTarget>();

		public event PropertyChangedEventHandler PropertyChanged = delegate { };

		public RenderTarget Render(object obj, string name = null)
        {
			if(name != null)
            {
				var target = Targets.FirstOrDefault(x => x.Name == name);

				if (target != null)
				{
					target.Value = obj;
					return target;
				}
            }

			var ret = new RenderTarget(obj, name ?? Guid.NewGuid().ToString());
			Targets.Add(ret);

			PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Targets)));

			return ret;
        }
	}

	public class RenderTarget : INotifyPropertyChanged
	{
		public string Name { get; }
		private object value;
		internal bool disposed = false;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public object Value
		{
			get => value;
			set
			{
				if(disposed)
					throw new ObjectDisposedException($"{nameof(RenderTarget)} '{Name}' was disposed and its value cannot be updated");

				this.value = value;
				PropertyChanged.Invoke(this, null);
			}
		}

		public RenderTarget(object obj, string name)
		{
			this.Name = name;

			value = obj;
		}

		public void Refresh()
		{
			if (disposed)
				throw new ObjectDisposedException($"{nameof(RenderTarget)} '{Name}' was disposed and cannot be refreshed");

			PropertyChanged.Invoke(this, null);
		}

        public void Serialize(Serializer serializer, Utf8JsonWriter writer, SerializerContext context, bool serializeNulls)
		{
			//serializer.Serialize(Value, Value.GetType(), serializeNulls, writer, context);
		}
    }

	[JsonConverter(typeof(Form.Serializer))]
	[MetadataConverter(typeof(Form.Serializer))]
	public class Form
	{
        private class Serializer : JsonConverter<Form>, IMetadataConverter
        {
            public override Form Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }

            public override void Write(Utf8JsonWriter writer, Form value, JsonSerializerOptions options)
            {
				writer.WriteStartObject();
				writer.WriteString("text", value.Text);

				writer.WriteStartArray("inputs");
				foreach (var formValue in value.FormValues)
				{
					writer.WriteStartObject();

					writer.WriteString(formValue.Name, formValue.Default.ToString());

					writer.WriteEndObject();
				}
				writer.WriteEndArray();
				writer.WriteEndObject();
			}

            public void Write(MetadataSerializer serializer, Utf8JsonWriter writer, object value, IEnumerable<string> valuePath, RendererContext context)
			{
				var form = value as Form;
				var id = Guid.NewGuid().ToString();

				writer.WriteString("id", id);

				if (!form.FormValues.Any())
				{
					writer.WriteString("type", "link");
					context.RegisterCallback(id, (Action)(() => form.callback(new Dictionary<string, object>())));
					return;
				}

				context.RegisterCallback(id, form.callback);
				writer.WriteString("type", "form");

				writer.WriteStartArray("inputs");
				foreach (var formValue in form.FormValues)
				{
					writer.WriteStartObject();

					// TODO add support for objects
					writer.WriteString("type", GetTypeName(formValue.Type));
					writer.WriteString("name", formValue.Name);

					writer.WriteEndObject();
				}
				writer.WriteEndArray();

			}

			private static Dictionary<Type, string> typeNameLookup = new Dictionary<Type, string>
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
				{ typeof(bool), "bool" },				
			};

			private string GetTypeName(Type type)
            {
				if (typeNameLookup.TryGetValue(type, out var ret))
					return ret;

				if (type.IsEnum)
					return "enum";

				throw new ArgumentException($"Expecting primitive type, got: {type.FullName}");
            }
        }

        private class FormValue
		{
			public string Name;
			public Type Type;
			public object Default;
		}

		private Action<Dictionary<string, object>> callback;
		private List<FormValue> FormValues = new List<FormValue>();

		public string Text { get; }

		public Form(string text, Action<Dictionary<string, object>> callback)
		{
			Text = text;
			this.callback = callback;
		}

		public Form WithInput<T>(string name, T @default = default)
		{
			FormValues.Add(new FormValue { Name = name, Type = typeof(T), Default = @default });

			return this;
		}
	}

	public class Actionable
	{
        private class Serializer : JsonConverter<Actionable>, IMetadataConverter
        {
            public override Actionable Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }

            public override void Write(Utf8JsonWriter writer, Actionable value, JsonSerializerOptions options)
            {
				writer.WriteStartObject();

				writer.WriteString("text", value.Text);
				/*
				foreach(var args in value.defaultArgs)
					writer.WriteString
				*/
				writer.WriteEndObject();
            }

            public void Write(MetadataSerializer serializer, Utf8JsonWriter writer, object value, IEnumerable<string> valuePath, RendererContext context)
            {
                throw new NotImplementedException();
            }
        }


        private Delegate action { get; }
		private List<object> defaultArgs { get; }

		public string Text { get; }

		public Actionable(string text, Delegate action, params object[] defaultArgs)
		{
			this.action = action;
			this.defaultArgs = defaultArgs.ToList();
			Text = text;
		}
		/*
        public void Serialize(Serializer serializer, Utf8JsonWriter writer, RendererContext context, bool serializeNulls)
        {
			writer.WriteString("text", Text);

			var id = Guid.NewGuid().ToString();

			context.Callbacks.Add(id, action);
			writer.WriteString("id", id);

			if (action is Action)
			{
				writer.WriteString("type", "link");

				return;
			}

			AppendDelegate(serializer, writer);
        }

		private void AppendDelegate(Serializer serializer, Utf8JsonWriter writer)
		{
			writer.WriteString("type", "form");

			writer.WriteStartArray("inputs");
			foreach (var (parameter, i) in action.GetMethodInfo().GetParameters().Select((p, i) => (p, i)))
			{
				if (!parameter.ParameterType.IsPrimitive && parameter.ParameterType != typeof(string) && !parameter.ParameterType.IsNested)
					throw new InvalidOperationException($"Delegate can only have primitive arguments");

				writer.WriteStartObject();

				writer.WriteString("name", parameter.Name);
				serializer.Serialize(defaultArgs?[i], parameter.ParameterType, true, writer, null);

				writer.WriteEndObject();
			}
			writer.WriteEndArray();
		}*/
    }

	[JsonConverter(typeof(Progress.Serializer))]
	[MetadataConverter(typeof(Progress.Serializer))]
	public class Progress : INotifyPropertyChanged
    {
        private class Serializer : JsonConverter<Progress>, IMetadataConverter
        {
            public override Progress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();

            public void Write(MetadataSerializer serializer, Utf8JsonWriter writer, object value, IEnumerable<string> valuePath, RendererContext context)
            {
				var prog = value as Progress;

				writer.WriteString("type", "progress");

				if (prog.pauseToggle != null)
				{
					var id = Guid.NewGuid().ToString();

					context.Callbacks.Add(id, prog.pauseToggle);
					writer.WriteString("pause", id);
				}
				else
					writer.WriteNull("pause");

				if (prog.setSpeed != null)
				{
					var id = Guid.NewGuid().ToString();

					context.Callbacks.Add(id, prog.setSpeed);
					writer.WriteString("setSpeed", id);
				}
				else
					writer.WriteNull("setSpeed");

				if (prog.cancellationTokenSource != null)
				{
					var id = Guid.NewGuid().ToString();

					context.Callbacks.Add(id, (Action)prog.cancellationTokenSource.Cancel);
					writer.WriteString("cancel", id);
				}
				else
					writer.WriteNull("cancel");
			}

            public override void Write(Utf8JsonWriter writer, Progress value, JsonSerializerOptions options)
            {
				writer.WriteStartObject();

				writer.WriteNumber("percent", value.Percent);
				writer.WriteString("value", value.Value);
				writer.WriteString("max", value.Max);
				writer.WriteString("min", value.Min);

				if (value.Speed != null)
					writer.WriteString("speed", value.Speed);
				else
					writer.WriteNull("speed");

				if (value.pauseToggle != null)
					writer.WriteBoolean("paused", value.paused);
				else
					writer.WriteNull("paused");

				writer.WriteEndObject();
			}
        }

        private int _percent;
		private string _value;
		private string _speed;
		private string _min;
		private string _max;

		private bool paused { get; set; }

		/// <summary>
		/// Value from 0 to 100; Changes the progress bar
		/// </summary>
		public int Percent { get => _percent; set { _percent = value; OnPropertyChanged(); } }

        /// <summary>
        /// The value to display on the bar itself. If empty Percent is used.
        /// Useful if the steps of the operations can be named (e.g. Initialising Job, Copying files, Cleaning up)
        /// </summary>
        public string Value { get => _value; set { _value = value; OnPropertyChanged(); } }

		/// <summary>
		/// An optional speed message (e.g. 100kb/s, 5m/s)
		/// </summary>
		public string Speed { get => _speed; set { _speed = value; OnPropertyChanged(); } }

		public string Min { get => _min; set { _min = value; OnPropertyChanged(); } }

		public string Max { get => _max; set { _max = value; OnPropertyChanged(); } }

		internal Action pauseToggle { get; }

		internal Action<int?> setSpeed { get; }

		internal CancellationTokenSource cancellationTokenSource { get; }

		public event PropertyChangedEventHandler PropertyChanged;

		public Progress(Action<bool> pause = null, Action<int?> setSpeed = null, CancellationTokenSource cancellationTokenSource = null)
		{
			this.setSpeed = setSpeed;
			this.cancellationTokenSource = cancellationTokenSource;

			if (pause != null)
				pauseToggle = () =>
				{
					paused = !paused;
					pause(paused);
				};

        }

		protected void OnPropertyChanged([CallerMemberName] string name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
    }

	[SeeRawType("navigation")]
    public class Navigation
    {
		public List<Actionable> Actions { get; } = new List<Actionable>();

		public Navigation WithAction(string text, Action action)
        {
			Actions.Add(new Actionable(text, action));
			return this;
        }
    }

	[SeeRawType("horizontal")]
	public class HorizontalRun
    {
		public List<object> Objects { get; set; }
    }

	[SeeRawType("vertical")]
	public class VerticalRun
	{
		public List<object> Objects { get; set; }
	}

	[JsonConverter(typeof(Logger.Serializer))]
	[MetadataConverter(typeof(Logger.Serializer))]
	public class Logger : ILogger, INotifyPropertyChanged
	{
        public class Serializer : JsonConverter<Logger>, IMetadataConverter
        {
			public override Logger Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();

            public void Write(MetadataSerializer serializer, Utf8JsonWriter writer, object value, IEnumerable<string> valuePath, RendererContext context)
            {
				writer.WriteString("type", "log");
            }

            public override void Write(Utf8JsonWriter writer, Logger value, JsonSerializerOptions options)
            {
				writer.WriteStartObject();

				writer.WriteString("message", value.Message);

				writer.WriteStartArray("children");

				foreach (var child in value.Children)
					Write(writer, child as Logger, options);

				writer.WriteEndArray();

				writer.WriteEndObject();
            }
        }

        public string Message { get; set; }

		public List<ILogger> Children { get; } = new List<ILogger>();

		private Logger parent;

		public event PropertyChangedEventHandler PropertyChanged;

		public ILogger CreateChild(string message = null)
		{
			var ret = new Logger
			{
				Message = message,
				parent = this
			};

			Children.Add(ret);

			return ret;
		}

		public IEnumerable<T> EnumerateWithProgress<T>(List<T> collection)
		{
			for (int i = 0; i < collection.Count; i++)
			{
				WithMessage($"{i + 1} of {collection.Count}");
				yield return collection[i];
			}
		}

		public void RemoveSelf()
		{
			parent?.Children.Remove(this);
		}

		public ILogger WithMessage(string messaage)
		{
			Console.WriteLine(messaage);
			Message = messaage;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
			return this;
		}

		public void Dispose()
		{
			RemoveSelf();
		}
	}
}
