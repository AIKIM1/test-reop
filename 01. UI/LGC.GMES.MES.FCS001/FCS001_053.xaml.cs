/*************************************************************************************
 Created Date : 2020.
      Creator : 
   Decription : OCV 채널별 불량조회
--------------------------------------------------------------------------------------
 [Change History]
  2020.  DEVELOPER : Initial Created.
  2023.01.01 형준우 : ESWA#1 OCV 설비레벨이 C 로 되어있어, 해당 사항 대응으로 동별공통코드를 활용하여 Machine 설비를 가져오도록 수정
  2023.05.09 최도훈 : 인도네시아 조회시 오류나는 현상 수정
  2023.09.01  손동혁 : NA 1동 요청 (L등급 수량 및 비율 ) 컬럼 동별공통코드 사용 NA1동만 보이게 추가


 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
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
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// TSK_120.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS001_053 : UserControl, IWorkArea
    {
        bool bEqpMachineUseFlag = false;
        Util _Util = new Util();

        bool bUseFlag = false; //2023.08.15 컬럼 동별공통코드 사용 NA1동만 보이게 추가 테스트 후 삭제
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FCS001_053()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
            InitControl();

            ///2023.08.15
            bUseFlag = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_062"); //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.
            if (bUseFlag)
            {
                dgList.Columns["L_GRADE"].Visibility = System.Windows.Visibility.Visible;
                dgList.Columns["L_RATE"].Visibility = System.Windows.Visibility.Visible;

            }
            else
            {
                dgList.Columns["L_GRADE"].Visibility = System.Windows.Visibility.Collapsed;
                dgList.Columns["L_RATE"].Visibility = System.Windows.Visibility.Collapsed;

            }
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

        private void InitCombo()
        {
            try
            {
                CommonCombo_Form _combo = new CommonCombo_Form();

                bEqpMachineUseFlag = _Util.IsAreaCommonCodeUse("EQP_LEVEL_TYPE", "MACHINE");

                if (bEqpMachineUseFlag)
                {
                    string[] sFilterEqpM = { "8", null, "C" };
                    _combo.SetCombo(cboEqp, CommonCombo_Form.ComboStatus.NONE, sCase: "EQPID", sFilter: sFilterEqpM);
                }
                else
                {
                    string[] sFilterEqp = { "8", null, "M" };
                    //C1ComboBox[] cboLineChild = { cboModel };
                    _combo.SetCombo(cboEqp, CommonCombo_Form.ComboStatus.NONE, sCase: "EQPID", sFilter: sFilterEqp);
                }

                //C1ComboBox[] cboModelParent = { cboLine };
                _combo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.ALL, sCase: "MODEL");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitControl()
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void GetList(string sExcelCell = null)
        {
            try
            {
                ShowLoadingIndicator();
                Util.gridClear(dgList);
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("FROM_TIME", typeof(string));
                dtRqst.Columns.Add("TO_TIME", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["EQPTID"] = Util.GetCondition(cboEqp);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["FROM_TIME"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + dtpFromTime.DateTime.Value.ToString(" HH:mm:00");
                dr["TO_TIME"] = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd") + dtpToTime.DateTime.Value.ToString(" HH:mm:00");
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_OCV_CH_DEFECT", "RQSTDT", "RSLTDT", dtRqst);

                txttotal.Text = "0";
                if (dtRslt.Rows.Count != 0)
                {
                    AddRow(dgList, dtRslt); //합계 ROW
                }
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }


        private void AddRow(C1.WPF.DataGrid.C1DataGrid datagrid, DataTable dt)
        {
            int start = dt.Columns.IndexOf("E_GRADE");
            int end = dt.Columns.IndexOf("Z_GRADE");

            DataRow row = dt.NewRow();
            row["CSTSLOT"] = ObjectDic.Instance.GetObjectName("Total");

            //Total 수량
            for (int i = start; i <= end; i = i + 2)
            {
                int gradeSum = 0;
                string colName = dt.Columns[i].ColumnName;
                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    gradeSum += Util.NVC_Int(dt.Rows[j][colName].ToString());
                }
                row[colName] = gradeSum;
            }
            int sum = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                sum += Util.NVC_Int(dt.Rows[i]["SUM"].ToString());
            }
            row["SUM"] = sum;
            txttotal.Text = sum.ToString();
            //Total 비율

            start = dt.Columns.IndexOf("E_RATE");
            end = dt.Columns.IndexOf("Z_RATE");

            for (int i = start; i <= end; i = i + 2)
            {
                if (row["SUM"].ToString().Equals("0") || row[i - 1].ToString().Equals("0"))
                    row[i] = 0;
                else
                {
                    double rate = Convert.ToDouble(row[i - 1]) / Convert.ToDouble(row["SUM"]) * 100;
                    row[i] = rate;
                }
            }
            dt.Rows.Add(row);
            Util.GridSetData(datagrid, dt, FrameOperation);
        }

        private void dgList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                if (e.Cell.Row.Index == dgList.Rows.Count - 1)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF7E9D5"));
                }
            }));
        }
    }
}
