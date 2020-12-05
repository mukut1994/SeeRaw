using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MK94.SeeRaw
{
	public abstract class RendererBase
	{
		protected JsonSerializerOptions jsonOptions;
		protected Serializer serializer = new Serializer();

		private Context previousContext;

		protected RendererBase()
		{
			jsonOptions = new JsonSerializerOptions();
			jsonOptions.Converters.Add(new JsonStringEnumConverter());
		}

		public abstract object OnClientConnected(Server server, WebSocket websocket);
		public abstract void OnMessageReceived(object state, Server server, WebSocket websocket, string message);

		public virtual void DownloadFile(Stream stream, string fileName, string mimeType = "text/plain")
		{
			var path = SeeRawContext.localSeeRawContext.Value.Server.ServeFile(() => stream, fileName, mimeType, timeout: TimeSpan.FromSeconds(30));

			SeeRawContext.localSeeRawContext.Value.WebSocket.SendAsync(Encoding.ASCII.GetBytes(@$"{{ ""download"": ""{path}"" }}"), WebSocketMessageType.Text, true, default);
		}

		protected void ExecuteCallback(Server server, RenderRoot renderRoot, WebSocket webSocket, Dictionary<string, Delegate> callbacks, string message)
		{
			JsonElement deserialized = (JsonElement) JsonSerializer.Deserialize<Object>(message);

			var id = deserialized.GetProperty("id").GetString();
			var type = deserialized.GetProperty("type").GetString();

			if (callbacks.TryGetValue(id, out var @delegate))
			{
				if (type == "link" && @delegate is Action a)
				{
					SetContext(server, renderRoot, webSocket);
					a();
					ResetContext();
				}

				else if (type == "form")
				{
					var jsonArgs = deserialized.GetProperty("args");
					var parameters = @delegate.Method.GetParameters();
					var deserializedArgs = new List<object>();

					if (parameters.Length == 1 && parameters[0].ParameterType == typeof(Dictionary<string, object>))
					{
						var dict = new Dictionary<string, object>();

						foreach(var kvpair in jsonArgs.EnumerateArray())
                        {
							var key = kvpair.GetProperty("key").GetString();
							object value = kvpair.GetProperty("value").ValueKind switch
							{
								JsonValueKind.Null => null,
								JsonValueKind.Number => kvpair.GetProperty("value").GetInt32(),
								JsonValueKind.String => kvpair.GetProperty("value").GetString(),
								JsonValueKind.True => true,
								JsonValueKind.False => false,

								_ => throw new NotImplementedException()
							};

							dict[key] = value;
						}

						deserializedArgs.Add(dict);
					}
					else
					{
						for (int i = 0; i < parameters.Length; i++)
						{
							// Hacky way to deserialize until https://github.com/dotnet/runtime/issues/31274 is implemented
							var jsonArg = JsonSerializer.Deserialize(jsonArgs[i].GetProperty("value").GetRawText(), parameters[i].ParameterType, jsonOptions);

							deserializedArgs.Add(jsonArg);
						}
					}

					SetContext(server, renderRoot, webSocket);
					@delegate.DynamicInvoke(deserializedArgs.ToArray());
					ResetContext();
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

		protected void SetContext(Server server, RenderRoot renderRoot, WebSocket webSocket)
        {
			previousContext = SeeRawContext.localSeeRawContext.Value;

			SeeRawContext.localSeeRawContext.Value = new Context
			{
				Renderer = this,
				Server = server,
				RenderRoot = renderRoot,
				WebSocket = webSocket
			};
        }

		protected void ResetContext()
        {
			SeeRawContext.localSeeRawContext.Value = previousContext;
			previousContext = null;
		}

		public RendererBase WithSerializer<T>(ISerialize serializer) => WithSerializer(typeof(T), serializer);

		public RendererBase WithSerializer(Type type, ISerialize serializer)
		{
			this.serializer.serializers[type] = serializer;
			return this;
		}
	}

	public class GlobalStateRenderer : RendererBase
	{
		private Server server;
		private RenderRoot state;
		private SerializerContext serializerContext;

		public GlobalStateRenderer(Server server, bool setGlobalContext, Action initialise)
		{
			this.server = server;
			state = new RenderRoot();

			serializerContext = new SerializerContext();
			serializerContext.onPropertyChanged = (s, e) => Refresh();

			if (setGlobalContext)
				SetContext(server, state, null);

			if(initialise != null)
			{
				if (!setGlobalContext)
					SetContext(server, state, null);

				initialise.Invoke();

				if (!setGlobalContext)
					ResetContext();
            }
		}

		public override object OnClientConnected(Server server, WebSocket websocket) { Refresh(); return null; }
		public override void OnMessageReceived(object state, Server server, WebSocket websocket, string message) => ExecuteCallback(server, this.state, websocket, serializerContext.Callbacks, message);

		public void Refresh()
		{
			serializerContext.Callbacks.Clear();
			serializerContext.ClearPropertyChangedHandlers();
			server.Broadcast(serializer.SerializeState(state, serializerContext));
		}
    }

	public class PerClientRenderer : RendererBase
	{
		private class ClientState
		{
			internal RenderRoot State { get; }

			internal SerializerContext serializerContext;

            public ClientState(RenderRoot state, SerializerContext serializerContext)
            {
                State = state;
                this.serializerContext = serializerContext;
            }
        }

		private readonly Action onClientConnected;

        public PerClientRenderer(Action onClientConnected)
		{
			this.onClientConnected = onClientConnected;
		}

        public override object OnClientConnected(Server server, WebSocket websocket)
		{
			var renderRoot = new RenderRoot();
			var serializerContext = new SerializerContext();

			var state = new ClientState(renderRoot, serializerContext);

			void Refresh(object o, System.ComponentModel.PropertyChangedEventArgs args)
			{
				serializerContext.Callbacks.Clear();
				serializerContext.ClearPropertyChangedHandlers();
				var message = serializer.SerializeState(renderRoot, state.serializerContext);
				Task.Run(() => websocket.SendAsync(message, WebSocketMessageType.Text, true, default));
			};

			serializerContext.onPropertyChanged = Refresh;

			SetContext(server, state.State, websocket);
			onClientConnected();
			Refresh(null, null);
			ResetContext();

			return state;
        }

        public override void OnMessageReceived(object state, Server server, WebSocket websocket, string message)
        {
			var clientState = state as ClientState;

			ExecuteCallback(server, clientState.State, websocket, clientState.serializerContext.Callbacks, message);
        }
	}
}
