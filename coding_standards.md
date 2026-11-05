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