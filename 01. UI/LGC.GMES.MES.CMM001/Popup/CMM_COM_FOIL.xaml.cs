/*************************************************************************************
 Created Date : 2017.03.06
      Creator : 유관수
   Decription : 전지 5MEGA-GMES 구축 - 추가기능 - 믹서 자주검사 입력 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.03.06  유관수 : Initial Created.
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
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_COM_ELEC_MIXCONFIRM.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_COM_FOIL : C1Window, IWorkArea
    {
        #region < Declaration & Constructor >
                
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion < Declaration & Constructor >


        #region < Initialize >

        public CMM_COM_FOIL()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();            
            
            Loaded -= C1Window_Loaded;
        }

        #endregion < Initialize >


        #region < Event >        

        private void txtFoilID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    if (!CheckDuplicate(Util.NVC(txtFoilID.Text)))
                    {
                        return;
                    }

                    DataTable dt = DoFind(Util.NVC(txtFoilID.Text));

                    if(dgList.ItemsSource == null)
                    {
                        dgList.ItemsSource = DataTableConverter.Convert(dt);
                    }
                    else
                    {
                        DataTable dtGrid = (dgList.ItemsSource as DataView).ToTable();
                        dtGrid.Merge(dt);
                        dgList.ItemsSource = DataTableConverter.Convert(dtGrid);
                    }

                    txtFoilID.SelectAll();
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
                if (!CheckDuplicate(Util.NVC(txtFoilID.Text)))
                {
                    return;
                }

                DataTable dt = DoFind(Util.NVC(txtFoilID.Text));

                if (dgList.ItemsSource == null)
                {
                    dgList.ItemsSource = DataTableConverter.Convert(dt);
                }
                else
                {
                    DataTable dtGrid = (dgList.ItemsSource as DataView).ToTable();
                    dtGrid.Merge(dt);
                    dgList.ItemsSource = DataTableConverter.Convert(dtGrid);
                }

                txtFoilID.Text = string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);                
            }            
        }

        private void btnBarCode_Click(object sender, RoutedEventArgs e)
        {
            try
            {

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
                DoSave();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);                
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void DataGridTextColumn_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (dgList.Rows.Count > 0 && dgList.CurrentColumn.Name == "QTY")
            {
                if (dgList.CurrentRow != null && dgList.CurrentColumn != null)
                {
                    int indexRow = dgList.CurrentRow.Index;
                    int indexCol = dgList.CurrentColumn.Index;

                    double Width = Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgList.Rows[indexRow].DataItem, "WIDTH")));
                    double Thick = Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgList.Rows[indexRow].DataItem, "THICK")));
                    double Rate = Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgList.Rows[indexRow].DataItem, "CONV_RATE")));
                    double Qty = Convert.ToDouble(Util.NVC(dgList.CurrentCell.Value));
                    double Weight = Math.Round(Width * Thick * Rate * Qty / 1000000, 2);

                    DataTableConverter.SetValue(dgList.Rows[indexRow].DataItem, "MLOT_INPUT_WEIGHT", Weight);
                }         
            }           
        }

        #endregion < Event >


        #region < Method >

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button> { btnSearch, btnClose };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private bool CheckDuplicate(string BarCode)
        {
            if (dgList.ItemsSource == null)
            {
                return true;
            }

            DataView view = (dgList.ItemsSource as DataView).ToTable().AsDataView();

            if (view == null)
            {
                return true;
            }
            else
            {
                view.RowFilter = string.Format("MLOTID = {0}", BarCode);

                if (view.Count > 0)
                {
                    /// 중복 스캔되었습니다. ///
                    Util.MessageInfo("SFU1914");
                    return false;
                }
            }            

            return true;
        }

        private DataTable DoFind(string BarCode)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("MLOTID");

                DataRow dr = RQSTDT.NewRow();
                dr[0] = Util.NVC(txtFoilID.Text);
                RQSTDT.Rows.Add(dr);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MLOT_FOIL_INFO", "RQSTDT", "RSLDT", RQSTDT);
                return dt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;              
            }
        }

        private void DoSave()
        {
            try
            {
                DataSet InDataSet = new DataSet();

                DataTable InData = new DataTable("INDATA");
                InData.Columns.Add("SRCTYPE");
                InData.Columns.Add("IFMODE");
                InData.Columns.Add("MLOTID");
                InData.Columns.Add("USERID");

                DataTable InData2 = new DataTable("IN_INPUT");
                InData2.Columns.Add("MLOTID");
                InData2.Columns.Add("MLOTQTY_CUR");
                InData2.Columns.Add("MLOT_INPUT_WEIGHT");

                InDataSet.Tables.Add(InData);
                InDataSet.Tables.Add(InData2);

                for (int i = 0; i < dgList.Rows.Count; i++)
                {
                    if (DataTableConverter.GetValue(dgList.Rows[i].DataItem, "CHK").ToString() == "True")
                    {
                        DataRow dr1 = InData.NewRow();
                        dr1["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        dr1["IFMODE"] = "OFF";
                        dr1["MLOTID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "MLOTID"));
                        dr1["USERID"] = LoginInfo.USERID;
                        InData.Rows.Add(dr1);

                        DataRow dr2 = InData2.NewRow();
                        dr2["MLOTID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "MLOTID"));
                        dr2["MLOTQTY_CUR"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "MLOTQTY_CUR"));
                        dr2["MLOT_INPUT_WEIGHT"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "MLOT_INPUT_WEIGHT"));
                        InData2.Rows.Add(dr2);
                    }
                }

                if (InData.Rows.Count > 0)
                {
                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_USE_MLOT_FOIL", "INDATA,INLOT", null, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.AlertByBiz("BR_PRD_REG_USE_MLOT_FOIL", bizException.Message, bizException.ToString());
                                return;
                            }

                            /// 저장되었습니다.///
                            Util.MessageInfo("SFU1270");
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    }, InDataSet);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);                
            }
        }

        #endregion < Method >
    }
}