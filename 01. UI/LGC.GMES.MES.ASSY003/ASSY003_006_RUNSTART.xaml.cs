/*************************************************************************************
 Created Date : 2018.04.09
      Creator : CNS 고현영S
   Decription : 전지 5MEGA-GMES 구축 - 공정진척 - 폴딩후테이핑공정진척 - 작업시작
--------------------------------------------------------------------------------------
 [Change History]
  2018.04.09  CNS 고현영S : 생성
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_021_RUNSTART.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_006_RUNSTART : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        string _LineID = "";
        string _EqptID = "";
        string _ProcID = "";

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();

        public string NEW_PROD_LOT = string.Empty;

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

        public ASSY003_006_RUNSTART()
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

                if (tmps != null && tmps.Length == 3)
                {
                    _LineID = Util.NVC(tmps[0]);
                    _EqptID = Util.NVC(tmps[1]);
                    _ProcID = Util.NVC(tmps[2]);
                }
                else
                {
                    _LineID = "";
                    _EqptID = "";
                    _ProcID = "";
                }

                txtEquipmentSegment.Text = _LineID;
                txtEquipment.Text = _EqptID;
                tbxInLot.Focus();
                ApplyPermissions();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!CanStart())
                return;

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("작업시작 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1240", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    string sNewLot = GetNewLotId();
                    if (sNewLot.Equals(""))
                        return;

                    RunStart(sNewLot);
                }
            });
        }

        private string GetNewLotId()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_GET_NEW_LOT_FD();

                DataTable inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                //newRow["NEXTDAY"] = "N";
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable input_LOT = indataSet.Tables["IN_INPUT"];
                newRow = input_LOT.NewRow();
                newRow["INPUT_LOTID"] = tbxInLot.Text;
                input_LOT.Rows.Add(newRow);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_NEW_PROD_LOTID_TAPING_AFTER_FOLDING", "IN_EQP,IN_INPUT", "OUTDATA", indataSet);

                string sNewLot = string.Empty;
                if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables["OUTDATA"] != null && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                {
                    sNewLot = Util.NVC(dsResult.Tables["OUTDATA"].Rows[0]["PROD_LOTID"]);
                }

                return sNewLot;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void RunStart(string sNewLot)
        {
            try
            {
                ShowLoadingIndicator();
                // 착공 처리..

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID");
                inTable.Columns.Add("EQPTID");
                inTable.Columns.Add("MOUNT_MTRL_TYPE_CODE");

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = txtEquipment.Text;
                newRow["MOUNT_MTRL_TYPE_CODE"] = "PROD"; // 바구니 투입위치만 조회.
                inTable.Rows.Add(newRow);

                DataTable dtEqptInfo = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_MOUNT_INFO", "INDATA", "OUTDATA", inTable);

                DataSet indataSet = new DataSet();

                DataTable inData = new DataTable("IN_EQP");
                inData.Columns.Add("SRCTYPE");
                inData.Columns.Add("IFMODE");
                inData.Columns.Add("PROCID");
                inData.Columns.Add("EQPTID");
                inData.Columns.Add("PROD_LOTID");
                inData.Columns.Add("USERID");

                DataRow dr1 = inData.NewRow();
                dr1["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr1["IFMODE"] = IFMODE.IFMODE_OFF;
                dr1["PROCID"] = Process.TAPING_AFTER_FOLDING;
                dr1["EQPTID"] = txtEquipment.Text;
                dr1["PROD_LOTID"] = sNewLot;
                dr1["USERID"] = LoginInfo.USERID;
                inData.Rows.Add(dr1);
                indataSet.Tables.Add(inData);

                DataTable inLot = new DataTable("IN_INPUT");
                inLot.Columns.Add("INPUT_LOTID");
                inLot.Columns.Add("MTRLID");
                inLot.Columns.Add("EQPT_MOUNT_PSTN_ID");
                inLot.Columns.Add("EQPT_MOUNT_PSTN_STATE");
                inLot.Columns.Add("ACTQTY");

                DataRow dr2 = inLot.NewRow();
                dr2["INPUT_LOTID"] = DataTableConverter.GetValue(dgdLotInfo.Rows[0].DataItem, "LOTID");;
                dr2["MTRLID"] = DataTableConverter.GetValue(dgdLotInfo.Rows[0].DataItem, "PRODID");
                dr2["EQPT_MOUNT_PSTN_ID"] = dtEqptInfo.Rows[0]["EQPT_MOUNT_PSTN_ID"];
                dr2["EQPT_MOUNT_PSTN_STATE"] = "A";
                dr2["ACTQTY"] = DataTableConverter.GetValue(dgdLotInfo.Rows[0].DataItem, "WIPQTY");
                inLot.Rows.Add(dr2);
                indataSet.Tables.Add(inLot);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_PROD_LOT_TAPING_AFTER_FOLDING_2", "INDATA,IN_INPUT", "OUTDATA", (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.

                        NEW_PROD_LOT = searchResult.Tables["OUTDATA"].Rows[0]["LOTID"].ToString();
                        
                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet
              );
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

        private void tbxInLot_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    ShowLoadingIndicator();

                    DataSet ds = new DataSet();
                    ds.Tables.Add("INDATA");
                    ds.Tables["INDATA"].Columns.Add("LANGID");
                    ds.Tables["INDATA"].Columns.Add("SHOPID");
                    ds.Tables["INDATA"].Columns.Add("AREAID");
                    ds.Tables["INDATA"].Columns.Add("PROCID");
                    ds.Tables["INDATA"].Columns.Add("EQSGID");
                    ds.Tables["INDATA"].Columns.Add("LOTID");

                    DataRow dr = ds.Tables["INDATA"].NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["PROCID"] = Process.PACKAGING;
                    dr["EQSGID"] = txtEquipmentSegment.Text;
                    dr["LOTID"] = tbxInLot.Text.Trim();
                    ds.Tables["INDATA"].Rows.Add(dr);

                    new ClientProxy().ExecuteService_Multi("BR_PRD_SEL_WAIT_LOT_LIST_TAPING_AFTER_FOLDING", "INDATA", "OUTDATA", (resultDs, exception) =>
                    {
                        try
                        {
                            ShowLoadingIndicator();

                            if (exception != null)
                            {
                                Util.MessageException(exception);
                                return;
                            }

                            Util.GridSetData(dgdLotInfo, resultDs.Tables["OUTDATA"], FrameOperation, false);
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {
                            HiddenLoadingIndicator();
                        }
                    },ds);
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
        }

        private void LoadTextBox()
        {
            //tbxInLot.Text = _inputLotId;
            //tbxWrkPstnName.Text = _EqptID;
        }

        #endregion

        #region Method

        #region [BizCall]

        #endregion

        #region [Validation]
        private bool CanStart()
        {
            bool bRet = false;
            if (string.IsNullOrWhiteSpace(tbxInLot.Text))
            {
                //"입력된 항목이 없습니다."
                Util.MessageValidation("SFU2052");
                return bRet;
            }

            if (dgdLotInfo.GetRowCount() == 0)
            {
                //"입력된 항목이 없습니다."
                Util.MessageValidation("SFU2052");
                return bRet;
            }

            bRet = true;
            return bRet;
        }
        #endregion

        #region [Func]

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnStart);
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

        #endregion

        #endregion

    }
}
