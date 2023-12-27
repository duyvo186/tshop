using TeaShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using TeaShop.Models.EntityFramework;
using Microsoft.Owin.Security.Twitter.Messages;
using System.Resources;
using Newtonsoft.Json;

namespace TeaShop.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ImportController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/Import
        public ActionResult Index(int? page)
        {
            var items = db.Imports.OrderByDescending(x => x.CreatedDate).ToList();
            if (page == null)
            {
                page = 1;
            }
            var pageNumber = page ?? 1;
            var pageSize = 10;
            ViewBag.PageSize = pageSize;
            ViewBag.Page = pageNumber;
            return View(items.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult View(int id)
        {
            var item = db.Imports.Find(id);
            return View(item);
        }

        public ActionResult Partial_SanPham(int id)
        {
            var items = db.ImportDetais.Where(x => x.ImportId == id).ToList();
            return PartialView(items);
        }


        public ActionResult Add()
        {
            ViewBag.Products = db.Products.ToList();
            ViewBag.JsonProducts = JsonConvert.SerializeObject(from entity in db.Products
                                                               select new
                                                               {
                                                                   ID = entity.Id,
                                                                   Name = entity.Title
                                                               });
            ViewBag.Unit = new SelectList(db.Unit.ToList(), "Id", "Name");
            ViewBag.JsonUnit = JsonConvert.SerializeObject(from entity in db.Unit
                                                                                       select new
                                                                                       {
                                                                                           ID = entity.Id,
                                                                                           Name = entity.Name
                                                                                       });
            ViewBag.Supplier = new SelectList(db.Supplier.ToList(), "Id", "Name");
            ViewBag.JsonSupp = JsonConvert.SerializeObject(from entity in db.Supplier
                                                                                       select new
                                                                                       {
                                                                                           ID = entity.Id,
                                                                                           Name = entity.Name
                                                                                       });
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(Import model, List<ImportDetail> details)
        {
            if (details != null && details.Count > 0)
            {
                foreach (var x in details)
                {
                    db.Products.Find(x.ProductId).Quanlity += x.Quantity;
                }

                details.ForEach(x => model.ImportDetails.Add(new ImportDetail
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    Price = x.Price,
                    UnitID = db.Products.Find(x.ProductId).UnitID,
                    PackageNumber = x.PackageNumber,
                    DateExpired = x.DateExpired,
                    SupplierID = x.SupplierID
                }));
            }
            Random rd = new Random();
            model.Code = "PN" + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9);
            model.CreatedDate = DateTime.Now;
            model.ModifiedDate = DateTime.Now;

            db.Imports.Add(model);
            db.SaveChanges();
            return Json(new { success = true });
        }

        public ActionResult Edit(int id)
        {
            ViewBag.Products = db.Products.ToList();
            ViewBag.Unit = new SelectList(db.Unit.ToList(), "Id", "Name");
            ViewBag.Supplier = new SelectList(db.Supplier.ToList(), "Id", "Name");
            var item = db.Imports.Find(id);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Import model)
        {
            if (ModelState.IsValid)
            {
                model.ModifiedDate = DateTime.Now;
                db.Imports.Attach(model);
                db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var item = db.Imports.Find(id);
            if (item != null)
            {
                var details = item.ImportDetails;
                foreach (var x in details)
                {
                    var p = db.Products.Find(x.ProductId);
                    var imp = db.ImportDetais.Where(i => i.ProductId == x.ProductId && i.PackageNumber == x.PackageNumber && i.Id != x.Id).ToList();
                    var sumQuant = imp.Sum(e => e.Quantity);

                    var exp = db.ExportDetais.Where(i => i.ProductId == x.ProductId && i.PackageNumber == x.PackageNumber).ToList();
                    var sumQuantE = exp.Sum(e => e.Quantity);

                    if (sumQuant - sumQuantE < 0) return Json(new { message = "OutOfStock", success = false });
                    db.Products.Find(x.ProductId).Quanlity -= x.Quantity;
                }


                db.Imports.Remove(item);
                db.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }


    }
}