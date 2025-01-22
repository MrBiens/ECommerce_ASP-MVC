using AutoMapper;
using ECommerceMVC.Data;
using ECommerceMVC.ViewModels;

namespace ECommerceMVC.Helpers
{
    public class AutoMapperProfile :Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<RegisterViewModel, KhachHang>();
                //.ForMember(kh =>
                //kh.HoTen, option =>
                //option.MapFrom(RegisterViewModel =>
                //RegisterViewModel.HoTen)
                //);
        }
    }
}
