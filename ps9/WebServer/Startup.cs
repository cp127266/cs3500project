/// Authors: Nerd Ropes (Claire Park and Alissa Shaw) for FA2024 CS3500, Dec. 4, 2024
/// Description: Start up for webserver
namespace WebServer
{
    /// <summary>
    /// Starts up webserver
    /// </summary>
    internal class Startup
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            server.Start();
        }
    }
}
