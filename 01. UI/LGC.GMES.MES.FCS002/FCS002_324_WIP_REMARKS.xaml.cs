/*************************************************************************************
 Created Date : 2017.10.07
      Creator : 주건태
   Decription : 활성화 재공 현황 재고 특이사항 일괄 변경
--------------------------------------------------------------------------------------
 [Change History]
  2017.10.17  최초착성.
  2023.03.30   이홍주   : 소형활성화 MES 복사
**************************************************************************************/

using System.Data;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Windows;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_324_WIP_REMARKS : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private static readonly int TRANSACTION_TYPE_SAVE = 1;
        private static readonly int TRANSACTION_TYPE_DELETE = 2;

        DataRow[] _LOT_ID_LISTS = null;

        public FCS002_324_WIP_REMARKS()
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
                txtWipRemarks.Text = _LOT_ID_LISTS[0]["WIP_REMARKS"].ToString();
            }
        }

        private bool ValidateSave()
        {
            if (txtWipRemarks.Text == null || string.IsNullOrEmpty(txtWipRemarks.Text))
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
                const string bizRuleName = "BR_PRD_REG_WIPATTR_WIP_REMARKS_FO_MB";

                // DATA SET
                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("WIP_REMARKS", typeof(string));

                DataTable inLot = inDataSet.Tables.Add("IN_LOT");
                inLot.Columns.Add("LOTID", typeof(string));

                // INDATA
                DataRow newRow = inTable.NewRow();
                newRow["USERID"] = LoginInfo.USERID;
                if (tranType == TRANSACTION_TYPE_SAVE)
                {
                    newRow["WIP_REMARKS"] = txtWipRemarks.Text;
                } else
                {
                    //newRow["WIP_REMARKS"] = ""; //삭제일 경우는 NULL 입력함
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