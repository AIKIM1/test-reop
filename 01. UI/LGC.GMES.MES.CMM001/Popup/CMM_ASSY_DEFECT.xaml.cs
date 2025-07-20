/*************************************************************************************
 Created Date : 2016.11.28
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - 조립 공정진척 화면 - 불량정보 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.11.28  INS 김동일K : Initial Created.
  
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ASSY_DEFECT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_DEFECT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _LotID = string.Empty;
        private string _WipSeq = string.Empty;
        private string _ProcID = string.Empty;
        private string _StackingYN = string.Empty;
        private string _WoDetlID = string.Empty;

        private double gDfctAQty = 0;
        private double gDfctCQty = 0;

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();
        #endregion

        #region Initialize 
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_ASSY_DEFECT()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 7)
                {
                    _LineID = Util.NVC(tmps[0]);
                    _EqptID = Util.NVC(tmps[1]);
                    _ProcID = Util.NVC(tmps[2]);
                    _LotID = Util.NVC(tmps[3]);
                    _WipSeq = Util.NVC(tmps[4]);
                    _StackingYN = Util.NVC(tmps[5]);
                    _WoDetlID = Util.NVC(tmps[6]);
                }
                else
                {
                    _LineID = "";
                    _EqptID = "";
                    _ProcID = "";
                    _LotID = "";
                    _WipSeq = "";
                    _StackingYN = "";
                    _WoDetlID = "";
                }

                ApplyPermissions();


                txtLotID.Text = _LotID;

                //if (_ProcID.Equals(Process.STACKING_FOLDING))
                //{
                //    grdMBomTypeCnt.Visibility = Visibility.Visible;

                //    if (dgDefect != null && dgDefect.Columns.Contains("A_TYPE_DFCT_QTY"))
                //        dgDefect.Columns["A_TYPE_DFCT_QTY"].Visibility = Visibility.Visible;
                //    if (dgDefect != null && dgDefect.Columns.Contains("C_TYPE_DFCT_QTY"))
                //        dgDefect.Columns["C_TYPE_DFCT_QTY"].Visibility = Visibility.Visible;

                //    if (_StackingYN.Equals("Y"))
                //    {
                //        tbAType.Text = "HALFTYPE";
                //        tbCType.Text = "MONOTYPE";

                //        if (dgDefect != null && dgDefect.Columns.Contains("A_TYPE_DFCT_QTY"))
                //            dgDefect.Columns["A_TYPE_DFCT_QTY"].Header = "HALFTYPE";
                //        if (dgDefect != null && dgDefect.Columns.Contains("C_TYPE_DFCT_QTY"))
                //            dgDefect.Columns["C_TYPE_DFCT_QTY"].Header = "MONOTYPE";
                //    }
                //    else
                //    {
                //        tbAType.Text = "ATYPE";
                //        tbCType.Text = "CTYPE";

                //        if (dgDefect != null && dgDefect.Columns.Contains("A_TYPE_DFCT_QTY"))
                //            dgDefect.Columns["A_TYPE_DFCT_QTY"].Header = "ATYPE";
                //        if (dgDefect != null && dgDefect.Columns.Contains("C_TYPE_DFCT_QTY"))
                //            dgDefect.Columns["C_TYPE_DFCT_QTY"].Header = "CTYPE";
                //    }

                //    GetMBOMInfo();
                //}
                //else
                //{
                    grdMBomTypeCnt.Visibility = Visibility.Collapsed;

                    if (dgDefect != null && dgDefect.Columns.Contains("A_TYPE_DFCT_QTY"))
                        dgDefect.Columns["A_TYPE_DFCT_QTY"].Visibility = Visibility.Collapsed;
                    if (dgDefect != null && dgDefect.Columns.Contains("C_TYPE_DFCT_QTY"))
                        dgDefect.Columns["C_TYPE_DFCT_QTY"].Visibility = Visibility.Collapsed;
                //}

                //if (LoginInfo.CFG_SHOP_ID.Equals("G182"))
                //    dgDefect.Columns["DFCT_QTY_CHG_BLOCK_FLAG"].Visibility = Visibility.Visible;
                //else
                //    dgDefect.Columns["DFCT_QTY_CHG_BLOCK_FLAG"].Visibility = Visibility.Collapsed;

                GetAllData();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDefectSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSaveDefect())
                return;

            //불량정보를 저장하시겠습니까?
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1587"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1587", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetDefect();
                }
            });
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void dgDefect_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                //if (!_ProcID.Equals(Process.STACKING_FOLDING))
                //    return;

                //double atypeqty;
                //double ctypeqty;
                //double resnqty;

                //if (e.Cell.Column.Name.Equals("A_TYPE_DFCT_QTY") || e.Cell.Column.Name.Equals("C_TYPE_DFCT_QTY"))
                //{
                //    string sAtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "A_TYPE_DFCT_QTY"));
                //    string sCtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "C_TYPE_DFCT_QTY"));
                //    atypeqty = double.Parse(sAtype);
                //    ctypeqty = double.Parse(sCtype);

                //    if (gDfctAQty > 0 && gDfctCQty > 0)
                //    {
                //        resnqty = Math.Round((atypeqty / gDfctAQty + ctypeqty / gDfctCQty) / 2, 0);

                //        DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "RESNQTY", resnqty);
                //    }
                //}
                //else if (e.Cell.Column.Name.Equals("RESNQTY"))
                //{
                //    string sResnQty = e.Cell.Text;
                //    resnqty = double.Parse(sResnQty);

                //    atypeqty = (resnqty * 2) * gDfctAQty;
                //    ctypeqty = (resnqty * 2) * gDfctCQty;

                //    DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "A_TYPE_DFCT_QTY", atypeqty / 2);
                //    DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "C_TYPE_DFCT_QTY", ctypeqty / 2);
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Mehod

        #region [BizCall]
        private void GetMBOMInfo()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable inTable = _Biz.GetDA_PRD_SEL_MBOM();

                DataRow newRow = inTable.NewRow();
                newRow["WO_DETL_ID"] = _WoDetlID;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                //newRow["EQSGID"] = _LineID;
                //newRow["INPUT_PROCID"] = Process.STACKING_FOLDING;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_MBOM_INFO", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (searchResult == null || searchResult.Rows.Count < 1)
                        {
                            Util.MessageValidation("SFU1941");      //타입별 불량 기준정보가 존재하지 않습니다.
                            return;
                        }
                        else
                        {
                            if (_StackingYN.Equals("Y"))
                            {
                                for (int i = 0; i < searchResult.Rows.Count; i++)
                                {
                                    if (Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL2_CODE"]).Equals("HC"))
                                    {
                                        txtInAType.Text = double.Parse(Util.NVC(searchResult.Rows[i]["PROC_INPUT_CNT"])).ToString();
                                    }
                                    else if (Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL2_CODE"]).Equals("MC"))
                                    {
                                        txtInCType.Text = double.Parse(Util.NVC(searchResult.Rows[i]["PROC_INPUT_CNT"])).ToString();
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < searchResult.Rows.Count; i++)
                                {
                                    if (Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL3_CODE"]).Equals("AT"))
                                    {
                                        txtInAType.Text = double.Parse(Util.NVC(searchResult.Rows[i]["PROC_INPUT_CNT"])).ToString();
                                    }
                                    else if (Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL3_CODE"]).Equals("CT"))
                                    {
                                        txtInCType.Text = double.Parse(Util.NVC(searchResult.Rows[i]["PROC_INPUT_CNT"])).ToString();
                                    }
                                }
                            }


                            if (txtInAType.Text.Equals(""))
                            {
                                if (_StackingYN.Equals("Y"))
                                    Util.MessageValidation("SFU1337");      //HALF TYPE 불량 기준정보가 존재하지 않습니다.
                                else
                                    Util.MessageValidation("SFU1306");        //ATYPE 불량 기준정보가 존재하지 않습니다.

                                return;
                            }

                            if (txtInCType.Text.Equals(""))
                            {
                                if (_StackingYN.Equals("Y"))
                                    Util.MessageValidation("SFU1401");     //MONO TYPE 불량 기준정보가 존재하지 않습니다.
                                else
                                    Util.MessageValidation("SFU1326");        //CTYPE 불량 기준정보가 존재하지 않습니다.

                                return;
                            }

                            gDfctAQty = double.Parse(txtInAType.Text);
                            gDfctCQty = double.Parse(txtInCType.Text);
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }
                );
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void GetDefectInfo()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                const string bizRuleName = "DA_QCA_SEL_WIPRESONCOLLECT";

                DataTable inTable = _Biz.GetDA_QCA_SEL_WIPRESONCOLLECT();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = _ProcID;
                newRow["EQPTID"] = _EqptID;
                newRow["LOTID"] = _LotID;
                newRow["WIPSEQ"] = _WipSeq;
                //newRow["ACTID"] = "DEFECT_LOT"; //"DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT"; //"DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";

                inTable.Rows.Add(newRow);
                //DataSet ds = new DataSet();
                //ds.Tables.Add(inTable);
                //string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgDefect.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgDefect, searchResult, null, true);

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }
                );
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }

        }

        private void SetDefect(bool bShowMsg = true)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                dgDefect.EndEdit();

                int iSeq = 0;

                DataSet indataSet = _Biz.GetBR_PRD_REG_DEFECT_ALL();
                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);


                DataTable inDEFECT_LOT = indataSet.Tables["INRESN"];

                for (int i = 0; i < dgDefect.Rows.Count - dgDefect.BottomRows.Count; i++)
                {
                    newRow = null;

                    newRow = inDEFECT_LOT.NewRow();
                    newRow["LOTID"] = _LotID.Trim();
                    newRow["WIPSEQ"] = int.TryParse(_WipSeq, out iSeq) ? iSeq : 1;
                    newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID"));
                    newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNCODE"));
                    newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY")));
                    newRow["RESNCODE_CAUSE"] = "";
                    newRow["PROCID_CAUSE"] = "";
                    newRow["RESNNOTE"] = "";
                    newRow["DFCT_TAG_QTY"] = 0;
                    newRow["LANE_QTY"] = 1;
                    newRow["LANE_PTN_QTY"] = 1;

                    if (Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID")).GetString() == "CHARGE_PROD_LOT")
                    {
                        newRow["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "COST_CNTR_ID"));
                    }
                    else
                    {
                        newRow["COST_CNTR_ID"] = "";
                    }

                    newRow["A_TYPE_DFCT_QTY"] = 0;
                    newRow["C_TYPE_DFCT_QTY"] = 0;
                    //newRow["A_TYPE_DFCT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "A_TYPE_DFCT_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "A_TYPE_DFCT_QTY")));
                    //newRow["C_TYPE_DFCT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "C_TYPE_DFCT_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "C_TYPE_DFCT_QTY")));

                    inDEFECT_LOT.Rows.Add(newRow);
                }

                if (inDEFECT_LOT.Rows.Count < 1)
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    return;
                }

                new ClientProxy().ExecuteService_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL", "INDATA,INRESN", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (bShowMsg)
                            Util.MessageInfo("SFU1275");      //정상 처리 되었습니다.

                        GetDefectInfo();
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
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Validation]
        private bool CanSaveDefect()
        {
            if (!CommonVerify.HasDataGridRow(dgDefect))
            {
                Util.MessageValidation("SFU1578");      //불량 항목이 없습니다.
                return false;
            }
            if (_LotID.Trim().Length < 1)
            {
                Util.MessageValidation("SFU1195");      //Lot 정보가 없습니다.
                return false;
            }

            return true;            
        }
        
        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnDefectSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

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

        private void GetAllData()
        {
            GetDefectInfo();
        }

        #endregion

        #endregion

        private void dgDefect_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (Util.NVC(e.Cell.Column.Name) != "ACTNAME")
                    {
                        string sFlag = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"));
                        if (sFlag == "Y")
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#D4D4D4"));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                        else
                        {
                            e.Cell.Presenter.Background = null;// new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                    }
                }
            }));
        }

        private void dgDefect_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e == null || e.Row == null || e.Row.DataItem == null || e.Column == null)
                    return;

                if (e.Column.Name.Equals("RESNQTY"))
                {
                    string sFlag = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"));
                    if (sFlag == "Y")
                    {
                        e.Cancel = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
