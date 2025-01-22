using ECommerceMVC.Data;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceMVC.ViewComponents
{
    public class MenuCategoryViewComponent : ViewComponent
    {
        private readonly Hshop2023Context _context;

        public MenuCategoryViewComponent(Hshop2023Context context) => _context = context; 
   
        public IViewComponentResult Invoke()
        {
            var data = _context.Loais.Select(loai => new MenuCategoryViewModel
            {
                MaLoai=loai.MaLoai,
                TenLoai = loai.TenLoai,
                SoLuong = loai.HangHoas.Count
            }).ToList();
            return View("categoryViewComponent",data); //return View(data) =default.cshtml
        }
    }
}
