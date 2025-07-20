using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace LGC.GMES.MES.ControlsLibrary
{
	public class InnerSearchTextBox : Control
	{
		private TextBox TbSearch;
		private Button BtnSearch;

		public event EventHandler ButtonClick;

		public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(InnerSearchTextBox), new PropertyMetadata(null));
		public string Text
		{
			get
			{
				TbSearch = base.GetTemplateChild("tbSearch") as TextBox;
				return (string)TbSearch.Text;
			}
			set
			{
				SetValue(TextProperty, value);
			}
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			BtnSearch = base.GetTemplateChild("btnSearch") as Button;
			BtnSearch.Click += BtnSearch_Click;
		}

		void BtnSearch_Click(object sender, RoutedEventArgs e)
		{
			if (ButtonClick != null)
			{
				ButtonClick(this, e);
			}
		}

	}
}
