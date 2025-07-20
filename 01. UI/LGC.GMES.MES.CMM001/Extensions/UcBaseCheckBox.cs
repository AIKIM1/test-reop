/*************************************************************************************
 Created Date : 2023.02.22
      Creator : 
   Decription : CheckBox Extension
--------------------------------------------------------------------------------------
 [Change History]
  2023.02.22  조영대 : Initial Created. 
**************************************************************************************/

using System;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;


namespace LGC.GMES.MES.CMM001.Controls
{
    public class UcBaseCheckBox : System.Windows.Controls.CheckBox
    {

        #region EventHandler

        public class CheckedChangedEventArgs : EventArgs
        {
            public CheckedChangedEventArgs(bool oldValue, bool newValue)
            {
                OldValue = oldValue;
                NewValue = newValue;
            }

            public bool OldValue { get; set; }
            public bool NewValue { get; set; }
        }

        public event CheckedChangedEventHandler CheckedChanged;

        public delegate void CheckedChangedEventHandler(object sender, CheckedChangedEventArgs e);

        #endregion


        #region Property

        private bool isUserConfigUse = true;
        [Category("GMES"), DefaultValue(true), Description("사용자 설정 사용유무")]
        public bool IsUserConfigUse
        {
            get { return isUserConfigUse; }
            set { isUserConfigUse = value; }
        }

        #endregion


        #region Declaration & Constructor 

        private Style originalStyle = null;

        private bool isEventProcess = false;

        private ContextMenu contextMenu = null;
        private ContextMenu saveContextMenu = null;
        private MenuItem mnuUserConfig = null;
        private bool isExistsUserConfig = false;
        private bool savedChecked = false;

        private string topParentName = string.Empty;

        public UcBaseCheckBox() { }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            InitializeControls();
        }

        public void InitializeControls()
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            if (originalStyle == null)
            {
                if (this.Style == null)
                {
                    this.Style = Application.Current.Resources["CheckBoxBaseStyle"] as Style;
                }

                originalStyle = this.Style;

                if (!Application.Current.Resources.Contains("UcBaseCheckBoxStyle"))
                {
                    ResourceDictionary resourceDic = new ResourceDictionary();
                    resourceDic.Source = new Uri(@"/LGC.GMES.MES.CMM001;component/Extensions/UcBaseStyle.xaml", UriKind.Relative);

                    Application.Current.Resources.MergedDictionaries.Insert(Application.Current.Resources.MergedDictionaries.Count - 3, resourceDic);
                }

                if (Application.Current.Resources.Contains("UcBaseCheckBoxStyle"))
                {
                    this.Style = Application.Current.Resources["UcBaseCheckBoxStyle"] as Style;
                    this.ApplyTemplate();
                }
            }

            this.Loaded += UcBaseCheckBox_Loaded;
            this.Unloaded += UcBaseCheckBox_Unloaded;

            isEventProcess = true;
        }

        private void UcBaseCheckBox_Loaded(object sender, RoutedEventArgs e)
        {
            savedChecked = this.IsChecked.Equals(true);

            if (isUserConfigUse)
            {
                topParentName = this.FindPageName();
                if (!string.IsNullOrEmpty(topParentName))
                {
                    GetUserConfig();

                    this.Loaded -= UcBaseCheckBox_Loaded;
                }
            }
        }

        private void UcBaseCheckBox_Unloaded(object sender, RoutedEventArgs e)
        {
            if (isUserConfigUse && isExistsUserConfig)
            {
                SaveUserConfig();
            }
        }

        #endregion


        #region Override

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);

            try
            {
                if (isUserConfigUse && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    if (contextMenu == null)
                    {
                        contextMenu = new ContextMenu();

                        mnuUserConfig = new MenuItem();
                        mnuUserConfig.Name = "mnuUserConfig";
                        mnuUserConfig.Header = ObjectDic.Instance.GetObjectName("LAST_STAT_SAVE");
                        System.Windows.Controls.Image imgUserConfig = new System.Windows.Controls.Image();
                        imgUserConfig.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/LGC.GMES.MES.ControlsLibrary;Component/Images/LGC/ico-delete.png", UriKind.Relative));
                        imgUserConfig.Stretch = System.Windows.Media.Stretch.Fill;
                        mnuUserConfig.Icon = imgUserConfig;
                        mnuUserConfig.IsChecked = isExistsUserConfig;
                        mnuUserConfig.Click += MnuUserConfig_Click;
                        contextMenu.Items.Add(mnuUserConfig);
                    }

                    if (ContextMenu != null && !ContextMenu.Equals(contextMenu)) saveContextMenu = ContextMenu;

                    ContextMenu = contextMenu;
                }
                else
                {
                    ContextMenu = saveContextMenu;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property.Equals(IsEnabledProperty))
            {
                if (IsEnabled)
                {
                    Opacity = 1.0;
                }
                else
                {
                    Opacity = 0.5;
                }
            }
        }

        protected override void OnChecked(RoutedEventArgs e)
        {
            if (isEventProcess)
            {
                CheckedChanged?.Invoke(this, new CheckedChangedEventArgs(!this.IsChecked.Equals(true), this.IsChecked.Equals(true)));

                base.OnChecked(e);

                if (this.IsLoaded) savedChecked = true;
            }
            else
            {
                e.Handled = true;
            }
        }

        protected override void OnUnchecked(RoutedEventArgs e)
        {
            if (isEventProcess)
            {
                CheckedChanged?.Invoke(this, new CheckedChangedEventArgs(!this.IsChecked.Equals(true), this.IsChecked.Equals(true)));

                base.OnUnchecked(e);

                if (this.IsLoaded) savedChecked = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        #endregion


        #region Event

        private void MnuUserConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                mnuUserConfig.IsChecked = !mnuUserConfig.IsChecked;

                if (mnuUserConfig.IsChecked)
                {
                    SaveUserConfig();
                }
                else
                {
                    DeleteUserConfig();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion


        #region Public Method

        #endregion


        #region Method

        private void GetUserConfig()
        {
            try
            {
                if (string.IsNullOrEmpty(topParentName)) return;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("WRK_TYPE", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("CONF_TYPE", typeof(string));
                dtRqst.Columns.Add("CONF_KEY1", typeof(string));
                dtRqst.Columns.Add("CONF_KEY2", typeof(string));
                dtRqst.Columns.Add("CONF_KEY3", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["WRK_TYPE"] = "SELECT";
                dr["USERID"] = LoginInfo.USERID;
                dr["CONF_TYPE"] = "USER_CONFIG_BASE_CHECKBOX";
                dr["CONF_KEY1"] = topParentName;
                dr["CONF_KEY2"] = this.Name;
                dr["CONF_KEY3"] = LoginInfo.CFG_MENUID;
                dtRqst.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SET_USER_CONF_INFO", "INDATA", "OUTDATA", dtRqst, noLogInputData: true, nologOutputData: true);
                if (dtResult != null && dtResult.Rows.Count > 0)
                {

                    if (!dtResult.Rows[0]["USER_CONF01"].IsNvc() && dtResult.Rows[0]["USER_CONF01"].Equals("Y"))
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (this.IsEnabled) this.IsChecked = true;
                        }));

                        if (this.IsLoaded) savedChecked = true;
                        isExistsUserConfig = true;
                    }
                    else if (!dtResult.Rows[0]["USER_CONF01"].IsNvc() && dtResult.Rows[0]["USER_CONF01"].Equals("N"))
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (this.IsEnabled) this.IsChecked = false;
                        }));

                        if (this.IsLoaded) savedChecked = false;
                        isExistsUserConfig = true;
                    }
                }
            }
            catch (Exception ex)
            {
                // 스킵
                isEventProcess = true;
            }
        }

        private void SaveUserConfig()
        {
            try
            {
                if (string.IsNullOrEmpty(topParentName)) return;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("WRK_TYPE", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("CONF_TYPE", typeof(string));
                dtRqst.Columns.Add("CONF_KEY1", typeof(string));
                dtRqst.Columns.Add("CONF_KEY2", typeof(string));
                dtRqst.Columns.Add("CONF_KEY3", typeof(string));
                dtRqst.Columns.Add("USER_CONF01", typeof(string));
                dtRqst.Columns.Add("USER_CONF02", typeof(string));
                dtRqst.Columns.Add("USER_CONF03", typeof(string));
                dtRqst.Columns.Add("USER_CONF04", typeof(string));
                dtRqst.Columns.Add("USER_CONF05", typeof(string));
                dtRqst.Columns.Add("USER_CONF06", typeof(string));
                dtRqst.Columns.Add("USER_CONF07", typeof(string));
                dtRqst.Columns.Add("USER_CONF08", typeof(string));
                dtRqst.Columns.Add("USER_CONF09", typeof(string));
                dtRqst.Columns.Add("USER_CONF10", typeof(string));

                DataRow drNew = dtRqst.NewRow();
                drNew["WRK_TYPE"] = "SAVE";
                drNew["USERID"] = LoginInfo.USERID;
                drNew["CONF_TYPE"] = "USER_CONFIG_BASE_CHECKBOX";
                drNew["CONF_KEY1"] = topParentName;
                drNew["CONF_KEY2"] = this.Name;
                drNew["CONF_KEY3"] = LoginInfo.CFG_MENUID;
                drNew["USER_CONF01"] = savedChecked ? "Y" : "N";
                dtRqst.Rows.Add(drNew);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SET_USER_CONF_INFO", "INDATA", "OUTDATA", dtRqst, noLogInputData: true, nologOutputData: true);

                isExistsUserConfig = true;
            }
            catch (Exception ex)
            {
                // 스킵
            }
        }

        private void DeleteUserConfig()
        {
            try
            {
                if (string.IsNullOrEmpty(topParentName)) return;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("WRK_TYPE", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("CONF_TYPE", typeof(string));
                dtRqst.Columns.Add("CONF_KEY1", typeof(string));
                dtRqst.Columns.Add("CONF_KEY2", typeof(string));
                dtRqst.Columns.Add("CONF_KEY3", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["WRK_TYPE"] = "DELETE";
                dr["USERID"] = LoginInfo.USERID;
                dr["CONF_TYPE"] = "USER_CONFIG_BASE_CHECKBOX";
                dr["CONF_KEY1"] = topParentName;
                dr["CONF_KEY2"] = this.Name;
                dr["CONF_KEY3"] = LoginInfo.CFG_MENUID;
                dtRqst.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SET_USER_CONF_INFO", "INDATA", "OUTDATA", dtRqst, noLogInputData: true, nologOutputData: true);
                if (dtResult != null && dtResult.Rows.Count == 1 && Util.NVC(dtResult.Rows[0]["USERID"]).Equals("OK"))
                {
                    isExistsUserConfig = false;
                }
            }
            catch (Exception ex)
            {
                // 스킵
            }

        }

        #endregion

    }
}