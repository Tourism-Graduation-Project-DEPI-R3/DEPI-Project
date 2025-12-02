using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using Tourism.IRepository;
using Tourism.Models;
using Tourism.Models.Relations;
using Tourism.Repository;
using Tourism.ViewModel;
using Microsoft.AspNetCore.Mvc.Filters;



namespace Tourism.Controllers
{
    [Authorize(Roles = "Hotel")]
    public class HotelController : Controller
    {
        private readonly ITouristRepository _touristRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHotelRepository _hotelRepository;
        private readonly IRoomRepository _roomRepository;


        public HotelController(IUnitOfWork unitOfWork, ITouristRepository touristRepository, IHotelRepository hotelRepository, IRoomRepository roomRepository)
        {
            _unitOfWork = unitOfWork;
            _hotelRepository = hotelRepository;
            _touristRepository = touristRepository;
            _roomRepository = roomRepository;
        }


        private async Task SetHotelNameInViewBag()
        {
            var hotelId = GetCurrentHotelId();
            if (hotelId > 0)
            {
                // افترض أن لديك دالة تجلب الفندق بواسطة ID
                var hotel = await _unitOfWork.Hotels.GetByIdAsync(hotelId);
                if (hotel != null)
                {
                    ViewBag.HotelName = hotel.name;
                }
            }
        }

        //---------Log in ----------------

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel hotelFromRequest)
        {
            if (!ModelState.IsValid)
            {
                hotelFromRequest.msg = "";
                return View(hotelFromRequest);
            }

            var hotel = _hotelRepository.GetByEmail(hotelFromRequest.email);

            if (hotel == null)
            {
                hotelFromRequest.msg = "Email or Password is incorrect! Please try again.";
                return View(hotelFromRequest);
            }

            var passwordHasher = new PasswordHasher<Hotel>();
            var result = passwordHasher.VerifyHashedPassword(
                hotel,
                hotel.passwordHash,
                hotelFromRequest.passwordHash
            );

            if (result == PasswordVerificationResult.Success)
            {
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, hotel.name),
                new Claim(ClaimTypes.Role, "Hotel"),
                 new Claim("HotelId", hotel.id.ToString()),
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
                        IsPersistent = true,
                        ExpiresUtc = DateTime.UtcNow.AddDays(7)
                    }
                );
                return RedirectToAction("HotelDashboard");
            }
            else
            {
                hotelFromRequest.msg = "Email or Password is incorrect! Please try again.";
                return View(hotelFromRequest);
            }
        }







        //-----------------Sign Up------------------
        public List<SelectListItem> GetGovernorateSelectList()
        {
            return new List<SelectListItem>

            {
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
        }
        private void PopulateViewModel(HotelSignUpViewModel model)
        {
            model.GovernorateList = GetGovernorateSelectList();
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult SignUp()
        {

            var model = new HotelSignUpViewModel();
           
            PopulateViewModel(model);
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SignUp(HotelSignUpViewModel HotelFromRequest)
        {
            // 1. التحقق من وجود البريد الإلكتروني
            if (_hotelRepository.GetByEmail(HotelFromRequest.Email) != null)
            {
                ModelState.AddModelError("Email", "This Email is already registered.");
                PopulateViewModel(HotelFromRequest);
                return View("SignUp", HotelFromRequest);
            }

            if (ModelState.IsValid)
            {
                // 2. معالجة المحافظة
                string finalGovernorate;
                if (HotelFromRequest.Governorate == "Other")
                {
                    finalGovernorate = HotelFromRequest.OtherGovernorate;
                }
                else
                {

                    finalGovernorate = HotelFromRequest.Governorate;
                }




                var existingCreditCard = await _unitOfWork.GetCCByCardNumberAsync(HotelFromRequest.CreditCard.CardNumber);

                if (existingCreditCard != null)
                {
                    ModelState.AddModelError("CreditCard.CardNumber",
                        $"This card number is already registered. Please use a different card number.");
                    PopulateViewModel(HotelFromRequest);
                    return View("SignUp", HotelFromRequest);
                }
                // 3. إنشاء كيان البطاقة الجديدة
                var newCreditCardModel = new CreditCard
                {
                    CardNumber = HotelFromRequest.CreditCard.CardNumber,
                    CVV = HotelFromRequest.CreditCard.CVV,
                    ExpiryDate = HotelFromRequest.CreditCard.ExpiryDate,
                    CardHolder = HotelFromRequest.CreditCard.CardHolder,
                    Balance = 0.00m
                };

                // 4. البحث عن البطاقة الموجودة باستخدام الرقم فقط ✅
                var finalCreditCard = new CreditCard
                {
                    CardNumber = HotelFromRequest.CreditCard.CardNumber,
                    CVV = HotelFromRequest.CreditCard.CVV,
                    ExpiryDate = HotelFromRequest.CreditCard.ExpiryDate,
                    CardHolder = HotelFromRequest.CreditCard.CardHolder,
                    Balance = 0.00m
                };
                // 5. إنشاء كيان الفندق
                Hotel hotel = new Hotel
                {
                    email = HotelFromRequest.Email,
                    name = HotelFromRequest.Name,
                    description = HotelFromRequest.Description,
                    address = HotelFromRequest.Address,
                    hotline = HotelFromRequest.Hotline,
                    creditCard = finalCreditCard,
                    verified = false,
                    Governorate = finalGovernorate
                };

                // 6. تشفير كلمة المرور
                var hasher = new PasswordHasher<Hotel>();
                hotel.passwordHash = hasher.HashPassword(hotel, HotelFromRequest.Password);

                // 7. حفظ ملف الصورة
                using (var ms = new MemoryStream())
                {
                    await HotelFromRequest.PicFile.CopyToAsync(ms);
                    hotel.pic = ms.ToArray();
                }

                // 8. حفظ وثيقة التحقق (إن وُجدت)
                if (HotelFromRequest.VerificationDocumentFile != null && HotelFromRequest.VerificationDocumentFile.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        await HotelFromRequest.VerificationDocumentFile.CopyToAsync(ms);
                        hotel.verificationDocuments = ms.ToArray();
                    }
                }

                // 9. إضافة الفندق إلى السياق
                await _unitOfWork.Hotels.AddAsync(hotel);

                // 10. حفظ التغييرات
                await _unitOfWork.SaveAsync();

                // 11. إنشاء طلب التحقق
                VerificationRequest verificationRequest = new VerificationRequest
                {
                    provider_Id = hotel.id,
                    role = "Hotel"
                };

                await _unitOfWork.VerificationRequests.AddAsync(verificationRequest);
                await _unitOfWork.SaveAsync();

                return RedirectToAction("Login", "Hotel");
            }

            PopulateViewModel(HotelFromRequest);
            return View("SignUp", HotelFromRequest);
        }




        //-----------LogOut-----------------
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }


        //----------DashBoard Settings--------------------


        public async Task PrepareDashboardAsync(HotelDashboardViewModel dashboardModel, int hotelId)
        {
            var hotel = await _unitOfWork.Hotels.GetByIdAsync(hotelId);

            await SetHotelNameInViewBag();
            dashboardModel.Name = hotel.name;
            dashboardModel.IsVerified = hotel.verified;
            dashboardModel.VerificationStatusText = hotel.verified ? "Accepted" : "Under Review";


            dashboardModel.AnnualRevenue = new List<double>(new double[12]);

            dashboardModel.TotalBookingsLastMonth = new List<int>(new int[4]);
            dashboardModel.MonthlyRevenue = new List<double>(new double[4]);


            for (int month = 1; month <= 12; month++)
            {

                dashboardModel.AnnualRevenue[month - 1] =
                    await _hotelRepository.MonthlyEarningsBookingsAsync(month, hotelId);
            }


            int[] days = { 0, 7, 14, 30 };
            for (int i = 0; i < days.Length; i++)
            {
                int daysBack = days[i];


                dashboardModel.TotalBookingsLastMonth[i] =
                    await _hotelRepository.GetSumAmountBookingsAsync(daysBack, hotelId);


                dashboardModel.MonthlyRevenue[i] =
                    await _hotelRepository.GetSumPriceBookingsAsync(daysBack, hotelId);
            }


            dashboardModel.Messages = _hotelRepository.GetMessages(hotelId);

            dashboardModel.TotalRooms = await _hotelRepository.GetTotalRoomsAsync(hotelId);
        }
        public async Task<IActionResult> HotelDashboardAsync()
        {
            await SetHotelNameInViewBag();
            if (!int.TryParse(User.FindFirst("HotelId")?.Value, out int hotelId))
            {

           
                return RedirectToAction("Login", "Hotel");
            }

            var hotel = await _unitOfWork.Hotels.GetByIdAsync(hotelId);
            string providerType = "Hotel";
            var allRequests = await _unitOfWork.VerificationRequests.GetAllAsync();
            bool pendingRequestExists = false;
            if (allRequests != null)
            {
                
                pendingRequestExists = allRequests
                    .Any(r => r.provider_Id == hotelId && r.role == providerType);
            }

            if (hotel == null)
            {
                
                return RedirectToAction("Login", "Hotel");
            }
            var viewModel = new HotelDashboardViewModel
            {
                HotelId = hotel.id,         
                Name = hotel.name,
                HasPendingRequest = pendingRequestExists
            };
          


            await PrepareDashboardAsync(viewModel, hotelId);

            return View(viewModel);
        }



        [HttpPost]
        public async Task<IActionResult> SubmitVerificationDocuments(int hotelId, IFormFile verificationDocumentFile)
        {
            await SetHotelNameInViewBag();
            var hotel = await _unitOfWork.Hotels.GetByIdAsync(hotelId);

            if (hotel == null)
            {
                return NotFound();
            }

            // التأكد من أن الفندق مش verified ومحتاج يرفع دوكيومنت تاني
            if (hotel.verified)
            {
                TempData["Error"] = "Hotel is already verified.";
                return RedirectToAction("HotelDashboard", "Hotel");
            }

            // التحقق من وجود ملف
            if (verificationDocumentFile == null || verificationDocumentFile.Length == 0)
            {
                TempData["Error"] = "Please select a document to upload.";
                return RedirectToAction("HotelDashboard", "Hotel");
            }

            // حفظ الملف الجديد
            using (var memoryStream = new MemoryStream())
            {
                await verificationDocumentFile.CopyToAsync(memoryStream);
                hotel.verificationDocuments = memoryStream.ToArray();
            }

            // تحديث الفندق
            _unitOfWork.Hotels.Update(hotel);
            await _unitOfWork.Hotels.SaveAsync();

            // إنشاء طلب تحقق جديد
            var newVerificationRequest = new VerificationRequest
            {
                provider_Id = hotelId,
                role = "Hotel"
            };

            await _unitOfWork.VerificationRequests.AddAsync(newVerificationRequest);
            await _unitOfWork.VerificationRequests.SaveAsync();

            TempData["Success"] = "Verification document uploaded successfully. Your request is under review.";
            return RedirectToAction("HotelDashboard", "Hotel");
        }

        [HttpGet]
        [AllowAnonymous] 
        public IActionResult GetHotelImage(int hotelId)
        {
          
            byte[]? imageBytes = _hotelRepository.GetHotelImageBytes(hotelId);

            if (imageBytes == null || imageBytes.Length == 0)
            {
                return NotFound();
            }

           
            return File(imageBytes, "image/jpeg");
        }

        //-------------------When Hotel Get Rejected--------------
        public IActionResult SendVerificationRequest()
        {
            return View(new Hotel());
        }

        [HttpPost]
        public async Task<IActionResult> SendVerificationRequest(IFormFile verificationDocuments)
        {

            if (verificationDocuments == null || verificationDocuments.Length == 0)
            {
                ModelState.AddModelError("", "Please upload a valid PDF file.");
                return View();
            }

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

            int hotelId = int.Parse(User.FindFirst("HotelId")?.Value);

            byte[] pdfBytes;
            using (var ms = new MemoryStream())
            {
                await verificationDocuments.CopyToAsync(ms);
                pdfBytes = ms.ToArray();
            }

            var hotel = await _unitOfWork.Hotels.GetByIdAsync(hotelId);
            if (hotel == null)
            {
                return NotFound("Hotel not found.");
            }

            hotel.verificationDocuments = pdfBytes;
            _hotelRepository.UpdateVerificationDocument(hotel, pdfBytes);

            var request = new VerificationRequest
            {
                provider_Id = hotelId,
                role = "Hotel"
            };

            await _unitOfWork.VerificationRequests.AddAsync(request);
            await _unitOfWork.VerificationRequests.SaveAsync();

            TempData["Success"] = "Verification file uploaded successfully. Your verification status is pending review.";
            return RedirectToAction("HotelDashboard");
        }

     


        //----------------------------------------Rooms--------------------------------------------------

        //---------------------Add Rooms ----------------------
        [HttpGet]
        [AllowAnonymous]

        public async Task<IActionResult> AddRoomAsync()
        {
             await SetHotelNameInViewBag();
            return View(new RoomAddViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRoom(RoomAddViewModel model)
        {

            await SetHotelNameInViewBag();


            if (model.PicFiles == null)
            {
                ModelState.AddModelError("PicFiles", "Please select at least one image.");
                return View(model);
            }
            
            // بعد التأكد أنه ليس null، التحقق من عدد الصور
            if (model.PicFiles.Count() > 3)
            {
                ModelState.AddModelError("PicFiles", "Maximum 3 images are allowed per room.");
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // التحقق من وجود ملفات وأنها ليست فارغة
            if (!model.PicFiles.Any() || model.PicFiles.Any(f => f == null || f.Length == 0))
            {
                ModelState.AddModelError("PicFiles", "Please select valid image files.");
                return View(model);
            }

            if (!int.TryParse(User.FindFirst("HotelId")?.Value, out int hotelId))
            {
                return Unauthorized("Hotel ID not found.");
            }

            // التحقق من تكرار رقم الغرفة بدون LINQ
            bool roomNumberExists = false;
            var allRooms = await _unitOfWork.Rooms.GetAllAsync();

            foreach (var room in allRooms)
            {
                if (room.hotelId == hotelId && room.number == model.RoomNumber)
                {
                    roomNumberExists = true;
                    break;
                }
            }

            if (roomNumberExists)
            {
                ModelState.AddModelError("RoomNumber", $"Room number {model.RoomNumber} already exists in this hotel.");
                return View(model);
            }

            var roomModel = new Room
            {
                hotelId = hotelId,
                number = model.RoomNumber,
                numberOfMembers = model.GuestCapacity,
                cost = (double)model.Price,
                status = true,
                accepted = false,
                dateAdded = DateTime.Now
            };

            await _unitOfWork.Rooms.AddAsync(roomModel);
            await _unitOfWork.SaveAsync();

            foreach (var picFile in model.PicFiles)
            {
                // تحقق إضافي داخل اللوب
                if (picFile == null || picFile.Length == 0)
                    continue;

                byte[] picBytes;
                using (var ms = new MemoryStream())
                {
                    await picFile.CopyToAsync(ms);
                    picBytes = ms.ToArray();
                }

                var imageModel = new Image
                {
                    imageData = picBytes,
                    serviceId = roomModel.id,
                    serviceType = "Room"
                };
                await _unitOfWork.Images.AddAsync(imageModel);
            }

            ServiceRequest reviewRequest = new ServiceRequest
            {
                serviceId = roomModel.id,
                role = "Hotel",
            };
            await _unitOfWork.ServiceRequests.AddAsync(reviewRequest);

            await _unitOfWork.SaveAsync();

            TempData["SuccessMessage"] = $"Room {model.RoomNumber} added successfully and submitted for Admin review.";
            return RedirectToAction("HotelDashboard");
        }






        //-----------------Show Rooms ----------------

        [HttpGet]
        public async Task<IActionResult> ShowRooms()
        {
            await SetHotelNameInViewBag();
            var hotelIdClaim = User.FindFirst("HotelId")?.Value;
            if (string.IsNullOrEmpty(hotelIdClaim) || !int.TryParse(hotelIdClaim, out int hotelId))
            {
                TempData["ErrorMessage"] = "Could not retrieve Hotel ID.";
                return RedirectToAction("Login", "Hotel");
            }
          

            List<Room> rooms = _roomRepository.GetRoomsByHotel(hotelId);
            var allServiceRequests = await _unitOfWork.ServiceRequests.GetAllAsync();
            var pendingRequestsDictionary = new Dictionary<int, bool>();

            foreach (var request in allServiceRequests)
            {
             
                if (request.role == "Hotel")
                {
                    
                    bool isRoomInHotel = false;
                    foreach (var room in rooms)
                    {
                        if (room.id == request.serviceId)
                        {
                            isRoomInHotel = true;
                            break; 
                        }
                    }


                    if (isRoomInHotel)
                    {
                        
                        pendingRequestsDictionary.Add(request.serviceId, true);
                    }
                }
            }
            ViewBag.PendingServiceRequests = pendingRequestsDictionary;

            return View(rooms);
        }

       
       
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetImage(int imageId)
        {

            var image = _unitOfWork.Images.GetByIdAsync(imageId).Result;
            byte[]? imageBytes = image?.imageData;

            if (imageBytes == null || imageBytes.Length == 0)
            {
                return NotFound();
            }

            return File(imageBytes, "image/jpeg");
        }

        //----------------------Delete Rooms ---------------------
        [HttpPost]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            await SetHotelNameInViewBag();
            var hotelIdClaim = User.FindFirst("HotelId")?.Value;
            if (string.IsNullOrEmpty(hotelIdClaim) || !int.TryParse(hotelIdClaim, out int hotelId))
            {
                TempData["ErrorMessage"] = "Unauthorized action: Could not identify hotel.";
                return RedirectToAction("ShowRooms");
            }
            var serviceRequest = _hotelRepository.GetServiceRequest(id);
            if (serviceRequest != null)
            {
                _unitOfWork.ServiceRequests.Delete(serviceRequest);
            }
            var roomToDelete = new Room { id = id, hotelId = hotelId };
            _unitOfWork.Rooms.Delete(roomToDelete);


            await _unitOfWork.SaveAsync();

            TempData["SuccessMessage"] = $"Room (ID: {id}) has been successfully deleted.";
            return RedirectToAction("ShowRooms");
        }


        //---------------- Update && Delete Rooms ----------------
        [HttpGet]
        public async Task<IActionResult> EditRoom(int id)
        {
            await SetHotelNameInViewBag();
            var room = await _unitOfWork.Rooms.GetByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            if (!int.TryParse(User.FindFirst("HotelId")?.Value, out int hotelId) || room.hotelId != hotelId)
            {
                return Unauthorized("You are not authorized to edit this room.");
            }

      
            ViewBag.ExistingImageIds = _hotelRepository.GetRoomImageIds(room.id);

        
            return View(room);
        }


     
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoom(Room room, List<IFormFile>? newPicFiles, List<int>? imagesToDelete)
        {
            await SetHotelNameInViewBag();

            var roomToUpdate = await _unitOfWork.Rooms.GetByIdAsync(room.id);

            if (roomToUpdate == null)
            {
                return NotFound();
            }

            
            roomToUpdate.number = room.number;
            roomToUpdate.numberOfMembers = room.numberOfMembers;
            roomToUpdate.cost = room.cost;
            roomToUpdate.status = room.status;
            roomToUpdate.accepted = false;
            _unitOfWork.Rooms.Update(roomToUpdate);


            roomToUpdate.accepted = false;
            _unitOfWork.Rooms.Update(roomToUpdate);


            if (imagesToDelete != null && imagesToDelete.Any())
            {
                foreach (var imageId in imagesToDelete)
                {
                   
                    var imageToDelete = await _unitOfWork.Images.GetByIdAsync(imageId);
                    if (imageToDelete != null && imageToDelete.serviceId == room.id && imageToDelete.serviceType == "Room")
                    {
                      
                        _unitOfWork.Images.Delete(imageToDelete);
                    }
                }
            }

           
            if (newPicFiles != null)
            {

                foreach (var picFile in newPicFiles.Where(f => f.Length > 0))
                {
                    byte[] picBytes;
                    using (var ms = new MemoryStream())
                    {
                        await picFile.CopyToAsync(ms);
                        picBytes = ms.ToArray();
                    }

                    var imageModel = new Image
                    {
                        imageData = picBytes,
                        serviceId = room.id,
                        serviceType = "Room"
                    };
               
                    await _unitOfWork.Images.AddAsync(imageModel);
                }
            }

            var currentImageIds = _hotelRepository.GetRoomImageIds(room.id);
            int countImagesToDelete = imagesToDelete?.Count ?? 0;
            int countAfterDeletion = currentImageIds.Count - countImagesToDelete;
            int countNewFiles = newPicFiles?.Count(f => f.Length > 0) ?? 0;
            int totalImagesFinal = countAfterDeletion + countNewFiles;

            if (totalImagesFinal > 3)
            {
                ModelState.AddModelError("newPicFiles", $"You can only have a maximum of 3 images in total. Final Total: {totalImagesFinal}.");
                ViewBag.ExistingImageIds = currentImageIds;
                return View(room);
            }
            await _unitOfWork.SaveAsync();


            var existingServiceRequest = _hotelRepository.GetServiceRequest(room.id);
            if (existingServiceRequest == null)
            {
                var reviewRequest = new ServiceRequest
                {
                    serviceId = room.id,
                    role = "Hotel",
                };
                await _unitOfWork.ServiceRequests.AddAsync(reviewRequest);
                await _unitOfWork.SaveAsync();
            }


            TempData["SuccessMessage"] = $"Room {room.number} updated successfully.";
            return RedirectToAction("ShowRooms");
        }

        //-------------------Show Bookings---------------------------


        private int GetCurrentHotelId()
        {

            var hotelIdClaim = User.FindFirst("HotelId")?.Value;
            if (int.TryParse(hotelIdClaim, out int hotelId))
            {
                return hotelId;
            }

            return 0;
        }

        [HttpGet]
        public async Task<IActionResult> GuestBookings()
        {
            await SetHotelNameInViewBag();
            var hotelId = GetCurrentHotelId();

            if (hotelId == 0)
            {
                TempData["Error"] = "Authorization failed. Please log in again.";
                return RedirectToAction("Login");
            }

           
            var allBookings = await _unitOfWork.TouristRooms.GetAllAsync();
            var allRooms = await _unitOfWork.Rooms.GetAllAsync();
            var allTourists = await _unitOfWork.Tourists.GetAllAsync();
            var touristLookup = allTourists.ToDictionary(t => t.id, t => t);
          
            ViewBag.DebugInfo = new
            {
                TotalBookings = allBookings.Count(),
                TotalRooms = allRooms.Count(),
                HotelId = hotelId,
                HotelRooms = allRooms.Count(r => r.hotelId == hotelId),
                RoomsWithBookings = allRooms.Count(r => r.hotelId == hotelId && allBookings.Any(b => b.roomId == r.id))
            };

            
            var activeBookings = allBookings
                .Where(tr => tr.room != null && tr.room.hotelId == hotelId && tr.endDate.Date >= DateTime.Today.Date)
                .ToList();

            return View(activeBookings);
        }




    }
}