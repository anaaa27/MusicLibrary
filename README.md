# MusicLibrary

An extensible domain-driven music catalog and inventory system built with modern C# and .NET. 

The project demonstrates clean object-oriented architecture, custom data modeling, type hierarchies, in-memory collection management, serialization, and test-driven development.

---

## Key Features

- **Domain Modeling & Hierarchy:**
  - Abstract base domain model (`PlayableItem`) defining common core contracts.
  - Polymorphic implementations for concrete kinds: `StudioTrack`, `LiveTrack`, and `PodcastEpisode`.
  - Encapsulated business rules (milestones, rating validation, duration formatting).

- **Polymorphism & Pattern Matching:**
  - Dynamic object description without explicit runtime type queries or `if` chains.
  - C# pattern matching and switch expressions inspecting types and property constraints (duration thresholds, release eras).

- **In-Memory Storage & Querying:**
  - Generic/collection-backed store (`MusicLibraryStore`) supporting CRUD operations, duplicate prevention, and case-insensitive lookups.
  - Safe search utilizing `out` parameter conventions (`TryFindByName`) and flexible multi-item additions via `params`.

- **Serialization & DTO Mapping:**
  - Custom data transfer mapping (`MusicLibraryMap`) using `System.Text.Json` to handle nullable constraints and structural conversion.

- **Testing & Code Quality:**
  - Thorough unit test suite validating domain behavior, edge cases, and contract compliance across all project milestones.

---

## Tech Stack

- **Platform:** .NET 8 / C#
- **Testing:** xUnit / SharpQuest Spec Framework
- **Serialization:** `System.Text.Json`
- **IDE:** JetBrains Rider / VS Code

---

## Getting Started

### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/download)

### Build & Run Tests
```powershell
# Restore and build the solution
dotnet build

# Run project unit tests
dotnet test

# Run course/spec test suite
.\sq test
