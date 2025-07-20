/*************************************************************************************
 Created Date : 2017.06.30
      Creator : 오화백K
   Decription : 초소형그룹 관리
--------------------------------------------------------------------------------------
 [Change History]
  2017.06.30  오화백K : Initial Created.
   
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
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
using C1.WPF.DataGrid;
namespace LGC.GMES.MES.COM001
{

    public partial class COM001_088 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();

        public COM001_088()
        {
            InitializeComponent();
            InitCombo();

            this.Loaded += UserControl_Loaded;
        }

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre_S = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        //그룹등록
        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };
        ////등록된 데이터 
        CheckBox chkAll_S = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        #endregion

        #region Initialize
        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {

            //라인셋팅
            const string gubun = "CS";
            String[] sFilter = { LoginInfo.CFG_AREA_ID, gubun, "A4000" };
            //그룹등록
            CommonCombo _combo = new CommonCombo();
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT,  sFilter: sFilter, sCase: "cboEquipmentSegmentAssy");


            //그룹조회
            CommonCombo _combo_S = new CommonCombo();
            _combo_S.SetCombo(cboLine, CommonCombo.ComboStatus.SELECT, sFilter: sFilter, sCase: "cboEquipmentSegmentAssy");



            //그룹등록
            C1.WPF.DataGrid.DataGridComboBoxColumn portTypeColumn = dgList.Columns["EQPTID"] as C1.WPF.DataGrid.DataGridComboBoxColumn;
            ////그룹조회
            //C1.WPF.DataGrid.DataGridComboBoxColumn portTypeColumn_S = dgList_S.Columns["EQPTID"] as C1.WPF.DataGrid.DataGridComboBoxColumn;
            //그룹등록
            if (portTypeColumn != null)
                portTypeColumn.ItemsSource = DataTableConverter.Convert(dtWinderEqp(cboEquipmentSegment.SelectedValue.ToString()));
           
        }
        DataTable dtWinderEqp(string Eqsgid)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));
            RQSTDT.Columns.Add("PROCID", typeof(string));
            RQSTDT.Columns.Add("COATER_EQPT_TYPE_CODE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQSGID"] = Eqsgid;
            dr["PROCID"] = Process.WINDING;
            dr["COATER_EQPT_TYPE_CODE"] = null;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_CBO", "RQSTDT", "RSLTDT", RQSTDT);
            return dtResult;
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            //if (ldpDateFromHist.Text != null)
            //{
            //    ldpDateFromHist.SelectedDateTime = ldpDateToHist.SelectedDateTime.AddDays(-30);
            //}
            //if (ldpDateFrom_S.Text != null)
            //{
            //    ldpDateFrom_S.SelectedDateTime = ldpDateTo_S.SelectedDateTime.AddDays(-30);
            //}
            //dtExpected.SelectedDateTime = DateTime.Now.AddDays(30);

            this.Loaded -= UserControl_Loaded;

        }
      
        #region 그룹등록
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //그룹등록대상 조회
            GetLotList(true);
        }

        private void txtLot_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotList(false);
            }
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dgList.EndEditRow(true);

                if (!Validation())
                {
                    return;
                }
                if (WinderGroupCount() > 0)
                {
                    string _ValueToMessage = string.Empty;
                    _ValueToMessage = MessageDic.Instance.GetMessage("SFU3673", new object[] { WinderGroupCount() }); //동일 그룹으로 등록된 조립 LOT이 {0}개 존재합니다. 그래도 등록하시겠습니까?"
                    Util.MessageConfirm(_ValueToMessage, (sresult) =>
                    {
                        if (sresult == MessageBoxResult.OK)
                        {
                            GroupSave();
                        }
                    });
                }
                else
                {
                    //저장 하시겠습니까?
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                            "SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    GroupSave();
                                }
                            });
                }



            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 그룹삭제
        private void btnSearch_S_Click(object sender, RoutedEventArgs e)
        {
            //그룹이 등록된 데이터 조회
            GetGroup_S(true);
        }

        private void txtLot_S_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetGroup_S(false);
            }
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation_Del())
                {
                    return;
                }
                //삭제 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                GroupModify();
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion





        #region [Group 등록 Check ALL]
        private void dgList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            //그룹조회 탭의 전체 선택 
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre.Content = chkAll;
                        e.Column.HeaderPresenter.Content = pre;
                        chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                    }
                }
            }));

        }

        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if (dgList.ItemsSource == null) return;

            DataTable dt = ((DataView)dgList.ItemsSource).Table;

            dt.Select("CHK = 0").ToList<DataRow>().ForEach(r => r["CHK"] = 1);
            dt.AcceptChanges();

        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgList.ItemsSource == null) return;

            DataTable dt = ((DataView)dgList.ItemsSource).Table;

            dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
            dt.AcceptChanges();

        }

        #endregion

        #region [Group 삭제 Check ALL]
        private void dgList_S_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            //그룹조회 탭의 전체 선택 
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK_S"))
                    {
                        pre_S.Content = chkAll_S;
                        e.Column.HeaderPresenter.Content = pre_S;
                        chkAll_S.Checked -= new RoutedEventHandler(checkAll_S_Checked);
                        chkAll_S.Unchecked -= new RoutedEventHandler(checkAll_S_Unchecked);
                        chkAll_S.Checked += new RoutedEventHandler(checkAll_S_Checked);
                        chkAll_S.Unchecked += new RoutedEventHandler(checkAll_S_Unchecked);
                    }
                }
            }));

        }

        private void checkAll_S_Checked(object sender, RoutedEventArgs e)
        {
            if (dgList_S.ItemsSource == null) return;

            DataTable dt = ((DataView)dgList_S.ItemsSource).Table;

            dt.Select("CHK_S = 0").ToList<DataRow>().ForEach(r => r["CHK_S"] = 1);
            dt.AcceptChanges();

        }
        private void checkAll_S_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgList_S.ItemsSource == null) return;

            DataTable dt = ((DataView)dgList_S.ItemsSource).Table;

            dt.Select("CHK_S = 1").ToList<DataRow>().ForEach(r => r["CHK_S"] = 0);
            dt.AcceptChanges();

        }

        #endregion

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //그룹등록 라인콤보 이벤트
            C1.WPF.DataGrid.DataGridComboBoxColumn portTypeColumn = dgList.Columns["EQPTID"] as C1.WPF.DataGrid.DataGridComboBoxColumn;
            if (portTypeColumn != null)
                portTypeColumn.ItemsSource = DataTableConverter.Convert(dtWinderEqp(cboEquipmentSegment.SelectedValue.ToString()));
        }

        private void cboLine_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //그룹등록 라인콤보 이벤트
            C1.WPF.DataGrid.DataGridComboBoxColumn portTypeColumn = dgList_S.Columns["EQPTID"] as C1.WPF.DataGrid.DataGridComboBoxColumn;
            if (portTypeColumn != null)
                portTypeColumn.ItemsSource = DataTableConverter.Convert(dtWinderEqp(cboLine.SelectedValue.ToString()));
        }

        #endregion

        #region Mehod

        #region [작업대상 가져오기]
        public void GetLotList(bool bButton)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                if (Util.GetCondition(txtLot).Equals("")) //lot id 가 없는 경우
                {
                    dr["FROM_DATE"] = Util.GetCondition(ldpDateFromHist);
                    dr["TO_DATE"] = Util.GetCondition(ldpDateToHist);
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, "SFU1223"); // 라인을선택하세요. >> 라인을 선택 하세요.
                    if (dr["EQSGID"].Equals("")) return;

                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    dr["LOTID"] = Util.GetCondition(txtLot);
                }

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WINDER_GROUP", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0 && bButton == true )
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    Util.gridClear(dgList);
                    return;
                }


                chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                chkAll.IsChecked = false;
                chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);

                if (dgList.GetRowCount() > 0 && bButton == false)
                {
                    DataTable dtSource = DataTableConverter.Convert(dgList.ItemsSource);
                    dtSource.Merge(dtRslt);

                    Util.gridClear(dgList);
                    dgList.ItemsSource = DataTableConverter.Convert(dtSource);
                    dgList.Columns[8].Width = new C1.WPF.DataGrid.DataGridLength(280);
                    //dgList.Columns[9].Width = new C1.WPF.DataGrid.DataGridLength(250);
                }
                else
                {
                    //Util.GridSetData(dgList, dtRslt, FrameOperation, true);
                    dgList.ItemsSource = DataTableConverter.Convert(dtRslt);
                    dgList.Columns[8].Width = new C1.WPF.DataGrid.DataGridLength(280);
                    //dgList.Columns[9].Width = new C1.WPF.DataGrid.DataGridLength(150);
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region [그룹삭제 조회]
        public void GetGroup_S(bool bButton)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                if (Util.GetCondition(txtLot_S).Equals("")) //lot id 가 없는 경우
                {
                    dr["FROM_DATE"] = Util.GetCondition(ldpDateFrom_S);
                    dr["TO_DATE"] = Util.GetCondition(ldpDateTo_S);
                    dr["EQSGID"] = Util.GetCondition(cboLine, "SFU1223"); // 라인을선택하세요. >> 라인을 선택 하세요.
                    if (dr["EQSGID"].Equals("")) return;

                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    dr["LOTID"] = Util.GetCondition(txtLot_S);
                }

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WINDER_GROUP_FOR_LIST", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0)
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    Util.gridClear(dgList_S);
                    return;
                }


                chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                chkAll.IsChecked = false;
                chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);

                if (dgList_S.GetRowCount() > 0 && bButton == false)
                {
                    DataTable dtSource = DataTableConverter.Convert(dgList_S.ItemsSource);
                    dtSource.Merge(dtRslt);

                    Util.gridClear(dgList_S);
                    dgList_S.ItemsSource = DataTableConverter.Convert(dtSource);
                    dgList_S.Columns[8].Width = new C1.WPF.DataGrid.DataGridLength(280);
                    //dgList.Columns[9].Width = new C1.WPF.DataGrid.DataGridLength(250);
                }
                else
                {
                    //Util.GridSetData(dgList, dtRslt, FrameOperation, true);
                    dgList_S.ItemsSource = DataTableConverter.Convert(dtRslt);
                    dgList_S.Columns[8].Width = new C1.WPF.DataGrid.DataGridLength(280);
                    //dgList.Columns[9].Width = new C1.WPF.DataGrid.DataGridLength(150);
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        #endregion

        #region [Group 정보 저장]
        private void GroupSave()
        {

            DataSet inData = new DataSet();

            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("WND_EQPTID", typeof(string));
            inDataTable.Columns.Add("WND_GR_CODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            DataRow row = null;
            for (int i = 0; i < dgList.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "CHK")).Equals("1"))
                {
                    row = inDataTable.NewRow();
                    row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "LOTID"));
                    row["WND_EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "EQPTID"));
                    row["WND_GR_CODE"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "GROUP_S"));
                    row["USERID"] = LoginInfo.USERID; ;
                    inDataTable.Rows.Add(row);
               }
            }
        
            try
            {
                //그룹저장 처리
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_WINDER_GROUP", "INDATA", null, inData);
                Util.MessageInfo("SFU1270");   //저장되었습니다
                Util.gridClear(dgList);
                GetLotList(false);
                chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                chkAll.IsChecked = false;
                chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
             }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_WINDER_GROUP", ex.Message, ex.ToString());

            }
        }
        #endregion

        #region [Group 정보 삭제]
        private void GroupModify()
        {

            DataSet inData = new DataSet();

            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("WND_EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            DataRow row = null;
            for (int i = 0; i < dgList_S.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgList_S.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgList_S.Rows[i].DataItem, "CHK")).Equals("1"))
                {
                    row = inDataTable.NewRow();
                    row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgList_S.Rows[i].DataItem, "LOTID"));
                    row["WND_EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgList_S.Rows[i].DataItem, "EQPTID"));
                    row["USERID"] = LoginInfo.USERID; ;
                    inDataTable.Rows.Add(row);
                }
            }

            try
            {
                //그룹저장 처리
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_WINDER_GROUP", "INDATA", null, inData);
                Util.MessageInfo("SFU1273");   //삭제되었습니다
                Util.gridClear(dgList_S);
                GetGroup_S(false);
                chkAll_S.Unchecked -= new RoutedEventHandler(checkAll_S_Unchecked);
                chkAll_S.IsChecked = false;
                chkAll_S.Unchecked += new RoutedEventHandler(checkAll_S_Unchecked);
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_WINDER_GROUP", ex.Message, ex.ToString());

            }
        }
        #endregion

        #region [VALIDATION]

        private bool Validation()
        {

            if(dgList.Rows.Count == 0)
            {

                Util.MessageValidation("SFU1905");//조회된 데이터가 없습니다.
                return false;
            }

            DataRow[] drInfo = Util.gridGetChecked(ref dgList, "CHK");

            if (drInfo.Count() <= 0)
            {
                Util.MessageValidation("10008");//선택된 데이터가 없습니다.
                return false;
            }

            for (int i = 0; i < dgList.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "CHK")).Equals("1"))
                {
                    if(Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "EQPTID")) == string.Empty)
                    {
                      
                        Util.MessageValidation("PSS9147");//선택된 데이터가 없습니다.
                    
                        return false;
                    }
                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "GROUP_S")) == string.Empty)
                    {
                          Util.MessageValidation("SFU3687");//그룹정보를 입력하세요.
                      
                        return false;
                    }

                    if (!CheckNumber(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "GROUP_S").ToString()))
                    {
                        Util.MessageValidation("그룹 ID는 숫자만 입력가능합니다."); 

                        return false;

                    }

                    if (DataTableConverter.GetValue(dgList.Rows[i].DataItem, "GROUP_S").ToString().Trim().Length != 5)
                    {
                        Util.MessageValidation("그룹 ID는 5자리입니다.");

                        return false;
                    }


                }
            }

            return true;
        }


        private bool Validation_Del()
        {

            if (dgList_S.Rows.Count == 0)
            {

                Util.MessageValidation("SFU1905");//조회된 데이터가 없습니다.
                return false;
            }

            DataRow[] drInfo = Util.gridGetChecked(ref dgList_S, "CHK");

            if (drInfo.Count() <= 0)
            {
                Util.MessageValidation("10008");//선택된 데이터가 없습니다.
                return false;
            }
           
            return true;
        }

        private int WinderGroupCount()
        {
            int cnt = 0;
            DataSet inData = new DataSet();

            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("GROUP", typeof(string));
            DataRow row = null;
            for (int i = 0; i < dgList.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "CHK")).Equals("1"))
                {
                    row = inDataTable.NewRow();
                    row["GROUP"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "GROUP_S"));
                    inDataTable.Rows.Add(row);
                }
            }
            DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("DA_PRD_SEL_WINDER_GROUP_COUNT", "INDATA", "OUTDATA", inData);
            //그룹저장 처리
            if (dsRslt.Tables["OUTDATA"].Rows.Count != 0)
            {
                //cnt = dsRslt.Tables[0].Rows[0]["GR_CNT"].ToString() == string.Empty? 0 : Convert.ToInt32(dsRslt.Tables[0].Rows[0]["GR_CNT"]);
                cnt = dsRslt.Tables["OUTDATA"].Rows[0]["GR_CNT"].ToString() == string.Empty ? 0 : Convert.ToInt32(dsRslt.Tables["OUTDATA"].Rows[0]["GR_CNT"]);
            }            

            return cnt;
        }

        #endregion

        #endregion

        private void dgList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name.Equals("GROUP_S"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                    }
                }
            }));
        }

     

        public static bool CheckNumber(string letter)

        {
            Int32 numchk = 0;
            bool IsCheck = int.TryParse(letter, out numchk);
            return IsCheck;
        }

              
        private void dgList_LostFocus(object sender, RoutedEventArgs e)
        {
            //IInputElement input = Keyboard.FocusedElement;

            //if (input != sender)
            //{
            //    var parent = VisualTreeHelper.GetParent(input as FrameworkElement) as FrameworkElement;

            //    while (parent != null)
            //    {
            //        if (parent == sender)
            //        {
            //            break;
            //        }

            //        parent = VisualTreeHelper.GetParent(parent) as FrameworkElement;
            //    }

            //    if (parent != null && parent is C1.WPF.DataGrid.C1DataGrid)
            //    {

            //    }
            //    else
            //    {
            //        C1.WPF.DataGrid.C1DataGrid datagrid = sender as C1.WPF.DataGrid.C1DataGrid;
            //        datagrid.EndEdit();
            //        datagrid.EndEditRow(true);
            //    }
            //}
        }

        private void dgList_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            //try
            //{
            //    if (e.Cell.Column.Name.Equals("GROUP_S"))
            //    {
            //        if (DataTableConverter.GetValue(dgList.Rows[e.Cell.Row.Index].DataItem, "GROUP_S").ToString() != string.Empty)
            //        {
            //            if (!CheckNumber(DataTableConverter.GetValue(dgList.Rows[e.Cell.Row.Index].DataItem, "GROUP_S").ToString()))
            //            {

            //                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("그룹 ID는 숫자만 입력가능합니다."), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
            //                {
            //                    if (result == MessageBoxResult.OK)
            //                    {
            //                        DataTableConverter.SetValue(dgList.Rows[e.Cell.Row.Index].DataItem, "GROUP_S", string.Empty);
            //                    }
            //                });
            //                return;

            //            }

            //            if (DataTableConverter.GetValue(dgList.Rows[e.Cell.Row.Index].DataItem, "GROUP_S").ToString().Trim().Length != 5)
            //            {
            //                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("그룹 ID는 5자리입니다."), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
            //                {
            //                    if (result == MessageBoxResult.OK)
            //                    {
            //                        DataTableConverter.SetValue(dgList.Rows[e.Cell.Row.Index].DataItem, "GROUP_S", string.Empty);
            //                    }
            //                });
            //                return;
            //            }
            //        }
            //    }
           
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //    return;
            //}
        }
    }
}
