namespace Tourism.ViewModel
{
    public class EcommerceMainPageViewModel
    {
       public List<ProductViewModel> BestSellers { get; set; } = new();
        public List<ProductViewModel> NewArrivals { get; set; } = new();
    }
}
