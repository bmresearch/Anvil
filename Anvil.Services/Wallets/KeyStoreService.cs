using Anvil.Services.Store;
using Anvil.Services.Store.State;
using Anvil.Services.Wallets.Enums;
using Microsoft.Extensions.Logging;
using Solnet.KeyStore;
using Solnet.Wallet;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Anvil.Services.Wallets
{
    /// <summary>
    /// The keystore service.
    /// </summary>
    public class KeyStoreService
    {
        /// <summary>
        /// The secret keystore.
        /// </summary>
        private static readonly SecretKeyStoreService SecretKeyStore = new();

        /// <summary>
        /// The solana keystore.
        /// </summary>
        private static readonly SolanaKeyStoreService SolanaKeyStore = new();

        /// <summary>
        /// The wallet service.
        /// </summary>
        private IWalletService _walletService;

        /// <summary>
        /// The key store state.
        /// </summary>
        private IKeyStore _keyStore;

        /// <summary>
        /// The logger.
        /// </summary>
        private ILogger Logger { get; }

        /// <summary>
        /// The user-provided password.
        /// </summary>
        private string _password;

        /// <summary>
        /// Initialize the keystore service.
        /// </summary>
        /// <param name="walletService">The wallet service.</param>
        public KeyStoreService(ILogger logger, IWalletService walletService, IKeyStore keyStore)
        {
            _walletService = walletService;
            _keyStore = keyStore;
            _keyStore.OnUpdate += _keyStore_OnUpdate;
            Logger = logger;
            Logger.Log(LogLevel.Information, $"Initializing {ToString()}");
        }

        /// <summary>
        /// Processes an update to the key store. This is used to encrypt the keystore and leave no sensitive information exposed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event.</param>
        private async void _keyStore_OnUpdate(object sender, Store.Events.KeyStoreUpdateEventArgs e)
        {
            if (_keyStore.IsEncrypted)
            {
                var w = _walletService.Wallets.FirstOrDefault(x => x.SubWalletType == SubWalletType.DerivationIndex);
                var mainWalletBytes = JsonSerializer.SerializeToUtf8Bytes(_keyStore.Wallet);
                var mainWalletJson =
                    await EncryptKeyStore(_password, mainWalletBytes, w.Address);
                var state = new KeyStoreState()
                {
                    IsEncrypted = _keyStore.IsEncrypted,
                    WalletExists = _keyStore.WalletExists,
                    WalletJson = mainWalletJson,
                };
                _keyStore.Persist(state);
            }
        }

        /// <summary>
        /// Encrypts a keystore.
        /// </summary>
        /// <param name="password">The registration password.</param>
        /// <param name="data">The registration data.</param>
        /// <param name="address">The solana address corresponding to the first account of the wallet.</param>
        public async Task<string> EncryptKeyStore(string password, byte[] data, string address)
        {
            return await Task.Run(() =>
            {
                Logger.Log(LogLevel.Information, $"Encrypting keystore.");
                var keyStoreJson = Encrypt(password, data, address);
                Logger.Log(LogLevel.Information, $"Successfully encrypted keystore.");
                return keyStoreJson;
            });
        }

        /// <summary>
        /// Decrypts the existing keystore in the application's state.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns>The restored wallet state.</returns>
        public async Task<byte[]> DecryptKeyStore(string password)
        {
            if (string.IsNullOrEmpty(password)) return null;
            return await Task.Run(() =>
            {
                Logger.Log(LogLevel.Information, $"Decrypting keystore.");
                byte[] decryptedBytes;
                try
                {
                    decryptedBytes = Decrypt(password, _keyStore.WalletJson);
                    if (decryptedBytes == null)
                    {
                        Logger.Log(LogLevel.Error, $"Failed to decrypt keystore json.");
                        return null;
                    }
                }
                catch (Exception e)
                {
                    Logger.Log(LogLevel.Error, $"Failed to decrypt keystore. Exception caught: {e.Message}");
                    return null;
                }

                Logger.Log(LogLevel.Information, $"Successfully decrypted keystore.");
                return decryptedBytes;
            });
        }

        /// <summary>
        /// Decrypts the existing keystore in the application's state and initializes the wallets.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns>The restored wallet state.</returns>
        public async Task<bool> DecryptKeyStoreAndInitializeWallets(string password)
        {
            IsProcessing = true;

            RaiseEvent("Decrypting keystore.", KeyStoreServiceState.Decoding);
            var decryptedKeyStoreBytes = await DecryptKeyStore(password);
            if (decryptedKeyStoreBytes == null)
            {
                Logger.Log(LogLevel.Information, $"Failed to decrypt keystore.");
                IsProcessing = false;
                return false;
            }

            _password = password;

            RaiseEvent("Preparing wallets..", KeyStoreServiceState.Decoding);

            Logger.Log(LogLevel.Information, $"Successfully decrypted keystore, recovering wallets...");
            InitializeWallets(decryptedKeyStoreBytes);
            IsProcessing = false;
            RaiseEvent("Wallets loaded.", KeyStoreServiceState.Done);
            return true;
        }

        /// <summary>
        /// Initializes wallets from a newly generated mnemonic and if a password is provided, encrypts the keystore.
        /// </summary>
        /// <param name="mnemonic">The mnemonic.</param>
        /// <param name="password">The password.</param>
        public async Task InitializeWallet(string mnemonic, string password)
        {
            IsProcessing = true;

            RaiseEvent("Generating Keys.", KeyStoreServiceState.Initializing);

            _keyStore.AddWallet(mnemonic);

            RaiseEvent("Preparing wallet...", KeyStoreServiceState.Initializing);

            var ws = _walletService.AddWallet(_keyStore.Wallet);            

            if (!string.IsNullOrEmpty(password))
            {
                _password = password;
                RaiseEvent("Encrypting keystore..", KeyStoreServiceState.Initializing);
                var mainWalletBytes = JsonSerializer.SerializeToUtf8Bytes(_keyStore.Wallet);
                var mainWalletJson =
                    await EncryptKeyStore(password, mainWalletBytes, ws.Address);
                _keyStore.WalletExists = true;
                _keyStore.IsEncrypted = true;
                _keyStore.WalletJson = mainWalletJson;
            }
            else
            {
                _keyStore.WalletExists = true;
                _keyStore.IsEncrypted = false;
                _keyStore.Wallet = _keyStore.Wallet;
            }

            IsProcessing = false;
            RaiseEvent("Finished.", KeyStoreServiceState.Done);
        }

        /// <summary>
        /// Initializes wallets from a serialized pre-existing main wallet.
        /// </summary>
        /// <param name="mainWalletBytes">The main wallet bytes.</param>
        private void InitializeWallets(byte[] mainWalletBytes)
        {
            _keyStore.Wallet = JsonSerializer.Deserialize<WalletStore>(mainWalletBytes);
            if (_keyStore.Wallet == null)
            {
                Logger.Log(LogLevel.Information, $"Something went wrong deserializing keystore.");
                return;
            }

            _walletService.AddWallet(_keyStore.Wallet);

            foreach (var store in _keyStore.Wallet.DerivationIndexWallets)
            {
                _walletService.AddWallet(store);
            }

            foreach (var store in _keyStore.Wallet.PrivateKeyWallets)
            {
                _walletService.AddWallet(store);
            }
        }

        /// <summary>
        /// Import wallet from private key file.
        /// </summary>
        /// <param name="path">The path to the private key file.</param>
        /// <param name="passphrase">The passphrase used to generate the key pair.</param>
        public void ImportPrivateKeyFile(string path, string passphrase = "")
        {
            _walletService.AddWallet(new PrivateKeyWallet(path));
        }


        /// <summary>
        /// Import wallet from private key file.
        /// </summary>
        /// <param name="path">The path to the private key file.</param>
        /// <param name="passphrase">The passphrase used to generate the key pair.</param>
        public static Wallet LoadPrivateKeyFile(string path, string passphrase = "")
        {
            return SolanaKeyStore.RestoreKeystoreFromFile(path, passphrase);
        }

        /// <summary>
        /// Encrypt the passed data and return the json string.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="data">The data to encrypt.</param>
        /// <param name="address">The address associated with the keystore.</param>
        /// <returns>The json string.</returns>
        private static string Encrypt(string password, byte[] data, string address)
        {
            return SecretKeyStore.EncryptAndGenerateDefaultKeyStoreAsJson(password, data, address);
        }

        /// <summary>
        /// Decrypt the passed json string,
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="data">The json string to decrypt.</param>
        /// <returns>The data.</returns>
        private static byte[] Decrypt(string password, string data)
        {
            try { return SecretKeyStore.DecryptKeyStoreFromJson(password, data); }
            catch (Exception) { return null; }
        }


        /// <summary>
        /// A boolean which defines if the wallet service is processing encryption or decryption.
        /// </summary>
        public bool IsProcessing { get; set; }

        /// <summary>
        /// Whether a wallet exists.
        /// </summary>
        public bool WalletExists => _keyStore.WalletExists;

        /// <summary>
        /// Whether the keystore is encrypted.
        /// </summary>
        public bool IsEncrypted => _keyStore.IsEncrypted;

        private void RaiseEvent(string message, KeyStoreServiceState newState)
            => OnStartupStateChanged?.Invoke(this, new Events.KeyStoreServiceStateChangedEventArgs(message, newState));


        /// <summary>
        /// Triggered when the startup state of the keystore chagnes.
        /// </summary>
        public event EventHandler<Events.KeyStoreServiceStateChangedEventArgs> OnStartupStateChanged;
    }
}
