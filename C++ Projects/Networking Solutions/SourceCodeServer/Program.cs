using System;
using System.Threading;
namespace Server
{
    class Program
    {
        private static bool isRunning = false;
        static void Main(string[] args)
        {
            Console.Title = "Joes Server";
            isRunning = true;
            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();
            Server.Start(4, 5566);
        }
        private static void MainThread()
        {
            DateTime DT = DateTime.UtcNow;

            while(isRunning)
            {
                while(DT < DateTime.UtcNow)
                {
                    GameLogic.Update();
                    DT = DT.AddMilliseconds(Constants.mSperTick); 
                    if(DT > DateTime.UtcNow)
                    {
                        Thread.Sleep(DT - DateTime.UtcNow);
                    }
                }
            }
        }
    }
}

