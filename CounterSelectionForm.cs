namespace DeathCounter;

internal sealed class CounterSelectionForm : Form
{
    private readonly AppSettings appSettings;
    private readonly ListBox countersListBox;
    private readonly TextBox newCounterNameTextBox;

    public CounterSelectionForm(AppSettings appSettings)
    {
        this.appSettings = appSettings;

        var infoLabel = new Label
        {
            AutoSize = true,
            Text = "Vorhandenen Counter laden oder einen neuen Namen vergeben.",
        };

        countersListBox = new ListBox
        {
            Dock = DockStyle.Fill,
            Height = 180,
        };
        countersListBox.DoubleClick += (_, _) => LoadSelectedCounter();

        newCounterNameTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            PlaceholderText = "Neuer Counter-Name",
        };

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

        var deleteButton = new Button
        {
            AutoSize = true,
            Text = "Löschen",
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
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        layout.Controls.Add(infoLabel, 0, 0);
        layout.Controls.Add(countersListBox, 0, 1);
        layout.Controls.Add(newCounterNameTextBox, 0, 2);
        layout.Controls.Add(buttonsPanel, 0, 3);

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
                "Bitte zuerst einen Counter aus der Liste auswählen.",
                "Kein Counter gewählt",
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
        var counterName = newCounterNameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(counterName))
        {
            MessageBox.Show(
                "Bitte einen Namen für den neuen Counter eingeben.",
                "Name fehlt",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        var filePath = CounterFileService.BuildCounterFilePath(appSettings.CountersDirectory, counterName);
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
                Name = counterName,
                CounterValue = 0,
            };

            profile.Save(filePath);
        }

        SelectedCounterFilePath = filePath;
        DialogResult = DialogResult.OK;
        Close();
    }

    private void DeleteSelectedCounter()
    {
        if (countersListBox.SelectedItem is not CounterListItem item)
        {
            MessageBox.Show(
                "Bitte zuerst einen Counter aus der Liste auswählen.",
                "Kein Counter gewählt",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        var result = MessageBox.Show(
            $"Soll der Counter '{item.DisplayName}' wirklich gelöscht werden?",
            "Counter löschen",
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
            appSettings.Save();
        }

        RefreshCounterList();
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
