using OpenTK.Mathematics;

namespace Gowtu
{
    public static class Debug
    {
        public static void Log(string format, params object[] arg)
        {
            System.Console.WriteLine(format, arg);
        }

        public static void DrawLine(Vector3 p1, Vector3 p2, Color color)
        {
            LineRenderer.AddToDrawList(p1, p2, color);
        }
    }
}