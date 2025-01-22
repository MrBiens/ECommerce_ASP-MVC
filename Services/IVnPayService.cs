using ECommerceMVC.ViewModels;

namespace ECommerceMVC.Service
{
    public interface  IVnPayService
    {
        string CreatePaymentUrl(HttpContext context, VnPaymentRequestModel model);
        VnPaymentResponseModel PaymentExecute(IQueryCollection collections);
    }
}
