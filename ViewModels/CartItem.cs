namespace ECommerceMVC.ViewModels
{

    public class CartItem
    {
        public int MaHangHoa { get; set; }
        public string Hinh { get; set; }
        public string TenHangHoa { get; set; }
        public double DonGia { get; set; }
        public int SoLuong { get; set; }
        public double ThanhTien => SoLuong * DonGia;
    }
}
