/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Data;
using System.IO;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_011 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        DataTable dtResult;
        DataTable dtLotInform;

        public C1.C1Report.C1Report cr;

        string tmp = string.Empty;
        string tmmp01 = string.Empty;
        bool textChange_YN = false;
        string procId;        

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_011()
        {
            try
            {
                InitializeComponent();

                this.Loaded += PACK001_011_Loaded;
            }
            catch(Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
           
        }
        #endregion

        #region Initialize
        private void Initialize()
        {         
            InitCombo();

            tbSearch_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
        }

        #endregion

        #region Event

        #region MAIN EVENT
        private void PACK001_011_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Initialize();

                setReport();

                //search();
                zoom_InOut(-30);
                txtPercent.Text = string.Format("{0:00}%", c1DocumentViewer.Zoom);
                

                this.Loaded -= PACK001_011_Loaded;
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }
        #endregion

        #region BUTTON EVENT
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(cboProduct.SelectedIndex == 0)
                {                    
                    ms.AlertWarning("SFU1895"); //제품을 선택하세요.
                    return;
                }
                //textchangedevent 시작
                //textChangedEventStart();

                search();

                setTexBox();
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void btnLotSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                textChange_YN = true;

                if (txtLotId.Text.Length == 0)
                {
                    return;
                }

                //lot 정보 검색
                getLotInform(txtLotId.Text);

                //binding 위해 datatable 가공
                bindDataCreate();

                //report에 binding
                //setReport();

                textChange_YN = false;

            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void textChangedEventStart()
        {
            //txtLotId.TextChanged += txtLotId_TextChanged;
            //txtMeasurement.TextChanged += txtMeasurement_TextChanged;
            //txtS5.TextChanged += txtS5_TextChanged;
            //txtName.TextChanged += txtName_TextChanged;
            //txtDate.TextChanged += txtDate_TextChanged;
            //txtSignature.TextChanged += txtSignature_TextChanged;

            rdoInsulationr_YES.Checked += rdoInsulationr_YES_Checked;
            rdoEquipotential_YES.Checked += rdoEquipotential_YES_Checked;
            rdoInsulations_YES.Checked += rdoInsulations_YES_Checked;
            rdoInsulationr_NO.Checked += rdoInsulationr_NO_Checked;
            rdoEquipotential_NO.Checked += rdoEquipotential_NO_Checked;
            rdoInsulations_NO.Checked += rdoInsulations_NO_Checked;
        }
     

        private void btnSmall_Click(object sender, RoutedEventArgs e)
        {
            getZoom(sender);
        }

        private void btnBig_Click(object sender, RoutedEventArgs e)
        {
            getZoom(sender);
        }
        #endregion

        #region GRID EVENT
        private void DataGrid_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            /*
                        e.Cell.Presenter.BorderThickness = new Thickness(0);

                        Dictionary<Point, Thickness> borderInfo = dataGrid.Resources["BorderInfo"] as Dictionary<Point, Thickness>;
                        Dictionary<Point, SolidColorBrush> colorInfo = dataGrid.Resources["ColorInfo"] as Dictionary<Point, SolidColorBrush>;

                        Dictionary<Point, CellStyleInfo> getCellStyle = dataGrid.Resources["CellStyleInfo"] as Dictionary<Point, CellStyleInfo>;


                        Point currentPoint = new Point(e.Cell.Row.Index + 1, e.Cell.Column.Index + 1);

                        //if (borderInfo.ContainsKey(currentPoint))
                        //{
                        //    Thickness thickness = borderInfo[currentPoint];

                        //    dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.BorderThickness = thickness;
                        //    dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Margin = new Thickness(0, 0, -1, -1);
                        //    dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                        //    dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                        //    dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                        //    dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                        //}

                        //if (colorInfo.ContainsKey(currentPoint))
                        //{
                        //    dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Background = colorInfo[currentPoint];
                        //}

                        if (getCellStyle.ContainsKey(currentPoint))
                        {
                            Thickness thickness = getCellStyle[currentPoint].cellThickness;

                            dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.BorderThickness = thickness;
                            dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Margin = new Thickness(0, 0, -1, -1);
                            dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                            dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                            dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                            dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);

                            dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Background = getCellStyle[currentPoint].cellBackColor;

                            dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.FontSize = getCellStyle[currentPoint].cellFontSize;
                            dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.FontFamily = new FontFamily(getCellStyle[currentPoint].cellFontName);
                        }
            */

        }

        private void dgResult_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgResult.Rows.Count == 0 || dgResult == null)
                {
                    return;
                }

                textChange_YN = true;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgResult.GetCellFromPoint(pnt);

                if (cell == null || cell.Value == null)
                {
                    return;
                }

                int currentRow = dgResult.CurrentRow.Index;
                string selectLot = DataTableConverter.GetValue(dgResult.Rows[currentRow].DataItem, "LOTID").ToString();
                procId = DataTableConverter.GetValue(dgResult.Rows[currentRow].DataItem, "PROCID").ToString();

                txtLotId.Text = selectLot;

                textChange_YN = false;
            }
            catch(Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }
        #endregion

        #region RADIOBUTTON EVENT
        private void rdoInsulationr_YES_Checked(object sender, RoutedEventArgs e)
        {
            rodioButtonControl(sender, e, "INSULATIONR_YES");
        }

        private void rdoInsulationr_NO_Checked(object sender, RoutedEventArgs e)
        {
            rodioButtonControl(sender, e, "INSULATIONR_NO");
        }

        private void rdoEquipotential_YES_Checked(object sender, RoutedEventArgs e)
        {
            rodioButtonControl(sender, e, "EQUIPOTENTIAL_YES");
        }

        private void rdoEquipotential_NO_Checked(object sender, RoutedEventArgs e)
        {
            rodioButtonControl(sender, e, "EQUIPOTENTIAL_NO");
        }

        private void rdoInsulations_YES_Checked(object sender, RoutedEventArgs e)
        {
            rodioButtonControl(sender, e, "INSULATIONS_YES");
        }

        private void rdoInsulations_NO_Checked(object sender, RoutedEventArgs e)
        {
            rodioButtonControl(sender, e, "INSULATIONS_NO");
        }

       
        #endregion

        #region TEXTBOX EVENT
        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            textBoxControl(sender, e, "COMPONENTDATA_SUP");
        }

        private void txtDesign_KeyDown(object sender, KeyEventArgs e)
        {
            textBoxControl(sender, e, "COMPONENTDATA_DES");
        }

        private void txtHwVersion_KeyDown(object sender, KeyEventArgs e)
        {
            textBoxControl(sender, e, "COMPONENTDATA_HWV");
        }

        private void txtVoltage_KeyDown(object sender, KeyEventArgs e)
        {
            textBoxControl(sender, e, "INSULATIONR_VDC1");
        }

        private void txtCurrent_KeyDown(object sender, KeyEventArgs e)
        {
            textBoxControl(sender, e, "EQUIPOTENTIAL_CUR");
        }

        private void txtMeasurement_KeyDown(object sender, KeyEventArgs e)
        {
            textBoxControl(sender, e, "INSULATIONR_MEA1");
        }

        private void txtS5_KeyDown(object sender, KeyEventArgs e)
        {
            textBoxControl(sender, e, "EQUIPOTENTIAL_MEA");
        }

        private void txtName_KeyDown(object sender, KeyEventArgs e)
        {
            textBoxControl(sender, e, "NAME");
        }

        private void txtDate_KeyDown(object sender, KeyEventArgs e)
        {
            textBoxControl(sender, e, "DATE");
        }

        private void txtSignature_KeyDown(object sender, KeyEventArgs e)
        {
            textBoxControl(sender, e, "SIGN");
        }

        private void txtLotId_TextChanged(object sender, TextChangedEventArgs e)
        {
            textBoxControl(sender, null, "COMPONENTDATA_SUP");
        }

        private void txtMeasurement_TextChanged(object sender, TextChangedEventArgs e)
        {
            textBoxControl(sender, null, "INSULATIONR_MEA1");
        }

        private void txtS5_TextChanged(object sender, TextChangedEventArgs e)
        {
            textBoxControl(sender, null, "EQUIPOTENTIAL_MEA");
        }

        private void txtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            textBoxControl(sender, null, "NAME");
        }

        private void txtDate_TextChanged(object sender, TextChangedEventArgs e)
        {
            textBoxControl(sender, null, "DATE");
        }

        private void txtSignature_TextChanged(object sender, TextChangedEventArgs e)
        {
            textBoxControl(sender, null, "SIGN");
        }

        private void rtbRemark_TextChanged(object sender, TextChangedEventArgs e)
        {
            textBoxControl(sender, null, "INSULATIONS_VOL1");
        }
        #endregion

        #region combobox
        private void cboProduct_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProduct == null || cboProduct.SelectedIndex == 0)
            {
                return;
            }
        }
        #endregion

        #region Grid
        private void btnExpandFrameTop_Checked(object sender, RoutedEventArgs e)
        {
            content_Right.RowDefinitions[1].Height = new GridLength(8);
            LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
            gla.From = new GridLength(0, GridUnitType.Star);
            gla.To = new GridLength(7, GridUnitType.Star);
            gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
            content_Right.RowDefinitions[2].BeginAnimation(RowDefinition.HeightProperty, gla);
        }

        private void btnExpandFrameTop_Unchecked(object sender, RoutedEventArgs e)
        {
            content_Right.RowDefinitions[1].Height = new GridLength(0);
            LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
            gla.From = new GridLength(7, GridUnitType.Star);
            gla.To = new GridLength(0, GridUnitType.Star); ;
            gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
            content_Right.RowDefinitions[2].BeginAnimation(RowDefinition.HeightProperty, gla);
        }

        private void btnExpandFrame_Checked(object sender, RoutedEventArgs e)
        {
            content_Right.RowDefinitions[1].Height = new GridLength(0);
            LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
            gla.From = new GridLength(3, GridUnitType.Star);
            gla.To = new GridLength(0, GridUnitType.Star); ;
            gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
            content_Right.RowDefinitions[0].BeginAnimation(RowDefinition.HeightProperty, gla);
        }

        private void btnExpandFrame_Unchecked(object sender, RoutedEventArgs e)
        {
            content_Right.RowDefinitions[1].Height = new GridLength(8);
            LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
            gla.From = new GridLength(0, GridUnitType.Star);
            gla.To = new GridLength(3, GridUnitType.Star);
            gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
            content_Right.RowDefinitions[0].BeginAnimation(RowDefinition.HeightProperty, gla);
        }

        #endregion

        #endregion

        #region Mehod
        private void InitCombo()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();

                C1ComboBox cboSHOPID = new C1ComboBox();
                cboSHOPID.SelectedValue = LoginInfo.CFG_SHOP_ID;
                C1ComboBox cboArea = new C1ComboBox();
                cboArea.SelectedValue = "P6";
                C1ComboBox cboEquipmentSegment = new C1ComboBox();
                cboEquipmentSegment.SelectedValue = "P6Q09";
                C1ComboBox cboProductModel = new C1ComboBox();
                cboProductModel.SelectedValue = "";
                C1ComboBox cboPrdtClass = new C1ComboBox();
                cboPrdtClass.SelectedValue = "CMA";

                setInfo(cboArea.SelectedValue.ToString(), cboEquipmentSegment.SelectedValue.ToString());

                //tbInfo.Text = LoginInfo.CFG_AREA_NAME + " // " + LoginInfo.CFG_EQSG_NAME + "(" + prdClass + ")";
                //tbInfo_Area.Text = LoginInfo.CFG_AREA_NAME;
                //tbInfo_Line.Text = LoginInfo.CFG_EQSG_NAME + "(" + "CMA" + ")";

                //제품    
                //C1ComboBox[] cboProductParent = { cboSHOPID, cboArea, cboEquipmentSegment, cboProductModel, cboAREA_TYPE_CODE, cboPrdtClass };
                //string[] line = { null, null, "P6Q09", null, null, "CMA" };
                //string[] line = { null, null, "P1Q02", null, null, "CMA" };
                //_combo.SetCombo(cboProduct, CommonCombo.ComboStatus.NONE, sFilter : line);


                //제품코드  
                C1ComboBox[] cboProductParent = { cboSHOPID, cboArea, cboEquipmentSegment, cboProductModel, cboPrdtClass };
                _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.SELECT, cbParent: cboProductParent, sCase: "PRJ_PRODUCT");

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
            

        private void setInfo(string area, string eqsg)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREA", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
              
                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREA"] = area;
                dr["EQSGID"] = eqsg;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_HV_REPORT_INFO", "INDATA", "OUTDATA", RQSTDT);             

                if (dtResult.Rows.Count != 0)
                {
                    tbInfo_Area.Text = dtResult.Rows[0]["AREANAME"].ToString();
                    tbInfo_Line.Text = dtResult.Rows[0]["EQSGNAME"].ToString() + "(" + "CMA" + ")";

                }

                Util.SetTextBlockText_DataGridRowCount(tbSearch_cnt, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void search()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));               
                RQSTDT.Columns.Add("PRODID", typeof(string));              
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PRODID"] = Util.GetCondition(cboProduct) == "" ? null : Util.GetCondition(cboProduct);                
                dr["FROMDATE"] = Util.GetCondition(dtpDateFrom);  //dtpDateFrom.SelectedDateTime.ToString();
                dr["TODATE"] = Util.GetCondition(dtpDateTo); //dtpDateTo.SelectedDateTime.ToString();
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID; 

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_HV_REPORT_SEARCH", "INDATA", "OUTDATA", RQSTDT);

                dgResult.ItemsSource = null;

                if (dtResult.Rows.Count != 0)
                {
                    Util.GridSetData(dgResult, dtResult, FrameOperation);
                }

                Util.SetTextBlockText_DataGridRowCount(tbSearch_cnt, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setReport()
        {
            try
            {
                string filename = string.Empty;
                string reportname = "Report_HV";

                cr = new C1.C1Report.C1Report();
                cr.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;

                filename = reportname + ".xml";

                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

                using (Stream stream = a.GetManifestResourceStream("LGC.GMES.MES.PACK001.Report." + filename))
                {
                    cr.Load(stream, reportname);

                    if (dtLotInform != null && dtLotInform.Rows.Count > 0)
                    {
                        for (int col = 0; col < dtLotInform.Columns.Count; col++)
                        {
                            string strColName = dtLotInform.Columns[col].ColumnName;
                            if (cr.Fields.Contains(strColName)) cr.Fields[strColName].Text = dtLotInform.Rows[0][strColName].ToString();
                        }
                    }

                    c1DocumentViewer.Document = cr.FixedDocumentSequence;
                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void getZoom(object sender)
        {
            switch (((Button)sender).Content.ToString())
            {
                case "+":
                    if (Convert.ToInt32(c1DocumentViewer.Zoom) < 200)
                        zoom_InOut(10);
                    break;
                case "-":
                    if (Convert.ToInt32(c1DocumentViewer.Zoom) > 50)
                        zoom_InOut(-10);
                    break;
            }

            //dataGrid.Update();
            txtPercent.Text = string.Format("{0:00}%", c1DocumentViewer.Zoom);
        }

        private void zoom_InOut(double value)
        {           
            c1DocumentViewer.Zoom += value;
        }

        private void btnOutput_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var pq = LocalPrintServer.GetDefaultPrintQueue();
                var writer = PrintQueue.CreateXpsDocumentWriter(pq);
                var paginator = c1DocumentViewer.Document.DocumentPaginator;
                writer.Write(paginator);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void getLotInform(string lot)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
               
                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = lot;
                dr["PROCID"] = procId;               

                RQSTDT.Rows.Add(dr);

                dtLotInform = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_HV_REPORT_FIND", "INDATA", "OUTDATA", RQSTDT);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void bindDataCreate()
        {
            if (dtLotInform == null || dtLotInform.Rows.Count == 0)
            {
                //테스트 목적으로 데이터 생성
                txtMeasurement.Text = "nodata";
                txtS5.Text = "nodata";
                txtName.Text = LoginInfo.USERID;
                txtDate.Text = DateTime.Now.ToString("yyyMMdd");
                txtSignature.Text = LoginInfo.USERID;
                return;
            }

            DataRow drLotInform = dtLotInform.Rows[0] as DataRow;

            DataTable INDATA = new DataTable();
            INDATA.TableName = "INDATA";
            INDATA.Columns.Add("REPORT_NUMBER", typeof(string));

            //2. Component data
            INDATA.Columns.Add("COMPONENTDATA_DUN", typeof(string));
            INDATA.Columns.Add("COMPONENTDATA_PAR", typeof(string));
            INDATA.Columns.Add("COMPONENTDATA_SUP", typeof(string));
            INDATA.Columns.Add("COMPONENTDATA_DES", typeof(string));
            INDATA.Columns.Add("COMPONENTDATA_HWV", typeof(string));
            INDATA.Columns.Add("COMPONENTDATA_COM", typeof(string));
            INDATA.Columns.Add("COMPONENTDATA_SUP", typeof(string));

            //3. Insulation resistance
            INDATA.Columns.Add("INSULATIONR_SPC1", typeof(string));
            INDATA.Columns.Add("INSULATIONR_SPC2", typeof(string));
            INDATA.Columns.Add("INSULATIONR_MEA1", typeof(string));
            INDATA.Columns.Add("INSULATIONR_MEA2", typeof(string));
            INDATA.Columns.Add("INSULATIONR_VDC1", typeof(string));
            INDATA.Columns.Add("INSULATIONR_VDC2", typeof(string));

            //4. Equipotential bonding
            INDATA.Columns.Add("EQUIPOTENTIAL_SEP", typeof(string));
            INDATA.Columns.Add("EQUIPOTENTIAL_MEA", typeof(string));
            INDATA.Columns.Add("EQUIPOTENTIAL_CUR", typeof(string));

            //5. Insulation strength
            INDATA.Columns.Add("INSULATIONS_SPC", typeof(string));
            INDATA.Columns.Add("INSULATIONS_VOL1", typeof(string));
            INDATA.Columns.Add("INSULATIONS_VOL2", typeof(string));
            INDATA.Columns.Add("INSULATIONS_VOL3", typeof(string));

            //8. Remark
            INDATA.Columns.Add("REMARK1", typeof(string));
            INDATA.Columns.Add("REMARK2", typeof(string));

            //name,date,signature
            INDATA.Columns.Add("NAME", typeof(string));
            INDATA.Columns.Add("DATE", typeof(string));
            INDATA.Columns.Add("SIGN", typeof(string));

            DataRow dr = INDATA.NewRow();
            dr["REPORT_NUMBER"] = SetValue(drLotInform, "REPORT_NUMBER");
            dr["COMPONENTDATA_DUN"] = SetValue(drLotInform, "");
            dr["COMPONENTDATA_PAR"] = SetValue(drLotInform, "");
            dr["COMPONENTDATA_SUP"] = SetValue(drLotInform, "");
            dr["COMPONENTDATA_DES"] = SetValue(drLotInform, "");
            dr["COMPONENTDATA_HWV"] = SetValue(drLotInform, "");
            dr["COMPONENTDATA_COM"] = SetValue(drLotInform, "");
            dr["COMPONENTDATA_SUP"] = SetValue(drLotInform, "");

            dr["INSULATIONR_SPC1"] = SetValue(drLotInform, "");
            dr["INSULATIONR_SPC2"] = SetValue(drLotInform, "");
            dr["INSULATIONR_MEA1"] = SetValue(drLotInform, "");
            dr["INSULATIONR_MEA2"] = SetValue(drLotInform, "");
            dr["INSULATIONR_VDC1"] = SetValue(drLotInform, "");
            dr["INSULATIONR_VDC2"] = SetValue(drLotInform, "");

            dr["EQUIPOTENTIAL_SEP"] = SetValue(drLotInform, "");
            dr["EQUIPOTENTIAL_MEA"] = SetValue(drLotInform, "");
            dr["EQUIPOTENTIAL_CUR"] = SetValue(drLotInform, "");

            dr["INSULATIONS_SPC"] = SetValue(drLotInform, "");
            dr["INSULATIONS_VOL1"] = SetValue(drLotInform, "");
            dr["INSULATIONS_VOL2"] = SetValue(drLotInform, "");
            dr["INSULATIONS_VOL3"] = SetValue(drLotInform, "");

            dr["REMARK1"] = SetValue(drLotInform, "");
            dr["REMARK2"] = SetValue(drLotInform, "");

            dr["NAME"] = SetValue(drLotInform, "");
            dr["DATE"] = SetValue(drLotInform, "");
            dr["SIGN"] = SetValue(drLotInform, "CLCTNAME");

            INDATA.Rows.Add(dr);

            txtMeasurement.Text = SetValue(drLotInform, "CLCTNAME");
            txtS5.Text = SetValue(drLotInform, "CLCTVAL");
            txtName.Text = LoginInfo.USERID;
            txtDate.Text = DateTime.Now.ToString();
            txtSignature.Text = LoginInfo.USERID;
        }

        private string SetValue(DataRow dr, string columnName)
        {
            string return_string;
            if (dr[columnName] == null)
            {
                return_string = "";
            }
            else if (dr[columnName].ToString().Length == 0)
            {
                return_string = "";
            }
            else
            {
                return_string = dr[columnName].ToString();
            }

            return return_string;
        }

        private void textBoxControl(object sender, KeyEventArgs e, string columnName)
        {
            try
            {
                string coumnValue = "";

                if (e == null) //textChanged event
                {
                    switch (sender.GetType().Name)
                    {
                        case "TextBox":
                            TextBox tb = sender as TextBox;
                            coumnValue = tb.Text;

                            if (coumnValue.Equals(""))
                            {
                                coumnValue = "";
                                break;
                            }
                            break;
                        case "RichTextBox":
                            RichTextBox rtb = sender as RichTextBox;

                            TextRange textRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);

                            if (textRange.Text.Equals("\r\n"))
                            {
                                coumnValue = "";
                                break;
                            }
                            else
                            {
                                coumnValue = textRange.Text;
                                break;
                            }
                    }
                }
                else // key_down event
                {
                    if (e.Key != Key.Enter)
                    {
                        return;
                    }

                    switch (sender.GetType().Name)
                    {
                        case "TextBox":
                            TextBox tb = sender as TextBox;
                            coumnValue = tb.Text;

                            if (coumnValue.Equals(""))
                            {
                                coumnValue = "";
                                break;
                            }
                            break;
                        case "RichTextBox":
                            RichTextBox rtb = sender as RichTextBox;

                            TextRange textRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);

                            if (textRange.Text.Equals("\r\n"))
                            {
                                coumnValue = "";
                                break;
                            }
                            else
                            {
                                coumnValue = textRange.Text;
                                break;
                            }
                    }
                }

                if (coumnValue.Length == 0)
                {
                    return;
                }

                setReportColumn(columnName, coumnValue);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void setReportColumn(string columnName, string columnValue)
        {
            try
            {
                if (cr.Fields.Contains(columnName)) cr.Fields[columnName].Text = columnValue;

                c1DocumentViewer.Document = cr.FixedDocumentSequence;
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void rodioButtonControl(object sender, RoutedEventArgs e, string columnName)
        {
            try
            {              
                string coumnValue = "";

                RadioButton rdo = sender as RadioButton;

                if (Convert.ToBoolean(rdo.IsChecked))
                {

                }

                setReportColumnColor(columnName, coumnValue);

                if (rdo.Name.Contains("rdoInsulationr"))
                {
                    if (rdo.Name.Contains("YES"))
                    {
                        setReportColumnColor_white("INSULATIONR_NO");
                    }
                    else
                    {
                        setReportColumnColor_white("INSULATIONR_YES");
                    }
                }
                else if (rdo.Name.Contains("rdoEquipotential"))
                {
                    if (rdo.Name.Contains("YES"))
                    {
                        setReportColumnColor_white("EQUIPOTENTIAL_NO");
                    }
                    else
                    {
                        setReportColumnColor_white("EQUIPOTENTIAL_YES");
                    }
                }
                else if (rdo.Name.Contains("rdoInsulations"))
                {
                    if (rdo.Name.Contains("YES"))
                    {
                        setReportColumnColor_white("INSULATIONS_NO");
                    }
                    else
                    {
                        setReportColumnColor_white("INSULATIONS_YES");
                    }
                }

                c1DocumentViewer.Document = cr.FixedDocumentSequence;
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void setReportColumnColor(string columnName, string columnValue)
        {
            try
            {
                if (cr.Fields.Contains(columnName)) cr.Fields[columnName].BackColor = Colors.SpringGreen;
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void setReportColumnColor_white(string columnName)
        {
            try
            {
                if (cr.Fields.Contains(columnName)) cr.Fields[columnName].BackColor = Colors.White;
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void setTexBox()
        {            
            txtDesign.Text = "B0 Sample";
            txtHwVersion.Text = "B0 Functional";            
            txtVoltage.Text = "500V";

            FlowDocument flowDoc = new FlowDocument();
            flowDoc.Blocks.Add(new Paragraph(new Run("2150VDC \nTest duration: 1sec,\nCriteria: under 1mA")));
            rtbRemark.Document = flowDoc;
        }



        #endregion

       
    }
}
