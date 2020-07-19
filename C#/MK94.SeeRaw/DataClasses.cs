using System;
using System.Collections.Generic;

namespace MK94.SeeRaw
{
    public class RenderRoot
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
	}

	public class Link
	{
		internal Delegate Action { get; }

		public string Text { get; }

		public Link(string text, Delegate action)
		{
			Action = action;
			Text = text;
		}
	}
}
