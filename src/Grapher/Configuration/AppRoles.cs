namespace Grapher.Configuration
{
    /// <summary>
    /// Application role names. Values are bound from configuration at startup.
    /// </summary>
    public class AppRoles
    {
        public string AdminRole { get; set; } = "Administrator";
        public string MemberRole { get; set; } = "Member";
    }
}