namespace ABEC_System.Constants;

public static class SystemConstants
{
    public const int DefaultCourseCapacity = 25;

    public static class ApplicationStatuses
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
        public const string Waitlisted = "Waitlisted";
    }

    public static class AccountStatuses
    {
        public const string Active = "Active";
        public const string Inactive = "Inactive";
    }

    public static class BatchStatuses
    {
        public const string Active = "Active";
        public const string Closed = "Closed";
    }

    public static class CourseStatuses
    {
        public const string Available = "Available";
        public const string Unavailable = "Unavailable";
    }

    public static class DocumentStatuses
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
        public const string Released = "Released";
    }

    public static class NotificationStatuses
    {
        public const string Unread = "Unread";
        public const string Read = "Read";
    }
}
