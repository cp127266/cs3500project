/// Authors: Nerd Ropes (Claire Park and Alissa Shaw) for FA2024 CS3500, Nov. 16, 2024
/// Description: Controller class of the MVC. Repeadetly recieves from the network and updates the model (deserialize into JSON objects).
///              Send over network when the key is pressed, informed via View (the GUI).

using CS3500.Networking;
using Model;
using System.Text.Json;
using System.Text;
using DatabaseController;
using System.Net.Sockets;
using System.Diagnostics;

namespace Networking
{
    public class Controller
    {
        /// <summary>
        /// The NetworkConnection object.
        /// </summary>
        private NetworkConnection network;


        /// <summary>
        /// locks GameWorld
        /// </summary>
        private readonly object gameWorldLock = new();

        /// <summary>
        /// Getter and setter for GameWorld data
        /// </summary>
        public World GameWorld
        {
            get
            {
                lock (gameWorldLock)
                {
                    return _gameWorld;
                }
            }
            private set
            {
                lock (gameWorldLock)
                {
                    _gameWorld = value;
                }
            }
        }

        /// <summary>
        /// Helper for property
        /// </summary>
        private World _gameWorld;

        /// <summary>
        /// Player ID
        /// </summary>
        public int ID { get; private set; }

        /// <summary>
        /// World size
        /// </summary>
        public int WorldSize { get; private set; }


        /// <summary>
        /// Returns the network connection status.
        /// </summary>
        public bool NetworkStatus { get => network.IsConnected; }

        /// <summary>
        /// Tells if GameWorld has been updated
        /// </summary>
        public event Action<World>? WorldUpdated;

        /// <summary>
        /// Instantiates a Database object
        /// </summary>
        private Database database;

        /// <summary>
        /// Default constructor. Creates a new NetworkConnection.
        /// </summary>
        /// <param name="serverNameOrAddress"></param>
        /// <param name="port"></param>
        public Controller()
        {
            network = new NetworkConnection();
            _gameWorld = new World();
            database = new Database();

        }

        /// <summary>
        /// Controller object with an already initialized TcpClient
        /// </summary>
        /// <param name="tcpclient"></param>
        public Controller(TcpClient tcpclient) : this()
        {
            network = new NetworkConnection(tcpclient);
        }

        /// <summary>
        /// Initiates network connection to passed in server and port. Starts a new thread and continously reads from the connection to the server.
        /// Parses the JSON describing the world's state from the server and deserializes it into a world object, GameWorld.
        /// </summary>
        /// <param name="serverNameOrAddress"></param>
        /// <param name="port"></param>
        /// <exception cref="Exception"></exception>
        public void ConnectToNetwork(string serverNameOrAddress, int port)
        {

            StringBuilder sb;
            var worldState_json = String.Empty;
            int serverMessageCount = 0;

            try
            {
                network.Connect(serverNameOrAddress, port);

                new Thread(() =>
                {
                    while (network.IsConnected)
                    {
                        try
                        {
                            if (serverMessageCount == 0)
                            {
                                ID = int.Parse(network.ReadLine());  //get player ID from server
                                serverMessageCount++;
                            }
                            else if (serverMessageCount == 1)
                            {
                                WorldSize = int.Parse(network.ReadLine()); //get world size from server
                                serverMessageCount++;
                            }
                            else
                            {
                                {

                                    if (worldState_json.Equals(string.Empty))
                                    {
                                        worldState_json = network.ReadLine();
                                    }

                                    Wall newWall;
                                    while (worldState_json.Contains("wall"))
                                    {
                                        newWall = JsonSerializer.Deserialize<Wall>(worldState_json.ToString());
                                        if (!_gameWorld.WallObjects.ContainsKey(newWall.ID))
                                        {
                                            _gameWorld.WallObjects.Add(newWall.ID, newWall);
                                        }
                                        worldState_json = network.ReadLine();
                                    }

                                    Snake newSnake;
                                    while (worldState_json.Contains("snake"))
                                    {
                                        newSnake = JsonSerializer.Deserialize<Snake>(worldState_json.ToString());
                                        if (_gameWorld.SnakeClients.ContainsKey(newSnake.ID))
                                        {
                                            _gameWorld.SnakeClients.Remove(newSnake.ID);
                                        }
                                        _gameWorld.SnakeClients.Add(newSnake.ID, newSnake);

                                        worldState_json = network.ReadLine();
                                    }

                                    Food newFood;
                                    while (worldState_json.Contains("power"))
                                    {
                                        newFood = JsonSerializer.Deserialize<Food>(worldState_json.ToString());
                                        if (!_gameWorld.FoodObjects.ContainsKey(newFood.ID))
                                        {
                                            _gameWorld.FoodObjects.Add(newFood.ID, newFood);
                                        }
                                        worldState_json = network.ReadLine();
                                    }

                                    lock (gameWorldLock)
                                    {
                                        NotifyWorldUpdated();
                                    }

                                }
                            }
                        }
                        catch
                        {
                            // disconnection from server
                            serverMessageCount = 0;
                        }

                    }

                }).Start();
            }
            catch
            {
                throw new Exception("Network connection failed.");
            }

            try
            {
                database.AddGame();
            }
            catch
            {
                Debug.WriteLine("databasse connection failed");
                throw new Exception("Connection to database failed.");
               
            }
        }

        /// <summary>
        /// Invokes the disconnect method of NetworkConnection
        /// </summary>
        public void DisconnectFromNetwork()
        {
            network.Disconnect();
            database.EndGame();
            _gameWorld.SnakeClients.Clear();
            _gameWorld.WallObjects.Clear();
            _gameWorld.FoodObjects.Clear();
        }

        /// <summary>
        /// Invokes the send method of NetworkConnection to send player name or movement commands
        /// </summary>
        /// <param name="input"></param>
        public void SendToNetwork(string input)
        {
            network.Send(input);
        }

        /// <summary>
        /// If GameWorld is changed, updated WorldUpdated.
        /// </summary>
        private void NotifyWorldUpdated()
        {
            WorldUpdated?.Invoke(GameWorld);
        }

        /// <summary>
        /// Invokes ReadLine of network controller.
        /// </summary>
        /// <returns></returns>
        public string ReadFromNetwork()
        {
            return network.ReadLine();
        }

        /// <summary>
        /// Invokes Database's LeaveGame method, removing the snake from the game.
        /// </summary>
        /// <param name="snakeID"></param>
        public void RemoveSnake(int snakeID)
        {
            database.LeaveGame(snakeID);
        }

        /// <summary>
        /// Invokes Database's AddPlayer method, adding the snake to the game only if it has not been previously added.
        /// </summary>
        /// <param name="snakeID"></param>
        public void AddSnake(int snakeID, string snakeName)
        {
            List<int> currentSnakes = new List<int>(database.GetPlayerIDs());
            if (!currentSnakes.Contains(snakeID))
            {
                database.AddPlayer(snakeID, snakeName);
            }
        }

        /// <summary>
        /// Invokes Database's GetMaxScore method.
        /// </summary>
        /// <param name="snakeID"></param>
        /// <returns></returns>
        public int GetSnakeMaxScore(int snakeID)
        {
            List<int> currentSnakes = new List<int>(database.GetPlayerIDs());
            if (currentSnakes.Contains(snakeID))
            {
                return database.GetMaxScore(snakeID);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Invokes Database's SetMaxScore method.
        /// </summary>
        /// <param name="snakeID"></param>
        /// <param name="max"></param>
        public void SetSnakeMaxScore(int snakeID, int max)
        {
            List<int> currentSnakes = new List<int>(database.GetPlayerIDs());
            if (currentSnakes.Contains(snakeID))
            {
                database.SetMaxScore(snakeID, max);
            }
        }
    }
}
