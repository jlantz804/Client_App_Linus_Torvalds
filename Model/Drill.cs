using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Client_App.Model;

[Table("drills")]
public class Drill : ObservableBaseModel
{
    int _drillId;
    int? _activityId;
    string _activityName = "";
    string _name = "";
    string _description = "";
    string? _imageUrl;
    string? _videoUrl;
    List<string> _steps = new (); 
    
    [PrimaryKey("drillid", false)]
    public int DrillId
    {
        get => _drillId;
        set => SetProperty(ref _drillId, value);
    }

    [Column("activity_id")]
    public int? ActivityId
    {
        get => _activityId;
        set => SetProperty(ref _activityId, value);
    }
    
    [JsonIgnore]
    public string ActivityName
    {
        get => _activityName;
        set => SetProperty(ref _activityName, value ?? "");
    }

    [Column("name")]
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value ?? "");
    }

    [Column("description")]
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value ?? "");
    }

    [Column("image_url")]
    public string? ImageUrl
    {
        get => _imageUrl;
        set => SetProperty(ref _imageUrl, value);
    }

    [Column("video_url")]
    public string? VideoUrl
    {
        get => _videoUrl;
        set => SetProperty(ref _videoUrl, value);
    }

    [Column("steps")]
    public List<string> Steps
    {
        get => _steps;
        set
        {
            if (SetProperty(ref _steps, value ?? new()))
                OnPropertyChanged(nameof(StepsText));
        }
    }

    // Display helpers (no [Column], so they aren't sent to the database)
    [JsonIgnore] public string ImageMark => string.IsNullOrWhiteSpace(ImageUrl) ? "✗" : "✓";
    [JsonIgnore] public string VideoMark => string.IsNullOrWhiteSpace(VideoUrl) ? "✗" : "✓";
    [JsonIgnore] public string StepsText => string.Join("\n", Steps.Select((s, i) => $"{i + 1}. {s}"));

    public Drill(int activityId, string activityName, string name, string description, string image, string video, List<string> steps)
    {
        ActivityId = activityId;
        ActivityName = activityName;
        Name = name;
        Description = description;
        ImageUrl = image;
        VideoUrl = video;
        Steps = steps;
    }

    public Drill() { }
}

public class StepItem : INotifyPropertyChanged
{
    string _text = "";
    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}