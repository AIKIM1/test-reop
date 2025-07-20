/*************************************************************************************
 Created Date : 2023.09.19
      Creator : 이의철
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2023.09.19  DEVELOPER : Initial Created.
  2023.10.16  이의철    : 폼 초기화 수정
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_019_E_SPEC_CHANGE : C1Window, IWorkArea
    {
        #region [Declaration & Constructor]
        //private string _sTrayID = string.Empty;
        string _Line = string.Empty;
        string _Model = string.Empty;

        public FCS001_019_E_SPEC_CHANGE()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region [Initialize]
        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();
            String[] sFilter = { LoginInfo.CFG_AREA_ID };

            C1ComboBox[] cboLineChild = { cboModel };
            _combo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelParent = { cboLine };
            _combo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.NONE, sCase: "LINEMODEL", cbParent: cboModelParent);

            string[] sFilterUse = { "USE_FLAG" };
            _combo.SetCombo(cboUseFlag, CommonCombo_Form.ComboStatus.NONE, sCase: "CMN", sFilter: sFilterUse);
        }
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            if (tmps != null && tmps.Length >= 1)
            {
                _Line = Util.NVC(tmps[0]);
                _Model = Util.NVC(tmps[1]);
            }
            else
            {
                _Line = "";
                _Model = "";
            }

            cboModel.SelectedValueChanged -= cboModel_SelectedValueChanged;

            InitCombo();

            if(!string.IsNullOrEmpty(_Line))
            {
                cboLine.SelectedValue = _Line;
            }

            if (!string.IsNullOrEmpty(_Model))
            {
                cboModel.SelectedValue = _Model;
            }

            InitData();

            if (!string.IsNullOrEmpty(_Line))
            {
                GetData();
            }                

            cboModel.SelectedValueChanged += cboModel_SelectedValueChanged;
        }
        #endregion

        #region [Method]
        private void InitData()
        {
            txtSpecLimit.Text = "";
        }

        private string GetLineByModel(string mdllot_id)
        {
            string LINE_ID = string.Empty;

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["MDLLOT_ID"] = mdllot_id;
                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_SEL_LINE_BY_MODEL", "RQSTDT", "RSLTDT", dtRqst, (result, Exception) =>
                {
                    try
                    {
                        if (Exception != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            LINE_ID = Util.NVC(result.Rows[0]["EQSGID"].ToString());                            

                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        //HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception e)
            {
                Util.MessageException(e);
            }

            return LINE_ID;

        }
        private void GetData()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LINE_ID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                
                DataRow dr = dtRqst.NewRow();
                dr["LINE_ID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel);
                dtRqst.Rows.Add(dr);
                                
                new ClientProxy().ExecuteService("DA_SEL_TB_SFC_LINE_MDL_E_GRD_LIMIT", "RQSTDT", "RSLTDT", dtRqst, (result, Exception) =>
                {
                    try
                    {
                        if (Exception != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            string E_GRD_LIMIT = Util.NVC(result.Rows[0]["E_GRD_LIMIT"].ToString());
                            string USE_FLAG = Util.NVC(result.Rows[0]["USE_FLAG"].ToString());

                            if(!string.IsNullOrEmpty(E_GRD_LIMIT))
                            {
                                txtSpecLimit.Text = Convert.ToDouble(E_GRD_LIMIT).ToString();
                            }
                            else
                            {
                                txtSpecLimit.Text = "";
                            }                            
                            
                            cboUseFlag.SelectedValue = USE_FLAG;

                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        //HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception e)
            {
                Util.MessageException(e);
            }

            
            
        }
        private void SaveData()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LINE_ID", typeof(string));
            inDataTable.Columns.Add("MDLLOT_ID", typeof(string));
            inDataTable.Columns.Add("E_GRD_LIMIT", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("USE_FLAG", typeof(string));

            DataRow dr = inDataTable.NewRow();

            dr["LINE_ID"] = Util.GetCondition(cboLine);
            dr["MDLLOT_ID"] = Util.GetCondition(cboModel);
            if(!string.IsNullOrEmpty(this.txtSpecLimit.Text))
            {
                dr["E_GRD_LIMIT"] = Util.GetCondition(this.txtSpecLimit);
            }            
            dr["USERID"] = LoginInfo.USERID;
            dr["USE_FLAG"] = Util.GetCondition(cboUseFlag);
            inDataTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_MER_TB_SFC_LINE_MDL_E_GRD_LIMIT", "INDATA", "OUTDATA", inDataTable);

            this.DialogResult = MessageBoxResult.No;
            Util.MessageInfo("FM_ME_0215");  //저장하였습니다.
        }
        
        #endregion

        #region [Event]
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            this.ClearValidation();
            if (cboLine.GetBindValue() == null)
            {
                // 라인을 선택해주세요.
                Util.MessageValidation("SFU1223");
                return;
            }

            if (cboModel.GetBindValue() == null)
            {
                // 모델을 선택해주세요
                Util.MessageValidation("SFU1257");
                return;
            }
            if(ValidationSave() == false)
            {
                return;
            }            

            Util.MessageConfirm("FM_ME_0214", (result) => //저장하시겠습니까?
             {
                 if (result == MessageBoxResult.OK)
                 {
                     try
                     {
                         SaveData();
                     }
                     catch (Exception ex)
                     {
                         Util.MessageException(ex);
                     }
                     Close();

                 }
                 else { return; }
             });
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            InitData();
            GetData();
        }

        private void cboLine_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {

        }

        private void cboModel_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            
        }
        private void cboModel_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            InitData();
            GetData();
        }




        #endregion


        private bool ValidationSave()
        {
            
            int iQty = 0;
            if(!string.IsNullOrEmpty(txtSpecLimit.Text))
            {
                if (int.TryParse(txtSpecLimit.Text.Trim(), out iQty))
                {
                    if (iQty < 1)
                    {
                        Util.MessageValidation("SFU3064"); // 수량은 0보다 큰 정수로 입력 하세요.
                        return false;
                    }
                }
                else
                {
                    Util.MessageValidation("SFU3065");  // 수량을 정수로 입력 하세요.
                    return false;
                }
            }
            

            return true;
        }


    }
}
