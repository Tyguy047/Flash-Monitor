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
        
        string printerIp = Preferences.Get("PrinterIp", string.Empty);
        string result = await GetDataAsync($"http://{printerIp}/endpoint/location/here"); // Incomplete, this is not a real endpoint, and the real one needs auth!
        Console.WriteLine(result);
        
    }

    // Get the printer IP with:
    // string printerIp = Preferences.Get("PrinterIp", string.Empty);
    
    private void OnResetClicked(object? sender, EventArgs e)
    {
        Preferences.Remove("PrinterIp");
        Application.Current!.Windows[0].Page = new NavigationPage(new SetupPage());
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