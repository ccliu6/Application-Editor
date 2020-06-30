using System.Collections.Generic;
using System.ComponentModel;

namespace DataCore
{
    /// <summary>
    /// VM base
    /// </summary> 
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    public class ParmAttribute : ViewModelBase
    { 
        public string Name { get; set; }
        public string Source { get; set; }
        public string Tag { get; set; }
        public string Visibility { get; set; }
        public string StrValue { get; set; }
        public string NumValue { get; set; }
        public string NumMin { get; set; }
        public string NumMax { get; set; }
        public string ExtZRatio { get; set; }
    }
}
