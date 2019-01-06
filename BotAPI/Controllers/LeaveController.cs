using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Http;

namespace BotAPI.Controllers
{
    public class LeaveController : ApiController
    {
        // GET api/Leave
        public IEnumerable<string> Get()
        {


            string[] result = new string[0];

            return result;
        }

        // GET api/Leave/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/Leave
        [HttpPost]
        public string BookLeave(LeaveDetails leaveDetails)
        {
            string retVal = "";
            try
            {

                string strcon = ConfigurationManager.ConnectionStrings["SQL_DBCon"].ConnectionString;
                SqlConnection con = new SqlConnection(strcon);
                SqlCommand cmd = new SqlCommand("usp_LeaveManager_Ins_Upd_V4", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EmpCode", leaveDetails.EmpCode);
                cmd.Parameters.AddWithValue("@StartDate", leaveDetails.StartDate);
                cmd.Parameters.AddWithValue("@EndDate", leaveDetails.EndDate);
                cmd.Parameters.AddWithValue("@LeaveType", leaveDetails.LeaveType);
                cmd.Parameters.AddWithValue("@LeaveCategory", (int)Enum.Parse(typeof(LeaveCategory), leaveDetails.LeaveCategory));
                cmd.Parameters.AddWithValue("@LeaveID", 0);
                cmd.Parameters.AddWithValue("@SysName", "");
                cmd.Parameters.AddWithValue("@MachineName", "");
                cmd.Parameters.AddWithValue("@IsFirstHalf", "");
                cmd.Parameters.Add("@RetMsg", SqlDbType.VarChar, 4000);
                cmd.Parameters["@RetMsg"].Direction = ParameterDirection.Output;

                cmd.Parameters.Add("@Success", SqlDbType.VarChar, 1);
                cmd.Parameters["@Success"].Direction = ParameterDirection.Output;

                cmd.Parameters.Add("@RetID", SqlDbType.Int, 1);
                cmd.Parameters["@RetID"].Direction = ParameterDirection.Output;

                con.Open();
                int k = cmd.ExecuteNonQuery();
                // if (k != 0)
                //{
                retVal = cmd.Parameters["@RetMsg"].Value.ToString();
                // }
                con.Close();
            }
            catch (Exception e1)
            {
                retVal = e1.Message.ToString();
            }
            return retVal;
        }


    public class LeaveDetails
        {
            public string EmpCode { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public string LeaveType { get; set; }
            public string LeaveCategory { get; set; }
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

    public enum LeaveCategory
    {
        CL = 120,
        SL = 121,
        EL = 122,
        MarriageLeave = 123,
        MaternityLeave = 124,
        PaternityLeave = 125,
        CompassionateLeave = 126,
        CompensationLeave = 127,
        VerySpecialLeave = 128,
        LOP = 129,
        LeaveSharing = 130,
        SabbaticalLeave = 131,
        AdoptionLeave = 132

    }
}