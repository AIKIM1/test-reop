/*************************************************************************************
 Created Date : 
      Creator : Developer
  Description : 예외대상
--------------------------------------------------------------------------------------
 [Change History]  
  2023.03.15  LEEHJ     : 소형활성화 MES 복사
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.FCS002
{
    /// <summary>
    /// FCS002_311_PACK_EXCEPTION_POPUP.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS002_311_PACK_EXCEPTION_POPUP : C1Window
    {
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FCS002_311_PACK_EXCEPTION_POPUP()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                DataTable dtLotList = (DataTable)tmps[0];
                string sType = tmps[1].ToString();

                if (sType.Equals("RELEASE_LOT")) // FCS002_311_Pack - HOLD 릴리즈요청 화면 
                {
                    foreach (var col in dgExceptLotList.Columns)
                    {
                        switch (col.Index)
                        {
                            case 0:  // INPUT_LOTID
                            case 5:  // 제품ID
                            case 6:  // 등급품코드
                            case 7:  // 홀드사유
                            case 8:  // 공정
                                col.Visibility = Visibility.Collapsed;
                                break;
                        }
                    }

                    for (int i = 0; i < dtLotList.Rows.Count; i++)
                    {

                        if (!string.IsNullOrEmpty(dtLotList.Rows[i]["REQ_NO"].ToString()))
                        {
                            dtLotList.Rows[i]["NOTE"] = ObjectDic.Instance.GetObjectName("요청 또는 승인 중입니다.");
                        }
                        else if (dtLotList.Rows[i]["WIPHOLD"].ToString().Equals("N"))
                        {
                            dtLotList.Rows[i]["NOTE"] = ObjectDic.Instance.GetObjectName("해당 재공은 홀드 상태가 아닙니다.");
                        }
                        else if (dtLotList.Rows[i]["WIPSTAT"].ToString().Equals("TERM"))
                        {
                            dtLotList.Rows[i]["NOTE"] = MessageDic.Instance.GetMessage("SFU8119"); // 해당 LOT은 재공종료 상태입니다.
                        }

                    }
                }
                else if (sType.Equals("HOLD_LOT")) // PACK001_060 - Lot 홀드 요청 화면
                {
                    foreach (var col in dgExceptLotList.Columns)
                    {
                        switch (col.Index)
                        {
                            case 2:  // REQ_NO
                            case 3:  // WIPHOLD
                            case 5:  // 제품ID
                            case 6:  // 등급품코드
                            case 7:  // 홀드사유
                            case 8:  // 공정
                                col.Visibility = Visibility.Collapsed;
                                break;
                        }
                    }
                }
                else if (sType.Equals("BATCH_PROCESSING_OFFGRADE")) // PACK001_075 - 일괄처리 화면
                {
                    foreach (var col in dgExceptLotList.Columns)
                    {
                        switch (col.Index)
                        {
                            case 0:  // INPUT_LOTID
                            case 2:  // REQ_NO
                            case 3:  // WIPHOLD
                            case 7:  // 홀드사유
                            case 8:  // 공정
                                col.Visibility = Visibility.Collapsed;
                                break;
                        }
                    }
                }
                else if (sType.Equals("BATCH_PROCESSING_RETURN")) // PACK001_075 - 일괄처리 화면
                {
                    foreach (var col in dgExceptLotList.Columns)
                    {
                        switch (col.Index)
                        {
                            case 0:  // INPUT_LOTID
                            case 2:  // REQ_NO
                            case 4:  // WIPSTAT
                            case 5:  // 제품ID
                            case 6:  // 등급품코드
                                col.Visibility = Visibility.Collapsed;
                                break;
                        }
                    }
                }

                Util.GridSetData(dgExceptLotList, dtLotList, FrameOperation);
            }
        }
    }
}
