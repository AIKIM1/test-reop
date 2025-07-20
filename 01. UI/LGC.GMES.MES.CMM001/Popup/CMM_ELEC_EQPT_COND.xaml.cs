using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ELEC_EQPT_COND.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_EQPT_COND : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string eqptID = string.Empty;
        private string eqptName = string.Empty;
        private string lotID = string.Empty;
        private string procID = string.Empty;

        private BizDataSet _Biz = new BizDataSet();
        #endregion

        #region Initialize 
        public IFrameOperation FrameOperation { get; set; }

        public CMM_ELEC_EQPT_COND()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                eqptID = Util.NVC(tmps[0]);
                eqptName = Util.NVC(tmps[1]);
                procID = Util.NVC(tmps[2]);
                lotID = Util.NVC(tmps[3]);
                
                txtLotID.Text = lotID;
                txtEqptID.Text = eqptID;
                txtEqptName.Text = eqptName;

                ApplyPermissions();
                GetEqptCondInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (dgEqptCond.ItemsSource == null || dgEqptCond.Rows.Count < 1)
            {
                Util.MessageValidation("SFU2052");  //입력된 항목이 없습니다.
                return;
            }

            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetEqptCond();
                }
            });
        }
        
        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region User Method        
        private void GetEqptCondInfo()
        {
            try
            {
                DataTable inTable = _Biz.GetDA_EQP_SEL_PROC_EQPT_PRDT_SET_ITEM_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = procID;
                newRow["EQPTID"] = eqptID;
                newRow["LOTID"] = lotID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_PROC_EQPT_SET_ITEM_INFO", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }
                    Util.GridSetData(dgEqptCond, searchResult, null, true);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void SetEqptCond()
        {
            try
            {
                dgEqptCond.EndEdit();
                
                DataSet indataSet = _Biz.GetBR_QCA_REG_EQPT_DATA_CLCT();
                DataTable inTable = indataSet.Tables["IN_EQP"];
                    
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = eqptID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["LOTID"] = lotID;
                newRow["INPUT_SEQ_NO"] = 1;

                inTable.Rows.Add(newRow);

                DataTable in_Data = indataSet.Tables["IN_DATA"];

                for (int i = 0; i < dgEqptCond.Rows.Count - dgEqptCond.BottomRows.Count; i++)
                {
                    newRow = null;

                    newRow = in_Data.NewRow();
                    newRow["CLCTITEM"] = Util.NVC(DataTableConverter.GetValue(dgEqptCond.Rows[i].DataItem, "CLCTITEM"));
                    newRow["CLCTITEM_VALUE01"] = Util.NVC(DataTableConverter.GetValue(dgEqptCond.Rows[i].DataItem, "INPUT_VALUE"));

                    in_Data.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_EQPT_DATA_CLCT", "IN_EQP,IN_DATA", null, indataSet);

                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                //btnSave.IsEnabled = false; // 2024.10.15. 김영국 - 저장 후 버튼 비활성화 되는 부분 주석 처리함.
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion
    }
}
