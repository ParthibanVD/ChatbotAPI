using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.Http;

namespace BotAPI.Controllers
{
    public class ShiftController : ApiController
    {

        // GET api/Shift
        public IEnumerable<string> Get()
        {

            string strcon = ConfigurationManager.ConnectionStrings["SQL_DBCon"].ConnectionString;
            string[] result = new string[0];
            using (SqlConnection con = new SqlConnection(strcon))
            {
                using (SqlCommand cmd = new SqlCommand("[usp_GetDepartment]", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@EmpCode", SqlDbType.VarChar).Value = 19;

                    con.Open();
                    cmd.ExecuteNonQuery();

                    DataSet ds = new DataSet();

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(ds);
                    result = new string[ds.Tables[0].Rows.Count];
                    int i = 0;
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        result[i++] = dr[0].ToString();
                    }


                }
            }
            return result;
        }

        [HttpPost]
        public string BookShift(ShiftDetails sDetails)
        {

            string retVal = "";
            try
            {

                int monthNo = DateTime.ParseExact(sDetails.monthName, "MMMM", CultureInfo.CurrentCulture).Month;
                DateTime startOfMonth = new DateTime(DateTime.Now.Year, monthNo, 1);   //new DateTime(year, month, 1);
                DateTime endOfMonth = new DateTime(DateTime.Now.Year, monthNo, DateTime.DaysInMonth(DateTime.Now.Year, monthNo)); //new DateTime(year, month,
                string sDate = startOfMonth.ToString("dd-MMM-yyyy");
                string eDate = endOfMonth.ToString("dd-MMM-yyyy");
                string strcon = ConfigurationManager.ConnectionStrings["SQL_DBCon"].ConnectionString;
                SqlConnection con = new SqlConnection(strcon);
                SqlCommand cmd = new SqlCommand("usp_MstrShiftEmp_ChatBot_Ins", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EmpCode", sDetails.EmpCode);
                cmd.Parameters.AddWithValue("@ShiftDesc", sDetails.shift);
                cmd.Parameters.AddWithValue("@FromDate", sDate);
                cmd.Parameters.AddWithValue("@ToDate", eDate);
                cmd.Parameters.AddWithValue("@ActionStartTime", sDetails.Starttime);
                cmd.Parameters.AddWithValue("@ActionEndTime", sDetails.Endtime);
                cmd.Parameters.Add("@RetMsg", SqlDbType.VarChar, 4000);
                cmd.Parameters["@RetMsg"].Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@Success", SqlDbType.VarChar, 1);
                cmd.Parameters["@Success"].Direction = ParameterDirection.Output;


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

        public class ShiftDetails
        {
            public string EmpCode { get; set; }
            public string shift { get; set; }
            public string monthName { get; set; }
            public string Starttime { get; set; }
            public string Endtime { get; set; }
        }


        // PUT api/Shift/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/Shift/5
        public void Delete(int id)
        {
        }
    }
}