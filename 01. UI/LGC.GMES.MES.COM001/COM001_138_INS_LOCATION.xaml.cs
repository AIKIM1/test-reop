using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// khs_Ins_Location.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_138_INS_LOCATION : C1Window, IWorkArea
    {
        public COM001_138_INS_LOCATION()
        {
            InitializeComponent();
        }
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            String[] sFilter_cboArea = { LoginInfo.CFG_SHOP_ID, Area_Type.PACK };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, sFilter: sFilter_cboArea);


            SetAreaByAreaType();

            String[] sFilterType_cboType = { LoginInfo.LANGID, "RTLS_PRD_TYPE" };

            _combo.SetCombo(cboType, CommonCombo.ComboStatus.NONE, sCase: "COMMCODES", sFilter: sFilterType_cboType);

            String[] sFilterType_cboPosition = { LoginInfo.LANGID, "RTLS_POSITION_TYPE" };
            //MultiSelectionBox _mcombo = new MultiSelectionBox();
            _combo.SetCombo(cboPosition, CommonCombo.ComboStatus.NONE, sCase: "COMMCODES", sFilter: sFilterType_cboPosition);

        }
        private void SetAreaByAreaType()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue.ToString();
                RQSTDT.Rows.Add(dr);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboLine.ItemsSource = DataTableConverter.Convert(dtResult);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboLine.SelectedItemsToString == "")
                {
                    Util.Alert("SFU1223");  //라인을 선택하세요.
                    return;
                }
                if (string.IsNullOrEmpty(txt_locname.Text))
                {
                    Util.Alert("SFU8420"); //보관위치명을 입력하세요.
                    return;
                }

                Util.MessageConfirm("SFU1241", (result) =>// 저장 하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        LocationSave();
                    }
                });
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void LocationSave()
        {
            try
            {
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("PRDT_TYPE_CODE", typeof(string));
                INDATA.Columns.Add("POSITION_TYPE_CODE", typeof(string));
                INDATA.Columns.Add("LOCATION_NAME", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("SHOPID", typeof(string));
               
                DataRow dr = INDATA.NewRow();
                dr["EQSGID"] = cboLine.SelectedItemsToString;
                dr["PRDT_TYPE_CODE"] = cboType.SelectedValue.ToString();
                dr["POSITION_TYPE_CODE"] = cboPosition.SelectedValue.ToString();
                dr["LOCATION_NAME"] = txt_locname.Text;
                dr["USERID"] = LoginInfo.USERID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                INDATA.Rows.Add(dr);


                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_RTLS_REG_LOCATION", "INDATA", "", INDATA);

                cboLine.ItemsSource = DataTableConverter.Convert(dtResult);

                Util.MessageInfo("SFU3532");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //private void cboArea_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        //{
        //    try
        //    {
        //        cboLine.isAllUsed = false;
        //        SetCboEQSG();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        private void SetCboEQSG()
        {
            try
            {
                this.cboArea.SelectedValueChanged -= new EventHandler<PropertyChangedEventArgs<object>>(cboArea_SelectedValueChanged);


                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue.ToString();
                RQSTDT.Rows.Add(dr);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboLine.ItemsSource = DataTableConverter.Convert(dtResult);

                this.cboArea.SelectedValueChanged += new EventHandler<PropertyChangedEventArgs<object>>(cboArea_SelectedValueChanged);

                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                cboLine.isAllUsed = false;
                SetCboEQSG();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
