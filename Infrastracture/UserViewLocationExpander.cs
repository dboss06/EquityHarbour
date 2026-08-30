using Microsoft.AspNetCore.Mvc.Razor;

namespace EquityHarbour.Infrastructure
{
    public class UserViewLocationExpander : IViewLocationExpander
    {
        public void PopulateValues(ViewLocationExpanderContext context) { }

        public IEnumerable<string> ExpandViewLocations(
            ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            var userLocations = new[]
            {
                "/Views/User/{1}/{0}.cshtml",
                "/Views/User/Shared/{0}.cshtml"
            };

            return userLocations.Concat(viewLocations);
        }
    }
}