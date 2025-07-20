using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DateTimeEditors;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.CMM001.Models
{
    public class ResultElement
    {
        /// <summary>
        /// Label 제목
        /// </summary>
        public string Title;

        /// <summary>
        /// 사용할 Control
        /// </summary>
        public Control Control;

        /// <summary>
        /// 팝업용 Button
        /// </summary>
        public Button PopupButton = null;

        /// <summary>
        /// 사용할 Control
        /// </summary>
        public Control Control2;

        /// <summary>
        /// 팝업용 Button
        /// </summary>
        public Button PopupButton2 = null;

        /// <summary>
        /// Radio Button
        /// </summary>
        public RadioButton radButton = null;

        /// <summary>
        /// Visibility
        /// </summary>
        public bool Visibility = true;

        /// <summary>
        /// 차지하는 공간 크기
        /// </summary>
        public int SpaceInCharge = 1;

        /// <summary>
        /// 새로운 행에서 시작 여부
        /// </summary>
        public bool IsNewLine = false;
    }

    public class ResultElementList
    {
        public static List<ResultElement> CommonList(int elementCountPerRow)
        {
            List<ResultElement> lst = new List<ResultElement>();

            //lst.Add(new ResultElement { Title = "작업일자", Control = new C1DateTimePicker() { Name = "dtpWorkDate", IsEnabled = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "작업일자", Control = new TextBox() { Name = "txtWorkDate", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "시작시간", Control = new TextBox() { Name = "txtStartDateTime", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "종료시간", Control = new TextBox() { Name = "txtEndDateTime", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, IsNewLine = true });
            lst.Add(new ResultElement { Title = "버전", Control = new TextBox() { Name = "txtVersion", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "Lane수", Control = new C1NumericBox() { Name = "txtLaneQty", IsEnabled = false, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center, ShowButtons = false } });
            
            return lst;
        }

        public static List<ResultElement> PreMixerList(int elementCountPerRow)
        {
            List<ResultElement> lst = new List<ResultElement>();

            //lst.Add(new ResultElement { Title = "작업일자", Control = new C1DateTimePicker() { Name = "dtpWorkDate", IsEnabled = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "작업일자", Control = new TextBox() { Name = "txtWorkDate", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "시작시간", Control = new TextBox() { Name = "txtStartDateTime", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "종료시간", Control = new TextBox() { Name = "txtEndDateTime", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, IsNewLine = true });
            lst.Add(new ResultElement { Title = "버전", Control = new TextBox() { Name = "txtVersion", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, PopupButton = new Button() { Name = "btnVersion", VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "BeadMill횟수", Control = new C1NumericBox() { Name = "txtBeadMillCount", IsEnabled = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center, Minimum = 0, Maximum = 10, ShowButtons = true } });

            return lst;
        }

        public static List<ResultElement> BinderSolution(int elementCountPerRow)
        {
            List<ResultElement> lst = new List<ResultElement>();

            //lst.Add(new ResultElement { Title = "작업일자", Control = new C1DateTimePicker() { Name = "dtpWorkDate", IsEnabled = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "작업일자", Control = new TextBox() { Name = "txtWorkDate", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "시작시간", Control = new TextBox() { Name = "txtStartDateTime", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "종료시간", Control = new TextBox() { Name = "txtEndDateTime", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, IsNewLine = true });
            lst.Add(new ResultElement { Title = "버전", Control = new TextBox() { Name = "txtVersion", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, PopupButton = new Button() { Name = "btnVersion", VerticalAlignment = VerticalAlignment.Center } });            

            return lst;
        }

        public static List<ResultElement> MixerList(int elementCountPerRow)
        {
            List<ResultElement> lst = new List<ResultElement>();

            //lst.Add(new ResultElement { Title = "작업일자", Control = new C1DateTimePicker() { Name = "dtpWorkDate", IsEnabled = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "작업일자", Control = new TextBox() { Name = "txtWorkDate", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "시작시간", Control = new TextBox() { Name = "txtStartDateTime", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "종료시간", Control = new TextBox() { Name = "txtEndDateTime", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, IsNewLine = true });
            lst.Add(new ResultElement { Title = "버전", Control = new TextBox() { Name = "txtVersion", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, PopupButton = new Button() { Name = "btnVersion", VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "가동시간(분)", Control = new C1NumericBox() { Name = "txtWorkTime", IsEnabled = false, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center, Format = "#,###.##", ShowButtons = false } });

            return lst;
        }

        public static List<ResultElement> CoaterList(int elementCountPerRow)
        {
            List<ResultElement> lst = new List<ResultElement>();

            //lst.Add(new ResultElement { Title = "작업일자", Control = new C1DateTimePicker() { Name = "dtpWorkDate", IsEnabled = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "작업일자", Control = new TextBox() { Name = "txtWorkDate", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "시작시간", Control = new TextBox() { Name = "txtStartDateTime", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "종료시간", Control = new TextBox() { Name = "txtEndDateTime", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, IsNewLine = true });
            lst.Add(new ResultElement { Title = "버전", Control = new TextBox() { Name = "txtVersion", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, PopupButton = new Button() { Name = "btnVersion", VerticalAlignment = VerticalAlignment.Center } });
            //txtCurLaneQty -> txtLaneQty로 수정
            lst.Add(new ResultElement { Title = "Lane수", Control = new C1NumericBox() { Name = "txtLaneQty", HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center, ShowButtons = false } });
            lst.Add(new ResultElement { Title = "FINALCUT", Control = new CheckBox() { Name = "chkFinalCut", HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            return lst;
        }
        public static List<ResultElement> DefectLaneCoaterList(int elementCountPerRow)
        {
            List<ResultElement> lst = new List<ResultElement>();

            lst.Add(new ResultElement { Title = "작업일자", Control = new TextBox() { Name = "txtWorkDate", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "시작시간", Control = new TextBox() { Name = "txtStartDateTime", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "종료시간", Control = new TextBox() { Name = "txtEndDateTime", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, IsNewLine = true });
            lst.Add(new ResultElement { Title = "버전", Control = new TextBox() { Name = "txtVersion", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, PopupButton = new Button() { Name = "btnVersion", VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "Lane수", Control = new C1NumericBox() { Name = "txtCurLaneQty", HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center, ShowButtons = false, IsEnabled = false } });
            lst.Add(new ResultElement { Title = "실물Lane수", Control = new C1NumericBox() { Name = "txtLaneQty", HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center, ShowButtons = false } });
            lst.Add(new ResultElement { Title = "FINALCUT", Control = new CheckBox() { Name = "chkFinalCut", HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            return lst;
        }

        public static List<ResultElement> SlurryAndCoreList(int elementCountPerRow)
        {
            List<ResultElement> lst = new List<ResultElement>();

            lst.Add(new ResultElement { Title = "Core(A)", Control = new TextBox() { Name = "txtCore1", IsReadOnly = false, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, radButton = new RadioButton() { Name = "radFoil1", VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "Core(B)", Control = new TextBox() { Name = "txtCore2", IsReadOnly = false, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, radButton = new RadioButton() { Name = "radFoil2", VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "", Control = new Button() { Name = "btnMtrlChange", Content = Common.ObjectDic.Instance.GetObjectName("장착"), HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, MinWidth = 67 } });
            lst.Add(new ResultElement { Title = "Slurry(Top)", Control = new TextBox() { Name = "txtSlurry1", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, PopupButton = new Button() { Name = "btnSlurry1", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Left }, IsNewLine = true });
            lst.Add(new ResultElement { Title = "Slurry(Back)", Control = new TextBox() { Name = "txtSlurry2", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, PopupButton = new Button() { Name = "btnSlurry2", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Left } });
            lst.Add(new ResultElement { Title = "", Control = new Button() { Name = "btnMtrlChange2", Content = Common.ObjectDic.Instance.GetObjectName("탈착"), HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, MinWidth = 67 } });
            return lst;
        }

        public static List<ResultElement> SlurryAndCoreListDL(int elementCountPerRow)
        {
            List<ResultElement> lst = new List<ResultElement>();

            lst.Add(new ResultElement { Title = "Core(A)", Control = new TextBox() { Name = "txtCore1", IsReadOnly = false, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, radButton = new RadioButton() { Name = "radFoil1", VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "Core(B)", Control = new TextBox() { Name = "txtCore2", IsReadOnly = false, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, radButton = new RadioButton() { Name = "radFoil2", VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "", Control = new Button() { Name = "btnMtrlChange", Content = Common.ObjectDic.Instance.GetObjectName("장착"), HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, MinWidth = 67 } });
            lst.Add(new ResultElement { Title = "Slurry(Top)", Control = new TextBox() { Name = "txtSlurry1", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, PopupButton = new Button() { Name = "btnSlurry1", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Left }, Control2 = new TextBox() { Name = "txtSlurry11", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, PopupButton2 = new Button() { Name = "btnSlurry11", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Left }, IsNewLine = true });
            lst.Add(new ResultElement { Title = "Slurry(Back)", Control = new TextBox() { Name = "txtSlurry2", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, PopupButton = new Button() { Name = "btnSlurry2", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Left }, Control2 = new TextBox() { Name = "txtSlurry22", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, PopupButton2 = new Button() { Name = "btnSlurry22", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Left } });
            lst.Add(new ResultElement { Title = "", Control = new Button() { Name = "btnMtrlChange2", Content = Common.ObjectDic.Instance.GetObjectName("탈착"), HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, MinWidth = 67 } });
            return lst;
        }

        public static List<ResultElement> BackSlurryAndCoreList(int elementCountPerRow)
        {
            List<ResultElement> lst = new List<ResultElement>();

            lst.Add(new ResultElement { Title = "Core(A)", Control = new TextBox() { Name = "txtCore1", IsReadOnly = false, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, PopupButton = new Button() { Name = "btnFoil1", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Left }, radButton = new RadioButton() { Name = "radFoil1", VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "Core(B)", Control = new TextBox() { Name = "txtCore2", IsReadOnly = false, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, PopupButton = new Button() { Name = "btnFoil2", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Left }, radButton = new RadioButton() { Name = "radFoil2", VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "", Control = new Button() { Name = "btnMtrlChange", Content = Common.ObjectDic.Instance.GetObjectName("장착"), HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, MinWidth = 90 } });
            lst.Add(new ResultElement { Title = "Slurry(Top)", Control = new TextBox() { Name = "txtSlurry1", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, PopupButton = new Button() { Name = "btnSlurry1", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Left }, IsNewLine = true });
            lst.Add(new ResultElement { Title = "Slurry(Back)", Control = new TextBox() { Name = "txtSlurry2", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, PopupButton = new Button() { Name = "btnSlurry2", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Left } });
            lst.Add(new ResultElement { Title = "", Control = new Button() { Name = "btnMtrlChange2", Content = Common.ObjectDic.Instance.GetObjectName("탈착"), HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, MinWidth = 90 } });
            return lst;
        }

        public static List<ResultElement> SingleCoaterTopList(int elementCountPerRow)
        {
            List<ResultElement> lst = new List<ResultElement>();

            //lst.Add(new ResultElement { Title = "작업일자", Control = new C1DateTimePicker() { Name = "dtpWorkDate", IsEnabled = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "작업일자", Control = new TextBox() { Name = "txtWorkDate", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "시작시간", Control = new TextBox() { Name = "txtStartDateTime", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "종료시간", Control = new TextBox() { Name = "txtEndDateTime", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, IsNewLine = true });
            lst.Add(new ResultElement { Title = "버전", Control = new TextBox() { Name = "txtVersion", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, PopupButton = new Button() { Name = "btnVersion", VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "Lane수", Control = new C1NumericBox() { Name = "txtLaneQty", HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center, ShowButtons = false } });
            lst.Add(new ResultElement { Title = "FINALCUT", Control = new CheckBox() { Name = "chkFinalCut", HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            
            return lst;
        }

        public static List<ResultElement> SingleCoaterBackList(int elementCountPerRow)
        {
            List<ResultElement> lst = new List<ResultElement>();

            //lst.Add(new ResultElement { Title = "작업일자", Control = new C1DateTimePicker() { Name = "dtpWorkDate", IsEnabled = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "작업일자", Control = new TextBox() { Name = "txtWorkDate", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "시작시간", Control = new TextBox() { Name = "txtStartDateTime", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "종료시간", Control = new TextBox() { Name = "txtEndDateTime", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, IsNewLine = true });
            lst.Add(new ResultElement { Title = "버전", Control = new TextBox() { Name = "txtVersion", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, PopupButton = new Button() { Name = "btnVersion", VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "Lane수", Control = new C1NumericBox() { Name = "txtLaneQty", IsEnabled = false, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center, ShowButtons = false } });
            lst.Add(new ResultElement { Title = "FINALCUT", Control = new CheckBox() { Name = "chkFinalCut", HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });

            return lst;
        }

        public static List<ResultElement> SrsCoaterList(int elementCountPerRow)
        {
            List<ResultElement> lst = new List<ResultElement>();

            //lst.Add(new ResultElement { Title = "작업일자", Control = new C1DateTimePicker() { Name = "dtpWorkDate", IsEnabled = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "작업일자", Control = new TextBox() { Name = "txtWorkDate", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "시작시간", Control = new TextBox() { Name = "txtStartDateTime", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "종료시간", Control = new TextBox() { Name = "txtEndDateTime", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, IsNewLine = true });
            lst.Add(new ResultElement { Title = "버전", Control = new TextBox() { Name = "txtVersion", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, PopupButton = new Button() { Name = "btnVersion", VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "Lane수", Control = new C1NumericBox() { Name = "txtLaneQty", IsEnabled = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center, ShowButtons = false } });
            lst.Add(new ResultElement { Title = "보호필름", Control = new C1ComboBox() { Name = "cboPet", HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, IsNewLine = true });
            lst.Add(new ResultElement { Title = "In", Control = new C1NumericBox() { Name = "txtSrs1Qty", IsEnabled = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center, Format = "F2", ShowButtons = false } });
            lst.Add(new ResultElement { Title = "Mid", Control = new C1NumericBox() { Name = "txtSrs3Qty", IsEnabled = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center, Format = "F2", ShowButtons = false } });
            lst.Add(new ResultElement { Title = "Out", Control = new C1NumericBox() { Name = "txtSrs2Qty", IsEnabled = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center, Format = "F2", ShowButtons = false }, IsNewLine = true });
            lst.Add(new ResultElement { Title = "이송탱크", Control = new TextBox() { Name = "txtTank", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "BATCHID", Control = new TextBox() { Name = "txtBatchId", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, PopupButton = new Button() { Name = "btnBatchId", VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "FINALCUT", Control = new CheckBox() { Name = "chkFinalCut", HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            
            return lst;
        }

        public static List<ResultElement> RollPressList(int elementCountPerRow)
        {
            List<ResultElement> lst = new List<ResultElement>();

            //lst.Add(new ResultElement { Title = "작업일자", Control = new C1DateTimePicker() { Name = "dtpWorkDate", IsEnabled = true,  HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "작업일자", Control = new TextBox() { Name = "txtWorkDate", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "시작시간", Control = new TextBox() { Name = "txtStartDateTime", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "종료시간", Control = new TextBox() { Name = "txtEndDateTime", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, IsNewLine = true });
            lst.Add(new ResultElement { Title = "버전", Control = new TextBox() { Name = "txtVersion", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "Lane수", Control = new C1NumericBox() { Name = "txtLaneQty", IsEnabled = false, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center, ShowButtons = false } });
            lst.Add(new ResultElement { Title = "추가압연", Control = new CheckBox() { Name = "chkExtraPress", HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
           
            return lst;
        }

        public static List<ResultElement> DefectLaneRollPressList(int elementCountPerRow)
        {
            List<ResultElement> lst = new List<ResultElement>();

            //lst.Add(new ResultElement { Title = "작업일자", Control = new C1DateTimePicker() { Name = "dtpWorkDate", IsEnabled = true,  HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "작업일자", Control = new TextBox() { Name = "txtWorkDate", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "시작시간", Control = new TextBox() { Name = "txtStartDateTime", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "종료시간", Control = new TextBox() { Name = "txtEndDateTime", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, IsNewLine = true });
            lst.Add(new ResultElement { Title = "버전", Control = new TextBox() { Name = "txtVersion", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "Lane수", Control = new C1NumericBox() { Name = "txtCurLaneQty", IsEnabled = false, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center, ShowButtons = false } });
            lst.Add(new ResultElement { Title = "실물Lane수", Control = new C1NumericBox() { Name = "txtLaneQty", IsEnabled = false, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center, ShowButtons = false } });
            lst.Add(new ResultElement { Title = "추가압연", Control = new CheckBox() { Name = "chkExtraPress", HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "FastTrack", Control = new CheckBox() { Name = "chkFastTrack", HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center} });
            return lst;
        }

        public static List<ResultElement> SlitterList(int elementCountPerRow)
        {
            List<ResultElement> lst = new List<ResultElement>();

            //lst.Add(new ResultElement { Title = "작업일자", Control = new C1DateTimePicker() { Name = "dtpWorkDate", IsEnabled = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "작업일자", Control = new TextBox() { Name = "txtWorkDate", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "시작시간", Control = new TextBox() { Name = "txtStartDateTime", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "종료시간", Control = new TextBox() { Name = "txtEndDateTime", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, IsNewLine = true });
            lst.Add(new ResultElement { Title = "버전", Control = new TextBox() { Name = "txtVersion", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "Lane수", Control = new C1NumericBox() { Name = "txtLaneQty", IsEnabled = false, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center, ShowButtons = false } });
           
            return lst;
        }

        public static List<ResultElement> DefectLaneSlitterList(int elementCountPerRow)
        {
            List<ResultElement> lst = new List<ResultElement>();

            //lst.Add(new ResultElement { Title = "작업일자", Control = new C1DateTimePicker() { Name = "dtpWorkDate", IsEnabled = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "작업일자", Control = new TextBox() { Name = "txtWorkDate", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "시작시간", Control = new TextBox() { Name = "txtStartDateTime", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "종료시간", Control = new TextBox() { Name = "txtEndDateTime", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, IsNewLine = true });
            lst.Add(new ResultElement { Title = "버전", Control = new TextBox() { Name = "txtVersion", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "Lane수", Control = new C1NumericBox() { Name = "txtCurLaneQty",  IsEnabled = false, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center, ShowButtons = false } });
            lst.Add(new ResultElement { Title = "실물Lane수", Control = new C1NumericBox() { Name = "txtLaneQty", IsEnabled = false, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center, ShowButtons = false } });
            return lst;
        }

        public static List<ResultElement> SrsSlitterList(int elementCountPerRow)
        {
            List<ResultElement> lst = new List<ResultElement>();

            //lst.Add(new ResultElement { Title = "작업일자", Control = new C1DateTimePicker() { Name = "dtpWorkDate", IsEnabled = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "작업일자", Control = new TextBox() { Name = "txtWorkDate", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "시작시간", Control = new TextBox() { Name = "txtStartDateTime", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "종료시간", Control = new TextBox() { Name = "txtEndDateTime", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center }, IsNewLine = true });
            lst.Add(new ResultElement { Title = "버전", Control = new TextBox() { Name = "txtVersion", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            lst.Add(new ResultElement { Title = "Lane수", Control = new C1NumericBox() { Name = "txtLaneQty", IsEnabled = false, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center, ShowButtons = false } });
            lst.Add(new ResultElement { Title = "보호필름", Control = new C1ComboBox() { Name = "cboPet", HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center } });
            
            return lst;
        }
    }
}