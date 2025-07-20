/*************************************************************************************
 Created Date : 2019.12.19
      Creator : INS 김동일K
   Decription : Notched Pancake QA Sample Monitoring 화면
--------------------------------------------------------------------------------------
 [Change History]
  2019.12.19  INS 김동일K : Initial Created.
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using System.Data;
using C1.WPF;

namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY004_010.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_010 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        private bool bSetAutoSelTime = false;

        List<ASSY004_010_GRP> list;

        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        #endregion

        #region Initialize        
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public ASSY004_010()
        {
            InitializeComponent();
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, null, cboProcessParent);

            // 자동 조회 시간 Combo
            String[] sFilter3 = { "SECOND_INTERVAL" };
            _combo.SetCombo(cboAutoSearchOut, CommonCombo.ComboStatus.NA, sFilter: sFilter3, sCase: "COMMCODE");

            if (cboAutoSearchOut != null && cboAutoSearchOut.Items != null && cboAutoSearchOut.Items.Count > 0)
                cboAutoSearchOut.SelectedIndex = 0;

        }
        #endregion

        #region Event

        #region [Main Window]
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            InitCombo();
            
            if (dispatcherTimer != null)
            {
                int iSec = 0;

                if (cboAutoSearchOut != null && cboAutoSearchOut.SelectedValue != null && !cboAutoSearchOut.SelectedValue.ToString().Equals(""))
                    iSec = int.Parse(cboAutoSearchOut.SelectedValue.ToString());

                dispatcherTimer.Tick += dispatcherTimer_Tick;
                dispatcherTimer.Interval = new TimeSpan(0, 0, iSec);
                //dispatcherTimer.Start();
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= UserControl_Loaded;
            ApplyPermissions();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void cboAutoSearchOut_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (dispatcherTimer != null)
                {
                    dispatcherTimer.Stop();

                    int iSec = 0;

                    if (cboAutoSearchOut != null && cboAutoSearchOut.SelectedValue != null && !cboAutoSearchOut.SelectedValue.ToString().Equals(""))
                        iSec = int.Parse(cboAutoSearchOut.SelectedValue.ToString());

                    if (iSec == 0 && bSetAutoSelTime)
                    {
                        dispatcherTimer.Interval = new TimeSpan(0, 0, iSec);
                        //Util.MessageValidation("");
                        return;
                    }

                    if (iSec == 0)
                    {
                        bSetAutoSelTime = true;
                        return;
                    }

                    dispatcherTimer.Interval = new TimeSpan(0, 0, iSec);
                    dispatcherTimer.Start();

                    if (bSetAutoSelTime)
                    {
                        //Util.MessageValidation("", cboAutoSearchOut.SelectedValue.ToString());
                    }

                    bSetAutoSelTime = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            System.Windows.Threading.DispatcherTimer dpcTmr = sender as System.Windows.Threading.DispatcherTimer;

            dpcTmr.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    //FrameOperation.PrintFrameMessage(DateTime.Now.ToLongTimeString() + ">>" + dpcTmr.Interval.TotalSeconds.ToString());

                    if (dpcTmr != null)
                        dpcTmr.Stop();

                    if (dpcTmr.Interval.TotalSeconds == 0)
                        return;
                    
                    this.Dispatcher.BeginInvoke(new Action(() => GetList(true)));
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    if (dpcTmr != null && dpcTmr.Interval.TotalSeconds > 0)
                        dpcTmr.Start();
                }
            }));

        }
        #endregion

        #endregion

        #region Mehod

        #region [BizCall]
        private void GetList(bool bAutoSearch = false)
        {
            try
            {
                if (!bAutoSearch && Util.NVC(cboArea.Text).IndexOf("SELECT") >= 0)
                {
                    Util.MessageValidation("SFU1499"); // 동을 선택하세요.
                    return;
                }

                ClearGroupLayout();

                DataTable dt = new DataTable();
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("COM_TYPE_CODE", typeof(string));

                DataRow dr = dt.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "QA_SMPL_MNT_GRP";

                dt.Rows.Add(dr);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_SRT", "RQSTDT", "RSLTDT", dt);

                if (result != null)
                {
                    if (list == null)
                        list = new List<ASSY004_010_GRP>();

                    int cnt = list.Count;
                    if (list.Count < result.Rows.Count)
                        for (int i = 0; i < result.Rows.Count - cnt; i++)
                            list.Add(new ASSY004_010_GRP(this, FrameOperation));

                    var rowDef = new RowDefinition { Height = new GridLength(1, GridUnitType.Star) };
                    grdGroup.RowDefinitions.Add(rowDef);

                    for (int i = 0; i < result.Rows.Count; i++)
                    {
                        list[i].ClearData();

                        list[i].GRP_NAME = Util.NVC(result.Rows[i]["COM_CODE_NAME"]);
                        list[i].GRP_CODE = Util.NVC(result.Rows[i]["COM_CODE"]);
                        list[i].AREAID = Util.NVC(cboArea.SelectedValue);
                        list[i].EQSGID = Util.NVC(cboEquipmentSegment.SelectedValue);
                        list[i].PROCID = Util.NVC(cboProcess.SelectedValue);
                        list[i].GRPATTR1 = Util.NVC(result.Rows[i]["ATTR1"]);
                        list[i].GRPATTR2 = Util.NVC(result.Rows[i]["ATTR2"]);
                        list[i].GRPATTR3 = Util.NVC(result.Rows[i]["ATTR3"]);
                        list[i].GRPATTR4 = Util.NVC(result.Rows[i]["ATTR4"]);
                        list[i].GRPATTR5 = Util.NVC(result.Rows[i]["ATTR5"]);

                        #region Layout 설정
                        var colDef = new ColumnDefinition
                        {
                            MinWidth = 400,
                            Width = new GridLength(1, GridUnitType.Star)
                        };
                        grdGroup.ColumnDefinitions.Add(colDef);

                        var grid = new Grid
                        {
                            Name = "gr0" + i,
                            Margin = new Thickness(i == 0 ? 0 : 8, 0, 0, 2)
                        };
                        grid.SetValue(Grid.RowProperty, 0);
                        grid.SetValue(Grid.ColumnProperty, i);
                        grid.Children.Add(list[i]);

                        grdGroup.Children.Add(grid);
                        #endregion

                        list[i].GetList();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Validation]

        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        
        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        public void HideLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }
        
        private void ClearGroupLayout()
        {
            grdGroup.ColumnDefinitions.Clear();
            grdGroup.RowDefinitions.Clear();

            foreach (var child in grdGroup.Children)
                if (child.GetType() == typeof(Grid))
                    (child as Grid).Children.Clear();

            grdGroup.Children.Clear();
        }

        #endregion

        #endregion
    }
}
