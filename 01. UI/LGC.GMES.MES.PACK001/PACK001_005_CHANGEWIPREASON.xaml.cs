/*************************************************************************************
 Created Date : 2016.08.01
      Creator : Jeong Hyeon Sik
   Decription : Pack Lot이력- 조치이력 변경 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.01  Jeong Hyeon Sik : Initial Created.
  2018.08.07  손우석 SM CMI Pack 메시지 다국어 처리 요청
  2019.12.11  손우석 SM CMI Pack 메시지 다국어 처리 요청
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
    public partial class PACK001_005_CHANGEWIPREASON : C1Window, IWorkArea
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

        public PACK001_005_CHANGEWIPREASON()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Button> listAuth = new List<Button>();
                listAuth.Add(btnOK);
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

                object[] tmps = C1WindowExtension.GetParameters(this);
                if (tmps != null)
                {
                    DataTable dtText = tmps[0] as DataTable;

                    if (dtText.Rows.Count > 0)
                    {
                        txtLot.Text = Util.NVC(dtText.Rows[0]["LOTID"]);
                        //txtSEQ.Text = Util.NVC(dtText.Rows[0]["SEQ"]);
                        //txtRESNCODE.Text = Util.NVC(dtText.Rows[0]["RESNCODE"]);

                        if (txtLot.Text.Length > 0)
                        {
                            //getKeypart(txtLot.Text);
                            getResncode(txtLot.Text);
                        }
                    }
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
                string sSEQ = txtSEQ.Text;

                TextRange textRange = new TextRange(rtxNote.Document.ContentStart, rtxNote.Document.ContentEnd);
                string sNote = textRange.Text.Replace("\r\n", "");
                if (sNote == "")
                {
                    //2018.08.07
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("교체사유를 입력하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3597"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);

                    rtxNote.Focus();
                    return;
                }

                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("수정하시겠습니까?\nSEQ:{0}\n교체전ID:{1}\n교체될ID:{2}", sSEQ, sBeforeID, sAfterID), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3598"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        changeWipReasoncodeCause();
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
            //if(this.DialogResult != MessageBoxResult.OK)
            //{
            //    this.DialogResult = MessageBoxResult.OK;
            //}
            this.DialogResult = MessageBoxResult.OK;
            this.Close();
        }

        private void txtLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtLot.Text.Length > 0)
                    {
                        getResncode(txtLot.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
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

        private void dgKeyPartChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rbt = sender as RadioButton;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rbt.Parent).DataGrid;

                dg.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rbt.Parent).Cell.Row.Index;

                string sSelectLotid = Util.NVC(DataTableConverter.GetValue(dgKeyPart.Rows[dg.SelectedIndex].DataItem, "LOTID"));
                string sSEQ = Util.NVC(DataTableConverter.GetValue(dgKeyPart.Rows[dg.SelectedIndex].DataItem, "WIPSEQ"));
                string sNote = Util.NVC(DataTableConverter.GetValue(dgKeyPart.Rows[dg.SelectedIndex].DataItem, "RESNCODE"));

                //if (sSEQ == "")
                //{
                //    DataTable dtTemp = DataTableConverter.Convert(dgKeyPart.ItemsSource);
                // DataRow[] drTemp = dtTemp.Select("MTRLID = '" + sMTRLID + "'");

                //if (drTemp.Length > 0)
                //{
                //int iPrevSeq = 0;
                //int iNextSeq = 0;
                //for (int i = 0; i < drTemp.Length; i++)
                //{
                //    iPrevSeq = Util.NVC_Int(drTemp[i]["WIPSEQ"]);
                //    if (i + 1 < drTemp.Length)
                //    {
                //        iNextSeq = Util.NVC_Int(drTemp[i + 1]["WIPSEQ"]);
                //    }
                //    else
                //    {
                //        sSEQ = Util.NVC(DataTableConverter.GetValue(dgKeyPart.Rows[dg.SelectedIndex].DataItem, "ROW_NUMBER"));
                //    }

                //    if ((iPrevSeq + 1) != iNextSeq)
                //    {
                //        sSEQ = Util.NVC(iPrevSeq + 1);
                //        break;
                //    }
                //}
                // }
                //else
                // {
                // sSEQ = Util.NVC(DataTableConverter.GetValue(dgKeyPart.Rows[dg.SelectedIndex].DataItem, "WIPSEQ"));
                // }
                //}

                //txtBeforeLot.Text = sSelectLotid;
                
                txtSEQ.Text = sSEQ;
                txtRESNCODE.Text = sNote;
                //rtxNote.AppendText = sNote;
                //txtMTRLID.Text = sMTRLID;
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

        private void chkLotChange_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                displayChange_By_CheckBox();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void chkLotChange_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                displayChange_By_CheckBox();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void displayChange_By_CheckBox()
        {
            try
            {
                
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void getResncode(string sLotid)
        {
            // 조치내역 수정에 필요한 컬럼 값들 가져오기.
            //LOTID를 기준으로 WIPREASONCOLLECRT에서 값가져오고 AND조건으로 ACTID = 'REPAIR_LOT' 가져오기
            try
            {
                //DA_PRD_SEL_TB_SFC_WIP_INPUT_MTRL_HIST_MBOM
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("ACTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLotid;
                dr["ACTID"] = "REPAIR_LOT";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPREASONCOLLECT_ACTID_LIST", "RQSTDT", "RSLTDT", RQSTDT);

                dgKeyPart.ItemsSource = DataTableConverter.Convert(dtResult);

            }
            catch (Exception ex)
            {
                //2019.12.11
                //Util.AlertByBiz("DA_PRD_SEL_WIPREASONCOLLECT_ACTID_LIST", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        // OK눌렀을 때 사유 변경 해주는 로직 작성.
        private void changeWipReasoncodeCause()
        {
            try
            {
                TextRange textRange = new TextRange(rtxNote.Document.ContentStart, rtxNote.Document.ContentEnd);
                string sNote = textRange.Text;


                //BR_PRD_REG_RESNCODE_CAUSE_CHANGE
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";

                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("ACTID", typeof(string));
                INDATA.Columns.Add("WIPSEQ", typeof(string));
                INDATA.Columns.Add("RESNNOTE", typeof(string));

                DataRow drINDATA = INDATA.NewRow();
                drINDATA["USERID"] = LoginInfo.USERID;
                drINDATA["LOTID"] = txtLot.Text;
                drINDATA["ACTID"] = "REPAIR_LOT";
                drINDATA["WIPSEQ"] = txtSEQ.Text;
                drINDATA["RESNNOTE"] = sNote;
                INDATA.Rows.Add(drINDATA);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_RESNCODE_CAUSE_CHANGE", "INDATA", "OUTDATA", INDATA, null);

                if (dtResult != null)
                {
                    txtSEQ.Text = "";
                    textRange.Text = "";
                    txtRESNCODE.Text = "";
                    getResncode(Util.NVC(dtResult.Rows[0]["LOTID"]));
                }

            }
            catch (Exception ex)
            {
                //2019.12.11
                //Util.AlertByBiz("BR_PRD_REG_RESNCODE_CAUSE_CHANGE", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        #endregion


    }
}
