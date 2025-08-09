using HidSharp;

namespace Driver;

public sealed record KeyboardFilter
{
    required public int VendorId, ProductId, Usage, UsagePage;
}

public sealed class KeyboardManager : IDisposable
{
    public static readonly KeyboardFilter[] CompatibleKeyboards = [
         new KeyboardFilter { VendorId = 16868, ProductId = 8449,   UsagePage =  1,     Usage = 0 },
         new KeyboardFilter { VendorId = 16868, ProductId = 8450,   UsagePage =  65280, Usage = 1 },
         new KeyboardFilter { VendorId = 6645,  ProductId = 64561,  UsagePage =  1,     Usage = 0 },
         new KeyboardFilter { VendorId = 6645,  ProductId = 65307,  UsagePage =  65280, Usage = 1 },
         new KeyboardFilter { VendorId = 16868, ProductId = 8451,   UsagePage =  1,     Usage = 0 },
         new KeyboardFilter { VendorId = 16868, ProductId = 8452,   UsagePage =  65280, Usage = 1 },
         new KeyboardFilter { VendorId = 6645,  ProductId = 64560,  UsagePage =  1,     Usage = 0 },
         new KeyboardFilter { VendorId = 6645,  ProductId = 65295,  UsagePage =  65280, Usage = 1 },
         new KeyboardFilter { VendorId = 14391, ProductId = 12290,  UsagePage =  1,     Usage = 0 },
         new KeyboardFilter { VendorId = 14391, ProductId = 12322,  UsagePage =  65280, Usage = 1 },
         new KeyboardFilter { VendorId = 16868, ProductId = 8468,   UsagePage =  1,     Usage = 0 },
         new KeyboardFilter { VendorId = 16868, ProductId = 8469,   UsagePage =  65280, Usage = 1 },
         new KeyboardFilter { VendorId = 16868, ProductId = 8470,   UsagePage =  1,     Usage = 0 },
         new KeyboardFilter { VendorId = 16868, ProductId = 8471,   UsagePage =  65280, Usage = 1 },
         new KeyboardFilter { VendorId = 14391, ProductId = 12291,  UsagePage =  1,     Usage = 0 },
         new KeyboardFilter { VendorId = 14391, ProductId = 12330,  UsagePage =  65280, Usage = 1 },
         new KeyboardFilter { VendorId = 16868, ProductId = 8486,   UsagePage =  1,     Usage = 0 },
         new KeyboardFilter { VendorId = 16868, ProductId = 8487,   UsagePage =  65280, Usage = 1 },
         new KeyboardFilter { VendorId = 16868, ProductId = 8466,   UsagePage =  1,     Usage = 0 },
         new KeyboardFilter { VendorId = 16868, ProductId = 8467,   UsagePage =  65280, Usage = 1 },
         new KeyboardFilter { VendorId = 16868, ProductId = 8464,   UsagePage =  1,     Usage = 0 },
         new KeyboardFilter { VendorId = 16868, ProductId = 8465,   UsagePage =  65280, Usage = 1 },
         new KeyboardFilter { VendorId = 16868, ProductId = 8472,   UsagePage =  1,     Usage = 0 },
         new KeyboardFilter { VendorId = 16868, ProductId = 8473,   UsagePage =  65280, Usage = 1 },
         new KeyboardFilter { VendorId = 16868, ProductId = 8474,   UsagePage =  1,     Usage = 0 },
         new KeyboardFilter { VendorId = 16868, ProductId = 8475,   UsagePage =  65280, Usage = 1 },
         new KeyboardFilter { VendorId = 14391, ProductId = 12292,  UsagePage =  1,     Usage = 0 },
         new KeyboardFilter { VendorId = 14391, ProductId = 12323,  UsagePage =  65280, Usage = 1 },
         new KeyboardFilter { VendorId = 16868, ProductId = 8476,   UsagePage =  1,     Usage = 0 },
         new KeyboardFilter { VendorId = 16868, ProductId = 8477,   UsagePage =  65280, Usage = 1 },
         new KeyboardFilter { VendorId = 16868, ProductId = 8480,   UsagePage =  1,     Usage = 0 },
         new KeyboardFilter { VendorId = 16868, ProductId = 8481,   UsagePage =  65280, Usage = 1 },
         new KeyboardFilter { VendorId = 16868, ProductId = 8478,   UsagePage =  1,     Usage = 0 },
         new KeyboardFilter { VendorId = 16868, ProductId = 8479,   UsagePage =  65280, Usage = 1 },
         new KeyboardFilter { VendorId = 16868, ProductId = 8482,   UsagePage =  1,     Usage = 0 },
         new KeyboardFilter { VendorId = 16868, ProductId = 8483,   UsagePage =  65280, Usage = 1 },
         new KeyboardFilter { VendorId = 14391, ProductId = 12298,  UsagePage =  1,     Usage = 0 },
         new KeyboardFilter { VendorId = 14391, ProductId = 12327,  UsagePage =  65280, Usage = 1 },
         new KeyboardFilter { VendorId = 16868, ProductId = 8498,   UsagePage =  1,     Usage = 0 },
         new KeyboardFilter { VendorId = 16868, ProductId = 8499,   UsagePage =  65280, Usage = 1 },
         new KeyboardFilter { VendorId = 14391, ProductId = 12301,  UsagePage =  1,     Usage = 0 },
         new KeyboardFilter { VendorId = 14391, ProductId = 12332,  UsagePage =  65280, Usage = 1 },
         new KeyboardFilter { VendorId = 14391, ProductId = 12296,  UsagePage =  1,     Usage = 0 },
         new KeyboardFilter { VendorId = 14391, ProductId = 12331,  UsagePage =  65280, Usage = 1 },
         new KeyboardFilter { VendorId = 6645,  ProductId = 40992,  UsagePage =  1,     Usage = 0 },
         new KeyboardFilter { VendorId = 14391, ProductId = 12310,  UsagePage =  1,     Usage = 0 },
         new KeyboardFilter { VendorId = 14391, ProductId = 12311,  UsagePage =  65280, Usage = 1 },
         new KeyboardFilter { VendorId = 14391, ProductId = 12339,  UsagePage =  1,     Usage = 0 },
         new KeyboardFilter { VendorId = 14391, ProductId = 12340,  UsagePage =  65280, Usage = 1 },
         new KeyboardFilter { VendorId = 14391, ProductId = 8206,   UsagePage =  1,     Usage = 0 },
         new KeyboardFilter { VendorId = 14391, ProductId = 8207,   UsagePage =  65280, Usage = 1 },
         new KeyboardFilter { VendorId = 14391, ProductId = 8207,   UsagePage =  1,     Usage = 0 },
         new KeyboardFilter { VendorId = 14391, ProductId = 8208,   UsagePage =  65280, Usage = 1 },
    ];

    public KeyboardWithSpecs? _keyboardWithSpecs;
    public KeyboardWithSpecs? KeyboardWithSpecs
    {
        get { return _keyboardWithSpecs; }
        set
        {
            if (!EqualityComparer<string?>.Default.Equals(_keyboardWithSpecs?.Keyboard.ToString(), value?.Keyboard.ToString()))
            {
                _keyboardWithSpecs = value;
                ConnectedKeyboardChanged?.Invoke(_keyboardWithSpecs);
            }
        }
    }
    public event Action<KeyboardWithSpecs?>? ConnectedKeyboardChanged;

    public KeyboardManager() { KeyboardWithSpecs = FindKeyboard(); Register(); }

    private void OnDeviceListChanged(object? sender, DeviceListChangedEventArgs e)
    {
        if (KeyboardWithSpecs is { } keyboard && keyboard.Keyboard.CanOpen && IsConnected(keyboard.Keyboard)) return;

        KeyboardWithSpecs = FindKeyboard();
    }

    private static bool IsConnected(HidDevice device)
        => DeviceList.Local.GetHidDevices().Any(_device => _device == device);

    private static KeyboardWithSpecs? FindKeyboard()
    {
        var (kb, specs) = DeviceList.Local.GetHidDevices()
            .Where(IsCompatibleKeyboard)
            .Select(kb => (kb, kb.Open().Using(s => s.GetKeyboardSpecs())))
            .OfType<KeyboardWithSpecs>()
            .FirstOrDefault();
        if (kb is not { } keyboard)
        {
            return null;
        }
        Console.WriteLine("Supported keyboard found {1} with firmware version {1}", keyboard.GetFriendlyName(), specs.Info.FirmwareVersion);
        return (keyboard, specs);
    }

    public static bool IsCompatibleKeyboard(HidDevice device)
    {
        // The HidSharp lib doesn't allow for easy access to the Usage and Usage page attributes.
        // Instead we check if the input and output report length are both over 64 bytes.
        // This indicates we probably have a device with read and write stream capability.
        return CompatibleKeyboards.Any(ddkbs => ddkbs.ProductId == device.ProductID && ddkbs.VendorId == device.VendorID && device.GetMaxOutputReportLength() >= 64 && device.GetMaxInputReportLength() >= 64);
    }

    public void Register() { DeviceList.Local.Changed += OnDeviceListChanged; }

    public void Unregister() { DeviceList.Local.Changed -= OnDeviceListChanged; }

    public void Dispose()
    {
        Unregister();
        KeyboardWithSpecs = null;
    }
}
