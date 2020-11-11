using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;

namespace MK94.SeeRaw
{
    public class RenderRoot : INotifyPropertyChanged
	{
		internal List<RenderTarget> Targets { get; } = new List<RenderTarget>();

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
	}

	public class Actionable : ISerializeable
	{
		private Delegate action { get; }
		private List<object> defaultArgs { get; }

		public string Text { get; }

		public Actionable(string text, Delegate action, params object[] defaultArgs)
		{
			this.action = action;
			this.defaultArgs = defaultArgs.ToList();
			Text = text;
		}

        public void Serialize(Serializer serializer, Utf8JsonWriter writer, SerializerContext context, bool serializeNulls)
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
		}
    }

	public class Progress : ISerializeable, INotifyPropertyChanged
    {
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

		public void Serialize(Serializer serializer, Utf8JsonWriter writer, SerializerContext context, bool serializeNulls)
		{
			writer.WriteString("type", "progress");

			writer.WriteNumber("percent", Percent);
			writer.WriteString("value", Value);
			writer.WriteString("max", Max);
			writer.WriteString("min", Min);

			if (Speed != null)
				writer.WriteString("speed", Speed);
			else
				writer.WriteNull("speed");

			if (pauseToggle != null)
			{
				var id = Guid.NewGuid().ToString();

				context.Callbacks.Add(id, pauseToggle);
				writer.WriteString("pause", id);
				writer.WriteBoolean("paused", paused);
			}
			else
				writer.WriteNull("pause");

			if (setSpeed != null)
			{
				var id = Guid.NewGuid().ToString();

				context.Callbacks.Add(id, setSpeed);
				writer.WriteString("setSpeed", id);
			}
			else
				writer.WriteNull("setSpeed");

			if (cancellationTokenSource != null)
			{
				var id = Guid.NewGuid().ToString();

				context.Callbacks.Add(id, (Action)cancellationTokenSource.Cancel);
				writer.WriteString("cancel", id);
			}
			else
				writer.WriteNull("speed");
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
}
