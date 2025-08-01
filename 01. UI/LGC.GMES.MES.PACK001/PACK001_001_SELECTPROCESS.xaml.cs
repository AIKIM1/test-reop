﻿/*************************************************************************************
 Created Date : 2016.08.01
      Creator : Jeong Hyeon Sik
   Decription : 공정 선택 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.01  Jeong Hyeon Sik : Initial Created.





 
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
    public partial class PACK001_001_SELECTPROCESS : C1Window, IWorkArea
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
        

        public PACK001_001_SELECTPROCESS()
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
                


                //사용자 권한의 팩 라인 조회되도록 해야함 ... 현재 임시로 하드코딩
                CommonCombo _combo = new CommonCombo();
                //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, null, null
                //    , new string[] { LoginInfo.CFG_AREA_ID }
                //    , null);
                this.cboEquipmentSegment.SelectedValueChanged -= new System.EventHandler<C1.WPF.PropertyChangedEventArgs<object>>(this.cboEquipmentSegment_SelectedValueChanged);
                string[] area = { LoginInfo.CFG_AREA_ID.ToString() };
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, sFilter: area);
                this.cboEquipmentSegment.SelectedValueChanged += new System.EventHandler<C1.WPF.PropertyChangedEventArgs<object>>(this.cboEquipmentSegment_SelectedValueChanged);

                setProcessList();
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }
        //private void cboEquipmentSegment_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        //{
        //    try
        //    {
        //        setProcessList();
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.Alert(ex.ToString());
        //    }
        //}

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                setProcessList();
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (drSelectProcessInfo == null || !(txtSelectedProcess.Text.Length > 0))
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1459"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    Util.Alert("SFU1459");
                    //공정을 선택하세요
                    return;
                }

                if (string.IsNullOrEmpty(cboEquipment.Text) || !(cboEquipment.Text.Length > 0))
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1459"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    Util.Alert("SFU1153");
                    //설비을 선택하세요
                    return;
                }

                DrSelectedProcessInfo = drSelectProcessInfo;
                sSelectedProcessTitle = "" + drSelectProcessInfo["EQSGNAME"].ToString() + "-" + drSelectProcessInfo["PROCNAME"].ToString();
                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
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
                //Util.Alert(ex.ToString());
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
                //setCboEqpt(Util.NVC(drSelectProcessInfo["EQSGID"]), Util.NVC(drSelectProcessInfo["PROCID"]));
                CommonCombo _combo = new CommonCombo();
                _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.NONE, null, null
                    , new string[] { Util.NVC(drSelectProcessInfo["EQSGID"]), Util.NVC(drSelectProcessInfo["PROCID"]) , null }
                    , sCase: "EQUIPMENT_BY_EQPTID");

                //buttonColorChange(Util.NVC(drSelectProcessInfo["EQSGID"]), Util.NVC(drSelectProcessInfo["PROCID"]));
                buttonColorChange(drSelectProcessInfo);
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
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
                //Util.Alert(ex.ToString());
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
                //DataTable dtLineProcessList = getLineProcessList();
                //if (dtLineProcessList != null)
                //{
                //    if (dtLineProcessList.Rows.Count > 0)
                //    {
                //        setGridProcessList(dtLineProcessList);
                //    }
                //}
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
                DataSet dsINDATA = new DataSet();
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("SHOPID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("AREA_TYPE_CODE", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));


                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["AREA_TYPE_CODE"] = Area_Type.PACK;
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString() == "" ? null : cboEquipmentSegment.SelectedValue;
                INDATA.Rows.Add(dr);

                dsINDATA.Tables.Add(INDATA);
                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("BR_PRD_SEL_PROCESS_LIST_PACK", "INDATA", "OUTDATA,OUTDATA_EQSG", (dsResult, dataException) =>
                {
                    try
                    {
                        

                        if (dataException != null)
                        {
                            //Util.AlertByBiz("BR_PRD_SEL_PROCESS_LIST_PACK", dataException.Message, dataException.ToString());
                            Util.MessageException(dataException);
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            return;
                        }
                        else
                        {
                            if (dsResult != null && dsResult.Tables.Count > 0)
                            {
                                Clear_SelectProc();

                                setGridProcessList_DataSet(dsResult);
                            }
                        }
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        return;
                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        throw ex;
                    }

                }, dsINDATA);


                //if (dtLineProcessList != null)
                //{
                //    if (dtLineProcessList.Rows.Count > 0)
                //    {
                //        setGridProcessList(dtLineProcessList);
                //    }
                //}

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

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
                    //string sPrjName = (dtLineList.Rows[i]["PROD_PRJ_MODLID"].ToString().Replace(" ", "_")).Replace("-", "_").Replace("(", "_").Replace(")", "_");

                    string sPrjName = dtLineList.Rows[i]["PROD_PRJ_MODLID"].ToString();
                    sPrjName = Regex.Replace(sPrjName, @"[^0-9a-zA-Z]", "_");
                    

                    grdSubGridNew.Name = dtLineList.Rows[i]["EQSGID"].ToString() + "_" + sPrjName;
                    grdSubGridNew.Tag = dtLineList.Rows[i]["EQSGID"] + "_" + dtLineList.Rows[i]["PROD_PRJ_MODLID"].ToString();

                    //데이터 line의 공정 추출
                    DataTable myNewTable = dtLineProcessList.Select("EQSGID = '" + dtLineList.Rows[i]["EQSGID"].ToString() 
                        + "' AND PROD_PRJ_MODLID= '" + dtLineList.Rows[i]["PROD_PRJ_MODLID"].ToString() + "'").CopyToDataTable();
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

                    //라인-PROJECT명
                    System.Windows.Controls.TextBlock newTextBlock_ProdID = new TextBlock();
                    newTextBlock_ProdID.Text = dtLineList.Rows[i]["PROD_PRJ_MODLID"].ToString();
                    newTextBlock_ProdID.HorizontalAlignment = HorizontalAlignment.Center;
                    newTextBlock_ProdID.Style = (Style)FindResource("Content_InputForm_LabelStyle");
                    newTextStackPanel.Children.Add(newTextBlock_ProdID);

                    //라인-선택유무 
                    String sWoExist = dtLineList.Rows[i]["WO_EXIST"].ToString();
                    String sCMA_WO_TYPE = Util.NVC(dtLineList.Rows[i]["CMA_WO_TYPE"]);
                    String sBMA_WO_TYPE = Util.NVC(dtLineList.Rows[i]["BMA_WO_TYPE"]);


                    if (sCMA_WO_TYPE == "PPRW" || sBMA_WO_TYPE == "PPRW")
                    {
                        System.Windows.Controls.TextBlock newTextBlock_Wo = new TextBlock();
                        newTextBlock_Wo.Text = ObjectDic.Instance.GetObjectName("재작업");
                        newTextBlock_Wo.HorizontalAlignment = HorizontalAlignment.Center;
                        newTextBlock_Wo.Style = (Style)FindResource("SearchCondition_MandatoryMarkStyle");
                        newTextStackPanel.Children.Add(newTextBlock_Wo);
                    }
                    else if (sWoExist == "N"
                        && !(sCMA_WO_TYPE.Length > 0)
                        && !(sBMA_WO_TYPE.Length > 0)
                        )
                    {
                        System.Windows.Controls.TextBlock newTextBlock_Wo = new TextBlock();
                        newTextBlock_Wo.Text = ObjectDic.Instance.GetObjectName("작업지시정보없음");
                        newTextBlock_Wo.HorizontalAlignment = HorizontalAlignment.Center;
                        newTextBlock_Wo.Style = (Style)FindResource("SearchCondition_MandatoryMarkStyle");
                        newTextStackPanel.Children.Add(newTextBlock_Wo);
                    }
                    

                    grdSubGridTitle.SetValue(Grid.RowProperty, 0);
                    grdSubGridTitle.SetValue(Grid.ColumnProperty, 0);

                    grdSubGridNew.Children.Add(newTextStackPanel);
                    //grdSubGridNew.Children.Add(grdSubGridTitle);
                    #endregion

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
                        ;

                        System.Windows.Controls.Button newBtn = new Button();
                        newBtn.Content = myNewTable.Rows[c]["PROCNAME"].ToString();
                        newBtn.Name = "Button_" 
                            + Util.NVC(myNewTable.Rows[c]["EQSGID"]) 
                            + "_" + Util.NVC(myNewTable.Rows[c]["PROCID"]) 
                            //+ "_" + (myNewTable.Rows[c]["PROD_PRJ_MODLID"].ToString().Replace(" ", "_")).Replace("-", "_").Replace("(", "_").Replace(")", "_");
                            +"_" + Regex.Replace(dtLineList.Rows[i]["PROD_PRJ_MODLID"].ToString(), @"[^0-9a-zA-Z]", "_");
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
                        //작업지시 선택 정보 없는경우 Enable False 
                        //EX)BMA생산 라인= CMA오더 + BMA 오더 가 존재해야 'Y'
                        //단,재작업 오더가 존재하는경우 재작업오더의 공정군만 활성화.
                        //CMI인경우 부분 생산이 가능하다하여 오더 물린곳만 오픈되도록 수정함 2017 03 23
                        //아래코드 수정시.. 클릭이벤트에 버튼 모양변경하는함수 buttonColorChange() 도.. 수정해야함!!!
                        if (sWoExist == "N")
                        {
                            bool bCheck = true;
                            if (Util.NVC(myNewTable.Rows[c]["PCSGID"]) == "C"
                                && Util.NVC(myNewTable.Rows[c]["CMA_WO_TYPE"]).Length > 0)
                            //&& Util.NVC(myNewTable.Rows[c]["CMA_WO_TYPE"]) == "PPRW")
                            {
                                bCheck = false;
                            }
                            else if (Util.NVC(myNewTable.Rows[c]["PCSGID"]) == "M"
                                && Util.NVC(myNewTable.Rows[c]["CMA_WO_TYPE"]).Length > 0)
                            //&& Util.NVC(myNewTable.Rows[c]["CMA_WO_TYPE"]) == "PPRW")
                            {
                                bCheck = false;
                            }
                            else if (Util.NVC(myNewTable.Rows[c]["PCSGID"]) == "P"
                                && Util.NVC(myNewTable.Rows[c]["BMA_WO_TYPE"]).Length > 0)
                            //&& Util.NVC(myNewTable.Rows[c]["BMA_WO_TYPE"]) == "PPRW")
                            {
                                bCheck = false;
                            }

                            if(bCheck)
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
                        + "_" + Util.NVC(drSelectProcessInfo["PROCID"])
                        + "_" + Regex.Replace(drSelectProcessInfo["PROD_PRJ_MODLID"].ToString(), @"[^0-9a-zA-Z]", "_"))
                    //+ "_" + (Util.NVC(drSelectProcessInfo["PROD_PRJ_MODLID"]).Replace(" ", "_")).Replace("-","_"))
                    {
                        Buttonparent.Style = (Style)FindResource("Content_MainButtonNextStyle05");

                        //Buttonparent.Style = (Style)FindResource("Content_MainButtonNextStyle");
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
                        //작업지시없는경우
                        if(Util.NVC(drButton["WO_EXIST"]) != "Y")
                        {
                            bool bCheck = true;
                            if (Util.NVC(drButton["PCSGID"]) == "C"
                                && Util.NVC(drButton["CMA_WO_TYPE"]).Length > 0)
                                //&& Util.NVC(drButton["CMA_WO_TYPE"]) == "PPRW")
                            {
                                bCheck = false;
                            }
                            else if (Util.NVC(drButton["PCSGID"]) == "M"
                                && Util.NVC(drButton["CMA_WO_TYPE"]).Length > 0)
                                //&& Util.NVC(drButton["CMA_WO_TYPE"]) == "PPRW")
                            {
                                bCheck = false;
                            }
                            else if (Util.NVC(drButton["PCSGID"]) == "P"
                                && Util.NVC(drButton["BMA_WO_TYPE"]).Length > 0)
                                //&& Util.NVC(drButton["BMA_WO_TYPE"]) == "PPRW")
                            {
                                bCheck = false;
                            }

                            if (bCheck)
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
