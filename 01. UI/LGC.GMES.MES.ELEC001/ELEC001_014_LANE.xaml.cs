/*******************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 슬리터 LANE
------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
*******************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.ELEC001
{
    public partial class ELEC001_014_LANE : C1Window, IWorkArea
    {
        #region Initialize
        string sLotId = string.Empty;
        string sStatus = string.Empty;

        string ChildLot = string.Empty;
        Util _Util = new Util();

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }

        public bool? IsSingleCoater { get; set; }

        public ELEC001_014_LANE()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
            {
                this.DialogResult = MessageBoxResult.Cancel;
                return;
            }

            sLotId = tmps[0].ToString();
            sStatus = tmps[1].ToString();

            GetDefect();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod
        private DataTable SearchLotInfo(string lotid, string status)
        {
            DataTable dt = new DataTable();
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID_PR", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow indata = inTable.NewRow();
                indata["LOTID_PR"] = lotid;
                indata["LOTID"] = null;
                indata["WIPSTAT"] = status;

                inTable.Rows.Add(indata);

                dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RUNLOT_SL", "INDATA", "RSLTDT", inTable);
            }
            catch (Exception ex)
            {
                //LOT 정보확인
                //LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(MessageDic.Instance.GetMessage("LOT 정보확인"), ex.Message, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                Util.MessageException(ex);
            }
            return dt;
        }

        private void GetDefect()
        {
            try
            {
                Util.gridClear(dgDefect);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["PROCID"] = Process.SLITTING;
                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_PRD_SEL_DEFECT", "INDATA", "OUTDATA", IndataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                            return;
                        }

                        dgDefect.ItemsSource = DataTableConverter.Convert(searchResult);

                        if (dgDefect.Rows.Count > 0)
                        {
                            DataTable Lotdt = SearchLotInfo(sLotId, sStatus);

                            if (Lotdt.Rows.Count > 0)
                            {
                                for (int i = dgDefect.Columns.Count; i-- > 0;)
                                {
                                    if (i >= 4)
                                        dgDefect.Columns.RemoveAt(i);
                                }
                                for (int i = 0; i < Lotdt.Rows.Count; i++)
                                {
                                    ChildLot = Util.NVC(Lotdt.Rows[i]["LOTID"]);

                                    if (i == 0) Util.SetGridColumnNumeric(dgDefect, "ALL", null, "ALL", false, false, false, false, -1, HorizontalAlignment.Right, Visibility.Visible);

                                    Util.SetGridColumnNumeric(dgDefect, ChildLot + "CNT", null, "태그수", false, false, false, false, -1, HorizontalAlignment.Right, Visibility.Visible);
                                    Util.SetGridColumnNumeric(dgDefect, ChildLot, null, ChildLot, true, true, true, false, -1, HorizontalAlignment.Right, Visibility.Visible);

                                    if (dgDefect.Rows.Count == 0) continue;

                                    DataTable dt = GetDefectDataByLot(ChildLot);
                                    if (dt != null)
                                    {
                                        for (int j = 0; j < dt.Rows.Count; j++)
                                        {
                                            if (dt.Rows[j]["RESNQTY"].ToString().Equals(""))
                                            {
                                                BindingDataGrid(dgDefect, j, dgDefect.Columns.Count, 0);
                                            }
                                            else
                                            {
                                                BindingDataGrid(dgDefect, j, dgDefect.Columns.Count, dt.Rows[j]["RESNQTY"]);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private DataTable GetDefectDataByLot(string LotId)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("RESNPOSITION", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["LOTID"] = LotId;
                Indata["RESNPOSITION"] = null;
                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPREASONCOLLECT_ELEC", "INDATA", "RSLTDT", IndataTable);

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private void BindingDataGrid(C1.WPF.DataGrid.C1DataGrid datagrid, int iRow, int iCol, object sValue)
        {
            if (datagrid.ItemsSource == null)
                return;
            else
            {

                DataTable dt = DataTableConverter.Convert(datagrid.ItemsSource);
                if (dt.Columns.Count < datagrid.Columns.Count)
                {
                    for (int i = dt.Columns.Count; i < datagrid.Columns.Count; i++)
                    {
                        dt.Columns.Add(datagrid.Columns[i].Name);
                    }
                }
                if (sValue.Equals("") || sValue.Equals(null))
                {
                    sValue = 0;
                }
                dt.Rows[iRow][iCol - 1] = sValue;

                datagrid.BeginEdit();
                datagrid.ItemsSource = DataTableConverter.Convert(dt);
                datagrid.EndEdit();
            }
        }
        #endregion

    }
}