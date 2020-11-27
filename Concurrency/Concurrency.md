### async Void vs async Task

Void metodu async olarak sadece eventhandler'larda kullanýlmalýdýr. Diðer türlü en büyük sorun bir hata ile karþýlaþýldýðý durumdur. Void metodu async olarak kullanýldýðýnda SynchronizationContext aktif olur. Ancak buradaki hataya eriþim mümkün deðildir.
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
Async void metodu farklý yorumlamaya sahiptir. Task ya da Task\<T\> dönen bir metotda kolaylýkla await, Task.WhenAny/All gibi metotlar kullanýlabilir. Bu yüzden void türündeki metotlarýn tamamlanýp tamamlanmadýðý gibi Task'a ait özelliklerin hepsinden mahrum kalmýþ oluyoruz. Async void metodu SynchronizationContext üzerinde ne zaman baþladýðýný ya da tamamlandýðý bilgisini set eder. Ancak buna eriþmek için kompleks çözümlere baþvurmak gerekecektir.

Kaynak:

https://docs.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming


