/*************************************************************************************
 Created Date : 2019.05.29
      Creator : INS 김동일K
   Decription : 전지 GMES 고도화 - 특이사항(비고) 관리 공통 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2019.05.29  INS 김동일K : Initial Created.
  
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_COM_WIP_NOTE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_COM_WIP_NOTE : C1Window, IWorkArea
    {
        private string _Eqptid = string.Empty;
        private string _Eqsgid = string.Empty;
        private string _Procid = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_COM_WIP_NOTE()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                _Eqsgid = Util.NVC(tmps[0]);
                _Eqptid = Util.NVC(tmps[1]);
                _Procid = Util.NVC(tmps[2]);

                ApplyPermissions();

                GetWipNote();
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
                if (!CanSave())
                    return;

                Util.MessageConfirm("SFU1241", (result) =>// 저장 하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveWipNote();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();

            listAuth.Add(btnSave);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private bool CanSave()
        {
            bool bRet = false;
            
            //if (txtRemark.Text.Trim().Equals(""))
            //{
            //    Util.MessageValidation("SFU1590"); // 비고를 입력하세요.
            //    return bRet;
            //}            

            bRet = true;
            return bRet;
        }

        private void SaveWipNote()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                                
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("WIPSEQ", typeof(string));
                inDataTable.Columns.Add("WIP_NOTE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string)); 
                
                foreach (C1.WPF.DataGrid.DataGridRow row in dgList.Rows)
                {
                    if (row.Type != DataGridRowType.Item) continue;

                    if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).Equals("1"))
                    {
                        DataRow param = inDataTable.NewRow();
                        param["LOTID"] = DataTableConverter.GetValue(row.DataItem, "LOTID");
                        param["WIPSEQ"] = DataTableConverter.GetValue(row.DataItem, "WIPSEQ");
                        param["WIP_NOTE"] = DataTableConverter.GetValue(row.DataItem, "WIP_NOTE");
                        param["USERID"] = LoginInfo.USERID;

                        inDataTable.Rows.Add(param);
                    }
                }

                if (inDataTable.Rows.Count < 1)
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    Util.MessageValidation("MMD0002");      //저장할 데이터가 존재하지 않습니다.
                    return;
                }

                new ClientProxy().ExecuteService("BR_PRD_REG_WIPHISTORY_NOTE", "INDATA", null, inDataTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");      //정상 처리 되었습니다.

                        GetWipNote();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void GetWipNote()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable inTable = null;

                //BizDataSet에 메소드를 추가하지 않고 직접 칼럼을 작성을 했습니다.
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("STDT", typeof(string));
                inDataTable.Columns.Add("EDDT", typeof(string));

                inTable = inDataTable;

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _Procid;
                newRow["EQSGID"] = _Eqsgid;
                newRow["EQPTID"] = _Eqptid;
                newRow["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                newRow["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_PROD_LOT_INFO_L", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgList.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgList, searchResult, null, true);                        
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }
                );
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                    return;

                LGCDatePicker dtPik = (sender as LGCDatePicker);

                if (string.Equals(dtPik.Tag, "CHANGE"))
                {
                    dtPik.Tag = null;
                    return;
                }

                if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                {
                    dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                    Util.MessageValidation("SFU3231");  // 종료시간이 시작시간보다 이전입니다

                    return;
                }
            }));
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                    return;

                LGCDatePicker dtPik = (sender as LGCDatePicker);

                if (string.Equals(dtPik.Tag, "CHANGE"))
                {
                    dtPik.Tag = null;
                    return;
                }

                if(Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                {
                    dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                    Util.MessageValidation("SFU1698");  //시작일자 이전 날짜는 선택할 수 없습니다.

                    return;
                }
            }));
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetWipNote();
        }

        private void WIP_NOTE_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBox textbox = sender as TextBox;

                C1.WPF.DataGrid.DataGridRow row = new C1.WPF.DataGrid.DataGridRow();
                IList<FrameworkElement> ilist = textbox.GetAllParents();

                foreach (var item in ilist)
                {
                    DataGridRowPresenter presenter = item as DataGridRowPresenter;
                    if (presenter != null)
                    {
                        row = presenter.Row;
                    }
                }
                dgList.SelectedItem = row.DataItem;

                if (textbox != null)
                {
                    //if (DataTableConverter.GetValue(dgList.SelectedItem, "WRK_DATE").GetString() != GetEquipmentWorkDate())
                    //{
                    //    //Util.MessageValidation("수정 가능한 작업일자의 데이터가 아닙니다.");
                    //    //Util.MessageValidation("SFU3123");
                    //    textbox.IsReadOnly = true;
                    //    return;
                    //}
                    //else
                    //{
                        textbox.IsReadOnly = false;
                        textbox.SelectionStart = textbox.Text.Length;
                    //}

                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
