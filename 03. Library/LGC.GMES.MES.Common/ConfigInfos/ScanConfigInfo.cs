using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace LGC.GMES.MES.Common.ConfigInfos
{
    public class ScanConfigInfo : INotifyPropertyChanged
    {
        // boiler-plate
        public event PropertyChangedEventHandler PropertyChanged;

        #region INotifyPropertyChanged Members
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private string _ID = null;
        // Get or set the Scann ID.
        public String ID
        {
            get { return _ID; }
            set { SetField(ref _ID, value, "ID"); }
        }

        private string _PortName = "COM1";
        // Get or set the PortName.
        public String PortName
        {
            get { return _PortName; }
            set { SetField(ref _PortName, value, "PortName"); }
        }

        private string _BaudRate = "9600";
        // Get or set the BaudRate.
        public String BaudRate
        {
            get { return _BaudRate; }
            set { SetField(ref _BaudRate, value, "BaudRate"); }
        }

        private string _DataBits = "8";
        // Get or set the DataBits.
        public String DataBits
        {
            get { return _DataBits; }
            set { SetField(ref _DataBits, value, "DataBits"); }
        }

        private string _StopBits = "One";
        // Get or set the StopBits.
        public String StopBits
        {
            get { return _StopBits; }
            set { SetField(ref _StopBits, value, "StopBits"); }
        }

        private string _Parity = "None";
        // Get or set the Parity.
        public String Parity
        {
            get { return _Parity; }
            set { SetField(ref _Parity, value, "Parity"); }
        }

        private string _IsManual = "N";
        // Get or set Whether is manual Scanner or not.
        public String IsManual
        {
            get { return _IsManual; }
            set { SetField(ref _IsManual, value, "IsManual"); }
        }

        private string _NoRead = "";
        // Get or set NoRead Data.
        public String NoRead
        {
            get { return _NoRead; }
            set { SetField(ref _NoRead, value, "NoRead"); }
        }

        private string _Command = "";
        // Get or set Command Data.
        public String Command
        {
            get { return _Command; }
            set { SetField(ref _Command, value, "Command"); }
        }

        private string _IsReadLine = "N";
        // Get or set Command Data.
        public String IsReadLine
        {
            get { return _IsReadLine; }
            set { SetField(ref _IsReadLine, value, "IsReadLine"); }
        }

        private string _IsRawData = "N";
        // Get or set Command Data.
        public String IsRawData
        {
            get { return _IsRawData; }
            set { SetField(ref _IsRawData, value, "IsRawData"); }
        }

        private string _IsSeperate = "N";
        // Get or set Command Data.
        public String IsSeperate
        {
            get { return _IsSeperate; }
            set { SetField(ref _IsSeperate, value, "IsSeperate"); }
        }

        private string _IsMixing = "N";
        // Get or set Whether is Mixing Scanner or not.
        public String IsMixing
        {
            get { return _IsMixing; }
            set { SetField(ref _IsMixing, value, "IsMixing"); }
        }
    }
}
