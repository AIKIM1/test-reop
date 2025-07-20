/*************************************************************************************
 Created Date : 2024.01.12
      Creator : 남기운
   Decription : 허용비율초과사유팝업록
--------------------------------------------------------------------------------------
 [Change History]
  2024.01.12  DEVELOPER : Initial Created.
  2024.03.07  남기운     : [NERP대응 프로젝트]다국어 처리
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;


namespace LGC.GMES.MES.CMM001
{
  
    public partial class CMM_PERMIT_RATE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();
       
       
        private string sLOTID = string.Empty;
  
        private string _UserID = string.Empty;
        private string _UserName = string.Empty;
        private string _DeptID = string.Empty;
        private string _DeptName = string.Empty;

        private DataTable _PERMIT_RATE = new DataTable();
        //

        private List<string> b;
        public List<string> B { get; set; }


        public string UserID
        {
            get { return _UserID; }
        }

        public string DeptID
        {
            get { return _DeptID; }
        }

        public DataTable PERMIT_RATE
        {
            get { return _PERMIT_RATE; }
        }
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

        public CMM_PERMIT_RATE()
        {
            InitializeComponent();
        }


        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }
            _PERMIT_RATE.Columns.Add("LOTID", typeof(string));
            _PERMIT_RATE.Columns.Add("WIPSEQ", typeof(string));

            _PERMIT_RATE.Columns.Add("ACTID", typeof(string));
            _PERMIT_RATE.Columns.Add("ACTNAME", typeof(string));
            _PERMIT_RATE.Columns.Add("RESNCODE", typeof(string));
            _PERMIT_RATE.Columns.Add("DFCT_CODE_DETL_NAME", typeof(string));
            _PERMIT_RATE.Columns.Add("RESNQTY", typeof(string));
            _PERMIT_RATE.Columns.Add("PERMIT_RATE", typeof(string));
            _PERMIT_RATE.Columns.Add("OVER_QTY", typeof(string));
            _PERMIT_RATE.Columns.Add("SPCL_RSNCODE", typeof(string));
            _PERMIT_RATE.Columns.Add("RESNNOTE", typeof(string));
            _PERMIT_RATE.Rows.Clear();

            sLOTID = Util.NVC(tmps[0]);
            DataTable data = new DataTable();
            if (tmps[1] == null || tmps[1] == System.DBNull.Value)
            {

            } else
            {
                data = (DataTable)tmps[1];                       
                //Data동기화
                Util.GridSetData(dgList, data, FrameOperation, false);

                cobRSNCODE();
            }

            txtUserName.Text = "";
            txtDEP.Text = "";

        }
        #endregion

        #region Event

        private void cobRSNCODE()
        {
            
            //DataTable dtRqstDt = new DataTable("RQSTDT");
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            DataRow newRow = inTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            inTable.Rows.Add(newRow);


            string bizRuleName = "DA_BAS_SEL_NERP_CHARGE_CBO";
            DataTable result = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
          
            (dgList.Columns["SPCL_RSNCODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(result);
          
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!isValid())
                return;
            else
            {
                SaveData();
                this.DialogResult = MessageBoxResult.OK;
            }
            
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
      
      
        #endregion

        #region Mehod
     
     
        private bool isValid()
        {

            string sCode = "";

            if(_UserID.IsEmpty() || _UserID.Equals(""))
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("ME_0355"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                txtUserName.Focus();

                return false;
            }
            for (int j = 0; j < dgList.Rows.Count - dgList.BottomRows.Count; j++)
            {
                sCode = Util.NVC(DataTableConverter.GetValue(dgList.Rows[j].DataItem, "SPCL_RSNCODE")).Trim(); ;
                if(sCode.Equals(""))
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("ME_0398"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return false;
                }                
            }
            return true;
        }
    

      


        private void SaveData()
        {
            try
            {
                dgList.EndEdit();

                _PERMIT_RATE.Rows.Clear();
                for (int j = 0; j < dgList.Rows.Count - dgList.BottomRows.Count; j++)
                {
                    DataRow newRow = _PERMIT_RATE.NewRow();

                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[j].DataItem, "LOTID"));
                    newRow["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[j].DataItem, "WIPSEQ"));

                    newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[j].DataItem, "ACTID"));
                    newRow["ACTNAME"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[j].DataItem, "ACTNAME"));
                    newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[j].DataItem, "RESNCODE"));
                    newRow["DFCT_CODE_DETL_NAME"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[j].DataItem, "DFCT_CODE_DETL_NAME"));
                    newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[j].DataItem, "RESNQTY"));
                    newRow["PERMIT_RATE"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[j].DataItem, "PERMIT_RATE"));
                    newRow["OVER_QTY"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[j].DataItem, "OVER_QTY"));
                    newRow["SPCL_RSNCODE"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[j].DataItem, "SPCL_RSNCODE"));
                    newRow["RESNNOTE"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[j].DataItem, "RESNNOTE"));
                    _PERMIT_RATE.Rows.Add(newRow);
                }

                int i = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        
        }
     

        private void txtUserName_KeyUp(object sender, KeyEventArgs e)
        {
            if (sender == null)
                return;
        }
        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }

        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtUserName.Text;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);

                Dispatcher.BeginInvoke(new Action(() => wndPerson.ShowModal()));
                /*
                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        grdMain.Children.Add(wndPerson);
                        wndPerson.BringToFront();
                        break;
                    }
                }
                */

                //grdMain.Children.Add(wndPerson);
                //wndPerson.BringToFront();
            }
        }
        

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtUserName.Text = wndPerson.USERNAME;
                txtUserName.Tag = wndPerson.USERID;

                txtDEP.Text = wndPerson.DEPTNAME;
                txtDEP.Tag = wndPerson.DEPTID;


                _UserID = wndPerson.USERID;
                _UserName = wndPerson.USERNAME;

                _DeptID = wndPerson.DEPTID;
                _DeptName = wndPerson.DEPTNAME;

            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(wndPerson);
                    break;
                }
            }
        }

        #endregion
    }
}
