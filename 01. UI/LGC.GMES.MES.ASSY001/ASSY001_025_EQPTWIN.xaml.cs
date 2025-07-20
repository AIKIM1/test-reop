/*************************************************************************************
 Created Date : 2017.01.14
      Creator : 이진선
   Decription : VD QA대상LOT조회 설비 내용
--------------------------------------------------------------------------------------
 [Change History]
  2017.01.14  이진선 : Initial Created.
  2019.03.05  오화백 : RF_ID일경우 CST 정보 보여주기
**************************************************************************************/

using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Documents;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001.Class;
using System.Net;
using System.Collections;
using System.Globalization;
using System.Linq;


using System.Windows.Media;

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_025_EQPTWIN : UserControl, IWorkArea
    {
        #region Declaration & Constructor        

        Util _Util = new Util();
        string Eqptid = string.Empty;
        string EqptName = string.Empty;
        string Elec = string.Empty;
        string ElecCheck = string.Empty;
        string LineSkipFalg = string.Empty;
        //2019.03.05 RF_ID 여부
        string LDR_LOT_IDENT_BAS_CODE = string.Empty;
        string UNLDR_LOT_IDENT_BAS_CODE = string.Empty;

        #endregion

        #region Initialize        
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }

        public string EQPTNAME
        {
            get { return EqptName; }
            set { EqptName = value; }
        }
        public string EQPTID
        {
            get { return Eqptid; }
            set { Eqptid = value; }
        }
        public string ELECCHECK
        {
            get { return ElecCheck; }
            set { ElecCheck = value; }
        }
        public string EQPT_ELEC
        {
            get { return Elec; }
            set { Elec = value; }
        }
        public string LINE_SKIP
        {
            get { return LineSkipFalg; }
            set { LineSkipFalg = value; }
        }
        //2019.03.05 RF_ID 여부
        public string _LDR_LOT_IDENT_BAS_CODE
        {
            get { return LDR_LOT_IDENT_BAS_CODE; }
            set { LDR_LOT_IDENT_BAS_CODE = value; }
        }
        public string _UNLDR_LOT_IDENT_BAS_CODE
        {
            get { return UNLDR_LOT_IDENT_BAS_CODE; }
            set { UNLDR_LOT_IDENT_BAS_CODE = value; }
        }

        public ASSY001_025_EQPTWIN()
        {
            InitializeComponent();
            this.Loaded += UserControl_Loaded;
        }

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

        #region[Event]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;

            //2019.03.05 RF_ID 여부 : RF_ID일 경우 CST 컬럼 보여줌
            if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
            {
                dgFinishLot.Columns["CSTID"].Visibility = Visibility.Visible;
            }
            else
            {
                dgFinishLot.Columns["CSTID"].Visibility = Visibility.Collapsed;
            }

            //  SetMoveLineCombo();
            SetCheckBox();
        }

        private void SetCheckBox()
        {
            try
            {
                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("USERID", typeof(String));
                dt.Columns.Add("AUTHID", typeof(String));

                DataRow dr = dt.NewRow();
                dr["USERID"] = LoginInfo.USERID;
                dr["AUTHID"] = "DOE_RSV";
                dt.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_USER_AUTH_PC", "INDATA", "OUTDATA", dt);
                if (dtResult.Rows.Count > 0)
                    chkIsDoe.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void dgFinishLot_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        int idx = e.Row.Index;
                        if (idx == -1) return;

                        string prodid = Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[idx].DataItem, "PRODID"));

                        if (Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[idx].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[idx].DataItem, "CHK")).Equals("True"))
                        {
                            SetMoveLineCombo(prodid, idx, "ADD");
                        }
                        else
                        {
                            DataTable tmp = DataTableConverter.Convert(dgFinishLot.ItemsSource).Select("CHK = 1").Count() == 0 ? null : DataTableConverter.Convert(dgFinishLot.ItemsSource).Select("CHK = 1").CopyToDataTable();
                            if (tmp != null && tmp.Select("PRODID = '" + prodid + "'").Count() > 0)
                            {
                                return;
                            }

                            SetMoveLineCombo(prodid, idx, null);
                        }

                    }
                }
            }));
           
        }

        private void dgFinishLot_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre.Content = chkAll;
                        e.Column.HeaderPresenter.Content = pre;
                        chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                    }
                }
            }));
        }
        private void btnMoveLami_Click(object sender, RoutedEventArgs e)
        {
            if (_Util.GetDataGridCheckFirstRowIndex(dgFinishLot, "CHK") == -1)
            {
                Util.Alert("SFU1261"); //선택된 LOT이 없습니다.
                return;
            }

            if (cboMoveLine.Text == "" || cboMoveLine.Text.Equals("-SELECT-"))
            {
                Util.MessageValidation("SFU4050");
                return;
            }


            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3039", Convert.ToString(cboMoveLine.Text)), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        DataSet inData = new DataSet();

                        DataTable inDataTable = inData.Tables.Add("INDATA");
                        inDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inDataTable.Columns.Add("IFMODE", typeof(string));
                        inDataTable.Columns.Add("USERID", typeof(string));
                        inDataTable.Columns.Add("PASSYN", typeof(string));
                        inDataTable.Columns.Add("TO_EQSGID", typeof(string));
                        inDataTable.Columns.Add("TO_PROCID", typeof(string));
                        inDataTable.Columns.Add("DOE_FLAG", typeof(string));

                        DataRow row = null;
                        row = inDataTable.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["IFMODE"] = IFMODE.IFMODE_OFF;
                        row["USERID"] = LoginInfo.USERID;
                        row["PASSYN"] = "Y";
                        row["TO_EQSGID"] = Convert.ToString(cboMoveLine.SelectedValue);
                        row["TO_PROCID"] = Process.LAMINATION;
                        row["DOE_FLAG"] = chkIsDoe.IsChecked.Value ? "Y" : "N";

                        inData.Tables["INDATA"].Rows.Add(row);


                        DataTable INLOT = inData.Tables.Add("IN_LOT");
                        INLOT.Columns.Add("LOTID", typeof(string));

                        for (int i = 0; i < dgFinishLot.Rows.Count; i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "CHK")).Equals("True"))
                            {
                                row = INLOT.NewRow();
                                row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "LOTID"));
                                inData.Tables["IN_LOT"].Rows.Add(row);
                            }
                        }

                        try
                        {
                            loadingIndicator.Visibility = Visibility.Visible;

                            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MOVE_LOT_LINE", "INDATA,IN_LOT", null, (bizResult, bizException) =>
                            {
                                try
                                {
                                    if (bizException != null)
                                    {
                                        Util.MessageException(bizException);
                                        return;
                                    }

                                    Util.MessageInfo("SFU1784"); //이송완료
                                    foreach (ASSY001_025 win in Util.FindVisualChildren<ASSY001_025>(Application.Current.MainWindow))
                                    {
                                        win.SearchData();
                                        return;
                                    }

                                }
                                catch (Exception ex)
                                {
                                    Util.MessageException(ex);
                                }
                                finally
                                {
                                    loadingIndicator.Visibility = Visibility.Collapsed;
                                }
                            }, inData);

                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }



                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            });
        }
        #endregion

        #region[Method]
      
        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgFinishLot.GetRowCount(); i++)
            {
                DataTableConverter.SetValue(dgFinishLot.Rows[i].DataItem, "CHK", true);
                SetMoveLineCombo(Convert.ToString(DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "PRODID")), i, "ADD");
            }

                
            

        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgFinishLot.GetRowCount(); i++)
            {
                DataTableConverter.SetValue(dgFinishLot.Rows[i].DataItem, "CHK", false);
                SetMoveLineCombo(Convert.ToString(DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "PRODID")), i, null);
            }
        }

        private void SetMoveLineCombo(string prodid, int idx, string sType = null)
        {
            try
            {
                DataTable data = new DataTable();
                data.Columns.Add("LANGID", typeof(string));
                data.Columns.Add("PROCID", typeof(string));
                data.Columns.Add("PRODID", typeof(string));
                data.Columns.Add("SHOPID", typeof(string));

                DataRow row = data.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["PROCID"] = Process.LAMINATION;
                row["PRODID"] = prodid;
                row["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                data.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LINE_LAMI", "RQSTDT", "RSLTDT", data);

                if (result.Rows.Count == 0 && (Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[idx].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[idx].DataItem, "CHK")).Equals("True")))
                {
                    DataTableConverter.SetValue(dgFinishLot.Rows[idx].DataItem, "CHK", 0);
                    Util.MessageValidation("SFU4051", prodid);
                   
                    return;
                }

                DataTable moveLine = null;

                if (cboMoveLine.ItemsSource == null)
                {
                    moveLine = new DataTable();
                    moveLine.Columns.Add("CBO_CODE", typeof(string));
                    moveLine.Columns.Add("CBO_NAME", typeof(string));

                }
                else
                {
                    moveLine = DataTableConverter.Convert(cboMoveLine.ItemsSource);
                }

                if (string.IsNullOrEmpty(sType) == false && sType.Equals("ADD")) //추가
                {
                   
                    foreach (DataRow dr in result.Rows)
                    {
                        if (moveLine.Select("CBO_CODE = '" + dr["CBO_CODE"] + "'").Count() == 0)
                        {
                            DataRow dr2 = moveLine.NewRow();
                            dr2["CBO_CODE"] = dr["CBO_CODE"];
                            dr2["CBO_NAME"] = dr["CBO_NAME"];
                            moveLine.Rows.Add(dr2);

                        }
                    }

                  
                   
                }
                else // 빼기
                {
                    if (moveLine.Rows.Count > 0 )
                    {
                        
                        foreach (DataRow dr in result.Rows)
                        {
                            for (int i = moveLine.Rows.Count - 1; i > -1; i--)
                            {
                                if (Convert.ToString(moveLine.Rows[i]["CBO_CODE"]).Equals(Convert.ToString(dr["CBO_CODE"])))
                                {
                                    moveLine.Rows.RemoveAt(i);
                                }
                            }
                          

                        }

                    }
                }

                if (moveLine.Rows.Count > 1 && string.IsNullOrEmpty(sType) == false && sType.Equals("ADD"))
                {
                    if (!Convert.ToString(moveLine.Rows[0]["CBO_NAME"]).Equals("-SELECT-"))
                    {
                        DataRow select_row = moveLine.NewRow();

                        select_row["CBO_CODE"] = "";
                        select_row["CBO_NAME"] = "-SELECT-";
                        moveLine.Rows.InsertAt(select_row, 0);
                    }
                }
                else if ((moveLine.Rows.Count == 2 || moveLine.Rows.Count == 1) && string.IsNullOrEmpty(sType) == true)
                {
                    if (Convert.ToString(moveLine.Rows[0]["CBO_NAME"]).Equals("-SELECT-"))
                    {
                        moveLine.Rows.RemoveAt(0);
                    }
                }

                if (moveLine.Rows.Count == 0)
                {
                    cboMoveLine.ItemsSource = null;
                    cboMoveLine.Text = "";
                }

                cboMoveLine.SelectedValuePath = "CBO_CODE";
                cboMoveLine.DisplayMemberPath = "CBO_NAME";

                cboMoveLine.ItemsSource = DataTableConverter.Convert(moveLine);
                cboMoveLine.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void ClearData()
        {
            dgFinishLot.ItemsSource = null;
            cboMoveLine.ItemsSource = null;
            cboMoveLine.Text = "";

            EQPTNAME = string.Empty;
            EQPTID = string.Empty;
            ELECCHECK = string.Empty;
            EQPT_ELEC = string.Empty;
            LINE_SKIP = string.Empty;

            tbEqptName.Text = string.Empty;

       }

        public void GetEqpt()
        {
            tbEqptName.Text = EQPTNAME;
          //  SetMoveLineCombo();
        }



        #endregion

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            chkIsDoe.Visibility = Visibility.Collapsed;
        }
    }

}