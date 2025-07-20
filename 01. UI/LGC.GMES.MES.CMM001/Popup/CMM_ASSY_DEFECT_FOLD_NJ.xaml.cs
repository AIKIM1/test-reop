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
    public partial class CMM_ASSY_DEFECT_FOLD_NJ : C1Window, IWorkArea
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
        private double gDfctLQty = 0;
        private double gDfctRQty = 0;
        private double gDfctMLQty = 0;
        private double gDfctMRQty = 0;

        private string _EQPT_DFCT_APPLY_FLAG = string.Empty;

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

        public CMM_ASSY_DEFECT_FOLD_NJ()
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

        private void dgDefect_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (!_ProcID.Equals(Process.STACKING_FOLDING))
                    return;

                double Reg_Atype;
                double Reg_Ctype;
                double Reg_Ltype;
                double Reg_Rtype;
                double Reg_MLtype;
                double Reg_MRtype;
                double Reg_Resnqty;

                double Division_A;
                double Division_C;
                double Division_L;
                double Division_R;
                double Division_MR;
                double Division_ML;

                string sEQP_DFCT_QTY = string.Empty;

                if (e.Cell.Column.Name.Equals("REG_A"))
                {
                    string sAtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_A"));
                    string sCtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_C"));
                    string sLtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_L"));
                    string sRtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_R"));
                    string sMLtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_ML"));
                    string sMRtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_MR"));
                    string sReg_FC = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_F"));

                    Reg_Atype = double.Parse(sAtype);
                    Reg_Ctype = double.Parse(sCtype);
                    Reg_Ltype = double.Parse(sLtype);
                    Reg_Rtype = double.Parse(sRtype);
                    Reg_MLtype = double.Parse(sMLtype);
                    Reg_MRtype = double.Parse(sMRtype);
                    Reg_Resnqty = double.Parse(sReg_FC);

                    int i = 0;
                    double Min = 0;
                    double Max = 0;
                    List<double> dIndex = new List<double>();

                    Division_A = Math.Floor(Reg_Atype / gDfctAQty);
                    dIndex.Add(Division_A);
                    Division_C = Math.Floor(Reg_Ctype / gDfctCQty);
                    dIndex.Add(Division_C);
                    Division_L = Math.Floor(Reg_Ltype / gDfctLQty);
                    dIndex.Add(Division_L);
                    Division_R = Math.Floor(Reg_Rtype / gDfctRQty);
                    dIndex.Add(Division_R);
                    i = 4;

                    double dSum = 0;

                    dSum = Division_A + Division_C + Division_L + Division_R;

                    if (gDfctMRQty != 0)
                    {
                        Division_MR = Math.Floor(Reg_MRtype / gDfctMRQty);
                        dSum = dSum + Division_MR;
                        i = i + 1;
                        dIndex.Add(Division_MR);
                    }

                    if (gDfctMLQty != 0)
                    {
                        Division_ML = Math.Floor(Reg_MLtype / gDfctMLQty);
                        dSum = dSum + Division_ML;
                        i = i + 1;
                        dIndex.Add(Division_ML);
                    }

                    Max = dIndex[0];
                    Min = dIndex[0];

                    for (int icnt = 0; icnt < i; icnt++)
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

                    // 0 이 존재시
                    if (Reg_Ctype == 0)
                    {
                        if ((double.Parse(txtLType.Text) == 0 && Reg_Ltype == 0) || (double.Parse(txtRType.Text) == 0 && Reg_Rtype == 0) || 
                            (double.Parse(txtMLType.Text) == 0 && Reg_MLtype == 0) || (double.Parse(txtMRType.Text) == 0 && Reg_MRtype == 0))
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_A", Reg_Atype);
                        }
                    }
                    // 모두 0 아닐때
                    else if (Reg_Ctype != 0)
                    {
                        DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_A", Reg_Atype - (Min * gDfctAQty));
                        DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_C", Reg_Ctype - (Min * gDfctCQty));
                        DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Reg_Resnqty + Min);

                        if (double.Parse(txtLType.Text) != 0 && Reg_Ltype != 0)
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_L", Reg_Ltype - (Min * gDfctLQty));
                        }

                        if (double.Parse(txtRType.Text) != 0 && Reg_Rtype != 0)
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_R", Reg_Rtype - (Min * gDfctRQty));
                        }

                        if (double.Parse(txtMLType.Text) != 0 && Reg_MLtype != 0)
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_ML", Reg_MLtype - (Min * gDfctMLQty));
                        }

                        if (double.Parse(txtMRType.Text) != 0 && Reg_MRtype != 0)
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_MR", Reg_MRtype - (Min * gDfctMRQty));
                        }
                    }
                }
                else if (e.Cell.Column.Name.Equals("REG_C"))
                {
                    string sAtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_A"));
                    string sCtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_C"));
                    string sLtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_L"));
                    string sRtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_R"));
                    string sMLtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_ML"));
                    string sMRtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_MR"));
                    string sReg_FC = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_F"));

                    Reg_Atype = double.Parse(sAtype);
                    Reg_Ctype = double.Parse(sCtype);
                    Reg_Ltype = double.Parse(sLtype);
                    Reg_Rtype = double.Parse(sRtype);
                    Reg_MLtype = double.Parse(sMLtype);
                    Reg_MRtype = double.Parse(sMRtype);
                    Reg_Resnqty = double.Parse(sReg_FC);

                    int i = 0;
                    double Min = 0;
                    double Max = 0;
                    List<double> dIndex = new List<double>();

                    Division_A = Math.Floor(Reg_Atype / gDfctAQty);
                    dIndex.Add(Division_A);
                    Division_C = Math.Floor(Reg_Ctype / gDfctCQty);
                    dIndex.Add(Division_C);
                    Division_L = Math.Floor(Reg_Ltype / gDfctLQty);
                    dIndex.Add(Division_L);
                    Division_R = Math.Floor(Reg_Rtype / gDfctRQty);
                    dIndex.Add(Division_R);
                    i = 4;

                    double dSum = 0;

                    dSum = Division_A + Division_C + Division_L + Division_R;

                    if (gDfctMRQty != 0)
                    {
                        Division_MR = Math.Floor(Reg_MRtype / gDfctMRQty);
                        dSum = dSum + Division_MR;
                        i = i + 1;
                        dIndex.Add(Division_MR);
                    }

                    if (gDfctMLQty != 0)
                    {
                        Division_ML = Math.Floor(Reg_MLtype / gDfctMLQty);
                        dSum = dSum + Division_ML;
                        i = i + 1;
                        dIndex.Add(Division_ML);
                    }

                    Max = dIndex[0];
                    Min = dIndex[0];

                    for (int icnt = 0; icnt < i; icnt++)
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

                    // 0 이 존재시
                    if (Reg_Atype == 0)
                    {
                        if ((double.Parse(txtLType.Text) == 0 && Reg_Ltype == 0) || (double.Parse(txtRType.Text) == 0 && Reg_Rtype == 0) ||
                            (double.Parse(txtMLType.Text) == 0 && Reg_MLtype == 0) || (double.Parse(txtMRType.Text) == 0 && Reg_MRtype == 0))
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_C", Reg_Ctype);
                        }
                    }
                    // 모두 0 아닐때
                    else if (Reg_Atype != 0 )
                    {
                        DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_A", Reg_Atype - (Min * gDfctAQty));
                        DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_C", Reg_Ctype - (Min * gDfctCQty));
                        DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Reg_Resnqty + Min);

                        if (double.Parse(txtLType.Text) != 0 && Reg_Ltype != 0)
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_L", Reg_Ltype - (Min * gDfctLQty));
                        }

                        if (double.Parse(txtRType.Text) != 0 && Reg_Rtype != 0)
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_R", Reg_Rtype - (Min * gDfctRQty));
                        }

                        if (double.Parse(txtMLType.Text) != 0 && Reg_MLtype != 0)
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_ML", Reg_MLtype - (Min * gDfctMLQty));
                        }

                        if (double.Parse(txtMRType.Text) != 0 && Reg_MRtype != 0)
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_MR", Reg_MRtype - (Min * gDfctMRQty));
                        }
                    }
                }
                else if (e.Cell.Column.Name.Equals("REG_L"))
                {
                    string sAtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_A"));
                    string sCtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_C"));
                    string sLtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_L"));
                    string sRtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_R"));
                    string sMLtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_ML"));
                    string sMRtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_MR"));
                    string sReg_FC = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_F"));

                    Reg_Atype = double.Parse(sAtype);
                    Reg_Ctype = double.Parse(sCtype);
                    Reg_Ltype = double.Parse(sLtype);
                    Reg_Rtype = double.Parse(sRtype);
                    Reg_MLtype = double.Parse(sMLtype);
                    Reg_MRtype = double.Parse(sMRtype);
                    Reg_Resnqty = double.Parse(sReg_FC);

                    int i = 0;
                    double Min = 0;
                    double Max = 0;
                    List<double> dIndex = new List<double>();

                    Division_A = Math.Floor(Reg_Atype / gDfctAQty);
                    dIndex.Add(Division_A);
                    Division_C = Math.Floor(Reg_Ctype / gDfctCQty);
                    dIndex.Add(Division_C);
                    Division_L = Math.Floor(Reg_Ltype / gDfctLQty);
                    dIndex.Add(Division_L);
                    Division_R = Math.Floor(Reg_Rtype / gDfctRQty);
                    dIndex.Add(Division_R);
                    i = 4;

                    double dSum = 0;

                    dSum = Division_A + Division_C + Division_L + Division_R;

                    if (gDfctMRQty != 0)
                    {
                        Division_MR = Math.Floor(Reg_MRtype / gDfctMRQty);
                        dSum = dSum + Division_MR;
                        i = i + 1;
                        dIndex.Add(Division_MR);
                    }

                    if (gDfctMLQty != 0)
                    {
                        Division_ML = Math.Floor(Reg_MLtype / gDfctMLQty);
                        dSum = dSum + Division_ML;
                        i = i + 1;
                        dIndex.Add(Division_ML);
                    }

                    Max = dIndex[0];
                    Min = dIndex[0];

                    for (int icnt = 0; icnt < i; icnt++)
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

                    // 0 이 존재시
                    if (Reg_Atype == 0 || Reg_Ctype == 0)
                    {
                        if ((double.Parse(txtLType.Text) == 0 && Reg_Ltype == 0) || (double.Parse(txtRType.Text) == 0 && Reg_Rtype == 0) ||
                            (double.Parse(txtMLType.Text) == 0 && Reg_MLtype == 0) || (double.Parse(txtMRType.Text) == 0 && Reg_MRtype == 0))
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_L", Reg_Ltype);
                        }
                    }
                    // 모두 0 아닐때
                    else if (Reg_Atype != 0)
                    {
                        DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_A", Reg_Atype - (Min * gDfctAQty));
                        DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_C", Reg_Ctype - (Min * gDfctCQty));
                        DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Reg_Resnqty + Min);

                        //if (double.Parse(txtLType.Text) != 0 && Reg_Ltype != 0)
                        //{
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_L", Reg_Ltype - (Min * gDfctLQty));
                        //}

                        if (double.Parse(txtRType.Text) != 0 && Reg_Rtype != 0)
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_R", Reg_Rtype - (Min * gDfctRQty));
                        }

                        if (double.Parse(txtMLType.Text) != 0 && Reg_MLtype != 0)
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_ML", Reg_MLtype - (Min * gDfctMLQty));
                        }

                        if (double.Parse(txtMRType.Text) != 0 && Reg_MRtype != 0)
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_MR", Reg_MRtype - (Min * gDfctMRQty));
                        }
                    }
                }
                else if (e.Cell.Column.Name.Equals("REG_R"))
                {
                    string sAtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_A"));
                    string sCtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_C"));
                    string sLtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_L"));
                    string sRtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_R"));
                    string sMLtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_ML"));
                    string sMRtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_MR"));
                    string sReg_FC = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_F"));

                    Reg_Atype = double.Parse(sAtype);
                    Reg_Ctype = double.Parse(sCtype);
                    Reg_Ltype = double.Parse(sLtype);
                    Reg_Rtype = double.Parse(sRtype);
                    Reg_MLtype = double.Parse(sMLtype);
                    Reg_MRtype = double.Parse(sMRtype);
                    Reg_Resnqty = double.Parse(sReg_FC);

                    int i = 0;
                    double Min = 0;
                    double Max = 0;
                    List<double> dIndex = new List<double>();

                    Division_A = Math.Floor(Reg_Atype / gDfctAQty);
                    dIndex.Add(Division_A);
                    Division_C = Math.Floor(Reg_Ctype / gDfctCQty);
                    dIndex.Add(Division_C);
                    Division_L = Math.Floor(Reg_Ltype / gDfctLQty);
                    dIndex.Add(Division_L);
                    Division_R = Math.Floor(Reg_Rtype / gDfctRQty);
                    dIndex.Add(Division_R);
                    i = 4;

                    double dSum = 0;

                    dSum = Division_A + Division_C + Division_L + Division_R;

                    if (gDfctMRQty != 0)
                    {
                        Division_MR = Math.Floor(Reg_MRtype / gDfctMRQty);
                        dSum = dSum + Division_MR;
                        i = i + 1;
                        dIndex.Add(Division_MR);
                    }

                    if (gDfctMLQty != 0)
                    {
                        Division_ML = Math.Floor(Reg_MLtype / gDfctMLQty);
                        dSum = dSum + Division_ML;
                        i = i + 1;
                        dIndex.Add(Division_ML);
                    }

                    Max = dIndex[0];
                    Min = dIndex[0];

                    for (int icnt = 0; icnt < i; icnt++)
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

                    // 0 이 존재시
                    if (Reg_Atype == 0 || Reg_Ctype == 0)
                    {
                        if ((double.Parse(txtLType.Text) == 0 && Reg_Ltype == 0) || (double.Parse(txtRType.Text) == 0 && Reg_Rtype == 0) ||
                            (double.Parse(txtMLType.Text) == 0 && Reg_MLtype == 0) || (double.Parse(txtMRType.Text) == 0 && Reg_MRtype == 0))
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_R", Reg_Rtype);
                        }
                    }
                    // 모두 0 아닐때
                    else if (Reg_Atype != 0)
                    {
                        DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_A", Reg_Atype - (Min * gDfctAQty));
                        DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_C", Reg_Ctype - (Min * gDfctCQty));
                        DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Reg_Resnqty + Min);

                        if (double.Parse(txtLType.Text) != 0 && Reg_Ltype != 0)
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_L", Reg_Ltype - (Min * gDfctLQty));
                        }

                        //if (double.Parse(txtRType.Text) != 0 && Reg_Rtype != 0)
                        //{
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_R", Reg_Rtype - (Min * gDfctRQty));
                        //}

                        if (double.Parse(txtMLType.Text) != 0 && Reg_MLtype != 0)
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_ML", Reg_MLtype - (Min * gDfctMLQty));
                        }

                        if (double.Parse(txtMRType.Text) != 0 && Reg_MRtype != 0)
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_MR", Reg_MRtype - (Min * gDfctMRQty));
                        }
                    }
                }
                else if (e.Cell.Column.Name.Equals("REG_ML"))
                {
                    string sAtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_A"));
                    string sCtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_C"));
                    string sLtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_L"));
                    string sRtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_R"));
                    string sMLtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_ML"));
                    string sMRtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_MR"));
                    string sReg_FC = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_F"));

                    Reg_Atype = double.Parse(sAtype);
                    Reg_Ctype = double.Parse(sCtype);
                    Reg_Ltype = double.Parse(sLtype);
                    Reg_Rtype = double.Parse(sRtype);
                    Reg_MLtype = double.Parse(sMLtype);
                    Reg_MRtype = double.Parse(sMRtype);
                    Reg_Resnqty = double.Parse(sReg_FC);

                    int i = 0;
                    double Min = 0;
                    double Max = 0;
                    List<double> dIndex = new List<double>();

                    Division_A = Math.Floor(Reg_Atype / gDfctAQty);
                    dIndex.Add(Division_A);
                    Division_C = Math.Floor(Reg_Ctype / gDfctCQty);
                    dIndex.Add(Division_C);
                    Division_L = Math.Floor(Reg_Ltype / gDfctLQty);
                    dIndex.Add(Division_L);
                    Division_R = Math.Floor(Reg_Rtype / gDfctRQty);
                    dIndex.Add(Division_R);
                    i = 4;

                    double dSum = 0;

                    dSum = Division_A + Division_C + Division_L + Division_R;

                    if (gDfctMRQty != 0)
                    {
                        Division_MR = Math.Floor(Reg_MRtype / gDfctMRQty);
                        dSum = dSum + Division_MR;
                        i = i + 1;
                        dIndex.Add(Division_MR);
                    }

                    if (gDfctMLQty != 0)
                    {
                        Division_ML = Math.Floor(Reg_MLtype / gDfctMLQty);
                        dSum = dSum + Division_ML;
                        i = i + 1;
                        dIndex.Add(Division_ML);
                    }

                    Max = dIndex[0];
                    Min = dIndex[0];

                    for (int icnt = 0; icnt < i; icnt++)
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

                    // 0 이 존재시
                    if (Reg_Atype == 0 || Reg_Ctype == 0)
                    {
                        if ((double.Parse(txtLType.Text) == 0 && Reg_Ltype == 0) || (double.Parse(txtRType.Text) == 0 && Reg_Rtype == 0) ||
                            (double.Parse(txtMLType.Text) == 0 && Reg_MLtype == 0) || (double.Parse(txtMRType.Text) == 0 && Reg_MRtype == 0))
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_ML", Reg_MLtype);
                        }
                    }
                    // 모두 0 아닐때
                    else if (Reg_Atype != 0)
                    {
                        DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_A", Reg_Atype - (Min * gDfctAQty));
                        DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_C", Reg_Ctype - (Min * gDfctCQty));
                        DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Reg_Resnqty + Min);

                        if (double.Parse(txtLType.Text) != 0 && Reg_Ltype != 0)
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_L", Reg_Ltype - (Min * gDfctLQty));
                        }

                        if (double.Parse(txtRType.Text) != 0 && Reg_Rtype != 0)
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_R", Reg_Rtype - (Min * gDfctRQty));
                        }

                        //if (double.Parse(txtMLType.Text) != 0 && Reg_MLtype != 0)
                        //{
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_ML", Reg_MLtype - (Min * gDfctMLQty));
                        //}

                        if (double.Parse(txtMRType.Text) != 0 && Reg_MRtype != 0)
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_MR", Reg_MRtype - (Min * gDfctMRQty));
                        }
                    }
                }
                else if (e.Cell.Column.Name.Equals("REG_MR"))
                {
                    string sAtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_A"));
                    string sCtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_C"));
                    string sLtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_L"));
                    string sRtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_R"));
                    string sMLtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_ML"));
                    string sMRtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_MR"));
                    string sReg_FC = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_F"));

                    Reg_Atype = double.Parse(sAtype);
                    Reg_Ctype = double.Parse(sCtype);
                    Reg_Ltype = double.Parse(sLtype);
                    Reg_Rtype = double.Parse(sRtype);
                    Reg_MLtype = double.Parse(sMLtype);
                    Reg_MRtype = double.Parse(sMRtype);
                    Reg_Resnqty = double.Parse(sReg_FC);

                    int i = 0;
                    double Min = 0;
                    double Max = 0;
                    List<double> dIndex = new List<double>();

                    Division_A = Math.Floor(Reg_Atype / gDfctAQty);
                    dIndex.Add(Division_A);
                    Division_C = Math.Floor(Reg_Ctype / gDfctCQty);
                    dIndex.Add(Division_C);
                    Division_L = Math.Floor(Reg_Ltype / gDfctLQty);
                    dIndex.Add(Division_L);
                    Division_R = Math.Floor(Reg_Rtype / gDfctRQty);
                    dIndex.Add(Division_R);
                    i = 4;

                    double dSum = 0;

                    dSum = Division_A + Division_C + Division_L + Division_R;

                    if (gDfctMRQty != 0)
                    {
                        Division_MR = Math.Floor(Reg_MRtype / gDfctMRQty);
                        dSum = dSum + Division_MR;
                        i = i + 1;
                        dIndex.Add(Division_MR);
                    }

                    if (gDfctMLQty != 0)
                    {
                        Division_ML = Math.Floor(Reg_MLtype / gDfctMLQty);
                        dSum = dSum + Division_ML;
                        i = i + 1;
                        dIndex.Add(Division_ML);
                    }

                    Max = dIndex[0];
                    Min = dIndex[0];

                    for (int icnt = 0; icnt < i; icnt++)
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

                    // 0 이 존재시
                    if (Reg_Atype == 0 || Reg_Ctype == 0)
                    {
                        if ((double.Parse(txtLType.Text) == 0 && Reg_Ltype == 0) || (double.Parse(txtRType.Text) == 0 && Reg_Rtype == 0) ||
                            (double.Parse(txtMLType.Text) == 0 && Reg_MLtype == 0) || (double.Parse(txtMRType.Text) == 0 && Reg_MRtype == 0))
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_MR", Reg_MRtype);
                        }
                    }
                    // 모두 0 아닐때
                    else if (Reg_Atype != 0)
                    {
                        DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_A", Reg_Atype - (Min * gDfctAQty));
                        DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_C", Reg_Ctype - (Min * gDfctCQty));
                        DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Reg_Resnqty + Min);

                        if (double.Parse(txtLType.Text) != 0 && Reg_Ltype != 0)
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_L", Reg_Ltype - (Min * gDfctLQty));
                        }

                        if (double.Parse(txtRType.Text) != 0 && Reg_Rtype != 0)
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_R", Reg_Rtype - (Min * gDfctRQty));
                        }

                        if (double.Parse(txtMLType.Text) != 0 && Reg_MLtype != 0)
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_ML", Reg_MLtype - (Min * gDfctMLQty));
                        }

                        //if (double.Parse(txtMRType.Text) != 0 && Reg_MRtype != 0)
                        //{
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_MR", Reg_MRtype - (Min * gDfctMRQty));
                        //}
                    }
                }
                else if (e.Cell.Column.Name.Equals("REG_F"))
                {
                    string sAtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_A"));
                    string sCtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_C"));
                    string sLtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_L"));
                    string sRtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_R"));
                    string sMLtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_ML"));
                    string sMRtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_MR"));
                    string sReg_FC = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG_F"));

                    Reg_Atype = double.Parse(sAtype);
                    Reg_Ctype = double.Parse(sCtype);
                    Reg_Ltype = double.Parse(sLtype);
                    Reg_Rtype = double.Parse(sRtype);
                    Reg_MLtype = double.Parse(sMLtype);
                    Reg_MRtype = double.Parse(sMRtype);
                    Reg_Resnqty = double.Parse(sReg_FC);

                    double dSum = 0;

                    int i = 0;
                    double Min = 0;
                    double Max = 0;
                    List<double> dIndex = new List<double>();

                    Division_A = Math.Floor(Reg_Atype / gDfctAQty);
                    dIndex.Add(Division_A);
                    Division_C = Math.Floor(Reg_Ctype / gDfctCQty);
                    dIndex.Add(Division_C);
                    Division_L = Math.Floor(Reg_Ltype / gDfctLQty);
                    dIndex.Add(Division_L);
                    Division_R = Math.Floor(Reg_Rtype / gDfctRQty);
                    dIndex.Add(Division_R);
                    i = 4;

                    dSum = Division_A + Division_C + Division_L + Division_R;

                    if (gDfctMRQty != 0)
                    {
                        Division_MR = Math.Floor(Reg_MRtype / gDfctMRQty);
                        dSum = dSum + Division_MR;
                        i = i + 1;
                        dIndex.Add(Division_MR);
                    }

                    if (gDfctMLQty != 0)
                    {
                        Division_ML = Math.Floor(Reg_MLtype / gDfctMLQty);
                        dSum = dSum + Division_ML;
                        i = i + 1;
                        dIndex.Add(Division_ML);
                    }

                    Max = dIndex[0];
                    Min = dIndex[0];

                    for (int icnt = 0; icnt < i; icnt++)
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

                    // 모두 0 일때
                    if (Reg_Atype == 0 || Reg_Ctype == 0)
                    {
                        if ((double.Parse(txtLType.Text) == 0 && Reg_Ltype == 0) || (double.Parse(txtRType.Text) == 0 && Reg_Rtype == 0) ||
                            (double.Parse(txtMLType.Text) == 0 && Reg_MLtype == 0) || (double.Parse(txtMRType.Text) == 0 && Reg_MRtype == 0))
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Reg_Resnqty);
                        }
                    }
                    // 모두 0 아닐때
                    else if (Reg_Atype != 0 && Reg_Ctype != 0)
                    {
                        if ((double.Parse(txtLType.Text) != 0 && Reg_Ltype != 0) || (double.Parse(txtRType.Text) != 0 && Reg_Rtype != 0) ||
                            (double.Parse(txtMLType.Text) != 0 && Reg_MLtype != 0) || (double.Parse(txtMRType.Text) != 0 && Reg_MRtype != 0))
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Reg_Resnqty + Min);
                        }
                        else if ((double.Parse(txtLType.Text) == 0 && Reg_Ltype == 0) || (double.Parse(txtRType.Text) == 0 && Reg_Rtype == 0) ||
                            (double.Parse(txtMLType.Text) == 0 && Reg_MLtype == 0) || (double.Parse(txtMRType.Text) == 0 && Reg_MRtype == 0))
                        {
                            DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Reg_Resnqty + Min);
                        }
                    }
                    //else if (Reg_Atype != 0 || Reg_Ctype != 0)
                    //{
                    //    if ((double.Parse(txtMLType.Text) != 0 && Reg_MLtype != 0) || (double.Parse(txtMRType.Text) != 0 && Reg_MRtype != 0))
                    //    {
                    //        DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Reg_Resnqty + Min);
                    //    }
                    //    else if ((double.Parse(txtMLType.Text) == 0 && Reg_MLtype == 0) || (double.Parse(txtMRType.Text) == 0 && Reg_MRtype == 0))
                    //    {
                    //        DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_F", Reg_Resnqty + Min);
                    //    }
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
                            for (int i = 0; i < searchResult.Rows.Count; i++)
                            {
                                if (Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL3_CODE"]).Equals("AT"))
                                {
                                    txtAType.Text = double.Parse(Util.NVC(searchResult.Rows[i]["PROC_INPUT_CNT"])).ToString();
                                    txtAType.Visibility = Visibility.Visible;
                                    dgDefect.Columns["REG_A"].Visibility = Visibility.Visible;
                                    dgDefect.Columns["CALC_A"].Visibility = Visibility.Visible;
                                    gDfctAQty = double.Parse(txtAType.Text);

                                    if (gDfctAQty == 0)
                                    {
                                        Util.MessageValidation("SFU1306");        //ATYPE 불량 기준정보가 존재하지 않습니다.                                        
                                        return;
                                    }
                                }
                                else if (Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL3_CODE"]).Equals("CT"))
                                {
                                    txtCType.Text = double.Parse(Util.NVC(searchResult.Rows[i]["PROC_INPUT_CNT"])).ToString();
                                    txtCType.Visibility = Visibility.Visible;
                                    dgDefect.Columns["REG_C"].Visibility = Visibility.Visible;
                                    dgDefect.Columns["CALC_C"].Visibility = Visibility.Visible;
                                    gDfctCQty = double.Parse(txtCType.Text);

                                    if (gDfctCQty == 0)
                                    {
                                        Util.MessageValidation("SFU1326");        //CTYPE 불량 기준정보가 존재하지 않습니다.
                                        return;
                                    }
                                }
                                else if (Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL3_CODE"]).Equals("LT"))
                                {
                                    txtLType.Text = double.Parse(Util.NVC(searchResult.Rows[i]["PROC_INPUT_CNT"])).ToString();
                                    txtLType.Visibility = Visibility.Visible;
                                    tbLType.Visibility = Visibility.Visible;
                                    dgDefect.Columns["REG_L"].Visibility = Visibility.Visible;
                                    dgDefect.Columns["CALC_L"].Visibility = Visibility.Visible;
                                    gDfctLQty = double.Parse(txtLType.Text);

                                    if (gDfctLQty == 0)
                                    {
                                        Util.MessageValidation("SFU4472", "LTYPE");  // [%1] 불량 기준정보가 존재하지 않습니다.
                                        return;
                                    }
                                }
                                else if (Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL3_CODE"]).Equals("RT"))
                                {
                                    txtRType.Text = double.Parse(Util.NVC(searchResult.Rows[i]["PROC_INPUT_CNT"])).ToString();
                                    txtRType.Visibility = Visibility.Visible;
                                    tbRType.Visibility = Visibility.Visible;
                                    dgDefect.Columns["REG_R"].Visibility = Visibility.Visible;
                                    dgDefect.Columns["CALC_R"].Visibility = Visibility.Visible;
                                    gDfctRQty = double.Parse(txtRType.Text);

                                    if (gDfctRQty == 0)
                                    {
                                        Util.MessageValidation("SFU4472", "RTYPE");  // [%1] 불량 기준정보가 존재하지 않습니다.
                                        return;
                                    }
                                }
                                else if (Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL3_CODE"]).Equals("ML"))
                                {
                                    txtMLType.Text = double.Parse(Util.NVC(searchResult.Rows[i]["PROC_INPUT_CNT"])).ToString();
                                    txtMLType.Visibility = Visibility.Visible;
                                    tbMLType.Visibility = Visibility.Visible;
                                    dgDefect.Columns["REG_ML"].Visibility = Visibility.Visible;
                                    dgDefect.Columns["CALC_ML"].Visibility = Visibility.Visible;
                                    gDfctMLQty = double.Parse(txtMLType.Text);

                                    if (gDfctMLQty == 0)
                                    {
                                        Util.MessageValidation("SFU4472", "MLTYPE");  // [%1] 불량 기준정보가 존재하지 않습니다.
                                        return;
                                    }
                                }
                                else if (Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL3_CODE"]).Equals("MR"))
                                {
                                    txtMRType.Text = double.Parse(Util.NVC(searchResult.Rows[i]["PROC_INPUT_CNT"])).ToString();
                                    txtMRType.Visibility = Visibility.Visible;
                                    tbMRType.Visibility = Visibility.Visible;
                                    dgDefect.Columns["REG_MR"].Visibility = Visibility.Visible;
                                    dgDefect.Columns["CALC_MR"].Visibility = Visibility.Visible;
                                    gDfctMRQty = double.Parse(txtMRType.Text);

                                    if (gDfctMRQty == 0)
                                    {
                                        Util.MessageValidation("SFU4472", "MRTYPE");  // [%1] 불량 기준정보가 존재하지 않습니다.
                                        return;
                                    }
                                }
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

        private void GetDefectInfo()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                const string bizRuleName = "DA_QCA_SEL_CELL_TYPE_DFCT_HIST_NJ";

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

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Defect_Sum();

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
                string sInLType = string.Empty;
                string sInRType = string.Empty;
                string sInMLType = string.Empty;
                string sInMRType = string.Empty;

                string sOut = string.Empty;


                //string sAtype = string.Empty;
                //string sCtype = string.Empty;
                //string sFtype = string.Empty;

                DataTable RQSTDT1 = new DataTable();
                RQSTDT1.TableName = "RQSTDT";
                RQSTDT1.Columns.Add("LOTID", typeof(String));
                RQSTDT1.Columns.Add("SHOPID", typeof(String));
                RQSTDT1.Columns.Add("AREAID", typeof(String));

                DataRow dr1 = RQSTDT1.NewRow();
                dr1["LOTID"] = _LotID;
                dr1["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                RQSTDT1.Rows.Add(dr1);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_PRODUCT_LEVEL_CODE_INFO", "RQSTDT", "RSLTDT", RQSTDT1);

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
                    else if (Util.NVC(SearchResult.Rows[i]["PRODUCT_LEVEL3_CODE"]).Equals("LT"))
                    {
                        sInLType = Util.NVC(SearchResult.Rows[i]["PRODID"]).ToString();
                    }
                    else if (Util.NVC(SearchResult.Rows[i]["PRODUCT_LEVEL3_CODE"]).Equals("RT"))
                    {
                        sInRType = Util.NVC(SearchResult.Rows[i]["PRODID"]).ToString();
                    }
                    else if (Util.NVC(SearchResult.Rows[i]["PRODUCT_LEVEL3_CODE"]).Equals("ML"))
                    {
                        sInMLType = Util.NVC(SearchResult.Rows[i]["PRODID"]).ToString();
                    }
                    else if (Util.NVC(SearchResult.Rows[i]["PRODUCT_LEVEL3_CODE"]).Equals("MR"))
                    {
                        sInMRType = Util.NVC(SearchResult.Rows[i]["PRODID"]).ToString();
                    }
                    else if (Util.NVC(SearchResult.Rows[i]["PRODUCT_LEVEL3_CODE"]).Equals("FC"))
                    {
                        sOut = Util.NVC(SearchResult.Rows[i]["PRODID"]).ToString();
                    }
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(String));
                RQSTDT.Columns.Add("WIPSEQ", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));
                RQSTDT.Columns.Add("ACTID", typeof(String));
                RQSTDT.Columns.Add("RESNCODE", typeof(String));
                RQSTDT.Columns.Add("REG_QTY", typeof(String));
                RQSTDT.Columns.Add("CALC_QTY", typeof(String));
                RQSTDT.Columns.Add("USERID", typeof(String));

                for (int icnt = 2; icnt < dgDefect.GetRowCount() + 2; icnt++)
                {
                    for (int jcnt = 0; jcnt < SearchResult.Rows.Count; jcnt++)
                    {
                        if (Util.NVC(SearchResult.Rows[jcnt]["PRODUCT_LEVEL3_CODE"]).Equals("AT"))
                        {
                            DataRow dr = RQSTDT.NewRow();
                            dr["LOTID"] = _LotID;
                            dr["WIPSEQ"] = int.TryParse(_WipSeq, out iSeq) ? iSeq : 1;
                            dr["PRODID"] = sInAType;
                            dr["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "ACTID"));
                            dr["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "RESNCODE"));
                            dr["REG_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "REG_A"));
                            dr["CALC_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "CALC_A"));
                            dr["USERID"] = LoginInfo.USERID;

                            RQSTDT.Rows.Add(dr);
                        }
                        if (Util.NVC(SearchResult.Rows[jcnt]["PRODUCT_LEVEL3_CODE"]).Equals("CT"))
                        {
                            DataRow dr = RQSTDT.NewRow();
                            dr["LOTID"] = _LotID;
                            dr["WIPSEQ"] = int.TryParse(_WipSeq, out iSeq) ? iSeq : 1;
                            dr["PRODID"] = sInCType;
                            dr["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "ACTID"));
                            dr["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "RESNCODE"));
                            dr["REG_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "REG_C"));
                            dr["CALC_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "CALC_C"));
                            dr["USERID"] = LoginInfo.USERID;

                            RQSTDT.Rows.Add(dr);
                        }
                        if (Util.NVC(SearchResult.Rows[jcnt]["PRODUCT_LEVEL3_CODE"]).Equals("LT"))
                        {
                            DataRow dr = RQSTDT.NewRow();
                            dr["LOTID"] = _LotID;
                            dr["WIPSEQ"] = int.TryParse(_WipSeq, out iSeq) ? iSeq : 1;
                            dr["PRODID"] = sInLType;
                            dr["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "ACTID"));
                            dr["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "RESNCODE"));
                            dr["REG_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "REG_L"));
                            dr["CALC_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "CALC_L"));
                            dr["USERID"] = LoginInfo.USERID;

                            RQSTDT.Rows.Add(dr);
                        }
                        if (Util.NVC(SearchResult.Rows[jcnt]["PRODUCT_LEVEL3_CODE"]).Equals("RT"))
                        {
                            DataRow dr = RQSTDT.NewRow();
                            dr["LOTID"] = _LotID;
                            dr["WIPSEQ"] = int.TryParse(_WipSeq, out iSeq) ? iSeq : 1;
                            dr["PRODID"] = sInRType;
                            dr["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "ACTID"));
                            dr["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "RESNCODE"));
                            dr["REG_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "REG_R"));
                            dr["CALC_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "CALC_R"));
                            dr["USERID"] = LoginInfo.USERID;

                            RQSTDT.Rows.Add(dr);
                        }
                        if (Util.NVC(SearchResult.Rows[jcnt]["PRODUCT_LEVEL3_CODE"]).Equals("ML"))
                        {
                            DataRow dr = RQSTDT.NewRow();
                            dr["LOTID"] = _LotID;
                            dr["WIPSEQ"] = int.TryParse(_WipSeq, out iSeq) ? iSeq : 1;
                            dr["PRODID"] = sInMLType;
                            dr["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "ACTID"));
                            dr["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "RESNCODE"));
                            dr["REG_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "REG_ML"));
                            dr["CALC_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "CALC_ML"));
                            dr["USERID"] = LoginInfo.USERID;

                            RQSTDT.Rows.Add(dr);
                        }
                        if (Util.NVC(SearchResult.Rows[jcnt]["PRODUCT_LEVEL3_CODE"]).Equals("MR"))
                        {
                            DataRow dr = RQSTDT.NewRow();
                            dr["LOTID"] = _LotID;
                            dr["WIPSEQ"] = int.TryParse(_WipSeq, out iSeq) ? iSeq : 1;
                            dr["PRODID"] = sInMRType;
                            dr["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "ACTID"));
                            dr["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "RESNCODE"));
                            dr["REG_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "REG_MR"));
                            dr["CALC_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "CALC_MR"));
                            dr["USERID"] = LoginInfo.USERID;

                            RQSTDT.Rows.Add(dr);
                        }
                        else if (Util.NVC(SearchResult.Rows[jcnt]["PRODUCT_LEVEL3_CODE"]).Equals("FC"))
                        {
                            DataRow dr = RQSTDT.NewRow();
                            dr["LOTID"] = _LotID;
                            dr["WIPSEQ"] = int.TryParse(_WipSeq, out iSeq) ? iSeq : 1;
                            dr["PRODID"] = sOut;
                            dr["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "ACTID"));
                            dr["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "RESNCODE"));
                            dr["REG_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "REG_F"));

                            if (_EQPT_DFCT_APPLY_FLAG == "Y")
                            {
                                if (double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "REG_A"))) == 0 &&
                                    double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "REG_C"))) == 0 &&
                                    double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "REG_L"))) == 0 &&
                                    double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "REG_R"))) == 0 &&
                                    double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "REG_ML"))) == 0 &&
                                    double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "REG_MR"))) == 0 &&
                                    double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "REG_F"))) == 0)
                                    //dr["CALC_QTY"] = double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "CALC_F"))) + double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "EQP_DFCT_QTY")));
                                    dr["CALC_QTY"] = double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "EQP_DFCT_QTY")));
                                else
                                    dr["CALC_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "CALC_F"));
                            }
                            else
                            {
                                dr["CALC_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "CALC_F"));
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
                                    double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "REG_L"))) == 0 &&
                                    double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "REG_R"))) == 0 &&
                                    double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "REG_ML"))) == 0 &&
                                    double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "REG_MR"))) == 0 &&
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
                    if (Convert.ToString(e.Cell.Column.Name) == "REG_A" || Convert.ToString(e.Cell.Column.Name) == "REG_C" || Convert.ToString(e.Cell.Column.Name) == "REG_L" 
                    || Convert.ToString(e.Cell.Column.Name) == "REG_R" || Convert.ToString(e.Cell.Column.Name) == "REG_ML" || Convert.ToString(e.Cell.Column.Name) == "REG_MR")
                    {
                        string sActid = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "ACTID"));
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
                    else if (Convert.ToString(e.Cell.Column.Name) == "REG_F")
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
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

                if (e.Column.Name.Equals("REG_A") || e.Column.Name.Equals("REG_C") || e.Column.Name.Equals("REG_L") || e.Column.Name.Equals("REG_R")
                    || e.Column.Name.Equals("REG_ML") || e.Column.Name.Equals("REG_MR"))
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

