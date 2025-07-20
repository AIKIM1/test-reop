/*************************************************************************************
 Created Date : 2023.03.30
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
    public delegate void DataGetEventHandler(string rack_ID, string rack_Name);

    public partial class UcPartialILT_Rack : UserControl, ISocketILTObjectEventHandler
    {
        public event DataGetEventHandler rackEvent;
        private DataRow _row; 
        private string _rackID = string.Empty;
        private string _rackName = string.Empty;

        #region Constructor...
        public UcPartialILT_Rack()
        {
            InitializeComponent();
            this.Loaded += (s, e) =>
            {
                PackILT_DataManager.Instance.AddILTObjectEventHandler(this as ISocketILTObjectEventHandler);
            };
            this.Unloaded += (s, e) =>
            {
                PackILT_DataManager.Instance.RemoveILTObjectEventHandler(this as ISocketILTObjectEventHandler);
            };
        }

        /// <summary>
        /// Rack reverse
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void grTitle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //sbRackSet();
            rackEvent(this._rackID, this._rackName);
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
                this._rackName = _row["PARENT_RACK_NAME"].ToString();

                string sDispColor = _row["DISP_COLOR"].ToString();
                this.tbLotCnt.Text = Convert.ToDecimal(_row["LOT_CNT"]).ToString("#,##0");
                if (_row["HOLD_DISP_YN"].ToString() == "Y" && _row["HOLD_YN"].ToString() == "Y")
                {
                    this.tbRackName.Text = "HOLD";
                }
                else
                {
                    if (this._rackName.ToUpper().Contains("VIRTUAL"))
                    {
                        string[] splt = _row["RACK_NAME"].ToString().Split('(');
                        if (splt.Length > 1)
                        {
                            int iLen0 = splt[0].Length;
                            int iLen1 = splt[1].Length;
                            if (iLen0 > iLen1)
                            {
                                int iLen = (iLen0 - iLen1) / 2;
                                if (iLen == 0) iLen++;
                                this.tbRackName.Text = string.Format("{0}{1}{2}({3}{4}", splt[0], "\r\n", new string(' ', iLen - 1), splt[1], new string(' ', iLen));
                            }
                            else if (iLen0 < iLen1)
                            {
                                int iLen = (iLen1 - iLen0) / 2;
                                if (iLen == 0) iLen++;
                                this.tbRackName.Text = string.Format("{0}{1}{2}{3}({4}", new string(' ', iLen), splt[0], new string(' ', iLen - 1), "\r\n", splt[1]);
                            }
                            else
                            {
                                this.tbRackName.Text = string.Format("{0}{1}({2}", splt[0], "\r\n", splt[1]);
                            }
                        }
                        else
                        {
                            this.tbRackName.Text = _row["RACK_NAME"].ToString();
                        }
                    }
                    else
                    {
                        this.tbRackName.Text = _row["RACK_NAME"].ToString();
                    }

                }

                BrushConverter conv = new BrushConverter();
                this.lbRackName.Background = conv.ConvertFromString(sDispColor) as SolidColorBrush;
                this.lbLotCnt.Background = conv.ConvertFromString(sDispColor) as SolidColorBrush;
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
                rackEvent(this._rackID, this._rackName);
            }
        }

        /// <summary>
        /// Rack Reverse
        /// </summary>
        private void sbRackSet()
        {
            if (this.grTitle.Visibility == Visibility.Visible)
            {
                this.grTitle.Visibility = Visibility.Hidden;
                this.grLotCnt.Visibility = Visibility.Visible;
            }
            else
            {
                this.grTitle.Visibility = Visibility.Visible;
                this.grLotCnt.Visibility = Visibility.Hidden;

            }
        }

        #region ISocketILTObjectEventHandler
        // 변경된 ILTObject를 포함하는 Folder를 표시하고 있는 경우 Reload
        // 해당 ILTObject의 속성을 보여주는 화면에서  reload
        public void OnILTObjectPropertyModified(Dictionary<bool, List<bool>> modifiedObjectPathDic)
        {
            try
            {
                sbRackSet();
            }
            catch
            {
            }
        }
        #endregion ISocketILTObjectEventHandler


        #endregion

    }
}