/*************************************************************************************
 Created Date : 2017.07.03
      Creator : 두잇 이선규K
   Decription : 전지 5MEGA-GMES 구축 - DSF 대기창고 관리 - 재입고
--------------------------------------------------------------------------------------
 [Change History]
  2017.07.03  두잇 이선규K : Initial Created.
  2017.09.18  INS  김동일K : 조립 Prj 에서 활성화 Prj 로 소스코드 이동
 **************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.FORM001
{
    /// <summary>
    /// FORM001_051_RERECEPTION.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FORM001_051_RERECEPTION : C1Window, IWorkArea
    {   
        #region Declaration & Constructor

        string _ProcID = string.Empty;
        string _LotID = string.Empty;

        private Util _Util = new Util();
        private BizDataSet _Biz = new BizDataSet();

        #endregion        

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FORM001_051_RERECEPTION()
        {
            InitializeComponent();
        }

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

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 2)
                {
                    _ProcID = Util.NVC(tmps[0]);
                    _LotID = Util.NVC(tmps[1]);
                }
                else
                {
                    _ProcID = string.Empty;
                    _LotID = string.Empty;
                }

                txtTrayID.Text = string.Empty;
                ApplyPermissions();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region [EVENT]

        #region [Main Window]

        private void txtTrayID_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    txtTrayID.Text = txtTrayID.Text.Trim();
                    txtTrayID.Select(txtTrayID.Text.Length, 0);

                    GetTrayInfo();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetTrayInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnReStock_Clicked(object sender, RoutedEventArgs e)
        {
            if (!CanTrayReStock())
                return;

            Util.MessageConfirm("SFU2073", (result) => // 입고 하시겠습니까?
            {
                if (result == MessageBoxResult.OK)
                {
                    TrayReStock();
                }
            });
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = MessageBoxResult.Cancel;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion [Main Window]

        #region [Main Grid]        

        #endregion [Main Grid]

        #endregion [EVENT]

        #region [Biz]

        private void GetTrayInfo()
        {
            try
            {
                if (!CanSearch())
                    return;

                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_TRAY_RESTOCK();

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = _ProcID;
                dr["CSTID"] = txtTrayID.Text;

                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_TRAY_RESTOCK", "RQSTDT", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        DataTable dtRst = null;
                        if (dgList != null && dgList.Rows.Count - dgList.FrozenTopRowsCount - dgList.FrozenBottomRowsCount > 0)
                        {
                            // 기존 내용과 머지
                            dtRst = DataTableConverter.Convert(dgList.ItemsSource);
                            dtRst.Merge(searchResult, true);
                        }
                        else
                        {
                            dtRst = searchResult;
                        }
                        Util.GridSetData(dgList, dtRst, null, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void TrayReStock()
        {
            try
            {
                ShowLoadingIndicator();

                DataRow newRow = null;
                DataSet inDataSet = _Biz.GetBR_PRD_REG_TRAY_RESTOCK();

                DataTable inTable = inDataSet.Tables["INDATA"];
                newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                newRow = null;
                DataTable inputLOT = inDataSet.Tables["INLOT"];
                for (int i = dgList.TopRows.Count; i < dgList.Rows.Count - dgList.BottomRows.Count; i++)
                {
                    newRow = inputLOT.NewRow();
                    newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "TRAYID"));

                    inputLOT.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_TRAY_RESTOCK", "INDATA,INLOT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.gridClear(dgList);
                        Util.MessageValidation("SFU1275"); // 정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion [Biz]

        #region [Validation]

        private bool CanSearch()
        {
            bool bRet = false;

            if (txtTrayID.Text.Length < 1)
                return bRet;

            int idx = _Util.GetDataGridRowIndex(dgList, "TRAYID", txtTrayID.Text);
            if (idx >= 0)
            {
                Util.MessageValidation("SFU3056"); // 이미 입력한 TRAY ID 입니다.
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanTrayReStock()
        {
            bool bRet = false;

            int listCount = 0;
            for (int i = dgList.TopRows.Count; i < dgList.Rows.Count - dgList.BottomRows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "TRAYID")).Length > 0)
                {
                    listCount++;
                    break;
                }
            }
            if (listCount < 1)
            {
                Util.MessageValidation("SFU1636"); // 선택된 대상이 없습니다.
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        #endregion

        #region [FUNC]

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();

            listAuth.Add(btnSearch);
            listAuth.Add(btnReStock);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion [FUNC]
    }
}
