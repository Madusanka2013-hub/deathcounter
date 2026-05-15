namespace DeathCounter;

internal sealed class CounterEditorForm : Form
{
    private readonly TextBox counterNameTextBox;
    private readonly NumericUpDown startValueInput;
    private readonly NumericUpDown currentValueInput;

    public CounterEditorForm(string title, string counterName, int startValue, int currentValue)
    {
        var nameLabel = new Label
        {
            AutoSize = true,
            Text = "Name",
        };

        counterNameTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Text = counterName,
        };

        var startValueLabel = new Label
        {
            AutoSize = true,
            Text = "Startwert",
        };

        var currentValueLabel = new Label
        {
            AutoSize = true,
            Text = "Aktueller Stand",
        };

        currentValueInput = CreateNumericInput(currentValue);
        startValueInput = CreateNumericInput(startValue);
        startValueInput.ValueChanged += (_, _) =>
        {
            if (currentValueInput.Value < startValueInput.Value)
            {
                currentValueInput.Value = startValueInput.Value;
            }
        };

        var saveButton = new Button
        {
            AutoSize = true,
            Text = "Speichern",
        };
        saveButton.Click += (_, _) => SaveCounter();

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
        buttonsPanel.Controls.Add(saveButton);

        var layout = new TableLayoutPanel
        {
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(nameLabel, 0, 0);
        layout.Controls.Add(counterNameTextBox, 1, 0);
        layout.Controls.Add(startValueLabel, 0, 1);
        layout.Controls.Add(startValueInput, 1, 1);
        layout.Controls.Add(currentValueLabel, 0, 2);
        layout.Controls.Add(currentValueInput, 1, 2);
        layout.Controls.Add(buttonsPanel, 0, 3);
        layout.SetColumnSpan(buttonsPanel, 2);

        AcceptButton = saveButton;
        CancelButton = cancelButton;
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(420, 210);
        Controls.Add(layout);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = title;
    }

    public string CounterName { get; private set; } = string.Empty;

    public int StartValue { get; private set; }

    public int CurrentValue { get; private set; }

    private void SaveCounter()
    {
        var counterName = counterNameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(counterName))
        {
            MessageBox.Show(
                "Bitte einen Namen eingeben.",
                "Name fehlt",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        CounterName = counterName;
        StartValue = Decimal.ToInt32(startValueInput.Value);
        CurrentValue = Decimal.ToInt32(currentValueInput.Value);

        if (CurrentValue < StartValue)
        {
            MessageBox.Show(
                "Der aktuelle Stand darf nicht kleiner als der Startwert sein.",
                "Ungueltiger Stand",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    private static NumericUpDown CreateNumericInput(int value) =>
        new()
        {
            Dock = DockStyle.Left,
            Maximum = 1_000_000,
            Minimum = 0,
            ThousandsSeparator = true,
            Value = Math.Clamp(value, 0, 1_000_000),
            Width = 160,
        };
}
