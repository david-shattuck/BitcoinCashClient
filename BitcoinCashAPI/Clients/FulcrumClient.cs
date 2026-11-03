using BitcoinCash.API.Clients.Interfaces;
using BitcoinCash.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;

namespace BitcoinCash.API.Clients
{
    // Electrum Cash protocol over TLS: https://electrum-cash-protocol.readthedocs.io/
    public class FulcrumClient(ILogger<FulcrumClient> logger) : IFulcrumClient, IHostedService, IDisposable
    {
        private const int Port = 50002;
        private const string ClientName = "bch-api";
        private static readonly string[] ProtocolVersions = ["1.5", "1.6"];
        private static readonly string[] HostNames =
        [
            "bch.imaginary.cash",
            "electroncash.de",
            "bch.loping.net",
            "bch.soul-dev.com",
            "blackie.c3-soft.com",
            "bch0.kister.net"
        ];

        private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(15);

        private readonly ILogger<FulcrumClient> _logger = logger;

        private readonly SemaphoreSlim _connectLock = new(1, 1);
        private readonly SemaphoreSlim _writeLock = new(1, 1);
        private readonly ConcurrentDictionary<int, TaskCompletionSource<JObject>> _pending = new();

        private TcpClient? _tcpClient;
        private SslStream? _stream;
        private CancellationTokenSource? _listenCts;
        private int _requestId;

        private bool IsConnected => _tcpClient?.Connected == true && _stream != null;

        public async Task<List<utxo>?> GetUtxos(List<string> addresses)
        {
            var results = await Task.WhenAll(addresses.Distinct().Select(GetUtxos));

            if (results.Any(r => r == null))
                return null;

            return [.. results.SelectMany(r => r!)];
        }

        private async Task<List<utxo>?> GetUtxos(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return null;

            var cashAddr = address.Contains(':') ? address : $"bitcoincash:{address}";

            try
            {
                var response = await Request("blockchain.address.listunspent", [cashAddr, "include_tokens"]);

                if (response?["error"] is JObject error)
                {
                    _logger.LogWarning("Fulcrum listunspent failed for {address}: {error}", cashAddr, error.ToString(Formatting.None));
                    return null;
                }

                var utxos = response?["result"]?.ToObject<List<utxo>>();

                utxos?.ForEach(u => u.address = address);

                return utxos;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Fulcrum listunspent failed for {address}", cashAddr);
                return null;
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await EnsureConnected();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Unable to connect to Fulcrum at startup");
                }
            }, CancellationToken.None);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Disconnect();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Disconnect();
            _connectLock.Dispose();
            _writeLock.Dispose();
            GC.SuppressFinalize(this);
        }

        private async Task EnsureConnected(CancellationToken cancellationToken = default)
        {
            if (IsConnected)
                return;

            await _connectLock.WaitAsync(cancellationToken);

            try
            {
                if (IsConnected)
                    return;

                Disconnect();

                foreach (var hostName in HostNames.OrderBy(_ => Random.Shared.Next()))
                {
                    try
                    {
                        await Connect(hostName, cancellationToken);
                        _logger.LogInformation("Connected to Fulcrum server {hostName}", hostName);
                        return;
                    }
                    catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogWarning("Unable to connect to Fulcrum server {hostName}: {message}", hostName, ex.Message);
                        Disconnect();
                    }
                }

                throw new InvalidOperationException("Unable to connect to any Fulcrum server");
            }
            finally
            {
                _connectLock.Release();
            }
        }

        private async Task Connect(string hostName, CancellationToken cancellationToken)
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(ConnectTimeout);

            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(hostName, Port, timeoutCts.Token);

            var stream = new SslStream(_tcpClient.GetStream());
            await stream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions { TargetHost = hostName }, timeoutCts.Token);
            _stream = stream;

            _listenCts = new CancellationTokenSource();
            _ = Task.Run(() => Listen(stream, _listenCts.Token));

            var handshake = await Send("server.version", [ClientName, ProtocolVersions]);

            if (handshake["error"] is JObject error)
                throw new InvalidOperationException($"Handshake rejected: {error.ToString(Formatting.None)}");
        }

        private async Task<JObject?> Request(string method, object[] args)
        {
            await EnsureConnected();
            return await Send(method, args);
        }

        private async Task<JObject> Send(string method, object[] args)
        {
            var stream = _stream ?? throw new InvalidOperationException("Not connected to Fulcrum");

            var id = Interlocked.Increment(ref _requestId);
            var tcs = new TaskCompletionSource<JObject>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pending[id] = tcs;

            try
            {
                var request = new { jsonrpc = "2.0", id, method, @params = args };
                var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request) + "\n");

                await _writeLock.WaitAsync();

                try
                {
                    await stream.WriteAsync(bytes);
                    await stream.FlushAsync();
                }
                finally
                {
                    _writeLock.Release();
                }

                return await tcs.Task.WaitAsync(RequestTimeout);
            }
            finally
            {
                _pending.TryRemove(id, out _);
            }
        }

        private async Task Listen(SslStream stream, CancellationToken cancellationToken)
        {
            try
            {
                using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);

                while (!cancellationToken.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync(cancellationToken);

                    if (line == null)
                        break;

                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    try
                    {
                        var message = JObject.Parse(line);

                        if (message["id"]?.Type == JTokenType.Integer && _pending.TryGetValue(message["id"]!.Value<int>(), out var tcs))
                            tcs.TrySetResult(message);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Unable to parse Fulcrum message");
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Fulcrum connection lost");
            }
            finally
            {
                if (ReferenceEquals(stream, _stream))
                    Disconnect();
            }
        }

        private void Disconnect()
        {
            _listenCts?.Cancel();
            _listenCts?.Dispose();
            _listenCts = null;

            _stream?.Dispose();
            _stream = null;

            _tcpClient?.Dispose();
            _tcpClient = null;

            foreach (var id in _pending.Keys)
                if (_pending.TryRemove(id, out var tcs))
                    tcs.TrySetException(new IOException("Fulcrum connection closed"));
        }
    }
}
