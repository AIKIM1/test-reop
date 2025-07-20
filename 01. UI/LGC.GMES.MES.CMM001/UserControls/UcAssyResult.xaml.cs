/*************************************************************************************
 Created Date : 2016.11.20
      Creator : JEONG JONGWON
   Decription : 원각형 조립 Result BaseForm
--------------------------------------------------------------------------------------
 [Change History]
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

namespace LGC.GMES.MES.CMM001.UserControls
{
    /// <summary>
    /// UcAssyResult.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcAssyResult
    {
        public UcAssyResult()
        {
            InitializeComponent();
        }

        private void txtNumeric_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if ( e.Text != ".")
                {
                    if(!char.IsDigit(c))
                    {
                        e.Handled = true;
                        break;
                    }
                }
            }
        }
    }
}
