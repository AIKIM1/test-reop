using System;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.MainFrame
{
    /// <summary>
    /// Login.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Login : Window
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetSystemTime(ref SYSTEMTIME time);

        [StructLayout(LayoutKind.Sequential)]
        struct SYSTEMTIME
        {
            [MarshalAs(UnmanagedType.U2)]
            public short Year;
            [MarshalAs(UnmanagedType.U2)]
            public short Month;
            [MarshalAs(UnmanagedType.U2)]
            public short DayOfWeek;
            [MarshalAs(UnmanagedType.U2)]
            public short Day;
            [MarshalAs(UnmanagedType.U2)]
            public short Hour;
            [MarshalAs(UnmanagedType.U2)]
            public short Minute;
            [MarshalAs(UnmanagedType.U2)]
            public short Second;
            [MarshalAs(UnmanagedType.U2)]
            public short Milliseconds;

            public SYSTEMTIME(DateTime dt)
            {
                Year = (short)dt.Year;
                Month = (short)dt.Month;
                DayOfWeek = (short)dt.DayOfWeek;
                Day = (short)dt.Day;
                Hour = (short)dt.Hour;
                Minute = (short)dt.Minute;
                Second = (short)dt.Second;
                Milliseconds = (short)dt.Millisecond;
            }
        }

        private bool isObjectDicLoaded = false;
        private bool isMessageDicLoaded = false;
        private bool autoLogin;

        public Login()
        {
            this.autoLogin = true;

            App.Current.MainWindow = this;

            InitializeComponent();

            this.Loaded += new RoutedEventHandler(Login_Loaded);

            ////20160629 scpark
            //NavigateMainWindow();
            //this.Close();
        }

        public Login(bool autoLogin)
        {
            this.autoLogin = autoLogin;

            App.Current.MainWindow = this;

            InitializeComponent();

            this.Loaded += new RoutedEventHandler(Login_Loaded);
        }

        void Login_Loaded(object sender, RoutedEventArgs e)
        {
            if (LGC.GMES.MES.Common.Common.APP_MODE == "DEBUG")
            {
                tbISystem.Visibility = Visibility.Visible;
                tbISystem.Text = LGC.GMES.MES.Common.Common.APP_System + " - " + LGC.GMES.MES.Common.Common.APP_SBC_MODE;
            }
            else
            {
                tbISystem.Visibility = Visibility.Collapsed;
                tbISystem.Text = string.Empty;
            }

            Logger.Instance.WriteLine("[FRAME] Login Loaded", Logger.MESSAGE_OPERATION_START);
            this.Loaded -= new RoutedEventHandler(Login_Loaded);

            loadingIndicator.Visibility = Visibility.Visible;

            new ClientProxy().ExecuteService("BR_COR_SEL_AP_TIMEZONE", null, "OUTDATA", null, (aptzResult, aptzException) =>
            {
                try
                {
                    ClientProxy.APServerTimeZoneDiff = new TimeSpan(long.Parse(aptzResult.Rows[0]["TICK"].ToString()));
                }
                catch
                {
                }

                tbID.Text = LoginInfo.USERID;

                //SecureLoginInfo securedInfo = new SecureLoginInfo();
                //tbID.Text = securedInfo.ID;
                //if (autoLogin && securedInfo.AUTOLOGIN)
                //{
                //    tbPW.Password = securedInfo.PW;
                //    tbPWWatermark.Visibility = Visibility.Collapsed;
                //}

                //DataTable configIndataTable = new DataTable();
                //configIndataTable.Columns.Add("KEYID", typeof(string));
                //DataRow configIndata = configIndataTable.NewRow();
                //configIndata["KEYID"] = "BIZLOGLEVEL";
                //configIndataTable.Rows.Add(configIndata);

                //new ClientProxy().ExecuteService("COR_SEL_CONFIG_TBL", "INDATA", "OUTDATA", configIndataTable, (configResult, configException) =>
                //{
                //    if (configException != null)
                //    {
                //        Common.Common.bizloglevel = "-1";
                //    }
                //    else
                //    {
                //        if (configResult.Rows.Count == 0 || !"Y".Equals(configResult.Rows[0]["KEYIUSE"]))
                //        {
                //            Common.Common.bizloglevel = "-1";
                //        }
                //        else
                //        {
                //            Common.Common.bizloglevel = configResult.Rows[0]["KEYVALUE"].ToString();
                //        }
                //    }

                //    new ClientProxy().ExecuteService("COR_SEL_LANGUAGE", null, "RSLTDT", null, (result, ex) =>
                //    {
                //        loadingIndicator.Visibility = Visibility.Collapsed;
                //        if (ex != null)
                //        {
                //            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //            return;
                //        }

                //        cboLang.ItemsSource = DataTableConverter.Convert(result);

                //        if (!string.IsNullOrEmpty(LoginInfo.LANGID))
                //        {
                //            cboLang.SelectedValue = LoginInfo.LANGID;
                //        }
                //        else if (result.Rows.Count > 0)
                //        {
                //            cboLang.SelectedIndex = 0;
                //        }


                //        //if (!string.IsNullOrEmpty(securedInfo.LANGID))
                //        //{
                //        //    cboLang.SelectedValue = securedInfo.LANGID;
                //        //}
                //        //else if (result.Rows.Count > 0)
                //        //{
                //        //    cboLang.SelectedIndex = 0;
                //        //}

                //        //if (autoLogin && securedInfo.AUTOLOGIN)
                //        //{
                //        //    Logger.Instance.WriteLine("[FRAME] Auto Login", Logger.MESSAGE_OPERATION_START);
                //            btnLogin_Click(null, null);
                //        //    Logger.Instance.WriteLine("[FRAME] Auto Login", Logger.MESSAGE_OPERATION_END);
                //        //}

                //        //checkSSO();
                //    }
                //    );

                //}
                //);
            });

            Logger.Instance.WriteLine("[FRAME] Login Loaded", Logger.MESSAGE_OPERATION_END);
        }

        private void checkSSO()
        {
            loadingIndicator.Visibility = Visibility.Visible;

            string sBody = string.Empty;
            string[] sBodySplit = null;
            Object obj = null;
            string sso_id = "";

            SHDocVw.InternetExplorer exp = new SHDocVw.InternetExplorer();
            exp.Visible = false;

            try
            {
                exp.Navigate2("http://mesdnkr.lginnotek.com/sso/login_exec.asp", ref obj, ref obj, ref obj, ref obj);

                exp.NavigateComplete2 += (object pDisp, ref object URL) =>
                {
                    SHDocVw.InternetExplorer ssoCom = pDisp as SHDocVw.InternetExplorer;

                    try
                    {
                        mshtml.IHTMLDocument2 hd;

                        if (ssoCom != null)
                        {
                            hd = (mshtml.IHTMLDocument2)ssoCom.Document;
                            sBody = hd.body.outerHTML.ToString();
                            sBodySplit = sBody.Split('^');

                            if (sBodySplit.Length > 1)
                            {
                                sso_id = sBodySplit[1].ToUpper().ToString();

                                Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    trySSOLogin(sso_id);
                                }));
                            }
                            else
                            {
                                Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    loadingIndicator.Visibility = Visibility.Collapsed;
                                }));
                            }
                        }
                    }
                    catch (Exception comEx)
                    {
                    }
                    finally
                    {
                        if (ssoCom != null)
                            ssoCom.Quit();
                    }
                };

                //System.Threading.Thread.Sleep(1500);//1.5초
            }
            catch (Exception ex)
            {
            }
            finally
            {
                //if (exp != null)
                //    exp.Quit();
            }
        }

        private void trySSOLogin(string sso_id)
        {
            loadingIndicator.Visibility = Visibility.Visible;

            DataTable loginIndataTable = new DataTable();
            loginIndataTable.Columns.Add("USERID", typeof(string));
            loginIndataTable.Columns.Add("USERPSWD", typeof(string));
            loginIndataTable.Columns.Add("LANGID", typeof(string));
            DataRow loginIndata = loginIndataTable.NewRow();
            loginIndata["USERID"] = sso_id;
            loginIndata["USERPSWD"] = sso_id;
            loginIndata["LANGID"] = cboLang.SelectedValue;
            loginIndataTable.Rows.Add(loginIndata);

            new ClientProxy().ExecuteService("BR_COR_SEL_USERS_PASSWORD_CHK_G", "INDATA", "OUTDATA", loginIndataTable, (loginOutdata, loginException) =>
            {
                if (loginException != null)
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(loginException.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    Logger.Instance.WriteLine("[FRAME] Login", loginException);
                    return;
                }

                if (loginOutdata.Rows.Count < 1)
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    return;
                }

                DataRow userInfo = loginOutdata.Rows[0];

                LoginInfo.LANGID = cboLang.SelectedValue.ToString();
                LoginInfo.USGRID = userInfo["USGRID"] != null ? userInfo["USGRID"].ToString() : string.Empty;
                LoginInfo.USERID = userInfo["USERID"] != null ? userInfo["USERID"].ToString() : string.Empty;
                LoginInfo.USERNAME = userInfo["USERNAME"] != null ? userInfo["USERNAME"].ToString() : string.Empty;
                LoginInfo.AUTHID = userInfo["AUTHID"] != null ? userInfo["AUTHID"].ToString() : string.Empty;
                LoginInfo.SUPPLIERID = userInfo["SUPPLIERID"] != null ? userInfo["SUPPLIERID"].ToString() : string.Empty;

                DataTable objectDicIndataTable = new DataTable();
                objectDicIndataTable.Columns.Add("LANGID", typeof(string));
                objectDicIndataTable.Columns.Add("OBJECTIUSE", typeof(string));
                DataRow objectDicIndata = objectDicIndataTable.NewRow();
                objectDicIndata["LANGID"] = LoginInfo.LANGID;
                objectDicIndata["OBJECTIUSE"] = "Y";
                objectDicIndataTable.Rows.Add(objectDicIndata);

                new ClientProxy().ExecuteService("COR_SEL_OBJECTDIC_BAS", "INDATA", "OUTDATA", objectDicIndataTable, (objectDicResult, objectDicException) =>
                {
                    if (objectDicException != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(objectDicException.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    ObjectDic.Instance.SetObjectDicData(objectDicResult);

                    isObjectDicLoaded = true;

                    NavigateMainWindow();
                }, null, false, true);

                DataTable messageDicIndataTable = new DataTable();
                messageDicIndataTable.Columns.Add("LANGID", typeof(string));
                messageDicIndataTable.Columns.Add("MSGIUSE", typeof(string));
                DataRow messageDicIndata = messageDicIndataTable.NewRow();
                messageDicIndata["LANGID"] = LoginInfo.LANGID;
                messageDicIndata["MSGIUSE"] = "Y";
                messageDicIndataTable.Rows.Add(messageDicIndata);

                new ClientProxy().ExecuteService("COR_SEL_MESSAGE_BAS", "INDATA", "OUTDATA", messageDicIndataTable, (messageDicResult, messageDicException) =>
                {
                    if (messageDicException != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(messageDicException.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    MessageDic.Instance.SetMessageDicData(messageDicResult);

                    isMessageDicLoaded = true;

                    NavigateMainWindow();
                }, null, false, true);
            }, null, true);
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            //==================================================================================================================
            //NavigateMainWindow();
            //this.Close();

            if (loadingIndicator.Visibility == Visibility.Visible)
                return;

            loadingIndicator.Visibility = Visibility.Visible;

            Logger.Instance.WriteLine("[FRAME] Login", Logger.MESSAGE_OPERATION_START);

            DataTable loginIndataTable = new DataTable();
            loginIndataTable.Columns.Add("USERID", typeof(string));
            loginIndataTable.Columns.Add("USERPSWD", typeof(string));
            loginIndataTable.Columns.Add("LANGID", typeof(string));
            DataRow loginIndata = loginIndataTable.NewRow();
            loginIndata["USERID"] = LoginInfo.USERID;
            loginIndata["USERPSWD"] = LoginInfo.USERPSWD;
            //loginIndata["USERID"] = tbID.Text;
            //loginIndata["USERPSWD"] = tbPW.Password;
            loginIndata["LANGID"] = cboLang.SelectedValue;
            loginIndataTable.Rows.Add(loginIndata);

            //tag
            
            new ClientProxy().ExecuteService("COR_SEL_PERSON_TBL", "INDATA", "OUTDATA", loginIndataTable, (loginOutdata, loginException) =>
            //new ClientProxy().ExecuteService("BR_COR_SEL_USERS_PASSWORD_CHK", "INDATA", "OUTDATA", loginIndataTable, (loginOutdata, loginException) =>
            {
                if (loginException != null)
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(loginException.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    Logger.Instance.WriteLine("[FRAME] Login", loginException);
                    return;
                }

                //SecureLoginInfo securedInfo = new SecureLoginInfo();

                //if (loginOutdata.Rows.Count < 1)
                //{
                //    loadingIndicator.Visibility = Visibility.Collapsed;

                //    //securedInfo.LANGID = cboLang.SelectedValue.ToString();
                //    //securedInfo.ID = string.Empty;
                //    //securedInfo.PW = string.Empty;
                //    //securedInfo.AUTOLOGIN = false;

                //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show("There is no user information", null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //    Logger.Instance.WriteLine("[FRAME] Login", "There is no user information");
                //    return;
                //}

                DataRow userInfo = loginOutdata.Rows[0];

                //if (!userInfo["VERIFY"].ToString().Equals("Y"))
                //{
                //    loadingIndicator.Visibility = Visibility.Collapsed;
                //    securedInfo.LANGID = cboLang.SelectedValue.ToString();
                //    securedInfo.ID = tbID.Text;
                //    securedInfo.PW = string.Empty;
                //    securedInfo.AUTOLOGIN = false;

                //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show("Incorrect password", null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //    Logger.Instance.WriteLine("[FRAME] Login", "Incorrect password");
                //    return;
                //}

                //if (tbPW.Password.Equals(tbID.Text))
                //{
                //    loadingIndicator.Visibility = Visibility.Collapsed;

                //    ChangePWD popup = new ChangePWD(tbID.Text);
                //    popup.ShowDialog();

                //    return;
                //}

                Logger.Instance.WriteLine("[FRAME] Login", "Confirmed");

                //securedInfo.LANGID = cboLang.SelectedValue.ToString();
                //securedInfo.ID = tbID.Text;
                //securedInfo.PW = tbPW.Password;
                //if (securedInfo.AUTOLOGIN && !this.autoLogin)
                //{
                //    securedInfo.AUTOLOGIN = false;
                //}

                LoginInfo.LANGID = cboLang.SelectedValue.ToString();
                LoginInfo.SUPPORTEDLANGLIST = (from object r in cboLang.ItemsSource select DataTableConverter.GetValue(r, "LANGID").ToString()).ToArray();
                LoginInfo.SUPPORTEDLANGINFOLIST = cboLang.ItemsSource.Cast<object>().ToArray();
                LoginInfo.USGRID = userInfo["USGRID"] != null ? userInfo["USGRID"].ToString() : string.Empty;
                LoginInfo.USERID = userInfo["USERID"] != null ? userInfo["USERID"].ToString() : string.Empty;
                LoginInfo.USERNAME = userInfo["USERNAME"] != null ? userInfo["USERNAME"].ToString() : string.Empty;
                LoginInfo.AUTHID = userInfo["AUTHID"] != null ? userInfo["AUTHID"].ToString() : string.Empty;
                //LoginInfo.SUPPLIERID = userInfo["SUPPLIERID"] != null ? userInfo["SUPPLIERID"].ToString() : string.Empty;

                Logger.Instance.WriteLine("[FRAME] Loading multi-localization data", Logger.MESSAGE_OPERATION_START);
                DataTable objectDicIndataTable = new DataTable();
                objectDicIndataTable.Columns.Add("LANGID", typeof(string));
                objectDicIndataTable.Columns.Add("OBJECTIUSE", typeof(string));
                DataRow objectDicIndata = objectDicIndataTable.NewRow();
                objectDicIndata["LANGID"] = LoginInfo.LANGID;
                objectDicIndata["OBJECTIUSE"] = "Y";
                objectDicIndataTable.Rows.Add(objectDicIndata);

                new ClientProxy().ExecuteService("COR_SEL_OBJECTDIC_BAS", "INDATA", "OUTDATA", objectDicIndataTable, (objectDicResult, objectDicException) =>
                {
                    if (objectDicException != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(objectDicException.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    ObjectDic.Instance.SetObjectDicData(objectDicResult);

                    isObjectDicLoaded = true;

                    NavigateMainWindow();
                }, null, false, true);

                DataTable messageDicIndataTable = new DataTable();
                messageDicIndataTable.Columns.Add("LANGID", typeof(string));
                messageDicIndataTable.Columns.Add("MSGIUSE", typeof(string));
                DataRow messageDicIndata = messageDicIndataTable.NewRow();
                messageDicIndata["LANGID"] = LoginInfo.LANGID;
                messageDicIndata["MSGIUSE"] = "Y";
                messageDicIndataTable.Rows.Add(messageDicIndata);

                new ClientProxy().ExecuteService("COR_SEL_MESSAGE_BAS", "INDATA", "OUTDATA", messageDicIndataTable, (messageDicResult, messageDicException) =>
                {
                    if (messageDicException != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(messageDicException.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    MessageDic.Instance.SetMessageDicData(messageDicResult);

                    isMessageDicLoaded = true;

                    NavigateMainWindow();
                }, null, false, true);

                Logger.Instance.WriteLine("[FRAME] Loading multi-localization data", Logger.MESSAGE_OPERATION_END);
            }, null, true);

            Logger.Instance.WriteLine("[FRAME] Login", Logger.MESSAGE_OPERATION_END);
            //==================================================================================================================

            //if (loadingIndicator.Visibility == Visibility.Visible)
            //    return;

            //loadingIndicator.Visibility = Visibility.Visible;

            //Logger.Instance.WriteLine("[FRAME] Login", Logger.MESSAGE_OPERATION_START);

            //DataTable loginIndataTable = new DataTable();
            //loginIndataTable.Columns.Add("USERID", typeof(string));
            //loginIndataTable.Columns.Add("USERPSWD", typeof(string));
            //loginIndataTable.Columns.Add("LANGID", typeof(string));
            //DataRow loginIndata = loginIndataTable.NewRow();
            //loginIndata["USERID"] = tbID.Text;
            //loginIndata["USERPSWD"] = tbPW.Password;
            //loginIndata["LANGID"] = cboLang.SelectedValue;
            //loginIndataTable.Rows.Add(loginIndata);

            ////tag
            //new ClientProxy().ExecuteService("BR_COR_SEL_USERS_PASSWORD_CHK_G", "INDATA", "OUTDATA", loginIndataTable, (loginOutdata, loginException) =>
            //{
            //    if (loginException != null)
            //    {
            //        loadingIndicator.Visibility = Visibility.Collapsed;
            //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(loginException.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //        Logger.Instance.WriteLine("[FRAME] Login", loginException);
            //        return;
            //    }

            //    SecureLoginInfo securedInfo = new SecureLoginInfo();

            //    if (loginOutdata.Rows.Count < 1)
            //    {
            //        loadingIndicator.Visibility = Visibility.Collapsed;
            //        securedInfo.LANGID = cboLang.SelectedValue.ToString();
            //        securedInfo.ID = string.Empty;
            //        securedInfo.PW = string.Empty;
            //        securedInfo.AUTOLOGIN = false;

            //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show("There is no user information", null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //        Logger.Instance.WriteLine("[FRAME] Login", "There is no user information");
            //        return;
            //    }

            //    DataRow userInfo = loginOutdata.Rows[0];
            //    if (!userInfo["VERIFY"].ToString().Equals("Y"))
            //    {
            //        loadingIndicator.Visibility = Visibility.Collapsed;
            //        securedInfo.LANGID = cboLang.SelectedValue.ToString();
            //        securedInfo.ID = tbID.Text;
            //        securedInfo.PW = string.Empty;
            //        securedInfo.AUTOLOGIN = false;

            //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show("Incorrect password", null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //        Logger.Instance.WriteLine("[FRAME] Login", "Incorrect password");
            //        return;
            //    }

            //    if (tbPW.Password.Equals(tbID.Text))
            //    {
            //        loadingIndicator.Visibility = Visibility.Collapsed;

            //        ChangePWD popup = new ChangePWD(tbID.Text);
            //        popup.ShowDialog();

            //        return;
            //    }

            //    Logger.Instance.WriteLine("[FRAME] Login", "Confirmed");

            //    securedInfo.LANGID = cboLang.SelectedValue.ToString();
            //    securedInfo.ID = tbID.Text;
            //    securedInfo.PW = tbPW.Password;
            //    if (securedInfo.AUTOLOGIN && !this.autoLogin)
            //    {
            //        securedInfo.AUTOLOGIN = false;
            //    }

            //    LoginInfo.LANGID = cboLang.SelectedValue.ToString();
            //    LoginInfo.USGRID = userInfo["USGRID"] != null ? userInfo["USGRID"].ToString() : string.Empty;
            //    LoginInfo.USERID = userInfo["USERID"] != null ? userInfo["USERID"].ToString() : string.Empty;
            //    LoginInfo.USERNAME = userInfo["USERNAME"] != null ? userInfo["USERNAME"].ToString() : string.Empty;
            //    LoginInfo.AUTHID = userInfo["AUTHID"] != null ? userInfo["AUTHID"].ToString() : string.Empty;
            //    LoginInfo.SUPPLIERID = userInfo["SUPPLIERID"] != null ? userInfo["SUPPLIERID"].ToString() : string.Empty;

            //    Logger.Instance.WriteLine("[FRAME] Loading multi-localization data", Logger.MESSAGE_OPERATION_START);
            //    DataTable objectDicIndataTable = new DataTable();
            //    objectDicIndataTable.Columns.Add("LANGID", typeof(string));
            //    objectDicIndataTable.Columns.Add("OBJECTIUSE", typeof(string));
            //    DataRow objectDicIndata = objectDicIndataTable.NewRow();
            //    objectDicIndata["LANGID"] = LoginInfo.LANGID;
            //    objectDicIndata["OBJECTIUSE"] = "Y";
            //    objectDicIndataTable.Rows.Add(objectDicIndata);
            //    new ClientProxy().ExecuteService("COR_SEL_OBJECTDIC_BAS", "INDATA", "OUTDATA", objectDicIndataTable, (objectDicResult, objectDicException) =>
            //    {
            //        if (objectDicException != null)
            //        {
            //            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(objectDicException.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //            return;
            //        }

            //        ObjectDic.Instance.SetObjectDicData(objectDicResult);

            //        isObjectDicLoaded = true;

            //        NavigateMainWindow();
            //    }, null, false, true
            //    );

            //    DataTable messageDicIndataTable = new DataTable();
            //    messageDicIndataTable.Columns.Add("LANGID", typeof(string));
            //    messageDicIndataTable.Columns.Add("MSGIUSE", typeof(string));
            //    DataRow messageDicIndata = messageDicIndataTable.NewRow();
            //    messageDicIndata["LANGID"] = LoginInfo.LANGID;
            //    messageDicIndata["MSGIUSE"] = "Y";
            //    messageDicIndataTable.Rows.Add(messageDicIndata);
            //    new ClientProxy().ExecuteService("COR_SEL_MESSAGE_BAS", "INDATA", "OUTDATA", messageDicIndataTable, (messageDicResult, messageDicException) =>
            //    {
            //        if (messageDicException != null)
            //        {
            //            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(messageDicException.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //            return;
            //        }

            //        MessageDic.Instance.SetMessageDicData(messageDicResult);

            //        isMessageDicLoaded = true;

            //        NavigateMainWindow();
            //    }, null, false, true
            //    );

            //    Logger.Instance.WriteLine("[FRAME] Loading multi-localization data", Logger.MESSAGE_OPERATION_END);
            //}, null, true
            //);

            //Logger.Instance.WriteLine("[FRAME] Login", Logger.MESSAGE_OPERATION_END);
        }

        private void NavigateMainWindow()
        {
            if (isObjectDicLoaded && isMessageDicLoaded)
            {
                new ClientProxy().ExecuteService("BR_CUS_GET_SYSTIME", null, "OUTDATA", null, (result, ex) =>
                {
                    if (ex == null && result != null && result.Rows.Count > 0)
                    {
                        DateTime dt = (DateTime)result.Rows[0]["SYSTIME"];
                        SYSTEMTIME st = new SYSTEMTIME(dt.ToUniversalTime());

                        SetSystemTime(ref st);
                    }

                    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(LoginInfo.LANGID);
                    System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(LoginInfo.LANGID);

                    this.Visibility = System.Windows.Visibility.Collapsed;
                    new MainWindow().Show();
                    this.Close();
                });
            }
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void tbID_GotFocus(object sender, RoutedEventArgs e)
        {
            tbIDWatermark.Visibility = Visibility.Collapsed;
        }

        private void tbID_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbID.Text))
                tbIDWatermark.Visibility = Visibility.Visible;
        }

        private void tbID_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbID.Text))
                tbIDWatermark.Visibility = Visibility.Visible;
            else
                tbIDWatermark.Visibility = Visibility.Collapsed;
        }

        private void tbPWWatermark_GotFocus(object sender, RoutedEventArgs e)
        {
            tbPWWatermark.Visibility = Visibility.Collapsed;
        }

        private void tbPWWatermark_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbPW.Password))
                tbPWWatermark.Visibility = Visibility.Visible;
        }

        private void tbPW_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;

                btnLogin.Focus();
                btnLogin_Click(null, null);
            }
        }
    }
}