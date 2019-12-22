using IndexGeneration.DataAccess;
using IndexGeneration.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IndexGeneration
{
    class Program
    {
        #region Global Variables

        public string exelaPDFPath = ConfigurationManager.AppSettings["ExelaPathForPDF"].ToString();
        public string exelaXeroxPDFPath = ConfigurationManager.AppSettings["ExelaPathForXeroxPDF"].ToString();
        public string FolderRRDPath = ConfigurationManager.AppSettings["FolderForRRD"].ToString();
        public int IndexFileCount = 1;
        public string Delimiter = ConfigurationManager.AppSettings["Delimiter"].ToString();
        public int PDFCountDetails = Convert.ToInt32(ConfigurationManager.AppSettings["PDFCountDetails"].ToString());
        public int PDFCountDetailsMax = Convert.ToInt32(ConfigurationManager.AppSettings["PDFCountDetailsMax"].ToString());
        public string StoredProcedureName = ConfigurationManager.AppSettings["StoredProcedureName"].ToString();
        public string StoredProcedureNameForCount = ConfigurationManager.AppSettings["StoredProcedureNameForCount"].ToString();

        public string CategoryType = ConfigurationManager.AppSettings["CategoryType"].ToString();

        public int IndexFileInitialCount = 0;
        IndexFileDetails indexFileInfo = new IndexFileDetails();
        public int PDFCountCCX4000 = 1;
        public int PDFCountCCX3000 = 1;
        public int PDFCountCCX2000 = 1;
        public int PDFCountCCX1000 = 1;
        public string PreviousPDFFileNameCCX1000 = "";
        public bool CCX4000ReachedData = false;
        public int CCX4000Index = 0;

        #endregion

        static void Main(string[] args)
        {
            Program program = new Program();
            Global.LogExecuteablePath = System.IO.Path.GetDirectoryName(ConfigurationManager.AppSettings["ApplicationExecutablePath"].ToString());
            IndexFileDetails indexFileInfo = new IndexFileDetails();
            var ExecutablePath = System.IO.Path.GetDirectoryName(ConfigurationManager.AppSettings["ApplicationExecutablePath"].ToString());
            string IndexInfoconfigPath = Path.Combine(ExecutablePath, "IndexDetails.json");
            var indexDetails = JsonConvert.DeserializeObject<IndexFileDetails>(File.ReadAllText(IndexInfoconfigPath));
            indexFileInfo = indexDetails;
            Foundation.LogDetail("Started the Index File Creation", "Program", "Main", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
            program.BindDataBasedOnType();
        }

        public void SavePDFCountValue(int count, string type)
        {
            var ExecutablePath = System.IO.Path.GetDirectoryName(ConfigurationManager.AppSettings["ApplicationExecutablePath"].ToString());
            string IndexInfoconfigPath = Path.Combine(ExecutablePath, "IndexDetails.json");
            var indexDetails = JsonConvert.DeserializeObject<IndexFileDetails>(File.ReadAllText(IndexInfoconfigPath));
            indexFileInfo = indexDetails;
            if (type == ClientAppType.CCX4000.ToString())
                indexFileInfo.IndexInnerPDFNumberCCX4000 = count.ToString();
            else if (type == ClientAppType.CCX3000.ToString())
                indexFileInfo.IndexInnerPDFNumberCCX3000 = count.ToString();
            else if (type == ClientAppType.CCX2000.ToString())
                indexFileInfo.IndexInnerPDFNumberCCX2000 = count.ToString();
            else if (type == ClientAppType.CCX1000.ToString())
                indexFileInfo.IndexInnerPDFNumberCCX1000 = count.ToString();
            var result = JsonConvert.SerializeObject(indexFileInfo);
            File.WriteAllText(IndexInfoconfigPath, result);
            indexDetails = JsonConvert.DeserializeObject<IndexFileDetails>(File.ReadAllText(IndexInfoconfigPath));
            indexFileInfo = indexDetails;
        }

        public bool contentAndWrite(DataRow dr, string pathforRRDIndex, string indexFileCountNumber, string type, string pathforRRDPDF, int pdfCount, string pathForRRDAudit, bool isOldRecord)
        {
            bool isFileCopied = false;
            try
            {
                try
                {
                    if (!File.Exists(Path.Combine(pathforRRDIndex, type + "_L_P_107_" + DateTime.Now.ToString("yyyyMMdd") + "_" + indexFileInfo.AuditFileNumber.PadLeft(3, '0') + "_archive_v2_IDX_" + indexFileCountNumber.ToString().PadLeft(3, '0') + ".idx")))
                    {
                        File.Create(Path.Combine(pathforRRDIndex, type + "_L_P_107_" + DateTime.Now.ToString("yyyyMMdd") + "_" + indexFileInfo.AuditFileNumber.PadLeft(3, '0') + "_archive_v2_IDX_" + indexFileCountNumber.ToString().PadLeft(3, '0') + ".idx")).Close();
                        Foundation.LogDetail("Folder Created Successfully", "Program", "contentAndWrite", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                    }
                }
                catch (Exception ex)
                {
                    Foundation.LogDetail("Error in creating the File : " + Path.Combine(pathforRRDIndex, type + "_L_P_107_" + DateTime.Now.ToString("yyyyMMdd") + "_" + indexFileInfo.AuditFileNumber.PadLeft(3, '0') + "_archive_v2_IDX_" + indexFileCountNumber.ToString().PadLeft(3, '0') + ".idx") + "Exception : " + ex.Message, "Program", "contentAndWrite", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                }
                string FileNameNew = "";
                if (!isOldRecord)
                {
                    if (!string.IsNullOrEmpty(dr["Individual_pdf_name"].ToString()))
                    {
                        FileNameNew = type + "_L_P_107_" + DateTime.Now.ToString("yyyyMMdd") + "_" + indexFileInfo.AuditFileNumber.PadLeft(3, '0') + "_archive_v2_IDX_" + indexFileCountNumber.ToString().PadLeft(3, '0') + "_PDF_" + pdfCount.ToString().PadLeft(5, '0') + ".pdf";
                        PreviousPDFFileNameCCX1000 = FileNameNew;
                    }
                }

                string content = dr["CLIENTAPPID"].ToString() + Delimiter + dr["Consumer ID"].ToString() + Delimiter +
                                            dr["Generic Index 1"].ToString() + Delimiter +
                                            dr["Generic Index 2"].ToString() + Delimiter +
                                            dr["Generic Index 3"].ToString() + Delimiter +
                                            dr["Generic Index 4"].ToString() + Delimiter +
                                            dr["Generic Index 5"].ToString() + Delimiter +
                                            dr["Generic Index 6"].ToString() + Delimiter +
                                            dr["Generic Index 7"].ToString() + Delimiter +
                                            dr["Generic Index 8"].ToString() + Delimiter +
                                            dr["Generic Index 9"].ToString() + Delimiter +
                                            dr["reference_number"].ToString() + Delimiter +
                                            dr["statement_date"].ToString() + Delimiter +
                                            dr["PDF_Block_Name"].ToString() + Delimiter +
                                            PreviousPDFFileNameCCX1000 + Delimiter +
                                            dr["PDF_Block_Begin_Page"].ToString() + Delimiter +
                                            dr["PDF_Block_End_Page"].ToString() + Delimiter +
                                            dr["Email_Address"].ToString() + Delimiter +
                                            dr["Attachment_Password"].ToString() + Delimiter +
                                            dr["Web_Presentment"].ToString() + Environment.NewLine;
                File.AppendAllText(Path.Combine(pathforRRDIndex, type + "_L_P_107_" + DateTime.Now.ToString("yyyyMMdd") + "_" + indexFileInfo.AuditFileNumber.PadLeft(3, '0') + "_archive_v2_IDX_" + indexFileCountNumber.ToString().PadLeft(3, '0') + ".idx"), content);
                Foundation.LogDetail("Data appended in the Index file successfully", "Program", "contentAndWrite", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                IndexGenerationDetails IndexGenerationDetails = new IndexGenerationDetails();
                try
                {
                    using (new Impersonator("NOVITEX", "tecramgr", "#admin001!"))
                    {
                        if (!Directory.Exists(Path.Combine(pathforRRDPDF)))
                            Directory.CreateDirectory(pathforRRDPDF);
                        if (dr["Individual_pdf_name"].ToString().StartsWith("ArchivePDF"))
                            File.Copy(exelaPDFPath + dr["Individual_pdf_name"].ToString(), pathforRRDPDF + "\\" + PreviousPDFFileNameCCX1000, true);
                        else
                            File.Copy(exelaXeroxPDFPath + dr["Individual_pdf_name"].ToString(), pathforRRDPDF + "\\" + PreviousPDFFileNameCCX1000, true);
                        Foundation.LogDetail("PDF file copied to the RRD Folder Successfully", "Program", "contentAndWrite", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                        IndexGenerationDetails.UpdateLetterComponentDetails(Convert.ToInt64(dr["LetterComponentId"].ToString()), dr["TableName"].ToString(), PreviousPDFFileNameCCX1000, type == ClientAppType.CCX2000.ToString() ? dr["Generic Index 1"].ToString() : dr["Generic Index 2"].ToString());
                        Foundation.LogDetail("Update the details to database successfully", "Program", "contentAndWrite", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                        isFileCopied = true;
                    }
                }
                catch (Exception ex)
                {
                    Foundation.LogDetail("Error in creating the Directory and error in copying the file : " + Path.Combine(pathforRRDPDF, type + "_" + DateTime.Now.ToString("yyyyMMdd") + "_" + indexFileInfo.AuditFileNumber.PadLeft(3, '0') + "_index_" + indexFileCountNumber.ToString().PadLeft(3, '0')) + "Exception : " + ex.Message, "Program", "contentAndWrite", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                    isFileCopied = false;
                }
                //try
                //{
                //    if (!File.Exists(Path.Combine(pathForRRDAudit, type + "_L_P_107_" + DateTime.Now.ToString("yyyyMMdd") + "_" + indexFileInfo.AuditFileNumber.ToString().PadLeft(3, '0') + "_archive_v2_IDX_" + IndexFileCount.ToString().PadLeft(3, '0') + ".adt")))
                //        File.Create(Path.Combine(pathForRRDAudit, type + "_L_P_107_" + DateTime.Now.ToString("yyyyMMdd") + "_" + indexFileInfo.AuditFileNumber.ToString().PadLeft(3, '0') + "_archive_v2_IDX_" + IndexFileCount.ToString().PadLeft(3, '0') + ".adt")).Close();
                //}
                //catch (Exception ex)
                //{
                //    Foundation.LogDetail("Error in creating the Audit File : " + Path.Combine(pathForRRDAudit, type + "_L_P_107_" + DateTime.Now.ToString("yyyyMMdd") + "_" + indexFileInfo.AuditFileNumber.ToString().PadLeft(3, '0') + "_archive_v2_IDX_" + IndexFileCount.ToString().PadLeft(3, '0') + ".adt") + "Exception : " + ex.Message, "Program", "contentAndWrite", "", dr["LetterComponentId"].ToString());
                //}
                //if (!isOldRecord)
                //{
                //    string auditContent = FileNameNew + "||" +
                //                                        DateTime.Now.ToString("yyyyMMdd hh:mm:ss") + "||" +
                //                                        PDFCountDetails.ToString() + "||1||||||||" + Environment.NewLine;
                //    File.AppendAllText(Path.Combine(pathForRRDAudit, type + "_L_P_107_" + DateTime.Now.ToString("yyyyMMdd") + "_" + indexFileInfo.AuditFileNumber.ToString().PadLeft(3, '0') + "_archive_v2_IDX_" + indexFileCountNumber.ToString().PadLeft(3, '0') + ".adt"), auditContent);
                //}
            }
            catch (Exception ex)
            {
                Foundation.LogDetail("Error in Writing the Content and Exception : " + ex.Message, "Program", "contentAndWrite", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                isFileCopied = false;
            }
            return isFileCopied;
        }

        public void BindDataBasedOnType()
        {
            var ExecutablePathInitial = System.IO.Path.GetDirectoryName(ConfigurationManager.AppSettings["ApplicationExecutablePath"].ToString());
            string IndexInfoconfigPathInitial = Path.Combine(ExecutablePathInitial, "IndexDetails.json");
            var indexDetailsInitial = JsonConvert.DeserializeObject<IndexFileDetails>(File.ReadAllText(IndexInfoconfigPathInitial));
            indexFileInfo = indexDetailsInitial;
            DataTable dt = new DataTable();
            IndexGenerationDetails indexGenerationDetails = new IndexGenerationDetails();
            List<Int64> lstLetters = indexGenerationDetails.GetArchiveLetters(StoredProcedureNameForCount, CategoryType);
            Foundation.LogDetail("Get the Letter Components Ids" + string.Join(",", lstLetters.Select(x => x.ToString()).ToArray()), "Program", "BindDataBasedOnType", "", "");
            dt = indexGenerationDetails.GetArchiveDetails(StoredProcedureName, string.Join(",", lstLetters.Select(x => x.ToString()).ToArray()));

            if (dt != null)
            {
                if (dt.Rows.Count != 0)
                {
                    Foundation.LogDetail("Get the Data based on the Letter Component Ids Count : " + dt.Rows.Count, "Program", "BindDataBasedOnType", "", "");
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        #region CCX4000

                        if (CategoryType == ClientAppType.CCX4000.ToString())
                        {
                            try
                            {
                                Foundation.LogDetail("CCX4000 : Started", "Program", "BindDataBasedOnType", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                                SavePDFCountValue(PDFCountCCX4000, ClientAppType.CCX4000.ToString());
                                string zipFolderName = ZipFolderName();
                                Foundation.LogDetail("CCX4000 : Zip Folder Name : " + zipFolderName, "Program", "BindDataBasedOnType", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                                string pathforRRDIndex = Path.Combine(FolderRRDPath, zipFolderName); FolderCreation(pathforRRDIndex);
                                string pathforRRDPDF = Path.Combine(FolderRRDPath, zipFolderName); FolderCreation(pathforRRDPDF);
                                string pathforRRDAudit = Path.Combine(FolderRRDPath, zipFolderName); FolderCreation(pathforRRDAudit);
                                Foundation.LogDetail("CCX4000 Category zip Folder Name Created: " + zipFolderName, "Program", "BindDataBasedOnType", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                                if (PDFCountCCX4000 <= PDFCountDetails)
                                {
                                    bool isOldRecord = false;
                                    if (i != 0)
                                    {
                                        if (Convert.ToInt64(dt.Rows[i - 1]["LetterComponentId"].ToString()) == Convert.ToInt64(dt.Rows[i]["LetterComponentId"].ToString()))
                                            isOldRecord = true;
                                    }
                                    bool isFileCopied = contentAndWrite(dt.Rows[i], pathforRRDIndex, indexFileInfo.IndexInnerFileNumberCCX4000, CategoryType, pathforRRDPDF, PDFCountCCX4000, pathforRRDAudit, isOldRecord);
                                    if (!isFileCopied)
                                    {
                                        Console.WriteLine("Error in copying files");
                                        System.Environment.Exit(0);
                                    }
                                    if (!isOldRecord)
                                        PDFCountCCX4000++;
                                    Foundation.LogDetail("CCX4000 : End ", "Program", "BindDataBasedOnType", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                                }
                                else
                                {
                                    PDFCountCCX4000 = 1;
                                    //string auditContent = CategoryType + "_L_P_107_" + DateTime.Now.ToString("yyyyMMdd") + "_" + indexFileInfo.AuditFileNumber.PadLeft(3, '0') + "_archive_v2_IDX_" + indexFileInfo.IndexInnerFileNumberCCX4000.ToString().PadLeft(3, '0') + ".idx||" +
                                    //DateTime.Now.ToString("yyyyMMdd hh:mm:ss") + "||" +
                                    //PDFCountDetails.ToString() + "||" + PDFCountDetails.ToString() + "||||||||" + Environment.NewLine;
                                    //File.AppendAllText(Path.Combine(pathforRRDAudit, CategoryType + "_L_P_107_" + DateTime.Now.ToString("yyyyMMdd") + "_" + indexFileInfo.AuditFileNumber.ToString().PadLeft(3, '0') + "_archive_v2_IDX_" + indexFileInfo.IndexInnerFileNumberCCX4000.ToString().PadLeft(3, '0') + ".adt"), auditContent);
                                    SavePDFCountValue(PDFCountCCX4000, ClientAppType.CCX4000.ToString());
                                    int innerNumber = Convert.ToInt32(indexFileInfo.IndexInnerFileNumberCCX4000);
                                    innerNumber++;
                                    if (innerNumber > (Convert.ToInt32(PDFCountDetailsMax) / PDFCountDetails))
                                    {
                                        try
                                        {
                                            ZipFile.CreateFromDirectory(Path.Combine(FolderRRDPath, zipFolderName), Path.Combine(FolderRRDPath, zipFolderName) + ".zip");
                                            //EncryptFile(Path.Combine(FolderRRDPath, zipFolderName) + ".zip", Path.Combine(FolderRRDPath, zipFolderName) + ".zip");
                                        }
                                        catch (Exception ex)
                                        {
                                            Foundation.LogDetail("CCX4000 : Error Creating the Zip File Exception : " + ex.Message, "Program", "BindDataBasedOnType", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                                        }
                                        var ExecutablePath1 = System.IO.Path.GetDirectoryName(ConfigurationManager.AppSettings["ApplicationExecutablePath"].ToString());
                                        string IndexInfoconfigPath1 = Path.Combine(ExecutablePath1, "IndexDetails.json");
                                        int auditFileNumber = Convert.ToInt32(indexFileInfo.AuditFileNumber);
                                        auditFileNumber++;
                                        indexFileInfo.AuditFileNumber = auditFileNumber.ToString();
                                        var result1 = JsonConvert.SerializeObject(indexFileInfo);
                                        File.WriteAllText(IndexInfoconfigPath1, result1);
                                        var indexDetails = JsonConvert.DeserializeObject<IndexFileDetails>(File.ReadAllText(IndexInfoconfigPath1));
                                        indexFileInfo = indexDetails;
                                        zipFolderName = ZipFolderName();
                                        pathforRRDIndex = Path.Combine(FolderRRDPath, zipFolderName); FolderCreation(pathforRRDIndex);
                                        pathforRRDPDF = Path.Combine(FolderRRDPath, zipFolderName); FolderCreation(pathforRRDPDF);
                                        pathforRRDAudit = Path.Combine(FolderRRDPath, zipFolderName); FolderCreation(pathforRRDAudit);
                                        var ExecutablePath = System.IO.Path.GetDirectoryName(ConfigurationManager.AppSettings["ApplicationExecutablePath"].ToString());
                                        string IndexInfoconfigPath = Path.Combine(ExecutablePath, "IndexDetails.json");
                                        indexFileInfo.IndexInnerFileNumberCCX4000 = "1";
                                        var result = JsonConvert.SerializeObject(indexFileInfo);
                                        File.WriteAllText(IndexInfoconfigPath, result);
                                       bool isFileCopied = contentAndWrite(dt.Rows[i], pathforRRDIndex, indexFileInfo.IndexInnerFileNumberCCX4000, CategoryType, pathforRRDPDF, PDFCountCCX4000, pathforRRDAudit, false);
                                        if (!isFileCopied)
                                        {
                                            Console.WriteLine("Error in copying files");
                                            System.Environment.Exit(0);
                                        }
                                        PDFCountCCX4000++;
                                        Foundation.LogDetail("CCX4000 : End ", "Program", "BindDataBasedOnType", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                                    }
                                    else
                                    {
                                        var ExecutablePath = System.IO.Path.GetDirectoryName(ConfigurationManager.AppSettings["ApplicationExecutablePath"].ToString());
                                        string IndexInfoconfigPath = Path.Combine(ExecutablePath, "IndexDetails.json");
                                        indexFileInfo.IndexInnerFileNumberCCX4000 = innerNumber.ToString();
                                        var result = JsonConvert.SerializeObject(indexFileInfo);
                                        File.WriteAllText(IndexInfoconfigPath, result);
                                        bool isFileCopied = contentAndWrite(dt.Rows[i], pathforRRDIndex, indexFileInfo.IndexInnerFileNumberCCX4000, CategoryType, pathforRRDPDF, PDFCountCCX4000, pathforRRDAudit, false);
                                        if (!isFileCopied)
                                        {
                                            Console.WriteLine("Error in copying files");
                                            System.Environment.Exit(0);
                                        }
                                        PDFCountCCX4000++;
                                        Foundation.LogDetail("CCX4000 : End ", "Program", "BindDataBasedOnType", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Foundation.LogDetail("CCX4000 : Error while doing logics : " + ex.Message, "Program", "BindDataBasedOnType", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                            }
                        }

                        #endregion

                        #region CCX3000

                        if (CategoryType == ClientAppType.CCX3000.ToString())
                        {
                            try
                            {
                                Foundation.LogDetail("CCX3000 : Started", "Program", "BindDataBasedOnType", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                                SavePDFCountValue(PDFCountCCX3000, ClientAppType.CCX3000.ToString());
                                string zipFolderName = ZipFolderName();
                                Foundation.LogDetail("CCX3000 : Zip Folder Name : " + zipFolderName, "Program", "BindDataBasedOnType", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                                string pathforRRDIndex = Path.Combine(FolderRRDPath, zipFolderName); FolderCreation(pathforRRDIndex);
                                string pathforRRDPDF = Path.Combine(FolderRRDPath, zipFolderName); FolderCreation(pathforRRDPDF);
                                string pathforRRDAudit = Path.Combine(FolderRRDPath, zipFolderName); FolderCreation(pathforRRDAudit);
                                if (PDFCountCCX3000 <= PDFCountDetails)
                                {
                                    bool isOldRecord = false;
                                    if (i != 0)
                                    {
                                        if (Convert.ToInt64(dt.Rows[i - 1]["LetterComponentId"].ToString()) == Convert.ToInt64(dt.Rows[i]["LetterComponentId"].ToString()))
                                            isOldRecord = true;
                                    }
                                   bool isFileCopied = contentAndWrite(dt.Rows[i], pathforRRDIndex, indexFileInfo.IndexInnerFileNumberCCX3000, CategoryType, pathforRRDPDF, PDFCountCCX3000, pathforRRDAudit, isOldRecord);
                                    if (!isFileCopied)
                                    {
                                        Console.WriteLine("Error in copying files");
                                        System.Environment.Exit(0);
                                    }
                                    if (!isOldRecord)
                                        PDFCountCCX3000++;
                                    Foundation.LogDetail("CCX3000 : End ", "Program", "BindDataBasedOnType", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                                }
                                else
                                {
                                    PDFCountCCX3000 = 1;
                                    //string auditContent = CategoryType + "_L_P_107_" + DateTime.Now.ToString("yyyyMMdd") + "_" + indexFileInfo.AuditFileNumber.PadLeft(3, '0') + "_archive_v2_IDX_" + indexFileInfo.IndexInnerFileNumberCCX3000.ToString().PadLeft(3, '0') + ".idx||" +
                                    //DateTime.Now.ToString("yyyyMMdd hh:mm:ss") + "||" +
                                    //PDFCountDetails.ToString() + "||" + PDFCountDetails.ToString() + "||||||||" + Environment.NewLine;
                                    //File.AppendAllText(Path.Combine(pathforRRDAudit, CategoryType + "_L_P_107_" + DateTime.Now.ToString("yyyyMMdd") + "_" + indexFileInfo.AuditFileNumber.ToString().PadLeft(3, '0') + "_archive_v2_IDX_" + indexFileInfo.IndexInnerFileNumberCCX3000.ToString().PadLeft(3, '0') + ".adt"), auditContent);
                                    SavePDFCountValue(PDFCountCCX3000, ClientAppType.CCX3000.ToString());
                                    int innerNumber = Convert.ToInt32(indexFileInfo.IndexInnerFileNumberCCX3000);
                                    innerNumber++;
                                    if (innerNumber > (Convert.ToInt32(PDFCountDetailsMax) / PDFCountDetails))
                                    {
                                        try
                                        {
                                            ZipFile.CreateFromDirectory(Path.Combine(FolderRRDPath, zipFolderName), Path.Combine(FolderRRDPath, zipFolderName) + ".zip");
                                            //EncryptFile(Path.Combine(FolderRRDPath, zipFolderName) + ".zip", Path.Combine(FolderRRDPath, zipFolderName) + ".zip");
                                        }
                                        catch (Exception ex)
                                        {
                                            Foundation.LogDetail("CCX3000 : Error Creating the Zip File Exception : " + ex.Message, "Program", "BindDataBasedOnType", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                                        }
                                        var ExecutablePath1 = System.IO.Path.GetDirectoryName(ConfigurationManager.AppSettings["ApplicationExecutablePath"].ToString());
                                        string IndexInfoconfigPath1 = Path.Combine(ExecutablePath1, "IndexDetails.json");
                                        int auditFileNumber = Convert.ToInt32(indexFileInfo.AuditFileNumber);
                                        auditFileNumber++;
                                        indexFileInfo.AuditFileNumber = auditFileNumber.ToString();
                                        var result1 = JsonConvert.SerializeObject(indexFileInfo);
                                        File.WriteAllText(IndexInfoconfigPath1, result1);
                                        var indexDetails = JsonConvert.DeserializeObject<IndexFileDetails>(File.ReadAllText(IndexInfoconfigPath1));
                                        indexFileInfo = indexDetails;
                                        zipFolderName = ZipFolderName();
                                        pathforRRDIndex = Path.Combine(FolderRRDPath, zipFolderName); FolderCreation(pathforRRDIndex);
                                        pathforRRDPDF = Path.Combine(FolderRRDPath, zipFolderName); FolderCreation(pathforRRDPDF);
                                        pathforRRDAudit = Path.Combine(FolderRRDPath, zipFolderName); FolderCreation(pathforRRDAudit);
                                        var ExecutablePath = System.IO.Path.GetDirectoryName(ConfigurationManager.AppSettings["ApplicationExecutablePath"].ToString());
                                        string IndexInfoconfigPath = Path.Combine(ExecutablePath, "IndexDetails.json");
                                        indexFileInfo.IndexInnerFileNumberCCX3000 = "1";
                                        var result = JsonConvert.SerializeObject(indexFileInfo);
                                        File.WriteAllText(IndexInfoconfigPath, result);
                                        bool isFileCopied = contentAndWrite(dt.Rows[i], pathforRRDIndex, indexFileInfo.IndexInnerFileNumberCCX3000, CategoryType, pathforRRDPDF, PDFCountCCX3000, pathforRRDAudit, false);
                                        if (!isFileCopied)
                                        {
                                            Console.WriteLine("Error in copying files");
                                            System.Environment.Exit(0);
                                        }
                                        PDFCountCCX3000++;
                                        Foundation.LogDetail("CCX3000 : End ", "Program", "BindDataBasedOnType", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                                    }
                                    else
                                    {
                                        var ExecutablePath = System.IO.Path.GetDirectoryName(ConfigurationManager.AppSettings["ApplicationExecutablePath"].ToString());
                                        string IndexInfoconfigPath = Path.Combine(ExecutablePath, "IndexDetails.json");
                                        indexFileInfo.IndexInnerFileNumberCCX3000 = innerNumber.ToString();
                                        var result = JsonConvert.SerializeObject(indexFileInfo);
                                        File.WriteAllText(IndexInfoconfigPath, result);
                                        bool isFileCopied = contentAndWrite(dt.Rows[i], pathforRRDIndex, indexFileInfo.IndexInnerFileNumberCCX3000, CategoryType, pathforRRDPDF, PDFCountCCX3000, pathforRRDAudit, false);
                                        if (!isFileCopied)
                                        {
                                            Console.WriteLine("Error in copying files");
                                            System.Environment.Exit(0);
                                        }
                                        PDFCountCCX3000++;
                                        Foundation.LogDetail("CCX3000 : End ", "Program", "BindDataBasedOnType", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Foundation.LogDetail("CCX3000 : Error while doing logics : " + ex.Message, "Program", "BindDataBasedOnType", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                            }
                        }

                        #endregion

                        #region CCX2000

                        if (CategoryType == ClientAppType.CCX2000.ToString())
                        {
                            try
                            {
                                Foundation.LogDetail("CCX2000 : Started", "Program", "BindDataBasedOnType", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                                SavePDFCountValue(PDFCountCCX2000, ClientAppType.CCX2000.ToString());
                                string zipFolderName = ZipFolderName();
                                Foundation.LogDetail("CCX2000 : Zip Folder Name : " + zipFolderName, "Program", "BindDataBasedOnType", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                                string pathforRRDIndex = Path.Combine(FolderRRDPath, zipFolderName); FolderCreation(pathforRRDIndex);
                                string pathforRRDPDF = Path.Combine(FolderRRDPath, zipFolderName); FolderCreation(pathforRRDPDF);
                                string pathforRRDAudit = Path.Combine(FolderRRDPath, zipFolderName); FolderCreation(pathforRRDAudit);
                                if (PDFCountCCX2000 <= PDFCountDetails)
                                {
                                    bool isOldRecord = false;
                                    if (i != 0)
                                    {
                                        if (Convert.ToInt64(dt.Rows[i - 1]["LetterComponentId"].ToString()) == Convert.ToInt64(dt.Rows[i]["LetterComponentId"].ToString()))
                                            isOldRecord = true;
                                    }
                                    bool isFileCopied = contentAndWrite(dt.Rows[i], pathforRRDIndex, indexFileInfo.IndexInnerFileNumberCCX2000, CategoryType, pathforRRDPDF, PDFCountCCX2000, pathforRRDAudit, isOldRecord);
                                    if (!isFileCopied)
                                    {
                                        Console.WriteLine("Error in copying files");
                                        System.Environment.Exit(0);
                                    }
                                    if (!isOldRecord)
                                        PDFCountCCX2000++;
                                    Foundation.LogDetail("CCX2000 : End ", "Program", "BindDataBasedOnType", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                                }
                                else
                                {
                                    PDFCountCCX2000 = 1;
                                    //string auditContent = CategoryType + "_L_P_107_" + DateTime.Now.ToString("yyyyMMdd") + "_" + indexFileInfo.AuditFileNumber.PadLeft(3, '0') + "_archive_v2_IDX_" + indexFileInfo.IndexInnerFileNumberCCX2000.ToString().PadLeft(3, '0') + ".idx||" +
                                    //DateTime.Now.ToString("yyyyMMdd hh:mm:ss") + "||" +
                                    //PDFCountDetails.ToString() + "||" + PDFCountDetails.ToString() + "||||||||" + Environment.NewLine;
                                    //File.AppendAllText(Path.Combine(pathforRRDAudit, CategoryType + "_L_P_107_" + DateTime.Now.ToString("yyyyMMdd") + "_" + indexFileInfo.AuditFileNumber.ToString().PadLeft(3, '0') + "_archive_v2_IDX_" + indexFileInfo.IndexInnerFileNumberCCX2000.ToString().PadLeft(3, '0') + ".adt"), auditContent);
                                    SavePDFCountValue(PDFCountCCX2000, ClientAppType.CCX2000.ToString());
                                    int innerNumber = Convert.ToInt32(indexFileInfo.IndexInnerFileNumberCCX2000);
                                    innerNumber++;
                                    if (innerNumber > (Convert.ToInt32(PDFCountDetailsMax) / PDFCountDetails))
                                    {
                                        try
                                        {
                                            ZipFile.CreateFromDirectory(Path.Combine(FolderRRDPath, zipFolderName), Path.Combine(FolderRRDPath, zipFolderName) + ".zip");
                                            //EncryptFile(Path.Combine(FolderRRDPath, zipFolderName) + ".zip", Path.Combine(FolderRRDPath, zipFolderName) + ".zip");
                                        }
                                        catch (Exception ex)
                                        {
                                            Foundation.LogDetail("CCX2000 : Error Creating the Zip File Exception : " + ex.Message, "Program", "BindDataBasedOnType", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                                        }
                                        var ExecutablePath1 = System.IO.Path.GetDirectoryName(ConfigurationManager.AppSettings["ApplicationExecutablePath"].ToString());
                                        string IndexInfoconfigPath1 = Path.Combine(ExecutablePath1, "IndexDetails.json");
                                        int auditFileNumber = Convert.ToInt32(indexFileInfo.AuditFileNumber);
                                        auditFileNumber++;
                                        indexFileInfo.AuditFileNumber = auditFileNumber.ToString();
                                        var result1 = JsonConvert.SerializeObject(indexFileInfo);
                                        File.WriteAllText(IndexInfoconfigPath1, result1);
                                        var indexDetails = JsonConvert.DeserializeObject<IndexFileDetails>(File.ReadAllText(IndexInfoconfigPath1));
                                        indexFileInfo = indexDetails;
                                        zipFolderName = ZipFolderName();
                                        pathforRRDIndex = Path.Combine(FolderRRDPath, zipFolderName); FolderCreation(pathforRRDIndex);
                                        pathforRRDPDF = Path.Combine(FolderRRDPath, zipFolderName); FolderCreation(pathforRRDPDF);
                                        pathforRRDAudit = Path.Combine(FolderRRDPath, zipFolderName); FolderCreation(pathforRRDAudit);
                                        var ExecutablePath = System.IO.Path.GetDirectoryName(ConfigurationManager.AppSettings["ApplicationExecutablePath"].ToString());
                                        string IndexInfoconfigPath = Path.Combine(ExecutablePath, "IndexDetails.json");
                                        indexFileInfo.IndexInnerFileNumberCCX2000 = "1";
                                        var result = JsonConvert.SerializeObject(indexFileInfo);
                                        File.WriteAllText(IndexInfoconfigPath, result);
                                        bool isFileCopied = contentAndWrite(dt.Rows[i], pathforRRDIndex, indexFileInfo.IndexInnerFileNumberCCX2000, CategoryType, pathforRRDPDF, PDFCountCCX2000, pathforRRDAudit, false);
                                        if (!isFileCopied)
                                        {
                                            Console.WriteLine("Error in copying files");
                                            System.Environment.Exit(0);
                                        }
                                        PDFCountCCX2000++;
                                        Foundation.LogDetail("CCX2000 : End ", "Program", "BindDataBasedOnType", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                                    }
                                    else
                                    {
                                        var ExecutablePath = System.IO.Path.GetDirectoryName(ConfigurationManager.AppSettings["ApplicationExecutablePath"].ToString());
                                        string IndexInfoconfigPath = Path.Combine(ExecutablePath, "IndexDetails.json");
                                        indexFileInfo.IndexInnerFileNumberCCX2000 = innerNumber.ToString();
                                        var result = JsonConvert.SerializeObject(indexFileInfo);
                                        File.WriteAllText(IndexInfoconfigPath, result);
                                        bool isFileCopied = contentAndWrite(dt.Rows[i], pathforRRDIndex, indexFileInfo.IndexInnerFileNumberCCX2000, CategoryType, pathforRRDPDF, PDFCountCCX2000, pathforRRDAudit, false);
                                        if (!isFileCopied)
                                        {
                                            Console.WriteLine("Error in copying files");
                                            System.Environment.Exit(0);
                                        }
                                        PDFCountCCX2000++;
                                        Foundation.LogDetail("CCX2000 : End ", "Program", "BindDataBasedOnType", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Foundation.LogDetail("CCX2000 : Error while doing logics : " + ex.Message, "Program", "BindDataBasedOnType", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                            }
                        }

                        #endregion

                        #region CCX1000

                        if (CategoryType == ClientAppType.CCX1000.ToString())
                        {
                            try
                            {
                                Foundation.LogDetail("CCX1000 : Started", "Program", "BindDataBasedOnType", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                                SavePDFCountValue(PDFCountCCX1000, ClientAppType.CCX1000.ToString());
                                string zipFolderName = ZipFolderName();
                                Foundation.LogDetail("CCX1000 : Zip Folder Name : " + zipFolderName, "Program", "BindDataBasedOnType", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                                string pathforRRDIndex = Path.Combine(FolderRRDPath, zipFolderName); FolderCreation(pathforRRDIndex);
                                string pathforRRDPDF = Path.Combine(FolderRRDPath, zipFolderName); FolderCreation(pathforRRDPDF);
                                string pathforRRDAudit = Path.Combine(FolderRRDPath, zipFolderName); FolderCreation(pathforRRDAudit);
                                if (PDFCountCCX1000 <= PDFCountDetails)
                                {
                                    bool isOldRecord = false;
                                    if (i != 0)
                                    {
                                        if (Convert.ToInt64(dt.Rows[i - 1]["LetterComponentId"].ToString()) == Convert.ToInt64(dt.Rows[i]["LetterComponentId"].ToString()))
                                            isOldRecord = true;
                                    }
                                    bool isFileCopied = contentAndWrite(dt.Rows[i], pathforRRDIndex, indexFileInfo.IndexInnerFileNumberCCX1000, CategoryType, pathforRRDPDF, PDFCountCCX1000, pathforRRDAudit, isOldRecord);
                                    if (!isFileCopied)
                                    {
                                        Console.WriteLine("Error in copying files");
                                        System.Environment.Exit(0);
                                    }
                                    if (!isOldRecord)
                                        PDFCountCCX1000++;
                                    Foundation.LogDetail("CCX1000 : End ", "Program", "BindDataBasedOnType", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                                }
                                else
                                {
                                    PDFCountCCX1000 = 1;
                                    //string auditContent = CategoryType + "_L_P_107_" + DateTime.Now.ToString("yyyyMMdd") + "_" + indexFileInfo.AuditFileNumber.PadLeft(3, '0') + "_archive_v2_IDX_" + indexFileInfo.IndexInnerFileNumberCCX1000.ToString().PadLeft(3, '0') + ".idx||" +
                                    //DateTime.Now.ToString("yyyyMMdd hh:mm:ss") + "||" +
                                    //PDFCountDetails.ToString() + "||" + PDFCountDetails.ToString() + "||||||||" + Environment.NewLine;
                                    //File.AppendAllText(Path.Combine(pathforRRDAudit, CategoryType + "_L_P_107_" + DateTime.Now.ToString("yyyyMMdd") + "_" + indexFileInfo.AuditFileNumber.ToString().PadLeft(3, '0') + "_archive_v2_IDX_" + indexFileInfo.IndexInnerFileNumberCCX1000.ToString().PadLeft(3, '0') + ".adt"), auditContent);
                                    SavePDFCountValue(PDFCountCCX1000, ClientAppType.CCX1000.ToString());
                                    int innerNumber = Convert.ToInt32(indexFileInfo.IndexInnerFileNumberCCX1000);
                                    innerNumber++;
                                    if (innerNumber > (Convert.ToInt32(PDFCountDetailsMax) / PDFCountDetails))
                                    {
                                        try
                                        {
                                            ZipFile.CreateFromDirectory(Path.Combine(FolderRRDPath, zipFolderName), Path.Combine(FolderRRDPath, zipFolderName) + ".zip");
                                            //EncryptFile(Path.Combine(FolderRRDPath, zipFolderName) + ".zip", Path.Combine(FolderRRDPath, zipFolderName) + ".zip");
                                        }
                                        catch (Exception ex)
                                        {
                                            Foundation.LogDetail("CCX1000 : Error Creating the Zip File Exception : " + ex.Message, "Program", "BindDataBasedOnType", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                                        }
                                        var ExecutablePath1 = System.IO.Path.GetDirectoryName(ConfigurationManager.AppSettings["ApplicationExecutablePath"].ToString());
                                        string IndexInfoconfigPath1 = Path.Combine(ExecutablePath1, "IndexDetails.json");
                                        int auditFileNumber = Convert.ToInt32(indexFileInfo.AuditFileNumber);
                                        auditFileNumber++;
                                        indexFileInfo.AuditFileNumber = auditFileNumber.ToString();
                                        var result1 = JsonConvert.SerializeObject(indexFileInfo);
                                        File.WriteAllText(IndexInfoconfigPath1, result1);
                                        var indexDetails = JsonConvert.DeserializeObject<IndexFileDetails>(File.ReadAllText(IndexInfoconfigPath1));
                                        indexFileInfo = indexDetails;
                                        zipFolderName = ZipFolderName();
                                        pathforRRDIndex = Path.Combine(FolderRRDPath, zipFolderName); FolderCreation(pathforRRDIndex);
                                        pathforRRDPDF = Path.Combine(FolderRRDPath, zipFolderName); FolderCreation(pathforRRDPDF);
                                        pathforRRDAudit = Path.Combine(FolderRRDPath, zipFolderName); FolderCreation(pathforRRDAudit);
                                        var ExecutablePath = System.IO.Path.GetDirectoryName(ConfigurationManager.AppSettings["ApplicationExecutablePath"].ToString());
                                        string IndexInfoconfigPath = Path.Combine(ExecutablePath, "IndexDetails.json");
                                        indexFileInfo.IndexInnerFileNumberCCX1000 = "1";
                                        var result = JsonConvert.SerializeObject(indexFileInfo);
                                        File.WriteAllText(IndexInfoconfigPath, result);
                                        bool isFileCopied = contentAndWrite(dt.Rows[i], pathforRRDIndex, indexFileInfo.IndexInnerFileNumberCCX1000, CategoryType, pathforRRDPDF, PDFCountCCX1000, pathforRRDAudit, false);
                                        if (!isFileCopied)
                                        {
                                            Console.WriteLine("Error in copying files");
                                            System.Environment.Exit(0);
                                        }
                                        PDFCountCCX1000++;
                                        Foundation.LogDetail("CCX1000 : End ", "Program", "BindDataBasedOnType", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                                    }
                                    else
                                    {
                                        var ExecutablePath = System.IO.Path.GetDirectoryName(ConfigurationManager.AppSettings["ApplicationExecutablePath"].ToString());
                                        string IndexInfoconfigPath = Path.Combine(ExecutablePath, "IndexDetails.json");
                                        indexFileInfo.IndexInnerFileNumberCCX1000 = innerNumber.ToString();
                                        var result = JsonConvert.SerializeObject(indexFileInfo);
                                        File.WriteAllText(IndexInfoconfigPath, result);
                                        bool isFileCopied = contentAndWrite(dt.Rows[i], pathforRRDIndex, indexFileInfo.IndexInnerFileNumberCCX1000, CategoryType, pathforRRDPDF, PDFCountCCX1000, pathforRRDAudit, false);
                                        if (!isFileCopied)
                                        {
                                            Console.WriteLine("Error in copying files");
                                            System.Environment.Exit(0);
                                        }
                                        PDFCountCCX1000++;
                                        Foundation.LogDetail("CCX1000 : End ", "Program", "BindDataBasedOnType", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Foundation.LogDetail("CCX1000 : Error while doing logics : " + ex.Message, "Program", "BindDataBasedOnType", DateTime.Now.ToString("yyyyMMdd"), indexFileInfo.AuditFileNumber.PadLeft(3, '0'));
                            }
                        }

                        #endregion
                    }
                    // For Sample PDF File Generation only
                    //if (Convert.ToInt32(indexFileInfo.AuditFileNumber) <= Convert.ToInt32(ConfigurationManager.AppSettings["CountForTest"].ToString()))
                    BindDataBasedOnType();
                }
            }


        }

        public string ZipFolderName()
        {
            var ExecutablePath = System.IO.Path.GetDirectoryName(ConfigurationManager.AppSettings["ApplicationExecutablePath"].ToString());
            string IndexInfoconfigPath = Path.Combine(ExecutablePath, "IndexDetails.json");
            var indexDetails = JsonConvert.DeserializeObject<IndexFileDetails>(File.ReadAllText(IndexInfoconfigPath));
            string zipFolderName = "";
            if (Convert.ToDateTime(indexFileInfo.StartDate).ToString("yyyyMMdd") != DateTime.Now.ToString("yyyyMMdd"))
            {
                TimeSpan span = DateTime.Now.Subtract(Convert.ToDateTime(indexFileInfo.StartDate));
                int indexNumber = Convert.ToInt32(indexFileInfo.IndexFileNumber);
                indexNumber = indexNumber + Convert.ToInt32(Math.Abs(span.TotalDays));
                indexFileInfo.AuditFileNumber = "1";
                indexFileInfo.StartDate = DateTime.Now.ToString("MM/dd/yyyy");
                var result = JsonConvert.SerializeObject(indexFileInfo);
                File.WriteAllText(IndexInfoconfigPath, result);
                indexDetails = JsonConvert.DeserializeObject<IndexFileDetails>(File.ReadAllText(IndexInfoconfigPath));
                indexFileInfo = indexDetails;
                zipFolderName = CategoryType + "_L_P_" + DateTime.Now.ToString("yyyyMMdd") + "_" + indexFileInfo.AuditFileNumber.ToString().PadLeft(3, '0') + "_archive_v2";
                Foundation.LogDetail("Index Number based on date", "Program", "Program", "", "");
            }
            else
            {
                zipFolderName = CategoryType + "_L_P_" + DateTime.Now.ToString("yyyyMMdd") + "_" + indexFileInfo.AuditFileNumber.ToString().PadLeft(3, '0') + "_archive_v2";
            }

            return zipFolderName;
        }

        public void FolderCreation(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Foundation.LogDetail("Path Created : " + path, "Program", "FolderCreation", "", "");
            }
        }

        public void EncryptFile(string file, string fileEncrypted)
        {
            string password = "abcd1234";

            byte[] bytesToBeEncrypted = File.ReadAllBytes(file);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            // Hash the password with SHA256
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            byte[] bytesEncrypted = AES_Encrypt(bytesToBeEncrypted, passwordBytes);

            File.WriteAllBytes(fileEncrypted, bytesEncrypted);
        }

        public void DecryptFile(string fileEncrypted, string file)
        {
            string password = "abcd1234";

            byte[] bytesToBeDecrypted = File.ReadAllBytes(fileEncrypted);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            byte[] bytesDecrypted = AES_Decrypt(bytesToBeDecrypted, passwordBytes);

            File.WriteAllBytes(file, bytesDecrypted);
        }

        public byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }

        public byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }
                    decryptedBytes = ms.ToArray();
                }
            }

            return decryptedBytes;
        }
    }
}
