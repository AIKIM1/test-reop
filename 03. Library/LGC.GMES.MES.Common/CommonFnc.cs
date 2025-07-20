using C1.WPF;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Controls;

namespace LGC.GMES.MES.Common
{
    public static class CommonFnc
    {
        [DllImport("user32")]
        public static extern IntPtr FindWindow(String lpClassName, String lpWindowName);

        /// <summary>
        /// Application DoEvent
        /// </summary>
        /// <typeparam name="T">Action Parameter Type</typeparam>
        /// <param name="ApplyAction">Action</param>
        /// <param name="ActionParams">ActionParameter</param>
        public static void DoEvent<T>(Action<T> applyAction, object actionParams)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, applyAction, actionParams);
            }
            catch
            {
            }
            
        }

        /// <summary>
        /// Application DoEvent
        /// </summary>
        /// <param name="ApplyAction">Action</param>
        public static void DoEvent(Action applyAction)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, applyAction);
            }
            catch
            {
            }

        }

        public static string Sha512Encrypt(string InString)
        {
            try
            {

                byte[] data = UTF8Encoding.UTF8.GetBytes(InString);
                byte[] result;
                SHA512 shaM = new SHA512Managed();
                result = shaM.ComputeHash(data);

                StringBuilder strResult = new StringBuilder();

                foreach (byte b in result)
                {
                    strResult.AppendFormat("{0:x2}", b);
                }

                return strResult.ToString().ToUpper();
            }
            catch
            {
                return null;
            }

        }

        public static string AESDecrypt(string inputString, string key)
        {
            try
            {
                RijndaelManaged rijndaelCipher = new RijndaelManaged();
                rijndaelCipher.Mode = CipherMode.CBC;
                rijndaelCipher.Padding = PaddingMode.PKCS7;

                rijndaelCipher.KeySize = 128;
                rijndaelCipher.BlockSize = 128;

                byte[] encryptedData = Convert.FromBase64String(inputString);
                byte[] pwdBytes = Encoding.UTF8.GetBytes(key);
                byte[] keyBytes = new byte[16];

                int len = pwdBytes.Length;
                if (len > keyBytes.Length)
                {
                    len = keyBytes.Length;
                }

                Array.Copy(pwdBytes, keyBytes, len);

                rijndaelCipher.Key = keyBytes;
                rijndaelCipher.IV = keyBytes;
                byte[] plainText = rijndaelCipher.CreateDecryptor().TransformFinalBlock(encryptedData, 0, encryptedData.Length);

                return Encoding.UTF8.GetString(plainText);
            }
            catch { return string.Empty; }
        }


        public static string AESEncrypt(string inputString, string key)
        {
            try
            {
                RijndaelManaged rijndaelCipher = new RijndaelManaged();
                rijndaelCipher.Mode = CipherMode.CBC;
                rijndaelCipher.Padding = PaddingMode.PKCS7;

                rijndaelCipher.KeySize = 128;
                rijndaelCipher.BlockSize = 128;

                byte[] pwdBytes = Encoding.UTF8.GetBytes(key);
                byte[] keyBytes = new byte[16];

                int len = pwdBytes.Length;
                if (len > keyBytes.Length)
                {
                    len = keyBytes.Length;
                }

                Array.Copy(pwdBytes, keyBytes, len);
                rijndaelCipher.Key = keyBytes;
                rijndaelCipher.IV = keyBytes;
                ICryptoTransform transform = rijndaelCipher.CreateEncryptor();

                byte[] plainText = Encoding.UTF8.GetBytes(inputString);

                return Convert.ToBase64String(transform.TransformFinalBlock(plainText, 0, plainText.Length));
            }
            catch { return string.Empty; }
        }


        /// <summary>
        /// Set C1ComboBox Control by DataTable
        /// </summary>
        /// <param name="dataSource">DataSource</param>
        /// <param name="comboBox">C1ComboBox</param>
        /// <param name="selectedValue">Initial SelectedItem Value</param>
        public static void SetCombo(DataTable dataSource, C1ComboBox comboBox, string selectedValue = null)
        {
            try
            {
                comboBox.ItemsSource = dataSource.AsDataView();

                if (selectedValue != null)
                {
                    if (SetCboSelectedValues(comboBox, selectedValue) == -1)
                    {
                        comboBox.SelectedIndex = 0;
                    }
                }
                else
                {
                    if (dataSource.Rows.Count > 0)
                    {
                        comboBox.SelectedIndex = 0;
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, MsgType.Error.ToMsgString(), MessageBoxButton.OK, MessageBoxImage.Warning);
                //ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Set C1ComboBox Control by BizActor Service Info
        /// </summary>
        /// <param name="bizActorSvcID">BizActor Service ID</param>
        /// <param name="inData">BizActor inData Parameter</param>
        /// <param name="strInData">BizActor inData Name</param>
        /// <param name="strOutData">BizActor OutData Name</param>
        /// <param name="cboBox">C1ComboBox</param>
        /// <param name="initItem">Initial ComboBox Item (DataRow/Dictionary)</param>
        /// <param name="selectedValue">Initial SelectedItem Value</param>
        public static void SetCombo(string bizActorSvcID, string strInData, string strOutData, DataSet inData, C1ComboBox cboBox, object initItem = null, string selectedValue = null)
        {
            try
            {
                DataSet result = ClientProxyMom.ExecuteServiceSync(bizActorSvcID, strInData, strOutData, inData, ProcQueueType.SQL, null, true, true, false);    //2022.03.30 이승헌 NO LOG 로 변경, 2022.04.13 Set Combo는 SQL로 변경

                if (initItem != null)
                {
                    if (initItem is DataRow)
                    {
                        result.Tables[strOutData].Rows.InsertAt((DataRow)initItem, 0);
                    }
                    else if (initItem is Dictionary<string, object>)
                    {
                        DataRow drInit = result.Tables[strOutData].NewRow();

                        //데이터가 없는 경우만 넘어온 "initItem"값 add.
                        //if (drInit.Table.Rows.Count < 1)
                        //{
                        //    foreach (var key in ((Dictionary<string, object>)initItem).Keys)
                        //    {
                        //        drInit.Table.Columns.Add(new DataColumn(key));
                        //        drInit[key] = ((Dictionary<string, object>)initItem)[key];
                        //    }
                        //    result.Tables[strOutData].Rows.InsertAt(drInit, 0);
                        //}

                        //데이터 유무와 상관없이 넘어온 "initItem"값 add.
                        foreach (var key in ((Dictionary<string, object>)initItem).Keys)
                        {
                            //데이터가 없는 경우 컬럼 생성 후 데이터 넣어줌
                            if (drInit.Table.Rows.Count < 1)
                            {
                                drInit.Table.Columns.Add(new DataColumn(key));
                            }
                            drInit[key] = ((Dictionary<string, object>)initItem)[key];
                        }

                        result.Tables[strOutData].Rows.InsertAt(drInit, 0);
                    }
                }

                cboBox.ItemsSource = result.Tables[strOutData].AsDataView();

                //ComboBox SelectionMode가 Multiple인 경우(cboEQP인 경우)
                //if (cboBox.SelectionMode.ToString() == "Multiple")
                //{
                //    if (result.Tables[strOutData].Rows.Count == 0)
                //    {
                //        return;
                //    }
                //    var selectedItems = result.Tables[strOutData].AsEnumerable().Where(item => selectedValue.Split(',').Contains(item[cboBox.SelectedValuePath]));

                //    cboBox.SelectedItems.clear();

                //    foreach (var item in cboBox.ItemsSource)
                //    {
                //        if (selectedValue != null && selectedValue.Split(',').Contains(((DataRowView)item)[cboBox.SelectedValuePath].ToString()))
                //        {
                //            cboBox.SelectedItems.Add(item);
                //        }
                //    }
                //}
                ////ComboBox SelectionMode가 Single인 경우(cboEQP이 아닌 경우)
                //else
                //{
                //    if (selectedValue != null)
                //    {
                //        if (SetCboSelectedValues(cboBox, selectedValue) == -1)
                //        {
                //            cboBox.SelectedIndex = 0;
                //        }
                //    }
                //    else
                //    {
                //        cboBox.SelectedIndex = 0;
                //    }
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), MsgType.Error.ToMsgString(), MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public static ObservableCollection<T> ToObservableCollection<T>(IEnumerable<T> enumeration)
        {
            return new ObservableCollection<T>(enumeration);
        }

        /// <summary>
        /// Set UCMultiSelectComboBox Control by DataTable
        /// </summary>
        /// <param name="dataSource">DataSource</param>
        /// <param name="comboBox">C1ComboBox</param>
        /// <param name="selectedValue">Initial SelectedItem Value</param>
        //      public static void MultiSetCombo(DataTable dataSource, UCMultiSelectComboBox cboBox, string selectedValue = null)
        //{
        //	Dictionary<string, object> Items;
        //	Dictionary<string, object> SelectedItems;
        //	char[] deligator = { ',' };
        //	try
        //	{
        //		Items = new Dictionary<string, object>();

        //		for (int iRow = 0; iRow < dataSource.Rows.Count; iRow++)
        //		{
        //			Items.Add(dataSource.Rows[iRow][1].ToString(), dataSource.Rows[iRow][0].ToString());
        //		}
        //		cboBox.ItemsSource = Items;

        //		if (selectedValue != null)
        //		{
        //			SelectedItems = new Dictionary<string, object>();
        //			string[] strsplit = selectedValue.Split(deligator);

        //			for (int iCount = 0; iCount < strsplit.Length; iCount++)
        //			{
        //				for (int iRow = 0; iRow < dataSource.Rows.Count; iRow++)
        //				{
        //					//if
        //					if (strsplit[iCount] == dataSource.Rows[iRow][cboBox.SelectedValuePath].ToString())
        //					{
        //						SelectedItems.Add(dataSource.Rows[iRow][cboBox.DisplayMemberPath].ToString(), dataSource.Rows[iRow][cboBox.SelectedValuePath].ToString());
        //					}
        //				}
        //			}
        //			cboBox.SelectedItems = SelectedItems;
        //		}
        //	}
        //	catch (Exception ex)
        //	{
        //		MessageBox.Show(ex.Message, MsgType.Error.ToMsgString(), MessageBoxButton.OK, MessageBoxImage.Error);
        //	}
        //}

        /// <summary>
        /// Set UCMultiSelectComboBox Control by BizActor Service Info
        /// </summary>
        /// <param name="bizActorSvcID"></param>
        /// <param name="strInData"></param>
        /// <param name="strOutData"></param>
        /// <param name="inData"></param>
        /// <param name="cboBox"></param>
        /// <param name="initItem"></param>
        /// <param name="selectedValue"></param>
        //public static void MultiSetCombo(string bizActorSvcID, string strInData, string strOutData, DataSet inData, UCMultiSelectComboBox cboBox, DataRow initItem = null, string selectedValue = null)
        //{
        //	Dictionary<string, object> Items;
        //	Dictionary<string, object> SelectedItems;
        //	char[] deligator = { ',' };
        //	try
        //	{
        //		DataSet result = Variables.BizActorService.ExecBizRule(bizActorSvcID, inData, strInData, strOutData);

        //		if (initItem != null)
        //		{
        //			result.Tables[strOutData].Rows.InsertAt(initItem, 0);
        //		}

        //		Items = new Dictionary<string, object>();

        //		for (int iRow = 0; iRow < result.Tables[strOutData].Rows.Count; iRow++)
        //		{
        //                  if (cboBox.DisplayMemberPath != string.Empty && cboBox.SelectedValuePath != string.Empty)
        //                  {
        //                      Items.Add(result.Tables[strOutData].Rows[iRow][cboBox.DisplayMemberPath].ToString(), result.Tables[strOutData].Rows[iRow][cboBox.SelectedValuePath].ToString());
        //                  }
        //                  else
        //                  {
        //                      Items.Add(result.Tables[strOutData].Rows[iRow][1].ToString(), result.Tables[strOutData].Rows[iRow][0].ToString());
        //                  }

        //		}
        //		cboBox.ItemsSource = Items;

        //		if (selectedValue != null)
        //		{
        //                  SelectedItems = new Dictionary<string, object>();
        //                  if (selectedValue == "All")
        //                  {
        //                      SelectedItems.Add("All", "All");

        //                      for (int iRow = 0; iRow < result.Tables[strOutData].Rows.Count; iRow++)
        //                      {
        //                          SelectedItems.Add(result.Tables[strOutData].Rows[iRow][cboBox.DisplayMemberPath].ToString(), result.Tables[strOutData].Rows[iRow][cboBox.SelectedValuePath].ToString());
        //                      }

        //                      cboBox.SelectedItems = SelectedItems;
        //                  }
        //                  else
        //                  {
        //                      string[] strsplit = selectedValue.Split(deligator);

        //                      for (int iCount = 0; iCount < strsplit.Length; iCount++)
        //                      {
        //                          for (int iRow = 0; iRow < result.Tables[strOutData].Rows.Count; iRow++)
        //                          {
        //                              //if
        //                              if (strsplit[iCount] == result.Tables[strOutData].Rows[iRow][cboBox.SelectedValuePath].ToString())
        //                              {
        //                                  SelectedItems.Add(result.Tables[strOutData].Rows[iRow][cboBox.DisplayMemberPath].ToString(), result.Tables[strOutData].Rows[iRow][cboBox.SelectedValuePath].ToString());
        //                              }
        //                          }
        //                      }
        //                      cboBox.SelectedItems = SelectedItems;
        //                  }
        //		}
        //		else
        //		{
        //			SelectedItems = new Dictionary<string, object>();

        //			if (result.Tables[strOutData].Rows.Count > 0)
        //			{
        //				SelectedItems.Add(result.Tables[strOutData].Rows[0][cboBox.DisplayMemberPath].ToString(), result.Tables[strOutData].Rows[0][cboBox.SelectedValuePath].ToString());
        //			}
        //			cboBox.SelectedItems = SelectedItems;
        //		}
        //	}
        //	catch (Exception ex)
        //	{
        //		MessageBox.Show(MultiLangMessage.GetMessage(ex), MsgType.Error.ToMsgString(), MessageBoxButton.OK, MessageBoxImage.Error);
        //	}
        //}

        /// <summary>
        /// C1ComboBox Select
        /// </summary>
        /// <param name="cboBox"></param>
        /// <param name="selectedValue"></param>
        /// <param name="r1"></param>
        private static int SetCboSelectedValues(C1ComboBox cboBox, string selectedValue)
        {
            try
            {
                int index = 0;
                foreach (var item in cboBox.Items)
                {
                    if (((DataRowView)item)[cboBox.SelectedValuePath].Equals(selectedValue))
                    {
                        cboBox.SelectedIndex = index;
                        return index;
                    }
                    index++;
                }
            }
            catch (Exception)
            {
                return -1;
            }

            return -1;
        }

        /// <summary>
        /// Multi C1ComboBox Set
        /// str : CodeToName, NameToCode 2가지 경우
        /// </summary>
        /// <param name="cboBox"></param>
        /// <param name="selectedValue"></param>
        /// <param name="str"></param>
        public static string MultiSetCombo(C1ComboBox cboBox, string selectedValue, string str)
        {
            //  MultiSetCombo => MultiGetCombo 로 변경
            string[] strsplit = selectedValue.Split(',');
            string selectedCodeName = null;
            try
            {
                for (int j = 0; j < strsplit.Length; j++)
                {
                    for (int i = 0; i < cboBox.Items.Count; i++)
                    {
                        //CodeToName
                        if (str == "CodeToName")
                        {
                            //Code가 값으면 Name을 세팅
                            if (((DataRowView)cboBox.Items[i])[cboBox.SelectedValuePath].Equals(strsplit[j]))
                                selectedCodeName += ((DataRowView)cboBox.Items[i])[cboBox.DisplayMemberPath] + ",";
                        }
                        //NameToCode
                        else
                        {
                            //Name이 같으면 Code를 세팅
                            if (((DataRowView)cboBox.Items[i])[cboBox.DisplayMemberPath].Equals(strsplit[j]))
                                selectedCodeName += ((DataRowView)cboBox.Items[i])[cboBox.SelectedValuePath] + ",";
                        }
                    }
                }
                selectedCodeName = selectedCodeName?.Substring(0, selectedCodeName.Length - 1);

                //값이 세팅이 안되는 경우는 "Select"가 화면에 보이도록 수정
                //if (selectedCodeName == null)
                //{
                //    selectedCodeName += ((DataRowView)cboBox.Items[0])[cboBox.DisplayMemberPath];
                //}

                return selectedCodeName;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Multi C1ComboBox Set
        /// str : CodeToName, NameToCode 2가지 경우
        /// </summary>
        /// <param name="cboBox"></param>
        /// <param name="selectedValue"></param>
        /// <param name="str"></param>
        public static string MultiGetCombo(C1ComboBox cboBox, string selectedValue, string str)
        {
            //  MultiSetCombo => MultiGetCombo 로 변경
            string[] strsplit = selectedValue.Split(',');
            string selectedCodeName = null;
            try
            {
                for (int j = 0; j < strsplit.Length; j++)
                {
                    for (int i = 0; i < cboBox.Items.Count; i++)
                    {
                        //CodeToName
                        if (str == "CodeToName")
                        {
                            //Code가 값으면 Name을 세팅
                            if (((DataRowView)cboBox.Items[i])[cboBox.SelectedValuePath].Equals(strsplit[j]))
                                selectedCodeName += ((DataRowView)cboBox.Items[i])[cboBox.DisplayMemberPath] + ",";
                        }
                        //NameToCode
                        else
                        {
                            //Name이 같으면 Code를 세팅
                            if (((DataRowView)cboBox.Items[i])[cboBox.DisplayMemberPath].Equals(strsplit[j]))
                                selectedCodeName += ((DataRowView)cboBox.Items[i])[cboBox.SelectedValuePath] + ",";
                        }
                    }
                }
                selectedCodeName = selectedCodeName?.Substring(0, selectedCodeName.Length - 1);

                //값이 세팅이 안되는 경우는 "Select"가 화면에 보이도록 수정
                //if (selectedCodeName == null)
                //{
                //    selectedCodeName += ((DataRowView)cboBox.Items[0])[cboBox.DisplayMemberPath];
                //}

                return selectedCodeName;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Set BizActor Input Parameter DataSet
        /// </summary>
        /// <param name="dsInParams">Referenced Input Parameter DataSet</param>
        /// <param name="tableName">Input DataTable Name</param>
        /// <param name="column">Input DataColumn Name</param>
        /// <param name="value">Value</param>
        public static void MakeDataTable(ref DataSet dsInParams, string tableName, String columnName)
        {
            try
            {
                if (!dsInParams.Tables.Contains(tableName))
                {
                    dsInParams.Tables.Add(new DataTable(tableName));
                }

                if (!dsInParams.Tables[tableName].Columns.Contains(columnName))
                {
                    dsInParams.Tables[tableName].Columns.Add(new DataColumn(columnName));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void MakeDataTable(ref DataSet dsInParams, string tableName, String columnName, object value)
        {
            try
            {
                if (!dsInParams.Tables.Contains(tableName))
                {
                    dsInParams.Tables.Add(new DataTable(tableName));
                }

                if (!dsInParams.Tables[tableName].Columns.Contains(columnName))
                {
                    dsInParams.Tables[tableName].Columns.Add(new DataColumn(columnName));
                }

                DataTable tmp = dsInParams.Tables[tableName];

                SetInDataVal(ref tmp, columnName, value);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Set BizActor Input Parameter DataSet
        /// </summary>
        /// <param name="dsInParams">Referenced Input Parameter DataSet</param>
        /// <param name="tableName">Input DataTable Name</param>
        /// <param name="column">Input DataColumn Name/Type</param>
        /// <param name="value">Value</param>
        public static void MakeDataTable(ref DataSet dsInParams, string tableName, Dictionary<string, Type> column, object value)
        {
            try
            {
                if (!dsInParams.Tables.Contains(tableName))
                {
                    dsInParams.Tables.Add(new DataTable(tableName));
                }

                if (!dsInParams.Tables[tableName].Columns.Contains(column.ToArray()[0].Key))
                {
                    dsInParams.Tables[tableName].Columns.Add(new DataColumn(column.ToArray()[0].Key, column.ToArray()[0].Value));
                }

                DataTable tmp = dsInParams.Tables[tableName];

                SetInDataVal(ref tmp, column.ToArray()[0].Key, value);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void MakeDataTable(ref DataSet dsInParams, string tableName, Dictionary<string, Type> column)
        {
            try
            {
                if (!dsInParams.Tables.Contains(tableName))
                {
                    dsInParams.Tables.Add(new DataTable(tableName));
                }

                if (!dsInParams.Tables[tableName].Columns.Contains(column.ToArray()[0].Key))
                {
                    dsInParams.Tables[tableName].Columns.Add(new DataColumn(column.ToArray()[0].Key, column.ToArray()[0].Value));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Set DataValue to biz Input Parameter Data
        /// </summary>
        /// <param name="dataTable">Input DataTable</param>
        /// <param name="columnName">Input DataColumn Name</param>
        /// <param name="value">Value</param>
        private static void SetInDataVal(ref DataTable dataTable, string columnName, object value)
        {
            try
            {
                if (dataTable.Rows.Count == 0)
                {
                    DataRow dr = dataTable.NewRow();
                    dr[columnName] = value;
                    dataTable.Rows.Add(dr);
                }
                else
                {
                    if (string.IsNullOrEmpty(dataTable.Rows[dataTable.Rows.Count - 1][columnName].ToString()))
                    {
                        dataTable.Rows[dataTable.Rows.Count - 1][columnName] = value;
                    }
                    else
                    {
                        DataRow dr = dataTable.NewRow();
                        dr[columnName] = value;
                        dataTable.Rows.Add(dr);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool GetIsRegexIpAddr(string pIpAddr)
        {
            Regex regStr = new Regex(@"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}:\d{1,5}$");
            return (regStr.IsMatch(pIpAddr));
        }

        /// <summary>
        /// Change Dictionary object To String
        /// </summary>
        /// <param name="Dic"></param>
        /// <returns></returns>
        //public static string DicToString(Dictionary<string, object> Dic)
        //      {
        //          string result = string.Empty;
        //          try
        //          {
        //              foreach (KeyValuePair<string, object> pair in Dic)
        //              {
        //                  result += pair.Value + ",";
        //                  result = result.Substring(0, result.Length - 1);
        //              }
        //          }
        //          catch (Exception)
        //          {
        //              return result;
        //          }

        //          return result;
        //      }

        /// <summary>
        /// Change ComboBox object To String
        /// </summary>
        /// <param name="Cbo"></param>
        /// <returns></returns>
        public static string CboToString(ObservableCollection<object> Cbo)
        {
            string result = string.Empty;
            try
            {
                //저장 시 CBO_CODE 기준으로 저장함
                for (int i = 0; i < Cbo.Count; i++)
                {
                    result += ((DataRowView)Cbo[i]).Row.ItemArray[1] + ",";
                }
                result = result.Substring(0, result.Length - 1);
            }
            catch (Exception)
            {
                return result;
            }

            return result;
        }

        /// <summary>
        /// Get Local IP address
        /// </summary>
        /// <returns></returns>
        public static string Get_LocalIP()
        {
            IPHostEntry host = Dns.GetHostByName(Dns.GetHostName());
            string myip = host.AddressList[0].ToString();

            //2022.03.31 이승헌 - AddressFamily.InterNetwork IP4 찾기
            for (int i = 0; i < host.AddressList.Length; i++)
            {
                if (host.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    myip = host.AddressList[i].ToString();
                    break;
                }
            }

            return myip;
        }

        /// <summary>
        /// OK,NG 사운드 취득후 실행함.
        /// </summary>
        /// <param name="soundKubun">"OK" or "NG" </param>
        //public static void PlaySoundFile(SoundType soundType)
        //{
        //    try
        //    {
        //        //Sound UseYN설정이 false 이면 종료
        //        if (Variables.appConfigInfo.ISUSE_SOUND.Equals("N")) return;

        //        SoundPlayer wp = null;
        //        String toFilePath = Variables.SaveAsSoundFilePath;

        //        if (soundType == SoundType.OK)
        //        {
        //            //Sound play Thread 분리
        //            SoundPlay clsSoundPlay = new SoundPlay();
        //            clsSoundPlay.FilePath = toFilePath + Variables.OKSoundName;
        //            clsSoundPlay.sndType = SoundType.OK;

        //            Thread thread = new Thread(clsSoundPlay.Play);
        //            thread.Priority = ThreadPriority.Highest;
        //            thread.Start();
        //        }
        //        else if (soundType == SoundType.NG)
        //        {
        //            //Soundplay Thread 분리
        //            SoundPlay clssoundPlay = new SoundPlay();
        //            clssoundPlay.FilePath = toFilePath + Variables.NGSoundName;
        //            clssoundPlay.sndType = SoundType.NG;

        //            Thread thread = new Thread(clssoundPlay.Play);
        //            thread.Priority = ThreadPriority.Highest;
        //            thread.Start();
        //        }
        //        else if (soundType == SoundType.ModelChange)
        //        {
        //            //Soundplay Thread 분리
        //            SoundPlay clssoundPlay = new SoundPlay();
        //            clssoundPlay.FilePath = toFilePath + Variables.ModelChangeSoundName;
        //            clssoundPlay.sndType = SoundType.ModelChange;

        //            Thread thread = new Thread(clssoundPlay.Play);
        //            thread.Priority = ThreadPriority.Highest;
        //            thread.Start();
        //        }
        //        else if (soundType == SoundType.Alarm)
        //        {
        //            //Soundplay Thread 분리
        //            SoundPlay clssoundPlay = new SoundPlay();
        //            clssoundPlay.FilePath = toFilePath + Variables.AlarmSoundName;
        //            clssoundPlay.sndType = SoundType.Alarm;

        //            Thread thread = new Thread(clssoundPlay.Play);
        //            thread.Priority = ThreadPriority.Highest;
        //            thread.Start();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //}

        /// <summary>
        /// Send Data To another SFC App.
        /// </summary>
        /// <param name="dataToSend"></param>
        //public static bool SendDataToSFCTarget(string dataToSend)
        //{
        //    try
        //    {
        //        if (Variables.appConfigInfo.IS_DATASEND == "N")
        //        {
        //            return true;
        //        }

        //        bool result = true;

        //        foreach (var target in Variables.appConfigInfo.DATASENDTARGET.Split('|'))
        //        {
        //            try
        //            {
        //                //IP Address 할당 
        //                IPAddress ipaAddress = IPAddress.Parse(target);

        //                Ping ping = new Ping();
        //                PingOptions pingOptions = new PingOptions();

        //                pingOptions.DontFragment = true;

        //                PingReply pingReply = ping.Send(ipaAddress, 2000);

        //                if (pingReply != null)
        //                {
        //                    if (pingReply.Status == IPStatus.Success)
        //                    {
        //                        //TCP Client 선언 
        //                        TcpClient tcpClient = new TcpClient();

        //                        //TCP Client연결 
        //                        tcpClient.Connect(ipaAddress, 5002);

        //                        //NetworkStream을 생성 
        //                        NetworkStream ntwStream = tcpClient.GetStream();

        //                        byte[] data = Encoding.UTF8.GetBytes(dataToSend);
        //                        ntwStream.Write(data, 0, data.Length);

        //                        ntwStream.Close();
        //                        tcpClient.Close();

        //                        //Log 생성
        //                        Logger.GetInstance().WriteLog(LogLevel.EVENT, "DataSend To [" + target + "]", dataToSend);
        //                    }
        //                    else
        //                    {
        //                        string status = string.Empty;

        //                        switch (pingReply.Status)
        //                        {
        //                            case IPStatus.TimedOut:
        //                                status = "TimedOut";
        //                                break;
        //                            case IPStatus.TimeExceeded:
        //                                status = "TimeExceeded";
        //                                break;
        //                            case IPStatus.NoResources:
        //                                status = "NoResources";
        //                                break;
        //                            case IPStatus.TtlReassemblyTimeExceeded:
        //                                status = "TtlReassemblyTimeExceeded";
        //                                break;
        //                            case IPStatus.BadDestination:
        //                                status = "BadDestination";
        //                                break;
        //                            case IPStatus.BadRoute:
        //                                status = "BadRoute";
        //                                break;
        //                            default:
        //                                status = "Unknown";
        //                                break;
        //                        }
        //                        Logger.GetInstance().WriteLog(LogLevel.DEBUG, "SendDataToSFCTarget():" + "Ping Error" + "[" + target + "]", status);

        //                        IntPtr hMessageBox = FindWindow(null, "Data Send Error");
        //                        if (hMessageBox == IntPtr.Zero)
        //                        {
        //                            MessageBox.Show("Ping Error - " + status + " [" + target + "]", "Data Send Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    Logger.GetInstance().WriteLog(LogLevel.DEBUG, "SendDataToSFCTarget():" + "Ping Error" + "[" + target + "]", "no PingReply");

        //                    IntPtr hMessageBox = FindWindow(null, "Data Send Error");
        //                    if (hMessageBox == IntPtr.Zero)
        //                    {
        //                        MessageBox.Show("Ping Error - " + "Ping Null Exception" + " [" + target + "]", "Data Send Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                Logger.GetInstance().WriteException(LogLevel.DEBUG, "SendDataToSFCTarget():" + dataToSend + "[" + target + "]", ex);

        //                IntPtr hMessageBox = FindWindow(null, "Data Send Error");
        //                if (hMessageBox == IntPtr.Zero)
        //                {
        //                    MessageBox.Show("Ping Error - " + "Connection Refused" + " [" + target + "]", "Data Send Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //                }

        //                result = false;
        //            }
        //        }

        //        return result;
        //    }
        //    catch (System.Exception ex)
        //    {
        //        //Variables.fileLog.WriteException(LogLevel.DEBUG, "SendDataToSFCTarget():" + dataToSend, ex);
        //        return false;
        //    }
        //}

        /// <summary>
        /// IP Validation Check
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        public static bool IsValidIPAddress(string addr)
        {
            try
            {
                IPAddress address;
                return IPAddress.TryParse(addr, out address);

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Check whether SFC Application is Test Mode or Not
        /// </summary>
        /// <returns></returns>
        public static bool IsTestMode()
        {
            try
            {
                return System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName.Contains("LGCI.GMES.SFC.FRM.Main_T.exe");
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 현재 CustomConfig 정보의 Config Key 존재여부를 체크합니다.
        /// </summary>
        /// <param name="configName">Config 키</param>
        /// <returns>true : 존재, false : 미존재</returns>
        public static bool CheckCustomConfig(string configName)
        {
            bool ret = false;
            try
            {
                DataTable dt = Variables.dtCurretConfigInfo;
                if (dt != null && dt.Columns.Contains(configName) && dt.Rows.Count > 0 && !string.IsNullOrWhiteSpace(dt.Rows[0][configName].ToString()))
                {
                    ret = true;
                }

                return ret;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 현재 CustomConfig 정보에서 Config Key의 값을 체크합니다.
        /// </summary>
        /// <param name="configName">Config 키</param>
        /// <param name="configValue">Config 값</param>
        /// <returns>true : 일치, false : 불일치</returns>
        public static bool CheckCustomConfig(string configName, string configValue)
        {
            bool ret = false;
            try
            {
                if (CheckCustomConfig(configName))
                {
                    DataTable dt = Variables.dtCurretConfigInfo;
                    if (dt.Rows[0][configName].ToString() == configValue)
                    {
                        ret = true;
                    }
                }

                return ret;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 현재 CustomConfig 정보에서 Config Key의 값을 체크합니다.
        /// </summary>
        /// <typeparam name="T">Config 값 타입</typeparam>
        /// <param name="configName">Config 키</param>
        /// <param name="configValue">Config 값</param>
        /// <returns>true : 일치, false : 불일치</returns>
        public static bool CheckCustomConfig<T>(string configName, T configValue)
        {
            bool ret = false;
            try
            {
                if (CheckCustomConfig(configName))
                {
                    DataTable dt = Variables.dtCurretConfigInfo;

                    T configValueT = (T)Convert.ChangeType(dt.Rows[0][configName], typeof(T));
                    if (configValueT.Equals(configValue))
                    {
                        ret = true;
                    }
                }

                return ret;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 현재 Config 정보에서 지정한 키의 값을 가져옵니다.
        /// </summary>
        /// <param name="configName">Config 키</param>
        /// <returns>결과값</returns>
        public static string GetCustomConfig(string configName)
        {
            string ret = string.Empty;
            try
            {
                if (CheckCustomConfig(configName))
                {
                    DataTable dt = Variables.dtCurretConfigInfo;
                    ret = dt.Rows[0][configName].ToString();
                }
                return ret;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 현재 Config 정보에서 지정한 키의 값을 가져옵니다.
        /// </summary>
        /// <typeparam name="T">리턴 타입입니다.</typeparam>
        /// <param name="configName">Config 키</param>
        /// <returns>결과값</returns>
        public static T GetCustomConfig<T>(string configName)
        {
            T ret = default(T);
            Type type = typeof(T);

            try
            {
                if (CheckCustomConfig(configName))
                {
                    DataTable dt = Variables.dtCurretConfigInfo;
                    ret = (T)Convert.ChangeType(dt.Rows[0][configName], typeof(T));
                }

                return ret;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// DateTime값에서 날짜와 시간을 추출하여 yyyy-MM-dd HH:mm:ss형식으로 출력
        /// </summary>
        /// <param name="DateTime">Input DateTime</param>
        /// <returns>결과값</returns>

        public static string ToDateTimeString(this DateTime _dtDate)
        {
            return _dtDate.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static async Task<JObject> CallRestAPI(string path, JObject jsonObject)
        {
            string responseBody = string.Empty;

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = client.GetAsync(path).Result;
            if (response.IsSuccessStatusCode)
            {
                responseBody = await response.Content.ReadAsStringAsync();
            }
            return (string.IsNullOrEmpty(responseBody) ? new JObject() : JObject.Parse(responseBody));
        }

        public static async Task<JObject> CallRestAPI(string path, MultipartFormDataContent multipartFormContent)
        {
            string responseBody = string.Empty;

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = client.PostAsync(path, multipartFormContent).Result;
            if (response.IsSuccessStatusCode)
            {
                responseBody = await response.Content.ReadAsStringAsync();
            }

            return (string.IsNullOrEmpty(responseBody) ? new JObject() : JObject.Parse(responseBody));
        }




        /// <summary>
        /// Get WorkGuide Images By Prod ID
        /// </summary>
        /// <param name="prodId">Prod ID</param>
        /// <returns>WorkGuide Image</returns>
        //public static Grid GetImageByProdID(string prodId)
        //{
        //    Grid grdWorkGuide = new Grid();

        //    //Image imgWorkGuide = new Image();
        //    try
        //    {
        //        List<string> ImagePaths;

        //        //WorkGuide Process 
        //        if (Variables.appConfigInfo.WORKGUIDE_PROCESS != string.Empty)
        //        {
        //            ImagePaths = WorkGuide.GetImagePaths(Variables.appConfigInfo.WORKGUIDE_PROCESS, prodId);
        //        }
        //        else
        //        {
        //            ImagePaths = WorkGuide.GetImagePaths(Variables.CurrentProcID, prodId);
        //        }

        //        if (ImagePaths.Count == 0)
        //        {
        //            Image imgWorkGuide = new Image();
        //            imgWorkGuide.Source = new BitmapImage(new Uri("/LGE.SFC.ControlsLib;component/Images/Icons/NoImage_msg.png", UriKind.RelativeOrAbsolute));

        //            grdWorkGuide.Children.Add(imgWorkGuide);
        //        }
        //        else
        //        {
        //            if (ImagePaths.Count == 1)
        //            {
        //                Image imgWorkGuide1 = new Image();
        //                imgWorkGuide1.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[0]);
        //                imgWorkGuide1.SetValue(Grid.RowProperty, 0);
        //                imgWorkGuide1.Margin = new Thickness(2);

        //                grdWorkGuide.Children.Add(imgWorkGuide1);
        //            }
        //            else if (ImagePaths.Count == 2)
        //            {
        //                grdWorkGuide.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
        //                grdWorkGuide.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

        //                Image imgWorkGuide1 = new Image();
        //                imgWorkGuide1.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[0]);
        //                imgWorkGuide1.SetValue(Grid.RowProperty, 0);
        //                imgWorkGuide1.Margin = new Thickness(2);
        //                imgWorkGuide1.HorizontalAlignment = HorizontalAlignment.Stretch;
        //                imgWorkGuide1.VerticalAlignment = VerticalAlignment.Stretch;
        //                grdWorkGuide.Children.Add(imgWorkGuide1);

        //                Image imgWorkGuide2 = new Image();
        //                imgWorkGuide2.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[1]);
        //                imgWorkGuide2.SetValue(Grid.RowProperty, 1);
        //                imgWorkGuide2.Margin = new Thickness(2);
        //                grdWorkGuide.Children.Add(imgWorkGuide2);

        //                Rectangle line = new Rectangle();
        //                line.Stroke = new SolidColorBrush(Colors.DarkGray);
        //                line.Height = 2;
        //                line.HorizontalAlignment = HorizontalAlignment.Stretch;
        //                line.VerticalAlignment = VerticalAlignment.Bottom;
        //                line.Margin = new Thickness(0, 0, 0, -1);

        //                grdWorkGuide.Children.Add(line);
        //            }
        //            else if (ImagePaths.Count == 3)
        //            {
        //                grdWorkGuide.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
        //                grdWorkGuide.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

        //                grdWorkGuide.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
        //                grdWorkGuide.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

        //                Image imgWorkGuide1 = new Image();
        //                imgWorkGuide1.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[0]);
        //                imgWorkGuide1.SetValue(Grid.RowProperty, 0);
        //                imgWorkGuide1.Margin = new Thickness(2);
        //                grdWorkGuide.Children.Add(imgWorkGuide1);

        //                Image imgWorkGuide2 = new Image();
        //                imgWorkGuide2.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[1]);
        //                imgWorkGuide2.SetValue(Grid.RowProperty, 1);
        //                imgWorkGuide2.Margin = new Thickness(2);
        //                grdWorkGuide.Children.Add(imgWorkGuide2);

        //                Image imgWorkGuide3 = new Image();
        //                imgWorkGuide3.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[2]);
        //                imgWorkGuide3.SetValue(Grid.ColumnProperty, 1);
        //                imgWorkGuide3.SetValue(Grid.RowSpanProperty, 2);
        //                imgWorkGuide3.Margin = new Thickness(2);
        //                grdWorkGuide.Children.Add(imgWorkGuide3);

        //                Rectangle lineX = new Rectangle();
        //                lineX.Stroke = new SolidColorBrush(Colors.DarkGray);
        //                lineX.Height = 2;
        //                lineX.HorizontalAlignment = HorizontalAlignment.Stretch;
        //                lineX.VerticalAlignment = VerticalAlignment.Bottom;
        //                lineX.Margin = new Thickness(0, 0, 0, -1);

        //                Rectangle lineY = new Rectangle();
        //                lineY.Stroke = new SolidColorBrush(Colors.DarkGray);
        //                lineY.Width = 2;
        //                lineY.HorizontalAlignment = HorizontalAlignment.Right;
        //                lineY.VerticalAlignment = VerticalAlignment.Stretch;
        //                lineY.SetValue(Grid.RowSpanProperty, 2);
        //                lineY.Margin = new Thickness(0, 0, -1, 0);

        //                grdWorkGuide.Children.Add(lineX);
        //                grdWorkGuide.Children.Add(lineY);

        //            }
        //            else
        //            {
        //                grdWorkGuide.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
        //                grdWorkGuide.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

        //                grdWorkGuide.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
        //                grdWorkGuide.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

        //                Image imgWorkGuide1 = new Image();
        //                imgWorkGuide1.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[0]);
        //                imgWorkGuide1.SetValue(Grid.RowProperty, 0);
        //                imgWorkGuide1.Margin = new Thickness(2);
        //                grdWorkGuide.Children.Add(imgWorkGuide1);

        //                Image imgWorkGuide2 = new Image();
        //                imgWorkGuide2.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[1]);
        //                imgWorkGuide2.SetValue(Grid.RowProperty, 1);
        //                imgWorkGuide2.Margin = new Thickness(2);
        //                grdWorkGuide.Children.Add(imgWorkGuide2);

        //                Image imgWorkGuide3 = new Image();
        //                imgWorkGuide3.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[2]);
        //                imgWorkGuide3.SetValue(Grid.ColumnProperty, 1);
        //                imgWorkGuide3.SetValue(Grid.RowProperty, 0);
        //                imgWorkGuide3.Margin = new Thickness(2);
        //                grdWorkGuide.Children.Add(imgWorkGuide3);

        //                Image imgWorkGuide4 = new Image();
        //                imgWorkGuide4.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[3]);
        //                imgWorkGuide4.SetValue(Grid.ColumnProperty, 1);
        //                imgWorkGuide4.SetValue(Grid.RowProperty, 1);
        //                imgWorkGuide4.Margin = new Thickness(2);
        //                grdWorkGuide.Children.Add(imgWorkGuide4);

        //                Rectangle lineX = new Rectangle();
        //                lineX.Stroke = new SolidColorBrush(Colors.DarkGray);
        //                lineX.Height = 2;
        //                lineX.HorizontalAlignment = HorizontalAlignment.Stretch;
        //                lineX.VerticalAlignment = VerticalAlignment.Bottom;
        //                lineX.SetValue(Grid.ColumnSpanProperty, 2);
        //                lineX.Margin = new Thickness(0, 0, 0, -1);

        //                Rectangle lineY = new Rectangle();
        //                lineY.Stroke = new SolidColorBrush(Colors.DarkGray);
        //                lineY.Width = 2;
        //                lineY.HorizontalAlignment = HorizontalAlignment.Right;
        //                lineY.VerticalAlignment = VerticalAlignment.Stretch;
        //                lineY.SetValue(Grid.RowSpanProperty, 2);
        //                lineY.Margin = new Thickness(0, 0, -1, 0);

        //                grdWorkGuide.Children.Add(lineX);
        //                grdWorkGuide.Children.Add(lineY);
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        Image imgWorkGuide = new Image();
        //        imgWorkGuide.Source = new BitmapImage(new Uri("/LGE.SFC.ControlsLib;component/Images/Icons/NoImage_msg.png", UriKind.RelativeOrAbsolute));

        //        grdWorkGuide.Children.Add(imgWorkGuide);
        //    }

        //    grdWorkGuide.HorizontalAlignment = HorizontalAlignment.Stretch;
        //    grdWorkGuide.VerticalAlignment = VerticalAlignment.Stretch;

        //    return grdWorkGuide;
        //}

        /// <summary>
        /// Get WorkGuide Images By Prod ID
        /// </summary>
        /// <param name="procId">Process ID</param>
        /// <param name="prodId">Prod ID</param>
        /// <returns>WorkGuide Image</returns>
        //public static Grid GetImageByProdID(string procId, string prodId)
        //{
        //    Grid grdWorkGuide = new Grid();

        //    //Image imgWorkGuide = new Image();
        //    try
        //    {
        //        List<string> ImagePaths;

        //        //WorkGuide Process 
        //        ImagePaths = WorkGuide.GetImagePaths(procId, prodId);

        //        if (ImagePaths.Count == 0)
        //        {
        //            Image imgWorkGuide = new Image();
        //            imgWorkGuide.Source = new BitmapImage(new Uri("/LGE.SFC.ControlsLib;component/Images/Icons/NoImage_msg.png", UriKind.RelativeOrAbsolute));

        //            grdWorkGuide.Children.Add(imgWorkGuide);
        //        }
        //        else
        //        {
        //            if (ImagePaths.Count == 1)
        //            {
        //                Image imgWorkGuide1 = new Image();
        //                imgWorkGuide1.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[0]);
        //                imgWorkGuide1.SetValue(Grid.RowProperty, 0);
        //                imgWorkGuide1.Margin = new Thickness(2);

        //                grdWorkGuide.Children.Add(imgWorkGuide1);
        //            }
        //            else if (ImagePaths.Count == 2)
        //            {
        //                grdWorkGuide.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
        //                grdWorkGuide.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

        //                Image imgWorkGuide1 = new Image();
        //                imgWorkGuide1.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[0]);
        //                imgWorkGuide1.SetValue(Grid.RowProperty, 0);
        //                imgWorkGuide1.Margin = new Thickness(2);
        //                imgWorkGuide1.HorizontalAlignment = HorizontalAlignment.Stretch;
        //                imgWorkGuide1.VerticalAlignment = VerticalAlignment.Stretch;
        //                grdWorkGuide.Children.Add(imgWorkGuide1);

        //                Image imgWorkGuide2 = new Image();
        //                imgWorkGuide2.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[1]);
        //                imgWorkGuide2.SetValue(Grid.RowProperty, 1);
        //                imgWorkGuide2.Margin = new Thickness(2);
        //                grdWorkGuide.Children.Add(imgWorkGuide2);

        //                Rectangle line = new Rectangle();
        //                line.Stroke = new SolidColorBrush(Colors.DarkGray);
        //                line.Height = 2;
        //                line.HorizontalAlignment = HorizontalAlignment.Stretch;
        //                line.VerticalAlignment = VerticalAlignment.Bottom;
        //                line.Margin = new Thickness(0, 0, 0, -1);

        //                grdWorkGuide.Children.Add(line);
        //            }
        //            else if (ImagePaths.Count == 3)
        //            {
        //                grdWorkGuide.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
        //                grdWorkGuide.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

        //                grdWorkGuide.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
        //                grdWorkGuide.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

        //                Image imgWorkGuide1 = new Image();
        //                imgWorkGuide1.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[0]);
        //                imgWorkGuide1.SetValue(Grid.RowProperty, 0);
        //                imgWorkGuide1.Margin = new Thickness(2);
        //                grdWorkGuide.Children.Add(imgWorkGuide1);

        //                Image imgWorkGuide2 = new Image();
        //                imgWorkGuide2.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[1]);
        //                imgWorkGuide2.SetValue(Grid.RowProperty, 1);
        //                imgWorkGuide2.Margin = new Thickness(2);
        //                grdWorkGuide.Children.Add(imgWorkGuide2);

        //                Image imgWorkGuide3 = new Image();
        //                imgWorkGuide3.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[2]);
        //                imgWorkGuide3.SetValue(Grid.ColumnProperty, 1);
        //                imgWorkGuide3.SetValue(Grid.RowSpanProperty, 2);
        //                imgWorkGuide3.Margin = new Thickness(2);
        //                grdWorkGuide.Children.Add(imgWorkGuide3);

        //                Rectangle lineX = new Rectangle();
        //                lineX.Stroke = new SolidColorBrush(Colors.DarkGray);
        //                lineX.Height = 2;
        //                lineX.HorizontalAlignment = HorizontalAlignment.Stretch;
        //                lineX.VerticalAlignment = VerticalAlignment.Bottom;
        //                lineX.Margin = new Thickness(0, 0, 0, -1);

        //                Rectangle lineY = new Rectangle();
        //                lineY.Stroke = new SolidColorBrush(Colors.DarkGray);
        //                lineY.Width = 2;
        //                lineY.HorizontalAlignment = HorizontalAlignment.Right;
        //                lineY.VerticalAlignment = VerticalAlignment.Stretch;
        //                lineY.SetValue(Grid.RowSpanProperty, 2);
        //                lineY.Margin = new Thickness(0, 0, -1, 0);

        //                grdWorkGuide.Children.Add(lineX);
        //                grdWorkGuide.Children.Add(lineY);

        //            }
        //            else
        //            {
        //                grdWorkGuide.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
        //                grdWorkGuide.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

        //                grdWorkGuide.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
        //                grdWorkGuide.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

        //                Image imgWorkGuide1 = new Image();
        //                imgWorkGuide1.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[0]);
        //                imgWorkGuide1.SetValue(Grid.RowProperty, 0);
        //                imgWorkGuide1.Margin = new Thickness(2);
        //                grdWorkGuide.Children.Add(imgWorkGuide1);

        //                Image imgWorkGuide2 = new Image();
        //                imgWorkGuide2.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[1]);
        //                imgWorkGuide2.SetValue(Grid.RowProperty, 1);
        //                imgWorkGuide2.Margin = new Thickness(2);
        //                grdWorkGuide.Children.Add(imgWorkGuide2);

        //                Image imgWorkGuide3 = new Image();
        //                imgWorkGuide3.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[2]);
        //                imgWorkGuide3.SetValue(Grid.ColumnProperty, 1);
        //                imgWorkGuide3.SetValue(Grid.RowProperty, 0);
        //                imgWorkGuide3.Margin = new Thickness(2);
        //                grdWorkGuide.Children.Add(imgWorkGuide3);

        //                Image imgWorkGuide4 = new Image();
        //                imgWorkGuide4.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[3]);
        //                imgWorkGuide4.SetValue(Grid.ColumnProperty, 1);
        //                imgWorkGuide4.SetValue(Grid.RowProperty, 1);
        //                imgWorkGuide4.Margin = new Thickness(2);
        //                grdWorkGuide.Children.Add(imgWorkGuide4);

        //                Rectangle lineX = new Rectangle();
        //                lineX.Stroke = new SolidColorBrush(Colors.DarkGray);
        //                lineX.Height = 2;
        //                lineX.HorizontalAlignment = HorizontalAlignment.Stretch;
        //                lineX.VerticalAlignment = VerticalAlignment.Bottom;
        //                lineX.SetValue(Grid.ColumnSpanProperty, 2);
        //                lineX.Margin = new Thickness(0, 0, 0, -1);

        //                Rectangle lineY = new Rectangle();
        //                lineY.Stroke = new SolidColorBrush(Colors.DarkGray);
        //                lineY.Width = 2;
        //                lineY.HorizontalAlignment = HorizontalAlignment.Right;
        //                lineY.VerticalAlignment = VerticalAlignment.Stretch;
        //                lineY.SetValue(Grid.RowSpanProperty, 2);
        //                lineY.Margin = new Thickness(0, 0, -1, 0);

        //                grdWorkGuide.Children.Add(lineX);
        //                grdWorkGuide.Children.Add(lineY);
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        Image imgWorkGuide = new Image();
        //        imgWorkGuide.Source = new BitmapImage(new Uri("/LGE.SFC.ControlsLib;component/Images/Icons/NoImage_msg.png", UriKind.RelativeOrAbsolute));

        //        grdWorkGuide.Children.Add(imgWorkGuide);
        //    }

        //    grdWorkGuide.HorizontalAlignment = HorizontalAlignment.Stretch;
        //    grdWorkGuide.VerticalAlignment = VerticalAlignment.Stretch;

        //    return grdWorkGuide;
        //}

        /// <summary>
        /// Get WorkGuide Images By Prod ID
        /// </summary>
        /// <param name="prodId">Prod ID</param>
        /// <returns>WorkGuide Image</returns>
        public static Grid GetImageByProdID_New(string prodId)
        {
            Grid grdWorkGuide = new Grid();

            //try
            //{
            //    List<WorkGuideInfo> ImageInfos;

            //    //WorkGuide Process 
            //    if (Variables.appConfigInfo.WorkGuideProcess != string.Empty)
            //    {
            //        ImageInfos = WorkGuide.GetImageInfos(Variables.appConfigInfo.WorkGuideProcess, prodId);
            //    }
            //    else
            //    {
            //        ImageInfos = WorkGuide.GetImageInfos(Variables.CurrentProcID, prodId);
            //    }

            //    List<string> ImagePaths = new List<string>();


            //    if (ImageInfos.Count > 1)
            //    {
            //        //Getting Image Sorting Info.
            //        DataTable dtSortInfo = ImageSortInfo(prodId);

            //        foreach (DataRow drsortdata in dtSortInfo.Rows)
            //        {
            //            List<WorkGuideInfo> tmpImageInfos = ImageInfos.Where(item => item.FileKey == drsortdata["FILEKEY"].ToString()).ToList();

            //            if (tmpImageInfos.Count > 0)
            //            {
            //                ImagePaths.Add(Environment.CurrentDirectory + "\\WorkGuide\\" + tmpImageInfos[0].ProcID + "\\" + tmpImageInfos[0].FilePath + "\\" + tmpImageInfos[0].FileName);
            //            }
            //        }
            //    }
            //    else if (ImageInfos.Count == 1)
            //    {
            //        ImagePaths.Add(Environment.CurrentDirectory + "\\WorkGuide\\" + ImageInfos[0].ProcID + "\\" + ImageInfos[0].FilePath + "\\" + ImageInfos[0].FileName);
            //    }

            //    if (ImagePaths.Count == 0)
            //    {
            //        Image imgWorkGuide = new Image();
            //        imgWorkGuide.Source = new BitmapImage(new Uri("/LGE.SFC.ControlsLib;component/Images/Icons/NoImage_msg.png", UriKind.RelativeOrAbsolute));

            //        grdWorkGuide.Children.Add(imgWorkGuide);
            //    }
            //    else
            //    {
            //        if (ImagePaths.Count == 1)
            //        {
            //            Image imgWorkGuide1 = new Image();
            //            imgWorkGuide1.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[0]);
            //            imgWorkGuide1.SetValue(Grid.RowProperty, 0);
            //            imgWorkGuide1.Margin = new Thickness(2);

            //            grdWorkGuide.Children.Add(imgWorkGuide1);
            //        }
            //        else if (ImagePaths.Count == 2)
            //        {
            //            grdWorkGuide.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            //            grdWorkGuide.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

            //            Image imgWorkGuide1 = new Image();
            //            imgWorkGuide1.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[0]);
            //            imgWorkGuide1.SetValue(Grid.RowProperty, 0);
            //            imgWorkGuide1.Margin = new Thickness(2);
            //            imgWorkGuide1.HorizontalAlignment = HorizontalAlignment.Stretch;
            //            imgWorkGuide1.VerticalAlignment = VerticalAlignment.Stretch;
            //            grdWorkGuide.Children.Add(imgWorkGuide1);

            //            Image imgWorkGuide2 = new Image();
            //            imgWorkGuide2.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[1]);
            //            imgWorkGuide2.SetValue(Grid.RowProperty, 1);
            //            imgWorkGuide2.Margin = new Thickness(2);
            //            grdWorkGuide.Children.Add(imgWorkGuide2);

            //            Rectangle line = new Rectangle();
            //            line.Stroke = new SolidColorBrush(Colors.DarkGray);
            //            line.Height = 2;
            //            line.HorizontalAlignment = HorizontalAlignment.Stretch;
            //            line.VerticalAlignment = VerticalAlignment.Bottom;
            //            line.Margin = new Thickness(0, 0, 0, -1);

            //            grdWorkGuide.Children.Add(line);
            //        }
            //        else if (ImagePaths.Count == 3)
            //        {
            //            grdWorkGuide.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            //            grdWorkGuide.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

            //            grdWorkGuide.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            //            grdWorkGuide.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            //            Image imgWorkGuide1 = new Image();
            //            imgWorkGuide1.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[0]);
            //            imgWorkGuide1.SetValue(Grid.RowProperty, 0);
            //            imgWorkGuide1.Margin = new Thickness(2);
            //            grdWorkGuide.Children.Add(imgWorkGuide1);

            //            Image imgWorkGuide2 = new Image();
            //            imgWorkGuide2.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[1]);
            //            imgWorkGuide2.SetValue(Grid.RowProperty, 1);
            //            imgWorkGuide2.Margin = new Thickness(2);
            //            grdWorkGuide.Children.Add(imgWorkGuide2);

            //            Image imgWorkGuide3 = new Image();
            //            imgWorkGuide3.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[2]);
            //            imgWorkGuide3.SetValue(Grid.ColumnProperty, 1);
            //            imgWorkGuide3.SetValue(Grid.RowSpanProperty, 2);
            //            imgWorkGuide3.Margin = new Thickness(2);
            //            grdWorkGuide.Children.Add(imgWorkGuide3);

            //            Rectangle lineX = new Rectangle();
            //            lineX.Stroke = new SolidColorBrush(Colors.DarkGray);
            //            lineX.Height = 2;
            //            lineX.HorizontalAlignment = HorizontalAlignment.Stretch;
            //            lineX.VerticalAlignment = VerticalAlignment.Bottom;
            //            lineX.Margin = new Thickness(0, 0, 0, -1);

            //            Rectangle lineY = new Rectangle();
            //            lineY.Stroke = new SolidColorBrush(Colors.DarkGray);
            //            lineY.Width = 2;
            //            lineY.HorizontalAlignment = HorizontalAlignment.Right;
            //            lineY.VerticalAlignment = VerticalAlignment.Stretch;
            //            lineY.SetValue(Grid.RowSpanProperty, 2);
            //            lineY.Margin = new Thickness(0, 0, -1, 0);

            //            grdWorkGuide.Children.Add(lineX);
            //            grdWorkGuide.Children.Add(lineY);

            //        }
            //        else
            //        {
            //            grdWorkGuide.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            //            grdWorkGuide.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

            //            grdWorkGuide.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            //            grdWorkGuide.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            //            Image imgWorkGuide1 = new Image();
            //            imgWorkGuide1.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[0]);
            //            imgWorkGuide1.SetValue(Grid.RowProperty, 0);
            //            imgWorkGuide1.Margin = new Thickness(2);
            //            grdWorkGuide.Children.Add(imgWorkGuide1);

            //            Image imgWorkGuide2 = new Image();
            //            imgWorkGuide2.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[1]);
            //            imgWorkGuide2.SetValue(Grid.RowProperty, 1);
            //            imgWorkGuide2.Margin = new Thickness(2);
            //            grdWorkGuide.Children.Add(imgWorkGuide2);

            //            Image imgWorkGuide3 = new Image();
            //            imgWorkGuide3.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[2]);
            //            imgWorkGuide3.SetValue(Grid.ColumnProperty, 1);
            //            imgWorkGuide3.SetValue(Grid.RowProperty, 0);
            //            imgWorkGuide3.Margin = new Thickness(2);
            //            grdWorkGuide.Children.Add(imgWorkGuide3);

            //            Image imgWorkGuide4 = new Image();
            //            imgWorkGuide4.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[3]);
            //            imgWorkGuide4.SetValue(Grid.ColumnProperty, 1);
            //            imgWorkGuide4.SetValue(Grid.RowProperty, 1);
            //            imgWorkGuide4.Margin = new Thickness(2);
            //            grdWorkGuide.Children.Add(imgWorkGuide4);

            //            Rectangle lineX = new Rectangle();
            //            lineX.Stroke = new SolidColorBrush(Colors.DarkGray);
            //            lineX.Height = 2;
            //            lineX.HorizontalAlignment = HorizontalAlignment.Stretch;
            //            lineX.VerticalAlignment = VerticalAlignment.Bottom;
            //            lineX.SetValue(Grid.ColumnSpanProperty, 2);
            //            lineX.Margin = new Thickness(0, 0, 0, -1);

            //            Rectangle lineY = new Rectangle();
            //            lineY.Stroke = new SolidColorBrush(Colors.DarkGray);
            //            lineY.Width = 2;
            //            lineY.HorizontalAlignment = HorizontalAlignment.Right;
            //            lineY.VerticalAlignment = VerticalAlignment.Stretch;
            //            lineY.SetValue(Grid.RowSpanProperty, 2);
            //            lineY.Margin = new Thickness(0, 0, -1, 0);

            //            grdWorkGuide.Children.Add(lineX);
            //            grdWorkGuide.Children.Add(lineY);
            //        }
            //    }
            //}
            //catch
            //{
            //    Image imgWorkGuide = new Image();
            //    imgWorkGuide.Source = new BitmapImage(new Uri("/LGE.SFC.ControlsLib;component/Images/Icons/NoImage_msg.png", UriKind.RelativeOrAbsolute));

            //    grdWorkGuide.Children.Add(imgWorkGuide);
            //}

            //grdWorkGuide.HorizontalAlignment = HorizontalAlignment.Stretch;
            //grdWorkGuide.VerticalAlignment = VerticalAlignment.Stretch;

            return grdWorkGuide;
        }

        /// <summary>
        /// Get WorkGuide Images By Prod ID
        /// </summary>
        /// <param name="procId">Process ID</param>
        /// <param name="prodId">Prod ID</param>
        /// <returns>WorkGuide Image</returns>
        //public static Grid GetImageByProdID_New(string procId, string prodId)
        //{
        //    Grid grdWorkGuide = new Grid();

        //    try
        //    {
        //        List<WorkGuideInfo> ImageInfos;

        //        ImageInfos = WorkGuide.GetImageInfos(procId, prodId);

        //        List<string> ImagePaths = new List<string>();


        //        if (ImageInfos.Count > 1)
        //        {
        //            //Getting Image Sorting Info.
        //            DataTable dtSortInfo = ImageSortInfo(prodId);

        //            foreach (DataRow drsortdata in dtSortInfo.Rows)
        //            {
        //                List<WorkGuideInfo> tmpImageInfos = ImageInfos.Where(item => item.FileKey == drsortdata["FILEKEY"].ToString()).ToList();

        //                if (tmpImageInfos.Count > 0)
        //                {
        //                    ImagePaths.Add(Environment.CurrentDirectory + "\\WorkGuide\\" + tmpImageInfos[0].ProcID + "\\" + tmpImageInfos[0].FilePath + "\\" + tmpImageInfos[0].FileName);
        //                }
        //            }
        //        }
        //        else if (ImageInfos.Count == 1)
        //        {
        //            ImagePaths.Add(Environment.CurrentDirectory + "\\WorkGuide\\" + ImageInfos[0].ProcID + "\\" + ImageInfos[0].FilePath + "\\" + ImageInfos[0].FileName);
        //        }

        //        if (ImagePaths.Count == 0)
        //        {
        //            Image imgWorkGuide = new Image();
        //            imgWorkGuide.Source = new BitmapImage(new Uri("/LGE.SFC.ControlsLib;component/Images/Icons/NoImage_msg.png", UriKind.RelativeOrAbsolute));

        //            grdWorkGuide.Children.Add(imgWorkGuide);
        //        }
        //        else
        //        {
        //            if (ImagePaths.Count == 1)
        //            {
        //                Image imgWorkGuide1 = new Image();
        //                imgWorkGuide1.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[0]);
        //                imgWorkGuide1.SetValue(Grid.RowProperty, 0);
        //                imgWorkGuide1.Margin = new Thickness(2);

        //                grdWorkGuide.Children.Add(imgWorkGuide1);
        //            }
        //            else if (ImagePaths.Count == 2)
        //            {
        //                grdWorkGuide.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
        //                grdWorkGuide.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

        //                Image imgWorkGuide1 = new Image();
        //                imgWorkGuide1.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[0]);
        //                imgWorkGuide1.SetValue(Grid.RowProperty, 0);
        //                imgWorkGuide1.Margin = new Thickness(2);
        //                imgWorkGuide1.HorizontalAlignment = HorizontalAlignment.Stretch;
        //                imgWorkGuide1.VerticalAlignment = VerticalAlignment.Stretch;
        //                grdWorkGuide.Children.Add(imgWorkGuide1);

        //                Image imgWorkGuide2 = new Image();
        //                imgWorkGuide2.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[1]);
        //                imgWorkGuide2.SetValue(Grid.RowProperty, 1);
        //                imgWorkGuide2.Margin = new Thickness(2);
        //                grdWorkGuide.Children.Add(imgWorkGuide2);

        //                Rectangle line = new Rectangle();
        //                line.Stroke = new SolidColorBrush(Colors.DarkGray);
        //                line.Height = 2;
        //                line.HorizontalAlignment = HorizontalAlignment.Stretch;
        //                line.VerticalAlignment = VerticalAlignment.Bottom;
        //                line.Margin = new Thickness(0, 0, 0, -1);

        //                grdWorkGuide.Children.Add(line);
        //            }
        //            else if (ImagePaths.Count == 3)
        //            {
        //                grdWorkGuide.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
        //                grdWorkGuide.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

        //                grdWorkGuide.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
        //                grdWorkGuide.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

        //                Image imgWorkGuide1 = new Image();
        //                imgWorkGuide1.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[0]);
        //                imgWorkGuide1.SetValue(Grid.RowProperty, 0);
        //                imgWorkGuide1.Margin = new Thickness(2);
        //                grdWorkGuide.Children.Add(imgWorkGuide1);

        //                Image imgWorkGuide2 = new Image();
        //                imgWorkGuide2.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[1]);
        //                imgWorkGuide2.SetValue(Grid.RowProperty, 1);
        //                imgWorkGuide2.Margin = new Thickness(2);
        //                grdWorkGuide.Children.Add(imgWorkGuide2);

        //                Image imgWorkGuide3 = new Image();
        //                imgWorkGuide3.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[2]);
        //                imgWorkGuide3.SetValue(Grid.ColumnProperty, 1);
        //                imgWorkGuide3.SetValue(Grid.RowSpanProperty, 2);
        //                imgWorkGuide3.Margin = new Thickness(2);
        //                grdWorkGuide.Children.Add(imgWorkGuide3);

        //                Rectangle lineX = new Rectangle();
        //                lineX.Stroke = new SolidColorBrush(Colors.DarkGray);
        //                lineX.Height = 2;
        //                lineX.HorizontalAlignment = HorizontalAlignment.Stretch;
        //                lineX.VerticalAlignment = VerticalAlignment.Bottom;
        //                lineX.Margin = new Thickness(0, 0, 0, -1);

        //                Rectangle lineY = new Rectangle();
        //                lineY.Stroke = new SolidColorBrush(Colors.DarkGray);
        //                lineY.Width = 2;
        //                lineY.HorizontalAlignment = HorizontalAlignment.Right;
        //                lineY.VerticalAlignment = VerticalAlignment.Stretch;
        //                lineY.SetValue(Grid.RowSpanProperty, 2);
        //                lineY.Margin = new Thickness(0, 0, -1, 0);

        //                grdWorkGuide.Children.Add(lineX);
        //                grdWorkGuide.Children.Add(lineY);

        //            }
        //            else
        //            {
        //                grdWorkGuide.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
        //                grdWorkGuide.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

        //                grdWorkGuide.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
        //                grdWorkGuide.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

        //                Image imgWorkGuide1 = new Image();
        //                imgWorkGuide1.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[0]);
        //                imgWorkGuide1.SetValue(Grid.RowProperty, 0);
        //                imgWorkGuide1.Margin = new Thickness(2);
        //                grdWorkGuide.Children.Add(imgWorkGuide1);

        //                Image imgWorkGuide2 = new Image();
        //                imgWorkGuide2.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[1]);
        //                imgWorkGuide2.SetValue(Grid.RowProperty, 1);
        //                imgWorkGuide2.Margin = new Thickness(2);
        //                grdWorkGuide.Children.Add(imgWorkGuide2);

        //                Image imgWorkGuide3 = new Image();
        //                imgWorkGuide3.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[2]);
        //                imgWorkGuide3.SetValue(Grid.ColumnProperty, 1);
        //                imgWorkGuide3.SetValue(Grid.RowProperty, 0);
        //                imgWorkGuide3.Margin = new Thickness(2);
        //                grdWorkGuide.Children.Add(imgWorkGuide3);

        //                Image imgWorkGuide4 = new Image();
        //                imgWorkGuide4.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ImagePaths[3]);
        //                imgWorkGuide4.SetValue(Grid.ColumnProperty, 1);
        //                imgWorkGuide4.SetValue(Grid.RowProperty, 1);
        //                imgWorkGuide4.Margin = new Thickness(2);
        //                grdWorkGuide.Children.Add(imgWorkGuide4);

        //                Rectangle lineX = new Rectangle();
        //                lineX.Stroke = new SolidColorBrush(Colors.DarkGray);
        //                lineX.Height = 2;
        //                lineX.HorizontalAlignment = HorizontalAlignment.Stretch;
        //                lineX.VerticalAlignment = VerticalAlignment.Bottom;
        //                lineX.SetValue(Grid.ColumnSpanProperty, 2);
        //                lineX.Margin = new Thickness(0, 0, 0, -1);

        //                Rectangle lineY = new Rectangle();
        //                lineY.Stroke = new SolidColorBrush(Colors.DarkGray);
        //                lineY.Width = 2;
        //                lineY.HorizontalAlignment = HorizontalAlignment.Right;
        //                lineY.VerticalAlignment = VerticalAlignment.Stretch;
        //                lineY.SetValue(Grid.RowSpanProperty, 2);
        //                lineY.Margin = new Thickness(0, 0, -1, 0);

        //                grdWorkGuide.Children.Add(lineX);
        //                grdWorkGuide.Children.Add(lineY);
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        Image imgWorkGuide = new Image();
        //        imgWorkGuide.Source = new BitmapImage(new Uri("/LGE.SFC.ControlsLib;component/Images/Icons/NoImage_msg.png", UriKind.RelativeOrAbsolute));

        //        grdWorkGuide.Children.Add(imgWorkGuide);
        //    }

        //    grdWorkGuide.HorizontalAlignment = HorizontalAlignment.Stretch;
        //    grdWorkGuide.VerticalAlignment = VerticalAlignment.Stretch;

        //    return grdWorkGuide;
        //}

        private static DataTable ImageSortInfo(string prodId)
        {
            try
            {
                //DataSet dsIndata = new DataSet();
                //Common.MakeDataTable(ref dsIndata, "INDATA", "ORGID", Variables.appConfigInfo.Organizations);
                //Common.MakeDataTable(ref dsIndata, "INDATA", "EQSGID", Variables.appConfigInfo.Line);
                //Common.MakeDataTable(ref dsIndata, "INDATA", "PROCID", Variables.CurrentProcID);
                //Common.MakeDataTable(ref dsIndata, "INDATA", "PRODID", prodId);


                //DataTable dtresult = Variables.BizActorService.ExecBizRule("CUS_SEL_EQSG_PROC_MTRL_IMG_BY_PRODID", dsIndata, "INDATA", "OUTDATA").Tables["OUTDATA"];

                //dtresult.DefaultView.Sort = "DPLY_ORD_NO";
                //dtresult = dtresult.DefaultView.ToTable();

                //return dtresult;

                DataTable dtresult = new DataTable();
                return dtresult;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static void SafeSleep(int time)
        {
            System.DateTime now = System.DateTime.Now;
            System.TimeSpan duration = new System.TimeSpan(0, 0, 0, 0, time);
            System.DateTime then = now.Add(duration);

            while (then > DateTime.Now)
            {
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(10);
            }
        }


        private static byte[] SetSerialNo(string sSerialNo)
        {
            byte[] bTemp = new byte[sSerialNo.Length];
            char[] cTemp = sSerialNo.ToCharArray();
            for (int idx = 0; idx < sSerialNo.Length; idx++)
            {
                bTemp[idx] = Convert.ToByte(cTemp[idx]);
            }
            return bTemp;
        }

        private static byte[] Combine(byte[] src1, byte[] src2)
        {
            byte[] result = new byte[src1.Length + src2.Length];
            System.Buffer.BlockCopy(src1, 0, result, 0, src1.Length);
            System.Buffer.BlockCopy(src2, 0, result, src1.Length, src2.Length);
            return result;
        }
    }
}
