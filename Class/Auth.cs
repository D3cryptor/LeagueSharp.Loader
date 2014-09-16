#region

using System.IO;
using System.Net;
using System.Text;

#region

using System;

#endregion

/*
    Copyright (C) 2014 LeagueSharp

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

namespace LeagueSharp.Loader.Class
{
    internal static class Auth
    {
        public const string AuthServer = "www.website.com";
        public static bool Authed { get; set; }

        public static Tuple<bool, string> Login(string user, string hash)
        {
            return new Tuple<bool, string>(true, "Success!");

            if (user == null || hash == null)
            {
                return new Tuple<bool, string>(false, "Password or username is empty");
            }

            try
            {
                var uri = "username=" + WebUtility.UrlEncode(user) + "&password=" + hash;
                var dataBytes = Encoding.UTF8.GetBytes(uri);

                var wr = WebRequest.Create("http://" + AuthServer + "/forum/api.php?request=login");
                wr.Timeout = 2000;
                wr.ContentLength = dataBytes.Length;
                wr.Method = "POST";
                wr.ContentType = "application/x-www-form-urlencoded";

                var dataStream = wr.GetRequestStream();
                dataStream.Write(dataBytes, 0, dataBytes.Length);
                dataStream.Close();

                var response = wr.GetResponse();

                using (var stream = response.GetResponseStream())
                {
                    if (stream != null)
                    {
                        var responseString = new StreamReader(stream, Encoding.UTF8).ReadToEnd();

                        if (responseString.Contains("success"))
                        {
                            return new Tuple<bool, string>(true, "Success!");
                        }
                    }
                }

                return new Tuple<bool, string>(false, "Wrong password or username, register at http://" + AuthServer);
            }
            catch (Exception e)
            {
                return new Tuple<bool, string>(true, "Fallback T_T");
            }
        }

        public static string Hash(string input)
        {
            return Utility.Md5Hash(input);
        }
    }
}