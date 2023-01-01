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
    public class AccountController : Controller
    {

        ApplicationContext db;
        public AccountController(ApplicationContext context)
        {
            db = context;
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult LoginForm()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LoginForm(string? email, string? password)
        {
            if (email == null || password == null)
                return Redirect("~/account/loginform");

            User? user = db.Users.FirstOrDefault(p => p.Email == email.ToString() && p.Password == password.ToString());
            if (user == null)
            {
                return Redirect("~/account/loginform");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };
            var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);   

            await HttpContext.SignInAsync(claimsPrincipal);


            return Redirect("~/Home");

        }


        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("~/home");
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


        [Authorize]
        public IActionResult Profile()
        {
            ViewBag.Role = UserRole();
            ViewBag.Name = UserName();
            User user = db.Users.FirstOrDefault(u => u.Email == UserName());
            List<Order> orders = db.Orders.Where(o => o.UserId == user.Id).ToList();

            ViewBag.Orders = orders;
            ViewBag.User = user;
            return View();
        }


        [Authorize]
        public IActionResult Cart()
        {
            ViewBag.Role = UserRole();
            ViewBag.Name = UserName();
            User user = db.Users.FirstOrDefault(u => u.Email == UserName());
            ViewBag.User = user;

            List<CartItem> itemsInCart = new List<CartItem>();
            List<Item> items = new List<Item>();

            int count = 0;
            float sum = 0f;

            foreach (CartItem ci in db.CartItems.ToList())
            {
                if (ci.UserId == user.Id)
                {
                    itemsInCart.Add(ci);

                    Item item = db.Items.Find(ci.ItemId);
                    items.Add(item);


                    count += ci.Amount;
                    sum += ci.Amount * item.Cost;

                }
            }

            ViewBag.itemsInCart = itemsInCart;
            ViewBag.items = items;
            ViewBag.Count = count;
            ViewBag.Sum = sum;

            return View();
        }


        [Route("Home/CancelOrder/{OrderId}")]
        public IActionResult CancelOrder(int OrderId)
        {

            ViewBag.Role = UserRole();
            ViewBag.Name = UserName();
            User user = db.Users.FirstOrDefault(u => u.Email == UserName());
            ViewBag.User = user;

            if (OrderId == null)
            {
                return Redirect("~/Home/AllOrders");
            }

            Order order = db.Orders.Find(OrderId);
            if (order == null)
            {
                return Redirect("~/Home/AllOrders");
            }

            if (order.UserId != user.Id && UserRole() != "admin")
            {
                return Redirect("~/Home/AllOrders");
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

            return Redirect("~/Home/AllOrders");
        }


        [Authorize]
        public IActionResult ConfirmCart()
        {

            ViewBag.Role = UserRole();
            ViewBag.Name = UserName();
            User user = db.Users.FirstOrDefault(u => u.Email == UserName());
            ViewBag.User = user;

            List<CartItem> itemsInCart = new List<CartItem>();
            List<Item> items = new List<Item>();

            List<int> itemsId = new List<int>();
            List<int> itemsAmount = new List<int>();

            int count = 0;
            float sum = 0f;

            foreach (CartItem ci in db.CartItems.ToList())
            {
                if (ci.UserId == user.Id)
                {
                    itemsInCart.Add(ci);

                    Item item = db.Items.Find(ci.ItemId);
                    items.Add(item);

                    itemsId.Add(item.Id);
                    itemsAmount.Add(ci.Amount);

                    count += ci.Amount;
                    sum += ci.Amount * item.Cost;

                }
            }

            string itemsString = Order.ListToString(itemsId);
            string amountString = Order.ListToString(itemsAmount);

            ViewBag.itemsString = itemsString;
            ViewBag.amountString = amountString;
            ViewBag.itemsInCart = itemsInCart;
            ViewBag.items = items;
            ViewBag.Count = count;
            ViewBag.Sum = sum;

            return View();
        }

        [Authorize]
        [HttpPost]
        public IActionResult ConfirmCartAccepted(int UserId, string AmountItems, string ItemsId, string Address, string Comment, float Amount)
        {

            ViewBag.Role = UserRole();
            ViewBag.Name = UserName();
            User user = db.Users.FirstOrDefault(u => u.Email == UserName());
            ViewBag.User = user;



            if (Address == null)
            {
                return Redirect("~/Home/ConfirmCart");
            }

            Order order = new Order();
            order.UserId = UserId;
            order.AmountItems = AmountItems;
            order.ItemsId = ItemsId;
            order.Address = Address;
            if (Comment == null)
            {
                order.Comment = "";
            }
            else
            {
                order.Comment = Comment;
            }
            order.StatusId = 1;
            order.Amount = Amount;

            db.Orders.Add(order);
            List<CartItem> cartItems = db.CartItems.Where(c => c.UserId == UserId).ToList();

            foreach (CartItem ci in cartItems)
            {
                Item item = db.Items.FirstOrDefault(c => c.Id == ci.ItemId);
                item.Quantity = Math.Max(0, item.Quantity - ci.Amount);

                db.Remove(ci);
            }

            db.SaveChanges();

            return Redirect("~/Home");
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

        [Authorize(Policy = "admin")]
        [Route("Home/EditJob/{jobId}/{Name?}/{Description?}/{Logo?}")]
        public IActionResult EditJob(int jobId = 0, string Name = "", string Description = "", string Logo = "")
        {
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
        [Route("Home/AddJob/{Name?}/{Description?}/{Logo?}")]
        public IActionResult AddJob(string Name = "", string Description = "", string Logo = "https://i.ibb.co/BP6wqLp/Screenshot-6.png")
        {
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
        [Route("Home/DeleteJob/{jobId}")]
        public IActionResult DeleteJob(int jobId = 0)
        {
            Job job = db.Jobs.Find(jobId);
            db.Remove(job);
            db.SaveChanges();

            return Redirect($"~/Home/Job");
        }

        [Authorize]
        public IActionResult AddToCart(int ItemId, int UserId, int Amount)
        {
            ViewBag.Role = UserRole();
            ViewBag.Name = UserName();
            User user = db.Users.FirstOrDefault(u => u.Email == UserName());
            ViewBag.User = user;

            CartItem existItem = db.CartItems.FirstOrDefault(u => u.ItemId == ItemId && u.UserId == UserId);
            if (existItem != null)
            {
                int willBe = db.Items.Find(ItemId).Quantity;

                if (willBe <= existItem.Amount + Amount)
                {
                    existItem.Amount = willBe;
                }
                else
                {
                    existItem.Amount += Amount;
                }

            }
            else
            {
                CartItem cartItem = new CartItem();
                cartItem.ItemId = ItemId;
                cartItem.UserId = UserId;
                cartItem.Amount = Amount;
                db.CartItems.Add(cartItem);
            }

            db.SaveChanges();

            return Redirect($"~/Home/ItemInfo/{ItemId}");
        }



        [Authorize(Policy = "admin")]
        [Route("Home/AllOrders")]
        public IActionResult AllOrders()
        {
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


        [Authorize]
        [Route("Home/MyOrders")]
        public IActionResult MyOrders()
        {
            ViewBag.Name = UserName();
            User user = db.Users.FirstOrDefault(u => u.Email == UserName());

            //List<Order> orders = db.Orders.Where(u => u.UserId == user.Id);
            //ViewBag.Orders = orders;

            return View();
        }

        [Authorize]
        [Route("Home/ShowOrder/{Id}")]
        public IActionResult ShowOrder(int Id)
        {

            ViewBag.Role = UserRole();
            ViewBag.Name = UserName();
            User user = db.Users.FirstOrDefault(u => u.Email == UserName());
            Order order = db.Orders.Find(Id);

            if (order.UserId != user.Id && UserRole() != "admin")
            {
                return Redirect("~/Home/AllOrders");
            }

            Status st = db.Statuses.Find(order.StatusId);
            ViewBag.Status = st.Name;
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

            return View();
        }


        [Authorize(Policy = "admin")]
        [Route("Home/EditOrder/{Id}/{StatusId?}")]
        public IActionResult EditOrder(int Id = 0, int StatusId = 0)
        {
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

            return Redirect($"~/Home/AllOrders");
        }


        [Authorize(Policy = "admin")]
        [Route("Home/Statuses")]
        public IActionResult Statuses()
        {
            ViewBag.Statuses = db.Statuses.ToList();
            return View();
        }

        [Authorize(Policy = "admin")]
        [Route("Home/AddStatus/{Name?}")]
        public IActionResult AddStatus(string Name = "")
        {
            if (Name == "")
            {
                return View();
            }

            Status status = new Status();
            status.Name = Name;
            db.Add(status);
            db.SaveChanges();

            return Redirect($"~/Home/Statuses");
        }

        [Authorize(Policy = "admin")]
        [Route("Home/EditStatus/{Id}/{Name?}")]
        public IActionResult EditStatus(int Id = 0, string Name = "")
        {
            Status status = db.Statuses.Find(Id);
            ViewBag.Status = status;

            if (Name == "")
            {
                return View();
            }

            status.Name = Name;
            db.SaveChanges();

            return Redirect($"~/Home/Statuses");
        }

        [Authorize(Policy = "admin")]
        [Route("Home/DeleteStatus/{Id}")]
        public IActionResult DeleteStatus(int Id = 0)
        {
            Order order = db.Orders.FirstOrDefault(o => o.StatusId == Id);
            if (order == null)
            {
                Status status = db.Statuses.Find(Id);
                db.Remove(status);
                db.SaveChanges();
            }

            return Redirect($"~/Home/Statuses");
        }








        [Authorize(Policy = "admin")]
        [Route("Home/Materials")]
        public IActionResult Materials()
        {
            ViewBag.Materials = db.Materials.ToList();
            return View();
        }

        [Authorize(Policy = "admin")]
        [Route("Home/AddMaterial/{Name?}")]
        public IActionResult AddMaterial(string Name = "")
        {
            if (Name == "")
            {
                return View();
            }

            Material mat = new Material();
            mat.Name = Name;
            db.Add(mat);
            db.SaveChanges();

            return Redirect($"~/Home/Materials");
        }

        [Authorize(Policy = "admin")]
        [Route("Home/EditMaterial/{Id}/{Name?}")]
        public IActionResult EditMaterial(int Id = 0, string Name = "")
        {
            Material mat = db.Materials.Find(Id);
            ViewBag.Material = mat;

            if (Name == "")
            {
                return View();
            }

            mat.Name = Name;
            db.SaveChanges();

            return Redirect($"~/Home/Materials");
        }

        [Authorize(Policy = "admin")]
        [Route("Home/DeleteMaterial/{Id}")]
        public IActionResult DeleteMaterial(int Id = 0)
        {
            Item item = db.Items.FirstOrDefault(o => o.MaterialId == Id);
            if (item == null)
            {
                Material mat = db.Materials.Find(Id);
                db.Remove(mat);
                db.SaveChanges();
            }

            return Redirect($"~/Home/Materials");
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

        [Route("Home/Catalog/{catId?}")]
        public IActionResult Catalog(int catId = 0)
        {
            ViewBag.Role = UserRole();
            ViewBag.Name = UserName();
            User user = db.Users.FirstOrDefault(u => u.Email == UserName());
            ViewBag.User = user;
            List<Catalog> cats = db.Catalogs.ToList();
            List<Item> items = db.Items.ToList();

            List<Catalog> ChildDirs = new List<Catalog> { };
            List<Item> ChildItems = new List<Item> { };

            foreach (Catalog cat in cats)
            {
                if (cat.ParentId == catId)
                {
                    ChildDirs.Add(cat);
                }
            }

            foreach (Item item in items)
            {
                if (item.CatalogId == catId)
                {
                    ChildItems.Add(item);
                }
            }

            ViewBag.CurDirId = catId;
            ViewBag.Catalogs = ChildDirs;
            ViewBag.Items = ChildItems;
            //ViewBag.Catalogs = db.Catalogs.ToList();
            return View();
        }


        [Authorize(Policy = "admin")]
        [Route("Home/AddItem/{curDirId}/{flag}/{Name?}/{Logo?}/{Cost?}/{Quantity?}/{MaterialId?}/{Height?}/{Width?}/{Length?}/{Quality?}/{Description?}")]
        public IActionResult AddItem(int curDirId = 0, string flag = "item", string Name = "", string Logo = "https://i.ibb.co/BP6wqLp/Screenshot-6.png",
            float Cost = 0.0f, int Quantity = 0, int MaterialId = 0, int Height = 0, int Width = 0, int Length = 0, int Quality = 0, string Description = "")
        {
            ViewBag.Flag = flag;

            if (Name != "")
            {
                Item item = new Item();
                item.Name = Name;
                item.Flag = flag;
                item.Logo = Logo;
                item.Quantity = Quantity;
                item.Quality = Quality;
                item.CatalogId = curDirId;
                item.MaterialId = MaterialId;
                item.Cost = Cost;
                item.Description = Description;

                int tmp;
                if (Width < Height) { tmp = Width; Width = Height; Height = tmp; }
                if (Length < Height) { tmp = Length; Length = Height; Height = tmp; }
                if (Width < Height) { tmp = Width; Width = Height; Height = tmp; }

                item.Height = Height;
                item.Width = Width;
                item.Length = Length;

                db.Items.Add(item);
                db.SaveChanges();

                return Redirect($"~/Home/Catalog/{curDirId}");
            }

            ViewBag.curDirId = curDirId;
            if (flag == "wood")
            {
                ViewBag.MaterialList = db.Materials.ToList();
            }


            return View();
        }

        [Authorize(Policy = "admin")]
        [Route("Home/ChangeItem/{ItemId}/{Name?}/{Logo?}/{Cost?}/{Quantity?}/{MaterialId?}/{Height?}/{Width?}/{Length?}/{Quality?}/{Description?}")]
        public IActionResult ChangeItem(int ItemId = 0, string Name = "", string Logo = "https://i.ibb.co/BP6wqLp/Screenshot-6.png",
            float Cost = 0.0f, int Quantity = 0, int MaterialId = 0, int Height = 0, int Width = 0, int Length = 0, int Quality = 0, string Description = "")
        {

            Item item = db.Items.Find(ItemId);
            ViewBag.Flag = item.Flag;

            if (Name != "")
            {
                item.Name = Name;
                item.Logo = Logo;
                item.Quantity = Quantity;
                item.Quality = Quality;
                item.MaterialId = MaterialId;
                item.Cost = Cost;
                item.Description = Description;

                int tmp;
                if (Width < Height) { tmp = Width; Width = Height; Height = tmp; }
                if (Length < Height) { tmp = Length; Length = Height; Height = tmp; }
                if (Width < Height) { tmp = Width; Width = Height; Height = tmp; }

                item.Height = Height;
                item.Width = Width;
                item.Length = Length;

                db.SaveChanges();

                return Redirect($"~/Home/Catalog/{item.CatalogId}");
            }

            if (item.Flag == "wood")
            {
                ViewBag.MaterialList = db.Materials.ToList();
            }

            ViewBag.item = item;
            return View();
        }

        [Route("Home/ItemInfo/{ItemId}")]
        public IActionResult ItemInfo(int ItemId = 0)
        {
            ViewBag.Role = UserRole();
            ViewBag.Name = UserName();
            User user = db.Users.FirstOrDefault(u => u.Email == UserName());
            ViewBag.User = user;
            Item item = db.Items.Find(ItemId);
            ViewBag.item = item;


            if (item.Flag == "wood")
            {
                Material mat = db.Materials.Find(item.MaterialId);
                ViewBag.Material = mat.Name;
            }

            ViewBag.item = item;
            return View();
        }

        [Authorize(Policy = "admin")]
        [Route("Home/DeleteItem/{ItemId}")]
        public IActionResult DeleteItem(int ItemId = 0)
        {
            Item item = db.Items.Find(ItemId);
            int curDirId = item.CatalogId;
            db.Remove(item);
            db.SaveChanges();
            return Redirect($"~/Home/Catalog/{curDirId}");
        }

        /*[Route("Home/AddItem/{curDirId?}/{flag?}")]
        public IActionResult AddItem(int curDirId = 0, string flag = "item")
        {
            ViewBag.Flag = flag;

           

            ViewBag.curDirId = curDirId;
            if (flag == "wood")
            {
                ViewBag.MaterialList = db.Materials.ToList();
            }


            return View();
        }
*/

        [Authorize(Policy = "admin")]
        [Route("Home/AddCatalog/{curDirId}/{name?}")]
        public IActionResult AddCatalog(int curDirId, string name = "")
        {
            ViewBag.curDirId = curDirId;
            if (name == "") return View();

            Catalog newCat = new Catalog();
            newCat.Name = name;
            newCat.ParentId = curDirId;

            db.Catalogs.Add(newCat);
            db.SaveChanges();

            return Redirect($"~/Home/Catalog/{curDirId}");
        }

        [Authorize(Policy = "admin")]
        [Route("Home/ChangeCatalog/{curDirId}/{name?}/{logo?}")]
        public IActionResult ChangeCatalog(int curDirId, string name = "", string logo = "https://i.ibb.co/BP6wqLp/Screenshot-6.png")
        {
            ViewBag.curDirId = curDirId;
            Catalog Cat = db.Catalogs.Find(curDirId);
            ViewBag.Catalog = Cat;
            if (name == "") return View();

            //Catalog Cat = db.Catalogs.Find(curDirId);
            Cat.Name = name;
            Cat.Logo = logo;
            db.SaveChanges();

            return Redirect($"~/Home/Catalog/{curDirId}");
        }

        [Authorize(Policy = "admin")]
        [Route("Home/DeleteCatalog/{delDir?}")]
        public IActionResult DeleteCatalog(int delDir = 0)
        {
            Catalog catToDel = db.Catalogs.Find(delDir);
            int ParentId = catToDel.ParentId;

            db.Remove(catToDel);
            db.SaveChanges();

            return Redirect($"~/Home/Catalog/{ParentId}");
        }

        public IActionResult CheckOrder(int Id)
        {
            ViewBag.Role = UserRole();
            ViewBag.Name = UserName();
            ViewBag.tmp = Id;
            Order order = db.Orders.Find(Id);
            ViewBag.Order = order;

            return View();
        }

        [Authorize(Policy = "admin")]
        [HttpPost]
        public async Task<IActionResult> Create(Item item)
        {
            ViewBag.Role = UserRole();
            ViewBag.Name = UserName();
            db.Items.Add(item);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }


        /*public IActionResult Index()
        {
            return View();
        }*/


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