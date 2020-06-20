using System;
using System.Collections.Generic;

namespace MK94.Dumper
{
    public class RenderRoot
	{
		public List<RenderTarget> targets { get; } = new List<RenderTarget>();
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
	}

	public class Link
	{
		internal Guid id { get; }

		internal Action action { get; }

		public string Text { get; }

		public Link(string text, Action action)
			: this(Extensions.instance.Value, text, action) { }

		public Link(Renderer parent, string text, Action action)
		{
			this.id = Guid.NewGuid();
			this.action = action;
			Text = text;

			parent.RegisterAction(this.id, action);
		}
	}
}
