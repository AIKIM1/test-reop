/*************************************************************************************
 Created Date : 2020.10.14
      Creator : Initial Created 
   Decription : [공Tray 수동배출]
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.07  NAME : Initial Created 
  2021.04.07  KDH : To Eqp 이벤트 변경 (SelectedIndexChanged -> SelectedValueChanged)
  2021.04.22  KDH : 위치 정보 그룹화 대응
  2022.05.13  이정미 : EQP_LOC_GRP_CD 동별 공통코드로 변경
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using C1.WPF.DataGrid;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using System.Collections.Generic;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_026_CMP_OUT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string sTrayId = string.Empty;
        private string sFromport = string.Empty;
        private string sToport = string.Empty;
        private string sCNCL_FLAG = string.Empty;

        public string TrayId
        {
            get { return sTrayId; }
        }

        public FCS002_026_CMP_OUT()
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

            sCNCL_FLAG = "N";

            if (tmps != null)
            {
                txtTrayID.Text = Util.NVC(tmps[0]);

                GetTrayInfo();
            }

            GetFromPort();

            GetToPort();


        }

     


    
        #endregion

        #region Mehod

        private void GetTrayInfo()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("CST_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["CST_ID"] = txtTrayID.Text;
                dtRqst.Rows.Add(dr);
                
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TB_SFC_FORM_TRF_TRGT_TRAY_LIST_MB", "RQSTDT", "RSLTDT", dtRqst);

                if(dtRslt.Rows.Count>0)
                {
                    btnOut.Content = ObjectDic.Instance.GetObjectName("OUT_CANCEL"); ;
                    sCNCL_FLAG = "Y";
                }
                
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
         
        }

        private void GetFromPort()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("COM_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("ATTR1", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "FORM_CMP_TRF_PORT_MB";
                dr["ATTR1"] = "Y";
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    sFromport = dtRslt.Rows[0]["COM_CODE"].ToString();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }

        private void GetToPort()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("COM_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("ATTR2", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "FORM_CMP_TRF_PORT_MB";
                dr["ATTR2"] = "Y";
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    sToport = dtRslt.Rows[0]["COM_CODE"].ToString();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }
        #endregion

        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            //상태를 변경하시겠습니까?
            Util.MessageConfirm("FM_ME_0337", (result) =>
            {
                try
                {
                    if (result == MessageBoxResult.OK)
                    {

                        DataTable dtInData = new DataTable();
                        dtInData.TableName = "INDATA";
                        dtInData.Columns.Add("CST_ID", typeof(string));
                        dtInData.Columns.Add("USERID", typeof(string));
                        dtInData.Columns.Add("CNCL_FLAG", typeof(string));
                        dtInData.Columns.Add("FROM_PORT", typeof(string));
                        dtInData.Columns.Add("TO_PORT", typeof(string));

                        DataRow drIn = dtInData.NewRow();
                        drIn["CST_ID"] = txtTrayID.Text;
                        drIn["USERID"] = LoginInfo.USERID;
                        drIn["CNCL_FLAG"] = sCNCL_FLAG;
                        drIn["FROM_PORT"] = sFromport;
                        drIn["TO_PORT"] = sToport;
                        dtInData.Rows.Add(drIn);

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_CMP_OUT_MB", "IN_REQ_TRF_INFO", null, dtInData);

                        this.DialogResult = MessageBoxResult.OK;
                        Util.AlertInfo("FM_ME_0136");  //변경완료하였습니다.
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });
        }
    }
}
