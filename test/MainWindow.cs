using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Gtk;
using WebKit;

namespace test;

class MainWindow : Window
{
    class Args
    {
        public int Id { get; set; }
        public string Method { get; set; }
        public ExpandoObject Paramters { get; set; }
        public string Namespace { get; set; }
    }

    private readonly WebView _webView;
    private readonly StaticFileServer _server;

    public MainWindow()
        : base(WindowType.Toplevel)
    {
        string serverPath = System.IO.Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "frontend/svelte/dist"
        );
#if DEBUG
        serverPath = "/home/anemonas/Projects/test/frontend/svelte/dist";
#endif
        _server = new StaticFileServer(serverPath);

        _ = _server.StartAsync();

        _webView = new WebView { Hexpand = true, Visible = true };
        _webView.Settings.EnableDeveloperExtras = true;

        _webView.LoadUri("http://localhost:8080/");

        var contentManager = _webView.UserContentManager;

        var messageHandlerName = "native";

        var script = new UserScript(
            source: "function sendMessageAsync(message){\n"
                + $"return window.webkit.messageHandlers.{messageHandlerName}.postMessage(message);\n"
                + "}",
            UserContentInjectedFrames.AllFrames,
            UserScriptInjectionTime.Start,
            null,
            null
        );

        contentManager.AddScript(script);
        contentManager.RegisterScriptMessageHandler(messageHandlerName);
        contentManager.ScriptMessageReceived += (o, e) =>
        {
            if (e.JsResult == null && !e.JsResult.JsValue.IsString)
                return;

            var args = JsonSerializer.Deserialize<Args>(e.JsResult.JsValue.ToString());

            var assembly = Assembly.GetExecutingAssembly();
            var type = assembly.GetType(args.Namespace);
            var instance = Activator.CreateInstance(type);
            MethodInfo method = type.GetMethod(args.Method);

            var parameters = args
                .Paramters.Select(x => ConvertJsonElement((JsonElement)x.Value))
                .ToArray();

            var result = method.Invoke(instance, [.. parameters]);

            if (result is string)
                result = $"\"{result}\"";

            string js = $"__resolveNativeCall({args.Id},{result});";
            _webView.RunJavascript(js, null, null);
        };

        Child = _webView;
        Child.Visible = true;
        DeleteEvent += Window_DeleteEvent;
    }

    private static object ConvertJsonElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                return element.GetString();
            case JsonValueKind.Number:
                if (element.TryGetInt64(out long l))
                    return l;
                else if (element.TryGetDouble(out double d))
                    return d;
                else
                    throw new InvalidOperationException("Número em formato desconhecido.");
            case JsonValueKind.True:
            case JsonValueKind.False:
                return element.GetBoolean();
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return null;
            default:
                throw new InvalidOperationException($"Tipo não primitivo: {element.ValueKind}");
        }
    }

    private void Window_DeleteEvent(object sender, DeleteEventArgs a)
    {
        _ = _server.StopAsync();
        Application.Quit();
    }
}
