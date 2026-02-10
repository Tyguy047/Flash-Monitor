namespace Flash_Monitor;

public partial class MainPage : ContentPage
{

    public MainPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        PrinterName.Text = Preferences.Get("PrinterName", "Printer");
        
        // Widget functions here...
        
    }

    // Get the printer IP with:
    // string printerIp = Preferences.Get("PrinterIp", string.Empty);
    
    private void OnResetClicked(object? sender, EventArgs e)
    {
        Preferences.Remove("PrinterIp");
        Application.Current!.Windows[0].Page = new NavigationPage(new SetupPage());
    }
}