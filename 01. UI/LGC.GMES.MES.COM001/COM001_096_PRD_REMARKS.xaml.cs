/*************************************************************************************
 Created Date : 2022.03.07
      Creator : 장희만
   Decription : 활성화 재공 현황 생산 특이사항 일괄 변경
--------------------------------------------------------------------------------------
 [Change History]
  2022.03.07  최초착성.
 
**************************************************************************************/

using System.Data;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Windows;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_096_PRD_REMARKS : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private static readonly int TRANSACTION_TYPE_SAVE = 1;
        private static readonly int TRANSACTION_TYPE_DELETE = 2;

        DataRow[] _LOT_ID_LISTS = null;

        public COM001_096_PRD_REMARKS()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public DataRow[] LOT_ID_LISTS
        {
            get { return _LOT_ID_LISTS; }
        }

        #endregion

        #region Initialize


        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _LOT_ID_LISTS = (DataRow[])tmps[0];

            setInit();
        }

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
                        SaveWipRemarks(TRANSACTION_TYPE_SAVE);

                    }

                   
                   
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            // 삭제 하시겠습니까?
            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveWipRemarks(TRANSACTION_TYPE_DELETE);
                }



            });
        }

        #endregion


        #region Mehod

        private void setInit()
        {
            if (_LOT_ID_LISTS.Length == 1)
            {
                txtPrdRemarks.Text = _LOT_ID_LISTS[0]["WIP_NOTE"].ToString();
            }
        }

        private bool ValidateSave()
        {
            if (txtPrdRemarks.Text == null || string.IsNullOrEmpty(txtPrdRemarks.Text))
            {
                //특이사항을 입력하세요.
                Util.MessageValidation("SFU1993");
                return false;
            }

            if (_LOT_ID_LISTS == null || _LOT_ID_LISTS.Length <= 0)
            {
                //선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }
            return true;
        }

        private void SaveWipRemarks( int tranType )
        {
            try
            {
                const string bizRuleName = "BR_PRD_REG_WIPHISTORYATTR_WIP_NOTE_FO";

                // DATA SET
                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("WIP_NOTE", typeof(string));

                DataTable inLot = inDataSet.Tables.Add("IN_LOT");
                inLot.Columns.Add("LOTID", typeof(string));
               
                // INDATA
                DataRow newRow = inTable.NewRow();
                newRow["USERID"] = LoginInfo.USERID;
                if (tranType == TRANSACTION_TYPE_SAVE)
                {
                    newRow["WIP_NOTE"] = txtPrdRemarks.Text;  
                } else
                {
                    newRow["WIP_NOTE"] = ""; //삭제일 경우는 NULL 입력함
                }
                inTable.Rows.Add(newRow);

                // IN_LOT
                for (int inx = 0; inx < _LOT_ID_LISTS.Length; inx++)
                {
                    newRow = inLot.NewRow();
                    newRow["LOTID"] = _LOT_ID_LISTS[inx]["LOTID"];
                    inLot.Rows.Add(newRow);
                }

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_LOT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //정상 처리 되었습니다
                        Util.MessageInfo("SFU1889");

                        this.DialogResult = MessageBoxResult.OK;

                        //실물 관리를 위해 실물 Tag 교체 필요 알림 문구 팝업
                        Util.MessageValidation("SFU8479"); //실물 Tag 교체 필요합니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion
    }
}