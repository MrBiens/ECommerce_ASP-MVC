using System.Security.Claims;
using AspNetCoreGeneratedDocument;
using ECommerceMVC.Data;
using ECommerceMVC.Helpers;
using ECommerceMVC.Service;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace ECommerceMVC.Controllers
{
    public class CartController : Controller
    {
        private readonly Hshop2023Context _context;
        private readonly PaypalClient _paypalClient;
        private readonly IVnPayService _vnPayService;

        public CartController(Hshop2023Context context, PaypalClient paypalClient,IVnPayService vnPayService)
        {
            _context = context;
            _paypalClient = paypalClient;
            _vnPayService = vnPayService;
        }
        public List<CartItem> Cart => HttpContext.Session.Get<List<CartItem>>(SessionKey.Cart_Key) ?? new List<CartItem>();

        public IActionResult Index()
        {
            return View(Cart);
        }

        public IActionResult AddToCart(int id, int quantity = 1)
        {
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.MaHangHoa == id);
            if (item == null)
            {
                var product = _context.HangHoas.SingleOrDefault(p => p.MaHh == id);
                if (product != null)
                {
                    item = new CartItem
                    {
                        MaHangHoa = product.MaHh,
                        Hinh = product.Hinh ?? string.Empty,
                        TenHangHoa = product.TenHh,
                        DonGia = product.DonGia ?? 0,
                        SoLuong = quantity
                    };
                    gioHang.Add(item);
                }
                else
                {
                    TempData["Message"] = $"Không tìm thấy sản phẩm có mã {id}";
                    return RedirectToAction("/404");
                }
            }
            else
            {
                item.SoLuong += quantity;
            }

            HttpContext.Session.Set(SessionKey.Cart_Key, gioHang);

            return RedirectToAction("Index");

        }

        public IActionResult RemoveCart(int id)
        {
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.MaHangHoa == id);
            if (item != null)
            {
                gioHang.Remove(item);
                HttpContext.Session.Set(SessionKey.Cart_Key, gioHang);
            }

            return RedirectToAction("Index");
        }

    
        [Authorize]
        [HttpGet]
        public IActionResult Checkout()
        {

            if (Cart.IsNullOrEmpty())
            {
                return Redirect("/");
            }
                        ViewBag.PaypalClientId = _paypalClient.ClientId;


            return View(Cart);
        }
        [Authorize]
        [HttpPost]
        public IActionResult Checkout(CheckoutViewModel checkoutViewModel, string payment ="COD")
        {
            if (ModelState.IsValid)
            {
                if (payment == "Thanh toán VNPay")
                {
                    var vnPayModel = new VnPaymentRequestModel
                    {
                        Amount = Cart.Sum(p => p.ThanhTien),
                        CreatedDate = DateTime.Now,
                        Description = $"{checkoutViewModel.HoTen} {checkoutViewModel.DienThoai}",
                        FullName = checkoutViewModel.HoTen,
                        OrderId = new Random().Next(1000, 100000)
                    };
                    return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, vnPayModel));
                }
                var customerId = HttpContext.User.Claims.SingleOrDefault(c => c.Type == SessionKey.CLAIM_CUSTOMER_ID).Value;

                var customer = new KhachHang();
                if (checkoutViewModel.GiongKhachHang)
                {
                    customer = _context.KhachHangs.SingleOrDefault(c => c.MaKh == customerId);
                }
                var hoadon = new HoaDon
                {
                    MaKh = customerId,
                    HoTen = checkoutViewModel.HoTen ?? customer.HoTen,
                    DiaChi = checkoutViewModel.DiaChi ?? customer.DiaChi,
                    DienThoai = checkoutViewModel.DienThoai ?? customer.DienThoai,
                    NgayDat = DateTime.Now,
                    CachThanhToan = "COD",
                    CachVanChuyen = "GHN",
                    MaTrangThai = 0,
                    GhiChu = checkoutViewModel.GhiChu
                };
                _context.Database.BeginTransaction();

                try
                {
                    _context.Database.CommitTransaction();
                    _context.Add(hoadon);
                    _context.SaveChanges();

                    var cthds = new List<ChiTietHd>();
                    foreach (var item in Cart)
                    {
                        cthds.Add(new ChiTietHd
                        {
                            MaHd = hoadon.MaHd,
                            SoLuong = item.SoLuong,
                            DonGia = item.DonGia,
                            MaHh = item.MaHangHoa,
                            GiamGia = 0
                        });
                    }
                        _context.AddRange(cthds);
                        _context.SaveChanges();
                    HttpContext.Session.Set<List<CartItem>>(SessionKey.Cart_Key, new List<CartItem>());


                    return View("PaymentSuccess");

                }
                catch 
                {
                    _context.Database.RollbackTransaction();
                }
              
            }
            return View(Cart);

        }

        [Authorize]
        public IActionResult PaymentSuccess()
        {
            return View("PaymentSuccess");
        }

        #region Paypal payment
        [Authorize]
        [HttpPost("/Cart/create-paypal-order")]
        public async Task<IActionResult> CreatePaypalOrder(CancellationToken cancellationToken)
        {
            var tongTien = Cart.Sum(p => p.ThanhTien).ToString();
            var donViTienTe = "USD";
            var orderId = "DH" + DateTime.Now.Ticks.ToString();

            try
            {
                var response = await _paypalClient.CreateOrder(tongTien, donViTienTe, orderId);
                return Ok(response);
            }
            catch(Exception ex)
            {
                
                return BadRequest(ex.GetBaseException().Message);
            }

        }


        [Authorize]
        [HttpPost("/Cart/capture-paypal-order")]
        public async Task<IActionResult> CapturePaypalOrder(string orderId,CancellationToken cancellationToken)
        {
            try
            {
                var response = await _paypalClient.CaptureOrder(orderId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.GetBaseException().Message);
            }
        }

        #endregion

        [Authorize]
        public IActionResult PaymentFail()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public IActionResult PaymentCallBack()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);
            if ((response==null) || response.VnPayResponseCode != "00")
            {
                TempData["Message"] = $"Thanh toán VnPay thất bại:{response.VnPayResponseCode}";
                return RedirectToAction("PaymentFail");

            }
            TempData["Message"] = "Thanh toán VnPay thành công";
            return RedirectToAction("PaymentSuccess");
        }


    }
}
