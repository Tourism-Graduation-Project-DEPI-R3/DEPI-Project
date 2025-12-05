// ========== NAVIGATION FUNCTIONS ==========
function navigateToDestination(page) {
  window.location.href = page;
}

function goBack() {
  window.location.href = "index.html";
}

// ========== FEATURED DESTINATION FUNCTIONS ==========
const destinations = [
  {
    name: "Grand Egyptian Museum",
    category: "Cultural & Historical",
    location: "Giza, Cairo",
    description:
      "The world's largest archaeological museum dedicated to a single civilization, housing over 100,000 artifacts including the complete Tutankhamun collection.",
    image: "Photos/grand_egyptian_museum_Thumbnail.jpg",
    page: "/destinations/Gem",
  },
  {
    name: "Pyramids of Giza",
    category: "Cultural & Historical",
    location: "Giza, Cairo",
    description:
      "The last remaining wonder of the ancient world, these magnificent pyramids have stood for over 4,500 years as a testament to ancient Egyptian engineering.",
    image: "Photos/Pyramids_of_the_Giza_Thumbnail.jpg",
    page: "/destinations/Pyramids",
  },
  {
    name: "The Egyptian Museum",
    category: "Cultural & Historical",
    location: "Cairo",
    description:
      "Home to the world's most extensive collection of pharaonic antiquities, featuring treasures spanning 5,000 years of Egyptian history.",
    image: "Photos/Egyptian-Museum_Thumbnail.jpg",
    page: "/destinations/EgyptianMuseum",
  },
  {
    name: "Khan El Khalili",
    category: "Cultural & Historical",
    location: "Cairo",
    description:
      "A historic bazaar dating back to the 14th century, offering an authentic taste of Cairo's vibrant market culture and traditional crafts.",
    image: "Photos/Khan_Khalili_Thumbnail.jpg",
    page: "/destinations/KhanElKhalili",
  },
  {
    name: "Citadel of Saladin",
    category: "Cultural & Historical",
    location: "Cairo",
    description:
      "A medieval fortress built in the 12th century, featuring the stunning Mosque of Muhammad Ali and offering sweeping views of Cairo.",
    image: "Photos/Citadel_Of_Saladin_Thumbnail.jpg",
    page: "/destinations/CitadelOfSaladin",
  },
  {
    name: "Luxor Temple",
    category: "Cultural & Historical",
    location: "Luxor City, East Bank",
    description:
      "An extraordinary monument built to celebrate kingship, featuring massive statues and intricate carvings that come alive under nighttime illumination.",
    image: "Photos/Luxor_Temple_Thumbnail.webp",
    page: "/destinations/LuxorTemple",
  },
  {
    name: "Karnak Temple",
    category: "Cultural & Historical",
    location: "East Bank, Luxor",
    description:
      "One of the largest religious sites ever constructed, featuring the famous Hypostyle Hall with 134 colossal columns covered in hieroglyphs.",
    image: "Photos/Karnak_Temple_Thumbnail.jpg",
    page: "/destinations/KarnakTemple",
  },
  {
    name: "Valley of the Kings",
    category: "Cultural & Historical",
    location: "West Bank, Luxor",
    description:
      "An ancient royal burial ground with over 60 elaborately decorated tombs, including the famous tomb of Tutankhamun.",
    image: "Photos/Valley_Of_The_Kings_Thumbnail.jpg",
    page: "/destinations/ValleyoftheKings",
  },
  {
    name: "Abu Simbel Temples",
    category: "Cultural & Historical",
    location: "Abu Simbel, near Aswan",
    description:
      "Carved into a mountainside by Ramses II, these temples feature colossal statues and a twice-yearly solar alignment event.",
    image: "Photos/Abu_Simbel_Temple_Thumbnail.jpg",
    page: "/destinations/AbuSimbelTemples",
  },
  {
    name: "Siwa Oasis",
    category: "Remedy & Wellness",
    location: "Western Desert",
    description:
      "Egypt's most peaceful wellness destination, offering natural hot springs, therapeutic sand baths, and magnesium-rich salt lakes.",
    image: "Photos/Siwa-Oasis-Thumbnail.jpg",
    page: "/destinations/SiwaOasis",
  },
  {
    name: "Safaga",
    category: "Remedy & Wellness",
    location: "Red Sea, 60 km south of Hurghada",
    description:
      "Globally known for its mineral-rich black sands, believed to help treat psoriasis and rheumatism.",
    image: "Photos/Safaga_Thumbnail.webp",
    page: "/destinations/Safaga",
  },
  {
    name: "Helwan Sulfur Springs",
    category: "Remedy & Wellness",
    location: "Helwan, 30 km south of Cairo",
    description:
      "Ancient sulfuric mineral springs used since pharaonic times to relieve skin diseases, bone pain, and respiratory issues.",
    image: "Photos/HelwanSulfurSprings_Thumbnail.jpg",
    page: "/destinations/HelwanSulfurSprings",
  },
  {
    name: "Hurghada",
    category: "Water Activities",
    location: "Red Sea Governorate",
    description:
      "One of Egypt's leading diving destinations, featuring long sandy coastlines, coral reefs, and world-class diving schools.",
    image: "Photos/Hurghada-Thumbnail.jpg",
    page: "/destinations/Hurghada",
  },
  {
    name: "Sharm El Sheikh",
    category: "Water Activities",
    location: "South Sinai",
    description:
      "Home to Ras Mohammed National Park, one of the world's top diving locations with deep reefs and vibrant marine life.",
    image: "Photos/Sharm_El_Sheikh_Thumbnail.jpg",
    page: "/destinations/SharmElSheikh",
  },
  {
    name: "El Gouna",
    category: "Water Activities",
    location: "20 km north of Hurghada",
    description:
      "A modern luxury lagoon town perfect for kite surfing, paddleboarding, and safe swimming in calm waters.",
    image: "Photos/El-Gouna-Thumbnail.webp",
    page: "/destinations/ElGouna",
  },
  {
    name: "White Desert",
    category: "Desert & Adventure",
    location: "Farafra, Western Desert",
    description:
      "A surreal landscape filled with chalk rock formations, perfect for camping under some of the clearest night skies in Egypt.",
    image: "Photos/white desert Thumbnail.webp",
    page: "/destinations/WhiteDesert",
  },
  {
    name: "Colored Canyon",
    category: "Desert & Adventure",
    location: "Sinai Peninsula",
    description:
      "A narrow passage featuring sandstone walls streaked with shades of red, orange, gold, and purple.",
    image: "Photos/ColoredCanyonThumbnail.webp",
    page: "/destinations/ColoredCanyon",
  },
  {
    name: "Mount Sinai",
    category: "Desert & Adventure",
    location: "South Sinai",
    description:
      "A sacred site where pilgrims climb before dawn to witness stunning sunrises and visit St. Catherine's Monastery.",
    image: "Photos/Mount_SinaiThumbnail.jpg",
    page: "/destinations/MountSinai",
  },
  {
    name: "Cairo Downtown",
    category: "Urban & Modern",
    location: "Cairo",
    description:
      "Elegant 19th-century buildings, cultural cafés, bookshops, and theatres create an authentic historic city vibe.",
    image: "Photos/CairoDowntownThumbnail.jpg",
    page: "/destinations/CairoDowntown",
  },
  {
    name: "New Administrative Capital",
    category: "Urban & Modern",
    location: "45 km east of Cairo",
    description:
      "A modern smart city featuring the Iconic Tower, Africa's largest cathedral, and cutting-edge technology.",
    image: "Photos/NewAdministrativeCapitalThumbnail.jpg",
    page: "/destinations/NewAdministrativeCapital",
  },
  {
    name: "Alexandria Corniche",
    category: "Urban & Modern",
    location: "Alexandria",
    description:
      "A Mediterranean coastal walkway with beaches, seafood restaurants, and views of ancient and modern Alexandria.",
    image: "Photos/AlexandriaCornicheThumbnail.jpg",
    page: "/destinations/AlexandriaCorniche",
  },
  {
    name: "Fayoum (Tunis Village)",
    category: "Eco & Oasis",
    location: "Fayoum",
    description:
      "A quiet artistic village famous for pottery workshops, lush farmland, and desert-lake scenery.",
    image: "Photos/TunisVillageThumbnail.jpg",
    page: "/destinations/TunisVillage",
  },
  {
    name: "Kharga Oasis",
    category: "Eco & Oasis",
    location: "New Valley",
    description:
      "Known for ancient temples, archaeological remains, and natural farms in Egypt's largest western oasis.",
    image: "Photos/Kharga-OasisThumbnail.webp",
    page: "/destinations/KhargaOasis",
  },
  {
    name: "Dakhla Oasis",
    category: "Eco & Oasis",
    location: "Western Desert",
    description:
      "Home to medieval mud-brick villages, hot springs, and palm groves in a tranquil desert setting.",
    image: "Photos/Dakhla-OasisThumbnail.jpg",
    page: "/destinations/DakhlaOasis",
  },
];

function getDayOfYear() {
  const now = new Date();
  const start = new Date(now.getFullYear(), 0, 0);
  const diff = now - start;
  const oneDay = 1000 * 60 * 60 * 24;
  return Math.floor(diff / oneDay);
}

function getFeaturedDestination() {
  const dayOfYear = getDayOfYear();
  const index = dayOfYear % destinations.length;
  return destinations[index];
}

function displayFeaturedDestination() {
  const featured = getFeaturedDestination();
  const container = document.getElementById("featuredDestination");

  if (container) {
    container.innerHTML = `
      <div class="featured-image-container">
        <img src="${featured.image}" alt="${featured.name}" class="featured-image">
        <div class="featured-overlay">
          <h3 class="featured-destination-title">${featured.name}</h3>
          <span class="featured-category-badge">${featured.category}</span>
        </div>
      </div>
      <div class="featured-content">
        <p class="featured-description">${featured.description}</p>
        <div class="featured-location">
          <span>📍</span>
          <span>${featured.location}</span>
        </div>
        <a href="${featured.page}" class="featured-cta">Explore This Destination →</a>
      </div>
    `;

    container.onclick = function () {
      window.location.href = featured.page;
    };
  }
}

// Initialize featured destination on home page
document.addEventListener("DOMContentLoaded", function () {
  if (document.getElementById("featuredDestination")) {
    displayFeaturedDestination();
  }

  // Create lightbox if on destination page
  if (document.querySelector(".gallery-grid")) {
    createLightbox();
    initializeGallery();
  }
});

// ========== QUICK-NAV ACTIVE HIGHLIGHT ==========
document.addEventListener("DOMContentLoaded", function () {
  const navLinks = document.querySelectorAll(".category-nav-link");
  const sections = [
    document.getElementById("cultural"),
    document.getElementById("wellness"),
    document.getElementById("water"),
    document.getElementById("desert"),
    document.getElementById("urban"),
    document.getElementById("eco"),
  ].filter(Boolean);

  if (!navLinks.length || !sections.length) return;

  const linkMap = {};
  navLinks.forEach((link) => {
    const href = link.getAttribute("href");
    if (href && href.startsWith("#")) linkMap[href.slice(1)] = link;
  });

  const observer = new IntersectionObserver(
    (entries) => {
      entries.forEach((entry) => {
        const id = entry.target.id;
        const link = linkMap[id];
        if (!link) return;
        if (entry.isIntersecting) {
          navLinks.forEach((l) => l.classList.remove("active"));
          link.classList.add("active");
        }
      });
    },
    { root: null, rootMargin: "0px 0px -60% 0px", threshold: 0 }
  );

  sections.forEach((sec) => observer.observe(sec));
});

// ========== LIGHTBOX FUNCTIONS ==========
let lightbox = null;
let lightboxImage = null;

// Initialize lightbox when DOM is loaded
document.addEventListener("DOMContentLoaded", function () {
  // Create lightbox if on destination page
  if (document.querySelector(".gallery-grid")) {
    createLightbox();
    initializeGallery();
  }
});

function createLightbox() {
  // Create lightbox elements
  lightbox = document.createElement("div");
  lightbox.className = "lightbox";
  lightbox.id = "lightbox";

  const lightboxContent = document.createElement("div");
  lightboxContent.className = "lightbox-content";

  lightboxImage = document.createElement("img");
  lightboxImage.className = "lightbox-image";
  lightboxImage.alt = "Enlarged view";

  const closeButton = document.createElement("button");
  closeButton.className = "lightbox-close";
  closeButton.innerHTML = "&times;";
  closeButton.onclick = closeLightbox;

  lightboxContent.appendChild(closeButton);
  lightboxContent.appendChild(lightboxImage);
  lightbox.appendChild(lightboxContent);
  document.body.appendChild(lightbox);

  // Close lightbox when clicking outside the image
  lightbox.addEventListener("click", function (e) {
    if (e.target === lightbox) {
      closeLightbox();
    }
  });

  // Close lightbox with Escape key
  document.addEventListener("keydown", function (e) {
    if (e.key === "Escape" && lightbox.classList.contains("active")) {
      closeLightbox();
    }
  });
}

function initializeGallery() {
  const galleryItems = document.querySelectorAll(".gallery-item img");
  galleryItems.forEach((item) => {
    item.addEventListener("click", function () {
      openLightbox(this.src);
    });
  });
}

function openLightbox(imageSrc) {
  if (lightbox && lightboxImage) {
    lightboxImage.src = imageSrc;
    lightbox.classList.add("active");
    document.body.style.overflow = "hidden"; // Prevent scrolling
  }
}

function closeLightbox() {
  if (lightbox) {
    lightbox.classList.remove("active");
    document.body.style.overflow = ""; // Restore scrolling
  }
}
