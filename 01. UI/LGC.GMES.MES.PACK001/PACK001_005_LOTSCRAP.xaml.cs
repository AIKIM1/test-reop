/*************************************************************************************
 Created Date : 2016.08.01
      Creator : Jeong Hyeon Sik
   Decription : Pack Lot이력- 폐기 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.01  Jeong Hyeon Sik : Initial Created.
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

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_005_LOTSCRAP : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }



        public PACK001_005_LOTSCRAP()
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
                List<Button> listAuth = new List<Button>();
                listAuth.Add(btnOK);
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

                object[] tmps = ControlsLibrary.C1WindowExtension.GetParameters(this);
                if (tmps != null)
                {
                    DataTable dtText = tmps[0] as DataTable;

                    if (dtText.Rows.Count > 0)
                    {
                        txtScrapLot.Text = Util.NVC(dtText.Rows[0]["LOTID"]);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                
                if (rtxNote.DataContext.ToString() == "")
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("폐기 사유를 입력하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);

                    rtxNote.Focus();
                    return;
                }
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("현재 LOT을 폐기 플래그 처리 하시겠습니까?"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        setScrapLot();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }

            this.DialogResult = MessageBoxResult.OK;
            this.Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }
        #endregion

        #region Mehod
        private void setScrapLot()
        {
            try
            {
                //BR_PRD_REG_LOT_SCRAP_PACK
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                DataRow drINDATA = INDATA.NewRow();
                drINDATA["SRCTYPE"] = "UI";
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["LOTID"] = txtScrapLot.Text;
                drINDATA["USERID"] = LoginInfo.USERID;
                INDATA.Rows.Add(drINDATA);

                new ClientProxy().ExecuteServiceSync("BR_PRD_REG_LOT_SCRAP_PACK", "INDATA", "OUTDATA", INDATA, null);
            }
            catch(Exception ex)
            {
                //2019.12.11
                //Util.AlertByBiz("BR_PRD_REG_LOT_SCRAP_PACK", ex.Message, ex.ToString());
                Util.MessageException(ex);

            }
        }

        #endregion


    }
}
