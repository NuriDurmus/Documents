Aþaðýdaki gibi iki datetime listemiz olsun. Burada listelerin birbiriyle ayný olup olmadýðý kontrol edilirse sonuç false olacaktýr.
```csharp
DateTime[] bankHols1 =
{
	new DateTime(2021, 1, 1)
};

DateTime[] bankHols2 =
{
	new DateTime(2021, 1, 1)
};
Console.WriteLine($"bn1==bh2? {bankHols1 == bankHols2}");
```
Burada dikkat edilmesi gereken önemli nokta Array'in referans tipli olmasýdýr. Deðer tipli olan DateTime tipine göre kýyaslama olsaydý true dönecektir. Value type kýyaslamasýnda referans olmadýðý için deðerler kýyaslanacaktýr. Referans deðer kýyaslamasýnda referanslar ayný memory location'ýnda olmadýðý için hiçbir zaman ayný olmayacaktýr. Ancak string ifadesi düþünüldüðünde referans tipli olmasýna raðmen deðer tipli gibi çalýþmakta. Burada Microsoft tarafýndan string'in davranýþý override edilmiþtir. Bunun için collection'larda **SequenceEqual** metodu kullanýlabilir.
```csharp
Console.WriteLine($"bn1==bh2? {bankHols1.SequenceEqual(bankHols2)}");
```
Buna benzer olarak yeni bir tane ayný referanstan alan bir deðiþken üretirsek eþitlik kontrolü true dönecektir. Bunun nedeni referanslarýn ayný olmasýndan kaynaklýdýr.
```csharp
DateTime[] bankHols3 = bankHols1;
```
Referans tipli deðiþkenlerin kýyaslanmasý hakkýnda daha detaylý bilgi için https://docs.microsoft.com/en-us/dotnet/api/system.object.equals?view=netcore-3.1 adresini inceleyebilirsiniz.
Array'lerde ilgili öðelere index yardýmýyla ulaþýlýr. Veriler ardýþýk tutulur ve herhangi bir veriye ulaþma tek bir iþlem (**O(1)**) gerektirir ve yeni öðe eklenemez.

Buna karþýn "List<DateTime> varName;" kullanýlarak istenilen öðe kadar ekleme yapýlabilir. 
Performans olarak list item incelendiðinde örnek vermek gerekirse RemoveAt metodu ile herhangi bir öðeyi silmek isteyelim. Bu da veri büyüdükçe daha çok vakit alacaktýr. (**O(N)**) Ancak herhangi bir öðeye eriþmek dizideki gibi O(1) iþlem gerektirecektir. Burada bir öðeye eriþmek bu kadar kýsayken neden silmesi bu kadar uzun sürsün ki diye düþünülebilir. Þu þekilde düþünüldüðünde fark ortaya çýkacaktýr. Örnek olarak bir milyon elemanlý bir listemiz olsun ve bunun son elemanýný çýkaralým. Burada listenin baþka bir elamaný için bir iþlem yapmayacaðýndan sadece tek bir iþlem gerçekleþecektir. Ancak ilk elemaný çýkardýðýmýzda ise diðer tüm elemanlarýn indexlerinin de deðiþmesi gerekmektedir. Burada *O(N)* kadar iþlem gerçekleþecektir.

Aþaðýdaki kod örneðinde RemoveAt **O(N²)** sayýda iþlem gerçekleþtirecektir.
```csharp
for (int i = lst.Count-1; i >0; i--)
{
    if (someExpression(anotherList[i]))
    {
        lst.RemoveAt(i);
    }
}
```
Ayný þekilde aþaðýdaki gibi **RemoveAll** metodunun içerisinde bir kullaným ile sayý **O(N)** e düþmüþ olacaktýr.
```csharp
lst.RemoveAll(x=>someExpression(x));
```

- OrderBy metodu genellikle O(nlog n) sayýda iþlem yapar. 
- ToList metodu enumerable gelen deðerleri *yeni bir instance* ile liste verir.
Listeyi Dictionary'e çevirmek için aþaðýdaki gibi function içerisinde key alanýna karþýlýk gelecek property set edilir.
```csharp
AllCountries.ToDictionary(x=>x.Code);
```
Dictionary için *TryGetValue()* metodu ile istenilen öðe **O(1)** notasyonuna göre çaðýrýlýr. Ancak case sensitive'dir. Bunu kaldýrmak için key deðerlerinin compare edilmesi gerekir.
```csharp
AllCountries.ToDictionary(x=>x.Code,StringComparer.OrdinalIgnoreCase);
```

**SortedDictionary**

Bir önceki örnekteki gibi direk olarak sorted dictionary'e çevirme iþlemi yoktur. Bunun için mevcut dictionary'i içeri alalým.
```csharp
var dict=AllCountries.ToDictionary(x=>x.Code,StringComparer.OrdinalIgnoreCase);
var sortedDict=new SortedDictionary<string,Country>(dict);
```

**SortedList**

SortedDictionary'e benzer. Fonksiyonel olarak aynýdýr. SortedList daha az memory kullanýr. SortedDictionary tarafýnda güncelleme yapmak (O(logn) vs O(n)) daha performanslýdýr. SortedList'de veriler daha önceden sýralanmýþtýr. SortedDictionary'de sýralanmamýþ þekilde durur.

Dictionary içerisinde key alanýnda custom type kullanmak için ilgili custom type'ýn **Equals** metodu override edilmesi gerekmektedir. Bununla birlikte **==** operatörünün de override edilmesi gerekir. Ancak bu þekilde kod yine istenilen þekilde çalýþmayacaktýr.
```csharp
public override bool Equals(object obj)
{
	if (obj is CountryCode other)
		return StringComparer.OrdinalIgnoreCase.Equals(this.Value, other.Value);
	return false;
}

public static bool operator ==(CountryCode lhs, CountryCode rhs)
{
	if (lhs != null)
		return lhs.Equals(rhs);
	else
		return rhs == null;
}
public static bool operator !=(CountryCode lhs, CountryCode rhs)
{
	return !(lhs == rhs);
}
```
Burada dictionary ekstra olarak GetHashCode'metoduna bakar. Son olarak aþaðýdaki kod eklenilir.
```csharp
	public override int GetHashCode() =>
		StringComparer.OrdinalIgnoreCase.GetHashCode(this.Value);
```

**Linked List**

 Öðe ekleme ve silme konusunda diðerlerine göre daha hýzlýdýr. Veriler ardýþýk olarak memory'de tutulur. Ýlgili next ve previous referanslarý silineceði ya da ekleneceði için hýzlý bir þekilde iþlem yapar. Silinen öðeler de direk olarak silinmez. Önce referanslar ayarlanýr ve boþta kalan öðe GB tarafýndan kaldýrýlýr. Herhangi bir öðeyi bulmak list'tekinin tam tersi olarak O(n) kadar iþleme neden olur. Aþaðýda öðe ekleme, n. elementi getirme ve listenin ortasýnda öðe iþlemi yazýlmýþtýr.

```csharp
LinkedList<T>.AddLast(variable);
LinkedList<T>.Remove(variable);

public static LinkedListNode<T> GetNthNode<T>(this LinkedList<T> lst, int n)
{
	LinkedListNode<T> currentNode = lst.First;
	for (int i = 0; i < n; i++)
		currentNode = currentNode.Next;
	return currentNode;
}

var insertBeforeNode = AllData.ItineraryBuilder.GetNthNode(selectedItinIndex);
AllData.ItineraryBuilder.AddBefore(insertBeforeNode, selectedCountry);
```

**Stack**

Last in first out. Add/remove yerine Pop/Push metotlarý kullanýlýr. Undo iþlemleri için uygundur.

**Queue**

First in first out. Enqueue/Dequeue. Peek metodu ile ilk sýradaki öðeyi getirir ancak silme iþlemi yapmaz.

**HashSet\<T\>**

Öðlerin unique olmasýný garantiler. Dictionary'e benzetilebilir ancak key alaný yoktur. Lookup'ý desteklemez. Dictionary içerisine ayný deðere sahip öðeler farklý key ile eklenebilir ancak hashset value'ye göre unique'liðe baktýðý için ayný öðeyi tekrar ekleme mümkün deðildir. Dictionary'de  ayný key eklemek exception fýrlatýrken hashset ignore ederek hata vermeyecektir.

**SortedSet**

Hashset gibidir sadece sýralanmýþ þekildedir. Ýlgili öðenin IComperable olmasý gerekir.

**UnionWith:** Ýlgili setleri distinct olarak birleþtirir.

**IntersectWith:** Ýki setin kesiþimini verir.

### ReadOnlyCollections
Bazen ilgili collection'larýn hiçbir þekilde baþka bir yerde deðiþmemesini isteriz. Bu durumda ReadOnlyCollection'lar kullanýlýr.
- **ReadOnlyDictionary<Key,Value>:** Aþaðýdaki gibi initialize edilir.
```csharp
new ReadOnlyDictionary<Key,Value>(dict);
```
List için ise ToList().**AsReadOnly()** ile **ReadOnlyCollection** dönen bir deðerimiz olacaktýr.
> **Not:** ReadOnlyCollection'larda instance alýrken baþka bir collection'ýn referans alýrsak bu durumda referanstaki bir güncelleme readonlycollection'da da gerçekleþmiþ olacaktýr. Normalde readonlycollection bu duruma izin vermezken instance ile referans verildiðinde bu durum aþýlmýþ olacaktýr. Bu durum için de immutablecollection'lar kullanýlýr

### ImmutableCollections

Örnek olarak *ImmutableArray\<Country\>* kullanýmý verilebilir. Benzer þekilde ToList().ToImmutableArray() metodu ile çevirme iþlemi gerçekleþir

ImmutableCollection'lar thread safe'dir.



### Concurrent Collections
https://app.pluralsight.com/library/courses/csharp-concurrent-collections/table-of-contents
Aþaðýdaki kodun sonucunda hep durmuþ yazabilir ve hatta  ya da ayný numarayla birçok kayýt dönebilir. Baþka bir çalýþmada hem nuri hem durmuþ karmaþýk þekilde de yazýlabilir ya da orders.Enqueue kýsmýnda "Destionation array was not long enough" hatasý verebilir. Bu aþamada concurrent collection'lar devreye girmektedir. Thread-safe kavramý burada daha rahat anlaþýlýr olacaktýr.
```csharp
static void PlaceOrders(Queue<string> orders, string customerName)
{
	for (int i = 0; i < 5; i++)
	{
		Thread.Sleep(1);
		string orderName = string.Format("{0} wants t-shirt {1}", customerName, i + 1);
		orders.Enqueue(orderName);
	}
}

static void RunProgramMultithreaded()
{
    var orders = new Queue<string>();
    Task task1 = Task.Run(() => PlaceOrders(orders, "Mark"));
    Task task2 = Task.Run(() => PlaceOrders(orders, "Ramdevi"));
    Task.WaitAll(task1, task2);

    foreach (string order in orders)
        Console.WriteLine("ORDER: " + order);
}
```


**ConcurrentQueue**

Burada hata olmadan çalýþtýrmak için queue yerine **ConcurrenQueue** kullanýlýr. Sonuç sýrasý karýþýk þekilde doðru sayýda tiþört ile dönecektir. 

TryDequeue metotu ile kuyruktan hata olmadan veri çýkarmayý mümkün kýlar. Eðer veri yoksa false döner.
Peek metodu yerine ise TryPeek metodu kullanýlýr.

#### Lock
ConcurrentQueue yerine Queue yine ayný þekilde **lock** ile kullanýlabilirdi. 
```csharp
		static object _lockObj = new object();
		static void PlaceOrders3(Queue<string> orders, string customerName)
		{
			for (int i = 0; i < 5; i++)
			{
				Thread.Sleep(1);
				string orderName = string.Format("{0} wants t-shirt {1}", customerName, i + 1);
				lock (_lockObj)
				{
					orders.Enqueue(orderName);
				}
			}
		}
```
Burada kullanýlan lock objesi ile veriye sadece tek bir thread'in eriþmesi mümkün kýlýnýr.

 **Peki lock varken neden Concurrent Collection'lara ihtiyaç duyuyoruz?** 

Lock mekanizmasý güzel olduðu kadar sorunlu olabilmekte. Thread'ler arasý kullanýlacak verinin olduðu her yerde lock eklenilmesi gerekli. Bu da çok büyük iþ süreçleri gerektirdiði yerlerde developer'ýn hatalý kod yazmasý ya da unutmasýna neden olabilir. Bunun yanýnda deadlock'lardan kaçýnmak da zordur. Küçük ölçekli kodlarda lock kullanmak önerilebilir ancak thread çoðaldýkça diðer thread'lerdeki bekleme süresi uzadýðý için kötü bir performans sergileyecektir(scalebility sorunu). 

Concurrent collection'lar lock mekanýzmasý da dahil olmak üzere thread safety için çeþitli thread synchronization tekniklerini kullanýr. Bunlardan bazýlarý *"Memory barriers, special atomic assembly instructions, mutex, semaphore, auto-reset event, reader-writer lock..."* .

Örnek olarak ConcurrentDictionary update edildiðinde istek gelir ve bir atomic operation içerisinde bu iþlemin baþka bir thread tarafýndan çalýþtýrýlýp çalýþtýrýlmadýðýna bakýlýr eðer çalýþtýrýlmýyorsa deðiþiklikler yapýlýr ve çalýþtýrýlýyorsa tekrar döngü baþa gelir. Bu tekniðe *Optimistic concurrency technique* denilir.

#### Race Conditions

Bir önceki örnekte concurrentqueue kullanýmý olduðunda farklý sýralamada verilerin geldiðini görmüþtük. Buradaki neden ise farklý threadlerin ayný anda ayný contexte ulaþmasýndan kaynaklý. Bu normal senaryolarda sorun olmayacaktýr ancak farklý senaryolarda çalýþma sýralamasýnýn önemli olduðu durumlarda bu önemli hale gelecektir.
```
ORDER: Mark wants t-shirt 1
ORDER: Ramdevi wants t-shirt 1
ORDER: Mark wants t-shirt 2
ORDER: Ramdevi wants t-shirt 2
ORDER: Mark wants t-shirt 3
ORDER: Ramdevi wants t-shirt 3
ORDER: Mark wants t-shirt 4
ORDER: Ramdevi wants t-shirt 4
ORDER: Mark wants t-shirt 5
ORDER: Ramdevi wants t-shirt 5
```
Daha detay bilgi için https://medium.com/@gokhansengun/race-condition-nedir-ve-nas%C4%B1l-%C3%B6nlenir-174e9ba3f567

Concurrent Collection'larýn sayýsý oldukça azdýr. Genel amaç için **ConcurrentDictionary\<TKey,TValue>** kullanýlýr. 

ProducerConsumer olarak gruplandýracak olursak: **ConcurrentQueue\<T>, Concurrent Stack\<T>, ConcurrentBag\<T>, BockingCollection\<T>,IProducerConsumerCollection\<T>**

Partitioner'lar ise : **Partitioner\<T>, OrderablePartitioner\<T>, Partitioner, EnumerablePartitionerOptions**

Partitioner'larý kýsaca özetlemek gerekirse. Foreach yerine Parallel.ForEach kullanýmý yaptýðýmý varsayalým. Döngüdeki her bir deðerin eriþilebilir bir thread'e verilmesi gerekmektedir. Burada partitioner'lar bunu yönetmede kullanýlýr.


### ConcurrentDictionary
Add ve Remove metotlarý gibi metotlar ConcurrentDictionarty'de olmadýðý için Try(GetValue,Add,Remove,Update) metotlarýyla birlikte GetOrAdd ve AddOrUpdate metotlarý kullanýlabilir.
Indexer ile veri güncelleme yapýlabilir ancak bu durumda farklý thread'lerin ayný veriyi override etme gibi bir durum söz konusu olabilir. Bu durumda TryUpdate("güncellenecek key","yeni deðer","eski deðer") þeklinde veri güncellemesi yapýlabilir.

Dictionary içerisindeki bir deðeri 1 arttýrdýðýmýzý varsayalým. Bu durumda aslýnda x=x+1 için bir geçici deðiþkende mevcut veri tutulur ve arttýrýlýr. Ancak bu durumda da race condition olabileceði için saðlýk bir metot olmayacaktýr. Bu durumda  **AddOrUpdate** metodu kullanýlýr. Dictionary'de ilgili key yoksa ekleme iþlemini kendisi yapacaktýr ve sonuç 2. parametre olan 1 deðerini dönecektir.
```csharp
int psStock = stock.AddOrUpdate("pluralsight", 1, (key, oldValue) => oldValue + 1);
```
Burada mevcut pluralsight deðerini stock["pluralsight"] olarak göstermeye çalýþtýðýmýzda yine race condition ile karþýlaþabiliriz. bunun yerine metotdan dönen yanýtý kullanarak daha doðru sonuç döndürmüþ oluruz.

- **Interlocked:** Lock yerine alternatif olarak kullanýlabilir. Farklý threadlerin paylaþýlmýþ deðiþkenlere yapacaðý deðiþikliklere karþý race condition'u engeller. 
https://www.dotnetperls.com/interlocked
```csharp
static int _value;

    static void Main()
    {
        Thread thread1 = new Thread(new ThreadStart(A));
        Thread thread2 = new Thread(new ThreadStart(A));
        thread1.Start();
        thread2.Start();
        thread1.Join();
        thread2.Join();
        Console.WriteLine(Program._value);
    }

    static void A()
    {
        // Add one.
        Interlocked.Add(ref Program._value, 1);
    }
```
> Output: 2

- **BlockingCollection:**Mevcut concurrent collection'larý sarmalar ve üzerine ek özellikler ekler. Add ve Take metotlarýyla ekleme ve çýkarma yapýlýr. 

FineGrainedLocking


#### Kaynak

https://app.pluralsight.com/library/courses/a19b0dc8-63a5-40c9-a60e-6674bd27f6ce/table-of-contents