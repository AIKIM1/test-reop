/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Windows;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ASSY002
{
    public partial class ASSY002_001_TRAY_HOLD : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        public ASSY002_001_TRAY_HOLD()
        {
            InitializeComponent();
        }


        #endregion

        #region Initialize

        #endregion

        #region Event

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = "LOT_001";
                dr["PRODID"] = "PRODID";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_INFO", "RQSTDT", "RSLTDT", RQSTDT);
                dgTray.ItemsSource = DataTableConverter.Convert(dtResult);


            }
            catch (Exception ex)
            {

            }
        }

        private void btnHOLD_Click(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("선택된 TRAY를 HOLD 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {

                    for (int i = 0; i < dgTray.Rows.Count; i++)
                    {
                        if (dgTray.GetCell(i, dgTray.Columns["CHK"].Index).Value.ToString() == "1")
                        {
                            // Biz 호출용 In param 작성
                        }
                    }

                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("TRAY HOLD"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                }

            });
        }

        private void btnRELEASE_Click(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("선택된 TRAY를 RELEASE 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    for (int i = 0; i < dgTray.Rows.Count; i++)
                    {
                        if (dgTray.GetCell(i, dgTray.Columns["CHK"].Index).Value.ToString() == "1")
                        {
                            // Biz 호출용 In param 작성
                        }
                    }


                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("TRAY RELEASE"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                }

            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }



        #endregion

        #region Mehod

        #endregion


    }
}
