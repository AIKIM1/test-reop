/*************************************************************************************
 Created Date : 2021.12.25
      Creator : INS 오화백K
   Decription : STK 재작업 - 대기LOT조회 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2021.12.25  INS 오화백K : Initial Created.
  
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
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001.Extensions;
using Microsoft.Win32;
using System.Configuration;
using System.IO;
using System.Linq;


namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY004_061_WAITLOT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_061_WAITLOT : C1Window, IWorkArea
    {        
        #region Declaration & Constructor 
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();
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
        public ASSY004_061_WAITLOT()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 2)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
            }
            else
            {
                _LineID = "";
                _EqptID = "";
            }
            ApplyPermissions();

            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID, Process.STACKING_FOLDING, EquipmentGroup.STACKING };
            C1ComboBox[] cboLineChild = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter, sCase: "EQUIPMENTSEGMENT_BY_EQGRID");

            String[] sFilter2 = { Process.STACKING_FOLDING, EquipmentGroup.STACKING };
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent, sFilter: sFilter2, sCase: "EQUIPMENT_BY_EQSGID_NEW");

            GetWaitLot();
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {
            
        }
  

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetWaitLot();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void btnMove_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationProcessMove()) return;
            // 이동 하시겠습니까?
            Util.MessageConfirm("SFU1763", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        ShowLoadingIndicator();

                        DataSet inDataSet = new DataSet();

                        DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inDataTable.Columns.Add("IFMODE", typeof(string));
                        inDataTable.Columns.Add("PROCID", typeof(string));
                        inDataTable.Columns.Add("EQPTID", typeof(string));
                        inDataTable.Columns.Add("USERID", typeof(string));
                        inDataTable.Columns.Add("PROCID_TO", typeof(string));
                        inDataTable.Columns.Add("EQSGID_TO", typeof(string));
                        inDataTable.Columns.Add("FLOWNORM", typeof(string));

                        DataTable inLot = inDataSet.Tables.Add("INLOT");
                        inLot.Columns.Add("LOTID", typeof(string));
                        inLot.Columns.Add("WIPNOTE", typeof(string));

                        DataRow inRow = inDataTable.NewRow();
                        inRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        inRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        inRow["PROCID"] = Process.RWK_LNS;
                        inRow["EQPTID"] = _EqptID;
                        inRow["USERID"] = LoginInfo.USERID;

                        inRow["PROCID_TO"] = Process.STACKING_FOLDING;
                        inRow["EQSGID_TO"] = cboEquipmentSegment.SelectedValue.ToString();
                        inRow["FLOWNORM"] = "Y";

                        inDataTable.Rows.Add(inRow);
                        inRow = null;

                        for (int i = 0; i < dgWaitLot.Rows.Count; i++)
                        {
                            if (!_Util.GetDataGridCheckValue(dgWaitLot, "CHK", i)) continue;

                            inRow = inLot.NewRow();
                            inRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitLot.Rows[i].DataItem, "LOTID"));
                            inRow["WIPNOTE"] = string.Empty;

                            inLot.Rows.Add(inRow);
                        }

                        if (inLot.Rows.Count < 1)
                            return;

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_LOCATE_LOT_S", "INDATA,INLOT", null, (bizResult, searchException) =>
                        {
                            try
                            {
                                if (searchException != null)
                                {
                                    Util.MessageException(searchException);
                                    return;
                                }

                                //Util.AlertInfo("정상 처리 되었습니다.");
                                Util.MessageInfo("SFU1275");
                                GetWaitLot();

                              
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                HiddenLoadingIndicator();
                            }
                        }, inDataSet);
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
            });
        }


        private void txtWaitPancakeLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetWaitLot();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Mehod

        #region [BizCall]
        private void GetWaitLot()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
            

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.RWK_LNS;
                newRow["EQPTID"] = _EqptID;
                newRow["EQSGID"] = _LineID;

                newRow["LOTID"] = txtWaitPancakeLot == null ? "" : txtWaitPancakeLot.Text;
                newRow["CSTID"] = txtWaitPancakeLot == null ? "" : txtWaitPancakeLot.Text;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WAIT_LOT_LIST_BY_LINE_STKREWORK", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgWaitLot.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgWaitLot, searchResult, null, true);

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
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btn);

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
        private bool ValidationProcessMove()
        {
            if (dgWaitLot.Rows.Count == 0)
            {
                Util.MessageValidation("SFU3537"); // 조회된 데이터가 없습니다
                return false;
            }

            DataRow[] drInfo = Util.gridGetChecked(ref dgWaitLot, "CHK");

            if (drInfo.Count() <= 0)
            {
                Util.MessageValidation("10008");//선택된 데이터가 없습니다.
            }

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                // 라인을 선택 하세요.
                Util.MessageValidation("SFU1223");
                return false;
            }

          
            return true;
        }
        #endregion

        #endregion











    }
}
