/*************************************************************************************
 Created Date : 2017.01.19
      Creator : 
   Decription : 원자재 현황 관리 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.01.19  : Initial Created.
  
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

namespace LGC.GMES.MES.ELEC001
{
    /// <summary>
    /// ELEC001_LOTSTART
    /// </summary>
    public partial class ELEC001_002_INPUT_CHK : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _EQPTID = string.Empty;

        Util _Util = new Util();
        
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
        public ELEC001_002_INPUT_CHK()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
            {
                return;
            }
            _EQPTID = Util.NVC(tmps[0]);
            InitCombo();

            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기
        }
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            if (LoginInfo.CFG_PROC_ID.Equals(Process.MIXING) || LoginInfo.CFG_PROC_ID.Equals(Process.SRS_MIXING) || LoginInfo.CFG_PROC_ID.Equals(Process.PRE_MIXING))
            {
                String[] sFilter = { LoginInfo.CFG_EQSG_ID, LoginInfo.CFG_PROC_ID, null };
                _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.NONE, sFilter: sFilter);
                cboEquipment.SelectedValue = _EQPTID;
            }
            else
            {
                Util.MessageValidation("SFU2841");  //Mixing공정에서 사용가능합니다.
                return;
            }
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataSet indataSet = new DataSet();
                DataTable inData = indataSet.Tables.Add("IN_EQP");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("IFMODE", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));
                inData.Columns.Add("MES_INPUT_CHK_FLAG", typeof(string));
                inData.Columns.Add("EQPT_MAINT_FLAG", typeof(string));
                inData.Columns.Add("USERID", typeof(string));

                DataRow row = inData.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = "OFF";
                row["EQPTID"] = cboEquipment.SelectedValue.ToString().Trim();
                row["MES_INPUT_CHK_FLAG"] = chkMES_INPUT_CHK_FLAG.IsChecked == true ? "Y" : "N";
                row["EQPT_MAINT_FLAG"] = chkEQPT_MAINT_FLAG.IsChecked == true ? "Y" : "N";
                row["USERID"] = LoginInfo.USERID;

                indataSet.Tables["IN_EQP"].Rows.Add(row);
                

                Util.MessageConfirm("SFU1241", (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        new ClientProxy().ExecuteService_Multi("BR_MAT_REG_RINGBLOWER_INFO", "IN_EQP", null, (result, ex) =>
                        {
                            try
                            {
                                if (ex != null)
                                {
                                    Util.AlertByBiz("BR_MAT_REG_RINGBLOWER_INFO", ex.Message, ex.ToString());
                                    return;
                                }
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None); // 저장하시겠습니까?
                                
                            }
                            catch (Exception ErrEx)
                            {
                                Util.MessageException(ErrEx);
                            }
                        }, indataSet);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
               

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            GetEioattrFlag();
        }
       
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.DialogResult = MessageBoxResult.OK;
        }
       
        #endregion

        #region Mehod
        private void GetEioattrFlag()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["EQPTID"] = cboEquipment.SelectedValue.ToString().Trim();
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EIOATTR_RINGBLOWER", "INDATA", "OUTDATA", IndataTable);

                if (dtMain.Rows[0]["MES_INPUT_CHK_FLAG"].ToString() == "Y")
                {
                    chkMES_INPUT_CHK_FLAG.IsChecked = true;
                }
                else
                {
                    chkMES_INPUT_CHK_FLAG.IsChecked = false;
                }
                if (dtMain.Rows[0]["EQPT_MAINT_FLAG"].ToString() == "Y")
                {
                    chkEQPT_MAINT_FLAG.IsChecked = true;
                }
                else
                {
                    chkEQPT_MAINT_FLAG.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
    }
}
