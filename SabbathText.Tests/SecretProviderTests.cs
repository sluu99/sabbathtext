namespace SabbathText.Tests
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Security.Cryptography;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test cases for secret providers
    /// </summary>
    [TestClass]
    public class SecretProviderTests
    {
        private static readonly string CertPath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            "devcert.pfx");

        /// <summary>
        /// Sanity test for Encrypt/Decrypt
        /// </summary>
        [TestMethod]
        public void SecretProvider_SanityTest()
        {
            SecretProvider provider = new SecretProvider(CertPath, password: "dev");
            
            for (int i = 0; i < 100; i++)
            {
                string str = Guid.NewGuid().ToString();
                Assert.AreEqual(str, provider.Decrypt(provider.Encrypt(str)));
            }
        }

        /// <summary>
        /// Test the secret provider when an invalid password is used.
        /// </summary>
        [TestMethod]
        public void SecretProvider_InvalidPassword()
        {
            try
            {
                SecretProvider provider = new SecretProvider(CertPath, password: "invalidpassword");
                Assert.Fail("An exception is expected when the wrong password is used.");
            }
            catch (CryptographicException ex)
            {
                StringAssert.Contains(ex.Message, "password");
            }
        }
    }
}
