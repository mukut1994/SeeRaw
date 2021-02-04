import WebSocket from 'websocket'
import http from 'http'
import{ RenderTarget } from './RenderTarget';

export class Server {
  static port = 3054;

  static WithServer() {
    let httpServer = http.createServer().listen(this.port);

    let wsServer = new WebSocket.server({
      httpServer: httpServer
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
  }
}
