namespace SabbathText.Tests
{
    /// <summary>
    /// Test helper
    /// </summary>
    public static class TestHelper
    {
        private static int currentNumber = 1000000000;
        private static object padLock = new object();

        /// <summary>
        /// Generates a random US phone number
        /// </summary>
        /// <returns>A US phone number</returns>
        public static string GetUSPhoneNumber()
        {
            lock (padLock)
            {
                currentNumber++;
                return "+1" + currentNumber.ToString();
            }
        }
    }
}
