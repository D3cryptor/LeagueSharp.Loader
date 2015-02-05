#region LICENSE

// Copyright 2014 LeagueSharp.Loader
// Auth.cs is part of LeagueSharp.Loader.
// 
// LeagueSharp.Loader is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// LeagueSharp.Loader is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with LeagueSharp.Loader. If not, see <http://www.gnu.org/licenses/>.

#endregion

namespace LeagueSharp.Loader.Class
{
    #region

    using System;
    using System.Net;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Security.Authentication;
    using System.Text;

    #endregion

    internal static class Auth
    {
        public const string AuthServer = "loader.joduska.me";
        public static bool Authed { get; set; }

        public static Tuple<bool, string> Login(string user, string hash)
        {
            if (user == null || hash == null)
            {
                return new Tuple<bool, string>(false, Utility.GetMultiLanguageText("AuthEmpty"));
            }

            try
            {
                var data = "p=" + hash;
                var dataBytes = Encoding.UTF8.GetBytes(data);

                var wr = HttpWebRequest.Create("https://" + AuthServer + "/login/" + WebUtility.UrlEncode(user));
                wr.Timeout = 2000;
                wr.ContentLength = dataBytes.Length;
                wr.Method = "POST";
                wr.ContentType = "application/x-www-form-urlencoded";

                try
                {
                    var dataStream = wr.GetRequestStream();
                    dataStream.Write(dataBytes, 0, dataBytes.Length);
                    dataStream.Close();
                    wr.GetResponse();
                }
                catch (WebException ex)
                {
                    if ((int)((HttpWebResponse)ex.Response).StatusCode == 403)
                    {
                        return new Tuple<bool, string>(false, string.Format(Utility.GetMultiLanguageText("WrongAuth"), "www.joduska.me"));
                    }
                }
                
                return new Tuple<bool, string>(true, "Success");
            }
            catch (Exception)
            {
                return new Tuple<bool, string>(true, "Fallback T_T");
            }
        }

        private static string IPB_Clean_Password(string pass)
        {
            pass = pass.Replace("\xC3\x8A", "");
            pass = pass.Replace("&", "&amp;");
            pass = pass.Replace("\\", "&#092;");
            pass = pass.Replace("!", "&#33;");
            pass = pass.Replace("$", "&#036;");
            pass = pass.Replace("\"", "&quot;");
            pass = pass.Replace("\"", "&quot;");
            pass = pass.Replace("<", "&lt;");
            pass = pass.Replace(">", "&gt;");
            pass = pass.Replace("'", "&#39;");

            return pass;
        }

        public static string Hash(string input)
        {
            return Utility.Md5Hash(IPB_Clean_Password(input));
        }
    }
}