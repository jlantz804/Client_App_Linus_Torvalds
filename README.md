# Client_App

A .NET MAUI application for adapted physical education planning. Teachers can choose a school level and work with lesson plans, activities, drills, and equipment modifications. The app uses Supabase for authentication and data storage.

## Features

- Teacher sign-in and account creation
- School-level navigation for elementary, middle, and high school
- Lesson plan browsing and management
- Activity browsing, search, creation, and deletion by category
- Drill browsing, search, creation, editing, and deletion
- Equipment modification entry

## Requirements

- .NET 10 SDK
- .NET MAUI workloads and the platform toolchain for the target device or operating system

The project targets Android, iOS, and Mac Catalyst. Windows is included when building on Windows. iOS and Mac Catalyst targets are not included when building on Linux.

## Build

Restore the MAUI workloads and build for a platform supported by your development environment:

```sh
dotnet workload restore ./Client_App.csproj
dotnet build ./Client_App.csproj -f net10.0-android
```

Other target frameworks are `net10.0-ios`, `net10.0-maccatalyst`, and `net10.0-windows10.0.19041.0`. Build for iOS and Mac Catalyst on macOS, and build for Windows on Windows. You can also open `./Client_App.csproj` in a compatible .NET MAUI IDE and select a device, simulator, or emulator to run the app.

## Supabase

The application connects to Supabase through `./SupabaseService.cs`. The backend must have the tables and access policies expected by the app. Anonymous sign-ins must be enabled in Supabase Authentication for features that request an anonymous activity-access session. Teacher sign-in and account creation use Supabase email/password authentication.

## Project layout

- `./Pages/` — MAUI XAML pages and their code-behind
- `./Models/` — data models, database access, and business logic
- `./Resources/` — app icon, fonts, images, styles, and other assets
- `./Platforms/` — platform-specific application entry points and configuration
- `./Client_App.csproj` — target frameworks, MAUI settings, and package references
