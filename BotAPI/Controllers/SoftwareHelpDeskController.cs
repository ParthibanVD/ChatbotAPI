using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Web.Http;

namespace BotAPI.Controllers
{
    [RoutePrefix("api/SoftwareHelpDesk")]
    public class SoftwareHelpDeskController : ApiController
    {
        // GET api/SoftwareHelpDesk/GetCategory
        [HttpGet, Route("GetCategory")]
        public IEnumerable<string> GetCategory()
        {

            string strcon = ConfigurationManager.ConnectionStrings["SQL_DBCon"].ConnectionString;
            string[] result = new string[0];
            using (SqlConnection con = new SqlConnection(strcon))
            {
                using (SqlCommand cmd = new SqlCommand("[usp_SoftwareHelpDesk_DDL]", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@intEmpCode", SqlDbType.VarChar).Value = 0;

                    con.Open();
                    cmd.ExecuteNonQuery();

                    DataSet ds = new DataSet();

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(ds);
                    result = new string[ds.Tables[1].Rows.Count];
                    int i = 0;
                    foreach (DataRow dr in ds.Tables[1].Rows)
                    {
                        result[i++] = dr[1].ToString();
                    }
                }
            }
            return result;
        }

        [HttpGet, Route("TicketType")]
        public IEnumerable<string> TicketType()
        {

            string strcon = ConfigurationManager.ConnectionStrings["SQL_DBCon"].ConnectionString;
            string[] result = new string[0];
            using (SqlConnection con = new SqlConnection(strcon))
            {
                using (SqlCommand cmd = new SqlCommand("[usp_SoftwareHelpDesk_DDL]", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@intEmpCode", SqlDbType.VarChar).Value = 0;

                    con.Open();
                    cmd.ExecuteNonQuery();

                    DataSet ds = new DataSet();

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(ds);
                    result = new string[ds.Tables[0].Rows.Count];
                    int i = 0;
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        result[i++] = dr[1].ToString();
                    }


                }
            }
            return result;
        }


        // GET api/SoftwareHelpDesk/5
        public string Get(int id)
        {
            return "value";
        }



        // POST api/SoftwareHelpDesk
        [HttpPost]
        public string SaveSoftwareHelpDesk(SoftwareHelpDeskDetails softwareDetails)
        {
            string retVal = "";
            try
            {
                WebClient client = new WebClient();

                string strFileUrlToDownload = softwareDetails.AttFileDataURL;
                byte[] attachmentData = new byte[0];

                if (softwareDetails.AttFileName != "")
                {
                    attachmentData = client.DownloadData((new Uri(strFileUrlToDownload)));
                }

                string strcon = ConfigurationManager.ConnectionStrings["SQL_DBCon"].ConnectionString;
                SqlConnection con = new SqlConnection(strcon);
                SqlCommand cmd = new SqlCommand("usp_SWHelpDesk_Ins_Chatbot", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SWHelpDeskID", 0);
                cmd.Parameters.AddWithValue("@CreatedBy", softwareDetails.EmpCode);
                cmd.Parameters.AddWithValue("@SWDesc", softwareDetails.TicketType);
                cmd.Parameters.AddWithValue("@SWCategoryDesc", softwareDetails.Category);
                cmd.Parameters.AddWithValue("@SupportDesc", softwareDetails.Description);
                cmd.Parameters.AddWithValue("@SWFileName", softwareDetails.AttFileName);
                cmd.Parameters.AddWithValue("@SWFileSource", attachmentData);
                cmd.Parameters.Add("@RetMsg_CBot", SqlDbType.VarChar, 4000);
                cmd.Parameters["@RetMsg_CBot"].Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@RetID_CBot", SqlDbType.VarChar, 1);
                cmd.Parameters["@RetID_CBot"].Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@Success_CBot", SqlDbType.VarChar, 400);
                cmd.Parameters["@Success_CBot"].Direction = ParameterDirection.Output;
                cmd.Parameters.AddWithValue("@ActionStartTime", softwareDetails.ActionStartTime);
                cmd.Parameters.AddWithValue("@ActionEndTime", softwareDetails.ActionEndTime);
                con.Open();
                int k = cmd.ExecuteNonQuery();
                // if (k != 0)
                //{
                retVal = cmd.Parameters["@RetMsg_CBot"].Value.ToString();
                // }
                con.Close();
            }
            catch (Exception e1)
            {
                retVal = e1.Message.ToString();
            }
            return retVal;
        }



        public class SoftwareHelpDeskDetails
        {
            public string EmpCode { get; set; }
            public string TicketType { get; set; }
            public string Category { get; set; }
            public string Description { get; set; }
            public string AttFileName { get; set; }
            public string AttFileDataURL { get; set; }
            public string ActionStartTime { get; set; }
            public string ActionEndTime { get; set; }
        }

        // PUT api/Leave/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/Leave/5
        public void Delete(int id)
        {
        }

    }
}