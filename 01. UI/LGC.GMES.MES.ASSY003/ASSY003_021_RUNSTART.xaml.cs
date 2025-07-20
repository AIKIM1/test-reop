/*************************************************************************************
 Created Date : 2017.12.09
      Creator : CNS 고현영S
   Decription : 전지 5MEGA-GMES 구축 - 특이작업 - C생산공정진척 - 작업시작
--------------------------------------------------------------------------------------
 [Change History]
  2017.12.09  CNS 고현영S : 생성
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
    public partial class ASSY003_021_RUNSTART : C1Window, IWorkArea
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

        public ASSY003_021_RUNSTART()
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
            catch ( Exception ex )
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if ( tmps != null && tmps.Length == 3 )
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
            catch ( Exception ex )
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
                    RunStart();
                }
            });
        }

        private void RunStart()
        {
            try
            {
                ShowLoadingIndicator();
                // 착공 처리..

                DataTable inTable = _Biz.GetDA_PRD_SEL_INPUT_POS_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _EqptID;
                newRow["MOUNT_MTRL_TYPE_CODE"] = "PROD"; // 바구니 투입위치만 조회.

                inTable.Rows.Add(newRow);

                DataTable dtEqptInfo = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_MOUNT_INFO", "INDATA", "OUTDATA", inTable);
                
                DataSet indataSet = new DataSet();
                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("IFMODE", typeof(string));
                inData.Columns.Add("PROCID", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("LOTTYPE", typeof(string));

                DataRow row = inData.NewRow();
                row["SRCTYPE"] = "UI";
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["PROCID"] = Process.CPROD;
                row["EQPTID"] = _EqptID;
                row["USERID"] = LoginInfo.USERID;
                row["LOTTYPE"] = DataTableConverter.GetValue(dgdLotInfo.Rows[0].DataItem, "LOTTYPE").ToString();

                indataSet.Tables["INDATA"].Rows.Add(row);

                DataTable inLot = indataSet.Tables.Add("IN_INPUT");
                inLot.Columns.Add("INPUT_LOTID", typeof(string));
                inLot.Columns.Add("MTRLID", typeof(string));
                inLot.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inLot.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));

                DataRow row2 = inLot.NewRow();
                row2["INPUT_LOTID"] = tbxInLot.Text.Trim();
                row2["MTRLID"] = DataTableConverter.GetValue(dgdLotInfo.Rows[0].DataItem, "PRODID").ToString();
                row2["EQPT_MOUNT_PSTN_ID"] = Util.NVC(dtEqptInfo.Rows[0]["EQPT_MOUNT_PSTN_ID"]);
                row2["EQPT_MOUNT_PSTN_STATE"] = "A";

                indataSet.Tables["IN_INPUT"].Rows.Add(row2);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_PROD_LOT_CPROD", "INDATA,IN_INPUT", "OUTDATA", (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.

                        //NEW_PROD_LOT = tbxInLot.Text.Trim();
                        NEW_PROD_LOT= searchResult.Tables["OUTDATA"].Rows[0]["LOTID"].ToString();

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

                //DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_START_PROD_LOT_CPROD", "INDATA,IN_INPUT", null, indataSet);

                //HiddenLoadingIndicator();

                //this.DialogResult = MessageBoxResult.OK;

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void tbxInLot_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                try
                {
                    ShowLoadingIndicator();

                    DataTable dt = new DataTable("INDATA");
                    dt.Columns.Add("LANGID");
                    dt.Columns.Add("LOTID");
                    dt.Columns.Add("PROCID");

                    DataRow dr = dt.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["LOTID"] = tbxInLot.Text.Trim();
                    dr["PROCID"] = Process.CPROD;

                    dt.Rows.Add(dr);

                    new ClientProxy().ExecuteService("DA_PRD_SEL_INPUT_LOT_INFO_CPROD", "INDATA", "OUTDATA", dt, (resultDt, exception) =>
                    {
                        try
                        {
                            ShowLoadingIndicator();
                                
                            if(exception != null)
                            {
                                Util.MessageException(exception);
                                return;
                            }

                            Util.GridSetData(dgdLotInfo, resultDt, FrameOperation, false);
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

            if (dgdLotInfo.GetRowCount() ==0)
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
            if ( loadingIndicator != null )
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if ( loadingIndicator != null )
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        #endregion

        #endregion

     
    }
}
