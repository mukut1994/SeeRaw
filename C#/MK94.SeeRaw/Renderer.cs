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

		private Server server;
		private RenderRoot state = new RenderRoot();
		private Dictionary<string, Delegate> callbacks = new Dictionary<string, Delegate>();
		private Serializer serializer = new Serializer();

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

		void MessageReceived(string message)
		{
			var deserialized = JsonSerializer.Deserialize<JsonElement>(message);

			var id = deserialized.GetProperty("id").GetString();
			var type = deserialized.GetProperty("type").GetString();

			if (callbacks.TryGetValue(id, out var @delegate))
			{
				if (type == "link" && @delegate is Action a)
					a();

				else if (type == "form")
				{
					var jsonArgs = deserialized.GetProperty("args");
					var parameters = @delegate.Method.GetParameters();
					var deserializedArgs = new List<object>();

					for (int i = 0; i < parameters.Length; i++)
					{
						// Hacky way to deserialize until https://github.com/dotnet/runtime/issues/31274 is implemented
						var jsonArg = JsonSerializer.Deserialize(jsonArgs[i].GetRawText(), parameters[i].ParameterType);

						deserializedArgs.Add(jsonArg);
					}

					@delegate.DynamicInvoke(deserializedArgs.ToArray());
				}
				else UnknownMessage();
			}
			else UnknownMessage();

			void UnknownMessage()
			{
#if DEBUG
				Console.WriteLine("Unknown message " + message);
#endif
			}
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
			callbacks.Clear();
			server.Broadcast(serializer.SerializeState(state, callbacks));
		}

		public Renderer WithSerializer<T>(ISerialize serializer) => WithSerializer(typeof(T), serializer);

		public Renderer WithSerializer(Type type, ISerialize serializer) 
		{
			this.serializer.serializers[type] = serializer;
			return this;
		}
	}
}
