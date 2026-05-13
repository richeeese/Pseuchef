# Pseuchef 🍽️

A modern Windows Forms application for smart pantry management and recipe discovery. Pseuchef helps users track their food inventory, discover recipes based on available ingredients, get expiry alerts, and chat with an AI kitchen assistant.

---

## Overview

Pseuchef is a multi-tier .NET 8 WinForms application designed to streamline meal planning and ingredient management. The application features an intuitive neobrutalist UI for browsing recipes, managing pantry items, discovering meal ideas tailored to dietary preferences, and an AI-powered Chefbot assistant.

---

## Key Features

- **Dashboard** — Quick overview of pantry status with expiry summaries and a donut chart visualization
- **Pantry Management** — Add, edit, and delete ingredients with expiry date tracking and status badges
- **Recipe Discovery** — Browse and filter recipes based on pantry contents, dietary restrictions, and tags
- **Smart Matching** — Ingredient match percentage shown per recipe based on current pantry
- **Surprise Pick** — Random recipe recommendation from the current filtered pool
- **Chefbot AI** — Local AI chatbot powered by Google Gemma 3 via Ollama for kitchen assistance
- **User Profiles** — Support for dietary preferences and excluded ingredients
- **Local Persistence** — Pantry inventory saved to a local JSON file across sessions

---

## Project Structure

```
Pseuchef/
├── Frontend/                        # WinForms UI (Pseuchef.csproj)
│   ├── UI/
│   │   ├── Form1.cs                 # Main application window
│   │   ├── Form1.Designer.cs        # Auto-generated UI layout
│   │   ├── AddItemForm.cs           # Add/edit pantry item form
│   │   └── RecipeDetailForm.cs      # Recipe detail popup
│   ├── Styles/
│   │   └── AppColors.cs             # Shared color palette
│   ├── Resources/                   # Icons and images
│   └── Properties/                  # Designer resources
│
├── Backend/                         # Business logic & models (Pseuchef.Backend.csproj)
│   ├── Data/
│   │   └── InventoryStorage.cs      # JSON persistence for pantry items
│   ├── Services/
│   │   ├── Strategies/              # Strategy pattern implementations
│   │   │   ├── ExpiringItemFilterStrategy.cs
│   │   │   ├── CategoryFilterStrategy.cs
│   │   │   ├── SortByExpiryStrategy.cs
│   │   │   └── CalorieCalculationStrategy.cs
│   │   ├── VirtualFridge.cs         # Pantry inventory management
│   │   ├── RecipeFetcher.cs         # Spoonacular API integration
│   │   └── ShoppingListManager.cs   # Shopping list logic
│   ├── Models/
│   │   ├── FoodItem.cs              # Base food item model
│   │   ├── PerishableItem.cs        # Subclass with expiry date logic
│   │   ├── NutrientProfile.cs       # Nutritional data composition
│   │   ├── NutrientValue.cs         # Individual nutrient values
│   │   ├── Recipe.cs                # Recipe model
│   │   ├── RecipeIngredient.cs      # Recipe ingredient model
│   │   └── UserProfile.cs           # User preferences and dietary restrictions
│   ├── Interfaces/
│   │   ├── IRecipeService.cs        # Recipe service contract
│   │   ├── IFoodItemFilterStrategy.cs
│   │   ├── IInventorySortStrategy.cs
│   │   ├── ICalculationStrategy.cs
│   │   ├── IDietaryRestrictionStrategy.cs
│   │   └── IRecipeFilterStrategy.cs
│   └── Enums/
│       ├── FoodCategory.cs          # Food classification
│       ├── DietaryRestriction.cs    # Dietary constraint types
│       ├── NutrientType.cs          # Nutrient keys
│       └── StorageType.cs           # Storage location types
│
├── AI/                              # AI services (Pseuchef.AI.csproj)
│   └── ChefbotService.cs            # Gemma 3 chatbot via Ollama
│
└── README.md
```

---

## Technology Stack

| Component | Technology |
|---|---|
| Framework | .NET 8 (`net8.0-windows`) |
| Language | C# 12 |
| UI Framework | Windows Forms with Guna.UI2 |
| JSON Serialization | `System.Text.Json` |
| Recipe API | Spoonacular |
| AI Chatbot | Google Gemma 3 via Ollama (local) |
| IDE | Visual Studio Community 2022+ |

---

## NuGet Dependencies

| Package | Version | Purpose |
|---|---|---|
| Guna.UI2.WinForms | 2.0.4.7 | Modern UI components |
| Newtonsoft.Json | 13.0.4 | JSON serialization |

---

## Getting Started

### Prerequisites

- Visual Studio 2022 or later (Community, Professional, or Enterprise)
- .NET 8 SDK
- Windows 10/11
- Spoonacular API key (free at https://spoonacular.com/food-api)
- Ollama (optional, for Chefbot AI — https://ollama.com/download)

### Installation

**1. Clone the repository**
```bash
git clone https://github.com/richeeese/Pseuchef.git
cd Pseuchef
```

**2. Open the solution**
```bash
start Pseuchef.sln
```
Or open directly in Visual Studio.

**3. Set your Spoonacular API key**

Add your key to `Backend/Config.cs` (this file is gitignored — do not commit it):
```csharp
public static class Config
{
    public const string SpoonacularApiKey = "your-api-key-here";
}
```

**4. Restore NuGet packages**
```bash
dotnet restore
```

**5. Build and run**
```bash
dotnet build
```
Or press **F5** in Visual Studio.

### Optional — Chefbot AI Setup

Install Ollama from https://ollama.com/download, then pull the model:
```bash
ollama pull gemma3:4b
```
Ollama runs automatically in the background. Chefbot will show a green status indicator when online.

---

## Architecture

### Three-Tier Architecture

```
┌─────────────────────────────────────┐
│         Frontend (Pseuchef)         │  WinForms UI, Guna.UI2 components
│  Form1 · AddItemForm · RecipeDetail │  Event-driven, decoupled via interfaces
└────────────────┬────────────────────┘
                 │ calls via interfaces
┌────────────────▼────────────────────┐
│      Backend (Pseuchef.Backend)     │  Business logic, models, strategies
│  VirtualFridge · RecipeFetcher      │  Strategy + Factory patterns
│  UserProfile · InventoryStorage     │  JSON persistence
└────────────────┬────────────────────┘
                 │
┌────────────────▼────────────────────┐
│         AI (Pseuchef.AI)            │  Local AI via Ollama
│         ChefbotService              │  Gemma 3 4b, multi-turn chat
└─────────────────────────────────────┘
```

### Design Patterns

| Pattern | Where Used |
|---|---|
| **Strategy** | Filter/sort/calculate operations in `VirtualFridge` via `IFoodItemFilterStrategy`, `IInventorySortStrategy`, `ICalculationStrategy` |
| **Service Interface** | `IRecipeService` decouples `RecipeFetcher` from `Form1` — swappable without UI changes |
| **Singleton / Service Locator** | `_virtualFridge`, `_recipeService`, `_userProfile` created once in `Form1` constructor and shared across all UI methods |
| **Factory** | `VirtualFridge` convenience methods (`GetExpiringItems`, `GetItemsByCategory`) internally construct and manage strategy instances |
| **Inheritance** | `PerishableItem` extends `FoodItem` adding expiry date logic |
| **Composition** | `NutrientProfile` composed of `NutrientValue` objects; `Recipe` composed of `RecipeIngredient` objects |

---

## Application Tabs

### Dashboard
- Donut chart showing pantry items by status (Use Now / Expiring Soon / Fresh)
- Alert center listing items expiring within 7 days
- Top 3 recipe recommendations based on current pantry

### Pantry
- Full inventory table with search and filter
- Add items via modal form with category and expiry date
- Edit or delete items via context menu (⋮)
- Status badges: 🔴 Use Now · 🟡 Expiring Soon · 🟢 Fresh

### Recipe Discovery
- Recipe cards with ingredient match percentage
- Filter chips: All · From My Pantry · Quick · Healthy · Budget-Friendly
- Search by recipe name
- Surprise Me — random pick from current filtered pool
- Cook Now → opens full recipe detail with ingredients and steps

### Chefbot AI
- Conversational kitchen assistant
- Context-aware — automatically knows your current pantry contents
- Powered by Google Gemma 3 running locally via Ollama
- Multi-turn memory within a session
- Clear chat button to start a new conversation

---

## Data Persistence

Pantry items are saved locally to `inventory.json` in the application directory. The file is created automatically on first launch and updated on every add, edit, or delete operation. Data persists across app restarts.

The file is gitignored and never committed to the repository.

---

## Development Guidelines

### Code Style
- Follow C# naming conventions
- Use LINQ for collection operations
- Enable nullable reference types
- Strategy Design Pattern for all backend filtering/sorting

### UI Development
- Use Guna.UI2 components for consistency
- Reference `AppColors.cs` for all colors — never hardcode color values
- Keep UI logic minimal — delegate all business logic to backend services

### Service Integration
- All backend calls go through interfaces (`IRecipeService`, etc.)
- Services are injected into `Form1` constructor
- Always wrap API calls in try/catch with user-facing fallback messages

---

## Troubleshooting

| Issue | Solution |
|---|---|
| NuGet packages not found | Run `dotnet restore` and rebuild |
| Windows Forms designer not loading | Ensure `UseWindowsForms` is `true` in project files |
| Missing Guna.UI2 components | Rebuild or manually restore `Guna.UI2.WinForms` package |
| Chefbot shows offline | Make sure Ollama is installed and running (`ollama serve`) |
| No recipes loading | Check that your Spoonacular API key is set in `Config.cs` |
| Pantry items lost after restart | Check that `inventory.json` exists next to the `.exe` in the build output folder |

---

## Future Enhancements

- [ ] Full database persistence (SQLite via EF Core)
- [ ] User authentication and cloud sync
- [ ] Dark / Light theme toggle
- [ ] Nutritional information tracking per pantry item
- [ ] Meal plan generation
- [ ] Shopping list export (PDF / CSV)
- [ ] Mobile companion app

---

## Authors

| Name | Role |
|---|---|
| **Regina Annemarie I. Bool** | API Developer / AI Researcher |
| **Ritzy Leewis G. Celestial** | Backend / Infrastructure Manager |
| **Mhalik B. Perez** | Frontend / UI Development |

---

## Support

For issues, questions, or feature requests, please open an issue on the [GitHub repository](https://github.com/richeeese/Pseuchef).

---

*Last Updated: 2026 · Framework: .NET 8 · UI: Windows Forms (Guna.UI2)*
