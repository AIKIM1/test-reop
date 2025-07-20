/*************************************************************************************
 Created Date : 2022.10.18
      Creator : 김용준
   Decription : 전지 5MEGA-GMES 구축 - 포장 HOLD 관리 - 보류재고 등록 팝업
--------------------------------------------------------------------------------------
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.ComponentModel;
using System.Threading;
using System.Linq;
using C1.WPF.DataGrid.Summaries;
using System.Windows.Media;
using static LGC.GMES.MES.CMM001.Class.CommonCombo;
using LGC.GMES.MES.CMM001.Popup;
using System.Configuration;
using Microsoft.Win32;
using System.IO;
using C1.WPF.Excel;

namespace LGC.GMES.MES.BOX001
{
    /// </summary>
    public partial class BOX001_334_NCR_HOLD : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        string _holdTrgtCode = string.Empty;

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
		
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public BOX001_334_NCR_HOLD()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitControl();
            InitCombo();
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            String _Pgubun = Util.NVC(tmps[1]);

            if(_Pgubun == "EXCEL")
            {
                btnDownLoad.Visibility = Visibility.Visible;
                btnUpLoad.Visibility = Visibility.Visible;              
            }
            else
            {
                DataTable dtInfo = (DataTable)tmps[0];
                btnDownLoad.Visibility = Visibility.Collapsed;
                btnUpLoad.Visibility = Visibility.Collapsed;

                if (dtInfo == null)
                {
                    dtInfo = new DataTable();

                    for (int i = 0; i < dgHold.Columns.Count; i++)
                    {
                        dtInfo.Columns.Add(dgHold.Columns[i].Name);
                    }
                }
                else

                Util.GridSetData(dgHold, dtInfo, FrameOperation);                
            }
        }

        private void dgHold_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
					
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }       
        #endregion

        #region Validation
        private void dgHold_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            if (e.Cell.Column.Name == "HOLD_REG_QTY")
            {
                string hold_req_qty = Util.NVC(dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["HOLD_REG_QTY"].Index).Value);
                int iHold_req_qty;

                if (!string.IsNullOrWhiteSpace(hold_req_qty) && !int.TryParse(hold_req_qty, out iHold_req_qty))
                {
                    //SFU3435	숫자만 입력해주세요
                    Util.MessageInfo("SFU3435");
                }
            }
        }
        #endregion

        #region Hold 리스트 추가/제거
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
                od.FileName = "Ncr_Hold_Lot_Upload_Sample.xlsx";

                if (od.ShowDialog() == true)
                {
                    C1XLBook c1XLBook1 = new C1XLBook();
                    XLSheet sheet = c1XLBook1.Sheets[0];
                    XLStyle styel = new XLStyle(c1XLBook1);
                    styel.AlignHorz = XLAlignHorzEnum.Center;

                    sheet[0, 0].Value = "Hold Group ID";                    
                    sheet[1, 0].Value = "HG-A120191113-1";
                    sheet[2, 0].Value = "HG-A120191114-1";                                      
                            

                    sheet.Columns[0].Width = sheet.Columns[1].Width = 1500;

                    sheet[0, 0].Style  = styel;
                    sheet[0, 0].Style.BackColor = Color.FromArgb(255, 255, 0, 0);

                    c1XLBook1.Save(od.FileName);
					
                    System.Diagnostics.Process.Start(od.FileName);

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 엑셀 업로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtInfo = DataTableConverter.Convert(dgHold.ItemsSource);

                dtInfo.Clear();

                OpenFileDialog fd = new OpenFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }

                fd.Filter = "Excel Files (.xlsx)|*.xlsx";

                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        C1XLBook book = new C1XLBook();
                        book.Load(stream, FileFormat.OpenXml);
                        XLSheet sheet = book.Sheets[0];

                        DataTable dataTable = new DataTable();
                        dataTable.Columns.Add("HOLD_GR_ID", typeof(string));

                        if (sheet.GetCell(0, 0).Text != "Hold Group ID"
                            )
                        {
                            //SFU4424	형식에 맞는 EXCEL파일을 선택해 주세요.
                            Util.MessageValidation("SFU4424");
                            return;
                        }

                        for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                        {
                            // Hold ID;
                            if (sheet.GetCell(rowInx, 0) == null)
                                return;

                            string hold_ID = Util.NVC(sheet.GetCell(rowInx, 0).Text);
                            DataRow dataRow = dataTable.NewRow();
                            dataRow["HOLD_GR_ID"] = hold_ID;
                            dataTable.Rows.Add(dataRow);
                        }                       

                        if (dataTable.Rows.Count > 0)
                            dataTable = dataTable.DefaultView.ToTable(true);

                        Util.GridSetData(dgHold, dataTable, FrameOperation);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }       
       
        #endregion

        #region 저장/닫기 버튼 이벤트

        /// <summary>
        /// HOLD 등록
        /// BIZ : BR_PRD_REG_ASSY_HOLD
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            DataTable dtInfo = DataTableConverter.Convert(dgHold.ItemsSource);
            if (dtInfo.Rows.Count < 1)
            {
                //SFU3552	저장 할 DATA가 없습니다.	
                Util.MessageValidation("SFU3552");
                return;
            }

            if (dtInfo.AsEnumerable().Where(c => string.IsNullOrWhiteSpace(c.Field<string>("HOLD_GR_ID"))).ToList().Count > 0)
            {
                //SFU4351		미입력된 항목이 존재합니다.	
                Util.MessageValidation("SFU4351");
                return;
            }

            //SFU8130 보류재고로 등록하시겠습니까?
            Util.MessageConfirm("SFU8130", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Save();
                    this.DialogResult = MessageBoxResult.OK;
                    this.Close();
                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }
        #endregion

        #region Biz

        /// <summary>
        /// 보류재고 등록 
        /// </summary>
        private void Save()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable RQSTDT = new DataTable("INDATA");               

                RQSTDT.Columns.Add("HOLD_GR_ID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));                

                DataRowView rowview = null;

                foreach (C1.WPF.DataGrid.DataGridRow row in dgHold.Rows)
                {
                    rowview = row.DataItem as DataRowView;

                   if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "HOLD_GR_ID")) != "")
                   {
                        DataRow dr = RQSTDT.NewRow();
                        dr["HOLD_GR_ID"] = DataTableConverter.GetValue(row.DataItem, "HOLD_GR_ID"); ;
                        dr["USERID"] = LoginInfo.USERID;                        
                        
                        RQSTDT.Rows.Add(dr);

                        new ClientProxy().ExecuteServiceSync("BR_PRD_REG_ASSY_NCR_HOLD", "INDATA", null, RQSTDT);
                        RQSTDT.Rows.Clear();
                    }
                }				

                Util.AlertInfo("SFU1270");  //저장되었습니다.
                dgHold.ItemsSource = null;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 타입으로 CommonCode 조회
        /// Biz : DA_BAS_SEL_COMMCODE_CBO_WITHOUT_CODE
        /// </summary>
        /// <param name="sFilter"></param>
        /// <returns></returns>
        private DataTable dtTypeCombo(string sFilter, ComboStatus cs)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sFilter;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO_WITHOUT_CODE", "RQSTDT", "RSLTDT", RQSTDT);
                AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();
                return dtResult;
            }
            catch (Exception ex)
            {
                return null;
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Method        
        private DataTable AddStatus(DataTable dt, ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case ComboStatus.ALL:
                    dr[sDisplay] = "";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case ComboStatus.NA:
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }

        #endregion

        private void dgHold_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
        }
       
    }
}
