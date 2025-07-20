/*************************************************************************************
 Created Date : 2018.07.24
      Creator : 정문교
   Decription : 불량LOT 삭제 처리
--------------------------------------------------------------------------------------
 [Change History]
    
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_POLYMER_FORM_CART_DEFECT_DELETE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_POLYMER_FORM_CART_DEFECT_DELETE : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _AreaID = string.Empty;
        private string _procID = string.Empty;        // 공정코드
        private string _eqptID = string.Empty;        // 설비코드
        private string _cartId = string.Empty;        // 대차ID
        private DataTable _defectLot;

        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();

        private bool _load = true;

        public string ShiftID { get; set; }
        public string ShiftName { get; set; }
        public string WorkerName { get; set; }
        public string WorkerID { get; set; }

        #endregion

        #region Initialize
        /// <summary>
        ///  
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_POLYMER_FORM_CART_DEFECT_DELETE()
        {
            InitializeComponent();
        }

        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                InitializeUserControls();
                SetParameters();
                SetControl();
                SetCombo();
                _load = false;
            }

        }

        private void InitializeUserControls()
        {
        }
        private void SetParameters()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _procID = tmps[0] as string;
            _eqptID = tmps[2] as string;
            _cartId = tmps[3] as string;

            txtProcess.Text = tmps[1] as string;
            txtCartID.Text = tmps[3] as string;

            DataRow defectLot = tmps[4] as DataRow;

            _defectLot = defectLot.Table.Clone();
            _defectLot.ImportRow(defectLot);
        }

        private void SetControl()
        {
            // Grid
            Util.GridSetData(dgDefectGroup, _defectLot, null, true);

            // Area ID 조회
            GetAreaID();

            // 전기일
            dtpDate.SelectedDateTime = GetComSelCalDate();
        }
        private void SetCombo()
        {
        }

        #endregion

        #region [삭제]
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDelete())
                return;

            // 삭제처리 하시겠습니까?
            Util.MessageConfirm("SFU1259", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DeleteProcess();
                }
            });
        }
        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #endregion

        #region Mehod

        private void GetAreaID()
        {
            try
            {
                if (_defectLot == null || _defectLot.Rows.Count == 0)
                {
                    _AreaID = string.Empty;
                    return;
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = _defectLot.Rows[0]["LOTID"].ToString(); 
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_AREAID", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    _AreaID = Util.NVC(dtResult.Rows[0]["AREAID"]);
                else
                    _AreaID = string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DateTime GetComSelCalDate()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = _AreaID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_CALDATE", "RQSTDT", "RSLTDT", RQSTDT);

                DateTime dCalDate;

                if (dtResult != null && dtResult.Rows.Count > 0)
                    dCalDate = Convert.ToDateTime(Util.NVC(dtResult.Rows[0]["CALDATE"]));
                else
                    dCalDate = DateTime.Now;

                return dCalDate;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return DateTime.Now;
            }
        }

        /// <summary>
        /// 삭제
        /// </summary>
        private void DeleteProcess()
        {
            try
            {
                ShowLoadingIndicator();

                // DATA SET 
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("SHIFT", typeof(string));
                inTable.Columns.Add("WRK_USERID", typeof(string));
                inTable.Columns.Add("WRK_USER_NAME", typeof(string));
                inTable.Columns.Add("POSTDATE", typeof(string));

                DataTable inRESN = inDataSet.Tables.Add("INRESN");
                inRESN.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = "UI";
                newRow["IFMODE"] = "OFF";
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["SHIFT"] = ShiftID;
                newRow["WRK_USERID"] = WorkerID;
                newRow["WRK_USER_NAME"] = WorkerName;
                newRow["POSTDATE"] = dtpDate.SelectedDateTime.ToString("yyyy-MM-dd");
                inTable.Rows.Add(newRow);

                newRow = inRESN.NewRow();
                newRow["LOTID"] = Util.NVC(_defectLot.Rows[0]["LOTID"]);
                inRESN.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_CANCEL_DEFECT_LOT_PC", "INDATA, INRESN", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.AlertInfo("정상 처리 되었습니다.");
                        Util.MessageInfo("SFU1889");

                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [Func]
        private bool ValidationDelete()
        {
            if (_defectLot == null ||  _defectLot.Rows.Count == 0)
            {
                // Lot 정보가 없습니다.
                Util.MessageValidation("SFU1195");
                return false;
            }

            return true;
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




    }
}