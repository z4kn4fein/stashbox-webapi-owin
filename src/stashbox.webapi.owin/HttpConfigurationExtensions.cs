using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Hosting;
using Microsoft.Owin;
using Owin;
using Stashbox.Infrastructure;
using Stashbox.Utils;
using Stashbox.Web.WebApi;

namespace System.Web.Http
{
    /// <summary>
    /// Represents the owin web api related stashbox extensions.
    /// </summary>
    public static class AppBuilderOwinWebApiExtensions
    {
        private static readonly DelegatingHandler StashboxScopeHandler = new ScopeHandler();

        /// <summary>
        /// Sets a custom messagehandler that uses the owin per request scope configured by <see cref="AppBuilderExtensions.UseStashbox"/>.
        /// </summary>
        /// <param name="builder">The app builder.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="container">The container.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="configuration"/> is <c>null</c>.
        /// </exception>
        public static IAppBuilder UseStashboxWebApi(this IAppBuilder builder, HttpConfiguration configuration, IStashboxContainer container)
        {
            Shield.EnsureNotNull(configuration, nameof(configuration));

            if (!configuration.MessageHandlers.Contains(StashboxScopeHandler))
                configuration.MessageHandlers.Insert(0, StashboxScopeHandler);

            configuration.UseStashbox(container);

            return builder;
        }

        private class ScopeHandler : DelegatingHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var context = request.GetOwinContext();
                var scope = context?.GetCurrentStashboxScope();
                if (scope != null)
                    request.Properties[HttpPropertyKeys.DependencyScope] = new StashboxDependencyScope(scope);

                return base.SendAsync(request, cancellationToken);
            }
        }
    }
}
