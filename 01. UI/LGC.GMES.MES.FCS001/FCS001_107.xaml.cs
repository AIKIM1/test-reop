/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2021.04.16  KDH       : SUBLOT 이동 인 이벤트에만 색상 표시
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_107 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        string _AREATYPE = "";

        public FCS001_107()
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
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            txtLotId.Focus();

            object[] parameters = this.FrameOperation.Parameters;

            //2019.12.20 해당 SEQ의 투입위치를 보여주는 칼럼 추가 하였음. 허나 임시로 A8, S4만 적용되도록 하였음.
            if (LoginInfo.CFG_AREA_ID.Equals("A8") || LoginInfo.CFG_AREA_ID.Equals("S4"))
            {
                dgHistory.Columns["EQPT_MOUNT_PSTN_NAME"].Visibility = Visibility.Visible;
            }

            if (parameters.Length > 0)
            {
                txtLotId.Text = Util.NVC(parameters[0]);

                if (!string.IsNullOrEmpty(txtLotId.Text))
                    btnSearch_Click(btnSearch, null);
            }
        }


        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string sID = Util.GetCondition(txtLotId);
            string sID2 = Util.GetCondition(txtBoxid);
            if (sID == "" && sID2 == "")
            {
                Util.MessageInfo("SFU1009");
                return;
            }
            if (sID != "")
            {
                GetLotAll("LOT");
            }
            else
            {
                GetLotAll("BOX");
            }
        }

        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtLotId.Text.Trim() != "")
                        GetLotAll("LOT");
                    else
                        return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void rbCheck_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            GetLotInfo((rb.DataContext as DataRowView).Row["LOTID"].ToString(), (rb.DataContext as DataRowView).Row["PROCID"].ToString());
            GetHistory((rb.DataContext as DataRowView).Row["LOTID"].ToString());
        }
        #endregion

        #region Mehod
        public void GetLotAll(string sType)
        {
            try
            {
                string sLOTID = string.Empty;

                if (sType == "BOX")
                {
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("CSTID", typeof(String));

                    DataRow dr2 = RQSTDT.NewRow();
                    dr2["CSTID"] = Util.GetCondition(txtBoxid);

                    RQSTDT.Rows.Add(dr2);
                    DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_SEL_CARRIER_CURR_LOT_FORM", "RQSTDT", "RSLTDT", RQSTDT);

                    if (SearchResult.Rows.Count != 0)
                    {
                        sLOTID = SearchResult.Rows[0]["CURR_LOTID"].ToString();
                    }
                    else
                    {
                        if (sType == "BOX")
                        {
                            DataTable SearchResult2 = new ClientProxy().ExecuteServiceSync("DA_SEL_CARRIER_WIPHISTORYATTR_LOT_FORM", "RQSTDT", "RSLTDT", RQSTDT);
                            if (SearchResult2.Rows.Count != 0)
                            {
                                sLOTID = SearchResult2.Rows[0]["CURR_LOTID"].ToString();
                            }
                            else
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1870"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                {
                                    if (result == MessageBoxResult.OK)
                                    {
                                        txtBoxid.Focus();
                                        Init();
                                        return;
                                    }
                                });
                            }
                        }
                        else
                        {
                            //재공 정보가 없습니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1870"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtLotId.Focus();
                                    Init();
                                    return;
                                }
                            });
                        }
                    }
                }
                else
                {
                    sLOTID = Util.GetCondition(txtLotId);
                }

                DataSet inData = new DataSet();
                DataTable dtRqst = inData.Tables.Add("INDATA");

                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("GUBUN", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLOTID;
                dr["GUBUN"] = (bool)rdoForward.IsChecked ? "F" : "R";

                dtRqst.Rows.Add(dr);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_SEL_LOT_INFO_END_FORM", "INDATA", "LOTSTATUS,TREEDATA", inData);

                Util.gridClear(dgLotInfo);

                Util.gridClear(dgHistory);

                dsRslt.Relations.Add("Relations", dsRslt.Tables["TREEDATA"].Columns["LOTID"], dsRslt.Tables["TREEDATA"].Columns["FROM_LOTID"], false);
                DataView dvRootNodes;
                dvRootNodes = dsRslt.Tables["TREEDATA"].DefaultView;
                dvRootNodes.RowFilter = "FROM_LOTID IS NULL";

                trvData.ItemsSource = dvRootNodes;
                TreeItemExpandAll();

                _AREATYPE = string.Empty;

                if (dsRslt != null && dsRslt.Tables["LOTSTATUS"].Rows.Count > 0)
                {
                    if (dsRslt.Tables["TREEDATA"].Rows.Count > 0)
                        _AREATYPE = dsRslt.Tables["TREEDATA"].Rows[0]["AREATYPE"].ToString();

                    GetLotInfo(sLOTID, dsRslt.Tables["LOTSTATUS"].Rows[0]["PROCID"].ToString());
                    GetHistory(sLOTID);
                }

                txtBoxid.Text = "";
                txtLotId.Text = "";

                if (sType == "BOX")
                {
                    txtBoxid.Focus();
                }
                else
                {
                    txtLotId.Focus();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Init();

                        if (sType == "BOX")
                        {
                            txtBoxid.Focus();
                        }
                        else
                        {
                            txtLotId.Focus();
                        }
                    }
                });
                return;
            }
        }

        private void Init()
        {
            Util.gridClear(dgLotInfo);
            Util.gridClear(dgHistory);
            trvData.ItemsSource = null;
        }

        public void GetLotInfo(string sLot, string sProcID)
        {
            try
            {
                string Stap = string.Empty;
                string sBizName = _AREATYPE.Equals("A") ? "DA_SEL_LOT_STATUS2_ASSY_FROM" : "DA_SEL_LOT_STATUS2_FORM";

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();


                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLot;
                dr["PROCID"] = sProcID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgLotInfo);

                //가로 세로로
                DataTable dtLotStatus = GetDataLot(dtResult);
                dgLotInfo.ItemsSource = DataTableConverter.Convert(dtLotStatus);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetHistory(string sLot)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                DataRow dr = RQSTDT.NewRow();


                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLot;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_WIPACTHISTORY_FORM", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgHistory);
                dgHistory.ItemsSource = DataTableConverter.Convert(dtResult);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region [동별 전극 등급 관리 여부 정보]  
        private bool EltrGrdCodeColumnVisible()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.TableName = "RQSTDT";
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "ELTR_GRD_JUDG_ITEM_CODE";
                inTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TB_MMD_AREA_COM_CODE_FORM", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        #endregion


        private DataTable GetDataLot(DataTable dataTable)
        {
            DataTable dtReturn = new DataTable();
            dtReturn.Columns.Add("ITEM", typeof(string));
            dtReturn.Columns.Add("DATA", typeof(string));

            if (dataTable.Rows.Count > 0)
            {
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("LOTID"), dataTable.Rows[0]["LOTID"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("MODELID"), dataTable.Rows[0]["MODLID"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("제품ID"), dataTable.Rows[0]["PRODID"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("제품명"), dataTable.Rows[0]["PRODNAME"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("프로젝트명"), dataTable.Rows[0]["PRJT_NAME"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("대Lot"), dataTable.Rows[0]["LOTID_RT"]);
                if (_AREATYPE.Equals("A"))
                {
                    dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("부모LOTID"), dataTable.Rows[0]["PR_LOTID"]);
                }
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("LOT유형"), dataTable.Rows[0]["LOTTYPE"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("시장유형"), dataTable.Rows[0]["MKT_TYPE_CODE"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("수량(Roll)"), Util.NVC_NUMBER(dataTable.Rows[0]["WIPQTY"]));
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("수량(Lane)"), Util.NVC_NUMBER(dataTable.Rows[0]["WIPQTY2"]));
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("HOLD"), dataTable.Rows[0]["WIPHOLD"]);
                if (!_AREATYPE.Equals("A"))
                {
                    dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("QMS 판정결과"), dataTable.Rows[0]["JUDG_NAME"]);
                }
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("버전"), dataTable.Rows[0]["PROD_VER_CODE"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("SHOP"), dataTable.Rows[0]["SHOPNAME"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("동"), dataTable.Rows[0]["AREANAME"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("라인"), dataTable.Rows[0]["EQSGNAME"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("설비"), dataTable.Rows[0]["EQPTNAME"]);
                if(LoginInfo.CFG_AREA_ID == "A7")
                {
                    dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("작업유형"), dataTable.Rows[0]["IRREGL_PROD_LOT_TYPE_NAME"]);
                }
                if (dataTable.Rows[0]["PROCID"].ToString().Equals(Process.NOTCHING))
                {
                    dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("NG Tag 개수"), dataTable.Rows[0]["NG_TAG_QTY"]);
                }
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("최종공정"), dataTable.Rows[0]["LAST_PROC"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("최종상태"), dataTable.Rows[0]["LAST_STATE"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("창고명"), dataTable.Rows[0]["WH_NAME"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("RACK ID"), dataTable.Rows[0]["RACK_ID"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("SKID ID"), dataTable.Rows[0]["SKID"]);
                //코팅라인 추가
                if (LoginInfo.CFG_AREA_ID == "A7")
                {
                    dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("Coating_LIne"), dataTable.Rows[0]["COATING_LINE"]);
                }
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("재공위치(Plant)"), dataTable.Rows[0]["LAST_SHOP"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("재공위치(동)"), dataTable.Rows[0]["LAST_AREA"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("재공위치(Line)"), dataTable.Rows[0]["LAST_EQSG"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("Carrier ID"), dataTable.Rows[0]["CSTID"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("MCS_SKIDID"), dataTable.Rows[0]["MCS_SKIDID"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("노칭위치"), dataTable.Rows[0]["NOTCH_OUT_LOT_PSTN"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("Lami셀 LEVEL3 코드"), dataTable.Rows[0]["BICELL_LEVEL3_CODE"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("셀상세분류코드"), dataTable.Rows[0]["CELL_DETL_CLSS_CODE"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("포트명"), dataTable.Rows[0]["PORT_NAME"]);
                dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("포트 설비명"), dataTable.Rows[0]["PORT_EQPTNAME"]);

                if (dataTable.Columns.Contains("PRV_CSTID"))
                    dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("PRV_CARRIERID"), dataTable.Rows[0]["PRV_CSTID"]);

                if (EltrGrdCodeColumnVisible())
                    dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("등급"), dataTable.Rows[0]["ELTR_GRD_CODE"]);

                //GQMS INTERLOCK Revision No 추가
                if (!_AREATYPE.Equals("A"))
                {
                    dtReturn.Rows.Add(ObjectDic.Instance.GetObjectName("REV_NO"), dataTable.Rows[0]["REV_NO"]);
                }

            }
            return dtReturn;
        }

        public void TreeItemExpandAll()
        {
            IList<DependencyObject> items = new List<DependencyObject>();
            VTreeHelper.GetChildrenOfType(trvData, typeof(C1TreeViewItem), ref items);

            foreach (C1TreeViewItem item in items)
            {
                TreeItemExpandNodes(item);
            }
        }

        public void TreeItemExpandNodes(C1TreeViewItem item)
        {
            item.IsExpanded = true;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                IList<DependencyObject> items = new List<DependencyObject>();
                VTreeHelper.GetChildrenOfType(item, typeof(C1TreeViewItem), ref items);
                foreach (C1TreeViewItem childItem in items)
                {
                    TreeItemExpandNodes(childItem);
                }
            }));
        }
        #endregion

        private void trvData_ItemExpanded(object sender, SourcedEventArgs e)
        {
            rb_Checked();
        }

        private void rb_Checked()
        {
            IList<DependencyObject> items = new List<DependencyObject>();
            VTreeHelper.GetChildrenOfType(trvData, typeof(C1TreeViewItem), ref items);
            foreach (C1TreeViewItem item in items)
            {
                TreeItemExpandNode(item);
            }
        }

        public void TreeItemExpandNode(C1TreeViewItem item)
        {

            item.IsExpanded = true;
            IList<DependencyObject> items = new List<DependencyObject>();
            VTreeHelper.GetChildrenOfType(item, typeof(C1TreeViewItem), ref items);
            foreach (C1TreeViewItem childItem in items)
            {
                if ((childItem.DataContext as DataRowView).Row.ItemArray[0].Equals(txtLotId.Text))
                {
                    childItem.IsSelected = true;
                }
                TreeItemExpandNode(childItem);
            }

        }

        private void txtBoxid_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtBoxid.Text.Trim() != "")
                        GetLotAll("BOX");
                    else
                        return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtLotId_GotFocus(object sender, RoutedEventArgs e)
        {
            txtLotId.SelectAll();
        }

        private void txtBoxid_GotFocus(object sender, RoutedEventArgs e)
        {
            txtBoxid.SelectAll();
        }

        private void dgHistory_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgHistory.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name.Equals("ACTNAME"))
                    {
                        if (!Util.NVC(DataTableConverter.GetValue(dgHistory.Rows[cell.Row.Index].DataItem, "ACTID")).Equals("TRANSFER_SUBLOT"))
                        {
                            return;
                        }
                        else
                        {
                            FCS001_107_CELL_LIST wndPopup = new FCS001_107_CELL_LIST();
                            wndPopup.FrameOperation = FrameOperation;

                            if (wndPopup != null)
                            {
                                object[] parameters = new object[3];
                                parameters[0] = Util.NVC(DataTableConverter.GetValue(dgHistory.Rows[cell.Row.Index].DataItem, "LOTID"));
                                parameters[1] = Util.NVC(DataTableConverter.GetValue(dgHistory.Rows[cell.Row.Index].DataItem, "WIPSEQ"));
                                parameters[2] = Util.NVC(DataTableConverter.GetValue(dgHistory.Rows[cell.Row.Index].DataItem, "ACTDTTM"));

                                C1WindowExtension.SetParameters(wndPopup, parameters);
                                wndPopup.Closed += new EventHandler(wndPopup_Closed);

                                // 팝업 화면 숨겨지는 문제 수정.
                                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void wndPopup_Closed(object sender, EventArgs e)
        {
            FCS001_107_CELL_LIST window = sender as FCS001_107_CELL_LIST;

            if (window.DialogResult == MessageBoxResult.Yes)
            {
                //GetESoStusList();
            }
        }

        private void dgHistory_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("ACTNAME"))
                    {
                        //e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Blue);
                        //e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;
                    }
                }
            }));
        }

        private void dgHistory_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //2021.04.16 SUBLOT 이동 인 이벤트에만 색상 표시 START
                DataRowView dr = (DataRowView)e.Cell.Row.DataItem;
                string sACTID = Util.NVC(dr.Row["ACTID"]);

                ////////////////////////////////////////////  default 색상 및 Cursor
                e.Cell.Presenter.Cursor = Cursors.Arrow;

                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                e.Cell.Presenter.FontSize = 12;
                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                ///////////////////////////////////////////////////////////////////////////////////

                //link 색변경
                if (e.Cell.Column.Name.Equals("ACTNAME") && sACTID.Equals("TRANSFER_SUBLOT"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    e.Cell.Presenter.Cursor = Cursors.Hand;
                }
                //2021.04.16 SUBLOT 이동 인 이벤트에만 색상 표시 END
            }));
        }
    }
}
