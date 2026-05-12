using System.Drawing.Text;

namespace DeathCounter;

internal sealed class SettingsForm : Form
{
    private readonly GlobalInputHook captureHook;
    private readonly TextBox increaseTextBox;
    private readonly TextBox decreaseTextBox;
    private readonly TextBox resetTextBox;
    private readonly TextBox stopSoundTextBox;
    private readonly TextBox countersDirectoryTextBox;
    private readonly TextBox mp3DirectoryTextBox;
    private readonly Panel backgroundColorPreview;
    private readonly Panel fillColorPreview;
    private readonly Panel outlineColorPreview;
    private readonly TrackBar volumeTrackBar;
    private readonly Label volumeValueLabel;
    private readonly ComboBox counterFormatComboBox;
    private readonly ComboBox fontFamilyComboBox;
    private readonly NumericUpDown fontSizeNumericUpDown;
    private readonly CheckBox enableAnimationCheckBox;
    private readonly CheckBox boldFontCheckBox;
    private readonly CheckBox showOutlineCheckBox;
    private readonly Button increaseEditButton;
    private readonly Button decreaseEditButton;
    private readonly Button resetEditButton;
    private readonly Button stopSoundEditButton;
    private HotkeyTarget activeTarget = HotkeyTarget.None;

    public SettingsForm(AppSettings currentSettings)
    {
        KeyPreview = true;

        IncreaseHotkey = currentSettings.IncreaseHotkey.Clone();
        DecreaseHotkey = currentSettings.DecreaseHotkey.Clone();
        ResetHotkey = currentSettings.ResetHotkey.Clone();
        StopSoundHotkey = currentSettings.StopSoundHotkey.Clone();
        BackgroundColor = currentSettings.BackgroundColor;
        CountersDirectory = currentSettings.CountersDirectory;
        Mp3Directory = currentSettings.Mp3Directory;
        SoundVolumePercent = currentSettings.SoundVolumePercent;
        UseThreeDigitCounterFormat = currentSettings.UseThreeDigitCounterFormat;
        CounterFontFamily = currentSettings.CounterFontFamily;
        CounterFontSize = currentSettings.CounterFontSize;
        EnableCounterAnimation = currentSettings.EnableCounterAnimation;
        CounterFillColor = currentSettings.CounterFillColor;
        CounterOutlineColor = currentSettings.CounterOutlineColor;
        CounterFontBold = currentSettings.CounterFontBold;
        ShowCounterOutline = currentSettings.ShowCounterOutline;

        captureHook = new GlobalInputHook(CaptureHotkeyPressed);

        var infoLabel = new Label
        {
            AutoSize = true,
            Text = "Auf Edit klicken und danach Taste, Maustaste oder Kombination drücken.",
        };

        increaseTextBox = CreateHotkeyTextBox(IncreaseHotkey);
        decreaseTextBox = CreateHotkeyTextBox(DecreaseHotkey);
        resetTextBox = CreateHotkeyTextBox(ResetHotkey);
        stopSoundTextBox = CreateHotkeyTextBox(StopSoundHotkey);

        increaseEditButton = CreateEditButton(HotkeyTarget.Increase);
        decreaseEditButton = CreateEditButton(HotkeyTarget.Decrease);
        resetEditButton = CreateEditButton(HotkeyTarget.Reset);
        stopSoundEditButton = CreateEditButton(HotkeyTarget.StopSound);

        countersDirectoryTextBox = CreatePathTextBox(CountersDirectory);
        mp3DirectoryTextBox = CreatePathTextBox(Mp3Directory);

        backgroundColorPreview = CreateColorPreview(BackgroundColor);
        fillColorPreview = CreateColorPreview(CounterFillColor);
        outlineColorPreview = CreateColorPreview(CounterOutlineColor);

        var backgroundColorButton = CreateColorButton("Farbe wählen", () => SelectColor(backgroundColorPreview, color => BackgroundColor = color));
        var fillColorButton = CreateColorButton("Farbe wählen", () => SelectColor(fillColorPreview, color => CounterFillColor = color));
        var outlineColorButton = CreateColorButton("Farbe wählen", () => SelectColor(outlineColorPreview, color => CounterOutlineColor = color));

        var countersBrowseButton = new Button
        {
            AutoSize = true,
            Text = "Ordner wählen",
        };
        countersBrowseButton.Click += (_, _) => SelectDirectory(countersDirectoryTextBox);

        var mp3BrowseButton = new Button
        {
            AutoSize = true,
            Text = "Sound-Ordner wählen",
        };
        mp3BrowseButton.Click += (_, _) => SelectDirectory(mp3DirectoryTextBox);

        counterFormatComboBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 220,
        };
        counterFormatComboBox.Items.Add("1 2 3 4 5");
        counterFormatComboBox.Items.Add("001 002 003 004 005");
        counterFormatComboBox.SelectedIndex = UseThreeDigitCounterFormat ? 1 : 0;
        counterFormatComboBox.SelectedIndexChanged += (_, _) => UseThreeDigitCounterFormat = counterFormatComboBox.SelectedIndex == 1;

        fontFamilyComboBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 280,
        };
        foreach (var fontName in GetInstalledFonts())
        {
            fontFamilyComboBox.Items.Add(fontName);
        }

        var selectedFontIndex = fontFamilyComboBox.FindStringExact(CounterFontFamily);
        fontFamilyComboBox.SelectedIndex = selectedFontIndex >= 0 ? selectedFontIndex : fontFamilyComboBox.FindStringExact("Segoe UI");
        if (fontFamilyComboBox.SelectedIndex < 0 && fontFamilyComboBox.Items.Count > 0)
        {
            fontFamilyComboBox.SelectedIndex = 0;
        }

        if (fontFamilyComboBox.SelectedItem is string selectedFont)
        {
            CounterFontFamily = selectedFont;
        }

        fontFamilyComboBox.SelectedIndexChanged += (_, _) =>
        {
            if (fontFamilyComboBox.SelectedItem is string fontName)
            {
                CounterFontFamily = fontName;
            }
        };

        fontSizeNumericUpDown = new NumericUpDown
        {
            Minimum = 12,
            Maximum = 300,
            DecimalPlaces = 0,
            Value = (decimal)CounterFontSize,
            Width = 100,
        };
        fontSizeNumericUpDown.ValueChanged += (_, _) => CounterFontSize = (float)fontSizeNumericUpDown.Value;

        enableAnimationCheckBox = new CheckBox
        {
            AutoSize = true,
            Text = "Zahlenwechsel animieren",
            Checked = EnableCounterAnimation,
        };
        enableAnimationCheckBox.CheckedChanged += (_, _) => EnableCounterAnimation = enableAnimationCheckBox.Checked;

        boldFontCheckBox = new CheckBox
        {
            AutoSize = true,
            Text = "Fett",
            Checked = CounterFontBold,
        };
        boldFontCheckBox.CheckedChanged += (_, _) => CounterFontBold = boldFontCheckBox.Checked;

        showOutlineCheckBox = new CheckBox
        {
            AutoSize = true,
            Text = "Rahmen anzeigen",
            Checked = ShowCounterOutline,
        };
        showOutlineCheckBox.CheckedChanged += (_, _) => ShowCounterOutline = showOutlineCheckBox.Checked;

        volumeValueLabel = new Label
        {
            AutoSize = true,
            Text = $"{SoundVolumePercent}%",
            Margin = new Padding(8, 8, 0, 0),
        };

        volumeTrackBar = new TrackBar
        {
            Minimum = 0,
            Maximum = 100,
            TickFrequency = 10,
            Value = SoundVolumePercent,
            Width = 220,
        };
        volumeTrackBar.ValueChanged += (_, _) =>
        {
            SoundVolumePercent = volumeTrackBar.Value;
            volumeValueLabel.Text = $"{SoundVolumePercent}%";
        };

        var buttonsPanel = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Bottom,
            Padding = new Padding(0, 12, 0, 0),
        };

        var saveButton = new Button
        {
            AutoSize = true,
            DialogResult = DialogResult.OK,
            Text = "Speichern",
        };
        saveButton.Click += SaveButton_Click;

        var cancelButton = new Button
        {
            AutoSize = true,
            DialogResult = DialogResult.Cancel,
            Text = "Abbrechen",
        };

        buttonsPanel.Controls.Add(saveButton);
        buttonsPanel.Controls.Add(cancelButton);

        var backgroundColorPanel = CreatePathRow(backgroundColorPreview, backgroundColorButton);
        var fillColorPanel = CreatePathRow(fillColorPreview, fillColorButton);
        var outlineColorPanel = CreatePathRow(outlineColorPreview, outlineColorButton);
        var countersPanel = CreatePathRow(countersDirectoryTextBox, countersBrowseButton);
        var mp3Panel = CreatePathRow(mp3DirectoryTextBox, mp3BrowseButton);
        var counterFormatPanel = CreateSingleControlRow(counterFormatComboBox);
        var fontPanel = CreateSingleControlRow(fontFamilyComboBox);
        var fontSizePanel = CreateSingleControlRow(fontSizeNumericUpDown);
        var animationPanel = CreateSingleControlRow(enableAnimationCheckBox);
        var boldPanel = CreateSingleControlRow(boldFontCheckBox);
        var outlineTogglePanel = CreateSingleControlRow(showOutlineCheckBox);
        var volumePanel = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
        };
        volumePanel.Controls.Add(volumeTrackBar);
        volumePanel.Controls.Add(volumeValueLabel);

        var increasePanel = CreateHotkeyRow(increaseTextBox, increaseEditButton);
        var decreasePanel = CreateHotkeyRow(decreaseTextBox, decreaseEditButton);
        var resetPanel = CreateHotkeyRow(resetTextBox, resetEditButton);
        var stopSoundPanel = CreateHotkeyRow(stopSoundTextBox, stopSoundEditButton);

        var layout = new TableLayoutPanel
        {
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            AutoSize = true,
            Padding = new Padding(12),
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        layout.Controls.Add(infoLabel, 0, 0);
        layout.SetColumnSpan(infoLabel, 2);
        layout.Controls.Add(new Label { AutoSize = true, Text = "Hochzählen:", Anchor = AnchorStyles.Left }, 0, 1);
        layout.Controls.Add(increasePanel, 1, 1);
        layout.Controls.Add(new Label { AutoSize = true, Text = "Runterzählen:", Anchor = AnchorStyles.Left }, 0, 2);
        layout.Controls.Add(decreasePanel, 1, 2);
        layout.Controls.Add(new Label { AutoSize = true, Text = "Reset:", Anchor = AnchorStyles.Left }, 0, 3);
        layout.Controls.Add(resetPanel, 1, 3);
        layout.Controls.Add(new Label { AutoSize = true, Text = "Sound stoppen:", Anchor = AnchorStyles.Left }, 0, 4);
        layout.Controls.Add(stopSoundPanel, 1, 4);
        layout.Controls.Add(new Label { AutoSize = true, Text = "Chroma Key:", Anchor = AnchorStyles.Left }, 0, 5);
        layout.Controls.Add(backgroundColorPanel, 1, 5);
        layout.Controls.Add(new Label { AutoSize = true, Text = "Zahlenformat:", Anchor = AnchorStyles.Left }, 0, 6);
        layout.Controls.Add(counterFormatPanel, 1, 6);
        layout.Controls.Add(new Label { AutoSize = true, Text = "Animation:", Anchor = AnchorStyles.Left }, 0, 7);
        layout.Controls.Add(animationPanel, 1, 7);
        layout.Controls.Add(new Label { AutoSize = true, Text = "Schriftart:", Anchor = AnchorStyles.Left }, 0, 8);
        layout.Controls.Add(fontPanel, 1, 8);
        layout.Controls.Add(new Label { AutoSize = true, Text = "Schriftgröße:", Anchor = AnchorStyles.Left }, 0, 9);
        layout.Controls.Add(fontSizePanel, 1, 9);
        layout.Controls.Add(new Label { AutoSize = true, Text = "Schriftfarbe:", Anchor = AnchorStyles.Left }, 0, 10);
        layout.Controls.Add(fillColorPanel, 1, 10);
        layout.Controls.Add(new Label { AutoSize = true, Text = "Schriftrahmen:", Anchor = AnchorStyles.Left }, 0, 11);
        layout.Controls.Add(outlineColorPanel, 1, 11);
        layout.Controls.Add(new Label { AutoSize = true, Text = "Rahmen:", Anchor = AnchorStyles.Left }, 0, 12);
        layout.Controls.Add(outlineTogglePanel, 1, 12);
        layout.Controls.Add(new Label { AutoSize = true, Text = "Fettschrift:", Anchor = AnchorStyles.Left }, 0, 13);
        layout.Controls.Add(boldPanel, 1, 13);
        layout.Controls.Add(new Label { AutoSize = true, Text = "Counter-Ordner:", Anchor = AnchorStyles.Left }, 0, 14);
        layout.Controls.Add(countersPanel, 1, 14);
        layout.Controls.Add(new Label { AutoSize = true, Text = "Sound-Ordner:", Anchor = AnchorStyles.Left }, 0, 15);
        layout.Controls.Add(mp3Panel, 1, 15);
        layout.Controls.Add(new Label { AutoSize = true, Text = "Lautstärke:", Anchor = AnchorStyles.Left }, 0, 16);
        layout.Controls.Add(volumePanel, 1, 16);
        layout.Controls.Add(buttonsPanel, 0, 17);
        layout.SetColumnSpan(buttonsPanel, 2);

        AcceptButton = saveButton;
        CancelButton = cancelButton;
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(820, 760);
        Controls.Add(layout);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Einstellungen";
    }

    public HotkeyBinding IncreaseHotkey { get; private set; }
    public HotkeyBinding DecreaseHotkey { get; private set; }
    public HotkeyBinding ResetHotkey { get; private set; }
    public HotkeyBinding StopSoundHotkey { get; private set; }
    public Color BackgroundColor { get; private set; }
    public string CountersDirectory { get; private set; }
    public string Mp3Directory { get; private set; }
    public int SoundVolumePercent { get; private set; }
    public bool UseThreeDigitCounterFormat { get; private set; }
    public string CounterFontFamily { get; private set; }
    public float CounterFontSize { get; private set; }
    public bool EnableCounterAnimation { get; private set; }
    public Color CounterFillColor { get; private set; }
    public Color CounterOutlineColor { get; private set; }
    public bool CounterFontBold { get; private set; }
    public bool ShowCounterOutline { get; private set; }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            captureHook.Dispose();
        }

        base.Dispose(disposing);
    }

    private static TextBox CreateHotkeyTextBox(HotkeyBinding hotkey) =>
        new()
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            Text = hotkey.ToString(),
        };

    private static TextBox CreatePathTextBox(string value) =>
        new()
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            Text = value,
            Width = 340,
        };

    private static Panel CreateColorPreview(Color color) =>
        new()
        {
            Width = 48,
            Height = 24,
            BackColor = color,
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(0, 6, 8, 0),
        };

    private static Button CreateColorButton(string text, Action onClick)
    {
        var button = new Button
        {
            AutoSize = true,
            Text = text,
        };
        button.Click += (_, _) => onClick();
        return button;
    }

    private Button CreateEditButton(HotkeyTarget target)
    {
        var button = new Button
        {
            AutoSize = true,
            Text = "Edit",
        };

        button.Click += (_, _) => SetActiveTarget(target);
        return button;
    }

    private static FlowLayoutPanel CreateHotkeyRow(Control leftControl, Control rightControl)
    {
        var panel = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
        };

        if (leftControl is TextBox textBox)
        {
            textBox.Width = 220;
        }

        panel.Controls.Add(leftControl);
        panel.Controls.Add(rightControl);
        return panel;
    }

    private static FlowLayoutPanel CreatePathRow(Control leftControl, Control rightControl)
    {
        var panel = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
        };

        panel.Controls.Add(leftControl);
        panel.Controls.Add(rightControl);
        return panel;
    }

    private static FlowLayoutPanel CreateSingleControlRow(Control control)
    {
        var panel = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
        };

        panel.Controls.Add(control);
        return panel;
    }

    private void SetActiveTarget(HotkeyTarget target)
    {
        activeTarget = target;

        increaseEditButton.Text = target == HotkeyTarget.Increase ? "Taste drücken..." : "Edit";
        decreaseEditButton.Text = target == HotkeyTarget.Decrease ? "Taste drücken..." : "Edit";
        resetEditButton.Text = target == HotkeyTarget.Reset ? "Taste drücken..." : "Edit";
        stopSoundEditButton.Text = target == HotkeyTarget.StopSound ? "Taste drücken..." : "Edit";
    }

    private void CaptureHotkeyPressed(HotkeyBinding binding)
    {
        if (activeTarget == HotkeyTarget.None)
        {
            return;
        }

        BeginInvoke(() =>
        {
            switch (activeTarget)
            {
                case HotkeyTarget.Increase:
                    IncreaseHotkey = binding;
                    increaseTextBox.Text = binding.ToString();
                    break;
                case HotkeyTarget.Decrease:
                    DecreaseHotkey = binding;
                    decreaseTextBox.Text = binding.ToString();
                    break;
                case HotkeyTarget.Reset:
                    ResetHotkey = binding;
                    resetTextBox.Text = binding.ToString();
                    break;
                case HotkeyTarget.StopSound:
                    StopSoundHotkey = binding;
                    stopSoundTextBox.Text = binding.ToString();
                    break;
            }

            SetActiveTarget(HotkeyTarget.None);
        });
    }

    private void SelectColor(Panel previewPanel, Action<Color> setter)
    {
        using var dialog = new ColorDialog
        {
            FullOpen = true,
            Color = previewPanel.BackColor,
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        previewPanel.BackColor = dialog.Color;
        setter(dialog.Color);
    }

    private void SelectDirectory(TextBox targetTextBox)
    {
        using var dialog = new FolderBrowserDialog();
        dialog.InitialDirectory = Directory.Exists(targetTextBox.Text)
            ? targetTextBox.Text
            : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        targetTextBox.Text = dialog.SelectedPath;

        if (ReferenceEquals(targetTextBox, countersDirectoryTextBox))
        {
            CountersDirectory = dialog.SelectedPath;
        }
        else
        {
            Mp3Directory = dialog.SelectedPath;
        }
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        CountersDirectory = countersDirectoryTextBox.Text.Trim();
        Mp3Directory = mp3DirectoryTextBox.Text.Trim();
        CounterFontSize = (float)fontSizeNumericUpDown.Value;

        if (activeTarget != HotkeyTarget.None)
        {
            ShowValidationMessage("Beende zuerst die laufende Hotkey-Eingabe.");
            return;
        }

        if (HasDuplicateHotkeys())
        {
            ShowValidationMessage("Bitte verwende vier unterschiedliche Hotkeys.");
            return;
        }

        if (IncreaseHotkey.IsEmpty || DecreaseHotkey.IsEmpty || ResetHotkey.IsEmpty || StopSoundHotkey.IsEmpty)
        {
            ShowValidationMessage("Alle Aktionen brauchen eine gültige Taste.");
            return;
        }

        if (string.IsNullOrWhiteSpace(CountersDirectory))
        {
            ShowValidationMessage("Bitte einen Counter-Ordner auswählen.");
            return;
        }

        if (string.IsNullOrWhiteSpace(CounterFontFamily))
        {
            ShowValidationMessage("Bitte eine Schriftart auswählen.");
            return;
        }

        if (!string.IsNullOrWhiteSpace(Mp3Directory) && !Directory.Exists(Mp3Directory))
        {
            ShowValidationMessage("Der Sound-Ordner existiert nicht.");
            return;
        }

        Directory.CreateDirectory(CountersDirectory);
    }

    private void ShowValidationMessage(string message)
    {
        MessageBox.Show(
            message,
            "Ungültige Eingabe",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);

        DialogResult = DialogResult.None;
    }

    private bool HasDuplicateHotkeys() =>
        IncreaseHotkey.Matches(DecreaseHotkey) ||
        IncreaseHotkey.Matches(ResetHotkey) ||
        IncreaseHotkey.Matches(StopSoundHotkey) ||
        DecreaseHotkey.Matches(ResetHotkey) ||
        DecreaseHotkey.Matches(StopSoundHotkey) ||
        ResetHotkey.Matches(StopSoundHotkey);

    private static IEnumerable<string> GetInstalledFonts()
    {
        using var fontCollection = new InstalledFontCollection();
        return fontCollection.Families
            .Select(family => family.Name)
            .OrderBy(name => name, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    private enum HotkeyTarget
    {
        None,
        Increase,
        Decrease,
        Reset,
        StopSound,
    }
}
