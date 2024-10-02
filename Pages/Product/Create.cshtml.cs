﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using DOINHE.Db;
using DOINHE.Entitys;
using System.Numerics;
using Microsoft.AspNetCore.Authorization;

namespace DOINHE.Pages.Product
{
    
    public class CreateModel : PageModel
    {
        private readonly DOINHE.Db.MyDbContext _context;

        public CreateModel(DOINHE.Db.MyDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
        ViewData["CategoryName"] = new SelectList(_context.Categories, "Id", "CategoryName");
        ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email");
            return Page();
        }

        [BindProperty]
        public DTO.ProductDTO Product { get; set; } = default!;
        

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
          if (!ModelState.IsValid || _context.Products == null || Product == null)
            {

                ViewData["CategoryName"] = new SelectList(_context.Categories, "Id", "CategoryName");
                ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email");
                return Page();
            }
            Entitys.Product product = new Entitys.Product()
            {
                
                ProductName = Product.ProductName,
                CategoryId = Product.CategoryId,
                UserId = int.Parse(HttpContext.Session.GetString("UserId")),
                DateTimeStart = Product.DateTimeStart,
                DateTimeEnd = Product.DateTimeEnd,
                CreateDate = DateTime.Now,
                Price = Product.Price,
                Address = Product.Address,
                Description = Product.Description,
                StatusIsBuy = false,
                StatusIsApprove = false,
                ImgDescription = Static.ConvertToByte.ConvertIFormFileToByte(Product.ImgDescription!),
                Key = Product.Key,
                ImgKey = Static.ConvertToByte.ConvertIFormFileToByte(Product.ImgKey!)
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
