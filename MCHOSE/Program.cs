using Driver;
using System.IO;
using System.Runtime.InteropServices;

namespace MCHOSE;

public partial class Program
{
    public static readonly string APP_DIR = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Path.GetFileNameWithoutExtension(Environment.ProcessPath ?? "MCHOSE UI"));

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool AllocConsole();

    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Contains("--console"))
        {
            AllocConsole();
            Console.SetWindowSize(220, 32);
        }
        if (args.Contains("--start-minimized"))
        {
            //MainWindow.ShouldStartMinimized = true;
        }

        using var _ = new KeyboardManager();

        //App app = new();
        //app.InitializeComponent();
        //app.Run();
        //App.Application_Exit();
    }
}
