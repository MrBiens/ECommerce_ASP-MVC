using ECommerceMVC.Data;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceMVC.Controllers
{
    public class HangHoaController : Controller
    {
        private readonly Hshop2023Context _context;

        public HangHoaController(Hshop2023Context context)
        {
            _context = context;
        }
        public IActionResult Index(int? category)
        {
            var products = _context.HangHoas.AsQueryable();
            if (category.HasValue)
            {
                products = products.Where(p => p.MaLoai ==category.Value);
            }
            var result = products.Select(p => new ProductViewModel
            {
                MaHangHoa = p.MaHh,
                TenHangHoa = p.TenHh,
                HinhAnh = p.Hinh??"",
                DonGia = p.DonGia ?? 0,
                Mota = p.MoTa?? "",
                TenLoai = p.MaLoaiNavigation.TenLoai
            });
            return View(result);
        }
        public IActionResult Search(string? query)
        {
            var products = _context.HangHoas.AsQueryable();
            if (query!=null)
            {
                products = products.Where(p => p.TenHh.Contains(query));
            }
            var result = products.Select(p => new ProductViewModel
            {
                MaHangHoa = p.MaHh,
                TenHangHoa = p.TenHh,
                HinhAnh = p.Hinh ?? "",
                DonGia = p.DonGia ?? 0,
                Mota = p.MoTa ?? "",
                TenLoai = p.MaLoaiNavigation.TenLoai
            });
            return View(result);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var productDetail= await _context.HangHoas
                .Include(p => p.MaLoaiNavigation)
                .SingleOrDefaultAsync(p => p.MaHh ==id);
            if(productDetail == null)
            {
                TempData["Message"] = $"Khong thay san pham co ma {id}";
                return Redirect("/404");
            }
            var result = new ProductDetailViewModel
            {
                MaHangHoa = productDetail.MaHh,
                TenHangHoa = productDetail.TenHh,
                HinhAnh = productDetail.Hinh ?? "",
                DonGia = productDetail.DonGia ?? 0,
                Mota = productDetail.MoTa ?? "",
                TenLoai = productDetail.MaLoaiNavigation.TenLoai,
                SoLuongTon = 10,
                DiemDanhGia = 5
            };
            return View(result);
        }

    }
}
