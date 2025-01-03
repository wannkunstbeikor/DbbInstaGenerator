using System.Net.Http;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using DbbInstaGenerator;
using DbbInstaGenerator.Browser;

internal sealed partial class Program
{
    private static Task Main(string[] args) => BuildAvaloniaApp()
            .WithInterFont().AfterSetup(a =>
            {
                App app = (a.Instance as App)!;
                app.ShareServiceType = typeof(BrowserShareService);
            })
            .StartBrowserAppAsync("out");

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}