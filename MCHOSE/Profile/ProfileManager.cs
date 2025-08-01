using Driver;
using HidSharp;
using MCHOSEUI.Extensions;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Text.Json;

namespace UI.Profile;

public sealed class ProfileManager(KeyboardManager keyboardManager, Settings settings)
{
    public ObservableCollection<Driver.Profile> Profiles { get; private set; } = [];
    public List<Tuple<Driver.Profile, string>> ProfileFileNames { get; private set; } = [];
    private readonly JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true, WriteIndented = true };
    private readonly string profileDir = Path.Combine(Program.APP_DIR, "profiles");
    private readonly KeyboardManager keyboardManager = keyboardManager;
    private readonly Settings settings = settings;
    private int lastIndex = -1;
    private int currentIndex = -1;
    public bool ProcessMatch { get; private set; } = false;
    private static readonly System.Threading.Lock writePacketLock = new();

    public Driver.Profile? LastProfile
    {
        get
        {
            return lastIndex >= 0 && lastIndex < Profiles.Count ? Profiles[lastIndex] : null;
        }
    }

    public int CurrentIndex
    {
        get { return currentIndex; }
        set
        {
            if (!EqualityComparer<int>.Default.Equals(currentIndex, value))
            {
                lastIndex = currentIndex;
                currentIndex = value;
                CurrentProfileChanged?.Invoke(currentIndex, Profiles[currentIndex]);
            }
        }
    }

    public event Action<int, Driver.Profile>? CurrentProfileChanged;
    public event Action<Driver.Profile[]>? ProfileCollectionChanged;

    public void DiscoverProfiles()
    {
        var info = Directory.CreateDirectory(profileDir);
        var discoveredProfiles = info.EnumerateFiles().Where(f => Path.GetExtension(f.Name) == ".json").Select(f => FromJsonFile(f.FullName)).Where(p => p is not null).Select(p => p!).ToArray();
        foreach (var profile in discoveredProfiles)
        {
            profile.PropertyChanged += ProfileItemChanged;
            Profiles.Add(profile);
        }
        ProfileCollectionChanged?.Invoke(discoveredProfiles);
        ProfileFileNames = [.. Profiles.Select(p => Tuple.Create(p, p.Name))];
        if (settings.LastProfileUsedName is { } s && !s.Equals(string.Empty))
        {
            var current = Profiles.FindIndex(p => p.Name.Equals(s));
            if (current >= 0 && current != CurrentIndex && current < Profiles.Count)
            {
                CurrentIndex = current;
            }
            else
            {
                current = Math.Max(Profiles.FindIndex(p => p.IsDefault), 0);
                if (current >= 0 && current != CurrentIndex && current < Profiles.Count)
                {
                    CurrentIndex = current;
                }
            }
        }
        else
        {
            var current = Math.Max(Profiles.FindIndex(p => p.IsDefault), 0);
            if (current >= 0 && current != CurrentIndex && current < Profiles.Count)
            {
                CurrentIndex = current;
            }
        }
    }

    private Driver.Profile? FromJsonFile(string path)
    {
        var text = File.ReadAllText(path);
        try
        {
            var profile = JsonSerializer.Deserialize<Driver.Profile>(text, options);
            if (profile != null)
            {
                profile.Name = Path.GetFileNameWithoutExtension(path);
                profile.IsDirty = false;
            }
            return profile;
        }
        catch (JsonException) { Console.WriteLine("Failed to deserialize profile file at {0}", path); }
        return null;
    }

    public void ImportProfile(string path)
    {
        try
        {
            var text = File.ReadAllText(path);
            var profile = JsonSerializer.Deserialize<KeyboardProfile>(text, options);
            if (profile is null) { Console.WriteLine("Failed importing {0}!", path); return; }
            var profileItem = new Driver.Profile
            {
                Name = Path.GetFileNameWithoutExtension(path),
                KeyboardProfile = profile,
                IsDirty = false
            };
            Save(profileItem);
            profileItem.PropertyChanged += ProfileItemChanged;
            Profiles.Add(profileItem);
            ProfileCollectionChanged?.Invoke([profileItem]);
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
        }
    }

    private void Save(Driver.Profile item)
    {
        var json = JsonSerializer.Serialize(item, options);
        var indexOld = ProfileFileNames.FindIndex(t => t.Item1 == item);
        if (indexOld >= 0 && !ProfileFileNames[indexOld].Item2.Equals(item.Name))
        {
            var old = ProfileFileNames[indexOld];
            // changed profile name, remove old one
            File.Delete(Path.Combine(profileDir, old.Item2 + ".json"));
            Console.WriteLine("Removing {0}", old.Item2);
            ProfileFileNames.RemoveAt(indexOld);
        }
        File.WriteAllText(Path.Combine(profileDir, item.Name + ".json"), json);
        Console.WriteLine("Saving {0}", item.Name);
        ProfileFileNames.Add(Tuple.Create(item, item.Name));
    }

    public void ProfileItemChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is Driver.Profile item && item.IsDirty)
        {
            if (nameof(Driver.Profile.IsDefault).Equals(e.PropertyName) && item.IsDefault)
            {
                foreach (var profile in Profiles.Where(p => p != item))
                {
                    profile.IsDefault = false;
                }
            }
            foreach (var profile in Profiles.Where(p => p.IsDirty))
            {
                Save(profile);
                profile.IsDirty = false;
            }
        }
    }

    public async Task PushCurrentProfile()
    {
        await Task.Run(() =>
        {
            lock (writePacketLock)
            {
                if (CurrentIndex >= Profiles.Count)
                {
                    Console.WriteLine("Current profile out of range!");
                    return;
                }
                var current = Profiles[CurrentIndex];
                Console.WriteLine("Pushing profile {0} to keyboard", current.Name);
                if (keyboardManager.KeyboardWithSpecs is { } keyboard)
                {
                    using HidStream stream = keyboard.Keyboard.Open();
                    stream.PushProfile(current.KeyboardProfile);
                }
                settings.LastProfileUsedName = current.Name;
            }
        });
    }

    public void QuickSwitchProfile()
    {
        var quickSwitchProfiles = Profiles.Where(p => p.SelectedForQuickSwitch).ToList();
        if (quickSwitchProfiles.Count < 2) return;
        var current = Profiles[Math.Max(CurrentIndex, 0)];
        var currentIndex = quickSwitchProfiles.IndexOf(current);
        var next = quickSwitchProfiles[(currentIndex + 1) % quickSwitchProfiles.Count];
        Console.WriteLine("Switching from {0} to profile {1}", current.Name, next.Name);
        SwitchTo(next, false);
    }

    public void RemoveProfileItems(params Driver.Profile[] items)
    {
        if (items.Length == 0) return;

        foreach (var item in items)
        {
            Profiles.Remove(item);
            var profileFileNamesIndex = ProfileFileNames.FindIndex(p => p.Item1 == item);
            if (profileFileNamesIndex < 0 || profileFileNamesIndex >= ProfileFileNames.Count) return;
            ProfileFileNames.RemoveAt(profileFileNamesIndex);
            File.Delete(Path.Combine(profileDir, item.Name + ".json"));
            Console.WriteLine("Removing {0}", item.Name);
        }
        ProfileCollectionChanged?.Invoke(items);
    }

    public bool IsSelected(Driver.Profile profileItem)
    {
        return Profiles.IndexOf(profileItem) == CurrentIndex;
    }

    public void SwitchTo(Driver.Profile profileItem, bool processMatch)
    {
        ProcessMatch = processMatch;
        var i = Profiles.IndexOf(profileItem);
        if (i >= 0 && i < Profiles.Count)
        {
            CurrentIndex = i;
        }
    }
}
