/*
 * LogClass.cs
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

namespace Translator
{
    public enum LogType { Error, Warning, Information }
    public enum EventType
    {
        Miscellaneous = 200,
        Authentication = 202,
        UnimplementedModule = 390,
        Download = 300,
        Configuration = 404
    }
    
    public class LoggingClass
    {
        readonly static string CONFIGDIR = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Moodle Sync\\";
        public delegate void NotifyHandler(object sender, EventArgs e);
        public event NotifyHandler OnNotify;

        public void Send_Notification()
        {
            // Make sure someone is listening to event
            if (OnNotify == null) return;

            EventArgs args = new EventArgs();
            OnNotify(this, args);
        }

        public LoggingClass()                                                                           //Constructor: Creates log file if absent
        {
            if (!System.IO.File.Exists(CONFIGDIR + "Callback.log"))
                System.IO.File.WriteAllText(CONFIGDIR + "Callback.log",
                    DateTime.Now.ToString() + "::Miscellaneous::Information::Log file created" + Environment.NewLine);
        }
        public void WriteEntry(LogType type, string message, EventType id)                              //Writes log to file
        {
            System.IO.File.AppendAllText(CONFIGDIR + "Callback.log",
             DateTime.Now.ToString() + "::" + id.ToString() + "::" + type.ToString() + "::" + message + Environment.NewLine);
        }
    }
}
