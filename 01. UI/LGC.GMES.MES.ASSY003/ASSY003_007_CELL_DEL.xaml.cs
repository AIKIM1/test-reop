/*************************************************************************************
 Created Date : 2024.05.31
      Creator : 이병윤
   Decription : 파우치 조립 불량 Cell 상세 조회 E20240528-000578
--------------------------------------------------------------------------------------
 [Change History]
  2024.05.31  이병윤 : Initial Created.
  2024.08.08  유명환 : E20240715-000211 Cell관리 삭제부분 한건처리에서 다중 처리되도록 수정
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using C1.WPF;

using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;


namespace LGC.GMES.MES.ASSY003
{
    public partial class ASSY003_007_CELL_DEL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _CellID = string.Empty;
        private string _CellIDs = string.Empty;

        public string _ReturnCellIDs
        {
            get { return _CellIDs; }
        }

        BizDataSet _Biz = new BizDataSet();
        private DataTable isDataTable = new DataTable();
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
        public ASSY003_007_CELL_DEL()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _CellID = Util.NVC(tmps[0]);

            //txtCellId.Text = _CellID;
            //txtCellId.IsReadOnly = true;
            //txtNg.IsReadOnly = true;
            //txtNg.Foreground = new SolidColorBrush(Colors.Red);
            //txtNg.FontWeight = FontWeights.Bold;
            GetTrayValid();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgCELLInfo.Rows.Count; i++)
            {
                if(_Util.GetDataGridCheckValue(dgCELLInfo, "CHK", i))
                    _CellIDs += "," + Util.NVC(DataTableConverter.GetValue(dgCELLInfo.Rows[i].DataItem, "CellID"));
            }

            _CellIDs = _CellIDs.Substring(1);
            this.DialogResult = MessageBoxResult.OK;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void txtCellID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _CellID = txtCellId.Text.Trim();

                Add_Scan_Cellid(txtCellId.Text.Trim(), sender, e);

                //if (Add_Scan_Cellid(txtCellId.Text.Trim(), sender, e))
                //    Util.DingPlayer();
                //else
                //    Util.WarningPlayer();
            }
        }
        #endregion

        #region Mehod

        #region [BizRule]

        private string GetTrayValid()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SUBLOTID"] = Util.NVC(_CellID);

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CELL_NG_DEL_NJ", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    string msg = string.Empty;
                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        string sNg = Convert.ToString(dtResult.Rows[i]["JUDGE"]);

                        if (sNg.Equals("AC"))
                        {
                            msg += "ACOH null; ";
                        }
                        else if (sNg.Equals("UT"))
                        {
                            msg += "UTAP null; ";
                        }
                        else if (sNg.Equals("TH"))
                        {
                            msg += "Thickness null; ";
                        }
                        else if (sNg.Equals("TA"))
                        {
                            msg += "Tape null; ";
                        }
                        else if (sNg.Equals("FI"))
                        {
                            msg += "Film Length null; ";
                        }
                        else if (sNg.Equals("NI"))
                        {
                            msg += "FILLING null; ";
                        }
                        else
                        {
                            msg += "";
                        }
                    }
                    HiddenLoadingIndicator();
                    return msg;                    
                }
                else
                {
                    HiddenLoadingIndicator();
                    return "";                    
                }                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HiddenLoadingIndicator();
            }

            return "";
        }

        private bool GetFCSTrayCheck(string sTrayID)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetBR_PRD_GET_FCS_TRAY_CHK_CL();

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = sTrayID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("", "INDATA", "OUTDATA", inTable);

                HiddenLoadingIndicator();

                if (dtResult == null || dtResult.Rows.Count <= 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                return false;
            }
        }

        private bool Add_Scan_Cellid(string sCellId, object sender, KeyEventArgs e = null)
        {
            //연속 데이터 입력으로 인하여 GMES가 뻗는 현상 방지
            ((TextBox)sender).Text = e.Key.ToString();
            FocusManager.SetFocusedElement(FocusManager.GetFocusScope((TextBox)sender), null);
            Keyboard.ClearFocus();

            // MagazineID 공백 or 널값 여부 확인
            if (string.IsNullOrWhiteSpace(sCellId))
            {
                // CELLID를 스캔 또는 입력하세요.
                // Util.MessageValidation("SFU1323");

                Util.MessageValidation("SFU1323", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtCellId.Focus();
                        txtCellId.SelectAll();
                    }
                });

                Util.WarningPlayer();
                return false;

            }

            if (sCellId.Length < 10)
            {
                // CELL ID 길이가 잘못 되었습니다.
                // Util.MessageValidation("SFU1318");
                Util.MessageValidation("SFU1318", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtCellId.Focus();
                        txtCellId.SelectAll();
                    }
                });

                Util.WarningPlayer();
                return false;

            }
            
            // 기본적인 Validation
            // 동일한 Cell ID 가 입력되었는지 여부 확인
            // 스프레드 rows 카운트가 0보다 크면 아래 로직 수행
            if (dgCELLInfo.GetRowCount() > 0)
            {
                // 스프레드에 있는 값과 동일한 값이 입력되었다면 return됨.
                for (int i = 0; i < dgCELLInfo.GetRowCount(); i++)
                {
                    if ((Util.NVC(dgCELLInfo.GetCell(i, dgCELLInfo.Columns["CellID"].Index).Value) == sCellId))
                    {
                        //아래쪽 List에 이미 존재하는 Cell ID입니다.
                        // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3161"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        Util.DingDongPlayer();

                        Util.MessageInfo("SFU3159", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtCellId.Focus();
                                txtCellId.SelectAll();
                            }
                        });
                        return false;
                    }
                }
            }

            try
            {
                if (isDataTable.Columns.Count <= 0)
                {
                    // 데이터 컬럼 정의
                    isDataTable.Columns.Add("CHK", typeof(int));
                    isDataTable.Columns.Add("CellID", typeof(string));
                    isDataTable.Columns.Add("NG", typeof(string));
                }

                DataRow row = isDataTable.NewRow();
                row["CHK"] = 0;                            // 현재 CHECK 여부
                row["CellID"] = sCellId;                   // 현재 스캔된 CELL ID
                row["NG"] = GetTrayValid();                // 현재 스캔된 CELL ID의 NG

                // 스프레드에 데이터 바인딩
                isDataTable.Rows.Add(row);
                Util.GridSetData(dgCELLInfo, isDataTable, FrameOperation, true);

                // 정상적인 경우 다음 Cell 입력할 수 있게 전체 선택.
                txtCellId.Focus();
                txtCellId.SelectAll();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                Util.WarningPlayer();
                return false;
            }
            finally
            {
                txtCellId.Focus();
                txtCellId.SelectAll();
            }

            return true;
        }


        #endregion

        #region [Validation]

        #endregion

        #region[Func]

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
