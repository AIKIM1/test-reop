/*************************************************************************************
 Created Date : 2020.10.22
      Creator : 
   Decription : Cell Pallet 재구성 - Pallet Cell 삭제 및 수정
--------------------------------------------------------------------------------------
 [Change History]
 2020.10.22  : Initial Created.
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_007_LOTCHANGE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK003_007_LOTCHANGE()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            string[] tmp = null;

            if (tmps != null && tmps.Length >= 1)
            {
                string Cstid = Util.NVC(tmps[0]);
                string Pltid = Util.NVC(tmps[1]);
                this.txtCstid.Text = Util.NVC(tmps[0]);
                this.txtCstid1.Text = Util.NVC(tmps[0]);
                this.txtPltid.Text = Util.NVC(tmps[1]);
                string Cellid = "";
                //string Prodid = Util.NVC(tmps[2]);

                LoadSearch(Cstid, Pltid, Cellid);
            }
            else
            {
            }

            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnOK);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }


        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sBeforeID = txtBeforeLot.Text;
                string sAfterID = txtAffterLot.Text;

                if (!(txtAffterLot.Text.Length > 0))
                {
                }


                TextRange textRange = new TextRange(rtxNote.Document.ContentStart, rtxNote.Document.ContentEnd);
                string sNote = textRange.Text.Replace("\r\n", "");
                if (sNote == "")
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3597"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);

                    rtxNote.Focus();
                    return;
                }

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4340"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ChangeKeypart();
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
        //조회조건 변수
        private void txtCellId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    clear();
                    if (string.IsNullOrWhiteSpace(txtCellId.Text) == false && txtCellId.Text != "")
                    {
                        if (rdoCst.IsChecked == true)
                        {
                            string Cstid = txtCellId.Text;
                            string Pltid = "";
                            string Cellid = "";
                            LoadSearch(Cstid, Pltid, Cellid);
                        }
                        if (rdoPlt.IsChecked == true)
                        {
                            string Cstid = "";
                            string Pltid = txtCellId.Text;
                            string Cellid = "";
                            LoadSearch(Cstid, Pltid, Cellid);
                        }
                        if (rdoCell.IsChecked == true)
                        {
                            string Cstid = "";
                            string Pltid = "";
                            string Cellid = txtCellId.Text;
                            LoadSearch(Cstid, Pltid, Cellid);
                        }
                    }
                    else
                    {
                        Util.MessageInfo("SFU1801");
                        txtCellId.Text = "";
                    }

                }
            }
            catch
            {
            }

        }
        private void dgKeyPart_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgKeyPart.GetCellFromPoint(pnt);
                if (cell != null)
                {
                    DataTableConverter.SetValue(dgKeyPart.Rows[cell.Row.Index].DataItem, "CHK", true);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void dgKeyPart_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        //조회결과 Grid 라디오 버튼 체크 시 오른쪽 Grid 값 이동
        private void dgKeyPartChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rbt = sender as RadioButton;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rbt.Parent).DataGrid;

                dg.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rbt.Parent).Cell.Row.Index;

                string sCstid = Util.NVC(DataTableConverter.GetValue(dgKeyPart.Rows[dg.SelectedIndex].DataItem, "CSTID"));
                string sSelectLotid = Util.NVC(DataTableConverter.GetValue(dgKeyPart.Rows[dg.SelectedIndex].DataItem, "CELLID"));
                string sMTRLID = Util.NVC(DataTableConverter.GetValue(dgKeyPart.Rows[dg.SelectedIndex].DataItem, "CELL_PRODID"));


                txtBeforeLot.Text = sSelectLotid;
                
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        //Cell 결합 삭제 처리
        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                int iRow = ((C1.WPF.DataGrid.DataGridCellPresenter)btn.Parent).Row.Index;

                string sCSTID = Util.NVC(dgKeyPart.GetCell(iRow, dgKeyPart.Columns["CSTID"].Index).Value);
                string sPLTID = Util.NVC(dgKeyPart.GetCell(iRow, dgKeyPart.Columns["PLTID"].Index).Value);
                string sINPUT_LOTID = Util.NVC(dgKeyPart.GetCell(iRow, dgKeyPart.Columns["CELLID"].Index).Value);
                string sINPUT_PRODID = Util.NVC(dgKeyPart.GetCell(iRow, dgKeyPart.Columns["CELL_PRODID"].Index).Value);

                DataTableConverter.SetValue(dgKeyPart.Rows[iRow].DataItem, "CHK", true);

                if (sINPUT_LOTID.Length > 0)
                {
                    //삭제처리 하시겠습니까?
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1259"), null, "Info", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            DeleteKeypart(sCSTID,sPLTID,sINPUT_LOTID,sINPUT_PRODID);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void txtSEQ_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        #endregion

        #region Mehod
        //Clear 처리
        private void clear()
        {
            try
            {
                txtCstid.Text = "";
                txtPltid.Text = "";
                txtMTRLID.Text = "";
                txtBeforeLot.Text = "";
                txtAffterLot.Text = "";


                Util.gridClear(dgKeyPart);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //Cell 결합이력 조회
        private void LoadSearch(string Cstid, string Pltid, string Cellid)
        {
            DataSet dsInput = new DataSet();
            try
            {

                DataTable INDATA = new DataTable();

                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("CSTID", typeof(string));
                INDATA.Columns.Add("PLTID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = null;
                dr["CSTID"] = Cstid.ToString() == "" ? null : Cstid.ToString();
                dr["PLTID"] = Pltid.ToString() == "" ? null : Pltid.ToString();
                dr["LOTID"] = Cellid.ToString() == "" ? null : Cellid.ToString();

                INDATA.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_LOGIS_PLT_INPUTCELL", "RQSTDT", "RSLTDT", INDATA, (dtResult, ex) =>
                {

                    if (dtResult.Rows.Count == 0)
                    {
                        Util.MessageInfo("SFU1801");
                        txtCellId.Text = "";
                        return;
                    }

                    if (dtResult.Rows.Count != 0)
                    {

                        Util.GridSetData(dgKeyPart, dtResult, FrameOperation);
                        txtCstid.Text = Util.NVC(dtResult.Rows[0]["CSTID"]);
                        txtPltid.Text = Util.NVC(dtResult.Rows[0]["PLTID"]);
                        txtMTRLID.Text = Util.NVC(dtResult.Rows[0]["PLT_PRODID"]);
                    }
                    return;
                });
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        //CELL 결합 이력 삭제
        private void DeleteKeypart(string Cst, string Plt, string Cell, string Prod)
        {
            try
            {
                TextRange textRange = new TextRange(rtxNote.Document.ContentStart, rtxNote.Document.ContentEnd);
                string sNote = textRange.Text;

                if (Util.gridFindDataRow(ref dgKeyPart, "CHK", "True", false).Equals(-1))
                {
                    Util.MessageInfo("SFU1636");
                    return;
                }

                int iRow = Util.gridFindDataRow(ref dgKeyPart, "CHK", "True", false); //수정시 Util.gridFindDataRow(ref dgKeyPart, "CELLID", txtBeforeLot.Text, false) 
                //string sChg_PROCID = Util.NVC(DataTableConverter.GetValue(dgKeyPart.Rows[iRow].DataItem, "INPUT_PROCID"));

                //int iChkOverLap = ((DataView)dgKeyPart.ItemsSource).Table.Select("CELL_PRODID <>'" + "' AND CELL_PRODID = '" + Util.NVC(txtMTRLID.Text.ToString()) + "'").Length;
                //if (iChkOverLap > 1)
                //{
                //    Util.MessageValidation("SFU8158", txtMTRLID.Text.ToString());
                //    return;
                //}
                DataSet indataSet = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";

                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("CSTID", typeof(string));
                INDATA.Columns.Add("PLTID", typeof(string));
                INDATA.Columns.Add("NOTE", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                DataTable INLOT = new DataTable();
                INLOT.TableName = "INLOT";
                INLOT.Columns.Add("LOTID", typeof(string));
                INLOT.Columns.Add("PRODID", typeof(string));



                DataRow drINDATA = INDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["CSTID"] = Cst.ToString();
                drINDATA["PLTID"] = Plt.ToString();
                drINDATA["NOTE"] = sNote;
                drINDATA["USERID"] = LoginInfo.USERID;

                DataRow drINLOT = INLOT.NewRow();
                drINLOT["LOTID"] = Cell.ToString();
                drINLOT["PRODID"] = Prod.ToString();

                INDATA.Rows.Add(drINDATA);
                INLOT.Rows.Add(drINLOT);

                indataSet.Tables.Add(INDATA);
                indataSet.Tables.Add(INLOT);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_LOGIS_PLT_CELL_DEL", "INDATA", "OUTDATA", INDATA, null);
                DataSet dtResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_LOGIS_PLT_CELL_DEL", "INDATA,INLOT", "OUTDATA", indataSet);
                if (dtResult != null)
                {
                    if (dtResult.Tables["OUTDATA"].Rows.Count > 0)
                    {
                        txtBeforeLot.Text = "";
                        txtAffterLot.Text = "";
                        Util.gridClear(dgKeyPart);
                        LoadSearch(Cst, "","");
                        //getKeypart(Util.NVC(dtResult.Rows[0]["LOTID"]));

                        //textRange.Text = "";
                    }
                    Util.MessageInfo("SFU3544");
                    return;
                }

            }
            catch (Exception ex)
            {
                //2019.12.11
                //Util.AlertByBiz("BR_PRD_REG_INPUTKEYPART_CHANGE", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }
        //CELL 결합 이력 수정
        private void ChangeKeypart()
        {
            try
            {
                TextRange textRange = new TextRange(rtxNote.Document.ContentStart, rtxNote.Document.ContentEnd);
                string sNote = textRange.Text;

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";

                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("CSTID", typeof(string));
                INDATA.Columns.Add("PLTID", typeof(string));
                INDATA.Columns.Add("BEF_CELLID", typeof(string));
                INDATA.Columns.Add("BEF_PRODID", typeof(string));
                INDATA.Columns.Add("AFT_CELLID", typeof(string));
                INDATA.Columns.Add("AFT_PRODID", typeof(string));
                INDATA.Columns.Add("NOTE", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                DataRow drINDATA = INDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["CSTID"] = txtCstid.Text;
                drINDATA["PLTID"] = txtPltid.Text;
                drINDATA["BEF_CELLID"] = txtBeforeLot.Text;
                drINDATA["BEF_PRODID"] = txtMTRLID.Text;
                drINDATA["AFT_CELLID"] = txtAffterLot.Text;
                drINDATA["AFT_PRODID"] = null;
                drINDATA["NOTE"] = sNote;
                drINDATA["USERID"] = LoginInfo.USERID;

                INDATA.Rows.Add(drINDATA);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_LOGIS_PLT_CELLCHANGE", "INDATA", "OUTDATA", INDATA, null);
                if (dtResult != null)
                {
                    if (dtResult.Rows.Count > 0)
                    {
                        txtBeforeLot.Text = "";
                        txtAffterLot.Text = "";
                        txtMTRLID.Text = "";
                        Util.gridClear(dgKeyPart);
                        LoadSearch(txtCstid.Text, "", "");

                    }
                    Util.MessageInfo("SFU1265");
                    return;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextRange textRange = new TextRange(rtxNote.Document.ContentStart, rtxNote.Document.ContentEnd);
                string sNote = textRange.Text.Replace("\r\n", "");
                if (sNote == "")
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3597"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                    return;
                }

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";

                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("CSTID", typeof(string));
                INDATA.Columns.Add("PLTID", typeof(string));
                INDATA.Columns.Add("NOTE", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                DataRow drINDATA = INDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["CSTID"] = txtCstid.Text;
                drINDATA["PLTID"] = txtPltid.Text;
                drINDATA["NOTE"] = sNote;
                drINDATA["LOTID"] = txtAffterLot1.Text;
                drINDATA["USERID"] = LoginInfo.USERID;

                INDATA.Rows.Add(drINDATA);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_LOGIS_PLT_CELL_ADD", "INDATA", "OUTDATA", INDATA, null);
                if (dtResult != null)
                {
                    if (dtResult.Rows.Count > 0)
                    {
                        txtAffterLot1.Text = "";
                        Util.gridClear(dgKeyPart);
                        LoadSearch(txtCstid.Text, "", "");
                    }
                    Util.MessageInfo("SFU1265");
                    return;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
