/*************************************************************************************
 Created Date : 2017.10.23
      Creator : CNS 고현영S
   Decription : 전지 5MEGA-GMES 구축 - C 생산 관리 화면
--------------------------------------------------------------------------------------
 [Change History]
  2017.10.23  CNS 고현영S : Initial Created.
**************************************************************************************/


using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_014_BOX_CREATE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_014_MAG_CREATE : C1Window, IWorkArea
    {
        public ASSY003_014_MAG_CREATE()
        {
            InitializeComponent();
        }

        #region Declaration & Constructor 
        private string _EqptId = string.Empty;
        private string _EqptName = string.Empty;
        private string _CellType = string.Empty;
        private string _ProdId = string.Empty;
        private string _PrjtName = string.Empty;
        private string _AreaId = string.Empty;
        //private string _Floor = string.Empty;
        private string _MktTypeCode = string.Empty;
        private string _MktTypeName = string.Empty;
        private string _workerName = string.Empty;
        private string _workerId = string.Empty;
        private string _CprodEqsgid = string.Empty;
        private string _CprodEqptid = string.Empty;

        private int _WipQty = 0;

        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

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

        private void InitTextBox()
        {
            tbWrkPstName.Text = _EqptName;
            tbCellType.Text = _CellType;
            //tbPjtName.Text = _PrjtName;
            tbWipQty.Text = _WipQty.ToString();
            tbProdID.Text = _ProdId;
            tbMktType.Text = _MktTypeName;
            tbGapQty.Text = _WipQty.ToString();
            tbWorker.Text = _workerName;
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

                if (tmps != null && tmps.Length > 12)
                {
                    _EqptId = Util.NVC(tmps[0]);
                    _EqptName = Util.NVC(tmps[1]);
                    _AreaId = Util.NVC(tmps[2]);
                    //_Floor = Util.NVC(tmps[3]);
                    _CellType = Util.NVC(tmps[3]);
                    _ProdId = Util.NVC(tmps[4]);
                    _PrjtName = Util.NVC(tmps[5]);
                    _MktTypeCode = Util.NVC(tmps[6]);
                    _MktTypeName = Util.NVC(tmps[7]);
                    _WipQty = Util.NVC_Int(tmps[8].ToString());
                    _workerId = Util.NVC(tmps[9].ToString());
                    _workerName = Util.NVC(tmps[10].ToString());
                    _CprodEqsgid = Util.NVC(tmps[11].ToString());
                    _CprodEqptid = Util.NVC(tmps[12].ToString());
                }
                else
                {
                    _EqptId = "";
                    _EqptName = "";
                    _AreaId = "";
                    //_Floor = "";
                    _CellType = "";
                    _ProdId = "";
                    _PrjtName = "";
                    _MktTypeCode = "";
                    _MktTypeName = "";
                    _WipQty = 0;
                    _workerId = "";
                    _workerName = "";
                    _CprodEqsgid = "";
                    _CprodEqptid = "";
                }

                InitTextBox();

                InitCombo();
                      
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
      
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            
            String[] sFilter = { LoginInfo.CFG_AREA_ID, null, Process.STACKING_FOLDING };
            _combo.SetCombo(cboTransferLine, CommonCombo.ComboStatus.SELECT, sFilter: sFilter, sCase: "cboEquipmentSegmentAssy");
        }
        
        
        private void btnRealse_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanRealse())
                {
                    MagRelease();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        
        #endregion

        #region Method

        #region [BizCall]
        
        private void MagRelease()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inTable = indataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("TO_EQSGID", typeof(string));
                inTable.Columns.Add("TO_PROCID", typeof(string));
                inTable.Columns.Add("MOVE_USERID", typeof(string));
                inTable.Columns.Add("NOTE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable in_lotTable = indataSet.Tables.Add("IN_LOT");
                in_lotTable.Columns.Add("PRODID", typeof(string));
                in_lotTable.Columns.Add("MKT_TYPE_CODE", typeof(string));
                in_lotTable.Columns.Add("CSTID", typeof(string));
                in_lotTable.Columns.Add("WIPQTY", typeof(decimal));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _CprodEqptid;
                newRow["PROCID"] = Process.CPROD;
                newRow["TO_EQSGID"] = Util.NVC(cboTransferLine.SelectedValue);
                newRow["TO_PROCID"] = Process.STACKING_FOLDING;
                newRow["MOVE_USERID"] = Util.NVC(_workerId); ;
                newRow["NOTE"] = "";
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                for (int i = 0; i < dgList.Rows.Count - dgList.BottomRows.Count; i++)
                {
                    newRow = in_lotTable.NewRow();
                    newRow["CSTID"] = "";
                    newRow["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PRODID"));
                    newRow["MKT_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "MKTYPE"));
                    newRow["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0 : decimal.Parse(Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "WIPQTY")));

                    in_lotTable.Rows.Add(newRow);
                }
                

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SEND_CPROD_CELL_WIP", "INDATA,IN_LOT", "OUTDATA", (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (searchResult != null && searchResult.Tables.Contains("OUTDATA") && searchResult.Tables["OUTDATA"].Rows.Count > 0)
                        {
                            // 매거진 감열지 발행.

                            int iCopys = 1;

                            if (LoginInfo.CFG_THERMAL_COPIES > 0)
                            {
                                iCopys = LoginInfo.CFG_THERMAL_COPIES;
                            }

                            List<Dictionary<string, string>> dicList = new List<Dictionary<string, string>>();


                            for (int i = 0; i < searchResult.Tables["OUTDATA"].Rows.Count; i++)
                            {

                                DataTable dtRslt = GetThermalPaperPrintingInfo(Util.NVC(searchResult.Tables["OUTDATA"].Rows[i]["LOTID"]));

                                if (dtRslt == null || dtRslt.Rows.Count < 1) continue;


                                Dictionary<string, string> dicParam = new Dictionary<string, string>();

                                //라미
                                dicParam.Add("reportName", "Lami"); //dicParam.Add("reportName", "Fold");
                                dicParam.Add("LOTID", Util.NVC(dtRslt.Rows[0]["LOTID_RT"]));
                                dicParam.Add("QTY", Convert.ToDouble(Util.NVC(dtRslt.Rows[0]["WIPQTY"])).ToString());
                                dicParam.Add("MAGID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                                dicParam.Add("MAGIDBARCODE", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                                dicParam.Add("CASTID", Util.NVC(dtRslt.Rows[0]["CSTID"])); // 카세트 ID 컬럼은??
                                dicParam.Add("MODEL", Util.NVC(dtRslt.Rows[0]["MODLID"]));
                                dicParam.Add("REGDATE", Util.NVC(dtRslt.Rows[0]["LOTDTTM_CR"]));
                                dicParam.Add("EQPTNO", Util.NVC(dtRslt.Rows[0]["EQPTSHORTNAME"]));
                                dicParam.Add("CELLTYPE", Util.NVC(dtRslt.Rows[0]["PRODUCT_LEVEL3_CODE"]));
                                dicParam.Add("TITLEX", "MAGAZINE ID");

                                dicParam.Add("PRINTQTY", iCopys.ToString());  // 발행 수

                                dicParam.Add("B_LOTID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                                dicParam.Add("B_WIPSEQ", Util.NVC(dtRslt.Rows[0]["WIPSEQ"]));

                                dicParam.Add("RE_PRT_YN", "N"); // 재발행 여부.

                                if (LoginInfo.CFG_SHOP_ID.Equals("G182"))
                                {
                                    //dicParam.Add("MKT_TYPE_CODE", Util.NVC(DataTableConverter.GetValue(winWorkOrder.dgWorkOrder.Rows[_Util.GetDataGridCheckFirstRowIndex(winWorkOrder.dgWorkOrder, "CHK")].DataItem, "MKT_TYPE_CODE")));
                                }

                                dicList.Add(dicParam);
                            }

                                CMM_THERMAL_PRINT_LAMI print = new CMM_THERMAL_PRINT_LAMI();
                                print.FrameOperation = FrameOperation;

                                if (print != null)
                                {
                                    object[] Parameters = new object[7];
                                    Parameters[0] = dicList;
                                    Parameters[1] = Process.LAMINATION;
                                    Parameters[2] = cboTransferLine.SelectedValue.ToString();
                                    Parameters[3] = "";// cboEquipment.SelectedValue.ToString();
                                    Parameters[4] = "N";   // 완료 메시지 표시 여부.
                                    Parameters[5] = "N";   // 디스패치 처리.
                                    Parameters[6] = "MAGAZINE";   // 발행 Type M:Magazine, B:Folded Box, R:Remain Pancake, N:매거진재구성(Folding공정)

                                    C1WindowExtension.SetParameters(print, Parameters);

                                    print.Closed += new EventHandler(print_Closed);

                                    print.Show();
                                }                            
                        }

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                        //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", Logger.MESSAGE_OPERATION_END);
                    }
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void print_Closed(object sender, EventArgs e)
        {
            CMM_THERMAL_PRINT_LAMI window = sender as CMM_THERMAL_PRINT_LAMI;

            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private DataTable GetThermalPaperPrintingInfo(string sLotID)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_THERMAL_PAPER_PRT_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["LOTID"] = sLotID;

                inTable.Rows.Add(newRow);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_THERMAL_PAPER_PRT_INFO_CPRD_LAMI", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region [Validation]

        private bool CanRealse()
        {
            bool bRet = true;

            if (cboTransferLine.SelectedIndex <= 0)
            {
                //Util.Alert("라인을 선택하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (txtOutQty.Text.Equals("") || decimal.Parse(txtOutQty.Text) < 1)
            {
                Util.MessageValidation("SFU1684");
                return bRet;
            }

            if (dgList.Rows.Count - dgList.BottomRows.Count < 1)
            {
                Util.MessageValidation("SFU1886");
                return bRet;
            }

            //int sum = DataTableConverter.Convert(dgMagList.ItemsSource).AsEnumerable().Sum(row => Util.NVC_Int(row.Field<object>("WIPQTY")));
            //if (sum != Util.NVC_Int(tbWipQty.Text))
            //{
            //    //Util.Alert("전체수량과 입력수량의 합이 일치하지 않습니다.");
            //    Util.MessageValidation("SFU4224");
            //    return bRet;
            //}

            bRet = false;
            return bRet;
        }
        
        #endregion

        #region [Func]
      
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnRealse);
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
        
        #endregion

        #endregion

        private void txtOutQty_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtOutQty.Text, 1))
                {
                    txtOutQty.Text = "";                    
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnAdd_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Util.NVC(txtOutQty.Text).Trim().Equals("") || Util.NVC(txtOutQty.Text).Trim().Equals("0"))
                {
                    Util.MessageValidation("SFU1684"); // 수량을 입력 하세요.
                    return;
                }

                if (dgList.ItemsSource == null)
                {
                    DataTable dtTmp = new DataTable();
                    dtTmp.Columns.Add("ROWNUM", typeof(string));
                    dtTmp.Columns.Add("CELLTYPE", typeof(string));
                    dtTmp.Columns.Add("PRODID", typeof(string));
                    dtTmp.Columns.Add("MKTYPE", typeof(string));
                    dtTmp.Columns.Add("WIPQTY", typeof(decimal));

                    DataRow dtRow = dtTmp.NewRow();
                    dtRow["ROWNUM"] = "1";
                    dtRow["CELLTYPE"] = tbCellType.Text;
                    dtRow["PRODID"] = tbProdID.Text;
                    dtRow["MKTYPE"] = _MktTypeCode;
                    dtRow["WIPQTY"] = decimal.Parse(Util.NVC(txtOutQty.Text));

                    dtTmp.Rows.Add(dtRow);

                    Util.GridSetData(dgList, dtTmp, FrameOperation, false);
                }
                else
                {
                    DataTable dtTmp = DataTableConverter.Convert(dgList.ItemsSource);
                    
                    DataRow dtRow = dtTmp.NewRow();
                    dtRow["ROWNUM"] = (dtTmp.Rows.Count + 1).ToString();
                    dtRow["CELLTYPE"] = tbCellType.Text;
                    dtRow["PRODID"] = tbProdID.Text;
                    dtRow["MKTYPE"] = _MktTypeCode;
                    dtRow["WIPQTY"] = decimal.Parse(Util.NVC(txtOutQty.Text));

                    dtTmp.Rows.Add(dtRow);

                    Util.GridSetData(dgList, dtTmp, FrameOperation, false);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnRemove_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgList.ItemsSource != null)
                {
                    DataTable dtTmp = DataTableConverter.Convert(dgList.ItemsSource);

                    if (dtTmp.Rows.Count > 0)
                    {
                        dtTmp.Rows.RemoveAt(dtTmp.Rows.Count - 1);

                        Util.GridSetData(dgList, dtTmp, FrameOperation, false);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }

}

