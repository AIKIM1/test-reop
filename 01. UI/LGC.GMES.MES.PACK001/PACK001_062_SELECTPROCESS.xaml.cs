/*************************************************************************************
  Created Date : 2020.06.10
      Creator : 염규범
   Decription : 공정 선택 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2020.06.10  염규범 : Initial Created.
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_062_SELECTPROCESS : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        

        public PACK001_062_SELECTPROCESS()
        {
            InitializeComponent();
        }

        /// <summary>
        /// PROCESS 의 LABELPRINTIUSE 라벨발행여부
        /// Y인경우 라벨발행여부가 Y인 공정만 버튼 활성화.
        /// </summary>
        private string sLabelPrintUse = "N";
        private DataRow drSelectProcessInfo = null;
        private DataRow drSelectedProcessInfo = null;
        private string sSelectedProcessTitle = string.Empty;
        
        /// <summary>
        /// COLUMNS:EQSGID, EQSGNAME, PROCID, PROCNAME, PROCSEQ, EQPTID, EQPTNAME
        /// </summary>
        public DataRow DrSelectedProcessInfo
        {
            get
            {
                return drSelectedProcessInfo;
            }

            set
            {
                drSelectedProcessInfo = value;
            }
        }
        public string SSelectedProcessTitle
        {
            get
            {
                return sSelectedProcessTitle;
            }

            set
            {
                sSelectedProcessTitle = value;
            }
        }

        #endregion

        #region Initialize

        #endregion

        #region Event
         
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
                if (tmps != null)
                {
                    DataTable dtText = tmps[0] as DataTable;

                    if (dtText.Rows.Count > 0)
                    {
                        sLabelPrintUse = Util.NVC(dtText.Rows[0]["LABELPRINTUSE"]);
                    }
                }

                //확인 필요
                //사용자 권한의 팩 라인 조회되도록 해야함 ... 현재 임시로 하드코딩
                CommonCombo _combo = new CommonCombo();
                this.cboEquipmentSegment.SelectedValueChanged -= new System.EventHandler<C1.WPF.PropertyChangedEventArgs<object>>(this.cboEquipmentSegment_SelectedValueChanged);
                string[] area = { LoginInfo.CFG_AREA_ID.ToString() };
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, sFilter: area);
                this.cboEquipmentSegment.SelectedValueChanged += new System.EventHandler<C1.WPF.PropertyChangedEventArgs<object>>(this.cboEquipmentSegment_SelectedValueChanged);

                setProcessList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                setProcessList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(drSelectProcessInfo == null || !(txtSelectedProcess.Text.Length > 0))
                {
                    Util.MessageInfo("SFU1459");
                    return;
                }
                DrSelectedProcessInfo = drSelectProcessInfo;
                sSelectedProcessTitle = "" + drSelectProcessInfo["EQSGNAME"].ToString() + "-" + drSelectProcessInfo["PROCNAME"].ToString();
                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            
        }

        private void btnProcess_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btnClicked = (Button)sender;
                //버튼의 정보 버튼 TAG에서 추출
                drSelectProcessInfo = (DataRow)btnClicked.Tag;

                //선택 TEXT박스 셋팅
                txtSelectedLine.Text = Util.NVC(drSelectProcessInfo["EQSGNAME"]);
                txtSelectedProcess.Text = Util.NVC(drSelectProcessInfo["PROCNAME"]);

                //설비콤보박스 셋팅
                CommonCombo _combo = new CommonCombo();
                _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.NONE, null, null, new string[] { Util.NVC(drSelectProcessInfo["EQSGID"]), Util.NVC(drSelectProcessInfo["PROCID"]) , null } , sCase: "LABELCODE_BY_EQPTID");

                // buttonColorChange(drSelectProcessInfo);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            
        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                drSelectProcessInfo["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                drSelectProcessInfo["EQPTNAME"] = Util.NVC(cboEquipment.Text);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion























        #region Mehod

        /// <summary>
        /// 
        /// </summary>
        private void setProcessList()
        {
            try
            {
                gdProcessList.RowDefinitions.Clear();
                gdProcessList.Children.Clear();

                getLineProcessList_DataSet();
            }
            catch(Exception ex)
            {
                throw ex;
            }
            
        }

        

        private void getLineProcessList_DataSet()
        {
            try
            {
                if (cboEquipmentSegment.SelectedValue.ToString() == "") return;

                DataSet dsINDATA = new DataSet();
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));


                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString() == "" ? null : cboEquipmentSegment.SelectedValue;
                INDATA.Rows.Add(dr);

                dsINDATA.Tables.Add(INDATA);
                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_PRD_SEL_PROCESS_LIST_BY_EQPT_PRINT", "INDATA", "OUTDATA", INDATA, (dtResult, ex) =>
                {

                    if (ex != null)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException(ex);
                        return;
                    }
                    else
                    {
                        if (dtResult != null && dtResult.Rows.Count > 0)
                        {
                            Clear_SelectProc();

                            setGridProcessList_DataSet_New(dtResult);
                        }
                    }
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    return;
                    
                });


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region [ 기존 로직 ] 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtLineProcessList"></param>
        private void setGridProcessList_DataSet(DataSet dsLineProcessList)
        {
            try
            {
                DataTable dtLineProcessList = dsLineProcessList.Tables["OUTDATA"];
                DataTable dtLineList = dsLineProcessList.Tables["OUTDATA_EQSG"];

                //Line 수 반복
                for (int i = 0; i < dtLineList.Rows.Count; i++)
                {
                    var rowMain = new RowDefinition();
                    rowMain.Height = GridLength.Auto;
                    gdProcessList.RowDefinitions.Add(rowMain);

                    //Main Grid 에 subGrid(line 공정) 추가
                    Grid grdSubGridNew = new Grid();

                    /*
                    _ 엔터 인가 확인 
                    string sPrjName = dtLineList.Rows[i]["PROD_PRJ_MODLID"].ToString();
                    sPrjName = Regex.Replace(sPrjName, @"[^0-9a-zA-Z]", "_");
                    grdSubGridNew.Name = dtLineList.Rows[i]["EQSGID"].ToString() + "_" + sPrjName;
                    grdSubGridNew.Tag = dtLineList.Rows[i]["EQSGID"] + "_" + dtLineList.Rows[i]["PROD_PRJ_MODLID"].ToString()
                    */

                    grdSubGridNew.Name = dtLineList.Rows[i]["EQSGID"].ToString();
                    grdSubGridNew.Tag = dtLineList.Rows[i]["EQSGID"].ToString();

                    //데이터 line의 공정 추출
                    DataTable myNewTable = dtLineProcessList.Select("EQSGID = '" + dtLineList.Rows[i]["EQSGID"].ToString() 
                        + "' AND NUM= '" + dtLineList.Rows[i]["NUM"].ToString() + "'").CopyToDataTable();
                    myNewTable.Columns.Add("EQPTID", typeof(string));
                    myNewTable.Columns.Add("EQPTNAME", typeof(string));
                    
                    var column = new ColumnDefinition();
                    column.MinWidth = 100;
                    column.Width = GridLength.Auto;
                    grdSubGridNew.ColumnDefinitions.Add(column);

                    #region 첫번째 컬럼 에 라인명 삽입.
                    Grid grdSubGridTitle = new Grid();
                    var row = new RowDefinition();
                    row.Height = GridLength.Auto;
                    grdSubGridTitle.RowDefinitions.Add(row);
                    StackPanel newTextStackPanel = new StackPanel();
                    newTextStackPanel.VerticalAlignment = VerticalAlignment.Center;
                    newTextStackPanel.SetValue(Grid.RowProperty, 0);
                    newTextStackPanel.SetValue(Grid.ColumnProperty, 0);

                    //라인명
                    System.Windows.Controls.TextBlock newTextBlock_EqsgName = new TextBlock();
                    newTextBlock_EqsgName.Text = dtLineList.Rows[i]["EQSGNAME"].ToString();
                    newTextBlock_EqsgName.HorizontalAlignment = HorizontalAlignment.Center;
                    newTextBlock_EqsgName.Style = (Style)FindResource("Content_InputForm_LabelStyle");
                    newTextStackPanel.Children.Add(newTextBlock_EqsgName);

                    grdSubGridTitle.SetValue(Grid.RowProperty, 0);
                    grdSubGridTitle.SetValue(Grid.ColumnProperty, 0);

                    grdSubGridNew.Children.Add(newTextStackPanel);
                    #endregion

                    //sub grid에 공정 컬럼 추가
                    var columnProcess = new ColumnDefinition();
                    columnProcess.Width = GridLength.Auto;
                    grdSubGridNew.ColumnDefinitions.Add(columnProcess);

                    //공정 컬럼에 panel 추가
                    WrapPanel wpProcess = new WrapPanel();

                    wpProcess.Width = 725;
                    wpProcess.Margin = new Thickness(0, 5, 0, 5);
                    wpProcess.SetValue(Grid.RowProperty, 0);
                    wpProcess.SetValue(Grid.ColumnProperty, 1);
                    grdSubGridNew.Children.Add(wpProcess);

                    //line의 공정 수 만큼 panel에 버튼 생성
                    for (int c = 0; c < myNewTable.Rows.Count; c++)
                    {
                        ;

                        System.Windows.Controls.Button newBtn = new Button();
                        newBtn.Content = myNewTable.Rows[c]["PROCNAME"].ToString();
                        newBtn.Name = "Button_" 
                            + Util.NVC(myNewTable.Rows[c]["EQSGID"]) 
                            + "_" + Util.NVC(myNewTable.Rows[c]["PROCID"]);
                        newBtn.Tag = myNewTable.Rows[c];
                        newBtn.FontSize = 12;

                        if (Util.NVC(myNewTable.Rows[c]["PRDT_CLSS_CODE"]) != "")
                        {
                            if (Util.NVC(myNewTable.Rows[c]["PRDT_CLSS_CODE"]) == "CELL")
                            {
                                newBtn.Style = (Style)FindResource("Content_MainButtonNextStyle02");
                            }
                            else if (Util.NVC(myNewTable.Rows[c]["PRDT_CLSS_CODE"]) == "CMA")
                            {
                                newBtn.Style = (Style)FindResource("Content_MainButtonNextStyle03");
                            }
                            else if (Util.NVC(myNewTable.Rows[c]["PRDT_CLSS_CODE"]) == "BMA")
                            {
                                newBtn.Style = (Style)FindResource("Content_MainButtonNextStyle04");
                            }
                            else
                            {
                                newBtn.Style = (Style)FindResource("Content_MainButtonNextStyle01");
                            }
                        }
                        else
                        {
                            if (Util.NVC(myNewTable.Rows[c]["PCSGID"]) == "C")
                            {
                                newBtn.Style = (Style)FindResource("Content_MainButtonNextStyle02");
                            }
                            else if (Util.NVC(myNewTable.Rows[c]["PCSGID"]) == "M")
                            {
                                newBtn.Style = (Style)FindResource("Content_MainButtonNextStyle03");
                            }
                            else if (Util.NVC(myNewTable.Rows[c]["PCSGID"]) == "P")
                            {
                                newBtn.Style = (Style)FindResource("Content_MainButtonNextStyle04");
                            }
                            else
                            {
                                newBtn.Style = (Style)FindResource("Content_MainButtonNextStyle01");
                            }
                        }

                        newBtn.Width = Double.NaN;//150;
                        newBtn.FontSize = 10;
                        newBtn.Margin = new Thickness(10, 5, 10, 5);
                        newBtn.Click += btnProcess_Click;
                        //라벨발행공정만 활성화하는경우
                        if (sLabelPrintUse.Equals("Y"))
                        {
                            if (Util.NVC(myNewTable.Rows[c]["LABELPRINTIUSE"]) == "Y")
                            {
                                newBtn.IsEnabled = true;
                            }
                            else
                            {
                                newBtn.Style = (Style)FindResource("Content_MainButtonStyle");
                                newBtn.IsEnabled = false;
                            }
                        }

                        wpProcess.Children.Add(newBtn);
                    }

                    grdSubGridNew.SetValue(Grid.RowProperty, gdProcessList.RowDefinitions.Count - 1);
                    gdProcessList.Children.Add(grdSubGridNew);

                    var border = new Border();
                    border.BorderBrush = Brushes.Gray;
                    border.BorderThickness = new Thickness(1);
                    border.SetValue(Grid.RowProperty, gdProcessList.RowDefinitions.Count - 1);
                    gdProcessList.Children.Add(border);


                    var rowEmpty = new RowDefinition();
                    rowEmpty.Height = new System.Windows.GridLength(20);
                    gdProcessList.RowDefinitions.Add(rowEmpty);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region [ 신규 로직 ]
        private void setGridProcessList_DataSet_New(DataTable dsLineProcessList)
        {
            try
            {
              //  DataTable dtLineProcessList = dsLineProcessList["OUTDATA"];

                    var rowMain = new RowDefinition();
                    rowMain.Height = GridLength.Auto;
                    gdProcessList.RowDefinitions.Add(rowMain);

                    //Main Grid 에 subGrid(line 공정) 추가
                    Grid grdSubGridNew = new Grid();

                    grdSubGridNew.Name = dsLineProcessList.Rows[0]["EQSGID"].ToString();
                    grdSubGridNew.Tag = dsLineProcessList.Rows[0]["EQSGID"].ToString();

                    if(dsLineProcessList.Select("LABELPRINTIUSE = 'Y'").ToArray().Count() < 1)
                    {
                        return;
                    }

                    //데이터 line의 공정 추출
                    DataTable myNewTable = dsLineProcessList.Select("EQSGID = '" + dsLineProcessList.Rows[0]["EQSGID"].ToString() + "' AND LABELPRINTIUSE= 'Y'").CopyToDataTable();
                    myNewTable.Columns.Add("EQPTID", typeof(string));
                    myNewTable.Columns.Add("EQPTNAME", typeof(string));

                    var column = new ColumnDefinition();
                    column.MinWidth = 100;
                    column.Width = GridLength.Auto;
                    grdSubGridNew.ColumnDefinitions.Add(column);

                    #region 첫번째 컬럼 에 라인명 삽입.
                    Grid grdSubGridTitle = new Grid();
                    var row = new RowDefinition();
                    row.Height = GridLength.Auto;
                    grdSubGridTitle.RowDefinitions.Add(row);
                    StackPanel newTextStackPanel = new StackPanel();
                    newTextStackPanel.VerticalAlignment = VerticalAlignment.Center;
                    newTextStackPanel.SetValue(Grid.RowProperty, 0);
                    newTextStackPanel.SetValue(Grid.ColumnProperty, 0);

                    //라인명
                    System.Windows.Controls.TextBlock newTextBlock_EqsgName = new TextBlock();
                    newTextBlock_EqsgName.Text = dsLineProcessList.Rows[0]["EQSGNAME"].ToString();
                    newTextBlock_EqsgName.HorizontalAlignment = HorizontalAlignment.Center;
                    newTextBlock_EqsgName.Style = (Style)FindResource("Content_InputForm_LabelStyle");
                    newTextStackPanel.Children.Add(newTextBlock_EqsgName);

                    grdSubGridTitle.SetValue(Grid.RowProperty, 0);
                    grdSubGridTitle.SetValue(Grid.ColumnProperty, 0);

                    grdSubGridNew.Children.Add(newTextStackPanel);
                    #endregion

                    //sub grid에 공정 컬럼 추가
                    var columnProcess = new ColumnDefinition();
                    columnProcess.Width = GridLength.Auto;
                    grdSubGridNew.ColumnDefinitions.Add(columnProcess);

                    //공정 컬럼에 panel 추가
                    WrapPanel wpProcess = new WrapPanel();

                    wpProcess.Width = 725;
                    wpProcess.Margin = new Thickness(0, 5, 0, 5);
                    wpProcess.SetValue(Grid.RowProperty, 0);
                    wpProcess.SetValue(Grid.ColumnProperty, 1);
                    grdSubGridNew.Children.Add(wpProcess);

                    //line의 공정 수 만큼 panel에 버튼 생성
                    for (int c = 0; c < myNewTable.Rows.Count; c++)
                    {
                        ;

                        System.Windows.Controls.Button newBtn = new Button();
                        newBtn.Content = myNewTable.Rows[c]["PROCNAME"].ToString();
                        newBtn.Name = "Button_"
                            + Util.NVC(myNewTable.Rows[c]["EQSGID"])
                            + "_" + Util.NVC(myNewTable.Rows[c]["PROCID"]);
                        newBtn.Tag = myNewTable.Rows[c];
                        newBtn.FontSize = 12;

                        if (Util.NVC(myNewTable.Rows[c]["PRDT_CLSS_CODE"]) != "")
                        {
                            if (Util.NVC(myNewTable.Rows[c]["PRDT_CLSS_CODE"]) == "CELL")
                            {
                                newBtn.Style = (Style)FindResource("Content_MainButtonNextStyle02");
                                newBtn.IsEnabled = true;
                            }
                            else if (Util.NVC(myNewTable.Rows[c]["PRDT_CLSS_CODE"]) == "CMA")
                            {
                                newBtn.Style = (Style)FindResource("Content_MainButtonNextStyle03");
                                newBtn.IsEnabled = true;
                            }
                            else if (Util.NVC(myNewTable.Rows[c]["PRDT_CLSS_CODE"]) == "BMA")
                            {
                                newBtn.Style = (Style)FindResource("Content_MainButtonNextStyle04");
                                newBtn.IsEnabled = true;
                            }
                            else
                            {
                                newBtn.Style = (Style)FindResource("Content_MainButtonNextStyle01");
                                newBtn.IsEnabled = true;
                            }
                        }
                        else
                        {
                            if (Util.NVC(myNewTable.Rows[c]["PCSGID"]) == "C")
                            {
                                newBtn.Style = (Style)FindResource("Content_MainButtonNextStyle02");
                                newBtn.IsEnabled = true;
                            }
                            else if (Util.NVC(myNewTable.Rows[c]["PCSGID"]) == "M")
                            {
                                newBtn.Style = (Style)FindResource("Content_MainButtonNextStyle03");
                                newBtn.IsEnabled = true;
                            }
                            else if (Util.NVC(myNewTable.Rows[c]["PCSGID"]) == "P")
                            {
                                newBtn.Style = (Style)FindResource("Content_MainButtonNextStyle04");
                                newBtn.IsEnabled = true;
                            }
                            else
                            {
                                newBtn.Style = (Style)FindResource("Content_MainButtonNextStyle01");
                                newBtn.IsEnabled = true;
                            }
                        }

                        newBtn.Width = Double.NaN;//150;
                        newBtn.FontSize = 10;
                        newBtn.Margin = new Thickness(10, 5, 10, 5);
                        newBtn.Click += btnProcess_Click;
                        //라벨발행공정만 활성화하는경우
                        /*
                        if (sLabelPrintUse.Equals("Y"))
                        {
                            if (Util.NVC(myNewTable.Rows[c]["LABELPRINTIUSE"]) == "Y")
                            {
                                newBtn.IsEnabled = true;
                            }
                            else
                            {
                                newBtn.Style = (Style)FindResource("Content_MainButtonStyle");
                                newBtn.IsEnabled = false;
                            }
                        }
                        */
                        wpProcess.Children.Add(newBtn);
                    }

                    grdSubGridNew.SetValue(Grid.RowProperty, gdProcessList.RowDefinitions.Count - 1);
                    gdProcessList.Children.Add(grdSubGridNew);

                    var border = new Border();
                    border.BorderBrush = Brushes.Gray;
                    border.BorderThickness = new Thickness(1);
                    border.SetValue(Grid.RowProperty, gdProcessList.RowDefinitions.Count - 1);
                    gdProcessList.Children.Add(border);


                    var rowEmpty = new RowDefinition();
                    rowEmpty.Height = new System.Windows.GridLength(20);
                    gdProcessList.RowDefinitions.Add(rowEmpty);
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        private void buttonColorChange(DataRow drSelectProcessInfo)
        {
            try
            {
                IList<DependencyObject> itemButtonparent = new List<DependencyObject>();
                VTreeHelper.GetChildrenOfType(gdProcessList, typeof(Button), ref itemButtonparent);
                for (int i = 0; i < itemButtonparent.Count; i++)
                {
                    Button Buttonparent = (Button)itemButtonparent[i];
                    if (Buttonparent.Name == "Button_" 
                        + Util.NVC(drSelectProcessInfo["EQSGID"]) 
                        + "_" + Util.NVC(drSelectProcessInfo["PROCID"]))
                    {
                        Buttonparent.Style = (Style)FindResource("Content_MainButtonNextStyle05");

                    }
                    else
                    {
                        DataRow drButton = (DataRow)Buttonparent.Tag;

                        if (Util.NVC(drButton["PRDT_CLSS_CODE"]) != "")
                        {
                            if (Util.NVC(drButton["PRDT_CLSS_CODE"]) == "CELL")
                            {
                                Buttonparent.Style = (Style)FindResource("Content_MainButtonNextStyle02");
                            }
                            else if (Util.NVC(drButton["PRDT_CLSS_CODE"]) == "CMA")
                            {
                                Buttonparent.Style = (Style)FindResource("Content_MainButtonNextStyle03");
                            }
                            else if (Util.NVC(drButton["PRDT_CLSS_CODE"]) == "BMA")
                            {
                                Buttonparent.Style = (Style)FindResource("Content_MainButtonNextStyle04");
                            }
                            else
                            {
                                Buttonparent.Style = (Style)FindResource("Content_MainButtonNextStyle01");
                            }
                        }
                        else
                        {
                            if (Util.NVC(drButton["PCSGID"]) == "C")
                            {
                                Buttonparent.Style = (Style)FindResource("Content_MainButtonNextStyle02");
                            }
                            else if (Util.NVC(drButton["PCSGID"]) == "M")
                            {
                                Buttonparent.Style = (Style)FindResource("Content_MainButtonNextStyle03");
                            }
                            else if (Util.NVC(drButton["PCSGID"]) == "P")
                            {
                                Buttonparent.Style = (Style)FindResource("Content_MainButtonNextStyle04");
                            }
                            else
                            {
                                Buttonparent.Style = (Style)FindResource("Content_MainButtonNextStyle01");
                            }
                        }

                        //라벨발행공정만 활성화하는경우
                        if (sLabelPrintUse.Equals("Y"))
                        {
                            if (Util.NVC(drButton["LABELPRINTIUSE"]) != "Y")
                            {
                                Buttonparent.Style = (Style)FindResource("Content_MainButtonStyle");
                            }
                        }
                        
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Clear_SelectProc()
        {
            txtSelectedLine.Text = "";
            txtSelectedProcess.Text = "";
            cboEquipment.SelectedValue = null;
            cboEquipment.ItemsSource = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private DataTable getLineProcessList()
        {

            DataTable dtResult = null;
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["AREA_TYPE_CODE"] = Area_Type.PACK;
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString() == "" ? null : cboEquipmentSegment.SelectedValue;
                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_LIST_BY_EQSGID", "RQSTDT", "RSLTDT", RQSTDT);

            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_PROCESS_LIST_BY_EQSGID", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }


            return dtResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtLineProcessList"></param>
        private void setGridProcessList(DataTable dtLineProcessList)
        {
            try
            {
                //line groupby 추출
                var list = dtLineProcessList.AsEnumerable().GroupBy(r => new
                {
                    PRODGROUP = r.Field<string>("EQSGID")
                }).Select(g => g.First());
                DataTable dtLineList = list.CopyToDataTable();
                if (dtLineList == null)
                {
                    return;
                }

                //Line 수 반복
                for (int i = 0; i < dtLineList.Rows.Count; i++)
                {
                    var rowMain = new RowDefinition();
                    rowMain.Height = GridLength.Auto;
                    gdProcessList.RowDefinitions.Add(rowMain);

                    //Main Grid 에 subGrid(line 공정) 추가
                    Grid grdSubGridNew = new Grid();
                    grdSubGridNew.Name = dtLineList.Rows[i]["EQSGID"].ToString();
                    grdSubGridNew.Tag = dtLineList.Rows[i]["EQSGID"];

                    //데이터 line의 공정 추출
                    DataTable myNewTable = dtLineProcessList.Select("EQSGID = '" + dtLineList.Rows[i]["EQSGID"].ToString() + "'").CopyToDataTable();
                    myNewTable.Columns.Add("EQPTID", typeof(string));
                    myNewTable.Columns.Add("EQPTNAME", typeof(string));

                    //첫번째 컬럼 에 라인명 삽입.
                    var column = new ColumnDefinition();

                    column.MinWidth = 100;
                    column.Width = GridLength.Auto;
                    grdSubGridNew.ColumnDefinitions.Add(column);

                    System.Windows.Controls.TextBlock newTextBlock = new TextBlock();

                    newTextBlock.Text = dtLineList.Rows[i]["EQSGNAME"].ToString();
                    newTextBlock.HorizontalAlignment = HorizontalAlignment.Right;
                    newTextBlock.Style = (Style)FindResource("Content_InputForm_LabelStyle");
                    newTextBlock.SetValue(Grid.RowProperty, 0);
                    newTextBlock.SetValue(Grid.ColumnProperty, 0);
                    grdSubGridNew.Children.Add(newTextBlock);

                    //sub grid에 공정 컬럼 추가
                    var columnProcess = new ColumnDefinition();
                    columnProcess.Width = GridLength.Auto;
                    grdSubGridNew.ColumnDefinitions.Add(columnProcess);

                    //공정 컬럼에 panel 추가
                    WrapPanel wpProcess = new WrapPanel();

                    //wpProcess.MinWidth = 750;
                    //wpProcess.Width = Double.NaN;// 750;
                    wpProcess.Width = 725;
                    wpProcess.Margin = new Thickness(0, 5, 0, 5);
                    //wpProcess.Margin = new Thickness(15);
                    wpProcess.SetValue(Grid.RowProperty, 0);
                    wpProcess.SetValue(Grid.ColumnProperty, 1);
                    grdSubGridNew.Children.Add(wpProcess);

                    //line의 공정 수 만큼 panel에 버튼 생성
                    for (int c = 0; c < myNewTable.Rows.Count; c++)
                    {
                        System.Windows.Controls.Button newBtn = new Button();
                        newBtn.Content = myNewTable.Rows[c]["PROCNAME"].ToString();
                        newBtn.Name = "Button_" + Util.NVC(myNewTable.Rows[c]["EQSGID"]) +"_"+Util.NVC(myNewTable.Rows[c]["PROCID"]);
                        newBtn.Tag = myNewTable.Rows[c];//myNewTable.Rows[c]["PROCID"].ToString();
                        newBtn.FontSize = 12;

                        if (Util.NVC(myNewTable.Rows[c]["PRDT_CLSS_CODE"]) != "")
                        {
                            if (Util.NVC(myNewTable.Rows[c]["PRDT_CLSS_CODE"]) == "CELL")
                            {
                                newBtn.Style = (Style)FindResource("Content_MainButtonNextStyle02");
                            }
                            else if (Util.NVC(myNewTable.Rows[c]["PRDT_CLSS_CODE"]) == "CMA")
                            {
                                newBtn.Style = (Style)FindResource("Content_MainButtonNextStyle03");
                            }
                            else if (Util.NVC(myNewTable.Rows[c]["PRDT_CLSS_CODE"]) == "BMA")
                            {
                                newBtn.Style = (Style)FindResource("Content_MainButtonNextStyle04");
                            }
                            else
                            {
                                newBtn.Style = (Style)FindResource("Content_MainButtonNextStyle01");
                            }
                        }
                        else
                        {
                            if (Util.NVC(myNewTable.Rows[c]["PCSGID"]) == "C")
                            {
                                newBtn.Style = (Style)FindResource("Content_MainButtonNextStyle02");
                            }
                            else if (Util.NVC(myNewTable.Rows[c]["PCSGID"]) == "M")
                            {
                                newBtn.Style = (Style)FindResource("Content_MainButtonNextStyle03");
                            }
                            else if (Util.NVC(myNewTable.Rows[c]["PCSGID"]) == "P")
                            {
                                newBtn.Style = (Style)FindResource("Content_MainButtonNextStyle04");
                            }
                            else
                            {
                                newBtn.Style = (Style)FindResource("Content_MainButtonNextStyle01");
                            }
                        }
                        //Content_MainButtonNextStyle02 cell
                        //Content_MainButtonNextStyle03 cma
                        //Content_MainButtonNextStyle04 bma

                        //Content_MainButtonNextStyle01 기타
                        //Content_MainButtonNextStyle   선택

                        


                        newBtn.Width = Double.NaN;//150;
                        newBtn.FontSize = 10;
                        newBtn.Margin = new Thickness(10, 5, 10, 5);
                        newBtn.Click += btnProcess_Click;
                        //라벨발행공정만 활성화하는경우
                        if (sLabelPrintUse.Equals("Y"))
                        {
                            if (Util.NVC(myNewTable.Rows[c]["LABELPRINTIUSE"]) == "Y")
                            {
                                newBtn.IsEnabled = true;
                            }
                            else
                            {
                                newBtn.Style = (Style)FindResource("Content_MainButtonStyle");
                                newBtn.IsEnabled = false;
                            }
                        }

                        wpProcess.Children.Add(newBtn);
                    }

                    grdSubGridNew.SetValue(Grid.RowProperty, gdProcessList.RowDefinitions.Count - 1);
                    gdProcessList.Children.Add(grdSubGridNew);

                    var border = new Border();
                    border.BorderBrush = Brushes.Gray;
                    border.BorderThickness = new Thickness(1);
                    border.SetValue(Grid.RowProperty, gdProcessList.RowDefinitions.Count - 1);
                    gdProcessList.Children.Add(border);


                    var rowEmpty = new RowDefinition();
                    rowEmpty.Height = new System.Windows.GridLength(20);
                    gdProcessList.RowDefinitions.Add(rowEmpty);
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void buttonColorChange(string sEqsgId, string sProcessId)
        {
            try
            {
                IList<DependencyObject> itemButtonparent = new List<DependencyObject>();
                VTreeHelper.GetChildrenOfType(gdProcessList, typeof(Button), ref itemButtonparent);
                for(int i=0; i < itemButtonparent.Count; i++)
                {
                    Button Buttonparent = (Button)itemButtonparent[i];
                    if(Buttonparent.Name == "Button_" +sEqsgId +"_"+ sProcessId)
                    {
                        Buttonparent.Style = (Style)FindResource("Content_MainButtonNextStyle05");
                        
                        //Buttonparent.Style = (Style)FindResource("Content_MainButtonNextStyle");
                    }
                    else
                    {
                        DataRow drButton = (DataRow)Buttonparent.Tag;
                        
                        if(Util.NVC(drButton["PRDT_CLSS_CODE"]) != "")
                        {
                            if (Util.NVC(drButton["PRDT_CLSS_CODE"]) == "CELL")
                            {
                                Buttonparent.Style = (Style)FindResource("Content_MainButtonNextStyle02");
                            }
                            else if (Util.NVC(drButton["PRDT_CLSS_CODE"]) == "CMA")
                            {
                                Buttonparent.Style = (Style)FindResource("Content_MainButtonNextStyle03");
                            }
                            else if (Util.NVC(drButton["PRDT_CLSS_CODE"]) == "BMA")
                            {
                                Buttonparent.Style = (Style)FindResource("Content_MainButtonNextStyle04");
                            }
                            else
                            {
                                Buttonparent.Style = (Style)FindResource("Content_MainButtonNextStyle01");
                            }
                        }
                        else
                        {
                            if (Util.NVC(drButton["PCSGID"]) == "C")
                            {
                                Buttonparent.Style = (Style)FindResource("Content_MainButtonNextStyle02");
                            }
                            else if (Util.NVC(drButton["PCSGID"]) == "M")
                            {
                                Buttonparent.Style = (Style)FindResource("Content_MainButtonNextStyle03");
                            }
                            else if (Util.NVC(drButton["PCSGID"]) == "P")
                            {
                                Buttonparent.Style = (Style)FindResource("Content_MainButtonNextStyle04");
                            }
                            else
                            {
                                Buttonparent.Style = (Style)FindResource("Content_MainButtonNextStyle01");
                            }
                        }

                        //라벨발행공정만 활성화하는경우
                        if (sLabelPrintUse.Equals("Y"))
                        {
                            if (Util.NVC(drButton["LABELPRINTIUSE"]) != "Y")
                            {
                                Buttonparent.Style = (Style)FindResource("Content_MainButtonStyle");
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }


        #endregion

        
    }
}
