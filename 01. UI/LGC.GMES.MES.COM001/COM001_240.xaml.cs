/*************************************************************************************
 Created Date : 2018.06.06
      Creator : 오화백K
   Decription : (임시) 투입대기 재공생성
--------------------------------------------------------------------------------------
 [Change History]
  2018.06.06  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_240 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        DataTable dtTemp;
        DataTable dtLotInform = null;
        bool change_chk = true;
        string sPROD = string.Empty;
        public COM001_240()
        {
            try
            {
                InitializeComponent();
            }
            catch(Exception ex)
            {
                Util.MessageValidation(ex.Message);
            }
            

            this.Loaded += COM001_240_Loaded;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            InitGrid();
            InitCombo();
             //dtTemp = DataTableConverter.Convert(dgResult.ItemsSource);
        }
        #endregion

        #region Event
            #region LOAD EVENT
        private void COM001_240_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();

            this.Loaded -= COM001_240_Loaded;
        }
        #endregion

            #region BUTTON_EVENT
        private void btnAddRow_Click(object sender, RoutedEventArgs e)
        {

            if(cboProcessRout.SelectedIndex == 0)
            {
                Util.MessageValidation("SFU1459"); // 공정을 선택하세요.
                return;
            }
            else
            {
                if (dtTemp.Rows.Count == 0)
                {
                    DataRow dr = dtTemp.NewRow();
                    dtTemp.Rows.Add(dr);
                }

                if (dgResult.GetRowCount() == 0)
                {
                    dgResult.ItemsSource = DataTableConverter.Convert(dtTemp);
                }
                else
                {
                    DataRow dr = dtTemp.NewRow();
                    dtTemp.Rows.Add(dr);
                    dgResult.BeginNewRow();
                    dgResult.EndNewRow(true);
                }

                for (int i = 0; i < dgResult.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgResult.Rows[i].DataItem, "NO", i + 1);
                }
            }
          
          
        }

        private void btnCreat_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataSet dsINDATA = new DataSet();

                DataTable dtINDATA = dsINDATA.Tables.Add("INDATA");                
                dtINDATA.Columns.Add("PROCID", typeof(string));
                dtINDATA.Columns.Add("USERID", typeof(string));

                DataRow drINDATA = dtINDATA.NewRow();
                drINDATA["PROCID"] = cboProcessRout.SelectedValue.ToString();
                drINDATA["USERID"] = LoginInfo.USERID;
                dtINDATA.Rows.Add(drINDATA);

                DataTable dtINLOT = dsINDATA.Tables.Add("IN_LOT");
                dtINLOT.Columns.Add("LOTID", typeof(string));
                dtINLOT.Columns.Add("PRODID", typeof(string));
                dtINLOT.Columns.Add("WIPQTY", typeof(string));

             

                for (int i = 0; i < dtTemp.Rows.Count; i++)
                {
                    bool null_chk = false;

                    for (int j = 0; j < dtTemp.Columns.Count; j++)
                    {
                        if (j == 1 || j == 2 || j == 3)
                        {
                            if(dtTemp.Rows[i][j].ToString().Length == 0)
                            {
                                null_chk = true;
                            }
                        }

                    }

                    if (!null_chk)
                    {
                        DataRow dr = dtINLOT.NewRow();
                        dr["LOTID"] = Util.NVC(dtTemp.Rows[i]["LOTID"]);
                        dr["PRODID"] = Util.NVC(dtTemp.Rows[i]["PRODID"]);
                        dr["WIPQTY"] = Util.NVC(dtTemp.Rows[i]["WIPQTY"]);
                              
                        dtINLOT.Rows.Add(dr);
                    }
                    else
                    {
                        Util.MessageValidation("SFU2052"); //	입력된 항목이 없습니다.
                        return;
                    }
                }

                if(dtINLOT.Rows.Count > 0)
                {
                    new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_TEMP_N6_2D_WAIT_LOT", "INDATA,IN_LOT", null, dsINDATA);

                    Util.MessageValidation("SFU1889"); //정상 처리 되었습니다

                    btnInit_Click(null, null);

                    sPROD = "";
                }
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    change_chk = false;

                    DataTable dt = DataTableConverter.Convert(dgResult.ItemsSource);

                    int row_index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;


                    DataGridCellPresenter pre;
                    for (int j = 0; j < dgResult.Columns.Count; j++)
                    {
                        pre = dgResult.GetCell(row_index, j).Presenter as DataGridCellPresenter;

                        switch (dgResult.GetCell(row_index, j).Column.Name)
                        {
                            case "LOTID":
                            case "PRODID":
                            case "PROD_VER_CODE":
                                TextBox tb = pre.Content as TextBox;
                                tb.Text = "";
                                tb.Background = new SolidColorBrush(Colors.White);
                                break;
                            case "WIPQTY":
                                C1NumericBox num = pre.Content as C1NumericBox;
                                num.Value = 1000;
                                num.Background = new SolidColorBrush(Colors.White);
                                break;
                            default:
                                break;
                        }
                    }
                 
                    dgResult.RemoveRow(row_index);
                    dtTemp.Rows.Remove(dtTemp.Rows[row_index]);
                    //dtTemp = ((DataView)dgResult.ItemsSource).Table.Select().CopyToDataTable();  //DataTableConverter.Convert(dgResult.ItemsSource).Clone();
                    //dtTemp.Rows.Remove(dtTemp.Select("PALLETID = '" + DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "PALLETID") + "'")[0]);
                    //if (dgResult.GetRowCount() == 0)
                    //{
                    //    dtTemp = DataTableConverter.Convert(dgResult.ItemsSource);
                    //}

                    for (int i = 0; i < dgResult.GetRowCount(); i++)
                    {
                        DataTableConverter.SetValue(dgResult.Rows[i].DataItem, "NO", i + 1);
                    }
                    change_chk = true;


                }
            });
        }

        private void btnInit_Click(object sender, RoutedEventArgs e)
        {          
            DataTable dtt = dtTemp;
           
            for(int i = dtTemp.Rows.Count-1; i >= 0 ; i--)
            {
                DataGridCellPresenter pre;
                for (int j = 0; j < dgResult.Columns.Count; j++)
                {
                    pre = dgResult.GetCell(i, j).Presenter as DataGridCellPresenter;

                    switch (dgResult.GetCell(i, j).Column.Name)
                    {
                        case "LOTID":
                        case "PRODID":
                        case "PROD_VER_CODE":
                           TextBox tb = pre.Content as TextBox;
                            tb.Text = "";
                            tb.Background = new SolidColorBrush(Colors.White);
                            break;
                        case "WIPQTY":
                            C1NumericBox num = pre.Content as C1NumericBox;
                            num.Value = 1000;
                            num.Background = new SolidColorBrush(Colors.White);
                            break;
                        default:
                            break;
                    }
                }
                dtTemp.Rows.RemoveAt(i);
            }
            dgResult.ItemsSource = null;
        }

        #endregion

        #region 그외 ENVET
        private void CELL_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e == null)
                {
                    DataGridCellPresenter pre = dgResult.GetCell(dgResult.CurrentRow.Index, dgResult.CurrentColumn.Index + 1).Presenter as DataGridCellPresenter;
                    C1NumericBox next_nric = pre.Content as C1NumericBox;
                    next_nric.Focus();
                    return;
                }

                if (e.Key == Key.Enter)
                {
                    string type_name = sender.GetType().Name;
                    C1.WPF.DataGrid.DataGridCellPresenter presenter;
                    int row_idx = 0;
                    int col_idx = 0;
                    string value = string.Empty;

                    switch (type_name)
                    {
                        case "C1NumericBox":
                            C1NumericBox n = sender as C1NumericBox;
                            //StackPanel panel = n.Parent as StackPanel;
                            C1.WPF.DataGrid.DataGridCellPresenter p = n.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                            row_idx = p.Cell.Row.Index;
                            col_idx = p.Cell.Column.Index;
                            n.Background = new SolidColorBrush(Colors.YellowGreen);

                            saveToTable(row_idx, col_idx, n.Value.ToString());
                            break;
                        case "ComboBox":
                            ComboBox cbo = sender as ComboBox;
                            presenter = cbo.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                            row_idx = presenter.Cell.Row.Index;
                            col_idx = presenter.Cell.Column.Index;
                            break;
                        case "TextBox":
                            TextBox txtBox = sender as TextBox;
                            if (txtBox == null || txtBox.Text.Length == 0) return;
                            presenter = txtBox.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                            row_idx = presenter.Cell.Row.Index;
                            col_idx = presenter.Cell.Column.Index;

                            DataGridCellPresenter pre; //다음 col 포커스를 위해 다음컬럼의 content 를 담아둠

                            if (txtBox.Name == "txtLotID" || txtBox.Name == "txtProdID")
                            {
                                //lotid, 제품코드가 DB에 있는지 chk
                                //string msg = lotProdValidation((Util.GetCondition(txtBox)), txtBox.Name);
                                string msg = string.Empty; 

                                if (txtBox.Name == "txtLotID")
                                {
                                    msg = lotValidation((Util.GetCondition(txtBox)), txtBox.Name);
                                }
                                else
                                {
                                    msg = ProdValidation((Util.GetCondition(txtBox)), txtBox.Name);
                                }
                                if (msg != "")
                                {
                                    Util.MessageValidation(msg);
                                    txtBox.Focus();
                                    return;
                                }

                                if (txtBox.Name == "txtProdID")
                                {
                                    if (sPROD.Length == 0)
                                    {
                                        sPROD = txtBox.Text;
                                    }
                                    else
                                    {
                                        //if (!prodChk(Util.GetCondition(txtBox)))
                                        //{
                                        //    Util.MessageValidation("제품코드가 다릅니다."); // //제품코드가 다릅니다.
                                        //    txtBox.Focus();
                                        //    return;
                                        //}
                                    }

                                    saveToTable(row_idx, col_idx, txtBox.Text);
                                    saveToGrid(row_idx, col_idx, txtBox.Text);

                                    txtBox.Background = new SolidColorBrush(Colors.YellowGreen);
                                }
                                else if (txtBox.Name == "txtLotID")
                                {
                                    if (dtTemp.Rows.Count > 0)
                                    {
                                        DataRow[] drs = dtTemp.Select("LOTID ='" + Util.GetCondition(txtBox) + "'");

                                        if (drs.Length > 0)
                                        {
                                            Util.MessageValidation("SFU1196"); //Lot 이 이미 추가되었습니다.
                                            txtBox.Focus();
                                            return;
                                        }
                                    }

                                    //LOT CHK
                                    if(!LotCHK(Util.GetCondition(txtBox)))
                                    {
                                        return;
                                    }

                                    //LOT 정보가 있는지 확인                                     
                                    LotInform(Util.GetCondition(txtBox));

                                    string col_name = string.Empty;
                                    string col_value = string.Empty;
                                    int Coumn_index = 0;
                                    bool textbox_YN = true;
                                    bool save_YN = false;
                                    //DataGridCellPresenter pre_biz;

                                    if (dtLotInform != null && dtLotInform.Rows.Count > 0) // lot의 정보가 있으면 그 정보를 그리드에 set.
                                    {
                                        for (int i = 0; i < dtLotInform.Columns.Count; i++)
                                        {
                                            col_name = dtLotInform.Columns[i].ColumnName.ToString();
                                            col_value = dtLotInform.Rows[0][col_name].ToString();


                                            switch (col_name)
                                            {
                                                case "LOTID":
                                                    Coumn_index = 0;
                                                    save_YN = true;
                                                    textbox_YN = true;
                                                    break;
                                                case "PRODID":
                                                    Coumn_index = 1;
                                                    save_YN = true;
                                                    textbox_YN = true;
                                                    if(sPROD.Length == 0)
                                                    {
                                                        sPROD = col_value;
                                                    }

                                                    //if (!prodChk(col_value))
                                                    //{
                                                    //    pre = dgResult.GetCell(row_idx, dgResult.Columns["LOTID"].Index).Presenter as DataGridCellPresenter;
                                                    //    TextBox cng_txtBox = pre.Content as TextBox;
                                                    //    cng_txtBox.Text = "";
                                                    //    cng_txtBox.Background = new SolidColorBrush(Colors.White);

                                                    //    Util.MessageValidation("제품코드가 다릅니다."); // //제품코드가 다릅니다.
                                                    //    txtBox.Focus();
                                                    //    return;
                                                    //}

                                                    break;
                                                case "WIPQTY":
                                                    Coumn_index = 6;
                                                    save_YN = true;
                                                    textbox_YN = true;
                                                    textbox_YN = false;
                                                    col_value = Math.Truncate(Convert.ToDouble(col_value)).ToString();
                                                    break;

                                                default:
                                                    save_YN = false;
                                                    break;
                                            }

                                            if (save_YN)
                                            {
                                                saveToTable(row_idx, Coumn_index, col_value.ToString());
                                                saveToGrid(row_idx, Coumn_index, col_value.ToString());

                                                if (textbox_YN)
                                                {
                                                    pre = dgResult.GetCell(row_idx, Coumn_index).Presenter as DataGridCellPresenter;
                                                    TextBox cng_txtBox = pre.Content as TextBox;
                                                    cng_txtBox.Text = col_value;
                                                    cng_txtBox.Background = new SolidColorBrush(Colors.YellowGreen);
                                                }
                                             
                                                else
                                                {
                                                    pre = dgResult.GetCell(row_idx, Coumn_index).Presenter as DataGridCellPresenter;
                                                    C1NumericBox cng_txtBox = pre.Content as C1NumericBox;
                                                    cng_txtBox.Value = Convert.ToInt32(col_value);
                                                    cng_txtBox.Background = new SolidColorBrush(Colors.YellowGreen);
                                                }
                                            }
                                        }
                                    }
                                    else // 없으면 다음 컬럼(prodid)입력 준비
                                    {
                                        saveToTable(row_idx, col_idx, txtBox.Text);
                                        saveToGrid(row_idx, col_idx, txtBox.Text);

                                        txtBox.Background = new SolidColorBrush(Colors.YellowGreen);

                                        pre = dgResult.GetCell(row_idx, col_idx + 1).Presenter as DataGridCellPresenter;
                                        TextBox next_txtBox = pre.Content as TextBox;
                                        next_txtBox.Focus();
                                    }
                                }
                            }
                           break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool LotCHK(string sLotID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = sLotID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_MOVE_LOT", "RQSTDT", "RSLTDT", RQSTDT);

                if(dtResult.Rows.Count > 0)
                {
                    dtResult.Rows[0]["RESULT"].ToString();
                }

                return true;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void LotInform(string sLotID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = sLotID;
                RQSTDT.Rows.Add(dr);

                dtLotInform = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_RECEIVE_MTRL_FOR_NJ", "RQSTDT", "RSLTDT", RQSTDT);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool prodChk(string prodid)
        {
            bool chk = false;
            int data_cnt = 0;

            for(int i = 0; i < dtTemp.Rows.Count; i++)
            {
                if (dtTemp.Rows[i]["PRODID"] != null && dtTemp.Rows[i]["PRODID"].ToString().Length > 0)
                {
                    if (dtTemp.Rows[i]["PRODID"].ToString() != prodid)
                    {
                        chk = false;
                        data_cnt++;
                    }

                }
            }

            if(data_cnt == 0)
            {
                chk = true;
            }



            return chk;
        }

        private string lotProdValidation(string lotProd, string textboxname)
        {
            try
            {
                string msg = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                if(textboxname == "txtLotID")
                {
                    dr["LOTID"] = lotProd;
                    dr["PRODID"] = null;

                    msg = "SFU2014"; //		해당 LOT이 이미 존재합니다.
                }
                else if(textboxname == "txtProdID")
                {
                    dr["LOTID"] = null;
                    dr["PRODID"] = lotProd;

                    msg = "SFU4211"; // 	등록된 제품 ID 가 아닙니다.
                }
                
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_PROD_SEARCH", "RQSTDT", "RSLTDT", RQSTDT);

                if(textboxname == "txtLotID")
                {
                    if (dtResult.Rows.Count == 0)
                    {
                        if(lotProd.Length != 10)
                        {

                            msg = "SFU4229"; // 조립LOT 정보는 10자리 입니다.
                        }
                        else
                        {
                            msg = "";
                        }                        
                    }
                }
                else if(textboxname == "txtProdID")
                {
                    if (dtResult.Rows.Count > 0)
                    {
                        msg = "";
                    }
                }               

                return msg;
            }
            catch(Exception ex)
            {
                throw ex;
            }           
        }

        private string lotValidation(string lot, string textboxname)
        {
            try
            {
                string msg = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                
                dr["LOTID"] = lot;
                dr["PRODID"] = null;

                msg = "SFU2014"; //		해당 LOT이 이미 존재합니다.

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_PROD_SEARCH", "RQSTDT", "RSLTDT", RQSTDT);

                if (textboxname == "txtLotID")
                {
                    if (dtResult.Rows.Count == 0)
                    {
                        if (lot.Length != 10)
                        {

                            msg = "SFU4229"; // 조립LOT 정보는 10자리 입니다.
                        }
                        else
                        {
                            msg = "";
                        }
                    }
                }             

                return msg;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string ProdValidation(string Prod, string textboxname)
        {
            try
            {
                string msg = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";               
                RQSTDT.Columns.Add("PRODID", typeof(string));

                DataRow dr = RQSTDT.NewRow();                   
                dr["PRODID"] = Prod;

                msg = "SFU4211"; // 	등록된 제품 ID 가 아닙니다.               

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROD", "RQSTDT", "RSLTDT", RQSTDT);
                
                if (dtResult.Rows.Count > 0)
                {
                    msg = "";
                }               

                return msg;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void saveToTable(int row_idx, int col_idx, string txt)
        {
            dtTemp.Rows[row_idx][col_idx] = txt;

            //dgResult.ItemsSource = DataTableConverter.Convert(dtTemp);

           // DataTable dt = DataTableConverter.Convert(dgResult.ItemsSource);
        }

        private void saveToGrid(int row_idx, int col_idx, string txt)
        {
            //dgResult.GetCell(row_idx, col_idx).Value = txt;

            //DataTable dt = DataTableConverter.Convert(dgResult.ItemsSource);
        }

 

        private void dgResult_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            int _row = 0;
            int _col = 0;
            string value = string.Empty;
            string old_value = string.Empty;

            if (e.Cell.Value == null)
            {
                return;
            }

            if (e.Cell.Column.Name == "LOTTYPE" || e.Cell.Column.Name == "MKT_TYPE_CODE")
            {
                _row = e.Cell.Row.Index;
                _col = e.Cell.Column.Index;
                value = e.Cell.Value.ToString();
                e.Cell.Presenter.Background = new SolidColorBrush(Colors.YellowGreen);

                old_value = (dtTemp.Rows[_row] as DataRow)[e.Cell.Column.Name].ToString();

                if (value == old_value)
                {
                    return;
                }

                dtTemp.Rows[_row][e.Cell.Column.Name] = value;
            }
        }

        private void btnSearch_HIST_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("FROMDATE", typeof(string));
                inDataTable.Columns.Add("TODATE", typeof(string));

                DataRow searchCondition = inDataTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["EQSGID"] = Util.GetCondition(cboEquipmentSegment_HIST) == "" ? null : Util.GetCondition(cboEquipmentSegment_HIST);
                searchCondition["PROCID"] = null; // Util.GetCondition(cboProcessRout_HIST) == "" ? null : Util.GetCondition(cboProcessRout_HIST);
                searchCondition["FROMDATE"] = Util.GetCondition(dtpDateFrom);
                searchCondition["TODATE"] = Util.GetCondition(dtpDateTo);
                inDataTable.Rows.Add(searchCondition);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPCREATE_HIST", "INDATA", "OUTDATA", inDataTable);

                dgResult_HIST.ItemsSource = null;

                if (dtResult.Rows.Count != 0)
                {
                    Util.GridSetData(dgResult_HIST, dtResult, FrameOperation);
                   
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

  

        private void txtChange_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (change_chk)
            {
                setChange(sender);
            }
        }

        private void txtWipQty_ValueChanged(object sender, PropertyChangedEventArgs<double> e)
        {
            if (change_chk)
            {
                setChange_NUMERIC(sender);
            }
        }

        #endregion

        #endregion

        #region Mehod
        private void InitGrid()
        {
            dtTemp = new DataTable();
            dtTemp.Columns.Add("NO", typeof(string));
            dtTemp.Columns.Add("LOTID", typeof(string));
            dtTemp.Columns.Add("PRODID", typeof(string));
            dtTemp.Columns.Add("WIPQTY", typeof(Int32));
            dtTemp.Columns.Add("Delete", typeof(string));
            DataRow dr = dtTemp.NewRow();
            dtTemp.Rows.Add(dr);
            //dgResult.ItemsSource = DataTableConverter.Convert(dtTable);


        
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            C1ComboBox cboSHOPID = new C1ComboBox();
            cboSHOPID.SelectedValue = LoginInfo.CFG_SHOP_ID;
            C1ComboBox cboAreaByAreaType = new C1ComboBox();
            cboAreaByAreaType.SelectedValue = LoginInfo.CFG_AREA_ID;
            C1ComboBox cboAREA_TYPE_CODE = new C1ComboBox();
            cboAREA_TYPE_CODE.SelectedValue = Area_Type.PACK;
            C1ComboBox cboProductModel = new C1ComboBox();
            cboProductModel.SelectedValue = "";
            C1ComboBox cboPrdtClass = new C1ComboBox();
            cboPrdtClass.SelectedValue = "";
            C1ComboBox cboEquipmentSegment = new C1ComboBox();
            cboEquipmentSegment.SelectedValue = LoginInfo.CFG_EQSG_ID;

            #region 재공생성 TAB
            //공정
            C1ComboBox[] cboProcessRoutParent = { cboAreaByAreaType };
            _combo.SetCombo(cboProcessRout, CommonCombo.ComboStatus.SELECT, cbParent: cboProcessRoutParent);

            cboProcessRout.SelectedIndex = 0;

            #endregion

            #region 재공생성이력 TAB
            //라인            
            C1ComboBox[] cboEquipmentSegmentParent_HIST = { cboAreaByAreaType };
            C1ComboBox[] cboEquipmentSegmentChild_HIST = { cboProcessRout };
            _combo.SetCombo(cboEquipmentSegment_HIST, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentSegmentParent_HIST, cbChild: cboEquipmentSegmentChild_HIST, sCase: "EQUIPMENTSEGMENT");

            //공정
            C1ComboBox[] cboProcessRoutParent_HIST = { cboAreaByAreaType, cboEquipmentSegment };
            _combo.SetCombo(cboProcessRout_HIST, CommonCombo.ComboStatus.SELECT, cbParent: cboProcessRoutParent_HIST, sCase: "PROCESSROUT");
            #endregion
        }
       private void setChange(object sender)
        {
            int row_index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as TextBox).Parent)).Row.Index;
            int col_index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as TextBox).Parent)).Column.Index;

            if (dtTemp == null || dtTemp.Rows.Count == 0 || dtTemp.Rows[row_index][col_index].ToString().Length == 0)
            {
                return;
            }

            //저장된 값하고 비교
            compareDataTable(row_index, col_index, sender);
        }

        private void setChange_NUMERIC(object sender)
        {
            int row_index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as C1NumericBox).Parent)).Row.Index;
            int col_index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as C1NumericBox).Parent)).Column.Index;

            if (dtTemp == null || dtTemp.Rows.Count == 0 || dtTemp.Rows[row_index][col_index].ToString().Length == 0)
            {
                return;
            }

            //저장된 값하고 비교
            compareDataTable_NUMERIC(row_index, col_index, sender);
        }

        private void compareDataTable(int row, int col, object obj)
        {
            TextBox txt = obj as TextBox;

            if (txt.Name == "txtVldDate")
            {
                char[] cha = txt.Text.ToCharArray();

                bool vali = false;
                for (int i = 0; i < cha.Length; i++)
                {
                    int idx = Convert.ToChar(cha[i]);
                    //숫자만 입력되도록 필터링
                    if (!(idx >= 48 && idx <= 57))
                    {
                        vali = true;
                    }
                }

                if (vali)
                {
                    txt.Text = dtTemp.Rows[row][col].ToString();
                    return;
                }
            }

            if (txt.Text != dtTemp.Rows[row][col].ToString())
            {
                dtTemp.Rows[row][col] = txt.Text;
                txt.Background = new SolidColorBrush(Colors.White);
            }
        }

        private void compareDataTable_NUMERIC(int row, int col, object obj)
        {
            C1NumericBox NUM = obj as C1NumericBox;


            if (NUM.Value.ToString() != dtTemp.Rows[row][col].ToString())
            {
                dtTemp.Rows[row][col] = NUM.Value.ToString();
                NUM.Background = new SolidColorBrush(Colors.White);
            }
        }
        #endregion

        private void dgResult_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
                int row_idx = e.Cell.Row.Index;
                int col_idx = e.Cell.Column.Index;
                string col_name = e.Cell.Column.Name.ToString();

                string value = dgResult.GetCell(row_idx, col_idx).Value == null ? "" : dgResult.GetCell(row_idx, col_idx).Value.ToString();

                if (e.Cell.Presenter == null)
                {
                    return;
                }


            }
            catch (Exception ex)
            {
                Util.MessageValidation(ex.ToString());
            }
        }

        private void cboProcessRout_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
          
            dgResult.ItemsSource = null;
        }
    }
}
