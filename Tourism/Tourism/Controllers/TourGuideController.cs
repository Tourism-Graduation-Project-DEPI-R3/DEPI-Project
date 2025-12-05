using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Tourism.IRepository;
using Tourism.Models;
using Tourism.Models.Relations;
using Tourism.ViewModels;
using Tourism.ViewModel;

namespace Tourism.Controllers
{
    public class TourGuideController : Controller
    {
        private readonly ITourGuideRepository _tourGuideRepository;
        private readonly ITripRepository _tripRepository;
        private readonly IUnitOfWork _unitOfWork;

        public TourGuideController(ITourGuideRepository tourGuideRepository, ITripRepository tripRepository, IUnitOfWork unitOfWork)
        {
            _tourGuideRepository = tourGuideRepository;
            _tripRepository = tripRepository;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.IsInRole("TourGuide"))
                return RedirectToAction("TourGuideDashboard");
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var tourGuide = await _tourGuideRepository.GetByEmailAsync(model.email);
            if (tourGuide == null)
            {
                model.msg = "Invalid email or password.";
                return View(model);
            }

            var hasher = new PasswordHasher<TourGuide>();
            var result = hasher.VerifyHashedPassword(tourGuide, tourGuide.passwordHash, model.passwordHash);

            if (result == PasswordVerificationResult.Failed)
            {
                model.msg = "Invalid email or password.";
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, tourGuide.TourGuideId.ToString()),
                new Claim(ClaimTypes.Name, $"{tourGuide.firstName} {tourGuide.lastName}"),
                new Claim(ClaimTypes.Email, tourGuide.email),
                new Claim(ClaimTypes.Role, "TourGuide"),
                new Claim("TourGuideId", tourGuide.TourGuideId.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction("TourGuideDashboard");
        }

        [HttpGet]
        public IActionResult TourGuideRegister()
        {
            if (User.IsInRole("TourGuide"))
                return RedirectToAction("TourGuideDashboard");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TourGuideRegister(TourGuide model, IFormFile pic)
        {
            ModelState.Remove(nameof(TourGuide.pic));
            ModelState.Remove(nameof(TourGuide.creditCard));
            
            if (!string.IsNullOrWhiteSpace(model.email) && await _tourGuideRepository.EmailExistsAsync(model.email))
                ModelState.AddModelError(nameof(TourGuide.email), "This email is already registered.");
            if (pic == null || pic.Length == 0) ModelState.AddModelError(nameof(TourGuide.pic), "Profile picture is required.");
            if (string.IsNullOrWhiteSpace(model.languages)) ModelState.AddModelError(nameof(TourGuide.languages), "Please select at least 2 languages.");

            if (!ModelState.IsValid) return View(model);

            using (var memoryStream = new MemoryStream()) { await pic.CopyToAsync(memoryStream); model.pic = memoryStream.ToArray(); }
            var hasher = new PasswordHasher<TourGuide>();
            model.passwordHash = hasher.HashPassword(model, model.passwordHash);
            
            // Don't save credit card during registration
            model.creditCard = null;
            
            await _tourGuideRepository.AddAsync(model);
            await _tourGuideRepository.SaveChangesAsync();

            // Automatically log in the user after registration
            var claims = new List<Claim>
            {
                new Claim("TourGuideId", model.TourGuideId.ToString()),
                new Claim(ClaimTypes.Role, "TourGuide"),
                new Claim(ClaimTypes.Name, model.firstName)
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("TourGuideDashboard");
        }

        [Authorize(Roles = "TourGuide")]
        [HttpGet]
        public IActionResult AddNewTrip() => View(new Trip { TourPlans = new List<TourPlan>() });

        [Authorize(Roles = "TourGuide")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNewTrip(Trip model, IFormFile MainImage, List<IFormFile> Images)
        {
            if (model.TourPlans == null) model.TourPlans = new List<TourPlan>();
            ModelState.Remove("MainImage"); ModelState.Remove("TripSecondaryImagesJson"); ModelState.Remove("verificationDocFile");

            if (MainImage == null || MainImage.Length == 0) ModelState.AddModelError("MainImage", "Main trip image is required.");
            else { using (var memoryStream = new MemoryStream()) { await MainImage.CopyToAsync(memoryStream); model.MainImage = memoryStream.ToArray(); } }

            if (Images == null || Images.Count < 3) ModelState.AddModelError("TripSecondaryImagesJson", "You must upload at least 3 images.");
            else
            {
                var imagesList = new List<byte[]>();
                foreach (var file in Images) { if (file != null && file.Length > 0) { using (var ms = new MemoryStream()) { await file.CopyToAsync(ms); imagesList.Add(ms.ToArray()); } } }
                model.Images = imagesList;
            }

            if (model.Capacity <= 0) ModelState.AddModelError("Capacity", "Capacity must be at least 1.");
            if (model.StartDate == default) ModelState.AddModelError("StartDate", "Start date is required.");
            model.RemainingSeats = model.Capacity;
            if (model.Duration <= 0) model.Duration = 1;

            var guideIdClaim = User.FindFirst("TourGuideId");
            if (guideIdClaim == null) return RedirectToAction("Login", "TourGuide");

            model.tourGuideId = int.Parse(guideIdClaim.Value);
            model.accepted = false;
            model.status = true;
            model.dateAdded = DateTime.Now;

            if (!ModelState.IsValid) return View(model);

            await _tripRepository.AddAsync(model);
            await _tripRepository.SaveChangesAsync();

            // Create service request for admin to review
            ServiceRequest req = new ServiceRequest
            {
                role = "TourGuide",
                serviceId = model.id
            };
            await _unitOfWork.ServiceRequests.AddAsync(req);
            await _unitOfWork.ServiceRequests.SaveAsync();

            TempData["ToastMessage"] = "Trip created successfully and sent for admin approval!";
            TempData["ToastType"] = "success";
            return RedirectToAction("TourGuideDashboard", "TourGuide");
        }

        [Authorize(Roles = "TourGuide")]
        public async Task<IActionResult> TourGuideDashboard()
        {
            var guideIdClaim = User.FindFirst("TourGuideId");
            if (guideIdClaim == null) return RedirectToAction("Login", "TourGuide");
            
            var tourGuide = await _tourGuideRepository.GetByIdAsync(int.Parse(guideIdClaim.Value));
            if (tourGuide == null) return RedirectToAction("Login", "TourGuide");
            
            // Check if tour guide is verified
            if (!tourGuide.verified)
            {
                // Check if they have submitted verification documents
                if (tourGuide.verificationDocuments == null || tourGuide.verificationDocuments.Length == 0)
                {
                    // No documents submitted, redirect to upload page
                    return RedirectToAction("SendVerificationRequest");
                }
                else
                {
                    // Documents submitted but pending review
                    ViewBag.VerificationPending = true;
                }
            }
            
            // Get only accepted trips
            var allTrips = await _tripRepository.GetTripsByTourGuideIdAsync(int.Parse(guideIdClaim.Value));
            var acceptedTrips = allTrips.Where(t => t.accepted == true).ToList();
            var pendingTrips = allTrips.Where(t => t.accepted == false).ToList();
            
            // Set pending trips count for notification
            ViewBag.PendingTripsCount = pendingTrips.Count;
            
            return View(acceptedTrips);
        }

        [HttpGet]
        [Authorize(Roles = "TourGuide")]
        public async Task<IActionResult> TripEdit(int id)
        {
            var trip = await _tripRepository.GetByIdWithDetailsAsync(id);
            return trip == null ? NotFound() : View(trip);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "TourGuide")]
        public async Task<IActionResult> TripEditActual(Trip model, IFormFile MainImage, List<IFormFile> Images)
        {
            ModelState.Remove("MainImage"); ModelState.Remove("Images"); ModelState.Remove("model.MainImage"); ModelState.Remove("model.Images");
            if (!ModelState.IsValid)
            {
                var original = await _tripRepository.GetByIdWithDetailsAsync(model.id);
                if (original == null) return NotFound();
                model.MainImage ??= original.MainImage; model.Images ??= original.Images;
                return View("TripEdit", model);
            }

            var existingTrip = await _tripRepository.GetByIdWithDetailsAsync(model.id);
            if (existingTrip == null) return NotFound();

            existingTrip.name = model.name; existingTrip.destination = model.destination; existingTrip.triptype = model.triptype;
            existingTrip.cost = model.cost; existingTrip.Duration = model.Duration; existingTrip.description = model.description;

            if (MainImage != null && MainImage.Length > 0) { using var ms = new MemoryStream(); await MainImage.CopyToAsync(ms); existingTrip.MainImage = ms.ToArray(); }
            if (Images != null && Images.Count > 0)
            {
                var imagesList = new List<byte[]>();
                foreach (var file in Images) { if (file != null && file.Length > 0) { using var ms = new MemoryStream(); await file.CopyToAsync(ms); imagesList.Add(ms.ToArray()); } }
                if (imagesList.Count > 0) existingTrip.Images = imagesList;
            }
            existingTrip.TourPlans = model.TourPlans ?? new List<TourPlan>();

            await _tripRepository.UpdateAsync(existingTrip);
            await _tripRepository.SaveChangesAsync();
            TempData["ToastMessage"] = "Trip updated successfully!";
            TempData["ToastType"] = "success";
            return RedirectToAction("TourGuideDashboard", "TourGuide");
        }

        [HttpGet]
        [Authorize(Roles = "TourGuide")]
        public async Task<IActionResult> TripDelete(int id)
        {
            var trip = await _tripRepository.GetByIdWithDetailsAsync(id);
            return trip == null ? NotFound() : View(trip);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "TourGuide")]
        public async Task<IActionResult> TripDeleteConfirmed(int id)
        {
            var trip = await _tripRepository.GetByIdWithDetailsAsync(id);
            if (trip == null) return NotFound();
            try
            {
                var carts = await _tripRepository.GetWhereAsync(tc => tc.TripId == trip.id);
                if (carts != null && carts.Any()) await _tripRepository.DeleteRangeAsync(carts);
                await _tripRepository.DeleteAsync(trip);
                await _tripRepository.SaveChangesAsync();
                TempData["ToastMessage"] = "Trip deleted successfully!";
                TempData["ToastType"] = "success";
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "Error deleting trip. " + ex.Message;
                TempData["ToastType"] = "danger";
                return RedirectToAction("TripDelete", new { id });
            }
            return RedirectToAction("TourGuideDashboard", "TourGuide");
        }

        [HttpGet]
        [Authorize(Roles = "TourGuide")]
        public async Task<IActionResult> TripDetails(int id)
        {
            var trip = await _tripRepository.GetByIdWithDetailsAsync(id);
            return trip == null ? NotFound() : View(trip);
        }

        [Authorize(Roles = "TourGuide")]
        public async Task<IActionResult> Orders()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("ChooseLogin", "Home");
            var tourGuide = await _tourGuideRepository.GetByEmailAsync(userEmail);
            if (tourGuide == null) return RedirectToAction("ChooseLogin", "Home");

            // Get all payment bookings
            var allBookings = await _unitOfWork.PaymentTripBookings.GetAllAsync();
            
            // Get all trips, tourists for this tour guide
            var allTrips = await _tripRepository.GetAllAsync();
            var allTourists = await _unitOfWork.Tourists.GetAllAsync();
            
            // Filter trips by tour guide
            var tourGuideTrips = allTrips.Where(t => t.tourGuideId == tourGuide.TourGuideId).ToList();
            
            // Group bookings by trip for this tour guide
            var model = tourGuideTrips
                .Select(trip =>
                {
                    var tripBookings = allBookings.Where(b => b.TripId == trip.id).ToList();
                    
                    if (!tripBookings.Any())
                        return null;
                    
                    return new TripOrdersVM
                    {
                        TripId = trip.id,
                        TripName = trip.name,
                        Destination = trip.destination,
                        Cost = trip.cost,
                        TotalBookingsCount = tripBookings.Sum(b => b.Quantity),
                        Bookings = tripBookings.Select(b =>
                        {
                            var tourist = allTourists.FirstOrDefault(t => t.id.ToString() == b.UserId);
                            return new BookingItemVM
                            {
                                TouristId = int.Parse(b.UserId),
                                TouristName = tourist != null ? $"{tourist.firstName} {tourist.LastName}" : "Unknown",
                                Quantity = b.Quantity,
                                TotalPrice = b.TotalPrice
                            };
                        }).ToList()
                    };
                })
                .Where(x => x != null)
                .ToList();
            
            return View(model);
        }

        public async Task<IActionResult> GetTripImage(int id)
        {
            var trip = await _tripRepository.GetByIdAsync(id);
            if (trip == null || trip.MainImage == null) return NotFound();
            return File(trip.MainImage, "image/jpeg");
        }

        [Authorize]
        public async Task<IActionResult> ViewFullDetailsOrder(int id)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("ChooseLogin", "Home");
            var tourGuide = await _tourGuideRepository.GetByEmailAsync(userEmail);
            if (tourGuide == null) return RedirectToAction("ChooseLogin", "Home");

            var trips = await _tripRepository.GetTripsWithBookingsByGuideIdAsync(tourGuide.TourGuideId);
            var trip = trips.FirstOrDefault(t => t.id == id);
            if (trip == null) return NotFound();

            var model = new TripDetailsVM
            {
                TripId = trip.id,
                TripName = trip.name,
                Destination = trip.destination,
                Description = trip.description,
                Cost = trip.cost,
                Duration = trip.Duration,
                StartDate = trip.StartDate,
                Images = new List<string>(),
                IsFullyBooked = trip.RemainingSeats <= 0,
                TourGuide = new GuideVM { Name = $"{tourGuide.firstName} {tourGuide.lastName}", Phone = tourGuide.phoneNumber, Languages = tourGuide.languages, Rating = 5, AvatarUrl = string.Empty },
                Itinerary = trip.TourPlans?.Select((p, index) => new ItineraryItemVM { DayNumber = index + 1, Title = p.Heading, ActivityDescription = p.Description }).ToList() ?? new List<ItineraryItemVM>(),
                Bookings = trip.TouristCarts?.Select(tc => new BookingDetailVM { TouristName = $"{tc.Tourist.firstName} {tc.Tourist.LastName}", Email = tc.Tourist.email, Phone = tc.Tourist.phoneNumber, Quantity = tc.Quantity, TotalPrice = tc.TotalPrice }).ToList() ?? new List<BookingDetailVM>()
            };
            return View(model);
        }

        [Authorize(Roles = "TourGuide")]
        [HttpGet]
        public async Task<IActionResult> SendVerificationRequest()
        {
            var guideIdClaim = User.FindFirst("TourGuideId");
            if (guideIdClaim == null) return RedirectToAction("Login", "TourGuide");

            var tourGuide = await _tourGuideRepository.GetByIdAsync(int.Parse(guideIdClaim.Value));
            if (tourGuide == null) return RedirectToAction("Login", "TourGuide");

            return View(tourGuide);
        }

        [Authorize(Roles = "TourGuide")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendVerificationRequest(IFormFile verificationDocuments)
        {
            var guideIdClaim = User.FindFirst("TourGuideId");
            if (guideIdClaim == null) return RedirectToAction("Login", "TourGuide");

            var tourGuide = await _tourGuideRepository.GetByIdAsync(int.Parse(guideIdClaim.Value));
            if (tourGuide == null) return RedirectToAction("Login", "TourGuide");

            if (verificationDocuments == null || verificationDocuments.Length == 0)
            {
                TempData["ToastMessage"] = "Please upload a verification document.";
                TempData["ToastType"] = "error";
                return View(tourGuide);
            }

            if (verificationDocuments.Length > 8 * 1024 * 1024)
            {
                TempData["ToastMessage"] = "File size exceeds 8 MB limit.";
                TempData["ToastType"] = "error";
                return View(tourGuide);
            }

            if (verificationDocuments.ContentType != "application/pdf")
            {
                TempData["ToastMessage"] = "Only PDF files are allowed.";
                TempData["ToastType"] = "error";
                return View(tourGuide);
            }

            using (var memoryStream = new MemoryStream())
            {
                await verificationDocuments.CopyToAsync(memoryStream);
                tourGuide.verificationDocuments = memoryStream.ToArray();
            }

            await _tourGuideRepository.UpdateAsync(tourGuide);
            await _tourGuideRepository.SaveChangesAsync();

            // Create verification request for admin to review
            var request = new VerificationRequest
            {
                provider_Id = tourGuide.TourGuideId,
                role = "TourGuide"
            };
            await _unitOfWork.VerificationRequests.AddAsync(request);
            await _unitOfWork.VerificationRequests.SaveAsync();

            TempData["ToastMessage"] = "Verification request submitted successfully!";
            TempData["ToastType"] = "success";
            return RedirectToAction("TourGuideDashboard");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            // Clear session
            HttpContext.Session.Clear();
            
            // Prevent caching
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            
            return RedirectToAction("Index", "Home");
        }
    }
}
