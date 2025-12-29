namespace Grapher.Configuration
{
    /// Application role names; values are bound from configuration at startup
    public class AppRoles
    {
        public string AdminRole { get; set; } = "Administrator";
        public string MemberRole { get; set; } = "Member";
    }
}