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

        public static Server WithGlobalRenderer(this Server server, Action initialise = null, bool defaultGlobalRenderer = true)
        {
            var renderer = new GlobalStateRenderer(server, defaultGlobalRenderer, initialise);
            server.WithRenderer(() => renderer);

            return server;
        }

        public static Server WithPerClientRenderer(this Server server, Action onClientConnected)
        {
            var renderer = new PerClientRenderer(onClientConnected);
            server.WithRenderer(() => renderer);

            return server;
        }
    }
}
