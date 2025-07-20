/*************************************************************************************
 Created Date : 2022.03.30
      Creator : 이춘우
   Decription : FastTrack Lot 특이 사항 입력
--------------------------------------------------------------------------------------
 [Change History]
  2022.03.30  최초착성.
 
**************************************************************************************/

using System.Data;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Windows;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_365_CREATE_HIST : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string lotid = string.Empty;
        private string eqptname = string.Empty;

        private string delay_flag = string.Empty;
        private string delay_note = string.Empty;
        private string wrin_flag = string.Empty;
        private string wrin_attr01 = string.Empty;
        private string wrin_attr02 = string.Empty;
        private string wrin_attr03 = string.Empty;
        private string note = string.Empty;
        private string regall_flag = "N";


        public COM001_365_CREATE_HIST()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion Declaration & Constructor

        #region Initialize

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Get Para
            object[] tmps = C1WindowExtension.GetParameters(this);

            lotid = tmps[0].ToString();
            eqptname = tmps[1].ToString();

            if (eqptname == "LOTID")
            {
                wrin_flag = tmps[2].ToString();
                wrin_attr01 = tmps[3].ToString();
                wrin_attr02 = tmps[4].ToString();
                wrin_attr03 = tmps[5].ToString();
                note = tmps[6].ToString();
            }
            else if (eqptname == "LOTID_RP")
            {
                delay_flag = tmps[2].ToString();
                delay_note = tmps[3].ToString();
                wrin_flag = tmps[4].ToString();
                wrin_attr01 = tmps[5].ToString();
                wrin_attr02 = tmps[6].ToString();
                wrin_attr03 = tmps[7].ToString();
                note = tmps[8].ToString();
            }
            else
            {
                delay_flag = tmps[2].ToString();
                delay_note = tmps[3].ToString();
                note = tmps[4].ToString();
            }

            // Init Combo
            CommonCombo _combo = new CommonCombo();

            string[] sFilter1 = { "FLAG_YN" };
            _combo.SetCombo(cboDelay, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE_WITHOUT_CODE");
            _combo.SetCombo(cboWrinkle, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE_WITHOUT_CODE");
            
            string[] sFilter2 = { "FASTTRACK_WRINKLE_FLAG" };
            _combo.SetCombo(cboWrinkle1, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODE_WITHOUT_CODE");

            string[] sFilter3 = { "FASTTRACK_ON_TYPE" };
            _combo.SetCombo(cboWrinkle2, CommonCombo.ComboStatus.ALL, sFilter: sFilter3, sCase: "COMMCODE_WITHOUT_CODE");

            string[] sFilter4 = { "FASTTRACK_SIDE_TYPE" };
            _combo.SetCombo(cboWrinkle3, CommonCombo.ComboStatus.ALL, sFilter: sFilter4, sCase: "COMMCODE_WITHOUT_CODE");

            if (eqptname == "LOTID")
            {
                cboWrinkle.SelectedValue = wrin_flag;
                cboWrinkle1.SelectedValue = wrin_attr01;
                cboWrinkle2.SelectedValue = wrin_attr02;
                cboWrinkle3.SelectedValue = wrin_attr03;
                txtOtherNote.Text = note;
            }
            else if (eqptname == "LOTID_RP")
            {
                cboDelay.SelectedValue = delay_flag;
                txtDelayNote.Text = delay_note;
                cboWrinkle.SelectedValue = wrin_flag;
                cboWrinkle1.SelectedValue = wrin_attr01;
                cboWrinkle2.SelectedValue = wrin_attr02;
                cboWrinkle3.SelectedValue = wrin_attr03;
                txtOtherNote.Text = note;
            }
            else
            {
                cboDelay.SelectedValue = delay_flag;
                txtDelayNote.Text = delay_note;
                txtOtherNote.Text = note;
            }

            if (cboDelay.SelectedIndex == 1)
                txtDelayNote.IsEnabled = true;
            else
                txtDelayNote.IsEnabled = false;

            if (cboWrinkle.SelectedIndex == 1)
            {
                cboWrinkle1.IsEnabled = true;
                cboWrinkle2.IsEnabled = true;
                cboWrinkle3.IsEnabled = true;
            }
            else
            {
                cboWrinkle1.IsEnabled = false;
                cboWrinkle2.IsEnabled = false;
                cboWrinkle3.IsEnabled = false;
            }

            // Init Grid
            if (eqptname == "LOTID")
            {
                if (grdDelay.Visibility.Equals(Visibility.Visible))
                    this.Height = this.Height - grdDelay.Height - 20;

                grdDelay.Visibility = Visibility.Collapsed;

                grdWrinkle.Margin = new Thickness(0, 0, 0, 0);
                grdOther.Margin = new Thickness(0, grdWrinkle.Height + 20, 0, 0);
            }
            else if (eqptname == "LOTID_SL")
            {
                if (grdWrinkle.Visibility.Equals(Visibility.Visible))
                    this.Height = this.Height - grdWrinkle.Height - 20;

                grdWrinkle.Visibility = Visibility.Collapsed;

                grdDelay.Margin = new Thickness(0, 0, 0, 0);
                grdOther.Margin = new Thickness(0, grdDelay.Height + 20, 0, 0);

                chkRegAll.Visibility = Visibility.Visible;
            }
        }

        #endregion Initialize

        #region Event

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateSave())
                {
                    return;
                }

                // 저장하시겠습니까?
                Util.MessageConfirm("SFU1241", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveHist();

                        DialogResult = MessageBoxResult.OK;

                        this.Close();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboDelay_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            if (cboDelay.SelectedIndex == 1)
                txtDelayNote.IsEnabled = true;
            else
            {
                txtDelayNote.Text = "";
                txtDelayNote.IsEnabled = false;
            }
        }

        private void cboWrinkle_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            if (cboWrinkle.SelectedIndex == 1)
            {
                cboWrinkle1.IsEnabled = true;
                cboWrinkle2.IsEnabled = true;
                cboWrinkle3.IsEnabled = true;
            }
            else
            {
                cboWrinkle1.SelectedIndex = 0;
                cboWrinkle2.SelectedIndex = 0;
                cboWrinkle3.SelectedIndex = 0;

                cboWrinkle1.IsEnabled = false;
                cboWrinkle2.IsEnabled = false;
                cboWrinkle3.IsEnabled = false;
            }
        }
        private void chkRegAll_Checked(object sender, RoutedEventArgs e)
        {
            regall_flag = "Y";
        }

        private void chkRegAll_Unchecked(object sender, RoutedEventArgs e)
        {
            regall_flag = "N";
        }
        #endregion Event


        #region Mehod

        private void SaveHist()
        {
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("DELAY_FLAG", typeof(string));
                inTable.Columns.Add("DELAY_NOTE", typeof(string));
                inTable.Columns.Add("WRIN_FLAG", typeof(string));
                inTable.Columns.Add("WRIN_ATTR01", typeof(string));
                inTable.Columns.Add("WRIN_ATTR02", typeof(string));
                inTable.Columns.Add("WRIN_ATTR03", typeof(string));
                inTable.Columns.Add("NOTE", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));
                inTable.Columns.Add("INSUSER", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));
                inTable.Columns.Add("REG_ALL_SL_FLAG", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LOTID"] = lotid;
                dr["DELAY_FLAG"] = cboDelay.SelectedValue.ToString();
                dr["DELAY_NOTE"] = txtDelayNote.Text.ToString();
                dr["WRIN_FLAG"] = cboWrinkle.SelectedValue.ToString();
                dr["WRIN_ATTR01"] = cboWrinkle1.SelectedValue.ToString();
                dr["WRIN_ATTR02"] = cboWrinkle2.SelectedValue.ToString();
                dr["WRIN_ATTR03"] = cboWrinkle3.SelectedValue.ToString();
                dr["NOTE"] = txtOtherNote.Text.ToString();
                dr["USE_FLAG"] = "N";
                dr["INSUSER"] = LoginInfo.USERID;
                dr["UPDUSER"] = LoginInfo.USERID;
                dr["REG_ALL_SL_FLAG"] = regall_flag;

                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteServiceSync("BR_PRD_REG_FASTTRACK_LOT_HIST", "INDATA", null, inTable);

                inTable.Rows.Clear();

                // 저장되었습니다.
                //Util.AlertInfo("SFU1270");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool ValidateSave()
        {
            if (string.IsNullOrEmpty(lotid))
            {
                Util.MessageValidation("SFU8275", "PRODID");
                return false;
            }

            return true;
        }




        #endregion Mehod

        
    }
}