using Sitecore.Analytics;
using Sitecore.Diagnostics;
using System;
using System.Web.Mvc;
using IMovieRepository = NetflixSitecore.Services.IMovieRepository;
using MovieRepository = NetflixSitecore.Services.MovieRepository;

namespace NetflixSitecore.Controllers
{
    public class AccountController : Controller
    {
        protected IMovieRepository MovieRespository { get; }

        public AccountController()
        {
            MovieRespository = new MovieRepository();
        }

        [HttpPost]
        public ActionResult Index(string username, string password)
        {
            bool isAuthenticated = false;
            try
            {
                string fullUserName = System.Web.Security.Membership.GetUserNameByEmail(username);
                if (!string.IsNullOrEmpty(fullUserName) && Sitecore.Security.Accounts.User.Exists(fullUserName))
                {
                    isAuthenticated = Sitecore.Security.Authentication.AuthenticationManager.Login(fullUserName, password, true);
                }

                if (isAuthenticated)
                {
                    if (Tracker.Current == null && Tracker.Enabled)
                    {
                        Tracker.StartTracking();
                    }
                    
                    if (Tracker.Current != null)
                    {
                        Tracker.Current.Session.IdentifyAs("netflixnext", fullUserName);
                    }
                    return Redirect("http://netflixnext.com/my%20movies"); 
                }
                return Json(new { Message = "login error"});
            }
            catch (Exception ex)
            {
                Log.Error("Login Error ", ex, this);
                return Json(new { ex.Message });
            }
        }

        public ActionResult Index()
        {
            var contactid = string.Empty;
            if (Tracker.Current != null && !string.IsNullOrEmpty(Tracker.Current.Contact?.ContactId.ToString()))
            {
                contactid = Tracker.Current.Contact.ContactId.ToString();
            }

            return View(Views.LogIn, new {contactID=contactid});
        }
        
        protected static class Views
        {
            public const string LogIn = "/Views/Accounts/LogIn.cshtml";
        }

    }
}