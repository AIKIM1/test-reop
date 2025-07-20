/*************************************************************************************
 Created Date : 2021.01.25
      Creator : 
   Decription : 공정성 LOT관리
--------------------------------------------------------------------------------------
 [Change History]
   
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Threading;
using System.IO;
using System.Windows.Media;


namespace LGC.GMES.MES.COM001
{
    public partial class COM001_350 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        CommonCombo _combo = new CommonCombo();

        DataSet inDataSet = null;
        DataTable dtComp = new DataTable();
        DataTable dtiUse = new DataTable();

        bool Inbool = false;

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_350()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            setHead();
            SetEvent();
        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnSearch);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void InitCombo()
        {
            // 사용여부 코드
            string[] sFilter1 = { "USE_FLAG" };
            _combo.SetCombo(cboUse, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);
            cboUse.SelectedIndex = 1;

            //GRID 사용여부 조회
            dtiUse = GetCommonCode("IUSE");

            //PIVOT 컬럼 TITLE 조회
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("SHOPID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            RQSTDT.Rows.Add(dr);

            dtComp = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRE_INPUT_PRDT_AREA", "RQSTDT", "RSLTDT", RQSTDT);
        }
        #endregion

        #region Event
        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetSearchData();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (dgResult.ItemsSource == null)
                return;

            Button button = sender as Button;
            if (button != null)
            {
                COM001_350_CREATE_PROD wndPopup = new COM001_350_CREATE_PROD();
                wndPopup.FrameOperation = FrameOperation;

                if (wndPopup != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = LoginInfo.CFG_AREA_ID;

                    C1WindowExtension.SetParameters(wndPopup, Parameters);

                    wndPopup.Closed += new EventHandler(OnCloseProd);
                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }
            }
        }

        private void OnCloseProd(object sender, EventArgs e)
        {
            COM001_350_CREATE_PROD window = sender as COM001_350_CREATE_PROD;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                DataTable dt = ((DataView)dgResult.ItemsSource).Table;

                DataRow dr = dt.NewRow();
                dr["CHK"] = true;
                dr["USE_FLAG"] = "Y";
                dr["ELTR_TYPE"] = window._GetElecType;
                dr["PRJT_NAME"] = window._GetPrjtName;
                dr["PRODID"] = window._GetProductName;
                dt.Rows.Add(dr);

                dgResult.ScrollIntoView(dt.Rows.Count - 1, 0);
            }
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            //if (dgSampling.ItemsSource == null || dgSampling.Rows.Count < 0)
            //    return;

            //DataTable dt = ((DataView)dgSampling.ItemsSource).Table;

            //for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            //    if (Convert.ToBoolean(dt.Rows[i]["CHK"]) && string.Equals(dt.Rows[i]["DELETEYN"], "Y"))
            //        dt.Rows[i].Delete();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgResult.Rows.Count < 1)
                {
                    Util.MessageValidation("SFU8276");  //슬러리 재고가 없습니다.
                    return;
                }

                int selChk = _Util.GetDataGridCheckFirstRowIndex(dgResult, "CHK");

                if (selChk < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                //저장하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        SetSaveProdData();
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
            }
        }
        #endregion

        #region Mehod
        private void dgResult_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1DataGrid grid = sender as C1DataGrid;
            if (grid != null)
            {
                if (e.Column.Index != grid.Columns["CHK"].Index && DataTableConverter.GetValue(e.Row.DataItem, "CHK").Equals(0))
                {
                    e.Cancel = true;
                }
            }
        }

        private void setHead()
        {
            DataTable dt = dtComp.DefaultView.ToTable();

            //사용유무 = 'Y'
            DataRow[] dr = dtComp.Select("CMCDIUSE = 'Y'");

            int _maxColumn = Util.NVC_Int(dtComp.Rows.Count);

            int startcol = dgResult.Columns["PRODID"].Index;
            int forCount = 0;
            string colName;

            //C1.WPF.DataGrid.DataGridLength width = new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto);
            C1.WPF.DataGrid.DataGridLength width = new C1.WPF.DataGrid.DataGridLength(120, C1.WPF.DataGrid.DataGridUnitType.Pixel);

            for (int col = 0; col < dr.Length; col++)
            {
                forCount++;

                colName = dr[forCount - 1]["CMCODE"].ToString();

                List<string> sHeader = new List<string>() { dr[forCount - 1]["ATTRIBUTE1"].ToString() };

                SetGridCheckBoxColumn(dgResult, colName, sHeader, false, false, false, false, width, HorizontalAlignment.Center, Visibility.Visible, true, dr[forCount - 1]["CMCODE"].ToString());
            }
        }

        private DataTable GetCommonCode(string sType)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sType;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return dtResult;
            }
            catch (Exception ex) { }

            return new DataTable();
        }

        private void SetGridCheckBoxColumn(C1DataGrid C1Grid, string sBinding, List<string> sHeadNames, bool bUserResize, bool bUserSort,
                                            bool bUserFilter, bool bReadOnly, C1.WPF.DataGrid.DataGridLength nHeaderWidth, HorizontalAlignment HorizontalAlignment,
                                            Visibility ColumnVisibility, bool bEditOnSelection, string sTag)
        {
            C1.WPF.DataGrid.DataGridCheckBoxColumn col = new C1.WPF.DataGrid.DataGridCheckBoxColumn();

            Binding databinding = new Binding(sBinding);
            databinding.Mode = BindingMode.TwoWay;

            col.Header = sHeadNames;
            col.Binding = databinding;
            col.CanUserResize = bUserResize;
            col.CanUserSort = bUserSort;
            col.CanUserFilter = bUserFilter;
            col.IsReadOnly = bReadOnly;
            col.Width = nHeaderWidth;
            col.HorizontalAlignment = HorizontalAlignment;
            col.Visibility = ColumnVisibility;
            col.EditOnSelection = bEditOnSelection;
            col.Tag = sTag;
            C1Grid.Columns.Add(col);
        }

        private void GetSearchData()
        {
            try
            {
                if (loadingIndicator != null)
                    loadingIndicator.Visibility = Visibility.Visible;
                DoEvents();

                Util.gridClear(dgResult);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("PRJ_NAME", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));
                IndataTable.Columns.Add("USE_FLAG", typeof(string));
              
                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["PRJ_NAME"] = Util.NVC(txtPRJ.Text) == "" ? null : Util.NVC(txtPRJ.Text);
                Indata["PRODID"] = Util.NVC(txtProd.Text) == "" ? null : Util.NVC(txtProd.Text);
                Indata["USE_FLAG"] = Util.GetCondition(cboUse) == "" ? null : Util.GetCondition(cboUse);

                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_PRE_INPUT_PRDT", "RQSTDT", "RSLTDT", IndataTable);

                Util.GridSetData(dgResult, dtMain, FrameOperation);
                (dgResult.Columns["USE_FLAG"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtiUse.Copy());
                
                if (dtMain.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU1498"); // 데이터가 없습니다.
                }
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally { loadingIndicator.Visibility = Visibility.Collapsed; }
        }

        private void SetSaveProdData()
        {
            //저장 메시지
            try
            {
                DataTable dt = ((DataView)dgResult.ItemsSource).Table;
                DataSet indataSet = new DataSet();

                DataTable inDATA = indataSet.Tables.Add("INDATA");
                inDATA.Columns.Add("AREAID", typeof(string));
                inDATA.Columns.Add("PRODID", typeof(string));
                //inDATA.Columns.Add("USE_FLAG", typeof(string));
                inDATA.Columns.Add("USERID", typeof(string));
                inDATA.Columns.Add("POPUP_FLAG", typeof(string));

                int iIndex = dgResult.Columns["PRODID"].Index;

                foreach (DataRow inRow in dt.Rows)
                {
                    if (Convert.ToBoolean(inRow["CHK"]))
                    {
                        int colValue = 0;

                        for (int col = iIndex + 1; col < dgResult.Columns.Count; col++)
                        {
                            colValue++;
                            if (colValue > dt.Columns.Count)
                                break;

                            DataRow[] dr = dtComp.Select("CMCODE = '" + dgResult.Columns[col].Tag.ToString() + "' AND CMCDIUSE = 'Y'");
                            if (dr.Length > 0)
                            {
                                DataRow newRow = inDATA.NewRow();
                                newRow["AREAID"] = Util.NVC(dr[0]["CMCODE"]);
                                newRow["PRODID"] = Util.NVC(inRow["PRODID"]).ToUpper();
                                //newRow["USE_FLAG"] = Util.NVC(inRow["USE_FLAG"]);
                                newRow["USERID"] = LoginInfo.USERID;
                                newRow["POPUP_FLAG"] = Util.NVC(inRow[col]) == "1" ? "Y" : "N";

                                inDATA.Rows.Add(newRow);
                            }
                        }
                    }
                }
                new ClientProxy().ExecuteService("BR_PRD_REG_PRE_INPUT_PRDT", "INDATA", null, inDATA, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    GetSearchData();
                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                   
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
                
        }
        #endregion

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {

        }

        
    }
}
