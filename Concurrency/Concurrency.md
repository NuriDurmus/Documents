### async Void vs async Task

Void metodu async olarak sadece eventhandler'larda kullanılmalıdır. Diğer türlü en büyük sorun bir hata ile karşılaşıldığı durumdur. Void metodu async olarak kullanıldığında SynchronizationContext aktif olur. Ancak buradaki hataya erişim mümkün değildir.
```csharp
private async void ThrowExceptionAsync()
{
  throw new InvalidOperationException();
}
public void AsyncVoidExceptions_CannotBeCaughtByCatch()
{
  try
  {
    ThrowExceptionAsync();
  }
  catch (Exception)
  {
    // The exception is never caught here!
    throw;
  }
}
```
Async void metodu farklı yorumlamaya sahiptir. Task ya da Task\<T\> dönen bir metotda kolaylıkla await, Task.WhenAny/All gibi metotlar kullanılabilir. Bu yüzden void türündeki metotların tamamlanıp tamamlanmadığı gibi Task'a ait özelliklerin hepsinden mahrum kalmış oluyoruz. Async void metodu SynchronizationContext üzerinde ne zaman başladığını ya da tamamlandığı bilgisini set eder. Ancak buna erişmek için kompleks çözümlere başvurmak gerekecektir.

Kaynak:

https://docs.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming

ParallelLoopState
https://docs.microsoft.com/tr-tr/dotnet/api/system.threading.tasks.parallelloopstate.shouldexitcurrentiteration?view=net-5.0
