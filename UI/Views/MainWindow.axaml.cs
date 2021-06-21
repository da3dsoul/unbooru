using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ImageInfrastructure.Abstractions.Interfaces;
using ImageInfrastructure.Pixiv;
using Meowtrix.PixivApi;
using Microsoft.Extensions.DependencyInjection;
using UI.ViewModels;
using WebViewControl;

namespace UI.Views
{
    public partial class MainWindow : Window
    {
        private Uri? _result;
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
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
                        var context = DataContext as MainWindowViewModel;
                        Debug.Assert(context != null, nameof(context) + " != null");
                        context.CurrentAddress = uri;
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

        private void WebView_OnBeforeNavigate(Request request)
        {
            var uri = new Uri(request.Url);
            if (uri.Scheme == "pixiv")
            {
                _result = uri;
                request.Cancel();
            }
        }
    }
}