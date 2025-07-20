/*************************************************************************************
 Created Date : 2017.07.20
      Creator : 오화백
   Decription : 전지 5MEGA-GMES 구축 - 와싱(초소형)공정진척 화면 - CELL관리 - 중복셀 삭제
--------------------------------------------------------------------------------------
 [Change History]
  2017.07.20  오화백 : 중복셀 제거
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

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ASSY_DUPCELLDELEE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_DUPCELLDELETE : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _eqptid = string.Empty; //설비정보
        private string _lotid = string.Empty;  //생산LOT ID
        private string _outlotid = string.Empty; //완성 LOTID
        private string _cstid = string.Empty; //Tray ID 
        private string _trayTag = string.Empty;              // R: 읽기 / W: 쓰기 모드
        private string _completeProd = string.Empty;          // R: 생산LOT 완료 여부

        Util _Util = new Util();
        BizDataSet _Biz = new BizDataSet();
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

        public CMM_ASSY_DUPCELLDELETE()
        {
            InitializeComponent();
        }
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                DataRow[] Sublotid = tmps[0] as DataRow[];   // SUBLOTID
                           _eqptid = Util.NVC(tmps[1]); //설비정보
                            _lotid = Util.NVC(tmps[2]);   //생산LOT ID
                         _outlotid = Util.NVC(tmps[3]); //완성 LOTID
                            _cstid = Util.NVC(tmps[4]); //Tray ID 
                          _trayTag = Util.NVC(tmps[5]); //읽기, 쓰기 체크 
                     _completeProd = Util.NVC(tmps[6]); //생산LOT 종료 여부

                if (_trayTag == "R") //읽기모드
                {
                    btnDelete.Visibility = Visibility.Collapsed;
                }
                else //쓰기모드
                {
                    btnDelete.Visibility = Visibility.Visible;
                }

                if (Sublotid == null)
                    return;
             
                DataTable SublotBind = new DataTable();
                SublotBind.Columns.Add("CHK", typeof(int));
                SublotBind.Columns.Add("SUBLOTID", typeof(string));
                
                for(int i=0; i< Sublotid.Length; i++)
                {
                    DataRow dr = SublotBind.NewRow();
                    dr["CHK"] = 0;
                    dr["SUBLOTID"] = Sublotid[i]["SUBLOTID"].ToString();
                    SublotBind.Rows.Add(dr);
                }
                Util.gridClear(dgCellList);
                dgCellList.ItemsSource = DataTableConverter.Convert(SublotBind);
            }
            else
            {
                _eqptid = string.Empty;
                _lotid = string.Empty;
                _outlotid = string.Empty;
                _cstid = string.Empty;
                _trayTag = string.Empty;
            }
            ApplyPermissions();
           
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation())
                {
                    return;
                }
                //삭제하시겠습니까?
                Util.MessageConfirm("SFU1230", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        //Cell관리 화면에서 호출
                        DeleteCell();
                    }
                });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
            }
        }
        private void dgCellList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
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
            if (dgCellList.ItemsSource == null) return;

            DataTable dt = ((DataView)dgCellList.ItemsSource).Table;

            dt.Select("CHK = 0").ToList<DataRow>().ForEach(r => r["CHK"] = 1);
            dt.AcceptChanges();

        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgCellList.ItemsSource == null) return;

            DataTable dt = ((DataView)dgCellList.ItemsSource).Table;

            dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
            dt.AcceptChanges();

        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod

        #region [BizCall]

        private void DeleteCell()
        {
            try
            {
                DataSet inDataSet = new DataSet();

                DataTable IndataTable = inDataSet.Tables.Add("INDATA");
                IndataTable.Columns.Add("SRCTYPE", typeof(string));
                IndataTable.Columns.Add("IFMODE", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("PROD_LOTID", typeof(string));
                IndataTable.Columns.Add("OUT_LOTID", typeof(string));
                IndataTable.Columns.Add("USERID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                Indata["IFMODE"] = IFMODE.IFMODE_OFF;
                Indata["EQPTID"] = _eqptid;
                Indata["PROD_LOTID"] = _lotid;
                Indata["OUT_LOTID"] = _outlotid;
                Indata["USERID"] = LoginInfo.USERID;
                IndataTable.Rows.Add(Indata);

                DataTable InAddDataTable = inDataSet.Tables.Add("IN_CST");
                InAddDataTable.Columns.Add("CSTID", typeof(string));
                InAddDataTable.Columns.Add("SUBLOTID", typeof(string));

                for (int _iRow = 0; _iRow < dgCellList.Rows.Count; _iRow++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[_iRow].DataItem, "CHK")).Equals("1"))
                    {
                        DataRow InUserData = InAddDataTable.NewRow();
                        InUserData["CSTID"] = _cstid;
                        InUserData["SUBLOTID"] = Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[_iRow].DataItem, "SUBLOTID"));
                        InAddDataTable.Rows.Add(InUserData);
                    }
                }

                string sBizName = ""; // 삭제

                if ("Y".Equals(_completeProd))
                {
                    sBizName = "BR_PRD_REG_DELETE_SUBLOT_WSS_UI";
                }
                else
                {
                    sBizName = "BR_PRD_REG_DELETE_SUBLOT_WSS";
                }

                if (IndataTable.Rows.Count != 0)
                {
                    new ClientProxy().ExecuteService_Multi(sBizName, "INDATA,IN_INPUT", null, (result, ex) =>
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;

                        if (ex != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                            return;
                        }

                        Util.AlertInfo("SFU1273"); //삭제되었습니다.
                        this.DialogResult = MessageBoxResult.OK;

                    }, inDataSet);
                }
                else
                {
                    Util.Alert("SFU1278");  //처리 할 항목이 없습니다.
                    return;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
            }
        }

        #endregion

        #region [Validation]
        private bool Validation()
        {
            DataRow[] drInfo = Util.gridGetChecked(ref dgCellList, "CHK");

            if (drInfo.Count() <= 0)
            {
                Util.MessageValidation("10008");//선택된 데이터가 없습니다.
                return false;
            }


            return true;
        }
        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btn);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
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

        #endregion

        #endregion

        
    }
}
