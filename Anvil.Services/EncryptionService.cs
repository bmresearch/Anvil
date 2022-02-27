using Solnet.KeyStore;
using System;

namespace Anvil.Services
{
    /// <summary>
    /// An encryption service that exposes the <see cref="SecretKeyStoreService"/>.
    /// </summary>
    internal static class EncryptionService
    {
        /// <summary>
        /// The secret keystore.
        /// </summary>
        private static readonly SecretKeyStoreService SecretKeyStore = new();

        /// <summary>
        /// Encrypt the passed data and return the json string.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="data">The data to encrypt.</param>
        /// <param name="address">The address associated with the keystore.</param>
        /// <returns>The json string.</returns>
        internal static string Encrypt(string password, byte[] data, string address)
        {
            return SecretKeyStore.EncryptAndGenerateDefaultKeyStoreAsJson(password, data, address);
        }

        /// <summary>
        /// Decrypt the passed json string,
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="data">The json string to decrypt.</param>
        /// <returns>The data.</returns>
        internal static byte[] Decrypt(string password, string data)
        {
            try { return SecretKeyStore.DecryptKeyStoreFromJson(password, data); }
            catch (Exception) { return null; }
        }
    }
}
