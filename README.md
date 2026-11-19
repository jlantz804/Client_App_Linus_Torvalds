Adapted Physical Education (APE) Support App
CS341 Software Engineering — Team Linus Torvalds
Client: Korey Kleinhans (Physical Education)
Team: Gavin Anderson, Joey Lantz, Alex Martinez, and Rishikesh Dhanasekaran

Our .NET MAUI mobile app helps adapted physical education (APE) teachers and paraprofessionals find school-appropriate activities, drills, equipment modifications, and lesson plans in one place.
Sprint Progress — Newest First
Sprint 3 — UI Improvements and Feature Integration (November 2026)
- Activities screen redesign (Rishikesh): Updated the Activities workspace to follow the Lesson Plans visual style, including a consistent green-and-white theme, rounded cards, a search field, and a custom New Activity editor. This makes the screens easier to recognize and use together. 🙂🙂🙂
- School-specific activity browsing: Preserved the two elementary categories—Object Control Skills and Locomotor Movements—while Middle and High School use their own Sports & Activities lists. Activity search works within the selected category.
- Alphabetical activity display (Rishikesh): Prepared an A–Z, case-insensitive sorting change for the Activities list, so entries are easier to find after loading, searching, adding, or deleting. Verify that this latest change has been committed to the repository.
- Shared design language: Brought the Activities layout closer to the existing Lesson Plans experience, with matching action buttons, information cards, and editor interactions. 🙂🙂
- Better separation of responsibilities: The Activities implementation uses BusinessLogic, IDatabase, Database, and ErrorReporting instead of keeping database operations directly in the page. This makes the code easier to maintain and debug. 🙂🙂
- Consistent coding practices: Added the team's coding_standards.md and .editorconfig files to document coding conventions and support C# formatting checks.🙂

Sprint 2 — Navigation and Database Connectivity 
- Connected the existing school-selection flow to the destination menu and its feature screens using .NET MAUI Shell navigation.
- Added Supabase PostgreSQL tables for school_levels, activity_categories, and activities, including 39 starting activities across Elementary, Middle, and High School.
- Connected Activities to Supabase so users can browse categories and add or remove activities, with validation and error messages.
- Developed the Lesson Plans screen and its database-backed viewing and management functionality.
- Added Supabase Row Level Security (RLS) policies and connected application services to the database.
- Worked through integration problems involving missing model files, dependency injection, Android emulator deployment, and Shell navigation.

Sprint 1 — Initial Prototype (October 2026)
- Gathered requirements for an easy-to-use APE support app for paraprofessionals.
- Created initial .NET MAUI screen prototypes for school selection, activities, sports and drills, equipment, and lesson plans.
- Organized navigation concepts and divided feature areas among the team.
- Began collaborating through GitHub branches and pull requests.