/*************************************************************************************
 Created Date : 
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
 2023.03.15  LEEHJ     : 소형활성화 MES 복사
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.ComponentModel;
using System.Threading;
using System.Linq;
using C1.WPF.DataGrid.Summaries;
using System.Windows.Media;

namespace LGC.GMES.MES.FCS002
{
    /// <summary>
    /// BOX001_201_RUNSTART.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS002_311_REQUEST_BIZWFLOT_SPLIT : C1Window, IWorkArea
    {
        Util _util = new Util();

        public string[] _sBizWFSPlitLotID;

        string _BizWFLotReqDocTypeCode = string.Empty;
        string _BizWFLotReqDocTypeName = string.Empty;
        string _BizWFLotReqDocNo = string.Empty;
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public string PALLET_ID
        {
            get;
            set;
        }
        #region Initialize
        public FCS002_311_REQUEST_BIZWFLOT_SPLIT()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            _BizWFLotReqDocTypeCode = Util.NVC(tmps[0]);
            _BizWFLotReqDocTypeName = Util.NVC(tmps[1]);
            _BizWFLotReqDocNo = Util.NVC(tmps[2]);

            setBizWFLotDocTextBox();

            InitControl();
        }

        private void setBizWFLotDocTextBox()
        {
            txtBizWFReqDocTypeName.Text = _BizWFLotReqDocTypeName.ToString();
            txtBizWFReqDocNo.Text = _BizWFLotReqDocNo.ToString();
        }
        private void InitControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
        }
        #endregion

        #region [EVENT]

        #region 텍스트박스 포커스 : text_GotFocus()
        private void text_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }
        #endregion

        #region Pallet 생성 : btnSave_Click()
        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationBizWFLotSplit())
                {
                    return;
                }


                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                "SFU8391"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {

                    if (result == MessageBoxResult.OK)
                    {

                        loadingIndicator.Visibility = Visibility.Visible;

                        DataTable dtRqst = new DataTable();
                        dtRqst.Columns.Add("SRCTYPE", typeof(string));
                        dtRqst.Columns.Add("LOTID", typeof(string));
                        dtRqst.Columns.Add("BIZWF_SPLIT_LOTID", typeof(string));
                        dtRqst.Columns.Add("SPLIT_QTY", typeof(decimal));
                        dtRqst.Columns.Add("USERID", typeof(string));

                        DataRow dr = null;

                        for (int i = 0; i < dgList.Rows.Count; i++)
                        {
                            dr = dtRqst.NewRow();
                            dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            dr["LOTID"] = DataTableConverter.GetValue(dgList.Rows[i].DataItem, "LOTID").ToString();
                            dr["BIZWF_SPLIT_LOTID"] = DataTableConverter.GetValue(dgList.Rows[i].DataItem, "BIZWF_SPLIT_LOTID").ToString();
                            dr["SPLIT_QTY"] = decimal.Parse(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "BIZWF_QTY").ToString());
                            dr["USERID"] = LoginInfo.USERID;
                            dtRqst.Rows.Add(dr);
                        }

                        new ClientProxy().ExecuteService("BR_PRD_REG_BIZWF_SPLIT_LOT", "INDATA", null , dtRqst, (dtResult, searchException) =>
                        {
                            try
                            {
                                if (searchException != null)
                                {
                                    Util.MessageException(searchException);
                                    return;
                                }
                                Util.MessageInfo("SFU1889");
                                //GridClear();
                                _sBizWFSPlitLotID = new string[dgList.Rows.Count];

                                for(int i = 0; i < dgList.Rows.Count; i++)
                                {
                                    _sBizWFSPlitLotID[i] = DataTableConverter.GetValue(dgList.Rows[i].DataItem, "BIZWF_SPLIT_LOTID").ToString();
                                }

                                this.DialogResult = MessageBoxResult.OK;
                                this.Close();
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                            }
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion


        private bool ValidationBizWFLotSplit()
        {

            if(dgList.Rows.Count == 0)
            {
                Util.Alert("SFU2052");
                return false;
            }

            for (int i = 0; i < dgList.Rows.Count; i++)
            {
                DataTableConverter.SetValue(dgList.Rows[i].DataItem, "CHK", false);
            }

            bool isFail = false;

            for (int i = 0; i < dgList.Rows.Count; i++)
            {
                double BizWFQty = double.Parse(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "BIZWF_QTY").ToString());
                double WipQty2 = double.Parse(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "WIPQTY2").ToString());

                if (BizWFQty <= 0 || BizWFQty >= WipQty2)
                {
                    DataTableConverter.SetValue(dgList.Rows[i].DataItem, "CHK", true);
                    isFail = true;
                }
            }

            if (isFail)
            {
                Util.Alert("SFU1749");
                return false;
            }

            return true;
        }

        #region 닫기 : btnClose_Click()
        /// <summary>
        /// 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }


        #endregion

        #region 삭제 : btnInPalletDelete_Click()
        private void btnInPalletDelete_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region OutBox 체크 : txtInPalletID_KeyDown()
        private void txtLOTID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetBizWFSplitLot(txtLOTID.Text.ToString());
            }
        }
        #endregion

        private void GetBizWFSplitLot(string sLotID)
        {
            try
            {

                if (dgList.GetRowCount() > 0)
                {
                    for (int i = 0; i < dgList.GetRowCount(); i++)
                    {
                        if (DataTableConverter.GetValue(dgList.Rows[i].DataItem, "LOTID").ToString() == sLotID)
                        {
                            Util.Alert("SFU3471", sLotID);   
                            return;
                        }
                    }
                }

                if (dgUnAvailableList.GetRowCount() > 0)
                {
                    for (int i = 0; i < dgUnAvailableList.GetRowCount(); i++)
                    {
                        if (DataTableConverter.GetValue(dgUnAvailableList.Rows[i].DataItem, "LOTID").ToString() == sLotID)
                        {
                            Util.Alert("SFU3471", sLotID);
                            return;
                        }
                    }
                }

                DataTable dtRqst = new DataTable();

                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("SHOPID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("BIZ_WF_REQ_DOC_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("BIZ_WF_REQ_DOC_NO", typeof(string));
                dtRqst.Columns.Add("LOT_REG_TYPE", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["BIZ_WF_REQ_DOC_TYPE_CODE"] = _BizWFLotReqDocTypeCode;
                dr["BIZ_WF_REQ_DOC_NO"] = _BizWFLotReqDocNo;
                dr["LOT_REG_TYPE"] = "REG";
                dr["LOTID"] = sLotID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_BIZWF_SPLIT_LOT", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count == 0)
                {
                    SetUnAvailableLotList(dtRslt, sLotID); //그리드에 디폴트 데이터소스 만들기 위해 dtRslt 도 보냄
                }
                else
                {
                    DataRow[] drUnAvailable = dtRslt.Select("BIZ_WF_CAN_FLAG = 'N'");
                    if (drUnAvailable != null && drUnAvailable.Length > 0)
                    {
                        DataTable dtUnAvailable = drUnAvailable.CopyToDataTable();
                        SetUnAvailableLotList(dtUnAvailable, sLotID);
                    }

                    DataRow[] drAvailable = dtRslt.Select("BIZ_WF_CAN_FLAG = 'Y'");
                    if (drAvailable != null && drAvailable.Length > 0)
                    {
                        DataTable dtAvailable = drAvailable.CopyToDataTable();

                        DataTable dtSource = DataTableConverter.Convert(dgList.ItemsSource);
                        dtSource.Merge(dtAvailable);

                        Util.GridSetData(dgList, dtSource, FrameOperation, true);
                    }
                }

                SetGridRowColor();

                if (dgList.GetRowCount() <= 0 && dgUnAvailableList.GetRowCount() <= 0)
                {
                    Util.AlertInfo("SFU3537");  //조회된 데이타가 없습니다
                }

                txtLOTID.Text = "";

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }


        }
        #endregion
        private void SetUnAvailableLotList(DataTable pDtRslt, string pStrLot)
        {
            if (pDtRslt == null || string.IsNullOrEmpty(pStrLot))
            {
                return;
            }

            DataTable dtSource = DataTableConverter.Convert(dgUnAvailableList.ItemsSource);
            dtSource.Merge(pDtRslt);

            Util.GridSetData(dgUnAvailableList, dtSource, FrameOperation, true);

            //dgUnAvailableList에 넣을 ROW 가 없는 데이터셋을 전달 받았을 경우 LOT 만 넣어줌
            if (pDtRslt.Rows.Count <= 0)
            {
                DataRow drRslt = dtSource.NewRow();
                drRslt["LOTID"] = pStrLot;
                dtSource.Rows.Add(drRslt);

                Util.GridSetData(dgUnAvailableList, dtSource, FrameOperation, true);
            }
        }



        private void setShipToPopControl(string prodID, String ShipToID = null)
        {
           
        }


        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            GridClear();
        }

        private void GridClear()
        {
            Util.gridClear(dgList);
            Util.gridClear(dgUnAvailableList);
            txtLOTID.Clear();
        }

        private void chk_Click(object sender, RoutedEventArgs e)
        {

        }


        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            GridRowRemove();
        }

        private void GridRowRemove() {

            if(dgList.Rows.Count == 0)
            {
                return;
            }

            DataTable dt = DataTableConverter.Convert(dgList.ItemsSource);

            List<DataRow> drList = dt.Select("CHK = 'True'")?.ToList();
            if (drList.Count > 0)
            {
                foreach (DataRow dr in drList)
                {
                    dt.Rows.Remove(dr);
                }
                Util.GridSetData(dgList, dt, FrameOperation, true);
            }
            SetGridRowColor();
        }

        private void dgList_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Column.Name.Equals("BIZWF_QTY"))
            {
                double BizWFQty = double.Parse(Util.NVC(DataTableConverter.GetValue(dgList.Rows[e.Cell.Row.Index].DataItem, "BIZWF_QTY")));
                double LaneQty = double.Parse(Util.NVC(DataTableConverter.GetValue(dgList.Rows[e.Cell.Row.Index].DataItem, "WIPQTY2")));

                if (BizWFQty <= (double)0)
                {
                    Util.Alert("SFU8375");
                    DataTableConverter.SetValue(dgList.Rows[e.Cell.Row.Index].DataItem, "BIZWF_QTY", (double)0);
                }
                else if (BizWFQty >= LaneQty)
                {
                    Util.Alert("SFU8374");
                    DataTableConverter.SetValue(dgList.Rows[e.Cell.Row.Index].DataItem, "BIZWF_QTY", (double)0);
                }
                else
                {
                    DataTableConverter.SetValue(dgList.Rows[e.Cell.Row.Index].DataItem, "RESIDUAL_QTY", LaneQty - BizWFQty);
                }

                SetGridRowColor();
            }
        }

        private void SetGridRowColor()
        {
            for(int i = 0; i < dgList.Rows.Count; i++)
            {
                double BizWFQty = double.Parse(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "BIZWF_QTY").ToString());
                double WipQty2 = double.Parse(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "WIPQTY2").ToString());

                if(BizWFQty <= 0 || BizWFQty >= WipQty2)
                {
                    dgList.Rows[i].Presenter.Background = new SolidColorBrush(Colors.Orange);
                }
                else
                {
                    dgList.Rows[i].Presenter.Background = new SolidColorBrush(Colors.White);
                }
            }
        }

  

        private void txtLot_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    string lotList = string.Empty;
                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (string.IsNullOrEmpty(sPasteStrings[i]))
                        {
                            break;
                        }                       

                        GetBizWFSplitLot(sPasteStrings[i]);
                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }

                e.Handled = true;
            }
        }
    }
}
