using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Configuration;

namespace NexusERP.Infrastructure.Database
{
    public class DatabaseHelper
    {
        internal static string connString = ConfigurationManager.ConnectionStrings["InventoryDb"].ConnectionString;

        public static DataTable ExecuteStoredProcedure(string procedureName, Dictionary<string, object> parametrs = null)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    using (SqlCommand cmd = new SqlCommand(procedureName, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        if (parametrs != null)
                        {
                            foreach (var param in parametrs)
                            {
                                cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                            }
                        }

                        DataTable dt = new DataTable();
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
            catch (SqlException)
            {
                throw new Exception("Database Error: We couldn't retrieve the products. Please check your connection.");
            }catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred: " + ex.Message);
            }
            
        }

        public static int ExecuteNonQuery(string procedureName, Dictionary<string, object> parameters)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    using (SqlCommand cmd = new SqlCommand(procedureName, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        foreach (var param in parameters)
                        {
                            cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                        }

                        conn.Open();
                        return cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException)
            {
                throw new Exception("Database Error: We couldn't update the products. Please check your connection.");
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred: " + ex.Message);
            }
        }
    }
}
