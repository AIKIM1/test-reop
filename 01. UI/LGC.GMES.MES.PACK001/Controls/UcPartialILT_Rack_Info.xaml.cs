/*************************************************************************************
 Created Date : 2023.04.05
      Creator : 김선준
   Decription : Partial ILT - Rack Lot List UserControl
--------------------------------------------------------------------------------------
 [Change History]
   2023.04.05   김선준  : Created
**************************************************************************************/
using LGC.GMES.MES.PACK001.Class;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Media;
using System.Windows.Media;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.PACK001.Controls
{
    public delegate void DataGetLotEventHandler(bool data, string sRack, string sRackName);

    public partial class UcPartialILT_Rack_Info : UserControl 
    {
        public event DataGetLotEventHandler rackLotEvent;
        private DataRow _row; 
        private string _rackID = string.Empty;
        private string _rackName = string.Empty;

        #region Constructor...
        public UcPartialILT_Rack_Info()
        {
            InitializeComponent(); 
        }


        /// <summary>
        /// Rack Setting
        /// </summary>
        /// <param name="row"></param>
        public void setRackInfo(DataRow row)
        {
            this._row = row;
            this._rackID = _row["RACK_ID"].ToString();
            this._rackName = _row["PARENT_RACK_NAME"].ToString();

            string sDispColor = _row["DISP_COLOR"].ToString();
            BrushConverter conv = new BrushConverter();
            this.grdMain.Background = conv.ConvertFromString(sDispColor) as SolidColorBrush;

            this.txtRackID.Text = ObjectDic.Instance.GetObjectName("RACK_ID");
            this.txtPARENT_RACK_NAME.Text = ObjectDic.Instance.GetObjectName("RACK_NAME");
            this.txtPROCID.Text = ObjectDic.Instance.GetObjectName("PROCID");
            this.txtLOT_CNT.Text = ObjectDic.Instance.GetObjectName("LOT_CNT");
            this.txtWIPHOLD.Text = ObjectDic.Instance.GetObjectName("WIPHOLD");
            this.txtAGINGHOLD.Text = ObjectDic.Instance.GetObjectName("AGINGHOLD");
            this.txtINPUT_DATE.Text = ObjectDic.Instance.GetObjectName("RACK 입고일시");
            this.txtAgingDay.Text = ObjectDic.Instance.GetObjectName("AgingDay");
            this.txtWIPDTTM_ED.Text = ObjectDic.Instance.GetObjectName("1ST OCV END");
            this.txtAGINGDAY_OVER_CNT.Text = _row["AGINGDAY_OVER_NAME"].ToString();

            this.bdrRoot.DataContext = row;

            if (_row["LOT_CNT"].ToString().Equals("0"))
            {
                this.tbAGINGHOLD.Text = "";
                this.tbAgingDay.Text = "";
                this.tbWIPHOLD.Text = "";
                this.tbINPUT_DATE.Text = "";
                this.tbWIPDTTM_ED.Text = "";
                this.tbAGINGDAY_OVER_CNT.Text = "";
            }
            else
            {
                if (string.IsNullOrEmpty(_row["AgingDay"].ToString()) || _row["AgingDay"].ToString().Equals("-1"))
                {
                    this.tbAgingDay.Text = "";
                }
                else
                {
                    this.tbAgingDay.Text = Convert.ToDecimal(_row["AgingDay"]).ToString("#,##0");
                }
            }
        }

        /// <summary>
        /// Rack List 호출
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bdrRoot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (null != rackLotEvent)
            {
                rackLotEvent(true, this._rackID, this._rackName);
            }
        }
        #endregion
    }
}