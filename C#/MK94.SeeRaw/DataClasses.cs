using System;
using System.Collections.Generic;
using System.Threading;

namespace MK94.SeeRaw
{
    class RenderRoot
	{
		public List<RenderTarget> Targets { get; } = new List<RenderTarget>();
	}

	public class RenderTarget
	{
		private readonly Renderer parent;
		private object value;

		public object Value
		{
			get => value;
			set
			{
				this.value = value;
				parent.Refresh();
			}
		}

		public RenderTarget(Renderer parent, object obj)
		{
			this.parent = parent;

			value = obj;
		}

		public void Refresh()
        {
			parent.Refresh();
        }
	}

	class Actionable
	{
		internal Delegate Action { get; }

		public string Text { get; }

		public Actionable(string text, Delegate action)
		{
			Action = action;
			Text = text;
		}
	}

	public class Progress
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
    }
}
