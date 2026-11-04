namespace Northwind.WebUI.Graftcode;

internal static class GraftAsync
{
    // Graft methods are synchronous for now, so this waits for the task at the boundary.
    // The method body stays async/await and this bridge can be removed when GG supports Task.
    internal static T Run<T>(Func<Task<T>> operation) =>
        Task.Run(operation).ConfigureAwait(false).GetAwaiter().GetResult();
}
