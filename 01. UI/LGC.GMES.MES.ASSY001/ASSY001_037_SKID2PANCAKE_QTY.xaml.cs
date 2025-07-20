/*************************************************************************************
 Created Date : 2016.11.14
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.11.14  DEVELOPER : Initial Created.





 
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
using LGC.GMES.MES.CMM001; 
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Popup;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_037_SKID2PANCAKE_QTY : C1Window, IWorkArea
    {

        #region Declaration & Constructor 

        public ASSY001_037_SKID2PANCAKE_QTY()
        {
            InitializeComponent(); 
        }

        private void setProductCombo()
        {
            SetProduct(cboProduct); 

        }

        private void SetProduct(C1ComboBox cbo)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string)); 


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID; 

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_QTYPERSKID_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                dr = dtResult.NewRow();

                dr["CBO_NAME"] = "-SELECT-";
                dr["CBO_CODE"] = "";

                dtResult.Rows.InsertAt(dr, 0);

                cbo.ItemsSource = dtResult.Copy().AsDataView();
                 

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            SearchData();
        }


        #endregion

        #region Event 
        
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {


            try
            {
                if (string.IsNullOrEmpty(cboProduct.SelectedValue.ToString())) return;
                //추가하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2965"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        
                        DataTable RQSTDT = new DataTable();
                        RQSTDT.Columns.Add("KEYGROUPID", typeof(string));
                        RQSTDT.Columns.Add("PRODID", typeof(string));
                        RQSTDT.Columns.Add("QTY", typeof(string));
                        RQSTDT.Columns.Add("INSUSER", typeof(string));

                        DataRow dr = RQSTDT.NewRow();
                        dr["KEYGROUPID"] = "QTYPERSKID";
                        dr["PRODID"] = cboProduct.SelectedValue.ToString();
                        dr["QTY"] = txtQTY.Value;
                        dr["INSUSER"] = LoginInfo.USERID;

                        RQSTDT.Rows.Add(dr);

                        DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_MCS_INS_QTYPERSKID", "INDATA", null, RQSTDT);

                        SearchData(); 
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
            this.DialogResult = MessageBoxResult.Cancel;
        }


        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                //삭제하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                        DataTable RQSTDT = new DataTable();
                        RQSTDT.Columns.Add("PRODID", typeof(string));

                        DataRow dr = RQSTDT.NewRow();
                        dr["PRODID"] = DataTableConverter.Convert(dgList.ItemsSource).Rows[index]["PRODID"];

                        RQSTDT.Rows.Add(dr);

                        DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_MCS_DEL_QTYPERSKID", "INDATA", null, RQSTDT);

                        SearchData();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        #region Mehod
         


        private void SearchData()
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("KEYGROUPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["KEYGROUPID"] = "QTYPERSKID";

                 RQSTDT.Rows.Add(dr);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_QTYPERSKID", "INDATA", "RSLTDT", RQSTDT);

                Util.GridSetData(dgList, dtMain , FrameOperation, false);

                setProductCombo();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex); 
            }
        }

         

 
        #endregion



    }
}