using System;
using System.Net;
using System.Threading;

namespace MK94.SeeRaw
{
    public static class SeeRawDefault
    {
        internal static AsyncLocal<Func<object, RenderTarget>> globalRenderer = new AsyncLocal<Func<object, RenderTarget>>();

        internal static Server server;

        public static Server WithServer(short port = 3054) => WithServer(IPAddress.Loopback, port);

        public static Server WithServer(IPAddress bindAddress, short port = 3054)
        {
            server = new Server(bindAddress, port);
            return server;
        }

        public static Server WithGlobalRenderer(this Server server, bool defaultForExtension = true)
        {
            var renderer = new Renderer(server);
            server.WithRenderer(() => renderer);

            if (defaultForExtension)
                globalRenderer.Value = (x) => { renderer.Render(x, out var t); return t; } ;

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
