/*************************************************************************************
 Created Date : 2017.05.22
      Creator : 이영준S
   Decription : 전지 5MEGA-GMES 구축 - 1차 포장 구성 - 작업시작 팝업
--------------------------------------------------------------------------------------
 [Change History]


**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using LGC.GMES.MES.CMM001.Extensions;

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.ComponentModel;
using System.Threading;
using System.Linq;
using System.Windows.Media;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_027_RUNSTART.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_038_OUTBOX_SPLIT_MERGE : C1Window, IWorkArea
    {
        Util _Util = new Util();

        string _mergeOutboxKey = string.Empty;
        DataTable _selectedOutbox = new DataTable();
        DataTable _splitQty = new DataTable("INTO_OUTBOX");
        DataTable _dtFromBoxes = new DataTable("INFROM_OUTBOX");

        string _USERID = string.Empty;

        string _sPGM_ID = "BOX001_038_OUTBOX_SPLIT_MERGE";

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public BOX001_038_OUTBOX_SPLIT_MERGE()
        {
            InitializeComponent();
        }
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            object[] tmps = C1WindowExtension.GetParameters(this);
            _USERID = Util.NVC(tmps[0]);
        }

        #region Events
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {

            Util.MessageConfirm("SFU1240", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                }
            });
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        private void btnSearch_Split_Click(object sender, RoutedEventArgs e)
        {
            ClearOutBoxDetail();
            GetReworkOutBoxList("SPLIT");
        }
        private void btnSearch_Merge_Click(object sender, RoutedEventArgs e)
        {
            //validation
            ClearOutBoxDetail();
            GetReworkOutBoxList("MERGE");
        }
        private void dgSplit_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            //validation
            SetOutBoxDetail();
        }


        private void btnSplit_Click(object sender, RoutedEventArgs e)
        {
            //validation
            if (dgSplit.SelectedItem == null)
            {
                Util.MessageValidation("SFU4097"); //먼저 Split 대상 OUTBOX를 선택하세요.
                return;
            }
            DoRework("SPLIT");

        }
        private void btnMerge_Click(object sender, RoutedEventArgs e)
        {
            if (dgMerge.SelectedItem == null)
            {
                Util.MessageValidation("SFU4098"); //먼저 Merge 대상 OUTBOX를 선택하세요.
                return;
            }
            DoRework("MERGE");
        }

        private void dgMerge_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            txtMergePalletID.Text = string.Empty;
            txtMergeQty.Text = string.Empty;
            txtMergeBoxQty.Text = string.Empty;

            _mergeOutboxKey = dgMerge.SelectedItem.GetValue("RCV_ISS_ID").ToString();

            if (sender == null)
                return;
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                for (int i = 0; i < dgMerge.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgMerge.Rows[i].DataItem, "RCV_ISS_ID")).Equals(_mergeOutboxKey))
                    {
                        for (int j = 0; j < dgMerge.Columns.Count; j++)
                        {
                            dgMerge.GetCell(i, j).Presenter.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFD0DA"));
                        }
                        txtMergeBoxQty.Text = (txtMergeBoxQty.Text.SafeToInt32() + 1).ToString();
                        txtMergeQty.Text = (txtMergeQty.Text.SafeToInt32() + Util.NVC(DataTableConverter.GetValue(dgMerge.Rows[i].DataItem, "TOTL_QTY")).SafeToInt32()).ToString();
                        txtMergePalletID.Text = Util.NVC(DataTableConverter.GetValue(dgMerge.Rows[i].DataItem, "PALLETID"));
                    }
                }
                //txtMergeQty.Text = totalQty.ToString();
                //txtMergeBoxQty.Text = boxCnt.ToString();
            }));
            SetOutBoxDetail();
        }
        private void dgMerge_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RCV_ISS_ID")).Equals(_mergeOutboxKey))
                    {
                        e.Cell.Presenter.Background = Brushes.Yellow;
                    }
                }
            }));
        }

        #endregion

        #region Methods
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        private void GetReworkOutBoxList(string statCode)
        {
            DataSet inDataSet = new DataSet();
            DataTable inDataTable = inDataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("OUTBOXID");
            inDataTable.Columns.Add("BOX_RCV_ISS_STAT_CODE");
            inDataTable.Columns.Add("LANGID");

            DataRow inDataRow = inDataTable.NewRow();
            inDataRow["OUTBOXID"] = statCode.Equals("SPLIT") ? txtOutboxID_Split.Text : txtOutBoxID_Merge.Text;
            inDataRow["BOX_RCV_ISS_STAT_CODE"] = statCode;
            inDataRow["LANGID"] = LoginInfo.LANGID;
            inDataTable.Rows.Add(inDataRow);

            new ClientProxy().ExecuteService_Multi("BR_PRD_GET_OUTBOX_REWORK_FM", "INDATA", "OUTDATA,OUTDETAIL", (resultSet, ex) =>
            {
                if (ex != null)
                {
                    Util.MessageException(ex);
                }
                Util.GridSetData(statCode.Equals("SPLIT") ? dgSplit : dgMerge, resultSet.Tables["OUTDATA"], FrameOperation, true);
                _selectedOutbox = resultSet.Tables["OUTDETAIL"];
            }, inDataSet);
        }
        private void SetOutBoxDetail()
        {
            txtInfo01.Text = _selectedOutbox.Rows[0]["PRT_ITEM01"].ToString();
            txtInfo02.Text = _selectedOutbox.Rows[0]["PRT_ITEM02"].ToString();
            txtInfo03.Text = _selectedOutbox.Rows[0]["PRT_ITEM03"].ToString();
            txtInfo04.Text = _selectedOutbox.Rows[0]["PRT_ITEM04"].ToString();
            txtInfo05.Text = _selectedOutbox.Rows[0]["PRT_ITEM05"].ToString();
            txtInfo11.Text = _selectedOutbox.Rows[0]["PRT_ITEM08"].ToString();
            txtInfo12.Text = _selectedOutbox.Rows[0]["PRT_ITEM09"].ToString();
            txtInfo13.Text = _selectedOutbox.Rows[0]["PRT_ITEM10"].ToString();
            txtInfo14.Text = _selectedOutbox.Rows[0]["PRT_ITEM11"].ToString();
            txtInfo15.Text = _selectedOutbox.Rows[0]["PRT_ITEM12"].ToString();
            txtInfo21.Text = _selectedOutbox.Rows[0]["PRT_ITEM14"].ToString();
            txtInfo31.Text = _selectedOutbox.Rows[0]["PRT_ITEM15"].ToString();
            txtInfo22.Text = _selectedOutbox.Rows[0]["PRT_ITEM16"].ToString();
            txtInfo32.Text = _selectedOutbox.Rows[0]["PRT_ITEM17"].ToString();
            txtInfo23.Text = _selectedOutbox.Rows[0]["PRT_ITEM18"].ToString();
            txtInfo33.Text = _selectedOutbox.Rows[0]["PRT_ITEM19"].ToString();
            txtInfo24.Text = _selectedOutbox.Rows[0]["PRT_ITEM20"].ToString();
            txtInfo34.Text = _selectedOutbox.Rows[0]["PRT_ITEM21"].ToString();
            txtInfo25.Text = _selectedOutbox.Rows[0]["PRT_ITEM22"].ToString();
            txtInfo35.Text = _selectedOutbox.Rows[0]["PRT_ITEM23"].ToString();
            txtInfo26.Text = _selectedOutbox.Rows[0]["PRT_ITEM24"].ToString();
            txtInfo36.Text = _selectedOutbox.Rows[0]["PRT_ITEM25"].ToString();
            txtInfoBoxID.Text = _selectedOutbox.Rows[0]["PRT_ITEM26"].ToString();
            bcBoxID.Text = _selectedOutbox.Rows[0]["PRT_ITEM26"].ToString();

        }

        private void ClearOutBoxDetail()
        {
            txtInfo01.Text = txtInfo02.Text = txtInfo03.Text = txtInfo04.Text = txtInfo05.Text = txtInfo11.Text = txtInfo12.Text = txtInfo13.Text =
            txtInfo14.Text = txtInfo15.Text = txtInfo21.Text = txtInfo31.Text = txtInfo22.Text = txtInfo32.Text = txtInfo23.Text = txtInfo33.Text = txtInfo24.Text =
            txtInfo34.Text = txtInfo25.Text = txtInfo35.Text = txtInfo26.Text = txtInfo36.Text = txtInfoBoxID.Text = bcBoxID.Text = string.Empty;
        }
        private bool PrintLabel(string zpl, DataRow drPrtInfo)
        {
            if (drPrtInfo?.Table == null)
            {
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3030"));

                return false;
            }

            bool brtndefault = false;
            if (drPrtInfo.Table.Columns.Contains("PORTNAME") && drPrtInfo["PORTNAME"].ToString().Trim().Length > 0)
            {
                if (drPrtInfo["PORTNAME"].ToString().ToUpper().Equals("USB"))
                {
                    brtndefault = FrameOperation.Barcode_ZPL_USB_Print(zpl);
                    if (brtndefault == false)
                    {
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else if (drPrtInfo["PORTNAME"].ToString().IndexOf("LPT", StringComparison.Ordinal) >= 0)
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_LPT_Print(drPrtInfo, zpl);
                    if (brtndefault == false)
                    {
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_Print(drPrtInfo, zpl);
                    if (brtndefault == false)
                    {
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
            }
            else
            {
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3031"));
                Util.MessageValidation("SFU3031");
            }

            return brtndefault;
        }
        private bool CanSplit()
        {
            int cnt = 0;
            int sum = 0;
            _splitQty.Reset();

            _splitQty.Columns.Add("TOTL_QTY");
            DataRow dr1 = _splitQty.NewRow();
            DataRow dr2 = _splitQty.NewRow();
            DataRow dr3 = _splitQty.NewRow();
            DataRow dr4 = _splitQty.NewRow();
            DataRow dr5 = _splitQty.NewRow();


            if (nbSplit1.Value > 0)
            {
                cnt++;
                sum += (int)nbSplit1.Value;
                dr1["TOTL_QTY"] = (int)nbSplit1.Value;
                _splitQty.Rows.Add(dr1);
            }
            if (nbSplit2.Value > 0)
            {
                cnt++;
                sum += (int)nbSplit2.Value;
                dr2["TOTL_QTY"] = (int)nbSplit2.Value;
                _splitQty.Rows.Add(dr2);
            }
            if (nbSplit3.Value > 0)
            {
                cnt++;
                sum += (int)nbSplit3.Value;
                dr3["TOTL_QTY"] = (int)nbSplit3.Value;
                _splitQty.Rows.Add(dr3);
            }
            if (nbSplit4.Value > 0)
            {
                cnt++;
                sum += (int)nbSplit4.Value;
                dr4["TOTL_QTY"] = (int)nbSplit4.Value;
                _splitQty.Rows.Add(dr4);
            }
            if (nbSplit5.Value > 0)
            {
                cnt++;
                sum += (int)nbSplit5.Value;
                dr5["TOTL_QTY"] = (int)nbSplit5.Value;
                _splitQty.Rows.Add(dr5);
            }
            if (sum != (int)dgSplit.SelectedItem.GetValue("TOTL_QTY"))
            {
                Util.MessageValidation("SFU4099");//선택한 Box 수량과 Split 수량의 합이 같아야합니다.
                return false;
            }
            if (cnt < 2)
            {
                Util.MessageValidation("SFU4100"); //최소 2개 이상의 Box로 Split 할 수 있습니다.
                return false;
            }
            return true;
        }
        private bool CanMerge()
        {
            //Validation 추가
            if (txtMergeBoxQty.Text.Equals("1"))
            {
                Util.MessageValidation("SFU4101"); //최소 2개 이상의 Box로 Merge 할 수 있습니다.
                return false;
            }

            DataTable dt = ((DataView)dgMerge.ItemsSource).Table;
            _dtFromBoxes = dt.Select("RCV_ISS_ID = '" + _mergeOutboxKey + "'").CopyToDataTable();
            _dtFromBoxes.TableName = "INFROM_OUTBOX";
            return true;
        }
        private void DoRework(string statCode)
        {
            try
            {
                string outBoxID = string.Empty;
                string lblCode = string.Empty;
                string zplCode = string.Empty;

                //PRINTER SETTING 변수
                string sPrt = string.Empty;
                string sRes = string.Empty;
                string sCopy = string.Empty;
                string sXpos = string.Empty;
                string sYpos = string.Empty;
                string sDark = string.Empty;
                DataRow drPrtInfo = null;

                if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                    return;

                if (statCode.Equals("SPLIT") && !CanSplit())
                    return;
                if (statCode.Equals("MERGE") && !CanMerge())
                    return;

                string sBizRule = "BR_PRD_REG_OUTBOX_REWORK_FM";

                DataSet inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");

                inDataTable.Columns.Add("LANGID");
                inDataTable.Columns.Add("USERID");
                inDataTable.Columns.Add("BOX_RCV_ISS_STAT_CODE");
                inDataTable.Columns.Add("ACTUSER");
                inDataTable.Columns.Add("PGM_ID");    //라벨 이력 저장용
                inDataTable.Columns.Add("BZRULE_ID"); //라벨 이력 저장용

                DataRow inDataRow = inDataTable.NewRow();
                inDataRow["LANGID"] = LoginInfo.LANGID;
                inDataRow["USERID"] = "";
                inDataRow["BOX_RCV_ISS_STAT_CODE"] = statCode;
                inDataRow["ACTUSER"] = _USERID;
                inDataRow["PGM_ID"] = _sPGM_ID;
                inDataRow["BZRULE_ID"] = sBizRule;
                inDataTable.Rows.Add(inDataRow);

                if (statCode.Equals("SPLIT"))
                {
                    DataTable inFromBoxTable = inDataSet.Tables.Add("INFROM_OUTBOX");
                    inFromBoxTable.Columns.Add("RCV_ISS_ID");
                    inFromBoxTable.Columns.Add("PALLETID");
                    inFromBoxTable.Columns.Add("OWMS_STCK_ID");
                    inFromBoxTable.Columns.Add("TOTL_QTY");
                    inFromBoxTable.Columns.Add("OUTBOXID");
                    DataRow inFromBoxRow = inFromBoxTable.NewRow();
                    inFromBoxRow["RCV_ISS_ID"] = dgSplit.SelectedItem.GetValue("RCV_ISS_ID").ToString();
                    inFromBoxRow["PALLETID"] = dgSplit.SelectedItem.GetValue("PALLETID").ToString();
                    inFromBoxRow["OWMS_STCK_ID"] = dgSplit.SelectedItem.GetValue("OWMS_STCK_ID").ToString();
                    inFromBoxRow["TOTL_QTY"] = dgSplit.SelectedItem.GetValue("TOTL_QTY").ToString();
                    inFromBoxRow["OUTBOXID"] = dgSplit.SelectedItem.GetValue("OUTBOXID").ToString();
                    inFromBoxTable.Rows.Add(inFromBoxRow);

                    inDataSet.Tables.Add(_splitQty); // "INTO_OUTBOX" DataTable

                }
                else
                {
                    inDataSet.Tables.Add(_dtFromBoxes); // "INFROM_OUTBOX" DataTable

                    DataTable inToBoxTable = inDataSet.Tables.Add("INTO_OUTBOX");
                    inToBoxTable.Columns.Add("TOTL_QTY");
                    DataRow inToBoxRow = inToBoxTable.NewRow();
                    inToBoxRow["TOTL_QTY"] = txtMergeQty.Text.SafeToInt32();
                    inToBoxTable.Rows.Add(inToBoxRow);
                }

                DataTable inPrintTable = inDataSet.Tables.Add("INPRINT");
                inPrintTable.Columns.Add("PRMK");
                inPrintTable.Columns.Add("RESO");
                inPrintTable.Columns.Add("PRCN");
                inPrintTable.Columns.Add("MARH");
                inPrintTable.Columns.Add("MARV");
                inPrintTable.Columns.Add("DARK");
                DataRow inPrintDr = inPrintTable.NewRow();
                inPrintDr["PRMK"] = sPrt;
                inPrintDr["RESO"] = sRes;
                inPrintDr["PRCN"] = sCopy;
                inPrintDr["MARH"] = sXpos;
                inPrintDr["MARV"] = sYpos;
                inPrintDr["DARK"] = sDark;
                inPrintTable.Rows.Add(inPrintDr);

                //DataSet resultSet = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_OUTBOX_REWORK_FM", "INDATA,INFROM_OUTBOX,INTO_OUTBOX,INPRINT", "OUTBOX,OUTZPL", inDataSet, null);
                DataSet resultSet = new ClientProxy().ExecuteServiceSync_Multi(sBizRule, "INDATA,INFROM_OUTBOX,INTO_OUTBOX,INPRINT", "OUTBOX,OUTZPL", inDataSet, null);

                if (resultSet != null && resultSet.Tables.Count > 0)
                {
                    if ((resultSet.Tables.IndexOf("OUTBOX") > -1) && resultSet.Tables["OUTBOX"].Rows.Count > 0)
                    {
                        outBoxID = resultSet.Tables["OUTBOX"].Rows[0]["BOXID"].ToString();
                    }
                    if ((resultSet.Tables.IndexOf("OUTZPL") > -1) && resultSet.Tables["OUTZPL"].Rows.Count > 0)
                    {
                        for (int i = 0; i < resultSet.Tables["OUTZPL"].Rows.Count; i++)
                        {
                            lblCode = resultSet.Tables["OUTZPL"].Rows[i]["LABEL_CODE"].ToString();
                            zplCode = resultSet.Tables["OUTZPL"].Rows[i]["ZPLCODE"].ToString();
                            PrintLabel(zplCode, drPrtInfo);
                        }
                    }
                    GetReworkOutBoxList(statCode);
                    Util.MessageInfo("SFU1889"); //정상 처리 되었습니다

                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }


        #endregion

    }
}
