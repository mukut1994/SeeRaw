import WebSocket from 'websocket'
import http from 'http'
import{ RenderTarget } from './RenderTarget';

const port = 3054;

let server = http.createServer().listen(port);

let wsServer = new WebSocket.server({
  httpServer: server
});

wsServer.on('request', function(request) {
  let connection = request.accept(null, request.origin);

  

  connection.on('message', (message) => {
    let firstMessage = new RenderTarget();
    firstMessage.Value = "Hello World";
    firstMessage.name = "Hello";

    connection.sendBytes(firstMessage);
  });

  connection.on('close', function(connection) {
    // close user connection
  });
});