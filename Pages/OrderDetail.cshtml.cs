using DOINHE.Db;
using DOINHE.Entitys;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace DOINHE.Pages
{
    public class OrderDetailModel : PageModel
    {
        private readonly MyDbContext _db;
        public User Users { get; set; }
        public Entitys.Product Product { get; set; }
        public Order Order { get; set; }

        public OrderDetailModel(MyDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var orders = await _db.Orders.FirstOrDefaultAsync(a => a.Id == id);
            Order = orders;

            var products = await _db.Products.FirstOrDefaultAsync(a => a.Id == Order.ProductId);
            Product = products;

            return Page();
        }

        public async Task<IActionResult> OnPostCompleteOrderAsync([FromBody] int orderId)
        {
            Console.WriteLine("Received Order ID: " + orderId);

            var order = await _db.Orders.FirstOrDefaultAsync(a => a.Id == orderId);
            if (order != null)
            {
                order.Status = true;
                await _db.SaveChangesAsync();
                return new JsonResult(new { success = true });
            }
            return new JsonResult(new { success = false });
        }
    }
}
