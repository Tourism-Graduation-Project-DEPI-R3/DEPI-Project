# Tourism Website - Graduation Project Summary

## Project Overview

This is an **ASP.NET Core MVC** Tourism platform developed as a graduation project for DEPI. The system connects tourists with various tourism-related service providers including hotels, restaurants, and merchants selling tourism products.

---

## Technology Stack

- **Framework**: ASP.NET Core MVC
- **Database**: SQL Server (Entity Framework Core)
- **Authentication**: Cookie-based Authentication
- **Architecture**: Repository Pattern with Unit of Work
- **Language**: C# (.NET)

---

## System Architecture

### Design Patterns Used

1. **Repository Pattern**: Abstraction layer for data access
2. **Unit of Work Pattern**: Coordinates multiple repository operations
3. **Dependency Injection**: Used throughout for loose coupling
4. **MVC Pattern**: Separation of concerns (Models, Views, Controllers)

### Database Connection

- Connection String: Local SQL Server instance
- Database Name: `Tourism`
- Uses Entity Framework Core migrations (14+ migrations applied)

---

## User Roles & Entities

### 1. **Tourist** (Main User)

- Registration with personal details (age, gender, nationality, phone)
- Email-based authentication
- Balance management for purchases
- Shopping cart functionality
- Favorites/wishlist for products
- Can book:
  - Hotel rooms
  - Restaurant tables
  - Purchase products from merchants

### 2. **Merchant** (Product Sellers)

- Sell tourism-related products
- Product management (add, update, delete)
- Credit card integration for payments
- Verification system (requires admin approval)
- Social media links support
- Zip code for location
- Products have:
  - Categories
  - Offers/discounts
  - Stock count
  - Delivery timeframe
  - Approval workflow (Pending → Approved)

### 3. **Hotel**

- Room management system
- Multiple rooms per hotel
- Room properties:
  - Room number
  - Capacity (number of members)
  - Cost
  - Availability status
  - Approval workflow
- Location by governorate
- Credit card for payment processing
- Verification required
- Hotline and contact info

### 4. **Restaurant**

- Table booking system
- Meal/menu management
- Features:
  - Tables with capacity and booking prices
  - Image gallery support
  - Inbox messaging system
  - Hotline (3-5 digits)
  - Credit card integration
  - Verification workflow

### 5. **Admin**

- System administrator
- Approves/verifies:
  - Merchants
  - Hotels
  - Restaurants
  - Products
  - Rooms
- Email and password authentication

### 6. **Tour Guide**

- Separate entity (specific implementation not fully visible)
- Likely manages trips/tours

---

## Core Features

### Authentication & Authorization

- Cookie-based authentication with 7-day expiration
- Role-based access control (Tourist, Merchant, Admin, Hotel, Restaurant)
- Login paths: `/Home/ChooseLogin`
- Access denied redirects to home page

### Shopping & E-Commerce

- **Products**: Tourism souvenirs/items sold by merchants
- **Cart System**: Tourists can add products to cart
- **Favorites**: Save products for later
- **Offers**: Discount system (0-100%)
- **Categories**: Product categorization
- **Stock Management**: Count tracking
- **Delivery Estimates**: DeliversWithin field

### Booking Systems

1. **Hotel Rooms**

   - Room availability checking
   - Capacity-based booking
   - Cost calculation
   - Status management

2. **Restaurant Tables**
   - Table number and capacity
   - Booking price
   - Reserved/available status

### Payment Processing

- **Credit Card Model**: Integrated payment system
- Used by:
  - Merchants (for receiving payments)
  - Hotels (for receiving payments)
  - Restaurants (for receiving payments)
  - Tourists (implied for making payments)

### Verification System

- Service providers must upload verification documents
- Admin approval required for:
  - New merchants
  - New hotels
  - New restaurants
  - Products
  - Rooms
- `verified` boolean flag on entities

### Communication

- **Inbox Messaging**: Restaurants have inbox for customer messages
- Likely extends to other entities

---

## Data Models

### Relationship Types

- **Tourist ↔ Product**: Many-to-Many (via CartProduct, FavouriteProduct)
- **Tourist ↔ Restaurant**: Many-to-Many (via TouristRestaurant)
- **Tourist ↔ Room**: Many-to-Many (via TouristRoom)
- **Tourist ↔ Trip**: Many-to-Many (via TouristTrip)
- **Merchant → Product**: One-to-Many
- **Hotel → Room**: One-to-Many
- **Restaurant → Meal**: One-to-Many
- **Restaurant → Table**: One-to-Many
- **Restaurant → Image**: One-to-Many

### Key Relation Tables

- `CartProduct`: Shopping cart items
- `FavouriteProduct`: Wishlist items
- `TouristProduct`: Purchase history
- `TouristRoom`: Room bookings
- `TouristRestaurant`: Restaurant interactions
- `TouristTrip`: Trip bookings
- `ServiceRequest`: Service requests
- `VerificationRequest`: Verification workflows
- `TouristFeedback`: Review/rating system
- `MerchantSocialMedia`: Social media links

---

## Controllers & Routes

### Main Controllers

1. **HomeController**: Landing pages, privacy, error handling
2. **TouristController**: Tourist dashboard and operations
3. **MerchantController**: Merchant management and product CRUD
4. **HotelController**: Hotel and room management
5. **RestaurantController**: Restaurant, meals, and table management
6. **AdminController**: Admin verification and approval operations
7. **DestinationsController**: Tourism destinations (Egypt locations based on photos)

### Default Route

```
{controller=Home}/{action=Index}/{id?}
```

---

## Repository Layer

### Generic Repository (`IRepository<T>`)

Provides standard CRUD operations for all entities

### Specialized Repositories

- `ITouristRepository` / `TouristRepository`
- `IMerchantRepository` / `MerchantRepository`
- `IAdminRepository` / `AdminRepository`
- `IProductRepository` / `ProductRepository`
- `IRestaurantRepository` / `RestaurantRepository`
- `IHotelRepository` / `HotelRepository`
- `IRoomRepository` / `RoomRepository`
- `ITableRepository` / `TableRepository`
- `IMealRepository` / `MealRepository`
- `ICreditCardRepository` / `CreditCardRepository`
- `ITourGuideRepository` / `TourGuideRepository`

### Unit of Work

Coordinates transactions across multiple repositories:

- Tourists, Merchants, Hotels, Restaurants
- Products, Meals, Rooms, Tables, Trips
- Cart, Favourites, Service/Verification Requests
- Credit Cards, Images, Inbox Messages

---

## Security Features

- Password hashing using `PasswordHasher<T>`
- Email uniqueness enforced via database indexes
- Role-based authorization on controllers
- HTTPS redirection
- HSTS in production

---

## Validation Rules

### Tourist

- Age: Required
- Email: Required, unique, valid format
- Password: 6-100 characters
- Phone: International format validation
- Names: 15 characters max

### Merchant

- Email: Required, unique, max 100 chars
- Password: 6-100 characters
- Phone: Valid phone format
- Zip Code: US format (5 digits or 5+4)

### Hotel

- Email: Required, unique
- Name: Max 20 characters
- Hotline: Required
- Image: Required (byte array)
- Credit card: Required

### Restaurant

- Email: Required, unique
- Password: 6-100 characters
- Hotline: 3-5 digit number
- Description: Max 60 characters
- Credit card: Required

### Product

- Count: Non-negative
- Name: Max 100 characters
- Description: Max 1000 characters
- Price: Must be positive
- Offer: 0-100%

---

## File Structure

```
Tourism/
├── Controllers/         # MVC Controllers (7 controllers)
├── Models/             # Domain entities (14+ models)
│   └── Relations/      # Junction/relation tables
├── Views/              # Razor views
├── Repository/         # Data access implementations
├── IRepository/        # Repository interfaces
├── ViewModel/          # View models for UI
├── Migrations/         # EF Core migrations (14 migrations)
├── Photos/             # Static tourism images
├── wwwroot/            # Static files (CSS, JS, images)
├── Program.cs          # Application entry point
└── appsettings.json    # Configuration
```

---

## Current State

- Database schema established through 14 migrations
- Authentication and authorization configured
- Repository pattern fully implemented
- Basic CRUD operations for all entities
- Verification workflow in place
- Shopping cart and favorites functionality
- Booking systems for hotels and restaurants
- Payment integration via credit cards

---

## Potential Improvements/Next Steps

1. **Payment Gateway Integration**: Actual payment processing (Stripe, PayPal)
2. **Trip Management**: Complete TourGuide and Trip functionality
3. **Reviews & Ratings**: Implement TouristFeedback system
4. **Search & Filtering**: Advanced product/hotel/restaurant search
5. **Email Notifications**: Booking confirmations, verification updates
6. **Image Upload**: Product and restaurant image management
7. **Analytics Dashboard**: For merchants, hotels, restaurants
8. **Mobile Responsiveness**: UI/UX improvements
9. **API Layer**: RESTful API for mobile apps
10. **Multi-language Support**: Tourism sites need internationalization

---

## Database Migrations History

The project has gone through multiple schema iterations (14 migrations total), indicating active development and iterative design improvements.

---

_Last Updated: December 3, 2025_
