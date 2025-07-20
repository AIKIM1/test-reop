/*************************************************************************************
 Created Date : 2016.08.01
      Creator : Jeong Hyeon Sik
   Decription : Pack 작업지시 선택 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.01  Jeong Hyeon Sik : Initial Created.





 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Media;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_003_BOXINFO : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private DataView dvRootNodes;

        public PACK001_003_BOXINFO()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
                if (tmps != null)
                {
                    DataTable dtText = tmps[0] as DataTable;

                    if (dtText.Rows.Count > 0)
                    {
                        txtBoxID.Text = Util.NVC(dtText.Rows[0]["BOXID"]);
                        setBoxInfo(txtBoxID.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnBoxSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtBoxID.Text.Length > 0)
                {
                    setBoxInfo(txtBoxID.Text);
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnKeyPartCopy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                copyClipboardTreeView();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void txtBoxID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtBoxID.Text.Length > 0)
                {
                    setBoxInfo(txtBoxID.Text);
                }
            }
        }

        #endregion

        #region Mehod

        private void setBoxInfo(string sBoxid)
        {
            try
            {
                viewClear();

                //DA_PRD_SEL_BOX
                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("BOXID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = sBoxid;
                INDATA.Rows.Add(dr);

                dsInput.Tables.Add(INDATA);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_BOX_INFOMATION", "INDATA", "BOXLOT,BOX,BOXHISTORY", dsInput);

                if (dsResult != null && dsResult.Tables.Count > 0)
                {
                    string sStartedLotid = string.Empty;
                    string sStartedProductid = string.Empty;

                    if ((dsResult.Tables.IndexOf("BOX") > -1))
                    {
                        if (dsResult.Tables["BOX"].Rows.Count > 0)
                        { 
                            txtInfoBoxID.Text = Util.NVC(dsResult.Tables["BOX"].Rows[0]["BOXID"]);
                            //txtInfoAreaID.Text = Util.NVC(dsResult.Tables["BOX"].Rows[0]["AREAID"]);
                            txtInfoProductID.Text = Util.NVC(dsResult.Tables["BOX"].Rows[0]["PRODID"]);
                            txtBoxCount.Text = Util.NVC(dsResult.Tables["BOX"].Rows[0]["TOTAL_QTY"]);
                            txtInfoBoxCreateDate.Text = Util.NVC(dsResult.Tables["BOX"].Rows[0]["INSDTTM"]);
                            txtInfoLineID.Text = Util.NVC(dsResult.Tables["BOX"].Rows[0]["EQSGNAME"]);
                            txtInfoBoxType.Text = Util.NVC(dsResult.Tables["BOX"].Rows[0]["BOXTYPENAME"]);
                            txtBoxBoxState.Text = Util.NVC(dsResult.Tables["BOX"].Rows[0]["BOXSTATNAME"]);
                            txtInfoBoxComfirmDate.Text = Util.NVC(dsResult.Tables["BOX"].Rows[0]["PACKDTTM"]);
                            txtOcopRtnLotCnt.Text = Util.NVC(dsResult.Tables["BOX"].Rows[0]["OCOP_RTN_FLAG"]);
                        }
                        else
                        {
                            //txtBoxID.Focus();
                            //txtBoxID.SelectAll();

                            //txtInfoBoxID.Text = "";
                            //txtInfoBoxType.Text = "";
                            //txtInfoProductID.Text = "";
                            //txtBoxCount.Text = "";
                            //txtInfoBoxCreateDate.Text = "";
                            //txtBoxBoxState.Text = "";
                            //txtInfoBoxComfirmDate.Text = "";

                            //trvKeypartList.ItemsSource = null;
                        }
                    }
                    if ((dsResult.Tables.IndexOf("BOXLOT") > -1))
                    {
                        if (dsResult.Tables["BOXLOT"].Rows.Count > 0)
                        {
                            setLotInputBoxTracking(dsResult.Tables["BOXLOT"]);
                        }
                        else
                        {
                            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("포장된 LOT이 없습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning, (MessageBoxResult) =>
                            //{
                            //    txtBoxID.Focus();
                            //    txtBoxID.SelectAll();
                            //    trvKeypartList.ItemsSource = null; 
                            //});
                            //return;
                            //txtBoxID.Focus();
                            //txtBoxID.SelectAll();
                            //trvKeypartList.ItemsSource = null;
                        }
                    }
                    if ((dsResult.Tables.IndexOf("BOXHISTORY") > -1))
                    {
                        if (dsResult.Tables["BOXHISTORY"].Rows.Count > 0)
                        {
                            dgBoxHistory.ItemsSource = DataTableConverter.Convert(dsResult.Tables["BOXHISTORY"]);
                        }else
                        {
                            //Util.gridClear(dgBoxHistory);
                        }
                    }
                    
                }
            }
            catch (Exception ex)
            {
                // Util.AlertByBiz("BR_PRD_GET_BOX_INFOMATION", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }


        private void setLotInputBoxTracking(DataTable dtBoxLot)
        {
            try
            {
                DataSet ds = new DataSet();
                string sBoxID = Util.NVC(dtBoxLot.Rows[0]["BOXID"]);
                if (dtBoxLot.Rows.Count == 1)
                {
                    if(Util.NVC(dtBoxLot.Rows[0]["BOXID"]) == Util.NVC(dtBoxLot.Rows[0]["LOTID"]))
                    {
                        dtBoxLot.Rows[0].Delete();
                        dtBoxLot.AcceptChanges();
                    }
                }

                DataRow dr = dtBoxLot.NewRow();
                // MES 2.0 테이블 컬럼 위치 오류 Patch
                //dr.ItemArray = new object[] { null, null, sBoxID, null, null, null, null, null };
                dr["LOTID"] = sBoxID;
                dtBoxLot.Rows.InsertAt(dr, 0);

                ds.Tables.Add(dtBoxLot.Copy());
                ds.Relations.Add("Relations", ds.Tables["BOXLOT"].Columns["LOTID"], ds.Tables["BOXLOT"].Columns["BOXID"]);

                dvRootNodes = ds.Tables["BOXLOT"].DefaultView;
                dvRootNodes.RowFilter = "BOXID IS NULL";
                trvKeypartList.ItemsSource = dvRootNodes;
                TreeItemExpandAll();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void TreeItemExpandAll()
        {
            IList<DependencyObject> items = new List<DependencyObject>();
            VTreeHelper.GetChildrenOfType(trvKeypartList, typeof(C1TreeViewItem), ref items);

            foreach (C1TreeViewItem item in items)
            {
                item.Expanding += ChildItem_Expanding;
                TreeItemExpandNodes(item);
            }
        }


        public void TreeItemExpandNodes(C1TreeViewItem item)
        {
            item.IsExpanded = true;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                IList<DependencyObject> items = new List<DependencyObject>();
                VTreeHelper.GetChildrenOfType(item, typeof(C1TreeViewItem), ref items);
                foreach (C1TreeViewItem childItem in items)
                {
                    childItem.Expanding += ChildItem_Expanding;
                    TreeItemExpandNodes(childItem);
                }
            }));
        }
        private void ChildItem_Expanding(object sender, SourcedEventArgs e)
        {
            return;

            C1TreeViewItem ChildItem = sender as C1TreeViewItem;
            ChildItem.Expanding -= ChildItem_Expanding;

            object ob = ChildItem.DataContext;
            if (ob != null)
            {
                string sOcopYn = Util.NVC(((DataRowView)ob)["OCOP_RTN_FLAG"]);
 
                    if (sOcopYn == "Y")
                    {
                        ChildItem.Foreground = new SolidColorBrush(Colors.Red);
                        ChildItem.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        ChildItem.Foreground = new SolidColorBrush(Colors.Black);
                        ChildItem.FontWeight = FontWeights.Bold;
                    }

            }
        }
        private void viewClear()
        {
            txtBoxID.Focus();
            txtBoxID.SelectAll();
            
            txtInfoBoxID.Text = "";
            txtInfoBoxType.Text = "";
            txtInfoProductID.Text = "";
            txtInfoLineID.Text = "";
            txtInfoBoxCreateDate.Text = "";
            txtBoxCount.Text = "";
            txtInfoBoxComfirmDate.Text = "";
            txtBoxBoxState.Text = "";

            trvKeypartList.ItemsSource = null;

            Util.gridClear(dgBoxHistory);
        }

        private void copyClipboardTreeView()
        {
            try
            {

                string strAllNodeText = string.Empty;
                //IList<DependencyObject> items = new List<DependencyObject>();
                //VTreeHelper.GetChildrenOfType(trvKeypartList, typeof(C1TreeViewItem), ref items);


                IList<DependencyObject> itemText = new List<DependencyObject>();
                VTreeHelper.GetChildrenOfType(trvKeypartList, typeof(TextBlock), ref itemText);

                for (int i = 0; i < itemText.Count; i++)
                {
                    TextBlock textBolock = (TextBlock)itemText[i];
                    strAllNodeText += string.Format("{0} ", i) + textBolock.Text + Environment.NewLine;
                }

                Clipboard.SetText(strAllNodeText);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
        
    }
}
