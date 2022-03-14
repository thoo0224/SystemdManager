namespace SystemdManager.Objects;

public class Server
{

    public string Name { get; set; }
    public string Host { get; set; }
    public short Port => 22;
    public string User { get; set; }
    public string Password { get; set; }

    public static Server CreateDefault()
    {
        return new Server
        {
            Name = "Server",
            Host = "127.0.0.1",
            User = "root"
        };
    }

}
