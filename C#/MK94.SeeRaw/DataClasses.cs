using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Threading;

namespace MK94.SeeRaw
{
    public class RenderRoot
	{
		private readonly Action<RenderRoot> onChange;

		internal RenderRoot(Action<RenderRoot> onChange)
        {
			this.onChange = onChange;
        }

		internal List<RenderTarget> Targets { get; } = new List<RenderTarget>();

		public RenderTarget Render(object obj)
        {
			var ret = new RenderTarget(() => onChange(this), obj);
			Targets.Add(ret);
			return ret;
        }
	}

	public class RenderTarget
	{
		private readonly Action onChange;
		private object value;

		public object Value
		{
			get => value;
			set
			{
				this.value = value;
				onChange();
			}
		}

		public RenderTarget(Action onChange, object obj)
		{
			this.onChange = onChange;

			value = obj;
		}

		public void Refresh() => onChange();
	}

	class Actionable : ISerializeable
	{
		internal Delegate Action { get; }

		public string Text { get; }

		public Actionable(string text, Delegate action)
		{
			Action = action;
			Text = text;
		}

        public void Serialize(Serializer serializer, Utf8JsonWriter writer, Dictionary<string, Delegate> callbacks, bool serializeNulls)
        {
			writer.WriteString("text", Text);

			var id = Guid.NewGuid().ToString();

			callbacks.Add(id, Action);
			writer.WriteString("id", id);

			if (Action is Action)
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
			foreach (var parameter in Action.GetMethodInfo().GetParameters())
			{
				if (!parameter.ParameterType.IsPrimitive && parameter.ParameterType != typeof(string) && !parameter.ParameterType.IsNested)
					throw new InvalidOperationException($"Delegate can only have primitive arguments");

				writer.WriteStartObject();

				writer.WriteString("name", parameter.Name);
				serializer.Serialize(null, parameter.ParameterType, true, writer, null);

				writer.WriteEndObject();
			}
			writer.WriteEndArray();
		}
    }

	public class Progress : ISerializeable
    {
		/// <summary>
		/// Value from 0 to 100; Changes the progress bar
		/// </summary>
		public int Percent { get; set; }

		/// <summary>
		/// The value to display on the bar itself. If empty Percent is used.
		/// Useful if the steps of the operations can be named (e.g. Initialising Job, Copying files, Cleaning up)
		/// </summary>
		public string Value { get; set; }

		/// <summary>
		/// An optional speed message (e.g. 100kb/s, 5m/s)
		/// </summary>
		public string Speed { get; set; }

		public string Min { get; set; }

		public string Max { get; set; }

		internal Action pauseToggle { get; }

		internal Action<int?> setSpeed { get; }

		internal CancellationTokenSource? cancellationTokenSource { get; }

		internal bool paused { get; set; }

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

		public void Serialize(Serializer serializer, Utf8JsonWriter writer, Dictionary<string, Delegate> callbacks, bool serializeNulls)
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

				callbacks.Add(id, pauseToggle);
				writer.WriteString("pause", id);
				writer.WriteBoolean("paused", paused);
			}
			else
				writer.WriteNull("pause");

			if (setSpeed != null)
			{
				var id = Guid.NewGuid().ToString();

				callbacks.Add(id, setSpeed);
				writer.WriteString("setSpeed", id);
			}
			else
				writer.WriteNull("setSpeed");

			if (cancellationTokenSource != null)
			{
				var id = Guid.NewGuid().ToString();

				callbacks.Add(id, (Action)cancellationTokenSource.Cancel);
				writer.WriteString("cancel", id);
			}
			else
				writer.WriteNull("speed");
		}
    }
}
