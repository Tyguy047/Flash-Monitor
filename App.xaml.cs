using Microsoft.Extensions.DependencyInjection;

namespace Flash_Monitor;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        
        Page startPage;

        if (Preferences.ContainsKey("PrinterIp"))
        {
            // Already configured — go straight to monitoring
            startPage = new MainPage();
        }
        else
        {
            // First launch — show setup
            startPage = new SetupPage();
        }
        
        return new Window(new NavigationPage(startPage));
    }
}