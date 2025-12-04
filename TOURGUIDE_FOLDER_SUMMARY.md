# TourGuide Folder - Comprehensive Summary

## Overview

The **TourGuide** folder is a **parallel/enhanced version** of the main Tourism project with significant architectural changes and expanded functionality focused on **Tour Package Booking System**. It appears to be an evolved iteration of the original project with different database technology and enhanced features.

---

## Key Differences from Main Tourism Folder

### 1. **Database Technology**

- **Main Tourism Folder**: SQL Server (Entity Framework Core)
- **TourGuide Folder**: **MySQL** (Entity Framework Core with Pomelo MySQL provider)
  - Connection String: `server=localhost;database=TourismProject_V2;user=root;password=123456;`
  - Uses `UseMySql()` instead of `UseSqlServer()`

### 2. **Session Management**

- Added **Session Support** with 30-minute timeout
- Used for maintaining state across requests (especially for cart functionality)

### 3. **Default Route**

- **Main**: `{controller=Home}/{action=Index}/{id?}`
- **TourGuide**: `{controller=Tourism}/{action=Index}/{id?}` (Tourism controller is the landing page)

### 4. **Admin Seeding**

- Automatically creates default admin on startup if none exists:
  - Email: `admin@tourism.com`
  - Password: `admin123`

---

## New/Enhanced Features

### 🎯 Tour Package Booking System

The main focus of this version is a **complete trip booking platform** where tourists can book multi-day tours.

#### **Trip Model** (Enhanced)

Comprehensive tour package entity with:

**Core Information:**

- Name, description, destination
- Trip type classification
- Duration (in days)
- Cost (decimal precision for pricing)

**Scheduling:**

- `StartDate`: When the trip begins
- `CalculatedEndDate`: Auto-calculated based on duration
- `Capacity`: Maximum number of tourists
- `RemainingSeats`: Real-time seat availability tracking

**Media:**

- `MainImage`: Primary trip photo (byte array)
- `TripSecondaryImagesJson`: Multiple images stored as JSON-serialized byte arrays
- `Images` property: List of images with JSON serialization/deserialization

**Tour Plans:**

- `TourPlans`: Collection of daily itinerary items
- Each day has heading and detailed description

**Management:**

- `status`: Active/inactive trip
- `accepted`: Admin approval workflow
- `tourGuideId`: Foreign key to assigned tour guide

**Booking Tracking:**

- `TouristCarts`: Many-to-many relationship for bookings

---

### 🧑‍✈️ Tour Guide System (Fully Implemented)

#### **TourGuide Model**

Professional tour guide entity with:

**Personal Info:**

- First name, last name, age
- Gender, nationality
- Email (unique), password hash
- Phone number

**Professional Credentials:**

- `experienceYears`: Years of experience (0-60)
- `languages`: Languages spoken (comma-separated or similar)
- `pic`: Profile picture (byte array)

**Business:**

- `trips`: List of trips managed by this guide
- `creditCard`: Payment receiving account
- `CreditCards`: Multiple credit cards support
- `verified`: Admin approval status

**Validation:**

- Age: 18-100
- Password: Minimum 6 characters
- At least one language required
- Profile picture mandatory

#### **TourGuide Repository**

- Email uniqueness checking
- Credit card linking
- Find unlinked cards by holder name
- CRUD operations

---

### 🛒 Shopping Cart for Trips (New Implementation)

#### **TouristCart Model**

Replaces simple product cart with sophisticated trip booking cart:

**Fields:**

- `UserId`: String-based user identifier
- `TouristId`: Tourist foreign key
- `TripId`: Trip being booked
- `Quantity`: Number of seats/tickets
- `UnitPrice`: Price per seat (decimal)
- `TotalPrice`: Calculated total (decimal)

**Features:**

- Real-time seat availability tracking
- Quantity adjustments with validation
- Automatic seat reservation/release
- Persistent cart across sessions

#### **Cart Operations** (TourPackagesController)

Comprehensive cart API endpoints:

1. **GetCartCount**: Returns number of items in cart
2. **GetCart**: Retrieves all cart items with trip details
3. **GetCartItem**: Gets specific trip booking quantity
4. **ChangeCartQuantity**: Add/remove seats with validation
   - Checks remaining seats before adding
   - Releases seats when removing
   - Updates trip availability status
5. **CancelBooking**: Remove trip from cart entirely
6. **ClearCart**: Empty entire cart and release all seats
7. **SaveCart**: Batch update cart items

**Business Logic:**

- Automatic seat management (reserve on add, release on remove)
- Prevents overbooking (validates against `RemainingSeats`)
- Updates trip status to unavailable when fully booked
- Calculates totals automatically

---

### 💳 Payment & Booking System

#### **PaymentTripBooking Model**

Completed booking record:

- `UserId`: Who booked
- `TripId`: Which trip
- `Quantity`: Number of seats
- `BookingDate`: When booked
- `TotalPrice`: Final payment amount
- Navigation to `Trip` entity

#### **Payment Flow**

1. Tourist browses available trips
2. Adds trips to cart (validates seats)
3. Views cart with totals
4. Proceeds to payment
5. Payment processed via credit card
6. Booking confirmed and saved
7. Tour guide receives payment to their credit card

**Payment Methods:**

- Credit card validation
- Transaction processing
- Balance management
- Tour guide commission/payment

---

### 📋 Tour Plan (Itinerary System)

#### **TourPlan Model**

Daily activity breakdown for trips:

- `Id`: Unique identifier
- `Heading`: Day title/summary (max 200 chars)
- `Description`: Detailed activities for the day
- `TripId`: Foreign key to parent trip
- `Trip`: Navigation property

**Usage:**

- Creates day-by-day itinerary
- Helps tourists understand trip schedule
- Displayed in trip details page
- Multiple plans per trip (e.g., 5-day trip has 5 tour plans)

---

## Controllers

### 1. **TourGuideController** (New)

Manages tour guide operations:

**Registration:**

- `TourGuideRegister` (GET/POST): Tour guide signup
  - Validates email uniqueness
  - Requires profile picture upload
  - Validates language requirements
  - Hashes password

**Trip Management:**

- `AddNewTrip` (GET/POST): Create new tour package
  - Uploads main image and 3+ secondary images
  - Sets capacity and remaining seats
  - Creates tour plans (itinerary)
  - Requires admin approval (`accepted = false`)
- `TourGuideDashboard`: View all guide's trips
- `TripEdit` (GET/POST): Modify existing trips
- `TripDelete` (GET/POST): Remove trips
- `Orders`: View all bookings for guide's trips
- `ViewFullDetailsOrder`: Detailed booking information

**Authorization:**

- Role-based: `[Authorize(Roles = "TourGuide")]`

### 2. **TourPackagesController** (New)

Tourist-facing trip booking interface:

**Browse & Details:**

- `TourPackagesHome`: Browse available trips
- `DynamicTrip`: View trip details, itinerary, guide info
- Shows remaining seats, dates, pricing

**Cart Management:**

- RESTful API endpoints for cart operations
- All endpoints return JSON for AJAX integration
- Real-time seat validation

**Payment:**

- `Payment`: Checkout page
- `BookingSuccess`: Confirmation page
- Credit card processing
- Creates `PaymentTripBooking` records

**Authorization:**

- Tourist role required for cart/booking operations

### 3. **TourismController** (Enhanced)

Central authentication and registration hub:

**Landing:**

- `Index`: Homepage
- `ExploreRegisterations`: Choose registration type

**Multi-Role Registration:**

- `TouristRegister`: Tourist signup
- `HotelRegister`: Hotel signup
- Merchant, Restaurant (implied from repositories)

**Unified Login:**

- Single login endpoint for all user types
- Role-based claim assignment
- Redirects based on role
- 7-day authentication cookie

### 4. **TouristController** (Enhanced)

Tourist operations with trip booking:

- Shopping cart integration
- Trip booking history
- Payment processing
- Profile management

### 5. **AdminController** (Enhanced)

Admin verification system:

- Approve tour guides
- Approve trips
- Verify merchants, hotels, restaurants
- Dashboard with pending requests

### 6. **MerchantController** (Existing)

Product sales management (similar to main folder)

### 7. **HomeController** (Basic)

Error handling, static pages

---

## Repository Layer

### **ITripRepository** (New)

Comprehensive trip data access:

**Retrievals:**

- `GetAllTripsAsync()`: All trips
- `GetTripByIdAsync(id)`: Trip with tour plans
- `GetByIdWithDetailsAsync(id)`: Trip with guide and plans
- `GetTripsByTourGuideIdAsync(guideId)`: Guide's trips
- `GetAcceptedActiveTripsAsync()`: Available trips for tourists
- `GetPendingTripsAsync()`: Trips awaiting admin approval
- `GetTripsWithBookingsByGuideIdAsync(guideId)`: Trips with booking details

**CRUD:**

- Add, Update, Delete operations
- Bulk delete for cart items

**Analytics:**

- `GetTripsCountAsync()`: Total trips count

### **ITouristRepository** (Enhanced)

Tourist operations with trip booking:

**Cart Operations:**

- `GetCartItemAsync(touristId, tripId)`
- `GetCartTotalAsync(touristId)`
- `AddCartItemAsync(item)`
- `UpdateCartItemAsync(item)`
- `SaveCartAsync(touristId, items)`
- `GetCartAsync(touristId)`
- `GetCartCountAsync(touristId)`
- `ChangeCartQuantityAsync(touristId, tripId, delta)`
- `ClearCartAsync(touristId)`
- `RemoveCartItemAsync(item)`

**Booking:**

- `AddBookingAsync(booking)`: Save completed booking
- `GetTripByIdAsync(id)`: Trip lookup

**Payment:**

- `GetCreditCardAsync(...)`: Validate credit card
- `UpdateCreditCardAsync(card)`: Update card balance
- `CreditTourGuideAsync(tourGuideId, amount)`: Pay tour guide
- `TransactionAsync(...)`: Generic payment processing

**Analytics:**

- `TotalEearningsTrips()`: Total revenue from trips
- Product and hotel earnings methods

**Transaction Support:**

- `BeginTransactionAsync()`
- `CommitTransactionAsync()`
- `RollbackTransactionAsync()`
- `SupportsTransactions` property

### **ITourGuideRepository** (New)

Tour guide data access:

- `GetByIdAsync(id)`
- `GetByEmailAsync(email)`
- `EmailExistsAsync(email)`
- `AddAsync(guide)`
- `SaveChangesAsync()`
- `GetCreditCardsAsync(tourGuideId)`
- `LinkCreditCardAsync(tourGuideId, cardNumber)`
- `FindFirstUnlinkedCardByHolderAsync(fullName)`

### **ITourismRepository** (New)

Unified authentication repository:

- Tourist email/registration operations
- Hotel email/registration operations
- Multi-role login support
- Email uniqueness validation

---

## View Models

### **TripDetailsVM**

Comprehensive trip display model:

**Trip Info:**

- TripId, TripName, Destination
- Description, Cost
- Duration, StartDate, EndDate (calculated)
- Capacity, RemainingSeats
- IsFullyBooked status
- MainImage + Images list

**Tour Guide Info (GuideVM):**

- Name, Phone, Languages
- Rating (1-5)
- Avatar URL

**Itinerary (ItineraryItemVM):**

- Day number
- Title and activity description

**Bookings (BookingDetailVM):**

- Tourist info (name, email, phone)
- Quantity, total price
- Booking date

### **TripOrdersVM**

Guide's booking summary:

- Trip basic info
- Total bookings count
- List of `BookingItemVM`:
  - Tourist ID and name
  - Quantity booked
  - Total price paid

### **CartItemVM**

Cart item for frontend:

- Trip ID
- Quantity
- Unit price
- Total price

### **PaymentViewModel**

Checkout page data (specific fields not shown but implied)

### **TransactionViewModel**

Payment transaction details

---

## Views

### **TourGuide Views:**

- `TourGuideRegister.cshtml`: Guide signup form
- `AddNewTrip.cshtml`: Create trip with itinerary
- `TourGuideDashboard.cshtml`: Guide's trip list
- `TripEdit.cshtml`: Edit trip details
- `TripDelete.cshtml`: Confirm deletion
- `TripDetails.cshtml`: Full trip information
- `Orders.cshtml`: Booking list
- `ViewFullDetailsOrder.cshtml`: Detailed booking view

### **TourPackages Views:**

- `TourPackagesHome.cshtml`: Browse available trips
- `DynamicTrip.cshtml`: Trip detail page with booking
- `Payment.cshtml`: Checkout page
- `BookingSuccess.cshtml`: Confirmation page

### **Tourism Views:**

- Login, registration views for all roles

---

## Database Migrations

**14 Migrations Total** (v1 through v14):

- Initial migration: Nov 22, 2025
- Latest migration: Dec 1, 2025
- Active development with frequent schema changes
- Migration naming: `v1`, `v2`, `v3`... through `v14`

**Key Schema Elements:**

- TourGuide table
- Trip table with complex fields
- TourPlan table
- TouristCart table
- PaymentTripBooking table
- Existing tables: Tourist, Merchant, Hotel, Restaurant, Product, etc.

---

## Technical Implementation

### **Image Handling**

1. **Single Images**: Stored as `byte[]` (MainImage, profile pics)
2. **Multiple Images**: JSON-serialized list of `byte[]`
   - Serialized to string for database storage
   - Deserialized to `List<byte[]>` via `[NotMapped]` property
   - Uses `System.Text.Json`

### **Seat Management Logic**

```
Add to Cart:
1. Check if RemainingSeats >= requested quantity
2. If yes: Subtract from RemainingSeats, add to cart
3. If no: Return error
4. Update trip.status = (RemainingSeats > 0)

Remove from Cart:
1. Add quantity back to RemainingSeats
2. Ensure RemainingSeats <= Capacity
3. Update trip.status = true (available again)
```

### **Date Calculations**

- `StartDate`: User-provided
- `EndDate`: Calculated property (StartDate + Duration days)
- No separate EndDate field in database

### **Authorization Flow**

1. User logs in via `TourismController.Login`
2. Claims created based on role (Tourist, TourGuide, Admin, etc.)
3. Cookie authentication with 7-day expiration
4. Controllers use `[Authorize(Roles = "...")]`
5. Claims include role-specific ID (e.g., "TouristId", "GuideId")

---

## Workflow Examples

### **Tour Guide Creates Trip:**

1. Guide registers → Admin verifies
2. Guide logs in → Redirected to dashboard
3. Guide clicks "Add New Trip"
4. Fills trip details, uploads images, creates itinerary
5. Submits → Trip saved with `accepted = false`
6. Admin reviews → Sets `accepted = true`
7. Trip appears in tourist browsing

### **Tourist Books Trip:**

1. Tourist browses trips on TourPackagesHome
2. Clicks trip → Views details, itinerary, guide info
3. Selects quantity → Adds to cart
4. Cart validates seats → Reserves seats
5. Tourist proceeds to payment
6. Enters credit card → Payment processed
7. Tourist balance debited
8. Tour guide balance credited
9. Booking saved to PaymentTripBooking
10. Confirmation page shown

---

## Key Business Rules

1. **Seat Availability**: Strictly enforced, prevents overbooking
2. **Trip Status**: Auto-updates when fully booked/available
3. **Admin Approval**: Trips and guides require verification
4. **Multi-Language**: Tour guides must specify languages
5. **Minimum Images**: Trips require 1 main + 3 secondary images
6. **Age Restrictions**: Tour guides must be 18+
7. **Experience Range**: 0-60 years tracked
8. **Password Security**: All passwords hashed using `PasswordHasher<T>`
9. **Email Uniqueness**: Enforced via database index for all roles
10. **Credit Card Required**: Tour guides need payment account

---

## Integration Points

### **With Main Tourism Folder:**

This folder could potentially:

- Share views/layouts
- Use same product/merchant system
- Integrate hotel bookings with trip packages
- Combine restaurant reservations with tours

### **Independent Operation:**

Currently operates as standalone application with:

- Separate database (TourismProject_V2)
- Own authentication system
- Independent migration history
- Different default route

---

## Current Development Status

**Completed Features:**
✅ Tour guide registration and authentication
✅ Trip creation with itinerary
✅ Image upload and storage
✅ Shopping cart for trip bookings
✅ Seat availability tracking
✅ Payment processing
✅ Admin approval workflow
✅ Multi-role login system
✅ MySQL database integration
✅ Session management

**Implied Missing/Future Features:**

- Trip reviews/ratings (TouristFeedback exists but implementation unclear)
- Tour guide rating system (field exists, logic TBD)
- Email notifications for bookings
- Refund system
- Trip cancellation policy
- Dynamic pricing (early bird, group discounts)
- Search and filtering for trips
- Calendar view for trip dates
- Tour guide earnings dashboard
- Tourist booking history

---

## Architecture Comparison

| Feature           | Main Tourism Folder                        | TourGuide Folder                |
| ----------------- | ------------------------------------------ | ------------------------------- |
| **Database**      | SQL Server                                 | MySQL                           |
| **Main Focus**    | E-commerce (products, hotels, restaurants) | Trip booking platform           |
| **Session**       | No                                         | Yes (30 min)                    |
| **Default Admin** | Manual                                     | Auto-seeded                     |
| **Cart Type**     | Product shopping cart                      | Trip booking cart               |
| **Image Storage** | Single images                              | Single + JSON arrays            |
| **Default Route** | Home/Index                                 | Tourism/Index                   |
| **Migrations**    | 14 migrations (init, init2, etc.)          | 14 migrations (v1-v14)          |
| **Trip System**   | Basic Trip model                           | Fully implemented with bookings |
| **TourGuide**     | Model exists, not implemented              | Complete implementation         |

---

## Recommendations for Next Steps

1. **Consolidation Decision**: Determine if you want to:

   - Merge both folders into one unified system
   - Keep separate (e-commerce vs trip booking)
   - Migrate TourGuide features into main Tourism folder

2. **Missing Implementations**:

   - Add trip search and filtering
   - Implement review/rating system
   - Create earnings dashboard for tour guides
   - Add email notifications
   - Build tourist booking history page

3. **Code Quality**:

   - Standardize error handling across controllers
   - Add logging for payment transactions
   - Implement proper transaction rollback on payment failures
   - Add unit tests for cart logic
   - Validate image size limits

4. **UI/UX**:

   - Design responsive trip browsing page
   - Create interactive calendar for trip dates
   - Add image gallery/carousel for trips
   - Implement real-time seat availability updates

5. **Security**:
   - Add rate limiting for cart operations
   - Implement CAPTCHA on registration
   - Add two-factor authentication for payments
   - Encrypt sensitive data at rest
   - Add audit logging for admin actions

---

_Last Updated: December 3, 2025_
_Database: TourismProject_V2 (MySQL)_
_Migration Version: v14_
