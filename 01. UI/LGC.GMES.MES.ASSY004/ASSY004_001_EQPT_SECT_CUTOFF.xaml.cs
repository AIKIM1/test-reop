/*************************************************************************************
 Created Date : 2020.01.20
      Creator : INS 김동일K
   Decription : 노칭 설비 구간별 파단 횟수 관리 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2020.01.20  INS 김동일K : Initial Created.

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

namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY004_001_EQPT_SECT_CUTOFF.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_001_EQPT_SECT_CUTOFF : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _EqptID = string.Empty;
        private string _LotID = string.Empty;
        private string _Wipseq = string.Empty;
        private string _EqsgID = string.Empty;
        private string _EqsgName = string.Empty;
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

        public ASSY004_001_EQPT_SECT_CUTOFF()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
                _EqptID = tmps.Length > 0 ? Util.NVC(tmps[0]) : "";
                _LotID = tmps.Length > 1 ? Util.NVC(tmps[1]) : "";
                _Wipseq = tmps.Length > 2 ? Util.NVC(tmps[2]) : "1";
                _EqsgID = tmps.Length > 3 ? Util.NVC(tmps[3]) : "";
                _EqsgName = tmps.Length > 4 ? Util.NVC(tmps[4]) : "";

                //dgList.Columns[0].Header = new List<string> { _EqsgName, "NO." };
                //dgList.Columns[1].Header = new List<string> { _EqsgName, "SECTION" };
                //dgList.Columns[2].Header = new List<string> { _EqsgName, "SECTION" };
                //dgList.Columns[3].Header = new List<string> { _EqsgName, "NTC_STANDARD" };
                //dgList.Columns[4].Header = new List<string> { _EqsgName, "COUNT" };
                //dgList.Columns[5].Header = new List<string> { _EqsgName, "COUNT" };
                //dgList.Columns[6].Header = new List<string> { _EqsgName, "TIMES" };

                dgList.UpdateLayout();

                GetEqptSectCutOffList();

                ApplyPermissions();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnPlus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button bt = sender as Button;

                if (bt == null || bt.DataContext == null) return;

                int iTimes = 0;

                int.TryParse(Util.NVC(DataTableConverter.GetValue(bt.DataContext, "OCCR_COUNT")), out iTimes);
                
                DataTableConverter.SetValue(bt.DataContext, "OCCR_COUNT", ++iTimes);


                //파단수량 및 전체 수량을 다시 계산함
                DataTableConverter.SetValue(bt.DataContext, "CUT_EA", Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(bt.DataContext, "SECTION_EA"))) * Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(bt.DataContext, "OCCR_COUNT"))));

                DataTable dtList = DataTableConverter.Convert(dgList.ItemsSource);
                Util.GridSetData(dgList, dtList, null, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnMinus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button bt = sender as Button;

                if (bt == null || bt.DataContext == null) return;

                int iTimes = 0;

                int.TryParse(Util.NVC(DataTableConverter.GetValue(bt.DataContext, "OCCR_COUNT")), out iTimes);

                DataTableConverter.SetValue(bt.DataContext, "OCCR_COUNT", iTimes < 1 ? 0 : --iTimes);

                //파단수량 및 전체 수량을 다시 계산함
                DataTableConverter.SetValue(bt.DataContext, "CUT_EA", Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(bt.DataContext, "SECTION_EA"))) * Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(bt.DataContext, "OCCR_COUNT"))));

                DataTable dtList = DataTableConverter.Convert(dgList.ItemsSource);
                Util.GridSetData(dgList, dtList, null, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSave_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("SECTION_ID", typeof(string));
                inDataTable.Columns.Add("OCCR_COUNT", typeof(decimal));
                inDataTable.Columns.Add("USERID", typeof(string));
                
                for (int i = dgList.TopRows.Count; i < dgList.Rows.Count - dgList.BottomRows.Count; i++)
                {
                    DataRow newRow = inDataTable.NewRow();

                    decimal dCnt = 0;
                    decimal.TryParse(Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "OCCR_COUNT")), out dCnt);

                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    newRow["EQPTID"] = _EqptID;
                    newRow["LOTID"] = _LotID;
                    newRow["SECTION_ID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "SECTION_ID"));
                    newRow["OCCR_COUNT"] = dCnt;
                    newRow["USERID"] = LoginInfo.USERID;

                    inDataTable.Rows.Add(newRow);
                }

                if (inDataTable.Rows.Count < 1)
                {
                    HideLoadingIndicator();
                    return;
                }

                new ClientProxy().ExecuteService("BR_PRD_REG_SECTION_CUTOFF", "INDATA", null, inDataTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");

                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

                HideLoadingIndicator();
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = MessageBoxResult.Cancel;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        #endregion

        #region Mehod
        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HideLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

      


        #endregion

        #region [BizCall]
        private void GetEqptSectCutOffList()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                //inTable.Columns.Add("WIPSEQ", typeof(string));
                
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _EqptID;
                newRow["LOTID"] = _LotID;
                //newRow["WIPSEQ"] = _Wipseq;
               
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_TB_SFC_EQPT_SECTION_CUTOFF", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //2020-01-25 OHB 파단수 계산
                        if (searchResult.Rows.Count > 0)
                        {
                            searchResult.Columns.Add(new DataColumn("CUT_EA", typeof(decimal)));

                            for (int i = 0; i < searchResult.Rows.Count; i++)
                            {
                                searchResult.Rows[i]["CUT_EA"] = Convert.ToDecimal(searchResult.Rows[i]["SECTION_EA"]) * Convert.ToDecimal(searchResult.Rows[i]["OCCR_COUNT"]);
                            }

                        }
                        Util.GridSetData(dgList, searchResult, null, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #endregion

        
    }
}
