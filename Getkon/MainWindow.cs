using System;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Gtk;
using WebKit;

namespace Getkon;

class MainWindow : Window
{
    class Args
    {
        public object Id { get; set; }
        public string Method { get; set; }
        public ExpandoObject Paramters { get; set; }
        public string Namespace { get; set; }
    }

    private readonly WebView _webView;
    private readonly StaticFileServer _server;

    public MainWindow()
        : base(WindowType.Toplevel)
    {
        var serverPath = System.IO.Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "frontend/svelte/dist"
        );
        var port = "8080";
        var startFileServer = true;

        _webView = new WebView { Hexpand = true, Visible = true };
#if DEBUG
        _webView.Settings.EnableDeveloperExtras = true;
        serverPath = "/home/anemonas/Projects/test/frontend/svelte/dist";
        if (Util.FrontendIsRunning())
        {
            port = "5173";
            startFileServer = false;
        }
#endif
        if (startFileServer)
        {
            _server = new StaticFileServer(serverPath);
            _ = _server.StartAsync();
        }

        _webView.LoadUri($"http://localhost:{port}/");

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
                .Paramters.Select(x => Util.ConvertJsonElement((JsonElement)x.Value))
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

    private void Window_DeleteEvent(object sender, DeleteEventArgs a)
    {
        _ = _server.StopAsync();
        Application.Quit();
    }
}
