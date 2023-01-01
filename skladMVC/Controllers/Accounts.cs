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

    public class Accounts : Controller
    {

        ApplicationContext db;
        public Accounts(ApplicationContext context)
        {
            db = context;
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult LoginForm()
        {
            return View();
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult RegistrationForm()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult RegistrationForm(string Name = null, string Email = null, string Password = null)
        {

            if (Name != null && Email != null && Password != null)
            {
                List<User> find = db.Users.Where(u => u.Email == Email).ToList();

                if (find.Count != 0)
                {
                    return Redirect("~/Accounts/RegistrationForm");
                }

                User newUser = new User();
                newUser.Name = Name;
                newUser.Email = Email;
                newUser.Password = Password;
                newUser.Role = "user";
                
                db.Users.Add(newUser);
                db.SaveChanges();

                return Redirect("~/Accounts/LoginForm");
            }

            return Redirect("~/Accounts/RegistrationForm");
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LoginForm(string? email, string? password)
        {
            if (email == null || password == null) return Redirect("~/Accounts/LoginForm");

            User? user = db.Users.FirstOrDefault(p => p.Email == email.ToString() && p.Password == password.ToString());
            if (user == null)
            {
                return Redirect("~/Accounts/LoginForm");
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
            return Redirect("~/Home");
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

        [Authorize]
        public IActionResult Profile()
        {
            ViewBag.Role = UserRole();
            ViewBag.Name = UserName();

            User user = db.Users.FirstOrDefault(u => u.Email == UserName());
            List<Order> orders = db.Orders.Where(o => o.UserId == user.Id).ToList();
            Dictionary<int, Status> statusList = new Dictionary<int, Status>();
            foreach (Status st in db.Statuses.ToList())
            {
                statusList.Add(st.Id, st);
            }

            ViewBag.Orders = orders;
            ViewBag.User = user;
            ViewBag.statusList = statusList;

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


        [Authorize]
        [Route("Accounts/ChangeCartItem/{itemId}/{newAmount?}")]
        public IActionResult ChangeCartItem(int itemId, int newAmount = -1)
        {
            ViewBag.Role = UserRole();
            ViewBag.Name = UserName();

            User user = db.Users.FirstOrDefault(u => u.Email == UserName());
            ViewBag.User = user;

            List<CartItem> itemsInCart = new List<CartItem>();
            CartItem cartItem = null;

            foreach (CartItem ci in db.CartItems.ToList())
            {
                if (ci.UserId == user.Id)
                {
                    if (ci.ItemId == itemId)
                    {
                        cartItem = ci;
                        break;
                    }
                }
            }

            if (cartItem == null)
            {
                return Redirect("~/Accounts/Cart");
            }

            Item item = db.Items.Find(cartItem.ItemId);

            if (newAmount != -1)
            {
                if (newAmount == 0)
                {
                    item.Quantity += cartItem.Amount;

                    db.CartItems.Remove(cartItem);
                    db.SaveChanges();
                    return Redirect("~/Accounts/Cart");
                }

                if (newAmount <= item.Quantity)
                {
                    item.Quantity += cartItem.Amount - newAmount;
                    cartItem.Amount = newAmount;

                    db.SaveChanges();
                    return Redirect("~/Accounts/Cart");
                }
                else
                {
                    return Redirect("~/Accounts/Cart");
                }

            }
            else
            {
                ViewBag.item = item;
                ViewBag.cartItem = cartItem;
            }

            return View();
        }


        [Route("Accounts/CancelOrder/{OrderId}")]
        public IActionResult CancelOrder(int OrderId)
        {

            ViewBag.Role = UserRole();
            ViewBag.Name = UserName();

            User user = db.Users.FirstOrDefault(u => u.Email == UserName());
            ViewBag.User = user;

            if (OrderId == null)
            {
                return Redirect("~/Accounts/Profile");
            }

            Order order = db.Orders.Find(OrderId);
            if (order == null)
            {
                return Redirect("~/Accounts/Profile");
            }

            if (order.UserId != user.Id && UserRole() != "admin")
            {
                return Redirect("~/Accounts/Profile");
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
            return Redirect("~/Accounts/Profile");
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
                return Redirect("~/Accounts/ConfirmCart");
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
            return Redirect($"~/Catalogs/ItemInfo/{ItemId}");
        }

        [Authorize]
        [Route("Accounts/MyOrders")]
        public IActionResult MyOrders()
        {
            ViewBag.Name = UserName();
            User user = db.Users.FirstOrDefault(u => u.Email == UserName());

            return View();
        }

        [Authorize]
        [Route("Accounts/ShowOrder/{Id}")]
        public IActionResult ShowOrder(int Id)
        {

            ViewBag.Role = UserRole();
            ViewBag.Name = UserName();

            User user = db.Users.FirstOrDefault(u => u.Email == UserName());
            Order order = db.Orders.Find(Id);

            if (order.UserId != user.Id && UserRole() != "admin")
            {
                return Redirect("~/Admin/AllOrders");
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
    }
}
