using System.Diagnostics;

namespace DeathCounter;

internal sealed class MainForm : Form
{
    private readonly AppSettings appSettings;
    private readonly AudioPlayerService audioPlayer;
    private readonly CounterDisplayControl counterDisplay;
    private readonly GlobalHotkeyManager hotkeyManager;
    private readonly MenuStrip menuStrip;
    private readonly ToolStripMenuItem currentCounterItem;
    private bool counterInitialized;
    private string currentCounterFilePath = string.Empty;
    private CounterProfile currentProfile = new();

    public MainForm()
    {
        appSettings = AppSettings.Load();
        audioPlayer = new AudioPlayerService();
        hotkeyManager = new GlobalHotkeyManager();
        Icon = AppIconLoader.LoadApplicationIcon();

        menuStrip = new MenuStrip();
        menuStrip.MouseDown += BeginWindowDrag;

        var fileItem = new ToolStripMenuItem("Datei");
        var loadItem = new ToolStripMenuItem("Counter laden");
        loadItem.Click += (_, _) => SelectAndLoadCounter(false);
        var newItem = new ToolStripMenuItem("Neuen Counter anlegen");
        newItem.Click += (_, _) => SelectAndLoadCounter(true);
        var editItem = new ToolStripMenuItem("Aktuellen Counter bearbeiten");
        editItem.Click += (_, _) => EditCurrentCounter();
        var exitItem = new ToolStripMenuItem("Beenden");
        exitItem.Click += (_, _) => Close();
        fileItem.DropDownItems.Add(loadItem);
        fileItem.DropDownItems.Add(newItem);
        fileItem.DropDownItems.Add(editItem);
        fileItem.DropDownItems.Add(new ToolStripSeparator());
        fileItem.DropDownItems.Add(exitItem);

        var optionsItem = new ToolStripMenuItem("Optionen");
        var settingsItem = new ToolStripMenuItem("Einstellungen");
        settingsItem.Click += (_, _) => ShowSettingsDialog();
        optionsItem.DropDownItems.Add(settingsItem);

        var helpItem = new ToolStripMenuItem("About");
        helpItem.Click += (_, _) => ShowAboutDialog();

        var githubItem = new ToolStripMenuItem("Github");
        githubItem.Click += (_, _) => OpenGithubPage();

        currentCounterItem = new ToolStripMenuItem("Kein Counter geladen")
        {
            Alignment = ToolStripItemAlignment.Right,
            Enabled = false,
        };

        menuStrip.Items.Add(fileItem);
        menuStrip.Items.Add(optionsItem);
        menuStrip.Items.Add(helpItem);
        menuStrip.Items.Add(githubItem);
        menuStrip.Items.Add(currentCounterItem);

        counterDisplay = new CounterDisplayControl
        {
            Dock = DockStyle.Fill,
        };
        counterDisplay.MouseDown += BeginWindowDrag;

        SuspendLayout();
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = appSettings.BackgroundColor;
        ClientSize = new Size(600, 300);
        Controls.Add(counterDisplay);
        Controls.Add(menuStrip);
        DoubleBuffered = true;
        FormBorderStyle = FormBorderStyle.None;
        MainMenuStrip = menuStrip;
        MinimumSize = new Size(350, 220);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Death Counter";
        ResumeLayout(false);
        PerformLayout();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        if (counterInitialized)
        {
            return;
        }

        counterInitialized = true;
        RegisterConfiguredHotkeys();
        EnsureInitialCounterLoaded();
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        hotkeyManager.Dispose();
        audioPlayer.Dispose();
        base.OnHandleDestroyed(e);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (HasLoadedCounter() && File.Exists(currentCounterFilePath))
        {
            currentProfile.Save(currentCounterFilePath);
        }

        appSettings.Save();
        base.OnFormClosing(e);
    }

    private void EnsureInitialCounterLoaded()
    {
        var lastCounterFilePath = ResolveLastCounterFilePath();
        if (!string.IsNullOrWhiteSpace(lastCounterFilePath))
        {
            LoadCounter(lastCounterFilePath);
            return;
        }

        SelectAndLoadCounter(false);

        if (string.IsNullOrWhiteSpace(currentCounterFilePath))
        {
            Close();
        }
    }

    private void SelectAndLoadCounter(bool preferCreateNew)
    {
        using var dialog = new CounterSelectionForm(appSettings);
        if (preferCreateNew)
        {
            dialog.Text = "Neuen Counter anlegen oder laden";
        }

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        LoadCounter(dialog.SelectedCounterFilePath);
    }

    private void LoadCounter(string filePath)
    {
        currentProfile = CounterProfile.Load(filePath);
        currentCounterFilePath = filePath;
        appSettings.LastCounterFilePath = filePath;
        appSettings.LastCounterName = currentProfile.Name;
        appSettings.Save();

        RefreshCounterUi(false);
        Text = $"Death Counter - {currentProfile.Name}";
    }

    private void EditCurrentCounter()
    {
        if (!HasLoadedCounter())
        {
            MessageBox.Show(
                "Bitte zuerst einen Counter laden.",
                "Kein Counter geladen",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        using var dialog = new CounterEditorForm(
            "Counter bearbeiten",
            currentProfile.Name,
            currentProfile.StartValue,
            currentProfile.CounterValue);

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var originalFilePath = currentCounterFilePath;
        var updatedFilePath = CounterFileService.BuildCounterFilePath(appSettings.CountersDirectory, dialog.CounterName);
        var shouldRenameFile = !string.Equals(originalFilePath, updatedFilePath, StringComparison.OrdinalIgnoreCase);

        if (shouldRenameFile && File.Exists(updatedFilePath))
        {
            MessageBox.Show(
                "Ein Counter mit diesem Namen existiert bereits.",
                "Name bereits vorhanden",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        currentProfile.Name = dialog.CounterName;
        currentProfile.StartValue = dialog.StartValue;
        currentProfile.CounterValue = dialog.CurrentValue;

        if (shouldRenameFile && File.Exists(originalFilePath))
        {
            File.Move(originalFilePath, updatedFilePath);
            currentCounterFilePath = updatedFilePath;
        }

        SaveAndRefreshCounter(false);
        appSettings.LastCounterFilePath = currentCounterFilePath;
        appSettings.LastCounterName = currentProfile.Name;
        appSettings.Save();
        Text = $"Death Counter - {currentProfile.Name}";
    }

    private void ShowSettingsDialog()
    {
        hotkeyManager.Clear();

        using var dialog = new SettingsForm(appSettings);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            RegisterConfiguredHotkeys();
            return;
        }

        appSettings.IncreaseHotkey = dialog.IncreaseHotkey.Clone();
        appSettings.DecreaseHotkey = dialog.DecreaseHotkey.Clone();
        appSettings.ResetHotkey = dialog.ResetHotkey.Clone();
        appSettings.StopSoundHotkey = dialog.StopSoundHotkey.Clone();
        appSettings.BackgroundArgb = dialog.BackgroundColor.ToArgb();
        appSettings.CountersDirectory = dialog.CountersDirectory;
        appSettings.Mp3Directory = dialog.Mp3Directory;
        appSettings.SoundVolumePercent = dialog.SoundVolumePercent;
        appSettings.UseThreeDigitCounterFormat = dialog.UseThreeDigitCounterFormat;
        appSettings.CounterFontFamily = dialog.CounterFontFamily;
        appSettings.CounterFontSize = dialog.CounterFontSize;
        appSettings.EnableCounterAnimation = dialog.EnableCounterAnimation;
        appSettings.CounterFillArgb = dialog.CounterFillColor.ToArgb();
        appSettings.CounterOutlineArgb = dialog.CounterOutlineColor.ToArgb();
        appSettings.CounterFontBold = dialog.CounterFontBold;
        appSettings.ShowCounterOutline = dialog.ShowCounterOutline;

        if (!string.IsNullOrWhiteSpace(currentCounterFilePath) &&
            !currentCounterFilePath.StartsWith(appSettings.CountersDirectory, StringComparison.OrdinalIgnoreCase) &&
            File.Exists(currentCounterFilePath))
        {
            appSettings.LastCounterFilePath = currentCounterFilePath;
            appSettings.LastCounterName = currentProfile.Name;
        }

        appSettings.Save();
        audioPlayer.UpdateVolume(appSettings.SoundVolumePercent / 100f);
        ApplySettingsToUi();
        RegisterConfiguredHotkeys();
    }

    private void ShowAboutDialog()
    {
        hotkeyManager.Clear();

        using var dialog = new AboutForm();
        dialog.ShowDialog(this);

        RegisterConfiguredHotkeys();
    }

    private static void OpenGithubPage()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://github.com/Madusanka2013-hub/deathcounter",
            UseShellExecute = true,
        });
    }

    private void IncreaseCounter()
    {
        if (!HasLoadedCounter())
        {
            return;
        }

        BeginInvoke(() =>
        {
            currentProfile.CounterValue++;
            SaveAndRefreshCounter(true);
            audioPlayer.PlayRandomSound(appSettings.Mp3Directory, appSettings.SoundVolumePercent / 100f);
        });
    }

    private void DecreaseCounter()
    {
        if (!HasLoadedCounter() || currentProfile.CounterValue == 0)
        {
            return;
        }

        BeginInvoke(() =>
        {
            if (currentProfile.CounterValue == 0)
            {
                return;
            }

            currentProfile.CounterValue--;
            SaveAndRefreshCounter(true);
        });
    }

    private void ResetCounter()
    {
        if (!HasLoadedCounter())
        {
            return;
        }

        BeginInvoke(() =>
        {
            currentProfile.CounterValue = currentProfile.StartValue;
            SaveAndRefreshCounter(true);
        });
    }

    private void StopCurrentSound()
    {
        BeginInvoke(() => audioPlayer.StopPlayback());
    }

    private bool HasLoadedCounter() => !string.IsNullOrWhiteSpace(currentCounterFilePath);

    private void SaveAndRefreshCounter(bool allowAnimation)
    {
        currentProfile.Save(currentCounterFilePath);
        RefreshCounterUi(allowAnimation);
    }

    private void RefreshCounterUi(bool allowAnimation)
    {
        ApplySettingsToUi();

        var nextText = FormatCounterValue(currentProfile.CounterValue);
        var currentValue = int.TryParse(counterDisplay.DisplayText.Trim(), out var parsed) ? parsed : currentProfile.CounterValue;
        var direction = currentProfile.CounterValue >= currentValue ? 1 : -1;

        counterDisplay.SetCounterText(nextText, allowAnimation, direction);
        currentCounterItem.Text = $"Counter: {currentProfile.Name}";
    }

    private void ApplySettingsToUi()
    {
        BackColor = appSettings.BackgroundColor;
        menuStrip.BackColor = ControlPaint.Dark(appSettings.BackgroundColor);
        menuStrip.ForeColor = Color.White;
        counterDisplay.BackColor = appSettings.BackgroundColor;
        counterDisplay.ForeColor = appSettings.CounterFillColor;
        counterDisplay.OutlineColor = appSettings.CounterOutlineColor;
        counterDisplay.CounterFontFamily = appSettings.CounterFontFamily;
        counterDisplay.CounterFontSize = appSettings.CounterFontSize;
        counterDisplay.CounterFontBold = appSettings.CounterFontBold;
        counterDisplay.EnableCounterAnimation = appSettings.EnableCounterAnimation;
        counterDisplay.ShowOutline = appSettings.ShowCounterOutline;
        counterDisplay.Invalidate();
    }

    private void RegisterConfiguredHotkeys()
    {
        hotkeyManager.Configure(
            appSettings.IncreaseHotkey,
            appSettings.DecreaseHotkey,
            appSettings.ResetHotkey,
            appSettings.StopSoundHotkey,
            IncreaseCounter,
            DecreaseCounter,
            ResetCounter,
            StopCurrentSound);
    }

    private void BeginWindowDrag(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        NativeMethods.ReleaseCapture();
        NativeMethods.SendMessage(Handle, NativeMethods.WmNclbuttondown, NativeMethods.HtCaption, 0);
    }

    private string FormatCounterValue(int value) =>
        appSettings.UseThreeDigitCounterFormat ? value.ToString("000") : value.ToString();

    private string ResolveLastCounterFilePath()
    {
        if (!string.IsNullOrWhiteSpace(appSettings.LastCounterFilePath) && File.Exists(appSettings.LastCounterFilePath))
        {
            return appSettings.LastCounterFilePath;
        }

        if (!string.IsNullOrWhiteSpace(appSettings.LastCounterName))
        {
            var fallbackPath = CounterFileService.BuildCounterFilePath(appSettings.CountersDirectory, appSettings.LastCounterName);
            if (File.Exists(fallbackPath))
            {
                appSettings.LastCounterFilePath = fallbackPath;
                appSettings.Save();
                return fallbackPath;
            }
        }

        return string.Empty;
    }
}
