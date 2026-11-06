# Team Linus Torvalds — Coding Standards

**Course:** CS341 Software Engineering  
**Project:** Adapted Physical Education (APE) — `Client_App` (.NET MAUI + Supabase PostgreSQL)  
**Version:** 1.0 team draft — review and approve before adopting

We agree to use the same rules across the codebase. Each member owns four standards (16 total). Initials identify ownership and must remain beside each standard. C# formatting and naming rules are partly enforced through `.editorconfig` and `dotnet format`; project-organization, XAML, and SQL rules are checked during pull-request review.

**PR review:** A different teammate must approve each contributor's pull request before it is merged. The document becomes the team's final standard only after all four sections have been reviewed and approved.
## Alex Martinez (AM) — four standards

1. **Prefix private fields with an underscore and use camelCase.** For example, `_databaseService`. (`.editorconfig`)
2. **Use camelCase for parameters and local variables.** For example, `userId` or `selectedItem`. (`.editorconfig`)
3. **Use PascalCase for XAML page and control names, and give named controls meaningful names.** For example, `LoginPage` and `UsernameEntry`. Keep `x:Name` values descriptive and consistent with the related page.
4. **Use parameterized database queries.** Never build SQL by concatenating user input into a query. Keep table and column names consistent with the existing Supabase schema, and handle database errors rather than silently ignoring them.

## Gavin Anderson (GA) — four standards

1. **Use PascalCase for types.** Classes, structs, enums, and delegates must begin each word with a capital letter (for example, `DatabaseService`). (`.editorconfig`)
2. **Use PascalCase for methods.** Method names must describe the action or result (for example, `LoadUserProfile`). (`.editorconfig`)
3. **Choose descriptive names.** Avoid unclear abbreviations and generic names such as `data`, `thing`, or `temp` when a more specific name is available.
4. **Use one primary type per C# file.** Name the file after that type (for example, `AuthenticationService.cs` contains `AuthenticationService`). Small related helper or private nested types may remain with the primary type when that improves readability.

## Joey Lantz (JL) — four standards

1. **Use Allman brace style.** Put opening braces on a new line for namespaces, types, methods, properties, and control blocks. (`.editorconfig`)
2. **Always use braces for control blocks.** `if`, `else`, loops, and similar statements must use braces, even when their body is one line. (`.editorconfig`)
3. **Do not write single-line blocks or statements.** Keep block contents on separate lines so changes are easier to read and review. (`.editorconfig`)
4. **Keep methods focused.** A method should perform one clear task. Split long or multi-purpose methods into smaller methods with descriptive names when doing so improves readability or reuse.

### 1. C# class names: PascalCase [RD]

**Standard:** Use PascalCase for every C# class name, including pages and services.

**Bad:**
```csharp
class activitiespage { }
```

**Good:**
```csharp
class ActivitiesPage { }
```

**How checked:** C# naming rule.

### 2. C# indentation: four spaces [RD]

**Standard:** Indent each nested C# block with four spaces. Do not use tabs.

**Bad:**
```csharp
if (isReady)
{
  LoadActivities();
}
```

**Good:**
```csharp
if (isReady)
{
    LoadActivities();
}
```

**How checked:** C# formatting.

### 3. SQL table names: plural snake_case [RD]

**Standard:** Name new SQL tables using lowercase, plural snake_case. Keep the current names such as school_levels and activity_categories.

**Bad:**
```sql
CREATE TABLE ActivityCategories (id BIGINT);
```

**Good:**
```sql
CREATE TABLE activity_categories (id BIGINT);
```

**How checked:** SQL review.

### 4. SQL column names: snake_case [RD]

**Standard:** Write column names using lowercase snake_case.

**Bad:**
```sql
SELECT SchoolLevel FROM public.activity_categories;
```

**Good:**
```sql
SELECT school_level FROM public.activity_categories;
```

**How checked:** SQL review.



## Enforcement and exceptions

Place `.editorconfig` next to `Client_App.csproj`. Check formatting with:

```bash
dotnet format Client_App.csproj --verify-no-changes
```

Apply formatting with:

```bash
dotnet format Client_App.csproj
```

Existing files may need a one-time formatting update. Run formatting before submitting a pull request and review the resulting changes before committing.

`.editorconfig` targets C# files (`[*.cs]`). It enforces or reports the configured indentation, brace, and naming preferences. It does not validate comments, method responsibility, XAML naming, SQL safety, or consistency with the Supabase schema. Reviewers check those standards manually.

Do not run automated mass formatting in the same pull request as a small feature change unless the team agrees. If a standard must be broken for a specific reason, explain the exception in the pull request and get reviewer approval.

## Sources for inspiration

- [Software Engineering at Google, Chapter 8: Style Guides and Rules](https://abseil.io/resources/swe-book/html/ch08.html)
- [C# at Google Style Guide](https://github.com/google/styleguide/blob/gh-pages/csharp-style.md)
- [SQL Style Guide](https://www.sqlstyle.guide/)
- [Microsoft Learn: naming rules](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/naming-rules)
- [Microsoft Learn: dotnet format](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-format)

*Our team intentionally uses four-space indentation and Allman braces, which match common .NET conventions and differ from Google's two-space / same-line brace preference. A style guide is inspiration, not a requirement to copy all of its choices.*