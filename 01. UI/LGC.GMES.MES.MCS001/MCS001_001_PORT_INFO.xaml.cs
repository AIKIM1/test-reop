/*************************************************************************************
 Created Date : 2017.01.16
      Creator : 김재호 부장
   Decription : SKID BUFFER 모니터링
--------------------------------------------------------------------------------------
  
**************************************************************************************/
using System;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

using LGC.GMES.MES.MCS001.Controls;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_001_PANCAKE_INFO.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_001_PORT_INFO : C1Window, IWorkArea
    {

        // 창고 ID - 폴란드 Skid 창고
        private string WH_ID;
        private string sRACK_ID;

        public MCS001_001_PORT_INFO() {
            InitializeComponent();

            IsUpdated = false;
        }

        public IFrameOperation FrameOperation {
            get;

            set;
        }


        private void OnC1WindowLoaded(object sender, RoutedEventArgs e) {

            //this.InitGrid();
            object[] Parameters = C1WindowExtension.GetParameters(this);
            WH_ID = Parameters[1].ToString();
            sRACK_ID = Parameters[0].ToString();

            InitGrid();

            this.SeachData();
        }

      
        /// <summary>
        /// 정보변경버튼으로 정보 변경여부 확인
        /// </summary>
        public bool IsUpdated {
            get;
            set;
        }     
    

        /// <summary>
        /// 닫기 버튼
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBtnClose(object sender, RoutedEventArgs e) {
            this.DialogResult = MessageBoxResult.Cancel;
        }


        private void SeachData()
        {
            object[] Parameters = C1WindowExtension.GetParameters(this);
            if (Parameters != null && Parameters.Length != 0)
            {
                SkidRack rack = (SkidRack)Parameters[2];

                if (rack != null)
                {
                    DataTable RQSTDT = new DataTable("RQSTDT");
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("RACK_ID", typeof(string));
                    RQSTDT.Columns.Add("WH_ID", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["RACK_ID"] = sRACK_ID;
                    dr["WH_ID"] = WH_ID;
                    RQSTDT.Rows.Add(dr);

                    new ClientProxy().ExecuteService("DA_MCS_SEL_PORT_INFO", "RQSTDT", "RSLTDT", RQSTDT, (result, exception) =>
                    {
                        try
                        {
                            if (exception != null)
                            {
                                Util.MessageException(exception);
                                return;
                            }

                            Util.GridSetData(dgList, result, FrameOperation);
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);                            
                        }
                        finally
                        {
                            //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", Logger.MESSAGE_OPERATION_END);
                        }
                    });
                }
                else
                {
                    DataTable RQSTDT = new DataTable("RQSTDT");
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("PORT_ID", typeof(string));
                    RQSTDT.Columns.Add("WH_ID", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["PORT_ID"] = sRACK_ID;
                    dr["WH_ID"] = WH_ID;
                    RQSTDT.Rows.Add(dr);

                    new ClientProxy().ExecuteService("DA_MCS_SEL_PORT_INFO_BY_PORT_ID", "RQSTDT", "RSLTDT", RQSTDT, (result, exception) =>
                    {
                        try
                        {
                            if (exception != null)
                            {
                                Util.MessageException(exception);
                                return;
                            }

                            Util.GridSetData(dgList, result, FrameOperation);
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);                           
                        }
                        finally
                        {
                        }
                    });
                }
            }
        }

        private void InitGrid()
        {
            object[] Parameters = C1WindowExtension.GetParameters(this);
            if (Parameters != null && Parameters.Length != 0)
            {
                SkidRack rack = (SkidRack)Parameters[2];

                if (rack != null)
                {
                    txtPancakeRow.Text = rack.Row.ToString();
                    txtPancakeColumn.Text = rack.Col.ToString();
                    txtPancakeStair.Text = rack.Stair.ToString();
                    txtZoneId.Text = rack.ZoneId;
                }
                else
                {
                    txtPancakeRow.Text = "";
                    txtPancakeColumn.Text = "";
                    txtPancakeStair.Text = "";
                    txtZoneId.Text = string.Empty;
                }
            }
        }
    }
}
