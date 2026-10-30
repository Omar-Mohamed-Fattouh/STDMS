//// Filters/CustomAuthorizationFilter.cs
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Filters;
//using System.Linq;

//namespace STDMS.Filters
//{
//    public class CustomAuthorizationFilter : IAuthorizationFilter
//    {
//        private readonly string[] _roles;

//        // The constructor now accepts roles passed from the [TypeFilter] attribute
//        public CustomAuthorizationFilter(params string[] roles)
//        {
//            _roles = roles;
//        }

//        public void OnAuthorization(AuthorizationFilterContext context)
//        {
//            var user = context.HttpContext.User;

//            // Check if user is authenticated AND has at least one of the required roles.
//            // If _roles array is empty, it means authorization is required but no specific roles,
//            // so just checking IsAuthenticated is enough.
//            if (!user.Identity.IsAuthenticated || (_roles.Any() && !_roles.Any(role => user.IsInRole(role))))
//            {
//                // If authorization fails, redirect to an access denied page.
//                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
//            }
//        }
//    }
//}
