namespace DeathCounter;

internal sealed class GlobalHotkeyManager : IDisposable
{
    private readonly GlobalInputHook inputHook;
    private HotkeyBinding? increaseHotkey;
    private HotkeyBinding? decreaseHotkey;
    private HotkeyBinding? resetHotkey;
    private HotkeyBinding? stopSoundHotkey;
    private Action? onIncrease;
    private Action? onDecrease;
    private Action? onReset;
    private Action? onStopSound;

    public GlobalHotkeyManager()
    {
        inputHook = new GlobalInputHook(HandleHotkeyPressed);
    }

    public void Configure(
        HotkeyBinding increase,
        HotkeyBinding decrease,
        HotkeyBinding reset,
        HotkeyBinding stopSound,
        Action onIncrease,
        Action onDecrease,
        Action onReset,
        Action onStopSound)
    {
        increaseHotkey = increase.Clone();
        decreaseHotkey = decrease.Clone();
        resetHotkey = reset.Clone();
        stopSoundHotkey = stopSound.Clone();
        this.onIncrease = onIncrease;
        this.onDecrease = onDecrease;
        this.onReset = onReset;
        this.onStopSound = onStopSound;
    }

    public void Clear()
    {
        increaseHotkey = null;
        decreaseHotkey = null;
        resetHotkey = null;
        stopSoundHotkey = null;
        onIncrease = null;
        onDecrease = null;
        onReset = null;
        onStopSound = null;
    }

    public void Dispose()
    {
        inputHook.Dispose();
    }

    private void HandleHotkeyPressed(HotkeyBinding pressed)
    {
        if (increaseHotkey is not null && increaseHotkey.Matches(pressed))
        {
            onIncrease?.Invoke();
            return;
        }

        if (decreaseHotkey is not null && decreaseHotkey.Matches(pressed))
        {
            onDecrease?.Invoke();
            return;
        }

        if (resetHotkey is not null && resetHotkey.Matches(pressed))
        {
            onReset?.Invoke();
            return;
        }

        if (stopSoundHotkey is not null && stopSoundHotkey.Matches(pressed))
        {
            onStopSound?.Invoke();
        }
    }
}
