namespace YIT.Akaun.Models.ViewModels.Administrations
{
    public class UserClaimsViewModel
    {
        public UserClaimsViewModel()
        {
            Claims = new List<UserClaim>();
        }
        public string UserId { get; set; } = string.Empty;
        public List<UserClaim> Claims { get; set; } = new List<UserClaim>();

        public class UserClaim
        {
            public string ClaimType { get; set; } = string.Empty;
            public string ClaimValue { get; set; } = string.Empty;
            public bool IsSelected { get; set; }
        }
    }
}
