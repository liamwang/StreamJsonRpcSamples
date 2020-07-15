using Contract;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace TcpSample.Server
{
    public class GreeterServer : IGreeter
    {
        private readonly ILogger<GreeterServer> _logger;

        public GreeterServer(ILogger<GreeterServer> logger)
        {
            _logger = logger;
        }

        public Task<HelloResponse> SayHelloAsync(HelloRequest request)
        {
            _logger.LogInformation("收到并回复了客户端消息");
            return Task.FromResult(new HelloResponse
            {
                Message = $"您好， {request.Name}！"
            });
        }
    }
}
