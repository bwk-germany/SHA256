// Manifest.cs, C# 9.0 
// ManifestCreate / ManifestCheck
// Copyright (c) 2026 Bernhard Simon Bock
// SPDX-License-Identifier: MIT


using System.Xml;
using System.Xml.Serialization;
/*
using System.Drawing;
using System.Windows.Forms;
*/


[XmlRoot("DocumentManifest")] public class DocumentManifest{
[XmlAttribute] public int ManifestVersion = 1;
[XmlAttribute] public string HashAlgorithm = "SHA-256";
[XmlElement("Document")] public List<DocumentEntry> Document = new();
}

public class DocumentEntry{
[XmlAttribute] public int Index;
[XmlAttribute] public string Name = "";
[XmlAttribute] public string Hash = "";
[XmlAttribute] public long Size;
}




public static class MessageBoxEx
{
    public static DialogResult Show(
        string text,
        string caption = "",
        int width = 700,
        int height = 300,
        MessageBoxIcon icon = MessageBoxIcon.None)
    {
        using Form form = new();

        form.Text = caption;
        form.FormBorderStyle = FormBorderStyle.FixedDialog;
        form.StartPosition = FormStartPosition.CenterScreen;
        form.MinimizeBox = false;
        form.MaximizeBox = false;
        form.ShowInTaskbar = false;
        form.ClientSize = new Size(width, height);


        // Grüner Haken
        Label check = new()
        {
            AutoSize = true,
            Text = "✔",
            Font = new Font("Segoe UI Symbol", 28, FontStyle.Bold),
            ForeColor = Color.ForestGreen,
            Left = 20,
            Top = 15
        };
PictureBox picture = new()
{
    Left = 18,
    Top = 16,
    Width = 40,
    Height = 40,
    SizeMode = PictureBoxSizeMode.StretchImage,
    Visible = false
};

        // Meldung
        TextBox box = new()
        {
            Left = 70,
            Top = 20,
            Width = width - 90,
            Height = height - 80,
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            Multiline = true,
            ScrollBars = ScrollBars.None,
            Font = new Font("Consolas", 10),
            Text = text,
            BackColor = SystemColors.Control,
            TabStop = false
        };

        // OK-Button
        Button ok = new()
        {
            Text = "OK",
            Width = 100,
            Height = 30,
            Left = (width - 100) / 2,
            Top = height - 45,
            DialogResult = DialogResult.OK
        };

switch (icon)
{
    case MessageBoxIcon.Information:
        check.Visible = true;
        picture.Visible = false;
        break;

    case MessageBoxIcon.Warning:
        check.Visible = false;
        picture.Visible = true;
        picture.Image = SystemIcons.Warning.ToBitmap();
        break;

    case MessageBoxIcon.Error:
        check.Visible = false;
        picture.Visible = true;
        picture.Image = SystemIcons.Error.ToBitmap();
        break;

    case MessageBoxIcon.Question:
        check.Visible = false;
        picture.Visible = true;
        picture.Image = SystemIcons.Question.ToBitmap();
        break;

    default:
        check.Visible = false;
        picture.Visible = false;
        break;
}

        form.Controls.Add(check);
        form.Controls.Add(picture);
        form.Controls.Add(box);
        form.Controls.Add(ok);

        form.AcceptButton = ok;
        form.CancelButton = ok;

        return form.ShowDialog();
    }
}