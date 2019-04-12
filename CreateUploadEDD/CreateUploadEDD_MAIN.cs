using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace CreateUploadEDD
{
    public partial class CreateUploadEDD_MAIN : Form
    {
        DataAccess da;
        string ds;
        string catalog = "";
        string user = "";
        string pwd = "";
        string tablename = "";

        private string script;
        string path;
        DirectoryInfo dir;

        LogHandler lh;

        ArrayList createtable;
         ArrayList createuploadtable;
         ArrayList createuploadarchivetable;
         ArrayList createuploadchecktable;
        ArrayList eddprocedure;
        ArrayList oldsp;

        ArrayList trigger;
        ArrayList proc_all;
        ArrayList grant_perm;

        ArrayList columns;

        public CreateUploadEDD_MAIN()
        {
            InitializeComponent();
            InitScripts();

            lh = new LogHandler("CreateUploadEDD_MAIN", "Constructor");


            DownloadProperties();
            SetRadiobutton();

            da = new DataAccess(ds, catalog, user, pwd);
            textBox1.Text = path;

            PopulateCombobox();
        }

        public void InitScripts()
        {
            createtable = new ArrayList();
            createuploadtable = new ArrayList();
            createuploadarchivetable = new ArrayList();
            eddprocedure = new ArrayList();
            oldsp = new ArrayList();
            proc_all = new ArrayList();
            trigger = new ArrayList();

            columns = new ArrayList();
        }

        private void DownloadProperties()
        {
            try
            {

                ds = Properties.Settings.Default.DataSource.ToString();
                catalog = Properties.Settings.Default.Catalog.ToString();
                user = Properties.Settings.Default.User.ToString();
                pwd = Properties.Settings.Default.PWD.ToString();
                tablename = Properties.Settings.Default.TableName.ToString();
                path = Properties.Settings.Default.FolderToWriteScripts;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            catch(Exception ex)
            {
                lh.LogWarning(ex.ToString());
                ds =  "DIVS704SQL1\\MSSQLPUBLIC";
                catalog = "Endar_CIMP";
                user = "kateryna.sayenko";
                pwd = "deti9503";
                tablename = "";
                path = Application.CommonAppDataPath + Path.DirectorySeparatorChar + "Scripts";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }

           
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            tablename = cbTables.SelectedItem.ToString();
            Properties.Settings.Default.TableName = tablename;
            Properties.Settings.Default.Save();

            DataTable dt = da.Edd_Process_Columns(tablename);
            TableStructure ts = new TableStructure(da, tablename, dt);
            CreateTableScript(ts); 
        }

        private void CreateTableScript(TableStructure ts)
        {
            createtable = ts.GetCreateTable();
            createuploadtable = ts.GetCreateUploadTable();
            createuploadarchivetable = ts.GetCreateUploadArchiveTable();
            createuploadchecktable = ts.GetCreateUploadCheckTable();
            eddprocedure = ts.GetCreateUploadRoutine();
            oldsp = ts.GetExistSP();

            trigger = ts.GetLastUpdateTrigger();
            proc_all = ts.GetRoutineUploadAll();
            //proc_all;
            //grant_perm;

        }

        //private void GetProdColumns()
        //{
        //    DataTable dt = da.Edd_Process_Columns(tablename);
        //    int columnNameID = 1;
        //    if (dt.Rows.Count > 0)
        //    {
        //        columns = new ArrayList();
        //        string temp;
        //        foreach (DataRow dr in dt.Rows)
        //        {
        //            temp = dr[columnNameID].ToString();
        //            columns.Add(temp);
        //        }
        //    }
        //}


        private void PopulateCombobox()
        {
            cbTables.Items.Clear();
            DataTable dt = da.Edd_Process_Tables();
            if (dt != null && dt.Rows.Count > 0)
            {
                String temp;
                foreach (DataRow dr in dt.Rows)
                {
                    temp = dr[0].ToString();
                    cbTables.Items.Add(temp);
                }

                if (tablename != "")
                    cbTables.SelectedItem = tablename;
            }


        }

        private void button1_Click(object sender, EventArgs e)
        {
            WriteFileFromArrayList(createtable, catalog + "_", "");
            WriteFileFromArrayList(createuploadtable, catalog+"_upload", "");
            WriteFileFromArrayList(createuploadarchivetable, catalog + "_upload", "archive");
            WriteFileFromArrayList(createuploadchecktable, catalog + "_upload", "check");
            WriteFileFromArrayList(eddprocedure, catalog + "_edd_upload", "");
            WriteFileFromArrayList(oldsp, catalog + "_old_edd_upload", "");
            WriteFileFromArrayList(trigger, catalog + "_last_update_", "trigger");
            WriteFileFromArrayList(proc_all, catalog + "_edd_upload_", "all");

        }

        private void WriteFileFromArrayList(ArrayList List,string prefix, string end)
        {
            if (prefix != "")
                prefix += "_";
            if (end != "")
                end = "_" + end;
            
            string filenameUpload = path + "\\"+prefix+ tablename +end+ ".sql";

            if (File.Exists(filenameUpload))
            {
                string prefix2 = "";
                DateTime dtime = DateTime.Now;
                prefix2 = dtime.Year.ToString() + dtime.Month.ToString() + dtime.Day.ToString() +
                    "_" + dtime.Hour.ToString() + dtime.Minute.ToString() + dtime.Second.ToString();
                filenameUpload = path + "\\" + prefix2 + "_" + prefix + tablename + end + ".sql";
            }
            StreamWriter sw = new StreamWriter(filenameUpload);

            foreach (string line in List)
            {
                sw.WriteLine(line);
            }
            sw.Close();
        }

        private void CreateUploadEDD_MAIN_Load(object sender, EventArgs e)
        {
        }

        private void button2_Click(object sender, EventArgs e)
        {
            PopulateCombobox();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {
            SetCatalog();
            Properties.Settings.Default.Catalog = catalog;
            Properties.Settings.Default.Save();
        }

        private void SetCatalog()
        {
            if (rbEnDAR.Checked)
                catalog = rbEnDAR.Text;
            else
                catalog = rbEnDARCIMP.Text;
            da = new DataAccess(ds, catalog, user, pwd);
        }
        private void SetRadiobutton()
        {
            if (catalog.ToLower().EndsWith("cimp"))
            {
                rbEnDARCIMP.Checked = true;
            }
            else
                rbEnDAR.Checked = true;
        }

        private void rb_CheckedChanged(object sender, EventArgs e)
        {
            SetCatalog();
            Properties.Settings.Default.Catalog = catalog;
            Properties.Settings.Default.Save();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            //textBox1.Text = path;

        }

        private void textBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            readPath();
        }

        private void textBox1_DoubleClick(object sender, EventArgs e)
        {
            readPath();
        }

        private void readPath()
        {
            FolderBrowserDialog ofd = new FolderBrowserDialog();
            ofd.RootFolder = Environment.SpecialFolder.MyComputer;
            ofd.SelectedPath = path;

            DialogResult r = ofd.ShowDialog();
            if (r == DialogResult.OK)
            {
                path = ofd.SelectedPath;
                textBox1.Text = path;
                Properties.Settings.Default.FolderToWriteScripts = path;
                Properties.Settings.Default.Save();
            }

        }

    }
}
