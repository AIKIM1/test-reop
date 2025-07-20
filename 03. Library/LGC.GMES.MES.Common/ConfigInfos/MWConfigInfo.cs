using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace LGC.GMES.MES.Common.ConfigInfos
{
    public class MWConfigInfo : INotifyPropertyChanged
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

        private string _HOST = null;
        // Get or set the M/W HOST IP.
        public String HOST
        {
            get { return _HOST; }
            set { SetField(ref _HOST, value, "HOST"); }
        }

        private string _VPNNAME = null;
        // Get or set the M/W VPN NAME.
        public String VPNNAME
        {
            get { return _VPNNAME; }
            set { SetField(ref _VPNNAME, value, "VPNNAME"); }
        }

        private string _USERNAME = null;
        // Get or set the M/W Connection USER NAME.
        public String USERNAME
        {
            get { return _USERNAME; }
            set { SetField(ref _USERNAME, value, "USERNAME"); }
        }

        private string _PASSWORD = null;
        // Get or set the M/W Connection Password.
        public String PASSWORD
        {
            get { return _PASSWORD; }
            set { SetField(ref _PASSWORD, value, "PASSWORD"); }
        }

        private string _PROPERTIES = null;
        // Get or set the M/W Connection Addtional Properties.
        public String PROPERTIES
        {
            get { return _PROPERTIES; }
            set { SetField(ref _PROPERTIES, value, "PROPERTIES"); }
        }

        private string _SQL_QUEUE = null;
        // Get or set the M/W SQL QUEUE INFO.
        public String SQL_QUEUE
        {
            get { return _SQL_QUEUE; }
            set { SetField(ref _SQL_QUEUE, value, "SQL_QUEUE"); }
        }

        private string _MRS_QUEUE = null;
        // Get or set the M/W MRS QUEUE INFO.
        public String MRS_QUEUE
        {
            get { return _MRS_QUEUE; }
            set { SetField(ref _MRS_QUEUE, value, "MRS_QUEUE"); }
        }

        private string _TEST_QUEUE = null;
        // Get or set the M/W TEST QUEUE INFO.
        public String TEST_QUEUE
        {
            get { return _TEST_QUEUE; }
            set { SetField(ref _TEST_QUEUE, value, "TEST_QUEUE"); }
        }

        private string _LOG_QUEUE = null;
        // Get or set the M/W LOG QUEUE INFO.
        public String LOG_QUEUE
        {
            get { return _LOG_QUEUE; }
            set { SetField(ref _LOG_QUEUE, value, "LOG_QUEUE"); }
        }

        private string _CONNECTION_MODE = "CO";
        // Get or set the M/W Connection Mode.
        public String CONNECTION_MODE
        {
            get { return _CONNECTION_MODE; }
            set { SetField(ref _CONNECTION_MODE, value, "CONNECTION_MODE"); }
        }
    }
}
