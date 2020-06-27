using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using WatsonWebsocket;

namespace MK94.SirUI
{
    public class Renderer
	{
		private RenderRoot state = new RenderRoot();
		private Lazy<WatsonWsServer> server;
		private ConcurrentDictionary<string, bool> clients = new ConcurrentDictionary<string, bool>();
		private ConcurrentDictionary<string, Action> actions = new ConcurrentDictionary<string, Action>();

		public Renderer(bool connect = false)
		{
			server = new Lazy<WatsonWsServer>(ConnectRenderer);

			if (connect)
				_ = server.Value;
		}

		private WatsonWsServer ConnectRenderer()
		{
			WatsonWsServer server = new WatsonWsServer("localhost", 3054, false);
			server.HttpHandler = HttpHandler;
			server.ClientConnected += ClientConnected;
			server.ClientDisconnected += ClientDisconnected;
			server.MessageReceived += MessageReceived;
			server.Start();

			return server;
		}

		void HttpHandler(HttpListenerContext httpListenerContext)
        {
			var file = CleanFileName(httpListenerContext.Request.RawUrl);

			httpListenerContext.Response.ContentType = "text/html";

			var stream = Assembly.GetExecutingAssembly()
				.GetManifestResourceStream(file);

			stream.CopyTo(httpListenerContext.Response.OutputStream);
			stream.Flush();

			httpListenerContext.Response.Close();
			stream.Dispose();
		}

		private static string CleanFileName(string file)
		{
			if (file == "/")
				file = "index.html";

			file = "MK94.SirUI.Client." + file.Trim('/');

			return file;
		}

		void ClientConnected(object sender, ClientConnectedEventArgs args)
		{
			clients.TryAdd(args.IpPort, true);
			var serialized = new StringBuilder();
			Serializer.Serialize(state, serialized);
			server.Value.SendAsync(args.IpPort, serialized.ToString());
		}

		void ClientDisconnected(object sender, ClientDisconnectedEventArgs args)
		{
			clients.TryRemove(args.IpPort, out _);
		}

		void MessageReceived(object sender, MessageReceivedEventArgs args)
		{
			var arg = Encoding.UTF8.GetString(args.Data).Split(';');

			if (arg[0].Equals("link"))
			{
				if (!actions.TryGetValue(arg[1].Trim(), out var action))
					return;

				action();
			}
#if DEBUG
			else
				Console.WriteLine("Unknown message from " + args.IpPort + ": " + Encoding.UTF8.GetString(args.Data));
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

			foreach (var connection in clients)
			{
				server.Value.SendAsync(connection.Key, payload);
			}
		}

		public void RegisterAction(Guid id, Action handler)
		{
			actions[id.ToString()] = handler;
		}
	}
}
