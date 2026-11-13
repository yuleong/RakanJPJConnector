namespace rakan_eservices
{
    public static class Logging
    {
        #region Variable Declaration
        private static string m_strTextLogFile = string.Empty;
        private static System.IO.StreamWriter m_objTextLog;
        #endregion

        public static void InitLogFile(string strLogFile)
        {
            string strErrMsg = null;
            try
            {
                m_strTextLogFile = strLogFile;
                (new System.IO.FileInfo(m_strTextLogFile)).Directory.Create();
                m_objTextLog = new System.IO.StreamWriter(m_strTextLogFile, true);
            }
            catch (System.Exception ex)
            {
                strErrMsg = "Error in creating an instance of Logging class with log path." + System.Environment.NewLine + ex.Message;
            }
        }
        public static bool Close()
        {
            string strErrMsg = null;
            try
            {
                return CloseTextLog();
            }
            catch (System.Exception ex)
            {
                strErrMsg = "Error in closing the Event Log File" + System.Environment.NewLine + ex.Message;
            }
            return false;
        }
        private static bool CloseTextLog()
        {
            string strErrMsg = null;
            try
            {
                if (m_objTextLog != null)
                {
                    m_objTextLog.Close();
                    return true;
                }
            }
            catch (System.Exception ex)
            {
                strErrMsg = "Error in creating an instance of Logger class" + System.Environment.NewLine + ex.Message;
                return false;
            }
            return false;
        }
        public static void WriteLine(string strLogMsg, System.Diagnostics.EventLogEntryType EvtType)
        {
            string strErrMsg = null;
            try
            {
                TxtLogWriteLine(strLogMsg, EvtType);
            }
            catch (System.Exception ex)
            {
                strErrMsg = "Error in creating an instance of Logger class" + System.Environment.NewLine + ex.Message;
            }
        }
        private static void TxtLogWriteLine(string strLogMsg, System.Diagnostics.EventLogEntryType EvtType)
        {
            string strErrMsg = null;
            try
            {
                if (m_objTextLog != null)
                {
                    strLogMsg = System.DateTime.Now.ToString("M/d/yyyy HH:mm:ss.fff") + " - " + EvtType.ToString() + " - " + strLogMsg;
                    m_objTextLog.WriteLine(strLogMsg);
                    m_objTextLog.Flush();
                }
            }
            catch (System.Exception ex)
            {
                strErrMsg = "Error in creating an instance of Logger class" + System.Environment.NewLine + ex.Message;
            }
        }
    }
}
