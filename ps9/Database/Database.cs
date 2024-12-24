/// Authors: Nerd Ropes (Claire Park and Alissa Shaw) for FA2024 CS3500, Dec. 4, 2024 
/// Description: Sends and receives information about the games and players from the database. 

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using Google.Protobuf.WellKnownTypes;
using MySql.Data.MySqlClient;

namespace DatabaseController
{
    /// <summary>
    /// Deals with sending and recieving data from the database.
    /// </summary>
    public class Database
    {
        /// <summary>
        /// The connection string.
        /// Your uID login name serves as both your database name and your uid
        /// </summary>
        public const string connectionString = "server=atr.eng.utah.edu;" +
                                               "database=u1413130;" +
                                               "uid=u1413130;" +
                                               "password=NerdRopes123;";

        /// <summary>
        /// the ID of the current game that the client is playing. 
        /// The game begins when the client connects and ends when the client disconnects.
        /// </summary>
        public int gID ;

        /// <summary>
        /// Returns sql table of all active games
        /// </summary>
        public string AllGames()
        {
            string games = "<tbody>";

            // Connect to the DB
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    // Open a connection
                    conn.Open();

                    // Create a command
                    MySqlCommand command = conn.CreateCommand();
                    command.CommandText = "select * from Games";

                    // Execute the command and cycle through the DataReader object
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            games += $"<tr><td><a href=\"/games?gid={reader["ID"]}\">{reader["ID"]}</a>" +
                                        $"</td><td>{reader["StartTime"]}" +
                                        $"</td><td>{reader["EndTime"]}" +
                                        $"</td></tr>";
                        }
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            games += "</tbody";
            return games;
        }

        /// <summary>
        /// Returns sql table of all players within specified game id
        /// </summary>
        /// <param name="gameID"></param>
        public string PlayersOfGame(int gameID)
        {
            string players = "<tbody>";
            // Connect to the DB
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    // Open a connection
                    conn.Open();

                    // Create a command
                    MySqlCommand command = conn.CreateCommand();
                    command.CommandText = $"select * from Players where gID = {gameID}";


                    // Execute the command and cycle through the DataReader object
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            players += $"<tr><td>{reader["pID"]}" +
                                    $"</td><td>{reader["pName"]}" +
                                    $"</td><td>{reader["MaxScore"]}" +
                                    $"</td><td>{reader["EnterTime"]}" +
                                    $"</td><td>{reader["LeaveTime"]}" +
                                    $"</td></tr>";
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            players += "</tbody>";
            return players;
        }

        /// <summary>
        /// Returns a List of the pIDs in the current game.
        /// </summary>
        /// <param name="gameID"></param>
        /// <returns></returns>
        public List<int> GetPlayerIDs()
        {

             List<int> ID = new List<int>();
            // Connect to the DB
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    // Open a connection
                    conn.Open();

                    // Create a command
                    MySqlCommand command = conn.CreateCommand();
                    command.CommandText = $"select * from Players where gID={gID}";


                    // Execute the command and cycle through the DataReader object
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ID.Add(int.Parse(reader["pID"].ToString()));
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return ID;
        }

        /// <summary>
        /// Executes the command to insert a new row into the Games table when the client connects to the server.
        /// Also updates the gID field.
        /// </summary>
        public void AddGame()
        {
            // Connect to the DB
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    // Open a connection
                    conn.Open();

                    string start = DateTime.Now.ToString("yyyy-MM-dd H:mm:ss");

                    // Insert a new row into the Games table
                    MySqlCommand insertCommand = conn.CreateCommand();
                    insertCommand.CommandText = $"insert into Games (StartTime, EndTime) values (\"{start}\", NULL);";
                    insertCommand.ExecuteNonQuery();


                    // get the game ID from the last inseted row for the current game ID
                    string gameID = string.Empty;
                    MySqlCommand selectCommand = conn.CreateCommand();
                    selectCommand.CommandText = "select last_insert_id();";


                    // Execute the command and cycle through the DataReader object
                    using (MySqlDataReader reader = selectCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //update gID with the ID of the row last inserted into
                            gID = int.Parse(reader[0].ToString());
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }


        /// <summary>
        /// Sends command to database to enter the end time of the game when the client disconnects.
        /// </summary>
        public void EndGame()
        {
            // Connect to the DB
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    // Open a connection
                    conn.Open();

                    string end = DateTime.Now.ToString("yyyy-MM-dd H:mm:ss");

                    // Update the end time in the Games table
                    MySqlCommand updateGamesCommand = conn.CreateCommand();
                    updateGamesCommand.CommandText = $"update Games set EndTime=\"{end}\" where ID = {gID};";
                    updateGamesCommand.ExecuteNonQuery();

                    // Update the leave times for players with NULL for the leave time
                    MySqlCommand updatePlayersCommand = conn.CreateCommand();
                    updatePlayersCommand.CommandText = $"update Players set LeaveTime=\"{end}\" where gID={gID} && LeaveTime IS NULL;";
                    updatePlayersCommand.ExecuteNonQuery();

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        /// <summary>
        /// Update the Players table when a player that is not the client has disconnected.
        /// </summary>
        /// <param name="pID"></param>
        public void LeaveGame(int pID)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    // Open a connection
                    conn.Open();

                    string leave = DateTime.Now.ToString("yyyy-MM-dd H:mm:ss");

                    // Update the leave times for players with NULL for the leave time
                    MySqlCommand updatePlayersCommand = conn.CreateCommand();
                    updatePlayersCommand.CommandText = $"update Players set LeaveTime=\"{leave}\" where gID={gID} && pID={pID};";
                    updatePlayersCommand.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        /// <summary>
        /// Adds a row into the Players table with the given player ID
        /// </summary>
        /// <param name="playerID"></param>
        public void AddPlayer(int playerID, string playerName)
        {
            // Connect to the DB
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    // Open a connection
                    conn.Open();

                    string enter = DateTime.Now.ToString("yyyy-MM-dd H:mm:ss");

                    // Insert row into Players table
                    MySqlCommand command = conn.CreateCommand();
                    command.CommandText = $"insert into Players (gID, pID, pName, MaxScore, EnterTime, LeaveTime) values " +
                                          $"({gID}, {playerID}, \"{playerName}\", 0, \"{enter}\", NULL);";
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        /// <summary>
        /// Returns the max score stored in the database for the player.
        /// </summary>
        /// <param name="playerID"></param>
        /// <returns></returns>
        public int GetMaxScore(int playerID)
        {
            int max = 0;
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    // Open a connection
                    conn.Open();

                    // Create a command
                    MySqlCommand command = conn.CreateCommand();
                    command.CommandText = $"select * from Players where gID={gID} && pID={playerID}";


                    // Execute the command and cycle through the DataReader object
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            max = (int.Parse(reader["MaxScore"].ToString()));
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            return max;
        }


        /// <summary>
        /// Update Player table with new MaxScore for given pID
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="max"></param>
        public void SetMaxScore(int playerID, int max)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    // Open a connection
                    conn.Open();

                    // Update the player with new max score
                    MySqlCommand command = conn.CreateCommand();
                    command.CommandText = $"update Players set MaxScore={max} where gID={gID} && pID={playerID};";
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}