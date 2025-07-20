/*************************************************************************************
 Created Date : 2023.06.27
      Creator : 김선준
   Decription : Partial ILT - Rack UserControl
--------------------------------------------------------------------------------------
 [Change History]
   2023.03.30   김선준  : Partial ILT Rack UserControl 
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

namespace LGC.GMES.MES.PACK001.Controls
{
    public delegate void StkDataGetEventHandler(string rack_ID, string rack_Name);

    public partial class UcPackSTK_Rack : UserControl, ISocketSTKObjectEventHandler
    {
        public event StkDataGetEventHandler rackEvent;
        private DataRow _row; 
        private string _rackID = string.Empty; 
        private string _reverse = string.Empty;

        #region Constructor...
        public UcPackSTK_Rack()
        {
            InitializeComponent();
            this.Loaded += (s, e) =>
            {
                PackSTK_DataManager.Instance.AddSTKObjectEventHandler(this as ISocketSTKObjectEventHandler);
            };
            this.Unloaded += (s, e) =>
            {
                PackSTK_DataManager.Instance.RemoveSTKObjectEventHandler(this as ISocketSTKObjectEventHandler);
            };
        }

        /// <summary>
        /// Rack reverse
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void grTitle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        { 
            rackEvent(this._rackID, string.Empty);
        }

        /// <summary>
        /// Rack Setting
        /// </summary>
        /// <param name="row"></param>
        public void setRackInfo(DataRow row)
        {
            try
            {
                _row = row;
                this._rackID = _row["RACK_ID"].ToString(); 
                this._reverse = _row["DISP_REVERSE"].ToString();

                string sRACK_STAT_CODE = _row["RACK_STAT_CODE"].ToString();

                if (!string.IsNullOrEmpty(_row["MCS_CST_ID"].ToString()) && !_row["MCS_CST_ID"].ToString().ToUpper().Equals("ROW"))
                {
                    this.grRCV.Visibility = Visibility.Visible;
                    BrushConverter conv = new BrushConverter();
                    this.grTitle.Background = conv.ConvertFromString(_row["RCV_COLOR"].ToString()) as SolidColorBrush;
                    this.grRCV.ColumnDefinitions[0].Width = new GridLength(3, GridUnitType.Pixel);
                }
                else
                {
                    this.grRCV.Visibility = Visibility.Collapsed;
                    this.grRCV.ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Pixel);
                }
                 
                //RACK없음 체크
                if (sRACK_STAT_CODE.Equals("DISABLE"))
                {
                    this.grTitle.Visibility = Visibility.Hidden;
                    this.grTitle1.Visibility = Visibility.Hidden;
                    this.grNotRack.Visibility = Visibility.Visible;
                    this._reverse = "N";
                }
                else
                {
                    string sDispColor = _row["DISP_COLOR"].ToString();
                    string sForeColor = _row["FORE_COLOR"].ToString();
                    string sRackName = _row["RACK_NAME"].ToString();
                    string sLotCnt = _row["LOT_CNT_DISP"].ToString();

                    if (string.IsNullOrEmpty(sRackName))
                    {
                        this.grTitle.Visibility = Visibility.Visible;
                        this.grTitle1.Visibility = Visibility.Hidden;
                        this.tbRackName.Text = sLotCnt;
                    }
                    else
                    {
                        if (row["DISP_REVERSE"].ToString().Equals("N"))
                        {
                            this.grTitle.Visibility = Visibility.Visible;
                            this.grTitle1.Visibility = Visibility.Hidden;
                            this.tbRackName.Text = sRackName;

                        }
                        else
                        {
                            this.grTitle.Visibility = Visibility.Hidden;
                            this.grTitle1.Visibility = Visibility.Visible;
                            this.tbStg1.Text = sLotCnt;
                            this.tbStg2.Text = sRackName;
                        }

                    }

                    BrushConverter conv = new BrushConverter();
                    this.grTitle.Background = conv.ConvertFromString(sDispColor) as SolidColorBrush;
                    this.grTitle1.Background = conv.ConvertFromString(sDispColor) as SolidColorBrush;
                    this.lbRackName.Foreground = conv.ConvertFromString(sForeColor) as SolidColorBrush;
                    this.lbStg1.Foreground = conv.ConvertFromString(sForeColor) as SolidColorBrush;
                    this.lbStg2.Foreground = conv.ConvertFromString(sForeColor) as SolidColorBrush;
                    this.tbRackName.Foreground = conv.ConvertFromString(sForeColor) as SolidColorBrush;
                    this.tbStg1.Foreground = conv.ConvertFromString(sForeColor) as SolidColorBrush;
                    this.tbStg2.Foreground = conv.ConvertFromString(sForeColor) as SolidColorBrush;

                }
            }
            catch (Exception ex)
            {
                //throw;
                MessageBox.Show(ex.Message);
            }             
        }

        /// <summary>
        /// Rack List 호출
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rackLotEvent(bool bSearch = false)
        { 
            if (null != rackEvent && bSearch)
            {
                rackEvent(this._rackID, string.Empty);
            }
        }

        /// <summary>
        /// Rack Reverse
        /// </summary>
        private void sbRackSet()
        {
            if (this._reverse.Equals("N")) return;
        }

        #region ISocketSTKObjectEventHandler
        // 변경된 STKObject를 포함하는 Folder를 표시하고 있는 경우 Reload
        // 해당 STKObject의 속성을 보여주는 화면에서  reload
        public void OnSTKObjectPropertyModified(STKObjectFolder _STKObjectFolder)
        {
            try
            {
                sbRackSet();
            }
            catch
            {
            }
        }
        #endregion ISocketSTKObjectEventHandler


        #endregion

    }
}