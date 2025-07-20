/*************************************************************************************
 Created Date : 2018.03.12
      Creator : 
   Decription : 원형 CELL 등록
--------------------------------------------------------------------------------------
 [Change History]
  2018.03.12  DEVELOPER : Initial Created.
  2019.10.21  이상준    : GB/T 발행기준 변경에 따른 Cell Prefix Validation 수정 및 Prefix, Cell ID Box 사이즈 변경 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using System.Configuration;
using C1.WPF.Excel;

namespace LGC.GMES.MES.CMM001
{
    public partial class CMM_ASSY_CIRCULAR_INPUT_CELL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private readonly Util _util = new Util();

        private string _lotId = string.Empty;
        private string _cstId = string.Empty;
        private string _eqptId = string.Empty;
        private string _outLotId = string.Empty;
        private string _eqptLot = string.Empty;
        private string _completeProd = string.Empty;
        //
        int addRows;
        private int iCell_Len = 0;                          // Cell ID 자릿수
        private int iPrefix_Len = 0;                        // Cell Prefix 자릿수

        public CMM_ASSY_CIRCULAR_INPUT_CELL()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }

            _lotId = Util.NVC(tmps[0]);
            _cstId = Util.NVC(tmps[1]);
            _eqptId = Util.NVC(tmps[2]);
            _outLotId = Util.NVC(tmps[3]);
            _eqptLot = Util.NVC(tmps[4]);
            _completeProd = Util.NVC(tmps[5]);

            //_outLotId = null;
            _eqptLot = null;

            txtLOTID.Text = _lotId;
            txtTRAYID.Text = _cstId;

            GetSubLotPreFix(_lotId);

            //switch (LoginInfo.CFG_SHOP_ID)
            //{
            //    case "A010":
            //        txtPREFIX.Text = "LO" + _lotId.Substring(3, 7);
            //        break;
            //    case "G182":
            //        txtPREFIX.Text = "LN" + _lotId.Substring(3, 7);
            //        break;
            //    default:
            //        txtPREFIX.Text = "LX" + _lotId.Substring(3, 7);
            //        break;
            //}

            dgList.ItemsSource = DataTableConverter.Convert(Util.MakeDataTable(dgList, true)) ;
        }
        #endregion

        #region Event

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            dgList.EndEdit();
            dgList.EndEditRow(true);

            if (!IsValid())return;
            SaveData();

        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (Math.Abs(rowCount.Value) > 0)
            {
                if (rowCount.Value + dgList.GetRowCount() > 256)
                {
                    // 최대 ROW수는 256입니다.
                    Util.MessageValidation("SFU4568");
                    return;
                }
            }

            Util.DataGridRowAdd(dgList, (int)rowCount.Value);
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Util.DataGridRowDelete(dgList, (int)rowCount.Value);
        }

        private void txtCELLID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtCELLID.Text)) return;

                if (e.Key == Key.Enter)
                {

                    DataTable dt = ((DataView)dgList.ItemsSource).Table;
                    DataRow dr = dt.NewRow();
                    dr["SUBLOTID"] = txtCELLID.Text.Trim();
                    dt.Rows.Add(dr);
                    dgList.ItemsSource = DataTableConverter.Convert(dt);

                    dgList.ScrollIntoView(dt.Rows.Count, dgList.Columns["SUBLOTID"].Index);

                    //DataTable dt = Util.MakeDataTable(dgSMSTargetPhoneList, true);
                    //DataRow dr = dt.NewRow();
                    //dr["CHARGE_USER_PHONE_NO"] = txtPhoneNo.Text;
                    //dr["EQPTID"] = cboEquipment.SelectedValue;
                    //dr["SMS_GR_ID"] = _smsGroupCode;
                    //dr["SEND_USER_FLAG"] = "N";

                    //dt.Rows.Add(dr);
                    //dgSMSTargetPhoneList.ItemsSource = DataTableConverter.Convert(dt);

                    /*
                    if (dgList == null || dgList.Rows.Count < 1 || dgList.SelectedIndex < 0) return;

                    if (dgList.GetRowCount() != dgList.SelectedIndex)
                    {
                        DataTableConverter.SetValue(dgList.Rows[dgList.CurrentRow.Index].DataItem, "SUBLOTID", txtCELLID.Text.Trim());

                        if (dgList.GetRowCount() - 1 > dgList.SelectedIndex)
                        {
                            dgList.SelectedIndex = dgList.SelectedIndex + 1;
                            dgList.CurrentCell = dgList.GetCell(dgList.SelectedIndex, 0);
                            dgList.ScrollIntoView(dgList.SelectedIndex, dgList.Columns["SUBLOTID"].Index);
                        }
                    }
                    */



                    txtCELLID.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgList_BeginningNewRow(object sender, DataGridBeginningNewRowEventArgs e)
        {
            e.Item.SetValue("SUBLOTID", string.Empty);
            e.Item.SetValue("CSTSLOT", string.Empty);
        }


        private void dgList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Convert.ToString(e.Cell.Column.Name) == "CHK_MESSAGE")
                    {
                        if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CHK_MESSAGE"))))
                        {
                            var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Red");
                            if (convertFromString != null)
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                        }
                    }
                }
            }));
        }

        private void dgList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }


        /// <summary>
        /// 엑셀 업로드 양식 다운로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDownLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog od = new SaveFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    od.InitialDirectory = @"\\Client\C$";
                }
                od.Filter = "Excel Files (.xlsx)|*.xlsx";
                od.FileName = "CIRCULAR_CELL_Upload_Sample.xlsx";

                if (od.ShowDialog() == true)
                {
                    C1XLBook c1XLBook1 = new C1XLBook();
                    XLSheet sheet = c1XLBook1.Sheets[0];
                    XLStyle styel = new XLStyle(c1XLBook1);
                    styel.AlignHorz = XLAlignHorzEnum.Center;

                   
                        sheet[0, 0].Value = "CELLID";
                        sheet[0, 1].Value = "Tray 위치";


                    
                        sheet[0, 0].Style = sheet[0, 1].Style =styel;
                        
                        
                        sheet.Columns[0].Width = 3000;
                        sheet.Columns[1].Width =1500;
                    
                    

                    c1XLBook1.Save(od.FileName);

                    //   if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] != "SBC")
                    System.Diagnostics.Process.Start(od.FileName);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region Excel 업로드
        private void btnUpLoad_Click(object sender, RoutedEventArgs e)
        {
            GetExcel();
        }

        void GetExcel()
        {
            try
            {
                Microsoft.Win32.OpenFileDialog fd = new Microsoft.Win32.OpenFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }

                fd.Filter = "Excel Files (.xlsx)|*.xlsx";
                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        LoadExcel(stream, (int)0);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        void LoadExcel(Stream excelFileStream, int sheetNo)
        {
            try
            {
                excelFileStream.Seek(0, SeekOrigin.Begin);
                C1XLBook book = new C1XLBook();
                book.Load(excelFileStream, FileFormat.OpenXml);
                XLSheet sheet = book.Sheets[sheetNo];

                if (sheet == null)
                {
                    //업로드한 엑셀파일의 데이타가 잘못되었습니다. 확인 후 다시 처리하여 주십시오.
                    Util.MessageValidation("9017");
                    return;
                }

                // 해더 제외
                DataTable dt = DataTableConverter.Convert(dgList.ItemsSource).Clone();

                for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                {
                    DataRow dr = dt.NewRow();

                    if (sheet.GetCell(rowInx, 0) == null)
                        dr["SUBLOTID"] = "";
                    else
                        dr["SUBLOTID"] = Util.NVC(sheet.GetCell(rowInx, 0).Text);

                    if (sheet.GetCell(rowInx, 1) == null)
                        dr["CSTSLOT"] = "";
                    else
                        dr["CSTSLOT"] = Util.NVC(sheet.GetCell(rowInx, 1).Text);
                    
                    dt.Rows.Add(dr);
                }

                dt.AcceptChanges();
                Util.GridSetData(dgList, dt, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        #endregion

        #region Mehod


        private void GetSubLotPreFix(string lotId)
        {
            try
            {
                const string bizRuleName = "BR_PRD_GET_SUBLOT_PRFX_WS_CELLID";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROD_LOTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["PROD_LOTID"] = lotId;
                inTable.Rows.Add(dr);

                DataTable resultTable = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                if (CommonVerify.HasTableRow(resultTable))
                {
                    txtPREFIX.Text = resultTable.Rows[0]["CELL_ID_PRFX"].ToString();
                    iCell_Len = Convert.ToInt32(resultTable.Rows[0]["CELLID_LEN"].ToString());
                    iPrefix_Len = Convert.ToInt32(resultTable.Rows[0]["PRFX_LEN"].ToString());
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private bool IsValid()
        {
            if (dgList.Rows.Count == 0)
            {
                //Util.Alert("SFU1651");  //처리 할 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            DataTable dt = ((DataView)dgList.ItemsSource).Table;
            var subLotInfo =
                from t in dt.AsEnumerable()
                group t by t.Field<string>("SUBLOTID")  into g
                select new { SubLotId = g.Key, SubLotCount = g.Count() };

            foreach (var item in subLotInfo)
            {
                if (item.SubLotCount > 1)
                {
                    //중복된 CellID가 존재합니다.
                    Util.MessageValidation("SFU3681");
                    return false;
                }
            }

            var cstsLotInfo =
                from t in dt.AsEnumerable()
                group t by t.Field<string>("CSTSLOT") into g
                select new { CstsLot = g.Key, CstsLotCount = g.Count() };

            foreach (var item in cstsLotInfo)
            {
                if (item.CstsLotCount > 1)
                {
                    //중복된 Tray 정보가 존재합니다.
                    Util.MessageValidation("SFU3682");
                    return false;
                }
            }

            var query = (from t in dt.AsEnumerable()
                select t).ToList();

            if (query.Any())
            {
                foreach (var item in query)
                {
                    if (string.IsNullOrEmpty(item["SUBLOTID"]?.GetString()))
                    {
                        Util.MessageValidation("SFU3679"); //Cell ID가 등록되지 않은 ROW가 있습니다
                        return false;
                    }

                    if (item["SUBLOTID"].GetString().Trim().Length != iCell_Len)
                    {
                        object[] parameters1 = new object[1];
                        parameters1[0] = iCell_Len.ToString();
                        //CELLID는 XX자리여야 합니다. 
                        Util.MessageValidation("SFU8115", parameters1);
                        return false;
                    }

                    if (_util.Left(item["SUBLOTID"].GetString().Trim(), iPrefix_Len) != txtPREFIX.Text)
                    {
                        object[] parameters = new object[3];
                        parameters[0] = item["SUBLOTID"].GetString();
                        parameters[1] = txtPREFIX.Text;
                        parameters[2] = iPrefix_Len.ToString();
                        //입력한 Cell ID 앞 XX 자리가 Prifix 정보와 일치하지 않습니다.   
                        Util.MessageValidation("SFU8112", parameters);
                        return false;
                    }

                    //if (!CheckNumber(item["SUBLOTID"].GetString()))
                    if (!CheckNumber(_util.Right(item["SUBLOTID"].GetString().Trim(), 6)))
                    {
                        //입력한 Cell ID[%1] 뒤 6자리는 숫자만 입력 가능합니다.
                        Util.MessageValidation("SFU4916", item["SUBLOTID"].GetString().Trim());
                        return false;
                    }

                    if (string.IsNullOrEmpty(item["CSTSLOT"]?.GetString()))
                    {
                        //CSTSLOT가 등록되지 않은 ROW가 있습니다.
                        Util.MessageValidation("SFU3680");
                        return false;
                    }

                    if (Convert.ToInt16(item["CSTSLOT"].GetString()) == 0)
                    {
                        //Tray 정보는 0 이상이어야 합니다.
                        Util.MessageValidation("SFU3683");
                        return false;
                    }

                    if (Convert.ToInt16(item["CSTSLOT"].GetString()) > 256)
                    {
                        //Tray 정보는 256보다 작아야 합니다.
                        Util.MessageValidation("SFU4567");
                        return false;
                    }


                }
            }


            return true;
        }
        #region #. CheckNumber

        /// <summary>
        /// 숫자체크
        /// </summary>
        /// <param name="letter">문자
        /// <returns></returns>
         public static bool CheckNumber(string letter)

        {
            Int32 numchk = 0;
            bool isCheck = int.TryParse(letter, out numchk);
            return isCheck;
        }

        #endregion

      


        private void SaveData()
        {
            try
            {
                Util.MessageConfirm("SFU1241", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        //const string bizRuleName = "BR_PRD_REG_PUT_SUBLOT_IN_CST_WS_CELLID";
                        string bizRuleName = _completeProd == "Y"
                            ? "BR_PRD_REG_PUT_SUBLOT_IN_CST_WS_CELLID_UI"
                            : "BR_PRD_REG_PUT_SUBLOT_IN_CST_WS_CELLID";

                        DataSet inDataSet = GetBR_PRD_REG_START_OUT_LOT_WSS();
                        DataTable inDataTable = inDataSet.Tables["IN_EQP"];
                        DataRow drow = inDataTable.NewRow();
                        drow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        drow["IFMODE"] = IFMODE.IFMODE_OFF;
                        drow["EQPTID"] = _eqptId;
                        drow["USERID"] = LoginInfo.USERID;
                        drow["PROD_LOTID"] = _lotId;
                        drow["EQPT_LOTID"] = _eqptLot;
                        drow["OUT_LOTID"] = _outLotId;
                        drow["CSTID"] = _cstId;
                        inDataSet.Tables["IN_EQP"].Rows.Add(drow);

                        DataTable inCST = inDataSet.Tables["IN_CST"];

                        for (int row = 0; row < dgList.Rows.Count - dgList.BottomRows.Count; row++)
                        {
                            if (!string.IsNullOrEmpty(DataTableConverter.GetValue(dgList.Rows[row].DataItem, "SUBLOTID").ToString()) || string.IsNullOrEmpty(DataTableConverter.GetValue(dgList.Rows[row].DataItem, "CSTSLOT").ToString()))
                            {
                                drow = inCST.NewRow();
                                drow["SUBLOTID"] = DataTableConverter.GetValue(dgList.Rows[row].DataItem, "SUBLOTID").ToString().Trim();
                                drow["CSTSLOT"] = DataTableConverter.GetValue(dgList.Rows[row].DataItem, "CSTSLOT").ToString().Trim();
                                inDataSet.Tables["IN_CST"].Rows.Add(drow);
                            }
                        }

                        new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_CST", "OUT_CST", (bizResult, ex) =>
                        {
                            if (ex != null)
                            {
                                Util.MessageException(ex);
                                return;
                            }

                            if (CommonVerify.HasTableInDataSet(bizResult))
                            {
                                DataTable dtResult = bizResult.Tables["OUT_CST"];

                                if (CommonVerify.HasTableRow(dtResult))
                                {
                                    var query = (from t in dtResult.AsEnumerable()
                                        where t.Field<string>("CHK_RESULT") == "2"
                                        select t).ToList();

                                    if (query.Any())
                                    {
                                        DataTable dt = ((DataView)dgList.ItemsSource).Table;

                                        for (int i = 0; i < dt.Rows.Count; i++)
                                        {
                                            foreach (var item in query)
                                            {
                                                if (dt.Rows[i]["SUBLOTID"].GetString() == item["SUBLOTID"].GetString())
                                                {
                                                    dt.Rows[i]["CHK_MESSAGE"] = item["CHK_MESSAGE"].GetString();
                                                }
                                            }
                                        }
                                        dgList.ItemsSource = DataTableConverter.Convert(dt);
                                    }
                                    else
                                    {
                                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                                        this.DialogResult = MessageBoxResult.OK;
                                    }
                                }
                                
                            }

                        }, inDataSet);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            
        }
        public DataSet GetBR_PRD_REG_START_OUT_LOT_WSS()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("EQPT_LOTID", typeof(string));
            inDataTable.Columns.Add("OUT_LOTID", typeof(string));
            inDataTable.Columns.Add("CSTID", typeof(string));

            DataTable inInputLot = indataSet.Tables.Add("IN_CST");
            inInputLot.Columns.Add("SUBLOTID", typeof(string));
            inInputLot.Columns.Add("CSTSLOT", typeof(string));
            //inInputLot.Columns.Add("EL_PRE_WEIGHT", typeof(string));
            //inInputLot.Columns.Add("EL_AFTER_WEIGHT", typeof(string));
            //inInputLot.Columns.Add("EL_WEIGHT", typeof(string));
            //inInputLot.Columns.Add("IROCV", typeof(string));

            return indataSet;
        }




        #endregion


    }
}
#endregion