/*************************************************************************************
 Created Date : 2019.06.24
      Creator : 이영준
   Decription : 전지 5MEGA-GMES 구축 - 코터 공정진척 화면 - Coating 등록 전수불량 Lane
--------------------------------------------------------------------------------------
 [Change History]
  2019.06.24  이영준 : Initial Created.
  
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

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ELEC_REG_DFCT_LANE_PANCAKE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_REG_DFCT_LANE_PANCAKE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _CUTID = string.Empty;
        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();
        #endregion

        #region Initialize 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_ELEC_REG_DFCT_LANE_PANCAKE()
        {
            InitializeComponent();
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

                if (tmps != null)
                {
                    _CUTID = Util.NVC(tmps[0]);
                }
                ApplyPermissions();
                getDefectLaneLotList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }

        #endregion

        #region Mehod

        #region [BizCall]        
        private void getDefectLaneLotList()
        {
            try
            {
                DataTable _DefectLane = new DataTable();

                Util.gridClear(dgLaneDfct);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("CUT_ID", typeof(string));
                DataRow Indata = IndataTable.NewRow();

                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["CUT_ID"] = _CUTID;

                IndataTable.Rows.Add(Indata);

                _DefectLane = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ELEC_DFCT_LANE_SL", "INDATA", "RSLTDT", IndataTable);

                if (_DefectLane != null && _DefectLane.Rows.Count > 0)
                {
                    Util.GridSetData(dgLaneDfct, _DefectLane, FrameOperation);
                    int LaneCount = 0;
                    int DefectLaneCount = 0;

                    DataRow[] trows = _DefectLane.Select("CHK = 1");
                    LaneCount = Util.NVC_Int(_DefectLane.Rows.Count);
                    DefectLaneCount = Util.NVC_Int(trows.Length);

                    txtDfctLaneQty.Text = Util.NVC(DefectLaneCount);
                    txtPhysicalLaneQty.Text = Util.NVC(LaneCount);

                    txtCoaterResource.Text = Util.NVC(_DefectLane.Rows[0]["CT_EQPTNAME"]);

                    var LanePostion = new List<string>();
                    foreach (DataRow dRow in _DefectLane.Select("LANE_NO IS NOT NULL"))
                    {
                         LanePostion.Add(Util.NVC(dRow["LANE_NO"]));
                    }

                    txtDefectLanePostion.Text = string.Join(",", LanePostion);
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        #endregion

        #region [Validation]
        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);

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
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            getDefectLaneLotList();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string[] sLane = Util.NVC(txtDefectLanePostion.Text).Split(',');

            DataTable dt = (dgLaneDfct.ItemsSource as DataView).Table;
            DataRow[] trows = dt.Select("CHK = 1");

            if (sLane.Length != trows.Length)
            {
                Util.MessageValidation("SFU7026");  // 등록된 Coater Lane 수와 선택된 Lane 수가 다릅니다..
                return;
            }
            saveDefectLane();
        }
        
        private void saveDefectLane()
        {
            try
            {
                //저장하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        DataTable IndataTable = new DataTable();
                        IndataTable.Columns.Add("LOTID", typeof(string));
                        IndataTable.Columns.Add("DFCT_LANE_EXST_FLAG", typeof(string));

                        DataTable dt = DataTableConverter.Convert(dgLaneDfct.ItemsSource);
                        foreach (DataRow inRow in dt.Rows)
                        {
                            DataRow Indata = IndataTable.NewRow();
                            Indata["LOTID"] = Util.NVC(inRow["LOTID"]);
                            Indata["DFCT_LANE_EXST_FLAG"] = Convert.ToBoolean(inRow["CHK"]) == true ? "Y" : "N";

                            IndataTable.Rows.Add(Indata);
                        }
                        if (IndataTable.Rows.Count != 0)
                        {
                            new ClientProxy().ExecuteService("DA_BAS_UPD_WIPATTR_DFCT_LANE_EXST_FLAG", "INDATA", null, IndataTable, (result, ex) =>
                            {
                                if (ex != null)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }
                                Util.AlertInfo("SFU1270");  //저장되었습니다.
                            });
                        }
                        else
                        {
                            Util.Alert("SFU1278");  //처리 할 항목이 없습니다.
                            return;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
            }
        }

        #endregion

        #endregion

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;


            if (DataTableConverter.GetValue(cb.DataContext, "CHK").Equals(1))//체크되는 경우
            {


            }
            else//체크 풀릴때
            {
                int iRow = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).Row.Index;

                if (!Util.NVC(DataTableConverter.GetValue(dgLaneDfct.Rows[iRow].DataItem, "RESNCODE")).Equals(""))
                {
                    //취소하시겠습니까?
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1243"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            DataTableConverter.SetValue(dgLaneDfct.Rows[iRow].DataItem, "CHK", false);
                            DataTableConverter.SetValue(dgLaneDfct.Rows[iRow].DataItem, "RESNCODE", string.Empty);
                            DataTableConverter.SetValue(dgLaneDfct.Rows[iRow].DataItem, "RESNNAME", string.Empty);
                        }
                        else
                            DataTableConverter.SetValue(dgLaneDfct.Rows[iRow].DataItem, "CHK", true);
                    });
                }


            }
        }
    }
}
