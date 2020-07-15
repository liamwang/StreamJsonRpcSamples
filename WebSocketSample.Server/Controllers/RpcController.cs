using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StreamJsonRpc;

namespace WebSocketSample.Server.Controllers
{
    public class RpcController : ControllerBase
    {
        private readonly ILogger<RpcController> _logger;
        private readonly GreeterServer _greeterServer;

        public RpcController(ILogger<RpcController> logger, GreeterServer greeterServer)
        {
            _logger = logger;
            _greeterServer = greeterServer;
        }

        [Route("/rpc/greeter")]
        public async Task<IActionResult> Greeter()
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                return new BadRequestResult();
            }

            _logger.LogInformation("等待客户端连接...");
            var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            _logger.LogInformation("已与客户端建立连接");

            var handler = new WebSocketMessageHandler(socket);

            using (var jsonRpc = new JsonRpc(handler, _greeterServer))
            {
                _logger.LogInformation("开始监听客户端消息...");
                jsonRpc.StartListening();
                await jsonRpc.Completion;
                _logger.LogInformation("客户端断开了连接");
            }

            return new EmptyResult();
        }
    }
}
