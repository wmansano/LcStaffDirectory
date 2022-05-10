using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace LcStaffDirectory.Models
{
    public class DirectoryModel : IDisposable {
        #region private properties

        private string _NameTitlePhoneField;
       
        private AlphaLookup _AlphaLookup;
        private SearchNameTitlePhoneType _SearchNameTitlePhoneType;
        private SearchTypeType _SearchType = SearchTypeType.NameTitlePhoneSearchType;
        private SearchOperator _SearchOperator;
        private string _DeptType;
        private bool _Validated;
        private string _Path;
        private string _SubDomain = "Not Hit";
        private List<Employee> _EmployeeList = new List<Employee>();
        private List<SelectListItem> _SearchTypes;
        private List<SelectListItem> _DeptTypes; 
        private PaginationFilter _PaginationFilter = new PaginationFilter();

        private imp_reporting_entities ictx = new imp_reporting_entities();

        #endregion

        #region constructors
        public DirectoryModel() {
            LoadDefaults();
        }

        #endregion

        #region methods

        public bool Load() {
            bool success = false;
            try
            {
                _Validated = LoadData();
                success = _Validated;
            }
            catch (Exception ex)
            {
                string msg = ex.ToString();
            }
            return success;
        }

        public bool LoadData()
        {
            bool success = false;
            try
            {

                IQueryable<Employee> results = LoadStaffData();

                switch (_SearchType) {
                    case SearchTypeType.AlphabeticalSearchType:
                        results = results.Where(x=>x.LastName.StartsWith(_AlphaLookup.ToString().ToUpper()));
                        break;
                    case SearchTypeType.DepartmentSearchType:
                        results = results.Where(x => x.DeptCode == _DeptType.ToString());
                        break;
                    case SearchTypeType.NameTitlePhoneSearchType:

                        switch (_SearchOperator) {
                            case SearchOperator.equals:
                                switch (_SearchNameTitlePhoneType)
                                {
                                    case SearchNameTitlePhoneType.FirstName:
                                        results = results.Where(x => x.FirstName.ToUpper() == _NameTitlePhoneField.ToUpper());
                                        break;
                                    case SearchNameTitlePhoneType.LastName:
                                        results = results.Where(x => x.LastName.ToUpper() == _NameTitlePhoneField.ToUpper());
                                        break;
                                    case SearchNameTitlePhoneType.Title:
                                        results = results.Where(x => x.InstrTitle.ToUpper() == _NameTitlePhoneField.ToUpper());
                                        break;
                                    case SearchNameTitlePhoneType.Phone:
                                        results = results.Where(x => x.Phone.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "").Trim().ToUpper() == 
                                                                        _NameTitlePhoneField.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "").Trim().ToUpper());
                                        break;
                                }
                                break;
                            case SearchOperator.contains:
                                switch (_SearchNameTitlePhoneType)
                                {
                                    case SearchNameTitlePhoneType.FirstName:
                                        results = results.Where(x => x.FirstName.ToUpper().Contains(_NameTitlePhoneField.ToUpper()));
                                        break;
                                    case SearchNameTitlePhoneType.LastName:
                                        results = results.Where(x => x.LastName.ToUpper().Contains(_NameTitlePhoneField.ToUpper()));
                                        break;
                                    case SearchNameTitlePhoneType.Title:
                                        results = results.Where(x => x.InstrTitle.ToUpper().Contains(_NameTitlePhoneField.ToUpper()));
                                        break;
                                    case SearchNameTitlePhoneType.Phone:
                                        results = results.Where(x => x.Phone.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "").Trim().ToUpper()
                                                                    .Contains(_NameTitlePhoneField.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "").Trim().ToUpper()));
                                        break;
                                }
                                break;
                            case SearchOperator.begins:
                                switch (_SearchNameTitlePhoneType)
                                {
                                    case SearchNameTitlePhoneType.FirstName:
                                        results = results.Where(x => x.FirstName.ToUpper().StartsWith(_NameTitlePhoneField.ToUpper()));
                                        break;
                                    case SearchNameTitlePhoneType.LastName:
                                        results = results.Where(x => x.LastName.ToUpper().StartsWith(_NameTitlePhoneField.ToUpper()));
                                        break;
                                    case SearchNameTitlePhoneType.Title:
                                        results = results.Where(x => x.InstrTitle.ToUpper().StartsWith(_NameTitlePhoneField.ToUpper()));
                                        break;
                                    case SearchNameTitlePhoneType.Phone:
                                        results = results.Where(x => x.Phone.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "").Trim().ToUpper()
                                                                    .StartsWith(_NameTitlePhoneField.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "").Trim().ToUpper()));
                                        break;
                                }
                                break;
                        }
                        break;
                    default:
                        break;
                }

                if (results.Count() > 0)
                {
                    _EmployeeList = results.OrderBy(x=>x.LastName).Skip((_PaginationFilter.PageIndex - 1) * _PaginationFilter.PageSize).Take(_PaginationFilter.PageSize).ToList();

                    _PaginationFilter.RecCount = results.Count();
                    _PaginationFilter.PageCount = (results.Count() + _PaginationFilter.PageSize - 1) / _PaginationFilter.PageSize;
                }
                success = true;
            }
            catch (Exception ex)
            {
                string msg = ex.ToString();
            }

            return success;
        }

        public IQueryable<Employee>  LoadStaffData() {
            IQueryable<Employee> results = null;
            try
            {
                IQueryable<Employee> _SessionResults = (IQueryable<Employee>)HttpContext.Current.Session["results"];

                if (_SessionResults == null || _SessionResults.Count() < 0)
                {

                    // pulling directly from imp_reporting vwcorpdirs
                    results = (from c in ictx.vwcorpdirs
                               where c.DIRECTORY != "N"
                               select new Employee
                               {
                                   Snumber = c.EMPLOYEEID.Trim(),
                                   FirstName = c.FIRSTNAME.Trim(),
                                   LastName = c.LASTNAME.Trim(),
                                   FullName = c.DISPLAYNAME.Trim(),
                                   Room = c.OFFICE.Trim(),
                                   Phone = c.PHONE.Trim(),
                                   Email = c.MAIL.Trim(),
                                   Show = c.DIRECTORY == "Y" ? true : false,
                                   SchoolCode = c.DIVISIONID.Trim(),
                                   SchoolName = c.DIVISION.Trim(),
                                   DeptCode = c.DEPTMENTID.Trim(),
                                   DeptName = c.DEPTMENT.Trim(),
                                   InstrCode = c.POSITIONID.Trim(),
                                   InstrTitle = c.POSITION.Trim(),

                               }).AsQueryable();

                    HttpContext.Current.Session["results"] = results;
                }
                else
                {
                    results = _SessionResults;
                }
            }
            catch (Exception ex)
            {
                string msg = ex.ToString();
            }
            return results;
        }

        private List<SelectListItem> GetLookupTypes()
        {
            List<SelectListItem> lookupTypes = new List<SelectListItem>();

            try
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>() {
                    { "By Name, Title or Phone", "0" },
                    { "By Department", "1" },
                    { "Alphabetically", "2" },
                };

                foreach (KeyValuePair<string, string> pair in dictionary) {
                    lookupTypes.Add(new SelectListItem() { Text = pair.Key, Value = pair.Value, Selected = false });
                }
            }
            catch (Exception ex)
            {
                string msg = ex.ToString();
                return null;
            }

            lookupTypes[0].Selected = true;

            return lookupTypes;
        }

        private List<SelectListItem> GetDepartmentTypes()
        {

            try
            {
                List<SelectListItem> _SessionDepts = (List<SelectListItem>)HttpContext.Current.Session["depttypes"];

                if (_SessionDepts == null || _SessionDepts.Count() < 0)
                {
                    DeptTypes = (from c in ictx.vwcorpdirs
                                   where c.DIRECTORY != "N"
                                   select new SelectListItem
                                   {
                                       Value = c.DEPTMENTID.Trim(),
                                       Text = c.DEPTMENT.Trim(),
                                   }).Distinct().OrderBy(x => x.Text).ToList();

                    HttpContext.Current.Session["depttypes"] = DeptTypes; ;

                }
                else {
                    DeptTypes = _SessionDepts;
                }

                DeptTypes[0].Selected = true;

            }
            catch (Exception ex)
            {
                string msg = ex.ToString();
                return null;
            }

            DeptTypes[0].Selected = true;

            return DeptTypes;
        }

        private void LoadDefaults() {
            if (_SearchTypes == null) {
                _SearchTypes = GetLookupTypes();
                _DeptTypes = GetDepartmentTypes();
            }
        }

        public void Dispose()
        {
            ictx.Dispose();
            this.Dispose();
        }

        #endregion

        #region public properties
        public List<Employee> EmployeeList { get { return _EmployeeList; } set { _EmployeeList = value; } }

        [Display(Name = "Search Type")]
        public SearchNameTitlePhoneType SearchNameTitleType { get { return _SearchNameTitlePhoneType; } set { _SearchNameTitlePhoneType = value; } }

        [Display(Name = "Search Operator")]
        public SearchOperator SearchOperator { get { return _SearchOperator; } set { _SearchOperator = value; } }

        [Display(Name = "Department")]
        public string DeptType { get { return _DeptType; } set { _DeptType = value; } }

        public string SearchType {
            get {
                return _SearchType.ToString();
            }
            set {
                _SearchType = (SearchTypeType)Enum.Parse(typeof(SearchTypeType), value);
            } }

        public string NameTitleField { get { return _NameTitlePhoneField; } set { _NameTitlePhoneField = value; } }

        public AlphaLookup AlphaLookup { get { return _AlphaLookup; } set { _AlphaLookup = value; } }

        public List<SelectListItem> SearchTypes {
            get {
                return  _SearchTypes;
            }
            set {
                _SearchTypes = value;
            }
        }

        public List<SelectListItem> DeptTypes
        {
            get
            {
                return _DeptTypes;
            }
            set
            {
                _DeptTypes = value;
            }
        }

        public bool Validated { get { return _Validated; } set { _Validated = true; } }

        public string Path { get { return _Path; } set { _Path = value; } }

        public string SubDomain { get { return _SubDomain; } }

        public PaginationFilter Pagination { get { return _PaginationFilter; } set { _PaginationFilter = value; } }
        #endregion
    }

    public class Employee
    {
        #region private properties
        private string _Snumber;
        private string _FirstName;
        private string _LastName;
        private string _FullName;
        private string _Room;
        private string _Phone;
        private string _Email;
        private bool _Show;
        private string _SchoolCode;
        private string _SchoolName;
        private string _DeptCode;
        private string _DeptName;
        private string _InstrCode;
        private string _InstrTitle;
        #endregion

        #region constructors
        public Employee() {

        }
        #endregion

        #region methods
        #endregion

        #region public properties
        [Display(Name = "sNumber")]
        public string Snumber { get { return _Snumber; } set { _Snumber = value; } }
        [Display(Name = "First Name")]
        public string FirstName { get { return _FirstName; } set { _FirstName = value; } }
        [Display(Name = "Last Name")]
        public string LastName { get { return _LastName; } set { _LastName = value; } }
        [Display(Name = "Full Name")]
        public string FullName { get { return _FullName; } set { _FullName = value; } }
        [Display(Name = "Room")]
        public string Room { get { return _Room; } set { _Room = value; } }
        [Display(Name = "Phone")]
        public string Phone { get { return _Phone; } set { _Phone = value; } }
        [Display(Name = "Email")]
        public string Email { get { return _Email; } set { _Email = value; } }
        [Display(Name = "Show")]
        public bool Show { get { return _Show; } set { _Show = value; } }
        [Display(Name = "School Code")]
        public string SchoolCode { get { return _SchoolCode; } set { _SchoolCode = value; } }
        [Display(Name = "School Name")]
        public string SchoolName { get { return _SchoolName; } set { _SchoolName = value; } }
        [Display(Name = "Department Code")]
        public string DeptCode { get { return _DeptCode; } set { _DeptCode = value; } }
        [Display(Name = "Department Name")]
        public string DeptName { get { return _DeptName; } set { _DeptName = value; } }
        [Display(Name = "Instructor Code")]
        public string InstrCode { get { return _InstrCode; } set { _InstrCode = value; } }
        [Display(Name = "Instructor Title")]
        public string InstrTitle { get { return _InstrTitle; } set { _InstrTitle = value; } }
        #endregion
    }

    public class DeptType {
        private string _deptCode = string.Empty;
        private string _deptName = string.Empty;

        [DataType(DataType.Text)]
        [Display(Name = "Department Code: ")]
        public string DeptCode {
            get {
                return _deptCode;
            }
            set {
                _deptCode = value;
            }
        }

        [DataType(DataType.Text)]
        [Display(Name = "Department Name: ")]
        public string DeptName
        {
            get
            {
                return _deptName;
            }
            set
            {
                _deptName = value;
            }
        }
    }



    public class PaginationFilter
    {
        #region private variables
        private int _pageSize = 10;
        private int _pageIndex = 1;
        private int _pageCount = 1;
        private int _recCount = 0;

        private IEnumerable<SelectListItem> _PageSizes;
        #endregion private variables

        #region methods
        public PaginationFilter()
        {

        }

        
        #endregion methods

        #region public variables
        [Display(Name = "Page Size: ")]
        [DataType(DataType.Text)]
        public int PageSize { get { return _pageSize; } set { _pageSize = value; } }

        [DataType(DataType.Text)]
        [Display(Name = "Page: ")]
        public int PageIndex { get { return _pageIndex; } set { _pageIndex = value; } }

        [DataType(DataType.Text)]
        [Display(Name = "Page Count: ")]
        public int PageCount { get { return _pageCount; } set { _pageCount = value; } }

        [DataType(DataType.Text)]
        [Display(Name = "Total Results: ")]
        public int RecCount { get { return _recCount; } set { _recCount = value; } }

        [Display(Name = "Results/Page: ")]
        public IEnumerable<SelectListItem> PageSizes {
            get
            {
                return new List<SelectListItem>
                {
                    new SelectListItem { Text = "10", Value = "10", Selected = true },
                    new SelectListItem { Text = "25", Value = "25"},
                    new SelectListItem { Text = "50", Value = "50"},
                    new SelectListItem { Text = "75", Value = "75"},
                    new SelectListItem { Text = "100", Value = "100"},
                    new SelectListItem { Text = "200", Value = "200"}
                };
            }
        }

        #endregion public variables

    }

    public enum SearchTypeType
    {
        [Display(Name = "by Name, Title or Phone")]
        NameTitlePhoneSearchType = 0,
        [Display(Name = "by Department")]
        DepartmentSearchType = 1,
        [Display(Name = "Alphabetically")]
        AlphabeticalSearchType = 2,
    }
        
    public enum SearchNameTitlePhoneType
    {
        [Display(Name = "First Name")]
        FirstName = 0,
        [Display(Name = "Last Name")]
        LastName = 1,
        [Display(Name = "Title")]
        Title = 2,
        [Display(Name = "Phone")]
        Phone = 3,
    }

    public enum SearchOperator
    {
        [Display(Name = "Equals")]
        equals = 0,
        [Display(Name = "Contains")]
        contains = 1,
        [Display(Name = "Begins With")]
        begins = 2,
    }

    //public enum DeptType {
    //    [Display(Name = "Accessibility Services")]
    //    ACCS = 1,
    //    [Display(Name = "Accounting Services")]
    //    ACSV = 2,
    //    [Display(Name = "Admin Office Professional")]
    //    AOP = 3,
    //    [Display(Name = "Admissions")]
    //    ADMS = 4,
    //    [Display(Name = "Advancement")]
    //    ADVT = 5,
    //    [Display(Name = "Advising")]
    //    ADVS = 6,
    //    [Display(Name = "Agricultural & Heavy Equipment")]
    //    AHM = 7,
    //    [Display(Name = "Agricultural Enterprise Mngmnt")]
    //    AEM = 8,
    //    [Display(Name = "Agriculture Sciences")]
    //    AGS = 9,
    //    [Display(Name = "Agriculture")]
    //    AGR = 10,
    //    [Display(Name = "Allied Health & Wellness Admin")]
    //    AHWA = 11,
    //    [Display(Name = "Allied Health & Wellness")]
    //    AHW = 12,
    //    [Display(Name = "Applied Arts & Science Admin")]
    //    AASA = 13,
    //    [Display(Name = "Applied Management Admin")]
    //    AMGT = 14,
    //    [Display(Name = "Applied Research")]
    //    APRS = 15,
    //    [Display(Name = "Athletics")]
    //    DRAT = 16,
    //    [Display(Name = "Audio Visual")]
    //    AUDV = 17,
    //    [Display(Name = "Auto Serv Tech - Apprentice")]
    //    MOT = 18,
    //    [Display(Name = "Bookstore")]
    //    BKST = 19,
    //    [Display(Name = "Buchanan Library")]
    //    LIBR = 20,
    //    [Display(Name = "Building Maintenance")]
    //    BDMA = 21,
    //    [Display(Name = "Business Adminstration")]
    //    BUS = 22,
    //    [Display(Name = "Business Development")]
    //    BDEV = 23,
    //    [Display(Name = "Business Train & Dvlpmnt Admin")]
    //    BTDA = 24,
    //    [Display(Name = "Business Training/Development")]
    //    BTD = 25,
    //    [Display(Name = "Caretaking")]
    //    CRTK = 26,
    //    [Display(Name = "Carpentry - Apprentice")]
    //    CRP = 27,
    //    [Display(Name = "Child and Youth Care")]
    //    CYC = 28,
    //    [Display(Name = "Civil Engineering Technology")]
    //    CIV = 29,
    //    [Display(Name = "College/University Preparation")]
    //    CUP = 30,
    //    [Display(Name = "Computer Info Technology")]
    //    CIT = 31,
    //    [Display(Name = "Conservation Enforcement")]
    //    CEN = 32,
    //    [Display(Name = "Construction Trades")]
    //    TRAD = 33,
    //    [Display(Name = "Correctional Centre Admin")]
    //    CRCT = 34,
    //    [Display(Name = "Correctional Centre")]
    //    CRC = 35,
    //    [Display(Name = "Correctional Studies")]
    //    COR = 36,
    //    [Display(Name = "Criminal Justice")]
    //    CJP = 37,
    //    [Display(Name = "Culinary Arts")]
    //    CUL = 38,
    //    [Display(Name = "Customer Services")]
    //    CSVR = 39,
    //    [Display(Name = "Development Office")]
    //    DVOF = 40,
    //    [Display(Name = "Digital Communications & Media")]
    //    DCM = 41,
    //    [Display(Name = "Disability and Community Rehab")]
    //    DCR = 42,
    //    [Display(Name = "E.S.L. - Admin")]
    //    ESLA = 43,
    //    [Display(Name = "Early Childhood Education")]
    //    ECE = 44,
    //    [Display(Name = "Education Enhancement Team")]
    //    EDET = 45,
    //    [Display(Name = "Educational Assistant")]
    //    EDA = 46,
    //    [Display(Name = "Electrical - Apprentice")]
    //    ELT = 47,
    //    [Display(Name = "Engineering Design & Drafting")]
    //    EDD = 48,
    //    [Display(Name = "English As a Second Language")]
    //    ESL = 49,
    //    [Display(Name = "Enterprise Application Systems")]
    //    EASD = 50,
    //    [Display(Name = "Environmental Assess & Restor")]
    //    EAR = 51,
    //    [Display(Name = "Exercise Science")]
    //    EXS = 52,
    //    [Display(Name = "Facilites Management")]
    //    DRPF = 53,
    //    [Display(Name = "Fashion Desgn & Sustain Prod")]
    //    FSP = 54,
    //    [Display(Name = "Financial Services")]
    //    DRFS = 55,
    //    [Display(Name = "Fitness & Recreation Admin")]
    //    FTRC = 56,
    //    [Display(Name = "Fitness & Recreation")]
    //    FTR = 57,
    //    [Display(Name = "Food Services")]
    //    FDSV = 58,
    //    [Display(Name = "General Studies")]
    //    GNS = 59,
    //    [Display(Name = "Geomatics Engineering Tech")]
    //    GET = 60,
    //    [Display(Name = "Grounds")]
    //    GRND = 61,
    //    [Display(Name = "Health & Wellness Admin")]
    //    HTWL = 62,
    //    [Display(Name = "Health and Safety")]
    //    SAFE = 63,
    //    [Display(Name = "Health Services")]
    //    HESV = 64,
    //    [Display(Name = "Heavy Equipment - Apprentice")]
    //    MEC = 65,
    //    [Display(Name = "Human Resources")]
    //    DRHR = 66,
    //    [Display(Name = "Human Services - Admin")]
    //    HMSV = 67,
    //    [Display(Name = "Inclusive Post Sec Ed Admin")]
    //    CCON = 68,
    //    [Display(Name = "Industr & Tech Training Admin")]
    //    ITTA = 69,
    //    [Display(Name = "Information Technology Serv.")]
    //    ITSV = 70,
    //    [Display(Name = "Interior Design Technology")]
    //    IDT = 71,
    //    [Display(Name = "Interior Design&Merchandising")]
    //    IDM = 72,
    //    [Display(Name = "International Services")]
    //    INTL = 73,
    //    [Display(Name = "Justice & Human Services Admin")]
    //    JSHS = 74,
    //    [Display(Name = "Justice Studies")]
    //    JUS = 75,
    //    [Display(Name = "LCC Faculty Association")]
    //    LCFA = 76,
    //    [Display(Name = "Learning Cafe")]
    //    LCF = 77,
    //    [Display(Name = "Marketing & Communications")]
    //    MKCM = 78,
    //    [Display(Name = "Massage Therapy")]
    //    MAS = 79,
    //    [Display(Name = "Mech./Electrical Maintenance")]
    //    MELM = 80,
    //    [Display(Name = "Multimedia Production")]
    //    MMP = 81,
    //    [Display(Name = "Network & Infrastructure")]
    //    NTIN = 82,
    //    [Display(Name = "Nursing")]
    //    NSG = 83,
    //    [Display(Name = "Partsman-Apprentice")]
    //    PRT = 84,
    //    [Display(Name = "Payroll Services")]
    //    PYSV = 85,
    //    [Display(Name = "PE Building")]
    //    PEBD = 86,
    //    [Display(Name = "Placement Office")]
    //    PLCE = 87,
    //    [Display(Name = "Planning & Reporting")]
    //    PLRP = 88,
    //    [Display(Name = "Plumbing - Apprenticeship")]
    //    PLM = 89,
    //    [Display(Name = "Practical Nursing")]
    //    PNG = 90,
    //    [Display(Name = "Presidents Office")]
    //    PRES = 91,
    //    [Display(Name = "Procurement Services")]
    //    PRSV = 92,
    //    [Display(Name = "Recruitment")]
    //    RCRT = 93,
    //    [Display(Name = "Regional Stewardship")]
    //    RSTW = 94,
    //    [Display(Name = "Registration")]
    //    RGST = 95,
    //    [Display(Name = "Research Bio Aqua Centre Excl")]
    //    RBAQ = 96,
    //    [Display(Name = "Residence Life")]
    //    HOUS = 97,
    //    [Display(Name = "Scheduling")]
    //    SCHD = 98,
    //    [Display(Name = "Security")]
    //    SCRT = 99,
    //    [Display(Name = "Student Association")]
    //    STAS = 100,
    //    [Display(Name = "Student Awards")]
    //    AWRD = 101,
    //    [Display(Name = "Student Engagement & Rententio")]
    //    STER = 102,
    //    [Display(Name = "Student Experience")]
    //    STEX = 103,
    //    [Display(Name = "Student Services")]
    //    STSV = 104,
    //    [Display(Name = "Teaching Learn & Innov Admin")]
    //    TLIN = 105,
    //    [Display(Name = "Tech Env & Design Admin")]
    //    TEDE = 106,
    //    [Display(Name = "Testing Services")]
    //    TSAS = 107,
    //    [Display(Name = "Therapeutic Recreation")]
    //    TRG = 108,
    //    [Display(Name = "VP Academic & COO")]
    //    VPCI = 109,
    //    [Display(Name = "VP Corporate Services")]
    //    VPCS = 110,
    //    [Display(Name = "Welding - Apprentice")]
    //    WDG = 111,
    //    [Display(Name = "Wellness Centre")]
    //    WLNS = 112,
    //    [Display(Name = "Wind Turbine Technician")]
    //    WTT = 113,
    //}

    public enum AlphaLookup {
        [Display(Name = "A")]
        A = 0,
        [Display(Name = "B")]
        B = 1,
        [Display(Name = "C")]
        C = 2,
        [Display(Name = "D")]
        D = 3,
        [Display(Name = "E")]
        E = 4,
        [Display(Name = "F")]
        F = 5,
        [Display(Name = "G")]
        G = 6,
        [Display(Name = "H")]
        H = 7,
        [Display(Name = "I")]
        I = 8,
        [Display(Name = "J")]
        J = 9,
        [Display(Name = "K")]
        K = 10,
        [Display(Name = "L")]
        L = 11,
        [Display(Name = "M")]
        M = 12,
        [Display(Name = "N")]
        N = 13,
        [Display(Name = "O")]
        O = 14,
        [Display(Name = "P")]
        P = 15,
        [Display(Name = "Q")]
        Q = 16,
        [Display(Name = "R")]
        R = 17,
        [Display(Name = "S")]
        S = 18,
        [Display(Name = "T")]
        T = 19,
        [Display(Name = "U")]
        U = 20,
        [Display(Name = "V")]
        V = 21,
        [Display(Name = "W")]
        W = 22,
        [Display(Name = "X")]
        X = 23,
        [Display(Name = "Y")]
        Y = 24,
        [Display(Name = "Z")]
        Z = 25,
    }

}