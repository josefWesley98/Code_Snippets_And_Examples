using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class ServerHandle
    {
        public static void WelcomeReceived(int _fromClient, Packet _packet)
        {
            int _clientIdCheck = _packet.ReadInt();
            string _username = _packet.ReadString();

            Console.WriteLine(Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint + " connected successfully and is now player " + _fromClient);
            if(_fromClient != _clientIdCheck)
            {
                Console.WriteLine(_username + "has got the wrong ID");
            }
            Server.clients[_fromClient].SendIntoGame(_username);
        }

        public static void PlayerRotation(int _fromClient, Packet _packet)
        {
            int _id = _packet.ReadInt();
            Quaternion _rotation = _packet.ReadQuaternion();

            Server.clients[_id].player.SetRotation(_rotation);   
        }

        public static void PlayerPosition(int _fromClient, Packet _packet)
        {
            int _id = _packet.ReadInt();
            float _time = _packet.ReadFloat();
            Vector3 _position = _packet.ReadVector3();
       

            Server.clients[_id].player.SetPosition(_position);
            //stores positions for quadratic predicition
            Server.clients[_id].player.positionHolder[Server.clients[_id].player.positionCalls] = _position;
            Server.clients[_id].player.timePosHolder[Server.clients[_id].player.positionCalls] = _time;
            
            if (Server.clients[_id].player.positionCalls == 2)
            {
                Server.clients[_id].player.positionCalls = 0;
            }

            Server.clients[_id].player.positionCalls += 1;
        }
        public static void SendScore(int _fromClient, Packet _packet)
        {
            int _id = _packet.ReadInt();
            int _score = _packet.ReadInt();
            Server.clients[_id].player.score = _score;
        }
        public static void SendHealth(int _fromClient, Packet _packet)
        {
            int _id = _packet.ReadInt();
            int _health = _packet.ReadInt();
            Server.clients[_id].player.health = _health;
        }
        public static void BulletSpawned(int _fromClient, Packet _packet)
        {
            int _bulletId = _packet.ReadInt();
            int _playerId = _packet.ReadInt();
            Vector3 _position = _packet.ReadVector3();
            Quaternion _rotation = _packet.ReadQuaternion();
            Vector3 _target = _packet.ReadVector3();

            //spawns the bullets and gives it the velocity and target same as the players who shot it so its created with the same position and rotation and velocity
            //and allowed to follow its path to conclusion the same on all players versions of the game.
            Server.bullets[_bulletId].target = _target;
            Server.bullets[_bulletId].bulletId = _bulletId;
            Server.bullets[_bulletId].playerId = _playerId;
            Server.bullets[_bulletId].SendBulletIntoGame(_bulletId, _playerId, _position, _rotation, _target);

        }
        public static void BulletCollided(int _fromClient, Packet _packet)
        {
            int _bulletId = _packet.ReadInt();
            int _targetId = _packet.ReadInt();
            int _id = _packet.ReadInt();
            bool isPlayer = _packet.ReadBool();
            bool isKillingBlow = _packet.ReadBool();
            bool isRespawning = _packet.ReadBool();

            //resets bullet in dictionary so its ready to be used later.
            Server.bullets[_bulletId].playerId = 0;
            Server.bullets[_bulletId].bulletId = 0;
            Server.bullets[_bulletId].position = new Vector3(0, 0, 0);
            Server.bullets[_bulletId].rotation = new Quaternion(0, 0, 0, 0);
            if(!isRespawning)
            {
                if(_targetId != 0 && isPlayer && _targetId != _id)
                {
                    if(isKillingBlow)
                    {
                        ServerSend.DamagePlayer(_id, _targetId, 25, true);
                        Server.clients[_id].player.score += 25;
                        Server.clients[_id].player.kills += 1;
                        Server.clients[_targetId].player.deaths += 1;
                    }
                    else
                    {
                        ServerSend.DamagePlayer(_fromClient, _targetId, 25, false);
                    }
                }
            }
    
        }
        public static void ResetGame(int _fromClient, Packet _packet)
        {
            int _id = _packet.ReadInt();
            Server.clients[_id].player.ResetPlayer();
        }
        public static void IncreasePlayerScore(int _fromClient, Packet _packet)
        {
            int _id = _packet.ReadInt();
            int _scoreInrease = _packet.ReadInt();

            Server.clients[_id].player.score += _scoreInrease;
        }
     
    }
}
