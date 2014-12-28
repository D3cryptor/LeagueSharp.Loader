#region LICENSE

// Copyright 2014 LeagueSharp.Loader
// Log.cs is part of LeagueSharp.Loader.
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

namespace LeagueSharp.Loader.Data
{
    #region

    using System.Collections.ObjectModel;
    using System.ComponentModel;

    #endregion

    public static class Logs
    {
        public static Log MainLog = new Log();
    }

    public class Log : INotifyPropertyChanged
    {
        private ObservableCollection<LogItem> _items = new ObservableCollection<LogItem>();

        public ObservableCollection<LogItem> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                OnPropertyChanged("Items");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class LogItem : INotifyPropertyChanged
    {
        private string _message;
        private string _source;
        private string _status;

        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged("Status");
            }
        }

        public string Source
        {
            get { return _source; }
            set
            {
                _source = value;
                OnPropertyChanged("Source");
            }
        }

        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged("Message");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public static class LogStatus
    {
        public static string Ok
        {
            get { return "Ok"; }
        }

        public static string Info
        {
            get { return "Info"; }
        }

        public static string Error
        {
            get { return "Error"; }
        }

        public static string Skipped
        {
            get { return "Skipped"; }
        }
    }
}