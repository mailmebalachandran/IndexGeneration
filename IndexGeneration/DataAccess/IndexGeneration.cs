using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace IndexGeneration.DataAccess
{
    public class IndexGenerationDetails : Foundation
    {
        public DataTable GetArchiveDetails(string storedProcedureName, string letterIds)
        {
            DataTable dt = new DataTable();
            try
            {
                using (var Connection = OpenConnection())
                {
                    var result = Connection.Query(storedProcedureName, new { LetterIds = letterIds },  commandTimeout:6000, commandType: CommandType.StoredProcedure);
                    dt = ToDataTable(result);
                }
            }
            catch (Exception ex)
            {
                LogDetail(ex.Message, "IndexGenerationDetails", "GetArchiveDetails", "", "");
            }
            return dt;
        }

        public List<Int64> GetArchiveLetters(string storedProcedureName, string type)
        {
            List<Int64> lstLetterDetails = new List<Int64>();
            try
            {
                using (var Connection = OpenConnection())
                {
                    var result = Connection.QueryMultiple(storedProcedureName, new { ClientAppType = type }, commandTimeout: 6000, commandType: CommandType.StoredProcedure);
                    lstLetterDetails = result.Read<Int64>().ToList();
                }
            }
            catch (Exception ex)
            {
                LogDetail(ex.Message, "IndexGenerationDetails", "GetArchiveLetters", "", "");
            }
            return lstLetterDetails;
        }

        public DataTable ToDataTable(IEnumerable<dynamic> items)
        {
            if (items == null) return null;
            var data = items.ToArray();
            if (data.Length == 0) return null;

            var dt = new DataTable();
            foreach (var pair in ((IDictionary<string, object>)data[0]))
            {
                dt.Columns.Add(pair.Key, (pair.Value ?? string.Empty).GetType());
            }
            foreach (var d in data)
            {
                dt.Rows.Add(((IDictionary<string, object>)d).Values.ToArray());
            }
            return dt;
        }

        public bool UpdateLetterComponentDetails(Int64 letterComponentId, string schemaName, string rrdFileName, string payerName)
        {
            bool isSaved = false;
            try
            {
                using (var Connection = OpenConnection())
                {
                    var result = Connection.QueryMultiple(Foundation.UpdateCCMLetterComponents, new
                    {
                        LetterComponentId = letterComponentId,
                        SchemaName = schemaName,
                        RRDFileName = rrdFileName,
                        PayerName = payerName
                    }, commandType: CommandType.StoredProcedure);
                    isSaved = result.Read<bool>().FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                LogDetail(ex.Message, "IndexGenerationDetails", "UpdateLetterComponentDetails", "", "");
            }
            return isSaved;
        }

        public bool SaveArchiveFileDetails(Int64 letterComponentId, string sourcePath, string destinationPath, string indexNumber)
        {
            bool isSaved = false;
            try
            {
                using (var Connection = OpenConnection())
                {
                    var result = Connection.QueryMultiple(Foundation.SetArchiveFileDetails, new
                    {
                        LetterComponentId = letterComponentId,
                        SourcePath = sourcePath,
                        DestinationPath = destinationPath,
                        IndexNumber = indexNumber
                    }, commandType: CommandType.StoredProcedure);
                    isSaved = result.Read<bool>().FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                LogDetail(ex.Message, "IndexGenerationDetails", "SaveArchiveFileDetails", "", "");
            }
            return isSaved;
        }


    }
}
