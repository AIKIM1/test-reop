/*************************************************************************************
 Created Date : 2019.10.09
      Creator : Dooly
   Decription : 실적 확정시 RF-ID 입력 화면(Only Coating)
--------------------------------------------------------------------------------------
 [Change History]
  2019.10.09  김태균 : Initial Created.
**************************************************************************************/


using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ELEC_CST_RELATION.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_CST_RELATION : C1Window, IWorkArea
    {
        #region Declaration & Constructor         
        private string _LOTID = string.Empty;
        private string _EQPTID = string.Empty;
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
        public CMM_ELEC_CST_RELATION()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 1)
                {
                    _LOTID = Util.NVC(tmps[0]);
                    _EQPTID = Util.NVC(tmps[1]);
                }
                ApplyPermissions();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtCstID_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtCstID.Text))
                {
                    txtCstID.Focus();
                    txtCstID.SelectAll();
                    return;
                }
                else
                {
                    //저장하시겠습니까?
                    Util.MessageConfirm("SFU1241", (sresult) =>
                    {
                        if (sresult == MessageBoxResult.OK)
                        {

                            DataSet inData = new DataSet();

                            DataTable inTable = inData.Tables.Add("IN_EQP");
                            inTable.Columns.Add("SRCTYPE", typeof(string));
                            inTable.Columns.Add("IFMODE", typeof(string));
                            inTable.Columns.Add("EQPTID", typeof(string));
                            inTable.Columns.Add("CSTID", typeof(string));
                            inTable.Columns.Add("USERID", typeof(string));

                            DataRow indata = inTable.NewRow();
                            indata["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            indata["IFMODE"] = IFMODE.IFMODE_OFF;
                            indata["EQPTID"] = _EQPTID;
                            indata["CSTID"] = txtCstID.Text.Trim(); 
                            indata["USERID"] = LoginInfo.USERID;
                            inTable.Rows.Add(indata);

                            new ClientProxy().ExecuteService_Multi("BR_PRD_CHK_UNLOADER_CSTID_L", "IN_EQP", null, (result, searchException) =>
                            {
                                try
                                {
                                    if (searchException != null)
                                    {
                                        Util.MessageException(searchException);
                                        txtCstID.Focus();
                                        txtCstID.SelectAll();
                                        return;
                                    }

                                    DataTable inTable2 = new DataTable();
                                    inTable2.Columns.Add("LOTID", typeof(string));
                                    inTable2.Columns.Add("CSTID", typeof(string));
                                    inTable2.Columns.Add("PROCID", typeof(string));
                                    inTable2.Columns.Add("USERID", typeof(string));
                                    inTable2.Columns.Add("SRCTYPE", typeof(string));

                                    DataRow newRow = inTable2.NewRow();
                                    newRow["LOTID"] = _LOTID;
                                    newRow["CSTID"] = txtCstID.Text.Trim();
                                    newRow["PROCID"] = Process.COATING;
                                    newRow["USERID"] = LoginInfo.USERID;
                                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;

                                    inTable2.Rows.Add(newRow);

                                    new ClientProxy().ExecuteService("BR_PRD_REG_CSTID_USING_RFID", "INDATA", null, inTable2, (searchResult, ex) =>
                                    {
                                        if (ex != null)
                                        {
                                            Util.MessageException(searchException);
                                            return;
                                        }

                                        this.DialogResult = MessageBoxResult.OK;
                                    });

                                }
                                catch (Exception ex) { Util.MessageException(ex); }
                            }, inData);
 
                        }
                    });
                }
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }

            
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion


        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button> {btnSave};
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

        
    }
}
