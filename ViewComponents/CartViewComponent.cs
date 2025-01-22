using ECommerceMVC.Helpers;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceMVC.ViewComponents
{
    public class CartViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var count= HttpContext.Session.Get<List<CartItem>>(SessionKey.Cart_Key) ?? new List<CartItem>();
            
            return View("_CartPanel", new CartModel
            {
                Quantity = count.Sum(p => p.SoLuong),
                Total = count.Sum(p => p.ThanhTien)
            });
        }
    }
}
