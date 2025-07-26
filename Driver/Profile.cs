using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Driver;

public record Profile : INotifyPropertyChanged
{
    [JsonIgnore]
    private string name = string.Empty;
    [JsonIgnore]
    private bool selectedForQuickSwitch = false;
    [JsonIgnore]
    private bool isDefault = false;
    [JsonIgnore]
    private string[] processTriggers = [];
    [JsonIgnore]
    private KeyboardProfile? keyboardProfile;

    [JsonIgnore]
    public bool IsDirty { get; set; }
    public event PropertyChangedEventHandler? PropertyChanged;


    [JsonIgnore]
    public string Name
    {
        get { return name; }
        set { SetField(ref name, value, nameof(Name)); }
    }

    public bool SelectedForQuickSwitch
    {
        get { return selectedForQuickSwitch; }
        set { SetField(ref selectedForQuickSwitch, value, nameof(SelectedForQuickSwitch)); }
    }

    public bool IsDefault
    {
        get { return isDefault; }
        set { SetField(ref isDefault, value, nameof(IsDefault)); }
    }

    public string[] ProcessTriggers
    {
        get { return processTriggers; }
        set { SetField(ref processTriggers, value, nameof(ProcessTriggers)); }
    }

    public required KeyboardProfile KeyboardProfile
    {
        get
        {
            if (keyboardProfile is null)
            {
                throw new Exception("Profile is null");
            }
            return keyboardProfile;
        }
        set { SetField(ref keyboardProfile, value, nameof(KeyboardProfile)); }
    }

    protected void SetField<T>(ref T field, T value, string propertyName)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            IsDirty = true;
            OnPropertyChanged(propertyName);
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
