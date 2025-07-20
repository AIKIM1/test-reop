/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Data;
using System.Windows.Controls;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Class;

namespace LGC.GMES.MES.MNT001
{
    public partial class MNT001_001_CMI1 : UserControl
    {
        #region Declaration & Constructor 

        System.Media.SoundPlayer playerPLC = new System.Media.SoundPlayer(LGC.GMES.MES.MNT001.Properties.Resources.cmi_monitoring_bell);
        System.Media.SoundPlayer playerMng = new System.Media.SoundPlayer(LGC.GMES.MES.MNT001.Properties.Resources.cmi_monitoring_bell);
        
        public MNT001_001 MNT001_001;
        bool bSoundPlayBitMng = false;
        bool bSoundPlayBitPLC = false;
        public MNT001_001_CMI1()
        {
            InitializeComponent();
        }



        #endregion

        #region Initialize
        

        #endregion

        #region Event

        #endregion

        #region Mehod

        public void setLineMapInfo(DataSet dsRslt)
        {
            int iSoundCountMng = 0;
            int iSoundCountPLC = 0;

            int iSoundSelect = -1;

            if (dsRslt.Tables.IndexOf("CALLMGR_RSLTDT") > -1)
            {
                if (dsRslt.Tables["CALLMGR_RSLTDT"].Rows.Count > 0)
                {
                    for (int i = 0; i < dsRslt.Tables["CALLMGR_RSLTDT"].Rows.Count; i++)
                    {
                        string strEqptID = dsRslt.Tables["CALLMGR_RSLTDT"].Rows[i]["EQPTID"].ToString();
                        string strCallBit = dsRslt.Tables["CALLMGR_RSLTDT"].Rows[i]["CALLBIT"].ToString();
                        string strCallTime = dsRslt.Tables["CALLMGR_RSLTDT"].Rows[i]["CALLTIME"].ToString();

                        iSoundSelect = this.color_change(strEqptID, strCallBit, strCallTime);

                        if (!strCallBit.Equals("0"))
                        {
                            if (iSoundSelect == 0)
                            {
                                iSoundCountMng++;
                            }
                            else if (iSoundSelect == 1)
                            {
                                iSoundCountPLC++;
                            }
                        }
                    }
                }
            }

            // 메니져 호출 처리
            if (iSoundCountMng > 0)
            {

                if (!bSoundPlayBitMng)
                {
                    bSoundPlayBitMng = true;
                    playerMng.Play();
                }
            }
            else
            {
                if (bSoundPlayBitMng)
                {
                    bSoundPlayBitMng = false;
                    playerMng.Stop();
                }
            }

            // PLC 경고 처리
            if (iSoundCountPLC > 0)
            {
                
                if (!bSoundPlayBitPLC)
                {
                    bSoundPlayBitPLC = true;
                    playerPLC.Play();
                }
            }
            else
            {
                if (bSoundPlayBitPLC)
                {
                    bSoundPlayBitPLC = false;
                    playerPLC.Stop();
                }
            }

        }

        public int color_change(string sEqptID, string sCallBit, string sCallTime)
        {
            int iResult = 0;
            try
            {
                switch (sEqptID)
                {
                    case "H1PBMA101-1-2":
                        //1호기 BMA 조립 설비 1
                        this.uf_CallBitBackColorSet(this.border_Assy1, sCallBit, "1"); iResult = 0;
                        break;
                    case "H1PBMA101-1-3":
                        //1호기 BMA 조립 설비 2
                        this.uf_CallBitBackColorSet(this.border_Assy2, sCallBit, "1"); iResult = 0;
                        break;
                    case "H1PBMA101-1-4":
                        //1호기 BMA 조립 설비 3
                        this.uf_CallBitBackColorSet(this.border_Assy3, sCallBit, "1"); iResult = 0;
                        break;
                    case "H1PBMA101-1-5":
                        //1호기 BMA 조립 설비 4
                        this.uf_CallBitBackColorSet(this.border_Assy4, sCallBit, "1"); iResult = 0;
                        break;
                    case "H1PBMA101-1-6":
                        //1호기 BMA 조립 설비 5
                        this.uf_CallBitBackColorSet(this.border_Assy5, sCallBit, "1"); iResult = 0;
                        break;
                    case "H1PBMA101-1-7":
                        //1호기 BMA 조립 설비 6
                        this.uf_CallBitBackColorSet(this.border_Assy6, sCallBit, "1"); iResult = 0;
                        break;
                    case "H1PBMA101-1-8":
                        //1호기 BMA 조립 설비 7
                        this.uf_CallBitBackColorSet(this.border_Assy7, sCallBit, "1"); iResult = 0;
                        break;
                    case "H1PBMA101-1-9":
                        //1호기 BMA 조립 설비 8
                        this.uf_CallBitBackColorSet(this.border_Assy8, sCallBit, "1"); iResult = 0;
                        break;
                    case "H1PBMA101-1-10":
                        //1호기 BMA 조립 설비 9
                        this.uf_CallBitBackColorSet(this.border_Assy9, sCallBit, "1"); iResult = 0;
                        break;
                    case "BMA_PLC_1":
                        this.uf_CallBitBackColorSet(this.border_PLC_BMA_1, sCallBit, "2"); iResult = 1;
                        break;
                    case "BMA_PLC_2":
                        this.uf_CallBitBackColorSet(this.border_PLC_BMA_2, sCallBit, "2"); iResult = 1;
                        break;
                    case "BMA_PLC_3":
                        this.uf_CallBitBackColorSet(this.border_PLC_BMA_3, sCallBit, "2"); iResult = 1;
                        break;
                    case "BMA_PLC_4":
                        this.uf_CallBitBackColorSet(this.border_PLC_BMA_4_1, sCallBit, "2");
                        this.uf_CallBitBackColorSet(this.border_PLC_BMA_4_2, sCallBit, "2"); iResult = 1;
                        break;
                    case "CMA_PLC_1":
                        this.uf_CallBitBackColorSet(this.border_PLC_CMA_1_1, sCallBit, "2");
                        this.uf_CallBitBackColorSet(this.border_PLC_CMA_1_2, sCallBit, "2"); iResult = 1;
                        break;
                    case "CMA_PLC_2":
                        this.uf_CallBitBackColorSet(this.border_PLC_CMA_2, sCallBit, "2"); iResult = 1;
                        break;
                    case "CMA_PLC_3":
                        this.uf_CallBitBackColorSet(this.border_PLC_CMA_3, sCallBit, "2"); iResult = 1;
                        break;
                    case "CMA_PLC_4":
                        this.uf_CallBitBackColorSet(this.border_PLC_CMA_4, sCallBit, "2"); iResult = 1;
                        break;
                }
            }
            catch(Exception ex)
            {
                Util.Alert(ex.ToString());
            }
            return iResult;
        }

        /// <summary>
        /// 배경 색상을 변경
        /// </summary>
        /// <param name="labTemp"> 변경할 label 객체</param>
        /// <param name="sCallBit">색상 변경 구분</param>
        /// <param name="sType"> 1 : 메니져 호출 / 2 : 경고음 호출</param>
        private void uf_CallBitBackColorSet(Border border, string sCallBit, string sType)
        {
            if (sType.Equals("1"))
            {
                if (sCallBit.Equals("1"))
                {
                    border.Background = Brushes.LawnGreen; // 호출 색상
                }
                else
                {
                    border.Background = Brushes.WhiteSmoke;  // 기본 색상
                }
            }
            else if (sType.Equals("2"))
            {
                if (sCallBit.Equals("0"))
                {
                    border.Background = Brushes.Black;  // 기본 색상 
                }
                else if (sCallBit.Equals("1"))
                {
                    border.Background = Brushes.Coral; // 경고 호출 색상
                }
                else if (sCallBit.Equals("2"))
                {
                    border.Background = Brushes.Red; // 에러 호출 색상
                }
            }

        }

        public void setQty(DataSet dsQty)
        {
            bool bYieldCheck = false;
            double dTotalYield = 1;

            if (dsQty.Tables.IndexOf("QTY_RSLTDT") > -1)
            {
                if (dsQty.Tables["QTY_RSLTDT"].Rows.Count > 0)
                {
                    for (int i = 0; i < dsQty.Tables["QTY_RSLTDT"].Rows.Count; i++)
                    {
                        string sType = dsQty.Tables["QTY_RSLTDT"].Rows[i]["TYPE"].ToString();
                        if (sType.Equals("QTY"))
                        {
                            this.tb_Qty.Text = dsQty.Tables["QTY_RSLTDT"].Rows[i]["QTY"].ToString();
                        }
                        else
                        //else if (sType.Equals("PLAN"))
                        {
                            this.tb_PlanQty.Text = dsQty.Tables["QTY_RSLTDT"].Rows[i]["QTY"].ToString();
                        }
                    }
                }
            }

            // RTY 적용
            if (dsQty.Tables.IndexOf("RTY_RSLTDT") > -1)
            {
                if (dsQty.Tables["RTY_RSLTDT"].Rows.Count > 0)
                {
                    for (int i = 0; i < dsQty.Tables["RTY_RSLTDT"].Rows.Count; i++)
                    {
                        string strProcid = dsQty.Tables["RTY_RSLTDT"].Rows[i]["PROCID"].ToString();
                        //string strProcName = dsLot.Tables["RTY_RSLTDT"].Rows[i]["PROCNAME"].ToString();
                        string strInQty = dsQty.Tables["RTY_RSLTDT"].Rows[i]["INQTY"].ToString(); //1
                        string strDefQty = dsQty.Tables["RTY_RSLTDT"].Rows[i]["DEFQTY"].ToString(); //5
                        string strOutQty = dsQty.Tables["RTY_RSLTDT"].Rows[i]["OUTQTY"].ToString(); //6
                        double dTRYield = double.Parse(dsQty.Tables["RTY_RSLTDT"].Rows[i]["TR_YIELD"].ToString());
                        string strTRYield = string.Format("{0:##0.00}", dTRYield); //7

                        string sObjName = this.uf_objectName(strProcid);

                        uf_ControlDataSet(sObjName + "01", strInQty);
                        uf_ControlDataSet(sObjName + "05", strDefQty);
                        uf_ControlDataSet(sObjName + "06", strOutQty);
                        uf_ControlDataSet(sObjName + "07", strTRYield);
                        if (dTRYield > 0)
                        {
                            bYieldCheck = true;
                            dTotalYield *= dTRYield / 100;
                        }
                    }
                    this.tb_RTY.Text = string.Format("{0:##0.00}", bYieldCheck ? dTotalYield * 100 : 0);
                }
            }

            // 수율 적용
            bYieldCheck = false;
            dTotalYield = 1; //초기화

            if (dsQty.Tables.IndexOf("YIELD_RSLTDT") > -1)
            {
                if (dsQty.Tables["YIELD_RSLTDT"].Rows.Count > 0)
                {
                    for (int i = 0; i < dsQty.Tables["YIELD_RSLTDT"].Rows.Count; i++)
                    {
                        string strProcid = dsQty.Tables["YIELD_RSLTDT"].Rows[i]["PROCID"].ToString();
                        //string strProcName = dsLot.Tables["RTY_RSLTDT"].Rows[i]["PROCNAME"].ToString();
                        string strInQty = dsQty.Tables["YIELD_RSLTDT"].Rows[i]["INQTY"].ToString(); //1
                        string strDefQty = dsQty.Tables["YIELD_RSLTDT"].Rows[i]["DEFQTY"].ToString(); //2
                        string strOutQty = dsQty.Tables["YIELD_RSLTDT"].Rows[i]["OUTQTY"].ToString(); //3
                        double dTRYield = double.Parse(dsQty.Tables["YIELD_RSLTDT"].Rows[i]["TR_YIELD"].ToString());
                        string strTRYield = string.Format("{0:##0.00}", dTRYield); //4

                        string sObjName = this.uf_objectName(strProcid);

                        //uf_ControlDataSet(sObjName + "01", strInQty);
                        uf_ControlDataSet(sObjName + "02", strDefQty);
                        uf_ControlDataSet(sObjName + "03", strOutQty);
                        uf_ControlDataSet(sObjName + "04", strTRYield);
                        if (dTRYield > 0)
                        {
                            bYieldCheck = true;
                            dTotalYield *= dTRYield / 100;
                        }
                    }
                    this.tb_Yield.Text = string.Format("{0:##0.00}", bYieldCheck ? dTotalYield * 100 : 0);
                }
            }
            
        }

        private string uf_objectName(string sProcid)
        {
            string sRtn = "";
            switch (sProcid)
            {
                case "P6000":
                    //BMA 조립
                    sRtn = "tb_Assy_";
                    break;
                case "P8000":
                    //BMA 기밀도 검사
                    sRtn = "tb_Tight_";
                    break;
                case "P8200":
                    //BMA EOL 검사 #1
                    sRtn = "tb_EOL1_";
                    break;
                case "P8300":
                    //BMA EOL 검사 #2
                    sRtn = "tb_EOL2_";
                    break;
            }
            return sRtn;
        }

        private void uf_ControlDataSet(string sControlName, string sValue)
        {
            var foundTextBox = (TextBlock)this.FindName(sControlName);
            
            foundTextBox.Text = sValue;
        }
        

        #endregion

    }
}
