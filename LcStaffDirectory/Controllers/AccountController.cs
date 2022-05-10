using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using LcStaffDirectory.Models;


namespace LcStaffDirectory.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            if (Membership.ValidateUser(model.UserName, model.Password))
            {
                FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                if (this.Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                    && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                {
                    return this.Redirect(returnUrl);
                }

                return this.RedirectToAction("Index", "Staff");
            }
            else {
                model.ErrorMsg = "Invalid username or password";
            }

            this.ModelState.AddModelError(string.Empty, "The user name or password provided is incorrect.");

            return this.View(model);
        }

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();

            return this.RedirectToAction("Index", "Staff");
        }
        //public void SignIn()
        //{
        //    // Send an OpenID Connect sign-in request.
        //    if (!Request.IsAuthenticated)
        //    {
        //        HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = "/" },
        //            OpenIdConnectAuthenticationDefaults.AuthenticationType);
        //    }
        //}

        //public void SignOut()
        //{
        //    string callbackUrl = Url.Action("SignOutCallback", "Account", routeValues: null, protocol: Request.Url.Scheme);

        //    HttpContext.GetOwinContext().Authentication.SignOut(
        //        new AuthenticationProperties { RedirectUri = callbackUrl },
        //        OpenIdConnectAuthenticationDefaults.AuthenticationType, CookieAuthenticationDefaults.AuthenticationType);
        //}

        //public ActionResult SignOutCallback()
        //{
        //    if (Request.IsAuthenticated)
        //    {
        //        // Redirect to home page if the user is authenticated.
        //        return RedirectToAction("Index", "Home");
        //    }

        //    return View();
        //}
    }
}
