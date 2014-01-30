using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.IO;
using System.Diagnostics;

namespace MoodleSyncInterface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private System.Windows.Forms.NotifyIcon _tray;
        private System.Windows.Forms.ContextMenuStrip _tray_menu;
        
        #region Notification Menu Entries
        private System.Windows.Forms.ToolStripMenuItem _tray_notification;
        private System.Windows.Forms.ToolStripMenuItem _tray_pause;
        private System.Windows.Forms.ToolStripMenuItem _tray_sync;
        private System.Windows.Forms.ToolStripSeparator _tray_separator1;
        private System.Windows.Forms.ToolStripMenuItem _tray_moodle;
        private System.Windows.Forms.ToolStripMenuItem _tray_browse;
        private System.Windows.Forms.ToolStripSeparator _tray_separator2;
        private System.Windows.Forms.ToolStripMenuItem _tray_setup;
        private System.Windows.Forms.ToolStripMenuItem _tray_log;
        private System.Windows.Forms.ToolStripMenuItem _tray_help;
        private System.Windows.Forms.ToolStripMenuItem _tray_about;
        private System.Windows.Forms.ToolStripSeparator _tray_separator3;
        private System.Windows.Forms.ToolStripMenuItem _tray_exit;
        #endregion

        private SetupWindow Setup;
        private Translator.LoggingClass Logger;

        readonly static string CONFIGDIR = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Moodle Sync\\";

        private static System.Threading.TimerCallback Sync_Callback = RunSync;
        private static System.Threading.Timer Schedule_Sync;
        
        private int New_Posts = 0;
        private int New_Material = 0;
        
        private static void RunSync(object state)                                           //Starts synchronisation and returns
        {
            ProcessStartInfo procStartInfo = new ProcessStartInfo();
            Process procExecuting = new Process();

            procStartInfo.UseShellExecute = true;
            procStartInfo.WorkingDirectory = CONFIGDIR;
            procStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            procStartInfo.FileName = "wscript.exe";
            procStartInfo.Arguments = "MoodleSyncCallback.vbs";

            procExecuting = Process.Start(procStartInfo);
            procExecuting.WaitForExit();
        }
        //----------Incomplete--------//
        private void Tray_Icon_Click(object sender, EventArgs e)                            //Handles tray icon and context menu clicks
        {
            ProcessStartInfo procStartInfo = new ProcessStartInfo();
            Process procExecuting = new Process();

            switch (sender.ToString())
            {
                case "System.Windows.Forms.NotifyIcon":
                    if (this.IsVisible)
                        this.Hide();
                    else
                        this.Show();
                    break;
                case "Pause":
                    Schedule_Sync.Dispose();
                    _tray_pause.Text = "Resume";
                    break;
                case "Resume":
                    Schedule_Sync = new System.Threading.Timer(Sync_Callback, 0, 0, 3 * 60 * 60 * 1000);    
                    _tray_pause.Text = "Pause";
                    break;
                case "Sync Now":
                    RunSync(-1);
                    break;
                case "Open Moodle Site":
                    Process.Start("http://moodle.iitb.ac.in/");
                    break;
                case "Open Moodle Folder":
                    MessageBox.Show("Clicked");
                    procStartInfo = new ProcessStartInfo();
                    procExecuting = new Process();

                    procStartInfo.UseShellExecute = true;
                    procStartInfo.WorkingDirectory = CONFIGDIR;
                    procStartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    procStartInfo.FileName = "explorer.exe";
                    procStartInfo.Arguments = File.ReadAllText("DownloadDir");

                    procExecuting = Process.Start(procStartInfo);
                    break;
                case "Setup":
                    Setup = new SetupWindow();
                    Setup.ShowDialog();
                    Schedule_Sync.Change(0, 3 * 60 * 60 * 1000);   
                    break;
                case "View Log":
                    procStartInfo = new ProcessStartInfo();
                    procExecuting = new Process();

                    procStartInfo.UseShellExecute = true;
                    procStartInfo.WorkingDirectory = CONFIGDIR;
                    procStartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    procStartInfo.FileName = "notepad.exe";
                    procStartInfo.Arguments = "MoodleSyncCallback.log";

                    procExecuting = Process.Start(procStartInfo);
                    break;
                case "Help":
                    Process.Start(System.Web.HttpUtility.UrlPathEncode("file:///" + System.AppDomain.CurrentDomain.BaseDirectory + "docs\\index.html").Replace("\\","/"));
                    break;
                case "About":
                    AboutDialog About = new AboutDialog();
                    About.Show();
                    break;
                case "Exit":
                    _tray.Visible = false;
                    this.Close();
                    break;
                default:
                    if ((New_Material > 0) || (New_Posts > 0))
                    {
                        this.Show();
                        MessageBox.Show("Notifications");                                   /////////////////
                    }
                    break;
            }

        }
        //----------Incomplete--------//
        private void Update_Notifications(object sender, EventArgs e)                       //Runs on new notification
        {
            //Fetch object and write notifications
            if ((New_Material == 0) && (New_Posts == 0))
            {
                _tray_notification.Text = "(No new notifications)";
                _tray_notification.Enabled = false;
            }
            else if ((New_Material > 0) && (New_Posts > 0))
            {
                _tray_notification.Text = New_Material + " new file(s) & " + New_Posts + " new post(s)";
                _tray_notification.Enabled = true;
            }
            else if ((New_Material > 0) && (New_Posts == 0))
            {
                _tray_notification.Text = New_Material + " new file(s)";
                _tray_notification.Enabled = true;
            }
            else if ((New_Material == 0) && (New_Posts > 0))
            {
                _tray_notification.Text = New_Posts + " new post(s)";
                _tray_notification.Enabled = true;
            }
        }

        private void LoadTrayIcon()                                                         //Loads tray icon and context menu
        {
            this._tray = new System.Windows.Forms.NotifyIcon();
            this._tray_menu = new System.Windows.Forms.ContextMenuStrip();
            this._tray_menu.SuspendLayout();
            //
            // _tray_menu_items
            //
            _tray_notification = new System.Windows.Forms.ToolStripMenuItem();
            _tray_pause = new System.Windows.Forms.ToolStripMenuItem();
            _tray_sync = new System.Windows.Forms.ToolStripMenuItem();
            _tray_separator1 = new System.Windows.Forms.ToolStripSeparator();
            _tray_moodle = new System.Windows.Forms.ToolStripMenuItem();
            _tray_browse = new System.Windows.Forms.ToolStripMenuItem();
            _tray_separator2 = new System.Windows.Forms.ToolStripSeparator();
            _tray_setup = new System.Windows.Forms.ToolStripMenuItem();
            _tray_log = new System.Windows.Forms.ToolStripMenuItem();
            _tray_help = new System.Windows.Forms.ToolStripMenuItem();
            _tray_about = new System.Windows.Forms.ToolStripMenuItem();
            _tray_separator3 = new System.Windows.Forms.ToolStripSeparator();
            _tray_exit = new System.Windows.Forms.ToolStripMenuItem();
            // 
            // _tray
            // 
            this._tray.ContextMenuStrip = this._tray_menu;
            this._tray.Icon = new System.Drawing.Icon(System.AppDomain.CurrentDomain.BaseDirectory + "images/favicon.ico");
            this._tray.Text = "Moodle Sync";
            this._tray.Visible = true;
            this._tray.DoubleClick += new System.EventHandler(this.Tray_Icon_Click);
            // 
            // _tray_menu
            // 
            this._tray_menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            _tray_notification,
            _tray_pause,
            _tray_sync,
            _tray_separator1,
            _tray_moodle,
            _tray_browse,
            _tray_separator2,
            _tray_setup,
            _tray_log,
            _tray_help,
            _tray_about,
            _tray_separator3,
            _tray_exit});
            this._tray_menu.Name = "_tray_menu";
            //
            //_tray_notification
            //
            this._tray_notification.Name = "_tray_notification";
            this._tray_notification.Size = new System.Drawing.Size(187, 22);
            this._tray_notification.Text = "(No new notifications)";
            this._tray_notification.Enabled = false;
            //
            //_tray_pause
            //
            this._tray_pause.Name = "_tray_pause";
            this._tray_pause.Size = new System.Drawing.Size(187, 22);
            this._tray_pause.Text = "Pause";
            this._tray_pause.Click += new System.EventHandler(this.Tray_Icon_Click);
            //
            //_tray_sync
            //
            this._tray_sync.Name = "_tray_sync_now";
            this._tray_sync.Size = new System.Drawing.Size(187, 22);
            this._tray_sync.Text = "Sync Now";
            this._tray_sync.Click += new System.EventHandler(this.Tray_Icon_Click);
            //
            //_tray_separator1
            //
            this._tray_separator1.Name = "_tray_separator";
            this._tray_separator1.Size = new System.Drawing.Size(184, 6);
            //
            //_tray_moodle
            //
            this._tray_moodle.Name = "_tray_moodle";
            this._tray_moodle.Size = new System.Drawing.Size(187, 22);
            this._tray_moodle.Text = "Open Moodle Site";
            this._tray_moodle.Click += new System.EventHandler(this.Tray_Icon_Click);
            //
            //_tray_browse
            //
            this._tray_browse.Name = "_tray_browse";
            this._tray_browse.Size = new System.Drawing.Size(187, 22);
            this._tray_browse.Text = "Open Moodle Folder";
            this._tray_browse.Click += new System.EventHandler(this.Tray_Icon_Click);
            //
            //_tray_separator2
            //
            this._tray_separator2.Name = "_tray_separator";
            this._tray_separator2.Size = new System.Drawing.Size(184, 6);
            //
            //_tray_setup
            //
            this._tray_setup.Name = "_tray_browse";
            this._tray_setup.Size = new System.Drawing.Size(187, 22);
            this._tray_setup.Text = "Setup";
            this._tray_setup.Click += new System.EventHandler(this.Tray_Icon_Click);
            //
            //_tray_log
            //
            this._tray_log.Name = "_tray_log";
            this._tray_log.Size = new System.Drawing.Size(187, 22);
            this._tray_log.Text = "View Log";
            this._tray_log.Click += new System.EventHandler(this.Tray_Icon_Click);
            //
            //_tray_help
            //
            this._tray_help.Name = "_tray_help";
            this._tray_help.Size = new System.Drawing.Size(187, 22);
            this._tray_help.Text = "Help";
            this._tray_help.Click += new System.EventHandler(this.Tray_Icon_Click);
            //
            //_tray_about
            //
            this._tray_about.Name = "_tray_about";
            this._tray_about.Size = new System.Drawing.Size(187, 22);
            this._tray_about.Text = "About";
            this._tray_about.Click += new System.EventHandler(this.Tray_Icon_Click);
            //
            //_tray_separator3
            //
            this._tray_separator3.Name = "_tray_separator";
            this._tray_separator3.Size = new System.Drawing.Size(184, 6);
            //
            //_tray_exit
            // 
            this._tray_exit.Name = "_tray_exit";
            this._tray_exit.Size = new System.Drawing.Size(187, 22);
            this._tray_exit.Text = "Exit";
            this._tray_exit.Click += new System.EventHandler(this.Tray_Icon_Click);

            this._tray_menu.ResumeLayout();
            this.UpdateLayout();
        }
        private void UnloadTrayIcon(object sender, EventArgs e)                             //Unloads tray icon and context menu on exit
        {
            _tray.Visible = false;

            _tray_notification.Dispose();
            _tray_pause.Dispose();
            _tray_sync.Dispose();
            _tray_separator1.Dispose();
            _tray_moodle.Dispose();
            _tray_browse.Dispose();
            _tray_separator2.Dispose();
            _tray_setup.Dispose();
            _tray_log.Dispose();
            _tray_help.Dispose();
            _tray_about.Dispose();
            _tray_separator3.Dispose();
            _tray_exit.Dispose();

            _tray_menu.Dispose();
            _tray.Dispose();
        }

        //----------Incomplete--------//
        public MainWindow()                                                                 //Main initialisation block
        {
            InitializeComponent();
            LoadTrayIcon();
            this.Hide();

            Dispatcher.ShutdownStarted += UnloadTrayIcon;

            if (!System.IO.Directory.Exists(CONFIGDIR))
                System.IO.Directory.CreateDirectory(CONFIGDIR);
            Environment.CurrentDirectory = CONFIGDIR;

            if (!(File.Exists("DownloadDir") && File.Exists("Login") && File.Exists("CourseList")))
            {
                Setup = new SetupWindow();
                Setup.ShowDialog();
                if (Setup.Successful == false)
                {
                    this.Close();
                }
            }

            //Load configuration                                                                          //////////////////
            //Load notifications                                                                          /////////////////

            Logger = new Translator.LoggingClass();
            Logger.OnNotify += new Translator.LoggingClass.NotifyHandler(Update_Notifications);

            Schedule_Sync = new System.Threading.Timer(Sync_Callback, 0, 0, 3 * 60 * 60 * 1000);
        }
    }
}
