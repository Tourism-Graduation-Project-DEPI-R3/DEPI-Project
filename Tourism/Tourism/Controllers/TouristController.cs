using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tourism.IRepository;
using Tourism.Models;

using Tourism.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Tourism.Models.Relations;
using Tourism.Repository;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Tourism.Controllers
{
    [Authorize(Roles = "Tourist")]
    public class TouristController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITouristRepository _touristRepository;
        private readonly IMerchantRepository _merchantRepository;
        private readonly IProductRepository _productRepository;
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IMealRepository _mealRepository;
        private readonly ITableRepository _tableRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly IRoomRepository _roomRepository;

        public TouristController(IHotelRepository hotelRepository, IRoomRepository roomRepository,IUnitOfWork unitOfWork, ITouristRepository touristRepository, IMerchantRepository merchantRepository, IProductRepository productRepository, IRestaurantRepository restaurantRepository, IMealRepository mealRepository, ITableRepository tableRepository)
        {
            _unitOfWork = unitOfWork;
            _touristRepository = touristRepository;
            _merchantRepository = merchantRepository;
            _productRepository = productRepository;
            _restaurantRepository = restaurantRepository;
            _mealRepository = mealRepository;
            _tableRepository = tableRepository;
            _hotelRepository = hotelRepository;
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
        public async Task<IActionResult> Login(LoginViewModel touristFromRequest)
        {

            if (!ModelState.IsValid)
            {
                touristFromRequest.msg = "";
                return View(touristFromRequest);
            }

            var tourist = _touristRepository.GetByEmail(touristFromRequest.email);

            if (tourist == null)
            {
                touristFromRequest.msg = "Email or Password is incorrect! Please try again.";
                return View(touristFromRequest);
            }

            var passwordHasher = new PasswordHasher<Tourist>();
            var result = passwordHasher.VerifyHashedPassword(
                tourist,
                tourist.passwordHash,
                touristFromRequest.passwordHash
            );

            if (result == PasswordVerificationResult.Success)
            {
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, tourist.firstName),
            new Claim(ClaimTypes.Role, "Tourist"),
            new Claim("TouristId", tourist.id.ToString())
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
                return RedirectToAction("TouristMain");
            }
            else
            {
                touristFromRequest.msg = "Email or Password is incorrect! Please try again.";
                return View(touristFromRequest);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult SignUp()
        {
            return View(new Tourist());
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SignUpAsync(Tourist tourist)
        {
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


        public IActionResult TouristMain()
        {
            return View();
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index","Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult EcommerceHomePage()
        {
            EcommerceMainPageViewModel model = new();
            model.NewArrivals = _productRepository.NewArrivals();
            model.BestSellers = _productRepository.BestSellers();
            return View(model);
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ShowProductsByCategory(string category)
        {
            var model = _productRepository.GetByCategory(category);
            ViewBag.category = category;
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
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

            if (_touristRepository.AddToCart(productId, int.Parse(claim.Value)) == true)
                return Ok();
            else return NotFound();
        }

        [HttpGet]
        
        public IActionResult ViewCart()
        {
            var claim = User.FindFirst("TouristId");
            int touristId = int.Parse(claim.Value);
            List<ProductViewModel> model = _productRepository.GetCartProducts(touristId);
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ShowProductsBySearch(string Search)
        {
            var model = _productRepository.GetBySearch(Search);
            ViewBag.search = Search;
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ApplyFilter(double? minPrice, double? maxPrice, string? sort, string? category, string? Search)
        {

            List<ProductViewModel> allProducts = new();
            if (Search != null)
            {
                allProducts = _productRepository.GetBySearch(Search);
                ViewBag.Search = Search;
            }
            if (category != null)
            {
                allProducts = _productRepository.GetByCategory(category);
                ViewBag.category = category;
            }
            var filteredList = _productRepository.GetByFilter(allProducts, minPrice, maxPrice, sort);
            return View("ShowProductsByCategory", filteredList);
        }


        [HttpGet]
       
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
  
        public async Task<IActionResult> Balance(TransactionViewModel model, decimal money, string operation)
        {

            if (!ModelState.IsValid) { return View(model); }
            var claim = User.FindFirst("TouristId");
            int touristId = int.Parse(claim.Value);
            var tourist = await _unitOfWork.Tourists.GetByIdAsync(touristId);
            var card = _unitOfWork.GetCC(model.cc);
            if (card == null)
            {
                TempData["Message"] = "The card information is invalid.";
                TempData["AlertType"] = "danger";
                return View(model);
            }
            if (operation == "deposit")
            {
                if (money > card.Balance)
                {
                    //alert 
                    TempData["Message"] = "Insufficient card balance.";
                    TempData["AlertType"] = "warning";
                    return View(model);
                }
                _touristRepository.transaction(card, money, operation, touristId);

            }
            if (operation == "withdraw")
            {
                if (tourist.balance < (double)money)
                {
                    //alert 
                    TempData["Message"] = "Insufficient balance.";
                    TempData["AlertType"] = "warning";
                    return View(model);
                }
                _touristRepository.transaction(card, money, operation, touristId);
            }
            return RedirectToAction("EcommerceHomePage");
        }
        [HttpPost]
   
        public IActionResult ProceedToCheckout(List<ProductViewModel> purchaseData)
        {
            return View(purchaseData);
        }
        [HttpPost]
      
        public async Task<IActionResult> PurchaseProductAsync(List<ProductViewModel> purchaseData, bool paymentType, CreditCard cc, string country, string address)
        {

            string location = country + "/" + address;
            double total = 0;
            foreach(var p in purchaseData)
            {
                total += (p.price * p.count);
            }
            var claim = User.FindFirst("TouristId");
            int touristId = int.Parse(claim.Value);
            if (paymentType == true) //CreditCard 
            {
                var card = _unitOfWork.GetCC(cc);
                if (card == null)
                {
                    TempData["ErrorMessage"] = "Invalid credit card information. Please check your details.";
                    return View("ProceedToCheckout", purchaseData);
                }
                else if (card.Balance < (decimal) total)
                {
                    TempData["ErrorMessage"] = "Insufficient balance in your credit card.";
                    return View("ProceedToCheckout", purchaseData);
                }
                foreach (var p in purchaseData)
                {
                    _touristRepository.BuyProduct(touristId, true, card, p, location);
                }
                return RedirectToAction("EcommerceHomePage");
            }
            else
            {
                var torist = await _unitOfWork.Tourists.GetByIdAsync(touristId);
                if (torist.balance < total)
                {
                    TempData["ErrorMessage"] = "Insufficient balance in your account.";
                    return View("ProceedToCheckout", purchaseData);
                }
                foreach (var p in purchaseData)
                {
                    _touristRepository.BuyProduct(touristId, false, null, p, location);
                }
                return RedirectToAction("EcommerceHomePage");
            }
        }

        [HttpPost]
      
        public IActionResult RemoveFromCart(int productId)
        {
            var claim = User.FindFirst("TouristId");
            int touristId = int.Parse(claim.Value);
            _touristRepository.RemoveFromCart(productId, touristId);
            return Ok();

        }


        public IActionResult ShowOrders()
        {
            var claim = User.FindFirst("TouristId");
            int touristId = int.Parse(claim.Value);
            List<ProductOrdersViewModel> model = _touristRepository.GetOrders(touristId);
            return View(model);
        }

        [HttpPost]
        public IActionResult CancelOrder(int orderId)
        {
            try
            {
                _touristRepository.CancelOrder(orderId);

                return Json(new
                {
                    success = true,
                    message = "Order cancelled successfully."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        public IActionResult SubmitReview (int rate, string type,int serviceId,string comment)
        {
            var claim = User.FindFirst("TouristId");
            int touristId = int.Parse(claim.Value);
            bool completed = _touristRepository.review(touristId, serviceId, rate, comment, type);
            if(completed) return Json(new
            {
                success = true,
                message = "Thanks for your review!"
            });
            else return Json(new
            {
                success = false,
                message = "You must buy first to review."
            });
        }

        //Restaurants
        public IActionResult ShowRestaurants()
        {
            var verifiedRestaurants = _restaurantRepository.GetAll()
            .Where(r => r.verified == true)
            .Select(r => new RestaurantHomePageViewModel
            {
                name = r.name,
                description = r.description,
                Image = r.Image,
                address = r.address,
                hotline = r.hotline,
                verified = r.verified,
                id = r.id
            })
            .ToList();
            var claim = User.FindFirst(ClaimTypes.Name);
            ViewBag.TouristName = claim != null ? claim.Value : "Name";
            var claim2 = User.FindFirst("TouristId");
            int touristId = claim2 != null ? int.Parse(claim2.Value) : 0;
            ViewBag.TouristName = claim != null ? claim.Value : "Name";

            return View("RestaurantHomePage", verifiedRestaurants);
        }


        public IActionResult ViewMenu(int restaurantId)
        {
            var meals = _mealRepository.GetAcceptedMeals(restaurantId)
         .Select(m =>
         {
             var image = _mealRepository.GetPic(m.id);
             return new MealViewModel
             {
                 id = m.id,
                 name = m.name,
                 description = m.description,
                 price = m.price,
                 RestaurantId = m.restaurantId,
                 accepted = m.accepted,
                 ImageBytes = image?.imageData
             };
         })
         .ToList();
            ViewBag.RestaurantId = restaurantId;
            return View("ViewMenu", meals);
        }

        [HttpGet]
        public IActionResult ShowRestaurantssBySearch(string Search)
        {
            var model = _restaurantRepository.GetBySearch(Search);
            ViewBag.search = Search;
            return PartialView("_RestaurantsListPartialView", model);
        }

        public IActionResult ViewTables(int restaurantId)
        {
            var touristClaim = User.FindFirst("TouristId");
            if (touristClaim == null || !int.TryParse(touristClaim.Value, out int touristId))
            {
                return RedirectToAction("Login", "Tourist");
            }

            var tables = _tableRepository.GetNotBookedTables(restaurantId)
         .Select(t =>
         {
             return new TableViewModel
             {
                 id = t.id,
                 number = t.number,
                 numberOfPersons = t.numberOfPersons,
                 bookingPrice = t.bookingPrice,
                 restaurantId = t.restaurantId,
                 booked = t.booked,
             };
         })
         .ToList();
            ViewBag.RestaurantId = restaurantId;
            return View("ViewTables", tables);
        }

        [HttpGet]
        [Authorize(Roles = "Tourist")]
        public async Task<IActionResult> BookTable(int tableId)
        {
            var touristClaim = User.FindFirst("TouristId");
            if (touristClaim == null)
                return RedirectToAction("Login");

            int touristId = int.Parse(touristClaim.Value);

            var table = await _unitOfWork.Tables.GetByIdAsync(tableId);
            if (table == null || table.booked)
            {
                TempData["ErrorMessage"] = "Table not available.";
                return RedirectToAction("ViewTables", new { restaurantId = table.restaurantId });
            }

            var restaurant = await _unitOfWork.Restaurants.GetByIdAsync(table.restaurantId);

            var model = new PurchaseRestaurantViewModel
            {
                touristId = touristId,
                restaurantId = table.restaurantId,
                tableId = tableId,
                tableNumber = table.number,
                numberOfGuests = table.numberOfPersons,
                bookingPrice = table.bookingPrice,
                date = DateTime.Now,
                creditCard = new CreditCard()
            };

            ViewBag.RestaurantName = restaurant.name;
            ViewBag.TableDetails = $"Table #{table.number} - {table.numberOfPersons} persons";

            return View("BookTable", model);
        }

        [HttpPost]
        [Authorize(Roles = "Tourist")]
        public async Task<IActionResult> ProcessBooking(PurchaseRestaurantViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload view with validation errors
                var table = await _unitOfWork.Tables.GetByIdAsync(model.tableId);
                var restaurant = await _unitOfWork.Restaurants.GetByIdAsync(table.restaurantId);
                ViewBag.RestaurantName = restaurant.name;
                ViewBag.TableDetails = $"Table #{table.number} - {table.numberOfPersons} persons";
                return View("BookTable", model);
            }

            try
            {
                // Check if table is still available
                var tableToBook = await _unitOfWork.Tables.GetByIdAsync(model.tableId);
                if (tableToBook == null || tableToBook.booked)
                {
                    TempData["ErrorMessage"] = "Sorry, this table is no longer available.";
                    return RedirectToAction("ViewTables", new { restaurantId = model.restaurantId });
                }

                // Validate credit card
                var card = _unitOfWork.GetCC(model.creditCard);
                if (card == null)
                {
                    TempData["ErrorMessage"] = "Invalid credit card information. Please check your details.";
                    return RedirectToAction("BookTable", new { tableId = model.tableId });
                }

                // Check balance
                if (card.Balance < model.bookingPrice)
                {
                    TempData["ErrorMessage"] = "Insufficient balance in your credit card.";
                    return RedirectToAction("BookTable", new { tableId = model.tableId });
                }

                // Process payment
                card.Balance -= model.bookingPrice;
                _unitOfWork.CreditCards.Update(card);

                // Create booking
                var booking = new TouristRestaurant
                {
                    touristId = model.touristId,
                    restaurantId = model.restaurantId,
                    tableId = model.tableId,
                    tableNumber = model.tableNumber,
                    numberOfGuests = model.numberOfGuests,
                    date = model.date,
                    bookingPrice = model.bookingPrice,
                    CreditCardId = card.CardNumber
                };

                await _unitOfWork.TouristRestaurants.AddAsync(booking);
                Console.WriteLine($"✅ Booking created with CreditCardId: {booking.CreditCardId}");
                Console.WriteLine($"✅ Booking amount: ${booking.bookingPrice}");

                // Mark table as booked
                tableToBook.booked = true;
                _unitOfWork.Tables.Update(tableToBook);

                await _unitOfWork.SaveAsync();

                TempData["SuccessMessage"] = $"Table #{model.tableNumber} booked successfully!";
                return RedirectToAction("BookingHistory");
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error processing booking: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while processing your booking. Please try again.";
                return RedirectToAction("BookTable", new { tableId = model.tableId });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Tourist")]
        public async Task<IActionResult> BookingHistory()
        {
            var touristClaim = User.FindFirst("TouristId");
            if (touristClaim == null)
                return RedirectToAction("Login");

            int touristId = int.Parse(touristClaim.Value);

            // Get bookings
            var bookings = await _unitOfWork.TouristRestaurants.WhereAsync(tr => tr.touristId == touristId);

            // Get all related tables
            var tableIds = bookings.Select(b => b.tableId).Distinct();
            var tables = await _unitOfWork.Tables.WhereAsync(t => tableIds.Contains(t.id));

            // Get all related restaurants
            var restaurantIds = tables.Select(t => t.restaurantId).Distinct();
            var restaurants = await _unitOfWork.Restaurants.WhereAsync(r => restaurantIds.Contains(r.id));

            // Create the view model
            var model = bookings.Select(b =>
            {
                var table = tables.FirstOrDefault(t => t.id == b.tableId);
                var restaurant = restaurants.FirstOrDefault(r => r.id == table?.restaurantId);

                return new TouristBookingHistoryViewModel
                {
                    BookingId = b.bookId,
                    RestaurantName = restaurant?.name ?? "Unknown Restaurant",
                    TableNumber = b.tableNumber,
                    NumberOfGuests = b.numberOfGuests,
                    BookingDate = b.date,
                    BookingPrice = b.bookingPrice,
                    RestaurantAddress = restaurant?.address ?? "Address not available",
                    RestaurantHotline = restaurant?.hotline ?? "Hotline not available"
                };
            }).ToList();

            return View("BookingHistory", model);
        }

        [HttpPost]
        [Authorize(Roles = "Tourist")]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            try
            {
                var touristClaim = User.FindFirst("TouristId");
                if (touristClaim == null)
                    return Json(new { success = false, message = "User not authenticated." });

                int touristId = int.Parse(touristClaim.Value);

                // Get the booking
                var booking = await _unitOfWork.TouristRestaurants.GetByIdAsync(bookingId);
                if (booking == null)
                    return Json(new { success = false, message = "Booking not found." });

                // Verify the booking belongs to the current tourist
                if (booking.touristId != touristId)
                    return Json(new { success = false, message = "You can only cancel your own bookings." });

                DateTime NowDate = DateTime.Now;
                if (booking.date.Date < DateTime.Now.Date)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Cannot cancel past bookings. This booking date has already passed."
                    });
                }

                if (booking.date.Date == NowDate.Date)
                {
                    // Calculate how much time until the booking
                    TimeSpan timeUntilBooking = booking.date - NowDate;

                    // Deny if less than 2 hours before booking time
                    if (timeUntilBooking.TotalHours < 24)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Cannot cancel booking 24 hours before the reserved time."
                        });
                    }
                }

                // Get the table
                var table = await _unitOfWork.Tables.GetByIdAsync(booking.tableId);
                if (table == null)
                    return Json(new { success = false, message = "Table not found." });

                // REFUND LOGIC - Process credit card refund
                bool refundProcessed = false;
                if (!string.IsNullOrEmpty(booking.CreditCardId))
                {
                    // Find the credit card by card number
                    var allCreditCards = await _unitOfWork.CreditCards.GetAllAsync();
                    var creditCard = allCreditCards.FirstOrDefault(c => c.CardNumber == booking.CreditCardId);

                    if (creditCard != null)
                    {
                        // Refund the amount
                        decimal refundAmount = (decimal)booking.bookingPrice;
                        creditCard.Balance += refundAmount;
                        _unitOfWork.CreditCards.Update(creditCard);
                        refundProcessed = true;

                        Console.WriteLine($"✅ Refunded ${refundAmount} to credit card: {creditCard.CardNumber}");
                        Console.WriteLine($"✅ New card balance: ${creditCard.Balance}");
                    }
                    else
                    {
                        Console.WriteLine($"❌ Credit card not found for refund: {booking.CreditCardId}");
                    }
                }
                else
                {
                    Console.WriteLine("❌ No CreditCardId stored for this booking");
                }

                // Mark table as available again
                table.booked = false;
                _unitOfWork.Tables.Update(table);

                // Remove the booking record
                _unitOfWork.TouristRestaurants.Delete(booking);

                await _unitOfWork.SaveAsync();

                // Success message based on refund status
                if (refundProcessed)
                {
                    TempData["SuccessMessage"] = $"Booking cancelled successfully! ${booking.bookingPrice} has been refunded to your credit card.";
                }
                else
                {
                    TempData["SuccessMessage"] = "Booking cancelled successfully! The table is now available again.";
                }

                return Json(new
                {
                    success = true,
                    message = refundProcessed ?
                        $"Booking cancelled! ${booking.bookingPrice} refunded to your card." :
                        "Booking cancelled successfully."
                });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"❌ Error cancelling booking: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = "An error occurred while cancelling the booking." });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Tourist")]
        public async Task<IActionResult> DebugBookings()
        {
            var touristClaim = User.FindFirst("TouristId");
            if (touristClaim == null) return Content("Not authenticated");

            int touristId = int.Parse(touristClaim.Value);
            var bookings = await _unitOfWork.TouristRestaurants.WhereAsync(tr => tr.touristId == touristId);

            var result = new System.Text.StringBuilder();
            result.AppendLine($"Found {bookings.Count()} bookings:");

            foreach (var booking in bookings)
            {
                result.AppendLine($"Booking #{booking.bookId}:");
                result.AppendLine($"  - Table: #{booking.tableNumber}");
                result.AppendLine($"  - Amount: ${booking.bookingPrice}");
                result.AppendLine($"  - CreditCardId: {booking.CreditCardId ?? "NULL"}");
                result.AppendLine($"  - Date: {booking.date}");
                result.AppendLine();
            }

            return Content(result.ToString());
        }


        //------------------------Hotel------------------------------

        //inbox



        //----------------------------
        [HttpGet]
        public async Task<IActionResult> ShowAllHotels(string governorate, string searchString)
        {


            var allHotels = await _hotelRepository.GetAllAsync();

            // فلترة الفنادق المقبولة فقط
            var acceptedHotels = new List<Hotel>();
            foreach (var hotel in allHotels)
            {
                if (hotel.verified == true)
                {
                    acceptedHotels.Add(hotel);
                }
            }

            // تطبيق الفلتر بالمحافظة إذا كانت محددة
            if (!string.IsNullOrEmpty(governorate))
            {
                var tempList = new List<Hotel>();
                foreach (var hotel in acceptedHotels)
                {
                    if (hotel.Governorate == governorate)
                    {
                        tempList.Add(hotel);
                    }
                }
                acceptedHotels = tempList;
            }

            // تطبيق البحث بالاسم إذا كان موجوداً
            if (!string.IsNullOrEmpty(searchString))
            {
                var tempList = new List<Hotel>();
                foreach (var hotel in acceptedHotels)
                {
                    if (hotel.name != null && hotel.name.ToLower().Contains(searchString.ToLower()))
                    {
                        tempList.Add(hotel);
                    }
                }
                acceptedHotels = tempList;
            }

            // حفظ القيم الحالية للعرض في الفيو
            ViewBag.CurrentGovernorate = governorate;
            ViewBag.CurrentSearch = searchString;

            // إنشاء قائمة المحافظات للفيو
            var governorateList = new List<SelectListItem>
    {
        new SelectListItem { Value = "", Text = "All Governorates" },
        new SelectListItem { Value = "Cairo", Text = "Cairo" },
        new SelectListItem { Value = "Giza", Text = "Giza" },
        new SelectListItem { Value = "Alexandria", Text = "Alexandria" },
        new SelectListItem { Value = "Luxor", Text = "Luxor" },
        new SelectListItem { Value = "Aswan", Text = "Aswan" },
        new SelectListItem { Value = "RedSea", Text = "Red Sea" },
        new SelectListItem { Value = "SouthSinai", Text = "South Sinai" },
        new SelectListItem { Value = "NorthSinai", Text = "North Sinai" },
        new SelectListItem { Value = "Fayoum", Text = "Fayoum" },
        new SelectListItem { Value = "Matrouh", Text = "Matrouh" },
        new SelectListItem { Value = "Suez", Text = "Suez" },
        new SelectListItem { Value = "PortSaid", Text = "Port Said" },
        new SelectListItem { Value = "Ismailia", Text = "Ismailia" },
        new SelectListItem { Value = "Qena", Text = "Qena" },
        new SelectListItem { Value = "Sohag", Text = "Sohag" },
        new SelectListItem { Value = "Minya", Text = "Minya" },
        new SelectListItem { Value = "BeniSuef", Text = "Beni Suef" },
        new SelectListItem { Value = "Asyut", Text = "Asyut" },
        new SelectListItem { Value = "Dakahlia", Text = "Dakahlia" },
        new SelectListItem { Value = "Sharqia", Text = "Sharqia" },
        new SelectListItem { Value = "Gharbia", Text = "Gharbia" },
        new SelectListItem { Value = "Beheira", Text = "Beheira" },
        new SelectListItem { Value = "KafrElSheikh", Text = "Kafr El-Sheikh" },
        new SelectListItem { Value = "Damietta", Text = "Damietta" },
        new SelectListItem { Value = "NewValley", Text = "New Valley" },
        new SelectListItem { Value = "SouthValley", Text = "South Valley" },
        new SelectListItem { Value = "Other", Text = "Other" }
    };

            // تحديد المحافظة المختارة حالياً
            foreach (var item in governorateList)
            {
                if (item.Value == governorate)
                {
                    item.Selected = true;
                }
            }

            ViewData["GovernorateList"] = governorateList;

            return View(acceptedHotels);
        }

        [HttpGet]
        public async Task<IActionResult> ShowHotel(int id)
        {
            var hotel = await _unitOfWork.Hotels.GetByIdAsync(id);

            if (hotel == null)
            {
                return NotFound();
            }

            var hotelImages = new List<Tourism.Models.Relations.Image>();

            ViewBag.HotelImages = hotelImages;

            return View("ViewHotelDetail", hotel);
        }


        [HttpGet]
        public async Task<IActionResult> ShowAvailableRooms(int hotelId)
        {
            var hotel = await _unitOfWork.Hotels.GetByIdAsync(hotelId);

            if (hotel == null)
            {
                TempData["ErrorMessage"] = "Hotel not found.";
                return RedirectToAction("ShowAllHotels");
            }

            var viewModel = new BookingProcessViewModel
            {
                HotelId = hotelId,
                HotelName = hotel.name,
                CheckInDate = DateTime.Today.AddDays(1),
                CheckOutDate = DateTime.Today.AddDays(2)
            };

            return View("ShowAvailableRooms", viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> SearchAvailableRooms(BookingProcessViewModel model)
        {
            var hotel = await _unitOfWork.Hotels.GetByIdAsync(model.HotelId);
            model.HotelName = hotel?.name;

            model.CheckInDate = model.CheckInDate.Date;
            model.CheckOutDate = model.CheckOutDate.Date;

            if (!ModelState.IsValid)
            {
                return View("ShowAvailableRooms", model);
            }

            TimeSpan duration = model.CheckOutDate - model.CheckInDate;
            model.TotalNights = (int)duration.TotalDays;

            var allRooms = (await _unitOfWork.Rooms.GetAllAsync()).ToList();
            var suitableRooms = new List<Tourism.Models.Room>();

            foreach (var room in allRooms)
            {
                bool isForThisHotel = room.hotelId == model.HotelId;
                bool isAccepted = room.accepted == true;
                bool isNotDenied = room.isDenied == false;
                bool meetsCapacity = room.numberOfMembers >= model.RequiredCapacity;

                if (isForThisHotel && isAccepted && isNotDenied && meetsCapacity)
                {
                    suitableRooms.Add(room);
                }
            }

            var availableRoomTypes = new List<Tourism.Models.Room>();

            foreach (var roomTypeGroup in suitableRooms.GroupBy(r => new { r.numberOfMembers, r.cost }))
            {
                int totalRoomsOfType = 0;
                var roomIdsOfType = new List<int>();

                foreach (var room in roomTypeGroup)
                {
                    totalRoomsOfType++;
                    roomIdsOfType.Add(room.id);
                }

                var allBookings = (await _unitOfWork.TouristRooms.GetAllAsync()).ToList();
                int roomsBooked = 0;
                var bookedRoomIds = new List<int>();

                foreach (var booking in allBookings)
                {
                    bool isConfirmed = true;
                    bool datesOverlap = booking.startDate < model.CheckOutDate && booking.endDate > model.CheckInDate;

                    bool isOurRoomType = roomIdsOfType.Contains(booking.roomId);

                    if (datesOverlap && isOurRoomType && isConfirmed)
                    {
                        if (!bookedRoomIds.Contains(booking.roomId))
                        {
                            bookedRoomIds.Add(booking.roomId);
                            roomsBooked++;
                        }
                    }
                }

                int roomsActuallyAvailable = totalRoomsOfType - roomsBooked;

                if (roomsActuallyAvailable >= model.RequiredRoomsCount)
                {
                    availableRoomTypes.Add(roomTypeGroup.First());
                }
            }

            if (availableRoomTypes.Any())
            {
                model.AvailableRoomTypes = availableRoomTypes;

                Tourism.Models.Room roomTypeToSelect = null;

                if (model.SelectedRoomTypeId.HasValue)
                {
                    roomTypeToSelect = availableRoomTypes.FirstOrDefault(r => r.id == model.SelectedRoomTypeId.Value);
                }

                if (roomTypeToSelect != null)
                {
                    model.SelectedRoomTypeId = roomTypeToSelect.id;
                    model.TotalPrice = (decimal)(roomTypeToSelect.cost * model.RequiredRoomsCount * model.TotalNights);
                }
                else
                {
                    model.SelectedRoomTypeId = null;
                    model.TotalPrice = null;
                }
            }
            else
            {
                model.ErrorMessage = "Sorry, no rooms are available that match your requirements or the selected time period.";
                model.SelectedRoomTypeId = null;
                model.TotalPrice = null;
            }

            return View("ShowAvailableRooms", model);
        }


        [HttpPost]
        public async Task<IActionResult> ConfirmBookingAndPay(BookingProcessViewModel model)
        {
            var touristIdClaim = User.FindFirst("TouristId")?.Value;
            if (string.IsNullOrEmpty(touristIdClaim) || !int.TryParse(touristIdClaim, out int touristId))
            {
                TempData["ErrorMessage"] = "Authentication error. Please log in again.";
                return RedirectToAction("Login");
            }

            if (model.TotalPrice == null || model.TotalPrice <= 0 || model.SelectedRoomTypeId == null)
            {
                TempData["ErrorMessage"] = "Invalid booking data. Please search again.";
                return RedirectToAction("ShowAvailableRooms", new { hotelId = model.HotelId });
            }

            var tourist = await _unitOfWork.Tourists.GetByIdAsync(touristId);
            if (tourist == null || tourist.balance < (double)model.TotalPrice)
            {
                TempData["ErrorMessage"] = $"Your current balance (${tourist?.balance.ToString("N2")}) is insufficient to cover the booking cost (${model.TotalPrice.Value.ToString("N2")}).";
                return RedirectToAction("ShowAvailableRooms", new { hotelId = model.HotelId });
            }

            var roomTypeToBook = await _unitOfWork.Rooms.GetByIdAsync(model.SelectedRoomTypeId.Value);

            var allRooms = (await _unitOfWork.Rooms.GetAllAsync()).ToList();
            var availableRoomIdsOfType = new List<int>();

            foreach (var room in allRooms)
            {
                if (room.numberOfMembers == roomTypeToBook?.numberOfMembers && room.hotelId == model.HotelId)
                {
                    availableRoomIdsOfType.Add(room.id);
                }
            }

            var allBookings = (await _unitOfWork.TouristRooms.GetAllAsync()).ToList();
            var bookedRoomIds = new List<int>();

            foreach (var booking in allBookings)
            {
                bool datesOverlap = booking.startDate < model.CheckOutDate && booking.endDate > model.CheckInDate;
                bool isConfirmed = true;

                if (datesOverlap && availableRoomIdsOfType.Contains(booking.roomId) && isConfirmed)
                {
                    if (!bookedRoomIds.Contains(booking.roomId))
                    {
                        bookedRoomIds.Add(booking.roomId);
                    }
                }
            }

            var actualRoomIdsToBook = availableRoomIdsOfType
                .Except(bookedRoomIds)
                .Take(model.RequiredRoomsCount)
                .ToList();

            if (actualRoomIdsToBook.Count != model.RequiredRoomsCount)
            {
                TempData["ErrorMessage"] = "Room availability changed during payment processing. Please search again.";
                return RedirectToAction("ShowAvailableRooms", new { hotelId = model.HotelId });
            }

            tourist.balance -= (double)model.TotalPrice.Value;
            _unitOfWork.Tourists.Update(tourist);

            var allCreditCards = (await _unitOfWork.CreditCards.GetAllAsync()).ToList();
            var hotel = await _unitOfWork.Hotels.GetByIdAsync(model.HotelId);

            var hotelCreditCard = allCreditCards.FirstOrDefault(c => c.CardHolder == hotel?.name);

            if (hotelCreditCard != null)
            {
                hotelCreditCard.Balance += model.TotalPrice.Value;
                _unitOfWork.CreditCards.Update(hotelCreditCard);
            }

            List<string> bookingReferences = new List<string>();

            foreach (var roomId in actualRoomIdsToBook)
            {
                var bookingRef = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                bookingReferences.Add(bookingRef);

                var booking = new Tourism.Models.Relations.TouristRoom
                {
                    roomId = roomId,
                    touristId = touristId,
                    startDate = model.CheckInDate,
                    endDate = model.CheckOutDate,
                    numberOfGuests = model.RequiredCapacity,
                };

                await _unitOfWork.TouristRooms.AddAsync(booking);
            }

            await _unitOfWork.SaveAsync();


            var inboxMessage = new InboxMsg
            {
                providerId = touristId,
                providerType = "Tourist",
                content = $"You have successfully booked {model.RequiredRoomsCount} room(s) at {model.HotelName}. Cost ${model.TotalPrice.Value.ToString("N2")}. Booking References: {string.Join(", ", bookingReferences)}. Check-in: {model.CheckInDate.ToString("yyyy-MM-dd")}, Check-out: {model.CheckOutDate.ToString("yyyy-MM-dd")}",
                date = DateTime.Now
            };

            await _unitOfWork.InboxMessages.AddAsync(inboxMessage);
            await _unitOfWork.SaveAsync();

            TempData["SuccessMessage"] = $"Booking confirmed successfully for {model.RequiredRoomsCount} rooms. " +
                                         $"${model.TotalPrice.Value.ToString("N2")} has been debited from your balance. " +
                                         $"Your Booking References: {string.Join(", ", bookingReferences)}. ";

            return RedirectToAction("ShowAllHotels");
        }


        [HttpGet]
        public async Task<IActionResult> MyRoomBookings(int hotelid)
        {
            var hotel = await _unitOfWork.Hotels.GetByIdAsync(hotelid);
            var touristIdClaim = User.FindFirst("TouristId")?.Value;
            if (string.IsNullOrEmpty(touristIdClaim))
            {
                return RedirectToAction("Login");
            }

            int touristId = int.Parse(touristIdClaim);

            // Get room bookings with related data
            var allBookings = await _unitOfWork.TouristRooms.GetAllAsync();
            var touristBookings = allBookings.Where(b => b.touristId == touristId).ToList();

            // Load room and hotel data for each booking
            var allRooms = await _unitOfWork.Rooms.GetAllAsync();
            var allHotels = await _unitOfWork.Hotels.GetAllAsync();
            
            foreach (var booking in touristBookings)
            {
                booking.room = allRooms.FirstOrDefault(r => r.id == booking.roomId);
                if (booking.room != null)
                {
                    booking.room.hotel = allHotels.FirstOrDefault(h => h.id == booking.room.hotelId);
                }
            }

            // Get trip bookings
            var allTripBookings = await _unitOfWork.PaymentTripBookings.GetAllAsync();
            var touristTripBookings = allTripBookings.Where(b => b.UserId == touristId.ToString()).ToList();

            var tripBookingViewModels = new List<Tourism.ViewModel.TripBookingViewModel>();
            foreach (var booking in touristTripBookings)
            {
                var trip = await _unitOfWork.Trips.GetByIdAsync(booking.TripId);
                if (trip != null)
                {
                    tripBookingViewModels.Add(new Tourism.ViewModel.TripBookingViewModel
                    {
                        BookingId = booking.Id,
                        TripId = trip.id,
                        TripName = trip.name,
                        Destination = trip.destination,
                        TotalPrice = booking.TotalPrice,
                        BookingDate = booking.BookingDate,
                        StartDate = trip.StartDate,
                        Duration = trip.Duration,
                        MainImage = trip.MainImage,
                        TourGuideId = trip.tourGuideId
                    });
                }
            }

            var viewModel = new Tourism.ViewModel.MyBookingsViewModel
            {
                RoomBookings = touristBookings,
                TripBookings = tripBookingViewModels
            };

            return View(viewModel);
        }




        [HttpPost]
        public async Task<IActionResult> CancelBookingHotel(int bookId)
        {
            var allBookings = await _unitOfWork.TouristRooms.GetAllAsync();
            var booking = allBookings.FirstOrDefault(b => b.bookId == bookId);

            if (booking == null)
            {
                TempData["Error"] = "Booking not found.";
                return RedirectToAction("MyBookings");
            }

            if (booking.startDate <= DateTime.Today.AddDays(2))
            {
                TempData["Error"] = "Booking cancellation is only allowed 48 hours prior to check-in.";
                return RedirectToAction("MyBookings");
            }

            var allTourists = await _unitOfWork.Tourists.GetAllAsync();
            var tourist = allTourists.FirstOrDefault(t => t.id == booking.touristId);

            var allRooms = await _unitOfWork.Rooms.GetAllAsync();
            var room = allRooms.FirstOrDefault(r => r.id == booking.roomId);

            if (room == null || tourist == null)
            {
                TempData["Error"] = "Associated data is missing.";
                return RedirectToAction("MyBookings");
            }

            TimeSpan duration = booking.endDate - booking.startDate;
            int nights = duration.Days > 0 ? duration.Days : 1;
            double refundAmount = room.cost * nights;

            tourist.balance += refundAmount;
            _unitOfWork.Tourists.Update(tourist);

            room.status = true;
            _unitOfWork.Rooms.Update(room);

            _unitOfWork.TouristRooms.Delete(booking);
            await _unitOfWork.SaveAsync();

            TempData["Success"] = $"Booking cancelled successfully and refund of {refundAmount.ToString("N2")} has been added to your balance.";
            return RedirectToAction("MyBookings");
        }

    }
}




