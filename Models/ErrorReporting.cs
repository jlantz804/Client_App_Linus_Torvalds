namespace Client_App.Models;
public enum DrillError
{
    NoError,
    DuplicateDrill,
    DBAdditionError
}
public enum EquipmentModificationError
{
    NoError,
    DuplicateEquipmentModiifcation,
    DBAdditionError
}
// Rishi: Errors returned from adding/removing activities.
public enum ActivityError
{
    NoError,
    InvalidActivityName,
    InvalidCategory,
    InvalidActivityId,
    DuplicateActivity,
    ActivityNotFound,
    PermissionDenied,
    DBAdditionError,
    DBDeletionError
}

public static class ActivityErrorReporting
{
    public static string ToUserMessage(this ActivityError error) => error switch
    {
        ActivityError.InvalidActivityName => "Enter an activity name between 1 and 100 characters.",
        ActivityError.InvalidCategory => "Select a valid activity category.",
        ActivityError.InvalidActivityId => "Invalid activity selected.",
        ActivityError.DuplicateActivity => "This activity is already in the selected category.",
        ActivityError.ActivityNotFound => "No activity was removed. It may already be deleted, or Supabase may be blocking the delete request.",
        ActivityError.PermissionDenied => "Supabase does not allow this change. Check the activities table RLS policies.",
        ActivityError.DBAdditionError => "Could not add the activity to Supabase. Please try again.",
        ActivityError.DBDeletionError => "Could not remove the activity from Supabase. Please try again.",
        _ => "No error."
    };
}
