using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using CefGlue.Avalonia;
using ImageInfrastructure.Abstractions.Interfaces;
using ImageInfrastructure.Pixiv;
using Meowtrix.PixivApi;
using Microsoft.Extensions.DependencyInjection;

namespace UI
{
    public class MainWindow : Window
    {
        private Uri _result;
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.FindControl<Button>("BtnLogin").Click += BtnLogin_Click;
            this.FindControl<AvaloniaCefBrowser>("WebView").LoadStart += WebView_OnLoadStart;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            await Task.Factory.StartNew(async () =>
            {
                var client = new PixivClient();
                var token = await client.LoginAsync(uri =>
                {
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        var webview = this.FindControl<AvaloniaCefBrowser>("WebView");
                        webview.Browser.GetMainFrame().LoadUrl(uri);
                    });

                    while (true)
                    {
                        if (_result != null) return Task.FromResult(_result);

                        Thread.Sleep(200);
                    }
                });

                var settingsProvider = Program.ServiceProvider?.GetService<ISettingsProvider<PixivSettings>>();
                settingsProvider?.Update(a => { a.Token = token; });
            });
        }

        private void WebView_OnLoadStart(object sender, LoadStartEventArgs e)
        {
            var uri = new Uri(e.Frame.Url);
            if (uri.Scheme == "pixiv")
            {
                _result = uri;
            }
        }
    }
}