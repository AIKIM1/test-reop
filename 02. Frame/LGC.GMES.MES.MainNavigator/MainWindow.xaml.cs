using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.MainNavigator
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        DataSet configSet = new DataSet();
        private Nullable<DateTime> clickTime = new DateTime();

        public MainWindow()
        {
            //Process[] processes = Process.GetProcesses();
            //foreach (Process p in processes)
            //{
            //    if (p.ProcessName.ToLower().Contains("werfault"))
            //    {
            //        p.Kill();
            //    }
            //}
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= new RoutedEventHandler(MainWindow_Loaded);
            SetLangCombo();
            Initialize();
        }

        private void btnNavigator_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.clickTime != null)
                {
                    if (DateTime.Now.Subtract(this.clickTime.Value).TotalMilliseconds > 500)
                    {
                        Button btn = sender as Button;
                        string systemid = string.Empty;
                        string param = "SYS:" + Convert.ToString(btn.DataContext) + ";" + "U:" + LoginInfo.USERID;
                        string filename = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + ConfigurationManager.AppSettings["STARTAPP"];
                        Process.Start(filename, param);
                    }

                    this.clickTime = DateTime.Now;
                }
                else
                {
                    this.clickTime = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnNaviGR_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                spButton1.Children.Clear();

                Button btn = sender as Button;
                foreach (DataRow dr in configSet.Tables["SHOP"].Rows)
                {
                    string sysid = Convert.ToString(dr["SYSID"]).Substring(5, 1);
                    if(sysid == Convert.ToString(btn.DataContext))
                    {
                        Button btn1 = new Button();
                        btn1.Content = Convert.ToString(GetObjectName(LoginInfo.LANGID, Convert.ToString(dr["SHOPLIST"])));
                        btn1.DataContext = Convert.ToString(dr["SYSID"]);
                        btn1.Style = Resources["Navigator_Button_Style"] as Style;
                        btn1.Click += btnNavigator_Click;

                        spButton1.Children.Add(btn1);
                    }
                }
                
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void Initialize()
        {
            //SetMainNavigatorConfg();
            GetMainNavigatorConfig();

            txtUserID.Text = LoginInfo.USERID;
            txtTitle.Text = Convert.ToString(GetObjectName(LoginInfo.LANGID, Convert.ToString(configSet.Tables["TITLE"].Rows[0]["TITLE"])));
            spButton.Children.Clear();
            spButton1.Children.Clear();

            foreach (DataRow dr1 in configSet.Tables["SYSGR"].Rows)
            {
                Button btn = new Button();
                btn.Content = Convert.ToString(GetObjectName(LoginInfo.LANGID, Convert.ToString(dr1["SYSGRNAME"])));
                btn.DataContext = Convert.ToString(dr1["SYSGRID"]);
                btn.Style = Resources["Navigator_Button_Style"] as Style;
                btn.Click += btnNaviGR_Click;

                spButton.Children.Add(btn);
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private object GetObjectName(string langid, string strShopName)
        {
            try
            {
                string[] tmp01 = strShopName.Split('|');

                for (int idx = 0; idx < tmp01.Length; idx++)
                {
                    string[] tmp02 = tmp01[idx].Split('\\');

                    if (langid == tmp02[0])
                        return tmp02[1];
                }

                return null;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        private void SetLangCombo()
        {
            DataTable dtResult = new DataTable();

            dtResult.Columns.Add("CBO_CODE", typeof(string));
            dtResult.Columns.Add("CBO_NAME", typeof(string));

            DataRow newRow = dtResult.NewRow();

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "ko-KR", "한국어" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "en-US", "English " };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "zh-CN", "中文" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "pl-PL", "Polski" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "uk-UA", "Українська" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "ru-RU", "Русский" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "id-ID", "Bahasa Indonesia" };
            dtResult.Rows.Add(newRow);

            cboLang.DisplayMemberPath = "CBO_NAME";
            cboLang.SelectedValuePath = "CBO_CODE";
            cboLang.ItemsSource = DataTableConverter.Convert(dtResult);

            CultureInfo ci = CultureInfo.InstalledUICulture;

            switch (ci.Name)
            {
                case "en-US":
                case "pl-PL":
                case "zh-CN":
                case "uk-UA":
                case "ru-RU":
                case "id-ID":
                    LoginInfo.LANGID = ci.Name;
                    break;

                default:
                    LoginInfo.LANGID = "ko-KR";
                    break;
            }

            cboLang.SelectedValue = LoginInfo.LANGID;
            cboLang.SelectedValueChanged += cboLang_SelectedValueChanged;
        }

        private void cboLang_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            LoginInfo.LANGID = Convert.ToString(cboLang.SelectedValue);
            Initialize();
        }

        private void SetMainNavigatorConfig()
        {
            DataSet dsConfig = new DataSet();

            //==================================================================================================
            //< TITLE >
            //< TITLE > ko-KR\오창공장|en-US\Ochang Factory|zh-CN\梧仓工厂|pl-PL\Ochang Fabryka|uk-UA\Ochang Factory|ru-RU\Ochang Factory|id-ID\Ochang Factory</ TITLE >
            //</ TITLE >

            DataTable dtTitle = new DataTable("TITLE");
            dtTitle.Columns.Add("TITLE", typeof(string));

            DataRow drtitlerow = dtTitle.NewRow();
            //drtitlerow["TITLE"] = @"ko-KR\오창공장|en-US\Ochang Factory|zh-CN\Ochang Factory|pl-pl\Ochang Factory|uk-UA\Ochang Factory";

            drtitlerow["TITLE"] = @"ko-KR\HD공장|en-US\Honda Factory|zh-CN\梧仓工厂|pl-PL\Honda Fabryka|uk-UA\Honda фабрика|ru-RU\Honda фабрика|id-ID\Tanaman Honda";
            //drtitlerow["TITLE"] = @"ko-KR\남경공장|en-US\Nanjing Factory|zh-CN\南京工厂|pl-PL\Nanjing Fabryka|uk-UA\Nanjing фабрика|ru-RU\Nanjing фабрика";
            //drtitlerow["TITLE"] = @"ko-KR\ESNA-공장|en-US\ESNA Factory|zh-CN\ESNA Factory|pl-PL\ESNA Fabryka|uk-UA\ESNA фабрика|ru-RU\ESNA фабрика";
            //drtitlerow["TITLE"] = @"ko-KR\ESNB 공장|en-US\ESNB Factory|zh-CN\ESNB 工厂|pl-PL\ESNB Fabryka|uk-UA\ESNB фабрика|ru-RU\ESNB фабрика";
            //drtitlerow["TITLE"] = @"ko-KR\ESMI 공장|en-US\ESMI Factory|zh-CN\美国工厂|pl-PL\ESMI Fabryka|uk-UA\ESMI фабрика|ru-RU\ESMI фабрика";
            //drtitlerow["TITLE"] = @"ko-KR\Ultium Cells 공장|en-US\Ultium Cells Factory|zh-CN\Ultium Cells 工厂|pl-PL\Ultium Cells Fabryka|uk-UA\Ultium Cells фабрика|ru-RU\Ultium Cells фабрика";
            //drtitlerow["TITLE"] = @"ko-KR\폴란드 공장|en-US\Poland Factory|zh-CN\波兰工厂|pl-PL\Polska Fabryka|uk-UA\Польща фабрика|ru-RU\Польская фабрика";
            
            dtTitle.Rows.Add(drtitlerow);

            dsConfig.Tables.Add(dtTitle);

            //==================================================================================================
            //<SYSGR>
            //<SYSGRNAME>ko-KR\전극|en-US\Elec|zh-CN\电极|pl-pl\Elec|uk-UA\Elec|ru-RU\Электрик|id-ID\Elektroda|</SYSGRNAME>
            //<SYSGRID>E</ SYSGRID >
            //</SYSGR>
            //<SYSGR>
            //<SYSGRNAME>ko-KR\조립|en-US\Assembly|zh-CN\Assembly|pl-pl\Assembly|uk-UA\Assembly|ru-RU\Assembly|id-ID\Assembly|</SYSGRNAME>
            //<SYSGRID>A</SYSGRID>
            //</SYSGR>
            //<SYSGR>
            //<SYSGRNAME>ko-KR\활성화|en-US\Form|zh-CN\Form|pl-pl\Form|uk-UA\Form|ru-RU\Form|id-ID\Form|</SYSGRNAME>
            //<SYSGRID>F</SYSGRID>
            //</SYSGR>
            //<SYSGR>
            //<SYSGRNAME>ko-KR\Pack|en-US\Pack|zh-CN\Pack|pl-pl\Pack|uk-UA\Pack|ru-RU\Pack|id-ID\Pack</SYSGRNAME>
            //<SYSGRID>P</SYSGRID>
            //</SYSGR>

            DataTable dtSYSGR = new DataTable("SYSGR");
            dtSYSGR.Columns.Add("SYSGRNAME", typeof(string));
            dtSYSGR.Columns.Add("SYSGRID", typeof(string));

            DataRow drSysgrrow = dtTitle.NewRow();
            drSysgrrow["SYSGRNAME"] = @"ko-KR\전극|en-US\Elec|zh-CN\电极|pl-pl\Elec|uk-UA\Elec|ru-RU\Электрик|id-ID\Elektroda|";
            drSysgrrow["SYSGRID"] = @"E";
            dtSYSGR.Rows.Add(drSysgrrow);

            drSysgrrow = dtTitle.NewRow();
            drSysgrrow["SYSGRNAME"] = @"ko-KR\소형 조립|en-US\Assy Mobile|zh-CN\Assy Mobile|pl-pl\Assy Mobile|uk-UA\Assy Mobile|ru-RU\Assy Mobile|id-ID\Assy Mobile";
            drSysgrrow["SYSGRID"] = @"S";
            dtSYSGR.Rows.Add(drSysgrrow);

            drSysgrrow = dtTitle.NewRow();
            drSysgrrow["SYSGRNAME"] = @"ko-KR\조립|en-US\Assy Auto|zh-CN\Assy Auto|pl-pl\Assy Auto|uk-UA\Assy Auto|ru-RU\Assy Auto|id-ID\Assy Auto";
            drSysgrrow["SYSGRID"] = @"A";
            dtSYSGR.Rows.Add(drSysgrrow);

            drSysgrrow = dtTitle.NewRow();
            drSysgrrow["SYSGRNAME"] = @"ko-KR\활성화|en-US\Form|zh-CN\Form|pl-pl\Form|uk-UA\Form|ru-RU\Form|id-ID\Form|";
            drSysgrrow["SYSGRID"] = @"F";
            dtSYSGR.Rows.Add(drSysgrrow);

            drSysgrrow = dtTitle.NewRow();
            drSysgrrow["SYSGRNAME"] = @"ko-KR\Pack|en-US\Pack|zh-CN\Pack|pl-pl\Pack|uk-UA\Pack|ru-RU\Pack|id-ID\Pack|";
            drSysgrrow["SYSGRID"] = @"P";
            dtSYSGR.Rows.Add(drSysgrrow);

            dsConfig.Tables.Add(dtSYSGR);

            //==================================================================================================
            //<SHOP >
            //<SHOPLIST > ko - KR\오창 전극[GMES - E - KR]| en - US\Ochang Elec[GMES - E - KR]| zh - CN\梧仓 电极[GMES - E - KR]| pl - PL\Ochang Elec[GMES - E - KR]| uk - UA\Ochang Elec[GMES - E - KR]| ru - RU\Ochang Elec[GMES - E - KR]</ SHOPLIST >
            //<SYSID > GMES - E - KR </ SYSID >
            //</SHOP >

            DataTable dtShop = new DataTable("SHOP");
            dtShop.Columns.Add("SHOPLIST", typeof(string));
            dtShop.Columns.Add("SYSID", typeof(string));

            #region OC

            //DataRow dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\오창 전극 [GMES-E-KR]|en-US\Ochang Elec [GMES-E-KR]|zh-CN\梧仓 电极 [GMES-E-KR]|pl-pl\Ochang Elec [GMES-E-KR]|uk-UA\Ochang Elec [GMES-E-KR]|ru-RU\Ochang Elec [GMES-E-KR]|id-ID\Ochang Elec [GMES-E-KR]";
            //dtShoprow["SYSID"] = "GMES-E-KR";
            //dtShop.Rows.Add(dtShoprow);

            //dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\오창 소형 조립 [GMES-S-KR]|en-US\Ochang Assy Mobile [GMES-S-KR]|zh-CN\梧仓 组装 小型 [GMES-S-KR]|pl-pl\Ochang Assy Mobile [GMES-S-KR]|uk-UA\Ochang Assy Mobile [GMES-S-KR]|ru-RU\Ochang Assy Mobile [GMES-S-KR]|id-ID\Ochang Assy Mobile [GMES-S-KR]";
            //dtShoprow["SYSID"] = "GMES-S-KR";
            //dtShop.Rows.Add(dtShoprow);

            //dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\오창 자동차 조립 [GMES-A-KR]|en-US\Ochang Assy Auto [GMES-A-KR]|zh-CN\梧仓 组装 汽车 [GMES-A-KR]|pl-pl\Ochang Assy Auto [GMES-A-KR]|uk-UA\Ochang Assy Auto [GMES-A-KR]|ru-RU\Ochang Assy Auto [GMES-A-KR]|id-ID\Ochang Assy Auto [GMES-A-KR]";
            //dtShoprow["SYSID"] = "GMES-A-KR";
            //dtShop.Rows.Add(dtShoprow);

            //dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\오창 Pack [GMES-P-KR]|en-US\Ochang Pack [GMES-P-KR]|zh-CN\梧仓 Pack [GMES-P-KR]|pl-pl\Ochang Pack [GMES-P-KR]|uk-UA\Ochang Pack [GMES-P-KR]|ru-RU\Ochang Pack [GMES-P-KR]|id-ID\Ochang Pack [GMES-P-KR]";
            //dtShoprow["SYSID"] = "GMES-P-KR";
            //dtShop.Rows.Add(dtShoprow);

            //dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\오창(2공장) 전극 [GMES-E-K2]|en-US\Ochang 2 Elec [GMES-E-K2]|zh-CN\梧仓 电极 [GMES-E-K2]|pl-pl\Ochang 2 Elec [GMES-E-K2]|uk-UA\Ochang 2 Elec [GMES-E-K2]|ru-RU\Ochang 2 Elec [GMES-E-K2]|id-ID\Ochang 2 Elec [GMES-E-K2]";
            //dtShoprow["SYSID"] = "GMES-E-K2";
            //dtShop.Rows.Add(dtShoprow);

            //dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\오창(2공장) 소형 조립 [GMES-S-K2]|en-US\Ochang 2 Assy Mobile [GMES-S-K2]|zh-CN\梧仓 组装 小型 [GMES-S-K2]|pl-pl\Ochang 2 Assy Mobile [GMES-S-K2]|uk-UA\Ochang 2 Assy Mobile [GMES-S-K2]|ru-RU\Ochang 2 Assy Mobile [GMES-S-K2]|id-ID\Ochang 2 Assy Mobile [GMES-S-K2]";
            //dtShoprow["SYSID"] = "GMES-S-K2";
            //dtShop.Rows.Add(dtShoprow);

            //dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\오창(IT3 동) 자동차 활성화[GMES-F-KR_I3]|en-US\Ochang(IT 3) Form [GMES-F-KR_I3]|zh-CN\梧仓 Form [GMES-F-KR_I3]|pl-pl\Ochang(IT 3) Form [GMES-F-KR_I3]|uk-UA\Ochang(IT 3) Form [GMES-F-KR_I3]|ru-RU\Ochang(IT 3) Form [GMES-F-KR_I3]|id-ID\Ochang(IT 3) Form [GMES-F-KR_I3]";
            //dtShoprow["SYSID"] = "GMES-F-KR_I3";
            //dtShop.Rows.Add(dtShoprow);

            #endregion OC

            #region NJ

            //DataRow dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\ESNJ Elec [GMES-E-NJ]|en-US\ESNJ Elec [GMES-E-NJ]|zh-CN\ESNJ Elec [GMES-E-NJ]|pl-PL\ESNJ Elec [GMES-E-NJ]|uk-UA\ESNJ Elec [GMES-E-NJ]|ru-RU\ESNJ Elec [GMES-E-NJ]|id-ID\ESNJ Elec [GMES-E-NJ]";
            //dtShoprow["SYSID"] = "GMES-E-NJ";
            //dtShop.Rows.Add(dtShoprow);

            //dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\ESNJ Assy Bldg 23 [GMES-S-N23]|en-US\ESNJ Assy Bldg 23 [GMES-S-N23]|zh-CN\ESNJ Assy Bldg 23 [GMES-S-N23]|pl-PL\ESNJ Assy Bldg 23 [GMES-S-N23]|uk-UA\ESNJ Assy Bldg 23 [GMES-S-N23]|ru-RU\ESNJ Assy Bldg 23 [GMES-S-N23]|id-ID\ESNJ Assy Bldg 23 [GMES-S-N23]";
            //dtShoprow["SYSID"] = "GMES-S-N23";
            //dtShop.Rows.Add(dtShoprow);

            //dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\ESNJ Assy Bldg 4 [GMES-S-N4]|en-US\ESNJ Assy Bldg 4 [GMES-S-N4]|zh-CN\ESNJ Assy Bldg 4 [GMES-S-N4] |pl-PL\ESNJ Assy Bldg 4 [GMES-S-N4]|uk-UA\ESNJ Assy Bldg 4 [GMES-S-N4]|ru-RU\ESNJ Assy Bldg 4 [GMES-S-N4]|id-ID\ESNJ Assy Bldg 4 [GMES-S-N4]";
            //dtShoprow["SYSID"] = "GMES-S-N4";
            //dtShop.Rows.Add(dtShoprow);

            //dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\ESNJ Assy Bldg 5 [GMES-S-N5]|en-US\ESNJ Assy Bldg 5 [GMES-S-N5]|zh-CN\ESNJ Assy Bldg 5 [GMES-S-N5]|pl-PL\ESNJ Assy Bldg 5 [GMES-S-N5]|uk-UA\ESNJ Assy Bldg 5 [GMES-S-N5]|ru-RU\ESNJ Assy Bldg 5 [GMES-S-N5]|id-ID\ESNJ Assy Bldg 5 [GMES-S-N5]";
            //dtShoprow["SYSID"] = "GMES-S-N5";
            //dtShop.Rows.Add(dtShoprow);

            //dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\ESNJ Assy Bldg 6 [GMES-S-N6_2D] |en-US\ESNJ Assy Bldg 6 [GMES-S-N6_2D] |zh-CN\ESNJ Assy Bldg 6 [GMES-S-N6_2D]|pl-PL\ESNJ Assy Bldg 6 [GMES-S-N6_2D]|uk-UA\ESNJ Assy Bldg 6 [GMES-S-N6_2D]|ru-RU\ESNJ Assy Bldg 6 [GMES-S-N6_2D]|id-ID\ESNJ Assy Bldg 6 [GMES-S-N6_2D]";
            //dtShoprow["SYSID"] = "GMES-S-N6_2D";
            //dtShop.Rows.Add(dtShoprow);

            //dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\ESNJ Assy Bldg 6 [GMES-S-N6_C] |en-US\ESNJ Assy Bldg 6 [GMES-S-N6_C] |zh-CN\ESNJ Assy Bldg 6 [GMES-S-N6_C]|pl-PL\ESNJ Assy Bldg 6 [GMES-S-N6_C]|uk-UA\ESNJ Assy Bldg 6 [GMES-S-N6_C]|ru-RU\ESNJ Assy Bldg 6 [GMES-S-N6_C]|id-ID\ESNJ Assy Bldg 6 [GMES-S-N6_C]";
            //dtShoprow["SYSID"] = "GMES-S-N6_C";
            //dtShop.Rows.Add(dtShoprow);

            //dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\ESNJ PACK 3 [GMES-P-NJ]|en-US\ESNJ PACK Bldg 3 [GMES-P-NJ]|zh-CN\ESNJ PACK Bldg 3 [GMES-P-NJ]|pl-PL\ESNJ PACK Bldg 3 [GMES-P-NJ]|uk-UA\ESNJ PACK Bldg 3 [GMES-P-NJ]|ru-RU\ESNJ PACK Bldg 3 [GMES-P-NJ]|id-ID\ESNJ PACK Bldg 3 [GMES-P-NJ]";
            //dtShoprow["SYSID"] = "GMES-P-NJ";
            //dtShop.Rows.Add(dtShoprow);

            #endregion NJ

            #region NA

            //DataRow dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\ESNA 전극 [GMES-E-NA]|en-US\ESNA Electrode [GMES-E-NA]|zh-CN\ESNA Electrode [GMES-E-NA]|pl-PL\ESNA Electrode [GMES-E-NA]|uk-UA\ESNA Electrode [GMES-E-NA]|ru-RU\ESNA Electrode [GMES-E-NA]|id-ID\ESNA Electrode [GMES-E-NA]";
            //dtShoprow["SYSID"] = "GMES-E-NA";
            //dtShop.Rows.Add(dtShoprow);

            //dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\ESNA 조립 [GMES-A-NA]|en-US\ESNA Assy [GMES-A-NA]|zh-CN\ESNA Assy [GMES-A-NA]|pl-PL\ESNA Assy [GMES-A-NA]|uk-UA\ESNA Assy [GMES-A-NA]|ru-RU\ESNA Assy [GMES-A-NA]|id-ID\ESNA Assy [GMES-A-NA]";
            //dtShoprow["SYSID"] = "GMES-A-NA";
            //dtShop.Rows.Add(dtShoprow);

            //dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\ESNA Pack [GMES-P-NA]|en-US\ESNA Pack [GMES-P-NA]|zh-CN\ESNA Pack [GMES-P-NA]|pl-PL\ESNA Pack [GMES-P-NA]|uk-UA\ESNA Pack [GMES-P-NA]|ru-RU\ESNA Pack [GMES-P-NA]|id-ID\ESNA Pack [GMES-P-NA]";
            //dtShoprow["SYSID"] = "GMES-P-NA";
            //dtShop.Rows.Add(dtShoprow);

            #endregion NA

            #region NB

            //DataRow dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\ESNB 전극 [GMES-E-NB]|en-US\ESNB Elec [GMES-E-NB]|zh-CN\ESNB 电极 [GMES-E-NB]|pl-PL\ESNB Elec [GMES-E-NB]|uk-UA\ESNB Elec [GMES-E-NB]|ru-RU\ESNB Elec [GMES-E-NB]|id-ID\ESNB Elec [GMES-E-NB]";
            //dtShoprow["SYSID"] = "GMES-E-NB";
            //dtShop.Rows.Add(dtShoprow);

            //dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\ESNB 조립 1동 [GMES-A-NB]|en-US\ESNB Assy 1 [GMES-A-NB]|zh-CN\ESNB 组装 1 [GMES-A-NB]|pl-PL\ESNB Assy 1 [GMES-A-NB]|uk-UA\ESNB Assy 1 [GMES-A-NB]|ru-RU\ESNB Assy 1 [GMES-A-NB]|id-ID\ESNB Assy 1 [GMES-A-NB]";
            //dtShoprow["SYSID"] = "GMES-A-NB";
            //dtShop.Rows.Add(dtShoprow);

            //dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\ESNB 조립 2동 [GMES-A-N2]|en-US\ESNB Assy 2 [GMES-A-N2]|zh-CN\ESNB 组装 2 [GMES-A-N2]|pl-PL\ESNB Assy 2 [GMES-A-N2]|uk-UA\ESNB Assy 2 [GMES-A-N2]|ru-RU\ESNB Assy 2 [GMES-A-N2]|id-ID\ESNB Assy 2 [GMES-A-N2]";
            //dtShoprow["SYSID"] = "GMES-A-N2";
            //dtShop.Rows.Add(dtShoprow);

            //dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\ESNB 활성화 2동 [GMES-F-N2]|en-US\ESNB Form 2 [GMES-F-N2]|zh-CN\ESNB Form 2 [GMES-F-N2]|pl-PL\EESNB Form 2 [GMES-F-N2]|uk-UA\ESNB Form 2 [GMES-F-N2]|ru-RU\ESNB Form 2 [GMES-F-N2]|id-ID\ESNB Form 2 [GMES-F-N2]";
            //dtShoprow["SYSID"] = "GMES-F-N2";
            //dtShop.Rows.Add(dtShoprow);

            //dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\ESNB PACK 2동 [GMES-P-N2]|en-US\ESNB PACK 2 [GMES-P-N2]|zh-CN\ESNB PACK 2 [GMES-P-N2]|pl-PL\EESNB PACK 2 [GMES-P-N2]|uk-UA\ESNB PACK 2 [GMES-P-N2]|ru-RU\ESNB PACK 2 [GMES-P-N2]|id-ID\ESNB PACK 2 [GMES-P-N2]";
            //dtShoprow["SYSID"] = "GMES-P-N2";
            //dtShop.Rows.Add(dtShoprow);

            #endregion NB

            #region MI

            //DataRow dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\ESMI 전극 [GMES-E-MI]|en-US\ESMI Electrode [GMES-E-MI]|zh-CN\美国 电极 [GMES-E-MI]|pl-PL\ESMI Electrode [GMES-E-MI]|uk-UA\ESMI Electrode [GMES-E-MI]|ru-RU\ESMI Electrode [GMES-E-MI]|id-ID\ESMI Electrode [GMES-E-MI]";
            //dtShoprow["SYSID"] = "GMES-E-MI";
            //dtShop.Rows.Add(dtShoprow);

            //dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\ESMI 조립 [GMES-A-MI]|en-US\ESMI Assembly [GMES-A-MI]|zh-CN\美国 组装 [GMES-A-MI]|pl-PL\ESMI Assembly [GMES-A-MI]|uk-UA\ESMI Assembly [GMES-A-MI]|ru-RU\ESMI Assembly [GMES-A-MI]|id-ID\ESMI Assembly [GMES-A-MI]";
            //dtShoprow["SYSID"] = "GMES-A-MI";
            //dtShop.Rows.Add(dtShoprow);

            //dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\ESMI Pack [GMES-P-MI]|en-US\ESMI Pack [GMES-P-MI]|zh-CN\美国 Pack [GMES-P-MI]|pl-PL\ESMI Pack [GMES-P-MI]|uk-UA\ESMI Pack [GMES-P-MI]|ru-RU\ESMI Pack [GMES-P-MI]|id-ID\ESMI Pack [GMES-P-MI]";
            //dtShoprow["SYSID"] = "GMES-P-MI";
            //dtShop.Rows.Add(dtShoprow);

            #endregion MI

            #region GM

            //DataRow dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\Ultium Cells 전극 [GMES-E-OH]|en-US\Ultium Cells Elec [GMES-E-OH]|zh-CN\Ultium Cells 电极 [GMES-E-OH]|pl-PL\Ultium Cells Elec [GMES-E-OH]|uk-UA\Ultium Cells Elec [GMES-E-OH]|ru-RU\Ultium Cells Elec [GMES-E-OH]|id-ID\Ultium Cells Elec [GMES-E-OH]";
            //dtShoprow["SYSID"] = "GMES-E-OH";
            //dtShop.Rows.Add(dtShoprow);

            //dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\Ultium Cells 조립 [GMES-A-OH]|en-US\Ultium Cells Assy Auto [GMES-A-OH]|zh-CN\Ultium Cells 组装 汽车 [GMES-A-OH]|pl-PL\Ultium Cells Assy Auto [GMES-A-OH]|uk-UA\Ultium Cells Assy Auto [GMES-A-OH]|ru-RU\Ultium Cells Assy Auto [GMES-A-OH]|id-ID\Ultium Cells Assy Auto [GMES-A-OH]";
            //dtShoprow["SYSID"] = "GMES-A-OH";
            //dtShop.Rows.Add(dtShoprow);

            //dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\Ultium Cells 활성화 [GMES-F-OH]|en-US\Ultium Cells Form [GMES-F-OH]|zh-CN\Ultium Cells Form [GMES-F-OH]|pl-PL\Ultium Cells Form [GMES-F-OH]|uk-UA\Ultium Cells Form [GMES-F-OH]|ru-RU\Ultium Cells Form [GMES-F-OH]|id-ID\Ultium Cells Form [GMES-F-OH]";
            //dtShoprow["SYSID"] = "GMES-F-OH";
            //dtShop.Rows.Add(dtShoprow);

            #endregion GM

            #region WA

            //DataRow dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\폴란드 전극 [GMES-E-WA]|en-US\Poland Elec [GMES-E-WA]|zh-CN\波兰 电极 [GMES-E-WA]|pl-PL\Polska Elec[GMES-E-WA]|uk-UA\Польща Elec [GMES-E-WA]|ru-RU\Польша Электрик [GMES-E-WA]|id-ID\Poland Elec  [GMES-E-WA]";
            //dtShoprow["SYSID"] = "GMES-E-WA";
            //dtShop.Rows.Add(dtShoprow);

            //dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\폴란드 조립 1,2 [GMES-A-WA]|en-US\Poland Assembly 1,2 [GMES-A-WA]|zh-CN\波兰 组装 1,2 [GMES-A-WA]|pl-PL\Polska Assembly 1,2 [GMES-A-WA]|uk-UA\Польща Assy 1,2 [GMES-A-WA]|ru-RU\Польская сборка 1.2 [GMES-A-WA]|id-ID\Poland Assembly 1,2 [GMES-A-WA]";
            //dtShoprow["SYSID"] = "GMES-A-WA";
            //dtShop.Rows.Add(dtShoprow);

            //dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\폴란드 조립 3 [GMES-A-W3]|en-US\Poland Assembly 3 [GMES-A-W3]|zh-CN\波兰 组装 3 [GMES-A-W3]|pl-PL\Polska Assembly 3 [GMES-A-W3]|uk-UA\Польща Assy 3 [GMES-A-W3]|ru-RU\Польша Сборка 3[GMES-A-W3]|id-ID\Poland Assembly 3 [GMES-A-W3]";
            //dtShoprow["SYSID"] = "GMES-A-W3";
            //dtShop.Rows.Add(dtShoprow);

            //dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\폴란드 조립 4 [GMES-A-W4]|en-US\Poland Assembly 4 [GMES-A-W4]|zh-CN\波兰 组装 4 [GMES-A-W4]|pl-PL\Polska Assembly 4 [GMES-A-W4]|uk-UA\Польща Assy 4 [GMES-A-W4]|ru-RU\Польша Сборка 4[GMES-A-W4]|id-ID\Poland Assembly 4 [GMES-A-W4]";
            //dtShoprow["SYSID"] = "GMES-A-W4";
            //dtShop.Rows.Add(dtShoprow);

            //dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\폴란드 Pack 1 [GMES-P-WA]|en-US\Poland Pack 1 [GMES-P-WA]|zh-CN\波兰 Pack 1 [GMES-P-WA]|pl-PL\Polska Pack 1 [GMES-P-WA]|uk-UA\Польща Pack 1 [GMES-P-WA]|ru-RU\Польша Пакет 1 [GMES-P-WA]|id-ID\Poland Pack 1 [GMES-P-WA]";
            //dtShoprow["SYSID"] = "GMES-P-WA";
            //dtShop.Rows.Add(dtShoprow);

            //dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\폴란드 Pack 2 [GMES-P-W2]|en-US\Poland Pack 2 [GMES-P-W2]|zh-CN\波兰 Pack 2 [GMES-P-W2]|pl-PL\Polska Pack 2 [GMES-P-W2]|uk-UA\Польща Pack 2 [GMES-P-W2]|ru-RU\Польша Пакет 2 [GMES-P-W2]|id-ID\Poland Pack 2 [GMES-P-W2]";
            //dtShoprow["SYSID"] = "GMES-P-W2";
            //dtShop.Rows.Add(dtShoprow);

            //dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\폴란드 Pack 3 [GMES-P-W3]|en-US\Poland Pack 3 [GMES-P-W3]|zh-CN\波兰 Pack 3 [GMES-P-W3]|pl-PL\Polska Pack 3 [GMES-P-W3]|uk-UA\Польща Pack 3 [GMES-P-W3]|ru-RU\Польша Пакет 3 [GMES-P-W3]|id-ID\Poland Pack 3 [GMES-P-W3]";
            //dtShoprow["SYSID"] = "GMES-P-W3";
            //dtShop.Rows.Add(dtShoprow);

            #endregion WA

            #region HM

            //DataRow dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\인니 전극 [GMES-E-JB]|en-US\Indonesia Elec [GMES-E-JB]|zh-CN\Indonesia Elec [GMES-E-JB]|pl-pl\Indonesia Elec [GMES-E-JB]|uk-UA\Indonesia Elec [GMES-E-JB]|ru-RU\Indonesia Elec [GMES-E-JB]|id-ID\Indonesia Elec [GMES-E-JB]";
            //dtShoprow["SYSID"] = "GMES-E-JB";
            //dtShop.Rows.Add(dtShoprow);

            //dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\인니 조립 [GMES-A-JB]|en-US\Indonesia Assy Auto [GMES-A-JB]|zh-CN\Indonesia Assy Auto [GMES-A-JB]|pl-pl\Indonesia Assy Auto [GMES-A-JB]|uk-UA\Indonesia Assy Auto [GMES-A-JB]|ru-RU\Indonesia Assy Auto [GMES-A-JB]|id-ID\Indonesia Assy Auto [GMES-A-JB]";
            //dtShoprow["SYSID"] = "GMES-A-JB";
            //dtShop.Rows.Add(dtShoprow);

            //dtShoprow = dtShop.NewRow();
            //dtShoprow["SHOPLIST"] = @"ko-KR\인니 활성화 [GMES-F-JB]|en-US\Indonesia Form [GMES-F-JB]|zh-CN\Indonesia Form [GMES-F-JB]|pl-pl\Indonesia Form [GMES-F-JB]|uk-UA\Indonesia Form [GMES-F-JB]|ru-RU\Indonesia Form [GMES-F-JB]|id-ID\Indonesia Form [GMES-F-JB]";
            //dtShoprow["SYSID"] = "GMES-F-JB";
            //dtShop.Rows.Add(dtShoprow);

            #endregion HM

            #region HD

            DataRow dtShoprow = dtShop.NewRow();
            dtShoprow["SHOPLIST"] = @"ko-KR\HD 전극 [GMES-E-HD1]|en-US\Honda Elec [GMES-E-HD1]|zh-CN\Honda Elec [GMES-E-HD1]|pl-pl\Honda Elec [GMES-E-HD1]|uk-UA\Honda Elec [GMES-E-HD1]|ru-RU\Honda Elec [GMES-E-HD1]|id-ID\Honda Elec [GMES-E-HD1]";
            dtShoprow["SYSID"] = "GMES-E-HD1";
            dtShop.Rows.Add(dtShoprow);

            dtShoprow = dtShop.NewRow();
            dtShoprow["SHOPLIST"] = @"ko-KR\HD 조립 [GMES-A-HD1]|en-US\Honda Assy Auto [GMES-A-HD1]|zh-CN\Honda Assy Auto [GMES-A-HD1]|pl-pl\Honda Assy Auto [GMES-A-HD1]|uk-UA\Honda Assy Auto [GMES-A-HD1]|ru-RU\Honda Assy Auto [GMES-A-HD1]|id-ID\Honda Assy Auto [GMES-A-HD1]";
            dtShoprow["SYSID"] = "GMES-A-HD1";
            dtShop.Rows.Add(dtShoprow);

            dtShoprow = dtShop.NewRow();
            dtShoprow["SHOPLIST"] = @"ko-KR\HD 활성화 [GMES-F-HD1]|en-US\Honda Form [GMES-F-HD1]|zh-CN\Honda Form [GMES-F-HD1]|pl-pl\Honda Form [GMES-F-HD1]|uk-UA\Honda Form [GMES-F-HD1]|ru-RU\Honda Form [GMES-F-HD1]|id-ID\Honda Form [GMES-F-HD1]";
            dtShoprow["SYSID"] = "GMES-F-HD1";
            dtShop.Rows.Add(dtShoprow);

            dtShoprow = dtShop.NewRow();
            dtShoprow["SHOPLIST"] = @"ko-KR\HD PACK [GMES-P-HD1]|en-US\Honda Pack [GMES-P-HD1]|zh-CN\Honda Pack [GMES-P-HD1]|pl-pl\Honda Pack [GMES-P-HD1]|uk-UA\Honda Pack [GMES-P-HD1]|ru-RU\Honda Pack [GMES-P-HD1]|id-ID\Honda Pack [GMES-P-HD1]";
            dtShoprow["SYSID"] = "GMES-P-HD1";
            dtShop.Rows.Add(dtShoprow);

            #endregion HD

            dsConfig.Tables.Add(dtShop);

            //==================================================================================================
            string customConfigPath = string.Empty;
            string settingFileName = string.Empty;

            customConfigPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\GMES\\SFU\\";
            settingFileName = "MainNavigator.confg"; // ConfigurationManager.AppSettings["GLOBAL_CONFIG_FILE_NAME"];

            string[] directoryNames = customConfigPath.Split('\\');
            string current = string.Empty;

            for (int inx = 0; inx < directoryNames.Length - 1; inx++)
            {
                string directoryName = directoryNames[inx];

                if (string.IsNullOrEmpty(current))
                    current = directoryName;
                else
                    current += "\\" + directoryName;

                DirectoryInfo directoryInfo = new DirectoryInfo(current);

                if (!directoryInfo.Exists)
                    directoryInfo.Create();
            }

            dsConfig.WriteXml(customConfigPath + settingFileName, XmlWriteMode.WriteSchema);
        }

        private void GetMainNavigatorConfig()
        {
            try
            {
                string customConfigPath = string.Empty;
                string settingFileName = string.Empty;

                FileInfo customConfigFile;
                customConfigFile = new FileInfo(Environment.CurrentDirectory + @"\MainNavigator.confg");

                if (customConfigFile.Exists)
                {
                    configSet = new DataSet();
                    configSet.ReadXml(customConfigFile.FullName, XmlReadMode.ReadSchema);
                }
            }
            catch
            {
            }
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch
            {
            }
        }
    }
}