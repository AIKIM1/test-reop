using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY_004_050_SEL.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_050_BOARD : UserControl, IWorkArea
    {

        #region [Initialize]
        UserControl _UCParent = null;

        public ASSY004_050_BOARD(UserControl parent)
        {
            InitializeComponent();
            LoginInfo.CFG_PROC_ID = Process.RWK_LNS;
            _UCParent = parent;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region [Event]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            txtArea.Text = LoginInfo.CFG_AREA_NAME;
            //월초로 세팅
            dateFrom.SelectedDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            //MonoCell 단위로 세팅
            rbMono.IsChecked = true;
            rbMono.Checked += rbType_Checked;
            rbStacked.Checked += rbType_Checked;
            //ComboBox 세팅
            InitCombo();
        }
        private void cboProject_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            btnSearch_Click(null, null);
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetDailyRecords();
        }

        private void rbType_Checked(object sender, RoutedEventArgs e)
        {
            GetDailyRecords();
        }
        #endregion

        #region [Method]
        private void GetDailyRecords()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inData = new DataTable();
                inData.Columns.Add("AREAID", typeof(string));
                inData.Columns.Add("EQSGID", typeof(string));
                inData.Columns.Add("PRJT_NAME", typeof(string));
                inData.Columns.Add("DATE_FROM", typeof(DateTime));
                inData.Columns.Add("DATE_TO", typeof(DateTime));
                inData.Columns.Add("UNIT", typeof(string));

                DataRow dr = inData.NewRow();
                //dr["AREAID"] = Util.NVC(LoginInfo.CFG_AREA_ID);
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue as string;
                // dr["PRJT_NAME"] = string.IsNullOrEmpty(Util.NVC(cboProject.SelectedValue)) == true ? null : Util.NVC(cboProject.SelectedValue);
                dr["PRJT_NAME"] = cboProject.SelectedIndex == 0 ? null : Util.NVC(cboProject.SelectedValue);
                dr["DATE_FROM"] = Util.NVC(dateFrom.SelectedDateTime);
                dr["DATE_TO"] = Util.NVC(dateTo.SelectedDateTime);
                if (rbMono.IsChecked.HasValue && rbMono.IsChecked.Value)
                {
                    dr["UNIT"] = "M";
                }
                else if (rbStacked.IsChecked.HasValue && rbStacked.IsChecked.Value)
                {
                    dr["UNIT"] = "S";
                }
                inData.Rows.Add(dr);

                new ClientProxy().ExecuteService("BR_PRD_SEL_RWK_RECORDS","INDATA","OUTDATA",inData, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                            throw bizException;

                        Util.gridClear(dgRecords);
                        Util.GridSetData(dgRecords, bizResult, FrameOperation);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });
            }
            catch(Exception e)
            {
                Util.MessageException(e);
                HideLoadingIndicator();
            }
        }
        #endregion

        #region [Util & Init Method]
        private void InitCombo()
        {
            //cboProject
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID, Process.RWK_LNS };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProject };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, sFilter: sFilter, sCase: "PROCESSEQUIPMENTSEGMENT");

            String[] sFilter2 = { };
            C1ComboBox[] cboProjectParent = { cboEquipmentSegment };
            _combo.SetCombo(cboProject, CommonCombo.ComboStatus.ALL, cbParent: cboProjectParent, sFilter: sFilter, sCase: "RWK_PRJT_CBO_L");

            if (cboEquipmentSegment.Items.Count >= 2)
                cboEquipmentSegment.SelectedIndex = 1;
        }

        private void ShowLoadingIndicator()
        {
            if(loadingIndicator.Visibility != Visibility.Visible)
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HideLoadingIndicator()
        {
            if (loadingIndicator.Visibility != Visibility.Collapsed)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion


    }
}
