/*************************************************************************************
 Created Date : 2016.08.19
      Creator : 
   Decription : 전극이력카드
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.19  : Initial Created.
  
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
using System.Windows.Shapes;
using System.Data;
using System.IO;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ELEC001
{
    /// <summary>
    /// ELEC001_PROCESSCARD
    /// </summary>
    public partial class ELEC001_PROCESSCARD : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        public string _PROCID = string.Empty;
        public string _LOTID = string.Empty;
        Util _Util = new Util();
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public ELEC001_PROCESSCARD()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }

            _PROCID = Util.NVC(tmps[0]);
            _LOTID= Util.NVC(tmps[1]);

            SetDataTable();
        }

        #endregion

        #region Mehod
        private void SetDataTable()
        {
            C1.C1Report.C1Report cr = new C1.C1Report.C1Report();
            cr.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;

            string filename = "ProcessCard.xml";
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

            using (Stream stream = a.GetManifestResourceStream("LGC.GMES.MES.ELEC001.Report." + filename))
            {
                cr.Load(stream, "RptHistoryCard");

                #region DataSet
                DataSet dsRptHistoryCard = new DataSet();

                #region dtHead
                DataTable dtHead = new DataTable("dtHead");
                dtHead.Columns.Add("HEAD_LOTID");
                dtHead.Columns.Add("HEAD_BARCODE");
                dtHead.Columns.Add("HEAD_VERSION");
                dtHead.Columns.Add("HEAD_ELECTRODE");
                dtHead.Columns.Add("HEAD_PRODID");
                dtHead.Columns.Add("HEAD_PRINTDATE");
                dtHead.Columns.Add("HEAD_EXPIREDATE");
                dtHead.Columns.Add("HEAD_MODELGRPNAME");
                dtHead.Columns.Add("HEAD_QTY");
                dtHead.Columns.Add("HEAD_QTY_BARCODE");
                dsRptHistoryCard.Tables.Add(dtHead);
                #endregion

                #region dtMixer
                DataTable dtMixer = new DataTable("dtMixer");
                dtMixer.Columns.Add("MIX1_BATCH");
                dtMixer.Columns.Add("MIX1_MTRL1");
                dtMixer.Columns.Add("MIX1_MTRL2");
                dtMixer.Columns.Add("MIX1_DATE1");
                dtMixer.Columns.Add("MIX1_DATE2");
                dtMixer.Columns.Add("MIX1_WORKER");
                dtMixer.Columns.Add("MIX1_JUMDO");
                dtMixer.Columns.Add("MIX1_GOHYUNG");
                dtMixer.Columns.Add("MIX1_PRODQTY");
                dtMixer.Columns.Add("MIX1_REMARK");
                dtMixer.Columns.Add("MIX_EQPTORDER");
                dsRptHistoryCard.Tables.Add(dtMixer);
                #endregion

                #region dtCoater            
                DataTable dtCoater = new DataTable("dtCoater");
                dtCoater.Columns.Add("COAT_LOTID");
                dtCoater.Columns.Add("COAT_WORKDATE");
                dtCoater.Columns.Add("COAT_WORKER");
                dtCoater.Columns.Add("COAT_PRODLEN");
                dtCoater.Columns.Add("COAT_PRODCELL");
                dtCoater.Columns.Add("COAT_REMARK");
                dtCoater.Columns.Add("COAT_EQPTORDER");
                dtCoater.Columns.Add("COAT_TOP_LOADING");
                dtCoater.Columns.Add("COAT_TOP_COATING");
                dtCoater.Columns.Add("COAT_TOP_UNCOATING");
                dtCoater.Columns.Add("COAT_BACK_LOADING");
                dtCoater.Columns.Add("COAT_BACK_COATING");
                dtCoater.Columns.Add("COAT_BACK_UNCOATING");
                dtCoater.Columns.Add("COAT_TOP_FOIL_L");
                dtCoater.Columns.Add("COAT_TOP_FOIL_M");
                dtCoater.Columns.Add("COAT_TOP_FOIL_R");
                dtCoater.Columns.Add("COAT_BACK_FOIL_L");
                dtCoater.Columns.Add("COAT_BACK_FOIL_M");
                dtCoater.Columns.Add("COAT_BACK_FOIL_R");
                dsRptHistoryCard.Tables.Add(dtCoater);
                #endregion

                #region dtRollPress
                DataTable dtRollPress = new DataTable("dtRollPress");
                dtRollPress.Columns.Add("RP_LOTID");
                dtRollPress.Columns.Add("RP_WORKDATE");
                dtRollPress.Columns.Add("RP_WORKER");
                dtRollPress.Columns.Add("RP_PRODLEN");
                dtRollPress.Columns.Add("RP_PRODCELL");
                dtRollPress.Columns.Add("RP_REMARK");
                dtRollPress.Columns.Add("RP_EQPTORDER");
                dtRollPress.Columns.Add("RP_ROLLTHICK");
                dtRollPress.Columns.Add("RP_RED");
                dtRollPress.Columns.Add("RP_BLUE");
                dtRollPress.Columns.Add("RP_WHITE");
                dtRollPress.Columns.Add("RP_YELLOW");
                dtRollPress.Columns.Add("RP_ORANGE");
                dtRollPress.Columns.Add("RP_PAPER");
                dsRptHistoryCard.Tables.Add(dtRollPress);
                #endregion

                #region dtSlitter
                DataTable dtSlitter = new DataTable("dtSlitter");
                dtSlitter.Columns.Add("SLIT_LOTID");
                dtSlitter.Columns.Add("SLIT_EQPTORDER");
                dtSlitter.Columns.Add("SLIT_WORKDATE");
                dtSlitter.Columns.Add("SLIT_WORKER");
                dtSlitter.Columns.Add("SLIT_CONNECT");
                dtSlitter.Columns.Add("SLIT_GOODCNT_TOT");
                dtSlitter.Columns.Add("SLIT_GOODCNT_1");
                dtSlitter.Columns.Add("SLIT_GOODCNT_2");
                dtSlitter.Columns.Add("SLIT_GOODCNT_3");
                dtSlitter.Columns.Add("SLIT_GOODCNT_4");
                dtSlitter.Columns.Add("SLIT_REMARK");
                dtSlitter.Columns.Add("SLIT_REMARK_1");
                dtSlitter.Columns.Add("SLIT_REMARK_2");
                dtSlitter.Columns.Add("SLIT_REMARK_3");
                dtSlitter.Columns.Add("SLIT_REMARK_4");
                dtSlitter.Columns.Add("SLIT_CUT");
                dsRptHistoryCard.Tables.Add(dtSlitter);
                #endregion

                #endregion

                dtMixer = GetList();

                #region  테스트용 출력
                foreach (DataTable dtData in dsRptHistoryCard.Tables)
                {
                    for (int col = 0; col < dtData.Columns.Count; col++)
                    {
                        string strColName = dtData.Columns[col].ColumnName;
                        if (cr.Fields.Contains(strColName)) cr.Fields[strColName].Text = col.ToString();//strColName;
                    }
                }
                #endregion                
            }
            c1DocumentViewer.Document = cr.FixedDocumentSequence;
        }
        private DataTable GetList()
        {
            try
            {

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["PROCID"] = _PROCID;
                Indata["LOTID"] = _LOTID;
                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESSCARD_INFO", "INDATA", "RSLTDT", IndataTable); ;

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }

        }

        #endregion


    }
}
