using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Numerics;

namespace Server
{
    class Server
    {
        public static int MaxPlayers { get; private set; }
        public static int MaxBullets = 100;
        public static int Port { get; private set; }
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
        public static Dictionary<int, Bullet> bullets = new Dictionary<int, Bullet>();
        public delegate void PacketHandler(int _fromClient, Packet _packet);
        public static Dictionary<int, PacketHandler> packetHandlers;

        private static TcpListener tcpListener;
        private static UdpClient udpListener;

        public static void Start(int _maxPlayers, int _port)
        {
            MaxPlayers = _maxPlayers;
            Port = _port;

            Console.WriteLine("Starting server up...");
            InitializeServerData();

            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

            udpListener = new UdpClient(Port);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            Console.WriteLine("Server started on port." + Port);
        }

        private static void TCPConnectCallback(IAsyncResult _result)
        {
            TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
            Console.WriteLine("incoming connection " + _client.Client.RemoteEndPoint);

            for (int i = 1; i <= MaxPlayers; i++)
            {
                if (clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(_client);
                    return;
                }
            }

            Console.WriteLine(_client.Client.RemoteEndPoint + " failed to connect: Server full!");
        }

        private static void UDPReceiveCallback(IAsyncResult _result)
        {
            try
            {
                
                IPEndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
                byte[] _data = udpListener.EndReceive(_result, ref clientEP);
                udpListener.BeginReceive(UDPReceiveCallback, null);

                if (_data.Length < 4)
                {
                    return;
                }

                using (Packet _packet = new Packet(_data))
                {
                    int _clientId = _packet.ReadInt();

                    if (_clientId == 0)
                    {
                        return;
                    }

                    if (clients[_clientId].udp.UDPEndPoint == null)
                    {
                        // If this is a new connection
                        clients[_clientId].udp.Connect(clientEP);
                        return;
                    }

                    // had to chance it to string or it always returned false regardless of it being a match or not. not sure why.
                    if (clients[_clientId].udp.UDPEndPoint.ToString() == clientEP.ToString())
                    {
                        // Ensures that the client is not being impersonated by another by sending a false clientID
                        clients[_clientId].udp.HandleData(_packet);
                    }
                    else
                    {
                        Console.WriteLine("here");
                    }
                }
            }
            catch (Exception _ex)
            {
                Console.WriteLine("udp error: " + _ex);
            }
        }

        public static void SendUDPData(IPEndPoint clientEP, Packet _packet)
        {
            try
            {
                if (clientEP != null)
                {
                    udpListener.BeginSend(_packet.ToArray(), _packet.Length(), clientEP, null, null);
                }
            }
            catch (Exception _ex)
            {
                Console.WriteLine("Error sending data to" + clientEP +  "via UDP: " + _ex);
            }
        }
        private static void InitializeServerData()
        {
            // creates player dictonary.
            for (int i = 1; i <= MaxPlayers; i++)
            {
                clients.Add(i, new Client(i));
            }

            //creates bullet dictonary.
            for (int i = 1; i <= MaxBullets; i++)
            {
                bullets.Add(i, new Bullet(0, 0, new System.Numerics.Vector3(0,0,0), new Quaternion(0,0,0,0), new Vector3(0,0,0)));
            }
            
            //setup the client packet handler.
            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
                { (int)ClientPackets.playerRotation, ServerHandle.PlayerRotation },
                { (int)ClientPackets.playerPosition, ServerHandle.PlayerPosition },
                { (int)ClientPackets.bulletSpawned, ServerHandle.BulletSpawned },
                { (int)ClientPackets.bulletCollided, ServerHandle.BulletCollided },
                { (int)ClientPackets.sendHealth, ServerHandle.SendHealth },
                { (int)ClientPackets.sendScore, ServerHandle.SendScore },
                { (int)ClientPackets.increasePlayerScore, ServerHandle.IncreasePlayerScore },
                { (int)ClientPackets.resetGame, ServerHandle.ResetGame},
            };
            Console.WriteLine("Initialized packets.");
            
        }
    }
}
