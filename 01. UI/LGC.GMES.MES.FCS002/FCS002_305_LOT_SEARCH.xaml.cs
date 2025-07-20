/*************************************************************************************
 Created Date : 2017.12.20
      Creator : Shin Kwang Hee
   Decription : 전지 5MEGA-GMES 구축 - 소형조립 공정진척(Assembly용) CMM_ASSY_CANCEL_TERM 파일 참조 
--------------------------------------------------------------------------------------
 [Change History]
  2017.12.20  Shin Kwang Hee : Initial Created.
  2020.05.27  김동일    : C20200513-000349 재고 및 수율 정합성 향상을 위한 투입Lot 종료 취소에 대한 기능변경
  2023.03.13  LEEHJ     : 소형활성화 MES 복사
**************************************************************************************/


using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using C1.WPF.DataGrid;
using System.Windows.Media;

namespace LGC.GMES.MES.FCS002
{

    public partial class FCS002_305_LOT_SEARCH : C1Window, IWorkArea
    {
        #region Declaration & Constructor         
        private string _processCode = string.Empty;
        public string lotCode = string.Empty;
        private readonly BizDataSet _bizDataSet = new BizDataSet();

        #endregion

        #region Initialize 
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public FCS002_305_LOT_SEARCH(string lotCode)
        {
            InitializeComponent();

            SEARCH_LOT(lotCode);
        }
        #endregion

        #region Event
        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtLotID_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(lotCode))
                {
                    Util.MessageValidation("SFU1636");
                    return;
                }
                this.Close();
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod



        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string lotCode = txtLotCode.Text.ToString();

                if (string.IsNullOrEmpty(lotCode))
                {
                    Util.MessageValidation("SFU8249"); // LotCode를 1자리 이상 입력해주세요.
                    return;
                }

                SEARCH_LOT(lotCode);
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }


        }

        private void dgLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            lotCode = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "LOTID").ToString());
        }

        private void SEARCH_LOT(string lotCode)
        {
            try
            {
                DataTable dtRqstDt = new DataTable();
                dtRqstDt.TableName = "RQSTDT";
                dtRqstDt.Columns.Add("LOTCODE", typeof(string));

                DataRow drnewrow = dtRqstDt.NewRow();
                drnewrow["LOTCODE"] = lotCode;
                dtRqstDt.Rows.Add(drnewrow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_ASSY_LOT", "RQSTDT", "RSLTDT", dtRqstDt);

                Util.GridSetData(dgLotId, dtResult, FrameOperation, true);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
    }
    #endregion
}
