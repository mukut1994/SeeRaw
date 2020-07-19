using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MK94.SeeRaw
{
	public class Renderer
	{
		private readonly short port;

		private RenderRoot state = new RenderRoot();

		private Server server;

		private Dictionary<string, Delegate> callbacks = new Dictionary<string, Delegate>();

		public Renderer(short port = 3054, bool openBrowser = false)
		{
			this.port = port;
			server = new Server(IPAddress.Loopback, port, Refresh, MessageReceived);

			if (openBrowser)
				OpenBrowser();
		}

		public void OpenBrowser()
		{
			var url = $"http://localhost:{port}";

			try
			{
				Process.Start(url);
			}
			catch
			{
				// hack because of this: https://github.com/dotnet/corefx/issues/10361
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					url = url.Replace("&", "^&");
					Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				{
					Process.Start("xdg-open", url);
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				{
					Process.Start("open", url);
				}
				else
				{
					throw;
				}
			}
		}

		ArraySegment<byte> SerializeState()
        {
			var memStream = new MemoryStream();
			var writer = new Utf8JsonWriter(memStream);
			writer.WriteStartObject();

			Serializer.Serialize(state, typeof(RenderRoot), false, writer, callbacks);

			writer.WriteEndObject();
			writer.Flush();

			return new ArraySegment<byte>(memStream.GetBuffer(), 0, (int)memStream.Position);
		}

		void MessageReceived(string message)
		{
			//var deserialized = JsonSerializer.Deserialize<JsonElement>(message);
			
			/*var arg = message.Split(';');
			if (arg[0].Equals("link"))
			{
				if (callbacks.TryGetValue(arg[1].Trim(), out var action) && action is Action a)
					a();
			}*/
#if DEBUG
			//else
				Console.WriteLine("Unknown message " + message);
#endif
		}

		public T Render<T>(T o)
		{
			return Render(o, out _);
		}

		public T Render<T>(T o, out RenderTarget target)
		{
			target = new RenderTarget(this, o);

			state.Targets.Add(target);

			return o;
		}

		public void Refresh()
		{
			server.Broadcast(SerializeState());
		}
	}
}
