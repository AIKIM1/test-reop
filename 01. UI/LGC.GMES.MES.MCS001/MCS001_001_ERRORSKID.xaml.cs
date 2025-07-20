/*************************************************************************************
 Created Date : 2017.01.16
      Creator : 김재호 부장
   Decription : SKID BUFFER 모니터링
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
    public partial class MCS001_001_ERRORSKID : C1Window, IWorkArea
    {
		#region Declaration & Constructor 

		private string WH_ID;

		public MCS001_001_ERRORSKID()
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

            object[] Parameters = C1WindowExtension.GetParameters(this);
            WH_ID = Parameters[0].ToString();

            InitCombo();

            SeachData();
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
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= C1Window_Loaded;			
        }

        private void OnBtnRefresh(object sender, RoutedEventArgs e)
        {
			SeachData();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod
        private void SeachData()
        {
            DataTable RQSTDT = new DataTable("RQSTDT");
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add( "WH_ID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["WH_ID"] = WH_ID;
			RQSTDT.Rows.Add(dr);

			new ClientProxy().ExecuteService("DA_MCS_SEL_SKID_ERROR", "RQSTDT", "RSLTDT", RQSTDT, ( result, exception ) => {
				try {
					if( exception != null ) {
						Util.MessageException( exception );
						return;
					}

					Util.GridSetData( dgList, result, FrameOperation );
				} catch( Exception ex ) {
					Util.MessageException( ex );
				} finally {
				}
			} );
		}

        private void InitCombo()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = null;
                dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_OUT_PORT", "RQSTDT", "RSLTDT", RQSTDT);
               
                DataRow drSel = dtResult.NewRow();

                drSel["CBO_CODE"] = "SELECT";
                drSel["CBO_NAME"] = "-SELECT-";
                dtResult.Rows.InsertAt(drSel, 0);

                cboPort.DisplayMemberPath = "CBO_NAME";
                cboPort.SelectedValuePath = "CBO_CODE";
                cboPort.ItemsSource = dtResult.Copy().AsDataView();

                cboPort.SelectedValue = "SELECT";
            }
            catch
            {
            }
        }
        #endregion

        private void Chk_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void btnManualIssue_Click(object sender, RoutedEventArgs e)
        {
            string port = (string)cboPort.SelectedValue;

            //if (dtCheck.Rows.Count == 0)
            //{
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4531"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);   //메세지 작성요
            //    return;
            //}


            if (port.Equals("SELECT"))
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4532"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);   //메세지 작성요
                return;
            }


            int checkcount = 0;
            foreach (DataRow row in ((System.Data.DataView)dgList.ItemsSource).Table.Rows)
            {
                if(row["CHK"].ToString() == "1")
                { checkcount++; }
            }

            if (checkcount < 1)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4531"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);   //메세지 작성요
                return;
            }

            Util.MessageConfirm("SFU4539", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    string sRackId = String.Empty;
                    foreach (DataRow row in ((System.Data.DataView)dgList.ItemsSource).Table.Rows)
                    {
                        if (row["CHK"].ToString() == "1")
                        {
                            sRackId = row["RACK_ID"].ToString();


                            DataTable RQSTDT = new DataTable();
                            RQSTDT.TableName = "RQSTDT";
                            RQSTDT.Columns.Add("LANGID", typeof(string));
                            RQSTDT.Columns.Add("RACK_ID", typeof(string));
                            RQSTDT.Columns.Add("RSV_TO_PORT_ID", typeof(string));
                            RQSTDT.Columns.Add("UPDUSER", typeof(string));
                            RQSTDT.Columns.Add("DTTM", typeof(DateTime));

                            DataRow dr = RQSTDT.NewRow();
                            dr["LANGID"] = LoginInfo.LANGID;
                            dr["RACK_ID"] = sRackId;
                            dr["RSV_TO_PORT_ID"] = port;
                            dr["UPDUSER"] = LoginInfo.USERID;
                            dr["DTTM"] = System.DateTime.Now;
                            RQSTDT.Rows.Add(dr);  //BR_MCS_UPD_MANUAL_ISS_RSV  DA_MCS_UPD_MAUAL_ISS_RSV
                            DataTable OUTDATA = new ClientProxy().ExecuteServiceSync("BR_MCS_UPD_MANUAL_ISS_RSV", "RQSTDT", "OUTDATA", RQSTDT);
                        }
                    }

                    Util.AlertInfo("SFU1275");

                    this.SeachData();
                }
            });
        }

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
    }
}