/************************************************************************************* 
 Created Date : 2023.11.10
      Creator : Chi Woo
   Decription : EOL/Degas 공 Tray 출고
--------------------------------------------------------------------------------------
 [Change History]
  2023.11.10  조영대 : Initial Created.
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_165_TRAY_OUT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string EQPTID = string.Empty;

        public FCS001_165_TRAY_OUT()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void btnForceRelease_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable chkTable = dgTrayOut.GetDataTable();

                for (int row1 = 0; row1 < chkTable.Rows.Count; row1++)
                {
                    DataRow dr1 = chkTable.Rows[row1];
                    for (int row2 = 0; row2 < chkTable.Rows.Count; row2++)
                    {
                        DataRow dr2 = chkTable.Rows[row2];
                        if (row1 == row2) continue;

                        if (dr1["EQPTID"].Nvc() + dr1["CST_TYPE_CODE"].Nvc() == dr2["EQPTID"].Nvc() + dr2["CST_TYPE_CODE"].Nvc())
                        {
                            Util.MessageValidation("SFU2051");
                            return;
                        }
                    }
                }

                if (!dgTrayOut.IsCheckedRow("CHK"))
                {
                    Util.MessageValidation("SFU1651");
                    return;
                }

                foreach (int chkRowInx in dgTrayOut.GetCheckedRowIndex("CHK"))
                {
                    DataRow chkRow = dgTrayOut.GetDataRow(chkRowInx);

                    if (chkRow["EQPTID"].IsNvc())
                    {
                        Util.MessageValidation("SFU1231");                        
                        return;
                    }

                    if (chkRow["CST_TYPE_CODE"].IsNvc())
                    {
                        Util.MessageValidation("SFU8275", dgTrayOut.Columns["CST_TYPE_CODE"].Header.Nvc());
                        return;
                    }

                    for (int colNo = 1; colNo <= 10; colNo++)
                    {
                        if (chkRow["ATTR" + colNo.Nvc()].Nvc() == string.Empty)
                        {
                            Util.MessageValidation("SFU1636", dgTrayOut.Columns["CST_TYPE_CODE"].Header.Nvc());
                            return;
                        }
                    }
                }

                //요청 하시겠습니까?
                Util.MessageConfirm("SFU2924", (result) =>
                {
                    try
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            DataTable dtRqst = new DataTable();
                            dtRqst.Columns.Add("AREAID", typeof(string));
                            dtRqst.Columns.Add("EQPTID", typeof(string));
                            dtRqst.Columns.Add("CST_TYPE_CODE", typeof(string));
                            dtRqst.Columns.Add("ATTR1", typeof(string));
                            dtRqst.Columns.Add("ATTR2", typeof(string));
                            dtRqst.Columns.Add("ATTR3", typeof(string));
                            dtRqst.Columns.Add("ATTR4", typeof(string));
                            dtRqst.Columns.Add("ATTR5", typeof(string));
                            dtRqst.Columns.Add("ATTR6", typeof(string));
                            dtRqst.Columns.Add("ATTR7", typeof(string));
                            dtRqst.Columns.Add("ATTR8", typeof(string));
                            dtRqst.Columns.Add("ATTR9", typeof(string));
                            dtRqst.Columns.Add("ATTR10", typeof(string));
                            dtRqst.Columns.Add("USERID", typeof(string));

                            foreach (DataRow chkRow in dgTrayOut.GetCheckedDataRow("CHK"))
                            {
                                if (chkRow["EQPTID"].Nvc() == string.Empty) continue;

                                DataRow drRqst = dtRqst.NewRow();
                                drRqst["AREAID"] = LoginInfo.CFG_AREA_ID;
                                drRqst["EQPTID"] = chkRow["EQPTID"].Nvc();
                                drRqst["CST_TYPE_CODE"] = chkRow["CST_TYPE_CODE"].Nvc();
                                drRqst["ATTR1"] = chkRow["ATTR1"].Nvc();
                                drRqst["ATTR2"] = chkRow["ATTR2"].Nvc();
                                drRqst["ATTR3"] = chkRow["ATTR3"].Nvc();
                                drRqst["ATTR4"] = chkRow["ATTR4"].Nvc();
                                drRqst["ATTR5"] = chkRow["ATTR5"].Nvc();
                                drRqst["ATTR6"] = chkRow["ATTR6"].Nvc();
                                drRqst["ATTR7"] = chkRow["ATTR7"].Nvc();
                                drRqst["ATTR8"] = chkRow["ATTR8"].Nvc();
                                drRqst["ATTR9"] = chkRow["ATTR9"].Nvc();
                                drRqst["ATTR10"] = chkRow["ATTR10"].Nvc();
                                drRqst["USERID"] = LoginInfo.USERID;
                                dtRqst.Rows.Add(drRqst);
                            }

                            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MER_FORMLGS_TRAY_MOVE_INFO", "RQSTDT", "RSLTDT", dtRqst);

                            Util.AlertInfo("FM_ME_0186");  //요청을 완료하였습니다.


                            this.DialogResult = MessageBoxResult.OK;
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

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void dgTrayOut_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (dgTrayOut.GetValue(e.Row.Index, e.Column.Name) == null) return;

            if (dgTrayOut.GetValue(e.Row.Index, e.Column.Name).Equals(true) ||
                Util.NVC(dgTrayOut.GetValue(e.Row.Index, e.Column.Name)).Equals("True") ||
                Util.NVC(dgTrayOut.GetValue(e.Row.Index, e.Column.Name)).Equals("Y"))
            {
                e.Cancel = true;
                return;
            }
        }

        private void dgTrayOut_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (e.Cell.Column.Index < 4) return;

            DataRow drClickRow = dgTrayOut.GetDataRow(e.Cell.Row.Index);

            foreach (DataGridColumn column in dgTrayOut.Columns)
            {
                if (!(column is DataGridCheckBoxColumn)) continue;

                if (e.Cell.Column.Name.Equals(column.Name))
                {
                    drClickRow[column.Name] = "Y";
                }
                else
                {
                    drClickRow[column.Name] = "N";
                }
            }
            dgTrayOut.Refresh();
        }

        private void dgTrayOut_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("S70", typeof(string));
            dtRqst.Columns.Add("EQPTLEVEL", typeof(string));

            DataRow drRqst = dtRqst.NewRow();
            drRqst["LANGID"] = LoginInfo.LANGID;
            drRqst["S70"] = "D,5";
            drRqst["EQPTLEVEL"] = "M";
            dtRqst.Rows.Add(drRqst);

            DataTable dtEqpt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_EQP", "RQSTDT", "RSLTDT", dtRqst);

            dgTrayOut.SetGridColumnCombo("EQPTID", dtEqpt, isInBlank: true, isInCode: true);
        }
        #endregion

        #region Mehod
        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable("RQSTDT");
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                // 백그라운드 실행. DA_SEL_FORMLGS_TRAY_MOVE_INFO
                dgTrayOut.ExecuteService("DA_SEL_FORMLGS_TRAY_MOVE_INFO", "RQSTDT", "RSLTDT", dtRqst, autoWidth: false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private void btnRowAdd_Click(object sender, RoutedEventArgs e)
        {
            DataTable dtData = dgTrayOut.GetDataTable(false);
            DataRow drNew = dtData.NewRow();
            drNew["CHK"] = 1;
            dtData.Rows.Add(drNew);
        }
    }
}
