using System.Security.Claims;
using AutoMapper;
using ECommerceMVC.Data;
using ECommerceMVC.Helpers;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceMVC.Controllers
{
    public class CustomerController : Controller
    {
        private readonly Hshop2023Context _context;
        private readonly IMapper _mapper;
        public CustomerController(Hshop2023Context context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        #region Register

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model, IFormFile Image)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var khachHang = _mapper.Map<KhachHang>(model);
                    khachHang.RandomKey = MyUtil.GenerateRamdomKey();
                    khachHang.MatKhau = model.MatKhau.ToMd5Hash(khachHang.RandomKey);
                    khachHang.HieuLuc = true;//sẽ xử lý khi dùng Mail để active
                    khachHang.VaiTro = 0;

                    if (Image != null)
                    {
                        khachHang.Hinh = MyUtil.UploadHinh(Image, "KhachHang");
                    }

                    _context.Add(khachHang);
                    _context.SaveChanges();
                    return RedirectToAction("Index", "HangHoa");
                }
                catch (Exception ex)
                {
                    var mess = $"{ex.Message} shh";
                }
            }
            return View();
        }
        #endregion[HttpPost]


        #region Login
        [HttpGet]
        public IActionResult Login(string? returnUrl)
        {

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
        #endregion [HttpGet]



        #region Login
        [HttpPost]
        public async Task<IActionResult> LoginAsync(string? returnUrl, LoginViewModel loginModel)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (ModelState.IsValid)
            {
                var customer = _context.KhachHangs.SingleOrDefault(c => c.MaKh == loginModel.UserName);
                if (customer == null)
                {
                    ModelState.AddModelError("UserName", "Tên đăng nhập không tồn tại");
                }
                else
                {
                    var password = loginModel.Password.ToMd5Hash(customer.RandomKey);
                    if (customer.MatKhau != password)
                    {
                        ModelState.AddModelError("Password", "Mật khẩu không đúng");
                    }
                    else
                    {
                        if (!customer.HieuLuc)
                        {
                            ModelState.AddModelError("UserName", "Tài khoản đã bị khóa");
                        }
                        else
                        {
                            if (customer.MatKhau != loginModel.Password.ToMd5Hash(customer.RandomKey))
                            {
                                ModelState.AddModelError("Password", "Sai thong tin dang nhap");
                            }
                            else
                            {
                                var claims = new List<Claim>
                                {
                                    new Claim(ClaimTypes.Name, customer.HoTen),
                                    new Claim(ClaimTypes.Email, customer.Email),
                                    new Claim(SessionKey.CLAIM_CUSTOMER_ID, customer.MaKh),
                                    //Claim dynamic
                                    new Claim(ClaimTypes.Role, customer.VaiTro.ToString())
                                };
                                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                                await HttpContext.SignInAsync(claimsPrincipal);

                                if (Url.IsLocalUrl(returnUrl))
                                {
                                    return Redirect(returnUrl);
                                }
                                else
                                {
                                    return Redirect("/");
                                }
                            }
                        }
                    }
                }
            }
            return View();
        }
        #endregion [HttpPost]

        





    }



}
