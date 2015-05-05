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
            : this(new X509Certificate2(pfxPath, password))
        {
        }

        /// <summary>
        /// Creates a new instance of the secret provider using the certificate
        /// </summary>
        /// <param name="certificate">The certificate.</param>
        public SecretProvider(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException("certificate");
            }

            this.certificate = certificate;
        }

        /// <summary>
        /// Creates a new instance of the secret provider using a certificate thumbprint.
        /// The certificate will be loaded from the store using the thumbprint.
        /// </summary>
        /// <param name="thumbprint">The certificate thumbprint.</param>
        public SecretProvider(string thumbprint)
            : this(GetCertificate(thumbprint))
        {
        }

        /// <summary>
        /// Gets a certificate from the certificate store based on the provided thumbprint.
        /// </summary>
        /// <param name="thumbprint">The thumbprint.</param>
        /// <returns>A certificate, or null.</returns>
        private static X509Certificate2 GetCertificate(string thumbprint)
        {
            X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            certStore.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection certCollection = certStore.Certificates.Find(
                X509FindType.FindByThumbprint,
                thumbprint,
                false /* valid only */);

            X509Certificate2 cert = null;
            if (certCollection.Count > 0)
            {
                cert = certCollection[0];
            }

            certStore.Close();
            return cert;
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
