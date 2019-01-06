using BotAPI.Models;
using BotAPI.Service;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Web.Http;

namespace BotAPI.Controllers
{
    [RoutePrefix("api/eNCR")]
    public class eNCRController : ApiController
    {


        [HttpGet, Route("GeteNCRCombo/{empCode}")]


        public DataSet GeteNCRCombo(string empCode)
        {
            DataSet ds = new DataSet();
            string strcon = ConfigurationManager.ConnectionStrings["SQL_DBCon"].ConnectionString;
            string[] result = new string[0];
            using (SqlConnection con = new SqlConnection(strcon))
            {
                using (SqlCommand cmd = new SqlCommand("[usp_eNCR_Combo_ChatBot]", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@EmpCode", SqlDbType.VarChar).Value = empCode;

                    con.Open();
                    cmd.ExecuteNonQuery();

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(ds);


                }
            }
            return ds;
        }



        // POST api/eNCR
        [HttpPost]
        public string SaveeNCR(eNCRDetails encrdetails)
        {
            string retVal = "";
            try
            {
                WebClient client = new WebClient();

                string strFileUrlToDownload = encrdetails.AttFileDataURL;
                byte[] attachmentData = new byte[0];

                if (encrdetails.AttFileName != "")
                {
                    attachmentData = client.DownloadData((new Uri(strFileUrlToDownload)));
                }
                string strcon = ConfigurationManager.ConnectionStrings["SQL_DBCon"].ConnectionString;
                SqlConnection con = new SqlConnection(strcon);
                SqlCommand cmd = new SqlCommand("usp_eNCR_Ins_Chatbot", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@NCRNo", 0);
                cmd.Parameters.AddWithValue("@RaisedBy", encrdetails.EmpCode);
                cmd.Parameters.AddWithValue("@NCRType", encrdetails.NCRType);
                cmd.Parameters.AddWithValue("@eFountAt", encrdetails.FountAt);
                cmd.Parameters.AddWithValue("@EventNCRCategory", encrdetails.EventCategory);
                cmd.Parameters.AddWithValue("@NCRDescription", encrdetails.Description);
                cmd.Parameters.AddWithValue("@HCDesc", encrdetails.HoldingContract);
                cmd.Parameters.AddWithValue("@ContractDesc", encrdetails.SubContract);
                cmd.Parameters.AddWithValue("@CompanyName", encrdetails.Location);
                cmd.Parameters.AddWithValue("@NCRDept", encrdetails.Department);
                cmd.Parameters.AddWithValue("@NCRCategory", encrdetails.Category);
                cmd.Parameters.AddWithValue("@NCRSubCategory", encrdetails.SubCategory);
                cmd.Parameters.AddWithValue("@eNCRFileName", encrdetails.AttFileName);
                cmd.Parameters.AddWithValue("@eNCRFileSource", attachmentData);
                cmd.Parameters.Add("@RetMsg_CBot", SqlDbType.VarChar, 4000);
                cmd.Parameters["@RetMsg_CBot"].Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@RetID_CBot", SqlDbType.Int, 1);
                cmd.Parameters["@RetID_CBot"].Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@Success_CBot", SqlDbType.VarChar, 1);
                cmd.Parameters["@Success_CBot"].Direction = ParameterDirection.Output;
                cmd.Parameters.AddWithValue("@StartTime", encrdetails.StartTime);
                cmd.Parameters.AddWithValue("@EndTime", encrdetails.EndTime);
                con.Open();
                int k = cmd.ExecuteNonQuery();
                // if (k != 0)
                //{
                retVal = cmd.Parameters["@RetID_CBot"].Value.ToString();
                SendMailNCRNew(retVal, encrdetails.NCRType);
                retVal = "eNCR saved successfully";
                // }
                con.Close();
            }
            catch (Exception e1)
            {

                retVal = e1.Message.ToString();
            }
            return retVal;
        }




        private void SendMailNCRNew(string NCRNo, string ncrtype)
        {

            string strMailContent = string.Empty;
            string strMailSendID = string.Empty;
            string strMailSubject = string.Empty;
            string strMailCCID = string.Empty;
            string strHeaderContent = string.Empty;
            string strDBNCRStatus = "";
            string strRoleEmpName = "";
            string strRoleEmpMailID = "";
            DataTable dtMailStage = new DataTable();
            int intHCCode = 0;
            int intDBPMRoleId;
            DataSet obj_Dset = new DataSet();
            DataSet dsRoleEmp = new DataSet();

            string MailTo = "";
            intDBPMRoleId = 4;
            // strMailCCID = "epms@hare.com"
            if (ncrtype == "System NCR")
            {
                strDBNCRStatus = "Pending with HOD";
            }
            else
            {
                strDBNCRStatus = "Pending with PM";
            }
            obj_Dset = NCRMailUnified(NCRNo, strDBNCRStatus);
            intHCCode = Convert.ToInt32(obj_Dset.Tables[0].Rows[0]["HCCode"].ToString());

            if (intHCCode == -1)
            {
                intHCCode = 0;
            }
            if (obj_Dset.Tables[0].Rows[0]["NCRType"].ToString() == "System NCR")
            {
                intDBPMRoleId = 10;
            }

            dsRoleEmp = GetNCRRoleMultipleEmpMailID(intHCCode);
            DataTable table = dsRoleEmp.Tables[0];
            var dtNew = new DataTable();
            if (table.Rows.Count > 0)
            {
                if (obj_Dset.Tables[0].Rows[0]["NCRType"].ToString() == "System NCR")
                {
                    table.DefaultView.RowFilter = "RoleId =  " + intDBPMRoleId + " AND  SubLocationCode=" + obj_Dset.Tables[0].Rows[0]["NCRDeptCode"].ToString() + "";
                }
                else
                {
                    table.DefaultView.RowFilter = "RoleId =  " + intDBPMRoleId + "";

                }
                dtNew = table.DefaultView.ToTable();
            }

            foreach (DataRow dr in dtNew.Rows)
            {
                if (!string.IsNullOrEmpty(dr[0].ToString()))
                {
                    strRoleEmpName += dr[1].ToString();
                    strRoleEmpMailID += dr[0].ToString();

                }
            }

            // Change the MailIds based on the status
            if (obj_Dset.Tables[0].Rows.Count > 0 && obj_Dset.Tables.Count > 0)
            {

                strDBNCRStatus = obj_Dset.Tables[0].Rows[0]["NCRStatus"].ToString();

                strHeaderContent = "eNCR No  " + obj_Dset.Tables[0].Rows[0]["NCRNo"].ToString() + " received for review.";

                if (obj_Dset.Tables[0].Rows[0]["NCRType"].ToString() == "SystemNCR")
                {
                    strMailSendID = strRoleEmpMailID;
                    MailTo = strRoleEmpName;
                    strMailSubject = "eNCR - " + obj_Dset.Tables[0].Rows[0]["NCRNo"].ToString() + " | Pending at stage 2a  | HOD | " + MailTo;
                }
                else
                {
                    strMailSendID = strRoleEmpMailID;
                    MailTo = strRoleEmpName;
                    strMailSubject = "eNCR - " + obj_Dset.Tables[0].Rows[0]["NCRNo"].ToString() + " | Pending at stage 2a  | PM | " + MailTo;
                }
                strMailContent = MailContent(obj_Dset, MailTo, strHeaderContent);

                SendgridNCRMail(strMailSendID, strMailContent, strMailSubject, "", obj_Dset.Tables[0].Rows[0]["NCRNo"].ToString());
            }


        }
        public DataSet NCRMailUnified(string intNCRNo, string strStatus)
        {
            DataSet ds = new DataSet();
            string strcon = ConfigurationManager.ConnectionStrings["SQL_DBCon"].ConnectionString;
            string[] result = new string[0];
            using (SqlConnection con = new SqlConnection(strcon))
            {
                using (SqlCommand cmd = new SqlCommand("[usp_NCR_System_Mail]", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@intNCRNO", SqlDbType.Int).Value = intNCRNo;
                    cmd.Parameters.Add("@strStatus", SqlDbType.VarChar).Value = strStatus;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(ds);


                }
            }
            return ds;
        }
        public DataSet GetNCRRoleMultipleEmpMailID(int HCCode)
        {
            DataSet ds = new DataSet();
            string strcon = ConfigurationManager.ConnectionStrings["SQL_DBCon"].ConnectionString;

            string SQL = "SELECT MstrEmp.EmpMailID , MstrEmp.EmpName AS EmpName, MstrHCNCR.RoleId,MstrHCNCR.SubLocationCode FROM MstrHCNCR, MstrEmp  WHERE  MstrHCNCR.EmpCode = MstrEmp.EmpCode AND (HCCode=" + HCCode + " OR HCCode=0)";

            SqlConnection conn = new SqlConnection(strcon);
            SqlDataAdapter da = new SqlDataAdapter();
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = SQL;
            da.SelectCommand = cmd;
            conn.Open();
            da.Fill(ds);
            conn.Close();
            return ds;
        }

        private async void SendgridNCRMail(string strMailSendID, string strMailContent, string strMailSubject, string strMailCCID, string streNCRNo, string sendgridNCRMail = "")
        {

            var messageInfo = new EmailMessageInfo()
            {
                FromEmailAddress = "EPMS.Helpdesk@hare.com",
                ToEmailAddress = strMailSendID,
                CcEmailAddress = strMailCCID,
                BccEmailAddress = "",
                EmailSubject = strMailSubject,
                EmailBody = strMailContent

            };
            SendGridEmailService objsendgridMail = new SendGridEmailService();
            var apiResponse = await objsendgridMail.Send(messageInfo);
            await SetResponseInfoContainers(apiResponse, streNCRNo);
        }
        private async System.Threading.Tasks.Task SetResponseInfoContainers(SendGrid.Response apiResponse, string streNCRNo)
        {
            string strResult;
            strResult = await apiResponse.Body.ReadAsStringAsync();
        }

        private string MailContent(DataSet obj_Dset, string Owner, string HeaderContent)
        {
            System.Text.StringBuilder strMailContent = new System.Text.StringBuilder();
            strMailContent.Append("<table width=650 cellpadding=0 cellspacing=0 border=0><tr><td style='padding: 3px;' bgcolor='#ffffff'>");
            strMailContent.Append("<table width=100% cellpadding=5 cellspacing=0 border=0 bgcolor=#FFFFFF>");
            strMailContent.Append("<tr><td colspan=2>");
            strMailContent.Append("<p><span style='font-family: Calibri; font-size: 14px; color: #000000; font-weight: bold;'> " + Owner + "</span></p><p><span style='font-family: Calibri; font-size: 14px; color: #000000; font-weight: bold;'> " + HeaderContent + "</span></p><br />");
            strMailContent.Append("</td></tr>");
            strMailContent.Append("<tr><td valign=top style='width: 140px; font-family: Calibri; font-size: 14px; color: #000000; font-weight: bold;'>eNCR No:</td>");
            strMailContent.Append("<td valign=top style='font-family: Calibri; font-size: 15px; color: #000000; font-weight: normal; font-style: italic; border-bottom:dashed 1px #BCBCBC;'>" + obj_Dset.Tables[0].Rows[0]["NCRNO"].ToString() + "</td></tr>");
            strMailContent.Append("<tr><td valign=top style='font-family: Calibri; font-size: 14px; color: #000000; font-weight: bold;'>Holding Contract:</td>");
            strMailContent.Append("<td valign=top style='width: 520px; font-family: Calibri; font-size: 15px; color: #000000; font-weight: normal; font-style: italic; border-bottom:dashed 1px #BCBCBC;'>" + obj_Dset.Tables[0].Rows[0]["HCDESC"].ToString() + "</td></tr>");
            strMailContent.Append("<tr><td valign=top style='font-family: Calibri; font-size: 14px; color: #000000; font-weight: bold;'>Sub Contract:</td>");
            strMailContent.Append("<td valign=top style='font-family: Calibri; font-size: 15px; color: #000000; font-weight: normal; font-style: italic; border-bottom:dashed 1px #BCBCBC;'>" + obj_Dset.Tables[0].Rows[0]["ContractDesc"].ToString() + "</td></tr>");
            strMailContent.Append("<tr><td valign=top style='width: 140px; font-family: Calibri; font-size: 14px; color: #000000; font-weight: bold;'>Raised On:</td>");
            strMailContent.Append("<td valign=top style='font-family: Calibri; font-size: 15px; color: #000000; font-weight: normal; font-style: italic; border-bottom:dashed 1px #BCBCBC;'>" + obj_Dset.Tables[0].Rows[0]["RaisedOn"].ToString() + "</td></tr>");
            strMailContent.Append("<tr><td valign=top style='width: 140px; font-family: Calibri; font-size: 14px; color: #000000; font-weight: bold;'>Department:</td>");
            strMailContent.Append("<td valign=top style='font-family: Calibri; font-size: 15px; color: #000000; font-weight: normal; font-style: italic; border-bottom:dashed 1px #BCBCBC;'>" + obj_Dset.Tables[0].Rows[0]["SubLocation"].ToString() + "</td></tr>");
            strMailContent.Append("<tr><td valign=top style='font-family: Calibri; font-size: 14px; color: #000000; font-weight: bold;'>Type:</td>");
            strMailContent.Append("<td valign=top style='width: 520px; font-family: Calibri; font-size: 15px; color: #000000; font-weight: normal; font-style: italic; border-bottom:dashed 1px #BCBCBC;'>" + obj_Dset.Tables[0].Rows[0]["NCRType"].ToString() + "</td></tr>");
            strMailContent.Append("<tr><td valign=top style='font-family: Calibri; font-size: 14px; color: #000000; font-weight: bold;'>NCR Found At:</td>");
            strMailContent.Append("<td valign=top style='font-family: Calibri; font-size: 15px; color: #000000; font-weight: normal; font-style: italic; border-bottom:dashed 1px #BCBCBC;'>" + obj_Dset.Tables[0].Rows[0]["NCRLocation"].ToString() + "</td></tr>");
            strMailContent.Append("<tr><td valign=top style='font-family: Calibri; font-size: 14px; color: #000000; font-weight: bold;'>Category:</td>");
            strMailContent.Append("<td valign=top style='font-family: Calibri; font-size: 15px; color: #000000; font-weight: normal; font-style: italic; border-bottom:dashed 1px #BCBCBC;'>" + obj_Dset.Tables[0].Rows[0]["NCRCategory"].ToString() + "</td></tr>");
            strMailContent.Append("<tr><td valign=top style='font-family: Calibri; font-size: 14px; color: #000000; font-weight: bold;'>Sub Category:</td>");
            strMailContent.Append("<td valign=top style='font-family: Calibri; font-size: 15px; color: #000000; font-weight: normal; font-style: italic; border-bottom:dashed 1px #BCBCBC;'>" + obj_Dset.Tables[0].Rows[0]["NCRSubCategory"].ToString() + "</td></tr>");
            strMailContent.Append("<tr><td valign=top style='font-family: Calibri; font-size: 14px; color: #000000; font-weight: bold;'>Description:</td>");
            strMailContent.Append("<td valign=top style='font-family: Calibri; font-size: 15px; color: #000000; font-weight: normal; font-style: italic; border-bottom:dashed 1px #BCBCBC;'>" + obj_Dset.Tables[0].Rows[0]["NCRDescription"].ToString() + "</td></tr>");
            strMailContent.Append("<tr><td valign=top style='font-family: Calibri; font-size: 14px; color: #000000; font-weight: bold;'>Status:</td>");
            strMailContent.Append("<td valign=top style='font-family: Calibri; font-size: 15px; color: #000000; font-weight: normal; font-style: italic; border-bottom:dashed 1px #BCBCBC;'>" + obj_Dset.Tables[0].Rows[0]["NCRStatus"].ToString() + "</td></tr>");
            strMailContent.Append("</table>");
            strMailContent.Append("</td></tr></table><br><br>");
            strMailContent.Append("<span style='font-family: Calibri; font-size: 14px; padding-top:10px; color: #000000; font-weight: normal;'>Regards,<br>EPMS Administration.</span><br /><br /><br />");
            strMailContent.Append("<table cellpadding=2 cellspacing=0 width=100%><tr><td style='border-top:solid 2px #ddd; font-family:Calibri; font-size: 13px;color:#6095d7;'>This is an automated message sent from a mailbox, which isn’t monitored. Please don’t reply to it directly. Instead, simply login to EPMSlive and respond through Support Module.</td></tr></table>");

            return strMailContent.ToString();
        }


        //private async void SendgridNCRMail (string strMailSendID, string strMailContent, string strMailSubject, string strMailCCID, string streNCRNo, string sendgridNCRMail = "")
        //{


        //    var messageInfo = new EmailMessageInfo()
        //    {
        //        FromEmailAddress = ConfigurationManager.AppSettings("MailUser"),
        //        ToEmailAddress = strMailSendID,
        //        CcEmailAddress = strMailCCID,
        //        BccEmailAddress = "",
        //        EmailSubject = strMailSubject,
        //        EmailBody = strMailContent
        //    };
        //    SendGridService objsendgridMail = new SendGridEmail();
        //    var apiResponse = await objsendgridMail.Send(messageInfo);
        //    await SetResponseInfoContainers(apiResponse, streNCRNo);
        //}

        public class eNCRDetails
        {
            public string EmpCode { get; set; }
            public string NCRType { get; set; }
            public string EventCategory { get; set; }
            public string FountAt { get; set; }
            public string Description { get; set; }
            public string Location { get; set; }
            public string Department { get; set; }
            public string Category { get; set; }
            public string SubCategory { get; set; }
            public string HoldingContract { get; set; }
            public string SubContract { get; set; }
            public string AttFileName { get; set; }
            public string AttFileDataURL { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }
        }
    }
}