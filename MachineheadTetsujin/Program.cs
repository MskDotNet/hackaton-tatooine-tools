using System;
using System.Threading;
using Pathfinder;

namespace MachineheadTetsujin
{
    class Program
    {
        static void Main(string[] args)
        {
            for (var c = 0; c < 30; c++)
                new Thread(() => Worker(c)).Start();


            Console.ReadKey();
            return;
            var bot = new Bot(
                new ApiClient("http://localhost:5000/raceapi/"), 
                "MachineheadTetsujin", "Redline", "spin");
            while (bot.CanContinue)
            {
                bot.MakeTurn();
            }
            Console.WriteLine(bot.Status);
        }

        public static void Worker(int num)
        {
            var login = "loadtest" + num;
            var password = "123";
            var client = new ApiClient("http://51.158.109.80:5000/raceapi/");
            try
            {
                client.Register(login, password);
            }
            catch
            {
                
            }
            var bot = new Bot(client, login, password, "maze");
            
            while (bot.CanContinue)
            {
                bot.MakeTurn();
            }
            Console.WriteLine(bot.Status);
        }
    }
}