/*************************************************************************************
 Created Date : 2017.07.05
      Creator : 
   Decription : 초소형 CELL 등록
--------------------------------------------------------------------------------------
 [Change History]
  2017.07.05  DEVELOPER : Initial Created.
  2017.11.07  신광희    : Message 코드 정의 및 공통 메세지 박스 메소드 적용     
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
using System.Text.RegularExpressions;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;


namespace LGC.GMES.MES.CMM001
{
    public partial class CMM_ASSY_INPUT_CELL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();
       
        private string sLOTID = string.Empty;
        private string sCSTID = string.Empty;
        private string sEQPTID = string.Empty;
        private string sOUT_LOTID = string.Empty;
        private string sEQPTLOT = string.Empty;
        private string sCompleteProd = string.Empty;
        //
        int addRows;

        public CMM_ASSY_INPUT_CELL()
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
            sLOTID = Util.NVC(tmps[0]);
            sCSTID = Util.NVC(tmps[1]);
            sEQPTID = Util.NVC(tmps[2]);
            sOUT_LOTID = Util.NVC(tmps[3]);
            sEQPTLOT = Util.NVC(tmps[4]);
            sCompleteProd = Util.NVC(tmps[5]);

            sOUT_LOTID = null;
            sEQPTLOT = null;

            txtLOTID.Text = sLOTID;
            txtTRAYID.Text = sCSTID;

            switch (LoginInfo.CFG_SHOP_ID)
            {
                case "A010":
                    txtPREFIX.Text = "LO" + sLOTID.Substring(3, 7);
                    break;
                case "G182":
                    txtPREFIX.Text = "LN" + sLOTID.Substring(3, 7);
                    break;
                default:
                    txtPREFIX.Text = "LX" + sLOTID.Substring(3, 7);
                    break;
            }

        }
        #endregion

        #region Event
        
        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "Excel Files (.xlsx)|*.xlsx";
            if (fd.ShowDialog() == true)
            {
                using (Stream stream = fd.OpenFile())
                {
                    LoadExcelHelper.LoadExcelData(dgList, stream, 0, 1);
                }
            }
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!isValid())
                return;
            else
            {
                SaveData();
            }
            
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
           DataGrid01RowAdd(dgList);
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DataGrid01RowDelete(dgList);
        }
      
        #endregion

        #region Mehod
        private void DataGrid01RowAdd(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                // 여러건 추가 시 안되는 부분 확인
                DataTable dt = new DataTable();
                //if (rowCount.Value != 0)
                if (Math.Abs(rowCount.Value) > 0)
                {
                    if (rowCount.Value + dg.Rows.Count > 576)
                    {
                        // 최대 ROW수는 576입니다.
                        Util.MessageValidation("SFU4264");
                        return;
                    }
                    else
                    {
                        addRows = int.Parse(rowCount.Value.GetString());
                    }
                }

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }
                if (dg.ItemsSource != null)
                {
                    for (int i = 0; i < addRows; i++)
                    {
                        dt = DataTableConverter.Convert(dg.ItemsSource);
                        DataRow dr2 = dt.NewRow();
                        dt.Rows.Add(dr2);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                }
                else
                {
                  for (int i = 0; i < addRows; i++)
                  {
                    DataRow dr = dt.NewRow();
                    dt.Rows.Add(dr);
                    dg.BeginEdit();
                    dg.ItemsSource = DataTableConverter.Convert(dt);
                    dg.EndEdit();
                  }
              }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void DataGrid01RowDelete(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                DataTable dt = ((DataView)dg.ItemsSource).Table;
                             
                if (dg.Rows.Count > 0)
                {
                    if (dg.SelectedIndex > -1)
                    {
                        dt.Rows[dg.SelectedIndex].Delete();
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
     
        private bool isValid()
        {
            bool bRet = false;
            int chkCellID = 0;
            int chkTray = 0;
            //int chkRowCount = 0;
            if (dgList.Rows.Count == 0)
            {
                //Util.Alert("SFU1651");  //처리 할 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }
            //row만 생성하고 저장버튼 클릭시 체크
            foreach (DataGridRow item in dgList.Rows)
            {
                if (DataTableConverter.GetValue(item.DataItem, "SUBLOTID") == null)
                {
                    //Util.Alert("SFU3679");  //Cell ID가 등록되지 않은 ROW가 있습니다.
                    Util.MessageValidation("SFU3679");
                    return false;
                    
                }
                if (string.IsNullOrEmpty(DataTableConverter.GetValue(item.DataItem, "SUBLOTID").ToString()))
                {
                    //Util.Alert("SFU3679");  //Cell ID가 등록되지 않은 ROW가 있습니다.
                    Util.MessageValidation("SFU3679");
                    return false;
                }
                if (DataTableConverter.GetValue(item.DataItem, "CSTSLOT") == null)
                {
                    //Util.Alert("SFU3680");  //CSTSLOT가 등록되지 않은 ROW가 있습니다.
                    Util.MessageValidation("SFU3680");
                    return false;
                }
                if (string.IsNullOrEmpty(DataTableConverter.GetValue(item.DataItem, "CSTSLOT").ToString()))
                {
                    //Util.Alert("SFU3680");  //CSTSLOT가 등록되지 않은 ROW가 있습니다.
                    Util.MessageValidation("SFU3680");
                    return false;
                }
            }
           

            foreach (DataGridRow iRow in dgList.Rows)
            {
                foreach (DataGridRow jRow in dgList.Rows)
                {
                    if (!string.IsNullOrEmpty(DataTableConverter.GetValue(iRow.DataItem, "SUBLOTID").ToString()) && DataTableConverter.GetValue(iRow.DataItem, "SUBLOTID").ToString().Trim() == DataTableConverter.GetValue(jRow.DataItem, "SUBLOTID").ToString().Trim())
                    {
                        chkCellID = chkCellID + 1;
                    }
                    if (!string.IsNullOrEmpty(DataTableConverter.GetValue(iRow.DataItem, "CSTSLOT").ToString()) && DataTableConverter.GetValue(iRow.DataItem, "CSTSLOT").ToString().Trim() == DataTableConverter.GetValue(jRow.DataItem, "CSTSLOT").ToString().Trim())
                    {
                        chkTray = chkTray + 1;
                    }
                }
                if (!string.IsNullOrEmpty(DataTableConverter.GetValue(iRow.DataItem, "SUBLOTID").ToString()))
                {
                    if (DataTableConverter.GetValue(iRow.DataItem, "SUBLOTID").ToString().Trim().Length != 6)
                    {
                        //Util.Alert("Cell ID는 6자리입니다.");  //중복된 CellID가 존재합니다.
                        Util.MessageValidation("SFU4265");
                        return false;
                    }
                    if (!CheckNumber(DataTableConverter.GetValue(iRow.DataItem, "SUBLOTID").ToString()))
                    {
                        //Util.Alert("Cell ID는 숫자만 입력가능합니다.");  //CellID는 숫자만 입력가능합니다..
                        Util.MessageValidation("SFU4266");
                        return false;
                    }

                  
                }
                //CellID 중복체크
                if (chkCellID > 1)
                {
                    //중복된 CellID가 존재합니다.
                    Util.MessageValidation("SFU3681");
                    return false;
                }
                //Tray중복체크
                if (chkTray > 1)
                {
                    //중복된 Tray 정보가 존재합니다.
                    Util.MessageValidation("SFU3682");
                    return false;
                }
                //Tray 최대수 및 0 체크
                if (!string.IsNullOrEmpty(DataTableConverter.GetValue(iRow.DataItem, "CSTSLOT").ToString()))
                {
                    if (Convert.ToInt16(DataTableConverter.GetValue(iRow.DataItem, "CSTSLOT")) == 0)
                    {
                        //Tray 정보는 0 이상이어야 합니다.
                        Util.MessageValidation("SFU3683");
                        return false;
                    }
                    if (Convert.ToInt16(DataTableConverter.GetValue(iRow.DataItem, "CSTSLOT")) > 576)
                    {
                        //Tray 정보는 576보다 작아야 합니다.
                        Util.MessageValidation("SFU3684");
                        return false;
                    }
                } 
                chkCellID = 0;
                chkTray = 0;
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
            bool IsCheck = int.TryParse(letter, out numchk);
            return IsCheck;
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
                        string sBizNAme = "";

                        if ("Y".Equals(sCompleteProd))
                        {
                            sBizNAme = "BR_PRD_REG_CELL_DATA_CLCT_WSS_UI";
                        }
                        else
                        {
                            sBizNAme = "BR_PRD_REG_CELL_DATA_CLCT_WSS";
                        }

                        DataSet inDataSet = GetBR_PRD_REG_START_OUT_LOT_WSS();
                        DataTable inDataTable = inDataSet.Tables["IN_EQP"];
                        DataRow drow = inDataTable.NewRow();
                        drow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        drow["IFMODE"] = IFMODE.IFMODE_OFF;
                        drow["EQPTID"] = sEQPTID;
                        drow["USERID"] = LoginInfo.USERID;
                        drow["PROD_LOTID"] = sLOTID;
                        drow["EQPT_LOTID"] = sEQPTLOT;
                        drow["OUT_LOTID"] = sOUT_LOTID;
                        drow["CSTID"] = sCSTID;
                        inDataSet.Tables["IN_EQP"].Rows.Add(drow);

                        DataTable inCST = inDataSet.Tables["IN_CST"];

                        for (int row = 0; row < dgList.Rows.Count; row++)
                        {
                            if (!string.IsNullOrEmpty(DataTableConverter.GetValue(dgList.Rows[row].DataItem, "SUBLOTID").ToString()) || string.IsNullOrEmpty(DataTableConverter.GetValue(dgList.Rows[row].DataItem, "CSTSLOT").ToString()))
                            {
                                drow = inCST.NewRow();
                                drow["SUBLOTID"] = txtPREFIX.Text.ToString() + DataTableConverter.GetValue(dgList.Rows[row].DataItem, "SUBLOTID").ToString().Trim();
                                drow["CSTSLOT"] = DataTableConverter.GetValue(dgList.Rows[row].DataItem, "CSTSLOT").ToString().Trim();
                                drow["EL_PRE_WEIGHT"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[row].DataItem, "EL_PRE_WEIGHT")) == string.Empty ? Util.NVC(DataTableConverter.GetValue(dgList.Rows[row].DataItem, "EL_PRE_WEIGHT")) : string.Format("{0:#,##0.00}", Convert.ToDecimal(DataTableConverter.GetValue(dgList.Rows[row].DataItem, "EL_PRE_WEIGHT")));
                                drow["EL_AFTER_WEIGHT"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[row].DataItem, "EL_AFTER_WEIGHT")) == string.Empty ? Util.NVC(DataTableConverter.GetValue(dgList.Rows[row].DataItem, "EL_AFTER_WEIGHT")) : string.Format("{0:#,##0.00}", Convert.ToDecimal(DataTableConverter.GetValue(dgList.Rows[row].DataItem, "EL_AFTER_WEIGHT")));
                                drow["EL_WEIGHT"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[row].DataItem, "EL_WEIGHT")) == string.Empty ? Util.NVC(DataTableConverter.GetValue(dgList.Rows[row].DataItem, "EL_WEIGHT")) : string.Format("{0:#,##0.00}", Convert.ToDecimal(DataTableConverter.GetValue(dgList.Rows[row].DataItem, "EL_WEIGHT")));
                                drow["IROCV"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[row].DataItem, "IROCV"));

                                inDataSet.Tables["IN_CST"].Rows.Add(drow);
                            }
                        }

                        new ClientProxy().ExecuteService_Multi(sBizNAme, "IN_EQP,IN_CST", null, (bizResult, ex) =>
                        {
                            if (ex != null)
                            {
                                Util.MessageException(ex);
                                return;
                            }

                            Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                            this.DialogResult = MessageBoxResult.OK;

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
            inInputLot.Columns.Add("EL_PRE_WEIGHT", typeof(string));
            inInputLot.Columns.Add("EL_AFTER_WEIGHT", typeof(string));
            inInputLot.Columns.Add("EL_WEIGHT", typeof(string));
            inInputLot.Columns.Add("IROCV", typeof(string));

            return indataSet;
        }
        
        #endregion
    }
}
