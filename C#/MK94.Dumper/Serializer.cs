using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MK94.SirUI
{
    public static class Serializer
	{
		public static void Serialize(object o, StringBuilder builder)
		{
			if (o == null)
				builder.Append(@"{ ""type"": null }");

			else if (o is string)
				builder.Append($@"{{ ""type"": ""string"", ""target"": ""{o}"" }}");

			else if (o is int) // TODO all the other types
				builder.Append($@"{{ ""type"": ""number"", ""target"": {o} }}");

			else if (o is RenderRoot r)
			{
				builder.Append(@"{ ""targets"": [ ");

				foreach ((object element, int i) in r.Targets.Select((rt, i) => (rt, i)))
				{
					if (i != 0)
						builder.Append(", ");

					Serialize(element, builder);
				}

				builder.Append(" ] }");
			}
			else if (o is RenderTarget t)
			{
				Serialize(t.Value, builder);
			}
			else if (o is IEnumerable a)
			{
				builder.Append(@"{ ""type"": ""array"", ""target"": [ ");

				var i = 0;
				foreach (var element in a)
				{
					if (i != 0)
						builder.Append(", ");

					Serialize(element, builder);

					i++;
				}

				builder.Append("] }");
			}
			else if (o is Link l)
			{
				builder.Append($@"{{ ""type"": ""link"", ""text"": ""{l.Text}"", ""id"": ""{l.Id}"" }}");
			}
			else
			{
				builder.Append(@"{ ""type"": ""object"", ""target"": { ");

				foreach ((PropertyInfo prop, int i) in o.GetType().GetProperties().Select((x, i) => (x, i)))
				{
					if (i != 0)
						builder.Append(", ");

					builder.Append($@"""{prop.Name}"": ");
					Serialize(prop.GetValue(o), builder);
				}

				builder.Append("} }");
			}
		}
	}
}
