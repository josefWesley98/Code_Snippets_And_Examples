using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Reflection;

namespace Server
{
    class Client
    {
        public static int dataBufferSize = 4096;

        public int id;
        
        public Player player;

        public Bullet bullet;
        
        public TCP tcp;

        public UDP udp;

       
        public Client(int _clientId)
        {
            id = _clientId;
            tcp = new TCP(id);
            udp = new UDP(id);
        }

        public class TCP
        {
            public TcpClient socket;

            private readonly int id;
            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;

            public TCP(int _id)
            {
                id = _id;
            }

            public void Connect(TcpClient _socket)
            {
                socket = _socket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                stream = socket.GetStream();

                receivedData = new Packet();
                receiveBuffer = new byte[dataBufferSize];

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

                ServerSend.Welcome(id, "Welcome to the server!");
            }

         
            public void SendData(Packet _newPacket)
            {
                try
                {
                    if (socket != null)
                    {
                        stream.BeginWrite(_newPacket.ToArray(), 0, _newPacket.Length(), null, null); // Send data to appropriate client
                    }
                }
                catch (Exception _ex)
                {
                    Console.WriteLine($"Error sending data to player {id} via TCP: {_ex}");
                }
            }

     
            private void ReceiveCallback(IAsyncResult _result)
            {
                try
                {
                    int _byteLength = stream.EndRead(_result);
                    if (_byteLength <= 0)
                    {
                        Server.clients[id].Disconnect();
                        return;
                    }

                    byte[] _data = new byte[_byteLength];
                    Array.Copy(receiveBuffer, _data, _byteLength);

                    receivedData.Reset(HandleData(_data));
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception _ex)
                {
                    Console.WriteLine($"Error receiving TCP data: {_ex}");
                    Server.clients[id].Disconnect();
                }
            }

        
            private bool HandleData(byte[] _data)
            {
                int _newPacketLength = 0;

                receivedData.SetBytes(_data);

                if (receivedData.UnreadLength() >= 4)
                {
    
                    _newPacketLength = receivedData.ReadInt();
                    if (_newPacketLength <= 0)
                    {
                        
                        return true; 
                    }
                }

                while (_newPacketLength > 0 && _newPacketLength <= receivedData.UnreadLength())
                {
                    
                    byte[] _newPacketBytes = receivedData.ReadBytes(_newPacketLength);
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet _newPacket = new Packet(_newPacketBytes))
                        {
                            int _newPacketId = _newPacket.ReadInt();
                            Server.packetHandlers[_newPacketId](id, _newPacket); 
                        }
                    });
                    _newPacketLength = 0; 
                    if (receivedData.UnreadLength() >= 4)
                    {
                        _newPacketLength = receivedData.ReadInt();
                        if (_newPacketLength <= 0)
                        {
                            return true;
                        }
                    }
                }

                if (_newPacketLength <= 1)
                {
                    return true;
                }

                return false;
            }
            public void Disconnect()
            {
                socket.Close();
                stream = null;
                receivedData = null;
                receiveBuffer = null;
                socket = null;
            }
        }

        public class UDP
        {
            public IPEndPoint UDPEndPoint;

            private int id;

            public UDP(int _id)
            {
            
                id = _id;
            }

            public void Connect(IPEndPoint _UDPEndPoint)
            {
                UDPEndPoint = _UDPEndPoint;
            }

            public void SendData(Packet _newPacket)
            {
                Server.SendUDPData(UDPEndPoint, _newPacket);
            }

            public void HandleData(Packet _newPacketData)
            {
                int _newPacketLength = _newPacketData.ReadInt();
                byte[] _newPacketBytes = _newPacketData.ReadBytes(_newPacketLength);

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _newPacket = new Packet(_newPacketBytes))
                    {
                        int _newPacketId = _newPacket.ReadInt();
                        Server.packetHandlers[_newPacketId](id, _newPacket);
                    }
                });
            }
            public void Disconnect()
            {
                UDPEndPoint = null;
            }
        }

        public void SendIntoGame(string _playerName)
        {
            player = new Player(id, _playerName, new Vector3(0, 0, 0), 50, 25, 0);

          
            foreach (Client _client in Server.clients.Values)
            {
                if (_client.player != null)
                {
                    if (_client.id != id)
                    {
                        ServerSend.SpawnPlayer(id, _client.player);
                    }
                }
            }
            foreach (Client _client in Server.clients.Values)
            {
                if (_client.player != null)
                {
                    ServerSend.SpawnPlayer(_client.id, player);
                }
            }
        }

        private void Disconnect()
        {
            Console.WriteLine(tcp.socket.Client.RemoteEndPoint + " has disconnected.");

            player = null;

            tcp.Disconnect();
            udp.Disconnect();
        }
    }
}
