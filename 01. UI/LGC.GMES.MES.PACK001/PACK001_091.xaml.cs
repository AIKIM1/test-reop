/*************************************************************************************
 Created Date : 
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]

 
**************************************************************************************/


using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.PACK001.Class;
using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.PACK001
{
    /// <summary>
    /// Window1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PACK001_091 : UserControl, IWorkArea
    {
        #region [ Initialize ] 

        #region [ Variable ]
        private PackComboSetDataHelper dataHelper = new PackComboSetDataHelper();
        private PackDataGridSetDataHelper dgHelper = new PackDataGridSetDataHelper();
        private PackInterLockHelper dtInterLock = new PackInterLockHelper();

        #endregion

        public enum ComboStatus
        {
            ALL,
            SELECT,
            NA,
            NONE
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_091()
        {
            InitializeComponent();
        }

        #endregion


        #region [ xaml Event ]

        #region [ Button Event ]
        private void btnProcessSelect_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                string MAINFORMPATH = "LGC.GMES.MES.PACK001";
                string MAINFORMNAME = "PACK001_091_SELECTPROCESS";

                Assembly asm = Assembly.LoadFrom("ClientBin\\" + MAINFORMPATH + ".dll");
                Type targetType = asm.GetType(MAINFORMPATH + "." + MAINFORMNAME);
                object obj = Activator.CreateInstance(targetType);

                IWorkArea workArea = obj as IWorkArea;
                workArea.FrameOperation = FrameOperation;

                C1Window popup = obj as C1Window;

                if (popup != null)
                {
                    DataTable dtData = new DataTable();
                    dtData.Columns.Add("LABELPRINTUSE", typeof(string));
                    dtData.Columns.Add("PROCESSUSE", typeof(string));

                    DataRow newRow = null;

                    newRow = dtData.NewRow();
                    newRow["LABELPRINTUSE"] = "N";
                    newRow["PROCESSUSE"] = "Y";
                    dtData.Rows.Add(newRow);

                    object[] Parameters = new object[1];
                    Parameters[0] = dtData;
                    C1WindowExtension.SetParameters(popup, Parameters);

                    popup.Closed -= popup_Closed;
                    popup.Closed += popup_Closed;
                    popup.ShowModal();
                    popup.CenterOnScreen();
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void btnExpandFrameTop_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (btnExpandFrameTop.IsChecked == false) return;

                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(1, GridUnitType.Star);
                gla.To = new GridLength(0, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 1, 0);
                mainGrid.ColumnDefinitions[3].BeginAnimation(ColumnDefinition.WidthProperty, gla);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnExpandFrameTop_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (btnExpandFrameTop.IsChecked == true) return;

                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(0, GridUnitType.Star);
                gla.To = new GridLength(1, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 1, 0);
                mainGrid.ColumnDefinitions[3].BeginAnimation(ColumnDefinition.WidthProperty, gla);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnEndExpandFrameTop_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (btnEndTabExpandFrameTop.IsChecked == false) return;

                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(1, GridUnitType.Star);
                gla.To = new GridLength(0, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 1, 0);
                EndTab.ColumnDefinitions[3].BeginAnimation(ColumnDefinition.WidthProperty, gla);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnEndExpandFrameTop_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (btnEndTabExpandFrameTop.IsChecked == true) return;

                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(0, GridUnitType.Star);
                gla.To = new GridLength(1, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 1, 0);
                EndTab.ColumnDefinitions[3].BeginAnimation(ColumnDefinition.WidthProperty, gla);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region
        void popup_Closed(object sender, EventArgs e)
        {
            try
            {
                PACK001_091_SELECTPROCESS popup = sender as PACK001_091_SELECTPROCESS;
                if (popup.DialogResult == MessageBoxResult.OK)
                {

                    DataRow drSelectedProcess = popup.DrSelectedProcessInfo;

                    if (drSelectedProcess != null)
                    {

                        tbTitle.Text = popup.SSelectedProcessTitle;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {

                SetComboBox();
                SetDataGrid();
                //  LeftGridWidth = Convert.ToDouble(mainGrid.ColumnDefinitions[3].Width);
                //cboJudge.ItemsSource = dtJubgeCombo.Copy().AsDataView();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region [ 주석 ] 
        private Boolean makeJudgeCombo()
        {
            try
            {
                DataTable dtCombo = new DataTable();

                dtCombo.Columns.Add("CBO_NAME", typeof(string));
                dtCombo.Columns.Add("CBO_CODE", typeof(string));

                DataRow newRow = dtCombo.NewRow();
                newRow["CBO_NAME"] = ObjectDic.Instance.GetObjectName("합격");
                newRow["CBO_CODE"] = "Y";

                dtCombo.Rows.Add(newRow);

                newRow = dtCombo.NewRow();
                newRow["CBO_NAME"] = ObjectDic.Instance.GetObjectName("불합격");
                newRow["CBO_CODE"] = "N";
                dtCombo.Rows.Add(newRow);

                cboJudge.ItemsSource = dtCombo.Copy().AsDataView();

                cboJudge.SelectedIndex = 0;

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }

        }
        #endregion

        #region [ Class ]

        #region [ Combo Class ] 
        public class PackComboSetDataHelper
        {
            #region Member Variable Lists...
            private DataTable dtJubgeCombo = new DataTable();                    // 판정 관련 콤보
            private DataTable dtPackEquipmentInfo = new DataTable();             // 포장기 관련 콤보
            private DataTable dtEquipmentInfo = new DataTable();             // 착공, 완공 설비 관련 콤보
            #endregion

            #region [ Constructor ]
            public PackComboSetDataHelper()
            {
                this.makeJudgeCombo(ref this.dtJubgeCombo, "JUDGE");
                this.GetPackEquipmentInfo(ref this.dtPackEquipmentInfo);
                this.GetEquipmentInfo(ref this.dtEquipmentInfo);
            }
            #endregion

            #region [ 판정 경과 콤보 만들기 ] 
            private void makeJudgeCombo(ref DataTable dtReturn, string strCmcdType)
            {
                try
                {
                    DataTable dtCombo = new DataTable();

                    dtCombo.Columns.Add("CBO_NAME", typeof(string));
                    dtCombo.Columns.Add("CBO_CODE", typeof(string));

                    DataRow newRow = dtCombo.NewRow();
                    newRow["CBO_NAME"] = ObjectDic.Instance.GetObjectName("양품");
                    newRow["CBO_CODE"] = "Y";

                    dtCombo.Rows.Add(newRow);

                    newRow = dtCombo.NewRow();
                    newRow["CBO_NAME"] = ObjectDic.Instance.GetObjectName("불량");
                    newRow["CBO_CODE"] = "N";
                    dtCombo.Rows.Add(newRow);

                    if (CommonVerify.HasTableRow(dtCombo))
                    {
                        dtReturn = dtCombo.Copy();
                    }

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
            #endregion

            #region [ 착공, 완공 설비 콤보 만들기 ] 
            private void GetEquipmentInfo(ref DataTable dtEquipmentInfo)
            {
                try
                {
                    string bizRuleName = "DA_BAS_SEL_EQUIPMENT_BY_EQSGID_PROCID_CBO";
                    DataTable dtRQSTDT = new DataTable("RQSTDT");
                    DataTable dtRSLTDT = new DataTable("RSLTDT");

                    dtRQSTDT.Columns.Add("LANGID", typeof(string));
                    dtRQSTDT.Columns.Add("AREAID", typeof(string));
                    dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                    dtRQSTDT.Columns.Add("PROCID", typeof(string));
                    

                    DataRow drRQSTDT = dtRQSTDT.NewRow();
                    drRQSTDT["LANGID"] = LoginInfo.LANGID;
                    drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                    drRQSTDT["EQSGID"] = "P8Q22";
                    drRQSTDT["PROCID"] = "P5250";
                    dtRQSTDT.Rows.Add(drRQSTDT);

                    dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                    if (CommonVerify.HasTableRow(dtRSLTDT))
                    {
                        dtEquipmentInfo = dtRSLTDT.Copy();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
            #endregion


            #region [ 포장기 콤보 만들기 ] 
            private void GetPackEquipmentInfo(ref DataTable dtReturn)
            {
                try
                {
                    string bizRuleName = "DA_BAS_SEL_LOGIS_PACK_EQPT_CBO";
                    DataTable dtRQSTDT = new DataTable("RQSTDT");
                    DataTable dtRSLTDT = new DataTable("RSLTDT");

                    dtRQSTDT.Columns.Add("LANGID", typeof(string));
                    dtRQSTDT.Columns.Add("AREAID", typeof(string));
                    dtRQSTDT.Columns.Add("PACK_CELL_AUTO_LOGIS_FLAG", typeof(string));
                    dtRQSTDT.Columns.Add("PACK_MEB_LINE_FLAG", typeof(string));
                    dtRQSTDT.Columns.Add("PACK_BOX_LINE_FLAG", typeof(string));

                    DataRow drRQSTDT = dtRQSTDT.NewRow();
                    drRQSTDT["LANGID"] = LoginInfo.LANGID;
                    drRQSTDT["AREAID"] = "P8"; // LoginInfo.CFG_AREA_ID;
                    drRQSTDT["PACK_CELL_AUTO_LOGIS_FLAG"] = DBNull.Value;
                    drRQSTDT["PACK_MEB_LINE_FLAG"] = DBNull.Value;
                    drRQSTDT["PACK_BOX_LINE_FLAG"] = "Y";
                    dtRQSTDT.Rows.Add(drRQSTDT);

                    dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                    if (CommonVerify.HasTableRow(dtRSLTDT))
                    {
                        dtReturn = dtRSLTDT.Copy();
                        //this.dtPackEquipmentInfo = dtRSLTDT.Copy();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
            #endregion

            #region [ 콤보 필드 설정 ] 
            public DataTable GetJudgeFlag()
            {
                if (!CommonVerify.HasTableRow(this.dtJubgeCombo))
                {
                    return null;
                }

                var query = this.dtJubgeCombo.AsEnumerable().Select(x => new
                {
                    JUDGE_CBO_CODE = x.Field<string>("CBO_CODE"),
                    JUDGE_CBO_NAME = x.Field<string>("CBO_NAME")
                });

                return PackCommon.queryToDataTable(query.ToList());
            }

            public DataTable GetPackEquipmentInfo()
            {
                if (!CommonVerify.HasTableRow(this.dtPackEquipmentInfo))
                {
                    return null;
                }

                var query = this.dtPackEquipmentInfo.AsEnumerable().Select(x => new
                {
                    EQPT_CBO_CODE = x.Field<string>("CBO_CODE"),
                    EQPT_CBO_NAME = x.Field<string>("CBO_NAME")
                });

                return PackCommon.queryToDataTable(query.ToList());
            }


            public DataTable GetEquipmentInfo()
            {
                if (!CommonVerify.HasTableRow(this.dtEquipmentInfo))
                {
                    return null;
                }

                var query = this.dtEquipmentInfo.AsEnumerable().Select(x => new
                {
                    EQPT_CBO_CODE = x.Field<string>("CBO_CODE"),
                    EQPT_CBO_NAME = x.Field<string>("CBO_NAME")
                });

                return PackCommon.queryToDataTable(query.ToList());
            }
            #endregion

        }
        #endregion

        #region [ DataGrid Class ]
        public class PackDataGridSetDataHelper
        {
            #region Member Variable Lists...
            private DataTable dtDefect = new DataTable();

            public PackDataGridSetDataHelper()
            {
                this.GetQmsDefect(ref this.dtDefect);
            }

            public void GetQmsDefect(ref DataTable dtReturn)
            {
                try
                {
                    string bizRuleName = "DA_QCA_SEL_ACTIVITYREASON_LINE";
                    DataTable dtRQSTDT = new DataTable("RQSTDT");
                    DataTable dtRSLTDT = new DataTable("RSLTDT");

                    dtRQSTDT.Columns.Add("LANGID", typeof(string));
                    dtRQSTDT.Columns.Add("DFCT_TYPE_CODE", typeof(string));
                    dtRQSTDT.Columns.Add("ACTID", typeof(string));
                    dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                    dtRQSTDT.Columns.Add("PROCID", typeof(string));

                    DataRow drRQSTDT = dtRQSTDT.NewRow();
                    drRQSTDT["LANGID"] = LoginInfo.LANGID;
                    drRQSTDT["DFCT_TYPE_CODE"] = null;
                    drRQSTDT["ACTID"] = "DEFECT_LOT";
                    drRQSTDT["EQSGID"] = null;
                    drRQSTDT["PROCID"] = "P5250";


                    dtRQSTDT.Rows.Add(drRQSTDT);

                    dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                    if (CommonVerify.HasTableRow(dtRSLTDT))
                    {
                        this.dtDefect = dtRSLTDT.Copy();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }

            public DataTable GetDefect()
            {
                if (!CommonVerify.HasTableRow(this.dtDefect))
                {
                    return null;
                }

                return dtDefect;
            }
            #endregion

        }
        #endregion

        #region [ Validation Class ] 
        public class PackInterLockHelper
        {
            #region Member Variable Lists...
            private DataTable dtTimeInterLock = new DataTable();
            #endregion

            public PackInterLockHelper()
            {
                this.GetTimeInterLock(ref this.dtTimeInterLock);
            }

            public void GetTimeInterLock(ref DataTable dtReturn)
            {
                try
                {
                    string bizRuleName = "DA_BAS_SEL_COMMONCODE_ATTRIBUTE";
                    DataTable dtRQSTDT = new DataTable("RQSTDT");
                    DataTable dtRSLTDT = new DataTable("RSLTDT");

                    dtRQSTDT.Columns.Add("LANGID", typeof(string));
                    dtRQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                    dtRQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));
                    //라인별을 위한 혹시 모른 대비
                    //dtRQSTDT.Columns.Add("ATTRIBUTE2", typeof(string));

                    DataRow drRQSTDT = dtRQSTDT.NewRow();
                    drRQSTDT["LANGID"] = LoginInfo.LANGID;
                    drRQSTDT["CMCDTYPE"] = "PACK_UI_TRUCKING_INTERLOCK";
                    drRQSTDT["ATTRIBUTE1"] = "END_LOT";
                    //라인별을 위한 혹시 모른 대비
                    //drRQSTDT["ATTRIBUTE2"] = null;

                    dtRQSTDT.Rows.Add(drRQSTDT);

                    dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                    if (CommonVerify.HasTableRow(dtRSLTDT))
                    {
                        this.dtTimeInterLock = dtRSLTDT.Copy();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }

            public DataTable GetInterLock()
            {
                if (!CommonVerify.HasTableRow(this.dtTimeInterLock))
                {
                    return null;
                }

                return dtTimeInterLock;
            }

        }
        #endregion

        #endregion


        private void SetComboBox()
        {
            this.SetComboBox(this.dataHelper.GetJudgeFlag(), this.cboJudge, ComboStatus.NA);
            this.SetComboBox(this.dataHelper.GetPackEquipmentInfo(), this.cboEqptId, ComboStatus.ALL);
            this.SetComboBox(this.dataHelper.GetPackEquipmentInfo(), this.cboEndTabEqptId, ComboStatus.ALL);

            //this.SetComboBox(this.dataHelper.GetEquipmentInfo(), this.cboStartEqptId, ComboStatus.NA);
            //this.SetComboBox(this.dataHelper.GetPackEquipmentInfo(), this.cboEndEqptId, ComboStatus.NA);

        }

        private void SetDataGrid()
        {
            this.SetRadioGrid(this.dgHelper.GetDefect(), dgDefect);
        }

        private void SetComboBox(DataTable dtSource, C1ComboBox c1ComboBox, ComboStatus cs)
        {
            if (!CommonVerify.HasTableRow(dtSource))
            {
                return;
            }

            DataTable dtBinding = dtSource.Copy();
            string codeColumnName = dtBinding.Columns.OfType<DataColumn>().Where(x => x.ColumnName.Contains("CBO_NAME")).Select(x => x.ColumnName).FirstOrDefault();
            string codeValueColumnName = dtBinding.Columns.OfType<DataColumn>().Where(x => x.ColumnName.Contains("CBO_CODE")).Select(x => x.ColumnName).FirstOrDefault();
            if (codeColumnName == null || codeValueColumnName == null)
            {
                return;
            }

            if (cs.ToString().Equals("ALL"))
            {
                DataRow drBinding = dtBinding.NewRow();
                drBinding[codeColumnName] = "-ALL-";
                drBinding[codeValueColumnName] = null;
                dtBinding.Rows.InsertAt(drBinding, 0);
            }

            c1ComboBox.DisplayMemberPath = codeColumnName;
            c1ComboBox.SelectedValuePath = codeValueColumnName;
            c1ComboBox.ItemsSource = dtBinding.AsDataView();
            c1ComboBox.SelectedIndex = 0;
        }

        private void SetRadioGrid(DataTable dtRadio, C1.WPF.DataGrid.C1DataGrid dgRadioGrid)
        {
            if (!CommonVerify.HasTableRow(dtRadio))
            {
                return;
            }

            dgRadioGrid.ItemsSource = DataTableConverter.Convert(dtRadio);
            Util.GridAllColumnWidthAuto(ref dgRadioGrid);

        }

        private void TextBox_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e == null) return;

                if ((Keyboard.IsKeyDown(Key.LeftCtrl)) && (e.Key == Key.V))
                {
                    this.loadingIndicator.Visibility = Visibility.Visible;
                    this.dgTruckingStart.Columns[1].Visibility = Visibility.Visible;
                    PackCommon.DoEvents();

                    DataTable dtStartLotList = new DataTable();

                    LotGrIdText.SelectAll();

                    dtStartLotList = Search("START_LOT", Clipboard.GetText().ToString());


                    if (dtStartLotList == null || dtStartLotList.Rows.Count == 0)
                    {
                        Util.MessageInfo("SFU3536", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                LotGrIdText.SelectAll();
                                LotGrIdText.Focus();
                            }
                        });

                        return;
                    }
                    else
                    {
                        Util.GridSetData(dgTruckingStart, dtStartLotList, FrameOperation);
                        Util.SetTextBlockText_DataGridRowCount(StartListCount, DataTableConverter.Convert(dgTruckingStart.ItemsSource).Rows.Count.ToString());
                        LotGrIdText.SelectAll();
                        LotGrIdText.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                this.loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally
            {
                this.loadingIndicator.Visibility = Visibility.Collapsed;
                //LotGrIdText.SelectAll();
                //LotGrIdText.Focus();
            }

        }

        private void LotGrIdText_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e == null) return;

                if (e.Key == Key.Enter)
                {
                    this.loadingIndicator.Visibility = Visibility.Visible;
                    this.dgTruckingStart.Columns[1].Visibility = Visibility.Visible;
                    PackCommon.DoEvents();

                    LotGrIdText.SelectAll();

                    DataTable dtStartLotList = new DataTable();

                    dtStartLotList = Search("START_LOT", LotGrIdText.Text.ToString());

                    if (dtStartLotList == null || dtStartLotList.Rows.Count == 0)
                    {

                        Util.MessageInfo("SFU3536", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                LotGrIdText.SelectAll();
                                LotGrIdText.Focus();
                                result = MessageBoxResult.None;
                                return;
                            }
                        });
                    }
                    else
                    {
                        Util.GridSetData(dgTruckingStart, dtStartLotList, FrameOperation);
                        Util.SetTextBlockText_DataGridRowCount(StartListCount, DataTableConverter.Convert(dgTruckingStart.ItemsSource).Rows.Count.ToString());
                        LotGrIdText.SelectAll();
                        LotGrIdText.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                this.loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally
            {
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }


        private DataTable Search(string searchType, string strLotGrId)
        {
            try
            {
                string bizRuleName = "BR_PRD_SEL_TB_SFC_LOGIS_MOD_GR_DETL";

                DataTable dtINDATA = new DataTable("INDATA");
                DataTable dtOUTDATA = new DataTable("OUTDATA");

                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("GR_ID", typeof(string));
                dtINDATA.Columns.Add("PROCID", typeof(string));
                dtINDATA.Columns.Add("WIPSTAT", typeof(string));
                dtINDATA.Columns.Add("CHK_TYPE", typeof(string));
                dtINDATA.Columns.Add("LOGIS_PACK_TYPE", typeof(string));

                DataRow drINDATA = dtINDATA.NewRow();

                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["GR_ID"] = strLotGrId;
                drINDATA["CHK_TYPE"] = searchType;
                drINDATA["LOGIS_PACK_TYPE"] = "TRU";

                if (searchType == "START_LOT")
                {
                    drINDATA["PROCID"] = "P5250";
                    drINDATA["WIPSTAT"] = "WAIT";
                }
                else
                {
                    drINDATA["PROCID"] = "P5250";
                    drINDATA["WIPSTAT"] = "PROC";
                }

                dtINDATA.Rows.Add(drINDATA);

                dtOUTDATA = new ClientProxy().ExecuteServiceSync(bizRuleName, dtINDATA.TableName, dtOUTDATA.TableName, dtINDATA);

                this.loadingIndicator.Visibility = Visibility.Collapsed;
                return dtOUTDATA;

            }
            catch (Exception ex)
            {
              //Util.MessageException(ex);
                LotGrIdText.SelectAll();
                this.loadingIndicator.Visibility = Visibility.Collapsed;
                return null;
            }
            finally
            {
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void serachSetColor(string strSearchType)
        {
            try
            {
                if (strSearchType == "START_LOT")
                {
                    if (dgTruckingStart == null || dgTruckingStart.Rows.Count == 0) return;

                    PackCommon.DoEvents();

                    for (int j = 0; j < dgTruckingStart.Rows.Count; j++)
                    {
                        if (this.dgTruckingStart.GetCell(j, dgTruckingStart.Columns["CAN_START_FLAG"].Index).ToString().Equals("Y"))
                        {
                            this.dgTruckingStart.GetCell(j, dgTruckingStart.Columns["RESULT"].Index).Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                        }
                        else
                        {
                            this.dgTruckingStart.GetCell(j, dgTruckingStart.Columns["RESULT"].Index).Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
                        }
                    }
                    dgTruckingStart.EndEdit();
                    dgTruckingStart.EndEditRow(true);
                }
                else
                {
                    if (dgTruckingEnd == null || dgTruckingEnd.Rows.Count == 0) return;

                    PackCommon.DoEvents();

                    for (int j = 0; j < dgTruckingEnd.Rows.Count; j++)
                    {
                        if (this.dgTruckingEnd.GetCell(j, dgTruckingEnd.Columns["CAN_END_FLAG"].Index).ToString().Equals("Y"))
                        {
                            this.dgTruckingEnd.GetCell(j, dgTruckingEnd.Columns["RESULT"].Index).Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                        }
                        else
                        {
                            this.dgTruckingEnd.GetCell(j, dgTruckingEnd.Columns["RESULT"].Index).Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
                        }
                    }

                    dgTruckingEnd.EndEdit();
                    dgTruckingEnd.EndEditRow(true);
                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkAll_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                PackCommon.GridCheckAllFlag_ExcepValue(this.dgTruckingStart, true, "CHK", "CAN_START_FLAG", "N");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void chkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                PackCommon.GridCheckAllFlag_ExcepValue(this.dgTruckingStart, false, "CHK", "CAN_START_FLAG", "N");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgTruckingStart_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgTruckingStart.GetCellFromPoint(pnt);

                if (cell == null) return;

                if (cell.Row.Index > -1)
                {
                    if (cell.Column.Name == null) return;

                    if (cell.Column.Name == "CHK")
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgTruckingStart.Rows[cell.Row.Index].DataItem, "CAN_START_FLAG")) == "Y")
                        {
                            if (cell.Value.ToString() == "True")
                            {

                                DataTableConverter.SetValue(dgTruckingStart.Rows[cell.Row.Index].DataItem, "CHK", false);
                            }
                            else
                            {
                                DataTableConverter.SetValue(dgTruckingStart.Rows[cell.Row.Index].DataItem, "CHK", true);
                            }

                            dgTruckingStart.EndEdit();
                            dgTruckingStart.EndEditRow(true);

                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.loadingIndicator.Visibility = Visibility.Visible;
                PackCommon.DoEvents();

                DataTable dt = DataTableConverter.Convert(dgTruckingStart.ItemsSource);


                if (dt.Rows.Count == 0 || dt.Select("CHK = '" + true + "'").Length == 0)
                {
                    Util.MessageConfirm("SFU1651", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            LotGrIdText.SelectAll();
                        }
                    });
                    this.loadingIndicator.Visibility = Visibility.Collapsed;
                    PackCommon.DoEvents();
                    return;


                }

                var dtChkDataGrid = dt.AsEnumerable().Where(row => Convert.ToString(row["CHK"]) == "True");

                DataTable dtStartLot = dtChkDataGrid.CopyToDataTable();

                this.dgTruckingStart.Columns[1].Visibility = Visibility.Visible;
                InputTrucking(dtStartLot);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                this.loadingIndicator.Visibility = Visibility.Collapsed;
                PackCommon.DoEvents();
            }
        }

        private void InputTrucking(DataTable dtLotList)
        {
            DataSet dsResult = new DataSet();

            try
            {

                if (dtLotList == null || dtLotList.Rows.Count == 0)
                {
                    Util.MessageConfirm("SFU1261",(result) =>
                        {
                            if(result == MessageBoxResult.OK)
                            {
                                LotGrIdText.SelectAll();
                            }
                        }
                    ); 
                }

                foreach (DataRow dr in dtLotList.Rows)
                {
                    dsResult = null;

                    DataSet dsInput = new DataSet();

                    DataTable dtINDATA = new DataTable();
                    dtINDATA.TableName = "INDATA";
                    dtINDATA.Columns.Add("SRCTYPE", typeof(string));
                    dtINDATA.Columns.Add("LANGID", typeof(string));
                    dtINDATA.Columns.Add("LOTID", typeof(string));
                    //dtINDATA.Columns.Add("MLOTID", typeof(string));
                    dtINDATA.Columns.Add("PROCID", typeof(string));
                    dtINDATA.Columns.Add("EQPTID", typeof(string));
                    dtINDATA.Columns.Add("USERID", typeof(string));
                    dtINDATA.Columns.Add("PRNFLAG", typeof(string));

                    DataRow drINDATA = dtINDATA.NewRow();
                    drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    drINDATA["LANGID"] = LoginInfo.LANGID;
                    drINDATA["LOTID"] = dr["LOTID"].ToString();
                    //drINDATA["MLOTID"] = null;
                    drINDATA["PROCID"] = "P5250";
                    drINDATA["EQPTID"] = "W1P2M8D06";
                    drINDATA["USERID"] = LoginInfo.USERID;
                    drINDATA["PRNFLAG"] = "N";

                    dtINDATA.Rows.Add(drINDATA);

                    dsInput.Tables.Add(dtINDATA);

                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_INPUTASSY", "INDATA,IN_MTRL", "OUTDATA,OUT_REWORKSTATION", (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                for (int j = 0; j < dgTruckingStart.Rows.Count; j++)
                                {
                                    if (Util.NVC(DataTableConverter.GetValue(dgTruckingStart.Rows[j].DataItem, "LOTID")) == dsInput.Tables["INDATA"].Rows[0]["LOTID"].ToString())
                                    {
                                        //this.dgTruckingStart.GetCell(j, dgTruckingStart.Columns["RESULT"].Index).Presenter.Background = new SolidColorBrush(Colors.Red);
                                        //this.dgTruckingStart.GetCell(j, dgTruckingStart.Columns["RESULT"].Index).Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                        DataTableConverter.SetValue(dgTruckingStart.Rows[j].DataItem, "RESULT", ObjectDic.Instance.GetObjectName("NG"));
                                        DataTableConverter.SetValue(dgTruckingStart.Rows[j].DataItem, "CAN_START_FLAG", ObjectDic.Instance.GetObjectName("F"));
                                        dgTruckingStart.EndEdit();
                                        dgTruckingStart.EndEditRow(true);

                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (dgTruckingStart != null && dgTruckingStart.Rows.Count > 0)
                                {

                                    string bizRuleName = "BR_PRD_SEL_TB_SFC_LOGIS_MOD_GR_DETL";

                                    DataTable dtINDATA2 = new DataTable("INDATA");
                                    DataTable dtOUTDATA = new DataTable("OUTDATA");

                                    dtINDATA2.Columns.Add("LANGID", typeof(string));
                                    dtINDATA2.Columns.Add("GR_ID", typeof(string));
                                    dtINDATA2.Columns.Add("PROCID", typeof(string));
                                    dtINDATA2.Columns.Add("WIPSTAT", typeof(string));
                                    dtINDATA2.Columns.Add("CHK_TYPE", typeof(string));

                                    DataRow drINDATA2 = dtINDATA2.NewRow();

                                    drINDATA2["LANGID"] = LoginInfo.LANGID;
                                    drINDATA2["GR_ID"] = bizResult.Tables["OUTDATA"].Rows[0]["LOTID"].ToString();
                                    drINDATA2["CHK_TYPE"] = "START_LOT";
                                    drINDATA2["PROCID"] = "P5250";
                                    drINDATA2["WIPSTAT"] = "WAIT";

                                    dtINDATA2.Rows.Add(drINDATA2);

                                    dtOUTDATA = new ClientProxy().ExecuteServiceSync(bizRuleName, dtINDATA.TableName, dtOUTDATA.TableName, dtINDATA2);

                                    for (int j = 0; j < dgTruckingStart.Rows.Count; j++)
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgTruckingStart.Rows[j].DataItem, "LOTID")) == bizResult.Tables["OUTDATA"].Rows[0]["LOTID"].ToString())
                                        {

                                            //this.dgTruckingStart.GetCell(j, dgTruckingStart.Columns["RESULT"].Index).Presenter.Background = new SolidColorBrush(Colors.Green);
                                            //this.dgTruckingStart.GetCell(j, dgTruckingStart.Columns["RESULT"].Index).Presenter.Foreground = new SolidColorBrush(Colors.Yellow);
                                            DataTableConverter.SetValue(dgTruckingStart.Rows[j].DataItem, "PROCID", dtOUTDATA.Rows[0]["PROCID"].ToString());
                                            DataTableConverter.SetValue(dgTruckingStart.Rows[j].DataItem, "PROCNAME", dtOUTDATA.Rows[0]["PROCNAME"].ToString());
                                            DataTableConverter.SetValue(dgTruckingStart.Rows[j].DataItem, "WIPSTAT", dtOUTDATA.Rows[0]["WIPSTAT"].ToString());
                                            DataTableConverter.SetValue(dgTruckingStart.Rows[j].DataItem, "WIPSNAME", dtOUTDATA.Rows[0]["WIPSNAME"].ToString());
                                            DataTableConverter.SetValue(dgTruckingStart.Rows[j].DataItem, "WIPDTTM_ST", dtOUTDATA.Rows[0]["WIPDTTM_ST"].ToString());
                                            DataTableConverter.SetValue(dgTruckingStart.Rows[j].DataItem, "CAN_START_FLAG", dtOUTDATA.Rows[0]["CAN_START_FLAG"].ToString());
                                            DataTableConverter.SetValue(dgTruckingStart.Rows[j].DataItem, "CAN_START_FLAG", ObjectDic.Instance.GetObjectName("OK"));
                                            DataTableConverter.SetValue(dgTruckingStart.Rows[j].DataItem, "RESULT", ObjectDic.Instance.GetObjectName("정상착공"));
                                            dgTruckingStart.EndEdit();
                                            dgTruckingStart.EndEditRow(true);

                                            break;
                                        }
                                    }
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            this.dgTruckingStart.Columns[1].Visibility = Visibility.Collapsed;
                            this.loadingIndicator.Visibility = Visibility.Collapsed;
                            PackCommon.DoEvents();
                            Util.MessageException(ex);
                        }
                        finally
                        {
                            this.loadingIndicator.Visibility = Visibility.Collapsed;
                            PackCommon.DoEvents();
                        }
                    }, dsInput);

                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

            }

        }

        private void chkEndAll_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                PackCommon.GridCheckAllFlag_ExcepValue(this.dgTruckingEnd, true, "CHK", "CAN_END_FLAG", "N");

                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkEndAll_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                PackCommon.GridCheckAllFlag_ExcepValue(this.dgTruckingEnd, false, "CHK", "CAN_END_FLAG", "N");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void textEndLotGrId_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e == null) return;

                if ((Keyboard.IsKeyDown(Key.LeftCtrl)) && (e.Key == Key.V))
                {
                    this.loadingIndicator.Visibility = Visibility.Visible;
                    this.dgTruckingEnd.Columns[1].Visibility = Visibility.Visible;
                    PackCommon.DoEvents();

                    DataTable dtEndLotList = new DataTable();

                    dtEndLotList = Search("END_LOT", Clipboard.GetText().ToString());

                    if (dtEndLotList == null || dtEndLotList.Rows.Count == 0)
                    {
                        Util.MessageInfo("SFU3536", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                textEndLotGrId.Text = Clipboard.GetText().ToString();
                                textEndLotGrId.SelectAll();
                                textEndLotGrId.Focus();
                                return;
                            }
                        });
                        return;
                    }
                    else
                    {
                        Util.GridSetData(dgTruckingEnd, dtEndLotList, FrameOperation);
                        textEndLotGrId.SelectAll();
                        textEndLotGrId.Focus();
                        txtEndWipQty.Text = dtEndLotList.Rows.Count.ToString();
                        this.txtEndWipNote.Text = dtEndLotList.Rows[0]["GR_ID"].ToString();
                        Util.SetTextBlockText_DataGridRowCount(EndListCount ,dtEndLotList.Rows.Count.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                this.loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally
            {
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void textEndLotGrId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e == null) return;

                if (e.Key == Key.Enter)
                {
                    this.loadingIndicator.Visibility = Visibility.Visible;
                    this.dgTruckingEnd.Columns[1].Visibility = Visibility.Visible;
                    PackCommon.DoEvents();

                    DataTable dtEndLotList = new DataTable();

                    dtEndLotList = Search("END_LOT", textEndLotGrId.Text.ToString());

                    if (dtEndLotList == null || dtEndLotList.Rows.Count == 0)
                    {
                        Util.MessageInfo("SFU3536", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                textEndLotGrId.SelectAll();
                                textEndLotGrId.Focus();
                                return;
                            }
                        });
                        return;
                    }
                    else
                    {
                        Util.GridSetData(dgTruckingEnd, dtEndLotList, FrameOperation);
                        Util.SetTextBlockText_DataGridRowCount(EndListCount, dtEndLotList.Rows.Count.ToString());
                        //serachSetColor("END_LOT");
                        txtEndWipQty.Text = dtEndLotList.Rows.Count.ToString();
                        textEndLotGrId.SelectAll();
                        textEndLotGrId.Focus();
                    }
                }

            }
            catch (Exception ex)
            {
                this.loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally
            {
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void EndConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.loadingIndicator.Visibility = Visibility.Visible;
                PackCommon.DoEvents();

                DataTable dt = DataTableConverter.Convert(dgTruckingEnd.ItemsSource);

                if (dt.Rows.Count == 0 || dt.Select("CHK = '" + true + "'").Length == 0)
                {
                    Util.MessageInfo("선택된 항목이 없습니다.");
                    this.loadingIndicator.Visibility = Visibility.Collapsed;
                    PackCommon.DoEvents();
                    return;
                }

                var dtChkDataGrid = dt.AsEnumerable().Where(row => Convert.ToString(row["CHK"]) == "True");

                DataTable dtEndLot = dtChkDataGrid.CopyToDataTable();
                if (cboJudge.SelectedValue.ToString().Equals("N") && string.IsNullOrEmpty(Util.gridFindDataRow_GetValue(ref dgDefect, "CHK", "True", "RESNCODE")))// MES 2.0 CHK 컬럼 Bool 오류 Patch
                {
                    Util.MessageInfo("SFU1639");
                    this.loadingIndicator.Visibility = Visibility.Collapsed;
                    PackCommon.DoEvents();
                    return;
                }

                this.EndTabTruck.Columns[1].Visibility = Visibility.Visible;
                OutputTrucking(dtEndLot);
            }
            catch (Exception ex)
            {

            }
        }

        private Boolean OutputTrucking(DataTable dtLotList)
        {
            Boolean bTruckingOk = false;
            DataSet dsResult = new DataSet();

            try
            {

                if (dtLotList == null || dtLotList.Rows.Count == 0)
                {
                    Util.MessageInfo("SFU1261");
                    return bTruckingOk;
                }

                DateTime dtChkTime = GetSysTime();

                string strTimeInterLockType = dtInterLock.GetInterLock().Rows[0]["ATTRIBUTE3"].ToString();
                int iInterTime = Convert.ToInt32(dtInterLock.GetInterLock().Rows[0]["ATTRIBUTE4"]);
                
                foreach (DataRow dr in dtLotList.Rows)
                {
                    double diffTime = 0;



                    if (strTimeInterLockType == "M")
                    {
                        diffTime = (dtChkTime - Convert.ToDateTime(dr["WIPDTTM_ST"])).TotalMinutes;
                    }
                    else if (strTimeInterLockType == "H")
                    {
                        diffTime = (dtChkTime - Convert.ToDateTime(dr["WIPDTTM_ST"])).TotalHours;
                    }
                    else if (strTimeInterLockType == "D")
                    {
                        diffTime = (dtChkTime - Convert.ToDateTime(dr["WIPDTTM_ST"])).TotalDays;
                    }

                    if (diffTime > iInterTime)
                    {
                        DataSet dsOutput = new DataSet();

                        DataTable dtINDATA = new DataTable();
                        dtINDATA.TableName = "INDATA";
                        dtINDATA.Columns.Add("SRCTYPE", typeof(string));
                        dtINDATA.Columns.Add("LANGID", typeof(string));
                        dtINDATA.Columns.Add("LOTID", typeof(string));
                        dtINDATA.Columns.Add("END_PROCID", typeof(string));
                        dtINDATA.Columns.Add("END_EQPTID", typeof(string));
                        dtINDATA.Columns.Add("STRT_PROCID", typeof(string));
                        dtINDATA.Columns.Add("STRT_EQPTID", typeof(string));
                        dtINDATA.Columns.Add("USERID", typeof(string));
                        dtINDATA.Columns.Add("WIPNOTE", typeof(string));
                        dtINDATA.Columns.Add("RESNCODE", typeof(string));
                        dtINDATA.Columns.Add("RESNNOTE", typeof(string));
                        dtINDATA.Columns.Add("RESNDESC", typeof(string));

                        DataRow drINDATA = dtINDATA.NewRow();
                        drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        drINDATA["LANGID"] = LoginInfo.LANGID;
                        drINDATA["LOTID"] = dr["LOTID"].ToString();
                        drINDATA["END_PROCID"] = "P5250";
                        drINDATA["END_EQPTID"] = "W1P2M8D06";
                        drINDATA["STRT_PROCID"] = null;
                        drINDATA["STRT_EQPTID"] = null;
                        drINDATA["USERID"] = LoginInfo.USERID;
                        drINDATA["WIPNOTE"] = cboJudge.SelectedValue.ToString().Equals("Y") ? Util.NVC(txtWipNote.Text) : null;
                        drINDATA["RESNCODE"] = cboJudge.SelectedValue.ToString().Equals("Y") ? "OK" : Util.gridFindDataRow_GetValue(ref dgDefect, "CHK", "True", "RESNCODE");// MES 2.0 CHK 컬럼 Bool 오류 Patch
                        drINDATA["RESNNOTE"] = cboJudge.SelectedValue.ToString().Equals("Y") ? null : Util.NVC(txtWipNote.Text);
                        drINDATA["RESNDESC"] = cboJudge.SelectedValue.ToString().Equals("Y") ? null : Util.gridFindDataRow_GetValue(ref dgDefect, "CHK", "True", "RESNDESC");// MES 2.0 CHK 컬럼 Bool 오류 Patch
                        dtINDATA.Rows.Add(drINDATA);

                        DataTable dtTemp = DataTableConverter.Convert(dgDefect.ItemsSource);

                        dsOutput.Tables.Add(dtINDATA);

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_OUTPUTASSY", "INDATA,IN_CLCTITEM,IN_CLCTDITEM", "OUTDATA", (bizResult, bizException) =>
                        {
                            try
                            {

                                if (bizException != null)
                                {
                                    for (int j = 0; j < dgTruckingEnd.Rows.Count; j++)
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgTruckingEnd.Rows[j].DataItem, "LOTID")) == dsOutput.Tables["INDATA"].Rows[0]["LOTID"].ToString())
                                        {
                                            if (this.dgTruckingEnd.GetCell(j, dgTruckingEnd.Columns["RESULT"].Index).Presenter != null)
                                            {
                                                this.dgTruckingEnd.GetCell(j, dgTruckingEnd.Columns["RESULT"].Index).Presenter.Background = new SolidColorBrush(Colors.Red);
                                                this.dgTruckingEnd.GetCell(j, dgTruckingEnd.Columns["RESULT"].Index).Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                            }

                                            DataTableConverter.SetValue(dgTruckingEnd.Rows[j].DataItem, "CAN_END_FLAG", "F");
                                            DataTableConverter.SetValue(dgTruckingEnd.Rows[j].DataItem, "RESULT", ObjectDic.Instance.GetObjectName("NG"));
                                            dgTruckingEnd.EndEdit();
                                            dgTruckingEnd.EndEditRow(true);

                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    if (dgTruckingEnd != null && dgTruckingEnd.Rows.Count > 0)
                                    {

                                        #region [ 조회 용 ]

                                        string bizRuleName = "BR_PRD_SEL_TB_SFC_LOGIS_MOD_GR_DETL";

                                        DataTable dtINDATA2 = new DataTable("INDATA");
                                        DataTable dtOUTDATA = new DataTable("OUTDATA");

                                        dtINDATA2.Columns.Add("LANGID", typeof(string));
                                        dtINDATA2.Columns.Add("GR_ID", typeof(string));
                                        dtINDATA2.Columns.Add("PROCID", typeof(string));
                                        dtINDATA2.Columns.Add("WIPSTAT", typeof(string));
                                        dtINDATA2.Columns.Add("CHK_TYPE", typeof(string));

                                        DataRow drINDATA2 = dtINDATA2.NewRow();

                                        drINDATA2["LANGID"] = LoginInfo.LANGID;
                                        drINDATA2["GR_ID"] = bizResult.Tables["OUTDATA"].Rows[0]["LOTID"].ToString();
                                        drINDATA2["CHK_TYPE"] = "END_LOT";
                                        drINDATA2["PROCID"] = "P5250";
                                        drINDATA2["WIPSTAT"] = "PROC";

                                        dtINDATA2.Rows.Add(drINDATA2);

                                        dtOUTDATA = new ClientProxy().ExecuteServiceSync(bizRuleName, dtINDATA.TableName, dtOUTDATA.TableName, dtINDATA2);

                                        #endregion

                                        #region [ GR_DETL, LOT별 해체 ] 
                                        //2022.02.16 염규범 선임
                                        //HEADER는 'Y' 유지
                                        //DETL 'N' 해체 , 'T' 완료, 'Y' 맵핑 상태

                                        string strUpbizRuleName = "DA_PRD_UPD_TB_SFC_LOGIS_MOD_GR_DETL";

                                        DataTable dtUpINDATA = new DataTable("RQSTDT");

                                        dtUpINDATA.Columns.Add("GR_ID", typeof(string));
                                        dtUpINDATA.Columns.Add("LOTID", typeof(string));
                                        dtUpINDATA.Columns.Add("USE_FLAG", typeof(string));
                                        dtUpINDATA.Columns.Add("USERID", typeof(string));

                                        DataRow drUpINDATA = dtUpINDATA.NewRow();
                                        #endregion

                                        for (int j = 0; j < dgTruckingEnd.Rows.Count; j++)
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dgTruckingEnd.Rows[j].DataItem, "LOTID")) == bizResult.Tables["OUTDATA"].Rows[0]["LOTID"].ToString())
                                            {
                                                if (bizResult.Tables["OUTDATA"].Rows[0]["RESNCODE"].ToString().Equals("OK"))
                                                {
                                                    if (dtOUTDATA.Rows.Count > 0) { 

                                                        drUpINDATA["GR_ID"] = dtOUTDATA.Rows[0]["GR_ID"].ToString();
                                                        drUpINDATA["LOTID"] = dtOUTDATA.Rows[0]["LOTID"].ToString();
                                                        drUpINDATA["USE_FLAG"] = "T";
                                                        drUpINDATA["USERID"] = LoginInfo.USERID;

                                                        dtUpINDATA.Rows.Add(drUpINDATA);

                                                        new ClientProxy().ExecuteServiceSync(strUpbizRuleName, "RQSTDT", "RSLTDT", dtUpINDATA);

                                                        DataTableConverter.SetValue(dgTruckingEnd.Rows[j].DataItem, "PROCID", dtOUTDATA.Rows[0]["PROCID"].ToString());
                                                        DataTableConverter.SetValue(dgTruckingEnd.Rows[j].DataItem, "PROCNAME", dtOUTDATA.Rows[0]["PROCNAME"].ToString());
                                                        DataTableConverter.SetValue(dgTruckingEnd.Rows[j].DataItem, "WIPSTAT", dtOUTDATA.Rows[0]["WIPSTAT"].ToString());
                                                        DataTableConverter.SetValue(dgTruckingEnd.Rows[j].DataItem, "WIPSNAME", dtOUTDATA.Rows[0]["WIPSNAME"].ToString());
                                                        DataTableConverter.SetValue(dgTruckingEnd.Rows[j].DataItem, "WIPDTTM_ED", dtOUTDATA.Rows[0]["WIPDTTM_ED"].ToString());
                                                        DataTableConverter.SetValue(dgTruckingEnd.Rows[j].DataItem, "CAN_END_FLAG", dtOUTDATA.Rows[0]["CAN_END_FLAG"].ToString());
                                                        DataTableConverter.SetValue(dgTruckingEnd.Rows[j].DataItem, "RESULT", ObjectDic.Instance.GetObjectName("정상완공"));
                                                        DataTableConverter.SetValue(dgTruckingEnd.Rows[j].DataItem, "CAN_END_FLAG", "C");
                                                        dgTruckingEnd.EndEdit();
                                                        dgTruckingEnd.EndEditRow(true);
                                                    }


                                                    if (this.dgTruckingEnd.GetCell(j, dgTruckingEnd.Columns["RESULT"].Index).Presenter != null)
                                                    {
                                                        this.dgTruckingEnd.GetCell(j, dgTruckingEnd.Columns["RESULT"].Index).Presenter.Background = new SolidColorBrush(Colors.Green);
                                                        this.dgTruckingEnd.GetCell(j, dgTruckingEnd.Columns["RESULT"].Index).Presenter.Foreground = new SolidColorBrush(Colors.Yellow);
                                                    }

                                                    
                                                }
                                                else if (bizResult.Tables["OUTDATA"].Rows[0]["RESNCODE"].ToString().Equals("NG"))
                                                {
                                                    if (dtOUTDATA.Rows.Count > 0)
                                                    {
                                                        drUpINDATA["GR_ID"] = dtOUTDATA.Rows[0]["GR_ID"].ToString();
                                                        drUpINDATA["LOTID"] = dtOUTDATA.Rows[0]["LOTID"].ToString();
                                                        drUpINDATA["USE_FLAG"] = "T";
                                                        drUpINDATA["USERID"] = LoginInfo.USERID;

                                                        dtUpINDATA.Rows.Add(drUpINDATA);

                                                        new ClientProxy().ExecuteServiceSync(strUpbizRuleName, "RQSTDT", "RSLTDT", dtUpINDATA);

                                                        DataTableConverter.SetValue(dgTruckingEnd.Rows[j].DataItem, "PROCID", dtOUTDATA.Rows[0]["PROCID"].ToString());
                                                        DataTableConverter.SetValue(dgTruckingEnd.Rows[j].DataItem, "PROCNAME", dtOUTDATA.Rows[0]["PROCNAME"].ToString());
                                                        DataTableConverter.SetValue(dgTruckingEnd.Rows[j].DataItem, "WIPSTAT", dtOUTDATA.Rows[0]["WIPSTAT"].ToString());
                                                        DataTableConverter.SetValue(dgTruckingEnd.Rows[j].DataItem, "WIPSNAME", dtOUTDATA.Rows[0]["WIPSNAME"].ToString());
                                                        DataTableConverter.SetValue(dgTruckingEnd.Rows[j].DataItem, "WIPDTTM_ED", dtOUTDATA.Rows[0]["WIPDTTM_ED"].ToString());
                                                        DataTableConverter.SetValue(dgTruckingEnd.Rows[j].DataItem, "CAN_END_FLAG", dtOUTDATA.Rows[0]["CAN_END_FLAG"].ToString());
                                                        DataTableConverter.SetValue(dgTruckingEnd.Rows[j].DataItem, "RESULT", ObjectDic.Instance.GetObjectName("불량완공"));
                                                        DataTableConverter.SetValue(dgTruckingEnd.Rows[j].DataItem, "CAN_END_FLAG", "NG");
                                                        dgTruckingEnd.EndEdit();
                                                        dgTruckingEnd.EndEditRow(true);
                                                    }

                                                    if (this.dgTruckingEnd.GetCell(j, dgTruckingEnd.Columns["RESULT"].Index).Presenter != null)
                                                    {
                                                        this.dgTruckingEnd.GetCell(j, dgTruckingEnd.Columns["RESULT"].Index).Presenter.Background = new SolidColorBrush(Colors.Blue);
                                                        this.dgTruckingEnd.GetCell(j, dgTruckingEnd.Columns["RESULT"].Index).Presenter.Foreground = new SolidColorBrush(Colors.Yellow);
                                                        
                                                    }

                                                   
                                                }
                                                else
                                                {
                                                    if (dtOUTDATA.Rows.Count > 0)
                                                    {
                                                        DataTableConverter.SetValue(dgTruckingEnd.Rows[j].DataItem, "PROCID", dtOUTDATA.Rows[0]["PROCID"].ToString());
                                                        DataTableConverter.SetValue(dgTruckingEnd.Rows[j].DataItem, "PROCNAME", dtOUTDATA.Rows[0]["PROCNAME"].ToString());
                                                        DataTableConverter.SetValue(dgTruckingEnd.Rows[j].DataItem, "WIPSTAT", dtOUTDATA.Rows[0]["WIPSTAT"].ToString());
                                                        DataTableConverter.SetValue(dgTruckingEnd.Rows[j].DataItem, "WIPSNAME", dtOUTDATA.Rows[0]["WIPSNAME"].ToString());
                                                        DataTableConverter.SetValue(dgTruckingEnd.Rows[j].DataItem, "WIPDTTM_ED", dtOUTDATA.Rows[0]["WIPDTTM_ED"].ToString());
                                                        DataTableConverter.SetValue(dgTruckingEnd.Rows[j].DataItem, "RESULT", ObjectDic.Instance.GetObjectName("확인필요"));
                                                        DataTableConverter.SetValue(dgTruckingEnd.Rows[j].DataItem, "CAN_END_FLAG", "F");

                                                        dgTruckingEnd.EndEdit();
                                                        dgTruckingEnd.EndEditRow(true);
                                                    }


                                                    if (this.dgTruckingEnd.GetCell(j, dgTruckingEnd.Columns["RESULT"].Index).Presenter != null)
                                                    {
                                                        this.dgTruckingEnd.GetCell(j, dgTruckingEnd.Columns["RESULT"].Index).Presenter.Background = new SolidColorBrush(Colors.Red);
                                                        this.dgTruckingEnd.GetCell(j, dgTruckingEnd.Columns["RESULT"].Index).Presenter.Foreground = new SolidColorBrush(Colors.Yellow);
                                                    }
                                                       
                                                }

                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                this.loadingIndicator.Visibility = Visibility.Visible;
                                PackCommon.DoEvents();
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                this.loadingIndicator.Visibility = Visibility.Collapsed;
                                PackCommon.DoEvents();
                            }
                        }, dsOutput);
                    }
                    else
                    {
                        for (int j = 0; j < dgTruckingEnd.Rows.Count; j++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgTruckingEnd.Rows[j].DataItem, "LOTID")) == dr["LOTID"].ToString())
                            {
                                if (this.dgTruckingEnd.GetCell(j, dgTruckingEnd.Columns["RESULT"].Index).Presenter != null)
                                {
                                    this.dgTruckingEnd.GetCell(j, dgTruckingEnd.Columns["RESULT"].Index).Presenter.Background = new SolidColorBrush(Colors.Red);
                                    this.dgTruckingEnd.GetCell(j, dgTruckingEnd.Columns["RESULT"].Index).Presenter.Foreground = new SolidColorBrush(Colors.Yellow);
                                }
                                    DataTableConverter.SetValue(dgTruckingEnd.Rows[j].DataItem, "RESULT", ObjectDic.Instance.GetObjectName("경과시간부족"));
                                    DataTableConverter.SetValue(dgTruckingEnd.Rows[j].DataItem, "CAN_END_FLAG", "T");
                                    dgTruckingEnd.EndEdit();
                                    dgTruckingEnd.EndEditRow(true);
                                    this.loadingIndicator.Visibility = Visibility.Collapsed;
                                    PackCommon.DoEvents();
                                    
                                    break;
                                
                            }
                        }
                    }
                }

                return bTruckingOk;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return bTruckingOk;

            }

        }


        public DateTime GetSysTime()
        {
            DateTime dtNow = System.DateTime.Now;

            try
            {
                string bizRuleName = "BR_CUS_GET_SYSTIME";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtNow = (DateTime)dtRSLTDT.Rows[0]["SYSTIME"];
                }

                return dtNow;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return dtNow;
            }
        }

        private void dgTruckingStart_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {

                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    if (e.Cell.Column.Name == "RESULT")
                    {
                        string strStartYn = DataTableConverter.GetValue(e.Cell.Row.DataItem, "CAN_START_FLAG").ToString();

                        if (strStartYn.Equals("N"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                            //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        }
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
                            //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);

                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgTruckingEnd_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {

                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    if (e.Cell.Column.Name == "RESULT")
                    {
                        string strEndYn = DataTableConverter.GetValue(e.Cell.Row.DataItem, "CAN_END_FLAG").ToString();

                        if (strEndYn.Equals("N"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                        }
                        else if (strEndYn.Equals("Y")) 
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
                        }
                        else if (strEndYn.Equals("T"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Yellow);
                        }
                        else if (strEndYn.Equals("F"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Yellow);
                        }
                        else if (strEndYn.Equals("C"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                        else if(strEndYn.Equals("NG"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);

                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgTruckingEnd_CurrentCellChanged(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {

                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e == null || e.Cell == null) return;

                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    if (e.Cell.Column.Name == "RESULT")
                    {
                        string strEndYn = this.dgTruckingEnd.GetCell(e.Cell.Row.Index, dgTruckingEnd.Columns["CAN_END_FLAG"].Index).ToString();

                        if (strEndYn.Equals("N"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                            //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        }
                        else if (strEndYn.Equals("T") || strEndYn.Equals("F"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                        else if (strEndYn.Equals("C"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        }
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
                            //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);

                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }



        private void dgDefectChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                //checked Mode 가 'TwoWay' 인경우 화면에 보이지 않는 부분의 체크가 남아있는 문제가 발생하여
                //한번더 체크 해제.
                RadioButton rbt = sender as RadioButton;
                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rbt.Parent).DataGrid;
                dg.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rbt.Parent).Cell.Row.Index;

                for (int i = 0; i < dgDefect.Rows.Count; i++)
                {
                    if (i != dg.SelectedIndex)
                    {
                        DataTableConverter.SetValue(dgDefect.Rows[i].DataItem, "CHK", false);
                    }
                    else
                    {
                        string sRESNDESC = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNNAME"));

                        txtWipNote.Text = sRESNDESC;

                    }
                }
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            DataTable dtList = new DataTable();


            dtList = searchGr(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd"), dtpDateTo.SelectedDateTime.AddDays(1).ToString("yyyyMMdd"), null, Util.NVC(cboEqptId.SelectedValue));

            if (dtList.Rows.Count == 0)
            {
                Util.MessageInfo("SFU3536");
                return;
            }

            Util.GridSetData(dgGroup, dtList, FrameOperation);

        }

        private void txtStartList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DataTable dtList = new DataTable();

                dtList = searchGr(null, null, txtStartList.Text.ToString(), null);

                if (dtList.Rows.Count == 0)
                {
                    Util.MessageConfirm("SFU3536", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtStartList.Focus();
                            //txtStartList.SelectAll();
                            return;
                        }
                    });
                }
                Util.GridSetData(dgGroup, dtList, FrameOperation);

                txtStartList.Focus();
                txtStartList.SelectAll();
            }
        }

        private void btnEndTabSearch_Click(object sender, RoutedEventArgs e)
        {
            DataTable dtList = new DataTable();


            dtList = searchGrEnd(EndDateFrom.SelectedDateTime.ToString("yyyyMMdd"), EndDateTo.SelectedDateTime.AddDays(1).ToString("yyyyMMdd"), null, Util.NVC(cboEqptId.SelectedValue));

            if (dtList.Rows.Count == 0)
            {
                Util.MessageInfo("SFU3536");
                return;
            }

            Util.GridSetData(dgEndTabGroup, dtList, FrameOperation);
        }

        private void txtEndList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DataTable dtList = new DataTable();

                dtList = searchGrEnd(null, null, txtEndList.Text.ToString(), null);

                if (dtList.Rows.Count == 0)
                {
                    Util.MessageInfo("SFU3536");
                    return;
                }
                Util.GridSetData(dgEndTabGroup, dtList, FrameOperation);

                txtEndList.SelectAll();
            }
        }


        private DataTable searchGr(string strFromDate, string strTodate, string strGrId = null, string strEqptid = null)
        {
            try
            {
                string bizRuleName = "DA_PRD_SEL_TB_SFC_LOGIS_MOD_GR_SEARCH";

                DataTable dtINDATA = new DataTable("RQSTDT");
                DataTable dtOUTDATA = new DataTable("RSLTDT");

                if (!string.IsNullOrEmpty(strGrId))
                {
                    dtINDATA.Columns.Add("GR_ID", typeof(string));
                }
                else
                {
                    if (!string.IsNullOrEmpty(strEqptid))
                    {
                        dtINDATA.Columns.Add("EQPTID", typeof(string));
                    }

                    dtINDATA.Columns.Add("FROM_DATE", typeof(string));
                    dtINDATA.Columns.Add("TO_DATE", typeof(string));
                }

                DataRow drINDATA = dtINDATA.NewRow();

                if (!string.IsNullOrEmpty(strGrId))
                {
                    drINDATA["GR_ID"] = strGrId;
                }
                else
                {
                    if (!string.IsNullOrEmpty(strEqptid))
                    {
                        drINDATA["EQPTID"] = Util.NVC(cboEqptId.SelectedValue);
                    }

                    drINDATA["FROM_DATE"] = strFromDate;
                    drINDATA["TO_DATE"] = strTodate;
                }

                dtINDATA.Rows.Add(drINDATA);

                dtOUTDATA = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", dtINDATA);

                return dtOUTDATA;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }

        }

        private DataTable searchGrEnd(string strFromDate, string strTodate, string strGrId = null, string strEqptid = null)
        {
            try
            {
                string bizRuleName = "DA_PRD_SEL_TB_SFC_LOGIS_MOD_GR_SEARCH";

                DataTable dtINDATA = new DataTable("RQSTDT");
                DataTable dtOUTDATA = new DataTable("RSLTDT");

                if (!string.IsNullOrEmpty(strGrId))
                {
                    dtINDATA.Columns.Add("GR_ID", typeof(string));
                }
                else
                {
                    if (!string.IsNullOrEmpty(strEqptid))
                    {
                        dtINDATA.Columns.Add("EQPTID", typeof(string));
                    }

                    dtINDATA.Columns.Add("FROM_DATE", typeof(string));
                    dtINDATA.Columns.Add("TO_DATE", typeof(string));
                }

                DataRow drINDATA = dtINDATA.NewRow();

                if (!string.IsNullOrEmpty(strGrId))
                {
                    drINDATA["GR_ID"] = strGrId;
                }
                else
                {
                    if (!string.IsNullOrEmpty(strEqptid))
                    {
                        drINDATA["EQPTID"] = Util.NVC(cboEndTabEqptId.SelectedValue);
                    }

                    drINDATA["FROM_DATE"] = strFromDate;
                    drINDATA["TO_DATE"] = strTodate;
                }

                dtINDATA.Rows.Add(drINDATA);

                dtOUTDATA = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", dtINDATA);

                return dtOUTDATA;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }

        }

        private void cboJudge_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (cboJudge == null) return;

                if (cboJudge.SelectedValue.ToString().Equals("N"))
                {
                    dgDefect.IsEnabled = true;
                }
                else
                {
                    dgDefect.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgTruckingEnd_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgTruckingEnd.GetCellFromPoint(pnt);

                if (cell == null) return;

                if (cell.Row.Index > -1)
                {
                    if (cell.Column.Name == null) return;

                    if (cell.Column.Name == "CHK")
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgTruckingEnd.Rows[cell.Row.Index].DataItem, "CAN_END_FLAG")) == "Y")
                        {
                            if (cell.Value.ToString() == "True")
                            {

                                DataTableConverter.SetValue(dgTruckingEnd.Rows[cell.Row.Index].DataItem, "CHK", false);
                            }
                            else
                            {
                                DataTableConverter.SetValue(dgTruckingEnd.Rows[cell.Row.Index].DataItem, "CHK", true);
                            }

                            dgTruckingEnd.EndEdit();
                            dgTruckingEnd.EndEditRow(true);

                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
