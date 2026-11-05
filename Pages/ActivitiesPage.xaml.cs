using Client_App.Models;
using Microsoft.Maui.Controls.Shapes;

namespace Client_App;

/*
Author: Rishikesh Dhanasekaran
Show activities from Supabase. Add and remove activities by school level.
*/
[QueryProperty(nameof(SchoolLevel), "schoolLevel")]
public partial class ActivitiesPage : ContentPage
{
    private readonly BusinessLogic _activities;
    private readonly List<ActivityCategory> _categories = [];
    private ActivityCategory? _selectedCategory;
    private string _schoolCode = string.Empty;
    private bool _loaded;
    private bool _busy;

    public string SchoolLevel
    {
        get => _schoolCode;
        set
        {
            _schoolCode = value;
            _loaded = false;
            SchoolLabel.Text = value switch
            {
                "elementary" => "Elementary School (PreK - 5)",
                "middle" => "Middle School (6 - 8)",
                "high" => "High School (9 - 12)",
                _ => value
            };
        }
    }

    public ActivitiesPage(BusinessLogic activities)
    {
        _activities = activities;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_loaded)
        {
            return;
        }

        await LoadCategoriesAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        if (_busy)
        {
            return;
        }

        SetBusy(true, "Loading categories...");
        try
        {
            _categories.Clear();
            _categories.AddRange(await _activities.GetActivityCategoriesAsync(_schoolCode));
            _loaded = true;
            if (_schoolCode == "elementary")
            {
                CategoryPanel.IsVisible = true;
                ActivityPanel.IsVisible = false;
                StatusLabel.Text = "Select Object Control Skills or Locomotor Movements.";
            }
            else
            {
                _selectedCategory = _categories.FirstOrDefault(c => c.Name == "Sports & Activities");
                if (_selectedCategory is null)
                {
                    ActivityPanel.IsVisible = false;
                    StatusLabel.Text = "Sports & Activities category was not found for this school level.";
                    return;
                }
                CategoryPanel.IsVisible = false;
                ActivityPanel.IsVisible = true;
                BackToCategoriesButton.IsVisible = false;
                CategoryTitle.Text = "Sports & Activities";
                await ReadActivitiesAsync();
            }
        }
        catch (Exception ex)
        {
            _loaded = false;
            StatusLabel.Text = "Could not load activities.";
            await DisplayAlertAsync("Supabase error",
                ex.Message + "\n\nIf anonymous sign-ins are disabled, enable them in Supabase Authentication settings.", "OK");
        }
        finally { SetBusy(false); }
    }

    private async Task OpenCategoryAsync(string categoryName)
    {
        if (_busy)
        {
            return;
        }

        _selectedCategory = _categories.FirstOrDefault(c => c.Name == categoryName);
        if (_selectedCategory is null)
        {
            await DisplayAlertAsync("Category not found", categoryName + " was not found in Supabase.", "OK");
            return;
        }
        CategoryTitle.Text = categoryName;
        BackToCategoriesButton.IsVisible = true;
        CategoryPanel.IsVisible = false;
        ActivityPanel.IsVisible = true;
        await RefreshActivitiesAsync();
    }

    private async void OnObjectControlClicked(object? sender, EventArgs e) =>
        await OpenCategoryAsync("Object Control Skills");

    private async void OnLocomotorClicked(object? sender, EventArgs e) =>
        await OpenCategoryAsync("Locomotor Movements");

    private void OnBackToCategoriesClicked(object? sender, EventArgs e)
    {
        ActivityPanel.IsVisible = false;
        CategoryPanel.IsVisible = true;
        _selectedCategory = null;
    }

    private async Task ReadActivitiesAsync()
    {
        if (_selectedCategory is null)
        {
            return;
        }

        var items = await _activities.GetActivitiesByCategoryAsync(_selectedCategory.Id);
        ActivityList.Children.Clear();
        foreach (var item in items)
        {
            ActivityList.Children.Add(CreateActivityCard(item));
        }

        StatusLabel.Text = $"{items.Count} activities in {CategoryTitle.Text}.";
    }

    private async Task RefreshActivitiesAsync()
    {
        if (_busy)
        {
            return;
        }

        SetBusy(true, "Loading activities...");
        try
        {
            await ReadActivitiesAsync();
        }
        catch (Exception ex) { await DisplayAlertAsync("Supabase error", ex.Message, "OK"); }
        finally { SetBusy(false); }
    }

    private async void OnRefreshClicked(object? sender, EventArgs e) =>
        await RefreshActivitiesAsync();

    private View CreateActivityCard(ActivityItem item)
    {
        var layout = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            ColumnSpacing = 10,
            Padding = new Thickness(12)
        };

        var name = new Label
        {
            Text = item.Name,
            FontSize = 16,
            TextColor = Color.FromArgb("#18352B"),
            FontAttributes = FontAttributes.Bold,
            VerticalTextAlignment = TextAlignment.Center
        };
        var remove = new Button
        {
            Text = "Remove",
            BackgroundColor = Color.FromArgb("#FFF0ED"),
            TextColor = Color.FromArgb("#A64232"),
            CornerRadius = 10,
            FontSize = 13
        };
        remove.Clicked += async (_, _) => await RemoveActivityAsync(item);

        Grid.SetColumn(name, 0);
        Grid.SetColumn(remove, 1);
        layout.Children.Add(name);
        layout.Children.Add(remove);

        return new Border
        {
            Stroke = new SolidColorBrush(Color.FromArgb("#E1E9E4")),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(14) },
            BackgroundColor = Colors.White,
            Content = layout
        };
    }

    private async void OnAddActivityClicked(object? sender, EventArgs e)
    {
        if (_busy || _selectedCategory is null)
        {
            return;
        }

        var name = await DisplayPromptAsync("Add Activity",
            $"New activity in {_selectedCategory.Name}:",
            "Add", "Cancel", placeholder: "Activity name", maxLength: 100);
        if (name is null) // Cancel was clicked.
        {
            return;
        }
        name = name.Trim();

        SetBusy(true, "Adding activity...");
        try
        {
            ActivityError result = await _activities.AddActivityAsync(_selectedCategory.Id, name);
            if (result == ActivityError.NoError)
            {
                await ReadActivitiesAsync();
            }
            else
            {
                await DisplayAlertAsync("Unable to add activity", result.ToUserMessage(), "OK");
            }
        }
        catch (Exception ex) { await DisplayAlertAsync("Unable to add activity", ex.Message, "OK"); }
        finally { SetBusy(false); }
    }

    private async Task RemoveActivityAsync(ActivityItem item)
    {
        if (_busy)
        {
            return;
        }

        bool confirm = await DisplayAlertAsync("Remove Activity",
            $"Remove '{item.Name}' from this school level?", "Remove", "Cancel");
        if (!confirm)
        {
            return;
        }

        SetBusy(true, "Removing activity...");
        try
        {
            ActivityError result = await _activities.RemoveActivityAsync(item.Id);
            if (result == ActivityError.NoError)
            {
                await ReadActivitiesAsync();
            }
            else
            {
                await DisplayAlertAsync("Unable to remove activity", result.ToUserMessage(), "OK");
            }
        }
        catch (Exception ex) { await DisplayAlertAsync("Unable to remove activity", ex.Message, "OK"); }
        finally { SetBusy(false); }
    }

    private void SetBusy(bool busy, string? status = null)
    {
        _busy = busy;
        BusySpinner.IsRunning = BusySpinner.IsVisible = busy;
        AddActivityButton.IsEnabled = !busy;
        if (status is not null)
        {
            StatusLabel.Text = status;
        }
    }
    private async void OnBackToHomeClicked(object? sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Navigation Error",
                ex.Message,
                "OK");
        }
    }
}
