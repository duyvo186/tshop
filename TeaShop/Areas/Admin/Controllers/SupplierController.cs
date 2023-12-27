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
    [Authorize(Roles = "Admin")]
    public class SupplierController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/Supplier
        public ActionResult Index(string SearchText, int?page)
        {
            var pageSize = 10;
            if (page == null)
            {
                page = 1;
            }
            IEnumerable<Supplier> items = db.Supplier.OrderByDescending(x => x.Id);
            if (!string.IsNullOrEmpty(SearchText))
            {
                items = items.Where(x => x.Name.Contains(SearchText));
            }
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            items = items.ToPagedList(pageIndex, pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page;
            return View(items);
        }
        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public ActionResult Add(Supplier model)
        {
            if (ModelState.IsValid)
            {
                if (db.Supplier.Any(e => e.Name == model.Name))
                {
                    ModelState.AddModelError(nameof(model.Name), "NCC đã tồn tại");
                    return View(model);
                }
                model.CreatedDate = DateTime.Now;
                model.ModifiedDate = DateTime.Now;
                model.Name = model.Name;
                
                db.Supplier.Add(model);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult Edit (int id)
        {
            var item = db.Supplier.Find(id);
            return View(item);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public ActionResult Edit (Supplier model)
        {
            if (ModelState.IsValid)
            {
                if (db.Supplier.Any(e => e.Name == model.Name && e.Id != model.Id))
                {
                    ModelState.AddModelError(nameof(model.Name), "NCC đã tồn tại");
                    return View(model);
                }
                db.Supplier.Attach(model);
                model.ModifiedDate = DateTime.Now;
                model.Name = model.Name;
                db.Entry(model).Property(x => x.Name).IsModified = true;
                db.Entry(model).Property(x => x.ModifiedDate).IsModified = true;
                db.Entry(model).Property(x => x.ModifieddBy).IsModified = true;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete (int id)
        {
            var item = db.Supplier.Find(id);
            if (item != null)
            {
                var DeleteItem = db.Supplier.Attach(item);
                db.Supplier.Remove(item);
                db.SaveChanges();
                return Json(new { success = true });
            }    
            return Json(true);

        }

        [HttpPost]
        public ActionResult DeleteAll(string ids)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                var items = ids.Split(',');
                if (items != null && items.Any())
                {
                    foreach (var item in items)
                    {
                        var obj = db.Supplier.Find(Convert.ToInt32(item));
                        db.Supplier.Remove(obj);
                        db.SaveChanges();
                    }
                }
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }
}