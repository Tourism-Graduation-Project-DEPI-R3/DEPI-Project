using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tourism.IRepository;
using Tourism.Models;
using Tourism.ViewModel;
using Tourism.Models.Relations;
using Microsoft.AspNetCore.Authorization;

namespace Tourism.Controllers
{
    public class TouristController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITouristRepository _touristRepository;
        private readonly IMerchantRepository _merchantRepository;
        private readonly IProductRepository _productRepository;

        public TouristController(IUnitOfWork unitOfWork, ITouristRepository touristRepository, IMerchantRepository merchantRepository, IProductRepository productRepository)
        {
            _unitOfWork = unitOfWork;
            _touristRepository = touristRepository;
            _merchantRepository = merchantRepository;
            _productRepository = productRepository;
        }

        [HttpGet]
        public IActionResult Login() => View(new LoginViewModel());

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel touristFromRequest)
        {
            if (!ModelState.IsValid) { touristFromRequest.msg = ""; return View(touristFromRequest); }
            var tourist = _touristRepository.GetByEmail(touristFromRequest.email);

            if (tourist == null)
            {
                touristFromRequest.msg = "Email or Password is incorrect! Please try again.";
                return View(touristFromRequest);
            }

            var passwordHasher = new PasswordHasher<Tourist>();
            var result = passwordHasher.VerifyHashedPassword(tourist, tourist.passwordHash, touristFromRequest.passwordHash);

            if (result == PasswordVerificationResult.Success)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, tourist.firstName),
                    new Claim(ClaimTypes.Role, "Tourist"),
                    new Claim("TouristId", tourist.id.ToString()),
                    // FIX: Added NameIdentifier so other controllers can use standard User.FindFirst(ClaimTypes.NameIdentifier)
                    new Claim(ClaimTypes.NameIdentifier, tourist.id.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties { IsPersistent = false, ExpiresUtc = DateTime.UtcNow.AddDays(7) });
                return RedirectToAction("EcommerceHomePage");
            }
            else
            {
                touristFromRequest.msg = "Email or Password is incorrect! Please try again.";
                return View(touristFromRequest);
            }
        }

        [HttpGet]
        public IActionResult SignUp() => View(new Tourist());

        [HttpPost]
        public async Task<IActionResult> SignUpAsync(Tourist tourist)
        {
            if (Request.Form["confirmPassword"] != tourist.passwordHash)
            {
                ModelState.AddModelError("passwordHash", "Password and Confirm Password do not match");
                return View(tourist);
            }
            if (ModelState.IsValid)
            {
                var hasher = new PasswordHasher<Tourist>();
                tourist.passwordHash = hasher.HashPassword(tourist, tourist.passwordHash);
                await _unitOfWork.Tourists.AddAsync(tourist);
                await _unitOfWork.SaveAsync();
                return RedirectToAction("Login");
            }
            return View(tourist);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult EcommerceHomePage()
        {
            EcommerceMainPageViewModel model = new();
            model.NewArrivals = _productRepository.NewArrivals();
            model.BestSellers = _productRepository.BestSellers();
            return View(model);
        }

        [HttpGet]
        public IActionResult ShowProductsByCategory(string category)
        {
            var model = _productRepository.GetByCategory(category);
            ViewBag.category = category;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ViewProductAsync(int id)
        {
            Product p = await _unitOfWork.Products.GetByIdAsync(id);
            Merchant m = await _unitOfWork.Merchants.GetByIdAsync(p.merchantId);
            ProductViewModel model = _productRepository.MergeProductToViewModel(p);
            model.MerchantName = m.name;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCartAsync(int productId)
        {
            var claim = User.FindFirst("TouristId");
            if (claim == null) return Unauthorized();
            int touristId = int.Parse(claim.Value);
            bool result = await _touristRepository.AddToCartAsync(productId, touristId);
            if (result) return Ok();
            return NotFound();
        }

        [HttpGet]
        [Authorize(Roles = "Tourist")]
        public IActionResult ViewCart()
        {
            var claim = User.FindFirst("TouristId");
            int touristId = int.Parse(claim.Value);
            List<ProductViewModel> model = _productRepository.GetCartProducts(touristId);
            return View(model);
        }

        [HttpGet]
        public IActionResult ShowProductsBySearch(string Search)
        {
            var model = _productRepository.GetBySearch(Search);
            ViewBag.search = Search;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ApplyFilter(double? minPrice, double? maxPrice, string? sort, string? category, string? Search)
        {
            List<ProductViewModel> allProducts = new();
            if (Search != null) { allProducts = _productRepository.GetBySearch(Search); ViewBag.Search = Search; }
            if (category != null) { allProducts = _productRepository.GetByCategory(category); ViewBag.category = category; }
            var filteredList = _productRepository.GetByFilter(allProducts, minPrice, maxPrice, sort);
            return View("ShowProductsByCategory", filteredList);
        }

        [HttpGet]
        [Authorize(Roles = "Tourist")]
        public async Task<IActionResult> BalanceAsync()
        {
            var claim = User.FindFirst("TouristId");
            int touristId = int.Parse(claim.Value);
            var tourist = await _unitOfWork.Tourists.GetByIdAsync(touristId);
            TransactionViewModel model = new();
            model.balance = tourist.balance;
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Tourist")]
        public async Task<IActionResult> Balance(TransactionViewModel model, decimal money, string operation)
        {
            if (!ModelState.IsValid) { return View(model); }
            var claim = User.FindFirst("TouristId");
            int touristId = int.Parse(claim.Value);
            var tourist = await _unitOfWork.Tourists.GetByIdAsync(touristId);
            var card = _unitOfWork.GetCC(model.cc);
            if (card == null) { TempData["Message"] = "The card information is invalid."; TempData["AlertType"] = "danger"; return View(model); }
            if (operation == "deposit")
            {
                if (money > card.Balance) { TempData["Message"] = "Insufficient card balance."; TempData["AlertType"] = "warning"; return View(model); }
                _touristRepository.TransactionAsync(card, money, operation, touristId);
            }
            if (operation == "withdraw")
            {
                if (tourist.balance < (double)money) { TempData["Message"] = "Insufficient balance."; TempData["AlertType"] = "warning"; return View(model); }
                _touristRepository.TransactionAsync(card, money, operation, touristId);
            }
            return RedirectToAction("EcommerceHomePage");
        }

        [HttpPost]
        [Authorize(Roles = "Tourist")]
        public IActionResult ProceedToCheckout(List<ProductViewModel> purchaseData) => View(purchaseData);

        [HttpPost]
        [Authorize(Roles = "Tourist")]
        public async Task<IActionResult> PurchaseProductAsync(List<ProductViewModel> purchaseData, bool paymentType, CreditCard cc, string country, string city, string address)
        {
            double total = 0;
            foreach (var p in purchaseData) total += (p.price * p.count);
            var claim = User.FindFirst("TouristId");
            int touristId = int.Parse(claim.Value);
            if (paymentType == true)
            {
                var card = _unitOfWork.GetCC(cc);
                if (card == null) { TempData["ErrorMessage"] = "Invalid credit card information."; return View("ProceedToCheckout", purchaseData); }
                else if (card.Balance < (decimal)total) { TempData["ErrorMessage"] = "Insufficient balance in your credit card."; return View("ProceedToCheckout", purchaseData); }
                foreach (var p in purchaseData) _touristRepository.BuyProduct(touristId, true, card, p);
                return RedirectToAction("EcommerceHomePage");
            }
            else
            {
                var torist = await _unitOfWork.Tourists.GetByIdAsync(touristId);
                if (torist.balance < total) { TempData["ErrorMessage"] = "Insufficient balance in your account."; return View("ProceedToCheckout", purchaseData); }
                foreach (var p in purchaseData) _touristRepository.BuyProduct(touristId, false, null, p);
                return RedirectToAction("EcommerceHomePage");
            }
        }
    }
}