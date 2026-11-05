using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Client_App.Models;

public sealed class Drill
{
    // Omitted from the JSON when 0, so the database generates it on insert
    [JsonPropertyName("drillid")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public long DrillId
    {
        get; set;
    }

    // Null if the activity was removed (FK is ON DELETE SET NULL)
    [JsonPropertyName("activity_id")]
    public long? ActivityId
    {
        get; set;
    }

    // Display only, not sent to the database
    [JsonIgnore]
    public string ActivityName { get; set; } = string.Empty;

    // Display only: lets the add-drill form remember which category was picked
    [JsonIgnore]
    public long? CategoryId
    {
        get; set;
    }
    // Display only: lets the add-drill form remember which school level was picked
    [JsonIgnore]
    public string SchoolLevel { get; set; } = string.Empty;

    // Display only, not sent to the database
    [JsonIgnore]
    public string CategoryName { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("image_url")]
    public string? ImageUrl
    {
        get; set;
    }

    [JsonPropertyName("video_url")]
    public string? VideoUrl
    {
        get; set;
    }

    // Maps to the text[] column as a JSON array
    [JsonPropertyName("steps")]
    public List<string> Steps { get; set; } = [];

    // Display helpers, aren't sent to the database
    [JsonIgnore]
    public string ImageMark => string.IsNullOrWhiteSpace(ImageUrl) ? "✗" : "✓";

    [JsonIgnore]
    public string VideoMark => string.IsNullOrWhiteSpace(VideoUrl) ? "✗" : "✓";

    [JsonIgnore]
    public string StepsText => string.Join("\n", Steps.Select((s, i) => $"{i + 1}. {s}"));

    public Drill()
    {
    }

    public Drill(long? activityId, string activityName, string name, string description,
                 string? image, string? video, List<string> steps)
    {
        ActivityId = activityId;
        ActivityName = activityName;
        Name = name;
        Description = description;
        ImageUrl = image;
        VideoUrl = video;
        Steps = steps;
    }
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
