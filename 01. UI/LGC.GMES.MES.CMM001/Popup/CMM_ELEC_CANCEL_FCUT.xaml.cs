/*************************************************************************************
 Created Date : 2017.01.04
      Creator : 
   Decription : 전지 5MEGA-GMES 구축 - 전지 공정진척 화면 - LOT 종료 취소 팝업
--------------------------------------------------------------------------------------
 [Change History]
 2025.03.18  이민형 : [HD_OSS_0123] 코터공정 일때만 "대 Lotid" 로 표시 하고 이외 공정엔 "LotID"로 표시
  
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

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ELEC_CANCEL_FCUT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_CANCEL_FCUT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _ProcID = string.Empty;
        private string _Single = string.Empty;

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

        public CMM_ELEC_CANCEL_FCUT()
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
                txtLotID.Focus();

                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 2)
                {
                    _ProcID = Util.NVC(tmps[0]);
                    _Single = Util.NVC(tmps[1]);
                }
                else
                {
                    _ProcID = "";
                    _Single = "";
                }

                ApplyPermissions();

                if (_ProcID.Equals(Process.COATING) || _ProcID.Equals(Process.TOP_COATING) || _ProcID.Equals(Process.BACK_COATING)
                    || _ProcID.Equals(Process.INS_COATING) || _ProcID.Equals(Process.INS_SLIT_COATING) || _ProcID.Equals(Process.SRS_COATING))
                {
                    if (dgConfirmLot != null && dgConfirmLot.Columns.Contains("WIPQTY2_ST"))
                        dgConfirmLot.Columns["WIPQTY2_ST"].Visibility = Visibility.Collapsed;
                }
                else
                {
                    if (dgConfirmLot != null && dgConfirmLot.Columns.Contains("WIPQTY2_ST"))
                        dgConfirmLot.Columns["WIPQTY2_ST"].Visibility = Visibility.Visible;
                }

                if ( string.Equals(_Single, "Y"))
                    if (dgConfirmLot != null && dgConfirmLot.Columns.Contains("WIPQTY2_ST"))
                        dgConfirmLot.Columns["WIPQTY_ST"].Visibility = Visibility.Visible;
                
                if (_ProcID.Equals(Process.COATING))
                {
                    blkLotID.Text = ObjectDic.Instance.GetObjectName("대LOTID");
                }
                else
                {
                    blkLotID.Text = ObjectDic.Instance.GetObjectName("LOTID");
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetConfirmedLot();
            }
        }

        private void btnCancelFcut_Click(object sender, RoutedEventArgs e)
        {
            //종료된 Lot을 복원 하시겠습니까?
            Util.MessageConfirm("SFU1274", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (CanSavePrdChg())
                        SetCancelFCut();
                }
            });
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod

        #region [BizCall]
        private void GetConfirmedLot()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                string sLotid = string.Empty;
                sLotid = txtLotID.Text.Trim();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(String));
                RQSTDT.Columns.Add("WIP_TYPE_CODE", typeof(String));
                RQSTDT.Columns.Add("PROCID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = sLotid;
                if (_ProcID.Equals(Process.COATING) || _ProcID.Equals(Process.TOP_COATING) || _ProcID.Equals(Process.BACK_COATING)
                    || _ProcID.Equals(Process.INS_COATING) || _ProcID.Equals(Process.INS_SLIT_COATING) || _ProcID.Equals(Process.SRS_COATING))
                {
                    dr["WIP_TYPE_CODE"] = "PROD";
                }
                else
                {
                    dr["WIP_TYPE_CODE"] = "IN";
                }
                dr["PROCID"] = _ProcID;

                RQSTDT.Rows.Add(dr);

                //DA_PRD_SEL_CANCEL_TERMINATE
                //DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CANCEL_TERMINATE", "RQSTDT", "RSLTDT", RQSTDT);

                new ClientProxy().ExecuteService("DA_PRD_SEL_CANCEL_TERMINATE", "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                        return;

                    Util.GridSetData(dgConfirmLot, result, FrameOperation, true);
                });

                txtLotID.Text = "";
                txtLotID.Focus();
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
                return;
            }
        }

        private void SetCancelFCut(bool bShowMsg = true)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                dgConfirmLot.EndEdit();

                DataSet IndataSet = new DataSet();
                DataTable IndataTable = IndataSet.Tables.Add("INDATA");
                IndataTable.Columns.Add("SRCTYPE", typeof(string));
                IndataTable.Columns.Add("USERID", typeof(string));

                DataRow indataNewRow = IndataTable.NewRow();
                indataNewRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                indataNewRow["USERID"] = LoginInfo.USERID;
                IndataTable.Rows.Add(indataNewRow);

                DataTable InlotTable = IndataSet.Tables.Add("INLOT");
                InlotTable.Columns.Add("LOTID", typeof(string));
                InlotTable.Columns.Add("LOTSTAT", typeof(string));
                InlotTable.Columns.Add("WIPQTY", typeof(decimal));
                InlotTable.Columns.Add("WIPQTY2", typeof(decimal));

                DataRow inlotNewRow = InlotTable.NewRow();

                for (int i = 0; i < dgConfirmLot.Rows.Count - dgConfirmLot.BottomRows.Count; i++)
                {
                    inlotNewRow = null;

                    inlotNewRow = InlotTable.NewRow();
                    inlotNewRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgConfirmLot.Rows[i].DataItem, "LOTID"));
                    inlotNewRow["LOTSTAT"] = "RELEASED";

                    if (string.Equals(_ProcID, Process.MIXING))
                    {
                        inlotNewRow["WIPQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgConfirmLot.Rows[0].DataItem, "WIPQTY2_ST"));
                        inlotNewRow["WIPQTY2"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgConfirmLot.Rows[0].DataItem, "WIPQTY2_ST"));
                    }
                    else if (string.Equals(_Single, "Y"))
                    {
                        decimal wipQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgConfirmLot.Rows[0].DataItem, "WIPQTY_ST"));
                        decimal laneQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgConfirmLot.Rows[0].DataItem, "LANE_QTY"));

                        inlotNewRow["WIPQTY"] = wipQty;
                        inlotNewRow["WIPQTY2"] = wipQty * laneQty;
                    }

                    InlotTable.Rows.Add(inlotNewRow);
                }

                if (IndataTable.Rows.Count < 1)
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    return;
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_ACT_REG_CANCEL_TERMINATE_LOT", "INDATA,INLOT", null, IndataSet);

                if (bShowMsg)
                {
                    //정상 처리 되었습니다.
                    Util.MessageInfo("SFU1275");
                }

                Util.gridClear(dgConfirmLot);
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region [Validation]
        private bool CanSavePrdChg()
        {
            bool bRet = false;

            if (dgConfirmLot.ItemsSource == null || dgConfirmLot.Rows.Count < 1)
            {
                Util.MessageValidation("SFU1228"); //종료 취소할 Lot 항목이 없습니다.
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
            listAuth.Add(btnCancelFcut);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion

        #endregion

    }
}
