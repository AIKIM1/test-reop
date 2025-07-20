/*************************************************************************************
 Created Date : 2020.10.14
      Creator : 
   Decription : Tray 이력
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.14  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows.Controls;
using System.Windows;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Media;
using System.Collections;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_001_BOX_MODEL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _sRow;
        private string _sCol;
        private string _sStg;
        private string _sEqpID;
        private string _sType;  //E : EQPTID, R : RACK

        public string ROW
        {
            set { this._sRow = value; }
        }

        public string COL
        {
            set { this._sCol = value; }
        }

        public string STG
        {
            set { this._sStg = value; }
        }

        public string EQP
        {
            set { this._sEqpID = value; }
        }

        public string TYPE
        {
            set { this._sType = value; }
        }
        public FCS002_001_BOX_MODEL()
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
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            if (tmps != null && tmps.Length >= 1)
            {
                ROW = Util.NVC(tmps[0]);
                COL = Util.NVC(tmps[1]);
                STG = Util.NVC(tmps[2]);
                EQP = Util.NVC(tmps[3]);
                TYPE = Util.NVC(tmps[4]);
            }
            SetText();
            InitControl();
        }

        private void SetText()
        {
            txtRow.Text = _sRow;
            txtCol.Text = _sCol;
            txtStg.Text = _sStg;
        }

        private void InitControl()
        {
            GetOp();
            GetModel();
            GetDeg();
            GetRoute();
            GetTmpeAmpere();
        }

        #endregion

        #region Event
        private void dgModel_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgModel.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        private void dgDeg_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgDeg.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        private void dgRoute_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgRoute.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }
        #endregion

        #region Mehod
        private void GetOp()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["EQPTID"] = _sEqpID;

                inDataTable.Rows.Add(dr);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_FORMATION_BOX_OP_CHECK_MB", "RQSTDT", "RSLTDT", inDataTable);

                if (dtRslt.Rows.Count > 0)
                {
                    // 충전
                    if (dtRslt.Rows[0]["CHG_ABLE_YN"].ToString() == "Y")
                    {
                        chkCharge.IsChecked = true;
                    }
                    else
                    {
                        chkCharge.IsChecked = false;
                    }

                    // 방전
                    if (dtRslt.Rows[0]["DCHG_ABLE_YN"].ToString() == "Y")
                    {
                        chkDischarge.IsChecked = true;
                    }
                    else
                    {
                        chkDischarge.IsChecked = false;
                    }

                    // OCV
                    if (dtRslt.Rows[0]["OCV_ABLE_YN"].ToString() == "Y")
                    {
                        chkOCV.IsChecked = true;
                    }
                    else
                    {
                        chkOCV.IsChecked = false;
                    }

                    // Impedance
                    if (dtRslt.Rows[0]["IMP_ABLE_YN"].ToString() == "Y")
                    {
                        chkImp.IsChecked = true;
                    }
                    else
                    {
                        chkImp.IsChecked = false;
                    }

                    // Impedance
                    if (dtRslt.Rows[0]["LCI_ABLE_YN"].ToString() == "Y")
                    {
                        chkLCI.IsChecked = true;
                    }
                    else
                    {
                        chkLCI.IsChecked = false;
                    }

                    // Impedance
                    if (dtRslt.Rows[0]["PURSE_ABLE_YN"].ToString() == "Y")
                    {
                        chkPurse.IsChecked = true;
                    }
                    else
                    {
                        chkPurse.IsChecked = false;
                    }

                    // Impedance
                    if (dtRslt.Rows[0]["PRE_ABLE_YN"].ToString() == "Y")
                    {
                        chkPre.IsChecked = true;
                    }
                    else
                    {
                        chkPre.IsChecked = false;
                    }
                }

                //상태가 다른게 있는 경우 fals 시킴
                if (dtRslt.Rows.Count > 1)
                {
                    ArrayList alCol = new ArrayList();
                    alCol.Add("CHG_ABLE_YN");
                    alCol.Add("DCHG_ABLE_YN");
                    alCol.Add("OCV_ABLE_YN");
                    alCol.Add("IMP_ABLE_YN");
                    alCol.Add("LCI_ABLE_YN");
                    alCol.Add("PURSE_ABLE_YN");
                    alCol.Add("PRE_ABLE_YN");

                    foreach (string sCol in alCol)
                    {
                        DataTable dt1 = dtRslt.DefaultView.ToTable(true, sCol);

                        if (dt1.Rows.Count > 1)
                        {
                            chkCharge.IsChecked = false;
                            chkDischarge.IsChecked = false;
                            chkOCV.IsChecked = false;
                            chkImp.IsChecked = false;
                            chkLCI.IsChecked = false;
                            chkPurse.IsChecked = false;
                            chkPre.IsChecked = false;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetModel()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQPTID"] = _sEqpID;

                inDataTable.Rows.Add(dr);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_FORMATION_BOX_ABLE_MODEL_MB", "RQSTDT", "RSLTDT", inDataTable);
                Util.GridSetData(dgModel, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetDeg()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = _sEqpID;

                inDataTable.Rows.Add(dr);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_FORMATION_BOX_ABLE_DEG_MB", "RQSTDT", "RSLTDT", inDataTable);
                Util.GridSetData(dgDeg, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetRoute()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQPTID"] = _sEqpID;

                inDataTable.Rows.Add(dr);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_FORMATION_BOX_ABLE_ROUTE_MB", "RQSTDT", "RSLTDT", inDataTable);
                Util.GridSetData(dgRoute, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetTmpeAmpere()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["EQPTID"] = _sEqpID;

                inDataTable.Rows.Add(dr);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_FORMATION_BOX_TEMP_AMPERE_MB", "RQSTDT", "RSLTDT", inDataTable);

                if (dtRslt.Rows[0]["BOX_TEMP"].ToString() == "29")
                {
                    txtTemp.Text = "29℃";
                }
                else
                {
                    txtTemp.Text = "25℃";
                }

                if (dtRslt.Rows[0]["BOX_AMPERE"].ToString() == "2")
                {
                    txtAmpere.Text = "2";
                }
                else
                {
                    txtAmpere.Text = string.Empty;
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
