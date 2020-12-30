import java.net.InetAddress;
import java.net.ServerSocket;
import java.net.Socket;

public class Server {

    private InetAddress ip;
    private short port;

    public Server(InetAddress ip, short port)
    {
        this.ip = ip;
        this.port = port;
    }

    /*public Server RunInBackground() {


        new Thread(RunAsync()).run();
        return this;
    }*/

    public Runnable Run() {
        Runnable task = () => {
            ServerSocket server = null;
            try {
                server = new ServerSocket(port, 50, ip);
            } catch (Exception e) {
                e.printStackTrace();
                System.exit(1);
            }

            Socket client = null;
            try {
                client = server.accept();
            } catch (Exception e) {
                e.printStackTrace();
            }

        };

        return task;
    }

    /*public async Task RunAsync()
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
        }*/
}
