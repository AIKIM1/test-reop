﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace LGC.GMES.MES.CMM001.Class
{
	public partial class inputBox
	{

		public static DialogResult InputBox(string title, string promptText, ref string value)
		{
		  Form form = new Form();
		  Label label = new Label();
		  TextBox textBox = new TextBox();
		  Button buttonOk = new Button();
		  Button buttonCancel = new Button();

		  form.Text = title;
		  label.Text = promptText;
		  textBox.Text = value;

		  buttonOk.Text = "OK";
		  buttonCancel.Text = "Cancel";
		  buttonOk.DialogResult = DialogResult.OK;
		  buttonCancel.DialogResult = DialogResult.Cancel;

		  label.SetBounds(12, 15, 372, 10);
		  textBox.SetBounds(12, 36, 372, 20);
		  buttonOk.SetBounds(228, 72, 75, 23);
		  buttonCancel.SetBounds(309, 72, 75, 23);

		  label.AutoSize = true;
		  textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
		  buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		  buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

		  form.ClientSize = new Size(396, 107);
		  form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
		  form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
		  form.FormBorderStyle = FormBorderStyle.FixedDialog;
		  form.StartPosition = FormStartPosition.CenterScreen;
		  form.MinimizeBox = false;
		  form.MaximizeBox = false;
		  form.AcceptButton = buttonOk;
		  form.CancelButton = buttonCancel;

		  DialogResult dialogResult = form.ShowDialog();
		  value = textBox.Text;
		  return dialogResult;
		}


	}
}
