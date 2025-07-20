/*************************************************************************************
 Created Date : 2024.04.04
      Creator : 이병윤
   Decription : 재계산여부 등록 팝업
--------------------------------------------------------------------------------------
 [Change History]
     Date         Author      CSR                    Description...
  2024.04.04      이병윤 :    E20240325-000251       Initial Created.
***************************************************************************************/
using System;
using System.Data;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;



namespace LGC.GMES.MES.MTRL001
{
    public partial class MTRL001_202_RECALC : C1Window
    {
        #region #. Declaration & Constructor

        private string sMLotId = string.Empty;
        private string sSeqno = string.Empty;
        private string sCalcFlag = string.Empty;
        public MTRL001_202_RECALC()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation { get; internal set; }

        #endregion

        #region [Events]

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
                sMLotId = Convert.ToString(tmps[0]);
                sSeqno = Convert.ToString(tmps[1]);
                sCalcFlag = Convert.ToString(tmps[2]);
                SetRd(sCalcFlag);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            // 저장하시겠습니까?
            Util.MessageConfirm("SFU1241", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetSave();
                    this.Close();
                }
            });
        }

        #endregion

        #region [Method]

        private void SetRd(string sCalcFlag)
        {
            try
            {
                if(sCalcFlag.Equals("P"))
                {
                    rdoP.IsChecked = true;
                }
                else if(sCalcFlag.Equals("F"))
                {
                    rdoF.IsChecked = true;
                }
                else
                {
                    rdoP.IsChecked = true;
                }
                txtSave.Text = sCalcFlag;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetSave()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("MLOTID", typeof(string));
                inTable.Columns.Add("INPUTSEQNO", typeof(string));
                inTable.Columns.Add("CALC_FLAG", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["MLOTID"] = Util.NVC(sMLotId);
                newRow["INPUTSEQNO"] = Util.NVC(sSeqno);
                if(rdoP.IsChecked == true)
                {
                    sCalcFlag = "P";
                }
                else if (rdoF.IsChecked == true)
                {
                    sCalcFlag = "F";
                }
                newRow["CALC_FLAG"] = (rdoP.IsChecked == true ? "P" : "F");
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_MTRL_LOSS_CALC_FLAG", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        txtSave.Text = sCalcFlag;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }

                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        
    }
}