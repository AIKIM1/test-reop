/*************************************************************************************
 Created Date : 2021.06.03
      Creator : 조영대
   Decription : 사용자정의 컨트롤 헤더.
--------------------------------------------------------------------------------------
 [Change History]
   2021.06.03  조영대 : Initial Created.
**************************************************************************************/

using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System.ComponentModel;
using System.Windows.Media.Animation;
using System;

namespace LGC.GMES.MES.FCS002.Controls
{
    public partial class UcListHeader : UserControl
    {
        #region Declaration
        
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private string headerId = string.Empty;
        [Browsable(false)]
        public string HeaderId
        {
            get { return headerId; }
            set
            {
                headerId = value;
            }
        }

        private string headerName = string.Empty;
        [Browsable(false)]
        public string HeaderName
        {
            get { return headerName; }
            set
            {
                txtHeaderName.Text = headerName = value;
            }
        }
        
        public UcListHeader()
        {
            InitializeComponent();

            InitializeControls();
        }

        #endregion

        #region Initialize

        public void InitializeControls()
        {
            txtHeaderName.Text = string.Empty;
        }


        #endregion

        #region Override

        #endregion

        #region Event

        #endregion

        #region Mehod
        

        #endregion


    }
}
