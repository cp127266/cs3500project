// < copyright file = "ChatServer.cs" company = "UofU-CS3500" >
// Copyright (c) 2024 UofU-CS3500. All rights reserved.
// Implementation completed by team Nerd Ropes (Claire Park and Alissa Shaw) for CS3500, Nov. 2, 2024.
// </copyright>

using CS3500.Networking;
using System.Data;
using System.Diagnostics;
using System.Text;

namespace CS3500.Chatting;

/// <summary>
///   A simple ChatServer that handles clients separately and replies with a static message.
/// </summary>
public partial class ChatServer
{
    /// <summary>
    /// Private list to keep track of all connections
    /// </summary>
    //private static List<NetworkConnection> connections = new List<NetworkConnection>();

    private static Dictionary<string, NetworkConnection> connections = new Dictionary<string, NetworkConnection>();

    /// <summary>
    /// hard-coded port value to 11000
    /// </summary>
    private readonly static int port = 11_000;

    /// <summary>
    ///   The main program.
    /// </summary>
    /// <param name="args"> ignored. </param>
    /// <returns> A Task. Not really used. </returns>
    private static void Main(string[] args)
    {
        Server.StartServer(HandleConnect, port);
        //takes in a NetworkConnection called HandleConnect, and port number 11_000
        //starts a new tcp listener

        Console.Read(); // don't stop the program.
    }

    /// <summary>
    ///   <pre>
    ///     When a new connection is established, enter a loop that receives from and
    ///     replies to a client.
    ///   </pre>
    /// </summary>
    ///
    private static void HandleConnect(NetworkConnection connection)
    {
        // handle all messages until disconnect.
        try
        {
            var senderName = connection.ReadLine();

            lock (connections)
            {
                connections.Add(senderName, connection);
            }

            while (true)
            {
                var message = connection.ReadLine();

                lock (connections)
                {
                    //Sends message to every open connection
                    foreach (NetworkConnection nc in connections.Values)
                    {
                        nc.Send($"Client {senderName}: " + message);
                    }
                }
            }
        }
        catch (Exception)
        {
            Debug.WriteLine("Unable to handle connection.");
        }
    }
}