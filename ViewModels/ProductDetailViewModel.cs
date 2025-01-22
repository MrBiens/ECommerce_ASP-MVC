namespace ECommerceMVC.ViewModels
{
    public class ProductDetailViewModel
    {
        public int DiemDanhGia { get; set; }
        public int SoLuongTon { get; set; }
        public int MaHangHoa { get; internal set; }
        public string TenHangHoa { get; internal set; }
        public string HinhAnh { get; internal set; }
        public double DonGia { get; internal set; }
        public string Mota { get; internal set; }
        public string TenLoai { get; internal set; }
    }
}
