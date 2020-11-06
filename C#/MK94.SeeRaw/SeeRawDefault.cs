using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading;

namespace MK94.SeeRaw
{
    public static class SeeRawDefault
    {
        internal static AsyncLocal<Context> localSeeRawContext = new AsyncLocal<Context>();

        internal static Server server;

        public static Server WithServer(short port = 3054) => WithServer(IPAddress.Loopback, port);

        public static Server WithServer(IPAddress bindAddress, short port = 3054)
        {
            server = new Server(bindAddress, port);
            return server;
        }

        public static Server WithGlobalRenderer(this Server server, Action initialise = null, bool defaultGlobalRenderer = true)
        {
            var renderer = new Renderer(server, defaultGlobalRenderer);
            server.WithRenderer(() => renderer);

            initialise?.Invoke();

            return server;
        }

        public static Server WithPerClientRenderer(this Server server, Action onClientConnected)
        {
            var renderer = new PerClientRenderer(onClientConnected);
            server.WithRenderer(() => renderer);

            return server;
        }
    }

    public class Context
    {
        public Server Server;
        public RenderRoot RenderRoot;
        public RendererBase Renderer;
        public WebSocket WebSocket;
    }

    public static class SeeRawContext
    {
        internal static Server Server => ValueOrException(x => x.Server);
        internal static RenderRoot RenderRoot => ValueOrException(x => x.RenderRoot);
        internal static RendererBase Renderer => ValueOrException(x => x.Renderer);
        internal static WebSocket WebSocket => ValueOrException(x => x.WebSocket);

        public static void DownloadOnClient(string file, string fileName = null)
        {
            Renderer.DownloadFile(File.OpenRead(file), fileName ?? new FileInfo(file).Name);
        }

        public static RenderTarget GetRenderTarget(string name)
        {
            return RenderRoot.Targets.FirstOrDefault(x => x.Name == name);
        }

        private static T ValueOrException<T>(Func<Context, T> property)
        {
            if (SeeRawDefault.localSeeRawContext.Value == null)
                throw new InvalidOperationException($"{nameof(SeeRawContext)} is not available. Make sure your method is a callback from the Renderer");

            return property(SeeRawDefault.localSeeRawContext.Value);
        }
    }
}
