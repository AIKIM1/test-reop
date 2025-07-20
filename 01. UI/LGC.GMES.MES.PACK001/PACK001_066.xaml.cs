/*************************************************************************************
 Created Date : 2020.05.26
      Creator : 
   Decription : CELL �̵� ó�� �� CMA �ϼ� LOT �̵� ó��, �μ� ȭ��
CMA ��� �� �̵��� END�� ó�� �� �̵�
--------------------------------------------------------------------------------------
 [Change History]
  2020.06.23  �ֿ켮   CSR ID 61613, ���� �԰� �� Cell ���� �̵� ó�� �ȷ�Ʈ ��ȸ �� ���� ���� [��û��ȣ] C20200518-000493
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_066 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        string sTagetArea = string.Empty;
        string sTagetEqsg = string.Empty;

        /// <summary>
        /// Frame�� ��ȣ�ۿ��ϱ� ���� ��ü
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_066()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
  
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Button> listAuth = new List<Button>();
                listAuth.Add(btnTagetInputComfirm);

                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

                dtpDateFrom.SelectedDateTime = DateTime.Now.AddDays(-7);
                dtpDateTo.SelectedDateTime = DateTime.Now;

                CommonCombo _combo = new CommonCombo();

                _combo.SetCombo(cboMoveToArea, CommonCombo.ComboStatus.NONE, sCase: "AREA");
                cboMoveToArea.SelectedValue = LoginInfo.CFG_AREA_ID;


                tbTagetListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("��") + " ]";
                tbSearchListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("��") + " ]";

                this.Loaded -= new RoutedEventHandler(this.UserControl_Loaded);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        #endregion


        #region Object Event -------------------------------------------------------------------------------------------------------------------------------------

        //�̵� �ȷ�Ʈ �Է�
        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    //dgTagetList.LoadedCellPresenter -= dgTagetListCellPresenter;
                    //dgTagetList.LoadedCellPresenter += dgTagetListCellPresenter;

                    GetTagetPalletInfoNew();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //��� ��ư
        private void btnTagetCancel_Click(object sender, RoutedEventArgs e)
        {
            //�۾��� ����Ͻðڽ��ϱ� ?
            Util.MessageConfirm("SFU1168", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Util.gridClear(dgTargetList);
                    ClearInput(); 
                }
            });

        }

        //�μ� �Ϸ� ��ư
        private void btnTagetInputComfirm_Click(object sender, RoutedEventArgs e)
        {
            // Declaration
            int FCSDataCheck = 0;

            try
            {
                if (this.dgTargetList.Rows.Count <= 0)
                {
                    Util.MessageInfo("SFU1796"); //�԰� ����� �����ϴ�. PALLETID�� �Է� �ϼ���.
                    return;
                }

                // �Է� üũ
                if (!this.ChkInputData())
                {
                    return;
                }

                // ���Ÿ� �˻� ���� �� �Ѷ��� ���� ���Ͷ� ���
                if (ChkReceive())
                {
                    Util.MessageInfo("SFU5139");    // ������ Cell�Դϴ�.
                    return;
                }

                // FCS Data Check (OCV ������ ����)
                if (this.ChkOCV_Exist())
                {
                    FCSDataCheck++;
                }

                // FCS Data Check (DCIR ������ ����)
                if (this.ChkDCIR_Exist())
                {
                    FCSDataCheck++;
                }

                // ���� �ΰ��� üũ �Լ� ����� �ϳ��� true�̸� Ȯ���Ŀ� �԰�ó��
                if (FCSDataCheck > 0)
                {
                    // OCV DATA�� ���� CELL�� �����մϴ�. �԰��Ͻðڽ��ϱ�?"
                    Util.MessageConfirm("SFU1405", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            CheckPopUpOpen();
                        }
                        else
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                        }
                    });
                }
                else    // �׷��� ������ �׳� �԰�ó��
                {
                    CheckPopUpOpen();   // ���� üũ �˾� ���� ���� close �� ��ȸ�� �԰�ó��.
                }

                //if (dgTagetList.Rows.Count > 0)
                //{
                //    if (ChkInputData()) //�Է� üũ
                //    {
                //        //������ �˻� ���� �� �Ѷ��� ���� ���Ͷ� ���
                //        if (!ChkReceive())
                //        {
                //            if (ChkOCV_Exist())
                //            {
                //                //OCV DATA�� ���� CELL�� �����մϴ�. �԰��Ͻðڽ��ϱ�?"
                //                Util.MessageConfirm("SFU1405", (result) =>
                //                {
                //                    if (result == MessageBoxResult.OK)
                //                    {
                //                        CheckPopUpOpen();
                //                    }
                //                });
                //            }
                //            else
                //            {
                //                CheckPopUpOpen();//���� üũ �˾� ���� ���� close �� ��ȸ�� �԰�ó��.
                //            }
                //        }
                //        //2019.11.07
                //        else
                //        {
                //            Util.MessageInfo("SFU5139"); //������ Cell�Դϴ�.
                //        }
                //    }
                //}
                //else
                //{
                //    Util.MessageInfo("SFU1796"); //�԰� ����� �����ϴ�. PALLETID�� �Է� �ϼ���.
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //��ȸ ��ư
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetWareHousingData();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }

        }

        //���� ���� ��ư
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new ExcelExporter().Export(dgSearchResultList);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }

        }

        //�� ����
        private void cboTagetModel_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                SetComboBox_Route_schd(Util.NVC(cboTagetModel.SelectedValue), txtTagetPRODID.Text);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //��� ����
        private void cboTagetRoute_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

            try
            {
                if (cboTagetRoute.SelectedItem == null)
                {
                    sTagetArea = "";
                }
                else
                {
                    sTagetArea = Convert.ToString(DataTableConverter.GetValue(cboTagetRoute.SelectedItem, "AREAID"));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion Object Event ----------------------------------------------------------------------------------------------------------------------------------

        #region Grid Event ---------------------------------------------------------------------------------------------------------------------------------------

        //�ȷ�Ʈ ���� ����
        private void dgTagetListCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dataGrid = sender as C1DataGrid;

                if (e.Cell.Presenter == null) return;
                if (e.Cell.Row.Type != DataGridRowType.Item) return;
                if (dgTargetList == null) return;

                if (e.Cell.Column.Name == "OCV_FLAG")
                {
                    if (e.Cell.Text == "N")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                }

                if (e.Cell.Column.Name == "RECEIVABLE_FLAG")
                {
                    if (e.Cell.Text == "N")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;

                        if (!(dataGrid.GetCell(e.Cell.Row.Index, 1).Presenter == null))
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, 1).Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            dataGrid.GetCell(e.Cell.Row.Index, 1).Presenter.FontWeight = FontWeights.Bold;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        //���콺 ���� Ŭ�� - �ȷ�Ʈ ���� �˾�
        private void dgTagetList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //PALLETID
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgTargetList.GetCellFromPoint(pnt);
                if (cell != null)
                {
                    if (cell.Row.Index > -1)
                    {
                        if (cell.Column.Name == "PALLETID")
                        {
                            string sRcvIssId = Util.NVC(DataTableConverter.GetValue(dgTargetList.Rows[cell.Row.Index].DataItem, "RCV_ISS_ID"));
                            string sSelectPalletId = Util.NVC(DataTableConverter.GetValue(dgTargetList.Rows[cell.Row.Index].DataItem, "PALLETID"));

                            popUpOpenPalletInfo(sRcvIssId, sSelectPalletId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        //��ȸ ��� �ȷ�Ʈ ���� �˾�
        private void dgSearchResultList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //PALLETID
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgSearchResultList.GetCellFromPoint(pnt);
                if (cell != null)
                {
                    if (cell.Row.Index > -1)
                    {
                        if (cell.Column.Name == "BOXID")
                        {
                            string sRcvIssId = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[cell.Row.Index].DataItem, "RCV_ISS_ID"));
                            string sSelectPalletId = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[cell.Row.Index].DataItem, "BOXID"));

                            popUpOpenPalletInfo(sRcvIssId, sSelectPalletId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        //������
        private void dgSearchResultList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dataGrid = sender as C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if (e.Cell.Column.Name == "BOXID")
                    {

                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        #endregion Grid Event ------------------------------------------------------------------------------------------------------------------------------------

        #region User Method --------------------------------------------------------------------------------------------------------------------------------------
        //����LOT �׸��� �߰�
        private bool GridLotBinding(DataTable dtData)
        {
            try
            {
                List<string> sLotIds = new List<string>();
                DataTable DTTMP = DataTableConverter.Convert(dgTargetList.ItemsSource);

                if (!dtData.Columns.Contains("CHK"))
                {
                    dtData.Columns.Add("CHK");
                    dtData.Columns["CHK"].DefaultValue = "False";
                }

                DTTMP.Merge(dtData);

                for (int i = 0; i < DTTMP.Rows.Count; i++)
                {
                    sLotIds.Add(DTTMP.Rows[i]["PALLETID"].ToString());
                }

                if (sLotIds.Distinct().Count() != sLotIds.Count())
                {
                    //�ߺ� LOT ����
                    Util.MessageInfo("SFU1410", (action) => { txtPalletID.Focus(); }, txtPalletID.Text);

                    return false;
                }


                Util.GridSetData(dgTargetList, DTTMP, FrameOperation, true);

                dgTargetList.EndEdit();
                dgTargetList.EndEditRow(true);

                Util.SetTextBlockText_DataGridRowCount(tbTagetListCount, Util.NVC(DTTMP.Rows.Count));

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex, (action) => { txtPalletID.Focus(); });
                return false;
            }
        }

        //���� ���� �ʱ�ȭ
        private void ClearInput()
        {
            try
            {
                txtTagetPRODID.Text = string.Empty;
                txtTagetPRODNAME.Text = string.Empty;

                cboTagetModel.ItemsSource = null;
                cboTagetModel.SelectedValue = null;

                cboTagetRoute.ItemsSource = null;
                cboTagetRoute.SelectedValue = null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //�μ� �Ϸ� ó�� Validation
        private bool ChkInputData()
        {
            bool bReturn = true;

            try
            {
                if (cboTagetRoute.SelectedIndex < 0)
                {
                    Util.MessageInfo("SFU1455", (action) => { cboTagetRoute.Focus(); }); //��θ� ���� �ϼ���
                    bReturn = false;
                    return bReturn;
                }

                if (cboTagetModel.SelectedIndex < 0)
                {
                    Util.MessageInfo("SFU1619", (action) => { cboTagetModel.Focus(); }); //���꿹������ ���� �ϼ���
                    bReturn = false;
                    return bReturn;
                }

                bReturn = ChkGbtInPallet();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return bReturn;
        }

        // CELL�ΰ�� FCS ������ ���� üũ (OCV)
        private bool ChkOCV_Exist()
        {
            bool bReturn = false;
            try
            {
                if(txtPrdType.Text.Trim().Equals("CELL"))
                {
                    DataTable dt = DataTableConverter.Convert(dgTargetList.ItemsSource);
                    if (dt.Select("OCV_FLAG = 'N'").Length > 0) // OCV���� ���� üũ
                    {
                        bReturn = true;
                    }

                }
                else
                {
                    bReturn = false;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return bReturn;
        }

        // CELL�ΰ�� FCS ������ ���� üũ (DCIR)
        private bool ChkDCIR_Exist()
        {
            bool bReturn = false;
            try
            {
                if (txtPrdType.Text.Trim().Equals("CELL"))
                {
                    DataTable dt = DataTableConverter.Convert(dgTargetList.ItemsSource);
                    if (dt.Select("DCIR_FLAG = 'N'").Length > 0) // DCIR ���� ���� üũ
                    {
                        bReturn = true;
                    }
                }
                else
                {
                    bReturn = false;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return bReturn;
        }


        //�μ� Ȯ�� �˾� â
        private void CheckPopUpOpen()
        {
            #region ����Ȯ�� �˾�
            PACK001_019_RECEIVEPRODUCT_SELECTCHECK popup = new PACK001_019_RECEIVEPRODUCT_SELECTCHECK();
            popup.FrameOperation = this.FrameOperation;
            if (popup != null)
            {
                DataTable dtData = new DataTable();
                dtData.Columns.Add("MODELNAME", typeof(string));
                dtData.Columns.Add("PRODUCTNAME", typeof(string));
                dtData.Columns.Add("PRODID", typeof(string));
                dtData.Columns.Add("ROUTENAME", typeof(string));
                dtData.Columns.Add("LOTTYPE", typeof(string));

                DataRow newRow = null;

                newRow = dtData.NewRow();
                newRow["MODELNAME"] = cboTagetModel.Text;
                newRow["PRODUCTNAME"] = txtTagetPRODNAME.Text;
                newRow["PRODID"] = txtTagetPRODID.Text;
                newRow["ROUTENAME"] = cboTagetRoute.Text;
                newRow["LOTTYPE"] = "";//cboTagetLotType.Text;
                dtData.Rows.Add(newRow);

                //========================================================================
                object[] Parameters = new object[2];
                Parameters[0] = dtData;
                Parameters[1] = GetSaveWarehousing_DataSet();
                C1WindowExtension.SetParameters(popup, Parameters);
                //========================================================================
                popup.Closed -= Popup_Closed;
                popup.Closed += Popup_Closed;
                popup.ShowModal();
                popup.CenterOnScreen();
            }
            #endregion
        }

        //�˾� �ݱ� ó��
        void Popup_Closed(object sender, EventArgs e)
        {
            PACK001_019_RECEIVEPRODUCT_SELECTCHECK popup = sender as PACK001_019_RECEIVEPRODUCT_SELECTCHECK;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                SetWarehousing(); //�԰�ó��
            }
            else
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        //�ȷ�Ʈ ���� �˾�
        private void popUpOpenPalletInfo(string sRcvIssId, string sPalletId)
        {
            try
            {
                PACK001_019_RECEIVEPRODUCT_PALLETINFO popup = new PACK001_019_RECEIVEPRODUCT_PALLETINFO();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = new object[2];
                    Parameters[0] = sRcvIssId;
                    Parameters[1] = sPalletId;
                    C1WindowExtension.SetParameters(popup, Parameters);

                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Ȯ�� �˾� ������ ����
        private DataSet GetSaveWarehousing_DataSet()
        {
            DataSet dsIndata = new DataSet();
            try
            {
                DataTable dt = DataTableConverter.Convert(dgTargetList.ItemsSource);
                //RCV_ISS_ID groupby ����
                var list = dt.AsEnumerable().GroupBy(r => new
                {
                    ISSIDGROUP = r.Field<string>("RCV_ISS_ID")
                }).Select(g => g.First());
                DataTable dtRCV_ISS_IDList = list.CopyToDataTable();

                DataRow drINDATA = null;
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("MODLID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("ROUTID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("RCV_ISS_ID", typeof(string));

                for (int i = 0; i < dtRCV_ISS_IDList.Rows.Count; i++)
                {
                    drINDATA = INDATA.NewRow();
                    drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    drINDATA["LANGID"] = LoginInfo.LANGID;
                    drINDATA["MODLID"] = cboTagetModel.SelectedValue;
                    drINDATA["AREAID"] = sTagetArea;
                    drINDATA["EQSGID"] = sTagetEqsg;
                    drINDATA["ROUTID"] = cboTagetRoute.SelectedValue;
                    drINDATA["USERID"] = LoginInfo.USERID;
                    drINDATA["RCV_ISS_ID"] = Util.NVC(dtRCV_ISS_IDList.Rows[i]["RCV_ISS_ID"]);
                    INDATA.Rows.Add(drINDATA);
                }


                DataRow drRCV_ISS = null;
                DataTable dtRCV_ISS = new DataTable();
                dtRCV_ISS.TableName = "RCV_ISS";
                dtRCV_ISS.Columns.Add("RCV_ISS_ID", typeof(string));
                dtRCV_ISS.Columns.Add("PALLETID", typeof(string));

                for (int i = 0; i < dgTargetList.Rows.Count; i++)
                {
                    drRCV_ISS = dtRCV_ISS.NewRow();
                    drRCV_ISS["RCV_ISS_ID"] = Util.NVC(DataTableConverter.GetValue(dgTargetList.Rows[i].DataItem, "RCV_ISS_ID"));
                    drRCV_ISS["PALLETID"] = Util.NVC(DataTableConverter.GetValue(dgTargetList.Rows[i].DataItem, "PALLETID"));
                    dtRCV_ISS.Rows.Add(drRCV_ISS);
                }


                dsIndata.Tables.Add(INDATA);
                dsIndata.Tables.Add(dtRCV_ISS);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dsIndata;
        }

        private void SetComboBox_Model_schd(string sMTRLID)
        {
            try
            {
                string strBizName = string.Empty;
                strBizName = txtPrdType.Text.Equals("CELL") ? "DA_BAS_SEL_MODLID_BY_MTRLID_MBOM_DETL_CBO" : "DA_PRD_SEL_MODLID_BY_PRDT_DETL_CBO";

                DataTable dtIndata = new DataTable();
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("MTRLID", typeof(string));
                dtIndata.Columns.Add("AREAID", typeof(string));
                DataRow drIndata = dtIndata.NewRow();

                drIndata["LANGID"] = LoginInfo.LANGID;
                drIndata["MTRLID"] = sMTRLID;

                if (LoginInfo.CFG_SHOP_ID.Equals("G481"))
                {
                    drIndata["AREAID"] = cboMoveToArea.SelectedValue;
                }
                else
                {
                    drIndata["AREAID"] = sTagetArea == "" ? null : sTagetArea;
                }

                dtIndata.Rows.Add(drIndata);
                new ClientProxy().ExecuteService(strBizName, "INDATA", "OUTDATA", dtIndata, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    if (dtResult.Rows.Count > 0)
                    {
                        DataRow dr = dtResult.NewRow();
                        dr["CBO_NAME"] = "-ALL-";
                        dr["CBO_CODE"] = null;
                        dtResult.Rows.Add(dr);
                    }


                    cboTagetModel.ItemsSource = DataTableConverter.Convert(dtResult);

                    if (dtIndata.Rows.Count > 0)
                    {
                        cboTagetModel.SelectedIndex = 0;
                        cboTagetModel_SelectedValueChanged(null, null);
                    }
                    else
                    {
                        cboTagetModel_SelectedValueChanged(null, null);
                    }

                }
                );
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SetComboBox_Route_schd(string sMODLID, string sMTRLID)
        {
            try
            {
                string strBizName = string.Empty;
                strBizName = txtPrdType.Text.Equals("CELL") ? "DA_BAS_SEL_ROUTID_BY_MTRLID_MBOM_DETL_CBO" : "DA_PRD_SEL_ROUTID_BY_PRDT_DETL_CBO";

                DataTable dtIndata = new DataTable();
                dtIndata.TableName = "RQSTDT";
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("MODLID", typeof(string));
                dtIndata.Columns.Add("MTRLID", typeof(string));
                dtIndata.Columns.Add("AREAID", typeof(string));
                DataRow drIndata = dtIndata.NewRow();

                drIndata["LANGID"] = LoginInfo.LANGID;
                drIndata["MODLID"] = sMODLID == "" ? null : sMODLID;
                drIndata["MTRLID"] = sMTRLID;
                // 2020-01-17 - ���Թ�S 
                // �������� ��� Itransit ����������, AEARID�� ��� DB�� �и� �Ǿ��ִ� ��Ȳ����, Ÿ������ Ÿ�� CELL �԰�� ISSUSE �߻�
                // �ش� ���뿡 ���ؼ�, Login�� ������ �԰� ó�� �����ϵ���, Login ��ġ�� ó��
                if (LoginInfo.CFG_SHOP_ID.Equals("G481"))
                {
                    drIndata["AREAID"] = cboMoveToArea.SelectedValue;
                }
                else
                {
                    drIndata["AREAID"] = sTagetArea == "" ? null : sTagetArea;
                }

                dtIndata.Rows.Add(drIndata);
                new ClientProxy().ExecuteService(strBizName, "RQSTDT", "RSLTDT", dtIndata, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        //Util.AlertByBiz("DA_BAS_SEL_ROUTID_BY_MTRLID_MBOM_DETL_CBO", ex.Message, ex.ToString());
                        Util.MessageException(ex);
                        return;
                    }

                    cboTagetRoute.ItemsSource = DataTableConverter.Convert(dtResult);

                    if (dtIndata.Rows.Count > 0)
                    {
                        cboTagetRoute.SelectedIndex = 0;
                    }
                    else
                    {
                        Util.MessageInfo("����� ������ �������� �ʽ��ϴ�.");
                    }
                }
                );
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion User Method -----------------------------------------------------------------------------------------------------------------------------------

        #region Biz Process --------------------------------------------------------------------------------------------------------------------------------------

        //���� �ȷ�Ʈ ���� ȣ��
        private void GetTagetPalletInfoNew()
        {
            try
            {
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("PALLETID", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("PRDTYPE", typeof(string));
                INDATA.Columns.Add("TO_AREAID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PALLETID"] = txtPalletID.Text;
                dr["PRODID"] = txtTagetPRODID.Text;
                dr["PRDTYPE"] = txtPrdType.Text;
                dr["TO_AREAID"] = cboMoveToArea.SelectedValue;

                INDATA.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_RECEIVE_LOT_MOVE", "INDATA", "OUTDATA", INDATA);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    txtTagetPRODID.Text = Util.NVC(dtResult.Rows[0]["PRODID"]);
                    txtTagetPRODNAME.Text = Util.NVC(dtResult.Rows[0]["PRODNAME"]);
                    txtPrdType.Text = Util.NVC(dtResult.Rows[0]["PRDTYPE"]);
                    sTagetArea = Util.NVC(dtResult.Rows[0]["TO_AREAID"]);
                    SetComboBox_Model_schd(txtTagetPRODID.Text);

                    GridLotBinding(dtResult);

                    //DataTable dtBefore = DataTableConverter.Convert(dgTagetList.ItemsSource);
                    //dtBefore.Merge(dtResult);

                    //dgTagetList.ItemsSource = DataTableConverter.Convert(dtBefore);
                    //Util.SetTextBlockText_DataGridRowCount(tbTagetListCount, Util.NVC(dtBefore.Rows.Count));
                }
                txtPalletID.Text = "";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //������ üũ(CELL��)
        private bool ChkReceive()
        {
            bool bReturn = false;

            try
            {
                if (txtPrdType.Text.Trim().Equals("CELL"))
                {
                    DataTable dt = DataTableConverter.Convert(dgTargetList.ItemsSource);

                    DataRow drINDATA = null;
                    DataTable INDATA = new DataTable();
                    INDATA.TableName = "INDATA";
                    INDATA.Columns.Add("EQSGID", typeof(string));
                    INDATA.Columns.Add("PROCID", typeof(string));
                    INDATA.Columns.Add("PRODID", typeof(string));
                    INDATA.Columns.Add("ROUTID", typeof(string));
                    INDATA.Columns.Add("RCV_ISS_ID", typeof(string));
                    INDATA.Columns.Add("PALLETID", typeof(string));

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        drINDATA = INDATA.NewRow();

                        drINDATA["EQSGID"] = sTagetEqsg;
                        drINDATA["PROCID"] = "P1000";
                        drINDATA["ROUTID"] = cboTagetRoute.SelectedValue;
                        drINDATA["PRODID"] = Util.NVC(dt.Rows[i]["PRODID"]);
                        drINDATA["RCV_ISS_ID"] = Util.NVC(dt.Rows[i]["RCV_ISS_ID"]);
                        drINDATA["PALLETID"] = Util.NVC(dt.Rows[i]["PALLETID"]);

                        INDATA.Rows.Add(drINDATA);
                    }

                    loadingIndicator.Visibility = Visibility.Visible;

                    DataTable dtReturn = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_RECEIVE_PRODUCT_PACK", "INDATA", "OUTDATA", INDATA);

                    if (dtReturn != null)
                    {
                        int StandDay = int.Parse(dtReturn.Rows[0]["STAND_TIME"].ToString());
                        int OverDay = int.Parse(dtReturn.Rows[0]["OVER_TIIME"].ToString());

                        if (StandDay > 0)
                        {
                            if (OverDay >= StandDay)
                            {
                                bReturn = true;
                            }
                            else
                            {
                                bReturn = false;
                            }
                        }
                        else
                        {
                            bReturn = false;
                        }
                    }
                }
                else
                {
                    bReturn = false;
                }    

                return bReturn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //���� �ȷ�Ʈ�� GBTID �ߺ� üũ
        private bool ChkGbtInPallet()
        {
            try
            {
                if (txtPrdType.Text.Trim().Equals("CELL"))
                {
                    loadingIndicator.Visibility = Visibility.Visible;

                    DataTable dt = DataTableConverter.Convert(dgTargetList.ItemsSource);

                    object[] arrPallet = dt.Select().Where(y => y["CHK"].ToString() == "True").Select(x => x["PALLETID"]).ToArray();
                    object[] arrRcv = dt.Select().Where(y => y["CHK"].ToString() == "True").Select(x => x["RCV_ISS_ID"]).ToArray();

                    string[] arrPalletStr = arrPallet.Cast<string>().ToArray();
                    string[] arrRcvStr = arrRcv.Cast<string>().ToArray();

                    string strSeparator = ",";

                    string strPallet = string.Join(strSeparator, arrPalletStr);
                    string strRcv = string.Join(strSeparator, arrRcvStr);


                    DataTable INDATA = new DataTable();

                    INDATA.TableName = "INDATA";
                    INDATA.Columns.Add("RCV_ISS_ID", typeof(string));
                    INDATA.Columns.Add("PALLETID", typeof(string));

                    DataRow dr = INDATA.NewRow();
                    dr["RCV_ISS_ID"] = strRcv;
                    dr["PALLETID"] = strPallet;

                    INDATA.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_RECEIVE_PRODUCT_GBT", "INDATA", null, INDATA);
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    return true;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                //Util.MessageInfo(ex.Message.ToString());
                Util.MessageInfo(ex.Data["CODE"].ToString());
                return false;
            }
        }

        //�μ� �Ϸ�
        private void SetWarehousing()
        {
            try
            {

                DataTable dt = DataTableConverter.Convert(dgTargetList.ItemsSource);
                //RCV_ISS_ID groupby ����
                //var list = dt.AsEnumerable().GroupBy(r => new
                //{
                //    ISSIDGROUP = r.Field<string>("RCV_ISS_ID")
                //}).Select(g => g.First());
                //DataTable dtRCV_ISS_IDList = list.CopyToDataTable();

                DataRow drINDATA = null;
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("SHOPID", typeof(string));
                INDATA.Columns.Add("RCV_ISS_ID", typeof(string));
                INDATA.Columns.Add("BOXID", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("MODLID", typeof(string));
                INDATA.Columns.Add("ROUTID", typeof(string));
                INDATA.Columns.Add("FROM_AREAID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    drINDATA = INDATA.NewRow();
                    drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    drINDATA["LANGID"] = LoginInfo.LANGID;
                    drINDATA["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                    drINDATA["RCV_ISS_ID"] = dt.Rows[i]["RCV_ISS_ID"].ToString(); ;
                    drINDATA["BOXID"] = dt.Rows[i]["PALLETID"].ToString(); ;
                    drINDATA["PRODID"] = dt.Rows[i]["PRODID"].ToString(); ;
                    drINDATA["MODLID"] = cboTagetModel.SelectedValue;
                    drINDATA["ROUTID"] = cboTagetRoute.SelectedValue;
                    drINDATA["FROM_AREAID"] = dt.Rows[i]["FROM_AREAID"].ToString();
                    drINDATA["USERID"] = LoginInfo.USERID;
                    INDATA.Rows.Add(drINDATA);
                }

                DataSet dsIndata = new DataSet();
                dsIndata.Tables.Add(INDATA);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RECEIVE_LOT_MOVE", "INDATA", "OUTDATA", (dsResult, dataException) =>
                {
                    try
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        if (dataException != null)
                        {
                            Util.MessageException(dataException);
                            return;
                        }
                        else
                        {
                            if (dsResult != null && dsResult.Tables.Count > 0)
                            {
                                if (dsResult.Tables["OUTDATA"].Rows.Count > 0)
                                {
                                    Util.MessageInfo("SFU1412");//PALLET���԰��Ͽ����ϴ�
                                    Util.gridClear(dgTargetList);
                                    Util.SetTextBlockText_DataGridRowCount(tbTagetListCount, "0");
                                    ClearInput();
                                    GetWareHousingData();//��ȸ
                                }
                            }
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        throw ex;
                    }

                }, dsIndata);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //�μ� ���� ��ȸ
        private void GetWareHousingData()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("ISS_DTTM_FROM", typeof(DateTime));
                RQSTDT.Columns.Add("ISS_DTTM_TO", typeof(DateTime));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = null;
                dr["EQSGID"] = null; //Util.NVC(cboSearchEQSGID.SelectedValue) == "" ? null : Util.NVC(cboSearchEQSGID.SelectedValue);

                dr["ISS_DTTM_FROM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd") + " 00:00:00";
                dr["ISS_DTTM_TO"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd") + " 23:59:59";


                RQSTDT.Rows.Add(dr);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_RECEIVE_LOT_MOVE_PALLET", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgSearchResultList, dtResult, FrameOperation, true);
                Util.SetTextBlockText_DataGridRowCount(tbSearchListCount, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion Biz Process -----------------------------------------------------------------------------------------------------------------------------------


    }
}