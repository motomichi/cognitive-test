using System.Configuration;
using System.Data.SqlClient;

namespace Core.Azure
{
    public class DataAccess
    {
        #region RegisterData
        public bool RegisterData(string user_id, string image_id, string json)
        {
            //log start
            System.Diagnostics.Trace.TraceInformation("RegisterData：start");
            System.Diagnostics.Trace.TraceInformation("RegisterData user_id：" + user_id);
            System.Diagnostics.Trace.TraceInformation("RegisterData image_id：" + image_id);
            System.Diagnostics.Trace.TraceInformation("RegisterData json：" + json);
            var result = 0;
            
            // connection info against Azure SQL DB
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBContext"].ConnectionString);
            
            //sql command
            SqlCommand insert = new SqlCommand("insert into [cognitive-result](user_id, image_id, cognitive_json) values(@user_id, @image_id, @cognitive_json)", conn);
            insert.Parameters.AddWithValue("@user_id", user_id);
            insert.Parameters.AddWithValue("@image_id", image_id);
            insert.Parameters.AddWithValue("@cognitive_json", json);

            //connection open
            conn.Open();

            //execute sql
            try { 
                result = insert.ExecuteNonQuery();
            } catch(SqlException e)
            {
                System.Diagnostics.Trace.TraceInformation("RegisterData insert error");
                System.Diagnostics.Trace.TraceInformation("RegisterData insert error detail：" + e.Message);
                if (!(e.InnerException == null))  System.Diagnostics.Trace.TraceInformation("RegisterData error occured：" + e.InnerException);
            }
            //close
            conn.Close();

            //log end
            System.Diagnostics.Trace.TraceInformation("RegisterData：end");

            return result > 0;
        }
        #endregion
    }
}
