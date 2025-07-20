/*************************************************************************************
 Created Date : 2018.11.13
      Creator : 오화백
   Decription : Pancake 정보
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
    public partial class MCS006_001_PANCAKE_INFO : C1Window, IWorkArea
    {
        public string sRACK_ID;
        public MCS006_001_PANCAKE_INFO() {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation {
            get;
            set;
        }
        private void OnC1WindowLoaded(object sender, RoutedEventArgs e)
        {           
            this.InitGrid();
        }

        private void InitGrid()
        {
            object[] Parameters = C1WindowExtension.GetParameters(this);
            if (Parameters != null && Parameters.Length != 0)
            {

                sRACK_ID = Parameters[0].ToString();

                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("RACKID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RACKID"] = sRACK_ID;

                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_MCS_SEL_LAMI_INPUT_LOT", "RQSTDT", "RSLTDT", RQSTDT, (result, exception) => {
                    try
                    {
                        if (exception != null)
                        {
                            Util.MessageException(exception);
                            return;
                        }

                        Util.GridSetData(dgList, result, FrameOperation, true);

                        if (result.Rows.Count >0)
                        {
                            txtPancakeRow.Text = result.Rows[0]["X_PSTN"].ToString();
                            txtPancakeColumn.Text = result.Rows[0]["Y_PSTN"].ToString();
                            txtPancakeStair.Text = result.Rows[0]["Z_PSTN"].ToString();
                        }
                        else
                        {
                            txtPancakeRow.Text = string.Empty;
                            txtPancakeColumn.Text = string.Empty;
                            txtPancakeStair.Text = string.Empty;
                        }
                       
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
        /// <summary>
        /// 닫기 버튼
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBtnClose(object sender, RoutedEventArgs e) {
            this.DialogResult = MessageBoxResult.Cancel;
        }
    
    }
}
