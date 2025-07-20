using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ELEC_RP_SAMPLING.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_RP_SAMPLING : C1Window, IWorkArea    {
        
        #region Initialize 
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        C1DataGrid ProdInfo;

        public C1DataGrid ProdInfo_GRID
        {
            get { return ProdInfo_GRID; }
            set { ProdInfo_GRID = value; }
        }



        public CMM_ELEC_RP_SAMPLING()
        {
            InitializeComponent();
        }
        #endregion

        #region Loaded Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ApplyPermissions();
                cboTarget.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
        #region Event Method
        private void dgSampling_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1DataGrid grid = sender as C1DataGrid;
            if (grid != null && e.Column.Index == grid.Columns["PRODID"].Index)
                if (string.Equals(DataTableConverter.GetValue(e.Row.DataItem, "DELETEYN"), "N"))
                    e.Cancel = true;            
        }

        private void cboTarget_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetSamplingTargetList();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (dgSampling.ItemsSource == null || dgSampling.Rows.Count < 0)
                return;

            DataTable dt = ((DataView)dgSampling.ItemsSource).Table;

            DataRow dr = dt.NewRow();
            dr["CHK"] = true;
            dr["TARGETNAME"] = Util.NVC(cboTarget.Text);
            dr["PRODID"] = string.Empty;
            dr["CONFIRMYN"] = "Y";            
            dr["DELETEYN"] = "Y";
            dr["QMS"] = "Y";
            dt.Rows.Add(dr);

            dgSampling.ScrollIntoView(dt.Rows.Count - 1, 0);
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (dgSampling.ItemsSource == null || dgSampling.Rows.Count < 0)
                return;

            DataTable dt = ((DataView)dgSampling.ItemsSource).Table;

            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
                if (Convert.ToBoolean(dt.Rows[i]["CHK"]) && string.Equals(dt.Rows[i]["DELETEYN"], "Y"))
                    dt.Rows[i].Delete();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = ((DataView)dgSampling.ItemsSource).Table;

            foreach (DataRow row in dt.Rows)
            {
                if(Convert.ToBoolean(row["CHK"]))
                {
                    if(string.IsNullOrEmpty(Util.NVC(row["PRODID"])))
                    {
                        Util.MessageValidation("SFU2949");  //제품ID를 입력하세요.
                        return;
                    }
                    else
                    {
                        //DB에서 입력된 prodid를 조회 해서 가져옴.
                        DataTable dtProdChek = new DataTable();

                        dtProdChek.Columns.Add("PRODID", typeof(string));

                        DataRow dataRow = dtProdChek.NewRow();
                        dataRow["PRODID"] = Util.NVC(row["PRODID"]).ToString();

                        dtProdChek.Rows.Add(dataRow);

                        DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_VW_PRODUCT", "RQLTDT", "RSLTDT", dtProdChek);

                        if(result == null || result.Rows.Count == 0)
                        {
                            //값이 없으면
                            Util.MessageValidation("SFU4211");  //등록된 제품 ID가 아닙니다 .
                            return;
                        }

                        
                    }


                }               

            }
            SaveSamplingTargetData();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetSamplingTargetList();
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion
        #region Method
        private void GetSamplingTargetList()
        {
            try
            {
                ComboBoxItem item = cboTarget.SelectedItem as ComboBoxItem;
                if ( item != null )
                {
                    
                    DataTable dt = new DataTable();
                    dt.Columns.Add("CMCDTYPE", typeof(string));                    

                    DataRow dataRow = dt.NewRow();
                    dataRow["CMCDTYPE"] = Util.NVC(item.Tag);
                    

                    dt.Rows.Add(dataRow);

                    DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_SAMPLE_PROD", "INDATA", "RSLTDT", dt);

                    Util.GridSetData(dgSampling, result, FrameOperation, true);
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void SaveSamplingTargetData()
        {
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        DataTable PRID = new DataTable();
                        string PRID2 = "";
                        ComboBoxItem item = cboTarget.SelectedItem as ComboBoxItem;
                        if (item != null)
                        {
                            
                            DataSet indataSet = new DataSet();
                            DataTable inData = indataSet.Tables.Add("INDATA");
                            inData.Columns.Add("CMCDTYPE", typeof(string));
                            inData.Columns.Add("CMCODE", typeof(string));
                            inData.Columns.Add("CMCDIUSE", typeof(string));
                            inData.Columns.Add("ATTRIBUTE1", typeof(string));
                            inData.Columns.Add("USERID", typeof(string));
                            

                            DataTable dt = ((DataView)dgSampling.ItemsSource).Table;
                            DataRow row = null;                           


                            foreach (DataRow inRow in dt.Rows)
                            {
                                if (Convert.ToBoolean(inRow["CHK"]))
                                {
                                    row = inData.NewRow();                                    
                                    
                                    row["CMCDTYPE"] = Util.NVC(item.Tag);
                                    row["CMCODE"] = Util.NVC(inRow["PRODID"]).ToUpper();
                                    row["CMCDIUSE"] = Util.NVC(inRow["CONFIRMYN"]);
                                    row["ATTRIBUTE1"] = Util.NVC(inRow["QMS"]);
                                    row["USERID"] = LoginInfo.USERID;
                                    




                                    indataSet.Tables["INDATA"].Rows.Add(row);

                                    PRID2 = Util.NVC(inRow["PRODID"]).ToUpper() + "," + PRID2;
                                }
  
                            }

                            new ClientProxy().ExecuteServiceSync_Multi("DA_PRD_UPD_LOT_SAMPLING_PROD", "INDATA", null, indataSet);                            
                            



                            GetSamplingTargetList();
                            }
                        Util.AlertInfo("SFU1270");  //저장되었습니다.

                        CMM_ELEC_RP_SAMPLING_POPUP popup = new CMM_ELEC_RP_SAMPLING_POPUP();
                        
                        if (popup != null)
                        {
                            
                            object[] Parameters = new object[1];
                            Parameters[0] = PRID2;
                            C1WindowExtension.SetParameters(popup, Parameters);

                            popup.owner = this;
                            popup.ShowModal();
                            popup.CenterOnScreen();
                            EventHandler popup_Closed = null;
                            popup.Closed -= popup_Closed;

                            popup.FrameOperation = this.FrameOperation;
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
        #region Authrity
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            listAuth.Add(btnSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion

    }
}
