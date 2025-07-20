using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.MainFrame
{
    [ContentProperty("ContentArea")]
    public class MainTabItemLayout : Control
    {
        internal event EventHandler BookmarkStatusChanged;

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(MainTabItemLayout), new PropertyMetadata(string.Empty));
        public string Title
        {
            get
            {
                return (string)GetValue(TitleProperty);
            }
            set
            {
                SetValue(TitleProperty, value); 
            }
        }

        public static readonly DependencyProperty TitleToolTipProperty = DependencyProperty.Register("TitleToolTip", typeof(string), typeof(MainTabItemLayout), new PropertyMetadata(string.Empty));
        public string TitleToolTip
        {
            get
            {
                return (string)GetValue(TitleToolTipProperty);
            }
            set
            {
                SetValue(TitleToolTipProperty, value);
            }
        }

        public static readonly DependencyProperty TitleDepth2Property = DependencyProperty.Register("TitleDepth2", typeof(string), typeof(MainTabItemLayout), new PropertyMetadata(string.Empty, TitleDepth2PropertyChanged));
        public string TitleDepth2
        {
            get
            {
                return (string)GetValue(TitleDepth2Property);
            }
            set
            {
                SetValue(TitleDepth2Property, value);
            }
        }
        private static void TitleDepth2PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MainTabItemLayout item = d as MainTabItemLayout;
            if (e.NewValue != null && !string.IsNullOrEmpty(e.NewValue.ToString()))
            {
                item.TitleDepth2Visibility = Visibility.Visible;
            }
            else
            {
                item.TitleDepth2Visibility = Visibility.Collapsed;
            }
        }

        public static readonly DependencyProperty TitleDepth2VisibilityProperty = DependencyProperty.Register("TitleDepth2Visibility", typeof(Visibility), typeof(MainTabItemLayout), new PropertyMetadata(Visibility.Collapsed));
        public Visibility TitleDepth2Visibility
        {
            get
            {
                return (Visibility)GetValue(TitleDepth2VisibilityProperty);
            }
            set
            {
                SetValue(TitleDepth2VisibilityProperty, value);
            }
        }

        public static readonly DependencyProperty TitleDepth3Property = DependencyProperty.Register("TitleDepth3", typeof(string), typeof(MainTabItemLayout), new PropertyMetadata(string.Empty, TitleDepth3PropertyChanged));
        public string TitleDepth3
        {
            get
            {
                return (string)GetValue(TitleDepth3Property);
            }
            set
            {
                SetValue(TitleDepth3Property, value);
            }
        }
        private static void TitleDepth3PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MainTabItemLayout item = d as MainTabItemLayout;
            if (e.NewValue != null && !string.IsNullOrEmpty(e.NewValue.ToString()))
            {
                item.TitleDepth3Visibility = Visibility.Visible;
            }
            else
            {
                item.TitleDepth3Visibility = Visibility.Collapsed;
            }
        }

        public static readonly DependencyProperty TitleDepth3VisibilityProperty = DependencyProperty.Register("TitleDepth3Visibility", typeof(Visibility), typeof(MainTabItemLayout), new PropertyMetadata(Visibility.Collapsed));
        public Visibility TitleDepth3Visibility
        {
            get
            {
                return (Visibility)GetValue(TitleDepth3VisibilityProperty);
            }
            set
            {
                SetValue(TitleDepth3VisibilityProperty, value);
            }
        }

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("Message", typeof(string), typeof(MainTabItemLayout), new PropertyMetadata(string.Empty, MessagePropertyChanged));
        public string Message
        {
            get
            {
                return (string)GetValue(MessageProperty);
            }
            set
            {
                SetValue(MessageProperty, value);
            }
        }
        private static void MessagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MainTabItemLayout item = d as MainTabItemLayout;
            if (e.NewValue != null && !string.IsNullOrEmpty(e.NewValue.ToString()))
            {
                item.MessageVisibility = Visibility.Visible;
            }
            else
            {
                item.MessageVisibility = Visibility.Collapsed;
            }
        }

        public static readonly DependencyProperty MessageVisibilityProperty = DependencyProperty.Register("MessageVisibility", typeof(Visibility), typeof(MainTabItemLayout), new PropertyMetadata(Visibility.Collapsed));
        public Visibility MessageVisibility
        {
            get
            {
                return (Visibility)GetValue(MessageVisibilityProperty);
            }
            set
            {
                SetValue(MessageVisibilityProperty, value);
            }
        }

        public static readonly DependencyProperty ContentAreaProperty = DependencyProperty.Register("ContentArea", typeof(object), typeof(MainTabItemLayout), new PropertyMetadata(null));

        private Image imgFavOff;
        private Image imgFavON;
        private LoadingIndicator loadingIndicator;
        public Button btnClose;

        public object ContentArea
        {
            get
            {
                return this.GetValue(ContentAreaProperty);
            }
            set
            {
                this.SetValue(ContentAreaProperty, value);
            }
        }

        public static readonly DependencyProperty IsSettingOpenProperty = DependencyProperty.Register("IsSettingOpen", typeof(bool), typeof(MainTabItemLayout), new PropertyMetadata(null));
        public bool IsSettingOpen
        {
            get
            {
                return (bool)GetValue(IsSettingOpenProperty);
            }
            set
            {
                SetValue(IsSettingOpenProperty, value);
            }
        }

        static MainTabItemLayout()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MainTabItemLayout), new FrameworkPropertyMetadata(typeof(MainTabItemLayout)));
        }

        public MainTabItemLayout()
        {
            this.Loaded += MainTabItemLayout_Loaded;
        }

        void MainTabItemLayout_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= MainTabItemLayout_Loaded;
            btnClose = GetTemplateChild("btnClose") as Button;
            if (btnClose != null)
            { 
                this.btnClose.Click -= btnClose_Click;
                this.btnClose.Click += btnClose_Click;
            }
            C1DropDown ddMonitoringOption = GetTemplateChild("ddMonitoringOption") as C1DropDown;
            if (ddMonitoringOption != null)
                ddMonitoringOption.MouseLeftButtonDown += C1DropDown_MouseLeftButtonDown;

            DataTable indataTable = new DataTable();
            indataTable.Columns.Add("LANGID", typeof(string));
            indataTable.Columns.Add("USERID", typeof(string));
            DataRow indataRow = indataTable.NewRow();
            indataRow["LANGID"] = LoginInfo.LANGID;
            indataRow["USERID"] = LoginInfo.USERID;
            indataTable.Rows.Add(indataRow);

            new ClientProxy().ExecuteService("COR_SEL_BOOKMARK_BY_USERID", "INDATA", "OUTDATA", indataTable, (result, ex) =>
                {
                    if (ex != null || imgFavON == null) 
                    {
                        return;
                    }

                    try
                    {
                        if ((from DataRow r in result.Rows where DataTableConverter.GetValue(DataContext, "MENUID").Equals(r["MENUID"]) select r).Count() > 0)
                        {
                            imgFavON.Visibility = Visibility.Visible;
                            imgFavOff.Visibility = Visibility.Collapsed;
                        }

                        imgFavON.MouseLeftButtonDown += imgFavON_MouseLeftButtonDown;
                        imgFavOff.MouseLeftButtonDown += imgFavOff_MouseLeftButtonDown;
                    }
                    catch (Exception handlingError)
                    {
                    }
                }
            );
        }

        private void C1DropDown_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        void imgFavOff_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;

            DataSet indataSet = new DataSet();
            DataTable userTable = indataSet.Tables.Add("INDATA");
            userTable.Columns.Add("USERID", typeof(string));
            userTable.Columns.Add("MENUID", typeof(string));
            userTable.Columns.Add("SORTSEQ", typeof(decimal));
            userTable.Columns.Add("INSUSER", typeof(string));
            userTable.Columns.Add("INSDTTM", typeof(DateTime));
            userTable.Columns.Add("UPDUSER", typeof(string));
            userTable.Columns.Add("UPDDTTM", typeof(DateTime));
            DataRow userRow = userTable.NewRow();
            userRow["USERID"] = LoginInfo.USERID;
            userRow["MENUID"] = DataTableConverter.GetValue(DataContext, "MENUID");
            userRow["SORTSEQ"] = 1;
            userRow["INSUSER"] = LoginInfo.USERID;
            userRow["INSDTTM"] = System.DateTime.Now;
            userRow["UPDUSER"] = LoginInfo.USERID;
            userRow["UPDDTTM"] = System.DateTime.Now;
            userTable.Rows.Add(userRow);

            new ClientProxy().ExecuteService_Multi("COR_INS_BOOKMARK_G", "INDATA", null, (result, ex) =>
                {
                    if (ex != null)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        return;
                    }

                    imgFavON.Visibility = Visibility.Visible;
                    imgFavOff.Visibility = Visibility.Collapsed;

                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (BookmarkStatusChanged != null)
                    {
                        BookmarkStatusChanged(this, null);
                    }

                }, indataSet);
        }

        void imgFavON_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;

            DataTable indataTable = new DataTable();
            indataTable.Columns.Add("USERID", typeof(string));
            indataTable.Columns.Add("MENUID", typeof(string));
            DataRow indataRow = indataTable.NewRow();
            indataRow["USERID"] = LoginInfo.USERID;
            indataRow["MENUID"] = DataTableConverter.GetValue(DataContext, "MENUID");
            indataTable.Rows.Add(indataRow);

            new ClientProxy().ExecuteService("COR_DEL_BOOKMARK", "INDATA", null, indataTable, (result, ex) =>
                {
                    if (ex != null)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        return;
                    }

                    imgFavON.Visibility = Visibility.Collapsed;
                    imgFavOff.Visibility = Visibility.Visible;

                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (BookmarkStatusChanged != null)
                    {
                        BookmarkStatusChanged(this, null);
                    }
                }
            );
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            imgFavOff = GetTemplateChild("imgFavOff") as Image;
            imgFavON = GetTemplateChild("imgFavON") as Image;
            btnClose = GetTemplateChild("btnClose") as Button;
            if (btnClose != null)
            {
                this.btnClose.Click -= btnClose_Click;
                this.btnClose.Click += btnClose_Click;
            }
            loadingIndicator = GetTemplateChild("loadingIndicator") as LoadingIndicator;
        }

        public void btnClose_Click(object sender, RoutedEventArgs e)
        {
            System.Collections.Generic.IList<System.Windows.FrameworkElement> ilist = this.GetAllParents();
            foreach (var item in ilist)
            {
                C1TabControl tab = item as C1TabControl;
                if (tab != null)
                {
                    tab.Items.Remove(this.Parent as C1.WPF.C1TabItem);
                }
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
