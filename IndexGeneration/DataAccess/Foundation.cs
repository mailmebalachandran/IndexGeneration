using IndexGeneration.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndexGeneration.DataAccess
{
    public class Foundation
    {
        public static IDbConnection OpenConnection()
        {
            var connectionString = ConfigurationManager.AppSettings["ConnectionString"].ToString();
            var connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        public static void LogDetail(string errorMessage, string className, string methodName, string Date, string IndexNumber)
        {
            try
            {
                if (!string.IsNullOrEmpty(Date) && !string.IsNullOrEmpty(IndexNumber))
                {
                    if (!File.Exists(Path.Combine(Global.LogExecuteablePath, "Log", "log_" + Date + "_" + IndexNumber + ".txt")))
                        File.Create(Path.Combine(Global.LogExecuteablePath, "Log", "log_" + Date + "_" + IndexNumber + ".txt")).Close();
                    File.AppendAllText(Path.Combine(Global.LogExecuteablePath, "Log", "log_" + Date + "_" + IndexNumber + ".txt"), (DateTime.Now.ToLongDateString() + "------>>" + className + "------>>" + methodName + "------>>" + errorMessage + ">>" + Environment.NewLine));
                }
                else
                    File.AppendAllText(Path.Combine(Global.LogExecuteablePath, "Log", "log.txt"), (DateTime.Now.ToLongDateString() + "------>>" + className + "------>>" + methodName + "------>>" + errorMessage + ">>" + Environment.NewLine));
            }
            catch (Exception ex)
            {
                string errorMess = ex.Message;
            }
        }

        #region Stored procedure 

        public const string UpdateCCMLetterComponents = "TM_UpdateCCMLetterComponents";
        public const string GetArchiveDetailsForCigna = "TM_ArchiveDetails_NHPSchema";
        public const string SetArchiveFileDetails = "TM_SaveArchiveFileDetails";
        #endregion

    }
}
