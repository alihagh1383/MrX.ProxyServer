namespace MrX.ProxyServer.Statics;

public class Funcs
{
    internal static bool Do(Action func)
    {
        try
        {
            func.Invoke();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
    }
    internal static Task<bool> DoAsync(Action func)
    {
        try
        {
            func.Invoke();
            return Task.FromResult<bool>(true);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return Task.FromResult<bool>(false);
        }
    }
    internal static T? DoReturn<T>(Func<T> func)
    {
        try
        {
            return func.Invoke();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message + e);
            return default;
        }
    }
}