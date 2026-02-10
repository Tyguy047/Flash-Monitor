using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash_Monitor;

public partial class SetupPage : ContentPage
{
    public SetupPage()
    {
        InitializeComponent();
    }
    
    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var ip = IpEntry.Text?.Trim();
        var name = NameEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(ip) || string.IsNullOrWhiteSpace(name))
        {
            await DisplayAlert("Error", "Please enter a valid IP address and name.", "OK");
            return;
        }

        // Save the IP using MAUI Preferences
        Preferences.Set("PrinterIp", ip);
        Preferences.Set("PrinterName", name);

        // Navigate to MainPage, replacing the navigation stack
        Application.Current!.Windows[0].Page = new NavigationPage(new MainPage());
    }
}