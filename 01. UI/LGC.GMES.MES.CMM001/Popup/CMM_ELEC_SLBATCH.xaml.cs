using System;
using System.Windows;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text;
using System.Windows.Media;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_ELEC_SlBatch.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_SLBATCH : C1Window, IWorkArea
    {
        private int _ROWCOUNT = 1;
        private bool search_chk = true;

        Util _Util = new Util();
        DataTable dtiUse = new DataTable();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_ELEC_SLBATCH()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            ComBo();
            Init();
            setDataTable();
        }
        void Init()
        {
            dgLotList.ItemsSource = null;

            DataTable emptyTransferTable = new DataTable();
            emptyTransferTable.Columns.Add("LOTID", typeof(string));
            emptyTransferTable.Columns.Add("PRODID", typeof(string));
            emptyTransferTable.Columns.Add("BTCH_NOTE", typeof(string));

            dgLotList.ItemsSource = DataTableConverter.Convert(emptyTransferTable);
        }

        private void ComBo()
        {
            String[] sFilter3 = { "DELFLAG" };//사용여부
            CommonCombo _combo = new CMM001.Class.CommonCombo();
            _combo.SetCombo(cboIUse, CommonCombo.ComboStatus.ALL, sFilter: sFilter3, sCase: "COMMCODE");            
        }

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
            
        };

        private void setDataTable()
        {
            //this.Loaded -= C1Window_Loaded;
            dtiUse = GetCommonCode("DELFLAG");
            cboIUse.SelectedIndex = 2;
        }

        private DataTable GetCommonCode(string sType)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sType;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return dtResult;
            }
            catch (Exception ex) { }

            return new DataTable();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataSet dataset = new DataSet();
                DataTable RQSTDT = dataset.Tables.Add("INDATA");

                RQSTDT.Columns.Add("INSUSER", typeof(string));
                RQSTDT.Columns.Add("UPDUSER", typeof(string));

                DataRow indata = RQSTDT.NewRow();

                indata["INSUSER"] = LoginInfo.USERID;
                indata["UPDUSER"] = LoginInfo.USERID;

                dataset.Tables["INDATA"].Rows.Add(indata);

                DataTable RQSTDT2 = dataset.Tables.Add("INLOT");

                RQSTDT2.Columns.Add("LOTID", typeof(string));
                RQSTDT2.Columns.Add("BTCH_NOTE", typeof(string));
                RQSTDT2.Columns.Add("DEL_FLAG", typeof(string));

                for (int i = 0; i < dgLotList.Rows.Count; i++)
                {
                    DataRow inlot = RQSTDT2.NewRow();

                    inlot["LOTID"] = DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "LOTID");
                    inlot["BTCH_NOTE"] = DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "BTCH_NOTE");
                    if (search_chk == false)
                    {
                        inlot["DEL_FLAG"] = "N";
                    }
                    else
                    {
                        inlot["DEL_FLAG"] = DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "DEL_FLAG");
                    }

                    dataset.Tables["INLOT"].Rows.Add(inlot);
                }

                DataSet result = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_ELTR_VISUAL_MNGT", "INDATA,INLOT", "RSLTDT", dataset);

                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                lotList_Search();
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
           
        }

        private void txtRowAdd_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!Char.IsDigit((char)KeyInterop.VirtualKeyFromKey(e.Key)) & e.Key != Key.Back | e.Key == Key.Space)
            {
                e.Handled = true;
                Util.MessageInfo("숫자만 입력해주세요.");
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            dgLotList.Columns["DEL_FLAG"].Visibility = Visibility.Collapsed;
            dgLotList.Columns["PRODID"].Visibility = Visibility.Collapsed;
            DataGridRowAdd(dgLotList);

        }

        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                DataTable dt = new DataTable();

                if(search_chk == true) { 

                    Util.gridClear(dgLotList);
                    search_chk = false;
                }


                if (dg.ItemsSource == null || dg.Rows.Count < 0)
                {
                    return;
                }
                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }

                dt = DataTableConverter.Convert(dg.ItemsSource);

                if (!string.IsNullOrWhiteSpace(txtRowAdd.Text))
                {
                    if (Int32.Parse(txtRowAdd.Text) > 1)
                    {
                        _ROWCOUNT = Int32.Parse(txtRowAdd.Text);
                    }
                }
                
                for (int i = 0; i < _ROWCOUNT; i++)
                {
                    DataRow dr2 = dt.NewRow();
                    dt.Rows.Add(dr2);
                    dt.AcceptChanges();
                    dg.ItemsSource = DataTableConverter.Convert(dt);
                }

                // 스프레드 스크롤 하단으로 이동
                dg.ScrollIntoView(dg.GetRowCount() - 1, 0);

                _ROWCOUNT = 1;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            DataGridRowRemove(dgLotList);
        }

        private void DataGridRowRemove(C1.WPF.DataGrid.C1DataGrid dg)
        {

            // 기존 저장자료는 제외
            if (dg.SelectedIndex > -1)
            {
                DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                
                dt.Rows[dg.SelectedIndex].Delete();
                dt.AcceptChanges();
                dg.ItemsSource = DataTableConverter.Convert(dt);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            lotList_Search();
        }

        private void lotList_Search()
        {
            Util.gridClear(dgLotList);
            dgLotList.Columns["DEL_FLAG"].Visibility = Visibility.Visible;
            dgLotList.Columns["PRODID"].Visibility = Visibility.Visible;
            search_chk = true;
            
            DataTable lotdt = new DataTable();

            lotdt.Columns.Add("DEL_FLAG", typeof(string));

            DataRow indata = lotdt.Rows.Add();

            if (cboIUse.SelectedValue.ToString() != "")
            {
                indata["DEL_FLAG"] = Util.GetCondition(cboIUse);
            }

            lotdt.Rows.Add(lotdt);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_ELTR_VISUAL_MNGT_LOT_LIST", "RQSTDT", "RSLTDT", lotdt);

            Util.GridSetData(dgLotList, result, FrameOperation, true);

           (dgLotList.Columns["DEL_FLAG"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtiUse.Copy());
        }
    }
}
