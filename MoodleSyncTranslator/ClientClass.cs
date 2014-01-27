/*
 * ClientClass.cs
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

using System.Net;
using System.IO;

namespace MoodleSyncParser
{
    public class ClientClass
    {
        private CookieContainer CookieJar;

        public CookieContainer getCookieJar                                                        //Get/Set CookieCollection
        {
            get { return CookieJar; }
            set { CookieJar = value; }
        }

        public ClientClass()                                                                       //Default Constructor
        {
            CookieJar = new CookieContainer();
        }
        public ClientClass(CookieContainer ParentCookieJar)                                        //Child Constructor
        {
            CookieJar = ParentCookieJar;
        }
        public HttpWebResponse getRawResponse(string URL, string PostString = "")                  //Gets raw response for URL
        {
            //Create Request
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(URL);

            //Set Headers and write POST data
            request.CookieContainer = CookieJar;
            if (PostString == "")
                request.Method = "GET";
            else
            {
                request.Method = "POST";
                request.ContentLength = PostString.Length;
                request.ContentType = "application/x-www-form-urlencoded";
                StreamWriter sw = new StreamWriter(request.GetRequestStream());
                sw.Write(PostString);
                sw.Close();
            }

            //Get Response
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            //Set Cookies
            foreach (Cookie _Cookie in response.Cookies)
            {
                CookieJar.Add(_Cookie);
            }

            return response;
        }
        public HttpStatusCode getResponse(string URL, ref string sHtml, string PostData = "")      //Gets response string for URL
        {
            try
            {
                //Get Response
                HttpWebResponse response = getRawResponse(URL, PostData);
                HttpStatusCode status = response.StatusCode;

                //Get response stream
                StreamReader sr = new StreamReader(response.GetResponseStream());
                sHtml = sr.ReadToEnd();
                sr.Close();

                response.Close();
                return status;
            }
            catch
            {
                return HttpStatusCode.NoContent;
            }
        }
    }
}

