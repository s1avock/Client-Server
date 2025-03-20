using System;
using System.IO;
using System.Net.Sockets;

using System.Text;
using System.Threading.Tasks;

class Client
{
    private const string ServerIp = "127.0.0.1";
    private const int Port = 12345;

    static async Task Main(string[] args)
    {
        using (TcpClient client = new TcpClient(ServerIp, Port))
        using (var networkStream = client.GetStream())
        using (var reader = new StreamReader(networkStream, Encoding.UTF8))
        using (var writer = new StreamWriter(networkStream, Encoding.UTF8) { AutoFlush = true })
        {
            Console.WriteLine("Подключено к серверу.");

            string command;
            while (true)
            {
                Console.WriteLine("Введите команду (GET_TIME, GET_DIR, EXEC <command>, UPLOAD <file_path> или 'exit' для выхода):");
                command = Console.ReadLine();

                if (command.Equals("exit", StringComparison.OrdinalIgnoreCase))
                    break;

                await writer.WriteLineAsync(command);
                string response = await reader.ReadLineAsync();
                Console.WriteLine($"Ответ от сервера: {response}");
            }
        }
    }
}

