/*************************************************************************************
 Created Date : 2018.05.08
      Creator : INS 김동일K
   Decription : 재튜빙 LOTID 추적 관리
--------------------------------------------------------------------------------------
 [Change History]
  2018.05.08 이상훈 최초작성
  
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ASSY_RETUBING_LOT_INPUT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_RETUBING_LOT_INPUT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();
        private string sLotGbrtMenuId = "SFU010160350";
        private string sLotGbrtMenuNM = "활성화 LOT별 검사현황 조회";
        
        #endregion

        #region Initialize 
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_ASSY_RETUBING_LOT_INPUT()
        {
            InitializeComponent();
            Initcombo();
        }


        private void Initcombo()
        {

            DataTable cboDT = new DataTable();
            cboDT.TableName = "RQSTDT";
            cboDT.Columns.Add("CBO_NAME", typeof(string));
            cboDT.Columns.Add("CBO_CODE", typeof(string));

            DataRow newRow = cboDT.NewRow();
            newRow["CBO_NAME"] = "Y";
            newRow["CBO_CODE"] = "Y";
            cboDT.Rows.Add(newRow);
            newRow = null;
            newRow = cboDT.NewRow();
            newRow["CBO_NAME"] = "N";
            newRow["CBO_CODE"] = "N";
            cboDT.Rows.Add(newRow);

            (dgRetubingList.Columns["DEL_FLAG"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(cboDT.Copy());

            newRow = null;
            newRow = cboDT.NewRow();
            newRow["CBO_NAME"] = "ALL";
            newRow["CBO_CODE"] = "ALL";
            cboDT.Rows.InsertAt(newRow,0);

            C1ComboBox cbo = cboDelFlag;
            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = cboDT.Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }

        #endregion

        #region Event
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

                if (tmps != null && tmps.Length >= 1)
                {
                    txtLotID.Text = Util.NVC(tmps[0]);
                }
                else
                {
                    txtLotID.Text = "";
                }
                getRetubingList();
                //ApplyPermissions();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            DataGridRowAdd(dgRetubingList);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DataGridRowRemove(dgRetubingList);
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            getRetubingList();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSaveRetubingHist())
                return;

            Util.MessageConfirm("SFU3533", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    setSaveRetubingHist();
                }
            });
        }
        

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod

        #region [BizCall]

        private void getRetubingList()
        {
            try
            {
                //입력값 확인
                int iLotIdLen = this.txtLotID.Text.Trim().Length;
                int iPerLotIdLen = this.txtPreLotID.Text.Trim().Length;

                if (iLotIdLen == 0 && iPerLotIdLen == 0)
                {
                    Util.MessageInfo("재튜빙LOTID 또는 이전 LOTID를 입력해야 합니다.");
                    return;
                }

                if (iLotIdLen >= 5  || iPerLotIdLen >= 5)
                {
                    //
                }
                else
                {
                    Util.MessageInfo("5자리 이상 입력해야 합니다.");
                    return;
                }

                loadingIndicator.Visibility = Visibility.Visible;
                    
                dgRetubingList.EndEdit();

                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LOTID", typeof(string));
                dtRQSTDT.Columns.Add("PRE_LOTID", typeof(string));
                dtRQSTDT.Columns.Add("DEL_FLAG", typeof(string));

                DataRow newRow = dtRQSTDT.NewRow();
                if (this.txtLotID.Text.Trim().Length > 0)
                    newRow["LOTID"] = this.txtLotID.Text.Trim();

                if (this.txtPreLotID.Text.Trim().Length > 0)
                    newRow["PRE_LOTID"] = this.txtPreLotID.Text.Trim();

                if (!this.cboDelFlag.SelectedValue.ToString().Equals("ALL"))
                    newRow["DEL_FLAG"] = this.cboDelFlag.SelectedValue.ToString();


                dtRQSTDT.Rows.Add(newRow);
                    
                new ClientProxy().ExecuteService("DA_PRD_SEL_TB_SFC_RE_TUBE_LOT_GNRT_HIST", "RQSTDT", "RSLTDT", dtRQSTDT, (dtResult, Exception) =>
                {
                    if (Exception != null)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (!dtResult.Columns.Contains("CHK"))
                        dtResult.Columns.Add("CHK");
                    Util.GridSetData(dgRetubingList, dtResult, FrameOperation, true);
                    loadingIndicator.Visibility = Visibility.Collapsed;
                });

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void setSaveRetubingHist(bool bShowMsg = true)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                
                dgRetubingList.EndEdit();

                List<int> iReturnList = _Util.GetDataGridCheckRowIndex(dgRetubingList, "CHK");

                if (iReturnList.Count <= 0)
                {
                    // SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                DataRow newRow = inDataTable.NewRow();
                inDataTable.Columns.Add("LOTID");
                inDataTable.Columns.Add("PRE_LOTID");
                inDataTable.Columns.Add("USERID");
                inDataTable.Columns.Add("LOT_GNRT_MENUID");
                inDataTable.Columns.Add("DEL_FLAG");


                foreach (int row in iReturnList)
                {
                    newRow = inDataTable.NewRow();
                    newRow["LOTID"] = Util.NVC(dgRetubingList.GetCell(row, dgRetubingList.Columns["LOTID"].Index).Value); 
                    newRow["PRE_LOTID"] = Util.NVC(dgRetubingList.GetCell(row, dgRetubingList.Columns["PRE_LOTID"].Index).Value); 
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["LOT_GNRT_MENUID"] = Util.NVC(dgRetubingList.GetCell(row, dgRetubingList.Columns["LOT_GNRT_MENUID"].Index).Value);
                    
                    newRow["DEL_FLAG"] = Util.NVC(dgRetubingList.GetCell(row, dgRetubingList.Columns["DEL_FLAG"].Index).Value); 
                    inDataTable.Rows.Add(newRow);
                }
                

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_TB_SFC_RE_TUBE_LOT_GNRT_HIST", "INDATA", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (bShowMsg)
                            Util.MessageInfo("SFU1275");      //정상 처리 되었습니다.

                        //GetDefectInfo();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, indataSet); 
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Validation]
        private bool CanSaveRetubingHist()
        {
            
            if (!CommonVerify.HasDataGridRow(dgRetubingList))
            {
                Util.MessageValidation("SFU3534");      //불량 항목이 없습니다.
                return false;
            }
            
            return true;
        }

        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            /*
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnDefectSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            */
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

        //private void GetAllData()
        //{
        //    GetDefectInfo();
        //}

        #endregion

        #endregion

        #region [Button]

        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                DataTable dt = new DataTable();
                if (dg.ItemsSource == null || dg.Rows.Count < 0)
                {
                    return;
                }

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }

                dt = DataTableConverter.Convert(dg.ItemsSource);
                DataRow dr = dt.NewRow();
                dr["CHK"] = 1;
                dr["LOTID"] = this.txtLotID.Text.Trim();
                dr["PRE_LOTID"] = this.txtPreLotID.Text.Trim();
                dr["LOT_GNRT_MENUID"] = sLotGbrtMenuId;
                dr["LOT_GNRT_MENUID_NM"] = sLotGbrtMenuNM;
                dr["DEL_FLAG"] = "N";

                dt.Rows.Add(dr);
                dt.AcceptChanges();

                dg.ItemsSource = DataTableConverter.Convert(dt);

                // 스프레드 스크롤 하단으로 이동
                //dg.ScrollIntoView(dg.GetRowCount() - 1, 0);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void DataGridRowRemove(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                DataTable dtInfo = DataTableConverter.Convert(dg.ItemsSource);

                List<DataRow> drInfo = dtInfo.Select("CHK = 1")?.ToList();
                foreach (DataRow dr in drInfo)
                {
                    dtInfo.Rows.Remove(dr);
                }
                Util.GridSetData(dg, dtInfo, FrameOperation, true);

                // 기존 저장자료는 제외
                //if (dg.SelectedIndex > -1)
                //{
                //    DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                //    dt.Rows[dg.SelectedIndex].Delete();
                //    dt.AcceptChanges();
                //    dg.ItemsSource = DataTableConverter.Convert(dt);
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        #endregion





    }
}
