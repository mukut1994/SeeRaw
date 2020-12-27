using System;
using System.Net;
using System.Net.WebSockets;

namespace MK94.SeeRaw
{
    public static class SeeRawSetup
    {
        internal static Server server;

        public static Server WithServer(short port = 3054) => WithServer(IPAddress.Loopback, port);

        public static Server WithServer(IPAddress bindAddress, short port = 3054)
        {
            server = new Server(bindAddress, port);
            return server;
        }

        public static Server WithGlobalRenderer(this Server server, Action initialise = null, Action<RendererBase> configure = null, bool defaultGlobalRenderer = true)
        {
            var renderer = new SharedStateRenderer(server, defaultGlobalRenderer, initialise);
            configure(renderer);
            server.WithRenderer(() => renderer);

            return server;
        }

        public static Server WithPerClientRenderer(this Server server, Action onClientConnected, Action<RendererBase> configure = null)
        {
            var renderer = new PerClientRenderer(onClientConnected);
            configure(renderer);
            server.WithRenderer(() => renderer);

            return server;
        }
    }
}
