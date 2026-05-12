using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DeathCounter;

internal sealed class GlobalInputHook : IDisposable
{
    private readonly Action<HotkeyBinding> onHotkeyPressed;
    private readonly HashSet<Keys> pressedKeys = [];
    private readonly NativeMethods.HookProc keyboardProc;
    private readonly NativeMethods.HookProc mouseProc;
    private IntPtr keyboardHook = IntPtr.Zero;
    private IntPtr mouseHook = IntPtr.Zero;

    public GlobalInputHook(Action<HotkeyBinding> onHotkeyPressed)
    {
        this.onHotkeyPressed = onHotkeyPressed;
        keyboardProc = KeyboardHookCallback;
        mouseProc = MouseHookCallback;
        InstallHooks();
    }

    public void Dispose()
    {
        if (keyboardHook != IntPtr.Zero)
        {
            NativeMethods.UnhookWindowsHookEx(keyboardHook);
            keyboardHook = IntPtr.Zero;
        }

        if (mouseHook != IntPtr.Zero)
        {
            NativeMethods.UnhookWindowsHookEx(mouseHook);
            mouseHook = IntPtr.Zero;
        }
    }

    private void InstallHooks()
    {
        var moduleHandle = NativeMethods.GetCurrentModuleHandle();
        keyboardHook = NativeMethods.SetWindowsHookEx(NativeMethods.WhKeyboardLl, keyboardProc, moduleHandle, 0);
        mouseHook = NativeMethods.SetWindowsHookEx(NativeMethods.WhMouseLl, mouseProc, moduleHandle, 0);
    }

    private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            var keyboardData = Marshal.PtrToStructure<NativeMethods.Kbdllhookstruct>(lParam);
            var key = (Keys)keyboardData.VkCode;
            var message = wParam.ToInt32();

            if (message is NativeMethods.WmKeydown or NativeMethods.WmSyskeydown)
            {
                if (!IsModifierKey(key) && pressedKeys.Add(key))
                {
                    onHotkeyPressed(CreateKeyboardBinding(key));
                }
            }
            else if (message is NativeMethods.WmKeyup or NativeMethods.WmSyskeyup)
            {
                pressedKeys.Remove(key);
            }
        }

        return NativeMethods.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
    }

    private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            var message = wParam.ToInt32();
            var mouseButton = TryGetMouseButton(message, lParam);
            if (mouseButton.HasValue)
            {
                onHotkeyPressed(CreateMouseBinding(mouseButton.Value));
            }
        }

        return NativeMethods.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
    }

    private static bool IsModifierKey(Keys key) =>
        key is Keys.ControlKey or Keys.ShiftKey or Keys.Menu or Keys.LControlKey or Keys.RControlKey or
            Keys.LShiftKey or Keys.RShiftKey or Keys.LMenu or Keys.RMenu;

    private static HotkeyBinding CreateKeyboardBinding(Keys key) =>
        new(
            key,
            IsModifierDown(NativeMethods.VkControl),
            IsModifierDown(NativeMethods.VkMenu),
            IsModifierDown(NativeMethods.VkShift));

    private static HotkeyBinding CreateMouseBinding(HotkeyMouseButton button) =>
        new(
            button,
            IsModifierDown(NativeMethods.VkControl),
            IsModifierDown(NativeMethods.VkMenu),
            IsModifierDown(NativeMethods.VkShift));

    private static bool IsModifierDown(int virtualKey) =>
        (NativeMethods.GetAsyncKeyState(virtualKey) & 0x8000) != 0;

    private static HotkeyMouseButton? TryGetMouseButton(int message, IntPtr lParam)
    {
        return message switch
        {
            NativeMethods.WmLbuttondown => HotkeyMouseButton.Left,
            NativeMethods.WmRbuttondown => HotkeyMouseButton.Right,
            NativeMethods.WmMbuttondown => HotkeyMouseButton.Middle,
            NativeMethods.WmXbuttondown => GetXButton(lParam),
            _ => null,
        };
    }

    private static HotkeyMouseButton? GetXButton(IntPtr lParam)
    {
        var mouseData = Marshal.PtrToStructure<NativeMethods.Msllhookstruct>(lParam).MouseData >> 16;
        return mouseData switch
        {
            NativeMethods.Xbutton1 => HotkeyMouseButton.XButton1,
            NativeMethods.Xbutton2 => HotkeyMouseButton.XButton2,
            _ => null,
        };
    }
}
