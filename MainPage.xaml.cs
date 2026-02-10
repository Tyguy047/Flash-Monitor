namespace Flash_Monitor;

public partial class MainPage : ContentPage
{
    int count = 0;

    public MainPage()
    {
        InitializeComponent();
    }

    // Get the printer IP with:
    // string printerIp = Preferences.Get("PrinterIp", string.Empty);
    
    private void OnCounterClicked(object? sender, EventArgs e)
    {
        count++;

        if (count == 1)
            CounterBtn.Text = $"Clicked {count} time";
        else
            CounterBtn.Text = $"Clicked {count} times";

        SemanticScreenReader.Announce(CounterBtn.Text);
    }

    private void OnResetClicked(object? sender, EventArgs e)
    {
        Preferences.Remove("PrinterIp");
        Application.Current!.Windows[0].Page = new NavigationPage(new SetupPage());
    }
}