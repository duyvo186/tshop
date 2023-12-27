using TeaShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TeaShop.Models.EntityFramework;
using PagedList;
using System.Drawing.Printing;
using System.Web.UI;
using Microsoft.Ajax.Utilities;

namespace TeaShop.Controllers
{
    public class ProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Products
        public ActionResult Index(string SearchText, int? page)
        {
            var pageSize = 10;
            if (page == null)
            {
                page = 1;
            }
            IEnumerable<Product> items = db.Products.OrderByDescending(x => x.DateExpired);
            // Check if the filterNam property is provided in the request
            if(SearchText == "sale-product")
            {
                items = items.Where(x => x.PriceSale != null);
            }
            else if (!string.IsNullOrEmpty(SearchText) && SearchText != "sale-product")
            {
                // Apply the filter if filterNam is not null or empty
                items = items.Where(x => x.Alias.Contains(SearchText) || x.Title.Contains(SearchText));
                // Adjust the filter condition based on your specific requirements
            }
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            items = items.ToPagedList(pageIndex, pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page;

            return View(items);
        }

        public ActionResult Detail(string alias, int id)
        {
            var item = db.Products.Find(id);
            if(item != null)
            {
                db.Products.Attach(item);
                item.ViewCount = item.ViewCount + 1;
                db.Entry(item).Property(x => x.ViewCount).IsModified = true;
                db.SaveChanges();
            }    
            return View(item);
        }    

        public ActionResult ProductCategory(string SearchText, string alias, int id)
        {
            var items = db.Products.ToList();
            if (id > 0)
            {
                items = items.Where(x => x.ProductCategoryId == id).ToList();
                if (!string.IsNullOrEmpty(SearchText))
                {
                    items = items.Where(x => x.Alias.Contains(SearchText) || x.Title.Contains(SearchText)).ToList();
                }
            }
            var cate = db.ProductCategories.Find(id);
            if(cate != null)
            {
                ViewBag.CateName = cate.Title;
            }    
            ViewBag.CateId = id;
            return View(items);
        }

        public ActionResult Partial_ItemsByCateId ()
        {
            var items = db.Products.Where(x => x.IsHome && x.IsActive).Take(40).ToList();
            return PartialView(items);
        }
        public ActionResult Partial_ProductSales()
        {
            var items = db.Products.Where(x => x.IsSale && x.IsActive).Take(40).ToList();
            return PartialView(items);
        }
    }
}