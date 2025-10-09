using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using InstagramAuto.Client.Models;
using InstagramAuto.Client.Services;
using System.Linq;
using System.Threading;
using System.Security.Cryptography;

namespace InstagramAuto.Client.Services
{
    /// <summary>
    /// Persian:
    ///     ?????????? ?????? ??????.
    ///     ??? ???? ????? ????? ? ??????? ??????? ??????? ???.
    /// English:
    ///     Session management implementation.
    ///     This class is responsible for storing and retrieving user sessions.
    /// </summary>
    public class SessionManager : ISessionManager
    {
        private readonly ILogger<SessionManager> _logger;
        private readonly IInstagramAutoClient _client;
        private readonly string _sessionsPath;
        private readonly Dictionary<string, AccountSession> _sessions;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly TimeSpan _sessionTimeout = TimeSpan.FromDays(30); // Sessions older than this will be cleaned up
        private readonly string _encryptionKey;

        public SessionManager(ILogger<SessionManager> logger, IInstagramAutoClient client)
        {
            _logger = logger;
            _client = client;
            _sessionsPath = Path.Combine(FileSystem.AppDataDirectory, "sessions.json");
            _encryptionKey = GetOrCreateEncryptionKey();
            _sessions = LoadSessionsFromDisk();
            
            // Start cleanup task for expired sessions
            _ = StartPeriodicCleanupAsync();
        }

        private string GetOrCreateEncryptionKey()
        {
            var keyPath = Path.Combine(FileSystem.AppDataDirectory, "session.key");
            if (File.Exists(keyPath))
            {
                return File.ReadAllText(keyPath);
            }

            var key = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
            File.WriteAllText(keyPath, key);
            return key;
        }

        /// <summary>
        /// Persian:
        ///     ?????? ???? ??????? ????.
        /// English:
        ///     Get all active sessions.
        /// </summary>
        public async Task<IEnumerable<AccountSession>> GetAllSessionsAsync()
        {
            await _lock.WaitAsync();
            try
            {
                await CleanupExpiredSessionsAsync();
                return _sessions.Values.ToList();
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Persian:
        ///     ?????? ??? ?? ?????.
        /// English:
        ///     Get session by id.
        /// </summary>
        public async Task<AccountSession> GetSessionAsync(string id)
        {
            await _lock.WaitAsync();
            try
            {
                if (!_sessions.TryGetValue(id, out var session))
                    throw new KeyNotFoundException($"Session {id} not found");

                if (IsSessionExpired(session))
                {
                    await RemoveSessionAsync(id);
                    throw new InvalidOperationException($"Session {id} has expired");
                }

                return session;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Persian:
        ///     ????? ???.
        /// English:
        ///     Save session.
        /// </summary>
        public async Task SaveSessionAsync(AccountSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            await _lock.WaitAsync();
            try
            {
                session.UpdatedAt = DateTimeOffset.UtcNow;
                _sessions[session.Id] = session;
                await SaveSessionsToDiskWithRetryAsync();
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Persian:
        ///     ??? ???.
        /// English:
        ///     Remove session.
        /// </summary>
        public async Task RemoveSessionAsync(string id)
        {
            await _lock.WaitAsync();
            try
            {
                if (_sessions.Remove(id))
                {
                    await SaveSessionsToDiskWithRetryAsync();
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Persian:
        ///     ????????? ???.
        /// English:
        ///     Refresh session.
        /// </summary>
        public async Task<AccountSession> RefreshSessionAsync(string id)
        {
            var session = await GetSessionAsync(id);

            try
            {
                // Export session to validate it's still active
                await _client.ExportAsync(id);
                session.UpdatedAt = DateTimeOffset.UtcNow;
                await SaveSessionAsync(session);
                return session;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to refresh session {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Persian:
        ///     ???????? ?????? ?? ????.
        /// English:
        ///     Load sessions from disk.
        /// </summary>
        private Dictionary<string, AccountSession> LoadSessionsFromDisk()
        {
            try
            {
                if (!File.Exists(_sessionsPath))
                    return new Dictionary<string, AccountSession>();

                var encryptedJson = File.ReadAllText(_sessionsPath);
                var json = DecryptData(encryptedJson);
                var sessions = JsonSerializer.Deserialize<List<AccountSession>>(json);
                
                // Filter out expired sessions during load
                return sessions?
                    .Where(s => !IsSessionExpired(s))
                    .ToDictionary(s => s.Id) 
                    ?? new Dictionary<string, AccountSession>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load sessions from disk");
                return new Dictionary<string, AccountSession>();
            }
        }

        private async Task SaveSessionsToDiskWithRetryAsync(int maxRetries = 3)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    var json = JsonSerializer.Serialize(
                        _sessions.Values,
                        new JsonSerializerOptions { WriteIndented = true }
                    );
                    
                    var encryptedJson = EncryptData(json);
                    var tempPath = _sessionsPath + ".tmp";
                    
                    // Write to temporary file first
                    await File.WriteAllTextAsync(tempPath, encryptedJson);
                    
                    // Then move to final destination (atomic operation)
                    File.Move(tempPath, _sessionsPath, true);
                    return;
                }
                catch (Exception ex)
                {
                    if (i == maxRetries - 1)
                    {
                        _logger.LogError(ex, "Failed to save sessions to disk after {RetryCount} retries", maxRetries);
                        throw;
                    }
                    await Task.Delay(100 * (i + 1)); // Exponential backoff
                }
            }
        }

        private string EncryptData(string data)
        {
            using var aes = System.Security.Cryptography.Aes.Create();
            aes.Key = Convert.FromBase64String(_encryptionKey);
            aes.GenerateIV();

            using var msEncrypt = new MemoryStream();
            msEncrypt.Write(aes.IV, 0, aes.IV.Length);

            using (var cryptoStream = new CryptoStream(msEncrypt, aes.CreateEncryptor(), CryptoStreamMode.Write))
            using (var writer = new StreamWriter(cryptoStream))
            {
                writer.Write(data);
            }

            return Convert.ToBase64String(msEncrypt.ToArray());
        }

        private string DecryptData(string encryptedData)
        {
            var fullCipher = Convert.FromBase64String(encryptedData);
            
            using var aes = System.Security.Cryptography.Aes.Create();
            var iv = new byte[16];
            Array.Copy(fullCipher, 0, iv, 0, 16);
            
            using var ms = new MemoryStream();
            ms.Write(fullCipher, 16, fullCipher.Length - 16);
            ms.Position = 0;
            
            aes.Key = Convert.FromBase64String(_encryptionKey);
            aes.IV = iv;

            using var cryptoStream = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var reader = new StreamReader(cryptoStream);
            return reader.ReadToEnd();
        }

        private bool IsSessionExpired(AccountSession session)
        {
            return DateTimeOffset.UtcNow - session.UpdatedAt > _sessionTimeout;
        }

        private async Task CleanupExpiredSessionsAsync()
        {
            var expiredSessions = _sessions.Values
                .Where(s => IsSessionExpired(s))
                .Select(s => s.Id)
                .ToList();

            foreach (var id in expiredSessions)
            {
                await RemoveSessionAsync(id);
                _logger.LogInformation("Removed expired session {Id}", id);
            }
        }

        private async Task StartPeriodicCleanupAsync()
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromHours(24));
                try
                {
                    await _lock.WaitAsync();
                    await CleanupExpiredSessionsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during periodic session cleanup");
                }
                finally
                {
                    _lock.Release();
                }
            }
        }
    }
}