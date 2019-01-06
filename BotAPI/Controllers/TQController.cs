using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Web.Http;

namespace BotAPI.Controllers
{
    [RoutePrefix("api/TQ")]
    public class TQController : ApiController
    {
        [HttpGet, Route("GetTQCombo/{empCode}")]


        public DataSet GetTQCombo(string empCode)
        {
            DataSet ds = new DataSet();
            string strcon = ConfigurationManager.ConnectionStrings["SQL_DBCon"].ConnectionString;
            string[] result = new string[0];
            using (SqlConnection con = new SqlConnection(strcon))
            {
                using (SqlCommand cmd = new SqlCommand("[usp_TQ_Combo_ChatBot]", con))
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

        [HttpGet, Route("GetSubCategory/{TQCategory}")]
        public IEnumerable<string> GetSubCategory(string TQCategory)
        {
            string strcon = ConfigurationManager.ConnectionStrings["SQL_DBCon"].ConnectionString;
            string[] result = new string[0];
            using (SqlConnection con = new SqlConnection(strcon))
            {
                string strQuer = "SELECT SubCategoryId,  SubCategory  FROM MstrTQSubCategory WHERE CategoryID ='" + TQCategory + "'";
                using (SqlCommand cmd = new SqlCommand(strQuer, con))
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

        // POST api/TQ
        [HttpPost]
        public string SaveTQ(TQDetails tqdetails)
        {
            string retVal = "";
            try
            {
                WebClient client = new WebClient();

                string strFileUrlToDownload = tqdetails.AttFileDataURL;
                byte[] attachmentData = new byte[0];

                if (tqdetails.AttFileName != "")
                {
                    attachmentData = client.DownloadData((new Uri(strFileUrlToDownload)));
                }

                string strcon = ConfigurationManager.ConnectionStrings["SQL_DBCon"].ConnectionString;
                SqlConnection con = new SqlConnection(strcon);
                SqlCommand cmd = new SqlCommand("usp_TQ_Ins_Chatbot", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TQNO", 0);
                cmd.Parameters.AddWithValue("@RaisedBy", tqdetails.EmpCode);
                cmd.Parameters.AddWithValue("@RepliedByName", tqdetails.IssuedTo);
                cmd.Parameters.AddWithValue("@ClientRefNo", tqdetails.ClientRefNo);
                cmd.Parameters.AddWithValue("@DwgRefNo", tqdetails.DrawingRefNo);
                cmd.Parameters.AddWithValue("@Category", tqdetails.TQCategory);
                cmd.Parameters.AddWithValue("@SubCategory", tqdetails.TQSubCategory);
                cmd.Parameters.AddWithValue("@Query_Type", tqdetails.ClosureType);
                cmd.Parameters.AddWithValue("@ExpectedDate", tqdetails.ExpectedResponse);
                cmd.Parameters.AddWithValue("@BriefDesc", tqdetails.TQDescription);
                cmd.Parameters.AddWithValue("@HCDesc", tqdetails.HoldingContract);
                cmd.Parameters.AddWithValue("@ContractDesc", tqdetails.SubContract);
                cmd.Parameters.AddWithValue("@DrgRange", 0);
                cmd.Parameters.AddWithValue("@VONO", "");
                cmd.Parameters.AddWithValue("@TQDescriptionPlainText", tqdetails.TQDescription);
                cmd.Parameters.AddWithValue("@SWFileName", tqdetails.AttFileName);
                cmd.Parameters.AddWithValue("@SWFileSource", attachmentData);
                cmd.Parameters.Add("@RetMsg_CBot", SqlDbType.VarChar, 4000);
                cmd.Parameters["@RetMsg_CBot"].Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@RetID_CBot", SqlDbType.VarChar, 1);
                cmd.Parameters["@RetID_CBot"].Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@Success_CBot", SqlDbType.VarChar, 1);
                cmd.Parameters["@Success_CBot"].Direction = ParameterDirection.Output;
                cmd.Parameters.AddWithValue("@TQStartTime", tqdetails.TQStartTime);
                cmd.Parameters.AddWithValue("@TQEndTime", tqdetails.TQEndTime);
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

        public class TQDetails
        {
            public string EmpCode { get; set; }
            public string TQCategory { get; set; }
            public string TQSubCategory { get; set; }
            public string ClientRefNo { get; set; }
            public string DrawingRefNo { get; set; }
            public string TQDescription { get; set; }
            public string ClosureType { get; set; }
            public string ExpectedResponse { get; set; }
            public string HoldingContract { get; set; }
            public string SubContract { get; set; }
            public string DrawingRange { get; set; }
            public string VONo { get; set; }
            public string IssuedTo { get; set; }
            public string AttFileName { get; set; }
            public string AttFileDataURL { get; set; }
            public string TQStartTime { get; set; }
            public string TQEndTime { get; set; }
        }

        // PUT api/TQ/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/TQ/5
        public void Delete(int id)
        {
        }

    }
}