import java.net.ServerSocket;

public class Server {

    private IPAddress ip;
    private short port;

    public Server(IPAddress ip, short port)
    {
        this.ip = ip;
        this.port = port;
    }

    public Server RunInBackground() {


        new Thread(RunAsync());
        return this;
    }

    public Runnable RunAsync() {
        ServerSocket serverSocket = new ServerSocket(port, 50, ip);
    }
}
