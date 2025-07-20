/*************************************************************************************
 Created Date : 2018.01.12
      Creator : 
   Decription :
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using System;
using System.Windows;
using System.Data;
using System.Windows.Input;
using C1.WPF.Excel;
using System.IO;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Collections.Generic;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using Microsoft.Win32;
using System.Configuration;

//using Application = System.Windows.Application;
//using DataGridLength = C1.WPF.DataGrid.DataGridLength;
//using DataGridRow = C1.WPF.DataGrid.DataGridRow;
//using System.Linq;
//using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_216_UPLOAD : C1Window, IWorkArea
    {
        #region Declaration
        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();
     
        private bool _load = true;
        private readonly Util _util = new Util();
        private readonly BizDataSet _bizDataSet = new BizDataSet();

        DataTable dtiUse = new DataTable();
        DataTable dtType = new DataTable();


        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize


        public COM001_216_UPLOAD()
        {
            InitializeComponent();
        }

        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Initialized(object sender, EventArgs e)
        {
            Initialize();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Initialize()
        {

        }


        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion


        #endregion

        #region User Method

        #region [BizCall]
        #endregion
   

        #region [Func]
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
        #endregion

        #endregion

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                ShowLoadingIndicator();

                string sCSTID = string.Empty;

                DataTable dtBasicInfo = new DataTable();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("SRCTYPE", typeof(string));
                for (int i = 0; i < dgSearch.GetRowCount(); i++)
                {
                    DataRow newRow = inTable.NewRow();
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgSearch.Rows[i].DataItem, "LOTID"));
                    newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgSearch.Rows[i].DataItem, "CSTID"));
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;

                    inTable.Rows.Add(newRow);
                }

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_CSTID_USING_UI", "INDATA", null, inTable);

                Util.MessageInfo("SFU1270");      //저장되었습니다.          

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

        private void btnFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();

            if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
            {
                fd.InitialDirectory = @"\\Client\C$";
            }

            fd.Filter = "Excel Files (.xlsx)|*.xlsx";
            if (fd.ShowDialog() == true)
            {
                using (Stream stream = fd.OpenFile())
                {
                    C1XLBook book = new C1XLBook();
                    book.Load(stream, FileFormat.OpenXml);
                    XLSheet sheet = book.Sheets[0];

                    DataTable dataTable = new DataTable();
                    dataTable.Columns.Add("CSTID", typeof(string));
                    dataTable.Columns.Add("LOTID", typeof(string));

                    for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                    {
                        DataRow dataRow = dataTable.NewRow();
                        for (int colInx = 0; colInx < sheet.Rows.Count; colInx++)
                        {
                            XLCell cell = sheet.GetCell(rowInx, colInx);
                            if (cell != null)
                            {
                                dataRow[colInx] = cell.Text;
                            }
                        }
                        dataTable.Rows.Add(dataRow);
                    }
                    dataTable.AcceptChanges();

                    dgSearch.ItemsSource = DataTableConverter.Convert(dataTable);
                }
            }
        }
    }
}
