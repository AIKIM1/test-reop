/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_088 : UserControl
    {
        #region Declaration & Constructor 
        DataTable dtResult;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PGM_GUI_088()
        {
            InitializeComponent();
        }


        #endregion

        #region Initialize
        private void Initialize()
        {
            testData();
            InitCombo();
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }
        #endregion

        #region Mehod

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //string area = LoginInfo.CFG_AREA_ID.ToString();

            //��
            //C1ComboBox[] cboAreaChild = { cboLine };
            //_combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChild);

            //����
            //C1ComboBox[] cboLineParent = { cboArea };
            //C1ComboBox[] cboLineChild = { cboProcess };
            string[] area = { LoginInfo.CFG_AREA_ID.ToString() };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, sFilter: area);
            
            //����
            //C1ComboBox[] cbProcessParent = { cboLine };
            //_combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, null, cbParent: cbProcessParent);


        }
        private void testData()
        {
            testGridData();
            testComboSet();
        }

        private void testGridData()
        {
            dtResult = new DataTable();
            dtResult.Columns.Add("CHK", typeof(bool));
            dtResult.Columns.Add("LOT_ID", typeof(string));
            dtResult.Columns.Add("PRODUCT", typeof(string));
            dtResult.Columns.Add("FAIL_OPER", typeof(string));
            dtResult.Columns.Add("DATE", typeof(string));
            dtResult.Columns.Add("FAIL_NAME", typeof(string));
            dtResult.Columns.Add("MODEL", typeof(string));            
            dtResult.Columns.Add("LAUTER", typeof(string));

            List<object[]> menulist = new List<object[]>();

            menulist.Add(new object[] { false, "LB02712392", "B10 CELL", "4ȣ�� Cell Ŀ��,��� ����", "2016-07-13 ���� 2:30:18", "���� ����", "", "PACK4ȣ_B10_CELL_����" });
            menulist.Add(new object[] { false, "LB02712268", "B10 CELL", "4ȣ�� Cell Ŀ��,��� ����", "2016-07-13 ���� 2:35:18", "���� ����", "", "PACK4ȣ_B10_CELL_����" });
            menulist.Add(new object[] { false, "LB02712209", "B10 CELL", "4ȣ�� Cell Ŀ��,��� ����", "2016-07-13 ���� 2:50:18", "���� ����", "", "PACK4ȣ_B10_CELL_����" });
            menulist.Add(new object[] { false, "LB02712392", "B10 CELL", "4ȣ�� Cell Ŀ��,��� ����", "2016-07-13 ���� 2:30:18", "���� ����", "", "PACK4ȣ_B10_CELL_����" });
            menulist.Add(new object[] { false, "LB02712268", "B10 CELL", "4ȣ�� Cell Ŀ��,��� ����", "2016-07-13 ���� 2:35:18", "���� ����", "", "PACK4ȣ_B10_CELL_����" });
            menulist.Add(new object[] { false, "LB02712209", "B10 CELL", "4ȣ�� Cell Ŀ��,��� ����", "2016-07-13 ���� 2:50:18", "���� ����", "", "PACK4ȣ_B10_CELL_����" });

            DataRow newRow;

            foreach (object[] item in menulist)
            {
                newRow = dtResult.NewRow();
                newRow.ItemArray = item;
                dtResult.Rows.Add(newRow);
            }

            SetBinding(dgSearchResult, dtResult);
        }

        private void testComboSet()
        {
            testSet_Cbo(cboEquipmentSegment, "1", "4ȣ��");
            testSet_Cbo(cboModel, "1", "B10");
            testSet_Cbo(cboProductType, "1", "CELL");
            testSet_Cbo(cboProduct, "1", "B10 CELL");
            testSet_Cbo(cboFailOper, "1", "4ȣ�� Cell Ŀ��,��� ����"); 

            testSet_Cbo(cboStatus, "1", "�������");
            testSet_Cbo(cboModel11, "1", "�ҷ���������");

            testSet_Cbo(cboWork, "1", "����");
            testSet_Cbo(cboReason, "1", "CELL ��˻�");
            testSet_Cbo(cboPart, "1", "PACK ���� PART");
           

            //Get_Data_Modify();
        }

        public void testSet_Cbo(C1.WPF.C1ComboBox cbo, string key, string value)
        {
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("KEY", typeof(string));
            dtResult.Columns.Add("VALUE", typeof(string));

            DataRow newRow = dtResult.NewRow();

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "��ü", "0" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { value + key, "1" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { value + "2", "2" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { value + "3", "3" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { value + "4", "4" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { value + "5", "5" };
            dtResult.Rows.Add(newRow);

            cbo.ItemsSource = DataTableConverter.Convert(dtResult);

            if (dtResult.Rows.Count > 0)
            {
                //CheckAll();
            }
        }

        private void SetBinding(C1.WPF.DataGrid.C1DataGrid dg, DataTable dt)
        {
            dg.ItemsSource = DataTableConverter.Convert(dt);
        }

        #endregion

        private void btnAllEnd_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgSearchResult.Rows.Count; i++)
            {
                if (DataTableConverter.GetValue(dgSearchResult.Rows[i].DataItem, "CHK").ToString() == "True")
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(i + " ��° ROW �ϰ� ó��");
                }
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void dgSearchResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int currentRow = dgSearchResult.CurrentRow.Index;

            if (dgSearchResult.CurrentColumn.Index == 1)
            {
                string selectLot = DataTableConverter.GetValue(dgSearchResult.Rows[currentRow].DataItem, "LOT_ID").ToString();

                PGM_GUI_079_LOTEHISTORY popup02 = new PGM_GUI_079_LOTEHISTORY();
                popup02.FrameOperation = this.FrameOperation;

                if (popup02 != null)
                {
                    DataTable dtData = new DataTable();

                    dtData.Columns.Add("VALUE", typeof(string));
                    //dtData.Columns.Add("COL02", typeof(string));

                    DataRow newRow = null;

                    newRow = dtData.NewRow();
                    newRow.ItemArray = new object[] { selectLot };
                    dtData.Rows.Add(newRow);

                    //newRow = dtData.NewRow();
                    //newRow.ItemArray = new object[] { "Text01", "Text02" };
                    //dtData.Rows.Add(newRow);

                    //========================================================================
                    string Parameter = "Parameter";
                    C1WindowExtension.SetParameter(popup02, Parameter);
                    //========================================================================
                    object[] Parameters = new object[2];
                    Parameters[0] = "Parameter01";
                    Parameters[1] = dtData;
                    C1WindowExtension.SetParameters(popup02, Parameters);
                    //========================================================================

                    //popup02.Closed -= popup02_Closed;
                    //popup02.Closed += popup02_Closed;
                    popup02.ShowModal();
                    popup02.CenterOnScreen();
                }

                //string MAINFORMPATH = "LGC.GMES.MES.ProtoType04";
                //string MAINFORMNAME = "PGM_GUI_079_LOTEHISTORY";

                //PopUpOpen(MAINFORMPATH, MAINFORMNAME);
            }




        }

        private void PopUpOpen(string sMAINFORMPATH, string sMAINFORMNAME)
        {
            Assembly asm = Assembly.LoadFrom("ClientBin\\" + sMAINFORMPATH + ".dll");
            Type targetType = asm.GetType(sMAINFORMPATH + "." + sMAINFORMNAME);
            object obj = Activator.CreateInstance(targetType);

            IWorkArea workArea = obj as IWorkArea;
            workArea.FrameOperation = FrameOperation;

            C1Window popup = obj as C1Window;
            if (popup != null)
            {
                popup.Closed -= popup_Closed;
                popup.Closed += popup_Closed;
                popup.ShowModal();
                popup.CenterOnScreen();
            }
        }

        void popup_Closed(object sender, EventArgs e)
        {

        }
    }
}
