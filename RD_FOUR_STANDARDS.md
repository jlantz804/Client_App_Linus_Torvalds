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

