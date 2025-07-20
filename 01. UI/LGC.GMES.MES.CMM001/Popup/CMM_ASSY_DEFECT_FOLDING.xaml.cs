/*************************************************************************************
 Created Date : 2016.11.28
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - 조립 공정진척 화면 - 불량정보 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.11.28  INS 김동일K : Initial Created.
  2017.10.25  INS 염규범S : 폴딩/스태킹 수량 표기 변경
  2017.11.01  INS 염규범S : 폴딩/스태킹 불량 장비 반영 여부에 따른 로직 변경
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
    /// CMM_ASSY_DEFECT_FOLDING.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_DEFECT_FOLDING : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _LotID = string.Empty;
        private string _WipSeq = string.Empty;
        private string _ProcID = string.Empty;
        private string _StackingYN = string.Empty;
        private string _WoDetlID = string.Empty;

        private double gDfctAQty = 0;
        private double gDfctCQty = 0;

        private string _EQPT_DFCT_APPLY_FLAG = string.Empty;
        private string _WorkTypeCode = string.Empty;

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();

        DataTable mData = new DataTable();
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

        public CMM_ASSY_DEFECT_FOLDING()
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


                txtLotID.Text = _LotID;

                if (_ProcID.Equals(Process.STACKING_FOLDING))
                {
                    grdMBomTypeCnt.Visibility = Visibility.Visible;

                    if (_StackingYN.Equals("Y"))
                    {
                        tbAType.Text = "HALFTYPE";
                        tbCType.Text = "MONOTYPE";

                        // 수량을
                        // 폴딩 = FOLDED CELL & 스테킹 = Stacking CELL 로 표기
                        dgDefect.Columns["REG_F"].Header = new List<string>() { "입력", "STACKING CELL" };
                        dgDefect.Columns["CALC_F"].Header = new List<string>() { "계산", "STACKING CELL" };

                        if (dgDefect != null && dgDefect.Columns.Contains("REG_A"))
                            dgDefect.Columns["REG_A"].Header = new List<string>() { "입력", "HALFTYPE" };
                        if (dgDefect != null && dgDefect.Columns.Contains("REG_C"))
                            dgDefect.Columns["REG_C"].Header = new List<string>() { "입력", "MONOTYPE" };
                        if (dgDefect != null && dgDefect.Columns.Contains("CALC_A"))
                            dgDefect.Columns["CALC_A"].Header = new List<string>() { "계산", "HALFTYPE" };
                        if (dgDefect != null && dgDefect.Columns.Contains("CALC_C"))
                            dgDefect.Columns["CALC_C"].Header = new List<string>() { "계산", "MONOTYPE" };
                    }
                    else
                    {
                        tbAType.Text = "ATYPE";
                        tbCType.Text = "CTYPE";

                        // 수량을
                        // 폴딩 = FOLDED CELL & 스테킹 = Stacking CELL 로 표기
                        dgDefect.Columns["REG_F"].Visibility = Visibility.Visible;
                        dgDefect.Columns["CALC_F"].Visibility = Visibility.Visible;

                        if (dgDefect != null && dgDefect.Columns.Contains("REG_A"))
                            dgDefect.Columns["REG_A"].Header = new List<string>() { "입력", "ATYPE" };
                        if (dgDefect != null && dgDefect.Columns.Contains("REG_C"))
                            dgDefect.Columns["REG_C"].Header = new List<string>() { "입력", "CTYPE" };
                        if (dgDefect != null && dgDefect.Columns.Contains("CALC_A"))
                            dgDefect.Columns["CALC_A"].Header = new List<string>() { "계산", "ATYPE" };
                        if (dgDefect != null && dgDefect.Columns.Contains("CALC_C"))
                            dgDefect.Columns["CALC_C"].Header = new List<string>() { "계산", "CTYPE" };
                    }

                    GetMBOMInfo();

                    #region SCRAP
                    _WorkTypeCode = GetWorkTypeCode();

                    if (_WorkTypeCode.Equals("SCRAP"))
                        dgDefect.Columns["DFCT_QTY_DDT_RATE"].Visibility = Visibility.Visible;

                    #endregion
                }
                //else
                //{
                //    grdMBomTypeCnt.Visibility = Visibility.Collapsed;

                //    if (dgDefect != null && dgDefect.Columns.Contains("A_TYPE_DFCT_QTY"))
                //        dgDefect.Columns["A_TYPE_DFCT_QTY"].Visibility = Visibility.Collapsed;
                //    if (dgDefect != null && dgDefect.Columns.Contains("C_TYPE_DFCT_QTY"))
                //        dgDefect.Columns["C_TYPE_DFCT_QTY"].Visibility = Visibility.Collapsed;
                //}

                GetAllData();
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
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1587"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
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

        private void dgDefect_CommittedEdit(object sender, DataGridCellEventArgs e)

        {
            try
            {
                if (!_ProcID.Equals(Process.STACKING_FOLDING))
                    return;

                double Reg_Atype;
                double Reg_Ctype;
                double Reg_Resnqty;
                double Cal_Resnqty;

                double Division_A;
                double Division_C;

                string sEQP_DFCT_QTY = string.Empty;
                double dEQP_DFCT_QTY;

                //sEQP_DFCT_QTY = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "EQP_DFCT_QTY"));
                //dEQP_DFCT_QTY = double.Parse(sEQP_DFCT_QTY);

                if (e.Cell.Column.Name.Equals("REG_A"))
                {
                    string sAtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_A"));
                    string sCtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_C"));
                    string sFtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_F"));
                    string sATemp = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "TEMP_A"));
                    string sCalc_FC = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F"));
                      
                    Reg_Atype = double.Parse(sAtype);
                    Reg_Ctype = double.Parse(sCtype);
                    Reg_Resnqty = double.Parse(sFtype);
                    double Temp_A = double.Parse(sATemp);
                    Cal_Resnqty = double.Parse(sCalc_FC);

                    double Remain_A = Math.Floor(Reg_Atype % gDfctAQty);
                    double Remain_C = Math.Floor(Reg_Ctype % gDfctCQty);

                    Division_A = Math.Floor(Reg_Atype / gDfctAQty);
                    Division_C = Math.Floor(Reg_Ctype / gDfctCQty);



                     if (gDfctAQty > 0 && gDfctCQty > 0)
                    {
                        if (Reg_Ctype == 0)
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_A", Reg_Atype);
                        }
                        else
                        {
                            if (Division_A != Division_C)
                            {
                                if (Division_A > Division_C)
                                {
                                    DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_A", Reg_Atype - (Division_C * gDfctAQty));
                                    DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_C", Reg_Ctype - (Division_C * gDfctCQty));

                                    if (_EQPT_DFCT_APPLY_FLAG == "Y")
                                    {
                                        if (Reg_Resnqty == 0)
                                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C );
                                        //DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + dEQP_DFCT_QTY);
                                        else
                                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + Reg_Resnqty);
                                        //DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + Cal_Resnqty + dEQP_DFCT_QTY);
                                    }
                                    else
                                    {
                                        if (Reg_Resnqty == 0)
                                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C);
                                        else
                                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + Cal_Resnqty);
                                    }

                                    //if (Reg_Resnqty == 0)
                                    //    DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C);
                                    //else
                                    //    DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + Cal_Resnqty);
                                }
                                else
                                {
                                    DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_A", Reg_Atype - (Division_A * gDfctAQty));
                                    DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_C", Reg_Ctype - (Division_A * gDfctCQty));

                                    if (_EQPT_DFCT_APPLY_FLAG == "Y")
                                    {
                                        if (Reg_Resnqty == 0)
                                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_A);
                                        //DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_A + dEQP_DFCT_QTY);
                                        else
                                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_A + Reg_Resnqty);
                                        //DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_A + Reg_Resnqty + dEQP_DFCT_QTY);
                                    }
                                    else
                                    {
                                        if (Reg_Resnqty == 0)
                                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_A);
                                        else
                                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_A + Reg_Resnqty);
                                    }

                                    //if (Reg_Resnqty == 0)
                                    //    DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_A);
                                    //else
                                    //   DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_A + Reg_Resnqty);
                                }
                            }
                            else
                            {
                                DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_A", Reg_Atype - (Division_C * gDfctAQty));
                                DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_C", Reg_Ctype - (Division_A * gDfctCQty));

                                if (_EQPT_DFCT_APPLY_FLAG == "Y")
                                {
                                    if (Reg_Resnqty == 0)
                                        DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C);
                                    //DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + dEQP_DFCT_QTY);
                                    else
                                        DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + Reg_Resnqty);
                                    //DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + Reg_Resnqty + dEQP_DFCT_QTY);
                                }
                                else
                                {
                                    if (Reg_Resnqty == 0)
                                        DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C);
                                    else
                                        DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + Reg_Resnqty);
                                }

                                //if (Reg_Resnqty == 0)
                                //    DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C);
                                //else
                                //    DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + Reg_Resnqty);
                            }
                        }

                        DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "TEMP_A", Reg_Atype);
                    }
                }
                else if (e.Cell.Column.Name.Equals("REG_C"))
                {
                    string sAtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_A"));
                    string sCtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_C"));
                    string sFtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_F"));
                    string sCTemp = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "TEMP_C"));
                    string sCalc_FC = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F"));

                    Reg_Atype = double.Parse(sAtype);
                    Reg_Ctype = double.Parse(sCtype);
                    Reg_Resnqty = double.Parse(sFtype);
                    double Temp_C = double.Parse(sCTemp);
                    Cal_Resnqty = double.Parse(sCalc_FC);

                    double Remain_A = Math.Floor(Reg_Atype % gDfctAQty);
                    double Remain_C = Math.Floor(Reg_Ctype % gDfctCQty);

                    Division_A = Math.Floor(Reg_Atype / gDfctAQty);
                    Division_C = Math.Floor(Reg_Ctype / gDfctCQty);

                    if (gDfctAQty > 0 && gDfctCQty > 0)
                    {
                        if (Reg_Atype == 0)
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_C", Reg_Ctype);
                        }
                        else
                        {
                            if (Division_A != Division_C)
                            {
                                if (Division_A > Division_C)
                                {
                                    DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_A", Reg_Atype - (Division_C * gDfctAQty));
                                    DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_C", Reg_Ctype - (Division_C * gDfctCQty));

                                    if (_EQPT_DFCT_APPLY_FLAG == "Y")
                                    {
                                        if (Reg_Resnqty == 0)
                                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C);
                                        //DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + dEQP_DFCT_QTY);
                                        else
                                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + Reg_Resnqty);
                                        // DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + Reg_Resnqty + dEQP_DFCT_QTY);
                                    }
                                    else
                                    {
                                        if (Reg_Resnqty == 0)
                                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C);
                                        else
                                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + Reg_Resnqty);
                                    }

                                    //if (Reg_Resnqty == 0)
                                    //    DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C);
                                    // else
                                    //    DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + Reg_Resnqty);
                                }
                                else
                                {
                                    DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_A", Reg_Atype - (Division_A * gDfctAQty));
                                    DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_C", Reg_Ctype - (Division_A * gDfctCQty));

                                    if (_EQPT_DFCT_APPLY_FLAG == "Y")
                                    {
                                        if (Reg_Resnqty == 0)
                                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_A);
                                        // DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_A + dEQP_DFCT_QTY);
                                        else
                                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_A + Reg_Resnqty);
                                        // DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_A + Reg_Resnqty + dEQP_DFCT_QTY);
                                    }
                                    else
                                    {
                                        if (Reg_Resnqty == 0)
                                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_A);
                                        else
                                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_A + Reg_Resnqty);
                                    }
                                    //if (Reg_Resnqty == 0)
                                    //    DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_A);
                                    //else
                                    //    DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_A + Reg_Resnqty);
                                }
                            }
                            else
                            {

                                DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_A", Reg_Atype - (Division_C * gDfctAQty));
                                DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_C", Reg_Ctype - (Division_A * gDfctCQty));

                                if (_EQPT_DFCT_APPLY_FLAG == "Y")
                                {
                                    if (Reg_Resnqty == 0)
                                        DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C);
                                    //DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + dEQP_DFCT_QTY);
                                    else
                                        DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + Reg_Resnqty);
                                    //   DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + Reg_Resnqty + dEQP_DFCT_QTY);
                                }
                                else
                                {
                                    if (Reg_Resnqty == 0)
                                        DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C);
                                    else
                                        DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + Reg_Resnqty);
                                }
                                //if (Reg_Resnqty == 0)
                                //    DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C);
                                //else
                                //    DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + Reg_Resnqty);

                            }
                        }

                        DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "TEMP_C", Reg_Ctype);
                    }

                }
                else if (e.Cell.Column.Name.Equals("REG_F"))
                {

                    string sReg_FC = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_F"));
                    string sCalc_FC = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F"));
                    string sTemp_FC = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "TEMP_F"));

                    string sAtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_A"));
                    string sCtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_C"));

                    Reg_Atype = double.Parse(sAtype);
                    Reg_Ctype = double.Parse(sCtype);
                    double dTemp = double.Parse(sTemp_FC);

                    Reg_Resnqty = double.Parse(sReg_FC);
                    Cal_Resnqty = double.Parse(sCalc_FC);

                    double Remain_A = Math.Floor(Reg_Atype % gDfctAQty);
                    double Remain_C = Math.Floor(Reg_Ctype % gDfctCQty);

                    Division_A = Math.Floor(Reg_Atype / gDfctAQty);
                    Division_C = Math.Floor(Reg_Ctype / gDfctCQty);

                    if (_EQPT_DFCT_APPLY_FLAG == "Y")
                    {
                        if (Reg_Atype == 0 && Reg_Ctype == 0)
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Reg_Resnqty);
                            // DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Reg_Resnqty + dEQP_DFCT_QTY);
                        }

                        if (Division_A > Division_C)
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + Reg_Resnqty);
                            // DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + Reg_Resnqty + dEQP_DFCT_QTY);
                        }
                        else
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_A + Reg_Resnqty);
                            //DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_A + Reg_Resnqty + dEQP_DFCT_QTY);
                        }
                    }
                    else
                    {
                        if (Reg_Atype == 0 && Reg_Ctype == 0)
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Reg_Resnqty);
                        }

                        if (Division_A > Division_C)
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + Reg_Resnqty);
                        }
                        else
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_A + Reg_Resnqty);
                        }
                    }
                    //if (Reg_Atype == 0 && Reg_Ctype == 0)
                    //{
                    //    DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Reg_Resnqty);
                    //}

                    //if (Division_A > Division_C)
                    //{
                    //    DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_C + Reg_Resnqty);
                    //}
                    //else
                    //{
                    //    DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Division_A + Reg_Resnqty);
                    //}
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
        private void GetMBOMInfo()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable inTable = _Biz.GetDA_PRD_SEL_MBOM();

                DataRow newRow = inTable.NewRow();
                newRow["WO_DETL_ID"] = _WoDetlID;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                //newRow["EQSGID"] = _LineID;
                //newRow["INPUT_PROCID"] = Process.STACKING_FOLDING;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_MBOM_INFO", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
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
                        else
                        {
                            if (_StackingYN.Equals("Y"))
                            {
                                for (int i = 0; i < searchResult.Rows.Count; i++)
                                {
                                    if (Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL2_CODE"]).Equals("HC"))
                                    {
                                        txtInAType.Text = double.Parse(Util.NVC(searchResult.Rows[i]["PROC_INPUT_CNT"])).ToString();
                                    }
                                    else if (Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL2_CODE"]).Equals("MC"))
                                    {
                                        txtInCType.Text = double.Parse(Util.NVC(searchResult.Rows[i]["PROC_INPUT_CNT"])).ToString();
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < searchResult.Rows.Count; i++)
                                {
                                    if (Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL3_CODE"]).Equals("AT"))
                                    {
                                        txtInAType.Text = double.Parse(Util.NVC(searchResult.Rows[i]["PROC_INPUT_CNT"])).ToString();
                                    }
                                    else if (Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL3_CODE"]).Equals("CT"))
                                    {
                                        txtInCType.Text = double.Parse(Util.NVC(searchResult.Rows[i]["PROC_INPUT_CNT"])).ToString();
                                    }
                                }
                            }


                            if (txtInAType.Text.Equals(""))
                            {
                                if (_StackingYN.Equals("Y"))
                                    Util.MessageValidation("SFU1337");      //HALF TYPE 불량 기준정보가 존재하지 않습니다.
                                else
                                    Util.MessageValidation("SFU1306");        //ATYPE 불량 기준정보가 존재하지 않습니다.

                                return;
                            }

                            if (txtInCType.Text.Equals(""))
                            {
                                if (_StackingYN.Equals("Y"))
                                    Util.MessageValidation("SFU1401");     //MONO TYPE 불량 기준정보가 존재하지 않습니다.
                                else
                                    Util.MessageValidation("SFU1326");        //CTYPE 불량 기준정보가 존재하지 않습니다.

                                return;
                            }

                            gDfctAQty = double.Parse(txtInAType.Text);
                            gDfctCQty = double.Parse(txtInCType.Text);
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

        private void GetDefectInfo()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                const string bizRuleName = "DA_QCA_SEL_CELL_TYPE_DFCT_HIST";

                ///DataTable inTable = _Biz.GetDA_QCA_SEL_WIPRESONCOLLECT();

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("WIPSEQ", typeof(string));
                inDataTable.Columns.Add("ACTID", typeof(string));
                inDataTable.Columns.Add("TYPE", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = _ProcID;
                newRow["EQPTID"] = _EqptID;
                newRow["LOTID"] = _LotID;
                newRow["WIPSEQ"] = _WipSeq;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT"; //"DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";
                newRow["TYPE"] = _StackingYN;

                inDataTable.Rows.Add(newRow);

                //DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);

                //Util.GridSetData(dgDefect, searchResult, null, true);

                //loadingIndicator.Visibility = Visibility.Collapsed;


                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        #region 불량비율
                        if (_WorkTypeCode.Equals("SCRAP"))
                        {
                            DataColumn[] keyColumn = new DataColumn[2];
                            keyColumn[0] = searchResult.Columns["ACTID"];
                            keyColumn[1] = searchResult.Columns["RESNCODE"];
                            searchResult.PrimaryKey = keyColumn;

                            searchResult.Merge(GetDefectRate());
                        }
                        #endregion

                        Defect_Sum();

                        //dgDefect.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgDefect, searchResult, null, true);

                        
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

                dgDefect.EndEdit();

                int iSeq = 0;

                string sInAType = string.Empty;
                string sInCType = string.Empty;
                string sOut = string.Empty;
                string sAtype = string.Empty;
                string sCtype = string.Empty;
                string sFtype = string.Empty;

                DataTable RQSTDT1 = new DataTable();
                RQSTDT1.TableName = "RQSTDT";
                RQSTDT1.Columns.Add("LOTID", typeof(String));
                RQSTDT1.Columns.Add("SHOPID", typeof(String));
                RQSTDT1.Columns.Add("AREAID", typeof(String));

                DataRow dr1 = RQSTDT1.NewRow();
                dr1["LOTID"] = _LotID;
                dr1["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                RQSTDT1.Rows.Add(dr1);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_PRODUCT_LEVEL_CODE", "RQSTDT", "RSLTDT", RQSTDT1);

                if (_StackingYN.Equals("Y"))
                {
                    for (int i = 0; i < SearchResult.Rows.Count; i++)
                    {
                        if (Util.NVC(SearchResult.Rows[i]["PRODUCT_LEVEL2_CODE"]).Equals("HC"))
                        {
                            sInAType = Util.NVC(SearchResult.Rows[i]["PRODID"]).ToString();
                        }
                        else if (Util.NVC(SearchResult.Rows[i]["PRODUCT_LEVEL2_CODE"]).Equals("MC"))
                        {
                            sInCType = Util.NVC(SearchResult.Rows[i]["PRODID"]).ToString();
                        }
                        else if (Util.NVC(SearchResult.Rows[i]["PRODUCT_LEVEL2_CODE"]).Equals("SC"))
                        {
                            sOut = Util.NVC(SearchResult.Rows[i]["PRODID"]).ToString();
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < SearchResult.Rows.Count; i++)
                    {
                        if (Util.NVC(SearchResult.Rows[i]["PRODUCT_LEVEL3_CODE"]).Equals("AT"))
                        {
                            sInAType = Util.NVC(SearchResult.Rows[i]["PRODID"]).ToString();
                        }
                        else if (Util.NVC(SearchResult.Rows[i]["PRODUCT_LEVEL3_CODE"]).Equals("CT"))
                        {
                            sInCType = Util.NVC(SearchResult.Rows[i]["PRODID"]).ToString();
                        }
                        else if (Util.NVC(SearchResult.Rows[i]["PRODUCT_LEVEL3_CODE"]).Equals("FC"))
                        {
                            sOut = Util.NVC(SearchResult.Rows[i]["PRODID"]).ToString();
                        }
                    }
                }

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

                for (int icnt = 2; icnt < dgDefect.GetRowCount() + 2; icnt++)
                {
                    for (int jcnt = 0; jcnt < 3; jcnt++)
                    {
                        if (jcnt == 0)
                        {
                            DataRow dr = RQSTDT.NewRow();
                            dr["LOTID"] = _LotID;
                            dr["WIPSEQ"] = int.TryParse(_WipSeq, out iSeq) ? iSeq : 1;
                            dr["PRODID"] = sInAType;
                            dr["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "ACTID"));
                            dr["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "RESNCODE"));
                            dr["REG_QTY"] = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "REG_A")));
                            dr["CALC_QTY"] = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "CALC_A")));
                            dr["USERID"] = LoginInfo.USERID;

                            RQSTDT.Rows.Add(dr);
                        }
                        else if (jcnt == 1)
                        {
                            DataRow dr = RQSTDT.NewRow();
                            dr["LOTID"] = _LotID;
                            dr["WIPSEQ"] = int.TryParse(_WipSeq, out iSeq) ? iSeq : 1;
                            dr["PRODID"] = sInCType;
                            dr["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "ACTID"));
                            dr["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "RESNCODE"));
                            dr["REG_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "REG_C"));
                            dr["CALC_QTY"] = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "CALC_C")));
                            dr["USERID"] = LoginInfo.USERID;

                            RQSTDT.Rows.Add(dr);
                        }
                        else if (jcnt == 2)
                        {
                            DataRow dr = RQSTDT.NewRow();
                            dr["LOTID"] = _LotID;
                            dr["WIPSEQ"] = int.TryParse(_WipSeq, out iSeq) ? iSeq : 1;
                            dr["PRODID"] = sOut;
                            dr["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "ACTID"));
                            dr["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "RESNCODE"));
                            dr["REG_QTY"] = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "REG_F")));

                            if (_EQPT_DFCT_APPLY_FLAG == "Y")
                            {
                                if (double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "REG_A"))) == 0 &&
                                    double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "REG_C"))) == 0 &&
                                    double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "REG_F"))) == 0)
                                    //dr["CALC_QTY"] = double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "CALC_F"))) + double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "EQP_DFCT_QTY")));
                                    dr["CALC_QTY"] = double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "EQP_DFCT_QTY")));
                                else
                                    dr["CALC_QTY"] = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "CALC_F")));
                            }
                            else
                            {
                                dr["CALC_QTY"] = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "CALC_F")));
                            }

                            //dr["CALC_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "CALC_F"));
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

                        //for (int i = 0; i < dgDefect.Rows.Count - dgDefect.BottomRows.Count; i++)
                        for (int i = 2; i < dgDefect.GetRowCount() + 2; i++)
                        {
                            newRow = null;

                            newRow = inDEFECT_LOT.NewRow();
                            newRow["LOTID"] = _LotID.Trim();
                            newRow["WIPSEQ"] = int.TryParse(_WipSeq, out iSeq) ? iSeq : 1;
                            newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID"));
                            newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNCODE"));

                            if (_EQPT_DFCT_APPLY_FLAG == "Y")
                            {
                                if (double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "REG_A"))) == 0 &&
                                    double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "REG_C"))) == 0 &&
                                    double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "REG_F"))) == 0)
                                    //newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "CALC_F")).Equals("") ? 0 :
                                    //double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "CALC_F"))) + double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "EQP_DFCT_QTY")));
                                    newRow["RESNQTY"] = double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "EQP_DFCT_QTY")));
                                else
                                    newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "CALC_F")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "CALC_F")));
                            }
                            else
                            {
                                newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "CALC_F")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "CALC_F")));
                            }

                            //newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "CALC_F")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "CALC_F")));
                            newRow["RESNCODE_CAUSE"] = "";
                            newRow["PROCID_CAUSE"] = "";
                            newRow["RESNNOTE"] = "";
                            newRow["DFCT_TAG_QTY"] = 0;
                            newRow["LANE_QTY"] = 1;
                            newRow["LANE_PTN_QTY"] = 1;

                            if (Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID")).GetString() == "CHARGE_PROD_LOT")
                            {
                                newRow["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "COST_CNTR_ID"));
                            }
                            else
                            {
                                newRow["COST_CNTR_ID"] = "";
                            }

                            newRow["A_TYPE_DFCT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "CALC_A")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "CALC_A")));
                            newRow["C_TYPE_DFCT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "CALC_C")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "CALC_C")));

                            inDEFECT_LOT.Rows.Add(newRow);
                        }

                        if (inDEFECT_LOT.Rows.Count < 1)
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            return;
                        }
                        // 폴딩공정의 경우 폐기후재생 처리 시 불량차감 비율 저장 [2018.02.08]
                        //if (_StackingYN.Equals("N") && _WorkTypeCode.Equals("SCRAP"))
                        if (_WorkTypeCode.Equals("SCRAP"))
                        {
                            DataTable inDEFECTLOT_RATE = indataSet.Tables.Add("INRESN_RATE");
                            inDEFECTLOT_RATE.Columns.Add("LOTID", typeof(string));
                            inDEFECTLOT_RATE.Columns.Add("WIPSEQ", typeof(string));
                            inDEFECTLOT_RATE.Columns.Add("ACTID", typeof(string));
                            inDEFECTLOT_RATE.Columns.Add("RESNCODE", typeof(string));
                            inDEFECTLOT_RATE.Columns.Add("DFCT_QTY_DDT_RATE", typeof(int));

                            DataTable dt = ((DataView)dgDefect.ItemsSource).Table.Select().CopyToDataTable();

                            DataRow inData = null;

                            foreach (DataRow row in dt.Rows)
                            {
                                inData = inDEFECTLOT_RATE.NewRow();

                                inData["LOTID"] = _LotID.Trim();
                                inData["WIPSEQ"] = int.TryParse(_WipSeq, out iSeq) ? iSeq : 1;
                                inData["ACTID"] = Util.NVC(row["ACTID"]);
                                inData["RESNCODE"] = Util.NVC(row["RESNCODE"]);
                                inData["DFCT_QTY_DDT_RATE"] = Util.NVC_Decimal(row["DFCT_QTY_DDT_RATE"]);
                                inDEFECTLOT_RATE.Rows.Add(inData);
                            }
                            if (inDEFECTLOT_RATE.Rows.Count < 1)
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                                return;
                            }

                            new ClientProxy().ExecuteService_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL_DEFECTRATE", "INDATA,INRESN,INRESN_RATE", null, (bizResult2, bizException2) =>
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
                        else
                        {
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
            if (!CommonVerify.HasDataGridRow(dgDefect))
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

            
           //if (_EQPT_DFCT_APPLY_FLAG == "Y")
           //{
           //     Defect_Sum();
           //}

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
                dr["PROCID"] = Process.STACKING_FOLDING;
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
                DataTable dtTmp = DataTableConverter.Convert(dgDefect.ItemsSource);

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
                        DataTableConverter.SetValue(dgDefect.Rows[icnt + 2].DataItem, "CALC_F", Sum);
                    }
                    else
                    {
                        Sum = Calc_F;
                        DataTableConverter.SetValue(dgDefect.Rows[icnt + 2].DataItem, "CALC_F", Sum);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private string GetWorkTypeCode()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = _LotID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTATTR", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow row in dtResult.Rows)
                    return Util.NVC(row["WIP_WRK_TYPE_CODE"]);
            }
            catch (Exception ex) { }

            return "";
        }

        private DataTable GetDefectRate()
        {
            try {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQPTID", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));
                RQSTDT.Columns.Add("WIPSEQ", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = _EqptID;
                dr["LOTID"] = _LotID;
                dr["WIPSEQ"] = _WipSeq;
                RQSTDT.Rows.Add(dr);

                DataTable SResult = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_DFCT_RATE", "RQSTDT", "RSLTDT", RQSTDT);

                DataColumn[] keyColumn = new DataColumn[2];
                keyColumn[0] = SResult.Columns["ACTID"];
                keyColumn[1] = SResult.Columns["RESNCODE"];
                SResult.PrimaryKey = keyColumn;

                return SResult;
            }
            catch (Exception ex) {
                Util.MessageException(ex);
                return null;
            }
        }

        #endregion

        #endregion

        private void dgDefect_UnloadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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

        private void dgDefect_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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
                    if (Util.NVC(e.Cell.Column.Name) != "ACTNAME")
                    {
                        string sFlag = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"));
                        if (sFlag == "Y")
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#D4D4D4"));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                        else
                        {
                            if (Util.NVC(e.Cell.Column.Name) == "REG_A" || Util.NVC(e.Cell.Column.Name) == "REG_C")
                            {
                                string sActid = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "ACTID"));
                                if (sActid == "DEFECT_LOT")
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                                    //e.Cell.Column.IsReadOnly = false;
                                    //e.Cell.Column.EditOnSelection = true;
                                }
                                else
                                {
                                    //e.Cell.Column.EditOnSelection = false;
                                    //e.Cell.Column.IsReadOnly = true;
                                    //e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                                }
                            }
                            else if (Util.NVC(e.Cell.Column.Name) == "REG_F" || (Util.NVC(e.Cell.Column.Name) == "DFCT_QTY_DDT_RATE" && _StackingYN.Equals("N")))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                            }
                        }
                    }
                }
            }));
        }

        private void dgDefect_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e == null || e.Row == null || e.Row.DataItem == null || e.Column == null)
                    return;

                if (Util.NVC(e.Column.Name).Equals("REG_A") ||
                    Util.NVC(e.Column.Name).Equals("REG_C") ||
                    Util.NVC(e.Column.Name).Equals("REG_F") ||
                    Util.NVC(e.Column.Name).Equals("DFCT_QTY_DDT_RATE"))
                {
                    string sFlag = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"));
                    if (sFlag == "Y")
                    {
                        e.Cancel = true;
                    }
                }
                
                if (e.Column.Name.Equals("REG_A") || e.Column.Name.Equals("REG_C"))
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

