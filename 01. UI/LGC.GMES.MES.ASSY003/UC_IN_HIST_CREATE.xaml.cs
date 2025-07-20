/*************************************************************************************
 Created Date : 2017.12.27
      Creator : Lee. D. R
   Decription : 전지 5MEGA-GMES 구축 - 투입 이력 생성 (공통 Popup)
--------------------------------------------------------------------------------------
 [Change History]
  2017.12.27  Lee. D. R   : Initial Created.


**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// UC_IN_HIST_CREATE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UC_IN_HIST_CREATE : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _ProcID = string.Empty;
        private string _LotID = string.Empty;
        private string _WipSeq = string.Empty;

        private string _Max_Pre_Proc_End_Day = string.Empty;

        private bool _StackingYN = false;

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();
        
        #endregion

        #region Initialize        
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 5)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
                _ProcID = Util.NVC(tmps[2]);
                _LotID = Util.NVC(tmps[3]);
                _WipSeq = Util.NVC(tmps[4]);
            }
            else
            {
                _LineID = "";
                _EqptID = "";
                _ProcID = "";
                _LotID = "";
                _WipSeq = "";
            }

            ApplyPermissions();

            InitializeControls();
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {

        }

        public UC_IN_HIST_CREATE()
        {
            InitializeComponent();
        }

        private void InitializeControls()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();

                // 자재 투입위치 코드
                String[] sFilter1 = { _EqptID, "PROD" };
                String[] sFilter2 = { _EqptID, null }; // 자재,제품 전체
                String[] sFilter3 = { _EqptID, "PROD", null };

                if (_ProcID.Equals(Process.DSF))
                {
                    String[] sFilterHistMountPstsID = { _EqptID }; // 투입 위치 모두..
                    _combo.SetCombo(cboHistMountPstsID, CommonCombo.ComboStatus.NONE, sFilter: sFilterHistMountPstsID, sCase: "EQPT_CURR_MOUNT_MTRL_CBO_ALL");
                }
                else if (_ProcID.Equals(Process.STP))
                {
                    _combo.SetCombo(cboHistMountPstsID, CommonCombo.ComboStatus.ALL, sFilter: sFilter3, sCase: "EQPT_CURR_MOUNT_MTRL_CBO_STP");
                }
                else
                {
                    _combo.SetCombo(cboHistMountPstsID, CommonCombo.ComboStatus.NONE, sFilter: sFilter2, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");
                }

                txtHistLotID.Focus();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Event

        #region [Main]


        private void ApplyPermissions()
        {
            try
            {
                List<Button> listAuth = new List<Button>();
                listAuth.Add(btnHistSave);

                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        private void cboHistMountPstsID_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            try
            {
                GetInputHistory();

                Init();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        public void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        #endregion

        private void btnHistSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtHistLotID.Text.Trim() != "")
                {
                    Save();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtHistLotID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (txtHistLotID.Text.Trim() != "")
                {
                    Save();
                }                
            }
        }

        private void Save()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();
                DataTable inTable = indataSet.Tables.Add("IN_EQP");
                inTable.Columns.Add("SRCTYPE");
                inTable.Columns.Add("IFMODE");
                inTable.Columns.Add("EQPTID");
                inTable.Columns.Add("PROD_LOTID");
                inTable.Columns.Add("USERID");

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["PROD_LOTID"] = _LotID;
                newRow["USERID"] = LoginInfo.USERID;

                indataSet.Tables["IN_EQP"].Rows.Add(newRow);

                DataTable inLot = indataSet.Tables.Add("IN_INPUT");
                inLot.Columns.Add("EQPT_MOUNT_PSTN_ID");
                inLot.Columns.Add("EQPT_MOUNT_PSTN_STATE");
                inLot.Columns.Add("INPUT_LOTID"); 
                //inLot.Columns.Add("MTRLID");
                //inLot.Columns.Add("ACTQTY");

                DataRow newRow2 = inLot.NewRow();
                
                newRow2["EQPT_MOUNT_PSTN_ID"] = cboHistMountPstsID.SelectedValue.ToString();
                newRow2["EQPT_MOUNT_PSTN_STATE"] = "A";  //설비 장착위치 상태 A : Active(Current),  S : Standby(대기)
                newRow2["INPUT_LOTID"] = txtHistLotID.Text.Trim();
                //newRow2["MTRLID"] = "";
                //newRow2["ACTQTY"] = "";

                indataSet.Tables["IN_INPUT"].Rows.Add(newRow2);

                newRow2 = inLot.NewRow();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_INPUT_HIST_S", "IN_EQP,IN_INPUT", null, (Result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        GetInputHistory();

                        //Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1275"));

                        Init();
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetInputHistory()
        {
            try
            {
                ShowLoadingIndicator();

                string sBizNAme = string.Empty;

                if (_ProcID.Equals(Process.DSF))
                    sBizNAme = "DA_PRD_SEL_INPUT_MTRL_HIST_DSF";
                else
                    sBizNAme = "DA_PRD_SEL_INPUT_MTRL_HIST";

                DataTable inTable = _Biz.GetUC_DA_PRD_SEL_INPUT_HIST();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _EqptID;
                newRow["PROD_LOTID"] = _LotID;
                newRow["PROD_WIPSEQ"] = _WipSeq.Equals("") ? 1 : Convert.ToDecimal(_WipSeq);
                //newRow["INPUT_LOTID"] = txtHistLotID.Text.Trim().Equals("") ? null : txtHistLotID.Text;
                newRow["EQPT_MOUNT_PSTN_ID"] = cboHistMountPstsID.SelectedValue.ToString();

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(sBizNAme, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgInputHist, searchResult, FrameOperation);

                        //if (dgInputHist.CurrentCell != null)
                        //    dgInputHist.CurrentCell = dgInputHist.GetCell(dgInputHist.CurrentCell.Row.Index, dgInputHist.Columns.Count - 1);
                        //else if (dgInputHist.Rows.Count > 0 && dgInputHist.GetCell(dgInputHist.Rows.Count, dgInputHist.Columns.Count - 1) != null)
                        //    dgInputHist.CurrentCell = dgInputHist.GetCell(dgInputHist.Rows.Count, dgInputHist.Columns.Count - 1);
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
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void Init()
        {
            txtHistLotID.Text = "";
            txtHistLotID.Focus();            
        }

    }
}