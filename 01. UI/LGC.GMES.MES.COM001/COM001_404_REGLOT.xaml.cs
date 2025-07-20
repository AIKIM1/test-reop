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
    public partial class COM001_404_REGLOT : C1Window, IWorkArea
    {
        string _SPCL_STCK_MNGT_NAME = string.Empty;
        string _SPCL_STCK_MNGT_ID = string.Empty;
        string _RackType = string.Empty;
        string _ReqStatus = string.Empty;
        string _MtrlPortID = string.Empty;
        string _sEquipmentSegment = string.Empty;
        bool _bFirst = false;

        public COM001_404_REGLOT()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
            InitCombo();
        }

        private void InitCombo()
        {

        }

        private void Init()
        {
            //Util.gridClear(dgList);
            //Util.gridClear(dgComplete);
            //txtMtrlPortID.Text = string.Empty;
            //txtAvailQty.Value = 0;

            object[] tmps = C1WindowExtension.GetParameters(this);

            _SPCL_STCK_MNGT_NAME = tmps[0].ToString();
            _SPCL_STCK_MNGT_ID = tmps[1].ToString();

            txtSITE.Text = LoginInfo.CFG_SHOP_NAME;
            txtSPCL_LOT.Text = _SPCL_STCK_MNGT_NAME;

            



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

            if (txtLOTID.Text.Equals(""))
            {
                Util.AlertInfo("3");  //입력한 데이터가 없습니다.
                return;
            }

            search_lot(txtLOTID.Text);
           
        
        }




        private bool search_lot(string lotid)
        {
            DataTable dtTo = DataTableConverter.Convert(dgLotList.ItemsSource);
            DataTable dtNotTo = DataTableConverter.Convert(dgNotPossibleLotList.ItemsSource);

            if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
            {
                dtTo.Columns.Add("CHK", typeof(Boolean));
                dtTo.Columns.Add("LOTID", typeof(string));
                dtTo.Columns.Add("PRODID", typeof(string));
                dtTo.Columns.Add("BASIC_NICKNAME", typeof(string));
                dtTo.Columns.Add("REG_ENABLE_FLAG", typeof(string));
                dtTo.Columns.Add("REG_ERR_CNTT", typeof(string));
                dtTo.Columns.Add("PROCID_CR", typeof(string));

            }

            if (dtNotTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
            {
                dtNotTo.Columns.Add("CHK", typeof(Boolean));
                dtNotTo.Columns.Add("LOTID", typeof(string));
                dtNotTo.Columns.Add("PRODID", typeof(string));
                dtNotTo.Columns.Add("BASIC_NICKNAME", typeof(string));
                dtNotTo.Columns.Add("REG_ENABLE_FLAG", typeof(string));
                dtNotTo.Columns.Add("REG_ERR_CNTT", typeof(string));
                dtNotTo.Columns.Add("PROCID_CR", typeof(string));

            }

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("LOTID", typeof(string));
            RQSTDT.Columns.Add("SPCL_STCK_MNGT_ID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["LOTID"] = lotid;
            dr["SPCL_STCK_MNGT_ID"] = _SPCL_STCK_MNGT_ID;
            RQSTDT.Rows.Add(dr);
            try
            {


                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FN_SFC_STCK_MNGT_LOT", "RQSTDT", "RSLTDT", RQSTDT);


                for (int i = 0; i < dtResult.Rows.Count; i++)
                {


                    DataRow drtemp = dtResult.Rows[i];

                    if (dtResult.Rows[i]["REG_ENABLE_FLAG"].ToString() == "Y")
                    {
                        for (int j = 0; j < dtTo.Rows.Count; j++)
                        {
                            if (dtResult.Rows[i]["LOTID"].ToString().Equals(dtTo.Rows[j]["LOTID"]))
                            {
                                drtemp["REG_ERR_CNTT"] = "Already searched Lot.";
                                dtNotTo.ImportRow(drtemp);
                                Util.gridClear(dgNotPossibleLotList);
                                dgNotPossibleLotList.ItemsSource = DataTableConverter.Convert(dtNotTo);


                                return true;
                            }
                        }

                        dtTo.ImportRow(drtemp);
                        Util.gridClear(dgLotList);
                        dgLotList.ItemsSource = DataTableConverter.Convert(dtTo);
                        DataTableConverter.SetValue(dgLotList.Rows[dtTo.Rows.Count - 1].DataItem, "CHK", true);

                    }
                    else if (dtResult.Rows[i]["REG_ENABLE_FLAG"].ToString() == "N")
                    {

                        dtNotTo.ImportRow(drtemp);
                        Util.gridClear(dgNotPossibleLotList);
                        dgNotPossibleLotList.ItemsSource = DataTableConverter.Convert(dtNotTo);

                    }

                }
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

    



        private void txtPerson_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        private void btnComplete_Click(object sender, RoutedEventArgs e)
        {



            try
            {

                DataSet inDataSet = new DataSet();

                DataTable dtRqst = inDataSet.Tables.Add("RQSTDT");
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("SPCL_STCK_MNGT", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));

                DataTable InLotTable = inDataSet.Tables.Add("INLOT");
                InLotTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = dtRqst.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["SPCL_STCK_MNGT"] = _SPCL_STCK_MNGT_ID;

                dtRqst.Rows.Add(newRow);

                DataTable dtLotList = DataTableConverter.Convert(dgLotList.ItemsSource);

                for (int i = 0; i < dgLotList.Rows.Count; i++)
                {
                    if (dtLotList.Rows[i]["CHK"].Equals(true))
                    {
                        DataRow newIDRow = InLotTable.NewRow();
                        newIDRow["LOTID"] = dtLotList.Rows[i]["LOTID"];
                        InLotTable.Rows.Add(newIDRow);
                    }
                }



                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SPCL_STCK_LOT", "RQSTDT,INLOT", null, (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                        {
                            Util.MessageException(exception);
                            return;
                        }

                        Util.MessageValidation("SFU1275");  //정상 처리 되었습니다.

                    this.DialogResult = MessageBoxResult.OK;
                        this.Close();

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        this.DialogResult = MessageBoxResult.Cancel;
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }



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

        private void txtLOTID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                btnSearch_Click(sender,e);
            }
        }

        private void txtLoTID_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
               
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    string _ValueToFind = string.Empty;

                   

                    if (sPasteStrings.Length > 1)
                    {
                        for (int i = 0; i < sPasteStrings.Length; i++)
                        {

                            if (!string.IsNullOrEmpty(sPasteStrings[i]) && search_lot(sPasteStrings[i]) == false)
                                break;

                            System.Windows.Forms.Application.DoEvents();
                        }

                        e.Handled = true;
                    }
                    else
                        e.Handled = false;
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    txtLOTID.Text = "";
                    txtLOTID.Focus();

                    //HiddenLoadingIndicator();
                }
            }
        }
    }
}
