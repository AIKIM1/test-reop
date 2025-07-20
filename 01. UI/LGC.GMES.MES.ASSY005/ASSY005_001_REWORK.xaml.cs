/*************************************************************************************
 Created Date : 2020.10.27
      Creator : 신광희
   Decription : 재작업대기LOT이동 팝업(ASSY004_002_REWORK Copy 후 작성)
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.27  신광희 : Initial Created.
**************************************************************************************/


using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.ASSY005
{
    /// <summary>
    /// ASSY005_001_REWORK.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY005_001_REWORK : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _ProcID = string.Empty;
        private string _UNLDR_LOT_IDENT_BAS_CODE = string.Empty;
        
        Util _Util = new Util();
        BizDataSet _Biz = new BizDataSet();
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

        public ASSY005_001_REWORK()
        {
            InitializeComponent();
        }

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
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 4)
                {
                    _LineID = Util.NVC(tmps[0]);
                    _EqptID = Util.NVC(tmps[1]);
                    _ProcID = Util.NVC(tmps[2]);
                    _UNLDR_LOT_IDENT_BAS_CODE = Util.NVC(tmps[3]);
                }

                ApplyPermissions();

                DataTable dtTmp = new DataTable();

                dtTmp.Columns.Add("CHK", typeof(string));
                dtTmp.Columns.Add("WIPHOLD", typeof(string));
                dtTmp.Columns.Add("LOTID_RT", typeof(string));
                dtTmp.Columns.Add("LOTID", typeof(string));
                dtTmp.Columns.Add("CSTID", typeof(string));
                dtTmp.Columns.Add("JUDG_VALUE", typeof(string));
                dtTmp.Columns.Add("JUDG_NAME", typeof(string));
                dtTmp.Columns.Add("PROCID", typeof(string));
                dtTmp.Columns.Add("PROCNAME", typeof(string));
                dtTmp.Columns.Add("WIPDTTM_ED", typeof(string));
                dtTmp.Columns.Add("PRJT_NAME", typeof(string));
                dtTmp.Columns.Add("MODLID", typeof(string));
                dtTmp.Columns.Add("PRODID", typeof(string));
                dtTmp.Columns.Add("PRODNAME", typeof(string));
                dtTmp.Columns.Add("LOTDTTM_CR", typeof(string));
                dtTmp.Columns.Add("WIPQTY", typeof(string));
                dtTmp.Columns.Add("EQSGID", typeof(string));
                dtTmp.Columns.Add("EQSGNAME", typeof(string));
                dtTmp.Columns.Add("DATEDIFF", typeof(string));

                Util.GridSetData(dgList, dtTmp, FrameOperation, true);

                this.Loaded -= C1Window_Loaded;

                txtLotCst.Focus();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void txtLotCst_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    string sLot = string.Empty;

                    try
                    {
                        if (sender.GetType() == typeof(string))
                        {
                            sLot = sender.ToString();
                        }
                        else
                        {
                            sLot = txtLotCst.Text.Trim();
                        }

                        if (sLot == "")
                        {
                            //Util.MessageValidation("SFU2060", (action) => { txtLotCst.Focus(); });   //스캔한 데이터가 없습니다.
                            return;
                        }

                        DataTable dt = DataTableConverter.Convert(dgList.ItemsSource);

                        #region [Scan Validation]
                        // 동일 Lot
                        DataRow[] dtRows = dt.Select("LOTID = '" + sLot + "' OR CSTID = '" + sLot + "'");
                        if (dtRows?.Length > 0)
                        {
                            //동일한 LOT[%1]이 스캔되었습니다.
                            Util.MessageValidation("SFU1504", (action) => 
                            {
                                txtLotCst.Text = "";
                                txtLotCst.Focus();
                            });
                            
                            return;
                        }
                        #endregion

                        DataTable dtRlst = GetLotInfo(sLot);

                        if (dtRlst?.Rows?.Count < 1)
                        {
                            // Lot 정보가 없습니다.
                            Util.MessageValidation("SFU1195", (action) => { txtLotCst.Focus(); });
                            return;
                        }

                        foreach (DataRow drTmp in dtRlst.Rows)
                        {
                            dt.ImportRow(drTmp);
                        }
                        
                        Util.GridSetData(dgList, dt, FrameOperation, true);

                        dgList.ScrollIntoView(dt.Rows.Count - 1, 0);

                        txtLotCst.Text = "";
                        txtLotCst.Focus();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex, (result) =>
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;

                            txtLotCst.SelectAll();
                            txtLotCst.Focus();
                        });

                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex, (result) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    txtLotCst.SelectAll();
                    txtLotCst.Focus();
                });
            }
        }
        
        private void btnRework_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanRework())
                    return;

                Util.MessageConfirm("SFU1763", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Save();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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

        private void txtLotCst_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtLotCst == null) return;
                InputMethod.SetPreferredImeConversionMode(txtLotCst, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button bt = sender as Button;

                if (bt == null || bt.DataContext == null) return;

                Util.MessageConfirm("SFU1230", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable dt = DataTableConverter.Convert(dgList.ItemsSource);

                        int index = ((C1.WPF.DataGrid.DataGridCellPresenter)(bt.Parent)).Row.Index;                 
                        if (index < 0) return;
                        dt.Rows.RemoveAt(index);

                        Util.GridSetData(dgList, dt, FrameOperation, true);

                        txtLotCst.Focus();
                    }
                });
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
            listAuth.Add(btnRework);

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

        private bool CanRework()
        {
            bool bRet = false;

            if (dgList.GetRowCount() < 1)
            {
                // Lot 정보가 없습니다.
                Util.MessageValidation("SFU1195");
                return bRet;
            }

            for (int i = 0; i < dgList.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "WIPHOLD")).Equals("Y"))
                {
                    Util.MessageValidation("SFU2888", Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "LOTID")));
                    return bRet;
                }
            }
            
            bRet = true;

            return bRet;
        }

        #endregion

        #region [BizCall]

        private DataTable GetLotInfo(string sLot)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("MODLID", typeof(string));
                inDataTable.Columns.Add("WIPDTTM", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = _LineID;
                newRow["MODLID"] = "";
                newRow["WIPDTTM"] = "";
                newRow["LOTID"] = sLot;

                inDataTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_VD_REWORK_L", "INDATA", "OUTDATA", inDataTable);

                return dtRslt;
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);

                return null;
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        private void Save()
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable dt = ds.Tables.Add("INDATA");
                dt.Columns.Add("SRCTYPE", typeof(string));
                dt.Columns.Add("IFMODE", typeof(string));
                dt.Columns.Add("USERID", typeof(string));
                dt.Columns.Add("PCSGID", typeof(string));
                dt.Columns.Add("WIPNOTE", typeof(string));
                dt.Columns.Add("PROCID_TO", typeof(string));
                dt.Columns.Add("EQSGID_TO", typeof(string));
                dt.Columns.Add("RE_VD_TYPE_CODE", typeof(string));

                DataRow row = dt.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["USERID"] = LoginInfo.USERID;
                row["PCSGID"] = "A"; // 조립
                row["WIPNOTE"] = "";
                row["PROCID_TO"] = Process.VD_LMN;
                row["EQSGID_TO"] = _LineID;
                row["RE_VD_TYPE_CODE"] = "L";

                dt.Rows.Add(row);

                row = null;

                DataTable inLot = ds.Tables.Add("IN_LOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("TREATTYPE", typeof(string));
                inLot.Columns.Add("PROCID", typeof(string));

                for (int i = 0; i < dgList.GetRowCount(); i++)
                {
                    row = inLot.NewRow();
                    row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "LOTID"));
                    row["TREATTYPE"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PROCID")).Equals(Process.LAMINATION) ? "L" : "V";
                    row["PROCID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PROCID"));

                    inLot.Rows.Add(row);
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MOVE_LOT_WAIT_FOR_RW_L", "INDATA,IN_LOT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        Util.gridClear(dgList);
                        Util.MessageInfo("SFU3073");
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                }, ds);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HideLoadingIndicator();
            }
        }

        #endregion

        #endregion


    }
}
