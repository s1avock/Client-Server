using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Server
{
    private const int Port = 12345;

    static async Task Main(string[] args)
    {
        TcpListener server = new TcpListener(IPAddress.Any, Port);
        server.Start();
        Console.WriteLine($"Сервер запущен на порту {Port}");

        while (true)
        {
            TcpClient client = await server.AcceptTcpClientAsync();
            _ = HandleClientAsync(client);
        }
    }

    private static async Task HandleClientAsync(TcpClient client)
    {
        using (client)
        using (var networkStream = client.GetStream())
        using (var reader = new StreamReader(networkStream, Encoding.UTF8))
        using (var writer = new StreamWriter(networkStream, Encoding.UTF8) { AutoFlush = true })
        {
            string command;
            while ((command = await reader.ReadLineAsync()) != null)
            {
                LogRequest(command);
                string response = await HandleCommandAsync(command, reader);
                await writer.WriteLineAsync(response);
                LogResponse(response);
            }
        }
    }

    private static async Task<string> HandleCommandAsync(string command, StreamReader reader)
    {
        string response;

        if (command.Equals("GET_TIME", StringComparison.OrdinalIgnoreCase))
        {
            response = DateTime.Now.ToString("G");
        }
        else if (command.Equals("GET_DIR", StringComparison.OrdinalIgnoreCase))
        {
            response = string.Join(", ", Directory.GetFileSystemEntries(Directory.GetCurrentDirectory()));
        }
        else if (command.StartsWith("EXEC ", StringComparison.OrdinalIgnoreCase))
        {
            string systemCommand = command.Substring(5);
            response = ExecuteCommand(systemCommand);
        }
        else if (command.StartsWith("UPLOAD ", StringComparison.OrdinalIgnoreCase))
        {
            var fileName = command.Substring(7);
            response = await ReceiveFileAsync(fileName, reader);
        }
        else
        {
            response = "Неизвестная команда.";
        }

        return response;
    }

    private static async Task<string> ReceiveFileAsync(string fileName, StreamReader reader)
    {
        using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
        {
            await reader.BaseStream.CopyToAsync(fs);
        }
        return $"Файл {fileName} успешно загружен.";
    }

    private static string ExecuteCommand(string command)
    {
        try
        {
            using (var process = new System.Diagnostics.Process())
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = "/c " + command;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                return string.IsNullOrEmpty(output) ? error : output;
            }
        }
        catch (Exception ex)
        {
            return "Ошибка выполнения команды: " + ex.Message;
        }
    }

    private static void LogRequest(string request)
    {
        File.AppendAllText("server_log.txt", $"Запрос: {request} {DateTime.Now}\n");
    }
    private static void LogResponse(string response)
    {
        File.AppendAllText("server_log.txt", $"Ответ: {response} {DateTime.Now}\n");
    }
}


