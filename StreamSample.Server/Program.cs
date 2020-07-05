using StreamJsonRpc;
using System;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace StreamSample.Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            int clientId = 1;

            while (true)
            {
                var stream = new NamedPipeServerStream("StringJsonRpc",
                   PipeDirection.InOut,
                   NamedPipeServerStream.MaxAllowedServerInstances,
                   PipeTransmissionMode.Byte,
                   PipeOptions.Asynchronous);

                Console.WriteLine("等待客户端连接...");
                await stream.WaitForConnectionAsync();
                Console.WriteLine($"已与客户端 #{clientId} 建立连接");

                _ = ResponseAsync(stream, clientId);

                clientId++;
            }
        }

        static async Task ResponseAsync(NamedPipeServerStream stream, int clientId)
        {
            var jsonRpc = JsonRpc.Attach(stream, new GreeterServer());
            await jsonRpc.Completion;
            Console.WriteLine($"客户端 #{clientId} 的已断开连接");
            jsonRpc.Dispose();
            await stream.DisposeAsync();
        }
    }

    public class GreeterServer
    {
        public string SayHello(string name)
        {
            Console.WriteLine($"收到【{name}】的问好，并回复了他");
            return $"您好，{name}！";
        }
    }
}
