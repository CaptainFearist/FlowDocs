﻿using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace File_Manager.Core
{
    class ObservableObject : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
