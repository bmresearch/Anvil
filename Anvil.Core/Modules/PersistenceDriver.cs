using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;

namespace Anvil.Core.Modules
{
    public class PersistenceDriver : IPersistenceDriver
    {
        /// <summary>
        /// File name extension for the state backup process.
        /// </summary>
        private const string BackupExtension = ".backup";

        private ILogger _logger { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="baseDirectory"></param>
        public PersistenceDriver(ILogger logger, string baseDirectory, string fileName)
        {
            _logger = logger;

            BaseDirectory = baseDirectory;
            FileName = fileName;
        }

        /// <summary>
        /// Combines file name with base directory to form the file path.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <returns>The file path.</returns>
        private string GetFilePath(string fileName)
            => Path.Combine(BaseDirectory, fileName);

        /// <inheritdoc cref="IPersistenceDriver.LoadState{T}()"/>
        public T LoadState<T>()
        {
            var backupPath = FileName + BackupExtension;
            var backupExists = File.Exists(GetFilePath(backupPath));
            var path = backupExists ? backupPath : GetFilePath(FileName);

            _logger.Log(LogLevel.Information, $"Loading state file: {path}");
            T state;
            try
            {
                var data = File.ReadAllText(path);
                state = JsonSerializer.Deserialize<T>(data);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Could not get state file. Exception: {ex.Message}");
                state = (T)Activator.CreateInstance(typeof(T));
            }
            return state;
        }

        /// <inheritdoc cref="IPersistenceDriver.SaveState{T}(T)"/>
        public void SaveState<T>(T state)
        {
            var path = GetFilePath(FileName);
            if (File.Exists(path)) BackupState(FileName);

            _logger.Log(LogLevel.Information, $"Persisting state file: {path}");

            var lines = JsonSerializer.Serialize(state);

            File.WriteAllText(GetFilePath(FileName), lines);
            InvalidateState(FileName + BackupExtension);
        }

        /// <inheritdoc cref="IPersistenceDriver.MigrateState{T}(string)"/>
        public void MigrateState<T>(string newBaseDirectory)
        {
            var oldBaseDirectory = BaseDirectory;
            T state = LoadState<T>();

            BaseDirectory = newBaseDirectory;

            SaveState<T>(state);

            BaseDirectory = oldBaseDirectory;

            InvalidateState(FileName);

            BaseDirectory = newBaseDirectory;
        }

        /// <summary>
        /// Invalidates the current state backup file.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        private void InvalidateState(string fileName)
        {
            var path = GetFilePath(fileName);
            _logger.Log(LogLevel.Information, $"Invalidating state file: {path}");
            if (File.Exists(path))
                File.Delete(path);
        }

        /// <summary>
        /// Stores a copy of the current state.
        /// </summary>
        /// <param name="fileName">The file to backup.</param>
        private void BackupState(string fileName)
        {
            _logger.Log(LogLevel.Information, "Performing state backup.");

            try
            {
                var data = File.ReadAllText(GetFilePath(fileName));
                File.WriteAllTextAsync(GetFilePath(fileName + BackupExtension), data);
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Critical, $"An error occurred performing state backup: {e.Message}");
            }
        }

        public string FileName { get; private set; }

        public string BaseDirectory { get; private set; }

        public string DefaultBaseDirectory { get; } = AppContext.BaseDirectory;
    }
}
