using System.Text.Json.Serialization;

namespace DeathCounter;

internal enum HotkeyTriggerType
{
    Keyboard,
    MouseButton,
}

internal enum HotkeyMouseButton
{
    Left,
    Right,
    Middle,
    XButton1,
    XButton2,
}

internal sealed class HotkeyBinding
{
    public HotkeyBinding()
    {
    }

    public HotkeyBinding(Keys key, bool control = false, bool alt = false, bool shift = false)
    {
        TriggerType = HotkeyTriggerType.Keyboard;
        Key = key;
        Control = control;
        Alt = alt;
        Shift = shift;
    }

    public HotkeyBinding(HotkeyMouseButton mouseButton, bool control = false, bool alt = false, bool shift = false)
    {
        TriggerType = HotkeyTriggerType.MouseButton;
        MouseButton = mouseButton;
        Control = control;
        Alt = alt;
        Shift = shift;
    }

    public HotkeyTriggerType TriggerType { get; set; } = HotkeyTriggerType.Keyboard;
    public Keys Key { get; set; }
    public HotkeyMouseButton MouseButton { get; set; }
    public bool Control { get; set; }
    public bool Alt { get; set; }
    public bool Shift { get; set; }

    [JsonIgnore]
    public bool IsEmpty =>
        TriggerType == HotkeyTriggerType.Keyboard ? Key == Keys.None : false;

    public HotkeyBinding Clone()
    {
        return TriggerType == HotkeyTriggerType.MouseButton
            ? new HotkeyBinding(MouseButton, Control, Alt, Shift)
            : new HotkeyBinding(Key, Control, Alt, Shift);
    }

    public bool Matches(HotkeyBinding other)
    {
        return TriggerType == other.TriggerType &&
            Key == other.Key &&
            MouseButton == other.MouseButton &&
            Control == other.Control &&
            Alt == other.Alt &&
            Shift == other.Shift;
    }

    public override string ToString()
    {
        var parts = new List<string>();

        if (Control)
        {
            parts.Add("Ctrl");
        }

        if (Alt)
        {
            parts.Add("Alt");
        }

        if (Shift)
        {
            parts.Add("Shift");
        }

        parts.Add(TriggerType == HotkeyTriggerType.MouseButton ? MouseButtonToDisplayName(MouseButton) : Key.ToString());
        return string.Join(" + ", parts);
    }

    private static string MouseButtonToDisplayName(HotkeyMouseButton mouseButton) =>
        mouseButton switch
        {
            HotkeyMouseButton.Left => "Mouse Left",
            HotkeyMouseButton.Right => "Mouse Right",
            HotkeyMouseButton.Middle => "Mouse Middle",
            HotkeyMouseButton.XButton1 => "Mouse X1",
            HotkeyMouseButton.XButton2 => "Mouse X2",
            _ => mouseButton.ToString(),
        };
}
