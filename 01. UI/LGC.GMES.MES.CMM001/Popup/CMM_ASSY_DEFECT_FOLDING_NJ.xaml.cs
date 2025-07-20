/*************************************************************************************
 Created Date : 2018.01.15
      Creator : Lee. D. R
   Decription : 전지 5MEGA-GMES 구축 - 조립 공정진척 화면 - 불량정보 팝업 (남경)
--------------------------------------------------------------------------------------
 [Change History]
  2018.01.15  Lee. D. R  :   Initial Created.


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
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ASSY_DEFECT_FOLDING_NJ.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_DEFECT_FOLDING_NJ : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _LotID = string.Empty;
        private string _WipSeq = string.Empty;
        private string _ProcID = string.Empty;
        private string _StackingYN = string.Empty;
        private string _WoDetlID = string.Empty;
                
        private string _EQPT_DFCT_APPLY_FLAG = string.Empty;

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();

        private int iCnt = 0;
        private DataTable dtBOM = new DataTable();
        private DataTable dtBOM_CHK = new DataTable();
        private int iBomcnt = 0;
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

        public CMM_ASSY_DEFECT_FOLDING_NJ()
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

                if (tmps != null && tmps.Length >= 7)
                {
                    _LineID = Util.NVC(tmps[0]);
                    _EqptID = Util.NVC(tmps[1]);
                    _ProcID = Util.NVC(tmps[2]);
                    _LotID = Util.NVC(tmps[3]);
                    _WipSeq = Util.NVC(tmps[4]);
                    _StackingYN = Util.NVC(tmps[5]);
                    _WoDetlID = Util.NVC(tmps[6]);
                }
                else
                {
                    _LineID = "";
                    _EqptID = "";
                    _ProcID = "";
                    _LotID = "";
                    _WipSeq = "";
                    _StackingYN = "";
                    _WoDetlID = "";
                }

                ApplyPermissions();                                
                GetMBOMInfo();
                GetAllData();
                txtLotID.Text = _LotID;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDefectSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSaveDefect())
                return;

            //불량정보를 저장하시겠습니까?
            Util.MessageConfirm("SFU1587", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetDefect();
                }
            });
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void dgDefect_NJ_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                int iChk = 1;
                int iTot = 0;
                double Min = 0;
                double Max = 0;
                List<double> dIndex = new List<double>();

                string sColname = string.Empty;
                string sCol = string.Empty;
                string sNo = string.Empty;

                sColname = e.Cell.Column.Name;

                sCol = sColname.Substring(0, 3);
                sNo = sColname.Substring(3, 1);

                int iColIdx = 0;
                int iRowIdx = 0;

                iColIdx = dgDefect_NJ.Columns[sColname].Index;
                iRowIdx = e.Cell.Row.Index;

                if (e.Cell.Column.Name.Equals("INPUT_FC"))
                {
                    if (iBomcnt == 1)
                    {
                        //for (int i = 0; i < iCnt + 1; i++)
                        //{
                            int ichk = Get_Type_Chk(Util.NVC(dtBOM_CHK.Rows[0]["PRODUCT_LEVEL3_CODE"]));

                            string sTemp = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[e.Cell.Row.Index].DataItem, "REG" + ichk.ToString()));

                            double dTemp = double.Parse(sTemp);

                            if (dTemp == 0)
                            {
                                iChk = 0;
                            }

                            double dInputCnt = Convert.ToDouble(Util.NVC(dtBOM_CHK.Rows[0]["PROC_INPUT_CNT"]));

                            double Division = Math.Floor(dTemp / dInputCnt);
                            dIndex.Add(Division);

                            iTot = iTot + 1;
                            //iFcChk = 0;
                        //}
                    }
                    else
                    {
                        for (int i = 0; i < iCnt + 1; i++)
                        {
                            int ichk = Get_Type_Chk(Util.NVC(dtBOM.Rows[i]["PRODUCT_LEVEL3_CODE"]));

                            string sTemp = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[e.Cell.Row.Index].DataItem, "REG" + ichk.ToString()));

                            double dTemp = double.Parse(sTemp);

                            if (dTemp == 0)
                            {
                                iChk = 0;
                            }

                            double dInputCnt = Convert.ToDouble(Util.NVC(dtBOM.Rows[i]["PROC_INPUT_CNT"]));

                            double Division = Math.Floor(dTemp / dInputCnt);
                            dIndex.Add(Division);

                            iTot = iTot + 1;
                            //iFcChk = 0;
                        }
                    }
                }
                else
                {
                    if (iBomcnt == 1)
                    {
                        //for (int i = 0; i < iCnt + 1; i++)
                        //{
                            int ichk = Get_Type_Chk(Util.NVC(dtBOM_CHK.Rows[0]["PRODUCT_LEVEL3_CODE"]));

                            string sTemp = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[e.Cell.Row.Index].DataItem, sCol + ichk.ToString()));

                            double dTemp = double.Parse(sTemp);

                            if (dTemp == 0)
                            {
                                iChk = 0;
                            }

                            double dInputCnt = Convert.ToDouble(Util.NVC(dtBOM_CHK.Rows[0]["PROC_INPUT_CNT"]));

                            double Division = Math.Floor(dTemp / dInputCnt);
                            dIndex.Add(Division);

                            iTot = iTot + 1;
                        //}
                    }
                    else
                    {
                        for (int i = 0; i < iCnt + 1; i++)
                        {
                            int ichk = Get_Type_Chk(Util.NVC(dtBOM.Rows[i]["PRODUCT_LEVEL3_CODE"]));

                            string sTemp = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[e.Cell.Row.Index].DataItem, sCol + ichk.ToString()));

                            double dTemp = double.Parse(sTemp);

                            if (dTemp == 0)
                            {
                                iChk = 0;
                            }

                            double dInputCnt = Convert.ToDouble(Util.NVC(dtBOM.Rows[i]["PROC_INPUT_CNT"]));

                            double Division = Math.Floor(dTemp / dInputCnt);
                            dIndex.Add(Division);

                            iTot = iTot + 1;
                        }
                    }
                }

                Max = dIndex[0];
                Min = dIndex[0];

                for (int icnt = 0; icnt < iTot; icnt++)
                {
                    if (Max < dIndex[icnt])
                    {
                        Max = dIndex[icnt];
                    }

                    if (Min > dIndex[icnt])
                    {
                        Min = dIndex[icnt];
                    }
                }


                if (e.Cell.Column.Name.Equals("INPUT_FC"))
                {
                    string sReg_FC = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[e.Cell.Row.Index].DataItem, "INPUT_FC"));
                    double Reg_Resnqty = double.Parse(sReg_FC);

                    //if (iChk == 0 && iFcChk == 0)
                    if (iChk == 0)
                    {
                        DataTableConverter.SetValue(dgDefect_NJ.Rows[e.Cell.Row.Index].DataItem, "CALC_FC", Reg_Resnqty);
                    }
                    else
                    {
                        DataTableConverter.SetValue(dgDefect_NJ.Rows[e.Cell.Row.Index].DataItem, "CALC_FC", Reg_Resnqty + Min);
                    }
                }
                else
                {
                    if (iChk != 0)
                    {
                        if (iBomcnt == 1)
                        {
                            //for (int i = 0; i < iCnt + 1; i++)
                            //{
                                int ichk = Get_Type_Chk(Util.NVC(dtBOM_CHK.Rows[0]["PRODUCT_LEVEL3_CODE"]));

                                string sTemp = String.Empty;

                                sTemp = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[e.Cell.Row.Index].DataItem, sCol + ichk.ToString()));

                                double dTemp = double.Parse(sTemp);

                                if (dTemp == 0)
                                {
                                    iChk = 0;
                                }

                                double dInputCnt = Convert.ToDouble(Util.NVC(dtBOM_CHK.Rows[0]["PROC_INPUT_CNT"]));

                                double Division = Math.Floor(dTemp / dInputCnt);
                                dIndex.Add(Division);

                                iTot = iTot + 1;

                                if (e.Cell.Column.Name.Equals("INPUT_FC"))
                                    DataTableConverter.SetValue(dgDefect_NJ.Rows[e.Cell.Row.Index].DataItem, "CALC_FC", dTemp - (Min * dInputCnt));
                                //else
                                //    DataTableConverter.SetValue(dgDefect_NJ.Rows[e.Cell.Row.Index].DataItem, sCol + ichk.ToString(), dTemp - (Min * dInputCnt));
                            //}
                        }
                        else
                        {
                            for (int i = 0; i < iCnt + 1; i++)
                            {
                                int ichk = Get_Type_Chk(Util.NVC(dtBOM.Rows[i]["PRODUCT_LEVEL3_CODE"]));

                                string sTemp = String.Empty;

                                sTemp = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[e.Cell.Row.Index].DataItem, sCol + ichk.ToString()));

                                double dTemp = double.Parse(sTemp);

                                if (dTemp == 0)
                                {
                                    iChk = 0;
                                }

                                double dInputCnt = Convert.ToDouble(Util.NVC(dtBOM.Rows[i]["PROC_INPUT_CNT"]));

                                double Division = Math.Floor(dTemp / dInputCnt);
                                dIndex.Add(Division);

                                iTot = iTot + 1;

                                if (e.Cell.Column.Name.Equals("INPUT_FC"))
                                    DataTableConverter.SetValue(dgDefect_NJ.Rows[e.Cell.Row.Index].DataItem, "CALC_FC", dTemp - (Min * dInputCnt));
                                //else
                                //    DataTableConverter.SetValue(dgDefect_NJ.Rows[e.Cell.Row.Index].DataItem, sCol + ichk.ToString(), dTemp - (Min * dInputCnt));
                            }
                        }
                    }
                        string sReg_FC = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[e.Cell.Row.Index].DataItem, "INPUT_FC"));
                        double Reg_Resnqty = double.Parse(sReg_FC);
                        DataTableConverter.SetValue(dgDefect_NJ.Rows[e.Cell.Row.Index].DataItem, "CALC_FC", Reg_Resnqty + Min);
                    //}
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private int Get_Type_Chk(string sType)
        {
            int iCode = 0;

            if (_ProcID.Equals(Process.STACKING_FOLDING))
            {
                if (sType == "AM")
                {
                    iCode = 0;
                }
                else if (sType == "AT")
                {
                    iCode = 1;
                }
                else if (sType == "CM")
                {
                    iCode = 2;
                }
                else if (sType == "CT")
                {
                    iCode = 3;
                }
                else if (sType == "LM")
                {
                    iCode = 4;
                }
                else if (sType == "LT")
                {
                    iCode = 5;
                }
                else if (sType == "MA")
                {
                    iCode = 6;
                }
                else if (sType == "MB")
                {
                    iCode = 7;
                }
                else if (sType == "ML")
                {
                    iCode = 8;
                }
                else if (sType == "MM")
                {
                    iCode = 9;
                }
                else if (sType == "MR")
                {
                    iCode = 10;
                }
                else if (sType == "MT")
                {
                    iCode = 11;
                }
                else if (sType == "RM")
                {
                    iCode = 12;
                }
                else if (sType == "RT")
                {
                    iCode = 13;
                }
            }
            else if (_ProcID.Equals(Process.STP))
            {
                if (sType == "SB") //SRC Mono-Type Cell
                {
                    iCode = 0;
                }
                else if (sType == "SH") //SRC HALF-Type Cell
                {
                    iCode = 1;
                }
                else if (sType == "SM") //SRC Mono-Middle-Type Cell
                {
                    iCode = 2;
                }
                else if (sType == "ST") //SRC Mono-Top-Type Cell
                {
                    iCode = 3;
                }
            }

            return iCode;
        }
        #endregion

        #region Mehod

        #region [BizCall]
        private void GetMBOMInfo()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("WO_DETL_ID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["WO_DETL_ID"] = _WoDetlID;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                if (_ProcID.Equals(Process.STACKING_FOLDING))
                    newRow["CMCDTYPE"] = "NJ_FOLDING_TYPE";
                else
                    newRow["CMCDTYPE"] = "NJ_STP_TYPE";

                inTable.Rows.Add(newRow);
                
                dtBOM_CHK = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MBOM_INFO_STP", "RQSTDT", "RSLTDT", inTable);

                iBomcnt = dtBOM_CHK.Rows.Count;

                new ClientProxy().ExecuteService("DA_PRD_SEL_MBOM_INFO_S", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (searchResult == null || searchResult.Rows.Count < 1)
                        {
                            Util.MessageValidation("SFU1941");      //타입별 불량 기준정보가 존재하지 않습니다.
                            return;
                        }

                        //dtBOM = searchResult.Copy();          

                        //if (iBomcnt == 1)
                        if (_ProcID.Equals(Process.STP))
                        {
                            InitDefectDataGrid();

                            dtBOM = dtBOM_CHK.Copy();

                            for (int i = 0; i < dtBOM_CHK.Rows.Count; i++)
                            {
                                string sColName = string.Empty;
                                string sColName2 = string.Empty;
                                string sHeader = string.Empty;

                                List<string> dIndex = new List<string>();
                                List<string> dIndex2 = new List<string>();

                                int ichk = 0;

                                ichk = Get_Type_Chk(Util.NVC(dtBOM_CHK.Rows[i]["PRODUCT_LEVEL3_CODE"]));

                                sColName = "REG" + (ichk).ToString();
                                sHeader = Util.NVC(dtBOM_CHK.Rows[i]["ATTRIBUTE3"]).ToString();

                                // 불량 수량 컬럼 위치 변경.
                                int iColIdx = 0;

                                dIndex.Add(ObjectDic.Instance.GetObjectName("입력"));
                                dIndex.Add(sHeader);

                                iColIdx = dgDefect_NJ.Columns["INPUT_FC"].Index;

                                Util.SetGridColumnNumeric(dgDefect_NJ, sColName, dIndex, sHeader, true, true, true, false, -1, HorizontalAlignment.Right, Visibility.Visible, iColIdx, "#,##0");  // [입력, FOLDED CELL]     

                                if (dgDefect_NJ.Columns.Contains(sColName))
                                {
                                    (dgDefect_NJ.Columns[sColName] as C1.WPF.DataGrid.DataGridNumericColumn).Minimum = 0;
                                    (dgDefect_NJ.Columns[sColName] as C1.WPF.DataGrid.DataGridNumericColumn).Maximum = 2147483647; // int max : 2147483647;
                                    (dgDefect_NJ.Columns[sColName] as C1.WPF.DataGrid.DataGridNumericColumn).EditOnSelection = true;
                                }

                                if (dgDefect_NJ.Rows.Count == 0) continue;

                                SetBOMCnt(i, Util.NVC(dtBOM_CHK.Rows[i]["PRODUCT_LEVEL3_CODE"]), Util.NVC(dtBOM_CHK.Rows[i]["PROC_INPUT_CNT"]));

                                iCnt = i;

                                C1.WPF.DataGrid.Summaries.DataGridAggregate.SetAggregateFunctions(dgDefect_NJ.Columns[sColName]
                                , new C1.WPF.DataGrid.Summaries.DataGridAggregatesCollection { new C1.WPF.DataGrid.Summaries.DataGridAggregateSum { ResultTemplate = this.Resources["ResultTemplate"] as DataTemplate } });

                            }
                        }
                        else
                        {
                            InitDefectDataGrid();

                            dtBOM = searchResult.Copy();

                            for (int i = 0; i < searchResult.Rows.Count; i++)
                            {
                                string sColName = string.Empty;
                                string sColName2 = string.Empty;
                                string sHeader = string.Empty;

                                List<string> dIndex = new List<string>();
                                List<string> dIndex2 = new List<string>();
                        
                                int ichk = 0;

                                ichk = Get_Type_Chk(Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL3_CODE"]));

                                sColName = "REG" + (ichk).ToString();
                                sHeader = Util.NVC(searchResult.Rows[i]["ATTRIBUTE3"]).ToString();

                                // 불량 수량 컬럼 위치 변경.
                                int iColIdx = 0;

                                dIndex.Add(ObjectDic.Instance.GetObjectName("입력"));
                                dIndex.Add(sHeader);

                                iColIdx = dgDefect_NJ.Columns["INPUT_FC"].Index;

                                Util.SetGridColumnNumeric(dgDefect_NJ, sColName, dIndex, sHeader, true, true, true, false, -1, HorizontalAlignment.Right, Visibility.Visible, iColIdx, "#,##0");  // [입력, FOLDED CELL]     

                                if (dgDefect_NJ.Columns.Contains(sColName))
                                {
                                    (dgDefect_NJ.Columns[sColName] as C1.WPF.DataGrid.DataGridNumericColumn).Minimum = 0;
                                    (dgDefect_NJ.Columns[sColName] as C1.WPF.DataGrid.DataGridNumericColumn).Maximum = 2147483647; // int max : 2147483647;
                                    (dgDefect_NJ.Columns[sColName] as C1.WPF.DataGrid.DataGridNumericColumn).EditOnSelection = true;
                                }

                                if (dgDefect_NJ.Rows.Count == 0) continue;

                                SetBOMCnt(i, Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL3_CODE"]), Util.NVC(searchResult.Rows[i]["PROC_INPUT_CNT"]));

                                iCnt = i;

                                C1.WPF.DataGrid.Summaries.DataGridAggregate.SetAggregateFunctions(dgDefect_NJ.Columns[sColName]
                                , new C1.WPF.DataGrid.Summaries.DataGridAggregatesCollection { new C1.WPF.DataGrid.Summaries.DataGridAggregateSum { ResultTemplate = this.Resources["ResultTemplate"] as DataTemplate } });

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }
                );
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void SetBOMCnt(int i, string sCode, string sCnt)
        {
            if (i == 0)
            {
                tbType0.Visibility = Visibility.Visible;
                txtType0.Visibility = Visibility.Visible;

                tbType0.Text = sCode;
                txtType0.Text = Convert.ToDouble(sCnt).ToString();
            }
            else if (i == 1)
            {
                tbType1.Visibility = Visibility.Visible;
                txtType1.Visibility = Visibility.Visible;

                tbType1.Text = sCode;
                txtType1.Text = Convert.ToDouble(sCnt).ToString();
            }
            else if (i == 2)
            {
                tbType2.Visibility = Visibility.Visible;
                txtType2.Visibility = Visibility.Visible;

                tbType2.Text = sCode;
                txtType2.Text = Convert.ToDouble(sCnt).ToString();
            }
            else if (i == 3)
            {
                tbType3.Visibility = Visibility.Visible;
                txtType3.Visibility = Visibility.Visible;

                tbType3.Text = sCode;
                txtType3.Text = Convert.ToDouble(sCnt).ToString();
            }
            else if (i == 4)
            {
                tbType4.Visibility = Visibility.Visible;
                txtType4.Visibility = Visibility.Visible;

                tbType4.Text = sCode;
                txtType4.Text = Convert.ToDouble(sCnt).ToString();
            }
            else if (i == 5)
            {
                tbType5.Visibility = Visibility.Visible;
                txtType5.Visibility = Visibility.Visible;

                tbType5.Text = sCode;
                txtType5.Text = Convert.ToDouble(sCnt).ToString();
            }
            else if (i == 6)
            {
                tbType6.Visibility = Visibility.Visible;
                txtType6.Visibility = Visibility.Visible;

                tbType6.Text = sCode;
                txtType6.Text = Convert.ToDouble(sCnt).ToString();
            }
            else if (i == 7)
            {
                tbType7.Visibility = Visibility.Visible;
                txtType7.Visibility = Visibility.Visible;

                tbType7.Text = sCode;
                txtType7.Text = Convert.ToDouble(sCnt).ToString();
            }
            else if (i == 8)
            {
                tbType8.Visibility = Visibility.Visible;
                txtType8.Visibility = Visibility.Visible;

                tbType8.Text = sCode;
                txtType8.Text = Convert.ToDouble(sCnt).ToString();
            }       
        }

        private void InitDefectDataGrid(bool bClearAll = false)
        {
            if (bClearAll)
            {
                Util.gridClear(dgDefect_NJ);

                for (int i = dgDefect_NJ.Columns.Count; i-- > 0;)
                {
                    if (dgDefect_NJ.Columns[i].Name.ToString().StartsWith("REG"))
                    {
                        dgDefect_NJ.Columns.RemoveAt(i);
                    }
                }
            }
            else
            {
                // 기존 추가된 Col 삭제..                
                for (int i = dgDefect_NJ.Columns.Count; i-- > 0;)
                {
                    if (dgDefect_NJ.Columns[i].Name.ToString().StartsWith("REG"))
                    {
                        DataTable dt = DataTableConverter.Convert(dgDefect_NJ.ItemsSource);
                        if (dt.Columns.Count > i)
                            if (dt.Columns[i].ColumnName.Equals(dgDefect_NJ.Columns[i].Name))
                                dt.Columns.RemoveAt(i);

                        dgDefect_NJ.Columns.RemoveAt(i);
                    }
                }
            }
        }

        private void GetDefectInfo()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                string bizRuleName = string.Empty;
                
                if (_ProcID.Equals(Process.STACKING_FOLDING))
                    bizRuleName = "DA_QCA_SEL_CELL_TYPE_DFCT_HIST_NJ";
                else if (_ProcID.Equals(Process.STP))
                    bizRuleName = "DA_QCA_SEL_CELL_TYPE_DFCT_HIST_STP";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("WIPSEQ", typeof(string));
                inDataTable.Columns.Add("ACTID", typeof(string));
                //inDataTable.Columns.Add("TYPE", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = _ProcID;
                newRow["EQPTID"] = _EqptID;
                newRow["LOTID"] = _LotID;
                newRow["WIPSEQ"] = _WipSeq;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT"; //"DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";
                //newRow["TYPE"] = _StackingYN;

                inDataTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //Defect_Sum();

                        Util.GridSetData(dgDefect_NJ, searchResult, null, true);
                        
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }
                );
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void SetDefect(bool bShowMsg = true)
        {               
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                dgDefect_NJ.EndEdit();

                int iSeq = 0;

                string sInAType = string.Empty;
                string sInCType = string.Empty;
                string sInLType = string.Empty;
                string sInRType = string.Empty;
                string sInMLType = string.Empty;
                string sInMRType = string.Empty;

                string sOut = string.Empty;

                string sAtype = string.Empty;
                string sCtype = string.Empty;
                string sFtype = string.Empty;

                DataTable RQSTDT1 = new DataTable();
                RQSTDT1.TableName = "RQSTDT";
                RQSTDT1.Columns.Add("LOTID", typeof(String));
                RQSTDT1.Columns.Add("SHOPID", typeof(String));
                RQSTDT1.Columns.Add("PROCID", typeof(String));

                DataRow dr1 = RQSTDT1.NewRow();
                dr1["LOTID"] = _LotID;
                dr1["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr1["PROCID"] = _ProcID;
                RQSTDT1.Rows.Add(dr1);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_PRODUCT_LEVEL_CODE_INFO", "RQSTDT", "RSLTDT", RQSTDT1);                

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(String));
                RQSTDT.Columns.Add("WIPSEQ", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));
                RQSTDT.Columns.Add("ACTID", typeof(String));
                RQSTDT.Columns.Add("RESNCODE", typeof(String));
                RQSTDT.Columns.Add("REG_QTY", typeof(Decimal));
                RQSTDT.Columns.Add("CALC_QTY", typeof(Decimal));
                RQSTDT.Columns.Add("USERID", typeof(String));

                for (int icnt = 2; icnt < dgDefect_NJ.GetRowCount() + 2; icnt++)
                {
                    for (int jcnt = 0; jcnt < SearchResult.Rows.Count; jcnt++)
                    {
                        string sCode = Util.NVC(SearchResult.Rows[jcnt]["PRODUCT_LEVEL3_CODE"]);

                        if (sCode.Equals("FC") || sCode.Equals("SC"))
                        {
                            DataRow dr = RQSTDT.NewRow();
                            dr["LOTID"] = _LotID;
                            dr["WIPSEQ"] = int.TryParse(_WipSeq, out iSeq) ? iSeq : 1;
                            dr["PRODID"] = Util.NVC(SearchResult.Rows[jcnt]["PRODID"]);
                            dr["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[icnt].DataItem, "ACTID"));
                            dr["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[icnt].DataItem, "RESNCODE"));
                            dr["REG_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgDefect_NJ.Rows[icnt].DataItem, "INPUT_FC"));
                            dr["CALC_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgDefect_NJ.Rows[icnt].DataItem, "CALC_FC"));
                            dr["USERID"] = LoginInfo.USERID;

                            RQSTDT.Rows.Add(dr);
                        }
                        else
                        {
                            int ichk = Get_Type_Chk(sCode);

                            DataRow dr = RQSTDT.NewRow();
                            dr["LOTID"] = _LotID;
                            dr["WIPSEQ"] = int.TryParse(_WipSeq, out iSeq) ? iSeq : 1;
                            dr["PRODID"] = Util.NVC(SearchResult.Rows[jcnt]["PRODID"]);
                            dr["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[icnt].DataItem, "ACTID"));
                            dr["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[icnt].DataItem, "RESNCODE"));
                            dr["REG_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgDefect_NJ.Rows[icnt].DataItem, "REG" + ichk.ToString()));
                            //dr["CALC_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[icnt].DataItem, "CALC_FC"));
                            dr["CALC_QTY"] = 0;
                            dr["USERID"] = LoginInfo.USERID;

                            RQSTDT.Rows.Add(dr);
                        }
                    }
                }

                new ClientProxy().ExecuteService("BR_PRD_REG_CELL_TYPE_DFCT_HIST", "INDATA", null, RQSTDT, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        DataSet indataSet = _Biz.GetBR_PRD_REG_DEFECT_ALL();
                        DataTable inTable = indataSet.Tables["INDATA"];

                        DataRow newRow = inTable.NewRow();
                        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        newRow["EQPTID"] = _EqptID;
                        newRow["USERID"] = LoginInfo.USERID;

                        inTable.Rows.Add(newRow);

                        DataTable inDEFECT_LOT = indataSet.Tables["INRESN"];

                        //for (int i = 0; i < dgDefect_NJ.Rows.Count - dgDefect_NJ.BottomRows.Count; i++)
                        for (int i = 2; i < dgDefect_NJ.GetRowCount() + 2; i++)
                        {
                            newRow = null;

                            newRow = inDEFECT_LOT.NewRow();
                            newRow["LOTID"] = _LotID.Trim();
                            newRow["WIPSEQ"] = int.TryParse(_WipSeq, out iSeq) ? iSeq : 1;
                            newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[i].DataItem, "ACTID"));
                            newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[i].DataItem, "RESNCODE"));

                            //if (_EQPT_DFCT_APPLY_FLAG == "Y")
                            //{
                            //    if (double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[i].DataItem, "REG_A"))) == 0 &&
                            //        double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[i].DataItem, "REG_C"))) == 0 &&
                            //        double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[i].DataItem, "REG_L"))) == 0 &&
                            //        double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[i].DataItem, "REG_R"))) == 0 &&
                            //        double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[i].DataItem, "REG_ML"))) == 0 &&
                            //        double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[i].DataItem, "REG_MR"))) == 0 &&
                            //        double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[i].DataItem, "REG_F"))) == 0)
                            //        //newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[i].DataItem, "CALC_F")).Equals("") ? 0 :
                            //        //double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[i].DataItem, "CALC_F"))) + double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[i].DataItem, "EQP_DFCT_QTY")));
                            //        newRow["RESNQTY"] = double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[i].DataItem, "EQP_DFCT_QTY")));
                            //    else
                            //        newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[i].DataItem, "CALC_F")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[i].DataItem, "CALC_F")));
                            //}
                            //else
                            //{
                            //    newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[i].DataItem, "CALC_F")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[i].DataItem, "CALC_F")));
                            //}


                            newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[i].DataItem, "CALC_FC")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[i].DataItem, "CALC_FC")));
                            newRow["RESNCODE_CAUSE"] = "";
                            newRow["PROCID_CAUSE"] = "";
                            newRow["RESNNOTE"] = "";
                            newRow["DFCT_TAG_QTY"] = 0;
                            newRow["LANE_QTY"] = 1;
                            newRow["LANE_PTN_QTY"] = 1;

                            if (Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[i].DataItem, "ACTID")).GetString() == "CHARGE_PROD_LOT")
                            {
                                newRow["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[i].DataItem, "COST_CNTR_ID"));
                            }
                            else
                            {
                                newRow["COST_CNTR_ID"] = "";
                            }

                            //newRow["A_TYPE_DFCT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[i].DataItem, "CALC_A")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[i].DataItem, "CALC_A")));
                            //newRow["C_TYPE_DFCT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[i].DataItem, "CALC_C")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[i].DataItem, "CALC_C")));

                            newRow["A_TYPE_DFCT_QTY"] = 0;
                            newRow["C_TYPE_DFCT_QTY"] = 0;

                            inDEFECT_LOT.Rows.Add(newRow);
                        }

                        if (inDEFECT_LOT.Rows.Count < 1)
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            return;
                        }

                        new ClientProxy().ExecuteService_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL", "INDATA,INRESN", null, (bizResult2, bizException2) =>
                        {
                            try
                            {
                                if (bizException2 != null)
                                {
                                    Util.MessageException(bizException2);
                                    return;
                                }

                                if (bShowMsg)
                                    Util.MessageInfo("SFU1275");      //정상 처리 되었습니다.

                                GetDefectInfo();
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                            }
                        }, indataSet);

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Validation]
        private bool CanSaveDefect()
        {
            if (!CommonVerify.HasDataGridRow(dgDefect_NJ))
            {
                Util.MessageValidation("SFU1578");      //불량 항목이 없습니다.
                return false;
            }
            if (_LotID.Trim().Length < 1)
            {
                Util.MessageValidation("SFU1195");      //Lot 정보가 없습니다.
                return false;
            }

            return true;
        }

        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnDefectSave);

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

        private void GetAllData()
        {
            GetEqpt_Dfct_Apply_Flag();
            GetDefectInfo();

            
           if (_EQPT_DFCT_APPLY_FLAG == "Y")
           {
                Defect_Sum();
           }

        }

        private void GetEqpt_Dfct_Apply_Flag()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PROCID", typeof(String));
                RQSTDT.Columns.Add("EQSGID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["PROCID"] = _ProcID;
                dr["EQSGID"] = _LineID;

                RQSTDT.Rows.Add(dr);

                DataTable SResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_DFCT_APPLY_FLAG", "RQSTDT", "RSLTDT", RQSTDT);

                _EQPT_DFCT_APPLY_FLAG = SResult.Rows[0]["EQPT_DFCT_APPLY_FLAG"].ToString();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Defect_Sum()
        {
            try
            {
                DataTable dtTmp = DataTableConverter.Convert(dgDefect_NJ.ItemsSource);

                for (int icnt = 0; icnt < dtTmp.Rows.Count; icnt++)
                {
                    //double Eqp_qty = double.Parse(Util.NVC(DataTableConverter.GetValue(dtTmp.Rows[icnt], "EQP_DFCT_QTY")));
                    //double Calc_F = double.Parse(Util.NVC(DataTableConverter.GetValue(dtTmp.Rows[icnt], "CALC_F")));
                    double Eqp_qty = double.Parse(Util.NVC(dtTmp.Rows[icnt]["EQP_DFCT_QTY"].ToString()));
                    double Calc_F = double.Parse(Util.NVC(dtTmp.Rows[icnt]["CALC_F"].ToString()));
                   
                    double Sum = 0;

                    if (Calc_F == 0)
                    {
                        Sum = Calc_F + Eqp_qty;
                        DataTableConverter.SetValue(dgDefect_NJ.Rows[icnt + 2].DataItem, "CALC_F", Sum);
                    }
                    else
                    {
                        Sum = Calc_F;
                        DataTableConverter.SetValue(dgDefect_NJ.Rows[icnt + 2].DataItem, "CALC_F", Sum);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #endregion

        private void dgDefect_NJ_UnloadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    //if (e.Cell.Row.Type == DataGridRowType.Item)
                    //{
                    e.Cell.Presenter.Background = null;
                    //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    //}
                }
            }));
        }

        private void dgDefect_NJ_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    //if (Convert.ToString(e.Cell.Column.Name) == "REG_A" || Convert.ToString(e.Cell.Column.Name) == "REG_C" || Convert.ToString(e.Cell.Column.Name) == "REG_L" 
                    //|| Convert.ToString(e.Cell.Column.Name) == "REG_R" || Convert.ToString(e.Cell.Column.Name) == "REG_ML" || Convert.ToString(e.Cell.Column.Name) == "REG_MR")
                    //{
                    if (e.Cell.Column.Name.ToString().StartsWith("REG"))
                    { 
                        string sActid = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[e.Cell.Row.Index].DataItem, "ACTID"));
                        if (sActid == "DEFECT_LOT")
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                        }
                        else
                        {
                            //e.Cell.Column.EditOnSelection = false;
                            //e.Cell.Column.IsReadOnly = true;
                            //e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                        }
                    }
                    else if (Convert.ToString(e.Cell.Column.Name) == "INPUT_FC")
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                    }
                }
            }));
        }

        private void dgDefect_NJ_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e == null || e.Row == null || e.Row.DataItem == null || e.Column == null)
                    return;

                //if (e.Column.Name.Equals("REG_A") || e.Column.Name.Equals("REG_C") || e.Column.Name.Equals("REG_L") || e.Column.Name.Equals("REG_R")
                //    || e.Column.Name.Equals("REG_ML") || e.Column.Name.Equals("REG_MR"))
                if (e.Column.Name.ToString().StartsWith("REG"))
                {
                    string sActid = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "ACTID"));
                    if (sActid != "DEFECT_LOT")
                    {
                        e.Cancel = true;
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

