/*************************************************************************************
 Created Date : 2018.02.06
      Creator : 이진선
   Decription : VD 수동검사
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2019.05.01  정문교  : 폴란드3동 대응 Carrier ID(CSTID) 조회조건, 조회칼럼 추가



 
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;


namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_038 : UserControl, IWorkArea
    {
        CommonCombo combo = new CommonCombo();
        Util _Util = new Util();

   


        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #region Declaration & Constructor 
        public ASSY001_038()
        {
            InitializeComponent();
            Loaded += UserControl_Loaded;

        }


        #endregion
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= UserControl_Loaded;
            ApplyPermissions();

            initcombo();

        }
   

        #region Initialize

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

        #endregion

        #region Event

        #region[LOT예약]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return; ;

            SearchData();
        }

        private void cboVDEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //공정
            String[] sFilter = { cboVDEquipmentSegment.SelectedValue.ToString() };
            combo.SetCombo(cboVDProcess, CommonCombo.ComboStatus.NONE, sFilter: sFilter);

        }
        private void cboVDProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //설비
            String[] sFilter = { cboVDProcess.SelectedValue.ToString(), cboVDEquipmentSegment.SelectedValue.ToString() };
            combo.SetCombo(cboVDEquipment, CommonCombo.ComboStatus.ALL, sFilter: sFilter);
        }

    
        private void txtLOTID_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchData();
                txtLOTID.Text = "";
            }

        }

        private void txtCstID_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchData();
                txtCstID.Text = "";
            }

        }


        private void initGridTable(C1.WPF.DataGrid.C1DataGrid datagrid)
        {
            DataTable dt = new DataTable();
            foreach (C1.WPF.DataGrid.DataGridColumn col in datagrid.Columns)
            {
                dt.Columns.Add(Convert.ToString(col.Name));
            }
            datagrid.BeginEdit();
            datagrid.ItemsSource = DataTableConverter.Convert(dt);
            datagrid.EndEdit();
        }

        #endregion
        
        #endregion

        #region Mehod
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnConfrim);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void initcombo()
        {
            string[] sFilter = { "A1000," + Process.VD_LMN + "," + Process.VD_ELEC, LoginInfo.CFG_AREA_ID };
            combo.SetCombo(cboVDEquipmentSegment, CommonCombo.ComboStatus.NONE, sFilter: sFilter);
        }
        private void SearchData()
        {
            try
            {
                string bizRuleName = "DA_PRD_SEL_WIP_AUTO_VD_QA_WAIT_LIST";

                dgLotInfo.ItemsSource = null;
                DataTable dt = new DataTable();

                if (String.IsNullOrWhiteSpace(txtCstID.Text))
                {
                    dt.Columns.Add("LANGID", typeof(string));
                    dt.Columns.Add("EQSGID", typeof(string));
                    dt.Columns.Add("EQPTID", typeof(string));
                    dt.Columns.Add("LOTID", typeof(string));
                    dt.Columns.Add("PROCID", typeof(string));

                    DataRow dr = dt.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["EQSGID"] = Convert.ToString(cboVDEquipmentSegment.SelectedValue);
                    dr["EQPTID"] = Convert.ToString(cboVDEquipment.SelectedValue).Equals("") ? null : Convert.ToString(cboVDEquipment.SelectedValue);
                    dr["LOTID"] = txtLOTID.Text.Equals("") ? null : txtLOTID.Text;
                    dr["PROCID"] = Process.VD_LMN;
                    dt.Rows.Add(dr);
                }
                else
                {
                    bizRuleName = "DA_PRD_SEL_WIP_AUTO_VD_QA_WAIT_LIST_L";

                    dt.Columns.Add("LANGID", typeof(string));
                    dt.Columns.Add("CSTID", typeof(string));

                    DataRow dr = dt.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["CSTID"] = txtCstID.Text;
                    dt.Rows.Add(dr);
                }

                DataTable result = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "RSLTDT", dt);

                Util.GridSetData(dgLotInfo, result, FrameOperation, true);

             
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }


        #endregion
    
     
 

        private void btnConfrim_Click(object sender, RoutedEventArgs e)
        {
            if (_Util.GetDataGridCheckFirstRowIndex(dgLotInfo, "CHK") == -1)
            {
                Util.MessageValidation("SFU3529");//선택된 LOT이 없습니다.
                return;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[_Util.GetDataGridCheckFirstRowIndex(dgLotInfo, "CHK")].DataItem, "REQ_RSLT_CODE")).Equals("REQ"))
            {
                Util.MessageValidation("SFU4519"); // 이미 요청된 상태입니다.
                return;
            }

            ASSY001_038_REQUEST wndPopup = new ASSY001_038_REQUEST();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = "NEW";

                string str = string.Empty;

                for (int i = 0; i < dgLotInfo.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CHK")).Equals("True"))
                    {
                        str += Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOTID")) + ",";
                    }
                }

                str = str.Remove(str.Length-1, 1);

                Parameters[1] = str;

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndPopup_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void wndPopup_Closed(object sender, EventArgs e)
        {
            ASSY001_038_REQUEST wndPopup = sender as ASSY001_038_REQUEST;
            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                SearchData();
            }
        }

        private void dgLotInfo_MergingCells(object sender, C1.WPF.DataGrid.DataGridMergingCellsEventArgs e)
        {
            try
            {
                if (dgLotInfo.GetRowCount() == 0) return;

                List<System.Data.DataRow> list = DataTableConverter.Convert(dgLotInfo.ItemsSource).Select().ToList();
                List<int> arr = list.GroupBy(c => c["EQPT_BTCH_WRK_NO"]).Select(group => group.Count()).ToList();


                int p = 0;
                for (int j = 0; j < arr.Count; j++)
                {
                    for (int i = 0; i < dgLotInfo.Columns.Count; i++)
                    {
                        if (dgLotInfo.Columns[i].Name.Equals("m"))
                        {
                            e.Merge(new DataGridCellsRange(dgLotInfo.GetCell(p, i), dgLotInfo.GetCell((p + arr[j] - 1), i)));
                          
                        }
                    }
                    p += arr[j];
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //private void btnConfrimCancel_Click(object sender, RoutedEventArgs e)
        //{

        //    try
        //    {
        //        if (_Util.GetDataGridCheckFirstRowIndex(dgLotInfo, "CHK") == -1)
        //        {
        //            Util.MessageValidation("SFU3529");//선택된 LOT이 없습니다.
        //            return;
        //        }

        //        if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[_Util.GetDataGridCheckFirstRowIndex(dgLotInfo, "CHK")].DataItem, "REQ_RSLT_CODE")).Equals(""))
        //        {
        //            Util.MessageValidation("SFU4521");//요청필요
        //            return;
        //        }

        //        if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[_Util.GetDataGridCheckFirstRowIndex(dgLotInfo, "CHK")].DataItem, "REQ_RSLT_CODE")).Equals("DEL"))
        //        {
        //            Util.MessageValidation("SFU4520"); //이미 요청 취소 되었습니다.
        //            return;
        //        }

        //        //취소 하시겠습니까?
        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
        //                "SFU1243"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
        //                {
        //                    if (result == MessageBoxResult.OK)
        //                    {
        //                        ReqCancel();
        //                    }
        //                });

        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }


        //}

        //private void ReqCancel()
        //{
        //    string sTo = "";
        //    string sCC = "";

        //    DataSet ds = new DataSet();

        //    DataTable inData = new DataTable();
        //    inData.TableName = "INDATA";
        //    inData.Columns.Add("LANGID", typeof(string));
        //    inData.Columns.Add("USERID", typeof(string));

        //    DataRow dr = inData.NewRow();
        //    dr["LANGID"] = LoginInfo.LANGID;
        //    dr["USERID"] = LoginInfo.USERID;
        //    inData.Rows.Add(dr);
        //    ds.Tables.Add(inData);

        //    DataTable dtRqstStatus = new DataTable();
        //    dtRqstStatus.TableName = "IN_REQ";
        //    dtRqstStatus.Columns.Add("REQ_NO", typeof(string));

        //    for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
        //    {
        //        if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CHK")).Equals("True"))
        //        {
        //            DataRow drStatus = dtRqstStatus.NewRow();
        //            drStatus["REQ_NO"] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "REQ_NO"));
        //            dtRqstStatus.Rows.Add(drStatus);
        //        }
        //    }
        //    ds.Tables.Add(dtRqstStatus);

        //    DataSet result = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_CANCEL_APPR_REG", "INDATA,IN_REQ", "APPR_USER", ds);

        //    if (result.Tables["APPR_USER"].Rows.Count != 0)
        //    {
        //        DataTable app_user = result.Tables["APPR_USER"];

        //        for (int i = 0; i < app_user.Rows.Count; i++)
        //        {
        //            sTo = Util.NVC(app_user.Rows[i]["APPR_USERID"]);
        //            sCC = Util.NVC(app_user.Rows[i]["APPR_REF"]);

        //            MailSend mail = new CMM001.Class.MailSend();
        //            string sMsg = ObjectDic.Instance.GetObjectName("요청취소"); //승인요청취소
        //            string sTitle = Util.NVC(app_user.Rows[i]["REQ_NO"]) + ObjectDic.Instance.GetObjectName("승인요청취소");

        //            mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, sTo, sCC, sMsg, mail.makeBodyApp(sTitle, ObjectDic.Instance.GetObjectName("승인요청취소")));
        //        }

        //    }

        //    Util.MessageInfo("SFU1937");  //취소되었습니다.
        //    SearchData();

        //}

        private void dgLotInfoGroup_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }

                //row 색 바꾸기
                dgLotInfo.SelectedIndex = idx;
            }
        }
    }
}
