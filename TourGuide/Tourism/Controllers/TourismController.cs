using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tourism.IRepository;
using Tourism.Models;
using Tourism.Repository;
using Tourism.ViewModel;

namespace RecreationalTourismWebsite.Controllers
{
    public class TourismController : Controller
    {
        private readonly ITourismRepository _tourismRepository;
        private readonly ITourGuideRepository _tourGuideRepository;

        public TourismController(ITourismRepository tourismRepository, ITourGuideRepository tourGuideRepository)
        {
            _tourismRepository = tourismRepository;
            _tourGuideRepository = tourGuideRepository;
        }

        #region Login and Register

        public IActionResult ExploreRegisterations()
        {
            return View();
        }

        // ===== Tourist Register =====
        public IActionResult TouristRegister()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TouristRegister(Tourist tourist)
        {
            if (!ModelState.IsValid)
                return View(tourist);

            // Check if email already exists
            if (await _tourismRepository.TouristEmailExistsAsync(tourist.email))
            {
                ModelState.AddModelError("", "Email already registered.");
                return View(tourist);
            }

            // Hash password
            var passwordHasher = new PasswordHasher<Tourist>();
            tourist.passwordHash = passwordHasher.HashPassword(tourist, tourist.passwordHash);

            // Save tourist
            await _tourismRepository.AddTouristAsync(tourist);
            await _tourismRepository.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        // ===== Hotel Register =====
        public IActionResult HotelRegister()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> HotelRegister(Hotel hotel)
        {
            if (!ModelState.IsValid)
                return View(hotel);

            // Check if email already exists
            if (await _tourismRepository.HotelEmailExistsAsync(hotel.email))
            {
                ModelState.AddModelError("", "Email already registered.");
                return View(hotel);
            }

            // Hash password
            var passwordHasher = new PasswordHasher<Hotel>();
            hotel.passwordHash = passwordHasher.HashPassword(hotel, hotel.passwordHash);

            // Save hotel
            await _tourismRepository.AddHotelAsync(hotel);
            await _tourismRepository.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        // ===== Login =====
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // ================= TRY TOURIST FIRST =================
            var tourist = await _tourismRepository.GetTouristByEmailAsync(model.email);

            if (tourist != null)
            {
                var touristHasher = new PasswordHasher<Tourist>();
                var touristVerification = touristHasher.VerifyHashedPassword(
                    tourist,
                    tourist.passwordHash,
                    model.passwordHash
                );

                if (touristVerification == PasswordVerificationResult.Success)
                {
                    HttpContext.Session.SetString(
                        "TouristName",
                        tourist.firstName + " " + tourist.LastName
                    );

                    var touristClaims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, tourist.id.ToString()),
        new Claim(ClaimTypes.Name, tourist.email), // ✅ مهم جدًا
        new Claim(ClaimTypes.Email, tourist.email),
        new Claim(ClaimTypes.GivenName, tourist.firstName + " " + tourist.LastName),
        new Claim(ClaimTypes.Role, "Tourist")
    };

                    var touristIdentity = new ClaimsIdentity(
                        touristClaims,
                        CookieAuthenticationDefaults.AuthenticationScheme
                    );

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(touristIdentity)
                    );

                    return RedirectToAction("Index", "Tourism");
                }

            }

            // ================= TRY TOUR GUIDE =================
            var tourGuide = await _tourGuideRepository.GetByEmailAsync(model.email);

            if (tourGuide != null)
            {
                var guideHasher = new PasswordHasher<TourGuide>();

                var guideVerification = guideHasher.VerifyHashedPassword(
                    tourGuide,
                    tourGuide.passwordHash,
                    model.passwordHash   // plain password entered in the login form
                );

                if (guideVerification == PasswordVerificationResult.Success)
                {
                    // Optional: store name in session
                    HttpContext.Session.SetString(
                        "TourGuideName",
                        tourGuide.firstName + " " + tourGuide.lastName
                    );

                    // ✅ Claims built correctly from your model
                    var guideClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, tourGuide.TourGuideId.ToString()),
            new Claim(ClaimTypes.Name, tourGuide.email),
            new Claim(ClaimTypes.Email, tourGuide.email),
            new Claim(ClaimTypes.Role, "TourGuide"),

            // This is used by the Dashboard to filter trips
            new Claim("GuideId", tourGuide.TourGuideId.ToString())
        };

                    var guideIdentity = new ClaimsIdentity(
                        guideClaims,
                        CookieAuthenticationDefaults.AuthenticationScheme
                    );

                    var principal = new ClaimsPrincipal(guideIdentity);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        principal
                    );

                    return RedirectToAction("TourGuideDashboard", "TourGuide");
                }
            }

            // ================= BOTH FAILED =================
            ModelState.AddModelError("", "Invalid email or password");
            return View(model);
        }



        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Tourism");
        }

        #endregion

        public IActionResult Index()
        {
            return View();
        }

        #region About Egypt Section

        public IActionResult AboutEgyptHome()
        {
            return View();
        }

        public IActionResult Geography()
        {
            return View();
        }

        public IActionResult ThisIsEgypt()
        {
            return View();
        }

        public IActionResult History()
        {
            return View();
        }

        public IActionResult LanguageCulture()
        {
            return View();
        }

        #endregion

        #region Destinations Section

        public IActionResult DestinationsHome()
        {
            return View();
        }

        public IActionResult TheNile()
        {
            return View();
        }

        public IActionResult TheRedSea()
        {
            return View();
        }

        public IActionResult TheMedSea()
        {
            return View();
        }

        public IActionResult DesertsOases()
        {
            return View();
        }

        #endregion
    }
}
