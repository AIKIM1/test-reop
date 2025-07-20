/*************************************************************************************
 Created Date : 2017.03.13
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - 조립 공정진척 화면 - 품질정보 조회 화면
--------------------------------------------------------------------------------------
 [Change History]
  2017.03.13  INS 김동일K : Initial Created.
  2017.07.24  Lee. D. R : 품질정보조회 화면 개선 - 품질정보조회시 입력한 값에 해당하는 Lot 도 같이 보여주기.
  2017.09.15  INS 염규범S : GMES 품질 치수 내용 저장시 System 확인후 " 저장 할까요" Message Pop up 요청의 - C20170911_81274, C20170911_81363
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ASSY_QUALITY_PKG.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_QUALITY_PKG : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _LineID = string.Empty;
        private string _ProcID = string.Empty;
        private string _EqptID = string.Empty;
        private string _LineName = string.Empty;
        private string _EqptName = string.Empty;
        int _maxColumn = 0;

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();

        #endregion

        #region Initialize    
        public CMM_ASSY_QUALITY_PKG()
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

        private void InitCombo()
        {

            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            string[] tmpFilter = new string[] { LoginInfo.CFG_AREA_ID };

            //라인
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, sFilter: tmpFilter);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboProcessChild, cbParent: cboProcessParent);

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent);

            //작업조
            //C1ComboBox[] cboShiftParent = { cboEquipmentSegment, cboProcess };
            //_combo.SetCombo(cboShift, CommonCombo.ComboStatus.NONE, cbParent: cboShiftParent, sCase: "SHIFT", sFilter: tmpFilter);
            //const string bizRuleName = "DA_BAS_SEL_SHIFT_CBO";
            //string[] arrColumn = { "LANGID", "SHOPID", "AREAID", "EQSGID", "PROCID" };
            //string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, _LineID, _ProcID };
            //string selectedValueText = cboShift.SelectedValuePath;
            //string displayMemberText = cboShift.DisplayMemberPath;
            //CommonCombo.CommonBaseCombo(bizRuleName, cboShift, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQSG_ID);

        }
        #endregion

        #region Event
        #endregion

        #region Mehod

        #region Biz
        private void GeqQualityTP1(C1.WPF.DataGrid.C1DataGrid dg, string sType)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("WIPSEQ", typeof(Int16));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("CLCTITEM_CLSS4", typeof(string));
                dtRqst.Columns.Add("CLCTITEM_CLSS3", typeof(string));
                dtRqst.Columns.Add("DIMEN_INSDTTM", typeof(string));
                dtRqst.Columns.Add("CALDATE", typeof(string));
                dtRqst.Columns.Add("CBO_CODE", typeof(string));
                dtRqst.Columns.Add("SHOPID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));


                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                dr["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);                
                dr["CLCTITEM_CLSS4"] = sType;
                dr["CBO_CODE"] = Util.NVC(cboShift.SelectedValue);
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                if ( dtpDate != null && dtpDate.SelectedDateTime != null )
                    dr["CALDATE"] = dtpDate.SelectedDateTime.Date.ToString("yyyy-MM-dd");
                else
                    dr["CALDATE"] = "";

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPDATACOLLECT_CL_TP1", "INDATA", "OUTDATA", dtRqst);

                if ( dtRslt != null && dtRslt.Columns.Count > 0 )
                {
                    C1.WPF.DataGrid.DataGridLength width = new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto);

                    for ( int i = 0 ; i < dtRslt.Columns.Count ; i++ )
                    {
                        if ( !dg.Columns.Contains(dtRslt.Columns[i].ColumnName) )
                        {
                            Util.SetGridColumnText(dg, dtRslt.Columns[i].ColumnName, null, dtRslt.Columns[i].ColumnName, true, false, false, true, 110, HorizontalAlignment.Center, Visibility.Visible);
                        }
                    }
                }

                //dg.ItemsSource = dtRslt.DefaultView;//DataTableConverter.Convert(dtRslt);

                Util.GridSetData(dg, dtRslt, FrameOperation, false);
            }
            catch ( Exception ex )
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GeqQualityTP2(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("WIPSEQ", typeof(Int16));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("CLCTITEM_CLSS4", typeof(string));
                dtRqst.Columns.Add("CLCTITEM_CLSS3", typeof(string));
                dtRqst.Columns.Add("DIMEN_INSDTTM", typeof(string));
                dtRqst.Columns.Add("CALDATE", typeof(string));
                dtRqst.Columns.Add("CBO_CODE", typeof(string));
                dtRqst.Columns.Add("SHOPID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));


                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                dr["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                dr["CLCTITEM_CLSS4"] = "B";
                dr["CBO_CODE"] = Util.NVC(cboShift.SelectedValue);
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                if ( dtpDate != null && dtpDate.SelectedDateTime != null )
                    dr["CALDATE"] = dtpDate.SelectedDateTime.Date.ToString("yyyy-MM-dd");
                else
                    dr["CALDATE"] = "";

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPDATACOLLECT_CL_TP2", "INDATA", "OUTDATA", dtRqst);

                if ( dtRslt != null && dtRslt.Columns.Count > 0 )
                {
                    C1.WPF.DataGrid.DataGridLength width = new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto);

                    for ( int i = 0 ; i < dtRslt.Columns.Count ; i++ )
                    {
                        if ( !dg.Columns.Contains(dtRslt.Columns[i].ColumnName) )
                        {
                            List<string> sList = new List<string>();
                            if ( dtRslt.Columns[i].ColumnName.IndexOf(":") >= 0 )
                            {
                                string[] sParmList = dtRslt.Columns[i].ColumnName.Split("||".ToArray(), StringSplitOptions.RemoveEmptyEntries);
                                foreach ( string sTmp in sParmList )
                                {
                                    sList.Add(sTmp);
                                }

                                Util.SetGridColumnText(dg, dtRslt.Columns[i].ColumnName, sList, dtRslt.Columns[i].ColumnName, true, false, false, true, 65, HorizontalAlignment.Center, Visibility.Visible);
                            }
                            else
                                Util.SetGridColumnText(dg, dtRslt.Columns[i].ColumnName, null, dtRslt.Columns[i].ColumnName, true, false, false, true, 65, HorizontalAlignment.Center, Visibility.Visible);
                        }
                    }
                }

                //dgQualityInfoSealing.ItemsSource = dtRslt.DefaultView;//DataTableConverter.Convert(dtRslt);

                Util.GridSetData(dg, dtRslt, FrameOperation, false);
            }
            catch ( Exception ex )
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GeqQualityTP3(C1.WPF.DataGrid.C1DataGrid dg, string sType)
        {
            try
            {
                ShowLoadingIndicator();

                string sBizName = "DA_QCA_SEL_WIPDATACOLLECT_CL_TP3";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("CLCTITEM_CLSS4", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CALDATE", typeof(string));
                inTable.Columns.Add("CBO_CODE", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                newRow["CLCTITEM_CLSS4"] = sType;
                newRow["EQPTID"] = _EqptID;
                if (dtpDate != null && dtpDate.SelectedDateTime != null)
                    newRow["CALDATE"] = dtpDate.SelectedDateTime.Date.ToString("yyyy-MM-dd");
                else
                    newRow["CALDATE"] = "";
                newRow["CBO_CODE"] = Util.NVC(cboShift.SelectedValue);
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", inTable);

                Util.GridSetData(dg, dtResult, null);

                DataTable dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPDATACOLLECT_ASSY_NOTE", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgdNote, dtRslt2, FrameOperation, false);

                if (dtResult == null || dtResult.Rows.Count == 0)
                    return;

                // 검사 항목의 Max Column까지만 보이게
                _maxColumn = 0;
                _maxColumn = dtResult.AsEnumerable().ToList().Max(r => (int)r["CLCT_COUNT"]);

                int Startcol = dg.Columns["ACTDTTM"].Index;
                int ForCount = 0;

                for (int col = Startcol + 1; col < dg.Columns.Count; col++)
                {
                    ForCount++;

                    if (ForCount > _maxColumn)
                        dg.Columns[col].Visibility = Visibility.Collapsed;
                    else
                        dg.Columns[col].Visibility = Visibility.Visible;
                }

                dg.AlternatingRowBackground = null;

                // Merge
                string[] sColumnName = new string[] { "CLCTITEM_CLSS_NAME4", "CLCTITEM_CLSS_NAME1" , "ACTDTTM"};
                _Util.SetDataGridMergeExtensionCol(dg, sColumnName, DataGridMergeMode.VERTICAL);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetQualityCMM()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("WIPSEQ", typeof(Int16));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("CLCTITEM_CLSS4", typeof(string));
                dtRqst.Columns.Add("CLCTITEM_CLSS3", typeof(string));
                dtRqst.Columns.Add("DIMEN_INSDTTM", typeof(string));
                dtRqst.Columns.Add("CALDATE", typeof(string));
                dtRqst.Columns.Add("CBO_CODE", typeof(string));
                dtRqst.Columns.Add("SHOPID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                dr["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                dr["CBO_CODE"] = Util.NVC(cboShift.SelectedValue);
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);

                if ( dtpDate != null && dtpDate.SelectedDateTime != null )
                    dr["CALDATE"] = dtpDate.SelectedDateTime.Date.ToString("yyyy-MM-dd");
                else
                    dr["CALDATE"] = "";

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPDATACOLLECT_ASSY", "INDATA", "OUTDATA", dtRqst);

                // 품질정보조회 화면 개선 - 품질정보조회시 입력한 값에 해당하는 Lot 도 같이 보여주기.
                if (dtRslt != null && dtRslt.Columns.Count > 0)
                {
                    C1.WPF.DataGrid.DataGridLength width = new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto);

                    for (int i = 0; i < dtRslt.Columns.Count; i++)
                    {
                        if (!dgCMMQualityInfo.Columns.Contains(dtRslt.Columns[i].ColumnName))
                        {
                            List<string> sList = new List<string>();
                            if (dtRslt.Columns[i].ColumnName.IndexOf(":") >= 0)
                            {
                                string[] sParmList = dtRslt.Columns[i].ColumnName.Split("||".ToArray(), StringSplitOptions.RemoveEmptyEntries);
                                foreach (string sTmp in sParmList)
                                {
                                    sList.Add(sTmp);
                                }

                                Util.SetGridColumnText(dgCMMQualityInfo, dtRslt.Columns[i].ColumnName, sList, dtRslt.Columns[i].ColumnName, true, false, false, true, 120, HorizontalAlignment.Center, Visibility.Visible);
                            }
                            else
                                Util.SetGridColumnText(dgCMMQualityInfo, dtRslt.Columns[i].ColumnName, null, dtRslt.Columns[i].ColumnName, true, false, false, true, 120, HorizontalAlignment.Center, Visibility.Visible);
                        }
                    }
                }

                //dg.ItemsSource = dtRslt.DefaultView;//DataTableConverter.Convert(dtRslt);

                Util.GridSetData(dgCMMQualityInfo, dtRslt, FrameOperation, false);

                DataTable dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPDATACOLLECT_ASSY_NOTE", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgdNote, dtRslt2, FrameOperation, false);
            }
            catch ( Exception ex )
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region Func
       
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSearch);
            //listAuth.Add(btnLossDfctSave);
            //listAuth.Add(btnPrdChgDfctSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void ShowLoadingIndicator()
        {
            if ( loadingIndicator != null )
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if ( loadingIndicator != null )
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void InitializeGrid(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                Util.gridClear(dg);

                if ( dg == null ) return;

                List<C1.WPF.DataGrid.DataGridColumn> dgList = new List<C1.WPF.DataGrid.DataGridColumn>();

                foreach ( C1.WPF.DataGrid.DataGridColumn col in dg.Columns )
                {
                    if ( col.Name.IndexOf(":") >= 0 )
                    {
                        dgList.Add(col);
                    }
                }

                if ( dgList.Count > 0 )
                {
                    foreach ( C1.WPF.DataGrid.DataGridColumn col in dgList )
                    {
                        dg.Columns.Remove(col);
                    }
                }
            }
            catch ( Exception ex )
            {
                Util.MessageException(ex);
            }
        }

        private void InitializeGridHeaders()
        {
            try
            {
                if ( dgQualityInfoDimen == null || dgQualityInfoDimen.ItemsSource == null ) return;

                //dgQualityInfoDimen.TopRows = 2;

            }
            catch ( Exception ex )
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if ( tmps != null && tmps.Length >= 5 )
                {
                    _LineID = Util.NVC(tmps[0]);
                    _ProcID = Util.NVC(tmps[1]);
                    _EqptID = Util.NVC(tmps[2]);
                    _LineName = Util.NVC(tmps[3]);
                    _EqptName = Util.NVC(tmps[4]);
                }
                else
                {
                    _LineID = "";
                    _EqptID = "";
                    _ProcID = "";
                    _LineName = "";
                    _EqptName = "";
                }

                // 남경인 경우 화면 SIZE 조정..
                if (LoginInfo.CFG_SHOP_ID.Equals("G182") || LoginInfo.CFG_AREA_ID.Equals("S5"))
                {
                    this.Width = 800;
                    this.Height = 700;
                }

                ApplyPermissions();

                // 라인
                if ( cboEquipmentSegment != null && cboEquipmentSegment.Items != null && cboEquipmentSegment.Items.Count > 0 )
                {
                    cboEquipmentSegment.SelectedValue = _LineID;
                    cboEquipmentSegment.IsEnabled = false;
                }

                //공정
                if ( cboProcess != null && cboProcess.Items != null && cboProcess.Items.Count > 0 )
                {
                    cboProcess.SelectedValue = _ProcID;

                    if ( !cboEquipmentSegment.IsEnabled )
                        cboProcess.IsEnabled = false;
                }

                //설비
                if ( cboEquipment != null && cboEquipment.Items != null && cboEquipment.Items.Count > 0 )
                {
                    cboEquipment.SelectedValue = _EqptID;

                    if ( !cboEquipmentSegment.IsEnabled && !cboProcess.IsEnabled )
                        cboEquipment.IsEnabled = false;
                }

                if (_ProcID.Equals(Process.PACKAGING))
                {
                    c1tabCmm.Visibility = Visibility.Collapsed;
                }
                else
                {
                    c1tab.Visibility = Visibility.Collapsed;
                    c1tabDimen.Visibility = Visibility.Collapsed;
                    c1tabSealing.Visibility = Visibility.Collapsed;
                }
            }
            catch ( Exception ex )
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if ( c1tabDimen.IsSelected )
            {
                InitializeGrid(dg: dgQualityInfoDimen);

                //GeqQualityTP2(dg: dgQualityInfoDimen);
                GeqQualityTP3(dg: dgQualityInfoDimen, sType: "B");
                
            }

            if ( c1tab.IsSelected )
            {
                InitializeGrid(dg: dgQualityInfo);

                //GeqQualityTP1(dg: dgQualityInfo, sType: "A");    // Tensil Strength
                GeqQualityTP3(dg: dgQualityInfo, sType: "A");    // Tensil Strength

            }

            if ( c1tabSealing.IsSelected )
            {
                InitializeGrid(dg: dgQualityInfoSealing);

                //GeqQualityTP1(dg: dgQualityInfoSealing, sType: "C");
                GeqQualityTP3(dg: dgQualityInfoSealing, sType: "C");
            }

            if ( c1tabCmm.IsSelected )
            {
                InitializeGrid(dg: dgCMMQualityInfo);

                GetQualityCMM();
            }



            //ShowLoadingIndicator();

            //if (c1tabDimen.Visibility == Visibility.Visible)
            //{
            //    InitializeGrid(dg: dgQualityInfoDimen);
            //    GeqQualityTP2(dg: dgQualityInfoDimen);
            //}

            //if (c1tab.Visibility == Visibility.Visible)
            //{
            //    InitializeGrid(dg: dgQualityInfo);
            //    GeqQualityTP1(dg: dgQualityInfo, sType: "A");    // Tensil Strength
            //}

            //if (c1tabSealing.Visibility == Visibility.Visible)
            //{
            //    InitializeGrid(dg: dgQualityInfoSealing);
            //    GeqQualityTP1(dg: dgQualityInfoSealing, sType: "C");
            //}

            //if (c1tabCmm.Visibility == Visibility.Visible)
            //{
            //    InitializeGrid(dg: dgCMMQualityInfo);
            //    GetQualityCMM();
            //}

            //HiddenLoadingIndicator();
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {
            InitCombo();
        }

        private void C1TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            C1TabControl c1TabControl = sender as C1TabControl;
            if ( c1TabControl.IsLoaded )
            {
                if ( c1tabDimen.IsSelected )
                {
                    InitializeGrid(dg: dgQualityInfoDimen);

                    //GeqQualityTP2(dg: dgQualityInfoDimen);
                    GeqQualityTP3(dg: dgQualityInfoDimen, sType: "B");
                }

                if ( c1tab.IsSelected )
                {
                    InitializeGrid(dg: dgQualityInfo);

                    //GeqQualityTP1(dg: dgQualityInfo, sType: "A");    // Tensil Strength
                    GeqQualityTP3(dg: dgQualityInfo, sType: "A");    // Tensil Strength

                }

                if ( c1tabSealing.IsSelected )
                {
                    InitializeGrid(dg: dgQualityInfoSealing);

                    //GeqQualityTP1(dg: dgQualityInfoSealing, sType: "C");
                    GeqQualityTP3(dg: dgQualityInfoSealing, sType: "C");
                }

                if ( c1tabCmm.IsSelected )
                {
                    InitializeGrid(dg: dgCMMQualityInfo);

                    GetQualityCMM();
                }
            }
        }

        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            InitializeGrid(dg: dgQualityInfoDimen);
            InitializeGrid(dg: dgQualityInfo);
            InitializeGrid(dg: dgQualityInfoSealing);
            InitializeGrid(dg: dgCMMQualityInfo);
            InitializeGrid(dg: dgdNote);

            this.Focus();
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                InitializeGrid(dg: dgQualityInfoDimen);
                InitializeGrid(dg: dgQualityInfo);
                InitializeGrid(dg: dgQualityInfoSealing);
                InitializeGrid(dg: dgCMMQualityInfo);
                InitializeGrid(dg: dgdNote);

                //if (cboEquipmentSegment != null && cboEquipmentSegment.Items != null && cboEquipmentSegment.Items.Count > 0 && !cboEquipmentSegment.SelectedValue.ToString().Equals(_LineID))
                //    cboEquipmentSegment.SelectedValue = _LineID;
            }
            catch ( Exception ex )
            {
                Util.MessageException(ex);
            }
        }

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                InitializeGrid(dg: dgQualityInfoDimen);
                InitializeGrid(dg: dgQualityInfo);
                InitializeGrid(dg: dgQualityInfoSealing);
                InitializeGrid(dg: dgCMMQualityInfo);
                InitializeGrid(dg: dgdNote);

                //if (cboProcess != null && cboProcess.Items != null && cboProcess.Items.Count > 0 && !cboProcess.SelectedValue.ToString().Equals(_ProcID))
                //    cboProcess.SelectedValue = _ProcID;

                string sEqsg = cboEquipmentSegment != null && cboEquipmentSegment.SelectedValue != null ? Util.NVC(cboEquipmentSegment.SelectedValue) : "";
                string sProc = cboProcess != null && cboProcess.SelectedValue != null ? Util.NVC(cboProcess.SelectedValue) : "";
                const string bizRuleName = "DA_BAS_SEL_SHIFT_CBO";
                string[] arrColumn = { "LANGID", "SHOPID", "AREAID", "EQSGID", "PROCID" };
                string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, sEqsg, sProc };
                string selectedValueText = cboShift.SelectedValuePath;
                string displayMemberText = cboShift.DisplayMemberPath;
                CommonCombo.CommonBaseCombo(bizRuleName, cboShift, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQSG_ID);

            }
            catch ( Exception ex )
            {
                Util.MessageException(ex);
            }
        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                InitializeGrid(dg: dgQualityInfoDimen);
                InitializeGrid(dg: dgQualityInfo);
                InitializeGrid(dg: dgQualityInfoSealing);
                InitializeGrid(dg: dgCMMQualityInfo);
                InitializeGrid(dg: dgdNote);

                //if (cboEquipment != null && cboEquipment.Items != null && cboEquipment.Items.Count > 0 && !cboEquipment.SelectedValue.ToString().Equals(_EqptID))
                //    cboEquipment.SelectedValue = _EqptID;
            }
            catch ( Exception ex )
            {
                Util.MessageException(ex);
            }
        }

        private void cboShift_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                InitializeGrid(dg: dgQualityInfoDimen);
                InitializeGrid(dg: dgQualityInfo);
                InitializeGrid(dg: dgQualityInfoSealing);
                InitializeGrid(dg: dgCMMQualityInfo);
                InitializeGrid(dg: dgdNote);
            }
            catch ( Exception ex )
            {
                Util.MessageException(ex);
            }
        }

        private void DataGridTemplateColumn_GettingCellValue(object sender, C1.WPF.DataGrid.DataGridGettingCellValueEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.DataGridTemplateColumn dgtc = sender as C1.WPF.DataGrid.DataGridTemplateColumn;
                System.Data.DataRowView drv = e.Row.DataItem as System.Data.DataRowView;

                if (dgtc != null && drv != null && dgtc.Name != null)
                {
                    e.Value = drv[dgtc.Name].ToString();
                }
            }
            catch (Exception ex)
            {
                //오류 발생할 경우 아무 동작도 하지 않음. try catch 없으면 이 로직에서 오류날 경우 복사 자체가 안됨
            }
        }
    }
}
