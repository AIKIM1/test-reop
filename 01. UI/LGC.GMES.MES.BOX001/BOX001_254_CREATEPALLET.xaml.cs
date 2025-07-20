/*************************************************************************************
 Created Date : 2022.02.21
      Creator : 이제섭
   Decription : Pallet 생성 팝업
--------------------------------------------------------------------------------------
 [Change History]


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

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_201_RUNSTART.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_254_CREATEPALLET : C1Window, IWorkArea
    {
        Util _util = new Util();

        string _AREAID = string.Empty;
        string sUSERID = string.Empty;
        string sSHFTID = string.Empty;
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }       

        public string PALLET_ID
        {
            get;
            set;
        }
        #region Initialize
        public BOX001_254_CREATEPALLET()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            _AREAID = Util.NVC(tmps[0]);
            sUSERID = Util.NVC(tmps[1]);
            sSHFTID = Util.NVC(tmps[2]);

            InitCombo();
            InitControl();
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
        }
        #endregion

        #region 작업 Pallet 색깔표시 : dgPalletList_LoadedCellPresenter()

        private void dgInPallet_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {

                if (e.Cell.Presenter == null)
                {
                    return;
                }
            }));
        }

        #endregion

        #region [EVENT]

        #region 텍스트박스 포커스 : text_GotFocus()
        private void text_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }
        #endregion

        #region Pallet 생성 : btnSave_Click()
        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            //Pallet 생성하시겠습니까?
            Util.MessageConfirm("SFU5009", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    RunStart();
                }
            });
        }
        #endregion

        #region 닫기 : btnClose_Click()
        /// <summary>
        /// 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }


        #endregion

        #region 삭제 : btnInPalletDelete_Click()
        private void btnInPalletDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dt = DataTableConverter.Convert(dgInPallet.ItemsSource);

                foreach (DataRow dr in dt.AsEnumerable().ToList())
                {
                    if (dr["CHK"].Equals(1))
                    {
                        dt.Rows.Remove(dr);
                    }
                }
                Util.GridSetData(dgInPallet, dt, FrameOperation, false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region OutBox 체크 : txtInPalletID_KeyDown()
        private void txtInPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    DataSet inDataSet = new DataSet();

                    DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                    inDataTable.Columns.Add("LANGID");
                    inDataTable.Columns.Add("PALLETID");

                    DataTable inBoxTable = inDataSet.Tables.Add("INOUTBOX");
                    inBoxTable.Columns.Add("BOXID");

                    DataRow newRow = inDataTable.NewRow();
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["PALLETID"] = null;
                    inDataTable.Rows.Add(newRow);
                    newRow = null;

                    newRow = inBoxTable.NewRow();
                    newRow["BOXID"] = txtInPalletID.Text;
                    inBoxTable.Rows.Add(newRow);

                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_INPUT_OUTBOX_NEW_NFF", "INDATA,INOUTBOX", "OUTDATA", inDataSet);

                    if (dsResult.Tables["OUTDATA"] != null || dsResult.Tables["OUTDATA"].Rows.Count > 0)
                    {
                        GetCompleteOutbox(dsResult.Tables["OUTDATA"].Rows[0]["BOXID"].ToString());
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    txtInPalletID.Text = string.Empty;
                }               
            }
        }
        #endregion

        #endregion

        #region [Method]
       
        #region Pallet 생성 : RunStart()
        private void RunStart()
        {
            if (dgInPallet.Rows.Count == 1)
            {
                //Outbox 정보가 없습니다
                Util.MessageValidation("SFU5010");
                return;
            }
            
            try
            {
                DataSet inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE");
                inDataTable.Columns.Add("SHFT_ID");
                inDataTable.Columns.Add("USERID");
                inDataTable.Columns.Add("SHIPTO_ID");

                DataTable inBoxTable = inDataSet.Tables.Add("INOUTBOX");
                inBoxTable.Columns.Add("BOXID");

                DataRow newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = "UI";
                newRow["SHFT_ID"] = sSHFTID;
                newRow["USERID"] = sUSERID;
                newRow["SHIPTO_ID"] = "M9999";
                inDataTable.Rows.Add(newRow);
                newRow = null;


                DataTable dt = DataTableConverter.Convert(dgInPallet.ItemsSource);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i]["OUTBOXID"].ToString() != string.Empty)
                    {
                        newRow = inBoxTable.NewRow();
                        newRow["BOXID"] = Util.NVC(dgInPallet.GetCell(i, dgInPallet.Columns["OUTBOXID"].Index).Value);
                        inBoxTable.Rows.Add(newRow);
                    }
                }


                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_2ND_PALLET_NFF_FM", "INDATA,INOUTBOX", "OUTDATA", inDataSet);
                if (dsResult != null && dsResult.Tables["OUTDATA"] != null)
                {
                    PALLET_ID = dsResult.Tables["OUTDATA"].Rows[0]["PALLETID"].ToString();
                }

                this.DialogResult = MessageBoxResult.OK;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion

        #region 조회된 OUTBOX 바인드 : GetCompleteOutbox()
        private void GetCompleteOutbox(string BoxID)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("BOXID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["BOXID"] = BoxID;
                newRow["LANGID"] = LoginInfo.LANGID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_COMPLETE_OUTBOX_PALLET_FM", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    DataTable dtSource = DataTableConverter.Convert(dgInPallet.ItemsSource);
                    var query = (from t in dtSource.AsEnumerable()
                                 where t.Field<string>("OUTBOXID") == BoxID
                                 select t).Distinct();
                    if (query.Any())
                    {
                        //	SFU1781	이미 추가 된 OUTBOX 입니다.
                        Util.MessageValidation("SFU5011");
                        return;
                    }
                    if (dtSource != null)
                    {
                        for (int i = 0; i < dtSource.Rows.Count; i++)
                        {
                            if(dtResult.Rows[0]["PRODID"].ToString () != dtSource.Rows[i]["PRODID"].ToString())
                            {
                                //동일 제품이 아닙니다
                                Util.MessageValidation("SFU1502");
                                return;
                            }
                            if (dtResult.Rows[0]["EXP_DOM_TYPE_CODE"].ToString() != dtSource.Rows[i]["EXP_DOM_TYPE_CODE"].ToString())
                            {
                                //동일한 시장유형이 아닙니다.
                                Util.MessageValidation("SFU4271");
                                return;
                            }
                            if (dtResult.Rows[0]["LOTTYPE"].ToString() != dtSource.Rows[i]["LOTTYPE"].ToString())
                            {
                                //동일 LOT 유형이 아닙니다.
                                Util.MessageValidation("SFU4513");
                                return;
                            }
                            if (dtResult.Rows[0]["PRDT_GRD_CODE"].ToString() != dtSource.Rows[i]["PRDT_GRD_CODE"].ToString())
                            {
                                // 동일한 전압등급이 아닙니다.
                                Util.MessageValidation("SFU4061");
                                return;
                            }
                            if (dtResult.Rows[0]["PKG_LOTID"].ToString() != dtSource.Rows[i]["PKG_LOTID"].ToString())
                            {
                                // 동일한 조립LOT이 아닙니다.
                                Util.MessageValidation("SFU4056");
                                return;
                            }
                        }
                    }

                    dtResult.Merge(dtSource);
                    Util.GridSetData(dgInPallet, dtResult, FrameOperation, false);

                }
                if (dtResult != null && dtResult.Rows.Count > 0)
                    dgInPallet.CurrentCell = dgInPallet.GetCell(0, 1);

                string[] sColumnName = new string[] { "OUTBOXID2", "BOXSEQ", "OUTBOXID", "OUTBOXQTY" };
                if (dgInPallet.Rows.Count > 0)
                {
                    DataGridAggregate.SetAggregateFunctions(dgInPallet.Columns["INBOXQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                }
                _util.SetDataGridMergeExtensionCol(dgInPallet, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #endregion



    }
}
