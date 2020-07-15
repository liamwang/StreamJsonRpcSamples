using Contract;
using StreamJsonRpc;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpSample.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("正在与服务端建立连接...");
            var tcpClient = new TcpClient("localhost", 6000);
            var jsonRpcStream = tcpClient.GetStream();
            Console.WriteLine("已建立连接");

            var messageFormatter = new JsonMessageFormatter(Encoding.UTF8);
            var messageHandler = new LengthHeaderMessageHandler(jsonRpcStream, jsonRpcStream, messageFormatter);
            var greeterClient = JsonRpc.Attach<IGreeter>(messageHandler);

            Console.WriteLine("开始向服务端发送消息...");
            var request = new HelloRequest { Name = "精致码农" };
            var response = await greeterClient.SayHelloAsync(request);
            Console.WriteLine($"收到来自服务端的响应：{response.Message}");

            Console.WriteLine(response.Message);

            Console.WriteLine("正在断开连接...");
            jsonRpcStream.Close();
            Console.WriteLine("已断开连接");

            Console.ReadKey();
        }
    }
}
