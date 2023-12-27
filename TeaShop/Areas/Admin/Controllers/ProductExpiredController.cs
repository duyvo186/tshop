using TeaShop.Models;
using TeaShop.Models.EntityFramework;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TeaShop.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin, Employee")]
    public class ProductExpiredController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/Products
        public ActionResult Index(string SearchText, int? numDay, int?page)
        {
            IEnumerable<Product> items = db.Products.OrderByDescending(x => x.DateExpired);
            var pageSize = 10;
            if (page == null)
            {
                page = 1;
            }
            if (!string.IsNullOrEmpty(SearchText))
            {
                items = items.Where(x => (x.Alias.Contains(SearchText) || x.Title.Contains(SearchText)));
            }
            if ((numDay ?? 0) != 0)
            {
                items = items.Where(x => ((x.DateExpired != null && (x.DateExpired.Value.Date - DateTime.Now.Date).TotalDays <= numDay)));
            }
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            items = items.ToPagedList(pageIndex, pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page;
            return View(items);
        }

        [HttpPost]
        public ActionResult IsSale(int id, int discount)
        {
            var item = db.Products.Find(id);
            if (item != null)
            {
                item.IsSale = true;
                item.PriceSale = discount;
                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true, IsSale = item.IsSale, PriceSale = item.PriceSale });
            }

            return Json(new { success = false });
        }

    }
}