using System.Security.Claims;

namespace AuditPilot.API.Helpers
{
    public static class SessionHelper
    {
        private static IHttpContextAccessor _httpContextAccessor;

        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public static Guid? GetCurrentUserId()
        {
            var user = _httpContextAccessor?.HttpContext?.User;
            if (user == null)
                return null;

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return null;

            if (Guid.TryParse(userIdClaim.Value, out var userId))
                return userId;

            return null;
        }
    }
}
