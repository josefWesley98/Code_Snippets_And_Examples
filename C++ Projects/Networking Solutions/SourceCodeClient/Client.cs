using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
public class Client : MonoBehaviour
{
    public static Client instance;
    public static int dataBufferSize = 4096;
    public string ip = "127.0.0.1";
    public int port = 5566;
    public int myId = 0;
    public TCP tcp;
    public UDP udp;
    private bool isConnected = false;
    private delegate void PacketHandler(Packet _newPacket);
    private static Dictionary<int, PacketHandler> packetHandlers;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != null)
        {
            Debug.Log("Instance already exists, destroying this object");
            Destroy(this);
        }
    }

   
    private void Start()
    {
        tcp = new TCP();
        udp = new UDP();
    }

    public void ConnectToServer()
    {
        InitialiseClientData();
        isConnected = true;
        tcp.Connect();
    }
    private void OnApplicationQuit()
    {
        Disconnect();
    }
   public class TCP
   {
        public TcpClient socket;
        private NetworkStream stream; 
        private Packet receivedData;
        private byte[] receiveBuffer;
        public void Connect()
        {
            socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize

            };
            
            receiveBuffer = new byte[dataBufferSize];
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
        }

        private void ConnectCallback(IAsyncResult _result)
        {
            socket.EndConnect(_result);
            
            if(!socket.Connected)
            {
                return;
            }
            
            stream = socket.GetStream();

            receivedData = new Packet();
            
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
           
        }

        public void SendData(Packet _newPacket)
        {
            try
            {
                if(socket != null)
                {
                    stream.BeginWrite(_newPacket.ToArray(), 0, _newPacket.Length(), null, null);
                }
            }
            catch(Exception ex)
            {
                Debug.Log(ex);
            }
        }
        private void ReceiveCallback(IAsyncResult _result)
        {
             try
                {
                    int _byteLength = stream.EndRead(_result);
                    if(_byteLength <= 0)
                    {
                        instance.Disconnect();
                        return;
                    }

                    byte[] _dataBlock = new byte[_byteLength];
                    Array.Copy(receiveBuffer, _dataBlock, _byteLength);
                   
                    receivedData.Reset(HandleData(_dataBlock));
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

                }
                catch
                {
                   Disconnect();
                }
        }
       
        private bool HandleData(byte[] _dataBlock)
        {
            //Note: Do more research as this was really tough to understand and implement.
            //Note: handles data been sent between potentially multiple deliveries and if the data in received data variable should be cleared or not depending on data left to be read.
            int _newPacketLength = 0;

            receivedData.SetBytes(_dataBlock);

           
            if(receivedData.UnreadLength() >= 4)
            {
                _newPacketLength = receivedData.ReadInt();
                if(_newPacketLength <= 0)
                {
                    return true;
                }
            }
           
            while(_newPacketLength > 0 && _newPacketLength <= receivedData.UnreadLength())
            {
              
                byte[] _newPacketBytes = receivedData.ReadBytes(_newPacketLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using(Packet _newPacket = new Packet(_newPacketBytes))
                    {
                      
                        int _newPacketId = _newPacket.ReadInt();
                        //Note: Invoke packetHandlers and pass it the packet instance. delegate function + dictionary. watch the end of the video explaining about using delegates functions if this still doesnt work.
                        packetHandlers[_newPacketId](_newPacket);
                    }
                });

                _newPacketLength = 0;
                if(receivedData.UnreadLength() >= 4)
                {
                   
                    _newPacketLength = receivedData.ReadInt();
                    if(_newPacketLength <= 0)
                    {
                        return true;
                    }
                }
            }
           
            if(_newPacketLength <= 1)
            {
                return true;
            }
            //Note: If packet length is greater than one don't reset received data because there is still partial packet to be read.
            return false;
        }
        private void Disconnect()
        {
            instance.Disconnect();
            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }
   }
  
  public class UDP
  {
    public UdpClient socket;
    public IPEndPoint clientEndPoint;

    public UDP()
    {
        clientEndPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
    }
    public void Connect(int _localPort)
    {
        socket = new UdpClient(_localPort);

        socket.Connect(clientEndPoint);
        socket.BeginReceive(ReceiveCallback, null);

        using(Packet _newPacket = new Packet())
        {
            SendData(_newPacket);   
        }
    }

    public void SendData(Packet _newPacket)
    {
        try
        {
            //only one instance of udp client can be active and we need udp to know which player is sending the information so we pass id as int so we know where the data packet is coming from.
            _newPacket.InsertInt(instance.myId);
            if(socket != null)
            {
                socket.BeginSend(_newPacket.ToArray(), _newPacket.Length(), null, null);
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
    }
    private void ReceiveCallback(IAsyncResult _result)
    {
        try
        {
            byte[] _dataBlock = socket.EndReceive(_result, ref clientEndPoint);
            socket.BeginReceive(ReceiveCallback, null);

            if(_dataBlock.Length < 4)
            {
                instance.Disconnect();
                return;
            }
            HandleData(_dataBlock);
        }
        catch
        {
            Disconnect();
        }

    }
    private void HandleData(byte[] _dataBlock)
    {
        using(Packet _newPacket = new Packet(_dataBlock))
        {
            int _newPacketLength = _newPacket.ReadInt();
            _dataBlock = _newPacket.ReadBytes(_newPacketLength);
        }

        ThreadManager.ExecuteOnMainThread(() =>
        {
            using(Packet _newPacket = new Packet(_dataBlock))
            {
                int _newPacketId = _newPacket.ReadInt();
                packetHandlers[_newPacketId](_newPacket);
            }
        });
    }
    private void Disconnect()
    {
        instance.Disconnect();
        clientEndPoint = null;
        socket = null;
    }
    
  }
   private void InitialiseClientData()
   {
        // initalise the dictionary with the packets.
        packetHandlers = new Dictionary<int , PacketHandler>()
        {
            {(int)ServerPackets.welcome, ClientHandle.Welcome },
            {(int)ServerPackets.spawnPlayer, ClientHandle.SpawnPlayer},
            {(int)ServerPackets.playerPosition, ClientHandle.PlayerPosition},
            {(int)ServerPackets.playerRotation, ClientHandle.PlayerRotation},
            {(int)ServerPackets.spawnBulletForEnemies, ClientHandle.SpawnBulletForEnemies},
            {(int)ServerPackets.playerHealth, ClientHandle.PlayerHealth},
            {(int)ServerPackets.damagePlayer, ClientHandle.DamagePlayer},
            {(int)ServerPackets.scoreUpdate, ClientHandle.scoreUpdate},
        };
        Debug.Log("Initialised packets.");
   }
    private void Disconnect()
    {
        // handles disconnections from the server so it doesnt crash
        if(isConnected)
        {
            isConnected = false;
            tcp.socket.Close();
            udp.socket.Close();
            Debug.Log("Disconnected.");
        }
    }
}
