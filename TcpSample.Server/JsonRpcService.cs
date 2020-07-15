using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StreamJsonRpc;

namespace TcpSample.Server
{
    public class JsonRpcService : BackgroundService
    {
        private const int _port = 6000;

        private IConnectionListener _connectionListener;
        private readonly GreeterServer _greeterServer;
        private readonly ILogger<JsonRpcService> _logger;
        private readonly IConnectionListenerFactory _connectionListenerFactory;

        private readonly ConcurrentDictionary<string, (ConnectionContext Context, Task ExecutionTask)> 
            _connections = new ConcurrentDictionary<string, (ConnectionContext, Task)>();

        public JsonRpcService(
            ILogger<JsonRpcService> logger,
            GreeterServer greeterServer,
            IConnectionListenerFactory connectionListenerFactory)
        {
            _logger = logger;
            _greeterServer = greeterServer;
            _connectionListenerFactory = connectionListenerFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var endPoint = new IPEndPoint(IPAddress.Loopback, _port);
            _connectionListener = await _connectionListenerFactory.BindAsync(endPoint, stoppingToken);
            _logger.LogInformation($"RPC 服务已绑定端口：{_port}");

            while (true)
            {
                _logger.LogInformation("等待客户端连接...");
                var connectionContext = await _connectionListener.AcceptAsync(stoppingToken);
                if (connectionContext == null)
                {
                    break;
                }
                _logger.LogInformation($"已与客户端建立连接 {connectionContext.ConnectionId}");

                _connections[connectionContext.ConnectionId] = (connectionContext, AcceptAsync(connectionContext));
            }

            _logger.LogInformation("正在结束 RPC 服务...");

            var connectionsExecutionTasks = new List<Task>(_connections.Count);

            foreach (var connection in _connections)
            {
                _logger.LogWarning($"尝试取消 {connection.Key} 连接上的任务...");
                connectionsExecutionTasks.Add(connection.Value.ExecutionTask);
                connection.Value.Context.Abort();
            }

            _logger.LogInformation("等待正在执行的任务完成...");
            await Task.WhenAll(connectionsExecutionTasks);
            _logger.LogInformation("所有任务已结束");
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _connectionListener.DisposeAsync();
        }

        private async Task AcceptAsync(ConnectionContext connectionContext)
        {
            try
            {
                await Task.Yield();
                var messageFormatter = new JsonMessageFormatter(Encoding.UTF8);
                var messageHandler = new LengthHeaderMessageHandler(connectionContext.Transport, messageFormatter);

                using (var jsonRpc = new JsonRpc(messageHandler, _greeterServer))
                {
                    _logger.LogInformation($"开始监听连接 {connectionContext.ConnectionId} 上的消息...");
                    jsonRpc.StartListening();

                    await jsonRpc.Completion;
                }

                await connectionContext.ConnectionClosed.WaitAsync();
            }
            catch (ConnectionResetException)
            { }
            catch (ConnectionAbortedException)
            { }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"连接 {connectionContext.ConnectionId} 出现异常");
            }
            finally
            {
                await connectionContext.DisposeAsync();
                _connections.TryRemove(connectionContext.ConnectionId, out _);
                _logger.LogInformation($"连接 {connectionContext.ConnectionId} 已断开");
            }
        }
    }
}
