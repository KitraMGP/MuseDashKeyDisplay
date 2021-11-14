using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuseDashKeyDisplay
{
    public class StaticData : INotifyPropertyChanged
    {
        private string msg;

        private static StaticData instance;
        public event PropertyChangedEventHandler PropertyChanged;

        public static StaticData Get()
        {
            if (instance == null) instance = new StaticData();
            return instance;
        }

        public string Msg
        {
            get { return msg; }
            set 
            { 
                msg = value; 
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Msg))); 
            }
        }
    }
}
