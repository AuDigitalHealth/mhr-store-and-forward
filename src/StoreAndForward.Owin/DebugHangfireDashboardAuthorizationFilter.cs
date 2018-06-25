using Hangfire.Dashboard;

namespace DigitalHealth.StoreAndForward.Owin
{
    /// <summary>
    /// Hangfire debug authorization filter. This allows the Hangfire dashboard to be viewable outside localhost.
    /// </summary>
    public class DebugHangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        /// <summary>
        /// Authorize call.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool Authorize(DashboardContext context)
        {
            return true;
        }
    }
}