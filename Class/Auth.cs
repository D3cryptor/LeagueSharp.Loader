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

namespace LeagueSharp.Loader.Class
{
    internal static class Auth
    {
        public static Tuple<bool, string> Login(string user, string md5Password)
        {
            if (user == ":^)")
            {
                return new Tuple<bool, string>(true, "Success!");
            }

            if (user == "banned")
            {
                return new Tuple<bool, string>(false, "Your username is banned");
            }

            return new Tuple<bool, string>(false, "Incorrect password");
        }

        public static string Hash(string input)
        {
            return Utility.Md5Hash(input);
        }
    }
}