/*************************************************************************************
 Created Date : 2023.06.16
      Creator : 
   Decription : 포장 출고 (Location 관리) - PALLET_INFO
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2023.07.21  주재홍 : 현장 테스트후 3차 개선
  2023.07.31  주재홍 : BizAct 다국어

**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001;
using System.Collections;
using LGC.GMES.MES.COM001;
using System.Windows.Controls;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_383_PALLET_INFO : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();

        private string sAREAID = string.Empty;
        private string sWH_PHYS_PSTN_CODE = string.Empty;
        private string sWH_ID = string.Empty;
        private string sRACK_ID = string.Empty;
        private string sPRJT_NAME = string.Empty;
        private string sEQSGID = string.Empty;
        private string sPRODID = string.Empty;

        public COM001_383_PALLET_INFO()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
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

            if (tmps == null)
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }

            sAREAID = Util.NVC(tmps[0]);
            sWH_PHYS_PSTN_CODE = Util.NVC(tmps[1]);
            sWH_ID = Util.NVC(tmps[2]);
            sRACK_ID = Util.NVC(tmps[3]);
            sPRJT_NAME = Util.NVC(tmps[4]);
            sEQSGID = Util.NVC(tmps[5]);
            sPRODID = Util.NVC(tmps[6]);

            txtProjectName.Text = sPRJT_NAME;

            InitCombo();

            GetPalletByLocation();
        }


        #endregion

        #region Event
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            SetcboSection(cboSection);
            SetcboLocation(cboLocation);
        }


        private static DataTable AddStatus(DataTable dt, string sValue, string sDisplay, string statusType)
        {
            DataRow dr = dt.NewRow();
            switch (statusType)
            {
                case "ALL":
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case "SELECT":
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case "NA":
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case "EMPTY":
                    dr[sValue] = string.Empty;
                    dr[sDisplay] = string.Empty;
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }


        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }
        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }


        private void SetcboSection(C1ComboBox cbo)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("WH_PHYS_PSTN_CODE", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = sAREAID;
                dr["WH_PHYS_PSTN_CODE"] = sWH_PHYS_PSTN_CODE;
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_GET_COMBO_SECTION_BY_BLDG", "RQSTDT", "RSLTDT", RQSTDT);
                cbo.DisplayMemberPath = "WH_NAME";
                cbo.SelectedValuePath = "WH_ID";

                cbo.ItemsSource = AddStatus(dtResult, "WH_ID", "WH_NAME", "ALL").Copy().AsDataView();

                if (sWH_ID != null && !sWH_ID.Equals(""))
                {
                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        DataRow row = dtResult.Rows[i];
                        if (sWH_ID.Equals(row["WH_ID"].ToString()))
                            cbo.SelectedIndex = i;
                    }
                } else {
                    cbo.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void cboSection_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sTemp = sWH_PHYS_PSTN_CODE;
            SetcboLocation(cboLocation);

        }


        private void SetcboLocation(MultiSelectionBox cbo)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("BLDG", typeof(string));
                RQSTDT.Columns.Add("WH_ID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["BLDG"] = sWH_PHYS_PSTN_CODE;
                String _whId = Util.GetCondition(cboSection, bAllNull: false).ToString();
                dr["WH_ID"] = Util.NVC(_whId).Equals(string.Empty) ? null : _whId;
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_GET_COMBO_RACK_ID_BY_SECTION", "RQSTDT", "RSLTDT", RQSTDT);
                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "RACK_ID";

                cbo.ItemsSource = DataTableConverter.Convert(dtResult);

                if (sRACK_ID != null && !sRACK_ID.Equals(""))
                {
                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        DataRow row = dtResult.Rows[i];
                        if (sRACK_ID.Contains(row["RACK_ID"].ToString()))
                            cbo.Check(i);
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetPalletByLocation();
        }


        public void GetPalletByLocation()
        {
            ShowLoadingIndicator();

            Util.gridClear(dgPalletInfo);

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                // INDATA.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("WH_ID", typeof(string));
                RQSTDT.Columns.Add("RACK_ID", typeof(string));
                // INDATA.Columns.Add("PRJT_NAME", typeof(string));
                // INDATA.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                //dr["AREAID"] = sAREAID;
                dr["WH_ID"] = sWH_ID;
                dr["RACK_ID"] = sRACK_ID;
                //dr["PRJT_NAME"] = sPRJT_NAME;
                //dr["EQSGID"] = sEQSGID;
                dr["PRODID"] = sPRODID;
                dr["LANGID"] = LoginInfo.LANGID;
                RQSTDT.Rows.Add(dr);

                string sBizName = "DA_PRD_GET_CELL_PLLT_SHIP_REQ_ABLE_LOCATION_BOXID";
                new ClientProxy().ExecuteService(sBizName, "RQSTDT", "RSLTDT", RQSTDT, (result, Exception) =>
                {
                    HiddenLoadingIndicator();
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (result.Rows.Count > 0)
                        Util.GridSetData(dgPalletInfo, result, FrameOperation, true);

                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }

        }



        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod
        private void Set_PopUp_ProductID()
        {
            try
            {
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        
        private void InitializeControls()
        {
            Util.gridClear(dgPalletInfo);

            GetPalletByLocation();
        }
        #endregion

    }
}
