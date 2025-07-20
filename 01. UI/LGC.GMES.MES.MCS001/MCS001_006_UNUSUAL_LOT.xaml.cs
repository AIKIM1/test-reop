/*************************************************************************************
 Created Date : 2018.11.01
      Creator : 오화백 
   Decription : 특이LOT 조회
--------------------------------------------------------------------------------------
  
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.MCS001 
{
    public partial class MCS001_006_UNUSUAL_LOT : C1Window, IWorkArea
    {

        #region Declaration & Constructor 

        //수동출고예약 여부
        private string Check = string.Empty;

        public MCS001_006_UNUSUAL_LOT()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            SeachData();
            this.Loaded -= C1Window_Loaded;
        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

        #region Event
        /// <summary>
        /// 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (Check == "Y")
            {
                this.DialogResult = MessageBoxResult.OK;

            }
            else
            {
                this.DialogResult = MessageBoxResult.Cancel;
            }
        }
        /// <summary>
        /// 수동반송
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnManualIssue_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationReturn())
                return;
            string port = (string)cboPort.SelectedValue;
            //출고예약 실행여부

            Util.MessageConfirm("SFU4539", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    string sRackId = String.Empty;
                    string sLotId = String.Empty;

                    int DubRack = 0;
                    DataTable dtRack = new DataTable();
                    dtRack.Columns.Add("RACK_ID", typeof(string));
                    foreach (DataRow row in ((System.Data.DataView)dgList.ItemsSource).Table.Rows)
                    {
                        if (row["CHK"].ToString() == "1")
                        {
                            DubRack = 0;
                            if (dtRack.Rows.Count == 0)
                            {
                                DataRow dr = dtRack.NewRow();
                                dr["RACK_ID"] = row["RACK_ID"].ToString();
                                dtRack.Rows.Add(dr);
                            }
                            else
                            {
                                for (int i = 0; i < dtRack.Rows.Count; i++)
                                {
                                    if (dtRack.Rows[i]["RACK_ID"].ToString() == row["RACK_ID"].ToString())
                                    {
                                        DubRack = 1;
                                    }
                                }

                                if (DubRack == 0)
                                {
                                    DataRow dr = dtRack.NewRow();
                                    dr["RACK_ID"] = row["RACK_ID"].ToString();
                                    dtRack.Rows.Add(dr);
                                }
                            }
                        }
                    }
                    foreach (DataRow row in dtRack.Rows)
                    {

                        DataSet inDataSet = new DataSet();
                        DataTable inTable = inDataSet.Tables.Add("INDATA");
                        inTable.Columns.Add("FROM_ID", typeof(string));
                        inTable.Columns.Add("FROM_TYPE", typeof(string));
                        inTable.Columns.Add("TO_ID", typeof(string));
                        inTable.Columns.Add("TO_TYPE", typeof(string));
                        inTable.Columns.Add("LOGIS_CMD_PRIORITY_NO", typeof(Int32));
                        inTable.Columns.Add("USERID", typeof(string));

                        DataTable inLot = inDataSet.Tables.Add("INLOT");
                        inLot.Columns.Add("LOTID", typeof(string));

                        DataRow newRow = inTable.NewRow();
                        newRow["FROM_ID"] = row["RACK_ID"].ToString();
                        newRow["FROM_TYPE"] = "RACK";
                        newRow["TO_ID"] = port;
                        newRow["TO_TYPE"] = "PORT";
                        newRow["LOGIS_CMD_PRIORITY_NO"] = 30;
                        newRow["USERID"] = LoginInfo.USERID;
                        inTable.Rows.Add(newRow);

                        DataTable _dtRackInfo = DataTableConverter.Convert(dgList.ItemsSource);
                        DataRow[] selectedRow = _dtRackInfo.Select("RACK_ID='" + row["RACK_ID"].ToString() + "'");

                        foreach (DataRow Lotrow in selectedRow)
                        {
                            newRow = inLot.NewRow();
                            newRow["LOTID"] = Lotrow["LOTID"];
                            inLot.Rows.Add(newRow);
                        }
                        new ClientProxy().ExecuteService_Multi("BR_MCS_REG_LOGIS_CMD_NSP_NSR_MGV", "INDATA,INLOT", "OUTDATA", (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }
                                Check = "Y";
                                Util.AlertInfo("SFU1275");
                                this.SeachData();

                            }
                            catch (Exception ex)
                            {

                                Util.MessageException(ex);

                            }
                        }, inDataSet);
                    }
                }
            });
        }
        /// <summary>
        /// 스프레드 머지
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgList_MergingCells(object sender, C1.WPF.DataGrid.DataGridMergingCellsEventArgs e)
        {
            try
            {
                int j = 0;
                if (dgList.GetRowCount() > 0)
                {
                    for (int i = 0; i <= dgList.GetRowCount() - 1; i++)
                    {

                        for (j = 1; j < dgList.GetRowCount() - i; j++)
                        {
                            if ((Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "RACK_ID"))) != (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i + j].DataItem, "RACK_ID"))))
                            {

                                break;
                            }
                        }
                        j--;


                        e.Merge(new C1.WPF.DataGrid.DataGridCellsRange(dgList.GetCell(i, 0), dgList.GetCell(i + j, 0)));
                        e.Merge(new C1.WPF.DataGrid.DataGridCellsRange(dgList.GetCell(i, 1), dgList.GetCell(i + j, 1)));
                        e.Merge(new C1.WPF.DataGrid.DataGridCellsRange(dgList.GetCell(i, 2), dgList.GetCell(i + j, 2)));
                        e.Merge(new C1.WPF.DataGrid.DataGridCellsRange(dgList.GetCell(i, 3), dgList.GetCell(i + j, 3)));
                        e.Merge(new C1.WPF.DataGrid.DataGridCellsRange(dgList.GetCell(i, 4), dgList.GetCell(i + j, 4)));
                        e.Merge(new C1.WPF.DataGrid.DataGridCellsRange(dgList.GetCell(i, 5), dgList.GetCell(i + j, 5)));


                        i = i + j;

                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageInfo(ex.Message.ToString());
            }


        }

        #endregion

        #region Mehod
        /// <summary>
        /// 조회
        /// </summary>
        private void SeachData()
        {
            loadingIndicator.Visibility = Visibility.Visible;
            DataTable RQSTDT = new DataTable( "RQSTDT" );
            RQSTDT.Columns.Add("LANGID", typeof(string));
            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            RQSTDT.Rows.Add( dr );

			new ClientProxy().ExecuteService("DA_MCS_SEL_LAMI_UNUSUAL_LOT", "RQSTDT", "RSLTDT", RQSTDT, ( result, exception ) => {
				try {
					if( exception != null ) {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException( exception );
						return;
					}

					Util.GridSetData( dgList, result, FrameOperation, true );
				} catch( Exception ex ) {
					Util.MessageException( ex );
				} finally {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }
			} );
		}

        /// <summary>
        /// 콤보박스 초기화
        /// </summary>
        private void InitCombo()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();
                //PORTID
                String[] sFilter2 = { "", "NPW" };
                _combo.SetCombo(cboPort, CommonCombo.ComboStatus.SELECT, sFilter: sFilter2, sCase: "CWALAMIPORT");
            }
            catch
            {
            }
        }

        /// <summary>
        /// Validation
        /// </summary>
        /// <returns></returns>
        private bool ValidationReturn()
        {

            if (cboPort.SelectedIndex <= 0)
            {
                // %1(을)를 선택하세요.
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("출고포트"));
                return false;

            }
            int checkcount = 0;
            foreach (DataRow row in ((System.Data.DataView)dgList.ItemsSource).Table.Rows)
            {
                if (row["CHK"].ToString() == "1")
                { checkcount++; }
            }

            if (checkcount < 1)
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("재작업대상"));
                return false;
            }

            return true;
        }
        #endregion


    }

}