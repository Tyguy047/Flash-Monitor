namespace Flash_Monitor;

using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

public partial class MainPage : ContentPage
{
    private const int PrinterPort = 8898;
    private readonly HttpClient _httpClient = new();

    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        PrinterName.Text = Preferences.Get("PrinterName", "Printer");
        LightControls.IsVisible = Preferences.Get("PrinterHasLight", false);

        await RefreshPrinterDetail();
    }

    // --- API Helpers ---

    private string GetBaseUrl()
    {
        string ip = Preferences.Get("PrinterIp", string.Empty);
        return $"http://{ip}:{PrinterPort}";
    }

    private object BuildAuthPayload()
    {
        return new
        {
            serialNumber = Preferences.Get("PrinterSn", string.Empty),
            checkCode = Preferences.Get("PrinterCheckCode", string.Empty)
        };
    }

    private async Task<string> PostAsync(string endpoint, object payload)
    {
        try
        {
            string json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync($"{GetBaseUrl()}{endpoint}", content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Request to {endpoint} failed: {ex.Message}");
            return string.Empty;
        }
    }

    // --- /detail ---

    private async Task RefreshPrinterDetail()
    {
        string result = await PostAsync("/detail", BuildAuthPayload());
        if (string.IsNullOrEmpty(result)) return;

        Console.WriteLine(result); // For debug!

        var response = JsonSerializer.Deserialize<DetailResponse>(result);
        if (response?.Code != 0 || response.Detail == null) return;

        // TODO: Update UI with response.Detail fields (temps, progress, status, etc.)
    }

    // --- /product ---

    private async Task<ProductInfo?> GetProductInfo()
    {
        string result = await PostAsync("/product", BuildAuthPayload());
        if (string.IsNullOrEmpty(result)) return null;

        Console.WriteLine(result); // For debug!

        var response = JsonSerializer.Deserialize<ProductResponse>(result);
        return response?.Code == 0 ? response.Product : null;
    }

    // --- /control helpers ---

    private async Task<bool> SendControlCommand(string cmd, object args)
    {
        var payload = new
        {
            serialNumber = Preferences.Get("PrinterSn", string.Empty),
            checkCode = Preferences.Get("PrinterCheckCode", string.Empty),
            payload = new { cmd, args }
        };

        string result = await PostAsync("/control", payload);
        if (string.IsNullOrEmpty(result)) return false;

        Console.WriteLine(result); // For debug!

        var response = JsonSerializer.Deserialize<ApiResponse>(result);
        return response?.Code == 0;
    }

    // --- /control: lightControl_cmd ---

    private async Task SetLight(bool on)
    {
        await SendControlCommand("lightControl_cmd", new { status = on ? "open" : "close" });
    }

    // --- /control: jobCtl_cmd ---

    private async Task SendJobControl(string action)
    {
        await SendControlCommand("jobCtl_cmd", new { jobID = "", action });
    }

    // --- /control: printerCtl_cmd ---

    private async Task SendPrinterControl(int? speed = null, int? chamberFan = null,
        int? coolingFan = null, int? coolingLeftFan = null, double? zAxisCompensation = null)
    {
        var args = new Dictionary<string, object>();
        if (speed != null) args["speed"] = speed.Value;
        if (chamberFan != null) args["chamberFan"] = chamberFan.Value;
        if (coolingFan != null) args["coolingFan"] = coolingFan.Value;
        if (coolingLeftFan != null) args["coolingLeftFan"] = coolingLeftFan.Value;
        if (zAxisCompensation != null) args["zAxisCompensation"] = zAxisCompensation.Value;

        await SendControlCommand("printerCtl_cmd", args);
    }

    // --- /control: circulateCtl_cmd ---

    private async Task SetCirculation(bool internalOn, bool externalOn)
    {
        await SendControlCommand("circulateCtl_cmd", new
        {
            @internal = internalOn ? "open" : "close",
            external = externalOn ? "open" : "close"
        });
    }

    // --- /control: streamCtrl_cmd ---

    private async Task SetCameraStream(bool on)
    {
        await SendControlCommand("streamCtrl_cmd", new { action = on ? "open" : "close" });
    }

    // --- /control: stateCtrl_cmd ---

    private async Task ClearPlatformState()
    {
        await SendControlCommand("stateCtrl_cmd", new { action = "setClearPlatform" });
    }

    // --- /gcodeList ---

    private async Task<List<string>?> GetGcodeList()
    {
        string result = await PostAsync("/gcodeList", BuildAuthPayload());
        if (string.IsNullOrEmpty(result)) return null;

        Console.WriteLine(result); // For debug!

        var response = JsonSerializer.Deserialize<GcodeListResponse>(result);
        return response?.Code == 0 ? response.GcodeList : null;
    }

    // --- /gcodeThumb ---

    private async Task<string?> GetGcodeThumb(string fileName)
    {
        var payload = new
        {
            serialNumber = Preferences.Get("PrinterSn", string.Empty),
            checkCode = Preferences.Get("PrinterCheckCode", string.Empty),
            fileName
        };

        string result = await PostAsync("/gcodeThumb", payload);
        if (string.IsNullOrEmpty(result)) return null;

        var response = JsonSerializer.Deserialize<GcodeThumbResponse>(result);
        return response?.Code == 0 ? response.ImageData : null;
    }

    // --- /printGcode ---

    private async Task<bool> PrintGcode(string fileName, bool levelingBeforePrint = true)
    {
        var payload = new
        {
            serialNumber = Preferences.Get("PrinterSn", string.Empty),
            checkCode = Preferences.Get("PrinterCheckCode", string.Empty),
            fileName,
            levelingBeforePrint,
            flowCalibration = false,
            useMatlStation = false,
            gcodeToolCnt = 0,
            materialMappings = Array.Empty<object>()
        };

        string result = await PostAsync("/printGcode", payload);
        if (string.IsNullOrEmpty(result)) return false;

        Console.WriteLine(result); // For debug!

        var response = JsonSerializer.Deserialize<ApiResponse>(result);
        return response?.Code == 0;
    }

    // --- /uploadGcode ---

    private async Task<bool> UploadGcode(string filePath, bool printNow = false,
        bool levelingBeforePrint = true)
    {
        try
        {
            byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
            string fileName = Path.GetFileName(filePath);

            var request = new HttpRequestMessage(HttpMethod.Post, $"{GetBaseUrl()}/uploadGcode");

            request.Headers.Add("serialNumber", Preferences.Get("PrinterSn", string.Empty));
            request.Headers.Add("checkCode", Preferences.Get("PrinterCheckCode", string.Empty));
            request.Headers.Add("fileSize", fileBytes.Length.ToString());
            request.Headers.Add("printNow", printNow.ToString().ToLower());
            request.Headers.Add("levelingBeforePrint", levelingBeforePrint.ToString().ToLower());
            request.Headers.Add("flowCalibration", "false");
            request.Headers.Add("useMatlStation", "false");
            request.Headers.Add("gcodeToolCnt", "0");
            request.Headers.Add("materialMappings", "[]");
            request.Headers.Add("Expect", "100-continue");

            var formContent = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            formContent.Add(fileContent, "gcodeFile", fileName);

            request.Content = formContent;

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string result = await response.Content.ReadAsStringAsync();

            Console.WriteLine(result); // For debug!

            var apiResponse = JsonSerializer.Deserialize<ApiResponse>(result);
            return apiResponse?.Code == 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Upload failed: {ex.Message}");
            return false;
        }
    }

    // --- UI Event Handlers ---

    private void OnResetClicked(object? sender, EventArgs e)
    {
        Preferences.Remove("PrinterIp");
        Application.Current!.Windows[0].Page = new NavigationPage(new SetupPage());
    }

    private async void OnPauseClicked(object? sender, EventArgs e)
    {
        await SendJobControl("pause");
    }

    private async void OnResumeClicked(object? sender, EventArgs e)
    {
        await SendJobControl("continue");
    }

    private async void OnStopClicked(object? sender, EventArgs e)
    {
        await SendJobControl("cancel");
    }

    private async void OnLightToggleClicked(object? sender, ToggledEventArgs e)
    {
        await SetLight(e.Value);
    }

    // --- Response Models ---

    private class ApiResponse
    {
        [JsonPropertyName("code")] public int Code { get; set; }
        [JsonPropertyName("message")] public string Message { get; set; } = "";
    }

    private class DetailResponse : ApiResponse
    {
        [JsonPropertyName("detail")] public PrinterDetail? Detail { get; set; }
    }

    private class ProductResponse : ApiResponse
    {
        [JsonPropertyName("product")] public ProductInfo? Product { get; set; }
    }

    private class GcodeListResponse : ApiResponse
    {
        [JsonPropertyName("gcodeList")] public List<string>? GcodeList { get; set; }
    }

    private class GcodeThumbResponse : ApiResponse
    {
        [JsonPropertyName("imageData")] public string? ImageData { get; set; }
    }

    public class PrinterDetail
    {
        [JsonPropertyName("autoShutdown")] public string AutoShutdown { get; set; } = "";
        [JsonPropertyName("autoShutdownTime")] public int AutoShutdownTime { get; set; }
        [JsonPropertyName("cameraStreamUrl")] public string CameraStreamUrl { get; set; } = "";
        [JsonPropertyName("chamberFanSpeed")] public int ChamberFanSpeed { get; set; }
        [JsonPropertyName("chamberTargetTemp")] public int ChamberTargetTemp { get; set; }
        [JsonPropertyName("chamberTemp")] public int ChamberTemp { get; set; }
        [JsonPropertyName("coolingFanSpeed")] public int CoolingFanSpeed { get; set; }
        [JsonPropertyName("cumulativeFilament")] public double CumulativeFilament { get; set; }
        [JsonPropertyName("cumulativePrintTime")] public int CumulativePrintTime { get; set; }
        [JsonPropertyName("currentPrintSpeed")] public int CurrentPrintSpeed { get; set; }
        [JsonPropertyName("doorStatus")] public string DoorStatus { get; set; } = "";
        [JsonPropertyName("errorCode")] public string ErrorCode { get; set; } = "";
        [JsonPropertyName("estimatedLeftLen")] public double EstimatedLeftLen { get; set; }
        [JsonPropertyName("estimatedLeftWeight")] public double EstimatedLeftWeight { get; set; }
        [JsonPropertyName("estimatedRightLen")] public double EstimatedRightLen { get; set; }
        [JsonPropertyName("estimatedRightWeight")] public double EstimatedRightWeight { get; set; }
        [JsonPropertyName("estimatedTime")] public int EstimatedTime { get; set; }
        [JsonPropertyName("externalFanStatus")] public string ExternalFanStatus { get; set; } = "";
        [JsonPropertyName("fillAmount")] public int FillAmount { get; set; }
        [JsonPropertyName("firmwareVersion")] public string FirmwareVersion { get; set; } = "";
        [JsonPropertyName("flashRegisterCode")] public string FlashRegisterCode { get; set; } = "";
        [JsonPropertyName("internalFanStatus")] public string InternalFanStatus { get; set; } = "";
        [JsonPropertyName("ipAddr")] public string IpAddr { get; set; } = "";
        [JsonPropertyName("leftFilamentType")] public string LeftFilamentType { get; set; } = "";
        [JsonPropertyName("leftTargetTemp")] public int LeftTargetTemp { get; set; }
        [JsonPropertyName("leftTemp")] public int LeftTemp { get; set; }
        [JsonPropertyName("lightStatus")] public string LightStatus { get; set; } = "";
        [JsonPropertyName("location")] public string Location { get; set; } = "";
        [JsonPropertyName("macAddr")] public string MacAddr { get; set; } = "";
        [JsonPropertyName("name")] public string Name { get; set; } = "";
        [JsonPropertyName("nozzleCnt")] public int NozzleCnt { get; set; }
        [JsonPropertyName("nozzleModel")] public string NozzleModel { get; set; } = "";
        [JsonPropertyName("nozzleStyle")] public int NozzleStyle { get; set; }
        [JsonPropertyName("pid")] public int Pid { get; set; }
        [JsonPropertyName("platTargetTemp")] public int PlatTargetTemp { get; set; }
        [JsonPropertyName("platTemp")] public int PlatTemp { get; set; }
        [JsonPropertyName("polarRegisterCode")] public string PolarRegisterCode { get; set; } = "";
        [JsonPropertyName("printDuration")] public int PrintDuration { get; set; }
        [JsonPropertyName("printFileName")] public string PrintFileName { get; set; } = "";
        [JsonPropertyName("printFileThumbUrl")] public string PrintFileThumbUrl { get; set; } = "";
        [JsonPropertyName("printLayer")] public int PrintLayer { get; set; }
        [JsonPropertyName("printProgress")] public double PrintProgress { get; set; }
        [JsonPropertyName("printSpeedAdjust")] public int PrintSpeedAdjust { get; set; }
        [JsonPropertyName("remainingDiskSpace")] public int RemainingDiskSpace { get; set; }
        [JsonPropertyName("rightFilamentType")] public string RightFilamentType { get; set; } = "";
        [JsonPropertyName("rightTargetTemp")] public int RightTargetTemp { get; set; }
        [JsonPropertyName("rightTemp")] public int RightTemp { get; set; }
        [JsonPropertyName("status")] public string Status { get; set; } = "";
        [JsonPropertyName("targetPrintLayer")] public int TargetPrintLayer { get; set; }
        [JsonPropertyName("tvoc")] public int Tvoc { get; set; }
        [JsonPropertyName("zAxisCompensation")] public double ZAxisCompensation { get; set; }
    }

    public class ProductInfo
    {
        [JsonPropertyName("chamberTempCtrlState")] public int ChamberTempCtrlState { get; set; }
        [JsonPropertyName("externalFanCtrlState")] public int ExternalFanCtrlState { get; set; }
        [JsonPropertyName("internalFanCtrlState")] public int InternalFanCtrlState { get; set; }
        [JsonPropertyName("lightCtrlState")] public int LightCtrlState { get; set; }
        [JsonPropertyName("nozzleTempCtrlState")] public int NozzleTempCtrlState { get; set; }
        [JsonPropertyName("platformTempCtrlState")] public int PlatformTempCtrlState { get; set; }
    }
}