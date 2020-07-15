using Contract;
using StreamJsonRpc;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketSample.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using (var webSocket = new ClientWebSocket())
            {
                Console.WriteLine("正在与服务端建立连接...");
                var uri = new Uri("ws://localhost:5000/rpc/greeter");
                await webSocket.ConnectAsync(uri, CancellationToken.None);
                Console.WriteLine("已建立连接");

                Console.WriteLine("开始向服务端发送消息...");
                var messageHandler = new WebSocketMessageHandler(webSocket);
                var greeterClient = JsonRpc.Attach<IGreeter>(messageHandler);
                var request = new HelloRequest { Name = "精致码农" };
                var response = await greeterClient.SayHelloAsync(request);
                Console.WriteLine($"收到来自服务端的响应：{response.Message}");

                Console.WriteLine("正在断开连接...");
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "断开连接", CancellationToken.None);
                Console.WriteLine("已断开连接");
            }

            Console.ReadKey();
        }
    }
}
