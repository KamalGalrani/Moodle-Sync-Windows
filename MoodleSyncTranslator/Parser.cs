/*
 * Parser.cs
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

using System.Text.RegularExpressions;
using System.Net;
using System.IO;

namespace MoodleSyncParser
{
    public class Parser : ClientClass
    {
        public string Read                                                                         //Returns page
        {
            get { return sHtml; }
        } 
        private string sHtml = "";

        private static Regex r_Title                = new Regex("<title>(?<Title>[^<]*)</title>");

        private static Regex r_Course               = new System.Text.RegularExpressions.Regex("(?<URL>https?://[^/]*/course/view.php.id=(?<ID>\\d*)).>(?<CODE>[A-Z]+\\s\\d+)[^A-Z]*(\\w\\d+\\s)?(Minor\\s)?(?<NAME>[^<]*)(?<INFO>.*)");
        private static Regex r_Teachers             = new System.Text.RegularExpressions.Regex("(?<=Teacher:[^>]*>)[^<]*");

        private static Regex r_Forum                = new Regex("(?<URL>https?://[^/]*/mod.forum.view.php.id=(?<FORUM_ID>\\d*))");
        private static Regex r_Module               = new Regex("(?<URL>https?://[^/]*/mod.(?<TYPE>\\w*).view.php.id=(?<ID>\\d*)).>[^>]*>[^>]*>(?<NAME>[^<]*)");
        private static Regex r_Folder               = new Regex("(?<URL>https?://[^/]*/pluginfile.php[^\"]*)[^>]*>[^>]*>[^>]*>[^>]*>[^>]*>(?<NAME>[^<]*)<.span>");
        private static Regex r_Resource             = new Regex("(?<URL>https?://[^/]*/pluginfile.php[^\"]*)([^>]*)>(?<NAME>[^<]*)<.a>");
        private static Regex r_Attachment           = new Regex("(?<URL>https?://[^/]*/pluginfile.php.\\d*.mod.forum.attachment[^\"]*)([^>]*)>(?<NAME>[^<]*)<.a>");

        private static Regex r_Discussion           = new Regex("(?<URL>https?://[^/]*/mod.forum.discuss.php.d=(?<DISCUSSION_ID>\\d*)[^\"]*)[^>]*>(?<DATA>[^<]*)");
        private static Regex r_DiscussionAuthor     = new Regex("author.[^\"]*[^>]*>(?<NAME>[^<]*)");
        private static Regex r_DiscussionLastPost   = new Regex("lastpost.[^\"]*[^>]*>(?<NAME>[^<]*)");

        public HttpStatusCode Load(string URL, string POST = "")                                    //Loads URL for parsing
        {
            sHtml = "";
            return getResponse(URL, ref sHtml, POST);
        }
        public string Title                                                                         //Returns page title
        {
            get { return r_Title.Match(sHtml).Groups["Title"].ToString(); }
        }
        public List<string[]> getCourseList()                                                       //Gets a list of registered courses
        {
            List<string[]> CourseList = new List<string[]>();
            MatchCollection _Courses = r_Course.Matches(sHtml);

            Load("http://home.iitb.ac.in/~kamalgalrani/resources/moodle/blacklist.html");
            List<string> Blacklist = new List<string>();
            Blacklist.AddRange(System.Text.RegularExpressions.Regex.Split(sHtml.Trim('\"'), "\",\""));
            if (Blacklist[0] != "Blacklist")
                return CourseList;

            foreach (Match _Course in _Courses)
            {
                if (Blacklist.Contains(r_Teachers.Match(_Course.Groups["INFO"].ToString()).Groups[0].ToString()))
                    continue;

                string[] Course = new string[3];
                Course[0] = _Course.Groups["CODE"].ToString();
                Course[1] = _Course.Groups["URL"].ToString();
                Course[2] = _Course.Groups["NAME"].ToString();

                Course[2] = Course[2].Replace("&amp;", "&");

                CourseList.Add(Course);
            }
            return CourseList;
        }
        public List<string[]> getModuleList(string URL)                                             //Gets a list of items on a course page
        {
            List<string[]> ModuleList = new List<string[]>();

            Load(URL);

            foreach (Match _Module in r_Module.Matches(sHtml))
            {
                string[] Module = new string[3];
                Module[0] = _Module.Groups["URL"].ToString();
                Module[1] = _Module.Groups["TYPE"].ToString();
                Module[2] = _Module.Groups["NAME"].ToString();

                ModuleList.Add(Module);
            }
            return ModuleList;
        }
        public string getResourceFile(string URL, ref string nme)                                   //Gets link to files embedded in links on course page
        {
            Load(URL);

            nme = r_Resource.Match(sHtml).Groups["NAME"].ToString();
            return r_Resource.Match(sHtml).Groups["URL"].ToString();
        }
        public List<string[]> getDiscussions(string URL)                                            //Gets a list of discussions on a forum
        {
            List<string[]> DiscussionList = new List<string[]>();

            Load(URL);
            MatchCollection _Discussions = r_Discussion.Matches(sHtml);
            MatchCollection _Discussions_author = r_DiscussionAuthor.Matches(sHtml);
            MatchCollection _Discussions_lastpost = r_DiscussionLastPost.Matches(sHtml);

            for (int i = 0; i < _Discussions.Count/3; i++)
            {
                string[] Discussion = new string[6];
                Discussion[0] = _Discussions[3 * i].Groups["URL"].ToString();
                Discussion[1] = _Discussions[3 * i].Groups["DATA"].ToString();
                Discussion[2] = _Discussions_author[i].Groups["NAME"].ToString();
                Discussion[3] = _Discussions[3 * i + 1].Groups["DATA"].ToString();
                Discussion[4] = _Discussions_lastpost[i].Groups["NAME"].ToString();
                Discussion[5] = _Discussions[3 * i + 2].Groups["DATA"].ToString();

                DiscussionList.Add(Discussion);
            }

            return DiscussionList;
        }
        public List<string[]> getPosts(string URL)                                                  //Gets a list of posts in a discussion along with file attachments
                                                                                                    //THIS FUNCTION IS NOT FULLY IMPLEMENTED
        {
            List<string[]> PostList = new List<string[]>();
            
            Load(URL);

            foreach (Match _File in r_Attachment.Matches(sHtml))
            {
                string[] Post = new string[8];
                Post[0] = "";
                Post[1] = "";
                Post[2] = "";
                Post[3] = "";
                Post[4] = "";
                Post[5] = "";
                Post[6] = _File.Groups["NAME"].ToString();
                Post[7] = _File.Groups["URL"].ToString();
                PostList.Add(Post);
            }

            return PostList;
        }                                               
        public Boolean Save(string dst)                                                             //Saves currently loaded html to <dst>
        {
            try
                { File.WriteAllText(dst, sHtml); return true; }
            catch
                { return false; }
        }
        public Boolean Save(HttpWebResponse response, string dst)                                   //Writes raw response to <dst>
        {
            int bytesRead = 0;
            Byte[] buffer = new Byte[4095];
            
            try
            {
                FileStream outStream = new FileStream(dst, FileMode.Create, FileAccess.Write);
                do
                {
                    bytesRead = response.GetResponseStream().Read(buffer, 0, 4095);
                    if (bytesRead == 0) { break; }
                    outStream.Write(buffer, 0, bytesRead);
                } while (true);
            
                outStream.Close();
                response.Close();

                return true;
            }
            catch
            {
                if (File.Exists(dst))
                    File.Delete(dst);
                return false;
            }
        }
        public Boolean Save(string URL, string _nme)                                                //Saves <URL> to <dst>
        {
            try
                { return Save(getRawResponse(URL), _nme); }
            catch
                { return false; }
        }
    }
}
