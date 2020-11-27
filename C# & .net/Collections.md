A�a��daki gibi iki datetime listemiz olsun. Burada listelerin birbiriyle ayn� olup olmad��� kontrol edilirse sonu� false olacakt�r.
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
Burada dikkat edilmesi gereken �nemli nokta Array'in referans tipli olmas�d�r. De�er tipli olan DateTime tipine g�re k�yaslama olsayd� true d�necektir. Value type k�yaslamas�nda referans olmad��� i�in de�erler k�yaslanacakt�r. Referans de�er k�yaslamas�nda referanslar ayn� memory location'�nda olmad��� i�in hi�bir zaman ayn� olmayacakt�r. Ancak string ifadesi d���n�ld���nde referans tipli olmas�na ra�men de�er tipli gibi �al��makta. Burada Microsoft taraf�ndan string'in davran��� override edilmi�tir. Bunun i�in collection'larda **SequenceEqual** metodu kullan�labilir.
```csharp
Console.WriteLine($"bn1==bh2? {bankHols1.SequenceEqual(bankHols2)}");
```
Buna benzer olarak yeni bir tane ayn� referanstan alan bir de�i�ken �retirsek e�itlik kontrol� true d�necektir. Bunun nedeni referanslar�n ayn� olmas�ndan kaynakl�d�r.
```csharp
DateTime[] bankHols3 = bankHols1;
```
Referans tipli de�i�kenlerin k�yaslanmas� hakk�nda daha detayl� bilgi i�in https://docs.microsoft.com/en-us/dotnet/api/system.object.equals?view=netcore-3.1 adresini inceleyebilirsiniz.
Array'lerde ilgili ��elere index yard�m�yla ula��l�r. Veriler ard���k tutulur ve herhangi bir veriye ula�ma tek bir i�lem (**O(1)**) gerektirir ve yeni ��e eklenemez.

Buna kar��n "List<DateTime> varName;" kullan�larak istenilen ��e kadar ekleme yap�labilir. 
Performans olarak list item incelendi�inde �rnek vermek gerekirse RemoveAt metodu ile herhangi bir ��eyi silmek isteyelim. Bu da veri b�y�d�k�e daha �ok vakit alacakt�r. (**O(N)**) Ancak herhangi bir ��eye eri�mek dizideki gibi O(1) i�lem gerektirecektir. Burada bir ��eye eri�mek bu kadar k�sayken neden silmesi bu kadar uzun s�rs�n ki diye d���n�lebilir. �u �ekilde d���n�ld���nde fark ortaya ��kacakt�r. �rnek olarak bir milyon elemanl� bir listemiz olsun ve bunun son eleman�n� ��karal�m. Burada listenin ba�ka bir elaman� i�in bir i�lem yapmayaca��ndan sadece tek bir i�lem ger�ekle�ecektir. Ancak ilk eleman� ��kard���m�zda ise di�er t�m elemanlar�n indexlerinin de de�i�mesi gerekmektedir. Burada *O(N)* kadar i�lem ger�ekle�ecektir.

A�a��daki kod �rne�inde RemoveAt **O(N�)** say�da i�lem ger�ekle�tirecektir.
```csharp
for (int i = lst.Count-1; i >0; i--)
{
    if (someExpression(anotherList[i]))
    {
        lst.RemoveAt(i);
    }
}
```
Ayn� �ekilde a�a��daki gibi **RemoveAll** metodunun i�erisinde bir kullan�m ile say� **O(N)** e d��m�� olacakt�r.
```csharp
lst.RemoveAll(x=>someExpression(x));
```

- OrderBy metodu genellikle O(nlog n) say�da i�lem yapar. 
- ToList metodu enumerable gelen de�erleri *yeni bir instance* ile liste verir.
Listeyi Dictionary'e �evirmek i�in a�a��daki gibi function i�erisinde key alan�na kar��l�k gelecek property set edilir.
```csharp
AllCountries.ToDictionary(x=>x.Code);
```
Dictionary i�in *TryGetValue()* metodu ile istenilen ��e **O(1)** notasyonuna g�re �a��r�l�r. Ancak case sensitive'dir. Bunu kald�rmak i�in key de�erlerinin compare edilmesi gerekir.
```csharp
AllCountries.ToDictionary(x=>x.Code,StringComparer.OrdinalIgnoreCase);
```

**SortedDictionary**

Bir �nceki �rnekteki gibi direk olarak sorted dictionary'e �evirme i�lemi yoktur. Bunun i�in mevcut dictionary'i i�eri alal�m.
```csharp
var dict=AllCountries.ToDictionary(x=>x.Code,StringComparer.OrdinalIgnoreCase);
var sortedDict=new SortedDictionary<string,Country>(dict);
```

**SortedList**

SortedDictionary'e benzer. Fonksiyonel olarak ayn�d�r. SortedList daha az memory kullan�r. SortedDictionary taraf�nda g�ncelleme yapmak (O(logn) vs O(n)) daha performansl�d�r. SortedList'de veriler daha �nceden s�ralanm��t�r. SortedDictionary'de s�ralanmam�� �ekilde durur.

Dictionary i�erisinde key alan�nda custom type kullanmak i�in ilgili custom type'�n **Equals** metodu override edilmesi gerekmektedir. Bununla birlikte **==** operat�r�n�n de override edilmesi gerekir. Ancak bu �ekilde kod yine istenilen �ekilde �al��mayacakt�r.
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
Burada dictionary ekstra olarak GetHashCode'metoduna bakar. Son olarak a�a��daki kod eklenilir.
```csharp
	public override int GetHashCode() =>
		StringComparer.OrdinalIgnoreCase.GetHashCode(this.Value);
```

**Linked List**

 ��e ekleme ve silme konusunda di�erlerine g�re daha h�zl�d�r. Veriler ard���k olarak memory'de tutulur. �lgili next ve previous referanslar� silinece�i ya da eklenece�i i�in h�zl� bir �ekilde i�lem yapar. Silinen ��eler de direk olarak silinmez. �nce referanslar ayarlan�r ve bo�ta kalan ��e GB taraf�ndan kald�r�l�r. Herhangi bir ��eyi bulmak list'tekinin tam tersi olarak O(n) kadar i�leme neden olur. A�a��da ��e ekleme, n. elementi getirme ve listenin ortas�nda ��e i�lemi yaz�lm��t�r.

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

Last in first out. Add/remove yerine Pop/Push metotlar� kullan�l�r. Undo i�lemleri i�in uygundur.

**Queue**

First in first out. Enqueue/Dequeue. Peek metodu ile ilk s�radaki ��eyi getirir ancak silme i�lemi yapmaz.

**HashSet\<T\>**

��lerin unique olmas�n� garantiler. Dictionary'e benzetilebilir ancak key alan� yoktur. Lookup'� desteklemez. Dictionary i�erisine ayn� de�ere sahip ��eler farkl� key ile eklenebilir ancak hashset value'ye g�re unique'li�e bakt��� i�in ayn� ��eyi tekrar ekleme m�mk�n de�ildir. Dictionary'de  ayn� key eklemek exception f�rlat�rken hashset ignore ederek hata vermeyecektir.

**SortedSet**

Hashset gibidir sadece s�ralanm�� �ekildedir. �lgili ��enin IComperable olmas� gerekir.

**UnionWith:** �lgili setleri distinct olarak birle�tirir.

**IntersectWith:** �ki setin kesi�imini verir.

### ReadOnlyCollections
Bazen ilgili collection'lar�n hi�bir �ekilde ba�ka bir yerde de�i�memesini isteriz. Bu durumda ReadOnlyCollection'lar kullan�l�r.
- **ReadOnlyDictionary<Key,Value>:** A�a��daki gibi initialize edilir.
```csharp
new ReadOnlyDictionary<Key,Value>(dict);
```
List i�in ise ToList().**AsReadOnly()** ile **ReadOnlyCollection** d�nen bir de�erimiz olacakt�r.
> **Not:** ReadOnlyCollection'larda instance al�rken ba�ka bir collection'�n referans al�rsak bu durumda referanstaki bir g�ncelleme readonlycollection'da da ger�ekle�mi� olacakt�r. Normalde readonlycollection bu duruma izin vermezken instance ile referans verildi�inde bu durum a��lm�� olacakt�r. Bu durum i�in de immutablecollection'lar kullan�l�r

### ImmutableCollections

�rnek olarak *ImmutableArray\<Country\>* kullan�m� verilebilir. Benzer �ekilde ToList().ToImmutableArray() metodu ile �evirme i�lemi ger�ekle�ir

ImmutableCollection'lar thread safe'dir.



### Concurrent Collections
https://app.pluralsight.com/library/courses/csharp-concurrent-collections/table-of-contents
A�a��daki kodun sonucunda hep durmu� yazabilir ve hatta  ya da ayn� numarayla bir�ok kay�t d�nebilir. Ba�ka bir �al��mada hem nuri hem durmu� karma��k �ekilde de yaz�labilir ya da orders.Enqueue k�sm�nda "Destionation array was not long enough" hatas� verebilir. Bu a�amada concurrent collection'lar devreye girmektedir. Thread-safe kavram� burada daha rahat anla��l�r olacakt�r.
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

Burada hata olmadan �al��t�rmak i�in queue yerine **ConcurrenQueue** kullan�l�r. Sonu� s�ras� kar���k �ekilde do�ru say�da ti��rt ile d�necektir. 

TryDequeue metotu ile kuyruktan hata olmadan veri ��karmay� m�mk�n k�lar. E�er veri yoksa false d�ner.
Peek metodu yerine ise TryPeek metodu kullan�l�r.

#### Lock
ConcurrentQueue yerine Queue yine ayn� �ekilde **lock** ile kullan�labilirdi. 
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
Burada kullan�lan lock objesi ile veriye sadece tek bir thread'in eri�mesi m�mk�n k�l�n�r.

 **Peki lock varken neden Concurrent Collection'lara ihtiya� duyuyoruz?** 

Lock mekanizmas� g�zel oldu�u kadar sorunlu olabilmekte. Thread'ler aras� kullan�lacak verinin oldu�u her yerde lock eklenilmesi gerekli. Bu da �ok b�y�k i� s�re�leri gerektirdi�i yerlerde developer'�n hatal� kod yazmas� ya da unutmas�na neden olabilir. Bunun yan�nda deadlock'lardan ka��nmak da zordur. K���k �l�ekli kodlarda lock kullanmak �nerilebilir ancak thread �o�ald�k�a di�er thread'lerdeki bekleme s�resi uzad��� i�in k�t� bir performans sergileyecektir(scalebility sorunu). 

Concurrent collection'lar lock mekan�zmas� da dahil olmak �zere thread safety i�in �e�itli thread synchronization tekniklerini kullan�r. Bunlardan baz�lar� *"Memory barriers, special atomic assembly instructions, mutex, semaphore, auto-reset event, reader-writer lock..."* .

�rnek olarak ConcurrentDictionary update edildi�inde istek gelir ve bir atomic operation i�erisinde bu i�lemin ba�ka bir thread taraf�ndan �al��t�r�l�p �al��t�r�lmad���na bak�l�r e�er �al��t�r�lm�yorsa de�i�iklikler yap�l�r ve �al��t�r�l�yorsa tekrar d�ng� ba�a gelir. Bu tekni�e *Optimistic concurrency technique* denilir.

#### Race Conditions

Bir �nceki �rnekte concurrentqueue kullan�m� oldu�unda farkl� s�ralamada verilerin geldi�ini g�rm��t�k. Buradaki neden ise farkl� threadlerin ayn� anda ayn� contexte ula�mas�ndan kaynakl�. Bu normal senaryolarda sorun olmayacakt�r ancak farkl� senaryolarda �al��ma s�ralamas�n�n �nemli oldu�u durumlarda bu �nemli hale gelecektir.
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
Daha detay bilgi i�in https://medium.com/@gokhansengun/race-condition-nedir-ve-nas%C4%B1l-%C3%B6nlenir-174e9ba3f567

Concurrent Collection'lar�n say�s� olduk�a azd�r. Genel ama� i�in **ConcurrentDictionary\<TKey,TValue>** kullan�l�r. 

ProducerConsumer olarak grupland�racak olursak: **ConcurrentQueue\<T>, Concurrent Stack\<T>, ConcurrentBag\<T>, BockingCollection\<T>,IProducerConsumerCollection\<T>**

Partitioner'lar ise : **Partitioner\<T>, OrderablePartitioner\<T>, Partitioner, EnumerablePartitionerOptions**

Partitioner'lar� k�saca �zetlemek gerekirse. Foreach yerine Parallel.ForEach kullan�m� yapt���m� varsayal�m. D�ng�deki her bir de�erin eri�ilebilir bir thread'e verilmesi gerekmektedir. Burada partitioner'lar bunu y�netmede kullan�l�r.


### ConcurrentDictionary
Add ve Remove metotlar� gibi metotlar ConcurrentDictionarty'de olmad��� i�in Try(GetValue,Add,Remove,Update) metotlar�yla birlikte GetOrAdd ve AddOrUpdate metotlar� kullan�labilir.
Indexer ile veri g�ncelleme yap�labilir ancak bu durumda farkl� thread'lerin ayn� veriyi override etme gibi bir durum s�z konusu olabilir. Bu durumda TryUpdate("g�ncellenecek key","yeni de�er","eski de�er") �eklinde veri g�ncellemesi yap�labilir.

Dictionary i�erisindeki bir de�eri 1 artt�rd���m�z� varsayal�m. Bu durumda asl�nda x=x+1 i�in bir ge�ici de�i�kende mevcut veri tutulur ve artt�r�l�r. Ancak bu durumda da race condition olabilece�i i�in sa�l�k bir metot olmayacakt�r. Bu durumda  **AddOrUpdate** metodu kullan�l�r. Dictionary'de ilgili key yoksa ekleme i�lemini kendisi yapacakt�r ve sonu� 2. parametre olan 1 de�erini d�necektir.
```csharp
int psStock = stock.AddOrUpdate("pluralsight", 1, (key, oldValue) => oldValue + 1);
```
Burada mevcut pluralsight de�erini stock["pluralsight"] olarak g�stermeye �al��t���m�zda yine race condition ile kar��la�abiliriz. bunun yerine metotdan d�nen yan�t� kullanarak daha do�ru sonu� d�nd�rm�� oluruz.

- **Interlocked:** Lock yerine alternatif olarak kullan�labilir. Farkl� threadlerin payla��lm�� de�i�kenlere yapaca�� de�i�ikliklere kar�� race condition'u engeller. 
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

- **BlockingCollection:**Mevcut concurrent collection'lar� sarmalar ve �zerine ek �zellikler ekler. Add ve Take metotlar�yla ekleme ve ��karma yap�l�r. 

FineGrainedLocking


#### Kaynak

https://app.pluralsight.com/library/courses/a19b0dc8-63a5-40c9-a60e-6674bd27f6ce/table-of-contents