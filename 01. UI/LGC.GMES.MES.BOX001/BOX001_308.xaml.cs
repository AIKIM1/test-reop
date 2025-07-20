/*************************************************************************************
 Created Date : 2021.01.05
      Creator : 이제섭
   Decription : 포장 공정 Cell 현황 조회 (Cell 포장 정보 & 활성화 측정 데이터 조회)
--------------------------------------------------------------------------------------
 [Change History]
  2021.01.05  DEVELOPER : Initial Created.
  2022.03.29  박수미 : 조회 시 양품 cell은 조회 결과에 반영하고 불량 cell은 메세지 팝업 표시
  2023.01.30  임근영 : 조회 시 없는 CELLID에 대한 팝업을 ID마다가 아닌 한번에 띄우도록 수정
  2023.08.30  김동훈 : IWorkArea 추가
  2024.01.18  이제섭 : 그리드 화면 조정 버튼 추가
  2024.04.24  지광현 : Pallet Hold YN 컬럼 추가, DataGrid 3개 Filter, Sorting 활성화
  2024.07.23  최석준 : 반품여부 컬럼 추가 (2025년 적용예정, 수정 시 연락부탁드립니다)
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
    public partial class BOX001_308 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        public BOX001_308()
        {
            InitializeComponent();

            Initialize();

            Loaded += BOX001_308_Loaded;
        }

        private void BOX001_308_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= BOX001_308_Loaded;

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
        private void Initialize()
        {
            // 사외반품여부 컬럼 숨김여부
            if (GetOcopRtnPsgArea())
            {
                dgCellResult.Columns["RTN_FLAG"].Visibility = Visibility.Visible;
            }

        }
        #endregion

        #region Event

        #region [조회 버튼 클릭 Event]
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
        #endregion

        #region [CellID KeyDown Event]
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

        #endregion

        #region Mehod

        #region [Cell 정보 조회 (BR_GET_CELL_FORM_DATA_BX)]
        private bool GetCellResult(string[] sCellid)
        {
            // 조회 전 이전 데이터 초기화
            dgCellResult.ItemsSource = null;
            dgCondition.ItemsSource = null;
            dgCheckResult.ItemsSource = null;

            try
            {

                //DataSet indataSet = new DataSet();
                //DataTable RQSTDT = indataSet.Tables.Add("INDATA");

                //RQSTDT.Columns.Add("SUBLOTID", typeof(string));

                //for (int i = 0; i < sCellid.Length; i++)
                //{
                //    if (!string.IsNullOrEmpty(sCellid[i]))
                //    {
                //        DataRow dr = RQSTDT.NewRow();
                //        dr["SUBLOTID"] = sCellid[i];
                //        RQSTDT.Rows.Add(dr);
                //    }
                //}

                //// ClientProxy2007
                //DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_CELL_FORM_DATA_BX", "INDATA", "OUTDATA_MES_CELL_DATA,OUTDATA_FCS_CELL_DATA,OUTDATA_SHIP_RANGE", indataSet);

                //Util.GridSetData(dgCellResult, dsResult.Tables["OUTDATA_MES_CELL_DATA"], FrameOperation);
                //Util.GridSetData(dgCondition, dsResult.Tables["OUTDATA_SHIP_RANGE"], FrameOperation);
                //Util.GridSetData(dgCheckResult, dsResult.Tables["OUTDATA_FCS_CELL_DATA"], FrameOperation);

                DataSet indataSet = new DataSet();
                DataTable RQSTDT = indataSet.Tables.Add("INDATA");

                RQSTDT.Columns.Add("SUBLOTID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SUBLOTID"] = "";
                dr["LANGID"] = LoginInfo.LANGID;
                RQSTDT.Rows.Add(dr);

                DataTable MES_CELL_DATA = new DataTable();
                DataTable SHIP_RANGE = new DataTable();
                DataTable FCS_CELL_DATA = new DataTable();

                string nCellid = string.Empty;//

                for (int i = 0; i < sCellid.Length; i++)
                {
                    if (!string.IsNullOrEmpty(sCellid[i]))
                    {
                        try
                        {
                            RQSTDT.Rows[0]["SUBLOTID"] = sCellid[i];
                            DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_CELL_FORM_DATA_BX", "INDATA", "OUTDATA_MES_CELL_DATA,OUTDATA_FCS_CELL_DATA,OUTDATA_SHIP_RANGE,RETVAL", indataSet);             
                            if (Convert.ToString(dsResult.Tables["RETVAL"].Rows[0]["RETVAL"]) == "-1")//
                            {
                                nCellid += sCellid[i]; 
                                if (i < sCellid.Length)  
                                {
                                    nCellid += ",";
                                }
                                continue;
                            }

                            MES_CELL_DATA.Merge(dsResult.Tables["OUTDATA_MES_CELL_DATA"]);
                            SHIP_RANGE.Merge(dsResult.Tables["OUTDATA_SHIP_RANGE"]);
                            FCS_CELL_DATA.Merge(dsResult.Tables["OUTDATA_FCS_CELL_DATA"]);
                            
                        }

                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    }
                }

                Util.GridSetData(dgCellResult, MES_CELL_DATA, FrameOperation);
                Util.GridSetData(dgCondition, SHIP_RANGE, FrameOperation);
                Util.GridSetData(dgCheckResult, FCS_CELL_DATA, FrameOperation);

                if (!string.IsNullOrEmpty(nCellid)) //   
                {
                    nCellid = nCellid.TrimEnd(',');
                    Util.Alert("FM_ME_0003", nCellid);  
                    //return false; 
                }
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            return true;
        }
        #endregion

        #endregion

        #region [Clipboard 데이터 파싱 처리]
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
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                e.Handled = true;
            }
        }
        #endregion

        /// <summary>
        /// 활성화 사외 반품 처리 여부 사용 Area 조회
        /// </summary>
        /// <returns></returns>
        private bool GetOcopRtnPsgArea()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "FORM_OCOP_RTN_PSG_YN";
            dr["CMCODE"] = LoginInfo.CFG_AREA_ID;

            inTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", inTable);

            if (dtRslt.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void btnExpend1_Click(object sender, RoutedEventArgs e)
        {
            if (btnExpend1.Content.Equals("↘"))
            {
                btnExpend1.Content = "↖";

                Row1.Height = new GridLength(1, GridUnitType.Star);
                Row2.Height = new GridLength(0);
                Row3.Height = new GridLength(0);
                Row4.Height = new GridLength(0);
                Row5.Height = new GridLength(0);
            }
            else
            {
                btnExpend1.Content = "↘";

                Row1.Height = new GridLength(1, GridUnitType.Star);
                Row2.Height = new GridLength(8);
                Row3.Height = new GridLength(1, GridUnitType.Star);
                Row4.Height = new GridLength(8);
                Row5.Height = new GridLength(1, GridUnitType.Star);
            }
        }

        private void btnExpend2_Click(object sender, RoutedEventArgs e)
        {
            if (btnExpend2.Content.Equals("↘"))
            {
                btnExpend2.Content = "↙";

                Row1.Height = new GridLength(0);
                Row2.Height = new GridLength(0);
                Row3.Height = new GridLength(1, GridUnitType.Star);
                Row4.Height = new GridLength(0);
                Row5.Height = new GridLength(0);
            }
            else
            {
                btnExpend2.Content = "↘";

                Row1.Height = new GridLength(1, GridUnitType.Star);
                Row2.Height = new GridLength(8);
                Row3.Height = new GridLength(1, GridUnitType.Star);
                Row4.Height = new GridLength(8);
                Row5.Height = new GridLength(1, GridUnitType.Star);
            }
        }

        private void btnExpend3_Click(object sender, RoutedEventArgs e)
        {
            if (btnExpend3.Content.Equals("↘"))
            {
                btnExpend3.Content = "↙";

                Row1.Height = new GridLength(0);
                Row2.Height = new GridLength(8);
                Row3.Height = new GridLength(0);
                Row4.Height = new GridLength(0);
                Row5.Height = new GridLength(1, GridUnitType.Star);
            }
            else
            {
                btnExpend3.Content = "↘";

                Row1.Height = new GridLength(1, GridUnitType.Star);
                Row2.Height = new GridLength(8);
                Row3.Height = new GridLength(1, GridUnitType.Star);
                Row4.Height = new GridLength(8);
                Row5.Height = new GridLength(1, GridUnitType.Star);
            }
        }

    }
}
