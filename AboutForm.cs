using System.Diagnostics;

namespace DeathCounter;

internal sealed class AboutForm : Form
{
    public AboutForm()
    {
        Icon = AppIconLoader.LoadApplicationIcon();
        BackColor = Color.FromArgb(238, 245, 240);

        var root = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(18),
            BackColor = BackColor,
        };

        var shell = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
        };

        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 162,
            BackColor = Color.FromArgb(22, 92, 58),
        };

        var badge = new Label
        {
            AutoSize = true,
            BackColor = Color.FromArgb(48, 255, 255, 255),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold, GraphicsUnit.Point),
            Location = new Point(28, 24),
            Padding = new Padding(10, 4, 10, 4),
            Text = "ABOUT / COMMUNITY PROJECT",
        };

        var title = new Label
        {
            AutoSize = true,
            BackColor = Color.Transparent,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 26, FontStyle.Bold, GraphicsUnit.Point),
            Location = new Point(28, 62),
            Text = "DeathCounter",
        };

        var subtitle = new Label
        {
            AutoSize = false,
            BackColor = Color.Transparent,
            ForeColor = Color.FromArgb(229, 244, 233),
            Font = new Font("Segoe UI", 11, FontStyle.Regular, GraphicsUnit.Point),
            Location = new Point(30, 110),
            Size = new Size(670, 26),
            Text = "Ein Stream-Tool für Runs, Challenges und alles, was sichtbar gezählt werden soll.",
        };

        header.Controls.Add(badge);
        header.Controls.Add(title);
        header.Controls.Add(subtitle);

        var content = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(22),
        };

        var ideaCard = CreateCard(new Point(0, 0), new Size(684, 170));
        ideaCard.Controls.Add(CreateTitleLabel("Wie dieses Projekt entstanden ist", new Point(18, 16)));
        ideaCard.Controls.Add(CreateTextLabel(
            "Dieser DeathCounter ist aus einer Idee entstanden, die direkt aus dem Streaming-Umfeld kam. Die Absicht dahinter war ein Tool, das im Stream sauber wirkt, schnell verständlich ist und sich ohne großen Aufwand an den eigenen Stil anpassen lässt.",
            new Point(18, 52),
            new Size(648, 58)));
        ideaCard.Controls.Add(CreateInlineTextWithLink(
            "Ein ganz besonderer Dank geht an",
            "Casjopaja_",
            "https://www.twitch.tv/casjopaja_",
            new Point(18, 118),
            648));
        ideaCard.Controls.Add(CreateTextLabel(
            "für die Idee, die Vorschläge und die Impulse hinter diesem Counter. Ganz viel Liebe geht an ihn raus.",
            new Point(18, 140),
            new Size(648, 22)));

        var creatorCard = CreateCard(new Point(0, 186), new Size(332, 188));
        creatorCard.Controls.Add(CreateTitleLabel("Creator", new Point(18, 16)));
        creatorCard.Controls.Add(CreateInlineTextWithLink(
            "Konzept, Umsetzung und Entwicklung",
            "Shinkaiyo",
            "https://www.twitch.tv/shinkaiyo",
            new Point(18, 54),
            296));
        creatorCard.Controls.Add(CreateMetaLabel("Copyright 2026", new Point(18, 92)));

        var projectCard = CreateCard(new Point(352, 186), new Size(332, 188));
        projectCard.Controls.Add(CreateTitleLabel("Wofür der Counter gedacht ist", new Point(18, 16)));
        projectCard.Controls.Add(CreateBulletLabel("Für OBS und Chroma Key.", new Point(18, 56), new Size(296, 24)));
        projectCard.Controls.Add(CreateBulletLabel("Für schlichte Counter mit Charakter.", new Point(18, 92), new Size(296, 24)));
        projectCard.Controls.Add(CreateBulletLabel("Für schnelle Bedienung und klare Optik.", new Point(18, 128), new Size(296, 24)));

        var footer = new Panel
        {
            Location = new Point(0, 390),
            Size = new Size(684, 84),
            BackColor = Color.FromArgb(243, 248, 244),
        };
        footer.Controls.Add(CreateTextLabel(
            "Danke fürs Nutzen, Testen und Weitertragen dieses kleinen Projekts.",
            new Point(18, 16),
            new Size(648, 22)));
        footer.Controls.Add(new Label
        {
            AutoSize = true,
            BackColor = Color.Transparent,
            ForeColor = Color.FromArgb(23, 94, 59),
            Font = new Font("Segoe UI", 10, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point),
            Location = new Point(18, 46),
            Text = "PS: Support ist kein Mord.",
        });

        var closeButton = new Button
        {
            DialogResult = DialogResult.OK,
            Text = "Schließen",
            Size = new Size(118, 38),
            Location = new Point(0, 492),
            BackColor = Color.FromArgb(22, 92, 58),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
        };
        closeButton.FlatAppearance.BorderSize = 0;

        content.Controls.Add(ideaCard);
        content.Controls.Add(creatorCard);
        content.Controls.Add(projectCard);
        content.Controls.Add(footer);
        content.Controls.Add(closeButton);

        shell.Controls.Add(content);
        shell.Controls.Add(header);
        root.Controls.Add(shell);

        AcceptButton = closeButton;
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(738, 690);
        Controls.Add(root);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "About";
    }

    private static Panel CreateCard(Point location, Size size)
    {
        return new Panel
        {
            Location = location,
            Size = size,
            BackColor = Color.FromArgb(251, 252, 251),
            BorderStyle = BorderStyle.FixedSingle,
        };
    }

    private static Label CreateTitleLabel(string text, Point location)
    {
        return new Label
        {
            AutoSize = true,
            BackColor = Color.Transparent,
            ForeColor = Color.FromArgb(18, 79, 51),
            Font = new Font("Segoe UI", 13, FontStyle.Bold, GraphicsUnit.Point),
            Location = location,
            Text = text,
        };
    }

    private static Label CreateTextLabel(string text, Point location, Size size)
    {
        return new Label
        {
            AutoSize = false,
            BackColor = Color.Transparent,
            ForeColor = Color.FromArgb(47, 55, 50),
            Font = new Font("Segoe UI", 10, FontStyle.Regular, GraphicsUnit.Point),
            Location = location,
            Size = size,
            Text = text,
        };
    }

    private static Label CreateMetaLabel(string text, Point location)
    {
        return new Label
        {
            AutoSize = true,
            BackColor = Color.Transparent,
            ForeColor = Color.FromArgb(23, 94, 59),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold, GraphicsUnit.Point),
            Location = location,
            Text = text,
        };
    }

    private static Label CreateBulletLabel(string text, Point location, Size size)
    {
        return new Label
        {
            AutoSize = false,
            BackColor = Color.Transparent,
            ForeColor = Color.FromArgb(47, 55, 50),
            Font = new Font("Segoe UI", 10, FontStyle.Regular, GraphicsUnit.Point),
            Location = location,
            Size = size,
            Text = $"• {text}",
        };
    }

    private static LinkLabel CreateLinkLabel(string text, string url, Point location)
    {
        var linkLabel = new LinkLabel
        {
            AutoSize = true,
            BackColor = Color.Transparent,
            ForeColor = Color.FromArgb(18, 109, 191),
            Font = new Font("Segoe UI", 10, FontStyle.Bold, GraphicsUnit.Point),
            LinkColor = Color.FromArgb(18, 109, 191),
            ActiveLinkColor = Color.FromArgb(180, 58, 58),
            VisitedLinkColor = Color.FromArgb(111, 74, 173),
            Location = location,
            Text = text,
        };

        linkLabel.LinkClicked += (_, _) =>
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
            });
        };

        return linkLabel;
    }

    private static FlowLayoutPanel CreateInlineTextWithLink(string text, string linkText, string url, Point location, int maxWidth)
    {
        var panel = new FlowLayoutPanel
        {
            AutoSize = true,
            WrapContents = true,
            MaximumSize = new Size(maxWidth, 0),
            Location = location,
            Margin = new Padding(0),
            Padding = new Padding(0),
            BackColor = Color.Transparent,
        };

        var textLabel = new Label
        {
            AutoSize = true,
            BackColor = Color.Transparent,
            ForeColor = Color.FromArgb(47, 55, 50),
            Font = new Font("Segoe UI", 10, FontStyle.Regular, GraphicsUnit.Point),
            Margin = new Padding(0, 0, 4, 0),
            Text = text,
        };

        var linkLabel = CreateLinkLabel(linkText, url, Point.Empty);
        linkLabel.Margin = new Padding(0);

        panel.Controls.Add(textLabel);
        panel.Controls.Add(linkLabel);
        return panel;
    }
}
