/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.


**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Data;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.BOX001
{

    /// <summary>
    /// BOX001_023.xaml에 대한 상호 작용 논리
    /// </summary>
    /// 

    public partial class BOX001_023 : UserControl, IWorkArea
    {
        CommonCombo combo = new CommonCombo();
        Util _Util = new Util();
        double sum = 0;
        double totalqty2 = 0;
        #region 

    
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public BOX001_023()
        {
            InitializeComponent();
        }

        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnReceive);
            listAuth.Add(btnReturn);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

        }
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            initCombo();
            initGridTable();

            dgReceive.ClipboardPasteMode = DataGridClipboardMode.None;
            rdoPancake.IsChecked = true;
            rdoPancakeReturn.IsChecked = true;


        }
        private void initCombo()
        {

            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbChild: null, cbParent: cboEquipmentSegmentParent, sCase: "EQUIPMENTSEGMENT");

            //공정
            combo.SetCombo(cboProcid, CommonCombo.ComboStatus.SELECT, cbChild: null, cbParent: cboEquipmentSegmentParent);

            string[] sFilters = { LoginInfo.CFG_SHOP_ID, "OUTSD_ELTR_TYPE_CODE" };
            //combo.SetCombo(cboInputArea, CommonCombo.ComboStatus.SELECT, sFilter: sFilters);

            combo.SetCombo(cboPancake, CommonCombo.ComboStatus.ALL, sFilter: sFilters, sCase: "COMMCODES");
        }

        #region[입고]
      
        private void Initialize()
        {
            dgReceive.Loaded += dgReceive_Loaded;
        }

        private void dgReceive_Loaded(object sender, RoutedEventArgs e)
        {
            dgReceive.Loaded -= dgReceive_Loaded;
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                Initialize_dgReceive();
            }));
        }

        private void dgReceive_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.V && KeyboardUtil.Ctrl)
                {
                    DataTable dt = DataTableConverter.Convert(dgReceive.ItemsSource);

                    for (int i = dgReceive.GetRowCount() - 1; i >= 0; i--)
                    {
                        dt.Rows[i].Delete();
                    }

                    string text = Clipboard.GetText();
                    string[] table = text.Split('\n');

                    if (table == null)
                    {
                        Util.MessageValidation("SFU1482");   //다시 복사 해주세요.
                        return;
                    }
                    if (table.Length == 1)
                    {
                        Initialize_dgReceive();
                        Util.MessageValidation("SFU1482");   //다시 복사 해주세요.
                        return;
                    }

                    if (rdoPancake.IsChecked == false && rdoRoll.IsChecked == false)
                    {
                        Util.MessageValidation("SFU3433");//Roll/Pancake 중 선택해주세요
                        return;
                    }


                    for (int i = 0; i < table.Length - 1; i++)
                    {
                        string[] rw = table[i].Split('\t');
                        if (rw == null)
                        {
                            Util.MessageValidation("SFU1498");   //데이터가 없습니다.
                            return;
                        }
                        if (rdoPancake.IsChecked == true ?  (rw.Length != 12) : (rw.Length != 10))
                        {
                            Util.MessageValidation("SFU1532");   //모든 항목을 다 복사해주세요.
                            Initialize_dgReceive();
                            return;
                        }

                        DataRow row = dt.NewRow();
                        row["CHK"] = true;
                        row["LOTID"] = rw[0];
                        row["PROJECTNAME"] = rw[1];
                        row["PRODID"] = rw[2];
                        //if (rw[3].Equals(""))
                        //{
                        //    Util.Alert("SFU1564");   //버전 항목 값이 없습니다.
                        //    Initialize_dgReceive();
                        //    return;
                        //}
                        row["VERSION"] = rw[3];//rw[3].Substring(0, rw[3].Length-2);// -E 제거
                        row["LOTID_RT"] = rw[4];
                        //row["WIPDTTM"] = rw[5];

                        try
                        {
                            if (rdoPancake.IsChecked == true)
                            {
                                int.Parse(Convert.ToString(rw[7]));
                                int.Parse(Convert.ToString(rw[8]));
                                int.Parse(Convert.ToString(rw[9]));
                                int.Parse(Convert.ToString(rw[10]));
                            }
                            else
                            {
                                int.Parse(Convert.ToString(rw[5]));
                                int.Parse(Convert.ToString(rw[6]));
                                int.Parse(Convert.ToString(rw[7]));
                                int.Parse(Convert.ToString(rw[8]));
                            }
                       

                        } catch (Exception ex)
                        {
                            Util.MessageValidation("SFU3435"); //숫자만 입력해주세요
                            return;
                            //Util.MessageException(ex);
                        }

                        if (rdoPancake.IsChecked == true)
                        {
                            row["WIPDTTM"] = rw[5];
                            row["SLITTCUTLOT"] = rw[6];
                            row["SLITTINGDATE"] = rw[7];
                            row["LANE_QTY"] = rw[8];
                            row["WIPQTYPTN"] = rw[9];
                            row["WIPQTYCELL"] = rw[10];
                            row["NOTE"] = rw[11];
                        }
                        else if(rdoRoll.IsChecked == true)
                        {
                            row["WIPDTTM"] = rw[5];
                            row["LANE_QTY"] = rw[6];
                            row["WIPQTYPTN"] = rw[7];
                            row["WIPQTYCELL"] = rw[8];
                            row["NOTE"] = rw[9];
                        }

                        dt.Rows.Add(row);
                    }

                    dgReceive.BeginEdit();
                    dgReceive.ItemsSource = DataTableConverter.Convert(dt);
                    dgReceive.EndEdit();


                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
                Initialize_dgReceive();

            }
        }
        private void Initialize_dgReceive()
        {
            dgReceive.ItemsSource = null;

            DataTable dt = new DataTable();
            dt.Columns.Add("CHK", typeof(bool));
            dt.Columns.Add("LOTID", typeof(string));
            dt.Columns.Add("PROJECTNAME", typeof(string));
            dt.Columns.Add("PRODID", typeof(string));
            dt.Columns.Add("VERSION", typeof(string));
            dt.Columns.Add("LOTID_RT", typeof(string));
            dt.Columns.Add("WIPDTTM", typeof(string));
            dt.Columns.Add("RPRESSDTTM", typeof(string));
            dt.Columns.Add("SLITTCUTLOT", typeof(string));
            dt.Columns.Add("SLITTINGDATE", typeof(string));
            dt.Columns.Add("LANE_QTY", typeof(string));
            dt.Columns.Add("WIPQTYPTN", typeof(string));
            dt.Columns.Add("WIPQTYCELL", typeof(string));
            dt.Columns.Add("NOTE", typeof(string));

            DataRow row = dt.NewRow();
            row["CHK"] = false;
            row["LOTID"] = "";
            row["PROJECTNAME"] = "";
            row["PRODID"] = "";
            row["VERSION"] = "";
            row["LOTID_RT"] = "";
            row["WIPDTTM"] = "";
            row["RPRESSDTTM"] = "";
            row["SLITTCUTLOT"] = "";
            row["SLITTINGDATE"] = "";
            row["LANE_QTY"] = "";
            row["WIPQTYPTN"] = "";
            row["WIPQTYCELL"] = "";
            row["NOTE"] = "";

            dt.Rows.Add(row);

            dgReceive.BeginEdit();
            dgReceive.ItemsSource = DataTableConverter.Convert(dt);
            dgReceive.EndEdit();

        }

        private void btnReceive_Click(object sender, RoutedEventArgs e)
        {
            if (cboArea.Text.Equals("-SELECT-"))
            {
                Util.MessageValidation("SFU1799");   //입고 할 동을 선택해주세요.
                return;
            }

            if (cboProcid.Text.Equals("-SELECT-"))
            {
                Util.MessageValidation("SFU1795");   //입고 공정을 선택해주세요.
                return;
            }
            if (_Util.GetDataGridCheckFirstRowIndex(dgReceive, "CHK") == -1)
            {
                Util.MessageValidation("SFU1632");   //선택된 LOT이 없습니다.
                return;
            }
            if (rdoPancake.IsChecked == false && rdoRoll.IsChecked == false)
            {
                Util.MessageValidation("SFU3433"); //Roll/Pancake중 선택해주세요
                return;
            }

            //입고 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2073"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("WO_DETL_ID", typeof(string));
                        inData.Columns.Add("PRODID", typeof(string));
                        inData.Columns.Add("BOMREV", typeof(string));
                        inData.Columns.Add("PROCID", typeof(string));
                        inData.Columns.Add("PCSGID", typeof(string));
                        inData.Columns.Add("EQSGID", typeof(string));
                        inData.Columns.Add("EQPTID", typeof(string));
                        inData.Columns.Add("RECIPEID", typeof(string));
                        //inData.Columns.Add("PROD_VER_CODE", typeof(string));
                        inData.Columns.Add("IFMODE", typeof(string));
                        inData.Columns.Add("TRSF_POST_FLAG", typeof(string));
                        inData.Columns.Add("AREAID", typeof(string));
                        inData.Columns.Add("SLOC_ID", typeof(string));
                        inData.Columns.Add("TOTAL_QTY", typeof(string));
                        inData.Columns.Add("TOTAL_QTY2", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));
                        inData.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                        inData.Columns.Add("NOTE", typeof(string));
                        inData.Columns.Add("OUTSD_ELTR_TYPE_CODE", typeof(string));

                        string[] str = Convert.ToString(cboProcid.SelectedValue).Split('|');
                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["WO_DETL_ID"] = "";//없음
                        row["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[_Util.GetDataGridCheckFirstRowIndex(dgReceive, "CHK")].DataItem, "PRODID"));
                        row["BOMREV"] = "";//없음
                        row["PROCID"] = str[1];//Convert.ToString(cboProcid.SelectedValue);
                        row["PCSGID"] = str[0];// cbo
                        row["EQSGID"] = Convert.ToString(cboEquipmentSegment.SelectedValue);
                        row["EQPTID"] = ""; //없음
                        row["RECIPEID"] = ""; //없음
                        //row["PROD_VER_CODE"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[_Util.GetDataGridCheckFirstRowIndex(dgReceive, "CHK")].DataItem, "VERSION")).Replace("-E", "");
                        row["IFMODE"] = "";//없음
                        row["TRSF_POST_FLAG"] = "N";
                        row["AREAID"] = Convert.ToString(cboArea.SelectedValue);
                        row["SLOC_ID"] = "";//없음
                        row["TOTAL_QTY"] = 0;//없음 // 이전전기할때 필요
                        row["TOTAL_QTY2"] = 0;//없음
                        row["USERID"] = LoginInfo.USERID;
                        row["PRDT_CLSS_CODE"] = "";
                        row["NOTE"] = new TextRange(rtxRemark.Document.ContentStart, rtxRemark.Document.ContentEnd).Text;
                        row["OUTSD_ELTR_TYPE_CODE"] = rdoPancake.IsChecked == true ? "P" : "R";

                        inData.Rows.Add(row);


                        DataTable inLot = indataSet.Tables.Add("INLOT");
                        inLot.Columns.Add("LOTID", typeof(string));
                        inLot.Columns.Add("LOTTYPE", typeof(string));
                        inLot.Columns.Add("LOTID_RT", typeof(string));
                        inLot.Columns.Add("ACTQTY", typeof(string));
                        inLot.Columns.Add("ACTQTY2", typeof(string));
                        inLot.Columns.Add("ACTUNITQTY", typeof(string));
                        inLot.Columns.Add("PR_LOTID", typeof(string));
                        inLot.Columns.Add("WIPNOTE", typeof(string));
                        inLot.Columns.Add("WIP_TYPE_CODE", typeof(string));
                        inLot.Columns.Add("HOTFLAG", typeof(string));
                        inLot.Columns.Add("PROD_LOTID", typeof(string));
                        inLot.Columns.Add("RT_LOT_CR_DTTM", typeof(string));
                        inLot.Columns.Add("PRJT_NAME", typeof(string));
                        inLot.Columns.Add("ROLLPRESS_DATE", typeof(string));
                        inLot.Columns.Add("SLIT_CUT_ID", typeof(string));
                        inLot.Columns.Add("SLIT_DATE", typeof(string));
                        inLot.Columns.Add("LANE_QTY", typeof(string));
                        inLot.Columns.Add("PROD_VER_CODE", typeof(string));

                        for (int i = 0; i < dgReceive.GetRowCount(); i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "CHK")).Equals("True") || Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "CHK")).Equals("1"))
                            {
                                row = inLot.NewRow();
                                row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "LOTID"));
                                row["LOTTYPE"] = "P";
                                row["LOTID_RT"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "LOTID_RT"));
                                row["ACTQTY"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "WIPQTYPTN"));
                                row["ACTQTY2"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "WIPQTYCELL"));
                                row["ACTUNITQTY"] = 0;//없고
                                row["PR_LOTID"] = "";//없고
                                row["WIPNOTE"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "NOTE"));
                                row["WIP_TYPE_CODE"] = "IN";
                                row["HOTFLAG"] = "";//없고
                                row["PROD_LOTID"] = "";//없고
                                row["RT_LOT_CR_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "WIPDTTM")).Replace("-", "");
                                row["PRJT_NAME"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "PROJECTNAME"));
                                row["ROLLPRESS_DATE"] = "";//;Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "RPRESSDTTM")).Replace("-", "");
                                row["SLIT_CUT_ID"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "SLITTCUTLOT"));
                                row["SLIT_DATE"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "SLITTINGDATE")).Replace("-", "");
                                row["LANE_QTY"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "LANE_QTY"));
                                row["PROD_VER_CODE"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "VERSION"));
                                inLot.Rows.Add(row);
                            }
                        }



                        loadingIndicator.Visibility = Visibility.Visible;

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RECEIVE_MTRL", "INDATA,INLOT", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    //Util.AlertByBiz("BR_PRD_REG_RECEIVE_MTRL", bizException.Message, bizException.ToString());
                                    Util.MessageException(bizException);
                                    return;
                                }

                                Util.MessageInfo("SFU1798");   //입고 처리 되었습니다.
                                Initialize_dgReceive();
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                            }

                        }, indataSet);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }

                }
            });
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //삭제 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        DataTable dt = DataTableConverter.Convert(dgReceive.ItemsSource);

                        for (int i = dt.Rows.Count - 1; i >= 0; i--)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "CHK")).Equals("True") || Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "CHK")).Equals("1"))
                            {
                                dt.Rows[i].Delete();
                            }
                        }

                        dgReceive.ItemsSource = DataTableConverter.Convert(dt);
                        if (dgReceive.GetRowCount() == 0)
                        {
                            Initialize_dgReceive();
                        }

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }

                }
            });
        }
        #endregion

        #region[반품]
        private void initGridTable()
        {
            DataTable dt = new DataTable();
            foreach (C1.WPF.DataGrid.DataGridColumn col in dgReturn.Columns)
            {
                dt.Columns.Add(Convert.ToString(col.Name));
            }
            dgReturn.BeginEdit();
            dgReturn.ItemsSource = DataTableConverter.Convert(dt);
            dgReturn.EndEdit();
        }

        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!ValidationRTN())
                {
                    return;
                }

                AddReturnRow();
            }
        }


        private void txtSkidID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!ValidationRTN())
                {
                    return;
                }

                AddReturnRow();
            }
        }

        private bool ValidationRTN()
        {
            if (rdoPancakeReturn.IsChecked == false && rdoRollReturn.IsChecked == false)
            {
                Util.MessageValidation("SFU3433"); //roll/pancake중 선택해주세요.
                return false;
            }

            DataTable dt = new DataTable();
            dt.Columns.Add("LOTID", typeof(string));
            dt.Columns.Add("CSTID", typeof(string));

            DataRow row = dt.NewRow();
            row["LOTID"] = txtLotID.Text == "" ? null : txtLotID.Text;
            row["CSTID"] = txtSkidID.Text == "" ? null : txtSkidID.Text;
            dt.Rows.Add(row);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_OUTSD_ELTR_RTN_HIST_DETL", "RQSTDT", "RSLTDT", dt);
            if (result.Rows.Count != 0)
            {
                Util.MessageValidation("SFU1775");   //이미 반품 된 LOT입니다.
                txtLotID.Text = "";
                txtSkidID.Text = "";
                return false;
            }

            return true;
        }

        private void AddReturnRow()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("LANGID", typeof(string));
            dt.Columns.Add("LOTID", typeof(string));
            dt.Columns.Add("CSTID", typeof(string));
            dt.Columns.Add("TYPE", typeof(string));

            DataRow row = dt.NewRow();
            row["LANGID"] = LoginInfo.LANGID;
            row["LOTID"] = txtLotID.Text == "" ? null : txtLotID.Text;
            row["CSTID"] = txtSkidID.Text == "" ? null : txtSkidID.Text;
            row["TYPE"] = rdoRollReturn.IsChecked == true ? "R" : rdoPancakeReturn.IsChecked == true ? "P" : null;
            dt.Rows.Add(row);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_OUTSD_ELTR_RCV_HIST_DETL_BYDATE", "RQSTDT", "RSLTDT", dt);
            if (result.Rows.Count == 0)
            {
                dgReturn.ItemsSource = null;
                initGridTable();
                txtLotID.Text = "";
                txtSkidID.Text = "";
                //FrameOperation.PrintMessage(result.Rows.Count + "건이 조회되었습니다.");
                return;
            }

            for (int i = 0; i < dgReturn.Rows.Count; i++)
            {
                for (int j = 0; j < result.Rows.Count; j++)
                {
                    if (result.Rows[j]["LOTID"].Equals(Util.NVC(DataTableConverter.GetValue(dgReturn.Rows[i].DataItem, "LOTID"))))
                    {
                        Util.Alert("SFU2014");   //해당 LOT이 이미 존재합니다.
                        txtLotID.Text = "";
                        txtSkidID.Text = "";
                        return;
                    }
                }
            }

            for (int i = 0; i < result.Rows.Count; i++)
            {
                dgReturn.IsReadOnly = false;
                dgReturn.BeginNewRow();
                dgReturn.EndNewRow(true);
                DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "CHK", true);
                DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "OUTSD_ELTR_RCV_ID", Convert.ToString(result.Rows[i]["OUTSD_ELTR_RCV_ID"]));
                DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "LOTID", Convert.ToString(result.Rows[i]["LOTID"]));
                DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "PRJT_NAME", Convert.ToString(result.Rows[i]["PRJT_NAME"]));
                DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "PRODID", Convert.ToString(result.Rows[i]["PRODID"]));
                DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "PROD_VER_CODE", Convert.ToString(result.Rows[i]["PROD_VER_CODE"]));
                DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "LOTID_RT", Convert.ToString(result.Rows[i]["LOTID_RT"]));
                DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "LOTID_RT_GNRT_DATE", Convert.ToString(result.Rows[i]["LOTID_RT_GNRT_DATE"]));
                DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "LANE_QTY", Convert.ToString(result.Rows[i]["LANE_QTY"]).Equals("") ? 1.ToString() : Convert.ToString(result.Rows[i]["LANE_QTY"]));
                DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "RCV_QTY", Convert.ToString(result.Rows[i]["RCV_QTY"]));
                DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "RCV_QTY2", Convert.ToString(result.Rows[i]["RCV_QTY2"]));
                DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "INSDTTM", Convert.ToString(result.Rows[i]["INSDTTM"]));
                DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "INSUSER", Convert.ToString(result.Rows[i]["INSUSER"]));
            }

            dgReturn.Columns[0].IsReadOnly = false;
            for (int i = 0; i < dgReturn.Columns.Count; i++)
            {
                if (i != 0)
                {
                    dgReturn.Columns[i].IsReadOnly = true;
                }
                
            }
          

            txtLotID.Text = "";
            txtSkidID.Text = "";
        }
        private void btnDeleteReturn_Click(object sender, RoutedEventArgs e)
        {
            //삭제 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        DataTable dt = DataTableConverter.Convert(dgReturn.ItemsSource);

                        for (int i = dt.Rows.Count - 1; i >= 0; i--)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgReturn.Rows[i].DataItem, "CHK")).Equals("True") || Util.NVC(DataTableConverter.GetValue(dgReturn.Rows[i].DataItem, "CHK")).Equals("1"))
                            {
                                dt.Rows[i].Delete();
                            }
                        }

                        dgReturn.ItemsSource = DataTableConverter.Convert(dt);
                        if (dgReturn.ItemsSource == null)
                        {
                            initGridTable();
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        //Util.Alert(ex.ToString());
                    }

                }
            });
          
        
        }
        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            if (rdoPancakeReturn.IsChecked == false && rdoRollReturn.IsChecked == false)
            {
                Util.MessageValidation("SFU3433"); //Roll/Pancake중 선택해주세요
                return;
            }

            //반품 처리 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2074"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("AREAID", typeof(string));
                        inData.Columns.Add("NOTE", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));
                        inData.Columns.Add("OUTSD_ELTR_TYPE_CODE", typeof(string));

                        DataRow row = inData.NewRow();
                        //row["AREAID"] = SRCTYPE.SRCTYPE_UI;
                        row["AREAID"] = LoginInfo.CFG_AREA_ID;
                        row["NOTE"] = "";//없음
                        row["USERID"] = LoginInfo.USERID;
                        row["OUTSD_ELTR_TYPE_CODE"] = rdoPancakeReturn.IsChecked == true ? "P" : "R";
                        inData.Rows.Add(row);


                        DataTable inLot = indataSet.Tables.Add("INLOT");
                        inLot.Columns.Add("LOTID", typeof(string));
                        inLot.Columns.Add("PRJT_NAME", typeof(string));
                        inLot.Columns.Add("PRODID", typeof(string));
                        inLot.Columns.Add("PROD_VER_CODE", typeof(string));
                        inLot.Columns.Add("LOTID_RT", typeof(string));
                        inLot.Columns.Add("LOTID_RT_GNRT_DATE", typeof(string));
                        inLot.Columns.Add("RTN_QTY", typeof(Decimal));
                        inLot.Columns.Add("RTN_QTY2", typeof(Decimal));
                        inLot.Columns.Add("OUTSD_ELTR_RCV_ID", typeof(string));

                        for (int i = 0; i < dgReturn.GetRowCount(); i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgReturn.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgReturn.Rows[i].DataItem, "CHK")).Equals("True"))
                            {
                                row = inLot.NewRow();
                                row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgReturn.Rows[i].DataItem, "LOTID"));
                                row["PRJT_NAME"] = Util.NVC(DataTableConverter.GetValue(dgReturn.Rows[i].DataItem, "PRJT_NAME"));
                                row["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgReturn.Rows[i].DataItem, "PRODID"));
                                row["PROD_VER_CODE"] = Util.NVC(DataTableConverter.GetValue(dgReturn.Rows[i].DataItem, "PROD_VER_CODE"));
                                row["LOTID_RT"] = Util.NVC(DataTableConverter.GetValue(dgReturn.Rows[i].DataItem, "LOTID_RT"));
                                row["LOTID_RT_GNRT_DATE"] = Util.NVC(DataTableConverter.GetValue(dgReturn.Rows[i].DataItem, "LOTID_RT_GNRT_DATE"));
                                row["RTN_QTY"] = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgReturn.Rows[i].DataItem, "RCV_QTY")));
                                row["RTN_QTY2"] = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgReturn.Rows[i].DataItem, "RCV_QTY2")));
                                row["OUTSD_ELTR_RCV_ID"] = Util.NVC(DataTableConverter.GetValue(dgReturn.Rows[i].DataItem, "OUTSD_ELTR_RCV_ID"));
                                inLot.Rows.Add(row);
                            }
                      
                        }


                        new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_RETURN_MTRL", "INDATA,INLOT ", null, indataSet);
                        Util.MessageInfo("SFU1557");   //반품처리 되었습니다.

                        DataTable dt =  DataTableConverter.Convert(dgReturn.ItemsSource);
                        //반품처리한 LOT만 빠지게 처리
                        for (int i = dt.Rows.Count-1; i >=0 ; i--)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgReturn.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgReturn.Rows[i].DataItem, "CHK")).Equals("True"))
                            {
                                dt.Rows[i].Delete();
                            }

                        }
                       
                        dgReturn.ItemsSource = DataTableConverter.Convert(dt);

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }

                }
            });
        }
        private void rdoRoll_Click(object sender, RoutedEventArgs e)
        {
            if (rdoRoll.IsChecked == true)
            {
                SlitCut.Visibility = Visibility.Collapsed;
                SlitDate.Visibility = Visibility.Collapsed;
            }
            else
            {
                SlitCut.Visibility = Visibility.Visible;
                SlitDate.Visibility = Visibility.Visible;
            }
            Initialize_dgReceive();
        }

        private void btnReturnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (rdoRollReturn.IsChecked == false && rdoPancakeReturn.IsChecked == false)
                {
                    Util.MessageValidation("SFU3433");// Roll/Pancake을 선택해주세요
                    dgReturn.ItemsSource = null;
                    return;
                }
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("DATE_FROM", typeof(string));
                dt.Columns.Add("DATE_TO", typeof(string));
                dt.Columns.Add("TYPE", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));
                dt.Columns.Add("CSTID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["DATE_FROM"] = dtpReturnDateFrom.SelectedDateTime.ToShortDateString();
                row["DATE_TO"] = dtpReturnDateTo.SelectedDateTime.ToShortDateString();
                row["TYPE"] = rdoRollReturn.IsChecked == true ? "R" : "P";
                row["AREAID"] = LoginInfo.CFG_AREA_ID;
                row["LOTID"] = txtLotID.Text == "" ? null : txtLotID.Text;
                row["CSTID"] = txtSkidID.Text == "" ? null : txtSkidID.Text;
                dt.Rows.Add(row);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_BAS_SEL_TB_SFC_OUTSD_ELTR_RCV_HIST_DETL_BYDATE", "RSLT", "RQUST", dt, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgReturn, bizResult, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        for (int i = 0; i < dgReturn.Columns.Count; i++)
                        {
                            if (i != 0)
                            {
                                dgReturn.Columns[i].IsReadOnly = true;
                            }

                        }
                    }

                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion


        #region[이력조회]
        private void C1TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabControl.SelectedIndex == 2)
            {
                dtpDateFrom.DateFormat = System.DateTime.Now.ToString("yyyy-MM-01");
                dtpDateTo.DateFormat = System.DateTime.Now.ToString("yyyy-MM-dd");

            }
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ReceiveSearchData();
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            new LGC.GMES.MES.Common.ExcelExporter().Export(dgReceive_Hist);
        }

        private void txtSearchLotid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ReceiveSearchData();
            }
        }

        private void ReceiveSearchData()
        {
            try
            {
                DataTable dt = null;
                DataRow row = null;
                DataTable result = null;

                dt = new DataTable();

                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("RCV_DATE_FROM", typeof(string));
                dt.Columns.Add("RCV_DATE_TO", typeof(string));
                dt.Columns.Add("OUTSD_ELTR_TYPE_CODE", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));

                row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["AREAID"] = LoginInfo.CFG_AREA_ID;

                if (txtSearchLotid.Text.Equals(""))
                {
                    row["RCV_DATE_FROM"] = dtpDateFrom.SelectedDateTime.ToShortDateString();
                    row["RCV_DATE_TO"] = dtpDateTo.SelectedDateTime.ToShortDateString();
                    row["OUTSD_ELTR_TYPE_CODE"] = Convert.ToString(cboPancake.SelectedValue).Equals("") ? null : Convert.ToString(cboPancake.SelectedValue);
                }
                else
                {
                    row["LOTID"] = txtSearchLotid.Text.Equals("") ? null : txtSearchLotid.Text;
                }

                dt.Rows.Add(row);

                row["LOTID"] = txtSearchLotid.Text.Equals("") ? null : txtSearchLotid.Text;
                result = rdoReceive.IsChecked == true ? new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_OUTSD_ELTR_RCV_HIST_FOR_TERM", "RQSTDT", "RSLTDT", dt) : new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_OUTSD_ELTR_RTN_HIST_FOR_TERM", "RQSTDT", "RSLTDT", dt);
                if (result.Rows.Count == 0)
                {
                    dgReceive_Hist.ItemsSource = null;
                }

                //dgReceive_Hist.ItemsSource = DataTableConverter.Convert(result);
                Util.GridSetData(dgReceive_Hist, result, FrameOperation);
                txtSearchLotid.Text = "";
                     
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion


        #region[전체선택]

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };
        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgReceive.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgReceive.Rows[i].DataItem, "CHK", true);
                }
            }
            
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgReceive.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgReceive.Rows[i].DataItem, "CHK", false);
                }
            }
        }

        private void dgReceive_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
               {
                   if (string.IsNullOrEmpty(e.Column.Name) == false)
                   {
                       if (e.Column.Name.Equals("CHK"))
                       {
                           pre.Content = chkAll;
                           e.Column.HeaderPresenter.Content = pre;
                           chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                           chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                           chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                           chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                       }
                   }
               }));
        }


        C1.WPF.DataGrid.DataGridRowHeaderPresenter preRTN = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAllRTN = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };

        private void dgReturn_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        preRTN.Content = chkAllRTN;

                        if (e.Column.HeaderPresenter == null) return;

                        e.Column.HeaderPresenter.Content = preRTN;
                        chkAllRTN.Checked -= new RoutedEventHandler(checkAllRTN_Checked);
                        chkAllRTN.Unchecked -= new RoutedEventHandler(checkAllRTN_Unchecked);
                        chkAllRTN.Checked += new RoutedEventHandler(checkAllRTN_Checked);
                        chkAllRTN.Unchecked += new RoutedEventHandler(checkAllRTN_Unchecked);
                    }
                }

            }));
        }
        void checkAllRTN_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAllRTN.IsChecked)
            {
                for (int i = 0; i < dgReturn.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgReturn.Rows[i].DataItem, "CHK", true);
                }
            }

        }
        private void checkAllRTN_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAllRTN.IsChecked)
            {
                for (int i = 0; i < dgReturn.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgReturn.Rows[i].DataItem, "CHK", false);
                }
            }
        }

        #endregion

        private void rdoReceive_Click(object sender, RoutedEventArgs e)
        {
            dgReceive_Hist.Columns["INSDTTM"].Header = ObjectDic.Instance.GetObjectName("입고일");
            dgReceive_Hist.ItemsSource = null;
        }

        private void rdoReturn_Click(object sender, RoutedEventArgs e)
        {
            dgReceive_Hist.Columns["INSDTTM"].Header = ObjectDic.Instance.GetObjectName("반품일");
            dgReceive_Hist.ItemsSource = null;
        }

        private void txtLotID_GotFocus(object sender, RoutedEventArgs e)
        {
            txtSkidID.Text = "";
        }

        private void txtSkidID_GotFocus(object sender, RoutedEventArgs e)
        {
            txtLotID.Text = "";
        }
    }
}
