/*************************************************************************************
 Created Date : 2018.10.30
      Creator : INS 김동일K
   Decription : GMES 고도화 - 노칭 공정진척 확정 시 메시지 팝업 추가
--------------------------------------------------------------------------------------
 [Change History]
  2018.10.30  INS 김동일K : Initial Created.
  
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Collections.Generic;
using System.Windows.Controls;

namespace LGC.GMES.MES.ASSY001
{
    /// <summary>
    /// ASSY001_001_CONFIRM_MSG.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY001_013_MSG : C1Window, IWorkArea
    {
        private bool _bChecked = false;
        private double _dRemain = 0;
        string sBoxId = String.Empty; // Pallet ID

        public bool CHECKED
        {
            get { return _bChecked; }
        }

        public double REMAINQTY
        {
            get { return _dRemain; }
        }

        public ASSY001_013_MSG()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

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
                sBoxId        = Convert.ToString(tmps[0]);
                txtNote.Text  = Convert.ToString(tmps[1]);
                txtSave.Text  = Convert.ToString(tmps[1]);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 특이사항 저장
                setSave();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCancel_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = MessageBoxResult.Cancel;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 저장
        /// </summary>
        private void setSave()
        {
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        ShowLoadingIndicator();

                        DataTable inTable = new DataTable();
                        inTable.Columns.Add("BOXID", typeof(string));
                        inTable.Columns.Add("SKID_NOTE", typeof(string));
                        inTable.Columns.Add("USERID", typeof(string));

                        DataRow newRow = inTable.NewRow();
                        newRow["BOXID"] = Util.NVC(sBoxId);
                        newRow["SKID_NOTE"] = Util.NVC(txtNote.Text);
                        newRow["USERID"] = LoginInfo.USERID;

                        inTable.Rows.Add(newRow);

                        new ClientProxy().ExecuteService("DA_PRD_UPD_BOX_SKID_NOTE", "INDATA", null, inTable, (bizResult, bizException) =>
                        {
                            HiddenLoadingIndicator();
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                                txtSave.Text = txtNote.Text;
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }

                        });

                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }
            });
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
    }
}
