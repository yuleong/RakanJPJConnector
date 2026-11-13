using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rakan_eservices
{
    public class eService
    {
        public delegate void OnInfoLog(string info);
        public event OnInfoLog InfoLog;
        public delegate void OnProgressUpdate(int value);
        public event OnProgressUpdate onProgressUpdate;

        public delegate void OnOperationUpdate(string value);
        public event OnOperationUpdate onOperationUpdate;

        public int TotalData { get; set; }
        private string nNowDate { get; set; }
        private string nNowTime { get; set; }
        private string security_code { get; set; }
        private string action { get; set; }
        private string request_date { get; set; }
        private string request_time { get; set; }
        private string ag_code { get; set; }
        private string owner_code { get; set; }
        private string developer_id { get; set; }
        private string[] AcceptCode { get; set; }
        private Database db;
        public eService()
        {
            db = new Database();
            db.InfoLog += Db_InfoLog;
            RetrieveConfig();
            AcceptCode = System.Configuration.ConfigurationManager.AppSettings["AcceptCode"].Split(',');
        }
        private void Db_InfoLog(string info)
        {
            InfoLog?.Invoke(info);
        }
        public void RetrieveConfig()
        {
            try
            {
                System.Data.DataSet rs;

                rs = db.Execute_FillDS("SELECT ConfigKey,ConfigVal FROM[Reda].[dbo].[Config] WHERE ConfigKey IN('CompSerial', 'OwnerCode', 'DeveloperID', 'SecurityCode')");
                if (rs != null)
                {
                    foreach (System.Data.DataRow dr in rs.Tables[0].Rows)
                    {
                        string ConfigKey = dr["ConfigKey"].ToString();
                        string ConfigVal = dr["ConfigVal"].ToString();
                        if (ConfigKey == "CompSerial")
                        {
                            ag_code = ConfigVal;
                        }
                        else if (ConfigKey == "OwnerCode")
                        {
                            owner_code = ConfigVal;
                        }
                        else if (ConfigKey == "DeveloperID")
                        {
                            developer_id = ConfigVal;
                        }
                        else if (ConfigKey == "SecurityCode")
                        {
                            security_code = ConfigVal;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                InfoLog?.Invoke($"ERROR retrieve config. {ex.Message}");
            }
        }
        private List<int> taskFailed = new List<int>();
        private List<int> taskSuccess = new List<int>();
        public async Task SendStudentInfoToMyEG()
        {
            DateTime now = DateTime.Now;
            nNowDate = now.ToString("yyyyMMdd");
            nNowTime = now.ToString("HHmmss");
            //InfoLog?.Invoke("==========================================================================");
            //InfoLog?.Invoke("DATE : " + nNowDate + " " + nNowTime);
            //InfoLog?.Invoke("BATCH SENDING STARTED");
            //InfoLog?.Invoke("");
            try
            {
                /// SCAPIFLAG
                /// 0: RECORD NOT READY TO SEND
                /// 1: ALLOW TO SEND
                /// 2: SUCCESS
                /// 3: FAILED
                /// 4: SKIP
                System.Data.DataSet rs = db.Execute_FillDS("SELECT StudentCourse.* FROM StudentCourse WHERE SCAPIFlag=1", false);
                List<StudentCourse> listSC = rs.Tables[0].ToList<StudentCourse>();
                TotalData = listSC.Count;
                int CurrentRow = 1;
                foreach (StudentCourse sc in listSC)
                {
                    IList<int> step = new List<int>();
                    if (sc.SCStep != null)
                    {
                        step = sc.SCStep.ToString().Select(digit => int.Parse(digit.ToString())).ToList();
                    }
                    taskFailed = new List<int>();
                    taskSuccess = new List<int>();
                    InfoLog?.Invoke("Student:" + Newtonsoft.Json.JsonConvert.SerializeObject(sc));
                    onProgressUpdate?.Invoke(CurrentRow);

                    if (step.Count == 0 || step.Contains(1))
                    {
                        onOperationUpdate?.Invoke("USER DECLARATION");
                        InfoLog?.Invoke("SENDING: USER DECLARATION");
                        await Task.Run(() => SendUserDeclaration(sc.SCID));
                    }


                    if (step.Count == 0 || step.Contains(2))
                    {
                        onOperationUpdate?.Invoke("USER REGISTRATION");
                        InfoLog?.Invoke("SENDING: USER REGISTRATION");
                        await Task.Run(() => SendUserRegistration(sc.SCID));
                    }


                    if (step.Count == 0 || step.Contains(3))
                    {
                        onOperationUpdate?.Invoke("DI RESULT");
                        InfoLog?.Invoke("SENDING: DI RESULT");
                        await Task.Run(() => SendDIResult(sc.SCID));
                    }


                    if (step.Count == 0 || step.Contains(4))
                    {
                        onOperationUpdate?.Invoke("SEMINAR ATTENDANCE");
                        InfoLog?.Invoke("SENDING: SEMINAR ATTENDANCE");
                        await Task.Run(() => SendRecordSeminarAttd(sc.SCID));
                    }


                    string failed = string.Join("", taskFailed.ToArray());
                    InfoLog?.Invoke("FAILED TASK: " + (failed?.Length == 0 ? "NULL" : failed));
                    /// SCAPIFLAG
                    /// 0: RECORD NOT READY TO SEND
                    /// 1: ALLOW TO SEND
                    /// 2: SUCCESS
                    /// 3: FAILED
                    /// 4: SKIP
                    if (taskFailed.Count > 0)
                    {
                        string query = $"UPDATE STUDENTCOURSE SET SCAPIFLAG=3, SCSTEP={failed} WHERE SCID={sc.SCID}";
                        db.Execute_NonQuery(query);
                    }
                    else
                    {
                        string query = $"UPDATE STUDENTCOURSE SET SCAPIFLAG=2, SCSTEP=NULL WHERE SCID={sc.SCID}";
                        db.Execute_NonQuery(query);
                    }
                    if (CurrentRow != listSC.Count)
                    {
                        InfoLog?.Invoke("==========================================================================");
                    }
                    CurrentRow++;
                }
            }
            catch (Exception ex)
            {
                InfoLog?.Invoke($"ERROR send data. {ex.Message}");
            }
            //InfoLog?.Invoke("BATCH SENDING COMPLETE");
            //InfoLog?.Invoke("==========================================================================");
        }
        private async Task SendUserDeclaration(long SCID)
        {
            try
            {
                string module = "USER DECLARATION";
                System.Data.DataSet rs;
                rs = db.Execute_FillDS($"SELECT STU.StuCategory, SC.SCStuTimeOut, STU.StuIDNo FROM Student AS STU INNER JOIN StudentCourse AS SC ON STU.StuID = SC.SCStuID WHERE SC.SCID = {SCID}");
                if (rs != null)
                {
                    if (rs.Tables[0].Rows.Count > 0)
                    {
                        foreach (System.Data.DataRow dr in rs.Tables[0].Rows)
                        {
                            DateTime scdate = Convert.ToDateTime(dr["SCStuTimeOut"].ToString());
                            string request_date = scdate.ToString("yyyyMMdd");
                            string request_time = scdate.ToString("HHmmss");
                            string id_no = dr["StuIDNo"].ToString();
                            string category = dr["StuCategory"].ToString();

                            string xmldata = "<?xml version='1.0' encoding='utf-8'?>" +
                                                "<!DOCTYPE MYEGReq SYSTEM \"MYEG_JPJ1_Declaration_Req.dtd\">" +
                                                "<MYEGReq>" +
                                                "<request>" +
                                                "<security_code>" + security_code + "</security_code>" +
                                                "<action>ACT_MYEG_JPJ1_Declaration</action>" +
                                                "<request_date>" + request_date + "</request_date>" +
                                                "<request_time>" + request_time + "</request_time>" +
                                                "<prim_id_no>" + id_no + "</prim_id_no>" +
                                                "<prim_id_type>" + category + "</prim_id_type>" +
                                                "<sec_id_no>" + id_no + "</sec_id_no>" +
                                                "<sec_id_type>" + category + "</sec_id_type>" +
                                                "<declaration_a>N</declaration_a>" +
                                                "<declaration_b>N</declaration_b>" +
                                                "<declaration_c>N</declaration_c>" +
                                                "<declaration_d>N</declaration_d>" +
                                                "<declaration_e>N</declaration_e>" +
                                                "<declaration_f>N</declaration_f>" +
                                                "<declaration_z>Y</declaration_z>" +
                                                "<remark></remark>" +
                                                "</request>" +
                                                "</MYEGReq>".Replace(" ", "%20").Replace("&", "&amp;").Replace(",", "&comma;").Replace("'", "&apos;").Replace("\"", "&quot;");

                            InfoLog?.Invoke($"USER DECLARATION: {Environment.NewLine}{xmldata}");

                            await Task.Delay(100);
                            string respo = await PostClient(xmldata);
                            if (respo != null)
                            {
                                XMLParser2 ss = new XMLParser2();
                                InfoLog?.Invoke($"RESPONSE: {respo}");

                                string status = ss.getEleValue(respo, "status_code");
                                string status_msg = ss.getEleValue(respo, "status_msg");

                                InfoLog?.Invoke($"Status Code: {status}");
                                InfoLog?.Invoke($"Status Msg: {status_msg}");

                                /// Status
                                /// 01: Success
                                /// 16: User Already Exists
                                /// 55: User Already Declare
                                /// MYEG3: Incomplete MYEG Request Data
                                /// 
                                if (AcceptCode.Any(x => status.Contains(x)))
                                {
                                    /// THIS IS THE SUCCESS CODE.
                                    /// UPDATE SCAPIFLAG=2
                                    /// ADD THE SEND LOG TO LogMyEG TABLE
                                    taskSuccess.Add(1);
                                    UpdateSendLog(SCID, status, status_msg, module);
                                }
                                else
                                {
                                    /// THIS IS THE FAILED CODE.
                                    /// UPDATE SCAPIFLAG=3
                                    /// ADD THE SEND LOG TO LogMyEG TABLE
                                    taskFailed.Add(1);
                                    UpdateSendLog(SCID, status, status_msg, module);
                                }

                                //var IsCodeFailed = new[] { "MYEG3" };
                                //if (IsCodeFailed.Any(x => status.Contains(x)))
                                //{
                                //    /// THIS IS THE FAILED CODE.
                                //    /// UPDATE SCAPIFLAG=3
                                //    /// ADD THE SEND LOG TO LogMyEG TABLE
                                //    taskFailed.Add(1);
                                //    UpdateSendLog(SCID, status, status_msg, module);
                                //}
                                //else
                                //{
                                //    /// THIS IS THE SUCCESS CODE.
                                //    /// UPDATE SCAPIFLAG=2
                                //    /// ADD THE SEND LOG TO LogMyEG TABLE
                                //    taskSuccess.Add(1);
                                //    UpdateSendLog(SCID, status, status_msg, module);
                                //}
                            }
                            else
                            {
                                /// THIS IS THE FAILED CODE.
                                /// UPDATE SCAPIFLAG=3
                                /// ADD THE SEND LOG TO LogMyEG TABLE
                                taskFailed.Add(1);
                                string status = "RN01";
                                string status_msg = "Response null from MyEG";
                                UpdateSendLog(SCID, status, status_msg, module);
                            }
                        }
                    }
                    else
                    {
                        taskFailed.Add(1);
                        string status = "NF404";
                        string status_msg = "STUDENT RECORD NOT FOUND";
                        UpdateSendLog(SCID, status, status_msg, module);
                    }
                }
                else
                {
                    taskFailed.Add(1);
                    string status = "NL404";
                    string status_msg = "STUDENT RECORD NULL";
                    UpdateSendLog(SCID, status, status_msg, module);
                }
            }
            catch (Exception ex)
            {
                InfoLog?.Invoke($"ERROR get user declaration. {ex.Message}");
            }
        }
        private string GetCityCode(string name)
        {
            string CityCode = "";
            try
            {
                //System.Data.DataSet rs = db.Execute_FillDS($"SELECT [CityCode] FROM [Reda].[dbo].[City] WHERE CityName LIKE '%{name}%' OR CityRef LIKE '%{name}%'");
                System.Data.DataSet rs = db.Execute_FillDS($"SELECT [CityCode] FROM [Reda].[dbo].[City] WHERE CityName={SQLS(name)} OR CityRef={SQLS(name)}");
                if (rs != null)
                {
                    CityCode = rs.Tables[0].Rows[0]["CityCode"].ToString();
                }
            }
            catch (Exception ex)
            {
                InfoLog?.Invoke($"ERROR get city code. {ex.Message}");
            }
            return CityCode;
        }
        private string GetStateCode(string name)
        {
            string StateCode = "";
            try
            {
                //System.Data.DataSet rs = db.Execute_FillDS($"SELECT [StateCode] FROM [Reda].[dbo].[State] WHERE StateName LIKE '%{name}%'");
                System.Data.DataSet rs = db.Execute_FillDS($"SELECT [StateCode] FROM [Reda].[dbo].[State] WHERE StateName={SQLS(name)}");
                if (rs != null)
                {
                    StateCode = rs.Tables[0].Rows[0]["StateCode"].ToString();
                }
            }
            catch (Exception ex)
            {
                InfoLog?.Invoke($"ERROR get state code. {ex.Message}");
            }
            return StateCode;
        }
        IEnumerable<string> SplitToLines(string stringToSplit, int maximumLineLength)
        {
            var words = stringToSplit.Split(' ');
            var line = words.First();
            foreach (var word in words.Skip(1))
            {
                var test = $"{line} {word}";
                if (test.Length > maximumLineLength)
                {
                    yield return line;
                    line = word;
                }
                else
                {
                    line = test;
                }
            }
            yield return line;
        }
        private async Task SendUserRegistration(long SCID)
        {
            try
            {
                string module = "USER REGISTRATION";
                System.Data.DataSet rs;
                rs = db.Execute_FillDS($"SELECT SC.SCStuTimeOut,S.StuIDNo,S.StuName,S.StuCategory,S.StuGender,S.StuDOB,S.StuAddress,S.StuAddress2,S.StuAddress3,S.StuPostcode,S.StuCity,S.StuState,CL.CountryName FROM Student AS S INNER JOIN StudentCourse AS SC ON S.StuID = SC.SCStuID INNER JOIN CountryList CL ON CL.CountryCode = S.StuNationality WHERE SC.SCID = {SCID}");
                if (rs != null)
                {
                    if (rs.Tables[0].Rows.Count > 0)
                    {
                        foreach (System.Data.DataRow dr in rs.Tables[0].Rows)
                        {
                            DateTime scdate = Convert.ToDateTime(dr["SCStuTimeOut"].ToString());
                            string request_date = scdate.ToString("yyyyMMdd");
                            string request_time = scdate.ToString("HHmmss");
                            string id_no = dr["StuIDNo"].ToString();
                            string category = dr["StuCategory"].ToString();
                            string name = dr["StuName"].ToString();

                            IEnumerable<string> splitName = SplitToLines(name, 30);
                            string name_1 = splitName.ElementAtOrDefault(0) ?? "";
                            string name_2 = splitName.ElementAtOrDefault(1) ?? "";
                            string name_3 = splitName.ElementAtOrDefault(2) ?? "";


                            int gender = Convert.ToInt32(dr["StuGender"].ToString());
                            string place_of_birth = "";
                            string dob = Convert.ToDateTime(dr["StuDOB"].ToString()).ToString("yyyyMMdd");
                            string address_1 = dr["StuAddress"].ToString();
                            string address_2 = dr["StuAddress2"].ToString();
                            string address_3 = dr["StuAddress3"].ToString();
                            string city = GetCityCode(dr["StuCity"].ToString());
                            string state = GetStateCode(dr["StuState"].ToString());
                            string postcode = dr["StuPostcode"].ToString();
                            string country = dr["CountryName"].ToString().ToUpper();

                            string xmldata = "<?xml version='1.0' encoding='utf-8'?>" +
                                                "<!DOCTYPE MYEGReq SYSTEM \"MYEG_JPJ1_Reg_Req.dtd\">" +
                                                "<MYEGReq>" +
                                                "<request>" +
                                                "<security_code>" + security_code + "</security_code>" +
                                                "<action>ACT_MYEG_JPJ1_REG</action>" +
                                                "<request_date>" + request_date + "</request_date>" +
                                                "<request_time>" + request_time + "</request_time>" +
                                                "<name_1>" + name_1 + "</name_1>" +
                                                "<name_2>" + name_2 + "</name_2>" +
                                                "<name_3>" + name_3 + "</name_3>" +
                                                "<prim_id_no>" + id_no + "</prim_id_no>" +
                                                "<prim_id_type>" + category + "</prim_id_type>" +
                                                "<sec_id_no>" + id_no + "</sec_id_no>" +
                                                "<sec_id_type>" + category + "</sec_id_type>" +
                                                "<gender>" + (gender == 0 ? "L" : "P") + "</gender>" +
                                                "<place_of_birth>" + place_of_birth + " </place_of_birth>" +
                                                "<DOB>" + dob + "</DOB>" +
                                                "<address_1>" + address_1 + "</address_1>" +
                                                "<address_2>" + address_2 + "</address_2>" +
                                                "<address_3>" + address_3 + "</address_3>" +
                                                "<city>" + city + "</city>" +
                                                "<state>" + state + "</state>" +
                                                "<postcode>" + postcode + "</postcode>" +
                                                "<country>" + country + "</country>" +
                                                "<house_tel></house_tel>" +
                                                "<handphone_no></handphone_no>" +
                                                "<fax></fax>" +
                                                "<email></email>" +
                                                "<ag_code>" + ag_code + "</ag_code>" +
                                                "</request>" +
                                                "</MYEGReq>";

                            //xmldata = xmldata.HtmlEncode();

                            InfoLog?.Invoke($"USER REGISTRATION: {Environment.NewLine}{xmldata}");

                            await Task.Delay(100);
                            string respo = await PostClient(xmldata);
                            if (respo != null)
                            {
                                XMLParser2 ss = new XMLParser2();
                                InfoLog?.Invoke($"RESPONSE: {respo}");
                                //InfoLog?.Invoke(ss.FormatXml(respo));


                                string status = ss.getEleValue(respo, "status_code");
                                string status_msg = ss.getEleValue(respo, "status_msg");

                                InfoLog?.Invoke($"Status Code: {status}");
                                InfoLog?.Invoke($"Status Msg: {status_msg}");

                                /// Status
                                /// 01: Success
                                /// 16: User Already Exists
                                /// 55: User Already Declare
                                /// MYEG3: Incomplete MYEG Request Data
                                /// 
                                if (AcceptCode.Any(x => status.Contains(x)))
                                {
                                    /// THIS IS THE SUCCESS CODE.
                                    /// UPDATE SCAPIFLAG=2
                                    /// ADD THE SEND LOG TO LogMyEG TABLE
                                    taskSuccess.Add(2);
                                    UpdateSendLog(SCID, status, status_msg, module);
                                }
                                else
                                {
                                    /// THIS IS THE FAILED CODE.
                                    /// UPDATE SCAPIFLAG=3
                                    /// ADD THE SEND LOG TO LogMyEG TABLE
                                    taskFailed.Add(2);
                                    UpdateSendLog(SCID, status, status_msg, module);
                                }


                                ///// Status
                                ///// 01: Success
                                ///// 16: User Already Exists
                                ///// 39: Invalid City Code
                                ///// 40: Invalid State Code
                                ///// 68: Invalid Name
                                ///// PDX10012: 
                                ///// PDX10068: Parameter Too Long
                                ///// MYEG3: Incomplete MYEG Request Data
                                //var IsCodeFailed = new[] { "39","40", "PDX10012", "PDX10068", "MYEG3" };
                                //if (IsCodeFailed.Any(x => status.Contains(x)))
                                //{
                                //    /// THIS IS THE FAILED CODE.
                                //    /// UPDATE SCAPIFLAG=3
                                //    /// ADD THE SEND LOG TO LogMyEG TABLE
                                //    taskFailed.Add(2);
                                //    UpdateSendLog(SCID, status, status_msg, module);
                                //}
                                //else
                                //{
                                //    /// THIS IS THE SUCCESS CODE.
                                //    /// UPDATE SCAPIFLAG=2
                                //    /// ADD THE SEND LOG TO LogMyEG TABLE
                                //    taskSuccess.Add(2);
                                //    UpdateSendLog(SCID, status, status_msg, module);
                                //}
                            }
                            else
                            {
                                /// THIS IS THE FAILED CODE.
                                /// UPDATE SCAPIFLAG=3
                                /// ADD THE SEND LOG TO LogMyEG TABLE
                                taskFailed.Add(2);
                                string status = "RN02";
                                string status_msg = "Response null from MyEG";
                                UpdateSendLog(SCID, status, status_msg, module);
                            }
                        }
                    }
                    else
                    {
                        taskFailed.Add(2);
                        string status = "NF404";
                        string status_msg = "STUDENT RECORD NOT FOUND";
                        UpdateSendLog(SCID, status, status_msg, module);
                    }
                }
                else
                {
                    taskFailed.Add(2);
                    string status = "NL404";
                    string status_msg = "STUDENT RECORD NULL";
                    UpdateSendLog(SCID, status, status_msg, module);
                }
            }
            catch (Exception ex)
            {
                InfoLog?.Invoke($"ERROR get user registration. {ex.Message}");
            }
        }
        private async Task SendDIResult(long SCID)
        {
            try
            {
                string module = "DI RESULT";
                System.Data.DataSet rs;
                rs = db.Execute_FillDS($"SELECT S.StuID,S.StuIDNo,S.StuCategory,I.InsIDNO,SC.SCStuTimeIn,SC.SCStuTimeOut FROM Class AS C INNER JOIN Instructor AS I ON C.ClassInstructorID = I.InsID INNER JOIN Student AS S INNER JOIN StudentCourse AS SC ON S.StuID = SC.SCStuID ON C.ClassID = SC.SCClassID WHERE SC.SCID = {SCID}");
                if (rs != null)
                {
                    if (rs.Tables[0].Rows.Count > 0)
                    {
                        foreach (System.Data.DataRow dr in rs.Tables[0].Rows)
                        {
                            string id_no = dr["StuIDNo"].ToString();
                            string category = dr["StuCategory"].ToString();
                            string officer_code = dr["InsIDNO"].ToString();
                            string rslt_date = Convert.ToDateTime(dr["SCStuTimeIn"].ToString()).ToString("yyyyMMdd");
                            string rslt_time = Convert.ToDateTime(dr["SCStuTimeIn"].ToString()).ToString("HHmmss");

                            string xmldata = "<?xml version='1.0' encoding='utf-8'?>" +
                                            "<!DOCTYPE MYEGReq SYSTEM \"MYEG_JPJ1_DIResult_Req.dtd\">" +
                                            "<MYEGReq>" +
                                            "<request>" +
                                            "<security_code>" + security_code + "</security_code>" +
                                            "<action>ACT_JPJ1_InsertResult</action>" +
                                            "<request_date>" + rslt_date + "</request_date>" +
                                            "<request_time>" + rslt_time + "</request_time>" +
                                            "<id_no>" + id_no + "</id_no>" +
                                            "<id_type>" + category + "</id_type>" +
                                            "<ag_code>" + ag_code + "</ag_code>" +
                                            "<owner_code>" + owner_code + "</owner_code>" +
                                            "<officer_code>" + officer_code + "</officer_code>" +
                                            "<ht_rslt_status>P</ht_rslt_status>" +
                                            "<ht_rslt_date>" + rslt_date + "</ht_rslt_date>" +
                                            "<ht_rslt_time>" + rslt_time + "</ht_rslt_time>" +
                                            "<ct_rslt_status>P</ct_rslt_status>" +
                                            "<ct_rslt_date>" + rslt_date + "</ct_rslt_date>" +
                                            "<ct_rslt_time>" + rslt_time + "</ct_rslt_time>" +
                                            "</request>" +
                                            "</MYEGReq>";
                            //.Replace(" ", "%20").Replace("&", "&amp;").Replace(",", "&comma;").Replace("'", "&apos;").Replace("\"", "&quot;");

                            InfoLog?.Invoke($"DI RESULT: {Environment.NewLine}{xmldata}");

                            await Task.Delay(100);
                            string respo = await PostClient(xmldata);
                            if (respo != null)
                            {
                                XMLParser2 ss = new XMLParser2();
                                InfoLog?.Invoke($"RESPONSE: {respo}");

                                string status = ss.getEleValue(respo, "status_code");
                                string status_msg = ss.getEleValue(respo, "status_msg");

                                InfoLog?.Invoke($"Status Code: {status}");
                                InfoLog?.Invoke($"Status Msg: {status_msg}");

                                /// Status
                                /// 01: Success
                                /// 16: User Already Exists
                                /// 55: User Already Declare
                                /// MYEG3: Incomplete MYEG Request Data
                                /// 
                                if (AcceptCode.Any(x => status.Contains(x)))
                                {
                                    /// THIS IS THE SUCCESS CODE.
                                    /// UPDATE SCAPIFLAG=2
                                    /// ADD THE SEND LOG TO LogMyEG TABLE
                                    taskSuccess.Add(3);
                                    UpdateSendLog(SCID, status, status_msg, module);
                                }
                                else
                                {
                                    /// THIS IS THE FAILED CODE.
                                    /// UPDATE SCAPIFLAG=3
                                    /// ADD THE SEND LOG TO LogMyEG TABLE
                                    taskFailed.Add(3);
                                    UpdateSendLog(SCID, status, status_msg, module);
                                }

                                ///// Status
                                ///// 01: Success
                                ///// 15: User Not Found
                                ///// 53: Duplicate Record
                                ///// MYEG3: Incomplete MYEG Request Data
                                //var IsCodeFailed = new[] { "15", "MYEG3" };
                                //if (IsCodeFailed.Any(x => status.Contains(x)))
                                //{
                                //    /// THIS IS THE FAILED CODE.
                                //    /// UPDATE SCAPIFLAG=3
                                //    /// ADD THE SEND LOG TO LogMyEG TABLE
                                //    taskFailed.Add(3);
                                //    UpdateSendLog(SCID, status, status_msg, module);
                                //}
                                //else
                                //{
                                //    /// THIS IS THE SUCCESS CODE.
                                //    /// UPDATE SCAPIFLAG=2
                                //    /// ADD THE SEND LOG TO LogMyEG TABLE
                                //    taskSuccess.Add(3);
                                //    UpdateSendLog(SCID, status, status_msg, module);
                                //}
                            }
                            else
                            {
                                /// THIS IS THE FAILED CODE.
                                /// UPDATE SCAPIFLAG=3
                                /// ADD THE SEND LOG TO LogMyEG TABLE
                                taskFailed.Add(3);
                                string status = "RN03";
                                string status_msg = "Response null from MyEG";
                                UpdateSendLog(SCID, status, status_msg, module);
                            }
                        }
                    }
                    else
                    {
                        taskFailed.Add(3);
                        string status = "NF404";
                        string status_msg = "STUDENT RECORD NOT FOUND";
                        UpdateSendLog(SCID, status, status_msg, module);
                    }
                }
                else
                {
                    taskFailed.Add(3);
                    string status = "NL404";
                    string status_msg = "STUDENT RECORD NULL";
                    UpdateSendLog(SCID, status, status_msg, module);
                }
            }
            catch (Exception ex)
            {
                InfoLog?.Invoke($"ERROR get DI result. {ex.Message}");
            }
        }
        private async Task SendRecordSeminarAttd(long SCID)
        {
            try
            {
                string module = "RECORD SEMINAR ATTENDANCE";
                System.Data.DataSet rs;
                rs = db.Execute_FillDS($"SELECT STU.StuCategory, SC.SCDate, SC.SCLicenseCode, STU.StuIDNo, INS.InsIDNO, SL.SLKPPNo, SC.SCCourseDate, Room.RoomClassCert, SC.SCCertNo, SC.SCStuTimeIn, LIC.LicenseCode FROM Student AS STU INNER JOIN StudentCourse AS SC ON STU.StuID = SC.SCStuID INNER JOIN Class AS CL ON SC.SCClassID = CL.ClassID INNER JOIN Instructor AS INS ON CL.ClassInstructorID = INS.InsID INNER JOIN StudentLicense AS SL ON SC.SCLicenseID = SL.SLLicenseID AND SLStuID = SCStuID AND SLEndDate IS NULL INNER JOIN Room ON CL.ClassRoomID = Room.RoomID INNER JOIN LicenseType AS LIC ON SC.SCLicenseID = LIC.LicenseID WHERE SC.SCID={SCID}");
                if (rs != null)
                {
                    InfoLog?.Invoke("DEBUG SendRecordSeminarAttd #1 RS IS NULL");
                    if (rs.Tables[0].Rows.Count > 0)
                    {
                        InfoLog?.Invoke("DEBUG SendRecordSeminarAttd #1 RS > 0");
                        foreach (System.Data.DataRow dr in rs.Tables[0].Rows)
                        {
                            string[] licenses = dr["SCLicenseCode"].ToString().Split(',');

                            DateTime scdate = Convert.ToDateTime(dr["SCStuTimeIn"].ToString());
                            string request_date = scdate.ToString("yyyyMMdd");
                            string request_time = scdate.ToString("HHmmss");
                            string id_no = dr["StuIDNo"].ToString();
                            string category = dr["StuCategory"].ToString();
                            string course_date = Convert.ToDateTime(dr["SCCourseDate"].ToString()).ToString("yyMMdd");
                            string class_cert = dr["RoomClassCert"].ToString();
                            string cert = dr["SCCertNo"].ToString();
                            string class_type_1 = licenses.Length > 0 ? licenses[0] : "";//dr["LicenseCode"].ToString();
                            string class_type_2 = licenses.Length > 1 ? licenses[1] : "";
                            string class_type_3 = "";
                            string attend_date = request_date;
                            string attend_time = request_time;
                            string doc_name = "L2A";
                            string cert_no = $"{ag_code}/{doc_name}/{course_date}{class_cert.PadLeft(2, '0')}{cert.PadLeft(2, '0')}";
                            string di_instructor = dr["InsIDNO"].ToString();
                            string book_serial_no = dr["SLKPPNo"].ToString();
                            string sm_code = "5H_KPP";  //dr["LicenseCode"].ToString();

                            string xmldata = "<?xml version='1.0' encoding='utf-8'?>" +
                                            "<!DOCTYPE MYEGReq SYSTEM \"MYEG_JPJ1_Kpp_Req.dtd\">" +
                                            "<MYEGReq>" +
                                            "<request>" +
                                            "<security_code>" + security_code + "</security_code>" +
                                            "<action>ACT_MYEG_JPJ1_KPP</action>" +
                                            "<request_date>" + attend_date + "</request_date>" +
                                            "<request_time>" + attend_time + "</request_time>" +
                                            "<id_no>" + id_no + "</id_no>" +
                                            "<id_type>" + category + "</id_type>" +
                                            "<class_type_1>" + class_type_1 + "</class_type_1>" +
                                            "<class_type_2>" + class_type_2 + "</class_type_2>" +
                                            "<class_type_3>" + class_type_3 + "</class_type_3>" +
                                            "<attend_date>" + attend_date + "</attend_date>" +
                                            "<attend_time>" + attend_time + "</attend_time>" +
                                            "<cert_no>" + cert_no + "</cert_no>" +
                                            "<sm_code>" + sm_code + "</sm_code>" +
                                            "<ag_code>" + ag_code + "</ag_code>" +
                                            "<owner_code>" + owner_code + "</owner_code>" +
                                            "<di_instructor>" + di_instructor + "</di_instructor>" +
                                            "<book_serial_no>" + book_serial_no + "</book_serial_no>" +
                                            "</request>" +
                                            "</MYEGReq>";

                            InfoLog?.Invoke($"RECORD SEMINAR ATTENDANCE: {Environment.NewLine}{xmldata}");
                            await Task.Delay(100);
                            string respo = await PostClient(xmldata);
                            if (respo != null)
                            {
                                XMLParser2 ss = new XMLParser2();
                                InfoLog?.Invoke($"RESPONSE: {respo}");

                                string status = ss.getEleValue(respo, "status_code");
                                string status_msg = ss.getEleValue(respo, "status_msg");

                                /// Status
                                /// 01: Success
                                /// 16: User Already Exists
                                /// 55: User Already Declare
                                /// MYEG3: Incomplete MYEG Request Data
                                /// 
                                if (AcceptCode.Any(x => status.Contains(x)))
                                {
                                    /// THIS IS THE SUCCESS CODE.
                                    /// UPDATE SCAPIFLAG=2
                                    /// ADD THE SEND LOG TO LogMyEG TABLE
                                    taskSuccess.Add(4);
                                    UpdateSendLog(SCID, status, status_msg, module);
                                }
                                else
                                {
                                    /// THIS IS THE FAILED CODE.
                                    /// UPDATE SCAPIFLAG=3
                                    /// ADD THE SEND LOG TO LogMyEG TABLE
                                    taskFailed.Add(4);
                                    UpdateSendLog(SCID, status, status_msg, module);
                                }

                                //InfoLog?.Invoke($"Status Code: {status}");
                                //InfoLog?.Invoke($"Status Msg: {status_msg}");
                                ///// Status
                                ///// 01: Success
                                ///// 15: User Not Found
                                ///// 64: Duplicate Certificate No
                                ///// 71: Found Two Seminar At The Same Time
                                ///// MYEG3: Incomplete MYEG Request Data

                                //var IsCodeFailed = new[] { "36", "26", "30", "JPJ0", "15", "80", "64", "JPJ12", "71", "MYEG3" };
                                //if (IsCodeFailed.Any(x => status.Contains(x)))
                                //{
                                //    /// THIS IS THE FAILED CODE.
                                //    /// UPDATE SCAPIFLAG=3
                                //    /// ADD THE SEND LOG TO LogMyEG TABLE
                                //    taskFailed.Add(4);
                                //    UpdateSendLog(SCID, status, status_msg, module);
                                //}
                                //else
                                //{
                                //    /// THIS IS THE SUCCESS CODE.
                                //    /// UPDATE SCAPIFLAG=2
                                //    /// ADD THE SEND LOG TO LogMyEG TABLE
                                //    taskSuccess.Add(4);
                                //    UpdateSendLog(SCID, status, status_msg, module);
                                //}
                            }
                            else
                            {
                                /// THIS IS THE FAILED CODE.
                                /// UPDATE SCAPIFLAG=3
                                /// ADD THE SEND LOG TO LogMyEG TABLE
                                taskFailed.Add(4);
                                string status = "RN04";
                                string status_msg = "Response null from MyEG";
                                UpdateSendLog(SCID, status, status_msg, module);
                            }
                        }
                    }
                    else
                    {
                        taskFailed.Add(4);
                        string status = "NF404";
                        string status_msg = "STUDENT RECORD NOT FOUND";
                        UpdateSendLog(SCID, status, status_msg, module);
                    }
                }
                else
                {
                    taskFailed.Add(4);
                    string status = "NL404";
                    string status_msg = "STUDENT RECORD NULL";
                    UpdateSendLog(SCID, status, status_msg, module);
                }
            }
            catch (Exception ex)
            {
                InfoLog?.Invoke($"ERROR get record seminar attendance. {ex.Message}");
            }
        }

        private async Task<string> PostClient(string xdoc)
        {
            string respo = null;
            try
            {
                Uri uri = new Uri("https://jpj.myeg.com.my/JPJPRAL/MyEG_DI_JPJ_Live_Req");
                Uri newuri = new Uri(uri, "?");
                string param = $"data={ xdoc}";
                using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
                {
                    var content = new System.Net.Http.StringContent(param, Encoding.UTF8, "application/xml");
                    var response = await client.PostAsync(newuri.ToString() + param, content);
                    respo = await response.Content.ReadAsStringAsync();
                    if (respo.Contains("RESPONSE FROM MYEG : "))
                    {
                        respo = respo.Replace("RESPONSE FROM MYEG : ", "");
                    }
                }
            }
            catch (Exception ex)
            {
                InfoLog?.Invoke($"ERROR post client xml for MYEG portal. {ex.Message}");
                respo = null;
            }
            return respo;
        }
        private string SQLS(string data)
        {
            return string.IsNullOrEmpty(data) ? "" : "N'" + data.Replace("'", "''") + "'";
        }
        private void UpdateSendLog(long SCID, string status, string status_msg, string module)
        {
            try
            {
                string query = $"INSERT INTO LOGMYEG (LogSCID,LogStatusCode,LogStatusMsg,LogModule) VALUES ({SCID},{SQLS(status)},{SQLS(status_msg)},{SQLS(module)})";
                db.Execute_NonQuery(query);
            }
            catch (Exception ex)
            {
                InfoLog?.Invoke($"ERROR update send log. {ex.Message}");
            }
        }
    }
}
