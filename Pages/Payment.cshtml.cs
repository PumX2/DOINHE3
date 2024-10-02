using DOINHE.Db;
using DOINHE.Entitys;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Net.payOS;
using Net.payOS.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Text.Json;
using System.Threading.Tasks;

namespace DOINHE.Pages
{
    public class PaymentModel : PageModel
    {
        private readonly PayOS _payOS;
        private readonly MyDbContext _db;

        public PaymentModel(PayOS payOS, MyDbContext db)
        {
            _db = db;
            _payOS = payOS;
        }

        public string PaymentUrl { get; private set; }
        public Entitys.Product products { get; set; }
        public User users { get; set; }
        public int UserIds { get; set; }

        // Tải sản phẩm từ CSDL và hiển thị trang Payment
        public async Task<IActionResult> OnGetAsync(int id, int UserId)
        {
            // Lấy session từ HttpContext
            var accountJson = HttpContext.Session.GetString("Account");

            if (!string.IsNullOrEmpty(accountJson))
            {
                // Giải mã đối tượng từ JSON
                var member = JsonSerializer.Deserialize<User>(accountJson);

                if (member != null)
                {
                    UserIds = (int)member.Id;  // Lấy UserId
                }
            }
            var product = await _db.Products.FirstOrDefaultAsync(a => a.Id == id);
            var user = await _db.Users.FirstOrDefaultAsync(a => a.Id == UserIds);

            if (product == null)
            {
                return NotFound();
            }
            if (user == null)
            {
                return NotFound();
            }

            products = product;
            users = user;
            HttpContext.Session.SetString("Product", JsonSerializer.Serialize(product));
            return Page(); // Trả về trang Payment mà không chuyển hướng
        }

        // Tạo liên kết thanh toán khi người dùng nhấn nút "Create Payment Link"
        public async Task<IActionResult> OnPostCreatePaymentLinkAsync(int availableCredit, int id)
        {
            var product = await _db.Products.FirstOrDefaultAsync(a => a.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            // Lấy thông tin người dùng từ Session
            var accountJson = HttpContext.Session.GetString("Account");
            if (string.IsNullOrEmpty(accountJson))
            {
                return RedirectToPage("/Login"); // Chuyển hướng tới trang đăng nhập nếu người dùng chưa đăng nhập
            }

            var user = JsonSerializer.Deserialize<User>(accountJson);
            if (user == null)
            {
                return NotFound();
            }

            // Kiểm tra nếu số tiền có sẵn lớn hơn hoặc bằng giá sản phẩm
            if (availableCredit >= product.Price)
            {
                // Cập nhật trạng thái sản phẩm thành đã mua
                product.StatusIsBuy = true;
                await _db.SaveChangesAsync();
                // Trừ tiền của người dùng bằng giá sản phẩm
                user.Money -= product.Price;
                var userdb = _db.Users.FirstOrDefault(x => x.Id == user.Id);
                userdb.Money -= product.Price;
                //_db.Users.Update(user);
                //_db.Entry(user).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                // Tạo đơn hàng mới
                var newOrder = new Order
                {
                    ProductId = product.Id,
                    UserId = user.Id,
                    OrderDate = DateTime.Now,
                    Price = product.Price,
                    Status = false // Đã thanh toán
                };

                _db.Orders.Add(newOrder);
                await _db.SaveChangesAsync();

                // Thông báo thành công mà không cần tạo liên kết thanh toán
                TempData["Message"] = "Mua hàng thành công mà không cần thanh toán thêm.";
                return RedirectToPage("/Profile"); // Chuyển hướng đến trang thành công hoặc trang khác bạn muốn
            }

            // Tính toán tổng thanh toán còn lại
            int totalPayment = (int)(product.Price - availableCredit);

            if (totalPayment < 0)
            {
                ModelState.AddModelError(string.Empty, "Số tiền thanh toán không hợp lệ.");
                return Page();
            }

            products = product;
            //string returnURL = "https://localhost:7040/PaymentSuccess?id=" + product.Id;
            //string returnURL2 = "https://localhost:7040/portfolio-details?id=" + product.Id;
            try
            {
                // Tạo mã đơn hàng (orderCode)
                long orderCode = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                // Tạo danh sách sản phẩm
                List<ItemData> items = new List<ItemData> { new ItemData(product.ProductName, 1, totalPayment) };

                // Tạo dữ liệu thanh toán
                PaymentData paymentData = new PaymentData(
                    orderCode: orderCode,
                    amount: totalPayment, // Số tiền thanh toán thực sự
                    description: "Thanh toán cho dịch vụ",
                    items: items,
                    cancelUrl: "https://localhost:7040/Index",
                    returnUrl: "https://localhost:7040/PaymentSuccess"
                );

                // Gọi API tạo liên kết thanh toán
                CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData);

                // Kiểm tra kết quả
                if (createPayment != null && !string.IsNullOrEmpty(createPayment.checkoutUrl))
                {

                    PaymentUrl = createPayment.checkoutUrl;
                    return Page();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Failed to create payment link. No URL returned.");
                }
            }
            catch (Exception ex)
            {
                // Ghi lại thông tin lỗi chi tiết
                ModelState.AddModelError(string.Empty, "Error creating payment link: " + ex.Message);
            }

            return Page();
        }



    }
}
