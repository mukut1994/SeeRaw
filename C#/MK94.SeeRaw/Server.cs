using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MK94.SeeRaw
{
    public class Server
    {
        private class ServerFile
        {
            public Func<Stream> Stream;
            public string FileName;
            public string Type;
            public int? TimesRemaining = null;

            public bool DecreaseCounter()
            {
                if (TimesRemaining == null)
                    return true;

                lock (this)
                {
                    TimesRemaining--;
                    return TimesRemaining >= 0;
                }
            }
        }

        private readonly IPAddress ip;
        private readonly short port;
        private readonly ConcurrentDictionary<IPEndPoint, WebSocket> connections = new ConcurrentDictionary<IPEndPoint, WebSocket>();
        private readonly CancellationTokenSource openBrowserCancel = new CancellationTokenSource();
        private readonly ConcurrentDictionary<string, ServerFile> files = new ConcurrentDictionary<string, ServerFile>();

        private Func<RendererBase> rendererFactory;

        public Server(IPAddress ip, short port)
        {
            this.ip = ip;
            this.port = port;
        }

        public Server WithRenderer(Func<RendererBase> rendererFactory)
        {
            Contract.Requires(rendererFactory == null, "Renderer already set");

            this.rendererFactory = rendererFactory;
            return this;
        }

        public Server RunInBackground()
        {
            Task.Run(RunAsync);
            return this;
        }

        public async Task RunAsync()
        {
            Contract.Ensures(rendererFactory != null, "Set renderer before starting server");

            var server = new TcpListener(ip, port);

            server.Start(50);

            while(true)
            {
                var client = await server.AcceptTcpClientAsync();

                openBrowserCancel.Cancel();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                // Fire and forget on purpose
                Task.Run(async() => await HandleClient(client));
#pragma warning restore CS4014 
            }
        }

        public void Broadcast(ArraySegment<byte> message)
        {
            foreach(var (_, socket) in connections)
            {
                socket.SendAsync(message, WebSocketMessageType.Text, true, default);
            }
        }

        /// <summary>
        /// Opens the browser if no client as connected during the waitTime
        /// Useful if the project is being restarted multiple times during debugging
        /// </summary>
        public Server OpenBrowserAfterWait(TimeSpan waitTime)
        {
            Task.Run(() =>
            {
                openBrowserCancel.Token.WaitHandle.WaitOne(waitTime);
                if(!openBrowserCancel.IsCancellationRequested)
                    OpenBrowser();
            });

            return this;
        }

        public Server OpenBrowser()
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

            return this;
        }

        public Server ServeDirectory(string directory, out List<string> paths, bool fileNameIsPath = true, int? times = null, TimeSpan? timeout = null)
        {
            string toPath(string file)
            {
                return fileNameIsPath ? Path.GetFileName(file) : Guid.NewGuid().ToString();
            }

            ServeDirectory(directory, toPath, out paths, times, timeout);

            return this;
        }

        public Server ServeDirectory(string directory, Func<string, string> fileToPath, out List<string> paths, int? times = null, TimeSpan? timeout = null)
        {
            var ret = new List<string>();

            foreach (var file in Directory.EnumerateFiles(directory))
            {
                var path = fileToPath(file);

                ServeFile(() => File.OpenRead(file), path, times: times, timeout: timeout);

                ret.Add(path);
            }

            paths = ret;
            return this;
        }

        public Server ServeFile(Func<Stream> streamFactory, out string path, string fileName = null, string type = "text/plain", int? times = 1, TimeSpan? timeout = null)
        {
            path = Guid.NewGuid().ToString();

            ServeFile(streamFactory, path, fileName, type, times, timeout);

            return this;
        }

        public Server ServeFile(Func<Stream> streamFactory, string path, string fileName = null, string type = "text/plain", int? times = 1, TimeSpan? timeout = null)
        {
            path = path.StartsWith('/') ? path : "/" + path;

            files.TryAdd(path, new ServerFile
            {
                Stream = streamFactory,
                FileName = fileName,
                Type = type,
                TimesRemaining = times
            });

            if (timeout != null)
            {
                var tokenSource = new CancellationTokenSource();
                tokenSource.Token.Register(() => StopServing(path));
                tokenSource.CancelAfter(timeout.Value);
            }

            return this;
        }

        public void StopServing(string path)
        {
            files.TryRemove(path, out var _);
        }

        private async Task HandleClient(TcpClient client)
        {
            using var _ = client;

            var stream = client.GetStream();
            var reader = new StreamReader(stream);
            var writer = new StreamWriter(stream);

            var request = reader.ReadLine();

            if (request == null)
                return;

            var headers = new Dictionary<string, string>();

            do
            {
                var header = reader.ReadLine();

                if (string.IsNullOrEmpty(header))
                    break;

                var split = header.Split(new[] { ':' }, 2, StringSplitOptions.None);

                headers.Add(split[0], split[1].TrimStart());
            }
            while (true);

            if (IsWebsocketUpgradeRequest(headers))
                await UpgradeToWebsocket(client.Client.RemoteEndPoint as IPEndPoint, headers, writer, stream);
            else
                await ServeFile(request, writer);
        }

        private Task ServeFile(string request, StreamWriter writer)
        {
            var path = request.Split(new[] { ' ' }, 3)[1];

            if (files.TryGetValue(path, out var file))
            {
                if (file.DecreaseCounter())
                    return ServeFileFromStream(file, writer);
                else
                {
                    files.Remove(path, out var _);
                    return ServeFileFromResource(path, writer);
                }
            }
            else
                return ServeFileFromResource(path, writer);

        }

        private async Task ServeFileFromStream(ServerFile file, StreamWriter writer)
        {
            using var stream = file.Stream();

            await writer.WriteLineAsync("HTTP/1.1 200 OK");
            await writer.WriteLineAsync($"Content-Type: {file.Type}");
            if (file.FileName != null)
            {
                await writer.WriteLineAsync($"Content-Length: {stream.Length}");
                await writer.WriteLineAsync($"Content-Disposition: attachment; filename={file.FileName}");
            }

            await writer.WriteLineAsync("");
            await writer.FlushAsync();

            await stream.CopyToAsync(writer.BaseStream);
            await writer.FlushAsync();
        }

        private async Task ServeFileFromResource(string path, StreamWriter writer)
        {
            var resource = RequestPathToResourcePath(path);

            var stream = Assembly.GetAssembly(typeof(Server))
                .GetManifestResourceStream(resource);

            if(stream == null)
            {
                await writer.WriteLineAsync("HTTP/1.1 404 Not Found");
                await writer.WriteLineAsync("");
                await writer.FlushAsync();
                return;
            }

            await writer.WriteLineAsync("HTTP/1.1 200 OK");
            await writer.WriteLineAsync("Content-Type: text/html");
            await writer.WriteLineAsync("");
            await writer.FlushAsync();

            await stream.CopyToAsync(writer.BaseStream);
        }

        private static string RequestPathToResourcePath(string file)
        {
            if (file == "/")
                file = "index.html";

            file = "MK94.SeeRaw." + file.Trim('/');

            return file;
        }

        private bool IsWebsocketUpgradeRequest(Dictionary<string, string> headers) => headers.TryGetValue("Upgrade", out var value) && value.Equals("websocket");

        private async Task UpgradeToWebsocket(IPEndPoint remoteEndpoint, Dictionary<string, string> headers, StreamWriter writer, Stream stream)
        {
            using var sha = SHA1.Create();

            var hash = sha.ComputeHash(Encoding.ASCII.GetBytes(headers["Sec-WebSocket-Key"] + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"));
            var base64hash = Convert.ToBase64String(hash);

            await writer.WriteLineAsync("HTTP/1.1 101 Switching Protocols");
            await writer.WriteLineAsync("Upgrade: websocket");
            await writer.WriteLineAsync("Connection: Upgrade");
            await writer.WriteLineAsync($"Sec-WebSocket-Accept: {base64hash}");
            await writer.WriteLineAsync();
            await writer.FlushAsync();

            var websocket = WebSocket.CreateFromStream(stream, true, null, TimeSpan.FromSeconds(30));

            connections.TryAdd(remoteEndpoint, websocket);

            var renderer = rendererFactory();
            var state = renderer.OnClientConnected(this, websocket);

            await HandleWebsocket(state, remoteEndpoint, websocket, renderer);
        }

        private async Task HandleWebsocket(object state, IPEndPoint remoteEndpoint, WebSocket socket, RendererBase renderer)
        {
            try
            {
                while (true)
                {
                    var message = new StringBuilder();

                    do
                    {
                        var buffer = WebSocket.CreateServerBuffer(1024);

                        var result = await socket.ReceiveAsync(buffer, default);

                        var messagePart = Encoding.ASCII.GetString(buffer.Array, buffer.Offset, result.Count);

                        message.Append(messagePart);

                        if (result.EndOfMessage)
                            break;
                    }
                    while (true);

                    if (socket.State != WebSocketState.Open)
                        return;

                    renderer.OnMessageReceived(state, this, socket, message.ToString());
                }
            }
            catch (WebSocketException)
            {
                connections.TryRemove(remoteEndpoint, out _);
            }
        }
    }
}
