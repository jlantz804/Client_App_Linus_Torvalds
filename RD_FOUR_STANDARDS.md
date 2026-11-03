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

### 13. camelCase for C# parameters and local variables [AM]

**Standard:** Use camelCase for parameters and local variables.

**Bad:**
```csharp
void ShowActivities(string SchoolLevel) { }
```

**Good:**
```csharp
void ShowActivities(string schoolLevel) { }
```

**How checked:** Look at parameters and local variables in C# files.

### 14. Keep C# comments short and useful [AM]

**Standard:** Use // comments for non-obvious actions. Explain why a certain action was done instead of only what it is doing.
**Bad:**
```csharp
// load activites
await LoadActivitiesAsync();
```

**Good:**
```csharp
// Reload after deleting so the screen matches Supabase.
await LoadActivitiesAsync();
```

**How checked:** Look over all C# file comments.

### 15. Use WHERE when using SQL DELETE AND UPDATE [AM]

**Standard:** Every DELETE or UPDATE must use a WHERE  to filter specific rows, unless the group unanimously wants to change entire tables.

**Bad:**
```sql
DELETE FROM Public.Activities;
```

**Good:**
```sql
DELETE FROM Public.Activities WHERE id = 1;
```

**How checked:** Look over all SQL queries.

### 16. SQL primary/foreign keys [AM]

**Standard:** Define a primary key for each new table and a foreign-key constraint for columns that reference another table.

**Bad:**
```sql
CREATE TABLE Activities (id INT, category_id INT);
```

**Good:**
```sql
CREATE TABLE Activities (
    id INT PRIMARY KEY,
    category_id INT NOT NULL REFERENCES activity_categories(id)
);
```

**How checked:** Look over all SQL queries.

