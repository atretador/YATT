using Avalonia;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;

namespace YATT;

class Program
{
    private static readonly string PipeName = "YATT_SingleInstance";

    [STAThread]
    public static void Main(string[] args)
    {
        bool isFirstInstance;
        using (Mutex mutex = new Mutex(true, "YATT_Mutex", out isFirstInstance))
        {
            if (!isFirstInstance)
            {
                // Send file path to the already running instance
                if (args.Length > 0)
                {
                    using (NamedPipeClientStream client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out))
                    {
                        client.Connect(500); // Wait up to 500ms for connection
                        using (StreamWriter writer = new StreamWriter(client))
                        {
                            writer.WriteLine(args[0]);
                        }
                    }
                }
                return;
            }

            // Start the Named Pipe server for inter-process communication
            StartPipeServer();

            // Start the app
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    private static void StartPipeServer()
    {
        new Thread(() =>
        {
            while (true)
            {
                using (NamedPipeServerStream server = new NamedPipeServerStream(PipeName, PipeDirection.In))
                {
                    server.WaitForConnection();
                    using (StreamReader reader = new StreamReader(server))
                    {
                        string? filePath = reader.ReadLine();
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            App.OnFileOpened(filePath); // Notify the running instance
                        }
                    }
                }
            }
        })
        { IsBackground = true }.Start();
    }
}