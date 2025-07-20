/*************************************************************************************
 Created Date : 2016.12.12
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - 특이작업 : 투입Lot 종료 취소(노칭)
--------------------------------------------------------------------------------------
 [Change History]
  2016.12.12  INS 김동일K : Initial Created.
  
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LGC.GMES.MES.ASSY001
{
    /// <summary>
    /// ASSY001_021.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY001_021 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        private Util _Util = new Util();
        private BizDataSet _Biz = new BizDataSet();
        #endregion

        #region Initialize
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY001_021()
        {
            InitializeComponent();
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            C1ComboBox[] cboLineChild = { cboProcess };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter);

            C1ComboBox[] cboProcParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcChild = { cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbParent: cboProcParent, cbChild: cboProcChild);
            
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent);

            _combo.SetCombo(cboModel, CommonCombo.ComboStatus.ALL, sCase:"PRJT_NAME");

        }

        #endregion

        #region Event

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            InitCombo();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSave())
                return;

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("복구 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Save();
                }
            });            
        }

        private void dgLotListt_CurrentCellChanged(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {

        }

        private void dgLotListChoice_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSearch())
                return;

            GetLotList();
        }
        #endregion

        #region Mehod

        #region [BizCall]

        private void GetLotList()
        {
            try
            {   
                string sPrvLot = string.Empty;
                if (dgLotList.ItemsSource != null && dgLotList.Rows.Count > 0)
                {
                    int idx = _Util.GetDataGridCheckFirstRowIndex(dgLotList, "CHK");
                    if (idx >= 0)
                        sPrvLot = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[idx].DataItem, "LOTID"));
                }

                ClearControls();

                ShowLoadingIndicator();
                
                DataTable inTable = _Biz.GetDA_PRD_SEL_TERM_LOT_LIST_NT();

                DataRow newRow = inTable.NewRow();
                
                if (!txtLotID.Text.Trim().Equals(""))
                {
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["LOTID"] = txtLotID.Text.Trim();
                }
                else
                {
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["PROCID"] = cboProcess.SelectedValue.ToString();
                    newRow["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                    newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                    newRow["MODLID"] = cboModel.SelectedValue.ToString();
                }
                
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_TERM_LOT_LIST_NT", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.AlertByBiz("DA_PRD_SEL_TERM_LOT_LIST_NT", searchException.Message, searchException.ToString());
                            //LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(searchException.Message, searchException.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        dgLotList.ItemsSource = DataTableConverter.Convert(searchResult);
                        
                        if (!sPrvLot.Equals(""))
                        {
                            int idx = _Util.GetDataGridRowIndex(dgLotList, "LOTID", sPrvLot);

                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgLotList.Rows[idx].DataItem, "CHK", true);

                                //row 색 바꾸기
                                dgLotList.SelectedIndex = idx;

                                // Checked Event 사용 불가로 인해 CurrentCellChanged 사용함으로 발생하는 동일 Cell Cheked  후 Unchecked 시 Event 안타는 문제로 처리..
                                dgLotList.CurrentCell = dgLotList.GetCell(idx, dgLotList.Columns.Count - 1);
                            }
                            else
                            {
                                if (dgLotList.CurrentCell != null)
                                    dgLotList.CurrentCell = dgLotList.GetCell(dgLotList.CurrentCell.Row.Index, dgLotList.Columns.Count - 1);
                                else if (dgLotList.Rows.Count > 0)
                                    dgLotList.CurrentCell = dgLotList.GetCell(dgLotList.Rows.Count, dgLotList.Columns.Count - 1);
                            }
                        }
                        else
                        {
                            if (dgLotList.CurrentCell != null)
                                dgLotList.CurrentCell = dgLotList.GetCell(dgLotList.CurrentCell.Row.Index, dgLotList.Columns.Count - 1);
                            else if (dgLotList.Rows.Count > 0)
                                dgLotList.CurrentCell = dgLotList.GetCell(dgLotList.Rows.Count, dgLotList.Columns.Count - 1);
                        }
                    }
                    catch (Exception ex)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                        //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", Logger.MESSAGE_OPERATION_END);
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void Save()
        {
            try
            {
                ShowLoadingIndicator();

                dgLotList.EndEdit();

                DataSet indataSet = _Biz.GetBR_PRD_REG_RESURRECT();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();

                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable outLotTable = indataSet.Tables["IN_INPUT"];

                for (int i = 0; i < dgLotList.Rows.Count - dgLotList.BottomRows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgLotList, "CHK", i)) continue;

                    if (Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "WIPQTY")).Equals("") || Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "WIPQTY"))) <= 0)
                    {
                        Util.Alert("{0} 에 입력된 수량이 없습니다.", Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "LOTID")));
                        return;
                    }

                    newRow = outLotTable.NewRow();

                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "LOTID"));
                    newRow["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "WIPSEQ")).Equals("") ? 1 : int.Parse(Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "WIPSEQ")));
                    newRow["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "WIPQTY")));
                     
                    outLotTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteServiceSync_Multi("", "INDATA,IN_INPUT", null, indataSet);

                GetLotList();
                Util.AlertInfo("정상 처리 되었습니다.");
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("", ex.Message, ex.ToString());
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        #endregion

        #region [Validation]
        private bool CanSearch()
        {
            bool bRet = false;

            if (txtLotID.Text.Trim().Equals(""))
            {
                if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    Util.Alert("라인을 선택 하세요.");
                    return bRet;
                }

                if (cboProcess.SelectedIndex < 0 || cboProcess.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    Util.Alert("공정을 선택 하세요.");
                    return bRet;
                }

                if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    Util.Alert("설비를 선택 하세요.");
                    return bRet;
                }

                if (cboModel.SelectedIndex < 0 || cboModel.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    Util.Alert("모델을 선택 하세요.");
                    return bRet;
                }
            }

            bRet = true;
            return bRet;
        }

        private bool CanSave()
        {
            bool bRet = false;

            int idx = _Util.GetDataGridCheckFirstRowIndex(dgLotList, "CHK");
            if (idx < 0)
            {
                Util.Alert("선택된 항목이 없습니다.");
                return bRet;
            }

            if (txtReason.Text.Trim().Equals(""))
            {
                Util.Alert("복구사유를 입력 하세요.");
                return bRet;
            }

            bRet = true;
            return bRet;
        }
        #endregion

        #region [PopUp Event]
        #endregion

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

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        public bool ClearControls()
        {
            bool bRet = false;

            try
            {
                Util.gridClear(dgLotList);
                txtReason.Text = "";

                bRet = true;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                bRet = false;
            }
            return bRet;
        }
        #endregion

        #endregion


    }
}
