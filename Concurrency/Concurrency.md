### async Void vs async Task

Void metodu async olarak sadece eventhandler'larda kullan�lmal�d�r. Di�er t�rl� en b�y�k sorun bir hata ile kar��la��ld��� durumdur. Void metodu async olarak kullan�ld���nda SynchronizationContext aktif olur. Ancak buradaki hataya eri�im m�mk�n de�ildir.
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
Async void metodu farkl� yorumlamaya sahiptir. Task ya da Task\<T\> d�nen bir metotda kolayl�kla await, Task.WhenAny/All gibi metotlar kullan�labilir. Bu y�zden void t�r�ndeki metotlar�n tamamlan�p tamamlanmad��� gibi Task'a ait �zelliklerin hepsinden mahrum kalm�� oluyoruz. Async void metodu SynchronizationContext �zerinde ne zaman ba�lad���n� ya da tamamland��� bilgisini set eder. Ancak buna eri�mek i�in kompleks ��z�mlere ba�vurmak gerekecektir.

Kaynak:

https://docs.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming


