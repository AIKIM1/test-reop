/*************************************************************************************
 Created Date : 2023.09.05
      Creator : 최경아
   Decription : 포장 Hold 관리_CELL HOLD 등록_HOLD 제외 CELL LIST
--------------------------------------------------------------------------------------
 [Change History]
  2023.09.05  DEVELOPER : Initial Created.
  2024.01.12  최경아    : CELL HOLD 시 HOLD 가능 여부 체크되지 않은 CELL을 NOT HOLD CELL POPUP 에 추가 :E20240112-001808
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using static LGC.GMES.MES.CMM001.Class.CommonCombo;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// Page1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_334_NOTHOLD : C1Window, IWorkArea
    {
        
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public BOX001_334_NOTHOLD()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitControl();
            InitCombo();
        }

        private void InitControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            DataTable dtInfo = (DataTable)tmps[0];

            if (dtInfo == null)
            {
                dtInfo = new DataTable();

                for (int i = 0; i < dgNHold.Columns.Count; i++)
                {
                    dtInfo.Columns.Add(dgNHold.Columns[i].Name);
                }
            }
            Util.GridSetData(dgNHold, dtInfo, this.FrameOperation);

            for (int iRow = 0; iRow < dgNHold.GetRowCount(); iRow++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgNHold.Rows[iRow].DataItem, "RESULT_CODE")).Equals("1"))
                {
                    DataTableConverter.SetValue(dgNHold.Rows[iRow].DataItem, "RESULT_CODE", MessageDic.Instance.GetMessage("SFU1195")); //Lot 정보가 없습니다.
                }
                else if(Util.NVC(DataTableConverter.GetValue(dgNHold.Rows[iRow].DataItem, "RESULT_CODE")).Equals("2"))
                {
                    DataTableConverter.SetValue(dgNHold.Rows[iRow].DataItem, "RESULT_CODE", MessageDic.Instance.GetMessage("SFU8429")); //이미 HOLD 처리 되어있습니다.
                }
                else if (Util.NVC(DataTableConverter.GetValue(dgNHold.Rows[iRow].DataItem, "RESULT_CODE")).Equals("3"))
                {
                    DataTableConverter.SetValue(dgNHold.Rows[iRow].DataItem, "RESULT_CODE", MessageDic.Instance.GetMessage("SFU8368")); //양품창고에 있는 경우 Hold 할 수 없습니다.
                }
            }
            object[] parameters = new object[2];
            parameters[0] = tmps[2];
            parameters[1] = dgNHold.Rows.Count;  
            txtNote.Text = MessageDic.Instance.GetMessage("SFU4652", parameters);//HOLD 요청 CELL [%1] 건 중 [%2] 건이 HOLD 제외 되었습니다.
        }

        private void InitCombo()
        {

        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }
    }
}
