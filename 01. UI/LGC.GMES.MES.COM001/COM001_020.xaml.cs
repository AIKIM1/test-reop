/*************************************************************************************
 Created Date : 2016.06.16
      Creator : Jeong Hyeon Sik
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_020 : UserControl, IWorkArea
    {
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #region Declaration & Constructor 
        public COM001_020()
        {
            InitializeComponent();
        }


        #endregion

        #region Initialize

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                setComboBox();

                tbErrListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";                

                this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Check_Select())
                {
                    getErrList();
                    InitColumnSize(); // 2024.12.10. 김영국 - 스크롤시 컬럼 사이즈 변경으로 인해 로직 반영.

                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new ExcelExporter().Export(dgEquipErrorList);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        #endregion

        #region Mehod
        private void setComboBox()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();

                string[] sFilter = { LoginInfo.CFG_AREA_ID };

                C1ComboBox[] cboEquipmentSegmentChild = { cboProcess , cboEquipment };
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, sFilter: sFilter, sCase: "EQUIPMENTSEGMENT");

                //공정
                C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
                C1ComboBox[] cboProcessChild = { cboEquipment };

                string strProcessCase = string.Empty;

                if (LoginInfo.CFG_AREA_ID.StartsWith("P"))
                {
                    strProcessCase = "cboProcessPack";
                }
                else
                {
                    strProcessCase = "PROCESS";
                }

                _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbParent: cboProcessParent, cbChild: cboProcessChild, sCase: strProcessCase);

                //설비
                C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
                _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent, sCase: "EQUIPMENT_BY_EQSGID_PROCID");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void getErrList()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("DATE_FROM", typeof(string));
                RQSTDT.Columns.Add("DATE_TO", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["DATE_FROM"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["DATE_TO"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                dr["PROCID"] = Util.NVC(cboProcess.SelectedValue) == "" ? null : Util.NVC(cboProcess.SelectedValue);
                dr["EQPTID"] = Util.NVC(cboEquipment.SelectedValue) == "" ? null : Util.NVC(cboEquipment.SelectedValue);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EIFLOG", "RQSTDT", "RSLTDT", RQSTDT);

                dgEquipErrorList.ItemsSource = DataTableConverter.Convert(dtResult);

                Util.SetTextBlockText_DataGridRowCount(tbErrListCount, Util.NVC(dgEquipErrorList.Rows.Count));
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private bool Check_Select()
        {
            bool bReturn = true;
            try
            {
                //라인선택확인
                if (Util.NVC(cboEquipmentSegment.SelectedValue) == "SELECT")
                {
                    Util.Alert("SFU1223");  //라인을 선택하세요.
                    bReturn = false;
                    cboEquipmentSegment.Focus();
                    return bReturn;
                }
                //라인선택확인
                if (Util.NVC(cboProcess.SelectedValue) == "SELECT")
                {
                    Util.Alert("SFU1459");  //공정을 선택하세요.
                    bReturn = false;
                    cboProcess.Focus();
                    return bReturn;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return bReturn;
        }

        private void InitColumnSize()
        {
            dgEquipErrorList.Columns["INSDTTM"].Width = new C1.WPF.DataGrid.DataGridLength(200);
            dgEquipErrorList.Columns["EQSGID"].Width = new C1.WPF.DataGrid.DataGridLength(150);
            dgEquipErrorList.Columns["EQSGNAME"].Width = new C1.WPF.DataGrid.DataGridLength(150);
            dgEquipErrorList.Columns["PROCID"].Width = new C1.WPF.DataGrid.DataGridLength(150);
            dgEquipErrorList.Columns["EQPTID"].Width = new C1.WPF.DataGrid.DataGridLength(150);
            dgEquipErrorList.Columns["EQPTNAME"].Width = new C1.WPF.DataGrid.DataGridLength(250);
        }
        #endregion



    }
}
