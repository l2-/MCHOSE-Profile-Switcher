using Common;
using Driver;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using UI.Components;
using UI.Hooks;
using UI.Profile;
using Application = System.Windows.Application;

namespace UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public static bool ShouldStartMinimized { get; set; } = false;
    private readonly Dictionary<int, KeyHandler> handlers = [];
    private readonly ProfileManager ProfileManager;
    private readonly KeyboardManager KeyboardManager;
    private readonly WinEventHook WinEventHook;
    private ProcessSelector? processSelectorWindow;

    public MainWindow(
        ProfileManager profileManager,
        WinEventHook winEventHook,
        TrayIcon icon,
        KeyboardManager keyboardManager)
    {
        WinEventHook = winEventHook;
        ProfileManager = profileManager;
        KeyboardManager = keyboardManager;
        InitializeComponent();
        icon.DoubleClick = Restore;
        icon.AppShouldClose = Close;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        DiscoverProfiles();
        RegisterKeyHandler();

        StartOnWindowsStartupToggle.IsChecked = StartupShortcutHelper.StartupFileExists();
        StartOnWindowsStartupToggle.Click += OnCheckChanged;

        dataGrid.ContextMenu = CreateDatagridContextMenu();

        WinEventHook.WinEventHookHandler += OnWinEventHook;

        KeyboardManager.ConnectedKeyboardChanged += OnConnectedKeyboardChanged;
        OnConnectedKeyboardChanged(KeyboardManager.KeyboardWithSpecs);
    }

    private static void OnUiThread(Action action)
    {
        if (Application.Current.Dispatcher.CheckAccess())
        {
            action();
        }
        else
        {
            Application.Current.Dispatcher.Invoke(action);
        }
    }

    private ContextMenu CreateDatagridContextMenu()
    {
        var menu = new ContextMenu();
        var removeItem = new MenuItem() { Header = "Remove" };
        removeItem.Click += OnRemoveClicked;
        menu.Items.Add(removeItem);

        void OnRemoveClicked(object sender, RoutedEventArgs e)
        {
            if (menu.PlacementTarget is DataGrid grid)
            {
                var itemsToRemove = grid.SelectedCells.Select(sc => sc.Item).OfType<Driver.Profile>().Distinct().ToArray();
                ProfileManager.RemoveProfileItems(itemsToRemove);
            }
        }
        return menu;
    }

    private void OnWinEventHook(object? sender, WinEventProcEvent e)
    {
        var path = e.Event.Process.GetPathFromProcessId();
        var profileToSwitchTo = ProfileManager.Profiles.FirstOrDefault(p => p.ProcessTriggers.Any(pt => pt.Equals(path, StringComparison.OrdinalIgnoreCase)));
        if (profileToSwitchTo is { } profile)
        {
            ProfileManager.SwitchTo(profile, true);
        }
        else if (ProfileManager.ProcessMatch && ProfileManager.LastProfile is { } lastProfile)
        {
            ProfileManager.SwitchTo(lastProfile, false);
        }
    }

    private void OnConnectedKeyboardChanged(KeyboardWithSpecs? keyboardWithSpecs)
    {
        OnUiThread(() =>
        {
            if (keyboardWithSpecs is { } _keyboardWithSpecs)
            {
                CurrentConnectedKeyboard.Content = string.Format("Connected to: \t{0} \tfirmware v{1}",
                    _keyboardWithSpecs.Keyboard.GetFriendlyName(),
                    _keyboardWithSpecs.Specs.Info.FirmwareVersion);
            }
            else
            {
                CurrentConnectedKeyboard.Content = "No Keyboard connected";
            }
        });
    }

    private void RefreshDataGrid()
    {
        dataGrid.Columns.First().Width = 0;
        var col = dataGrid.Columns.First(c => c.Header.Equals("Process triggers"));
        col.Width = DataGridLength.SizeToCells;
        dataGrid.UpdateLayout();
        dataGrid.Columns.First().Width = new DataGridLength(1, DataGridLengthUnitType.Star);
    }

    private void ProfileChanged(int index, Driver.Profile item)
    {
        OnUiThread(() =>
        {
            CurrentProfileLabel.Content = string.Format("Current Profile: \t{0}", item.Name);
            _ = ProfileManager.PushCurrentProfile();
        });
    }

    private void ProfilesChanged(Driver.Profile[] _)
    {
        OnUiThread(RefreshDataGrid);
    }

    private void DiscoverProfiles()
    {
        ProfileManager.CurrentProfileChanged += ProfileChanged;
        dataGrid.ItemsSource = ProfileManager.Profiles;
        ProfileManager.ProfileCollectionChanged += ProfilesChanged;
        ProfileManager.DiscoverProfiles();
    }

    private void RegisterKeyHandler()
    {
        var windowHandle = new WindowInteropHelper(this).Handle;
        var source = HwndSource.FromHwnd(windowHandle);

        var enterHandler = new KeyHandler(KeyHandler.ToKeycode(Key.Enter), windowHandle, source, KeyHandler.MOD_CONTROL | KeyHandler.MOD_ALT | KeyHandler.MOD_NOREPEAT)
        {
            Callback = ProfileManager.QuickSwitchProfile,
        };
        handlers[enterHandler.GetHashCode()] = enterHandler;
        foreach (var handler in handlers.Values)
        {
            handler.Register();
        }
    }

    private void Window_StateChanged(object sender, EventArgs e)
    {
        if (WindowState is WindowState.Minimized)
        {
            WindowStyle = WindowStyle.ToolWindow;
            ShowInTaskbar = false;
        }
        else
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
            ShowInTaskbar = true;
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (ShouldStartMinimized)
        {
            WindowState = WindowState.Minimized;
        }
    }

    public void Restore()
    {
        if (WindowState is WindowState.Minimized)
        {
            WindowState = WindowState.Normal;
        }
    }

    protected void OnImportButtonClicked(object sender, EventArgs e)
    {
        // Configure open file dialog box
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            DefaultExt = ".json", // Default file extension
            Filter = "Text documents (.json)|*.json", // Filter files by extension
            Multiselect = true,
        };

        // Show open file dialog box
        bool? result = dialog.ShowDialog();

        // Process open file dialog box results
        if (result == true)
        {
            foreach (var path in dialog.FileNames)
            {
                ProfileManager.ImportProfile(path);
            }
        }
    }

    private void OnCheckChanged(object? sender, EventArgs e)
    {
        StartupShortcutHelper.OnCheckChanged(StartOnWindowsStartupToggle.IsChecked ?? false);
    }

    private void HandleStoredProcessesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (processSelectorWindow is { } window)
        {
            window.Profile.ProcessTriggers = [.. window.StoredProcesses.Select(pr => pr.ProcessPath)];
        }
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button && button.DataContext is Driver.Profile profile)
        {
            processSelectorWindow?.Close();
            processSelectorWindow = null;
            processSelectorWindow = new ProcessSelector(profile);
            processSelectorWindow.SetStoredProcesses(profile.ProcessTriggers);
            // Bind after creating the collection
            processSelectorWindow.StoredProcesses.CollectionChanged += HandleStoredProcessesCollectionChanged;
            processSelectorWindow.Show();
        }
    }
}
