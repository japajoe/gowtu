using System;
using Gowtu;

namespace SnakeGame
{
    class Program
    {
        static void Main(string[] args)
        {
            Application application = new Application(512, 512, "Snake", WindowFlags.VSync);
            application.Load += OnApplicationLoaded;
            application.Run();
        }

        private static void OnApplicationLoaded()
        {
            GameObject g = new GameObject();

            g.AddComponent<SnakeGame>();
        }
    }
}