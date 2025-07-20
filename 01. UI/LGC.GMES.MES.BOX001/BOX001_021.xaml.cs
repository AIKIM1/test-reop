/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_021 : UserControl
    {

        #region Declaration & Constructor 

        CommonCombo _combo = new CMM001.Class.CommonCombo();

        public BOX001_021()
        {
            InitializeComponent();
            Loaded += BOX001_021_Loaded;
        }

        private void BOX001_021_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= BOX001_021_Loaded;

            txtCellid.Focus();
            txtCellid.SelectAll();

        }

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion


        #region Initialize


        #endregion


        #region Event

        /// <summary>
        /// 교체 이력 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string sCellid = txtCellid.Text.Trim();
            if (sCellid == string.Empty)
            {
                Util.AlertInfo("SFU1323"); //"CELLID를 스캔 또는 입력하세요."
            }
            else
            {
                GetCellResult(new string[] { sCellid });
            }

        }

        private void txtCellid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {               
                string sCellid = txtCellid.Text.Trim();
                if (sCellid != string.Empty)
                {
                    GetCellResult(new string[] { sCellid });
                }
            }
        }

        #endregion

        #region Mehod

        /// <summary>
        /// Cell 정보 조회
        /// </summary>
        /// <param name="sCellid"></param>
        private bool GetCellResult(string[] sCellid)
        {
            // 조회 전 이전 데이터 초기화
            dgCellResult.ItemsSource = null;
            dgCondition.ItemsSource = null;
            dgCheckResult.ItemsSource = null;

            try
            {

                DataSet indataSet = new DataSet();
                DataTable RQSTDT = indataSet.Tables.Add("INDATA");

                RQSTDT.Columns.Add("SUBLOTID", typeof(string));

                for (int i = 0; i < sCellid.Length; i++)
                {
                    if (!string.IsNullOrEmpty(sCellid[i]))
                    {
                        DataRow dr = RQSTDT.NewRow();
                        dr["SUBLOTID"] = sCellid[i];
                        RQSTDT.Rows.Add(dr);
                    }
                }


                // ClientProxy2007
                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_SHIPABLE_FCS_DATA", "INDATA", "OUTDATA_MES_CELL_DATA,OUTDATA_FCS_CELL_DATA,OUTDATA_SHIP_RANGE", indataSet);

                Util.GridSetData(dgCellResult, dsResult.Tables["OUTDATA_MES_CELL_DATA"], FrameOperation);
                Util.GridSetData(dgCondition, dsResult.Tables["OUTDATA_SHIP_RANGE"], FrameOperation);
                Util.GridSetData(dgCheckResult, dsResult.Tables["OUTDATA_FCS_CELL_DATA"], FrameOperation);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 검사 조건 / 활성화 데이터 조회 함수 호출
        /// </summary>
        /// <param name="sCellid"></param>
        private void SelectFormationData(string sCellid, string sAREAID, string sSHOPID, string sMDLLOT_ID)
        {

            try
            {
                //DataTable RQSTDT = new DataTable();
                //RQSTDT.Columns.Add("CELLID", typeof(string));

                //DataRow dr = RQSTDT.NewRow();
                //dr["CELLID"] = sCellid;
                //RQSTDT.Rows.Add(dr);

                //DataTable dtRslt = null;
                //if (sAREAID.Equals("A1"))
                //{
                //    dtRslt = new ClientProxy2007("AF1").ExecuteServiceSync("GET_GMES_SHIPMENT_CELL_INFO_CHECK_V01", "INDATA", "OUTDATA", RQSTDT);
                //}
                //else if (sAREAID.Equals("A2") || sAREAID.Equals("S2"))
                //{
                //    dtRslt = new ClientProxy2007("AF2").ExecuteServiceSync("GET_GMES_SHIPMENT_CELL_INFO_CHECK_V01", "INDATA", "OUTDATA", RQSTDT);
                //}
                //else
                //{
                //    return;
                //}

                DataSet indataSet = new DataSet();
                DataTable RQSTDT = indataSet.Tables.Add("INDATA");

                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("SUBLOTID", typeof(string));                
                RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SUBLOTID"] = sCellid;
                dr["SHOPID"] = sSHOPID;
                dr["AREAID"] = sAREAID;
                dr["MDLLOT_ID"] = sMDLLOT_ID;
                dr["USERID"] = LoginInfo.USERID;
                RQSTDT.Rows.Add(dr);


                // ClientProxy2007
                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_FCS_VALIDATION", "INDATA", "OUTDATA", indataSet);


                if (dsResult.Tables[0].Rows.Count > 0)
                {
                    ////dgCondition.ItemsSource = DataTableConverter.Convert(dtRslt);
                    ////dgCheckResult.ItemsSource = DataTableConverter.Convert(dtRslt);

                    Util.GridSetData(dgCondition, dsResult.Tables["OUTDATA_SHIP_RANGE"], FrameOperation);
                    Util.GridSetData(dgCheckResult, dsResult.Tables["OUTDATA_CELL_DATA"], FrameOperation);
                }
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        private void txtCellid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    GetCellResult(sPasteStrings);
                    //for (int i = 0; i < sPasteStrings.Length; i++)
                    //{
                    //    if (!string.IsNullOrEmpty(sPasteStrings[i]) && GetCellResult(sPasteStrings[i].Trim()) == false)
                    //    {
                    //        break;
                    //    }
                    //    System.Windows.Forms.Application.DoEvents();
                    //}
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                e.Handled = true;
            }
        }
    }
}
