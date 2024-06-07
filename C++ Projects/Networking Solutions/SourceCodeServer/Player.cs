using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Security.Cryptography;

namespace Server
{
   class Player
   {
        public int id;
        public string username;
        public int health;
        public int damage;
        public int score;
        public int kills;
        public int deaths;
        public int targetId;
        public bool useQuadraticPrediction = false;
        public Vector3 position;
        public Vector3 predictedPos;
        public Quaternion rotation;
        public int positionCalls = 0;
        public Vector3[] positionHolder = { new Vector3(0,0,0), new Vector3(0, 0, 0), new Vector3(0, 0, 0) };
        public double[] timePosHolder = { 0, 0, 0 };
        public Player(int _id, string _username, Vector3 _position, int _health, int _damage, int _score)
        {
            id = _id;
            username = _username;
            position = _position;
            predictedPos = new Vector3(0, 0, 0);
            rotation = Quaternion.Identity;
            health = 100;
            damage = _damage;
            score = _score;
            targetId = 0;
            kills = 0;
            deaths = 0;
          
        }
        public void Update()
        {
            Send();
        }

        private void Send()
        {
            if (positionHolder[0] != new Vector3(0,0,0) && positionHolder[1] != new Vector3(0, 0, 0) && positionHolder[2] != new Vector3(0, 0, 0))
            {
                useQuadraticPrediction = true;
                predictedPos = QuadraticPrediction(positionHolder[0], timePosHolder[0], positionHolder[1], timePosHolder[1], positionHolder[2], timePosHolder[2]);
            }
            else
            {
                useQuadraticPrediction = false;
            }

            ServerSend.PlayerRotation(this);
            ServerSend.PlayerHealth(this);
            ServerSend.ScoreUpdate(this);
            ServerSend.PlayerPosition(this, useQuadraticPrediction);
            
        }
        public void SetRotation(Quaternion _rotation)
        {
            rotation = _rotation;
        }
        public void SetPosition(Vector3 _position)
        {
            position = _position;
        }
        public void SetTarget(int _targetId)
        {
            targetId = _targetId;
        }
        public void SetScore(int _score)
        {
            score = _score;
        }
        public void SetHealth(int _health)
        {
            health = _health;
        }
        public Vector3 QuadraticPrediction(Vector3 pos0, double timePos0, Vector3 pos1, double timePos1, Vector3 pos2, double timePos2)
        {
            // my attempt at adding quadratic prediction in unity, it works but causes some bugs, im sure i made a  small error somewhere thats causing it but ran
            // out of time to go test over and over to figure it out.
            Vector3 position_diff;
            position_diff = new Vector3(Math.Abs(pos0.X - pos1.X), Math.Abs(pos0.Y - pos1.Y), Math.Abs(pos0.Z - pos1.Z));
            double xy_diff = Math.Sqrt(Math.Pow(position_diff.X, 2.0f) + Math.Pow(position_diff.Y, 2.0f));
            double time_diff = Math.Abs(timePos1 - timePos0);
            double speed = Math.Abs(xy_diff / time_diff);

            Vector3 position_diff2;
            position_diff2 = new Vector3(Math.Abs(pos1.X - pos2.X), Math.Abs(pos1.Y - pos2.Y), Math.Abs(pos1.Z - pos2.Z));
            double xy_diff2 = Math.Sqrt(Math.Pow(position_diff2.X, 2.0) + Math.Pow(position_diff2.Y, 2.0));
            double time_diff2 = Math.Abs(timePos2 - timePos1);
            double speed2 = Math.Abs(xy_diff2 / time_diff2);

            double speed_diff = Math.Abs(speed - speed2);
            double time_between_pos = Math.Abs(time_diff - time_diff2);

            double acc = 0;
            double displacement = (speed2 * time_diff2) + 0.5 * acc * Math.Pow(time_diff2, 2.0);

            Vector3 quad_pos = new Vector3(pos2.X + (float)displacement, pos2.Y + (float)displacement, pos2.Z + (float)displacement);

            return quad_pos;
        }
        public void ResetPlayer()
        {
            score = 0;
            kills = 0;
            deaths = 0;
            health = 100;
            targetId = 0;
        }
            
    }
}
