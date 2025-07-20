/*************************************************************************************
 Created Date : 2021.11.04
      Creator : 공민경
   Decription : VD 설비 SKID 공급 요청 이력 조회 - 부분 요청 취소 팝업
--------------------------------------------------------------------------------------
 [Change History]
    2021.11.04  공민경 선임 : 신규 생성
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Media;

namespace LGC.GMES.MES.MCS001
{
	public partial class MCS001_071_REQUEST_CANCEL : C1Window, IWorkArea
	{
        private readonly Util _util = new Util();

        public bool IsUpdated;

        private CheckBoxHeaderType _inBoxHeaderType;

        private string _REQUEST_ID = string.Empty;  //요청ID
        private string _USERID = string.Empty;  //요청자
        private string _REQ_CNCL_RSN_CNTT = string.Empty;  //취소사유(비고)

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public MCS001_071_REQUEST_CANCEL()
		{
			InitializeComponent();
		}

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameters = C1WindowExtension.GetParameters(this);
            _REQUEST_ID = parameters[0] as string; // 요청ID
            _USERID = parameters[1] as string; //요청자
            _REQ_CNCL_RSN_CNTT = parameters[2] as string; //취소사유(비고)

            txtRequestID.Text = _REQUEST_ID;

            SelectLotList();
            this.Loaded -= C1Window_Loaded;

        }

        private void SelectLotList()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "DA_MCS_SEL_VD_EQPT_SPLY_REQ_LOT_NJ";

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("SPLY_REQ_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["SPLY_REQ_ID"] = _REQUEST_ID;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgList, bizResult, null, true);

                    _util.SetDataGridMergeExtensionCol(dgList, new string[] { "CSTID" }, DataGridMergeMode.VERTICALHIERARCHI);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
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

        private void tbCheckHeaderAll_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1DataGrid dg = dgList;
            if (dg?.ItemsSource == null) return;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                switch (_inBoxHeaderType)
                {
                    case CheckBoxHeaderType.Zero:

                        if (DataTableConverter.GetValue(row.DataItem, "DEL_FLAG").ToString() == "Y")
                        {
                            DataTableConverter.SetValue(row.DataItem, "CHK", false);
                        }
                        else
                        {
                            DataTableConverter.SetValue(row.DataItem, "CHK", true);
                        }
                        break;
                    case CheckBoxHeaderType.One:
                        DataTableConverter.SetValue(row.DataItem, "CHK", false);
                        break;
                }
            }

            switch (_inBoxHeaderType)
            {
                case CheckBoxHeaderType.Zero:
                    _inBoxHeaderType = CheckBoxHeaderType.One;
                    break;
                case CheckBoxHeaderType.One:
                    _inBoxHeaderType = CheckBoxHeaderType.Zero;
                    break;
            }

            dg.EndEdit();
            dg.EndEditRow(true);
        }

        private enum CheckBoxHeaderType
        {
            Zero,
            One,
            Two,
            Three
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (dgList.GetRowCount() == 0 || dgList.GetCheckedDataRow("CHK").Count == 0)
            {
                Util.MessageValidation("SFU1651"); //선택된 항목이 없습니다.
                return;
            }

            try
            {
                //취소 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU1243"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                RequestCancel();
                            }
                        });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }

        //부분 요청 취소
        private void RequestCancel()
        {
            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SPLY_REQ_ID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("REQ_CNCL_RSN_CNTT", typeof(string));
            inDataTable.Columns.Add("SRCTYPE", typeof(string));

            DataRow row = null;

            row = inDataTable.NewRow();
            row["SPLY_REQ_ID"] = _REQUEST_ID; //요청ID
            row["USERID"] = _USERID; //요청자
            row["REQ_CNCL_RSN_CNTT"] = _REQ_CNCL_RSN_CNTT; //취소사유(비고)
            row["SRCTYPE"] = "UI"; //입력구분

            inDataTable.Rows.Add(row);

            //SKID 정보
            DataTable inLotTable = inData.Tables.Add("INLOT");
            inLotTable.Columns.Add("CSTID", typeof(string));
            inLotTable.Columns.Add("LOTID", typeof(string));
            
            foreach (DataRow drSelect in dgList.GetCheckedDataRow("CHK"))
            {
                DataRow row_Lot = inLotTable.NewRow();

                row_Lot["CSTID"] = drSelect["CSTID"];
                row_Lot["LOTID"] = drSelect["LOTID"];               

                inLotTable.Rows.Add(row_Lot);
            }

            //DataTable inLotTable = new DataTable();
            //inLotTable.Columns.Add("CSTID", typeof(string));

            //foreach (DataRow drSelect in dgList.GetCheckedDataRow("CHK"))
            //{
            //    DataRow row_Lot = inLotTable.NewRow();

            //    row_Lot["CSTID"] = drSelect["CSTID"];

            //    inLotTable.Rows.Add(row_Lot);
            //}
            //inLotTable = inLotTable.DefaultView.ToTable(true);

            //DataTable inLot = inData.Tables.Add("INLOT");
            //inLot.Columns.Add("CSTID", typeof(string));

            //foreach (DataRow dr in inLotTable.Rows)
            //{
            //    DataRow rowDistinct = inLot.NewRow();

            //    rowDistinct["CSTID"] = dr["CSTID"];

            //    inLot.Rows.Add(rowDistinct);
            //}

            try
            {
                //VD SKID 공급 요청 취소 (부분 취소)
                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SKID_SPLY_REQ_CANCEL_NJ", "INDATA,INLOT", null, (Result, ex) =>
                {
                    HiddenLoadingIndicator();
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    };
                    Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                    IsUpdated = true;
                    SelectLotList();
                }, inData);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.AlertByBiz("BR_PRD_REG_SKID_SPLY_REQ_CANCEL_NJ", ex.Message, ex.ToString());
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void dgList_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dgList.GetRowCount() == 0)
            {
                return;
            }

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgList.GetCellFromPoint(pnt);

            if (cell == null || cell.Value == null)
            {
                return;
            }
            if (cell.Column.Name != "CHK")
            {
                return;
            }

            int seleted_row = dgList.CurrentRow.Index;
            string CSTID = DataTableConverter.GetValue(dgList.Rows[seleted_row].DataItem, "CSTID").ToString();

            if(DataTableConverter.GetValue(dgList.Rows[seleted_row].DataItem, "DEL_FLAG").ToString() == "Y")
            {
                DataTableConverter.SetValue(dgList.Rows[seleted_row].DataItem, "CHK", false);
                return;
            }

            if (DataTableConverter.GetValue(dgList.Rows[seleted_row].DataItem, "CHK").Equals("True"))
            {
                foreach (C1.WPF.DataGrid.DataGridRow row in dgList.Rows)
                {
                    if(CSTID == DataTableConverter.GetValue(row.DataItem, "CSTID").ToString())
                    {
                        DataTableConverter.SetValue(row.DataItem, "CHK", true);
                    }
                }
            }
            else
            {
                foreach (C1.WPF.DataGrid.DataGridRow row in dgList.Rows)
                {
                    if (CSTID == DataTableConverter.GetValue(row.DataItem, "CSTID").ToString())
                    {
                        DataTableConverter.SetValue(row.DataItem, "CHK", false);
                    }
                }
            }
        }
    }
}