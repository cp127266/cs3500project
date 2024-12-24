using iTextSharp.awt.geom;
using Org.BouncyCastle.Asn1;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Model
{
    /// <summary>
    /// Stores the coordinates of the snake and updates accordingly.
    /// </summary>
    public class Point2D
    {
        public int X { get; set; }
        public int Y { get; set; }

        /// <summary>
        /// Method to move the point by a certain amount
        /// </summary>
        /// <param name="deltaX"></param>
        /// <param name="deltaY"></param>
        public void Move(int deltaX, int deltaY)
        {
            X += deltaX;
            Y += deltaY;
        }
    }

    /// <summary>
    /// TODO
    /// </summary>
    public class Snake
    {
        /// <summary>
        /// ID of this snake object
        /// </summary>
        [JsonPropertyName("snake")]
        public int ID { get; set; }

        [JsonPropertyName("body")]
        /// <summary>
        /// Returns current length of snake
        /// </summary>
        public List<Point2D> Body { get; set; } = new List<Point2D>();

        /// <summary>
        /// Username of this snake client
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = String.Empty;

        /// <summary>
        /// Returns current score of snake
        /// </summary>
        [JsonPropertyName("score")]
        public int Score { get; set; }

        /// <summary>
        /// True if snake is connected and alive
        /// </summary>
        [JsonPropertyName("alive")]
        public bool Alive { get; set; }

        /// <summary>
        /// True if snake died in that frame false otherwise
        /// </summary>
       [JsonPropertyName("died")]

        public bool Died { get; set; }

        /// <summary>
        /// True if client is disconnected true otherwise
        /// </summary>
        [JsonPropertyName("dc")]
        public bool DC { get; set; }

        /// <summary>
        /// True if snake joined game false otherwise
        /// </summary>
        [JsonPropertyName("join")]
        public bool Join { get; set; }

        /// <summary>
        /// Stores the max score of this snake during the current game.
        /// </summary>
        [JsonIgnore]
        public int MaxScore = 0;
    }

    /// <summary>
    /// Stores properties of teh Food objects.
    /// </summary>
    public class Food
    {

        /// <summary>
        /// ID of this food object
        /// </summary>
        /// </summary>
        [JsonPropertyName("power")]
        public int ID { get; set; }

        /// <summary>
        /// Returns coordinates of this food object
        /// </summary>
        [JsonPropertyName("loc")]
        public Point2D Coords { get; set; } = new Point2D();

        /// <summary>
        /// Determines the status of this food object
        /// </summary>
        [JsonPropertyName("died")]
        public bool died { get; set; }

    }

    /// <summary>
    /// Creates 'barrier' for game.
    /// All objects are placed within barrier determined by Wall object.
    /// Moving objects cannot move past barrier.
    /// </summary>
    public class Wall
    {

        /// <summary>
        /// Sets and returns ID of this wall object
        /// </summary>
        [JsonPropertyName("wall")]
        public int ID { get; set; }

        /// <summary>
        /// Returns first set of coorindates to determine wall location.
        /// </summary>
        /// 
        [JsonPropertyName("p1")]
        public Point2D P1 { get; set; } = new Point2D();

        ///<summary>
        /// Returns second set of coorindates to determine wall location.
        /// </summary>
        [JsonPropertyName("p2")]
        public Point2D P2 { get; set; } = new Point2D();
    }

    /// <summary>
    /// Stores Dicrionaries of Walls, Foods, and Food objects.
    /// </summary>
    public class World
    {
        /// <summary>
        /// Set of food objects for world to handle
        /// </summary>
        public Dictionary<int, Wall> WallObjects { get; set; }

        /// <summary>
        /// Set of food objects for world to handle
        /// </summary>
        public Dictionary<int, Food> FoodObjects { get; set; }

        /// <summary>
        /// Set of all snake clients connected
        /// </summary>
        public Dictionary<int, Snake> SnakeClients { get; set; }

        /// <summary>
        /// New instance of a world object
        /// </summary>
        public World()
        {
            WallObjects = new Dictionary<int, Wall>();
            FoodObjects = new Dictionary<int, Food>();
            SnakeClients = new Dictionary<int, Snake>();
        }
    }
}
