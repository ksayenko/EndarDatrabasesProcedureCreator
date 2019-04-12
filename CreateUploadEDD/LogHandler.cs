using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace CreateUploadEDD
{
    public class LogHandler
    {
        private string logFile = "log.txt";
        private readonly string LOG_NAME = "log";
        string path;
        DirectoryInfo dir;

        public LogHandler()
            : this("", "")
        {
        }


        public LogHandler(string sn, string mn)
        {  
            try
            {
                LOG_NAME = Application.ProductName;
                logFile = Application.ProductName + "_log.txt";
                path = Properties.Settings.Default.LogPath.Trim();
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            catch
            {
                path = Application.CommonAppDataPath + Path.DirectorySeparatorChar + "Logs";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            dir = new DirectoryInfo(path);
            if (sn != "")
            {
                LogWarning("Start " + sn + " " + mn);
            }

        }

        public void LogWarning(string strLogText)
        {
            LogWarning(strLogText, false);
        }

        public void LogWarning(string strLogText, bool bWriteToApplicationLog)
        {

            try
            {
                {

                    if (strLogText.Trim().Length == 0)
                    {
                        return;
                    }
                    else
                    {

                        // Create a writer && open the file:
                        StreamWriter log = null;
                        string fullpath = dir.ToString() + "\\" + logFile;

                        if (!File.Exists(fullpath))
                        {

                            log = new StreamWriter(fullpath, true);
                        }
                        else
                        {
                            //get file date
                            //if( today { use, else rename current && make new
                            DateTime fDate = File.GetLastWriteTime(fullpath);
                            if (fDate.Day == DateTime.Now.Day)
                                log = new StreamWriter(fullpath, true);
                            else
                            {
                                File.Move(fullpath, dir.ToString() + "\\" + fDate.Date.ToString("MMddyyyy") + LOG_NAME + ".txt");
                                log = new StreamWriter(fullpath, true);
                            }
                        }


                        // Write to the file:
                        log.Write(DateTime.Now);
                        log.Write("," + strLogText);
                        log.Write("\n\r");
                        log.Write(Environment.NewLine);

                        // Close the stream:
                        log.Close();

                    }
                }
            }
            catch (Exception e)
            {
            }

        }


    }
}


