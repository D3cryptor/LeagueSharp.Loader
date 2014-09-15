#region

using System.Collections.Specialized;
using System.Net;
using System.Net.NetworkInformation;
using System.Windows.Forms;

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
        public static bool Authed { get; set; }

        public static Tuple<bool, string> Login(string user, string password)
        {
            return new Tuple<bool, string>(true, "Success!");

            var ping = new Ping();
            var reply = ping.Send("http://www.joduska.me/");

            if (reply == null || reply.Status != IPStatus.Success)
            {
                return new Tuple<bool, string>(true, "Fallback T_T");
            }

            using (var wb = new WebClient())
            {
                var data = new NameValueCollection();
                data["username"] = user;
                data["password"] = Hash(password);

                var response = wb.UploadValues("http://www.joduska.me/forum/api.php", "POST", data);

                if (response.ToString().Contains("success"))
                {
                    return new Tuple<bool, string>(true, "Success!");
                }

                if (response.ToString().Contains("banned"))
                {
                    return new Tuple<bool, string>(false, "Your username is banned");
                }

                return new Tuple<bool, string>(false, "Wrong password or username");
            }
        }

        public static string Hash(string input)
        {
            return new phpBBCryptoServiceProvider().Hash(input);
        }
    }
}