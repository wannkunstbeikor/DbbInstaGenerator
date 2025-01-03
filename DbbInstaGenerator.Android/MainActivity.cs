using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;

namespace DbbInstaGenerator.Android;

[Activity(
    Label = "DbbInstaGenerator",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@mipmap/ic_launcher",
    RoundIcon = "@mipmap/ic_launcher_round",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    internal static MainActivity Instance { get; private set; }

    public MainActivity()
    {
        Instance = this;
    }
    
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont().AfterSetup(a =>
            {
                App app = (a.Instance as App)!;
                app.ShareServiceType = typeof(AndroidShareService);
            });
    }
}
