using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tourism.Models;
using Tourism.ViewModel;
using Tourism.Models.Relations;
using Azure.Core;
using Tourism.Repository;
using Tourism.IRepository;

namespace Tourism.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController:Controller
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly ITouristRepository _touristRepository;
        private readonly IAdminRepository _adminRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMealRepository _mealRepository;
        private readonly IRoomRepository _roomRepository;

        public AdminController(IRoomRepository roomRepository ,IUnitOfWork unitOfWork, ITouristRepository touristRepository, IProductRepository productRepository, IAdminRepository adminRepository, IMealRepository mealRepository)
        {
            _unitOfWork = unitOfWork;
            _touristRepository = touristRepository;
            _productRepository = productRepository;
            _adminRepository = adminRepository;
            _mealRepository = mealRepository;
            _roomRepository = roomRepository;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel AdminFromRequest)
        {
            if (!ModelState.IsValid)
            {
                AdminFromRequest.msg = "";
                return View(AdminFromRequest);
            }

            var admin = _adminRepository.GetByEmail(AdminFromRequest.email);

            if (admin == null)
            {
                AdminFromRequest.msg = "Email or Password is incorrect! Please try again.";
                return View(AdminFromRequest);
            }

            var passwordHasher = new PasswordHasher<Admin>();
            var result = passwordHasher.VerifyHashedPassword(
                admin,
                admin.passwordHash,
                AdminFromRequest.passwordHash
            );

            if (result == PasswordVerificationResult.Success)
            {
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim("AdminId", admin.id.ToString())
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

                return RedirectToAction("AdminDashboard");
            }
            else
            {
                AdminFromRequest.msg = "Email or Password is incorrect! Please try again.";
                return View(AdminFromRequest);
            }
        }

        [HttpGet]
        public async Task<IActionResult> AdminDashboardAsync()
        {
            AdminDashboardViewModel Dashboardmodel = new();

            Dashboardmodel.touristsCount = await _unitOfWork.Tourists.CountAsync();
            Dashboardmodel.merchantsCount = await _unitOfWork.Merchants.CountAsync();
            Dashboardmodel.tourGuidesCount = await _unitOfWork.TourGuides.CountAsync();
            Dashboardmodel.hotelsCount =await _unitOfWork.Hotels.CountAsync();
            Dashboardmodel.restaurantsCount = await _unitOfWork.Restaurants.CountAsync();
            Dashboardmodel.earningsFromEcommerce = _touristRepository.TotalEearningsProducts();
            Dashboardmodel.earningsFromTrips = _touristRepository.TotalEearningsTrips();
            Dashboardmodel.earningsFromHotels = _touristRepository.TotalEearningsHotels();
            Dashboardmodel.earningsFromRestaurants = _touristRepository.TotalEearningsRestaurants();

            return View(Dashboardmodel);
        }


        [HttpGet]
        public IActionResult VerificationRequests(string type)
        {
            List<VerificationViewModel> requests = new();

            if (type == "Merchant")
            {
                requests = _adminRepository.GetVerificationViewModels("Merchant");
            }
            else if (type == "TourGuide")
            {
                requests = _adminRepository.GetVerificationViewModels("TourGuide");
            }
            else if (type == "Hotel")
            {
                requests = _adminRepository.GetVerificationViewModels("Hotel");
            }
            else if (type == "Restaurant")
            {
                requests = _adminRepository.GetVerificationViewModels("Restaurant");
            }

            return View(requests);
        }

        public async Task<IActionResult> DownloadPdfAsync(string type, int id)
        {
            byte[] pdf = null;
            string name = "";

            switch (type)
            {
                case "Merchant":
                    var mer = await _unitOfWork.Merchants.GetByIdAsync(id);
                    if (mer != null)
                    {
                        pdf = mer.verificationDocuments;
                        name = mer.name;
                    }
                    break;

                case "Hotel":
                    var hotel =await _unitOfWork.Hotels.GetByIdAsync(id);
                    if (hotel != null)
                    {
                        pdf = hotel.verificationDocuments;
                        name = hotel.name;
                    }
                    break;

                case "Restaurant":
                    var res = await _unitOfWork.Restaurants.GetByIdAsync(id);
                    if (res != null)
                    {
                        pdf = res.verificationDocuments;
                        name = res.name;
                    }
                    break;

                case "TourGuide":
                    var tour = await _unitOfWork.TourGuides.GetByIdAsync(id);
                    if (tour != null)
                    {
                        pdf = tour.verificationDocuments;
                        name = tour.firstName;
                    }
                    break;
            }

            if (pdf == null || pdf.Length == 0)
                return NotFound();

            return File(pdf, "application/pdf", $"{name}_verification.pdf");
        }

        public async Task<IActionResult> AcceptRequestAsync(string type, int providerId, int requestId)
        {
  
            var msg = new InboxMsg
            {
                providerType = type,
                content = "You are now officially verified and can upload your services!",
                providerId = providerId,
                date = DateTime.Today
            };
            await _unitOfWork.InboxMessages.AddAsync(msg);
            await _unitOfWork.InboxMessages.SaveAsync();

            var req = await _unitOfWork.VerificationRequests.GetByIdAsync(requestId);
            if (req != null)
            {
                _unitOfWork.VerificationRequests.Delete(req);
                await _unitOfWork.VerificationRequests.SaveAsync();
            }

            if (type == "Merchant")
            {
                var mer = await _unitOfWork.Merchants.GetByIdAsync(providerId);
                if (mer != null)
                    mer.verified = true;
               await _unitOfWork.Merchants.SaveAsync();
            }
            else if (type == "Hotel")
            {
                var hot = await _unitOfWork.Hotels.GetByIdAsync(providerId);
                if (hot != null)
                    hot.verified = true;
                await _unitOfWork.Hotels.SaveAsync();
            }
            else if (type == "Restaurant")
            {
                var res = await _unitOfWork.Restaurants.GetByIdAsync(providerId);
                if (res != null)
                    res.verified = true;
                await _unitOfWork.Restaurants.SaveAsync();
            }
            else if (type == "TourGuide")
            {
                var tour = await _unitOfWork.TourGuides.GetByIdAsync(providerId);
                if (tour != null)
                    tour.verified = true;
                await _unitOfWork.TourGuides.SaveAsync();
            }


            return RedirectToAction("VerificationRequests", new { type });
        }
        public async Task<IActionResult> DenyRequestAsync(string type, int providerId, int requestId)
        {

            var msg = new InboxMsg
            {
                providerType = type,
                content = "Unfortunetly your verification request has been denied, please read the instructions well and make sure to provide all the requested info.",
                providerId = providerId,
                date = DateTime.Today
            };
            await _unitOfWork.InboxMessages.AddAsync(msg);
            await _unitOfWork.InboxMessages.SaveAsync();

            var req =await _unitOfWork.VerificationRequests.GetByIdAsync(requestId);
            if (req != null)
            {
                _unitOfWork.VerificationRequests.Delete(req);
                await _unitOfWork.VerificationRequests.SaveAsync();
            }
            return RedirectToAction("VerificationRequests", new { type });
        }


        [HttpGet]
        public IActionResult ServiceRequests(string type)
        {
            List<ServiceRequestsViewModel> requests = new();
             type = type.Trim();
            if(type=="Merchant" || type=="Hotel" || type=="Restaurant" || type=="TourGuide")
                requests = _adminRepository.GetServiceRequestsViewModels(type);
            
            else return NotFound();
            return View(requests);
        }

        [HttpGet]
        public async Task<IActionResult> ViewProductAsync(int id)
        {
            Product product = await _unitOfWork.Products.GetByIdAsync(id);
            List<Image> images = _productRepository.GetPics(id);
            ProductViewModel productModel = new();
            productModel.name = product.name;
            productModel.description = product.description;
            productModel.price = product.price;
            productModel.category = product.category;
            productModel.count = product.count;
            productModel.offer = product.offer;
            productModel.id = product.merchantId;
            productModel.productId = id;
            int index = 0;
            foreach (var img in images)
            {
                if (img.imageData != null && img.imageData.Length > 0)
                {
                    // Create a memory stream from the image bytes
                    var stream = new MemoryStream(img.imageData);

                    // Create an IFormFile from the memory stream
                    var file = new FormFile(stream, 0, img.imageData.Length, $"image{index}", $"image{index}.jpg");

                    // Add to the view model list
                    productModel.UploadedImages.Add(file);

                    index++;
                }
            }
            return View(productModel);
        }

        [HttpPost]
        public async Task<IActionResult> AcceptServiceAsync(int serviceId,string role,string msg,int providerId)
        {
            if (role == "Merchant")
            {
                if (msg != null)
                {
                    InboxMsg inboxMsg = new();
                    inboxMsg.content = msg;
                    inboxMsg.date = DateTime.Now;
                    inboxMsg.providerType = "Merchant";
                    inboxMsg.providerId = providerId;
                    await _unitOfWork.InboxMessages.AddAsync(inboxMsg);
                    await _unitOfWork.InboxMessages.SaveAsync();
                }
                var product = await _unitOfWork.Products.GetByIdAsync(serviceId);
                product.state = "Approved";
                await _unitOfWork.Products.SaveAsync();
            }
            else if (role == "TourGuide")
            {
                if (msg != null)
                {
                    InboxMsg inboxMsg = new();
                    inboxMsg.content = msg;
                    inboxMsg.date = DateTime.Now;
                    inboxMsg.providerType = "TourGuide";
                    inboxMsg.providerId = providerId;
                    await _unitOfWork.InboxMessages.AddAsync(inboxMsg);
                    await _unitOfWork.InboxMessages.SaveAsync();
                }
                var trip =await _unitOfWork.Trips.GetByIdAsync(serviceId);
                trip.accepted = true;
                await _unitOfWork.Trips.SaveAsync();

            }
            else if (role == "Restaurant")
            {
                if (msg != null)
                {
                    InboxMsg inboxMsg = new();
                    inboxMsg.content = msg;
                    inboxMsg.date = DateTime.Now;
                    inboxMsg.providerType = "Restaurant";
                    inboxMsg.providerId = providerId;
                    await _unitOfWork.InboxMessages.AddAsync(inboxMsg);
                    await _unitOfWork.InboxMessages.SaveAsync();
                }
                var meal = await _unitOfWork.Meals.GetByIdAsync(serviceId);
                meal.accepted = true;
                await _unitOfWork.Meals.SaveAsync();
            }
            else if (role == "Hotel")
            {
                if (msg != null)
                {
                    InboxMsg inboxMsg = new();
                    inboxMsg.content = msg;
                    inboxMsg.date = DateTime.Now;
                    inboxMsg.providerType = "Hotel";
                    inboxMsg.providerId = providerId;
                    await _unitOfWork.InboxMessages.AddAsync(inboxMsg);
                    await _unitOfWork.InboxMessages.SaveAsync();
                }
                var room = await _unitOfWork.Rooms.GetByIdAsync(serviceId);
                room.accepted = true;
                await _unitOfWork.Rooms.SaveAsync();
            }
            var service = _adminRepository.GetServiceRequest(serviceId, role);
            if (service != null)
            {
                _unitOfWork.ServiceRequests.Delete(service);
                await _unitOfWork.ServiceRequests.SaveAsync();
            }
            return RedirectToAction("ServiceRequests", new { type = role });
        }

        [HttpPost]
        public async Task<IActionResult> RejectServiceAsync(int serviceId, string role, string msg, int providerId)
        {
            if (role == "Merchant")
            {
                if (msg != null)
                {
                    InboxMsg inboxMsg = new();
                    inboxMsg.content = msg;
                    inboxMsg.date = DateTime.Now;
                    inboxMsg.providerType = "Merchant";
                    inboxMsg.providerId = providerId;
                    await _unitOfWork.InboxMessages.AddAsync(inboxMsg);
                    await _unitOfWork.InboxMessages.SaveAsync();
                }
                var product = await _unitOfWork.Products.GetByIdAsync(serviceId);
                product.state = "Rejected";
                await _unitOfWork.Products.SaveAsync();
            }
            else if (role == "TourGuide")
            {
                if (msg != null)
                {
                    InboxMsg inboxMsg = new();
                    inboxMsg.content = msg;
                    inboxMsg.date = DateTime.Now;
                    inboxMsg.providerType = "TourGuide";
                    inboxMsg.providerId = providerId;
                    await _unitOfWork.InboxMessages.AddAsync(inboxMsg);
                    await _unitOfWork.InboxMessages.SaveAsync();
                }

            }
            else if (role == "Restaurant")
            {
                if (msg != null)
                {
                    InboxMsg inboxMsg = new();
                    inboxMsg.content = msg;
                    inboxMsg.date = DateTime.Now;
                    inboxMsg.providerType = "Restaurant";
                    inboxMsg.providerId = providerId;
                    await _unitOfWork.InboxMessages.AddAsync(inboxMsg);
                    await _unitOfWork.InboxMessages.SaveAsync();
                }
            }
            else if (role == "Hotel")
            {
                if (msg != null)
                {
                    InboxMsg inboxMsg = new();
                    inboxMsg.content = msg;
                    inboxMsg.date = DateTime.Now;
                    inboxMsg.providerType = "Hotel";
                    inboxMsg.providerId = providerId;
                    await _unitOfWork.InboxMessages.AddAsync(inboxMsg);
                    await _unitOfWork.InboxMessages.SaveAsync();
                }
            }
            var service = _adminRepository.GetServiceRequest(serviceId, role);
            if (service != null)
            {
                _unitOfWork.ServiceRequests.Delete(service);
                await _unitOfWork.ServiceRequests.SaveAsync();
            }
            return RedirectToAction("ServiceRequests", new { type = role });
        }

        public IActionResult ShowOrders()
        {
         
            List<ProductViewModel> model = _productRepository.ShowOrdersAdmin();
            return View(model);
        }

        public void ShipProduct(int orderId)
        {
            _adminRepository.ShipProduct(orderId);
        }

        public IActionResult RefreshTransactions()
        {
            _adminRepository.RefreshEcommerceTransactions();
            return RedirectToAction("AdminDashboard");
        }


        //////////////////
        public async Task<IActionResult> ViewMealAsync(int id)
        {
            Meal meal = await _unitOfWork.Meals.GetByIdAsync(id);
            if (meal.restaurant == null)
                meal.restaurant = await _unitOfWork.Restaurants.GetByIdAsync(meal.restaurantId);

            Image image = _mealRepository.GetPic(id);

            MealViewModel mealModel = new()
            {
                name = meal.name,
                description = meal.description,
                price = meal.price,
                id = meal.id,
                mealId = id,
                RestaurantName = meal.restaurant.name,
                RestaurantId = meal.restaurantId,
                ImageBytes = image?.imageData
            };

            return View(mealModel);
        }
        [HttpGet]
        public async Task<IActionResult> ViewRoomServiceRequest(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            Room room = await _unitOfWork.Rooms.GetByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            List<Image> images = _roomRepository.GetPics(id);

            RoomViewModel roomModel = new()
            {
                id = room.id,
                hotelid = room.hotelId,
                number = room.number,
                capacity = room.numberOfMembers,
                price = (decimal)room.cost, // تحويل double إلى decimal
                description = $"Room {room.number} with capacity for {room.numberOfMembers} guests",
                count = 1,
                offer = false,
                discount = 0,
                accepted = room.accepted

            };


            foreach (var img in images)
            {
                roomModel.ImageIds.Add(img.id);
            }

            return View(roomModel);
        }
        [HttpPost]
        public async Task<IActionResult> AcceptRoom(int serviceId, string msg, int providerId)
        {
            try
            {
                if (!string.IsNullOrEmpty(msg))
                {
                    InboxMsg inboxMsg = new()
                    {
                        content = msg,
                        date = DateTime.Now,
                        providerType = "Hotel",
                        providerId = providerId
                    };
                    await _unitOfWork.InboxMessages.AddAsync(inboxMsg);
                    await _unitOfWork.InboxMessages.SaveAsync();
                }

                // تحديث حالة الغرفة إلى مقبولة
                var room = await _unitOfWork.Rooms.GetByIdAsync(serviceId);
                if (room != null)
                {
                    room.accepted = true;
                    await _unitOfWork.Rooms.SaveAsync();
                }

                // حذف طلب الخدمة من جدول ServiceRequests
                var serviceRequest = _adminRepository.GetServiceRequest(serviceId, "Hotel");
                if (serviceRequest != null)
                {
                    _unitOfWork.ServiceRequests.Delete(serviceRequest);
                    await _unitOfWork.ServiceRequests.SaveAsync();
                }

                TempData["SuccessMessage"] = "Room accepted successfully!";
                return RedirectToAction("ServiceRequests", new { type = "Hotel" });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while accepting the room.";
                return RedirectToAction("ServiceRequests", new { type = "Hotel" });
            }
        }





        [HttpPost]
        public async Task<IActionResult> DenyRoom(int serviceId, string msg, int providerId)
        {
            try
            {
                var room = await _unitOfWork.Rooms.GetByIdAsync(serviceId);
                if (room == null)
                {
                    TempData["ErrorMessage"] = "Room not found.";
                    return RedirectToAction("ServiceRequests", new { type = "Hotel" });
                }


                var allImages = await _unitOfWork.Images.GetAllAsync();
                foreach (var image in allImages)
                {
                    if (image.serviceId == serviceId && image.serviceType == "Room")
                    {
                        _unitOfWork.Images.Delete(image);
                    }
                }
                await _unitOfWork.Images.SaveAsync();


                _unitOfWork.Rooms.Delete(room);
                await _unitOfWork.Rooms.SaveAsync();


                var serviceRequest = _adminRepository.GetServiceRequest(serviceId, "Hotel");
                if (serviceRequest != null)
                {
                    _unitOfWork.ServiceRequests.Delete(serviceRequest);
                    await _unitOfWork.ServiceRequests.SaveAsync();
                }





                TempData["AlertMessage"] = $"Room {room.number} rejected and removed Please Try to Submit again.";
                return RedirectToAction("ServiceRequests", new { type = "Hotel" });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while rejecting the room.";
                return RedirectToAction("ServiceRequests", new { type = "Hotel" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            
            return RedirectToAction("Login", "Admin");
        }
    }
}
