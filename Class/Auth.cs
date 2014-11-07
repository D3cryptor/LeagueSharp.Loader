#region

using System.IO;
using System.Net;
using System.Text;
using System.Windows;

#region

using System;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;

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
        public const string AuthServer = "5.196.9.111";
        public static bool Authed { get; set; }

        public static Tuple<bool, string> Login(string user, string hash)
        {
            if (user == null || hash == null)
            {
                return new Tuple<bool, string>(false, Utility.GetMultiLanguageText("AuthEmpty"));
            }

            try
            {
                using (var client = new TcpClient())
                {
                    client.Connect(AuthServer, 8080);
                    if(client.Connected)
                    {
                        var stream = new SslStream(client.GetStream(), false,  new RemoteCertificateValidationCallback((sender, certificate, chain, policyErrors) => { return true; }), null);
                        try
                        {
                            stream.AuthenticateAsClient(AuthServer);
                            stream.Write(System.Text.Encoding.UTF8.GetBytes("{\"action\" : \"ll\", \"user\" : \"" + user.Trim() + "\", \"pass\" : \"" + hash.Trim() + "\"}"));
                            if(stream.ReadByte() == '1')
                            {
                                return new Tuple<bool, string>(true, "Success!");
                            }
                            else
                            {
                                return new Tuple<bool, string>(false, string.Format(Utility.GetMultiLanguageText("WrongAuth"), "www.joduska.me"));
                            }
                        }
                        catch (AuthenticationException)
                        {
                            return new Tuple<bool, string>(false, "Fallback T_T");
                        }
                    }
                }
                return new Tuple<bool, string>(true, "Fallback T_T");
            }
            catch (Exception)
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
