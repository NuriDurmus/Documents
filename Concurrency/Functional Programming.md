Sample
```csharp
var time = Disposable.Using(() => new System.Net.WebClient(),
                client => XDocument.Parse(client.DownloadString("http://time.gov/actualtime.cgi"))).Root.Attribute("time").Value;
```

```csharp
public static class Disposable
{
    public static TResult Using<TDisposable, TResult>(
        Func<TDisposable> factory,
        Func<TDisposable, TResult> map) where TDisposable : IDisposable
    {
        using(var disposable=factory())
        {
            return map(disposable);
        }
    }
}

```