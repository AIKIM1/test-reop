/*************************************************************************************
 Created Date : 2023.02.06
      Creator : 
   Decription : Button Extension
--------------------------------------------------------------------------------------
 [Change History]
  2023.02.06  조영대 : Initial Created. 
**************************************************************************************/

using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.CMM001.Controls
{
    public class UcBaseButton : Button
    {
        #region EventHandler
        #endregion

        #region Declaration & Constructor 
        private bool isClickAllow = true;

        private Style originalStyle = null;
        private ContextMenu buttonMenu = null;
        private ContextMenu saveMenu = null;
        private string topParentName = string.Empty;
        private string authorityMethod = "MESSAGE";
        
        #region Property
        private bool isAuthorityUse = false;
        [Category("GMES"), DefaultValue(false), Description("기본 권한 사용 유무")]
        public bool IsAuthorityUse
        {
            get { return isAuthorityUse; }
            set { isAuthorityUse = value; }
        }

        private bool isUserConfigAuthoUse = true;
        [Category("GMES"), DefaultValue(true), Description("확장 권한 사용 유무")]
        public bool IsUserConfigAuthoUse
        {
            get { return isUserConfigAuthoUse; }
            set { isUserConfigAuthoUse = value; }
        }

        private bool isAdmin = false;
        [Category("GMES"), DefaultValue(false), Description("관리자 권한 유무")]
        public bool IsAdmin
        {
            get { return isAdmin; }
        }

        private bool isBaseStyle = true;
        [Category("GMES"), DefaultValue(true), Description("Style 사용 유무")]
        public bool IsBaseStyle
        {
            get { return isBaseStyle; }
            set { isBaseStyle = value; }
        }

        private TextWrapping textWrapping = TextWrapping.NoWrap;
        [Category("GMES"), DefaultValue(TextWrapping.NoWrap), Description("TextWrapping")]
        public TextWrapping TextWrapping
        {
            get
            {
                return (TextWrapping)GetValue(TextWrappingProperty);
            }
            set
            {
                textWrapping = value;

                SetValue(TextWrappingProperty, value);
            }
        }
        public static readonly DependencyProperty TextWrappingProperty =
            DependencyProperty.Register("TextWrapping", typeof(TextWrapping), typeof(UcBaseButton), new PropertyMetadata(TextWrapping.NoWrap, TextWrappingPropertyChangedCallback));

        private TextTrimming textTrimming = TextTrimming.CharacterEllipsis;
        public TextTrimming TextTrimming
        {
            get
            {
                return (TextTrimming)GetValue(TextTrimmingProperty);
            }
            set
            {
                SetValue(TextTrimmingProperty, value);
            }
        }
        public static readonly DependencyProperty TextTrimmingProperty =
            DependencyProperty.Register("TextTrimming", typeof(TextTrimming), typeof(UcBaseButton), new PropertyMetadata(TextTrimming.CharacterEllipsis, TextTrimmingPropertyChangedCallback));

        #endregion


        public UcBaseButton()
        {
            
        }

        public void InitializeControls()
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            try
            {
                if (isBaseStyle && originalStyle == null)
                {
                    if (this.Style == null)
                    {
                        this.Style = Application.Current.Resources["ButtonBaseStyle"] as Style;
                    }

                    originalStyle = this.Style;

                    var parent = this.Template.LoadContent();

                    #region 기존 정보 저장
                    Border borderSave = parent.FindChild<Border>("Border");
                    Border pressSave = parent.FindChild<Border>("PressedVisualElement");
                    Border disableSave = parent.FindChild<Border>("DisabledVisualElement");

                    ContentPresenter normalPre = null;
                    TextBlock normalTextSave = borderSave.FindChild<TextBlock>("");                    
                    if (normalTextSave == null)
                    {
                        normalPre = borderSave.FindChild<ContentPresenter>("");                        
                    }

                    ContentPresenter pressPre = null;
                    TextBlock pressTextSave = pressSave.FindChild<TextBlock>("");
                    if (pressTextSave == null)
                    {
                        pressPre = pressSave.FindChild<ContentPresenter>("");
                    }

                    ContentPresenter disablePre = null;
                    TextBlock disableTextSave = disableSave.FindChild<TextBlock>("");
                    if (disableTextSave == null)
                    {
                        disablePre = disableSave.FindChild<ContentPresenter>("");
                    }

                    if (normalPre == null && pressPre == null && disablePre == null) return;

                    System.Windows.Controls.Image imageSave = borderSave.FindChild<System.Windows.Controls.Image>("");
                    #endregion

                    #region Style 적용
                    if (!Application.Current.Resources.Contains("UcBaseButtonStyle"))
                    {
                        ResourceDictionary resourceDic = new ResourceDictionary();
                        resourceDic.Source = new Uri(@"/LGC.GMES.MES.CMM001;component/Extensions/UcBaseStyle.xaml", UriKind.Relative);

                        Application.Current.Resources.MergedDictionaries.Insert(Application.Current.Resources.MergedDictionaries.Count - 3, resourceDic);
                    }

                    if (Application.Current.Resources.Contains("UcBaseButtonStyle"))
                    {
                        this.Style = Application.Current.Resources["UcBaseButtonStyle"] as Style;
                        this.ApplyTemplate();
                    }
                    #endregion

                    #region 속성 복원
                    foreach (Setter setter in originalStyle.Setters)
                    {
                        switch (setter.Property.ToString())
                        {
                            case "FontSize":
                            case "FontFamily":
                                this.SetValue(setter.Property, setter.Value);
                                break;
                            case "Height":
                                if (this.TextWrapping == TextWrapping.NoWrap)
                                {
                                    this.SetValue(setter.Property, setter.Value);
                                }
                                break;
                        }
                    }

                    DependencyObject borderBase = this.GetTemplateChild("Border");
                    if (borderSave != null && borderBase != null)
                    {
                        borderBase.SetValue(BackgroundProperty, borderSave.Background);
                        borderBase.SetValue(BorderBrushProperty, borderSave.BorderBrush);
                    }
                    
                    DependencyObject pressBase = this.GetTemplateChild("PressedVisualElement");
                    if (pressSave != null && pressBase != null)
                    {
                        pressBase.SetValue(BackgroundProperty, pressSave.Background);
                        pressBase.SetValue(BorderBrushProperty, pressSave.BorderBrush);
                    }
                    DependencyObject disableBase = this.GetTemplateChild("DisabledVisualElement");
                    if (disableSave != null && disableBase != null)
                    {
                        disableBase.SetValue(BackgroundProperty, disableSave.Background);
                        disableBase.SetValue(BorderBrushProperty, disableSave.BorderBrush);
                    }

                    DependencyObject normalTextBase = this.GetTemplateChild("NormalTextBlock");
                    if (normalTextSave != null && normalTextBase != null)
                    {
                        normalTextBase.SetValue(ForegroundProperty, normalTextSave.Foreground);
                    }
                    else if (normalPre != null && normalTextBase != null)
                    {
                        normalTextBase.SetValue(ForegroundProperty, normalPre.ReadLocalValue(ForegroundProperty));
                    }
                    DependencyObject pressTextBase = this.GetTemplateChild("PressedTextBlock");
                    if (pressTextSave != null && pressTextBase != null)
                    {
                        pressTextBase.SetValue(ForegroundProperty, pressTextSave.Foreground);
                    }
                    else if (pressPre != null && pressTextBase != null)
                    {
                        pressTextBase.SetValue(ForegroundProperty, pressPre.ReadLocalValue(ForegroundProperty));
                    }
                    DependencyObject disableTextBase = this.GetTemplateChild("DisabledTextBlock");
                    if (disableTextSave != null && disableTextBase != null)
                    {
                        disableTextBase.SetValue(ForegroundProperty, disableTextSave.Foreground);
                    }
                    else if (disablePre != null && disableTextBase != null)
                    {
                        disableTextBase.SetValue(ForegroundProperty, disablePre.ReadLocalValue(ForegroundProperty));
                    }

                    if (imageSave != null)
                    {
                        DependencyObject borderObj = this.GetTemplateChild("BorderImage");
                        if (borderObj != null && borderObj is Image)
                        {
                            Image borderImage = borderObj as Image;
                            borderImage.Source = imageSave.Source;
                            borderImage.Visibility = Visibility.Visible;
                        }
                        DependencyObject pressObj = this.GetTemplateChild("PressedVisualElementImage");
                        if (pressObj != null && pressObj is Image)
                        {
                            Image pressImage = pressObj as Image;
                            pressImage.Source = imageSave.Source;
                            pressImage.Visibility = Visibility.Visible;
                        }
                        DependencyObject disableObj = this.GetTemplateChild("DisabledVisualElementImage");
                        if (disableObj != null && disableObj is Image)
                        {
                            Image disableImage = disableObj as Image;
                            disableImage.Source = imageSave.Source;
                            disableImage.Visibility = Visibility.Visible;
                        }
                    }
                    #endregion

                    this.InvalidateVisual();
                }

                this.Loaded += UcBaseButton_Loaded;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void UcBaseButton_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (isAuthorityUse)
                {
                    object topPage = this.FindPageControl();
                    if (topPage != null && topPage is IWorkArea)
                    {
                        IWorkArea workArea = topPage as IWorkArea;
                        if (workArea != null && workArea.FrameOperation != null)
                        {
                            this.IsEnabled = workArea.FrameOperation.AUTHORITY.Equals("W");
                        }
                    }
                }

                topParentName = this.FindPageName();
                if (isUserConfigAuthoUse && !string.IsNullOrEmpty(topParentName))
                {
                    AuthorityConfig();

                    this.Loaded -= UcBaseButton_Loaded;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Override

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            InitializeControls();
        }

        protected override void OnClick()
        {
            if (!isAdmin && !isClickAllow)
            {
                Util.MessageValidation("FM_ME_0183");
                return;
            }

            base.OnClick();
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);

            try
            {
                if (isUserConfigAuthoUse && isAdmin && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    if (buttonMenu == null)
                    {
                        buttonMenu = new ContextMenu();

                        MenuItem mnuUserConfig = new MenuItem();
                        mnuUserConfig.Name = "mnuAuthoConfig";
                        mnuUserConfig.Header = ObjectDic.Instance.GetObjectName("관리자권한");
                        System.Windows.Controls.Image imgUserConfig = new System.Windows.Controls.Image();
                        imgUserConfig.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/LGC.GMES.MES.ControlsLibrary;Component/Images/LGC/icon_login_bizconfig.png", UriKind.Relative));
                        imgUserConfig.Stretch = System.Windows.Media.Stretch.Fill;
                        mnuUserConfig.Icon = imgUserConfig;
                        mnuUserConfig.Click += MnuUserConfig_Click;
                        buttonMenu.Items.Add(mnuUserConfig);
                    }

                    if (ContextMenu != null && !ContextMenu.Equals(buttonMenu)) saveMenu = ContextMenu;

                    ContextMenu = buttonMenu;
                }
                else
                {
                    ContextMenu = saveMenu;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            try
            {
                if (newContent != null && newContent is string)
                {
                    if (textWrapping.Equals(TextWrapping.NoWrap))
                    {
                        this.ToolTip = newContent.ToString();
                    }
                    else
                    {
                        this.ToolTip = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Event
        private static void TextWrappingPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UcBaseButton ucBaseButton = d as UcBaseButton;
            ucBaseButton.textWrapping = (TextWrapping)e.NewValue;
            switch (ucBaseButton.textWrapping)
            {
                case TextWrapping.WrapWithOverflow:
                case TextWrapping.Wrap:
                    ucBaseButton.TextTrimming = TextTrimming.None;
                    ucBaseButton.Height = double.NaN;
                    break;
                case TextWrapping.NoWrap:
                    ucBaseButton.TextTrimming = TextTrimming.CharacterEllipsis;
                    break;
            }
            ucBaseButton.InvalidateVisual();
        }

        private static void TextTrimmingPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UcBaseButton ucBaseButton = d as UcBaseButton;
            ucBaseButton.textTrimming = (TextTrimming)e.NewValue;
            ucBaseButton.InvalidateVisual();
        }

        private void MnuUserConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CMM_AUTH_CONF popAuthConf = new CMM_AUTH_CONF();

                if (popAuthConf != null)
                {
                    object[] Parameters = new object[2];
                    Parameters[0] = this;
                    Parameters[1] = this.FindPageName();

                    ControlsLibrary.C1WindowExtension.SetParameters(popAuthConf, Parameters);

                    popAuthConf.Closed += new EventHandler(popAuthConf_Closed);
                    popAuthConf.ShowModal();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void popAuthConf_Closed(object sender, EventArgs e)
        {
            try
            {
                AuthorityConfig();
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
        private void AuthorityConfig()
        {
            try
            {
                if (string.IsNullOrEmpty(topParentName)) return;

                DataTable dtRqst = new DataTable("RQSTDT");
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("WORK_TYPE", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("CONF_KEY1", typeof(string));
                dtRqst.Columns.Add("CONF_KEY2", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["WORK_TYPE"] = "CHECK";
                dr["USERID"] = LoginInfo.USERID;
                dr["CONF_KEY1"] = topParentName;
                dr["CONF_KEY2"] = this.Name;
                dtRqst.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_GET_AUTHORITY_INFO", "RQSTDT", "RSLTDT", dtRqst, noLogInputData: true, nologOutputData: true);
                if (dtResult != null && dtResult.Rows.Count > 0)
                {                    
                    List<DataRow> drList = dtResult.AsEnumerable().Where(w => w.Field<string>("AUTH_TYPE") != null && w.Field<string>("AUTH_TYPE").Equals("Y")).ToList();
                    if (drList != null && drList.Count > 0)
                    {
                        isClickAllow = true;
                        drList = dtResult.AsEnumerable().Where(w => w.Field<string>("AUTHID").Equals("MESADMIN")).ToList();
                        if (drList != null && drList.Count > 0) isAdmin = true;
                    }
                    else
                    {
                        drList = dtResult.AsEnumerable().Where(w => w.Field<string>("AUTH_TYPE") != null && w.Field<string>("AUTH_TYPE").Equals("N")).ToList();
                        if (drList != null && drList.Count > 0)
                        {
                            isClickAllow = false;

                            if (dtResult != null && dtResult.Columns.Contains("PROCESS_MTH"))
                            {
                                authorityMethod = drList[0]["PROCESS_MTH"].Nvc();

                                switch (authorityMethod)
                                {
                                    case "DISABLE":
                                        this.IsEnabled = false;
                                        break;
                                    case "VISIBLE":
                                        this.Visibility = Visibility.Collapsed;
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 처리 없음. 스킵
            }
        }
        #endregion

    }
}
