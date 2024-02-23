using Microsoft.AspNetCore.Mvc;
using REALLY9.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Azure;
using PagedList.Core;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis;
using NToastNotify;
using REALLY9.ModelViews;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Runtime.InteropServices;

namespace REALLY9.Controllers
{
    public class ProductController : Controller
    {
        private readonly Really9Context _context;
        private readonly IToastNotification toastNotification;

        public ProductController(Really9Context context, IToastNotification toastNotification)
        {
            _context = context;
            this.toastNotification = toastNotification;

        }
        
        [Route("shop.html", Name = "ShopProduct")]
        public IActionResult Index(int? page, string sortOrder)
        {

            try
            {
                var pageNumber = page == null || page < 0 ? 1 : page.Value;
                var pageSize = 16;
                IQueryable<Product> lsProduct = _context.Products
                    .AsNoTracking()
                    .Where(p => p.UnitslnStock > 0);

                switch (sortOrder)
                {
                    case "name":
                        lsProduct = lsProduct.OrderBy(p => p.ProductName); 
                        break;
                    case "highPrice":
                        lsProduct = lsProduct.OrderByDescending(p => p.Price);
                        break;
                    case "lowPrice":
                        lsProduct = lsProduct.OrderBy(p => p.Price);
                        break;
                    default:
                        lsProduct = lsProduct.OrderByDescending(x => x.DateCreated);
                        break;
                }

                PagedList<Product> models = new PagedList<Product>(lsProduct, pageNumber, pageSize);
                ViewBag.CurrentSort = sortOrder;
                ViewBag.CurrentPage = pageNumber;
                return View(models);
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }
           
           
        }

        [HttpPost]
        public IActionResult FindProduct(string keyword)
        {
            if (!string.IsNullOrEmpty(keyword) && keyword.Length >= 3)
            {
                var products = _context.Products
                    .Where(x => x.ProductName.Contains(keyword))
                    .Take(10) 
                    .ToList();

                return PartialView("_SearchResultsPartial", products); 
            }
            return PartialView("_SearchResultsPartial", new List<Product>());
        }

        [Route("/{Alias}", Name = "ListProduct")]
        public IActionResult List(string Alias, int page=1)
        {
            try
            {
                var pageSize = 8;
                var danhmuc = _context.Categories.AsNoTracking().SingleOrDefault(x => x.Alias == Alias);
                var lsProduct = _context.Products
                          .Include(p => p.Cat) 
                          .AsNoTracking()
                          .Where(x => x.CatId == danhmuc.CatId)
                          .OrderByDescending(x => x.DateCreated);
                PagedList<Product> models = new PagedList<Product>(lsProduct, page, pageSize);
                ViewBag.CurrentPage = page;
                ViewBag.CurrentCat = danhmuc;
                return View(models);
            }
            catch 
            {
                return RedirectToAction("Index","Home");
            }
            
        }

        [Route("/{Alias}-{id}.html", Name = "ProductDetails")]
        public IActionResult Details(string Alias, int id)
        {
            try
            {
                var product = _context.Products
                     .Include(p => p.Cat) 
                     .Include(p => p.Comments)
                     .AsNoTracking()
                     .FirstOrDefault(x => x.ProductId == id);
                if (product == null)
                {
                    return RedirectToAction("Index");
                }
                var danhmuc = _context.Categories.AsNoTracking().SingleOrDefault(x => x.Alias == Alias);
                ViewBag.CurrentCat = danhmuc;
                var lsProduct=_context.Products.AsNoTracking().Where(x=>x.CatId== product.CatId&&x.ProductId!=id&&x.Active==true).OrderBy(x=>x.DateCreated).Take(5).ToList();
                ViewBag.Sanpham=lsProduct;
                return View(product);
            }
            catch 
            {
                return RedirectToAction("Index", "Home");
            }
          
        }

        //[HttpPost]
        //[Authorize]
        //public IActionResult AddComment(CommentViewModel comment)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        // Thực hiện lưu trữ bình luận vào cơ sở dữ liệu

        //        var taikhoanID = HttpContext.Session.GetString("CustomerId");


        //            // Người dùng đã đăng nhập, lấy CustomerId của họ
        //           CommentViewModel model = new CommentViewModel();
        //        if (taikhoanID != null)
        //        {
        //            Comment com= new Comment();
        //            var cmt = _context.Comments.AsNoTracking().Include(p=>p.Product).Include(p => p.Customer);
        //            var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(taikhoanID));
        //            model.CustomerId = khachhang.CustomerId;
        //            model.FullName = khachhang.FullName;
        //            model.Phone = khachhang.Phone;
        //            model.Email = khachhang.Email;
        //            model.Comment = com.CommentText;
        //            com.CreatedAt= DateTime.Now;


        //            _context.Comments.Add(com);
        //            _context.SaveChanges();
        //        }
        //        else
        //        {
        //            // Người dùng chưa đăng nhập, xử lý lỗi hoặc chuyển hướng đến trang đăng nhập
        //            return RedirectToAction("Login", "Account");
        //        }
        //    }

        //    // Chuyển hướng trở lại trang sản phẩm
        //    return RedirectToAction("Details");
        //}



        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment(Comment comment)
        {
            if (ModelState.IsValid)
            {
                List<string> bannedWords = new List<string> { "sex", "chịch", "chjch","đụ má mày", "đm", "clm", "con mẹ mày", "thằng ml", "ăn con cặc gì ngu vậy?", "ngu như chó", "não bị úng hả", "mả cha mày" };
                var product = await _context.Products
                           .AsNoTracking()
                           .Where(p => p.ProductId == comment.ProductId)
                           .Include(p => p.Cat)
                           .FirstOrDefaultAsync();
                // Kiểm tra xem bình luận có chứa từ cấm không
                if (bannedWords.Any(word => comment.CommentText.Contains(word)))
                {
                    // Thông báo cho người dùng và không thêm bình luận
                    toastNotification.AddWarningToastMessage("Bình luận của bạn chứa từ ngữ không phù hợp.");
                    return RedirectToAction("Details", "Product", new { Alias = product.Alias, id = product.ProductId });
                }

                
                if (User.Identity.IsAuthenticated)
                {
                    var taikhoanID = HttpContext.Session.GetString("CustomerId");
                    var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(taikhoanID));
                    if (khachhang != null)
                    {
                        // Người dùng đã đăng nhập, lấy CustomerId của họ
                        var customerId = khachhang;

                        // Thực hiện lưu trữ bình luận vào cơ sở dữ liệu
                        var commentss = new Comment
                        {
                            CustomerId = khachhang.CustomerId,
                            ProductId = comment.ProductId,
                            CommentText = comment.CommentText,
                            CustomerName = khachhang.FullName,
                            CreatedAt = DateTime.Now,
                            Active = 1,
                            Rating = comment.Rating
                        };

                        _context.Comments.Add(commentss);
                        _context.SaveChanges();
                        

                        if (product != null)
                        {
                            toastNotification.AddSuccessToastMessage("Phan hoi thanh cong");
                            
                            return RedirectToAction("Details", "Product", new { Alias = product.Cat.Alias, id = product.ProductId });
                        }
                    }
                    else
                    {
                        
                        toastNotification.AddWarningToastMessage("Vui long dang nhap de binh luan");
                        return RedirectToAction("Login", "Account");
                    }
                }
                else
                {
                    toastNotification.AddWarningToastMessage("Vui long dang nhap de binh luan");
                    return RedirectToAction("Login", "Account");
                }
                    
            }

            // Chuyển hướng trở lại trang sản phẩm
            return RedirectToAction("Details","Product");
        }
        [HttpGet]
        public async Task<IActionResult> LoadComments(int productId)
        {
            var comments = _context.Comments.Where(c => c.ProductId == productId).ToList();
            
            var product = await _context.Products
                        .AsNoTracking()
                        .Where(p => p.ProductId == productId)
                        .Include(p => p.Cat)
                        .FirstOrDefaultAsync();

            if (product != null)
            {
                
                return RedirectToAction("Details", "Product", new { Alias = product.Cat.Alias, id = product.ProductId });
            }

            
            return RedirectToAction("Index", "Home");
        }

        //[HttpPost]
        //public ActionResult PostComment(int productId, string commentText)
        //{
        //    if (!User.Identity.IsAuthenticated)
        //    {
        //        // Redirect to login page if user is not authenticated
        //        return RedirectToAction("Login", "Account");
        //    }



        //    var comment = new Comment
        //    {

        //        ProductId = productId,
        //        CommentText = commentText,
        //        CreatedAt = DateTime.Now
        //    };

        //    _context.Comments.Add(comment);
        //    _context.SaveChanges();

        //    return RedirectToAction("ProductDetails");
        //}

    }
}

