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
	public partial class MCS001_001_InSKID : C1Window, IWorkArea
	{

		#region Declaration & Constructor 

		private string WH_ID;

		public MCS001_001_InSKID() {
			InitializeComponent();
		}
		public IFrameOperation FrameOperation {
			get;
			set;
		}
		private void C1Window_Loaded( object sender, RoutedEventArgs e ) {
			ApplyPermissions();

			object[] Parameters = C1WindowExtension.GetParameters( this );
			WH_ID = Parameters[0].ToString();

			SeachData();
		}

		/// <summary>
		/// 화면내 버튼 권한 처리
		/// </summary>
		private void ApplyPermissions() {
			List<Button> listAuth = new List<Button>();
			//listAuth.Add(btnInReplace);
			Util.pageAuth( listAuth, FrameOperation.AUTHORITY );
		}

		#endregion

		#region Event

		/// <summary>
		/// Initializing 이후에 FormLoad시 Event를 생성.
		/// </summary>
		private void SetEvent() {
			this.Loaded -= C1Window_Loaded;
		}

		private void OnBtnRefresh( object sender, RoutedEventArgs e ) {
			SeachData();
		}

		#endregion

		#region Mehod
		private void SeachData() {
			DataTable RQSTDT = new DataTable( "RQSTDT" );
			RQSTDT.Columns.Add( "LANGID", typeof( string ) );
			RQSTDT.Columns.Add( "WH_ID", typeof( string ) );

			DataRow dr = RQSTDT.NewRow();
			dr["LANGID"] = LoginInfo.LANGID;
			dr["WH_ID"] = WH_ID;

			RQSTDT.Rows.Add( dr );

			new ClientProxy().ExecuteService("DA_MCS_SEL_IN_SKID_BUFFER", "RQSTDT", "RSLTDT", RQSTDT, ( result, exception ) => {
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
        #endregion

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void dgList_MergingCells(object sender, C1.WPF.DataGrid.DataGridMergingCellsEventArgs e)
        {
            try
            {
                int j= 0;
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

                        {
                            e.Merge(new C1.WPF.DataGrid.DataGridCellsRange(dgList.GetCell(i, 2), dgList.GetCell(i + j, 2)));
                            e.Merge(new C1.WPF.DataGrid.DataGridCellsRange(dgList.GetCell(i, 3), dgList.GetCell(i + j, 3)));
                            e.Merge(new C1.WPF.DataGrid.DataGridCellsRange(dgList.GetCell(i, 4), dgList.GetCell(i + j, 4)));
                            e.Merge(new C1.WPF.DataGrid.DataGridCellsRange(dgList.GetCell(i, 5), dgList.GetCell(i + j, 5)));
                        }

                        i = i + j;

                    }
                }
            }
            catch(Exception ex)
            {
                Util.MessageInfo(ex.Message.ToString());
            }
        }
    }
}