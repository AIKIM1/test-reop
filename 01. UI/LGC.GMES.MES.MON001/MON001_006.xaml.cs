/*************************************************************************************
 Created Date : 2017.03.07
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.03.07  DEVELOPER : Initial Created.
 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.MON001
{
    public partial class MON001_006 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private BizDataSet _Biz = new BizDataSet();
        public MON001_006()
        {
            InitializeComponent();

            InitCombo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        /// 

        private int diff = 0;

        private void InitCombo()
        {

            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //동
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL);

            //재공상태
            Set_Combo_COMMCODE(cboWipState);

            #region DIFF 구분콤보
            DataTable dt = new DataTable();
            dt.Columns.Add("CBO_CODE", typeof(string));
            dt.Columns.Add("CBO_NAME", typeof(string));

            DataRow dr = dt.NewRow();
            dr["CBO_CODE"] = "QTY0";
            dr["CBO_NAME"] = ObjectDic.Instance.GetObjectName("수량(1일전)");
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            dr["CBO_CODE"] = "QTY1";
            dr["CBO_NAME"] = ObjectDic.Instance.GetObjectName("수량(1~3일전)");
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            dr["CBO_CODE"] = "QTY3";
            dr["CBO_NAME"] = ObjectDic.Instance.GetObjectName("수량(3~7일전)");
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            dr["CBO_CODE"] = "QTY7";
            dr["CBO_NAME"] = ObjectDic.Instance.GetObjectName("수량(7~31일전)");
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            dr["CBO_CODE"] = "QTY30";
            dr["CBO_NAME"] = ObjectDic.Instance.GetObjectName("수량(31~90일전)");
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            dr["CBO_CODE"] = "QTY90";
            dr["CBO_NAME"] = ObjectDic.Instance.GetObjectName("수량(90~180일전)");
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            dr["CBO_CODE"] = "QTY180";
            dr["CBO_NAME"] = ObjectDic.Instance.GetObjectName("수량(180일이후)");
            dt.Rows.Add(dr);

            cboDiff.ItemsSource = dt.Copy().AsDataView();
            cboDiff.SelectedIndex = 0;

            //cboDiff.SelectedValueChanged += cboDiff_SelectedValueChanged;

            #endregion
        }

        private void Set_Combo_COMMCODE(C1ComboBox cbo)
        {
            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";
            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("CMCDTYPE", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["LANGID"] = LoginInfo.LANGID;
            if (cbo == cboWipState)
            {
                drnewrow["CMCDTYPE"] = "LONG_TERM_WIP_PROC_STAT_CODE";
            }
            
            dtRQSTDT.Rows.Add(drnewrow);

            new ClientProxy().ExecuteService("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
            {
                if (Exception != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (result.Rows.Count > 0)
                {
                    if (cbo == cboWipState)
                    {
                        DataRow dRow = result.NewRow();
                        dRow["CBO_NAME"] = "-ALL-";
                        dRow["CBO_CODE"] = "";
                        result.Rows.InsertAt(dRow, 0);

                        cbo.ItemsSource = DataTableConverter.Convert(result);
                        cbo.SelectedIndex = 0;
                    }
                }
                else if (result.Rows.Count == 0)
                {
                    cbo.SelectedItem = null;
                }
            });
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기
        }

        private void dgResult1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgResult1.GetCellFromPoint(pnt);

                //Util.Alert(cell.Column.Name);

                if (cell != null)
                {
                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("LANGID", typeof(string));
                    dtRqst.Columns.Add("AREAID", typeof(string));
                    dtRqst.Columns.Add("PROCID", typeof(string));
                    dtRqst.Columns.Add("UNIT_CODE", typeof(string));
                    dtRqst.Columns.Add("DIFF", typeof(int));
                    dtRqst.Columns.Add("WIPSTAT", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = Util.NVC(DataTableConverter.GetValue(dgResult1.Rows[cell.Row.Index].DataItem, "AREAID"));

                    if (cell.Column.Name != "AREANAME")
                    { 
                        dr["PROCID"] = Util.NVC(DataTableConverter.GetValue(dgResult1.Rows[cell.Row.Index].DataItem, "PROCID"));
                        dr["UNIT_CODE"] = Util.NVC(DataTableConverter.GetValue(dgResult1.Rows[cell.Row.Index].DataItem, "UNIT_CODE"));
                    }

                    dr["DIFF"] = diff;
                    dr["WIPSTAT"] = Util.GetCondition(cboWipState, bAllNull: true);

                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LONG_TERM_WIP_PROC_DETAIL", "INDATA", "OUTDATA", dtRqst);

                    Util.GridSetData(dgResult2, dtRslt, FrameOperation);
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearValue();
            setDiffColumn();
            GetResult();
        }

        //private void cboDiff_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        public void setDiffColumn()
        {
            if (cboDiff.SelectedValue == null)
                return;

            //if (dgResult1.Rows.Count - dgProductLot.FrozenBottomRowsCount == 0)
            //    return;

            if (Util.NVC(cboDiff.SelectedValue).Equals("QTY0"))
            {
                dgResult1.Columns["QTY0"].Visibility   = Visibility.Visible;
                dgResult1.Columns["QTY1"].Visibility   = Visibility.Visible;
                dgResult1.Columns["QTY3"].Visibility   = Visibility.Visible;
                dgResult1.Columns["QTY7"].Visibility   = Visibility.Visible;
                dgResult1.Columns["QTY30"].Visibility  = Visibility.Visible;
                dgResult1.Columns["QTY90"].Visibility  = Visibility.Visible;
                dgResult1.Columns["QTY180"].Visibility = Visibility.Visible;

                diff = 0;
            }
            else if (Util.NVC(cboDiff.SelectedValue).Equals("QTY1"))
            {
                dgResult1.Columns["QTY0"].Visibility   = Visibility.Collapsed;
                dgResult1.Columns["QTY1"].Visibility   = Visibility.Visible;
                dgResult1.Columns["QTY3"].Visibility   = Visibility.Visible;
                dgResult1.Columns["QTY7"].Visibility   = Visibility.Visible;
                dgResult1.Columns["QTY30"].Visibility  = Visibility.Visible;
                dgResult1.Columns["QTY90"].Visibility  = Visibility.Visible;
                dgResult1.Columns["QTY180"].Visibility = Visibility.Visible;

                diff = 1;
            }
            else if (Util.NVC(cboDiff.SelectedValue).Equals("QTY3"))
            {
                dgResult1.Columns["QTY0"].Visibility   = Visibility.Collapsed;
                dgResult1.Columns["QTY1"].Visibility   = Visibility.Collapsed;
                dgResult1.Columns["QTY3"].Visibility   = Visibility.Visible;
                dgResult1.Columns["QTY7"].Visibility   = Visibility.Visible;
                dgResult1.Columns["QTY30"].Visibility  = Visibility.Visible;
                dgResult1.Columns["QTY90"].Visibility  = Visibility.Visible;
                dgResult1.Columns["QTY180"].Visibility = Visibility.Visible;

                diff = 3;
            }
            else if (Util.NVC(cboDiff.SelectedValue).Equals("QTY7"))
            {
                dgResult1.Columns["QTY0"].Visibility   = Visibility.Collapsed;
                dgResult1.Columns["QTY1"].Visibility   = Visibility.Collapsed;
                dgResult1.Columns["QTY3"].Visibility   = Visibility.Collapsed;
                dgResult1.Columns["QTY7"].Visibility   = Visibility.Visible;
                dgResult1.Columns["QTY30"].Visibility  = Visibility.Visible;
                dgResult1.Columns["QTY90"].Visibility  = Visibility.Visible;
                dgResult1.Columns["QTY180"].Visibility = Visibility.Visible;

                diff = 7;
            }
            else if (Util.NVC(cboDiff.SelectedValue).Equals("QTY30"))
            {
                dgResult1.Columns["QTY0"].Visibility   = Visibility.Collapsed;
                dgResult1.Columns["QTY1"].Visibility   = Visibility.Collapsed;
                dgResult1.Columns["QTY3"].Visibility   = Visibility.Collapsed;
                dgResult1.Columns["QTY7"].Visibility   = Visibility.Collapsed;
                dgResult1.Columns["QTY30"].Visibility  = Visibility.Visible;
                dgResult1.Columns["QTY90"].Visibility  = Visibility.Visible;
                dgResult1.Columns["QTY180"].Visibility = Visibility.Visible;

                diff = 31;
            }
            else if (Util.NVC(cboDiff.SelectedValue).Equals("QTY90"))
            {
                dgResult1.Columns["QTY0"].Visibility   = Visibility.Collapsed;
                dgResult1.Columns["QTY1"].Visibility   = Visibility.Collapsed;
                dgResult1.Columns["QTY3"].Visibility   = Visibility.Collapsed;
                dgResult1.Columns["QTY7"].Visibility   = Visibility.Collapsed;
                dgResult1.Columns["QTY30"].Visibility  = Visibility.Collapsed;
                dgResult1.Columns["QTY90"].Visibility  = Visibility.Visible;
                dgResult1.Columns["QTY180"].Visibility = Visibility.Visible;

                diff = 90;
            }
            else if (Util.NVC(cboDiff.SelectedValue).Equals("QTY180"))
            {
                dgResult1.Columns["QTY0"].Visibility   = Visibility.Collapsed;
                dgResult1.Columns["QTY1"].Visibility   = Visibility.Collapsed;
                dgResult1.Columns["QTY3"].Visibility   = Visibility.Collapsed;
                dgResult1.Columns["QTY7"].Visibility   = Visibility.Collapsed;
                dgResult1.Columns["QTY30"].Visibility  = Visibility.Collapsed;
                dgResult1.Columns["QTY90"].Visibility  = Visibility.Collapsed;
                dgResult1.Columns["QTY180"].Visibility = Visibility.Visible;

                diff = 180;
            }

            //GetTrayInfo();
        }

        #endregion

        #region Mehod
        public void GetResult()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("SHOPID", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("SYSTEM_ID", typeof(string));
                dtRqst.Columns.Add("DIFF", typeof(int));
                dtRqst.Columns.Add("WIPSTAT", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboArea, bAllNull: true);
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["USERID"] = LoginInfo.USERID;
                dr["SYSTEM_ID"] = LoginInfo.SYSID;
                dr["DIFF"] = diff;
                dr["WIPSTAT"] = Util.GetCondition(cboWipState, bAllNull: true);

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LONG_TERM_WIP_PROC", "INDATA", "OUTDATA", dtRqst);
                
                Util.GridSetData(dgResult1, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #region [초기화]
        private void ClearValue()
        {
            Util.gridClear(dgResult1);
            Util.gridClear(dgResult2);
        }
        #endregion

        #endregion

    }
}
