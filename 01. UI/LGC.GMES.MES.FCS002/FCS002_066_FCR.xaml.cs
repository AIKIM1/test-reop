/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_066_FCR : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        string area = string.Empty;
        string process = string.Empty;
        //string eqsgid = string.Empty;

        CommonCombo combo = new CommonCombo();

        public FCS002_066_FCR()
        {
            InitializeComponent();

        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        #endregion

        #region Initialize

        
        #endregion

        #region Event


        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                area = Util.NVC(tmps[0]);
                process = Util.NVC(tmps[1]);
                //eqsgid = Util.NVC(tmps[2]);

            }

            initCombo();
        }
   
        private void cboArea_Loaded(object sender, RoutedEventArgs e)
        {
            if (area.Equals(string.Empty) || area == null) return;

            cboArea.SelectedValue = area;
        }
        //private void cboProcess_Loaded(object sender, RoutedEventArgs e)
        //{
        //    if (process.Equals(string.Empty) || process == null) return;

        //    cboProcess.SelectedValue = process;
        //}

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation())
                {
                    return;
                }
                //FCR그룹코드를 저장하시겠습니까?
                Util.MessageConfirm("SFU3209", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveFCRGroup();
                      
                    }
                });

            }
            catch (Exception ex)
            {

            }

        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
        }

   


        #endregion

        #region Mehod
        private void initCombo()
        {
            combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT);

            //string[] sFilter = { eqsgid };
            //combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT,  sFilter: sFilter);

            //현상코드
            String[] sFilterFailure = { "F" };
            combo.SetCombo(cboFailure, CommonCombo.ComboStatus.NA, sFilter: sFilterFailure, sCase: "FCRCODE");

            //원인코드
            String[] sFilterCause = { "C" };
            combo.SetCombo(cboCause, CommonCombo.ComboStatus.NA, sFilter: sFilterCause, sCase: "FCRCODE");

            //조치코드
            String[] sFilterResolution = { "R" };
            combo.SetCombo(cboResolution, CommonCombo.ComboStatus.NA, sFilter: sFilterResolution, sCase: "FCRCODE");

        }

        private void SaveFCRGroup()
        {
            try
            {
                if (loadingIndicator != null)
                    loadingIndicator.Visibility = Visibility.Visible;

                DataSet ds = new DataSet();

                DataTable dt = ds.Tables.Add("INDATA");
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("FCR_GR_CODE", typeof(string));
                dt.Columns.Add("FCR_GR_NAME", typeof(string));
                dt.Columns.Add("F_TYPE_CODE", typeof(string));
                dt.Columns.Add("C_TYPE_CODE", typeof(string));
                dt.Columns.Add("R_TYPE_CODE", typeof(string));
                dt.Columns.Add("USERID", typeof(string));

                DataRow row = dt.NewRow();
                row["AREAID"] = Convert.ToString(cboArea.SelectedValue);
                row["PROCID"] = process;
                row["FCR_GR_CODE"] = txtFCRGRCode.Text;
                row["FCR_GR_NAME"] = txtFCRGrName.Text;
                row["F_TYPE_CODE"] = Convert.ToString(cboFailure.SelectedValue);
                row["C_TYPE_CODE"] = Convert.ToString(cboCause.SelectedValue);
                row["R_TYPE_CODE"] = Convert.ToString(cboResolution.SelectedValue);
                row["USERID"] = LoginInfo.USERID;
                dt.Rows.Add(row);

                new ClientProxy().ExecuteService_Multi("BR_SET_FCR_GR", "INDATA", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        if (loadingIndicator != null)
                            loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, ds);


            }
            catch (Exception ex)
            {

            }
        }

        private bool Validation()
        {
            if (cboArea.SelectedValue.Equals("-SELECT-") || cboArea.SelectedValue.Equals(string.Empty))
            {
                Util.MessageValidation("SFU3206"); //동을 선택해주세요
                return false;
            }
            //if (cboProcess.SelectedValue.Equals("-SELECT-") || cboProcess.SelectedValue.Equals(string.Empty))
            //{
            //    Util.MessageValidation("SFU3207"); //공정을 선택해주세요
            //    return false;
            //}
            if (txtFCRGRCode.Text.Equals(string.Empty))
            {
                Util.MessageValidation("SFU3210"); //FCR그룹코드를 입력해주세요
                return false;
            }
            if (txtFCRGrName.Text.Equals(string.Empty))
            {
                Util.MessageValidation("SFU3211"); //FCR그룹명을 입력해주세요
                return false;
            }
            if (cboFailure.SelectedValue.Equals("N/A") || cboFailure.SelectedValue.Equals(string.Empty))
            {
                Util.MessageValidation("SFU3212");//현상을 선택해주세요
                return false;
            }
            if (cboCause.SelectedValue.Equals("N/A") || cboCause.SelectedValue.Equals(string.Empty))
            {
                Util.MessageValidation("SFU3213");//원인을 선택해주세요
                return false;
            }
            if (cboResolution.SelectedValue.Equals("N/A") || cboResolution.SelectedValue.Equals(string.Empty))
            {
                Util.MessageValidation("SFU3214"); //조치를 선택해주세요
                return false; 
            }
            return true;
        }






        #endregion


    }
}
