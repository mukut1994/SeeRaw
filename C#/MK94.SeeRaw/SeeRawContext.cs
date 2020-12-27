using System;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace MK94.SeeRaw
{
    public class Context
    {
        public Server Server { get; internal set; }
        public RenderRoot RenderRoot { get; internal set; }
        public RendererBase Renderer { get; internal set; }
        public WebSocket WebSocket { get; internal set; }
    }

    /// <summary>
    /// Holds references to instances of the current async context. <br />
    /// It internally uses <see cref="AsyncLocal{T}"/> to track the context. <br />
    /// Should only be used from a callback method or if the global default renderer needs to set. <br />
    /// </summary>
    public static class SeeRawContext
    {
        internal static AsyncLocal<Context> localSeeRawContext = new AsyncLocal<Context>();

        public static Context Current => localSeeRawContext.Value;
        public static void ExecuteOnRenderThread(Context context, Action action)
        {
            Task.Run(() =>
            {
                localSeeRawContext.Value = context;

                action();

                localSeeRawContext.Value = null;
            });
        }

        /// <summary>
        /// The Server instance of the current async context
        /// </summary>
        public static Server Server => ValueOrException(x => x.Server);

        /// <summary>
        /// The RendererRoot instance of the current async context <br /> 
        /// This is the state the the currrent client is rendering.
        /// </summary>
        public static RenderRoot RenderRoot => ValueOrException(x => x.RenderRoot);

        /// <summary>
        /// The renderer handling communication to the current async context
        /// </summary>
        public static RendererBase Renderer => ValueOrException(x => x.Renderer);

        /// <summary>
        /// The Websocket to the client. Can be null. <br />
        /// Useful for sending raw messages to the client if you implement a custom client.
        /// </summary>
        public static WebSocket WebSocket => ValueOrException(x => x.WebSocket);

        /// <summary>
        /// The state for the current client. Can be set to any value. Useful to attach custom state objects.
        /// </summary>
        public static object State { get; set; }

        /// <summary>
        /// Downloads a file once on the current client and removes it from the server. 
        /// </summary>
        /// <param name="file">Path to the file</param>
        /// <param name="fileName">The filename the client should download the file as (local file name if null)</param>
        public static void DownloadOnClient(string file, string fileName = null)
        {
            Renderer.DownloadFile(File.OpenRead(file), fileName ?? new FileInfo(file).Name);
        }

        /// <summary>
        /// Gets a <see cref="RenderTarget"/> based on name. If it doesn't exist it added to the renderer first.
        /// </summary>
        public static RenderTarget GetRenderTarget(string name)
        {
            return RenderRoot.Targets.FirstOrDefault(x => x.Name == name);
        }

        /// <summary>
        /// Removes a <see cref="RenderTarget"/> based on name. Can be safely called even if the name doesn't exist.
        /// </summary>
        public static void RemoveTarget(string name)
        {
            var target = RenderRoot.Targets.FirstOrDefault(x => x.Name == name);

            if (target == null)
                return;

            RenderRoot.Targets.Remove(target);

            target.disposed = true;
        }

        private static T ValueOrException<T>(Func<Context, T> property)
        {
            if (localSeeRawContext.Value == null)
                throw new InvalidOperationException($"{nameof(SeeRawContext)} is not available. Make sure your method is a callback from the Renderer");

            return property(localSeeRawContext.Value);
        }
    }
}
