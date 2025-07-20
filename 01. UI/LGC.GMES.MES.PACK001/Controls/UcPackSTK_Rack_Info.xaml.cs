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
    public delegate void StkDataGetRackEventHandler(bool data, string sRack, string sRackName);

    public partial class UcPackSTK_Rack_Info : UserControl 
    {
        public event StkDataGetRackEventHandler rackLotEvent;
        private DataRow _row; 
        private string _rackID = string.Empty;
        private string _rackName = string.Empty;

        #region Constructor...
        public UcPackSTK_Rack_Info()
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
            this._rackName = _row["RACK_NAME"].ToString();

            string sDispColor = _row["DISP_COLOR"].ToString();
            string sForeColor = _row["Fore_COLOR"].ToString();
            BrushConverter conv = new BrushConverter();
            this.grdMain.Background = conv.ConvertFromString(sDispColor) as SolidColorBrush;

            #region Control ForeCplor
            this.txtRackID.Foreground = conv.ConvertFromString(sForeColor) as SolidColorBrush;
            this.tbRACK_ID.Foreground = conv.ConvertFromString(sForeColor) as SolidColorBrush;
            this.txtXYZ.Foreground = conv.ConvertFromString(sForeColor) as SolidColorBrush;
            this.tbXYZ.Foreground = conv.ConvertFromString(sForeColor) as SolidColorBrush;
            this.txtRackStat.Foreground = conv.ConvertFromString(sForeColor) as SolidColorBrush;
            this.tbRackStat.Foreground = conv.ConvertFromString(sForeColor) as SolidColorBrush;
            this.txtAbnormal.Foreground = conv.ConvertFromString(sForeColor) as SolidColorBrush;
            this.tbAbnormal.Foreground = conv.ConvertFromString(sForeColor) as SolidColorBrush;
            this.txtCst_Id.Foreground = conv.ConvertFromString(sForeColor) as SolidColorBrush;
            this.tbCst_Id.Foreground = conv.ConvertFromString(sForeColor) as SolidColorBrush;
            this.txtRCV_DTTM.Foreground = conv.ConvertFromString(sForeColor) as SolidColorBrush;
            this.tbRCV_DTTM.Foreground = conv.ConvertFromString(sForeColor) as SolidColorBrush;
            this.txtZONE_ID.Foreground = conv.ConvertFromString(sForeColor) as SolidColorBrush;
            this.tbZONE_ID.Foreground = conv.ConvertFromString(sForeColor) as SolidColorBrush;
            #endregion

            this.txtRackID.Text = ObjectDic.Instance.GetObjectName("LOC");
            this.txtXYZ.Text = ObjectDic.Instance.GetObjectName("상세위치");
            this.txtRackStat.Text = ObjectDic.Instance.GetObjectName("RACK 상태");
            this.txtAbnormal.Text = ObjectDic.Instance.GetObjectName("비정상사유");
            this.txtCst_Id.Text = ObjectDic.Instance.GetObjectName("CST ID");
            this.txtRCV_DTTM.Text = ObjectDic.Instance.GetObjectName("RCV_DATE");
            this.txtZONE_ID.Text = ObjectDic.Instance.GetObjectName("ZONE_ID");
             
            this.bdrRoot.DataContext = row;
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