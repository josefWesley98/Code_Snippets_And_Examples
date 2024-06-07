using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

namespace Server
{
    class Bullet
    {
        public Player player;
        public int bulletId;
        public int playerId;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 target;
        public Bullet(int _bulletId, int _playerId, Vector3 _position, Quaternion _rotation, Vector3 _target)
        {
            bulletId = _bulletId;
            playerId = _playerId;
            position = _position;
            rotation = _rotation;
            target = _target;
        }
        public void SendBulletIntoGame(int _bulletId, int _playerId, Vector3 _position, Quaternion _rotation, Vector3 _target)
        {
            bulletId = _bulletId;
            position = _position;
            rotation = _rotation;
            target = _target;
            ServerSend.SpawnBullet(_playerId, bulletId, this);  
            
        }
    }
}
