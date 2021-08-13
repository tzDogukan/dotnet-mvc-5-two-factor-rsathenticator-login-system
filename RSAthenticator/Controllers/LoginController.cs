using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using Google.Authenticator;
using RSAthenticator.Models.Entity;

namespace RSAuthenticator.Controllers
{
    public class LoginController : Controller
    {
        authenticationEntities db = new authenticationEntities();

        public ActionResult Index()
        {
            return View();
        }
        //   string userName = WebConfigurationManager.AppSettings["GAuthPrivateKey"]
        //  private const string key = "Z234sfSS"; //You can add your own Key
        public ActionResult Login()
        {           
            return View();
        }
        [HttpPost]
        public ActionResult Login(users login)
        {
            string message = "";
            bool status = false;
            //db username pw check 
            string GAuthPrivKey = WebConfigurationManager.AppSettings["GAuthPrivateKey"];
            string UserUniqueKey = (login.UserName + GAuthPrivKey);



            login.Password = md5hash.hashmd5(md5hash.hashmd5(login.Password) + "@F=$½tV4c5£Un$}₺->$i#9{TMC5hZ½1@F29]U4JYF*");

            var usersVarmi = db.users.FirstOrDefault(x => x.UserName == login.UserName && x.Password == login.Password);

            if (usersVarmi != null)

            {
                status = true;
                Session["UserName"] = login.UserName;

                if (WebConfigurationManager.AppSettings["GAuthEnable"].ToString() == "1")
                {
                    HttpCookie TwoFCookie = Request.Cookies["TwoFCookie"];
                    int k = 0;
                    if (TwoFCookie == null)
                    {
                        k = 1;
                    }
                    else
                    {

                        if (!string.IsNullOrEmpty(TwoFCookie.Values["UserCode"]))
                        {
                            string UserCodeE = TwoFCookie.Values["UserCode"].ToString();
                            string UserCodeD = Encoding.UTF8.GetString(MachineKey.Unprotect(Convert.FromBase64String(UserCodeE)));


                            if (UserUniqueKey == UserCodeD)
                            {
                                FormsAuthentication.SetAuthCookie(Session["Username"].ToString(), false);
                                ViewBag.Message = "Welcome to Mr. " + Session["Username"].ToString();
                                //       return View("UserProfile");
                                return RedirectToAction("UserProfile");
                            }
                            else
                            {
                                k = 1;
                            }


                        }
                    }

                    if (k == 1)
                    {

                        message = "Two Factor Authentication Verification";

                        //Two Factor Authentication
                        TwoFactorAuthenticator TwoFacAuth = new TwoFactorAuthenticator();

                        Session["UserUniqueKey"] = UserUniqueKey;
                        var setupInfo = TwoFacAuth.GenerateSetupCode("HaneefPuttur.com", login.UserName, UserUniqueKey, 300, 300);
                        ViewBag.BarcodeImageUrl = setupInfo.QrCodeSetupImageUrl;
                        ViewBag.SetupCode = setupInfo.ManualEntryKey;
                    }
                }
                else
                {
                    FormsAuthentication.SetAuthCookie(Session["Username"].ToString(), true);
                    ViewBag.Message = "Welcome to Mr. " + Session["Username"].ToString();
                    //       return View("UserProfile");
                    return RedirectToAction("UserProfile");
                }

            }

            else
            {
                message = "Please Enter the Valid Credential!";
            }
            ViewBag.Message = message;
            ViewBag.Status = status;
            return View();
        }
        [Authorize]
        public ActionResult UserProfile()
        {

            ViewBag.Message = "Welcome to  " + Session["Username"].ToString();
            return View();
        }

        public ActionResult TwoFactorAuthenticate()
        {
            var token = Request["CodeDigit"];
            TwoFactorAuthenticator TwoFacAuth = new TwoFactorAuthenticator();
            string UserUniqueKey = Session["UserUniqueKey"].ToString();
            bool isValid = TwoFacAuth.ValidateTwoFactorPIN(UserUniqueKey, token);
            if (isValid)
            {
                HttpCookie TwoFCookie = new HttpCookie("TwoFCookie");
                string UserCode = Convert.ToBase64String(MachineKey.Protect(Encoding.UTF8.GetBytes(UserUniqueKey)));

                TwoFCookie.Values.Add("UserCode", UserCode);
                TwoFCookie.Expires = DateTime.Now.AddMinutes(45);
                Response.Cookies.Add(TwoFCookie);
                Session["IsValidTwoFactorAuthentication"] = true;
                return RedirectToAction("UserProfile", "Login");
            }
            return RedirectToAction("Login", "Login");
        }
        public ActionResult Logoff()
        {
            Session["UserName"] = null;
            FormsAuthentication.SignOut();
            FormsAuthentication.RedirectToLoginPage();
          return RedirectToAction("Login");
        }
    }
}