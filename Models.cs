using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rakan_eservices
{
    public class DTDFiles
    {
        private DTDFiles(string value) { Value = value; }
        public string Value { get; private set; }
        public static DTDFiles MYEG_JPJ1_PraL_Req { get { return new DTDFiles("MYEG_JPJ1_PraL_Req.dtd"); } }
        public static DTDFiles MYEG_JPJ1_PraL_Addclass_Req { get { return new DTDFiles("MYEG_JPJ1_PraL_Addclass_Req.dtd"); } }
        public static DTDFiles MYEG_JPJ1_Declaration_Req { get { return new DTDFiles("MYEG_JPJ1_Declaration_Req.dtd"); } }
        public static DTDFiles MYEG_JPJ1_Reg_Req { get { return new DTDFiles("MYEG_JPJ1_Reg_Req.dtd"); } }
        public static DTDFiles MYEG_JPJ1_DIResult_Req { get { return new DTDFiles("MYEG_JPJ1_DIResult_Req.dtd"); } }
        public static DTDFiles MYEG_JPJ1_Kpp_Req { get { return new DTDFiles("MYEG_JPJ1_Kpp_Req.dtd"); } }
    }
    public class Action
    {
        private Action(string value) { Value = value; }
        public string Value { get; private set; }
        public static Action ACT_MYEG_JPJ1_Declaration { get { return new Action("ACT_MYEG_JPJ1_Declaration"); } }
        public static Action ACT_MYEG_JPJ1_REG { get { return new Action("ACT_MYEG_JPJ1_REG"); } }
        public static Action ACT_JPJ1_InsertResult { get { return new Action("ACT_JPJ1_InsertResult"); } }
        public static Action ACT_MYEG_JPJ1_KPP { get { return new Action("ACT_MYEG_JPJ1_KPP"); } }
        public static Action ACT_JPJ1_RecordSeminarPraL { get { return new Action("ACT_JPJ1_RecordSeminarPraL"); } }
    }
    // StudentCourse myDeserializedClass = JsonConvert.DeserializeObject<StudentCourse>(myJsonResponse);
    public class StudentCourse
    {
        public long SCID { get; set; }
        public long SCStuID { get; set; }
        public int SCCourseID { get; set; }
        public DateTime SCDate { get; set; }
        public int SCHQID { get; set; }
        public DateTime SCCourseDate { get; set; }
        public DateTime SCCourseDateEnd { get; set; }
        public double SCPrice { get; set; }
        public double SCTax { get; set; }
        public int SCInsID { get; set; }
        public bool SCPaxFlag { get; set; }
        public long SCClassID { get; set; }
        public DateTime SCInsTimeIn { get; set; }
        public DateTime SCInsTimeOut { get; set; }
        public DateTime SCStuTimeIn { get; set; }
        public DateTime SCStuTimeOut { get; set; }
        public int SCResult { get; set; }
        public int SCVehicleID { get; set; }
        public int SCLicenseID { get; set; }
        public int SCCertNo { get; set; }
        public int SCQueue { get; set; }
        public string SCFormSM4 { get; set; }
        public long SCInsClassID { get; set; }
        public int SCCommFlag { get; set; }
        public int SCPartnerID { get; set; }
        public int SCDuration { get; set; }
        public double SCSession { get; set; }
        public int SCStatus { get; set; }
        public int SCSLID { get; set; }
        public DateTime SCPracLicenseEnd { get; set; }
        public int SCJPJNo { get; set; }
        public bool SCRepeat { get; set; }
        public int SCAPIFlag { get; set; }
        public object SCStep { get; set; }
    }


    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Request_UD
    {
        public string security_code { get; set; }
        public string action { get; set; }
        public string request_date { get; set; }
        public string request_time { get; set; }
        public string prim_id_no { get; set; }
        public string prim_id_type { get; set; }
        public string sec_id_no { get; set; }
        public string sec_id_type { get; set; }
        public string declaration_a { get; set; }
        public string declaration_b { get; set; }
        public string declaration_c { get; set; }
        public string declaration_d { get; set; }
        public string declaration_e { get; set; }
        public string declaration_f { get; set; }
        public string declaration_z { get; set; }
        public string remark { get; set; }
    }
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Request_UR
    {
        public string security_code { get; set; }
        public string action { get; set; }
        public string request_date { get; set; }
        public string request_time { get; set; }
        public string name_1 { get; set; }
        public string name_2 { get; set; }
        public string name_3 { get; set; }
        public string prim_id_no { get; set; }
        public string prim_id_type { get; set; }
        public string sec_id_no { get; set; }
        public string sec_id_type { get; set; }
        public string gender { get; set; }
        public string place_of_birth { get; set; }
        public string DOB { get; set; }
        public string address_1 { get; set; }
        public string address_2 { get; set; }
        public string address_3 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string postcode { get; set; }
        public string country { get; set; }
        public string house_tel { get; set; }
        public string handphone_no { get; set; }
        public string fax { get; set; }
        public string email { get; set; }
        public string ag_code { get; set; }
    }
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Request_DI
    {
        public string security_code { get; set; }
        public string action { get; set; }
        public string request_date { get; set; }
        public string request_time { get; set; }
        public string id_no { get; set; }
        public string id_type { get; set; }
        public string ag_code { get; set; }
        public string owner_code { get; set; }
        public string officer_code { get; set; }
        public string ht_rslt_status { get; set; }
        public string ht_rslt_date { get; set; }
        public string ht_rslt_time { get; set; }
        public string ct_rslt_status { get; set; }
        public string ct_rslt_date { get; set; }
        public string ct_rslt_time { get; set; }
    }
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Request_ATTD
    {
        public string security_code { get; set; }
        public string action { get; set; }
        public string request_date { get; set; }
        public string request_time { get; set; }
        public string id_no { get; set; }
        public string id_type { get; set; }
        public string class_type_1 { get; set; }
        public string class_type_2 { get; set; }
        public string class_type_3 { get; set; }
        public string attend_date { get; set; }
        public string attend_time { get; set; }
        public string cert_no { get; set; }
        public string sm_code { get; set; }
        public string ag_code { get; set; }
        public string owner_code { get; set; }
        public string di_instructor { get; set; }
        public string book_serial_no { get; set; }
    }
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Request_PRAL
    {
        public string security_code { get; set; }
        public string action { get; set; }
        public string request_date { get; set; }
        public string request_time { get; set; }
        public string id_no { get; set; }
        public string id_type { get; set; }
        public string ag_code { get; set; }
        public string owner_code { get; set; }
        public string cert_no { get; set; }
        public string sm_code { get; set; }
        public string indicator { get; set; }
        public string developer_id { get; set; }
        public string theory_date1 { get; set; }
        public string theory_start_time1 { get; set; }
        public string theory_end_time1 { get; set; }
        public string theory_instructor1 { get; set; }
        public string theory_date2 { get; set; }
        public string theory_start_time2 { get; set; }
        public string theory_end_time2 { get; set; }
        public string theory_instructor2 { get; set; }
        public string theory_vehicle_reg_no2 { get; set; }
        public string practical_date1 { get; set; }
        public string practical_start_time1 { get; set; }
        public string practical_end_time1 { get; set; }
        public string practical_instructor1 { get; set; }
        public string practical_vehicle_reg_no1 { get; set; }
        public string practical_date2 { get; set; }
        public string practical_start_time2 { get; set; }
        public string practical_end_time2 { get; set; }
        public string practical_instructor2 { get; set; }
        public string practical_vehicle_reg_no2 { get; set; }
        public string practical_date3 { get; set; }
        public string practical_start_time3 { get; set; }
        public string practical_end_time3 { get; set; }
        public string practical_instructor3 { get; set; }
        public string practical_vehicle_reg_no3 { get; set; }
    }
}
