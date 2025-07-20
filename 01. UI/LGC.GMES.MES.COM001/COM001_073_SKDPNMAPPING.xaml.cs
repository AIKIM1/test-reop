/*************************************************************************************
 Created Date : 2016.11.14
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.11.14  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_073_SKDPNMAPPING : C1Window, IWorkArea
    {

        #region Declaration & Constructor 

        public COM001_073_SKDPNMAPPING()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtSkidID.Focus();
            //ApplyPermissions();
            //SeachData();
        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        //private void ApplyPermissions()
        //{
        //    List<Button> listAuth = new List<Button>();
        //    //listAuth.Add(btnInReplace);
        //    Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        //}

        #endregion

        #region Event

        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= C1Window_Loaded;
        }

        private void txtSkid_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if(CheckSkid(txtSkidID.Text)) //Skid Id가 null이라면
                    txtLotID.Focus();
                else {

                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("이미 매핑된 SKID ID가 있습니다"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtSkidID.Focus();
                            txtSkidID.SelectAll();
                        }
                    });

                }

            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            DataTable gridList = DataTableConverter.Convert(dgEqptCond.ItemsSource);
            if(gridList.Rows.Count == 2)
            {
                for(int i = 0; i < 2; i++) { 
                    DataTable getGrid = DataTableConverter.Convert(dgEqptCond.ItemsSource);

                    DataTable saveData = new DataTable();
                    saveData.Columns.Add("SKIDID");
                    saveData.Columns.Add("LOTID");

                    DataRow indata = saveData.NewRow();
                    indata["SKIDID"] = txtSkidID;
                    indata["LOTID"] = getGrid.Rows[0][1];

                    var rslt = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_SKID_PANCAKE_MAPPING", "INDATA", "RSLTDT", saveData);
                    if (rslt.Rows.Count > 0)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show("Mapping 되었습니다.");
                        this.DialogResult = MessageBoxResult.Cancel;
                    }
                }
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void txtLotID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter) { 
                DataTable data = SearchLotInfo(txtLotID.Text);
                
                if (data.Rows.Count > 0)
                {
                    DrawGrid(txtSkidID.Text, data);
                }

                else
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ObjectDic.Instance.GetObjectName("LOT ID 가 없습니다.")), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtLotID.Focus();
                            txtLotID.SelectAll();
                        }
                    });
                }
                
            }
            
        }


        #endregion

        #region Mehod
        private void DrawGrid(string skidId, DataTable lotInfo)
        {
            DataTable setGrid = DataTableConverter.Convert(dgEqptCond.ItemsSource);
            if (setGrid.Rows.Count > 0)
            {
                if (setGrid.Rows[0][2].Equals(lotInfo.Rows[0][1]) && setGrid.Rows[0][4].Equals(lotInfo.Rows[0][3]) && setGrid.Rows[0][5].Equals(lotInfo.Rows[0][4]))
                {
                    DataRow dgRow = setGrid.NewRow();
                    dgRow["SKIDID"] = skidId;
                    dgRow["PANCAKEID"] = lotInfo.Rows[0][0];
                    dgRow["ELEC_TYPE_CODE"] = lotInfo.Rows[0][1];
                    dgRow["PJT"] = lotInfo.Rows[0][2];
                    dgRow["ProductID"] = lotInfo.Rows[0][3];
                    dgRow["ModelID"] = lotInfo.Rows[0][4];
                    dgRow["Quantity"] = lotInfo.Rows[0][5];

                    setGrid.Rows.Add(dgRow);
                    Util.gridClear(dgEqptCond);
                    dgEqptCond.ItemsSource = DataTableConverter.Convert(setGrid);

                    txtLotID.Focus();
                    txtLotID.SelectAll();
                }
                else
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ObjectDic.Instance.GetObjectName("먼저 입력한 팬케익과 전극 코드, 제품, 모델정보가 맞지 않습니다.")), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtLotID.Focus();
                            txtLotID.SelectAll();
                        }
                    });

            }
            else if (setGrid.Rows.Count == 0)
            {
                DataTable gridList = new DataTable();
                gridList.Columns.Add("SKIDID", typeof(string));
                gridList.Columns.Add("PANCAKEID", typeof(string));
                gridList.Columns.Add("ELEC_TYPE_CODE", typeof(string));
                gridList.Columns.Add("PJT", typeof(string));
                gridList.Columns.Add("ProductID", typeof(string));
                gridList.Columns.Add("ModelID", typeof(string));
                gridList.Columns.Add("Quantity", typeof(string));

                DataRow dgRow = gridList.NewRow();
                dgRow["SKIDID"] = skidId;
                dgRow["PANCAKEID"] = lotInfo.Rows[0][0];
                dgRow["ELEC_TYPE_CODE"] = lotInfo.Rows[0][1];
                dgRow["PJT"] = lotInfo.Rows[0][2];
                dgRow["ProductID"] = lotInfo.Rows[0][3];
                dgRow["ModelID"] = lotInfo.Rows[0][4];
                dgRow["Quantity"] = lotInfo.Rows[0][5];
                gridList.Rows.Add(dgRow);

                dgEqptCond.ItemsSource = DataTableConverter.Convert(gridList);


                txtLotID.Focus();
                txtLotID.SelectAll();
            }
            //else
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ObjectDic.Instance.GetObjectName("더이상 추가할 수 없습니다."));
            
        }


        private DataTable SearchLotInfo(string lotId)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.Columns.Add("LOTID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LOTID"] = lotId;

            RQSTDT.Rows.Add(dr);

            DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SKIP_MAPP_LOTINFO", "INDATA", "RSLTDT", RQSTDT);

            return dtMain;

        }

        private bool CheckSkid(string lotId)
        {
            DataTable RQSTDT = new DataTable("RQSTDT");
            RQSTDT.Columns.Add("SKIDID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["SKIDID"] = lotId;

            RQSTDT.Rows.Add(dr);

            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SKID_CHECK", "INDATA", "RSLTDT", RQSTDT);
            if(dt.Rows.Count >= 1)
                return false;

            return true;

        }
        
        #endregion
        
    }
}