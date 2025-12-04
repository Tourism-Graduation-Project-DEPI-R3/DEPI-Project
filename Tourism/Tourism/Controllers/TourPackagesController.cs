using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tourism.IRepository;
using Tourism.Models;
using Tourism.Models.ViewModels;
using Tourism.ViewModel;

namespace Tourism.Controllers
{
    public class TourPackagesController : Controller
    {
        private readonly ITripRepository _tripRepository;
        private readonly ITouristRepository _repo;

        public TourPackagesController(ITripRepository tripRepository, ITouristRepository repo)
        {
            _tripRepository = tripRepository ?? throw new ArgumentNullException(nameof(tripRepository));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        // ================= HELPERS =================

        private int? GetCurrentTouristId()
        {
            var idClaim = User.FindFirst("TouristId")?.Value;
            if (string.IsNullOrEmpty(idClaim))
            {
                idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }

            if (int.TryParse(idClaim, out int touristId))
            {
                return touristId;
            }
            return null;
        }

        private string? GetCurrentUserIdString()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        // ================= DTOs =================
        public class ChangeQuantityDto
        {
            public int TripId { get; set; }
            public int Delta { get; set; }
        }

        public class TripIdDto
        {
            public int TripId { get; set; }
        }

        // ================= CART ENDPOINTS =================

        [Authorize(Roles = "Tourist")]
        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            var touristId = GetCurrentTouristId();
            if (touristId == null) return Unauthorized();

            var count = await _repo.GetCartCountAsync(touristId.Value);
            return Json(new { count });
        }

        [Authorize(Roles = "Tourist")]
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var touristId = GetCurrentTouristId();
            if (touristId == null) return Unauthorized();

            var cartItems = await _repo.GetCartAsync(touristId.Value) ?? new List<TouristCart>();

            var result = cartItems.Select(c => new
            {
                cartItemId = c.Id,
                tripId = c.TripId,
                quantity = c.Quantity,
                unitPrice = c.UnitPrice,
                totalPrice = c.TotalPrice,
                tripName = c.Trip?.name,
                image = c.Trip?.MainImage != null
                             ? "data:image/jpeg;base64," + Convert.ToBase64String(c.Trip.MainImage)
                             : null
            });

            return Json(result);
        }

        [Authorize(Roles = "Tourist")]
        [HttpGet]
        public async Task<IActionResult> GetCartItem(int tripId)
        {
            var touristId = GetCurrentTouristId();
            if (touristId == null) return Unauthorized();

            var item = await _repo.GetCartItemAsync(touristId.Value, tripId);

            if (item == null)
                return Json(new { quantity = 0 });

            return Json(new { quantity = item.Quantity });
        }

        [Authorize(Roles = "Tourist")]
        [HttpPost]
        public async Task<IActionResult> ChangeCartQuantity([FromBody] ChangeQuantityDto dto)
        {
            if (dto == null || dto.TripId <= 0)
                return BadRequest(new { success = false, message = "Invalid request." });

            var touristId = GetCurrentTouristId();
            var userId = GetCurrentUserIdString();

            if (touristId == null) return Unauthorized(new { message = "User not found." });

            try
            {
                var trip = await _tripRepository.GetByIdAsync(dto.TripId);
                if (trip == null) return NotFound(new { success = false, message = "Trip not found." });

                var item = await _repo.GetCartItemAsync(touristId.Value, dto.TripId);

                int currentQty = item?.Quantity ?? 0;
                int newQty = currentQty + dto.Delta;

                if (newQty < 0) newQty = 0;

                int diff = newQty - currentQty;

                if (diff == 0)
                {
                    var total = await _repo.GetCartTotalAsync(touristId.Value);
                    return Ok(new { success = true, quantity = currentQty, cartTotal = total, availableSeats = trip.RemainingSeats });
                }

                if (diff > 0) // adding seats
                {
                    if (trip.RemainingSeats < diff)
                    {
                        return BadRequest(new { success = false, message = $"Not enough seats. Only {trip.RemainingSeats} left." });
                    }
                    trip.RemainingSeats -= diff;
                }
                else // removing seats
                {
                    trip.RemainingSeats += Math.Abs(diff);
                    if (trip.RemainingSeats > trip.Capacity) trip.RemainingSeats = trip.Capacity;
                }

                trip.status = trip.RemainingSeats > 0;

                if (newQty == 0)
                {
                    if (item != null) await _repo.RemoveCartItemAsync(item);
                }
                else
                {
                    if (item == null)
                    {
                        item = new TouristCart
                        {
                            TouristId = touristId.Value,
                            UserId = userId,
                            TripId = dto.TripId,
                            Quantity = newQty,
                            UnitPrice = trip.cost,
                            TotalPrice = trip.cost * newQty
                        };
                        await _repo.AddCartItemAsync(item);
                    }
                    else
                    {
                        item.Quantity = newQty;
                        item.TotalPrice = item.UnitPrice * newQty;
                        await _repo.UpdateCartItemAsync(item);
                    }
                }

                await _tripRepository.SaveChangesAsync();

                var cartTotal = await _repo.GetCartTotalAsync(touristId.Value);

                return Ok(new
                {
                    success = true,
                    quantity = newQty,
                    cartTotal,
                    availableSeats = trip.RemainingSeats,
                    TripId = trip.id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Server error processing cart.", detail = ex.Message });
            }
        }

        [Authorize(Roles = "Tourist")]
        [HttpPost]
        public async Task<IActionResult> CancelBooking([FromBody] TripIdDto dto)
        {
            var touristId = GetCurrentTouristId();
            if (touristId == null) return Unauthorized();

            if (dto == null || dto.TripId <= 0) return BadRequest(new { success = false, message = "Invalid Trip ID" });

            var item = await _repo.GetCartItemAsync(touristId.Value, dto.TripId);
            if (item == null) return NotFound(new { success = false, message = "Booking not found in cart." });

            var trip = await _tripRepository.GetByIdAsync(dto.TripId);
            if (trip != null)
            {
                trip.RemainingSeats += item.Quantity;
                if (trip.RemainingSeats > trip.Capacity) trip.RemainingSeats = trip.Capacity;
                trip.status = trip.RemainingSeats > 0;
            }

            await _repo.RemoveCartItemAsync(item);
            await _tripRepository.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                TripId = trip?.id,
                RemainingSeats = trip?.RemainingSeats
            });
        }

        [Authorize(Roles = "Tourist")]
        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            var touristId = GetCurrentTouristId();
            if (touristId == null) return Unauthorized();

            var items = await _repo.GetCartAsync(touristId.Value) ?? new List<TouristCart>();

            foreach (var item in items)
            {
                var trip = await _tripRepository.GetByIdAsync(item.TripId);
                if (trip != null)
                {
                    trip.RemainingSeats += item.Quantity;
                    if (trip.RemainingSeats > trip.Capacity) trip.RemainingSeats = trip.Capacity;
                    trip.status = trip.RemainingSeats > 0;
                }
            }

            await _repo.ClearCartAsync(touristId.Value);
            await _tripRepository.SaveChangesAsync();

            return Ok(new { success = true });
        }

        [Authorize(Roles = "Tourist")]
        [HttpPost]
        public async Task<IActionResult> SaveCart([FromBody] List<CartItemVM> items)
        {
            if (items == null || !items.Any()) return BadRequest(new { success = false, message = "Cart is empty" });

            var touristId = GetCurrentTouristId();
            var userId = GetCurrentUserIdString();

            if (touristId == null) return Unauthorized();

            try
            {
                foreach (var itemVM in items)
                {
                    var trip = await _tripRepository.GetByIdAsync(itemVM.TripId);
                    if (trip == null) continue;

                    var existingItem = await _repo.GetCartItemAsync(touristId.Value, itemVM.TripId);
                    int oldQty = existingItem?.Quantity ?? 0;
                    int diff = itemVM.Quantity - oldQty;

                    if (diff == 0) continue;

                    if (diff > 0 && trip.RemainingSeats < diff)
                    {
                        return BadRequest(new { success = false, message = $"Not enough seats for {trip.name}." });
                    }

                    if (diff > 0) trip.RemainingSeats -= diff;
                    else trip.RemainingSeats += Math.Abs(diff);

                    if (trip.RemainingSeats > trip.Capacity) trip.RemainingSeats = trip.Capacity;
                    trip.status = trip.RemainingSeats > 0;

                    if (existingItem == null)
                    {
                        var newItem = new TouristCart
                        {
                            TouristId = touristId.Value,
                            UserId = userId,
                            TripId = itemVM.TripId,
                            Quantity = itemVM.Quantity,
                            UnitPrice = itemVM.UnitPrice,
                            TotalPrice = itemVM.UnitPrice * itemVM.Quantity
                        };
                        await _repo.AddCartItemAsync(newItem);
                    }
                    else
                    {
                        existingItem.Quantity = itemVM.Quantity;
                        existingItem.TotalPrice = itemVM.UnitPrice * itemVM.Quantity;
                        await _repo.UpdateCartItemAsync(existingItem);
                    }
                }

                await _tripRepository.SaveChangesAsync();

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Server error saving cart.", detail = ex.Message });
            }
        }

        // ================= VIEWS =================

        public async Task<IActionResult> TourPackagesHome()
        {
            var trips = await _tripRepository.GetAcceptedActiveTripsAsync() ?? new List<Trip>();
            return View(trips);
        }

        public async Task<IActionResult> TripDetails(int id)
        {
            var trip = await _tripRepository.GetTripByIdAsync(id);
            if (trip == null) return NotFound();
            return View("DynamicTrip", trip);
        }

        [Authorize(Roles = "Tourist")]
        public async Task<IActionResult> DynamicTrip(int id)
        {
            var trip = await _tripRepository.GetTripByIdAsync(id);
            if (trip == null) return NotFound();
            return View(trip);
        }

        // ================= PAYMENT =================

        [HttpGet]
        [Authorize(Roles = "Tourist")]
        public async Task<IActionResult> Payment()
        {
            var userId = GetCurrentUserIdString();
            if (string.IsNullOrEmpty(userId)) { TempData["Error"] = "Unable to identify user."; return RedirectToAction("TourPackagesHome"); }

            var cartItems = await _repo.GetCartItemsWithTripAsync(userId);
            if (cartItems == null || !cartItems.Any()) { TempData["Error"] = "Your cart is empty."; return RedirectToAction("TourPackagesHome"); }

            var vmItems = cartItems.Select(c => new CartItemViewModel
            {
                TripId = c.TripId,
                TripName = c.Trip?.name ?? "Unknown",
                UnitPrice = c.Trip?.cost ?? 0m,
                Quantity = c.Quantity
            }).ToList();

            var model = new CheckoutViewModel
            {
                CartItems = vmItems,
                TotalAmount = vmItems.Sum(x => x.TotalPrice)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Tourist")]
        public async Task<IActionResult> Payment(CheckoutViewModel model)
        {
            var userIdString = GetCurrentUserIdString();
            if (string.IsNullOrEmpty(userIdString))
            {
                ModelState.AddModelError("", "Unable to identify user.");
                return View(model);
            }

            int? currentUserId = null;
            if (int.TryParse(userIdString, out var parsedId)) currentUserId = parsedId;

            var dbCartItems = await _repo.GetCartItemsWithTripAsync(userIdString);
            if (dbCartItems == null || !dbCartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("TourPackagesHome");
            }

            model.CartItems = dbCartItems.Select(c => new CartItemViewModel
            {
                TripId = c.TripId,
                TripName = c.Trip?.name ?? "Unknown",
                UnitPrice = c.Trip?.cost ?? 0m,
                Quantity = c.Quantity
            }).ToList();

            model.TotalAmount = dbCartItems.Sum(c => c.Quantity * (c.Trip?.cost ?? 0m));

            if (!ModelState.IsValid) return View(model);

            var cardNumber = (model.CardNumber ?? "").Trim();
            var cvv = (model.CVV ?? "").Trim();
            var cardHolder = (model.CardHolder ?? "").Trim();
            var expiry = (model.ExpiryDate ?? "").Trim();

            if (string.IsNullOrWhiteSpace(cardNumber) || string.IsNullOrWhiteSpace(cvv) ||
                string.IsNullOrWhiteSpace(cardHolder) || string.IsNullOrWhiteSpace(expiry))
            {
                ModelState.AddModelError("", "Please provide complete card information.");
                return View(model);
            }

            var creditCard = await _repo.GetCreditCardAsync(cardNumber, cvv, cardHolder, expiry);
            if (creditCard == null)
            {
                ModelState.AddModelError("", "Invalid card information.");
                return View(model);
            }

            bool cardAllowed = false;
            if (creditCard.UserId.HasValue && currentUserId.HasValue && creditCard.UserId.Value == currentUserId.Value)
            {
                cardAllowed = true;
            }

            if (!cardAllowed)
            {
                ModelState.AddModelError("", "This card does not belong to the current user.");
                return View(model);
            }

            if (creditCard.Balance < model.TotalAmount)
            {
                ModelState.AddModelError("", "Insufficient balance.");
                return View(model);
            }

            var transactionSupported = _repo.SupportsTransactions;
            if (transactionSupported) await _repo.BeginTransactionAsync();

            try
            {
                creditCard.Balance -= model.TotalAmount;
                await _repo.UpdateCreditCardAsync(creditCard);

                foreach (var cartItem in dbCartItems)
                {
                    var trip = cartItem.Trip;
                    decimal itemTotal = cartItem.Quantity * (trip?.cost ?? 0m);

                    var booking = new PaymentTripBooking
                    {
                        UserId = userIdString,
                        TripId = cartItem.TripId,
                        Quantity = cartItem.Quantity,
                        BookingDate = DateTime.UtcNow,
                        TotalPrice = itemTotal
                    };
                    await _repo.AddBookingAsync(booking);

                    if (trip != null)
                    {
                        await _repo.CreditTourGuideAsync(trip.tourGuideId, itemTotal);
                    }
                }

                await _repo.RemoveCartItemsAsync(dbCartItems);

                await _tripRepository.SaveChangesAsync();
                await _repo.SaveChangesAsync();

                if (transactionSupported) await _repo.CommitTransactionAsync();

                return RedirectToAction("BookingSuccess");
            }
            catch (Exception ex)
            {
                if (transactionSupported) try { await _repo.RollbackTransactionAsync(); } catch { }
                ModelState.AddModelError("", "An error occurred while processing payment. Please try again later.");
                return View(model);
            }
        }

        public IActionResult BookingSuccess()
        {
            return View();
        }
    }
}
