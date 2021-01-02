import WebSocket from 'websocket'
import http from 'http'

const port = 3054;

let server = http.createServer().listen(port);

let wsServer = new WebSocket.server({
  httpServer: server
});

wsServer.on('request', function(request) {
  let connection = request.accept(null, request.origin);

  connection.on('message', (message) => {
    //process
  });

  connection.on('close', function(connection) {
    // close user connection
  });
});