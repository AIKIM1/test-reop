using System.ComponentModel;
using System.Diagnostics;

namespace LGC.GMES.MES.Common.ViewModel
{
    public abstract class ViewModelBase
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string DisplayName { get; set; }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.VerifyPropertyName(propertyName);

            PropertyChangedEventHandler handler = this.PropertyChanged;

            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;

                //if (this.ThrowOnInvalidPropertyName)
                //    throw new Exception(msg);
                //else
                //    Debug.Fail(msg);
            }
        }
    }
}