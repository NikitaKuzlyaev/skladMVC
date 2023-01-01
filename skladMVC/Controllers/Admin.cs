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
    public class Admin : Controller
    {
        ApplicationContext db;
        public Admin(ApplicationContext context)
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
            var loggedInUser = HttpContext.User;
            var loggedInUserName = "NOT FOUND";

            if (loggedInUser.Identity != null)
            {
                loggedInUserName = loggedInUser.Identity.Name;
            }
            return loggedInUserName;
        }


        [Authorize(Policy = "admin")]
        [Route("Admin/EditJob/{jobId}/{Name?}/{Description?}/{Logo?}")]
        public IActionResult EditJob(int jobId = 0, string Name = "", string Description = "", string Logo = "")
        {
            ViewBag.Role = UserRole();
            ViewBag.Name = UserName();

            if (Name != "")
            {
                Job job = db.Jobs.Find(jobId);
                job.Name = Name;
                job.Description = Description;
                job.Logo = Logo;

                db.SaveChanges();
                return Redirect($"~/Home/Job");
            }
            Job job2 = db.Jobs.Find(jobId);
            ViewBag.job = job2;
            return View();
        }

        [Authorize(Policy = "admin")]
        [Route("Admin/AddJob/{Name?}/{Description?}/{Logo?}")]
        public IActionResult AddJob(string Name = "", string Description = "", string Logo = "https://i.ibb.co/BP6wqLp/Screenshot-6.png")
        {
            ViewBag.Role = UserRole();
            ViewBag.Name = UserName();

            if (Name != "")
            {
                Job job = new Job();
                job.Name = Name;
                job.Logo = Logo;
                job.Description = Description;

                db.Add(job);
                db.SaveChanges();
                return Redirect($"~/Home/Job");
            }
            return View();
        }

        [Authorize(Policy = "admin")]
        [Route("Admin/DeleteJob/{jobId}")]
        public IActionResult DeleteJob(int jobId = 0)
        {
            Job job = db.Jobs.Find(jobId);

            db.Remove(job);
            db.SaveChanges();
            return Redirect($"~/Home/Job");
        }


        [Authorize(Policy = "admin")]
        [Route("Admin/Statuses")]
        public IActionResult Statuses()
        {
            ViewBag.Role = UserRole();
            ViewBag.Name = UserName();

            ViewBag.Statuses = db.Statuses.ToList();
            return View();
        }

        [Authorize(Policy = "admin")]
        [Route("Admin/AddStatus/{Name?}")]
        public IActionResult AddStatus(string Name = "")
        {
            ViewBag.Role = UserRole();
            ViewBag.Name = UserName();

            if (Name == "")
            {
                return View();
            }

            Status status = new Status();
            status.Name = Name;

            db.Add(status);
            db.SaveChanges();
            return Redirect($"~/Admin/Statuses");
        }

        [Authorize(Policy = "admin")]
        [Route("Admin/EditStatus/{Id}/{Name?}")]
        public IActionResult EditStatus(int Id = 0, string Name = "")
        {
            ViewBag.Role = UserRole();
            ViewBag.Name = UserName();

            Status status = db.Statuses.Find(Id);
            ViewBag.Status = status;

            if (Name == "")
            {
                return View();
            }

            status.Name = Name;

            db.SaveChanges();
            return Redirect($"~/Admin/Statuses");
        }

        [Authorize(Policy = "admin")]
        [Route("Admin/DeleteStatus/{Id}")]
        public IActionResult DeleteStatus(int Id = 0)
        {
            Order order = db.Orders.FirstOrDefault(o => o.StatusId == Id);
            if (order == null)
            {
                Status status = db.Statuses.Find(Id);
                db.Remove(status);
                db.SaveChanges();
            }

            return Redirect($"~/Admin/Statuses");
        }


        [Authorize(Policy = "admin")]
        [Route("Admin/Materials")]
        public IActionResult Materials()
        {
            ViewBag.Role = UserRole();
            ViewBag.Name = UserName();

            ViewBag.Materials = db.Materials.ToList();
            return View();
        }

        [Authorize(Policy = "admin")]
        [Route("Admin/AddMaterial/{Name?}")]
        public IActionResult AddMaterial(string Name = "")
        {
            ViewBag.Role = UserRole();
            ViewBag.Name = UserName();

            if (Name == "")
            {
                return View();
            }

            Material mat = new Material();
            mat.Name = Name;

            db.Add(mat);
            db.SaveChanges();
            return Redirect($"~/Admin/Materials");
        }

        [Authorize(Policy = "admin")]
        [Route("Admin/EditMaterial/{Id}/{Name?}")]
        public IActionResult EditMaterial(int Id = 0, string Name = "")
        {
            ViewBag.Role = UserRole();
            ViewBag.Name = UserName();

            Material mat = db.Materials.Find(Id);
            ViewBag.Material = mat;

            if (Name == "")
            {
                return View();
            }

            mat.Name = Name;

            db.SaveChanges();
            return Redirect($"~/Admin/Materials");
        }

        [Authorize(Policy = "admin")]
        [Route("Admin/DeleteMaterial/{Id}")]
        public IActionResult DeleteMaterial(int Id = 0)
        {
            Item item = db.Items.FirstOrDefault(o => o.MaterialId == Id);
            if (item == null)
            {
                Material mat = db.Materials.Find(Id);

                db.Remove(mat);
                db.SaveChanges();
            }
            return Redirect($"~/Admin/Materials");
        }

        [Authorize(Policy = "admin")]
        [Route("Admin/AllOrders")]
        public IActionResult AllOrders()
        {
            ViewBag.Role = UserRole();
            ViewBag.Name = UserName();

            List<Order> orders = db.Orders.ToList();
            ViewBag.Orders = orders;

            List<User> users = new List<User>();
            List<Status> status = new List<Status>();

            foreach (Order ord in orders)
            {
                User user = db.Users.Find(ord.UserId);
                Status stat = db.Statuses.Find(ord.StatusId);
                users.Add(user);
                status.Add(stat);
            }

            ViewBag.Users = users;
            ViewBag.Statuses = status;

            return View();
        }

        [Authorize(Policy = "admin")]
        [Route("Admin/EditOrder/{Id}/{StatusId?}")]
        public IActionResult EditOrder(int Id = 0, int StatusId = 0)
        {
            ViewBag.Role = UserRole();
            ViewBag.Name = UserName();

            Order order = db.Orders.Find(Id);
            ViewBag.Statuses = db.Statuses.ToList();
            ViewBag.order = order;

            List<int> items = Order.GetItemsId(order.ItemsId);
            List<int> amounts = Order.GetAmountItems(order.AmountItems);

            int count = amounts.Sum();
            ViewBag.Count = count;

            List<Item> itemsInOrder = new List<Item>();
            foreach (int it in items)
            {
                itemsInOrder.Add(db.Items.Find(it));
            }
            ViewBag.Items = itemsInOrder;
            ViewBag.Amounts = amounts;

            if (StatusId == 0)
            {
                return View();
            }
            order.StatusId = StatusId;

            db.SaveChanges();
            return Redirect($"~/Admin/AllOrders");
        }

        [Authorize(Policy = "admin")]
        [Route("Admin/CancelOrder/{OrderId}")]
        public IActionResult CancelOrder(int OrderId)
        {

            if (OrderId == null)
            {
                return Redirect("~/Admin/AllOrders");
            }

            Order order = db.Orders.Find(OrderId);
            if (order == null)
            {
                return Redirect("~/Admin/AllOrders");
            }

            List<int> items = Order.GetItemsId(order.ItemsId);
            List<int> amounts = Order.GetItemsId(order.AmountItems);

            for (int i = 0; i < items.Count; i++)
            {
                Item it = db.Items.Find(items[i]);
                it.Quantity += amounts[i];
            }

            db.Orders.Remove(order);
            db.SaveChanges();
            return Redirect("~/Admin/AllOrders");
        }


    }
}
