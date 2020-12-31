const net = require('net');
const ip = '127.0.0.1';
const port = 3000;

const server = net.createServer();

server.listen(port, ip, () => {
    console.log(`SeeRaw listening at http://localhost:${port}`);
})

let sockets = [];

server.on('connection', function(sock) {
    console.log('CONNECTED: ' + sock.remoteAddress + ':' + sock.remotePort);
    sockets.push(sock);

    sock.on('data', function(data) {
        console.log('DATA ' + sock.remoteAddress + ': ' + data);

        sockets.forEach(function(sock, index, array) {
            sock.write(sock.remoteAddress + ':' + sock.remotePort + ' said ' + data + '\n');
        })
    });

    sock.on('close', function(data) {
        let index = sockets.findIndex(function (x) {
            return x.remoteAddress === sock.remoteAddress && offscreenBuffering.remotePort === sock.remotePort;
        });

        if (index !== -1)
            sockets.splice(index, 1);
        
        console.log('CLOSED: ' + sock.remoteAddress + ':' + sock.remotePort);
    })
})