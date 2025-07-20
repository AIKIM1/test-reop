using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace LGC.GMES.MES.Common.ConfigInfos
{
    public class AppConfigInfo : INotifyPropertyChanged
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

        private string _ORGANIZATION_IDS = null;
        // Get or set the Organization IDs.
        public String ORGANIZATION_IDS
        {
            get { return _ORGANIZATION_IDS; }
            set { SetField(ref _ORGANIZATION_IDS, value, "ORGANIZATION_IDS"); }
        }

        private string _SCHEDULE_GROUP_ID = null;
        // Get or set the Schedule Group ID.
        public String SCHEDULE_GROUP_ID
        {
            get { return _SCHEDULE_GROUP_ID; }
            set { SetField(ref _SCHEDULE_GROUP_ID, value, "SCHEDULE_GROUP_ID"); }
        }

        private string _SCHEDULE_GROUP_NAME = null;
        // Get or set the Schedule Group NAME.
        public String SCHEDULE_GROUP_NAME
        {
            get { return _SCHEDULE_GROUP_NAME; }
            set { SetField(ref _SCHEDULE_GROUP_NAME, value, "SCHEDULE_GROUP_NAME"); }
        }

        private string _FACTORY_CODE = null;
        // Get or set the Factory Code.
        public String FACTORY_CODE
        {
            get { return _FACTORY_CODE; }
            set { SetField(ref _FACTORY_CODE, value, "FACTORY_CODE"); }
        }

        private string _FACILITY_CODE = null;
        // Get or set the Facility Code.
        public String FACILITY_CODE
        {
            get { return _FACILITY_CODE; }
            set { SetField(ref _FACILITY_CODE, value, "FACILITY_CODE"); }
        }

        private string _FACTORY_AREA_CODE = null;
        // Get or set the Factory Area Code.
        public String FACTORY_AREA_CODE
        {
            get { return _FACTORY_AREA_CODE; }
            set { SetField(ref _FACTORY_AREA_CODE, value, "FACTORY_AREA_CODE"); }
        }

        private string _EQUIPMENT_ID = null;
        // Get or set the Equipment ID.
        public String EQUIPMENT_ID
        {
            get { return _EQUIPMENT_ID; }
            set { SetField(ref _EQUIPMENT_ID, value, "EQUIPMENT_ID"); }
        }

        private string _EQUIP_SFC_ID = null;
        // Get or set the Equipment SFC ID.
        public String EQUIP_SFC_ID
        {
            get { return _EQUIP_SFC_ID; }
            set { SetField(ref _EQUIP_SFC_ID, value, "EQUIP_SFC_ID"); }
        }

        private string _DEFAULT_APP = "";

        // Get or set the Default Application.
        public String DEFAULT_APP
        {
            get { return _DEFAULT_APP; }
            set { SetField(ref _DEFAULT_APP, value, "DEFAULT_APP"); }
        }

        private string _ISUSE_MESSAGE = "Y";

        public String ISUSE_MESSAGE
        {
            get { return _ISUSE_MESSAGE; }
            set { SetField(ref _ISUSE_MESSAGE, value, "ISUSE_MESSAGE"); }
        }


        private string _ISUSE_MSG_AUTO_CLEAR = "N";
        // Get or set the Message Auto Clear Y/N.
        public String ISUSE_MSG_AUTO_CLEAR
        {
            get { return _ISUSE_MSG_AUTO_CLEAR; }
            set { SetField(ref _ISUSE_MSG_AUTO_CLEAR, value, "ISUSE_MSG_AUTO_CLEAR"); }
        }

        private string _MSG_AUTO_CLEAR_INTERVAL = "3000";
        // Get or set the Message Auto Clear Interval.
        public String MSG_AUTO_CLEAR_INTERVAL
        {
            get { return _MSG_AUTO_CLEAR_INTERVAL; }
            set { SetField(ref _MSG_AUTO_CLEAR_INTERVAL, value, "MSG_AUTO_CLEAR_INTERVAL"); }
        }

        private string _ISUSE_AUTO_LOGIN = "N";
        // Get or set Whether Auto Log In.
        public String ISUSE_AUTO_LOGIN
        {
            get { return _ISUSE_AUTO_LOGIN; }
            set { SetField(ref _ISUSE_AUTO_LOGIN, value, "ISUSE_AUTO_LOGIN"); }
        }

        private string _AUTO_LOGIN_ID = null;
        // Get or set Auto Log In Account ID.
        public String AUTO_LOGIN_ID
        {
            get { return _AUTO_LOGIN_ID; }
            set { SetField(ref _AUTO_LOGIN_ID, value, "AUTO_LOGIN_ID"); }
        }

        private string _AUTO_LOGIN_LANG_ID = "en-US";
        // Get or set Auto Log In Language Code ID.
        public String AUTO_LOGIN_LANG_ID
        {
            get { return _AUTO_LOGIN_LANG_ID; }
            set { SetField(ref _AUTO_LOGIN_LANG_ID, value, "AUTO_LOGIN_LANG_ID"); }
        }

        private string _CUSTOM_CONFIG_INFOS = null;
        // Get or set Custom Configurations.
        public String CUSTOM_CONFIG_INFOS
        {
            get { return _CUSTOM_CONFIG_INFOS; }
            set { SetField(ref _CUSTOM_CONFIG_INFOS, value, "CUSTOM_CONFIG_INFOS"); }
        }

        private string _ISUSE_FL = "N";
        // Get or set Whether F/L Use or not.
        public String ISUSE_FL
        {
            get { return _ISUSE_FL; }
            set { SetField(ref _ISUSE_FL, value, "ISUSE_FL"); }
        }

        private string _FL_CONNECTION_STRING = "TYPE=Host,BINDING=Tcp,NAME=GMES,ADDRESS=127.0.0.1,PORT=64000";
        // Get or set Whether F/L Use or not.
        public String FL_CONNECTION_STRING
        {
            get { return _FL_CONNECTION_STRING; }
            set { SetField(ref _FL_CONNECTION_STRING, value, "FL_CONNECTION_STRING"); }
        }

        private string _FL_ACK_CHECK = "N";
        public String FL_ACK_CHECK
        {
            get { return _FL_ACK_CHECK; }
            set { SetField(ref _FL_ACK_CHECK, value, "FL_ACK_CHECK"); }
        }

        private string _FL_ACK_INTERVAL = "3000";
        public String FL_ACK_INTERVAL
        {
            get { return _FL_ACK_INTERVAL; }
            set { SetField(ref _FL_ACK_INTERVAL, value, "FL_ACK_INTERVAL"); }
        }




        // Get or set whether Is WorkGuide enabled.
        private string _IS_ENABLE_WORKGUIDE = "N";
        public String IS_ENABLE_WORKGUIDE
        {
            get { return _IS_ENABLE_WORKGUIDE; }
            set { SetField(ref _IS_ENABLE_WORKGUIDE, value, "IS_ENABLE_WORKGUIDE"); }
        }

        // Get or set WorkGuide Type
        private string _WORKGUIDE_TYPE = "Right";
        public String WORKGUIDE_TYPE
        {
            get { return _WORKGUIDE_TYPE; }
            set { SetField(ref _WORKGUIDE_TYPE, value, "WORKGUIDE_TYPE"); }
        }

        // Get or set WorkGuide Margin
        private string _WORKGUIDE_MARGIN = "0,0,0,0";
        public String WORKGUIDE_MARGIN
        {
            get { return _WORKGUIDE_MARGIN; }
            set { SetField(ref _WORKGUIDE_MARGIN, value, "WORKGUIDE_MARGIN"); }
        }

        // Get or set WorkGuide Process
        private string _WORKGUIDE_PROCESS = string.Empty;
        public String WORKGUIDE_PROCESS
        {
            get { return _WORKGUIDE_PROCESS; }
            set { SetField(ref _WORKGUIDE_PROCESS, value, "WORKGUIDE_PROCESS"); }
        }

        private string _ISUSE_SOUND = "N";
        // Get or set Whether Sound use or not.
        public String ISUSE_SOUND
        {
            get { return _ISUSE_SOUND; }
            set { SetField(ref _ISUSE_SOUND, value, "ISUSE_SOUND"); }
        }

        private string _ADD_TITLE = "";
        // Get or set Add App. Title
        public String ADD_TITLE
        {
            get { return _ADD_TITLE; }
            set { SetField(ref _ADD_TITLE, value, "ADD_TITLE"); }
        }

        private string _ISALLOW_RESIZE = "Y";
        // Get or set Add App. Title
        public String ISALLOW_RESIZE
        {
            get { return _ISALLOW_RESIZE; }
            set { SetField(ref _ISALLOW_RESIZE, value, "ISALLOW_RESIZE"); }
        }

        private string _WIN_SIZE = null;
        // Get or set Add App. Title
        public String WIN_SIZE
        {
            get { return _WIN_SIZE; }
            set { SetField(ref _WIN_SIZE, value, "WIN_SIZE"); }
        }

        private string _ISUSE_LOGSERVER = "N";
        // Get or set Whether Log Server use or not.
        public String ISUSE_LOGSERVER
        {
            get { return _ISUSE_LOGSERVER; }
            set { SetField(ref _ISUSE_LOGSERVER, value, "ISUSE_LOGSERVER"); }
        }

        private string _LOG_LEVEL = "1";
        // Get or set Log Level
        public String LOG_LEVEL
        {
            get { return _LOG_LEVEL; }
            set { SetField(ref _LOG_LEVEL, value, "LOG_LEVEL"); }
        }

        private string _IS_TESTMODE = "N";
        // Get or set Whether Test Mode use or not.
        public String IS_TESTMODE
        {
            get { return _IS_TESTMODE; }
            set { SetField(ref _IS_TESTMODE, value, "IS_TESTMODE"); }
        }

        private string _IS_DATARECEIVE = "N";
        // Get or set Whether Signal Receive use or not.
        public String IS_DATARECEIVE
        {
            get { return _IS_DATARECEIVE; }
            set { SetField(ref _IS_DATARECEIVE, value, "IS_DATARECEIVE"); }
        }

        private string _IS_DATASEND = "N";
        // Get or set Whether Signal Send use or not.
        public String IS_DATASEND
        {
            get { return _IS_DATASEND; }
            set { SetField(ref _IS_DATASEND, value, "IS_DATASEND"); }
        }

        private string _DATASENDTARGET = "127.0.0.1";
        // Get or set Target IP
        public String DATASENDTARGET
        {
            get { return _DATASENDTARGET; }
            set { SetField(ref _DATASENDTARGET, value, "DATASENDTARGET"); }
        }

        private string _SPECIALCHARS = "";
        /// <summary>
        /// Scan Special Character
        /// </summary>
        public String SPECIALCHARS
        {
            get { return _SPECIALCHARS; }
            set { SetField(ref _SPECIALCHARS, value, "SPECIALCHARS"); }
        }

        // Get or set Scanner Sleep
        private String _SCANNER_SLEEP = "0.1";
        public String SCANNER_SLEEP
        {
            get { return _SCANNER_SLEEP; }
            set { SetField(ref _SCANNER_SLEEP, value, "SCANNER_SLEEP"); }
        }

        private List<ScanConfigInfo> _SCANNER_INFOS = new List<ScanConfigInfo>();

        public List<ScanConfigInfo> SCANNER_INFOS
        {
            get { return _SCANNER_INFOS; }
            set { SetField(ref _SCANNER_INFOS, value, "SCANNER_INFOS"); }
        }

        private List<PrinterConfigInfo> _PRINTER_INFOS = new List<PrinterConfigInfo>();

        public List<PrinterConfigInfo> PRINTER_INFOS
        {
            get { return _PRINTER_INFOS; }
            set { SetField(ref _PRINTER_INFOS, value, "PRINTER_INFOS"); }
        }
    }
}
