using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
//
using C1.WPF;
using LGC.GMES.MES.Common;
using C1.WPF.PdfViewer;
using System.Data;
using System.Net;
using System.IO.IsolatedStorage;
using System.Windows.Threading;

namespace LGC.GMES.MES.ControlsLibrary
{
	/// <summary>
	/// ManualViewer.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class ManualViewer : C1Window
	{

		private string _formName = "";
		string _fileName = string.Empty;
		private string _manualPageNo;
		private int _PageNo = 0;
		
		public ManualViewer()
		{
			InitializeComponent();
			try
			{
				this.CenterOnScreen();
				this._formName = "";
				LoadPdf();
				this.Loaded += ManualViewer_Loaded;
			}
			catch (Exception ex)
			{
			}
		}

        public ManualViewer(string pMenuNames, string pManualPageNo)
		{
			InitializeComponent();
			try
			{
				this.CenterOnScreen();
				this._formName = pMenuNames;
				this._manualPageNo = pManualPageNo;
				LoadPdf();
				_resizeTimer.Tick += _resizeTimer_Tick;

				this.Loaded += ManualViewer_Loaded;
			}
			catch (Exception ex)
			{
			}
		}

		string[] _menuDepth;
		string menuLanguage;
		char x = (char)65279;
		void ManualViewer_Loaded(object sender, RoutedEventArgs e)
		{
			this.Loaded -= ManualViewer_Loaded;
			MakeBookmarkTree();
		}


		private void LoadPdfOnLoad()
		{
			LoadPdf();
		}

		WebClient wc;
		private void LoadPdf()
		{
			string _fileName = "MES.Manual_" + LoginInfo.LANGID + ".pdf";
            //Uri filepath = new Uri(Common.Common.DeploymentUrl + _fileName);
            //Uri filepath = new Uri(@"http://localhost/" + _fileName);

            Uri filepath = new Uri(AppDomain.CurrentDomain.BaseDirectory.ToString() + _fileName);
			wc = new WebClient();
			wc.OpenReadAsync(filepath);
			wc.OpenReadCompleted += wc_OpenReadCompleted;
		}

		void wc_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
		{
			try
			{
				pdf.LoadDocument(e.Result);
				GoToBookmarkByPageNo();
				pdf.Loaded += pdf_Loaded;
			}
			catch (Exception ex)
			{
				//C1MessageBox.Show("Could not load PDF file:\r\n" + ex.Message);
			}

		}

		void pdf_Loaded(object sender, RoutedEventArgs e)
		{
			GoToBookmarkByPageNo();
		}

		private void GoToBookmarkByPageNo()
		{
			try
			{
				_PageNo = int.Parse(_manualPageNo);
			}
			catch (Exception ex)
			{
				_PageNo = 1;
			}

			if (_PageNo != 0)
			{
				pdf.GoToPage(_PageNo);
				pdf.ViewMode = ViewMode.FitWidth;
			}
		}

		private void GoToBookmarkByFormName()
		{
			if (_formName != "")
			{
				_menuDepth = _formName.Split(',');
				List<string> titles = new List<string>();
				titles.Insert(0, "SFU");
				for (int i = 0; i < _menuDepth.Length; i++)
				{
					if (checkOtherLetter(_menuDepth[i]))
					{
						menuLanguage = _menuDepth[i];
					}
					else
					{
						menuLanguage = x + _menuDepth[i];
					}
					titles.Insert(i + 1, menuLanguage);
					//titles.Insert(i + 1, _menuDepth[i]);
				}

				pdf.GoToBookmark(titles.ToArray());
			}
		}


		private bool checkOtherLetter(string pStr)
		{
			char[] charArr = pStr.ToCharArray();
			foreach (char c in charArr)
			{
				if (char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.OtherLetter)
				{
					return false;
				}
			}

			return true;
		}


		private void btnOK_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = MessageBoxResult.OK;
		}
		 




		void MakeBookmarkTree()
		{
            //loadingIndicator.Visibility = System.Windows.Visibility.Visible;
            DataTable menuIndataTable = new DataTable();
            menuIndataTable.Columns.Add("USERID", typeof(string));
            menuIndataTable.Columns.Add("PROGRAMTYPE", typeof(string));
            menuIndataTable.Columns.Add("MENUIUSE", typeof(string));
            menuIndataTable.Columns.Add("LANGID", typeof(string));
            menuIndataTable.Columns.Add("MENULEVEL", typeof(string));

            DataRow menuIndata = menuIndataTable.NewRow();
            menuIndata["USERID"] = LoginInfo.USERID;
            menuIndata["PROGRAMTYPE"] = LGC.GMES.MES.Common.Common.APP_System;
            menuIndata["MENUIUSE"] = "Y";
            menuIndata["LANGID"] = LoginInfo.LANGID;
            menuIndata["MENULEVEL"] = null;
            menuIndataTable.Rows.Add(menuIndata);
            new ClientProxy().ExecuteService("DA_BAS_SEL_MENU_WITH_BOOKMARK", "INDATA", "OUTDATA", menuIndataTable, (menuResult, menuException) =>
            {
                try
                {
                    if (menuException != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(menuException), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    try
                    {
                        tvProcTree.Items.Clear();

                        if (menuResult.Rows.Count > 0)
                        {
                            C1TreeViewItem treeViewItemMain = new C1TreeViewItem();
                            treeViewItemMain.Header = ObjectDic.Instance.GetObjectName("HelpTitle"); //"표지";
                            treeViewItemMain.Click += treeViewItem_Click;
                            tvProcTree.Items.Add(treeViewItemMain);
                            //setTreeMenuItems(tvProcTree, txtLotId.Text, DataTableConverter.Convert(searchResult["OUT_DATA_PROC_HIST"]));
                            setTreeMenuItems(tvProcTree, "SFU010000000", DataTableConverter.Convert(menuResult));
                            C1TreeViewItem treeViewItemTail = new C1TreeViewItem();
                            treeViewItemTail.Header = ObjectDic.Instance.GetObjectName("HelpSetup"); //"설치방법";
                            treeViewItemTail.Click += treeViewItem_Click;
                            tvProcTree.Items.Add(treeViewItemTail);
                            OpenBookmark();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.WriteLine("[MANUAL] Create Menu", ex);
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    C1MessageBox.Show(MessageDic.Instance.GetMessage(ex), "Error", C1MessageBoxButton.OK, C1MessageBoxIcon.Error);
                }
                finally
                {
                    //loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                }

                //         //loadingIndicator.Visibility = System.Windows.Visibility.Visible;
                //         DataTable menuIndataTable = new DataTable();
                //menuIndataTable.Columns.Add("USERID", typeof(string));
                //menuIndataTable.Columns.Add("AUTHID", typeof(string));
                //menuIndataTable.Columns.Add("LANG_TYPE", typeof(string));
                //menuIndataTable.Columns.Add("PROGRAMTYPE", typeof(string));
                //menuIndataTable.Columns.Add("SHOPID", typeof(string));
                //DataRow menuIndata = menuIndataTable.NewRow();
                //menuIndata["USERID"] = LoginInfo.USERID;
                //menuIndata["AUTHID"] = LoginInfo.AUTHID;
                //menuIndata["LANG_TYPE"] = LoginInfo.LANGID;
                //menuIndata["PROGRAMTYPE"] = "SUI";
                //menuIndata["SHOPID"] = CustomConfig.Instance.CONFIG_COMMON_SHOP;
                //menuIndataTable.Rows.Add(menuIndata);
                //new ClientProxy().ExecuteService("COR_SEL_MENU_WITH_BOOKMARK_G", "INDATA", "OUTDATA", menuIndataTable, (menuResult, menuException) =>
                //{
                //	try
                //	{
                //		if (menuException != null)
                //		{
                //			LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(menuException), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //			return;
                //		}
                //		try
                //		{
                //			tvProcTree.Items.Clear();

                //                     if (menuResult.Rows.Count > 0)
                //			{
                //				C1TreeViewItem treeViewItemMain = new C1TreeViewItem();
                //				treeViewItemMain.Header = ObjectDic.Instance.GetObjectName("HelpTitle"); //"표지";
                //				treeViewItemMain.Click += treeViewItem_Click;
                //				tvProcTree.Items.Add(treeViewItemMain);
                //				//setTreeMenuItems(tvProcTree, txtLotId.Text, DataTableConverter.Convert(searchResult["OUT_DATA_PROC_HIST"]));
                //				setTreeMenuItems(tvProcTree, "", DataTableConverter.Convert(menuResult));
                //				C1TreeViewItem treeViewItemTail = new C1TreeViewItem();
                //				treeViewItemTail.Header = ObjectDic.Instance.GetObjectName("HelpSetup"); //"설치방법";
                //				treeViewItemTail.Click += treeViewItem_Click;
                //				tvProcTree.Items.Add(treeViewItemTail);
                //				OpenBookmark();
                //			}
                //		}
                //		catch (Exception ex)
                //		{
                //			Logger.Instance.WriteLine("[MANUAL] Create Menu", ex);
                //			LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //		}
                //	}
                //	catch (Exception ex)
                //	{
                //		C1MessageBox.Show(MessageDic.Instance.GetMessage(ex), "Error", C1MessageBoxButton.OK, C1MessageBoxIcon.Error);
                //	}
                //	finally
                //	{
                //                 //loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                //	}
            }
			);

		}

		private void OpenBookmark()
		{
			if (_formName != "")
			{
				_menuDepth = _formName.Split(',');

				string _topMenu = "";
				string _middleMenu = "";
				string _bottomMenu = "";
				if (_menuDepth.Length == 3)
				{
					_topMenu = _menuDepth[0];
					_middleMenu = _menuDepth[1];
					_bottomMenu = _menuDepth[2];
				}

				//var _itemList = tvProcTree.Items.ToList();


				foreach (C1TreeViewItem TopItem in tvProcTree.Items)
				{
					if (TopItem.Header.ToString() == _topMenu)
					{
						TopItem.IsExpanded = true;
						TopItem.UpdateLayout();
						foreach (C1TreeViewItem MiddleItem in TopItem.Items)
						{
							if (MiddleItem.Header.ToString() == _middleMenu)
							{
								MiddleItem.IsExpanded = true;
								MiddleItem.UpdateLayout();

								foreach (C1TreeViewItem BottmeItem in MiddleItem.Items)
								{
									if (BottmeItem.Header.ToString() == _bottomMenu)
									{
										BottmeItem.IsSelected = true;
										BottmeItem.UpdateLayout();
									}
								}

							}
						}
					}
				}


			}
		}


		private void setTreeMenuItems(ItemsControl control, string MENUID_PR, System.Collections.IEnumerable treeMenuList)
		{
            IEnumerable<object> childMenuList = (from object menu in treeMenuList
                                                 where MENUID_PR.Equals(DataTableConverter.GetValue(menu, "MENUID_PR") == null ? "" : DataTableConverter.GetValue(menu, "MENUID_PR").ToString())
                                                 orderby DataTableConverter.GetValue(menu, "MENUSEQ")
                                                 select menu);

            foreach (object childMenu in childMenuList)
			{
                //C1TreeViewItem treeViewItem = new C1TreeViewItem();
                C1TreeViewItem treeViewItem = new C1TreeViewItem();
                treeViewItem.Click += treeViewItem_Click;
                treeViewItem.DataContext = childMenu;
                //treeViewItem.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                //treeViewItem.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch;
                if ("".Equals(MENUID_PR))
                {
                    //treeViewItem.HeaderTemplate = tvProcTree.Resources["ProcTreeViewItemTemplate"] as DataTemplate;
                }
                else
                {
                    //treeViewItem.HeaderTemplate = tvProcTree.Resources["EqptTreeViewItemTemplate"] as DataTemplate;
                }
                treeViewItem.Header = DataTableConverter.GetValue(childMenu, "MENUNAME");
                control.Items.Add(treeViewItem);
                setTreeMenuItems(treeViewItem, DataTableConverter.GetValue(childMenu, "MENUID").ToString(), treeMenuList);
            }
        }

		void treeViewItem_Click(object sender, SourcedEventArgs e)
		{

			C1TreeViewItem treeViewItem = sender as C1TreeViewItem;
			if (treeViewItem == null) return;

			if (treeViewItem.IsExpanded)
			{
				treeViewItem.Collapse();
			}
			else
			{
				treeViewItem.Expand();
			}

			string _menuLevel = "";
			if (DataTableConverter.GetValue(treeViewItem.DataContext, "MENULEVEL") != null)
			{
				_menuLevel = DataTableConverter.GetValue(treeViewItem.DataContext, "MENULEVEL").ToString();
			}


			if (treeViewItem.Header.ToString() == ObjectDic.Instance.GetObjectName("HelpTitle"))//"표지")
			{
				pdf.GoToPage(1);
				pdf.ViewMode = ViewMode.FitWidth;
				return;
			}

			if (treeViewItem.Header.ToString() == ObjectDic.Instance.GetObjectName("HelpSetup")) //"설치방법")
			{
				pdf.GoToPage(85);
				pdf.ViewMode = ViewMode.FitWidth;
				return;
			}

			if (_menuLevel != "3")
				return;


			string _treePageNoName = "";
			int _treePageNo = 0;
			if (DataTableConverter.GetValue(treeViewItem.DataContext, "MENUSEQ") != null)
			{
				_treePageNoName = DataTableConverter.GetValue(treeViewItem.DataContext, "MENUSEQ").ToString();
			}

			try
			{
				_treePageNo = int.Parse(_treePageNoName);
				_PageNo = _treePageNo;
			}
			catch (Exception ex)
			{
				_treePageNo = 1;
			}

			if (treeViewItem.Header.ToString() == ObjectDic.Instance.GetObjectName("HelpSetup")) //"설치방법")
				_treePageNo = 85;

			if (_treePageNo != 0)
			{
				pdf.GoToPage(_treePageNo);
				pdf.ViewMode = ViewMode.FitWidth;
			}

		}

		void treeViewItem_Click_byBookmark(object sender, SourcedEventArgs e)
		{

			C1TreeViewItem treeViewItem = sender as C1TreeViewItem;
			if (treeViewItem == null) return;
			string bookmark = treeViewItem.Header.ToString();
			if (bookmark == null || bookmark == "") return;
			List<string> titles = new List<string>();
			while (bookmark != null || bookmark == "")
			{
				if (checkOtherLetter(bookmark))
					menuLanguage = bookmark;
				else
					menuLanguage = x + bookmark;

				titles.Insert(0, menuLanguage);
				if (treeViewItem.Parent == null)
					break;
				treeViewItem = treeViewItem.Parent as C1TreeViewItem;
				if (treeViewItem == null) break; ;
				bookmark = treeViewItem.Header.ToString();
				if (bookmark == null) break;
			}
			try
			{
				pdf.GoToBookmark(titles.ToArray());
				pdf.ViewMode = ViewMode.FitWidth;
			}
			catch (Exception ex)
			{
			}
		}

		private void Rectangle_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
		{
			if (imgClose.Visibility == System.Windows.Visibility.Visible)
			{//닫을때
				tvProcTree.Width = 0;
				imgClose.Visibility = System.Windows.Visibility.Collapsed;
				imgOpen.Visibility = System.Windows.Visibility.Visible;
			}
			else
			{
				tvProcTree.Width = 200;
				imgClose.Visibility = System.Windows.Visibility.Visible;
				imgOpen.Visibility = System.Windows.Visibility.Collapsed;
			}
		}

		private void btnClose_Click_1(object sender, RoutedEventArgs e)
		{
			this.DialogResult = MessageBoxResult.OK;
		}


		DispatcherTimer _resizeTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 300), IsEnabled = false };

		private void pdf_SizeChanged_1(object sender, SizeChangedEventArgs e)
		{
			_resizeTimer.IsEnabled = true;
			_resizeTimer.Stop();
			_resizeTimer.Start();
		}

		void _resizeTimer_Tick(object sender, EventArgs e)
		{
			_resizeTimer.IsEnabled = false;    

			//Do end of resize processing
			pdf.GoToPage(1);
			pdf.GoToPage(_PageNo);
			pdf.UpdateLayout();
		}

	}
}
