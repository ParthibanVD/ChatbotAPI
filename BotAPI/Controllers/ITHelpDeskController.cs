using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BotAPI.Controllers
{
    [RoutePrefix("api/ITHelpDesk")]
    public class ITHelpDeskController : ApiController
    {
        [HttpGet, Route("GetITHelpDesk/{empCode}")]

        public DataSet GetITHelpDesk(string empCode)
        {
            DataSet ds = new DataSet();
            string strcon = ConfigurationManager.ConnectionStrings["SQL_DBCon"].ConnectionString;
            string[] result = new string[0];
            using (SqlConnection con = new SqlConnection(strcon))
            {
                using (SqlCommand cmd = new SqlCommand("[usp_ITHelpDesk_DDL]", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@intEmpCode", SqlDbType.VarChar).Value = empCode;

                    con.Open();
                    cmd.ExecuteNonQuery();

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(ds);


                }
            }
            return ds;
        }

        // POST api/ITHelpDesk
        [HttpPost]
        public string SaveIT(ITDetails ITdetails)
        {
            string retVal = "";
            try
            {
                //int
                //if (ITdetails.Priority == "LOW")
                //{

                //}

                WebClient client = new WebClient();
                string strcon = ConfigurationManager.ConnectionStrings["SQL_DBCon"].ConnectionString;
                SqlConnection con = new SqlConnection(strcon);
                SqlCommand cmd = new SqlCommand("usp_ITHD_Ins_Chatbot", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@HelpDeskID", 0);
                cmd.Parameters.AddWithValue("@TicketType", ITdetails.TicketType);
                cmd.Parameters.AddWithValue("@Category", ITdetails.Category);
                cmd.Parameters.AddWithValue("@SubCategory", ITdetails.SubCategory);
                cmd.Parameters.AddWithValue("@PriorityDesc", ITdetails.Priority);
                cmd.Parameters.AddWithValue("@TicketDesc", ITdetails.Description);
                cmd.Parameters.AddWithValue("@MachineNo", ITdetails.MachineNum);
                cmd.Parameters.AddWithValue("@ExtensionNo", ITdetails.ExtensionNum);
                cmd.Parameters.AddWithValue("@RaisedBy", ITdetails.EmpCode);
                cmd.Parameters.AddWithValue("@AcceptedBy", 0);
                cmd.Parameters.AddWithValue("@ITRemarks", "");
                cmd.Parameters.Add("@RetMsg_CBot", SqlDbType.VarChar, 4000);
                cmd.Parameters["@RetMsg_CBot"].Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@RetID_CBot", SqlDbType.VarChar, 1);
                cmd.Parameters["@RetID_CBot"].Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@Success_CBot", SqlDbType.VarChar, 1);
                cmd.Parameters["@Success_CBot"].Direction = ParameterDirection.Output;
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


        // PUT api/IT/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/IT/5
        public void Delete(int id)
        {
        }

    }
    public class ITDetails
    {
        public string EmpCode { get; set; }
        public string TicketType { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string Description { get; set; }
        public string MachineNum { get; set; }
        public string ExtensionNum { get; set; }
        public string Priority { get; set; }
    }
}
