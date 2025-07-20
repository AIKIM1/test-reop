/*************************************************************************************
 Created Date : 2018.05.01
      Creator : 
   Decription : 활성화 대차 LOSS 이력 조회
--------------------------------------------------------------------------------------
 [Change History]
  2018.05.01  오화백 : Initial Created.
  2018.07.26  오화백 : 불량LOSS 취소.

**************************************************************************************/

using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using System.Linq;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_246 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        BizDataSet _Biz = new BizDataSet();

       

        public COM001_246()
        {
            InitializeComponent();
        }
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
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            InitCombo();

            this.Loaded -= UserControl_Loaded;
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
        }
        //화면내 combo 셋팅
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboProcid, cboEqsgid };
            _combo.SetCombo(cboAreaid, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild, sCase: "AREA");
            cboAreaid.SelectedValue = LoginInfo.CFG_AREA_ID;

            //공정
            C1ComboBox[] cboLineParent = { cboAreaid };
            C1ComboBox[] cboLineChild = { cboEqsgid };

            string[] sFilter = { "FORM_PROCID" };
            _combo.SetCombo(cboProcid, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);
            cboProcid.SelectedValue = LoginInfo.CFG_PROC_ID;

            DataTable dt = ((DataView)cboProcid.ItemsSource).Table;
            
            var query = (from t in dt.AsEnumerable()
                where t.Field<string>("CBO_CODE") == LoginInfo.CFG_EQPT_ID
                select t).FirstOrDefault();
            if (query != null)
            {
                cboProcid.SelectedValue = LoginInfo.CFG_EQPT_ID;
            }
            else
            {
                cboProcid.SelectedIndex = 0;
            }

            //라인
            C1ComboBox[] cboProcessParent = { cboAreaid, cboProcid };
            _combo.SetCombo(cboEqsgid, CommonCombo.ComboStatus.ALL, cbParent: cboProcessParent, sCase: "PROCESS_EQUIPMENT");


            ////공장
            //C1ComboBox[] cboShopChild = { cboProcid, cboEqsgid };
            //_combo.SetCombo(cboShopid, CommonCombo.ComboStatus.SELECT, cbChild: cboShopChild, sCase: "SHOP");
            //cboAreaid.SelectedValue = LoginInfo.CFG_SHOP_ID;
        }

        #endregion

        #region Event

        #region 활성화 대차 LOSS 이력 조회 : btnSearch_Click()
        //조회

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetDEFECT_Info();
            GetLOSS_Info();
            GetGOOD_Info();
        }
        #endregion

        #endregion

        #region Method

        #region 파우치 활성화 FORMATION 불량이력 조회 : GetDEFECT_Info()
        // 조회
        private void GetDEFECT_Info()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("ACTID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FR_DTTM", typeof(string));
                inTable.Columns.Add("TO_DTTM", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PJT_NAME", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("LOTID_RT", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
          
                DataRow newRow = inTable.NewRow();

                newRow["ACTID"] = "DEFECT_LOT"; //불량이력
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["FR_DTTM"] = Util.GetCondition(dtpDateFrom);
                newRow["TO_DTTM"] = Util.GetCondition(dtpDateTo);
                newRow["AREAID"]  = Util.GetCondition(cboAreaid, "SFU4238"); //동을 선택하세요
                if (newRow["AREAID"].Equals("")) return;
                //newRow["PROCID"] = Util.GetCondition(cboProcid, "SFU1459"); //공정을 선택하세요
                //if (newRow["PROCID"].Equals("")) return;
                newRow["PROCID"] = Util.GetCondition(cboProcid, bAllNull: true);
                newRow["EQSGID"] = Util.GetCondition(cboEqsgid, bAllNull: true);
                newRow["PJT_NAME"] = txtPjt.Text == string.Empty ? null : txtPjt.Text;
                newRow["PRODID"] = txtProdid.Text == string.Empty ? null : txtProdid.Text;
                newRow["CTNR_ID"] = txtCartID.Text == string.Empty ? null : txtCartID.Text;
                newRow["LOTID_RT"] = txtLot_RT.Text == string.Empty ? null : txtLot_RT.Text;

                inTable.Rows.Add(newRow);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_DEFECT_HISTORY_PC", "INDATA", "OUTDATA", inTable);
                Util.gridClear(dgSearch_DEFECT);
                Util.GridSetData(dgSearch_DEFECT, dtMain, FrameOperation);
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
        #endregion
        #region 파우치 활성화 FORMATION LOSS이력 조회 : GetLOSS_Info()
        // 조회
        private void GetLOSS_Info()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("ACTID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FR_DTTM", typeof(string));
                inTable.Columns.Add("TO_DTTM", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PJT_NAME", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("LOTID_RT", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));

                DataRow newRow = inTable.NewRow();

                newRow["ACTID"] = "LOSS_LOT"; // LOSS이력
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["FR_DTTM"] = Util.GetCondition(dtpDateFrom);
                newRow["TO_DTTM"] = Util.GetCondition(dtpDateTo);
                newRow["AREAID"] = Util.GetCondition(cboAreaid); //동을 선택하세요
                if (newRow["AREAID"].Equals("")) return;
                //newRow["PROCID"] = Util.GetCondition(cboProcid, "SFU1459"); //공정을 선택하세요
                //if (newRow["PROCID"].Equals("")) return;
                newRow["PROCID"] = Util.GetCondition(cboProcid, bAllNull: true);
                newRow["EQSGID"] = Util.GetCondition(cboEqsgid, bAllNull: true);
                newRow["PJT_NAME"] = txtPjt.Text == string.Empty ? null : txtPjt.Text;
                newRow["PRODID"] = txtProdid.Text == string.Empty ? null : txtProdid.Text;
                newRow["CTNR_ID"] = txtCartID.Text == string.Empty ? null : txtCartID.Text;
                newRow["LOTID_RT"] = txtLot_RT.Text == string.Empty ? null : txtLot_RT.Text;

                inTable.Rows.Add(newRow);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_DEFECT_HISTORY_PC", "INDATA", "OUTDATA", inTable);
                Util.gridClear(dgSearch_LOSS);
                Util.GridSetData(dgSearch_LOSS, dtMain, FrameOperation);
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
        #endregion
        #region 파우치 활성화 FORMATION 양품화이력 조회 : GetGOOD_Info()
        // 조회
        private void GetGOOD_Info()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("ACTID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FR_DTTM", typeof(string));
                inTable.Columns.Add("TO_DTTM", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PJT_NAME", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("LOTID_RT", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));

                DataRow newRow = inTable.NewRow();

                newRow["ACTID"] = "GOOD_LOT"; //양품화이력
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["FR_DTTM"] = Util.GetCondition(dtpDateFrom);
                newRow["TO_DTTM"] = Util.GetCondition(dtpDateTo);
                newRow["AREAID"] = Util.GetCondition(cboAreaid);
                if (newRow["AREAID"].Equals("")) return;
                //newRow["PROCID"] = Util.GetCondition(cboProcid, "SFU1459"); //공정을 선택하세요
                //if (newRow["PROCID"].Equals("")) return;
                newRow["PROCID"] = Util.GetCondition(cboProcid, bAllNull: true);
                newRow["EQSGID"] = Util.GetCondition(cboEqsgid, bAllNull: true);
                newRow["PJT_NAME"] = txtPjt.Text == string.Empty ? null : txtPjt.Text;
                newRow["PRODID"] = txtProdid.Text == string.Empty ? null : txtProdid.Text;
                newRow["CTNR_ID"] = txtCartID.Text == string.Empty ? null : txtCartID.Text;
                newRow["LOTID_RT"] = txtLot_RT.Text == string.Empty ? null : txtLot_RT.Text;

                inTable.Rows.Add(newRow);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_DEFECT_HISTORY_PC", "INDATA", "OUTDATA", inTable);
                Util.gridClear(dgSearch_GOOD);
                Util.GridSetData(dgSearch_GOOD, dtMain, FrameOperation);
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
        #endregion
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

        #region [Validation]
        /// <summary>
        /// Loss 췻 Validation
        /// </summary>
        private bool ValidationCancel()
        {
            if (dgSearch_DEFECT.Rows.Count == 1)
            {
                // 조회된 데이터가 없습니다.
                Util.MessageValidation("SFU3537");
                return false;
            }

            DataRow[] drInfo = Util.gridGetChecked(ref dgSearch_DEFECT, "CHK");

            if (drInfo.Count() <= 0)
            {
                // 선택된 데이터가 없습니다.
                Util.MessageValidation("SFU3538");
                return false;
            }
         

            return true;
        }

        #endregion

        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void dgSearch_GOOD_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Name == "CHK")
            {
                if (Util.NVC(DataTableConverter.GetValue(dgSearch_GOOD.Rows[e.Row.Index].DataItem, "CANCELDTTM")) != string.Empty
                    || (Util.NVC(DataTableConverter.GetValue(dgSearch_GOOD.Rows[e.Row.Index].DataItem, "CANCEL_ABLE_FLAG"))) != "Y")
                {
                    e.Cancel = true;
                }
                else
                {
                    e.Cancel = false;
                }
            }
        }

        private void btnGoodCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationGoodCancel())
            {
                return;
            }

            popUpGoodCancel();
        }

        private void popUpGoodCancel()
        {
            DataTable dtGoodCancel = DataTableConverter.Convert(dgSearch_GOOD.ItemsSource).Select("CHK = '1'").CopyToDataTable();

            COM001_246_GOOD_CANCEL popupGoodCancel = new COM001_246_GOOD_CANCEL();


            object[] parameters = new object[2];
            parameters[0] = dtGoodCancel;
            parameters[1] = cboAreaid.SelectedValue.ToString();
            C1WindowExtension.SetParameters(popupGoodCancel, parameters);
            popupGoodCancel.Closed += new EventHandler(popupGoodCancel_Closed);
            grdMain.Children.Add(popupGoodCancel);
            popupGoodCancel.BringToFront();
        }

        private void popupGoodCancel_Closed(object sender, EventArgs e)
        {
            COM001_246_GOOD_CANCEL popup = sender as COM001_246_GOOD_CANCEL;

            if (popup.DialogResult == MessageBoxResult.OK)
            {
                // 재조회
                GetGOOD_Info();
            }

            this.grdMain.Children.Remove(popup);
        }

        private bool ValidationGoodCancel()
        {
            if (dgSearch_GOOD.Rows.Count == 0)
            {
                Util.MessageValidation("SFU3537");  //조회된 데이타가 없습니다
                return false;
            }

            DataRow[] drInfo = Util.gridGetChecked(ref dgSearch_GOOD, "CHK");

            if (drInfo.Count() <= 0)
            {
                Util.MessageValidation("SFU3538");  //선택된 데이터가 없습니다
                return false;
            }

            for(int inx = 0; inx < drInfo.Length; inx++)
            {
                if (Util.NVC(drInfo[inx]["CANCELDTTM"]) != string.Empty || Util.NVC(drInfo[inx]["CANCEL_ABLE_FLAG"]) != "Y")
                {
                    Util.MessageValidation("SFU1939");  //취소 할 수 있는 상태가 아닙니다.
                }
            }


            return true;
        }
    }
}
