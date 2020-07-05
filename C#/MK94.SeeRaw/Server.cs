using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MK94.SeeRaw
{
    internal class Server
    {
        private readonly IPAddress ip;
        private readonly short port;

        public readonly Action onClientConnected;
        public readonly Action<string> onMessage;

        private readonly ConcurrentDictionary<IPEndPoint, WebSocket> connections = new ConcurrentDictionary<IPEndPoint, WebSocket>();

        public Server(IPAddress ip, short port, Action onClientConnected, Action<string> onMessage)
        {
            this.ip = ip;
            this.port = port;
            this.onClientConnected = onClientConnected;
            this.onMessage = onMessage;

            Task.Run(RunAsync);
        }

        public async Task RunAsync()
        {
            var server = new TcpListener(ip, port);

            server.Start(50);

            while(true)
            {
                var client = await server.AcceptTcpClientAsync();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                // Fire and forget on purpose
                Task.Run(async() => await HandleClient(client));
#pragma warning restore CS4014 
            }
        }

        public void Broadcast(string message)
        {
            var asArray = Encoding.ASCII.GetBytes(message);

            foreach(var (_, socket) in connections)
            {
                socket.SendAsync(asArray, WebSocketMessageType.Text, true, default);
            }
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
                await ServeFileFromResource(request, writer);
        }

        private async Task ServeFileFromResource(string request, StreamWriter writer)
        {
            var path = request.Split(new[] { ' ' }, 3)[1];

            var resource = RequestPathToResourcePath(path);

            var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(resource);

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

            file = "MK94.SeeRaw.Client." + file.Trim('/');

            return file;
        }

        private bool IsWebsocketUpgradeRequest(Dictionary<string, string> headers) => headers.TryGetValue("Upgrade", out var value) && value.Equals("websocket");

        public async Task UpgradeToWebsocket(IPEndPoint remoteEndpoint, Dictionary<string, string> headers, StreamWriter writer, Stream stream)
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

            onClientConnected.Invoke();

            await HandleWebsocket(websocket);
        }

        private async Task HandleWebsocket(WebSocket socket)
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

                    onMessage.Invoke(message.ToString());
                }
            }
            catch (WebSocketException)
            {

            }
        }
    }
}
