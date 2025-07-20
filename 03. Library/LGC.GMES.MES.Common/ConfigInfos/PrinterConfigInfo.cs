using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace LGC.GMES.MES.Common.ConfigInfos
{
    public class PrinterConfigInfo : INotifyPropertyChanged
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

        private string _PRINTER_ID = "";
        // Get or set Printer ID.
        public String PRINTER_ID
        {
            get { return _PRINTER_ID; }
            set { SetField(ref _PRINTER_ID, value, "PRINTER_ID"); }
        }

        private string _PRINTER_NAME = "";
        // Get or set Printer ID.
        public String PRINTER_NAME
        {
            get { return _PRINTER_NAME; }
            set { SetField(ref _PRINTER_NAME, value, "PRINTER_NAME"); }
        }

        private string _PRINTER_GROUP = "";
        // Get or set Printer ID.
        public String PRINTER_GROUP
        {
            get { return _PRINTER_GROUP; }
            set { SetField(ref _PRINTER_GROUP, value, "PRINTER_GROUP"); }
        }

        private string _PRINTER_SETTING = "";
        // Get or set Printer ID.
        public String PRINTER_SETTING
        {
            get { return _PRINTER_SETTING; }
            set { SetField(ref _PRINTER_SETTING, value, "PRINTER_SETTING"); }
        }

        private string _PARENT_PRINTER_ID = "";
        // Get or set Printer ID.
        public String PARENT_PRINTER_ID
        {
            get { return _PARENT_PRINTER_ID; }
            set { SetField(ref _PARENT_PRINTER_ID, value, "PARENT_PRINTER_ID"); }
        }

        private string _IS_STANDBY = "N";
        // Get or set Printer ID.
        public String IS_STANDBY
        {
            get { return _IS_STANDBY; }
            set { SetField(ref _IS_STANDBY, value, "IS_STANDBY"); }
        }

        private string _PRINTER_STATUS = "NG";
        // Get or set Printer ID.
        public String PRINTER_STATUS
        {
            get { return _PRINTER_STATUS; }
            set { SetField(ref _PRINTER_STATUS, value, "PRINTER_STATUS"); }
        }

        private string _INDEX = "0";
        // Get or set Printer ID.
        public String INDEX
        {
            get { return _INDEX; }
            set { SetField(ref _INDEX, value, "INDEX"); }
        }

        private string _IS_LTP_SEND = "N";
        // Get or set Printer ID.
        public String IS_LTP_SEND
        {
            get { return _IS_LTP_SEND; }
            set { SetField(ref _IS_LTP_SEND, value, "IS_LTP_SEND"); }
        }

        private string _LABEL_TYPE = "";
        // Get or set Print Label Type
        public String LABEL_TYPE
        {
            get { return _LABEL_TYPE; }
            set { SetField(ref _LABEL_TYPE, value, "LABEL_TYPE"); }
        }

        private string _LABEL_WORK_TYPE = "";
        // Get or set Print Label Work Type
        public String LABEL_WORK_TYPE
        {
            get { return _LABEL_WORK_TYPE; }
            set { SetField(ref _LABEL_WORK_TYPE, value, "LABEL_WORK_TYPE"); }
        }

        private string _PRINTER_TYPE = "";
        // Get or set Printer Type (SERIAL/TCP/LPT).
        public String PRINTER_TYPE
        {
            get { return _PRINTER_TYPE; }
            set { SetField(ref _PRINTER_TYPE, value, "PRINTER_TYPE"); }
        }

        private string _PRINTER_PROPERTY_01 = "";
        // Get or set Printer .
        public String PRINTER_PROPERTY_01
        {
            get { return _PRINTER_PROPERTY_01; }
            set { SetField(ref _PRINTER_PROPERTY_01, value, "PRINTER_PROPERTY_01"); }
        }

        private string _PRINTER_PROPERTY_02 = "";
        // Get or set Printer .
        public String PRINTER_PROPERTY_02
        {
            get { return _PRINTER_PROPERTY_02; }
            set { SetField(ref _PRINTER_PROPERTY_02, value, "PRINTER_PROPERTY_02"); }
        }

        private string _PRINTER_PROPERTY_03 = "";
        // Get or set Printer .
        public String PRINTER_PROPERTY_03
        {
            get { return _PRINTER_PROPERTY_03; }
            set { SetField(ref _PRINTER_PROPERTY_03, value, "PRINTER_PROPERTY_03"); }
        }

        private string _PRINTER_PROPERTY_04 = "";
        // Get or set Printer .
        public String PRINTER_PROPERTY_04
        {
            get { return _PRINTER_PROPERTY_04; }
            set { SetField(ref _PRINTER_PROPERTY_04, value, "PRINTER_PROPERTY_04"); }
        }

        private string _PRINTER_PROPERTY_05 = "";
        // Get or set Printer .
        public String PRINTER_PROPERTY_05
        {
            get { return _PRINTER_PROPERTY_05; }
            set { SetField(ref _PRINTER_PROPERTY_05, value, "PRINTER_PROPERTY_05"); }
        }

        private string _DESCRIPTION = "";
        // Get or set Printer .
        public String DESCRIPTION
        {
            get { return _DESCRIPTION; }
            set { SetField(ref _DESCRIPTION, value, "DESCRIPTION"); }
        }

        
    }
}
