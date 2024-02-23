using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REALLY9.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace REALLY9.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SearchController : Controller
    {
        private readonly Really9Context _context;
        public SearchController(Really9Context context)
        {
            _context = context;
        }
        //Search Product
        [HttpPost]
        public IActionResult FindProduct(string keyword)
        {
            List<Product> ls = new List<Product>();
            if(string.IsNullOrEmpty(keyword)||keyword.Length <1) 
            {
                return PartialView("ListProductsSearchPartial", null);
            }
            ls = _context.Products
                .AsNoTracking() 
                .Include(a =>a.Cat)
                .Where(x=>x.ProductName.Contains(keyword))
                .OrderByDescending(x=>x.ProductName)
                .Take(10)
                .ToList();
            if (ls == null)
            {
                return PartialView("ListProductsSearchPartial", null);
            }
            else
            {
                return PartialView("ListProductsSearchPartial", ls);
            }
        }
        public IActionResult FindCustomer(string keyword)
        {
            List<Customer> customers = new List<Customer>(); // Changed from Product to Customer
            if (string.IsNullOrEmpty(keyword) || keyword.Length < 1)
            {
                return PartialView("ListCustomersSearchPartial", null); // Changed the PartialView name
            }
            customers = _context.Customers // Changed from Products to Customers
                .AsNoTracking()
                // Removed the Include line as it might not apply to customers
                .Where(x => x.FullName.Contains(keyword)) // Changed the property name to match Customer
                .OrderByDescending(x => x.FullName) // Changed to sort by CustomerName
                .Take(10)
                .ToList();

            if (customers == null)
            {
                return PartialView("ListCustomersSearchPartial", null); // Changed the PartialView name
            }
            else
            {
                return PartialView("ListCustomersSearchPartial", customers); // Changed the PartialView name
            }
        }
        public IActionResult FindAllCustomers()
        {
            var customers = _context.Customers.ToList();
            return PartialView("ListCustomersSearchPartial", customers); 
        }

        public IActionResult FindAllProducts()
        {
            var products = _context.Products.ToList();
            return PartialView("ListProductsSearchPartial", products); 
        }
    }
}
