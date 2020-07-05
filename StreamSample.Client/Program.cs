using StreamJsonRpc;
using System;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace StreamSample.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var stream = new NamedPipeClientStream(".",
                "StringJsonRpc",
                PipeDirection.InOut,
                PipeOptions.Asynchronous);

            Console.WriteLine("正在连接服务器...");
            await stream.ConnectAsync();
            Console.WriteLine("已建立连接！");

            Console.WriteLine("我是精致码农，开始向服务端问好...");
            var jsonRpc = JsonRpc.Attach(stream);
            var message = await jsonRpc.InvokeAsync<string>("SayHello", "精致码农");
            Console.WriteLine($"来自服务端的响应：{message}");

            Console.ReadKey();
        }
    }
}
