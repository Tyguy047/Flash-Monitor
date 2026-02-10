namespace Flash_Monitor;
using System.Net.Http;

public partial class MainPage : ContentPage
{

    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        PrinterName.Text = Preferences.Get("PrinterName", "Printer");
        
        LightControls.IsVisible = Preferences.Get("PrinterHasLight", false);

        string printerIp = Preferences.Get("PrinterIp", string.Empty);
        string result = await GetDataAsync($"http://{printerIp}/endpoint/location/here"); // Incomplete, this is not a real endpoint, and the real one needs auth!
        Console.WriteLine(result);
        
        // Image of what's printing
        
        // Print Progress
        
        // Pause/Resume Button (<--- Left), Light on/off Button (if supported by printer!) (<--- Right)
        
        // Camera feed (if supported by printer!)
        
    }

    // Get the printer IP with:
    // string printerIp = Preferences.Get("PrinterIp", string.Empty);
    
    private void OnResetClicked(object? sender, EventArgs e)
    {
        Preferences.Remove("PrinterIp");
        Application.Current!.Windows[0].Page = new NavigationPage(new SetupPage());
    }

    private async void OnPauseClicked(object? sender, EventArgs e)
    { 
        string printerIp = Preferences.Get("PrinterIp", string.Empty);
        string result = await GetDataAsync($"http://{printerIp}/pause/endpoint/here");
        Console.WriteLine(result); // For debug!
    }

    private async void OnResumeClicked(object? sender, EventArgs e)
    {
        string printerIp = Preferences.Get("PrinterIp", string.Empty);
        string result = await GetDataAsync($"http://{printerIp}/resume/endpoint/here");
        Console.WriteLine(result); // For debug!
    }

    private async void OnStopClicked(object? sender, EventArgs e)
    {
        string printerIp = Preferences.Get("PrinterIp", string.Empty);
        string result = await GetDataAsync($"http://{printerIp}/resume/endpoint/here");
        Console.WriteLine(result); // For debug!
    }

    private async void OnLightToggleClicked(object? sender, ToggledEventArgs e)
    {

        string printerIp = Preferences.Get("PrinterIp", string.Empty);

        if (e.Value)
        {
            // Switch is ON
            Console.WriteLine("Light ON");
            string result = await GetDataAsync($"http://{printerIp}/light/toggle/endpoint/here");
            Console.WriteLine(result);
        }
        else
        {
            // Switch is OFF
            Console.WriteLine("Light OFF");
            string result = await GetDataAsync($"http://{printerIp}/light/toggle/endpoint/here");
            Console.WriteLine(result);
        }
    }

    private readonly HttpClient _httpClient = new();
    private async Task<string> GetDataAsync(string url)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Request failed: {ex.Message}");
            return string.Empty;
        }
    }
}