/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_021 : UserControl, IWorkArea
    {
        Util _Util = new Util();

        #region Declaration & Constructor 



        public PGM_GUI_021()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            Initialize();
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            //dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateFrom.Text = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

            Initialize_dgAdd();
        }
        #endregion

        private void Initialize_dgAdd()
        {

            DataTable dtRANID_ADD = new DataTable();
            DataRow newRow = null;

            dtRANID_ADD = new DataTable();
            dtRANID_ADD.Columns.Add("NUMBER", typeof(string));
            dtRANID_ADD.Columns.Add("RAN_ID", typeof(string));
            dtRANID_ADD.Columns.Add("DATE", typeof(string));
            dtRANID_ADD.Columns.Add("ELECTRODE", typeof(string));

            List<object[]> list_RAN = new List<object[]>();

            for (int i = 1; i < 51; i++)
            {
                list_RAN.Add(new object[] { i, "", "", "" });
            }
            //list_RAN.Add(new object[] { "", "", "", "" });
            //list_RAN.Add(new object[] { "", "", "", "" });

            foreach (object[] item in list_RAN)
            {
                newRow = dtRANID_ADD.NewRow();
                newRow.ItemArray = item;
                dtRANID_ADD.Rows.Add(newRow);
            }

            dgAdd.ItemsSource = DataTableConverter.Convert(dtRANID_ADD);
        }


        #region Mehod

        #endregion

        #region Button Event
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            int iCnt = 0;

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("RAN_ID", typeof(string));
            RQSTDT.Columns.Add("ELECTRO", typeof(string));
            RQSTDT.Columns.Add("DATE", typeof(string));

            for (int i = 0; i < dgAdd.Rows.Count; i++)
            {
                if (DataTableConverter.GetValue(dgAdd.Rows[i].DataItem, "RAN_ID").ToString() != null)
                {
                    if (DataTableConverter.GetValue(dgAdd.Rows[i].DataItem, "RAN_ID").ToString() == null
                        && DataTableConverter.GetValue(dgAdd.Rows[i].DataItem, "DATE").ToString() != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("날짜는 있는데 RANID가 존재 하지 않습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    else if (DataTableConverter.GetValue(dgAdd.Rows[i].DataItem, "RAN_ID").ToString() == null
                        && DataTableConverter.GetValue(dgAdd.Rows[i].DataItem, "ELECTRODE").ToString() != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("극성은 있는데 RANID가 입력되지 않았습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    else if (DataTableConverter.GetValue(dgAdd.Rows[i].DataItem, "RAN_ID").ToString() != null
                        && DataTableConverter.GetValue(dgAdd.Rows[i].DataItem, "ELECTRODE").ToString() == null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("극성이 입력되지 않았습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    DataTable RQSTDT1 = new DataTable();
                    RQSTDT1.TableName = "RQSTDT";
                    RQSTDT1.Columns.Add("RAN_ID", typeof(string));

                    DataRow dr1 = RQSTDT1.NewRow();
                    dr1["RAN_ID"] = Util.NVC(DataTableConverter.GetValue(dgWait.Rows[i].DataItem, "RAN_ID").ToString());
                    RQSTDT1.Rows.Add(dr1);

                    new ClientProxy().ExecuteService("DA_PRD_SEL_RANID_DUP", "RQSTDT", "RSLTDT", RQSTDT1, (Dupresult, ex) =>
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        if (ex != null)
                        {
                            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        //dgMovePlan.ItemsSource = DataTableConverter.Convert(result);

                    });

                    DataRow dr = RQSTDT.NewRow();
                    dr["RAN_ID"] = Util.NVC(DataTableConverter.GetValue(dgWait.Rows[i].DataItem, "RAN_ID").ToString());
                    dr["ELECTRO"] = Util.NVC(DataTableConverter.GetValue(dgWait.Rows[i].DataItem, "ELECTRODE").ToString());
                    dr["DATE"] = Util.NVC(DataTableConverter.GetValue(dgWait.Rows[i].DataItem, "DATE").ToString());
                    RQSTDT.Rows.Add(dr);

                    new ClientProxy().ExecuteService("DA_PRD_INS_RANID", "RQSTDT", "RSLTDT", RQSTDT, (InsBizresult, ex) =>
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        if (ex != null)
                        {
                            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        //dgMovePlan.ItemsSource = DataTableConverter.Convert(result);

                    });

                    iCnt = i;
                }             
            }

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(iCnt + "개가 정상 처리 되었습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);

        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if(_Util.GetDataGridCheckCnt(dgWait, "CHK") < 0)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("취소할 LOT이 선택되지 않았습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("삭제 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {

                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("RAN_ID", typeof(string));

                    for (int i = 0; i < dgWait.Rows.Count ; i++)
                    {
                        if ((dgWait.GetCell(i, dgWait.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                            (bool)(dgWait.GetCell(i, dgWait.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked)
                        {
                            DataRow dr = RQSTDT.NewRow();
                            //dr["RAN_ID"] = Util.NVC(DataTableConverter.GetValue(dgWait.Rows[i].DataItem, dgWait.Columns[1].Name));                            
                            dr["RAN_ID"] = Util.NVC(DataTableConverter.GetValue(dgWait.Rows[i].DataItem, "RAN_ID").ToString());
                            RQSTDT.Rows.Add(dr);
                        }
                    }

                    new ClientProxy().ExecuteService("DA_PRD_DEL_RANID", "RQSTDT", "RSLTDT", RQSTDT, (Bizresult, ex) =>
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        if (ex != null)
                        {
                            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        //dgMovePlan.ItemsSource = DataTableConverter.Convert(result);

                    });

                    Search_Date();

                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("정상 처리 되었습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            });
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search_Date();
        }
        #endregion

        private void Search_Date()
        {
            string sStartdate = dtpDateFrom.ToString();
            string sEnddate = dtpDateTo.ToString();

            // 미완료 작업
            // 이전 그리드 내용 초기화
            // 날짜 형식 맞추기
            // 조회 Biz 내용 수정

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("STARTDATE", typeof(DateTime));
            RQSTDT.Columns.Add("ENDDATE", typeof(DateTime));

            DataRow dr = RQSTDT.NewRow();
            dr["STARTDATE"] = sStartdate;
            dr["ENDDATE"] = sEnddate;
            RQSTDT.Rows.Add(dr);


            //dgWait List

            new ClientProxy().ExecuteService("DA_PRD_SEL_RANID_WAITLIST", "RQSTDT", "RSLTDT", RQSTDT, (Waitresult, ex) =>
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                if (ex != null)
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
                dgWait.ItemsSource = DataTableConverter.Convert(Waitresult);

            });



            //dgUsed List

            new ClientProxy().ExecuteService("DA_PRD_SEL_RANID_USEDLIST", "RQSTDT", "RSLTDT", RQSTDT, (Usedresult, ex) =>
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                if (ex != null)
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
                dgUsed.ItemsSource = DataTableConverter.Convert(Usedresult);

            });
        }
    }
}
