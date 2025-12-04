using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tourism.IRepository;
using Tourism.Models;
using Tourism.ViewModels;

namespace Tourism.Controllers
{
    public class TourGuideController : Controller
    {
        private readonly ITourGuideRepository _tourGuideRepository;
        private readonly ITripRepository _tripRepository;

        public TourGuideController(ITourGuideRepository tourGuideRepository, ITripRepository tripRepository)
        {
            _tourGuideRepository = tourGuideRepository;
            _tripRepository = tripRepository;
        }

        [HttpGet]
        public IActionResult TourGuideRegister() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TourGuideRegister(TourGuide model, IFormFile pic)
        {
            ModelState.Remove(nameof(TourGuide.pic));
            if (!string.IsNullOrWhiteSpace(model.email) && await _tourGuideRepository.EmailExistsAsync(model.email))
                ModelState.AddModelError(nameof(TourGuide.email), "This email is already registered.");
            if (pic == null || pic.Length == 0) ModelState.AddModelError(nameof(TourGuide.pic), "Profile picture is required.");
            if (string.IsNullOrWhiteSpace(model.languages)) ModelState.AddModelError(nameof(TourGuide.languages), "Please select at least 2 languages.");

            if (!ModelState.IsValid) return View(model);

            using (var memoryStream = new MemoryStream()) { await pic.CopyToAsync(memoryStream); model.pic = memoryStream.ToArray(); }
            var hasher = new PasswordHasher<TourGuide>();
            model.passwordHash = hasher.HashPassword(model, model.passwordHash);
            await _tourGuideRepository.AddAsync(model);
            await _tourGuideRepository.SaveChangesAsync();
            return RedirectToAction("Login", "Tourism");
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

            var guideIdClaim = User.FindFirst("GuideId");
            if (guideIdClaim == null) return RedirectToAction("Login", "TourGuide");

            model.tourGuideId = int.Parse(guideIdClaim.Value);
            model.accepted = false;
            model.status = true;

            if (!ModelState.IsValid) return View(model);

            await _tripRepository.AddAsync(model);
            await _tripRepository.SaveChangesAsync();
            TempData["ToastMessage"] = "Trip created successfully!";
            TempData["ToastType"] = "success";
            return RedirectToAction("TourGuideDashboard", "TourGuide");
        }

        [Authorize(Roles = "TourGuide")]
        public async Task<IActionResult> TourGuideDashboard()
        {
            var guideIdClaim = User.FindFirst("GuideId");
            if (guideIdClaim == null) return RedirectToAction("Login", "TourGuide");
            return View(await _tripRepository.GetTripsByTourGuideIdAsync(int.Parse(guideIdClaim.Value)));
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
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Account");
            var tourGuide = await _tourGuideRepository.GetByEmailAsync(userEmail);
            if (tourGuide == null) return RedirectToAction("Login", "Account");

            var trips = await _tripRepository.GetTripsWithBookingsByGuideIdAsync(tourGuide.TourGuideId);
            var model = trips.Where(t => t.TouristCarts.Any()).Select(t => new TripOrdersVM
            {
                TripId = t.id,
                TripName = t.name,
                Destination = t.destination,
                Cost = t.cost,
                TotalBookingsCount = t.TouristCarts.Sum(tc => tc.Quantity),
                Bookings = t.TouristCarts.Select(tc => new BookingItemVM { TouristId = tc.TouristId, TouristName = $"{tc.Tourist.firstName} {tc.Tourist.LastName}", Quantity = tc.Quantity, TotalPrice = tc.TotalPrice }).ToList()
            }).ToList();
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
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Account");
            var tourGuide = await _tourGuideRepository.GetByEmailAsync(userEmail);
            if (tourGuide == null) return RedirectToAction("Login", "Account");

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
    }
}