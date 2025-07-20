/*************************************************************************************
 Created Date : 2021.07.13
      Creator : INS 이형대
   Decription : 소형3동 Pilot5라인 ZZS 증설 - GROUPLOT PRINT 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2021.07.13  INS 이형대  : Initial Created.  

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
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;

namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY004_GROUPLOT_PRINT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_GROUPLOT_PRINT : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _LineName = string.Empty;
        private string _Procid = string.Empty;
        private string _EqpName = string.Empty;

        private string _LDR_LOT_IDENT_BAS_CODE = string.Empty;

        private BizDataSet _Biz = new BizDataSet();
        private Util _util = new Util();
        
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY004_GROUPLOT_PRINT()
        {
            InitializeComponent();
        }

        #endregion

        #region Event       
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 5)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
                _Procid = Util.NVC(tmps[2]);
                _LineName = Util.NVC(tmps[3]);
                _EqpName = Util.NVC(tmps[4]);

                _LDR_LOT_IDENT_BAS_CODE = Util.NVC(tmps[4]);
            }
            else
            {
                _LineID = "";
                _EqptID = "";
                _Procid = "";
                _LineName = "";
                _EqpName = "";

                _LDR_LOT_IDENT_BAS_CODE = "";
            }

            ApplyPermissions();

            txtLineName.Text = _LineName;
            txtEqpName.Text = _EqpName;

            //기본 정보 조회
            GetGroupLotInfo();

        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {
                DataTable dtTmp = new DataTable();
                
                dtTmp.Columns.Add("CHK", typeof(Boolean));
                dtTmp.Columns.Add("PROD_LOTID", typeof(string));
                dtTmp.Columns.Add("START_TIME", typeof(string));
                dtTmp.Columns.Add("END_TIME", typeof(string));
                dtTmp.Columns.Add("EQPTNAME", typeof(string));
                dtTmp.Columns.Add("PRODID", typeof(string));
                dtTmp.Columns.Add("WIPSNAME", typeof(string));

                dgGroupLot.ItemsSource = DataTableConverter.Convert(dtTmp);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitializeControls()
        {
            Util.gridClear(dgGroupLot);
        }


        /// <summary>
        /// 초기화
        /// </summary>
        private void btnInitialize_Click(object sender, RoutedEventArgs e)
        {
            ////InitializeControls();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod

        #region [BizCall]
        private void GetGroupLotInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();

                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _EqptID;

                inTable.Rows.Add(newRow);

                DataSet inDataSet = new DataSet();
                inDataSet.Tables.Add(inTable);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROD_LOTID_FOR_PRINT", "INDATA", "OUTDATA", inTable);

                AddLotList(searchResult);


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


        private DataTable GetThermalPaperPrintingInfo(string sLotID)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_THERMAL_PAPER_PRT_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = sLotID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_THERMAL_PAPER_PRT_INFO_PROD_LOTID", "INDATA", "OUTDATA", inTable);

                return dtRslt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
            finally
            {
                //HideLoadingIndicator();
                HiddenLoadingIndicator();
            }
        }

        #endregion
        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //20210713 xaml 파일에서 주석처리 하면서 해당 코드도 주석처리함
            //listAuth.Add(btnSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
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

        private void AddLotList(DataTable dtRslt)
        {
            try
            {
                if (dtRslt == null) return;

                if (dtRslt.Rows.Count < 1)
                {
                    Util.MessageInfo("SFU1386");   // LOT정보가 없습니다.
                    return;
                }

                DataTable dtList = DataTableConverter.Convert(dgGroupLot.ItemsSource);

                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    DataRow dtRow = dtList.NewRow();
                    dtRow["CHK"] = 0;
                    dtRow["PROD_LOTID"] = Util.NVC(dtRslt.Rows[i]["PROD_LOTID"]);
                    dtRow["START_TIME"] = Util.NVC(dtRslt.Rows[i]["START_TIME"]);
                    dtRow["END_TIME"] = Util.NVC(dtRslt.Rows[i]["END_TIME"]);
                    dtRow["EQPTNAME"] = Util.NVC(dtRslt.Rows[i]["EQPTNAME"]);
                    dtRow["PRODID"] = Util.NVC(dtRslt.Rows[i]["PRODID"]);
                    dtRow["WIPSNAME"] = Util.NVC(dtRslt.Rows[i]["WIPSNAME"]);

                    dtList.Rows.Add(dtRow);
                    dtList.AcceptChanges();
                }
                
                Util.GridSetData(dgGroupLot, dtList, FrameOperation, true);

            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
                Util.MessageValidation("SFU1386", result =>
                {

                });

                return;
            }
        }

        #endregion            

        #endregion

        //20210713 발행부분
        private void btnOutPrint_Click(object sender, RoutedEventArgs e)
        {
            {
                if (!CanPrint())
                    return;

                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("인쇄 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU1237", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        for (int i = 0; i < dgGroupLot.Rows.Count - dgGroupLot.BottomRows.Count; i++)
                        {
                            if (!_util.GetDataGridCheckValue(dgGroupLot, "CHK", i)) continue;

                            //위의 조건을 사용하지 않고 그냥 출력
                            BoxIDPrint(Util.NVC(DataTableConverter.GetValue(dgGroupLot.Rows[i].DataItem, "PROD_LOTID")));
                        }
                        
                    }
                });
            }
        }


        private bool CanPrint()
        {
            bool bRet = false;

            if (_util.GetDataGridCheckFirstRowIndex(dgGroupLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            for (int i = 0; i < dgGroupLot.Rows.Count - dgGroupLot.BottomRows.Count; i++)
            {
                if (!_util.GetDataGridCheckValue(dgGroupLot, "CHK", i)) continue;

            }

            bRet = true;

            return bRet;


        }

        // GROUP LOT ID 정보를 조회하는 BIZ RULE 필요함
        private void BoxIDPrint(string sBoxID = "", decimal dQty = 0)
        {
            try
            {
                int iCopys = 2;

                if (LoginInfo.CFG_THERMAL_COPIES > 0)
                {
                    iCopys = LoginInfo.CFG_THERMAL_COPIES;
                }

                btnOutPrint.IsEnabled = false;

                List<Dictionary<string, string>> dicList = new List<Dictionary<string, string>>();

                if (!sBoxID.Equals(""))
                {
                    // 발행..  바로 여기!!
                    DataTable dtRslt = GetThermalPaperPrintingInfo(sBoxID);

                    if (dtRslt == null || dtRslt.Rows.Count < 1)
                        return;

                    Dictionary<string, string> dicParam = new Dictionary<string, string>();

                    //폴딩 - ZZStacking
                    dicParam.Add("reportName", "ZZStacking");
                    dicParam.Add("GROUPLOTID", Util.NVC(dtRslt.Rows[0]["LOTID_RT"]));
                    dicParam.Add("GROUPLOTIDBARCODE", Util.NVC(dtRslt.Rows[0]["LOTID_RT"]));
                    dicParam.Add("CALDATE", Convert.ToDateTime(Util.NVC(dtRslt.Rows[0]["CAL_DATE"])).ToString("yyyy-MM-dd"));  // 폴딩 LOT의 생성시간(공장시간기준)
                    dicParam.Add("MODEL", Util.NVC(dtRslt.Rows[0]["MODLID"]));
                    dicParam.Add("REGDATE", Util.NVC(dtRslt.Rows[0]["NOW_DTTM"]));
                    dicParam.Add("EQPTNAME", Util.NVC(dtRslt.Rows[0]["EQPTSHORTNAME"]));
                    dicParam.Add("TITLEX", "BASKET ID");

                    dicParam.Add("PRINTQTY", iCopys.ToString());  // 발행 수
                    
                    dicParam.Add("RE_PRT_YN", "N"); // 재발행 여부.

                    dicList.Add(dicParam);
                }


                //프린터 양식
                CMM_THERMAL_PRINT_ZZS_GROUPLOT print = new CMM_THERMAL_PRINT_ZZS_GROUPLOT();
                print.FrameOperation = FrameOperation;

                if (print != null)
                {
                    object[] Parameters = new object[6];
                    Parameters[0] = dicList;
                    Parameters[1] = _Procid;                    
                    Parameters[2] = "";
                    Parameters[3] = _EqptID;                    
                    Parameters[4] = "Y";   // 완료 메시지 표시 여부.
                    Parameters[5] = "N";   // 디스패치 처리.

                    C1WindowExtension.SetParameters(print, Parameters);

                    print.Closed += new EventHandler(print_Closed);

                    print.Show();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                btnOutPrint.IsEnabled = true;
            }
        }



        private void print_Closed(object sender, EventArgs e)
        {
            CMM_THERMAL_PRINT_ZZS_GROUPLOT window = sender as CMM_THERMAL_PRINT_ZZS_GROUPLOT;

            if (window.DialogResult == MessageBoxResult.OK)
            {

            }

        }

        private void dgGroupLotChoice_Checked(object sender, RoutedEventArgs e)
        {   
            try
            {
                if (sender == null)
                    return;

                RadioButton rb = sender as RadioButton;

                if (rb.DataContext == null)
                    return;

                if ((bool)rb.IsChecked && ((rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0") || (rb.DataContext as DataRowView).Row["CHK"].ToString().ToUpper().Equals("FALSE")))
                {
                    int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                    DataRow dtRow = (rb.DataContext as DataRowView).Row;

                    for (int i = 0; i < ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                    {
                        if (idx == i)   // Mode = OneWay 이므로 Set 처리.
                            DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
                        else
                            DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
                    }

                    //row 색 바꾸기
                    dgGroupLot.SelectedIndex = idx;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

    }
}
