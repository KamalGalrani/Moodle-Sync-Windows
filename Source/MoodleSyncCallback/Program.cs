/*
 * Program.cs
 * This file is part of Moodle Sync.
 * 
 * (c) 2014 by Kamal Galrani
 *
 * Moodle Sync is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * Moodle Sync is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Moodle Sync.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using Translator;

namespace MoodleSyncCallback
{
    class Program
    {
        static MoodleSyncParser.Parser document = new MoodleSyncParser.Parser();
        static Translator.LoggingClass Log = new Translator.LoggingClass();

        static IList<string[]> Courses;
        static string DownloadDir;
        static string LoginString;
        static Boolean REDALERT = false;

        private static Boolean SyncMaterial()                                                       //Wrapper function for synchronising files
        {
            Boolean SUCCESS = true;
            lock ("SyncMaterial")
            {
                List<string[]> _Modules = new List<string[]>();

                foreach (string[] _Course in Courses)
                {
                    if (!Directory.Exists(_Course[0]))
                    {
                        Directory.CreateDirectory(_Course[0]);
                        Log.WriteEntry(LogType.Information, "Directory created: " + DownloadDir + "\\" + _Course[0], EventType.Miscellaneous);
                    }

                    if (!Directory.Exists(_Course[0] + "\\Forum"))
                    {
                        Directory.CreateDirectory(_Course[0] + "\\Forum");
                        Log.WriteEntry(LogType.Information, "Directory created: " + DownloadDir + "\\" + _Course[0] + "\\Forum", EventType.Miscellaneous);
                    }

                    string dst = "";

                    if (!LoadModules(ref _Modules, _Course[1])) return false;
                    document.Save(DownloadDir + "\\" + _Course[0] + "\\Material.html");

                    foreach (string[] _Module in _Modules)
                    {
                        switch (_Module[1])
                        {
                            case "forum":
                                dst = "";

                                List<string[]> DiscussionList = document.getDiscussions(_Module[0]);

                                foreach (string[] Discussion in DiscussionList)
                                {
                                    List<string[]> FileList = document.getPosts(Discussion[0]);
                                 
                                    foreach (string[] File in FileList)
                                    {
                                        dst = DownloadDir + "\\" + _Course[0] + "\\Forum\\" + File[6];
                                        if (!System.IO.File.Exists(dst))
                                        {
                                            if (document.Save(File[7], dst))
                                                Log.WriteEntry(LogType.Information, "\"" + _Course[0] + "\",\"" + File[6] + "\",\"" + dst + "\"", EventType.Download);
                                            else
                                            {
                                                SUCCESS = false;
                                                Log.WriteEntry(LogType.Error, "\"" + _Course[0] + "\",\"" + File[6] + "\",\"" + dst + "\"", EventType.Download);
                                            }
                                        }
                                    }

                                }
                                break;
                            case "folder":
                                break;
                            case "resource":
                                string url = "";
                                string nme = "";
                                dst = "";

                                document.Load(_Module[0]);

                                url = document.getResourceFile(_Module[0], ref nme);
                                if (nme == "") { url = _Module[0]; nme = _Module[2]; }
                                dst = DownloadDir + "\\" + _Course[0] + "\\" + nme;

                                if (!System.IO.File.Exists(dst))
                                {
                                    if (document.Save(url, dst))
                                        Log.WriteEntry(LogType.Information, "\"" + _Course[0] + "\",\"" + nme + "\",\"" + dst + "\"", EventType.Download);
                                    else
                                    {
                                        SUCCESS = false;
                                        Log.WriteEntry(LogType.Error, "\"" + _Course[0] + "\",\"" + nme + "\",\"" + dst + "\"", EventType.Download);
                                    }
                                }
                                break;
                            default:
                                Log.WriteEntry(LogType.Warning, "\"" + _Course[0] + "\",\"" + _Module[2] + "\",\"" + _Module[1] + "\",\"" + _Module[0] + "\"", EventType.UnimplementedModule);
                                break;
                        }
                    }

                }
            }
            return SUCCESS;
        }
        private static Boolean LoadModules(ref List<string[]> _Modules, string _Course)             //Method to fetch list of items (modules) on course page
        {
            try
            {
                _Modules = document.getModuleList(_Course);

                if (document.Title.Substring(0, 6) != "Course")
                    throw new Exception();

                return true;
            }
            catch
            {
                if (reLogin()) return LoadModules(ref _Modules, _Course);
                else return false;
            }
        }
        private static Boolean reLogin()                                                            //Method to log in again in case we log out
        {
            if (document.Title == "IIT Bombay Moodle: Login to the site")
            {
                Log.WriteEntry(LogType.Information, "You are logged out!!! Logging in again...", EventType.Authentication);
                document.Load("http://moodle.iitb.ac.in/login/index.php", LoginString);
                if (document.Title == "IIT Bombay Moodle")
                {
                    Log.WriteEntry(LogType.Information, "Successfully logged in... Synchronising", EventType.Authentication);
                    return true;
                }
                else if (document.Title == "IIT Bombay Moodle: Login to the site")
                {
                    Log.WriteEntry(LogType.Error, "Error logging in! Re-enter your credentials...", EventType.Authentication);
                    REDALERT = true;
                    return false;
                }
            }
            Log.WriteEntry(LogType.Warning, "Error connecting to Moodle! Please check your internet connection.", EventType.Authentication);
            return false;
        }
        static void Main(string[] args)                                                             //Loads configuration and begins sync
        {
            //---------------------------------READING CONFIGURATION FROM DISK---------------------------------//

            string ConfigurationDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Moodle Sync";
            if (!Directory.Exists(ConfigurationDir))
            {
                Log.WriteEntry(LogType.Error, "Configuration folder missing!!! Did you setup your account?", EventType.Configuration);
                return;
            }
            Environment.CurrentDirectory = ConfigurationDir;

            if (!(File.Exists("DownloadDir") && File.Exists("Login") && File.Exists("CourseList")))
            {
                Log.WriteEntry(LogType.Error, "Configuration files missing!!! Did you setup your account?", EventType.Configuration);
                return;
            }
            
            DownloadDir = File.ReadAllText("DownloadDir");
            if (!Directory.Exists(DownloadDir))
            {
                Log.WriteEntry(LogType.Error, "Download directory missing!!! Did you setup your account?", EventType.Configuration);
                return;
            }

            Courses = new List<string[]>();
            string[] _CourseFile = File.ReadAllLines("CourseList");
            foreach (string _CourseLine in _CourseFile)
            {
                Courses.Add(System.Text.RegularExpressions.Regex.Split(_CourseLine.Trim('\"'), "\",\""));
            }

            LoginString = File.ReadAllText("Login");
            Environment.CurrentDirectory = DownloadDir;
            
            Log.WriteEntry(LogType.Information, "Configuration loaded successfully... Logging in...", EventType.Miscellaneous);

            //-----------------------------------LOGGING IN AND SYNCHRONISING----------------------------------//
            document.Load("http://moodle.iitb.ac.in/login/index.php", LoginString);
            if (document.Title == "IIT Bombay Moodle")
                Log.WriteEntry(LogType.Information, "Successfully logged in... Synchronising", EventType.Authentication);
            else if (document.Title == "IIT Bombay Moodle: Login to the site")
            {
                Log.WriteEntry(LogType.Error, "Error logging in! Re-enter your credentials...", EventType.Authentication);
                return;
            }

            DateTime startBoundary = DateTime.Now;
            while (DateTime.Now < startBoundary.AddMinutes(30))
            {
                if (SyncMaterial()) break;
                if (REDALERT)
                {
                    Log.WriteEntry(LogType.Error, "Aborting Synchronisation!!!", EventType.Miscellaneous);
                    break;
                }
                System.Threading.Thread.Sleep(60000);
            }

            Log.WriteEntry(LogType.Information, "Callback completed. Exiting thread.", EventType.Miscellaneous);
        }
    }
}
