/*************************************************************************************
 Created Date : 2016.08.01
      Creator : Jeong Hyeon Sik
   Decription : Pack Lot이력- LOT ID 교체 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.01    Jeong Hyeon Sik : Initial Created.
  2018.08.07    손우석          : SM CMI Pack 메시지 다국어 처리 요청
  2019.12.11    손우석          : SM CMI Pack 메시지 다국어 처리 요청
  2020.01.15    염규범          : SI 키파츠 결합 해체시 SEQ 중복 Validtion 추가 
  2023.08.22    정용석          : SM Keypart 삭제시 N빵 삭제 기능 추가
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_005_LOTCHANGE : C1Window, IWorkArea
    {
        #region Member Variable List
        private string LOTID = string.Empty;
        private bool isLoading = false;
        #endregion

        #region Property
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Constructor 
        public PACK001_005_LOTCHANGE()
        {
            InitializeComponent();
        }
        #endregion

        #region Member Function Lists...
        private void Initialize()
        {
            try
            {
                btnDelete.Visibility = Visibility.Collapsed; // 2024.11.08. 김영국 - Delete Button Hidden 처리.
                this.isLoading = true;
                List<Button> listAuth = new List<Button>();
                listAuth.Add(this.btnOK);
                listAuth.Add(this.btnDelete);
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

                object[] arrObject = C1WindowExtension.GetParameters(this);
                if (arrObject != null)
                {
                    DataTable dt = arrObject[0] as DataTable;
                    if (!CommonVerify.HasTableRow(dt))
                    {
                        return;
                    }

                    this.LOTID = Util.NVC(dt.Rows[0]["LOTID"]);
                    this.txtLOTID.Text = this.LOTID;
                    this.txtCurrentMaterialLOTID.Text = Util.NVC(dt.Rows[0]["LOTMID"]);
                    if (string.IsNullOrEmpty(this.LOTID))
                    {
                        return;
                    }
                    this.GetKeypartInfo(this.LOTID);
                }
                this.isLoading = false;
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void ChangeKeypartAndDelete(bool isDeleteFlag)
        {
            try
            {
                TextRange textRange = new TextRange(rtxNote.Document.ContentStart, rtxNote.Document.ContentEnd);
                string changeKeypartNote = textRange.Text;

                if (Util.gridFindDataRow(ref dgKeyPart, "CHK", "True", false).Equals(-1))
                {
                    Util.MessageInfo("SFU1636");
                    return;
                }

                int rowIndex = isDeleteFlag ? Util.gridFindDataRow(ref dgKeyPart, "INPUT_LOTID", txtCurrentMaterialLOTID.Text, false) : Util.gridFindDataRow(ref dgKeyPart, "CHK", "True", false);
                string attacheSequenceNo = Util.NVC(DataTableConverter.GetValue(dgKeyPart.Rows[rowIndex].DataItem, "PRDT_ATTCH_PSTN_NO"));
                string inputProcessID = Util.NVC(DataTableConverter.GetValue(dgKeyPart.Rows[rowIndex].DataItem, "INPUT_PROCID"));

                if (isDeleteFlag.Equals(false))
                {
                    int iChkOverLap = ((DataView)dgKeyPart.ItemsSource).Table.Select("MTRLID <>'" + "' AND MTRLID = '" + Util.NVC(txtMaterialID.Text.ToString()) + "' AND PRDT_ATTCH_PSTN_NO = '" + Util.NVC(txtAttachSequenceNo.Text) + "'AND INPUT_PROCID = '" + Util.NVC(txtInputProcessID.Text) + "'").Length;

                    if (iChkOverLap > 1)
                    {
                        Util.MessageValidation("SFU8158", txtMaterialID.Text.ToString());
                        return;
                    }
                    //else if (iChkOverLap == 1 && !(attacheSequenceNo.Equals(Util.NVC(txtAttachSequenceNo.Text))))
                    //{
                    //    Util.MessageInfo("SFU4562");
                    //    return;
                    //}
                }

                DataTable dt = this.TransactionKeypartChange(this.LOTID, this.txtCurrentMaterialLOTID.Text, this.txtChangeMaterialLOTID.Text, this.txtAttachSequenceNo.Text, this.txtInputProcessID.Text, isDeleteFlag, changeKeypartNote);
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        this.txtCurrentMaterialLOTID.Text = txtChangeMaterialLOTID.Text;
                        this.txtChangeMaterialLOTID.Text = string.Empty;
                        this.txtAttachSequenceNo.Text = string.Empty;
                        this.txtMaterialID.Text = string.Empty;
                        this.GetKeypartInfo(Util.NVC(dt.Rows[0]["LOTID"]));
                    }

                    if (isDeleteFlag.Equals(false))
                    {
                        Util.MessageInfo("SFU1265");
                    }
                    else
                    {
                        Util.MessageInfo("SFU3544");
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 순서도 호출 : BR_PRD_REG_INPUTKEYPART_CHANGE
        private DataTable TransactionKeypartChange(string LOTID, string currentMaterialLOTID, string changeMaterialLOTID, string attachSequenceNo, string inputProcessID, bool isDeleteFlag, string changeKeypartNote)
        {
            DataTable dtINDATA = new DataTable("INDATA");
            DataTable dtOUTDATA = new DataTable("OUTDATA");

            dtINDATA.Columns.Add("SRCTYPE", typeof(string));
            dtINDATA.Columns.Add("LANGID", typeof(string));
            dtINDATA.Columns.Add("LOTID", typeof(string));
            dtINDATA.Columns.Add("LOTMID_BEFORE", typeof(string));
            dtINDATA.Columns.Add("LOTMID_AFTER", typeof(string));
            dtINDATA.Columns.Add("PRDT_ATTCH_PSTN_NO", typeof(string));
            dtINDATA.Columns.Add("INPUT_PROCID", typeof(string));
            dtINDATA.Columns.Add("DELETE_FLAG", typeof(string));
            dtINDATA.Columns.Add("SCRAP_FLAG", typeof(string));
            dtINDATA.Columns.Add("NOTE", typeof(string));
            dtINDATA.Columns.Add("USERID", typeof(string));

            DataRow drINDATA = dtINDATA.NewRow();
            drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            drINDATA["LANGID"] = LoginInfo.LANGID;
            drINDATA["LOTID"] = this.LOTID;
            drINDATA["LOTMID_BEFORE"] = !string.IsNullOrEmpty(currentMaterialLOTID) ? currentMaterialLOTID : null;
            drINDATA["LOTMID_AFTER"] = !string.IsNullOrEmpty(changeMaterialLOTID) ? changeMaterialLOTID : null;
            drINDATA["PRDT_ATTCH_PSTN_NO"] = !string.IsNullOrEmpty(attachSequenceNo) ? attachSequenceNo : null;
            drINDATA["INPUT_PROCID"] = inputProcessID;
            drINDATA["DELETE_FLAG"] = isDeleteFlag ? "Y" : "N";
            drINDATA["SCRAP_FLAG"] = "N";
            drINDATA["NOTE"] = changeKeypartNote;
            drINDATA["USERID"] = LoginInfo.USERID;
            dtINDATA.Rows.Add(drINDATA);

            dtOUTDATA = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_INPUTKEYPART_CHANGE", "INDATA", "OUTDATA", dtINDATA, null);

            return dtOUTDATA;
        }

        private void GetKeypartInfo(string LOTID)
        {
            try
            {
                Util.gridClear(this.dgKeyPart);
                this.LOTID = string.Empty;
                this.txtProcessName.Text = string.Empty;
                if (!this.IsLoaded)
                {
                    this.txtCurrentMaterialLOTID.Text = string.Empty;
                }
                this.txtMaterialID.Text = string.Empty;
                this.txtChangeMaterialLOTID.Text = string.Empty;
                this.txtAttachSequenceNo.Text = string.Empty;

                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("LOTID", typeof(string));
                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["LOTID"] = LOTID;
                dtRQSTDT.Rows.Add(drRQSTDT);
                dtRSLTDT = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_WIP_INPUT_MTRL_HIST_MBOM", dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    this.dgKeyPart.ItemsSource = DataTableConverter.Convert(dtRSLTDT);
                    this.LOTID = LOTID;
                    if (!string.IsNullOrEmpty(txtCurrentMaterialLOTID.Text))
                    {
                        int findRowIndex = Util.gridFindDataRow(ref dgKeyPart, "INPUT_LOTID", txtCurrentMaterialLOTID.Text, false);
                        if (findRowIndex >= 0)
                        {
                            DataTableConverter.SetValue(dgKeyPart.Rows[findRowIndex].DataItem, "CHK", true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Event Lists...
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        private void txtLOTID_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (e.Key != Key.Enter)
            {
                return;
            }

            if (textBox.Text.Length < 0)
            {
                return;
            }

            this.GetKeypartInfo(textBox.Text);
        }

        private void txtAttachSequenceNo_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void SetKeypartInfoDetail(C1DataGrid c1DataGrid, int rowIndex)
        {
            DataTableConverter.SetValue(c1DataGrid.Rows[rowIndex].DataItem, "CHK", true);
            string materialLOTID = Util.NVC(DataTableConverter.GetValue(c1DataGrid.Rows[rowIndex].DataItem, "INPUT_LOTID"));
            string attachSequenceNo = Util.NVC(DataTableConverter.GetValue(c1DataGrid.Rows[rowIndex].DataItem, "PRDT_ATTCH_PSTN_NO"));
            string materialID = Util.NVC(DataTableConverter.GetValue(c1DataGrid.Rows[rowIndex].DataItem, "MTRLID"));
            string processName = Util.NVC(DataTableConverter.GetValue(c1DataGrid.Rows[rowIndex].DataItem, "PROCNAME"));
            string inputProcessID = Util.NVC(DataTableConverter.GetValue(c1DataGrid.Rows[rowIndex].DataItem, "INPUT_PROCID"));

            if (attachSequenceNo == "")
            {
                DataTable dt = DataTableConverter.Convert(c1DataGrid.ItemsSource);
                DataRow[] drs = dt.Select("MTRLID = '" + materialID + "'");

                if (drs.Length > 0)
                {
                    int iPrevSeq = 0;
                    int iNextSeq = 0;
                    for (int i = 0; i < drs.Length; i++)
                    {
                        iPrevSeq = Util.NVC_Int(drs[i]["PRDT_ATTCH_PSTN_NO"]);
                        if (i + 1 < drs.Length)
                        {
                            iNextSeq = Util.NVC_Int(drs[i + 1]["PRDT_ATTCH_PSTN_NO"]);
                        }
                        else
                        {
                            attachSequenceNo = Util.NVC(DataTableConverter.GetValue(c1DataGrid.Rows[rowIndex].DataItem, "ROW_NUMBER"));
                        }

                        if ((iPrevSeq + 1) != iNextSeq)
                        {
                            attachSequenceNo = Util.NVC(iPrevSeq + 1);
                            break;
                        }
                    }
                }
                else
                {
                    attachSequenceNo = Util.NVC(DataTableConverter.GetValue(c1DataGrid.Rows[rowIndex].DataItem, "ROW_NUMBER"));
                }
            }

            this.txtCurrentMaterialLOTID.Text = materialLOTID;
            this.txtAttachSequenceNo.Text = attachSequenceNo;
            this.txtMaterialID.Text = materialID;
            this.txtProcessName.Text = processName;
            this.txtInputProcessID.Text = inputProcessID;
        }

        private void dgKeyPart_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid c1DataGrid = (C1DataGrid)sender;
                Point point = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell dataGridCell = c1DataGrid.GetCellFromPoint(point);

                if (dataGridCell == null)
                {
                    return;
                }

                this.SetKeypartInfoDetail(c1DataGrid, dataGridCell.Row.Index);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnMaterialLOTDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button button = sender as Button;
                C1DataGrid c1DataGrid = ((DataGridCellPresenter)button.Parent).DataGrid;
                int selectedIndex = ((DataGridCellPresenter)button.Parent).Row.Index;

                string currentMaterialLOT = Util.NVC(c1DataGrid.GetCell(selectedIndex, c1DataGrid.Columns["INPUT_LOTID"].Index).Value);
                string materialID = Util.NVC(c1DataGrid.GetCell(selectedIndex, c1DataGrid.Columns["MTRLID"].Index).Value);
                string attachSequenceNo = Util.NVC(c1DataGrid.GetCell(selectedIndex, c1DataGrid.Columns["PRDT_ATTCH_PSTN_NO"].Index).Value);
                DataTableConverter.SetValue(c1DataGrid.Rows[selectedIndex].DataItem, "CHK", true);

                if (currentMaterialLOT.Length > 0)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3595", attachSequenceNo, currentMaterialLOT), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            this.ChangeKeypartAndDelete(true);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sSEQ = txtAttachSequenceNo.Text;
                string sBeforeID = txtCurrentMaterialLOTID.Text;
                string sAfterID = txtChangeMaterialLOTID.Text;

                if (string.IsNullOrEmpty(txtAttachSequenceNo.Text))
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3596"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);    // 결합할 위치를 선택하세요.
                    this.txtChangeMaterialLOTID.Focus();
                    return;
                }

                TextRange textRange = new TextRange(this.rtxNote.Document.ContentStart, this.rtxNote.Document.ContentEnd);
                string changeNote = textRange.Text.Replace("\r\n", "");
                if (string.IsNullOrEmpty(changeNote))
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3597"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);    // 교체사유를 입력하세요.
                    rtxNote.Focus();
                    return;
                }

                // 수정하시겠습니까?
                // 교체전ID : [$1]
                // 교체후ID : [$2]
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3598", sSEQ, sBeforeID, sAfterID), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ChangeKeypartAndDelete(false);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
            this.Close();
        }

        // E20230720-001450 : 자재 LOT N빵 삭제
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (this.dgKeyPart == null || this.dgKeyPart.GetRowCount() <= 0)
            {
                return;
            }

            var query = DataTableConverter.Convert(this.dgKeyPart.ItemsSource).AsEnumerable().Where(x => x.Field<string>("CHK").ToUpper() == "TRUE");
            if (query.Count() <= 0)
            {
                return;
            }

            TextRange textRange = new TextRange(this.rtxNote.Document.ContentStart, this.rtxNote.Document.ContentEnd);

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3595", string.Empty, string.Empty), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
            {
                // 키파트 삭제만 됨. 1개씩 삭제를 하면서 에러가 발생하면 Break
                if (result != MessageBoxResult.OK)
                {
                    return;
                }

                try
                {
                    int transactionCount = 0;
                    foreach (var item in query)
                    {
                        string currentMaterialLOTID = item.Field<string>("INPUT_LOTID");
                        string materialID = string.Empty;
                        string attachSequenceNo = string.Empty;
                        string inputProcessID = string.Empty;
                        string changeKeypartNote = textRange.Text;

                        if (string.IsNullOrEmpty(currentMaterialLOTID))
                        {
                            continue;
                        }
                        materialID = item.Field<string>("MTRLID");
                        attachSequenceNo = item.Field<int>("PRDT_ATTCH_PSTN_NO").ToString();
                        inputProcessID = item.Field<string>("INPUT_PROCID");
                        DataTable dt = this.TransactionKeypartChange(this.LOTID, currentMaterialLOTID, string.Empty, attachSequenceNo, inputProcessID, true, changeKeypartNote);
                        transactionCount++;
                    }

                    if (transactionCount > 0)
                    {
                        // 삭제가 완료되면.
                        this.txtCurrentMaterialLOTID.Text = string.Empty;
                        this.txtChangeMaterialLOTID.Text = string.Empty;
                        this.txtAttachSequenceNo.Text = string.Empty;
                        this.txtMaterialID.Text = string.Empty;
                        this.GetKeypartInfo(this.LOTID);
                        Util.MessageInfo("SFU3544");
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });
        }

        private void chkKeypart_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox checkBox = sender as CheckBox;
                C1DataGrid c1DataGrid = ((C1.WPF.DataGrid.DataGridCellPresenter)checkBox.Parent).DataGrid;
                int selectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)checkBox.Parent).Cell.Row.Index;

                this.SetKeypartInfoDetail(c1DataGrid, selectedIndex);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        #endregion
    }
}