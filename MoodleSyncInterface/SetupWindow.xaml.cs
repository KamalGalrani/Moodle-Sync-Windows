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
using System.Windows.Shapes;

using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;
using System.Web;

namespace MoodleSyncInterface
{
    /// <summary>
    /// Interaction logic for SetupWindow.xaml
    /// </summary>
    public partial class SetupWindow : Window
    {
        //GLOBALS
        private List<string[]> CourseList;
        //private List<string[]> SyncContentList;
        private List<string[]> SyncAllList;
        private MoodleSyncParser.Parser document;
        private bool SUCCESS;

        readonly string CONFIGDIR = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Moodle Sync";

        public bool Successful
        {
            get { return SUCCESS; }
        }

        private void auth_Click(object sender, RoutedEventArgs e)
        {
            _courselist.Items.Clear();
            _save.IsEnabled = false;

            string loginString = "username=" + HttpUtility.UrlEncode(_uname.Text) + "&password=" + HttpUtility.UrlEncode(_password.Password);
            document.Load("http://moodle.iitb.ac.in/login/index.php", loginString);
            
            if (document.Title == "IIT Bombay Moodle")
            {
                System.IO.File.WriteAllText("Login", loginString);
                _auth_err.Visibility = Visibility.Hidden;
                _netw_err.Visibility = Visibility.Hidden;

                CourseList = document.getCourseList();
                if (CourseList.Count == 0)
                {
                    MessageBox.Show("Could not download course blacklist. Are you able to access http://home.iitb.ac.in/~kamalgalrani/resources/moodle/blacklist.html ?", "Connection Error!!!");
                    return;
                }
                foreach (string[] Course in CourseList)
                    _courselist.Items.Add(new CheckedItem { Content = Course[2], IsChecked = false });

                _auth.IsDefault = false;
                _save.IsEnabled = true;
                _save.IsDefault = true;
                //_add_url.IsEnabled = true;
            }
            else if (document.Title == "IIT Bombay Moodle: Login to the site")
                _auth_err.Visibility = Visibility.Visible;
            else
                _netw_err.Visibility = Visibility.Visible;
        }
        private void browse_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new CommonOpenFileDialog();

            dlg.Title = "Download Directory";
            dlg.IsFolderPicker = true;
            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            dlg.AddToMostRecentlyUsedList = false;
            dlg.AllowNonFileSystemItems = false;

            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;

            while (true)
            {
                if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    _downloaddir.IsEnabled = true;
                    _downloaddir.Content = dlg.FileName;
                    break;
                }
                else
                    MessageBox.Show("Select a valid download directory." + Environment.NewLine + "This is the directory where the downloaded material will be stored.", "Invalid Selection!");
            }

        }
        private void save_Click(object sender, RoutedEventArgs e)
        {
            foreach (CheckedItem Course in _courselist.Items)
            {
                if (Course.IsChecked)
                    SyncAllList.Add(CourseList[_courselist.Items.IndexOf(Course)]);
            }
            if (SyncAllList.Count == 0)
            {
                MessageBox.Show("Select at least one course...", "Invalid Selection!");
                return;
            }

            if (_downloaddir.Content.ToString() == "Where to save downloaded stuff?")
            {
                MessageBox.Show("Select a valid download directory." + Environment.NewLine + "This is the directory where the downloaded material will be stored.", "Invalid Selection!");
                return;
            }

            _auth.IsEnabled = false;

            System.IO.File.WriteAllText("DownloadDir", _downloaddir.Content.ToString());
            System.IO.StreamWriter sw = new System.IO.StreamWriter("CourseList", false);
            foreach (string[] Course in SyncAllList)
            {
                sw.WriteLine("\"" + Course[0] + "\",\"" + Course[1] + "\",\"" + Course[2] + "\"");
            }
            sw.Close();

            System.IO.File.WriteAllText(CONFIGDIR + "\\MoodleSyncCallback.vbs",
                "CreateObject(\"Wscript.Shell\").Run \"\"\"" + System.AppDomain.CurrentDomain.BaseDirectory + "MoodleSyncCallback.exe\"\"\", 0, False");

            //ProcessStartInfo procStartInfo = new ProcessStartInfo();
            //Process procExecuting = new Process();

            //procStartInfo.UseShellExecute = true;
            //procStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //procStartInfo.FileName = "schtasks";
            //procStartInfo.Arguments = "/create /sc hourly /mo 3 /sd " + DateTime.Now.ToString("dd/MM/yyyy") + " /tn \"Moodle Sync\" /tr \"wscript.exe \"\"" + CONFIGDIR + "\\MoodleSyncCallback.vbs\"\"\" /F";

            //procExecuting = Process.Start(procStartInfo);
            //procExecuting.WaitForExit();

            //procStartInfo = new ProcessStartInfo();
            //procExecuting = new Process();

            //procStartInfo.UseShellExecute = true;
            //procStartInfo.WorkingDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            //procStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //procStartInfo.FileName = "wscript.exe";
            //procStartInfo.Arguments = "Callback.vbs";

            //procExecuting = Process.Start(procStartInfo);
            //procExecuting.WaitForExit();

            SUCCESS = true;
            this.Close();
        }
        private void add_url_Click(object sender, RoutedEventArgs e)
        {

        }
        public SetupWindow()
        {
            InitializeComponent();

            SyncAllList = new List<string[]>();
            //SyncContentList = new List<string[]>();
            document = new MoodleSyncParser.Parser();
            SUCCESS = false;

            if (!System.IO.Directory.Exists(CONFIGDIR))
                System.IO.Directory.CreateDirectory(CONFIGDIR);
            Environment.CurrentDirectory = CONFIGDIR;
        }

        public class CheckedItem
        {
            public string Content { get; set; }
            public bool IsChecked { get; set; }
        }
        private void uid_FocusChange(object sender, RoutedEventArgs e)
        {
            if ((_uname.Text == "LDAP Username") && (e.RoutedEvent.Name == "GotFocus"))
            {
                _uname.Text = "";
                _uname.Foreground = Brushes.Black;
            }
            else if ((_uname.Text == "") && (e.RoutedEvent.Name == "LostFocus"))
            {
                _uname.Text = "LDAP Username";
                _uname.Foreground = Brushes.WhiteSmoke;
            }
        }
        private void pwd_FocusChange(object sender, RoutedEventArgs e)
        {
            if (e.RoutedEvent.Name == "GotFocus")
            {
                _password.Password = "";
                _password.Foreground = Brushes.Black;
            }
            else if ((_password.Password == "") && (e.RoutedEvent.Name == "LostFocus"))
            {
                _password.Password = "LDAP Password";
                _password.Foreground = Brushes.WhiteSmoke;
            }
        }
    }
}
