/*************************************************************************************
 Created Date : 2023.02.22
      Creator : 
   Decription : RadioButton Extension
--------------------------------------------------------------------------------------
 [Change History]
  2023.02.22  조영대 : Initial Created. 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.CMM001.Controls
{
    public class UcBaseRadioButton : RadioButton
    {
        #region EventHandler
        public event CheckedChangedEventHandler CheckedChanged;
        /// <summary>
        /// Check 변경시 발생하는 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="isChecked">체크여부</param>
        /// <param name="e"></param>
        public delegate void CheckedChangedEventHandler(object sender, bool isChecked, RoutedEventArgs e);
        #endregion

        #region Declaration & Constructor 


        #region Property

        private bool isUserConfigUse = true;
        [Category("GMES"), DefaultValue(true), Description("사용자 설정 사용 유무")]
        public bool IsUserConfigUse
        {
            get { return isUserConfigUse; }
            set { isUserConfigUse = value; }
        }

        private bool isExistsUserConfig = false;
        [Category("GMES"), DefaultValue(true), Description("사용자 설정 존재 유무")]
        public bool IsExistsUserConfig
        {
            get { return isExistsUserConfig; }
            set { isExistsUserConfig = value; }
        }

        private List<RadioButton> radioButtonGroupList = null;
        [Category("GMES"), DefaultValue(null), Description("현재 컨트롤의 RadioButton Group의 목록")]
        public List<RadioButton> RadioButtonGroupList
        {
            get { return radioButtonGroupList; }
        }

        #endregion

        private Style originalStyle = null;

        private bool isEventProcess = false;

        private ContextMenu contextMenu = null;
        private ContextMenu saveContextMenu = null;
        private MenuItem mnuUserConfig = null;
        private bool savedChecked = false;

        private string topParentName = string.Empty;
        private string defaultGroupName = "Default Group";

        public UcBaseRadioButton()
        {

        }

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
                    this.Style = Application.Current.Resources["RadioButtonBaseStyle"] as Style;
                }

                originalStyle = this.Style;

                if (!Application.Current.Resources.Contains("UcBaseRadioButtonStyle"))
                {
                    ResourceDictionary resourceDic = new ResourceDictionary();
                    resourceDic.Source = new Uri(@"/LGC.GMES.MES.CMM001;component/Extensions/UcBaseStyle.xaml", UriKind.Relative);

                    Application.Current.Resources.MergedDictionaries.Insert(Application.Current.Resources.MergedDictionaries.Count - 3, resourceDic);
                }

                if (Application.Current.Resources.Contains("UcBaseRadioButtonStyle"))
                {
                    this.Style = Application.Current.Resources["UcBaseRadioButtonStyle"] as Style;
                    this.ApplyTemplate();
                }
            }

            Loaded += UcBaseRadioButton_Loaded;
            Unloaded += UcBaseRadioButton_Unloaded;

            isEventProcess = true;
        }

        #endregion

        #region Override

        protected override void OnClick()
        {
            if (!IsEnabled) return;

            base.OnClick();
        }

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
                        mnuUserConfig.Click += MnuUserConfig_Click;
                        contextMenu.Items.Add(mnuUserConfig);
                    }

                    if (ContextMenu != null && !ContextMenu.Equals(contextMenu)) saveContextMenu = ContextMenu;

                    mnuUserConfig.IsChecked = isExistsUserConfig;

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

            if (e.Property.Equals(IsEnabledProperty) && Visibility == Visibility.Visible)
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
                CheckedChanged?.Invoke(this, true, e);

                base.OnChecked(e);
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
                CheckedChanged?.Invoke(this, false, e);

                base.OnUnchecked(e);
            }
            else
            {
                e.Handled = true;
            }
        }

        #endregion

        #region Event

        private void UcBaseRadioButton_Loaded(object sender, RoutedEventArgs e)
        {
            savedChecked = IsChecked.Equals(true);

            if (isUserConfigUse)
            {
                topParentName = this.FindPageName();
                if (!string.IsNullOrEmpty(topParentName))
                {
                    GetUserConfig();

                    Loaded -= UcBaseRadioButton_Loaded;
                }
            }
        }

        private void UcBaseRadioButton_Unloaded(object sender, RoutedEventArgs e)
        {
            if (isUserConfigUse && isExistsUserConfig && IsChecked.Equals(true) && !IsChecked.Equals(savedChecked))
            {
                SaveUserConfig();
            }
        }

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

        public void SetCheckStatus(bool isChecked, bool isEvent = true)
        {
            if (isUserConfigUse && isExistsUserConfig) return;

            isEventProcess = isEvent;
            IsChecked = isChecked;
            isEventProcess = true;
        }

        public UcBaseRadioButton GetCheckedInGroup()
        {
            if (radioButtonGroupList == null)
            {
                object topPage = this.FindPageControl();
                if (topPage != null && topPage is IWorkArea)
                {
                    radioButtonGroupList = FindRadioGroup(topPage as DependencyObject, this.GroupName);
                }
            }

            if (radioButtonGroupList.Count == 0) return this;

            foreach (RadioButton radio in radioButtonGroupList)
            {
                if (radio is UcBaseRadioButton && radio.IsChecked.Equals(true)) return radio as UcBaseRadioButton;
            }
            return this;
        }

        #endregion

        #region Method

        private void GetUserConfig()
        {
            try
            {
                if (string.IsNullOrEmpty(topParentName)) return;
                if (string.IsNullOrEmpty(GroupName)) GroupName = "Default Group";

                if (radioButtonGroupList == null)
                {
                    object topPage = this.FindPageControl();
                    if (topPage != null && topPage is IWorkArea)
                    {
                        radioButtonGroupList = FindRadioGroup(topPage as DependencyObject, this.GroupName);
                    }
                }

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
                dr["CONF_TYPE"] = "USER_CONFIG_BASE_RADIOBUTTON";
                dr["CONF_KEY1"] = topParentName;
                dr["CONF_KEY2"] = GroupName;
                dr["CONF_KEY3"] = LoginInfo.CFG_MENUID;
                dtRqst.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SET_USER_CONF_INFO", "INDATA", "OUTDATA", dtRqst, noLogInputData: true, nologOutputData: true);
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    foreach (RadioButton radio in radioButtonGroupList)
                    {
                        if (radio is UcBaseRadioButton)
                        {
                            UcBaseRadioButton baseRadioButton = radio as UcBaseRadioButton;
                            baseRadioButton.isExistsUserConfig = true;
                        }

                        if (!dtResult.Rows[0]["USER_CONF01"].IsNvc() && dtResult.Rows[0]["USER_CONF01"].Equals(this.Name))
                        {
                            savedChecked = true;
                            isExistsUserConfig = true;

                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                if (this.IsEnabled)
                                {
                                    isEventProcess = false;
                                    IsChecked = true;
                                    isEventProcess = true;
                                }
                            }));
                        }
                        else
                        {
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                if (this.IsEnabled)
                                {
                                    isEventProcess = false;
                                    IsChecked = false;
                                    isEventProcess = true;
                                }
                            }));
                        }
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
                if (!string.IsNullOrEmpty(GroupName)) defaultGroupName = GroupName;

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
                drNew["CONF_TYPE"] = "USER_CONFIG_BASE_RADIOBUTTON";
                drNew["CONF_KEY1"] = topParentName;
                drNew["CONF_KEY2"] = defaultGroupName;
                drNew["CONF_KEY3"] = LoginInfo.CFG_MENUID;
                drNew["USER_CONF01"] = Name;
                dtRqst.Rows.Add(drNew);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SET_USER_CONF_INFO", "INDATA", "OUTDATA", dtRqst, noLogInputData: true, nologOutputData: true);

                savedChecked = IsChecked.Equals(true);
                foreach (RadioButton radio in radioButtonGroupList)
                {
                    if (radio is UcBaseRadioButton)
                    {
                        UcBaseRadioButton baseRadioButton = radio as UcBaseRadioButton;
                        baseRadioButton.isExistsUserConfig = true;
                    }
                }
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
                if (string.IsNullOrEmpty(GroupName)) GroupName = "Default Group";

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
                dr["CONF_TYPE"] = "USER_CONFIG_BASE_RADIOBUTTON";
                dr["CONF_KEY1"] = topParentName;
                dr["CONF_KEY2"] = GroupName;
                dr["CONF_KEY3"] = LoginInfo.CFG_MENUID;
                dtRqst.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SET_USER_CONF_INFO", "INDATA", "OUTDATA", dtRqst, noLogInputData: true, nologOutputData: true);
                if (dtResult != null && dtResult.Rows.Count == 1 && Util.NVC(dtResult.Rows[0]["USERID"]).Equals("OK"))
                {
                    foreach (RadioButton radio in radioButtonGroupList)
                    {
                        if (radio is UcBaseRadioButton)
                        {
                            UcBaseRadioButton baseRadioButton = radio as UcBaseRadioButton;
                            baseRadioButton.isExistsUserConfig = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 스킵
            }

        }

        private List<RadioButton> FindRadioGroup(DependencyObject parent, string groupName)
        {
            if (parent == null) return null;

            List<RadioButton> returnRadioButtons = new List<RadioButton>();

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is RadioButton)
                {
                    RadioButton childRadioButton = child as RadioButton;
                    if (childRadioButton != null && childRadioButton.GroupName.Equals(groupName))
                    {
                        returnRadioButtons.Add(childRadioButton);
                    }
                }
                else
                {
                    List<RadioButton> foundRadioButtons = FindRadioGroup(child, groupName);
                    if (foundRadioButtons != null && foundRadioButtons.Count > 0)
                    {
                        foreach (RadioButton radio in foundRadioButtons)
                        {
                            returnRadioButtons.Add(radio);
                        }
                    }
                }
            }

            return returnRadioButtons;
        }

        #endregion

    }
}