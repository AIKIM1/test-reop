/*************************************************************************************
 Created Date : 2017.12.08
      Creator : 
   Decription : RFID Carrier 관리
--------------------------------------------------------------------------------------
 [Change History]
  2017.12.08  DEVELOPER : Initial Created.

**************************************************************************************/

using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;
using System.Windows.Media;
using System.Linq;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_216 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        BizDataSet _Biz = new BizDataSet();

        DataTable dtiUse = new DataTable();
        DataTable dtType = new DataTable();

        string CSTStatus = string.Empty;

        public COM001_216()
        {
            InitializeComponent();
        }
        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        private void UserControl_Initialized(object sender, EventArgs e)
        {
            Initialize();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnNew);
            listAuth.Add(btnMap);
            listAuth.Add(btnEmpty);
            listAuth.Add(btnUsing);
            listAuth.Add(btnDel);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            btnDel.Visibility = Visibility.Collapsed;

            SetEvent();

            if (LoginInfo.CFG_PROC_ID.Equals(Process.LAMINATION))
            {
                if (cboCstType != null)
                    cboCstType.SelectedValue = "MG";
            }

            if (!LoginInfo.CFG_AREA_ID.Equals(""))
            {
                if (cboArea != null)
                    cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;
            }
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            CommonCombo combo = new CommonCombo();

            //동
            combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL);

            //CST 유형
            String[] sFilter1 = { "", "CARRIER_TYPE" };
            combo.SetCombo(cboCstType, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "COMMCODES");

            //CST 상태
            String[] sFilter2 = { "", "CSTSTAT" };
            combo.SetCombo(cboCstStatus, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODES");
            
            //사용여부
            String[] sFilter3 = { "", "IUSE" };
            combo.SetCombo(cboiUse, CommonCombo.ComboStatus.ALL, sFilter: sFilter3, sCase: "COMMCODES");

            cboiUse.SelectedIndex = 1;
            cboArea.SelectedIndex = 0;

            String[] sFilter4 = { "", "CARRIER_ELEC" };
            combo.SetCombo(cboSELEC, CommonCombo.ComboStatus.ALL, sFilter: sFilter4, sCase: "COMMCODES");


            String[] sFilter5 = { "", "CARRIER_PROD" };
            combo.SetCombo(cboSPROD, CommonCombo.ComboStatus.ALL, sFilter: sFilter5, sCase: "COMMCODES");

        }

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;                      

            dtiUse = CommonCodeS("IUSE");

            if (dtiUse != null && dtiUse.Rows.Count > 0)
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CBO_CODE");
                dt.Columns.Add("CBO_NAME");

                DataRow newRow = null;

                for (int i = 0; i < dtiUse.Rows.Count; i++)
                {
                    newRow = dt.NewRow();
                    newRow.ItemArray = new object[] { dtiUse.Rows[i]["CBO_CODE"].ToString(), dtiUse.Rows[i]["CBO_NAME"].ToString() };
                    dt.Rows.Add(newRow);
                }
            }

            dtType = CommonCodeS("CARRIER_TYPE");

            if (dtType != null && dtType.Rows.Count > 0)
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CBO_CODE");
                dt.Columns.Add("CBO_NAME");

                DataRow newRow = null;

                for (int i = 0; i < dtType.Rows.Count; i++)
                {
                    newRow = dt.NewRow();
                    newRow.ItemArray = new object[] { dtType.Rows[i]["CBO_CODE"].ToString(), dtType.Rows[i]["CBO_NAME"].ToString() };
                    dt.Rows.Add(newRow);
                }                
            }
        }

        private DataTable CommonCodeS(string sType)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sType;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMM_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return dtResult;
                else
                    return null;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        private void Init()
        {
            Util.gridClear(dgSearchList);
        }
        #endregion

        #region Funct
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region Event
        private void txtCSTid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int cnt = 6;

                if (txtCSTid.Text.Trim() != "")
                {
                    if (txtCSTid.Text.Trim().Length < cnt)
                    {
                        //SFU4342	[%1] 자리수 이상 입력하세요.
                        Util.MessageInfo("SFU4342", new object[] { cnt });
                        return;
                    }
                    else
                    {
                        Cst_Info("CSTID", txtCSTid.Text.ToString());
                    }
                }
            }
        }

        private void txtSLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtSLotID.Text.Trim() != "")
                {
                    Cst_Info("CURR_LOTID", txtSLotID.Text.ToString());
                }
            }
        }

        private void txtLotid_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Enter)
            //{
            //    if (txtLotid.Text != "")
            //        Cst_Info("CURR_LOTID", txtLotid.Text.ToString());
            //}
        }

        private void Cst_Info(string sType, string sID)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add(sType, typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("SYSTEM_ID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow[sType] = sID;
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["SYSTEM_ID"] = LoginInfo.SYSID;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CARRIER_INFO_RFID", "INDATA", "OUTDATA", inTable);
                Util.GridSetData(dgSearch, dtMain, FrameOperation, true);

                //(dgSearch.Columns["CSTIUSE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtiUse.Copy());
                //(dgSearch.Columns["CSTTYPE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtType.Copy());

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetCST_Info();
        }
        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowLoadingIndicator();
        }
        private void btnSave_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowLoadingIndicator();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            int iRow = _Util.GetDataGridCheckFirstRowIndex(dgSearchList, "CHK");

            if (iRow < 0)
            {
                Util.MessageValidation("SFU1651");  //선택된 항목이 없습니다.
                HiddenLoadingIndicator();
                return;
            }

            Util.MessageConfirm("SFU4340", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ChangerCarrier();
                }
                else
                {
                    HiddenLoadingIndicator();
                }
            });

        }

        private void ChangerCarrier()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("CSTIUSE", typeof(string));
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                for (int i = 0; i < dgSearchList.GetRowCount(); i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgSearchList, "CHK", i)) continue;

                    DataRow newRow = inTable.NewRow();
                    newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgSearchList.Rows[i].DataItem, "CSTID"));
                    newRow["CSTIUSE"] = Util.NVC(DataTableConverter.GetValue(dgSearchList.Rows[i].DataItem, "CSTIUSE"));
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["USERID"] = LoginInfo.USERID;
                 
                    inTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService("BR_PRD_REG_CARRIER_IUSE_CHG", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        GetCST_Info();

                        Util.MessageInfo("SFU1275");	//정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        private void GetCreateInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("CSTOWNER", typeof(string));
                inTable.Columns.Add("CSTTYPE", typeof(string));
                inTable.Columns.Add("CSTSTAT", typeof(string));
                inTable.Columns.Add("CSTIUSE", typeof(string));
                //inTable.Columns.Add("CSTID", typeof(string));
                //inTable.Columns.Add("CURR_LOTID", typeof(string));
                inTable.Columns.Add("FROM_DATE", typeof(string));
                inTable.Columns.Add("TO_DATE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["CSTOWNER"] = cboArea.SelectedValue.ToString();
                newRow["CSTTYPE"] = Util.GetCondition(cboCstType, bAllNull: true);
                newRow["CSTSTAT"] = Util.GetCondition(cboCstStatus, bAllNull: true);
                newRow["CSTIUSE"] = Util.GetCondition(cboiUse, bAllNull: true);
                //newRow["CSTID"] = txtCSTid.Text == "" ? null : txtCSTid.Text;
                //newRow["CURR_LOTID"] = txtLotid.Text == "" ? null : txtLotid.Text;
                newRow["FROM_DATE"] = DateTime.Now.ToString("yyyyMMdd");
                newRow["TO_DATE"] = DateTime.Now.AddDays(1).ToString("yyyyMMdd");

                inTable.Rows.Add(newRow);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CARRIER_INFO", "INDATA", "OUTDATA", inTable);
                Util.GridSetData(dgSearch, dtMain, FrameOperation);

                (dgSearch.Columns["CSTIUSE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtiUse.Copy());
                //(dgSearch.Columns["CSTTYPE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtType.Copy());

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetCST_Info()
        {
            try
            {
                if (cboCstType.SelectedIndex < 0 || cboCstType.SelectedValue.GetString().Equals("SELECT"))
                {
                    // Carrier를 선택하세요.
                    Util.MessageValidation("SFU4523");
                    return;
                }

                ShowLoadingIndicator();
                
                DataTable inTable = new DataTable();
                inTable.Columns.Add("CSTOWNER", typeof(string));
                inTable.Columns.Add("CSTTYPE", typeof(string));
                inTable.Columns.Add("CSTSTAT", typeof(string));
                inTable.Columns.Add("CSTIUSE", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("CURR_LOTID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CSTELEC", typeof(string));
                inTable.Columns.Add("CSTPROD", typeof(string));

                inTable.Columns.Add("SYSTEM_ID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["CSTOWNER"] = Util.GetCondition(cboArea, bAllNull: true);
                newRow["CSTTYPE"] = Util.GetCondition(cboCstType, bAllNull: true);
                newRow["CSTSTAT"] = Util.GetCondition(cboCstStatus, bAllNull: true);
                newRow["CSTIUSE"] = Util.GetCondition(cboiUse, bAllNull: true);
                newRow["CSTID"] = txtCSTid.Text == "" ? null : txtCSTid.Text;
                newRow["CURR_LOTID"] = txtSLotID.Text == "" ? null : txtSLotID.Text;
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CSTELEC"] = Util.GetCondition(cboSELEC, bAllNull: true);
                newRow["CSTPROD"] = Util.GetCondition(cboSPROD, bAllNull: true);

                newRow["SYSTEM_ID"] = LoginInfo.SYSID;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CARRIER_INFO_RFID", "INDATA", "OUTDATA", inTable);
                Util.GridSetData(dgSearch, dtMain, FrameOperation);

                //(dgSearch.Columns["CSTIUSE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtiUse.Copy());

                txtChangeCSTID.Text = "";
                txtCSTStatus.Text = "";
                txtLotID.Text = "";
                CSTStatus = "";
                Util.gridClear(dgSearchList);


            }
            catch (Exception ex)
            {
                //HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                //HiddenLoadingIndicator();
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private DateTime GetDBDateTime()
        {
            try
            {
                DateTime tmpDttm = DateTime.Now;
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_GETDATE", null, "OUTDATA", null);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    tmpDttm = (DateTime)dtRslt.Rows[0]["DATE"];
                }

                return tmpDttm;
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
                return DateTime.Now;
            }
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            int iRow = _Util.GetDataGridCheckFirstRowIndex(dgSearchList, "CHK");

            if (iRow < 0)
            {
                Util.MessageValidation("SFU1651");  //선택된 항목이 없습니다.
                return;
            }

            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DeleteCarrier();
                }
            });
        }

        private void btnDel_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void DeleteCarrier()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("CSTID", typeof(string));

                for (int i = 0; i < dgSearchList.GetRowCount(); i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgSearchList, "CHK", i)) continue;

                    DataRow newRow = inTable.NewRow();
                    newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgSearchList.Rows[i].DataItem, "CSTID"));

                    inTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService("BR_PRD_REG_DELETE_RFID_CARRIER", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        GetCST_Info();

                        Util.MessageInfo("SFU1275");	//정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void btnEmpty_Click(object sender, RoutedEventArgs e)
        {
            int iRow = _Util.GetDataGridCheckFirstRowIndex(dgSearchList, "CHK");

            if (iRow < 0)
            {
                Util.MessageValidation("SFU1651");  //선택된 항목이 없습니다.
                HiddenLoadingIndicator();
                return;
            }
            if (EmptyValidation())
                CarrierEmpty();

        }

        private void CarrierEmpty()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                for (int i = 0; i < dgSearchList.GetRowCount(); i++)
                {
                    DataRow newRow = inTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgSearchList.Rows[i].DataItem, "CSTID"));
                    newRow["USERID"] = LoginInfo.USERID;

                    inTable.Rows.Add(newRow);
                }

                DataTable RSLTDT = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_CSTID_EMPTY_UI", "INDATA", null, inTable);

                GetCST_Info();

                Util.MessageInfo("SFU1275");	//정상 처리 되었습니다.

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void btnEmpty_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowLoadingIndicator();
        }

        private void dgSearch_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            

            try
            {
                if (dgSearch.CurrentRow != null)
                {
                    txtChangeCSTID.Text = Util.NVC(DataTableConverter.GetValue(dgSearch.CurrentRow.DataItem, "CSTID"));
                    txtCSTStatus.Text = Util.NVC(DataTableConverter.GetValue(dgSearch.CurrentRow.DataItem, "CSTSTAT"));
                    txtLotID.Text = Util.NVC(DataTableConverter.GetValue(dgSearch.CurrentRow.DataItem, "CURR_LOTID"));
                    CSTStatus = Util.NVC(DataTableConverter.GetValue(dgSearch.CurrentRow.DataItem, "CSTSTAT_CODE"));
                }

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgSearch.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "CSTID")
                    {
                        if (!string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgSearch.Rows[cell.Row.Index].DataItem, cell.Column.Name))))
                        {
                            //this.FrameOperation.OpenMenu("SFU010090090", true, cell.Text);

                            COM001_216_CST_HIST wndHist = new COM001_216_CST_HIST();
                            wndHist.FrameOperation = FrameOperation;

                            if (wndHist != null)
                            {
                                object[] Parameters = new object[1];
                                Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgSearch.Rows[cell.Row.Index].DataItem, cell.Column.Name));

                                C1WindowExtension.SetParameters(wndHist, Parameters);

                                wndHist.Closed += new EventHandler(wndHist_Closed);

                                // 팝업 화면 숨겨지는 문제 수정.
                                this.Dispatcher.BeginInvoke(new Action(() => wndHist.ShowModal()));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void CarrierUsing()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("SRCTYPE", typeof(string));
                for (int i = 0; i < dgSearchList.GetRowCount(); i++)
                {
                    DataRow newRow = inTable.NewRow();
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgSearchList.Rows[i].DataItem, "CURR_LOTID"));
                    newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgSearchList.Rows[i].DataItem, "CSTID"));
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;

                    inTable.Rows.Add(newRow);
                }

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_CSTID_USING_UI", "INDATA", null, inTable);

                GetCST_Info();

                Util.MessageInfo("SFU1270");      //저장되었습니다.          

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        private void btnUsing_Click(object sender, RoutedEventArgs e)
        {
            int iRow = _Util.GetDataGridCheckFirstRowIndex(dgSearchList, "CHK");

            if (iRow < 0)
            {
                Util.MessageValidation("SFU1651");  //선택된 항목이 없습니다.
                HiddenLoadingIndicator();
                return;
            }
            if (MergeValidation())
                CarrierUsing();
        }

        private void btnUsing_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowLoadingIndicator();
        }

        private void btnMerge_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoadingIndicator();
                string BizRule = string.Empty;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("SRCTYPE", typeof(string));

                DataRow newRow = inTable.NewRow();

                if (CSTStatus.Trim().Equals("U")) // Using의 경우 Empty 처리
                {
                    BizRule = "BR_PRD_REG_CSTID_EMPTY_RFID";

                    newRow["LOTID"] = Util.NVC(txtLotID.Text);
                    newRow["USERID"] = LoginInfo.USERID;

                    inTable.Rows.Add(newRow);
                }
                else  // Empty의 경우 Using 처리
                {
                    BizRule = "BR_PRD_REG_CSTID_USING_UI";

                    newRow["LOTID"] = Util.GetCondition(txtLotID, "SFU1195"); //Lot정보가 없습니다.
                    newRow["CSTID"] = Util.NVC(txtChangeCSTID.Text);
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;

                    inTable.Rows.Add(newRow);
                }

                DataTable RSLTDT = new ClientProxy().ExecuteServiceSync(BizRule, "INDATA", null, inTable);

                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                txtChangeCSTID.Text = "";
                txtLotID.Text = "";
                txtCSTStatus.Text = "";
                CSTStatus = "";
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private bool MergeValidation()
        {
            for (int i = 0; i < dgSearchList.GetRowCount(); i++)
            {
                if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgSearchList.Rows[i].DataItem, "CURR_LOTID"))))
                {
                    object[] parameters = new object[1];
                    parameters[0] = Util.NVC(Util.NVC(DataTableConverter.GetValue(dgSearchList.Rows[i].DataItem, "CSTID")));
                    Util.MessageInfo("100885", parameters); // Carrier [%1]에 할당된 LOTID 정보가 없습니다.
                    HiddenLoadingIndicator();
                    return false;
                }
            }

            return true;
        }

        private bool EmptyValidation()
        {
            for (int i = 0; i < dgSearchList.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgSearchList.Rows[i].DataItem, "CSTSTAT_CODE")).Equals("E"))
                {
                    object[] parameters = new object[1];
                    parameters[0] = Util.NVC(Util.NVC(DataTableConverter.GetValue(dgSearchList.Rows[i].DataItem, "CSTID")));
                    Util.MessageInfo("SFU4893", parameters); // %1은 이미 Empty 상태인 Carrier 입니다.
                    HiddenLoadingIndicator();
                    return false;
                }

                if (!Util.NVC(DataTableConverter.GetValue(dgSearchList.Rows[i].DataItem, "CSTIUSE")).Equals("Y"))
                {
                    object[] parameters = new object[1];
                    parameters[0] = Util.NVC(Util.NVC(DataTableConverter.GetValue(dgSearchList.Rows[i].DataItem, "CSTID")));
                    Util.MessageInfo("SFU4964", parameters); // 사용여부가 N 인 캐리어(%1)는 초기화할 수 없습니다.
                    HiddenLoadingIndicator();
                    return false;
                }
            }

            return true;
        }

        private void btnMerge_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowLoadingIndicator();
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            COM001_216_CREATE popupTagPrint = new COM001_216_CREATE();
            popupTagPrint.FrameOperation = this.FrameOperation;
            object[] parameters = new object[2];
            parameters[0] = "";
            parameters[1] = "";

            C1WindowExtension.SetParameters(popupTagPrint, parameters);
            popupTagPrint.Closed += new EventHandler(Carrier_Closed);

            grdMain.Children.Add(popupTagPrint);
            popupTagPrint.BringToFront();
        }
        private void Carrier_Closed(object sender, EventArgs e)
        {
            COM001_216_CREATE popup = sender as COM001_216_CREATE;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }

            this.grdMain.Children.Remove(popup);

        }

        private void btnMap_Click(object sender, RoutedEventArgs e)
        {
            COM001_216_UPLOAD popupTagPrint = new COM001_216_UPLOAD();
            popupTagPrint.FrameOperation = this.FrameOperation;
            object[] parameters = new object[2];
            parameters[0] = "";
            parameters[1] = "";

            C1WindowExtension.SetParameters(popupTagPrint, parameters);
            popupTagPrint.Closed += new EventHandler(Upload_Closed);

            grdMain.Children.Add(popupTagPrint);
            popupTagPrint.BringToFront();
        }

        private void Upload_Closed(object sender, EventArgs e)
        {
            COM001_216_UPLOAD popup = sender as COM001_216_UPLOAD;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }

            this.grdMain.Children.Remove(popup);

        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            dgSearch.Selection.Clear();

            CheckBox cb = sender as CheckBox;

            if (DataTableConverter.GetValue(cb.DataContext, "CHK").Equals(1))//체크되는 경우
            {
                DataTable dtTo = DataTableConverter.Convert(dgSearchList.ItemsSource);

                if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
                {
                    dtTo.Columns.Add("CHK", typeof(Boolean));
                    dtTo.Columns.Add("CSTIUSE", typeof(string));
                    dtTo.Columns.Add("CSTOWNER", typeof(string));
                    dtTo.Columns.Add("CSTID", typeof(string));
                    dtTo.Columns.Add("CSTTYPE", typeof(string));
                    dtTo.Columns.Add("CSTSTAT", typeof(string));
                    dtTo.Columns.Add("CSTSTAT_CODE", typeof(string));
                    dtTo.Columns.Add("CURR_LOTID", typeof(string));
                    dtTo.Columns.Add("PROCID_CUR", typeof(string));
                    dtTo.Columns.Add("EQPT_CUR", typeof(string));
                }

                if (dtTo.Select("CSTID = '" + DataTableConverter.GetValue(cb.DataContext, "CSTID") + "'").Length > 0) //중복조건 체크
                {
                    return;
                }

                DataRow dr = dtTo.NewRow();
                foreach (DataColumn dc in dtTo.Columns)
                {
                    if (dc.DataType.Equals(typeof(Boolean)))
                    {
                        dr[dc.ColumnName] = DataTableConverter.GetValue(cb.DataContext, dc.ColumnName);
                    }
                    else
                    {
                        dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(cb.DataContext, dc.ColumnName));
                    }
                }

                dtTo.Rows.Add(dr);
                dgSearchList.ItemsSource = DataTableConverter.Convert(dtTo);

                (dgSearchList.Columns["CSTIUSE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtiUse.Copy());

                DataRow[] drUnchk = DataTableConverter.Convert(dgSearchList.ItemsSource).Select("CHK = 0");

                //if (drUnchk.Length == 0)
                //{
                //    chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                //    chkAll.IsChecked = true;
                //    chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                //}

            }
            else//체크 풀릴때
            {
                //chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                //chkAll.IsChecked = false;
                //chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);

                DataTable dtTo = DataTableConverter.Convert(dgSearchList.ItemsSource);

                dtTo.Rows.Remove(dtTo.Select("CSTID = '" + DataTableConverter.GetValue(cb.DataContext, "CSTID") + "'")[0]);

                dgSearchList.ItemsSource = DataTableConverter.Convert(dtTo);
            }
        }

        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if (dgSearch.ItemsSource == null) return;

            DataTable dt = ((DataView)dgSearch.ItemsSource).Table;

            dt.Select("CHK = 0").ToList<DataRow>().ForEach(r => r["CHK"] = 1);
            dt.AcceptChanges();

            chkAllSelect();
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgSearch.ItemsSource == null) return;

            DataTable dt = ((DataView)dgSearch.ItemsSource).Table;

            dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
            dt.AcceptChanges();

            chkAllClear();
        }

        private void chkAllSelect()
        {
            Util.gridClear(dgSearchList);

            DataTable dtSelect = new DataTable();

            DataTable dtTo = DataTableConverter.Convert(dgSearch.ItemsSource);
            dtSelect = dtTo.Copy();

            dgSearchList.ItemsSource = DataTableConverter.Convert(dtSelect);

        }

        private void chkAllClear()
        {
            Util.gridClear(dgSearchList);
        }

        #endregion

        private void Run_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed &&
                        (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
                        (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt &&
                        (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    if (Util.pageAuthCheck(FrameOperation.AUTHORITY))
                    {
                        btnDel.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    btnDel.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtCSTid_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtCSTid == null) return;
                InputMethod.SetPreferredImeConversionMode(txtCSTid, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtSLotID_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtSLotID == null) return;
                InputMethod.SetPreferredImeConversionMode(txtSLotID, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgSearch_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name == "CSTID")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        //e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                        //e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }
            }));
        }

        private void dgSearch_UnloadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }
            }));
        }

        private void wndHist_Closed(object sender, EventArgs e)
        {
            COM001_216_CST_HIST window = sender as COM001_216_CST_HIST;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }
    }
}

