using Gowtu;

namespace GowtuApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Configuration config = new Configuration();
            config.width = 512;
            config.height = 512;
            config.title = "Gowtu";
            config.flags = WindowFlags.VSync;

            using(var pack = new AssetPack("assets.dat", "assets.dat"))
            {
                if(pack.Loaded)
                    config.iconData = pack.GetFileBuffer("Resources/Textures/logo.png");
            }

            Application application = new Application(config);
            application.Load += OnLoaded;
            application.Run();
        }

        private static void OnLoaded()
        {
            GameObject g = new GameObject();
            g.AddComponent<GameManager>();
        }
    }
}