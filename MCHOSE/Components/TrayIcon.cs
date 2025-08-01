using UI.Profile;
using UI.Properties;

namespace UI.Components;

public sealed class ProfileItemToolStripItem : ToolStripMenuItem
{
    private readonly ProfileManager profileManager;
    public readonly Driver.Profile profile;
    public readonly bool isSelected;

    public ProfileItemToolStripItem(ProfileManager profileManager, Driver.Profile profile, bool isSelected)
    {
        this.profileManager = profileManager;
        this.profile = profile;
        this.isSelected = isSelected;
        Text = profile.Name;
        if (isSelected)
        {
            Image = Resources.checkmark.ToBitmap();
        }
        DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
    }

    protected override void OnClick(EventArgs e)
    {
        profileManager.SwitchTo(profile, false);
        base.OnClick(e);
    }
}

public sealed class TrayIcon : IDisposable
{
    private readonly NotifyIcon icon;
    private readonly ProfileManager profileManager;
    public Action DoubleClick = () => { };
    public Action AppShouldClose = () => { };

    public TrayIcon(ProfileManager profileManager)
    {
        this.profileManager = profileManager;
        this.profileManager.CurrentProfileChanged += ProfileChanged;
        this.profileManager.ProfileCollectionChanged += ProfileCollectionChanged;
        icon = new()
        {
            Icon = Resources.keyboard,
            Visible = true
        };
        icon.BalloonTipClosed += (sender, e) =>
        {
            var thisIcon = sender as NotifyIcon;
            if (thisIcon is { } icon)
            {
                icon.Visible = false;
                icon.Dispose();
            }
        };
        icon.DoubleClick += new EventHandler(TrayIconOnClick);
        icon.ContextMenuStrip = CreateContextMenu();
    }

    private ContextMenuStrip CreateContextMenu()
    {
        ContextMenuStrip menu = new();

        var label = new ToolStripLabel("Select a profile")
        {
            Margin = new Padding(0, 3, 0, 3),
        };
        menu.Items.Add(label);
        menu.Items.Add(new ToolStripSeparator());
        var items = profileManager.Profiles.Select(profile => new ProfileItemToolStripItem(profileManager, profile, profileManager.IsSelected(profile))).ToArray();
        menu.Items.AddRange(items);
        menu.Items.Add(new ToolStripSeparator());
        var exit = new ToolStripMenuItem() { Text = "Exit", DisplayStyle = ToolStripItemDisplayStyle.Text };
        exit.Click += (sender, e) => AppShouldClose.Invoke();
        menu.Items.Add(exit);
        return menu;
    }

    void TrayIconOnClick(object? sender, EventArgs e)
    {
        DoubleClick.Invoke();
    }

    private void ProfileCollectionChanged(Driver.Profile[] _)
    {
        icon.ContextMenuStrip = CreateContextMenu();
    }

    private void ProfileChanged(int index, Driver.Profile item)
    {
        icon.Text = string.Format("Current Profile: {0}", item.Name);
        icon.ContextMenuStrip = CreateContextMenu();
    }

    public void Dispose()
    {
        icon.Visible = false;
        icon.Dispose();
    }
}
