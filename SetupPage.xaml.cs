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
        var sn = SnEntry.Text?.Trim();
        var checkCode = CheckCodeEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(ip) || string.IsNullOrWhiteSpace(name)
            || string.IsNullOrWhiteSpace(sn) || string.IsNullOrWhiteSpace(checkCode))
        {
            await DisplayAlertAsync("Error", "Please fill in all required fields.", "OK");
            return;
        }

        // Save the printer config using MAUI Preferences
        Preferences.Set("PrinterIp", ip);
        Preferences.Set("PrinterName", name);
        Preferences.Set("PrinterSn", sn);
        Preferences.Set("PrinterCheckCode", checkCode);

        bool printerHasLight = HasLightCheckBox.IsChecked;
        bool printerHasCam = HasCameraCheckBox.IsChecked;

        Preferences.Set("PrinterHasLight", printerHasLight);
        Preferences.Set("PrinterHasCam", printerHasCam);

        // Navigate to MainPage, replacing the navigation stack
        Application.Current!.Windows[0].Page = new NavigationPage(new MainPage());
    }
}