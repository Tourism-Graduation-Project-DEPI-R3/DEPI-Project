using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tourism.Models;
using Tourism.Models.Relations;
using Tourism.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Tourism.Repository;
using Tourism.IRepository;



namespace Tourism.Controllers
{
    [Authorize(Roles = "Merchant")]
    public class MerchantController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITouristRepository _touristRepository;
        private readonly IMerchantRepository _merchantRepository;
        private readonly IProductRepository _productRepository;

        public MerchantController(IUnitOfWork unitOfWork, ITouristRepository touristRepository,IMerchantRepository merchantRepository, IProductRepository productRepository)
        {
            _unitOfWork = unitOfWork;
            _touristRepository = touristRepository;
            _merchantRepository = merchantRepository;
            _productRepository = productRepository;
        }


      
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel merchantFromRequest)
        {

            if (!ModelState.IsValid)
            {
                merchantFromRequest.msg = "";
                return View(merchantFromRequest);
            }

            var merchant = _merchantRepository.GetByEmail(merchantFromRequest.email);

            if (merchant == null)
            {
                merchantFromRequest.msg = "Email or Password is incorrect! Please try again.";
                return View(merchantFromRequest);
            }

            var passwordHasher = new PasswordHasher<Merchant>();
            var result = passwordHasher.VerifyHashedPassword(
                merchant,
                merchant.passwordHash,
                merchantFromRequest.passwordHash
            );

            if (result == PasswordVerificationResult.Success)
            {
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, merchant.name),
            new Claim(ClaimTypes.Role, "Merchant"),
            new Claim("MerchantId", merchant.id.ToString())
        };

                var claimsIdentity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme
                );

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties
                    {
                        IsPersistent = false,
                        ExpiresUtc = DateTime.UtcNow.AddDays(7)
                    }
                );
                return RedirectToAction("MerchantDashboard");
            }
            else
            {
                merchantFromRequest.msg = "Email or Password is incorrect! Please try again.";
                return View(merchantFromRequest);
            }
        }

       


        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

     

        [HttpGet]
        [AllowAnonymous]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SignUpAsync(Merchant MerchantFromRequest)
        {
            var card = _unitOfWork.GetCC(MerchantFromRequest.creditCard);
            if (ModelState.IsValid && card != null)
            {
                // Create a password hasher instance
                var hasher = new PasswordHasher<Merchant>();
                // Hash the password
                MerchantFromRequest.passwordHash = hasher.HashPassword(MerchantFromRequest, MerchantFromRequest.passwordHash);
                MerchantFromRequest.creditCard = card;
                await _unitOfWork.Merchants.AddAsync(MerchantFromRequest);
                await _unitOfWork.SaveAsync();
                return RedirectToAction("Login", "Merchant");
            }

            return View(MerchantFromRequest);
        }

        [HttpGet]
        public async Task<IActionResult> MerchantDashBoardAsync()
        {
            int id = int.Parse(User.FindFirst("MerchantId").Value);
            MerchantDashboardViewModel dashboardModel = new();
             _merchantRepository.PrepareDashboard(dashboardModel, id);
            return View(dashboardModel);
        }

        public async Task<IActionResult> AddProductAsync()
        {
            int merchantId = int.Parse(User.FindFirst("MerchantId")?.Value);
            var merchant = await _unitOfWork.Merchants.GetByIdAsync(merchantId);
            if (merchant.verified == false)
            {
                return RedirectToAction("SendVerificationRequest");
            }

            return View(new ProductViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> AddProductAsync(ProductViewModel productFromRequest)
        {
            if (productFromRequest.UploadedImages == null || productFromRequest.UploadedImages.Count < 4)
            {
                ModelState.AddModelError("UploadedImages", "Please upload at least 4 images.");
                return View(productFromRequest);
            }

            if (ModelState.IsValid)
            {
                Product p = new();
                int merchantId = int.Parse(User.FindFirst("MerchantId")?.Value);
                p.merchantId = merchantId;
                p.merchant = await _unitOfWork.Merchants.GetByIdAsync(merchantId);
                p.price = productFromRequest.price;
                p.offer = productFromRequest.offer;
                p.state = "Pending";
                p.category = productFromRequest.category;
                p.description = productFromRequest.description;
                p.count = productFromRequest.count;
                p.dateAdded = DateTime.Today;
                p.name = productFromRequest.name;
                p.DeliversWithin = (int)productFromRequest.DeliversWithin;
                await _unitOfWork.Products.AddAsync(p);
                await _unitOfWork.Products.SaveAsync();
                foreach (var file in productFromRequest.UploadedImages)
                {
                 
                    using var ms = new MemoryStream();
                    file.CopyTo(ms);

                    var image = new Image
                    {
                        imageData = ms.ToArray(),
                        serviceType = "Product",
                        serviceId = p.id
                    };
                   await _unitOfWork.Images.AddAsync(image);
                    await _unitOfWork.Images.SaveAsync();
                }
                ServiceRequest req = new();
                req.role = "Merchant";
                req.serviceId = p.id;
                await _unitOfWork.ServiceRequests.AddAsync(req);
                await _unitOfWork.ServiceRequests.SaveAsync();
                return RedirectToAction("ShowProducts");
            }
            return View(productFromRequest);
        }
        [HttpGet]
        public IActionResult ShowProducts()
        {
            int merchantId = int.Parse(User.FindFirst("MerchantId")?.Value);
            var products = _productRepository.GetProductsViewModel(merchantId);
            return View(products);
        }
        [HttpPost]
        public IActionResult DeleteProduct(int id)
        {
            _productRepository.DeleteProduct(id);
            return Ok();
        }
        [HttpGet]
        public IActionResult EditProduct(int id)
        {
            int merchantId = int.Parse(User.FindFirst("MerchantId")?.Value);
            var pr = _productRepository.MerchantProduct(id, merchantId);
            ProductViewModel pvm = new();
            pvm.price = pr.price;
            pvm.offer = pr.offer;
            pvm.id = pr.id;
            pvm.category = pr.category;
            pvm.description = pr.description;
            pvm.name = pr.name;
            pvm.count = pr.count;

            return View(pvm);
        }//dont forget pics
        [HttpPost]
        public async Task<IActionResult> EditProductAsync(ProductViewModel productFromRequest)
        {
            if (productFromRequest.UploadedImages == null || productFromRequest.UploadedImages.Count() < 4)
            {
                ModelState.AddModelError("UploadedImages", "Please upload at least 4 images.");
                return View(productFromRequest);
            }
            if (ModelState.IsValid)
            {

                Product p = new();
                int merchantId = int.Parse(User.FindFirst("MerchantId")?.Value);
                p.merchantId = merchantId;
                p.merchant = await _unitOfWork.Merchants.GetByIdAsync(merchantId);
                p.price = productFromRequest.price;
                p.offer = productFromRequest.offer;
                p.state = "Pending";
                p.category = productFromRequest.category;
                p.description = productFromRequest.description;
                p.count = productFromRequest.count;
                p.dateAdded = DateTime.Today;
                p.name = productFromRequest.name;
                p.DeliversWithin = (int)productFromRequest.DeliversWithin;
                var oldproduct = await _unitOfWork.Products.GetByIdAsync((int) productFromRequest.id);
                oldproduct.count = 0;

                var req = _merchantRepository.GetServiceRequest(oldproduct.id);
                if (req != null)
                {
                    _unitOfWork.ServiceRequests.Delete(req);
                    await _unitOfWork.ServiceRequests.SaveAsync();
                }
                await _unitOfWork.Products.AddAsync(p);
                await _unitOfWork.Products.SaveAsync();
                foreach (var file in productFromRequest.UploadedImages)
                {
                    using var ms = new MemoryStream();
                    file.CopyTo(ms);

                    var image = new Image
                    {
                        imageData = ms.ToArray(),
                        serviceType = "Product",
                        serviceId = p.id
                    };
                    await _unitOfWork.Images.AddAsync(image);
                    await _unitOfWork.Images.SaveAsync();
                }
                req = new();
                req.role = "Merchant";
                req.serviceId = p.id;
               await _unitOfWork.ServiceRequests.AddAsync(req);
                return RedirectToAction("ShowProducts");
            }
            return View(productFromRequest);
        }
        public IActionResult SendVerificationRequest()
        {
            return View(new Merchant());
        }
        [HttpPost]
        public async Task<IActionResult> SendVerificationRequest(IFormFile verificationDocuments)
        {
            // Validate file presence
            if (verificationDocuments == null || verificationDocuments.Length == 0)
            {
                ModelState.AddModelError("", "Please upload a valid PDF file.");
                return View();
            }

            // Validate file type
            if (verificationDocuments.ContentType != "application/pdf")
            {
                ModelState.AddModelError("", "Only PDF files are allowed.");
                return View();
            }

            if (verificationDocuments.Length > 8 * 1024 * 1024)
            {
                ModelState.AddModelError("", "File size must be under 8 MB.");
                return View();
            }


            int merchantId = int.Parse(User.FindFirst("MerchantId")?.Value);


            byte[] pdfBytes;
            using (var ms = new MemoryStream())
            {
                await verificationDocuments.CopyToAsync(ms);
                pdfBytes = ms.ToArray();
            }

            var merchant = await _unitOfWork.Merchants.GetByIdAsync(merchantId);
            if (merchant == null)
            {
                return NotFound("Merchant not found.");
            }

            merchant.verificationDocuments = pdfBytes;
            _merchantRepository.UpdateVerificationDocument(merchant,pdfBytes);
            var request = new VerificationRequest
            {
                provider_Id = merchantId,
                role = "Merchant"
            };
            await _unitOfWork.VerificationRequests.AddAsync(request);
            await _unitOfWork.VerificationRequests.SaveAsync();
            TempData["Success"] = "Verification file uploaded successfully. Your verification status is pending review.";
            return RedirectToAction("MerchantDashBoard");
        }

        public IActionResult ShowOrders()
        {
            int id = int.Parse(User.FindFirst("MerchantId").Value);
            List<ProductOrdersViewModel> model = _merchantRepository.GetOrders(id);
            return View(model);
        }

    }
}
