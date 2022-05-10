using LcStaffDirectory.Models;
using System;
using System.Web.Mvc;

namespace LcStaffDirectory.Controllers
{
    public class StaffController : Controller
    {
        public ActionResult Index(DirectoryModel model = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                DirectoryModel _Model = model ?? new DirectoryModel();
   
                try
                {
                    _Model.Validated = User.Identity.IsAuthenticated;

                    if (_Model.Validated)
                    {
                        _Model.Load();
                    }
                    else {
                        _Model = null;
                    }
                }
                catch (Exception ex)
                {
                    string msg = ex.ToString();
                }

                return View("Index", _Model);
            } else
            {
                return Redirect("https://www.lethbridgecollege.ca");
            }
        }

        //public JsonResult CheckReferrer(string referrer)
        //{
        //    bool success = false;
        //    try
        //    {
        //        if (CheckValidUrl(referrer)) {
        //            Session["validated"] = true;
        //            success = true;
        //        }
                  
        //    }
        //    catch (Exception ex)
        //    {
        //        string msg = ex.ToString();
        //    }

        //    return Json(success);
        //}

        //private bool CheckValidUrl(string referrer)
        //{
        //    bool success = false;

        //    try
        //    {
        //        if (Request.Url.Host == "localhost")
        //        {
        //            success = true;
        //        }
        //        else if (!string.IsNullOrEmpty(referrer))
        //        {
        //            referrer = referrer.Replace("https://", "");
        //            referrer = referrer.Substring(0, referrer.IndexOf("/") + 1);
        //            var nodes = referrer.Split('.');

        //            if (nodes != null && nodes.Length > 0)
        //            {
        //                if (nodes[0] == "myhorizon" && nodes[1] == "lethbridgecollege")
        //                {
        //                    success = true;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string msg = ex.ToString();
        //    }
        //    return success;
        //}
    }
}