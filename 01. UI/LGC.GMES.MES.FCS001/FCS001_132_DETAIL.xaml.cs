/*************************************************************************************
 Created Date : 2024.01.23
      Creator : 이정미
   Decription : PKG LOT HOLD 상세 현황
--------------------------------------------------------------------------------------
 [Change History]
  2024.01.23  NAME : Initial Created
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
    public partial class FCS001_132_DETAIL : C1Window, IWorkArea
    {

        #region Declaration & Constructor 
        Util _Util = new Util();
        private string MODEL = string.Empty;
        private string FROM_DATE = string.Empty;
        private string TO_DATE = string.Empty;
        private string EQSGID = string.Empty;
        #endregion

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FCS001_132_DETAIL()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            Object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null) return;

            else
            {
                txtWorkOrder.Text = tmps[0] as string;
                MODEL = tmps[1] as string;
                EQSGID = tmps[2] as string;
                FROM_DATE = tmps[3] as string;
                TO_DATE = tmps[4] as string;
            }

            InitCombo();
            GetList();
        }
        #endregion

        #region Event

        private void InitCombo()
        {
            CommonCombo_Form ComCombo = new CommonCombo_Form();

            string[] sFilter = { "LOT_HOLD_MODEL_BY_LINE", MODEL };
            GetModel(cboModel, sFilter: sFilter);
        }
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }
        #endregion

        #region Method
        private void GetList()
        {
            try
            {
                Util.gridClear(dgHoldDetail);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("MODEL", typeof(string));
                dtRqst.Columns.Add("ATTR1", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("FROM_TIME", typeof(string));
                dtRqst.Columns.Add("TO_TIME", typeof(string));
                

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                if (!string.IsNullOrEmpty(Util.GetCondition(cboModel, bAllNull: true)))
                    dr["MODEL"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["ATTR1"] = MODEL;
                dr["EQSGID"] = EQSGID;
                dr["FROM_TIME"] = FROM_DATE;
                dr["TO_TIME"] = TO_DATE;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LOT_HOLD_DETAIL_UI", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgHoldDetail, dtRslt, FrameOperation, true);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void GetList(string sPKGLotID)
        {
            try
            {
                Util.gridClear(dgHoldDetail);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("HOLD_ID", typeof(string));
                dtRqst.Columns.Add("MODEL", typeof(string));
                dtRqst.Columns.Add("ATTR1", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                if (!string.IsNullOrEmpty(txtPKGLotID.Text))
                    dr["HOLD_ID"] = sPKGLotID;
                if (!string.IsNullOrEmpty(Util.GetCondition(cboModel, bAllNull: true)))
                    dr["MODEL"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["ATTR1"] = MODEL;
                dr["EQSGID"] = EQSGID;
                dr["FROM_DATE"] = FROM_DATE;
                dr["TO_DATE"] = TO_DATE;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LOT_HOLD_DETAIL_UI", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgHoldDetail, dtRslt, FrameOperation, true);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void GetModel(C1ComboBox cbo, String[] sFilter)
        {
            try
            {
                const string bizRuleName = "DA_BAS_SEL_AREA_COM_CODE_CBO_ATTR";
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("COM_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("ATTR1", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sFilter[0];
                dr["ATTR1"] = sFilter[1];
                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    if (result.Rows.Count > 0)
                    {
                        DataTable dtTemp = new DataTable();
                        dtTemp = result.Copy();

                        cbo.DisplayMemberPath = "CBO_NAME";
                        cbo.SelectedValuePath = "CBO_CODE";
                        cbo.ItemsSource = AddStatus(dtTemp, CommonCombo.ComboStatus.ALL, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                        cbo.SelectedIndex = 0;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable AddStatus(DataTable dt, CommonCombo.ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case CommonCombo.ComboStatus.ALL:
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }
            return dt;
        }
        #endregion

        private void txtCellId_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && FrameOperation.AUTHORITY.Equals("W"))
            {
                if(string.IsNullOrEmpty(txtPKGLotID.Text) || (txtPKGLotID.Text.Length < 10 && txtPKGLotID.Text.Length > 8))
                {
                    //잘못된 ID입니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0205"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: true);
                    return;
                }
                GetList(txtPKGLotID.Text);
            }
        }

        private void cboModel_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if(!string.IsNullOrEmpty(txtPKGLotID.Text))
            {
                GetList(txtPKGLotID.Text);
            }
            else
            {
                GetList();
            }         
        }
    }
}
