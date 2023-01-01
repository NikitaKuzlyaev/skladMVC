using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using skladMVC.Models;
using System;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace skladMVC.Controllers
{

    public class HomeController : Controller
    {
       
        ApplicationContext db;
        public HomeController(ApplicationContext context)
        {
            db = context;
        }
       
        public string UserRole()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var loggedInUser = HttpContext.User;
            var loggedInUserName2 = "NOT FOUND";
            var loggedInUserName = "NOT FOUND";

            if (loggedInUser.Identity != null)
            {
                loggedInUserName = loggedInUser.Identity.Name;
            }

            string role = "NOT FOUND";
            if (loggedInUser.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role) != null)
            {
                loggedInUserName2 = loggedInUser.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role).Value;
                role = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
            }
            return role;
        }

        public string UserName()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var loggedInUser = HttpContext.User;
            var loggedInUserName2 = "NOT FOUND";
            var loggedInUserName = "NOT FOUND";

            if (loggedInUser.Identity != null)
            {
                loggedInUserName = loggedInUser.Identity.Name;
            }
            return loggedInUserName;
        }


        public IActionResult Index()
        {    
            ViewBag.AllOrders = db.Orders.ToList();
            ViewBag.Role = UserRole();
            ViewBag.Name = UserName();
            return View();
        }


        public IActionResult About()
        {
            ViewBag.Role = UserRole();
            ViewBag.Name = UserName();
            return View();
        }

        public IActionResult Job()
        {
            ViewBag.Role = UserRole();
            ViewBag.Name = UserName();
            ViewBag.JobList = db.Jobs.ToList();
            return View();
        }


        public IActionResult Contacts()
        {
            ViewBag.Role = UserRole();
            ViewBag.Name = UserName();
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult CheckOrder(int Id)
        {
            ViewBag.Role = UserRole();
            ViewBag.Name = UserName();
            ViewBag.tmp = Id;
            Order order =  db.Orders.Find(Id);
            ViewBag.Order = order;

            ViewBag.OrderName = db.Statuses.Find(Id).Name;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}