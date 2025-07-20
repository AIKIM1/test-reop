/*************************************************************************************
 Created Date : 2018.10.29
      Creator : 손우석
   Decription : Pack Lot이력- GB/T 변경
--------------------------------------------------------------------------------------
 [Change History
  2018.10.29 손우석 CSR ID:3821490 GB/T 바코드 조립 이력 조회 기능 요청 건 [요청번호]C20181019_21490
  2018.12.07 손우석 CSR ID:3855538 팩 라벨 관리 강화 시스템 개발 요청 [요청번호]C20181128_55538
  2018.12.20 손우석 다국어 메시지 코드 변경
  2019.12.11  손우석 SM CMI Pack 메시지 다국어 처리 요청
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_005_CHANGEGBT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();

        //2018.12.07
        DataTable dtReturn;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_005_CHANGEGBT()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
                if (tmps == null)
                {
                    this.DialogResult = MessageBoxResult.Cancel;
                    this.Close();
                }

                txtLot.Text = Util.NVC(tmps[0]);
                txtGbt.Text = Util.NVC(tmps[1]);
            }
            catch(Exception ex)
            {
                Util.Alert(ex.ToString());

            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
            this.Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("변경 내용을 저장하시겠습니까?"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        if (txtChangeGBT.Text.Length == 24)
                        {
                            SetGBTChange();

                            //2018.12.07
                            if (dtReturn.Rows.Count > 0)
                            {
                                ms.AlertInfo("SFU1270");
                            }
                        }
                        else
                        {
                            ms.AlertWarning("SFU6002"); //GB/T는 24자리입니다.
                            return;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());

            }
        }

        #endregion

        #region Mehod
        private void SetGBTChange()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("GBTID", typeof(string));
                RQSTDT.Columns.Add("USER", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = txtLot.Text;
                dr["GBTID"] = txtChangeGBT.Text;
                dr["USER"] = LoginInfo.USERID.ToString();
                RQSTDT.Rows.Add(dr);

                //2018.12.07
                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_GBT_CHANGE", "RQSTDT", "RSLTDT", RQSTDT);
                dtReturn = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_GBT_CHANGE", "RQSTDT", "RSLTDT", RQSTDT);

                //dgDetailData.ItemsSource = DataTableConverter.Convert(dtResult);
            }
            catch(Exception ex)
            {
                //2019.12.11
                //Util.AlertByBiz("BR_PRD_CHK_GBT_CHANGE", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }
        #endregion
    }
}
