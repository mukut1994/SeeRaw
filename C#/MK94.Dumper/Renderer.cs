using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace MK94.SirUI
{
	public class Renderer
	{
		private readonly short port;

		private RenderRoot state = new RenderRoot();

		private Lazy<Server> server;

		private ConcurrentDictionary<string, Action> actions = new ConcurrentDictionary<string, Action>();

		public Renderer(short port = 3054, bool openBrowser = false)
		{
			this.port = port;
			server = new Lazy<Server>(() => new Server(IPAddress.Loopback, port, ClientConnected, MessageReceived));

			if (openBrowser)
				OpenBrowser();
		}

		public void OpenBrowser()
		{
			_ = server.Value;
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
	
		void ClientConnected()
		{
			var serialized = new StringBuilder();
			Serializer.Serialize(state, serialized);

			server.Value.Broadcast(serialized.ToString());
		}

		void MessageReceived(string message)
		{
			var arg = message.Split(';');

			if (arg[0].Equals("link"))
			{
				if (!actions.TryGetValue(arg[1].Trim(), out var action))
					return;

				action();
			}
#if DEBUG
			else
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
			var serialized = new StringBuilder();
			Serializer.Serialize(state, serialized);
			var payload = serialized.ToString();

			server.Value.Broadcast(payload);
		}

		public void RegisterAction(Guid id, Action handler)
		{
			actions[id.ToString()] = handler;
		}
	}
}
