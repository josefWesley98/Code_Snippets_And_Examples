using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class GameLogic
    {
        public static void Update()
        {
            foreach(Client _client in Server.clients.Values)
            {
                if(_client.player != null)
                {
                    _client.player.Update();
               
                    //Console.WriteLine("Doing player update");
                }
            }
            //foreach (Bullet _bullet in Server.bullets.Values)
            //{
            //    if (_bullet.bulletId != 0)
            //    {
            //        //_bullet.Update();
            //    }
            //}
            ThreadManager.UpdateMain();
        }
    }
}
