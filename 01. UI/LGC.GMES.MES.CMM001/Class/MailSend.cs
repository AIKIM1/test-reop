using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Data;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.CMM001.Class
{
    public class MailSend
    {
        private string SMTPaddress = "exedge1.lgensol.com";
        //private string SMTPaddress = "150.150.187.36";
        //private string senderID = "delta@lgchem.com";
        //private string senderNAME = "박상철";
        //private string toID = "delta@lgchem.com";
        //private string toSub = "제목";
        //private string toBody = "메일 테스트";

        public MailSend() { }

        public void SendMail(string sSenderID, string sSenderName, string sTo, string sCC, string sSub, string sBody) {

            //P_RECIPIENTS String      메시지를 받을 전자 메일 주소 목록(각 주소는 세미콜론으로 구분)
            //P_COPY_RECIPIENTS String      참조로 메시지를 받을 전자 메일 주소 목록(각 주소는 세미콜론으로 구분)
            //P_BLIND_COPY_RECIPIENTS String      숨은참조로 메시지를 받을 전자 메일 주소 목록(각 주소는 세미콜론으로 구분)
            //P_FROM_ADDRESS String      전자 메일 메시지의 '보낸 사람 주소' 값
            //P_REPLY_TO  String 전자 메일 메시지의 '회신 주소' 값(전자 메일 주소만 유효한 값으로 허용)
            //P_SUBJECT String      전자 메일 메시지의 제목
            //P_BODY String      전자 메일 메시지의 본문
            //P_BODY_FORMAT String      메시지 본문의 형식(TEXT, HTML) 기본값은 TEXT입니다.
            //P_IMPORTANCE String  Normal 메시지의 중요도(Low, Normal, High) 기본값은 Normal입니다.
            //P_SENSITIVITY String  Normal 메시지의 기밀성(Normal, Personal, Private, Confidential
            //P_FILE_ATTACHMENTS  String      전자 메일 메시지에 첨부되는 파일 이름 목록(각 파일 이름은 세미콜론으로 구분)--기본적으로 데이터베이스 메일의 첨부 파일은 파일당 1MB로 제한
            //P_QUERY String      실행할 쿼리(쿼리 결과를 파일로 첨부할 수도 있고 전자 메일 메시지의 본문에 포함할 수도 있습니다.)
            //P_EXECUTE_QUERY_DATABASE    String      기본값은 현재 데이터베이스입니다.이 매개 변수는 @query가 지정된 경우에만 적용됩니다.
            //P_ATTACH_QUERY_RESULT_AS_FILE   Byte    0   쿼리의 결과 집합이 첨부 파일로 반환되는지 여부를 지정 기본값은 0
            //P_QUERY_ATTACHMENT_FILENAME String      쿼리 결과 집합 첨부 파일에 사용할 파일 이름을 지정 기본값은 NULL attach_query_result가 0이면 이 매개 변수는 무시됩니다.attach_query_result가 1이고 이 매개 변수가 NULL이면 데이터베이스 메일에서 임의로 파일 이름을 만듭니다.
            //P_QUERY_RESULT_HEADER   Byte    1   쿼리 결과에 열 머리글을 포함할 것인지 여부를 지정(값이 1이면 쿼리 결과에 열 머리글이 포함됩니다.값이 0이면 쿼리 결과에 열 머리글이 포함되지 않습니다.이 매개 변수의 기본값은 1입니다.이 매개 변수는 @query가 지정된 경우에만 적용됩니다.)
            //P_QUERY_RESULT_WIDTH    Int32       쿼리 결과에 서식을 지정할 때 사용하는 문자 줄 너비
            //P_QUERY_RESULT_SEPARATOR    String  ''  쿼리 출력에서 열을 구분하는 데 사용되는 문자 기본값은 ' '(공백)
            //P_EXCLUDE_QUERY_OUTPUT  Byte    0   쿼리 실행 출력을 전자 메일 메시지로 반환할지 여부를 지정 기본값은 0
            //P_APPEND_QUERY_ERROR    Byte    0   @query 인수에 지정된 쿼리에서 오류가 반환될 때 전자 메일을 보낼지 여부를 지정
            //P_QUERY_NO_TRUNCATE Byte    0   가변 길이 데이터 형식(String, nString, varbinary(max), xml, text, ntext, image 및 사용자 정의 데이터 형식)의 잘림을 방지하는 옵션을 사용하여 쿼리를 실행할지 여부를 지정
            //OUT_ERROR_NUMBER    Int64
            //OUT_ERROR_MESSAGE   String

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("P_RECIPIENTS", typeof(string));
                dtRqst.Columns.Add("P_COPY_RECIPIENTS", typeof(string));
                dtRqst.Columns.Add("P_FROM_ADDRESS", typeof(string));
                dtRqst.Columns.Add("P_SUBJECT", typeof(string));
                dtRqst.Columns.Add("P_BODY", typeof(string));
                dtRqst.Columns.Add("P_BODY_FORMAT", typeof(string));

                DataRow dr = dtRqst.NewRow();
                if (sTo.Contains(";"))
                {
                    sTo = sTo.Replace(";", "@lgensol.com;");
                }
                else
                {
                    sTo = sTo + "@lgensol.com";
                }
                dr["P_RECIPIENTS"] = sTo;

                if (!string.IsNullOrEmpty(sCC))
                {
                    if (sCC.Contains(";"))
                    {
                        sCC = sCC.Replace(";", "@lgensol.com;");
                    }
                    else
                    {
                        sCC = sCC + "@lgensol.com";
                    }
                }

                dr["P_COPY_RECIPIENTS"] = sCC;
                dr["P_FROM_ADDRESS"] = sSenderID + "@lgensol.com";
                dr["P_SUBJECT"] = sSub;
                dr["P_BODY"] = sBody;
                dr["P_BODY_FORMAT"] = "HTML";



                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_MAIL_SEND", "INDATA", "OUTDATA", dtRqst);
            }
            catch (Exception ex)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.Alert(ex.ToString());
            }

            //MailMessage mail = new MailMessage();
            //SmtpClient SmtpServer = new SmtpClient(SMTPaddress);
            //mail.From = new MailAddress(sSenderID + "@lgchem.com", sSenderName, System.Text.Encoding.UTF8);
            //mail.IsBodyHtml = true;
            //string[] sArayTO = sTo.Split(';');

            //foreach (string sToUser in sArayTO)
            //{
            //    if (sToUser.Length > 0)
            //    {
            //        mail.To.Add(sToUser + "@lgchem.com");
            //    }
            //}

            //string[] sArrayCC = sCC.Split(';');
            //foreach (string sCCUser in sArrayCC)
            //{
            //    if (sCCUser.Length > 0)
            //    {
            //        mail.CC.Add(sCCUser + "@lgchem.com");
            //    }
            //}

            //mail.Subject = sSub;
            //mail.Body = sBody;
            //mail.BodyEncoding = System.Text.Encoding.UTF8;
            //mail.SubjectEncoding = System.Text.Encoding.UTF8;
            //SmtpServer.Port = 25;
            //SmtpServer.Credentials = new System.Net.NetworkCredential();


            //SmtpServer.Send(mail);
        }

        public string makeBodyApp(string sTitle, string sContent, DataTable dtLot = null) {
            StringBuilder sb = new StringBuilder();
            try
            {
                sb.Append("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
                sb.Append("<html xmlns=\"http://www.w3.org/1999/xhtml\">");
                sb.Append("<head>");
                sb.Append("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />");
                sb.Append("<title>Untitled Document</title>");
                sb.Append("<style>");
                sb.Append("	* {margin:0;padding:0;}");
                sb.Append("	body {font-family:Malgun Gothic, Arial, Helvetica, sans-serif;font-size:14px;line-height:1.8;color:#333333;}");
                sb.Append("	table {border-collapse:collapse;width:100%;}");
                sb.Append("	table th {background:#f5f5f5;border-bottom:1px solid #e1e1e1;}");
                sb.Append("	table td {background:#fff;border-right:1px solid #e1e1e1;border-left:1px solid #e1e1e1;border-bottom:1px solid #e1e1e1;}");
                sb.Append("	table tbody th {border-left:1px solid #e1e1e1;text-align:right;padding:6px 8px;		}");
                sb.Append("	table tbody td {text-align:left;padding:6px 8px;}");
                sb.Append("	table thead th {text-align:center;padding:3px;border-right:1px solid #e1e1e1;border-left:1px solid #e1e1e1;	border-bottom:1px solid #d1d1d1;}");
                sb.Append("	.hori-table table tbody td {text-align:center;padding:3px;}");
                sb.Append("	.vertical-table, .hori-table {margin-bottom:20px;}");
                sb.Append("</style>");
                sb.Append("</head>");
                sb.Append("<body>");
                sb.Append("	<div class=\"wrap\">");
                sb.Append("    	<div class=\"vertical-table\">");
                sb.Append("            <table style=\"border-top:2px solid #c8294b; max-width:720px;\">");
                sb.Append("                <tbody>");
                sb.Append("                    <tr>");
                sb.Append("                        <th>" + ObjectDic.Instance.GetObjectName("요청구분") + "</th>");
                sb.Append("                        <td>" + sTitle + "</td>");
                sb.Append("                    </tr>");
                sb.Append("                    <tr>");
                sb.Append("                        <th>" + ObjectDic.Instance.GetObjectName("사유") + "</th>");
                sb.Append("                        <td>" + sContent.Replace(Environment.NewLine, "<br>") + "&nbsp;</td>");
                sb.Append("                    </tr>                 ");
                sb.Append("                </tbody>");
                sb.Append("            </table>");
                sb.Append("        </div>");
                if (dtLot != null && dtLot.Rows.Count > 0)
                {
                    sb.Append("    <div class=\"hori-table\">");
                    sb.Append("        	<table style=\"border-top:2px solid #c8294b; max-width:720px;\" >");
                    sb.Append("            	<colgroup>");
                    sb.Append("                	<col width=\"\" />");
                    sb.Append("                    <col width=\"\" />");
                    sb.Append("                    <col width=\"\" />");
                    sb.Append("                    <col width=\"\" />");
                    sb.Append("                    <col width=\"\" />");
                    sb.Append("                </colgroup>");
                    sb.Append("                <thead>");
                    sb.Append("                	<tr>");
                    sb.Append("                    	<th>Lot ID</th>");
                    sb.Append("                        <th>" + ObjectDic.Instance.GetObjectName("수량") + "</th>");
                    sb.Append("                        <th>" + ObjectDic.Instance.GetObjectName("제품ID") + "</th>");
                    sb.Append("                        <th>" + ObjectDic.Instance.GetObjectName("제품명") + "</th>");
                    sb.Append("                        <th>" + ObjectDic.Instance.GetObjectName("모델ID") + "</th>");
                    sb.Append("                    </tr>");
                    sb.Append("                </thead>");
                    sb.Append("                <tbody>");
                    foreach (DataRow dr in dtLot.Rows)
                    {
                        sb.Append("                	<tr>");
                        sb.Append("                     <td>" + Util.NVC(dr["LOTID"]) + "&nbsp;</td>");
                        sb.Append("                     <td>" + Util.NVC_NUMBER(dr["WIPQTY"]) + "&nbsp;</td>");
                        sb.Append("                     <td>" + Util.NVC(dr["PRODID"]) + "&nbsp;</td>");
                        sb.Append("                     <td>" + Util.NVC(dr["PRODNAME"]) + "&nbsp;</td>");
                        sb.Append("                     <td>" + Util.NVC(dr["MODELID"]) + "&nbsp;</td>");
                        sb.Append("                 </tr>");
                    }
                    sb.Append("                </tbody>");
                    sb.Append("            </table>");
                    sb.Append("        </div>");
                }
                sb.Append("    </div>");
                sb.Append("</body>");
                sb.Append("</html>");

                
            }
            catch (Exception ex)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.Alert(ex.ToString());
            }
            return sb.ToString();
        }
    }
}
