namespace SabbathText.V1
{
    using System;

    /// <summary>
    /// This class represents an authentication key scheme
    /// </summary>
    public class AuthKey
    {
        private static Random rand = new Random();

        /// <summary>
        /// Gets or sets the function to create new authentication keys.
        /// This is used for testing purpose. See <see cref="Create"/> for its usage.
        /// </summary>
        public static Func<string> CreateFunc { get; set; }
        
        /// <summary>
        /// Creates a new authentication key
        /// </summary>
        /// <returns>A new authentication key</returns>
        public static string Create()
        {
            Func<string> customCreate = CreateFunc;
            if (customCreate != null)
            {
                return customCreate();
            }

            char[] key = new char[8];

            for (int i = 0; i < key.Length; i++)
            {
                key[i] = (char)rand.Next((int)'0', (int)'9' + 1);
            }

            return new string(key);
        }
    }
}
