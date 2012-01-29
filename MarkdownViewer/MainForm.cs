using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace MarkdownViewer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void Open()
        {
            TransformMarkdown();
            webBrowser.Navigate("file://" + HtmlPath);
            fileSystemWatcher.Path = Path.GetDirectoryName(path);
            fileSystemWatcher.Filter = Path.GetFileName(path);
            fileSystemWatcher.EnableRaisingEvents = true;
        }
        private void TransformMarkdown()
        {
            try
            {
                pathLabel.Text = path;
                var text = File.ReadAllText(path);
                var html = markdown.Transform(text);
                html = string.Format(
@"<html>
    <head>
        <link rel=""stylesheet"" type=""text/css"" href=""{0}"" />
        <meta http-equiv=""X-UA-Compatible"" content=""IE=9"" >
    </head>
    <body>
        {1}
    </body>
</html>", CssFileName, html);
                File.WriteAllText(HtmlPath, html);
                lastRefreshedTime = LastWriteTime;
            }
            catch (System.IO.IOException)
            {
                //sometimes we are too quick in accessing the files and it is still being used by other process
            }
        }
        private void RefreshView()
        {
            TransformMarkdown();
            webBrowser.Refresh();
        }
        private string GetPath(string extension)
        {
            var dir = Path.GetDirectoryName(path);
            var fileName = Path.GetFileNameWithoutExtension(path);
            return Path.Combine(dir, fileName + "." + extension);
        }
        private string HtmlPath
        {
            get { return GetPath("htm"); }
        }
        private DateTime LastWriteTime
        {
            get
            {
                return (new FileInfo(path)).LastWriteTime;
            }
        }
        private string CssFileName
        {
            get
            {
                var fileName = Path.GetFileNameWithoutExtension(path);
                return fileName + ".css";
            }
        }

        private string path = null;
        private MarkdownSharp.Markdown markdown = new MarkdownSharp.Markdown();
        private DateTime lastRefreshedTime = new DateTime(1900, 1, 1);

        private void changeBtn_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                path = openFileDialog.FileName;
                Open();
            }
        }
        private void fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (LastWriteTime > lastRefreshedTime)
                RefreshView();
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("FileName"))
            {
                path = ((string[])e.Data.GetData("FileNameW"))[0];
                Open();
            }
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("FileNameW"))
                e.Effect = DragDropEffects.Link;
        }
    }
}
