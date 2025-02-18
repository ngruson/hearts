using Hearts.BlazorApp.Services.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System.Linq.Expressions;

namespace Hearts.BlazorApp.Services.SignalR;

internal static class HubConnectionExtensions
{
    public static IDisposable On<T>(this HubConnection hubConnection, Expression<Func<T, Task>> method, Action<T> handler)
    {
        string methodName = ((MethodCallExpression)method.Body).Method.Name;
        return hubConnection.On(methodName, handler);
    }
}
