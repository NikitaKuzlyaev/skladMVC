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
    public class Catalogs : Controller
    {

        ApplicationContext db;
        public Catalogs(ApplicationContext context)
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


        [Route("Catalogs/Catalog/{catId?}/{MaterialSelected?}/{QualityMin?}/{QualityMax?}/{CostMin?}/{CostMax?}/{YesCheckBox?}/{NoCheckBox?}")]
        public IActionResult Catalog(int catId = 1, int MaterialSelected = 0, int QualityMin = 0, int QualityMax = 3,
            int CostMin = 0, int CostMax = 999999, string Amount = "YesOrNo")
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

            bool onlyItems = true;


            foreach (Item item in items)
            {
                if (item.CatalogId == catId)
                {
                    bool flag = true;

                    if (Amount == "Yes" && item.Quantity == 0) { flag = false; }
                    if (Amount == "No" && item.Quantity != 0) { flag = false; }
                    if (CostMin != 0) { if (item.Cost < CostMin) { flag = false; } }
                    if (CostMax != 999999) { if (item.Cost > CostMax) { flag = false; } }
                    if (item.Flag == "wood")
                    {
                        if (MaterialSelected != 0) { if (item.MaterialId != MaterialSelected) { flag = false; } }
                        if (QualityMin != 0) { if (item.Quality < QualityMin) { flag = false; } }
                        if (QualityMax != 3) { if (item.Quality > QualityMax) { flag = false; } }
                    }
                    else
                    {
                        onlyItems = false;
                    }

                    if (flag)
                    {
                        ChildItems.Add(item);
                    }
                }
            }

            ViewBag.CurDirId = catId;
            ViewBag.Catalogs = ChildDirs;
            ViewBag.Items = ChildItems;

            
            
            Dictionary<int, Material> dict = new Dictionary<int, Material>();

            foreach (Material mat in db.Materials.ToList())
            {
                dict[mat.Id] = mat;
            }

            
            ViewBag.Materials = db.Materials.ToList();
            ViewBag.MaterialList = dict;
            ViewBag.Last_MaterialSelected = MaterialSelected;
            ViewBag.Last_QualityMin = QualityMin;
            ViewBag.Last_QualityMax = QualityMax;
            ViewBag.Amount = Amount;
            ViewBag.Last_CostMin = CostMin;
            ViewBag.Last_CostMax = CostMax;
            ViewBag.OnlyItems = onlyItems;

            return View();
        }


        [Authorize(Policy = "admin")]
        [Route("Catalogs/AddItem/{curDirId}/{flag}/{Name?}/{Logo?}/{Cost?}/{Quantity?}/{MaterialId?}/{Height?}/{Width?}/{Length?}/{Quality?}/{Description?}")]
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

                Catalog cat = db.Catalogs.Find(curDirId);
                return Redirect($"~/Catalogs/Catalog/{cat.ParentId}");
            }

            ViewBag.curDirId = curDirId;
            if (flag == "wood")
            {
                ViewBag.MaterialList = db.Materials.ToList();
            }


            return View();
        }

        [Authorize(Policy = "admin")]
        [Route("Catalogs/ChangeItem/{ItemId}/{Name?}/{Logo?}/{Cost?}/{Quantity?}/{MaterialId?}/{Height?}/{Width?}/{Length?}/{Quality?}/{Description?}")]
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
                return Redirect($"~/Catalogs/Catalog/{item.CatalogId}");
            }

            if (item.Flag == "wood")
            {
                ViewBag.MaterialList = db.Materials.ToList();
            }

            ViewBag.item = item;
            return View();
        }

        [Route("Catalogs/ItemInfo/{ItemId}")]
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
        [Route("Catalogs/DeleteItem/{ItemId}")]
        public IActionResult DeleteItem(int ItemId = 0)
        {
            Item item = db.Items.Find(ItemId);
            int curDirId = item.CatalogId;

            db.Remove(item);
            db.SaveChanges();
            return Redirect($"~/Catalogs/Catalog/{curDirId}");
        }


        [Authorize(Policy = "admin")]
        [Route("Catalogs/AddCatalog/{curDirId}/{name?}/{logo?}")]
        public IActionResult AddCatalog(int curDirId, string name = "", string logo = "https://i.ibb.co/BP6wqLp/Screenshot-6.png")
        {
            ViewBag.curDirId = curDirId;
            if (name == "") return View();

            Models.Catalog newCat = new Models.Catalog();
            newCat.Name = name;
            newCat.ParentId = curDirId;
            newCat.Logo = logo;

            db.Catalogs.Add(newCat);
            db.SaveChanges();
            return Redirect($"~/Catalogs/Catalog/{curDirId}");
        }


        [Authorize(Policy = "admin")]
        [Route("Catalogs/ChangeCatalog/{curDirId}/{name?}/{logo?}")]
        public IActionResult ChangeCatalog(int curDirId, string name = "", string logo = "https://i.ibb.co/BP6wqLp/Screenshot-6.png")
        {
            ViewBag.curDirId = curDirId;
            Catalog Cat = db.Catalogs.Find(curDirId);
            ViewBag.Catalog = Cat;

            List<int> myChild = AllChild(Cat.Id);
            myChild.Add(Cat.Id);

            List<Catalog> notMyChild = new List<Catalog>();
            List<Catalog> allCats = db.Catalogs.ToList();

            foreach (Catalog cat in allCats)
            {
                if (!(0 <= myChild.IndexOf(cat.Id) && myChild.IndexOf(cat.Id) < myChild.Count))
                {
                    notMyChild.Add(cat);
                }
            }

            ViewBag.NotMyChild = notMyChild;

            if (name == "") return View();

            Cat.Name = name;
            Cat.Logo = logo;

            db.SaveChanges();
            return Redirect($"~/Catalogs/Catalog/{curDirId}");
        }


        [Authorize(Policy = "admin")]
        [Route("Catalogs/DeleteCatalog/{delDir?}")]
        public IActionResult DeleteCatalog(int delDir = 0)
        {
            Catalog catToDel = db.Catalogs.Find(delDir);
            int ParentId = catToDel.ParentId;

            List<Catalog> cats = db.Catalogs.Where(u => u.ParentId == catToDel.Id).ToList();
            List<Item> items = db.Items.Where(u => u.CatalogId == catToDel.Id).ToList();

            if (cats.Count == 0 && items.Count == 0)
            {
                db.Remove(catToDel);
                db.SaveChanges();
            }
            return Redirect($"~/Catalogs/Catalog/{ParentId}");
        }


        public List<int> AllChild(int ParentId)
        {
            List<int> allMyChild = new List<int>();
            List<Catalog> cats = db.Catalogs.Where(u => u.ParentId == ParentId).ToList();

            foreach (Catalog c in cats)
            {
                allMyChild.Add(c.Id);
                List<int> newChild = AllChild(c.Id);

                foreach (int i in newChild)
                {
                    allMyChild.Add(i);
                }
            }
            return allMyChild;
        }

        [Authorize(Policy = "admin")]
        [Route("Catalogs/ReplaceCatalog/{Dir?}/{ParentDir?}")]
        public IActionResult ReplaceCatalog(int Dir = -1, int ParentDir = -1)
        {
            Catalog Curr = db.Catalogs.Find(Dir);
            Catalog Parent = db.Catalogs.Find(ParentDir);

            if (Curr == null || Parent == null)
            {
                return Redirect($"~/Home/");
            }

            List<int> ParentChild = AllChild(ParentDir);

            if (!ParentChild.Contains(Dir))
            {
                Curr.ParentId = Parent.Id;
            }

            db.SaveChanges();
            return Redirect($"~/Home/");
        }
    }
}
