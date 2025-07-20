/*************************************************************************************
  Created Date : 2020.11.27
  Creator : 김길용
  Decription :
--------------------------------------------------------------------------------------
  [Change History]
  2020.11.27  김길용 : Initial Created.
  2021.02.03  정용석 : 라인ID Parameter 추가
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_005_INPUT_INFO : C1Window, IWorkArea
    {
        #region Initialize

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public PACK003_005_INPUT_INFO()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] objs = C1WindowExtension.GetParameters(this);
            if (objs != null && objs.Length >= 1)
            {
                string prodID = Util.NVC(objs[0]);
                string holdFlag = Util.NVC(objs[1]);
                string eqsgID = Util.NVC(objs[2]);
                LoadSearch(string.Empty, string.Empty, string.Empty, prodID, holdFlag, eqsgID);
            }
        }
        #endregion

        #region [ Event ]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }
        #endregion

        #region [ Mehod ]
        //최초 로드시 제품과 홀드여부로 조회(이후 CST,PLT,LOT 조회 가능하도록 구현)
        private void LoadSearch(string carrierID, string palletID, string cellID, string prodID, string holdFlag, string eqsgID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("CSTID", typeof(string));
                RQSTDT.Columns.Add("PLTID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("HOLD", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = string.IsNullOrEmpty(eqsgID) ? null : eqsgID.ToString();
                dr["CSTID"] = string.IsNullOrEmpty(carrierID) ? null : carrierID.ToString();
                dr["PLTID"] = string.IsNullOrEmpty(palletID) ? null : palletID.ToString();
                dr["LOTID"] = string.IsNullOrEmpty(cellID) ? null : cellID.ToString();
                dr["HOLD"] = string.IsNullOrEmpty(holdFlag) ? null : holdFlag.ToString();
                dr["PRODID"] = string.IsNullOrEmpty(prodID) ? null : prodID.ToString();
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_LOGIS_STK_INPUTCELL_INFO", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {
                    if (dtResult.Rows.Count == 0)
                    {
                        Util.MessageInfo("SFU1801");
                        txtCellId.Text = string.Empty;
                        Util.SetTextBlockText_DataGridRowCount(txRightRowCnt, "0");
                        return;
                    }

                    if (dtResult.Rows.Count != 0)
                    {
                        Util.GridSetData(dgSearchCell, dtResult, FrameOperation);
                    }

                    Util.SetTextBlockText_DataGridRowCount(txRightRowCnt, Util.NVC(dtResult.Rows.Count));
                });
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }

        }

        //초기화
        private void clear()
        {
            try
            {
                Util.gridClear(dgSearchCell);
                Util.SetTextBlockText_DataGridRowCount(txRightRowCnt, "0");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        //조회 클릭 이벤트
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(txtCellId.Text.ToString()))
                {
                    if (rdoCst.IsChecked == true)
                    {
                        string carrierID = this.txtCellId.Text.ToString();
                        string palletID = string.Empty;
                        string cellID = string.Empty;
                        LoadSearch(carrierID, palletID, cellID, string.Empty, string.Empty, string.Empty);
                    }
                    if (rdoPlt.IsChecked == true)
                    {
                        string carrierID = string.Empty;
                        string palletID = txtCellId.Text.ToString();
                        string cellID = string.Empty;
                        LoadSearch(carrierID, palletID, cellID, string.Empty, string.Empty, string.Empty);
                    }
                    if (rdoCell.IsChecked == true)
                    {
                        string carrierID = string.Empty;
                        string palletID = string.Empty;
                        string cellID = txtCellId.Text.ToString();
                        LoadSearch(carrierID, palletID, cellID, string.Empty, string.Empty, string.Empty);
                    }

                    clear();
                }
                else
                {
                    Util.MessageInfo("SFU1801");
                    txtCellId.Text = string.Empty;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtCellId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnSearch_Click(null, null);
            }
        }
        #endregion
    }
}