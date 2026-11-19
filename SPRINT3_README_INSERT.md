### Sprint 3 — UI Improvements and Feature Integration (November 2026)

- **Activities screen redesign (Rishikesh):** Updated the Activities workspace to follow the Lesson Plans visual style, including a consistent green-and-white theme, rounded cards, a search field, and a custom **New Activity** editor. This makes the screens easier to recognize and use together. 🙂
- **School-specific activity browsing:** Preserved the two elementary categories—**Object Control Skills** and **Locomotor Movements**—while Middle and High School use their own **Sports & Activities** lists. Activity search works within the selected category.
- **Alphabetical activity display (Rishikesh):** Prepared an A–Z, case-insensitive sorting change for the Activities list, so entries are easier to find after loading, searching, adding, or deleting. **Verify that this latest change has been committed to the repository.**
- **Shared design language:** Brought the Activities layout closer to the existing Lesson Plans experience, with matching action buttons, information cards, and editor interactions. 🙂
- **Better separation of responsibilities:** The Activities implementation uses `BusinessLogic`, `IDatabase`, `Database`, and `ErrorReporting` instead of keeping database operations directly in the page. This makes the code easier to maintain and debug. 🙂
- **Other feature screens:** The project contains Drills and Equipment Modifications pages. The Drills UI includes search, editing, steps, and delete controls; the Equipment Modifications UI includes activity selection and an entry form. These features still require team-level testing.
- **Consistent coding practices:** Added the team's `coding_standards.md` and `.editorconfig` files to document coding conventions and support C# formatting checks.

**Sprint 3 verification:** Confirm the alphabetical-sort and optional back-button cleanup changes are committed. Run an Android build and test Activities, Drills, Equipment, and Lesson Plans before treating the sprint as complete.

- **Teacher sign-in and account creation (Joey):** Added Supabase email-and-password sign-in and teacher account creation, with name and password validation and email confirmation handling.
- **Lesson plan management (Joey):** Added lesson plan browsing, search, school-level filtering, and forms to create and edit plans with activity, grade, date, duration, and objectives. Lesson plans are loaded from and saved to Supabase.
- **Lesson plan ownership and admin controls (Joey):** Recorded each plan’s creator and displayed creator names when available. Only a plan’s creator or an administrator can edit or delete a locked lesson plan.
