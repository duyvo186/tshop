using TeaShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using TeaShop.Models.EntityFramework;
using System.Collections;
using Newtonsoft.Json;

namespace TeaShop.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ExportController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/Export
        public ActionResult Index(int? page)
        {
            var items = db.Exports.OrderByDescending(x => x.CreatedDate).ToList();
            if(page == null)
            {
                page = 1;
            }
            var pageNumber = page ?? 1;
            var pageSize = 10;
            ViewBag.PageSize = pageSize;
            ViewBag.Page = pageNumber;
            return View(items.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult View (int id)
        {
            var item = db.Exports.Find(id);
            return View(item);
        }

        public ActionResult Partial_SanPham (int id)
        {
            var items = db.ExportDetais.Where(x => x.ExportId == id).ToList();
            return PartialView(items);
        }

        public ActionResult Edit(int id)
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

            var pack = db.ImportDetais.Where(e => e.DateExpired >= DateTime.Now)

                .GroupBy(item => item.PackageNumber)

                 .Select(group => new { Id = group.Key, Name = group.FirstOrDefault().PackageNumber, ProuductId = group.FirstOrDefault().ProductId })
                 .ToList();
            ViewBag.Pack = pack;

            var items = db.ExportDetais.Where(x => x.ExportId == id).ToList();
            ViewBag.IsExport = db.Exports.Find(id).IsExport;

            ViewBag.JSonDetails = JsonConvert.SerializeObject(from entity in items
                                                           select new
                                                           {
                                                               ID = entity.Id,
                                                               ProductID = entity.ProductId,
                                                               entity.Quantity,
                                                               entity.ExportId

                                                           });

            ViewBag.JSonPack = JsonConvert.SerializeObject(from entity in pack
                                                              select new
                                                              {
                                                                  ID = entity.Id,
                                                                  ProductID = entity.ProuductId,
                                                                  entity.Name

                                                              });
            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(List<ExportDetail> details)
        {
            if (details != null && details.Count > 0)
            {
                var Export = db.Exports.Find(details[0].ExportId);
                db.Exports.Attach(Export);
                var dtNow = DateTime.Now;
                if (Export.IsExport == 0)
                {
                    foreach (var x in details)
                    {
                        var p = db.Products.Find(x.ProductId);
                        var imp = db.ImportDetais.Where(i => i.ProductId == x.ProductId && i.PackageNumber == x.PackageNumber).ToList();
                        var sumQuant = imp.Sum(e => e.Quantity);

                        var exp = db.ExportDetais.Where(i => i.ProductId == x.ProductId && i.PackageNumber == x.PackageNumber).ToList();
                        var sumQuantE = exp.Sum(e => e.Quantity);

                        if (sumQuant - sumQuantE < 0) return Json(new { message = "OutOfStock", success = false });


                        db.ExportDetais.Find(x.Id).PackageNumber = x.PackageNumber;
                        db.Products.Find(x.ProductId).Quanlity -= x.Quantity;
                    }
                    Export.IsExport = 1;
                    Export.DateExport = dtNow;
                }
                else
                {
                    foreach (var x in details)
                    {
                        db.ExportDetais.Find(x.Id).PackageNumber = "";
                        db.Products.Find(x.ProductId).Quanlity += x.Quantity;
                    }
                    Export.IsExport = 0;
                    Export.DateExport = dtNow;
                }

            }

            db.SaveChanges();
            return Json(new { success = true });
        }

    }
}