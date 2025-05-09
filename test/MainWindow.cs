using System;
using System.Text.Json;
using Gtk;
using WebKit;

namespace test;

class MainWindow : Window
{
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
        contentManager.ScriptMessageReceived += (o, args) =>
        {
            // var t = typeof(ExposedMethods);
            // foreach (var method in t.GetMethods()) {
            //     // if()
            // }

            // var json = args.Message.ToString(); // algo como "{\"id\":1,\"method\":\"calcular\",\"args\":[7,6]}"

            // var parsed = JsonDocument.Parse(json);
            // var root = parsed.RootElement;

            // int id = root.GetProperty("id").GetInt32();
            // string method = root.GetProperty("method").GetString();

            // int a = root.GetProperty("args")[0].GetInt32();
            // int b = root.GetProperty("args")[1].GetInt32();
            // int resultado = a * b;

            string js = $"__resolveNativeCall(1,\"I'm JS\");";
            _webView.RunJavascript(js, null, null);
            return;

            var value = args.JsResult?.JsValue;
            if (value is { IsString: true } v)
                Console.WriteLine(
                    $"{nameof(contentManager.ScriptMessageReceived)}:\t{nameof(JavascriptResult.JsValue)}\t{v?.ToString()}"
                );
            args.RetVal = "Teste C#";
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
