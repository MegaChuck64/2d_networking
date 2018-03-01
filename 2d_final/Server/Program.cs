using NetworkTools;
using Lidgren.Network;
using System;
using System.Collections.Generic;

namespace Server
{
    class Program
    {

        // Server object
        static NetServer Server;
        // Configuration object
        static NetPeerConfiguration Config;

        // Object that can be used to store and read messages
        static NetIncomingMessage inc;

        static List<Messenger> clients;

        static void Init()
        {
            // Create new instance of configs. Parameter is "application Id". It has to be same on client and server.
            Config = new NetPeerConfiguration("game");

            // Set server port
            Config.Port = 14242;

            // Max client amount
            Config.MaximumConnections = 200;

            // Enable New messagetype. Explained later
            Config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);

            // Create new server based on the configs just defined
            Server = new NetServer(Config);

            // Start it
            Server.Start();

            // Eh..
            Console.WriteLine("Server Started");

            // Create list of "Characters" ( defined later in code ). This list holds the world state. Character positions
             clients = new List<Messenger>();

            // Check time
            DateTime time = DateTime.Now;

            // Create timespan of 30ms
            TimeSpan timetopass = new TimeSpan(0, 0, 0, 0, 30);

            // Write to con..
            Console.WriteLine("Waiting for new connections");
        }
        static void Main(string[] args)
        {
            

            while (true)
            {
                // Server.ReadMessage() Returns new messages, that have not yet been read.
                // If "inc" is null -> ReadMessage returned null -> Its null, so dont do this :)
                if ((inc = Server.ReadMessage()) != null)
                {
                    // Theres few different types of messages. To simplify this process, i left only 2 of em here
                    switch (inc.MessageType)
                    {
                        case NetIncomingMessageType.Error:
                            OnError();
                            break;
                        case NetIncomingMessageType.ConnectionApproval:
                            OnConnectionApproval();
                            break;
                        case NetIncomingMessageType.Data:
                            OnData();
                            break;
                        default:
                            break;
                    }
                }
            }
        }


        static void OnConnectionApproval()
        {
            // Read the first byte of the packet
            // ( Enums can be casted to bytes, so it be used to make bytes human readable )
            if (inc.ReadByte() == (byte)Packets.Type.LOGIN)
            {
                Console.WriteLine("Incoming LOGIN");

                // Approve clients connection ( Its sort of agreenment. "You can be my client and i will host you" )
                inc.SenderConnection.Approve();
            }

            clients.Add(new Messenger(inc.ToString(), inc.SenderConnection));

            // Create message, that can be written and sent
            NetOutgoingMessage outmsg = Server.CreateMessage();

            // first we write byte
            outmsg.Write((byte)Packets.Type.MESSAGE);

            // then int
            outmsg.Write(clients.Count + " clients connected.");

            // iterate trought every character ingame
            foreach (Messenger cl in clients)
            {
                // This is handy method
                // It writes all the properties of object to the packet
                outmsg.WriteAllProperties(cl);
            }


            // Send message/packet to all connections, in reliably order, channel 0
            // Reliably means, that each packet arrives in same order they were sent. Its slower than unreliable, but easyest to understand
            Server.SendMessage(outmsg, inc.SenderConnection, NetDeliveryMethod.ReliableOrdered, 0);

            // Debug
            Console.WriteLine("Approved new connection and updated clients list");


        }

        static void OnData()
        {

        }

        static void OnError()
        {

        }
    }
}
