using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TeaShop.Models;
using TeaShop.Models.EntityFramework;

namespace TeaShop.Controllers
{
    public class ShoppingCartController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ShoppingCartController()
        {
        }

        public ShoppingCartController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: ShoppingCart
        public ActionResult Index()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if(cart != null)
            {
                return View(cart.Items);
            }    
            return View();
        }

        public ActionResult CheckOut()
        {
            ShoppingCart cart1 = (ShoppingCart)Session["Cart"];
            if (cart1 != null && cart1.Items.Any())
            {
                ViewBag.CheckCart = cart1;
                
            }
            var user = db.Users.Find(User.Identity.GetUserId());
            return View(user);
        }

        public ActionResult CheckOutSuccess()
        {
            return View();
        }

        public ActionResult Partial_Item_ThanhToan()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null && cart.Items.Any())
            {
                return PartialView(cart.Items);
            }
            return PartialView();
        }

        public ActionResult Partial_Item_Cart()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null && cart.Items.Any())
            {
                return PartialView(cart.Items);
            }
            return PartialView();
        }

        public ActionResult ShowCount()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null)
            {
                return Json(new { Count = cart.Items.Count }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Count = 0 }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Partial_CheckOut()
        {
            if (Session["UserName"] != null)
            {
                string UserName = Session["UserName"].ToString();
                var _user = db.Users.Where(t => t.UserName.Equals(UserName)).FirstOrDefault();
                if (_user != null)
                {
                    OrderViewModel model = new OrderViewModel();
                    model.CustomerName = _user.FullName;
                    model.Phone= _user.Phone;
                    model.Address=_user.Address;
                    model.Email= _user.Email;
                    return PartialView(model);
                }
            }
                return PartialView();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CheckOut(OrderViewModel req)
        {
            var userEmail = "";
            if (!User.Identity.IsAuthenticated)
            {
                var user = new ApplicationUser
                {
                    UserName = req.Email,
                    Email = req.Email,
                    FullName = req.CustomerName,
                    Phone = req.Phone,
                    Address = req.Address
                };
                var result = await UserManager.CreateAsync(user, req.PassWord);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    //string codeVerify = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = codeVerify }, protocol: Request.Url.Scheme);
                    //await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");
                  

                    Session["UserName"] = req.Email;
                }
                userEmail = user.Email;
            }
            userEmail = req.DeliveryName;

                  ShoppingCart cart = (ShoppingCart)Session["Cart"];
                if (cart != null)
                {
                    Order order = new Order();
                    order.CustomerName = userEmail;
                    order.Phone = req.DeliveryPhoneNumber;
                    order.Address = req.DeliveryAddress;
                    order.Email = userEmail;
                    cart.Items.ForEach(x => order.OrderDetails.Add(new OrderDetail
                    {
                        ProductId = x.ProductId,
                        Quantity = x.Quantity,
                        Price = x.Price
                    }));
                    order.TotalAmount = cart.Items.Sum(x => (x.Price * x.Quantity));
                    order.TypePayment = req.TypePayment;
                    order.CreatedDate = DateTime.Now;
                    order.ModifiedDate = DateTime.Now;
                    Random rd = new Random();
                    order.Code = "DH" + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9);
                    order.DeliveryDate=req.DeliveryDate;
                    order.DeliveryHour=req.DeliveryHour;
                    order.DeliveryMinute=req.DeliveryMinute;
                    //order.E = req.CustomerName;
                    db.Orders.Add(order);
                    db.SaveChanges();

                    Export export = new Export();
                    export.WareName = "Kho 1";
                    export.Quantity = cart.Items.Sum(x => (x.Quantity)); ;
                    export.IsExport = 0;

                    cart.Items.ForEach(x => export.ExportDetails.Add(new ExportDetail
                    {
                        ProductId = x.ProductId,
                        Quantity = x.Quantity,
                        UnitID = db.Products.Find(x.ProductId).UnitID
                    }));
                    export.CreatedDate = DateTime.Now;
                    export.ModifiedDate = DateTime.Now;
                    export.CreatedBy = req.Phone;
                    Random rd1 = new Random();
                    export.Code = "PX-" + order.Code;
                    //order.E = req.CustomerName;
                    db.Exports.Add(export);
                    db.SaveChanges();
                    //send mail cho khachs hang
                    var strSanPham = "";
                    var thanhtien = decimal.Zero;
                    var TongTien = decimal.Zero;
                    foreach (var sp in cart.Items)
                    {
                        var item = db.Products.Find(sp.ProductId);
                        int a = item.Quanlity - sp.Quantity;
                    try
                    {
                        if (item != null)
                        {
                            item.Quanlity = a;
                            item.DateExpired = DateTime.Now;
                            db.SaveChanges();
                        }
                    }
                    catch (DbEntityValidationException ex)
                    {
                        // Log validation errors
                        foreach (var validationErrors in ex.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                var ok = validationError.PropertyName;
                                var hey = validationError.ErrorMessage;
                                Console.WriteLine($"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                            }
                        }
                    }


                    strSanPham += "<tr>";
                        strSanPham += "<td>" + sp.ProductName + "</td>";
                        strSanPham += "<td>" + item.Quanlity + "</td>";
                        strSanPham += "<td>" + TeaShop.Common.Common.FormatNumber(sp.TotalPrice, 0) + "</td>";
                        strSanPham += "</tr>";
                        thanhtien += sp.Price * sp.Quantity;
                    }
                    TongTien = thanhtien;
                    string contentCustomer = System.IO.File.ReadAllText(Server.MapPath("~/Content/templates/send2.html"));
                    contentCustomer = contentCustomer.Replace("{{MaDon}}", order.Code);
                    contentCustomer = contentCustomer.Replace("{{SanPham}}", strSanPham);
                    contentCustomer = contentCustomer.Replace("{{NgayDat}}", DateTime.Now.ToString("dd/MM/yyyy"));
                    contentCustomer = contentCustomer.Replace("{{TenKhachHang}}", order.CustomerName);
                    contentCustomer = contentCustomer.Replace("{{Phone}}", order.Phone);
                    contentCustomer = contentCustomer.Replace("{{Email}}", userEmail);
                    contentCustomer = contentCustomer.Replace("{{DiaChiNhanHang}}", order.Address); 
                    contentCustomer = contentCustomer.Replace("{{ThoiGianNhanHang}}", order.DeliveryDate.Value.ToString("dd/MM/yyy") + " " + order.DeliveryHour + ":" + order.DeliveryMinute);
                    contentCustomer = contentCustomer.Replace("{{ThanhTien}}", TeaShop.Common.Common.FormatNumber(thanhtien, 0));
                    contentCustomer = contentCustomer.Replace("{{TongTien}}", TeaShop.Common.Common.FormatNumber(TongTien, 0));
                    TeaShop.Common.Common.SendMail("TeaShop", "Đơn hàng #" + order.Code, contentCustomer.ToString(), userEmail);

                    string contentAdmin = System.IO.File.ReadAllText(Server.MapPath("~/Content/templates/send1.html"));
                    contentAdmin = contentAdmin.Replace("{{MaDon}}", order.Code);
                    contentAdmin = contentAdmin.Replace("{{SanPham}}", strSanPham); 
                    contentAdmin = contentAdmin.Replace("{{NgayDat}}", DateTime.Now.ToString("dd/MM/yyyy"));
                    contentAdmin = contentAdmin.Replace("{{TenKhachHang}}", order.CustomerName);
                    contentAdmin = contentAdmin.Replace("{{Phone}}", order.Phone);
                    contentAdmin = contentAdmin.Replace("{{Email}}", req.DeliveryName);
                    contentAdmin = contentAdmin.Replace("{{DiaChiNhanHang}}", order.Address);
                    contentAdmin = contentAdmin.Replace("{{ThoiGianNhanHang}}", order.DeliveryDate.Value.ToString("dd/MM/yyy") + " " + order.DeliveryHour + ":" + order.DeliveryMinute);
                    contentAdmin = contentAdmin.Replace("{{ThanhTien}}", TeaShop.Common.Common.FormatNumber(thanhtien, 0));
                    contentAdmin = contentAdmin.Replace("{{TongTien}}", TeaShop.Common.Common.FormatNumber(TongTien, 0));
                    TeaShop.Common.Common.SendMail("TeaShop", "Đơn hàng mới #" + order.Code, contentAdmin.ToString(), ConfigurationManager.AppSettings["EmailAdmin"]);
                    cart.ClearCart();
                   
                }
             return RedirectToAction("CheckOutSuccess");
    }

        [HttpPost]
        public ActionResult AddToCart(int id, int quantity)
        {
            var code = new { Success = false, msg = "", code = -1, Count = 0 };
            var db = new ApplicationDbContext();
            var checkProduct = db.Products.FirstOrDefault(x => x.Id == id);
            if (checkProduct != null)
            {
                ShoppingCart cart = (ShoppingCart)Session["Cart"];
                if (cart == null)
                {
                    cart = new ShoppingCart();
                }
                ShoppingCartItem item = new ShoppingCartItem
                {
                    ProductId = checkProduct.Id,
                    ProductName = checkProduct.Title,
                    CategoryName = checkProduct.ProductCategory.Title,
                    Alias = checkProduct.Alias,
                    Quantity = quantity
                };
                if (checkProduct.ProductImage.FirstOrDefault(x => x.IsDefault) != null)
                {
                    item.ProductImg = checkProduct.ProductImage.FirstOrDefault(x => x.IsDefault).Image;
                }
                item.Price = checkProduct.Price;
                if (checkProduct.PriceSale > 0)
                {
                    item.Price = (decimal)checkProduct.PriceSale;
                }
                item.TotalPrice = item.Quantity * item.Price;
                cart.AddToCart(item, quantity);
                Session["Cart"] = cart;
                code = new { Success = true, msg = "Thêm sản phẩm vào giỏ hàng thành công!", code = 1, Count = cart.Items.Count };
            }
            return Json(code);
        }

        [HttpPost]
        public ActionResult Update(int id, int quantity)
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null)
            {
                cart.UpdateQuantity(id, quantity);
                return Json(new { Success = true });
            }
            return Json(new { Success = false });
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var code = new { Success = false, msg = "", code = -1, Count = 0 };

            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null)
            {
                var checkProduct = cart.Items.FirstOrDefault(x => x.ProductId == id);
                if (checkProduct != null)
                {
                    cart.Remove(id);
                    code = new { Success = true, msg = "", code = 1, Count = cart.Items.Count };
                }
            }
            return Json(code);
        }

        [HttpPost]
        public ActionResult DeleteAll()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null)
            {
                cart.ClearCart();
                return Json(new { Success = true });
            }
            return Json(new { Success = false });
        }
    }
}