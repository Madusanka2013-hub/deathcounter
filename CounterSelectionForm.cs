namespace DeathCounter;

internal sealed class CounterSelectionForm : Form
{
    private readonly AppSettings appSettings;
    private readonly ListBox countersListBox;

    public CounterSelectionForm(AppSettings appSettings)
    {
        this.appSettings = appSettings;

        var infoLabel = new Label
        {
            AutoSize = true,
            Text = "Vorhandenen Counter laden, bearbeiten oder einen neuen Counter anlegen.",
        };

        countersListBox = new ListBox
        {
            Dock = DockStyle.Fill,
            Height = 180,
        };
        countersListBox.DoubleClick += (_, _) => LoadSelectedCounter();

        var loadButton = new Button
        {
            AutoSize = true,
            Text = "Laden",
        };
        loadButton.Click += (_, _) => LoadSelectedCounter();

        var newButton = new Button
        {
            AutoSize = true,
            Text = "Neu anlegen",
        };
        newButton.Click += (_, _) => CreateNewCounter();

        var editButton = new Button
        {
            AutoSize = true,
            Text = "Bearbeiten",
        };
        editButton.Click += (_, _) => EditSelectedCounter();

        var deleteButton = new Button
        {
            AutoSize = true,
            Text = "Loeschen",
        };
        deleteButton.Click += (_, _) => DeleteSelectedCounter();

        var cancelButton = new Button
        {
            AutoSize = true,
            DialogResult = DialogResult.Cancel,
            Text = "Abbrechen",
        };

        var buttonsPanel = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Bottom,
            Padding = new Padding(0, 12, 0, 0),
        };
        buttonsPanel.Controls.Add(cancelButton);
        buttonsPanel.Controls.Add(deleteButton);
        buttonsPanel.Controls.Add(editButton);
        buttonsPanel.Controls.Add(newButton);
        buttonsPanel.Controls.Add(loadButton);

        var layout = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(infoLabel, 0, 0);
        layout.Controls.Add(countersListBox, 0, 1);
        layout.Controls.Add(buttonsPanel, 0, 2);

        AcceptButton = loadButton;
        CancelButton = cancelButton;
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(520, 360);
        Controls.Add(layout);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Counter laden";

        RefreshCounterList();
    }

    public string SelectedCounterFilePath { get; private set; } = string.Empty;

    private void RefreshCounterList()
    {
        countersListBox.Items.Clear();

        foreach (var filePath in CounterFileService.GetCounterFiles(appSettings.CountersDirectory))
        {
            countersListBox.Items.Add(new CounterListItem(filePath));
        }

        if (countersListBox.Items.Count > 0)
        {
            countersListBox.SelectedIndex = 0;
        }
    }

    private void LoadSelectedCounter()
    {
        if (countersListBox.SelectedItem is not CounterListItem item)
        {
            MessageBox.Show(
                "Bitte zuerst einen Counter aus der Liste auswaehlen.",
                "Kein Counter gewaehlt",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        SelectedCounterFilePath = item.FilePath;
        DialogResult = DialogResult.OK;
        Close();
    }

    private void CreateNewCounter()
    {
        using var dialog = new CounterEditorForm("Neuen Counter anlegen", string.Empty, 0, 0);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var filePath = CounterFileService.BuildCounterFilePath(appSettings.CountersDirectory, dialog.CounterName);
        if (File.Exists(filePath))
        {
            var result = MessageBox.Show(
                "Dieser Counter existiert bereits. Soll die vorhandene Datei geladen werden?",
                "Counter existiert bereits",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }
        }
        else
        {
            var profile = new CounterProfile
            {
                Name = dialog.CounterName,
                StartValue = dialog.StartValue,
                CounterValue = dialog.CurrentValue,
            };

            profile.Save(filePath);
        }

        SelectedCounterFilePath = filePath;
        DialogResult = DialogResult.OK;
        Close();
    }

    private void EditSelectedCounter()
    {
        if (countersListBox.SelectedItem is not CounterListItem item)
        {
            MessageBox.Show(
                "Bitte zuerst einen Counter aus der Liste auswaehlen.",
                "Kein Counter gewaehlt",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        var profile = CounterProfile.Load(item.FilePath);
        using var dialog = new CounterEditorForm(
            "Counter bearbeiten",
            profile.Name,
            profile.StartValue,
            profile.CounterValue);

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var updatedFilePath = CounterFileService.BuildCounterFilePath(appSettings.CountersDirectory, dialog.CounterName);
        var shouldRenameFile = !string.Equals(item.FilePath, updatedFilePath, StringComparison.OrdinalIgnoreCase);
        if (shouldRenameFile && File.Exists(updatedFilePath))
        {
            MessageBox.Show(
                "Ein Counter mit diesem Namen existiert bereits.",
                "Name bereits vorhanden",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        profile.Name = dialog.CounterName;
        profile.StartValue = dialog.StartValue;
        profile.CounterValue = dialog.CurrentValue;

        if (shouldRenameFile)
        {
            File.Move(item.FilePath, updatedFilePath);
        }

        profile.Save(updatedFilePath);

        if (string.Equals(appSettings.LastCounterFilePath, item.FilePath, StringComparison.OrdinalIgnoreCase))
        {
            appSettings.LastCounterFilePath = updatedFilePath;
            appSettings.LastCounterName = profile.Name;
            appSettings.Save();
        }

        RefreshCounterList();
        SelectCounter(updatedFilePath);
    }

    private void DeleteSelectedCounter()
    {
        if (countersListBox.SelectedItem is not CounterListItem item)
        {
            MessageBox.Show(
                "Bitte zuerst einen Counter aus der Liste auswaehlen.",
                "Kein Counter gewaehlt",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        var result = MessageBox.Show(
            $"Soll der Counter '{item.DisplayName}' wirklich geloescht werden?",
            "Counter loeschen",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result != DialogResult.Yes)
        {
            return;
        }

        File.Delete(item.FilePath);

        if (string.Equals(appSettings.LastCounterFilePath, item.FilePath, StringComparison.OrdinalIgnoreCase))
        {
            appSettings.LastCounterFilePath = string.Empty;
            appSettings.LastCounterName = string.Empty;
            appSettings.Save();
        }

        RefreshCounterList();
    }

    private void SelectCounter(string filePath)
    {
        for (var index = 0; index < countersListBox.Items.Count; index++)
        {
            if (countersListBox.Items[index] is CounterListItem item &&
                string.Equals(item.FilePath, filePath, StringComparison.OrdinalIgnoreCase))
            {
                countersListBox.SelectedIndex = index;
                break;
            }
        }
    }

    private sealed class CounterListItem
    {
        public CounterListItem(string filePath)
        {
            FilePath = filePath;
            DisplayName = Path.GetFileNameWithoutExtension(filePath);
        }

        public string DisplayName { get; }
        public string FilePath { get; }

        public override string ToString() => DisplayName;
    }
}
