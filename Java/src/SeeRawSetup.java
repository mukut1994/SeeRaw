import java.net.InetAddress;

public class SeeRawSetup {

    static Server server;

    public static Server WithServer(short port) {
        return WithServer(InetAddress.getLoopbackAddress(), port);
    }

    public static Server WithServer(InetAddress bindAddress, short port){
        server = new Server(bindAddress, port);
        return server;
    }



}
