namespace Gowtu
{
    public static class Random
    {
        private static System.Random random = new System.Random();

        /// <summary>
        /// Gets a random number between min and max value. Min and max are both inclusive
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float Range(float min, float max)
        {
            return (float)(random.NextDouble() * (max - min) + min);
        }

        public static float GetNext()
        {
            return (float)random.NextDouble();
        }
    }
}