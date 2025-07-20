/*************************************************************************************
 Created Date : 2023.09.23
      Creator : 백광영
   Decription : 조립 원자재 재고현황 - Delivering 자재 현황
--------------------------------------------------------------------------------------
 [Change History]
  2024.01.19  백광영 : 라입별 자재Port 조회 시 AREAID 조회 조건 추가

**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Linq;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_404_REG_MNGT : C1Window, IWorkArea
    {
        string _RackType = string.Empty;
        string _ReqStatus = string.Empty;
        string _MtrlPortID = string.Empty;
        string _sEquipmentSegment = string.Empty;
        bool _bFirst = false;

        public COM001_404_REG_MNGT()
        {
            InitializeComponent();
            Init();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            
            InitCombo();
        }

        private void InitCombo()
        {

        }

        private void Init()
        {
            txtSITE.Text = LoginInfo.CFG_SHOP_NAME;
            dtExpected.SelectedDateTime = DateTime.Now.AddDays(30);




        }


        /// <summary>
        /// Delivering 자재 현황
        /// </summary>

        private void getMtrlPortInfo()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("MTRL_PORT_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["MTRL_PORT_ID"] = _MtrlPortID;

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TB_MMD_ELTR_ASSY_MTRL_PORT", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtRslt == null || dtRslt.Rows.Count < 1)
                {
                    return;
                }
                _sEquipmentSegment = dtRslt.Rows[0]["EQSGID"].ToString();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        /// <summary>
        /// Line별 자재 Port
        /// </summary>
       

        /// <summary>
        /// Port ID, 투입가능 수량
        /// </summary>
        /// <param name="_portid"></param>
        

        /// <summary>
        /// Port별 투입가능 자재 확인
        /// </summary>
        /// <param name="_portid"></param>
        /// <param name="_mtrlid"></param>
        /// <returns></returns>
        private bool _getPortMaterial(string _portid, string _mtrlid)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("MTRL_PORT_ID", typeof(string));
                RQSTDT.Columns.Add("MTRLID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["MTRL_PORT_ID"] = _portid;
                dr["MTRLID"] = _mtrlid;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MTRL_CHK_PORT_ID_KANBAN", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                    return true;

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return true;
            }
        }
        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            Init();
        }

        

        

        


        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
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

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

        }

        #region [담당자]
        private void txtPerson_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                try
                {
                    if (txtPerson.Text.Trim() == string.Empty)
                        return;

                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("USERNAME", typeof(string));
                    dtRqst.Columns.Add("LANGID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["USERNAME"] = txtPerson.Text;
                    dr["LANGID"] = LoginInfo.LANGID;

                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_NAME", "INDATA", "OUTDATA", dtRqst);

                    if (dtRslt.Rows.Count == 0)
                    {
                        Util.Alert("SFU1592");  //사용자 정보가 없습니다.
                    }
                    else if (dtRslt.Rows.Count == 1)
                    {
                        txtPerson.Text = dtRslt.Rows[0]["USERNAME"].ToString();
                        txtPersonId.Text = dtRslt.Rows[0]["USERID"].ToString();
                        txtPersonDept.Text = dtRslt.Rows[0]["DEPTNAME"].ToString();
                    }
                    else
                    {
                        dgPersonSelect.Visibility = Visibility.Visible;

                        Util.gridClear(dgPersonSelect);

                        dgPersonSelect.ItemsSource = DataTableConverter.Convert(dtRslt);
                        this.Focusable = true;
                        this.Focus();
                        this.Focusable = false;
                    }

                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
        }
        #endregion

        #region [담당자 검색결과 여러개일경우]
        private void dgPersonSelect_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            txtPersonId.Text = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERID").ToString());
            txtPerson.Text = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERNAME").ToString());
            txtPersonDept.Text = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "DEPTNAME"));

            dgPersonSelect.Visibility = Visibility.Collapsed;

        }
        #endregion

        private void btnComplete_Click(object sender, RoutedEventArgs e)
        {




            try
            {
             if(txtPersonId.Text.Equals(""))
                {
                    Util.AlertInfo("PSS9141");  //입력한 데이터가 없습니다.
                    return;
                }
             if(txtSPCL_LOT.Text.Equals(""))
                {
                    Util.AlertInfo("SFU9983");  //입력한 데이터가 없습니다.
                    return;
                }







                //저장하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                DataSave();
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void DataSave()
        {


            DataSet inData = new DataSet();




            DataTable RQSTDT = inData.Tables.Add("INDATA");
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("SPCL_STCK_MNGT_NAME", typeof(string));
            RQSTDT.Columns.Add("SPCL_STCK_MNGT_DESC", typeof(string));
            RQSTDT.Columns.Add("PRCS_USERID", typeof(string));
            RQSTDT.Columns.Add("PRCS_SCHD_DATE", typeof(string));
            RQSTDT.Columns.Add("NOTE", typeof(string));
            RQSTDT.Columns.Add("USER", typeof(string));


            DataRow row = null;

            row = RQSTDT.NewRow();
            row["SPCL_STCK_MNGT_NAME"] = txtSPCL_LOT.Text;
            row["SPCL_STCK_MNGT_DESC"] = "";
            row["PRCS_USERID"] = txtPersonId.Text;
            row["PRCS_SCHD_DATE"] = dtExpected.SelectedDateTime.ToString("yyyyMMdd");
            row["NOTE"] = txtNote.Text;
            row["USER"] = LoginInfo.USERID;

            RQSTDT.Rows.Add(row);




            


            try
            {
                //저장 처리
                
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_SPCL_STCK", "INDATA", null, inData); // MES 2.0 - INLOT 제거 (유재홍 선임 요청)

                Util.AlertInfo("SFU1270");  //저장되었습니다
                
                this.DialogResult = MessageBoxResult.Cancel;
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_SPCL_STCK", ex.Message, ex.ToString());
            }
        }



        private void dtExpected_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Convert.ToDecimal(dtExpected.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(DateTime.Now.ToString("yyyyMMdd")))
            {
                Util.AlertInfo("SFU1740");  //오늘 이후 날짜만 지정 가능합니다.
                dtExpected.SelectedDateTime = DateTime.Now;
            }
        }
    }
}
