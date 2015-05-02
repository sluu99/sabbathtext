namespace SabbathText
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// A provider to encrypt and decrypt text.
    /// </summary>
    public class SecretProvider
    {
        private X509Certificate2 certificate;

        /// <summary>
        /// Creates a new instance of the secret provider.
        /// </summary>
        /// <param name="pfxPath">The path to the PFX certificate file.</param>
        /// <param name="password">The password for the certificate.</param>
        public SecretProvider(string pfxPath, string password)
        {
            this.certificate = new X509Certificate2(pfxPath, password);
        }

        /// <summary>
        /// Encrypt a plain text string.
        /// </summary>
        /// <param name="plainText">The plain text string.</param>
        /// <returns>The encrypted string.</returns>
        public string Encrypt(string plainText)
        {
            if (plainText == null)
            {
                return null;
            }

            RSACryptoServiceProvider publicKeyProvider =
                (RSACryptoServiceProvider)this.certificate.PublicKey.Key;

            byte[] buffer = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(publicKeyProvider.Encrypt(buffer, false));
        }

        /// <summary>
        /// Decrypt an encrypted text string.
        /// </summary>
        /// <param name="encryptedText">The encrypted text string.</param>
        /// <returns>The plain text string.</returns>
        public string Decrypt(string encryptedText)
        {
            if (encryptedText == null)
            {
                return null;
            }

            if (this.certificate.HasPrivateKey == false)
            {
                throw new NotSupportedException("Cannot decrypt without a private key from the certificate.");
            }

            RSACryptoServiceProvider privateKeyProvider =
                (RSACryptoServiceProvider)this.certificate.PrivateKey;

            byte[] buffer = Convert.FromBase64String(encryptedText);
            return Encoding.UTF8.GetString(privateKeyProvider.Decrypt(buffer, false));
        }
    }
}
