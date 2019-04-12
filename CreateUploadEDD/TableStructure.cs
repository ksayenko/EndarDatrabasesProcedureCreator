using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Collections;

namespace CreateUploadEDD
{

    class ColumnStructure : IComparable
    {
        public string TableName = "";

        public string ColumnName = "";
        public bool BColumnNullable = false;
        public string ColumnType = "nvarchar";
        public int IColumnLen = 0;

        public int OrdinalPosition = -1;

        public ColumnStructure(string tn, string cn, string ct, bool isnull, int len, int pos)
        {
            SetupConstructor(tn, cn, ct, isnull, len, pos);
        }

        public ColumnStructure()
            : this("", "", "", true, 0, 0)
        {
        }

        public ColumnStructure(string tn, string cn, string ct, string isnull, string slen, string spos)
        {
            int len = -1;
            int pos = -1;
            if (slen != DBNull.Value.ToString())
                Int32.TryParse(slen, out len);

            Int32.TryParse(spos, out pos);
            if (isnull.ToUpper().Contains("YES"))
            {
                SetupConstructor(tn, cn, ct, true, len, pos);
            }
            else
                SetupConstructor(tn, cn, ct, false, len, pos);

        }

        public ColumnStructure(string tn, string cn, string ct, string isnull, int len, int pos)
        {
            if (isnull.ToUpper().Contains("YES"))
            {
                SetupConstructor(tn, cn, ct, true, len, pos);
            }
            else
                SetupConstructor(tn, cn, ct, false, len, pos);

        }

        public void SetupConstructor(string tn, string cn, string ct, bool isnull, int len, int pos)
        {
            TableName = tn;
            ColumnName = cn;
            if (cn.Contains(" ")
                || cn.ToLower().Contains("description")
                || cn.ToLower().Contains("rank")
                || cn.ToLower().Contains("days")
                || cn.ToLower().Contains("replicate")
                || cn.ToLower().Contains("start_date")
               )
                ColumnName = "[" + cn + "]";
            BColumnNullable = isnull;
            ColumnType = ct;
            IColumnLen = len;
            OrdinalPosition = pos;


        }

        private string ColumnTypeCreate()
        {
            string sql = "";
            if (ColumnType.ToLower().Contains("char"))
                sql = ColumnType + " (" + IColumnLen.ToString() + ")";
            else
                sql = ColumnType;
            return sql;
        }

        public string For_CreateTable()
        {
            return For_CreateTable(this.BColumnNullable);
        }


        public string For_CreateTable(bool bNullable)
        {
            string sql = ", " + ColumnName + " " + ColumnTypeCreate() + " ";

            if (!bNullable)
                sql += " NOT NULL ";
            return sql;
        }



        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            ColumnStructure ct = obj as ColumnStructure;
            if (ct != null)
                return this.OrdinalPosition.CompareTo(ct.OrdinalPosition);
            else
                throw new ArgumentException("Object is not a ColumnStructure");
        }

    }

    //---------------TableStructure ----------------------//
    class TableStructure
    {
        public string TableName = "";

        private string uploadTN = "";
        private string uploadArchiveTN = "";
        private string uploadCheckTN = "";
        private string tranName = "";

        public DataTable dtStructure;
        public DataTable dtSP;
        DataAccess da;

        ArrayList Columns;

        string Spacessmall = "  ";
        string Spaces = "     ";


        public TableStructure(DataAccess daa) : this(daa, "", null) { }

        public TableStructure(DataAccess daa, string tn, DataTable tablestructure)
        {
            TableName = tn;
            da = daa;
            dtSP = da.GetExistentSP("edd_upload_" + TableName);
            if (tablestructure == null)
                if (TableName != "")
                {
                    dtStructure = da.Edd_Process_Columns(TableName);
                }
                else
                { dtStructure = null; }
            else
            { dtStructure = tablestructure; }

            uploadTN = "UPLOAD_" + TableName;
            uploadArchiveTN = uploadTN + "_ARCHIVE";
            uploadCheckTN = uploadTN + "_CHECK";
            tranName = "U" + TableName.ToLower();
            if (tranName.Length > 30)
                tranName = tranName.Remove(29);

            CreateDBStructure();
        }

        public void CreateDBStructure()
        {
            Columns = new ArrayList();
            /*
            string sql = "SELECT ";
            sql += " COL.TABLE_NAME AS [PROD_TABLE]"; 0
            sql += ", COL.COLUMN_NAME AS [PROD_COLUMN]";1
            sql += ", COL.IS_NULLABLE AS [PROD_COL_NULLABLE]";2
            sql += ", COL.DATA_TYPE AS [PROD_COL_TYPE]";3
            sql += ", COL.CHARACTER_MAXIMUM_LENGTH AS [PROD_COL_LEN]";4
            sql += ", COL.ORDINAL_POSITION as [POSITION]";5
             * */

            if (dtStructure == null)
                return;
            string tcn = "", tct = "", tcln = "", tcpos = "", tisnull = "";
            foreach (DataRow dr in dtStructure.Rows)
            {
                tcn = dr[1].ToString();
                tct = dr[3].ToString();
                tcln = dr[4].ToString();
                tisnull = dr[2].ToString();
                tcpos = dr[5].ToString();


                ColumnStructure col = new ColumnStructure(this.TableName, tcn, tct, tisnull, tcln, tcpos);
                Columns.Add(col);
            }

            Columns.Sort();
        }

        public ArrayList GetCreateUploadRoutine()
        {
            ArrayList edd = new ArrayList();

            EDD_CreateProcHeader(edd);

            EDD_InitialDeclare(edd);

            EDD_DataFromUploadRequest(edd);

            EDD_CreateTempTable(edd);

            EDD_DataFromUploadRequest_Filename(edd);

            EDD_RowsFromUploadFile(edd);

            EDD_Insert_FileUpload(edd);

            EDD_InsertTemp_FromUploadFile(edd);

            //EDD_CheckIsAllInserts(edd);
            EDD_UploadMode(edd);

            EDD_Open_Trans(edd);

            EDD_LockUploadTable(edd);

            EDD_RunBulk_InsUpdDel(edd);

            EDD_DeclareCursor(edd);

            EDD_CursorPart(edd);

            EDD_Close_Trans(edd);

            //EDD_InsertToArchive(edd);
            EDD_Archive(edd);

            EDD_InsertToUploadResponse(edd);

            EDD_WriteToErrorLog(edd);

            edd.Add("END");
            edd.Add("return @status");
            edd.Add("GO");



            return edd;
        }
        
        private void EDD_WriteToErrorLog(ArrayList edd)
        {
            edd.Add("END TRY");

            edd.Add("BEGIN CATCH        ");
            edd.Add("INSERT INTO ERROR_LOG(ERROR_DATE, ERROR_LOG_SOURCE,ERROR_DATA) ");
            edd.Add("VALUES (dbo.UFN_PST_DATE(),OBJECT_NAME(@@PROCID),ERROR_MESSAGE()+' Line : ' + cast(ERROR_LINE() as VARCHAR(40))) ");
            edd.Add("END CATCH      ");

            edd.Add("");
        }


        private void EDD_Insert_FileUpload(ArrayList edd)
        {
            edd.Add(Spacessmall + "INSERT INTO FILE_UPLOAD (FILE_NAME, PROCESS, UPLOAD_DATE, UPLOADER_NAME, BATCH_ID,TABLE_NAME,SERVER_FILE_PATH)  ");
            edd.Add(Spacessmall + "VALUES (@file_name, OBJECT_NAME(@@PROCID), dbo.UFN_PST_DATE(),@uploader_name, @batch_id,'" + TableName + "',@fpath)");
            edd.Add(Spacessmall + "SELECT @filesourceid = SOURCE_ID FROM  FILE_UPLOAD WHERE BATCH_ID = @batch_id  ");
            edd.Add("");
        }


        private void EDD_InsertTemp_FromUploadFile(ArrayList edd)
        {
            edd.Add(Spacessmall + "INSERT INTO  @temp (ROW_INDEX");
            foreach (ColumnStructure c in Columns)
                edd.Add(Spaces + ", " + c.ColumnName);
            edd.Add(Spaces + ", COMMAND)");
            edd.Add(Spacessmall + "SELECT  ROW_INDEX ");
            foreach (ColumnStructure c in Columns)
                edd.Add(Spaces + ", " + c.ColumnName);
            edd.Add(Spaces + ", _COMMAND FROM " + uploadTN); ;
            edd.Add(Spacessmall + "WHERE _BATCH_ID = @batch_id");
            edd.Add("");
        }

        private void EDD_RowsFromUploadFile(ArrayList edd)
        {
            edd.Add(Spacessmall + "SELECT @nRows = COUNT(*)");
            edd.Add(Spacessmall + "FROM " + uploadTN + " WHERE _BATCH_ID = @batch_id");
            edd.Add("");

            edd.Add(Spacessmall + "IF @nRows =0 --");
            edd.Add(Spacessmall + "begin");
            edd.Add(Spaces + "insert into upload_response  ");
            edd.Add(Spaces + "([EDD_NAME],[UPLOADER_NAME],   ");
            edd.Add(Spaces + "[FILE_NAME],[BATCH_ID],  ");
            edd.Add(Spaces + "[EMAIL_TO],[EMAIL_TO_CC],     ");
            edd.Add(Spaces + "[UPLOAD_STATUS],[RESPONSE_TEXT],[UPLOAD_MODE])  ");
            edd.Add(Spaces + "VALUES ('NO NAME', 'NO NAME',    ");
            edd.Add(Spaces + " @file_name, @batch_id,    ");
            edd.Add(Spaces + "@email_to, @email_to_cc,");
            edd.Add(Spaces + " 0, 'NO DATA FOR THIS BATCH',@upload_mode) ");
            edd.Add(Spaces + "RETURN 0   ");
            edd.Add(Spacessmall + "END   ");
            edd.Add("");


        }
        private void EDD_DataFromUploadRequest_Filename(ArrayList edd)
        {
            edd.Add(Spacessmall + "SELECT top(1)");
            edd.Add(Spacessmall + "@filename =  FILE_NAME,");
            edd.Add(Spacessmall + "@fpath =   SERVER_FILE_PATH");
            edd.Add(Spacessmall + "FROM upload_request WHERE BATCH_ID = @batch_id");
            edd.Add("");
        }

        private void EDD_CreateTempTable(ArrayList edd)
        {
            edd.Add("");
            edd.Add("DECLARE @temp table (");
            edd.Add(Spaces + "ROW_INDEX bigint");
            string t = "";
            foreach (ColumnStructure c in Columns)
            {
                t = Spaces + c.For_CreateTable(true);
                edd.Add(t);
            }
            t = Spaces + ", COMMAND nvarchar(15)";
            edd.Add(t);
            t = Spaces + ", __PROCESS_COMMENTS nvarchar(max))";
            edd.Add(t);
            edd.Add("");

        }

        private void EDD_DataFromUploadRequest(ArrayList edd)
        {
            edd.Add(Spacessmall + "SELECT top(1) @id_request = ID,      ");
            edd.Add(Spacessmall + "@edd_name = EDD_NAME,       ");
            edd.Add(Spacessmall + "@uploader_name = UPLOADER_NAME,       ");
            edd.Add(Spacessmall + "@file_name = [FILE_NAME] ,        ");
            edd.Add(Spacessmall + "@email_to = [EMAIL_TO],       ");
            edd.Add(Spacessmall + "@email_to_cc = [EMAIL_TO_CC]  , ");
            edd.Add(Spacessmall + "@edd_type = EDD_TYPE, ");
            edd.Add(Spacessmall + "@upload_mode = UPLOAD_MODE");
            edd.Add(Spacessmall + "FROM UPLOAD_REQUEST WHERE BATCH_ID = @batch_id order by id desc ");

            edd.Add("");
            edd.Add(Spacessmall+ "if  @edd_type is null  ");
            edd.Add(Spacessmall + "  set @edd_type ='" + TableName + "'");
            edd.Add("");
        }

        private void EDD_InitialDeclare(ArrayList edd)
        {
            edd.Add(Spacessmall + "DECLARE @uploader_name nvarchar(150), @edd_name nvarchar(150)  ");
            edd.Add(Spacessmall + "DECLARE @file_name nvarchar(255), @filesourceid int  ");
            edd.Add(Spacessmall + "DECLARE @email_to  nvarchar(255), @email_to_cc nvarchar(255)");
            edd.Add(Spacessmall + "DECLARE @id_request bigint");
            edd.Add(Spacessmall + "DECLARE @iErrorCount int, @maxErrorPrint int");
            edd.Add(Spacessmall + "SET @maxErrorPrint = 5  ");
            edd.Add(Spacessmall + "SET @iErrorCount = 0 ");
            edd.Add(Spacessmall + "DECLARE @edd_type nvarchar(20) ");
            edd.Add("");
            edd.Add(Spacessmall + "set  @edd_type ='na'");
            edd.Add("");
            edd.Add(Spacessmall + "DECLARE @processDate datetime");
            edd.Add(Spacessmall + "SET @processDate = dbo.UFN_PST_DATE()");
            edd.Add("");
            edd.Add(Spacessmall + "DECLARE @status bit ");
            edd.Add(Spacessmall + "SET @status = 1 ");
            edd.Add("");
            edd.Add(Spacessmall + "DECLARE @comment nvarchar(max), @comment_temp as VARCHAR(MAX)");
            edd.Add(Spacessmall + "SET @comment = ''");
            edd.Add("");
            edd.Add(Spacessmall + "DECLARE @command nvarchar(50) ");
            edd.Add("");
            edd.Add(Spacessmall + "DECLARE @filename nvarchar(150)   ");
            edd.Add(Spacessmall + "DECLARE @fpath NVARCHAR(500) ");
            edd.Add("");
            edd.Add(Spacessmall + "DECLARE @nRows int");
            edd.Add("");
            edd.Add(Spacessmall + "DECLARE @upload_mode nvarchar(15)");
            edd.Add(Spacessmall + "DECLARE @bCheck bit =0, @bBulk bit =0");
            edd.Add("");
            edd.Add("DECLARE @IdentityOutput table ( ID bigint, ROW_INDEX  bigint)");
            edd.Add("DECLARE @tempComment AS NVARCHAR(MAX)  ");
            edd.Add("DECLARE @Tablename NVARCHAR(50)  ");
            edd.Add("");

        }

        private void EDD_CreateProcHeader(ArrayList edd)
        {
            edd.Add("SET ANSI_NULLS ON");
            edd.Add("GO");
            edd.Add("");
            edd.Add("SET QUOTED_IDENTIFIER ON");
            edd.Add("GO");
            edd.Add("");
            edd.Add("");
            edd.Add(" --=============================================      ");
            edd.Add("-- Author:  Kateryna sayenko ");
            edd.Add("-- Create date: " + DateTime.Now.ToShortDateString());
            edd.Add(" --=============================================      ");
            edd.Add("CREATE PROCEDURE [edd_upload_" + TableName.ToLower() + "]" +
                " (@batch_id nvarchar(50),@bNeedTransaction bit = 1, @bBulkInsert bit = 0)    ");
            edd.Add("AS");
            edd.Add("BEGIN");
            edd.Add(Spacessmall + "SET NOCOUNT ON; ");
            edd.Add("");
            edd.Add(Spacessmall + "BEGIN TRY");
        }

        public ArrayList GetCreateTable()
        {
            ArrayList table = new ArrayList();
            string t = "";
            string Spaces = "     ";
            t = "CREATE TABLE " + this.TableName.ToUpper() + "(";
            table.Add(t);
            foreach (ColumnStructure c in Columns)
            {
                if (c.ColumnName.ToLower() != "id")
                    t = Spaces + c.For_CreateTable();
                else
                    t = Spaces + "ID bigint IDENTITY(1,1) NOT NULL";

                table.Add(t);
            }
            table.Add(Spaces + ",_LAST_UPDATE_SOURCE_FILE_ID bigint");
            table.Add(Spaces + ",_LAST_UPDATE_USER nvarchar(150)");
            table.Add(Spaces + ",_LAST_UPDATE_DATE datetime");
            table.Add(Spaces + ",_LAST_DT_OFFSET_GLOBAL datetimeoffset(7)");
            table.Add(Spaces + "CONSTRAINT PK_" + TableName + " PRIMARY KEY CLUSTERED ");
            table.Add(Spaces + "(	ID ASC))");


            return table;
        }

        public ArrayList GetExistSP()
        {
            ArrayList SP = new ArrayList();
            string next = "\r\n";
            if (dtSP != null)
                foreach (DataRow dr in dtSP.Rows)
                {
                    string l = "";

                    for (int i = 0; i < dtSP.Columns.Count; i++)
                    {
                        string l1 = dr[i].ToString();
                        if (l1 != "" && l1 != next)
                            l += l1;
                    }
                    if (l.EndsWith(next))
                        l = l.Remove(l.Length - next.Length);
                    //if (l != "")
                    SP.Add(l);
                }

            return SP;
        }

        public ArrayList GetRoutineUploadAll()
        {
            ArrayList edd = new ArrayList();
            edd.Add("CREATE PROCEDURE [dbo].edd_upload_" + TableName.ToLower() + "_all ");
            edd.Add("");
            edd.Add("AS  ");
            edd.Add("BEGIN ");
            edd.Add("-- SET NOCOUNT ON added to prevent extra result sets FROM  ");
            edd.Add("-- interfering with SELECT statements.  ");
            edd.Add("SET NOCOUNT ON;  ");

            edd.Add("BEGIN TRY --   ");

            edd.Add("DECLARE @batch_id nvarchar(50)  ");

            edd.Add("DECLARE cBATCH CURSOR FOR   ");
            edd.Add("SELECT DISTINCT _BATCH_ID FROM UPLOAD_" + TableName);

            edd.Add("OPEN cBATCH  ");
            edd.Add("FETCH NEXT FROM cBATCH INTO @batch_id;  ");

            edd.Add("WHILE @@FETCH_STATUS = 0  ");
            edd.Add("BEGIN  ");

            edd.Add("execute edd_upload_" + TableName.ToLower() + " @batch_id = @batch_id ");

            edd.Add("FETCH NEXT FROM cBATCH INTO @batch_id;  ");
            edd.Add("END  ");
            edd.Add("CLOSE cBATCH;  ");
            edd.Add("DEALLOCATE cBATCH; ");
            edd.Add(" ");
            edd.Add("END TRY  ");
            edd.Add("BEGIN CATCH  ");
            edd.Add("INSERT INTO ERROR_LOG(ERROR_DATE, ERROR_LOG_SOURCE,ERROR_DATA)  ");
            edd.Add("VALUES (dbo.UFN_PST_DATE(),OBJECT_NAME(@@PROCID),ERROR_MESSAGE()+' Line : ' + cast(ERROR_LINE() as VARCHAR(40)))  ");

            edd.Add("exec edd_upload_moving_all_from_upload_to_archive  ");
            edd.Add("@table_name = '" + TableName + "',  ");
            edd.Add("@processed_comment = 'Unprocessed',  ");
            edd.Add("@file_processed = 0   ");

            edd.Add("delete from UPLOAD_" + TableName);
            edd.Add("END CATCH  ");
            edd.Add(" ");
            edd.Add("END");
            edd.Add("");
            edd.Add("GO");

            return edd;
        }

        public ArrayList GetLastUpdateTrigger()
        {
            ArrayList tr = new ArrayList();
            tr.Add(Spacessmall);
            tr.Add("SET ANSI_NULLS ON");
            tr.Add("GO");
            tr.Add("");
            tr.Add("SET QUOTED_IDENTIFIER ON");
            tr.Add("GO");
            tr.Add("-- =============================================  ");
            tr.Add("---- Author:  kateryna sayenko  ");
            tr.Add("-- Create date: " + DateTime.Now.ToShortDateString());
            tr.Add("-- =============================================  ");
            tr.Add("-- -- =============================================  ");
            tr.Add(Spacessmall + "CREATE trigger[dbo].[last_update_" + TableName.ToLower() + "]   ON [dbo]." + TableName + "       AFTER INSERT,UPDATE  ");
            tr.Add(Spacessmall + "AS  ");
            tr.Add("");

            tr.Add(Spacessmall + "DECLARE  @username nvarchar(100),   ");
            tr.Add(Spacessmall + "@updatedate datetime,");
            tr.Add(Spacessmall + "@updatedtoffset datetimeoffset  ");

            tr.Add(Spacessmall + "BEGIN  ");
            tr.Add(Spacessmall + "-- SET NOCOUNT ON added to pr noise extra result sets from  ");
            tr.Add(Spacessmall + " -- interfering with SELECT statements.  ");
            tr.Add(Spacessmall + " SET NOCOUNT ON;  ");

            tr.Add(Spacessmall + "set @username = SYSTEM_USER ");
            tr.Add(Spacessmall + "select  @updatedate =  dbo.UFN_PST_DATE();");
            tr.Add(Spacessmall + "select @updatedtoffset = SYSDATETIMEOFFSET();");

            tr.Add(Spacessmall + "update  " + TableName);
            tr.Add(Spacessmall + "set _LAST_UPDATE_DATE = @updatedate, ");
            tr.Add(Spacessmall + "	_LAST_UPDATE_USER = @username,");
            tr.Add(Spacessmall + "	_LAST_DT_OFFSET_GLOBAL = @updatedtoffset  ");
            tr.Add(Spacessmall + "from  " + TableName);
            tr.Add(Spacessmall + "inner join inserted on   ");
            tr.Add(Spacessmall + TableName + ".ID = inserted.ID  ");
            tr.Add(Spacessmall + " ");
            tr.Add(Spacessmall + "end");
            tr.Add(Spacessmall + "");
            tr.Add(Spacessmall + "GO");

            return tr;
        }

        public ArrayList GetCreateUploadTable()
        {
            ArrayList CreateUploadTable = new ArrayList();
            string t = "";
            string Spaces = "     ";


            t = "CREATE TABLE " + uploadTN.ToUpper() + "(";
            CreateUploadTable.Add(t);

            t = Spaces + "ROW_INDEX bigint IDENTITY(1,1) NOT NULL ";
            CreateUploadTable.Add(t);

            foreach (ColumnStructure c in Columns)
            {
                t = Spaces + c.For_CreateTable(true);
                CreateUploadTable.Add(t);
            }
            t = Spaces + ", _BATCH_ID nvarchar(50)";
            CreateUploadTable.Add(t);
            t = Spaces + ", _COMMAND nvarchar(15)";
            CreateUploadTable.Add(t);

            t = "CONSTRAINT PK_UPLOAD_" + TableName.ToUpper() + " PRIMARY KEY CLUSTERED (";
            CreateUploadTable.Add(t);

            t = "[ROW_INDEX] ASC)) ON [PRIMARY]";
            CreateUploadTable.Add(t);


            return CreateUploadTable;
        }

        public ArrayList GetCreateUploadCheckTable()
        {
            ArrayList CreateUploadCheckTable = new ArrayList();
            CreateUploadCheckTable = GetCreateUploadArchiveTable();

            if (CreateUploadCheckTable.Count > 0)
            {
                string t1 = (string)CreateUploadCheckTable[0];
                t1 = t1.Replace(uploadArchiveTN, uploadCheckTN);
                CreateUploadCheckTable[0] = t1;
            }

            return CreateUploadCheckTable;
        }


        public ArrayList GetCreateUploadArchiveTable()
        {
            ArrayList CreateUploadTable = new ArrayList();
            string t = "";
            string Spaces = "     ";


            t = "CREATE TABLE " + uploadArchiveTN.ToUpper() + " (";
            CreateUploadTable.Add(t);
            t = Spaces + "ROW_INDEX bigint";
            CreateUploadTable.Add(t);

            foreach (ColumnStructure c in Columns)
            {
                t = Spaces + c.For_CreateTable(true);
                CreateUploadTable.Add(t);
            }
            t = Spaces + ", _BATCH_ID nvarchar(50)";
            CreateUploadTable.Add(t);
            t = Spaces + ", _COMMAND nvarchar(15)";
            CreateUploadTable.Add(t);

            t = Spaces + ", __FILE_PROCESSED BIT";
            CreateUploadTable.Add(t);
            t = Spaces + ", __PROCESS_COMMENTS nvarchar(max))";
            CreateUploadTable.Add(t);


            return CreateUploadTable;
        }

        private void EDD_UploadMode(ArrayList edd)
        {
            edd.Add("");
            edd.Add(Spacessmall + "if @upload_mode like '%check%'");
            edd.Add(Spacessmall + "select @bcheck =1, @bBulk = 0");
            edd.Add("");
            edd.Add(Spacessmall + "if @upload_mode like '%Bulk%' or @bBulkInsert = 1");
            edd.Add(Spacessmall + "select @bcheck =0, @bBulk = 1");
        }

        private void EDD_CheckIsAllInserts(ArrayList edd)
        {
            edd.Add("");
            edd.Add("DECLARE @tempCount int");
            edd.Add(Spacessmall + "SELECT  @tempCount = count(*) from @temp where id is null and command is null or command like 'Ins%'");
            edd.Add(Spacessmall + "SELECT  @AllInserts = case when @nrows = @tempCount then 1 else 0 end ");
            edd.Add(Spacessmall + "from @temp ");
            edd.Add("");
        }
        private void EDD_a(ArrayList edd)
        {
            edd.Add("");
        }

        private void EDD_DeclareCursor(ArrayList edd)
        {
            edd.Add("");
            edd.Add(Spacessmall + "ELSE  --not (@bBulk =1)");
            edd.Add("");
            edd.Add(Spacessmall + "DECLARE Cur CURSOR for SELECT row_index, id, COMMAND FROM @temp   ");
            edd.Add(Spacessmall + "DECLARE @index bigint, @id bigint  ");
            edd.Add(Spacessmall + "SET @iErrorCount = 0    ");
            edd.Add("");
            edd.Add(Spacessmall + "OPEN Cur  ");

            
        }

        private void EDD_LockUploadTable(ArrayList edd)
        {
            edd.Add("-- Lock the UPLOAD table");
            edd.Add(Spacessmall + "SELECT * ");
            edd.Add(Spacessmall + "FROM " + uploadTN);
            edd.Add(Spacessmall + "WITH(TABLOCKX)");
            edd.Add(Spacessmall + "WHERE _BATCH_ID = @batch_id");
            edd.Add("");
        }

        private void EDD_CreateInsertUpdateDeleteRows_WithMerge(ArrayList edd)
        {
            edd.Add("");
            edd.Add(Spaces + "MERGE " + TableName + " as main_table ");
            edd.Add(Spaces + "USING @temp AS t");
            edd.Add(Spaces + "on t.Id = main_table.Id");
            edd.Add(Spaces + "WHEN MATCHED AND t.command like 'DEL%' THEN");
            edd.Add(Spaces + "  DELETE");
            edd.Add(Spaces + "WHEN MATCHED THEN");
            edd.Add(Spaces + "UPDATE ");
            edd.Add(Spaces + "Set ");

            EDD_CreateUpdateRowsColumn(edd, "t");

            edd.Add(Spaces + "WHEN NOT MATCHED THEN");
            edd.Add(Spaces + "INSERT (");
            foreach (ColumnStructure c in this.Columns)
            {
                if (c.ColumnName.ToLower() != "id")
                    edd.Add(Spacessmall + Spaces + c.ColumnName + ",");
            }
            edd.Add(Spacessmall + Spaces + "_LAST_UPDATE_SOURCE_FILE_ID )");
            edd.Add(Spaces + "VALUES	 (");
            foreach (ColumnStructure c in this.Columns)
            {
                if (c.ColumnName.ToLower() != "id")
                    edd.Add(Spacessmall + Spaces + c.ColumnName + ",");
            }
            edd.Add(Spacessmall + Spaces + "@filesourceid ");
            edd.Add("   )   OUTPUT inserted.ID, t.row_index into @IdentityOutput ;   ");
        }

        private void EDD_CreateInsertRows_WithMerge(ArrayList edd)
        {
            edd.Add("");
            edd.Add(Spaces + "MERGE " + TableName);
            edd.Add(Spaces + "USING @temp AS t");
            edd.Add(Spaces + "on 1=0");
            edd.Add(Spaces + "WHEN NOT MATCHED THEN");
            edd.Add(Spaces + "INSERT (");
            foreach (ColumnStructure c in this.Columns)
            {
                if (c.ColumnName.ToLower() != "id")
                    edd.Add(Spacessmall + Spaces + c.ColumnName + ",");
            }
            edd.Add(Spacessmall + Spaces + "_LAST_UPDATE_SOURCE_FILE_ID )");
            edd.Add(Spaces + "VALUES	 (");
            foreach (ColumnStructure c in this.Columns)
            {
                if (c.ColumnName.ToLower() != "id")
                    edd.Add(Spacessmall + Spaces + c.ColumnName + ",");
            }
            edd.Add(Spacessmall + Spaces + "@filesourceid ");
            edd.Add("   )   OUTPUT inserted.ID, t.row_index into @IdentityOutput ;   ");
        }

        private void EDD_CreateInsertRows(ArrayList edd)
        {
            edd.Add("");
            edd.Add(Spaces + "INSERT INTO " + TableName + "(");
            foreach (ColumnStructure c in this.Columns)
            {
                if (c.ColumnName.ToLower() != "id")
                    edd.Add(Spacessmall + Spaces + c.ColumnName + ",");
            }
            edd.Add(Spacessmall + Spaces + "_LAST_UPDATE_SOURCE_FILE_ID )");
            edd.Add(Spaces + "OUTPUT inserted.ID, @index into @IdentityOutput ");
            edd.Add(Spaces + "SELECT");
            foreach (ColumnStructure c in this.Columns)
            {
                if (c.ColumnName.ToLower() != "id")
                    edd.Add(Spacessmall + Spaces + c.ColumnName + ",");
            }
            edd.Add(Spacessmall + Spaces + "@filesourceid ");
            edd.Add(Spaces + "FROM @temp t");

        }

        private void EDD_CreateUpdateRows(ArrayList edd)
        {
            edd.Add("");
            edd.Add(Spaces + "UPDATE " + TableName + "");

            edd.Add(Spaces + "Set ");
            EDD_CreateUpdateRowsColumn(edd);
            edd.Add(Spaces + "FROM " + TableName + " u ");
            edd.Add(Spaces + " inner join @temp temp on u.id= temp.ID ");
            edd.Add(Spaces + "WHERE u.ID = @id");
        }

        private void EDD_CreateUpdateRowsColumn(ArrayList edd)
        {
            EDD_CreateUpdateRowsColumn(edd,"temp");
        }

        private void EDD_CreateUpdateRowsColumn(ArrayList edd, string fromTable)
        {

            edd.Add(Spaces + "_LAST_UPDATE_SOURCE_FILE_ID =@filesourceid ");
            foreach (ColumnStructure c in this.Columns)
            {
                if (c.ColumnName.ToLower() != "id")
                    edd.Add(Spaces + ", " + c.ColumnName + "= "+fromTable+"." + c.ColumnName);
            }
            
        }
        private void EDD_CreateDeleteRows(ArrayList edd)
        {
            edd.Add("");
            edd.Add(Spaces + "DELETE FROM " + TableName + "  WHERE ID = ISNULL(@id,-9999)  ");

        }

        private void EDD_RunBulk_InsUpdDel(ArrayList edd)
        {
            edd.Add("");
            edd.Add(Spacessmall + "if @bBulk = 1");
            edd.Add(Spacessmall + "BEGIN ");
            edd.Add(Spacessmall + "BEGIN TRY ");
            //EDD_CreateInsertRows_WithMerge(edd);
            this.EDD_CreateInsertUpdateDeleteRows_WithMerge(edd);
            edd.Add(Spacessmall + "END TRY");
            edd.Add(Spacessmall + "BEGIN CATCH -- 1");
            edd.Add(Spacessmall + "SET @tempComment =  ERROR_MESSAGE() ");
            edd.Add(Spacessmall + "SET @Tablename = UPPER(SUBSTRING(OBJECT_NAME(@@PROCID),12,LEN(OBJECT_NAME(@@PROCID))))  ");
            edd.Add(Spacessmall + "SELECT @tempComment  = dbo.ufn_parse_error_message(error_number(),ERROR_MESSAGE(),@Tablename,  '') ");
            edd.Add(Spacessmall + "SET @status=0  ");
            edd.Add(Spacessmall + "SET @iErrorCount = @iErrorCount+1");
            edd.Add(Spacessmall + "UPDATE @temp   ");
            edd.Add(Spacessmall + "SET __PROCESS_COMMENTS = @tempComment");
            edd.Add(Spacessmall + "END CATCH   -- 1");
            edd.Add(Spacessmall + "END ");

            edd.Add("");

            //edd.Add("Close Cur");
            //edd.Add( "DEALLOCATE CUR");
            //edd.Add("");
        }

        private void EDD_Open_Trans(ArrayList edd)
        {
            edd.Add("");
            edd.Add(Spacessmall + "if @bNeedTransaction = 1");
            edd.Add(Spacessmall + "BEGIN ");
            edd.Add(Spaces + "BEGIN TRANSACTION " + tranName);
            edd.Add(Spacessmall + "END  ");
            edd.Add("");
        }

        private void EDD_UpdArchiveIds(ArrayList edd)
        {
            edd.Add("");
            edd.Add(Spaces + Spacessmall + " UPDATE @TEMP set ID = i.ID ");
            edd.Add(Spaces + Spacessmall + " From @temp t inner join @IdentityOutput i on i.Row_index = t.Row_index ");
            edd.Add(Spaces + Spacessmall + " Where  t.Id is null");
            edd.Add("");
        }

        private void EDD_Close_Trans(ArrayList edd)
        {

            edd.Add(Spacessmall + "if @bNeedTransaction = 1    ");
            edd.Add(Spacessmall + "BEGIN ");
            edd.Add(Spaces + "IF @status = 0");
            edd.Add(Spaces + "BEGIN");

            edd.Add(Spaces + Spacessmall + "ROLLBACK TRANSACTION " + tranName);

            edd.Add(Spaces + Spacessmall + "PRINT 'rollback'");
            edd.Add(Spaces + "END");
            edd.Add(Spaces + "ELSE");
            edd.Add(Spaces + "BEGIN");
            edd.Add(Spaces + Spacessmall + "COMMIT TRANSACTION " + tranName);
            edd.Add(Spaces + Spacessmall + "-- update temp table with Ids");
            EDD_UpdArchiveIds(edd);

            edd.Add(Spaces + Spacessmall + "PRINT 'commit'");
            edd.Add(Spaces + "END");
            edd.Add(Spacessmall + "END");
            edd.Add("");

        }

        private void EDD_Archive(ArrayList edd)
        {
            edd.Add("");
            edd.Add("MOVE_TO_ARCHIVE:");
            edd.Add(Spacessmall + " if @bcheck = 0");
            edd.Add(Spacessmall + "begin");
            EDD_InsertToArchiveCheck(edd, true);
            edd.Add(Spacessmall + "end");
            edd.Add(Spacessmall + "else");
            edd.Add(Spacessmall + "begin");
            EDD_InsertToArchiveCheck(edd, false);
            edd.Add(Spacessmall + "end");

            edd.Add("");
            edd.Add("DELETE FROM " + uploadTN + " WHERE _BATCH_ID = @batch_id     ");
        }

        private void EDD_InsertToArchiveCheck(ArrayList edd, bool barchive)
        {
            edd.Add("");
            string tn = uploadArchiveTN;
            if (!barchive)
                tn = uploadCheckTN;
            edd.Add(Spacessmall + "INSERT INTO " + tn + " (ROW_INDEX ");
            foreach (ColumnStructure c in this.Columns)
                edd.Add(Spacessmall + ", " + c.ColumnName);
            edd.Add(Spacessmall + "	,_COMMAND,_BATCH_ID,__FILE_PROCESSED, __PROCESS_COMMENTS) ");
            edd.Add(Spacessmall + "SELECT ROW_INDEX ");
            foreach (ColumnStructure c in this.Columns)
                edd.Add(Spacessmall + ", " + c.ColumnName);
            edd.Add("");
            edd.Add(Spacessmall + "	,@command, @batch_id, @status, __PROCESS_COMMENTS FROM @temp ");           

        }

        private void EDD_InsertToArchive(ArrayList edd)
        {
            edd.Add("");
            edd.Add(Spacessmall + "INSERT INTO " + uploadArchiveTN + " (ROW_INDEX ");
            foreach (ColumnStructure c in this.Columns)
                edd.Add(Spacessmall + ", " + c.ColumnName);
            edd.Add(Spacessmall + "	,_COMMAND,_BATCH_ID,__FILE_PROCESSED, __PROCESS_COMMENTS) ");
            edd.Add(Spacessmall + "SELECT ROW_INDEX ");
            foreach (ColumnStructure c in this.Columns)
                edd.Add(Spacessmall + ", " + c.ColumnName);
            edd.Add("");
            edd.Add(Spacessmall + "	,@command, @batch_id, @status, __PROCESS_COMMENTS FROM @temp     ");
            edd.Add("");
            edd.Add("DELETE FROM " + uploadTN + " WHERE _BATCH_ID = @batch_id     ");

        }

        private void EDD_InsertToUploadResponse(ArrayList edd)
        {
            edd.Add("");
            edd.Add("BEGIN");
            edd.Add(Spacessmall + "INSERT INTO UPLOAD_RESPONSE(EDD_NAME, UPLOADER_NAME, FILE_NAME, BATCH_ID, EMAIL_TO, eMAIL_TO_CC, UPLOAD_STATUS, UPLOAD_MODE,RESPONSE_TEXT,RESPONSE_DATE)  ");
            edd.Add(Spacessmall + "SELECT EDD_NAME, UPLOADER_NAME, FILE_NAME, BATCH_ID, EMAIL_TO, eMAIL_TO_CC,@status,@upload_mode, @comment, ");
            edd.Add(Spacessmall + "@processDate FROM upload_request WHERE batch_id = @batch_id   ");
            edd.Add(Spacessmall + "Update UPLOAD_REQUEST SET REQUEST_PROCESSED=1 WHERE BATCH_ID = @batch_id");
            edd.Add("END");
            edd.Add("");
        }

        private void EDD_CursorPart(ArrayList edd)
        {
            //edd.Add(Spacessmall + "ELSE  --not (@bBulk =1)");
            edd.Add(Spacessmall + "BEGIN  ");
            edd.Add(Spacessmall + Spacessmall + "FETCH NEXT FROM Cur INTO  @index,@id, @command ");
            edd.Add(Spacessmall + Spacessmall + "WHILE @@FETCH_STATUS = 0 ");
            edd.Add(Spacessmall + Spacessmall + "BEGIN --  CUR  ");
            edd.Add("");
            edd.Add(Spacessmall + Spacessmall + "BEGIN TRY        ");
            edd.Add(Spacessmall + Spacessmall + "IF UPPER(@command)='DELETE' ");
            EDD_CreateDeleteRows(edd);
            edd.Add(Spacessmall + Spacessmall + "ELSE --'UPDATE' ");
            edd.Add(Spacessmall + Spacessmall + "BEGIN--'UPDATE' OR  INSERT");
            edd.Add(Spacessmall + Spacessmall + "IF ISNULL(@id,-9999)=-9999   ");
            this.EDD_CreateInsertRows(edd);
            edd.Add(Spacessmall + Spacessmall + "WHERE ROW_INDEX = @index   ");
            edd.Add(Spacessmall + Spacessmall + "ELSE");
            this.EDD_CreateUpdateRows(edd);
            edd.Add(Spacessmall + Spacessmall + "END----'UPDATE OR INSERT'    ");
            edd.Add(Spacessmall + Spacessmall + "END TRY      ");
            edd.Add(Spacessmall + Spacessmall + "BEGIN CATCH -- 1  ");
            edd.Add(Spaces + Spacessmall + "SET @status=0  ");
            edd.Add(Spaces + Spacessmall + "SET @iErrorCount = @iErrorCount+1   ");
            //edd.Add(Spaces + Spacessmall + "DECLARE @tempComment AS NVARCHAR(MAX)    ");

            edd.Add(Spacessmall + "BEGIN TRY   -- 2     ");
            edd.Add(Spaces + Spacessmall + "set @tempComment =  ERROR_MESSAGE()  ");
            edd.Add(Spaces + Spacessmall + "SET @Tablename = UPPER(SUBSTRING(OBJECT_NAME(@@PROCID),12,LEN(OBJECT_NAME(@@PROCID))))   ");
            edd.Add(Spaces + Spacessmall + "SELECT @tempComment  = dbo.ufn_parse_error_message(error_number(),ERROR_MESSAGE(),@Tablename,  '') ");
            edd.Add(Spaces + Spacessmall + "PRINT  @tempComment      ");
            edd.Add(Spacessmall + "END TRY  -- 2     ");
            edd.Add(Spacessmall + "BEGIN CATCH -- 2    ");
            edd.Add(Spaces + Spacessmall + "INSERT INTO ERROR_LOG(ERROR_DATE, ERROR_LOG_SOURCE,ERROR_DATA)   ");
            edd.Add(Spaces + Spacessmall + "VALUES (dbo.UFN_PST_DATE(),OBJECT_NAME(@@PROCID),ERROR_MESSAGE()+' Line : ' + cast(ERROR_LINE() as VARCHAR(40)))   ");
            edd.Add(Spaces + Spacessmall + "SET @tempComment = ERROR_MESSAGE()  ");
            edd.Add(Spacessmall + "END CATCH   -- 2    ");
            edd.Add("");
            edd.Add(Spacessmall + "IF @iErrorCount <= @maxErrorPrint      ");
            edd.Add(Spacessmall + "SET @comment = @comment + CAST(ISNULL(@iErrorCount,0) AS NVARCHAR(2))+'. '+@tempComment+'; '  ");
            edd.Add("");

            edd.Add(Spaces + Spacessmall + "UPDATE @temp   ");
            edd.Add(Spaces + Spacessmall + "SET __PROCESS_COMMENTS = @tempComment     ");
            edd.Add(Spaces + Spacessmall + "WHERE ROW_INDEX = @index    ");
            edd.Add("");
            edd.Add(Spacessmall + "END CATCH   -- 1 ");
            edd.Add("");
            edd.Add(Spacessmall + "FETCH NEXT FROM Cur INTO  @index,@id, @command     ");
            edd.Add(Spacessmall + "END --END Cur ");
            edd.Add("");
            edd.Add(Spacessmall + "END  --NOT BULK INSERT");
            edd.Add(Spacessmall + "CLOSE CUR  ");
            edd.Add(Spacessmall + "DEALLOCATE CUR");
            edd.Add("");
            //edd.Add(Spacessmall + "edd.Add(Spacessmall + 

        }
    }
}
