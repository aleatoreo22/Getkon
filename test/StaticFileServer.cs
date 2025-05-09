using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace test;

public class StaticFileServer : IDisposable
{
    private readonly HttpListener _listener;
    private readonly string _basePath;
    private CancellationTokenSource _cts;
    private Task _serverTask;
    private bool _disposed;

    /// <summary>
    /// Initialize a new static file server
    /// </summary>
    /// <param name="basePath">Pasta base para servir os arquivos</param>
    /// <param name="port">Porta do servidor (padrão: 8080)</param>
    public StaticFileServer(string basePath, int port = 8080)
    {
        if (!Directory.Exists(basePath))
            throw new DirectoryNotFoundException($"O diretório '{basePath}' não foi encontrado.");

        _basePath = Path.GetFullPath(basePath);
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://localhost:{port}/");
        _listener.Prefixes.Add($"http://127.0.0.1:{port}/");
    }

    /// <summary>
    /// Start the server asynchronously
    /// </summary>
    /// <returns>Task que representa a operação</returns>
    public async Task StartAsync()
    {
        if (_serverTask != null && !_serverTask.IsCompleted)
            throw new InvalidOperationException("O servidor já está em execução.");

        _cts = new CancellationTokenSource();
        _serverTask = RunServerAsync(_cts.Token);

        while (!_listener.IsListening)
        {
            await Task.Delay(10);
        }
    }

    /// <summary>
    /// Stop the server asynchronously
    /// </summary>
    /// <returns>Task que representa a operação</returns>
    public async Task StopAsync()
    {
        if (_serverTask == null || _serverTask.IsCompleted)
            return;

        _cts?.Cancel();
        await _serverTask;
    }

    private async Task RunServerAsync(CancellationToken ct)
    {
        try
        {
            _listener.Start();

            while (!ct.IsCancellationRequested)
            {
                var context = await _listener.GetContextAsync().WaitAsync(ct);
                _ = ProcessRequestAsync(context, ct);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Console.WriteLine($"Erro no servidor: {ex.Message}");
        }
        finally
        {
            _listener.Stop();
        }
    }

    private async Task ProcessRequestAsync(HttpListenerContext context, CancellationToken ct)
    {
        var request = context.Request;
        var response = context.Response;
        try
        {
            var relativePath = Uri.UnescapeDataString(request.Url!.AbsolutePath.TrimStart('/'));
            if (relativePath == "")
                relativePath = "index.html";
            var fullPath = Path.GetFullPath(Path.Combine(_basePath, relativePath));

            if (!fullPath.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase))
            {
                response.StatusCode = 403;
                return;
            }

            if (File.Exists(fullPath))
                await ServeFileAsync(fullPath, response, ct);
            else
                response.StatusCode = 404;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            context.Response.StatusCode = 500;
            Console.WriteLine($"Erro ao processar requisição: {ex.Message}");
        }
        finally
        {
            response.Close();
        }
    }

    private static async Task ServeFileAsync(
        string filePath,
        HttpListenerResponse response,
        CancellationToken ct
    )
    {
        var fileInfo = new FileInfo(filePath);
        response.ContentType = GetMimeType(filePath);
        response.ContentLength64 = fileInfo.Length;

        using var fileStream = File.OpenRead(filePath);
        await fileStream.CopyToAsync(response.OutputStream, 81920, ct);
    }

    private static string GetMimeType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".html" or ".htm" => "text/html",
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".json" => "application/json",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".svg" => "image/svg+xml",
            ".ico" => "image/x-icon",
            ".txt" => "text/plain",
            ".pdf" => "application/pdf",
            ".zip" => "application/zip",
            _ => "application/octet-stream",
        };
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _cts?.Cancel();
        _serverTask?.Wait(TimeSpan.FromSeconds(5));
        _listener.Close();

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
