﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
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
	public class RendererContext
	{
		internal Dictionary<string, Delegate> Callbacks { get; } = new Dictionary<string, Delegate>();
		private List<INotifyPropertyChanged> PropertyChangedNotifiers { get; } = new List<INotifyPropertyChanged>();

		internal PropertyChangedEventHandler onPropertyChanged;

		public void AddPropertyChangedNotifier(INotifyPropertyChanged notifyPropertyChanged)
		{
			PropertyChangedNotifiers.Add(notifyPropertyChanged);
			notifyPropertyChanged.PropertyChanged += onPropertyChanged;
		}

		public void ClearPropertyChangedHandlers()
		{
			foreach (var p in PropertyChangedNotifiers)
			{
				p.PropertyChanged -= onPropertyChanged;
			}
		}

        internal void RegisterCallback(string id, Delegate action)
        {
			Callbacks.Add(id, action);
        }
    }

	public abstract class RendererBase
	{
		protected JsonSerializerOptions jsonOptions;
		protected Serializer serializer = new Serializer();
		protected string renderOptions;

		public ISerializerConfigure Serializer => serializer;

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
			var path = SeeRawContext.Current.Server.ServeFile(() => stream, fileName, mimeType, timeout: TimeSpan.FromSeconds(30));

			SeeRawContext.Current.WebSocket.SendAsync(Encoding.ASCII.GetBytes(@$"{{ ""download"": ""{path}"" }}"), WebSocketMessageType.Text, true, default);
		}

		public RendererBase WithOptions(string jsonRenderOptions)
        {
			renderOptions = jsonRenderOptions;

			return this;
        }

		protected void SendOptions(WebSocket socket)
        {
			if (renderOptions == null)
				return;

			var x = new
			{
				kind = 4,
				options = renderOptions
			};

			Task.Run(() => socket.SendAsync(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(x)), WebSocketMessageType.Text, true, default));
		}

		protected void ExecuteCallback(Server server, RenderRoot renderRoot, WebSocket webSocket, Dictionary<string, Delegate> callbacks, string message)
		{
			JsonElement deserialized = (JsonElement) JsonSerializer.Deserialize<Object>(message);

			var type = deserialized.GetProperty("type").GetString();

			if (!string.Equals(type, MessageType.Execute.ToString(), StringComparison.OrdinalIgnoreCase))
			{ 
				UnknownMessage();
				return;
			}

			var id = deserialized.GetProperty("id").GetString();

			if (callbacks.TryGetValue(id, out var @delegate))
			{
				if (@delegate is Action a)
				{
					SetContext(server, renderRoot, webSocket);
					a();
					ResetContext();
				}

				else
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
	}

	public class GlobalStateRenderer : RendererBase
	{
		private Server server;
		private RenderRoot state;
		private RendererContext context;

		public GlobalStateRenderer(Server server, bool setGlobalContext, Action initialise)
		{
			this.server = server;
			state = new RenderRoot();

			context = new RendererContext();
			context.onPropertyChanged = (s, e) => Refresh();

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

		public override object OnClientConnected(Server server, WebSocket websocket) { SendOptions(websocket); Refresh(); return null; }
		public override void OnMessageReceived(object state, Server server, WebSocket websocket, string message) => ExecuteCallback(server, this.state, websocket, context.Callbacks, message);

		public void Refresh()
		{
			context.Callbacks.Clear();
			context.ClearPropertyChangedHandlers();
			foreach (var target in state.Targets)
			{
				server.Broadcast(serializer.Serialize(target.Name, target.Value, context));
			}
		}
    }

	public class PerClientRenderer : RendererBase
	{
		private class ClientState
		{
			internal RenderRoot State { get; }

			internal RendererContext serializerContext;

            public ClientState(RenderRoot state, RendererContext serializerContext)
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
			SendOptions(websocket);

			var renderRoot = new RenderRoot();
			var serializerContext = new RendererContext();

			var state = new ClientState(renderRoot, serializerContext);

			void Refresh(object o, System.ComponentModel.PropertyChangedEventArgs args)
			{
				serializerContext.Callbacks.Clear();
				serializerContext.ClearPropertyChangedHandlers();

				foreach (var target in renderRoot.Targets)
				{
					var message = serializer.Serialize(target.Name, target.Value, state.serializerContext);
					Task.Run(() => websocket.SendAsync(message, WebSocketMessageType.Text, true, default));
				}
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
