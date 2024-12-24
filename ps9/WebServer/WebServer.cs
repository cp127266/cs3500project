/// Authors: Nerd Ropes (Claire Park and Alissa Shaw) for FA2024 CS3500, Dec. 5, 2024
/// Description: Begins webserver and updates page depending on http requests and responses
/// Updates data on refresh
/// 
using DatabaseController;
using Networking;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Security.Permissions;
using System.Text;

internal class Server
{
    /// <summary>
    /// Instantiates a Database object
    /// </summary>
    Database database; 

    /// <summary>
    /// Invokes the Database constructor
    /// </summary>
    public Server()
    {
        database = new Database();
    }

    /// <summary>
    /// Starts the connection to the given port, 8080
    /// Begins reading from a controller to determine requests and responses
    /// Updates webserver page depending on HTTP link response
    /// </summary>
    public void Start()
    {
        const int port = 8080;
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();

        Console.WriteLine($"Web server running on localhost:{port}. " +
                          $"\nServer will not open on its own.");

        while (true)
        {
            // Initializing connection between network
            TcpClient tcpClient = listener.AcceptTcpClient();
            Controller controller = new Controller(tcpClient);

            try
            {
                string requestLine = controller.ReadFromNetwork();
                if (string.IsNullOrEmpty(requestLine))
                    continue;

                // Ensures validity of request then makes request
                string[] tokens = requestLine.Split(' ');
                if (tokens.Length < 2 || tokens[0] != "GET")
                    continue;

                string path = tokens[1];
                string response = GenerateHttpResponse(path);

                controller.SendToNetwork(response);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e.Message);
            }
        }
    }

    /// <summary>
    /// Determines HTTP request made and responds to it
    /// If HTTP links to / -> homepage
    /// If HTTP links to /games -> Table of all games and its data
    /// If HTTP links to /games?gid=# -> Table of all players and its data within that game
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    string GenerateHttpResponse(string path)
    {
        // Determines which path is current
        string body = path switch
        {
            "/" => "<html><h3>Welcome to the Snake Games Database!</h3><a href=\"/games\">View Games</a></html>",
            "/games" => GenerateGamesList(),
            _ when path.StartsWith("/games?gid=") => GenerateGameDetails(path.Split("=")[1]),
            _ => "<html><h3>404 Not Found</h3></html>"
        };
        int contentLength = Encoding.UTF8.GetByteCount(body);

        return $"HTTP/1.1 200 OK\r\n" +
               $"Content-Type: text/html; charset=UTF-8\r\n" +
               $"Connection: close\r\n" +
               $"Content-Length: {contentLength}\r\n" +
               $"\r\n" +
               $"{body}";
    }

    /// <summary>
    /// Generates table of all games connected to a network connection into a table
    /// </summary>
    /// <returns></returns>
    string GenerateGamesList()
    {
        // Mock data; replace with a database query.
        return "<html>" +
               "<table border=\"1\">" +
               "<thead><tr><td>ID</td><td>Start</td><td>End</td></tr></thead>" + database.AllGames();
    }

    /// <summary>
    /// Generates table of all active players in the specified game
    /// </summary>
    /// <param name="gameId"></param>
    /// <returns></returns>
    string GenerateGameDetails(string gameId)
    {
        if (int.TryParse(gameId, out var id))
        {
            return $"<html><h3>Stats for Game {gameId}</h3>" +
                   "<table border=\"1\">" +
                   "<thead><tr><td>Player ID</td><td>Player Name</td><td>Max Score</td><td>Enter Time</td><td>Leave Time</td></tr></thead>"
                   + database.PlayersOfGame(id);
        }
        else
        {
            return "<h1>Invalid gameID</h1>";
        }
    }
}