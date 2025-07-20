/*************************************************************************************
 Created Date : 2020.10.22
      Creator : 신광희
   Decription : CNB2동 증설 - ASSY004_001_LOTCOMMENTHIST Copy 후 작성
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.22  신광희 : Initial Created.
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;

namespace LGC.GMES.MES.ASSY005
{
    /// <summary>
    /// ASSY005_001_LOTCOMMENTHIST.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY005_001_LOTCOMMENTHIST : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _LotID = string.Empty;
        private string _WipSeq = string.Empty;

        Util _Util = new Util();
        BizDataSet _Biz = new BizDataSet();
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

        public ASSY005_001_LOTCOMMENTHIST()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 4)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
                _LotID = Util.NVC(tmps[2]);
                _WipSeq = Util.NVC(tmps[3]);
            }
            else
            {
                _LineID = "";
                _EqptID = "";
                _LotID = "";
                _WipSeq = "";
            }
            ApplyPermissions();
            GetLotCommentHistory();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod

        #region [BizCall]

        private void GetLotCommentHistory()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_LOT_NOTE_HISTORY();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = _LotID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_LOT_NOTE_HIST", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (searchResult.Columns.Contains("NOTE") == true)
                        {
                            for (int inx = 0; inx < searchResult.Rows.Count; inx++)
                            {
                                searchResult.Rows[inx]["NOTE"] = GetConvertRemark(searchResult.Rows[inx]["NOTE"].ToString());
                            }
                        }
                        
                        Util.GridSetData(dgHist, searchResult, null, false);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [Validation]

        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btn);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HideLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private string GetConvertRemark(string sRemark)
        {
            System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
            strBuilder.Clear();

            string[] wipNotes = sRemark.Split('|');

            for (int i = 0; i < wipNotes.Length; i++)
            {
                if (!string.IsNullOrEmpty(wipNotes[i]))
                {
                    if (i == 0)
                    {
                        strBuilder.Append(ObjectDic.Instance.GetObjectName("특이사항") + " : " + wipNotes[i]);
                    }
                    else if (i == 1)
                    {
                        strBuilder.Append(ObjectDic.Instance.GetObjectName("공통특이사항") + " : " + wipNotes[i]);
                    }
                    else if (i == 2)
                    {
                        strBuilder.Append(ObjectDic.Instance.GetObjectName("조정횟수") + " : " + wipNotes[i]);
                    }
                    else if (i == 3)
                    {
                        strBuilder.Append(ObjectDic.Instance.GetObjectName("압연횟수") + " : " + wipNotes[i]);
                    }
                    else if (i == 4)
                    {
                        strBuilder.Append(ObjectDic.Instance.GetObjectName("색지정보") + " : " + wipNotes[i]);
                    }
                    else if (i == 5)
                    {
                        strBuilder.Append(ObjectDic.Instance.GetObjectName("합권이력") + " : " + wipNotes[i]);
                    }

                    strBuilder.Append("\n");
                }
            }

            return strBuilder.ToString();
        }
        #endregion

        #endregion

    }
}
