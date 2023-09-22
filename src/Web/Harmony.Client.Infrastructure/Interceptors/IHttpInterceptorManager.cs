using Harmony.Client.Infrastructure.Managers;
using Toolbelt.Blazor;

namespace Harmony.Client.Infrastructure.Interceptors
{
    public interface IHttpInterceptorManager : IManager
    {
        void RegisterEvent();

        Task InterceptBeforeHttpAsync(object sender, HttpClientInterceptorEventArgs e);

        void DisposeEvent();
    }
}