using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Numerics;

namespace Server
{
    class ServerSend
    {
        private static void SendTCPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].tcp.SendData(_packet);
        }
        private static void SendTCPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for(int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }
        private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if(i != _exceptClient)
                {
                    Server.clients[i].tcp.SendData(_packet);
                }
            }
        }
        private static void SendUDPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].udp.SendData(_packet);
        }
        private static void SendUDPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.clients[i].udp.SendData(_packet);
            }
        }
        private static void SendUDPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if (i != _exceptClient)
                {
                    Server.clients[i].udp.SendData(_packet);
                }
            }
        }
        public static void Welcome(int _toClient, string _msg)
        {
            //Note: using the "using" functionality it means it will disponse of it self protecting memory.
            using (Packet _packet = new Packet((int)ServerPackets.welcome))
            {
                _packet.Write(_msg);
                _packet.Write(_toClient);

                SendTCPData(_toClient, _packet);
            }
        }
        public static void SpawnPlayer(int _toClient, Player _player)
        {
            using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.username);
                _packet.Write(_player.position);
                _packet.Write(_player.rotation);

                SendTCPData(_toClient, _packet);
            }
        }
        public static void SpawnBullet(int _toClient,int _bulletId ,Bullet _bullet)
        {
            using (Packet _packet = new Packet((int)ServerPackets.spawnBulletForEnemies))
            {

                _packet.Write(Server.bullets[_bulletId].bulletId);

                _packet.Write(Server.bullets[_bulletId].playerId);
 
                _packet.Write(Server.bullets[_bulletId].position);

                _packet.Write(Server.bullets[_bulletId].rotation);

                _packet.Write(Server.bullets[_bulletId].target);

                SendTCPDataToAll(Server.bullets[_bulletId].playerId, _packet);
               
               
            }
        }

        public static void PlayerPosition(Player _player,bool _useQuadraticPrediction)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerPosition))
            {
                if(!_useQuadraticPrediction)
                {
                    _packet.Write(_player.id);
                    _packet.Write(_player.position);
                }
                else
                {
                    _packet.Write(_player.id);
                    _packet.Write(_player.predictedPos);
                   
                }

                SendUDPDataToAll(_player.id,_packet);
            }
        }
        public static void PlayerRotation(Player _player)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerRotation))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.rotation);
            
                SendUDPDataToAll(_player.id, _packet);
            }
        }
        public static void PlayerHealth(Player _player)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerHealth))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.health);

                SendUDPDataToAll(_packet);
            }
        }
        public static void DamagePlayer(int _id, int _targetId, int _damage, bool _isKillingBlow)
        {
            using (Packet _packet = new Packet((int)ServerPackets.damagePlayer))
            {
                _packet.Write(_id);
                _packet.Write(_targetId);
                _packet.Write(_damage);
                _packet.Write(_isKillingBlow);
     
                SendTCPData(_targetId,_packet);
            }
        }
        public static void ScoreUpdate(Player _player)
        {
            using (Packet _packet = new Packet((int)ServerPackets.scoreUpdate))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.score);
                _packet.Write(_player.kills);
                _packet.Write(_player.deaths);
      
                SendUDPDataToAll(_packet);
            }
        }
    }
}
