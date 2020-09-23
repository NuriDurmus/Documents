### Kaynak
https://www.dotnetcurry.com/patterns-practices/1412/dataflow-pattern-csharp-dotnet
https://www.blinkingcaret.com/2019/05/15/tpl-dataflow-in-net-core-in-depth-part-1/
https://csharppedia.com/en/tutorial/3110/task-parallel-library--tpl--dataflow-constructs

#### Ek kaynak
https://michaelscodingspot.com/pipeline-pattern-implementations-csharp/

### ActionBlock
Action gibi �al���r ve bunlar� bir veri seti halinde tutar. Geri d�n�� parametresi yoktur. ��erisindeki fonksiyonu asenkron olarak �al��t�r�r.
```csharp
var actionBlock = new ActionBlock<int>(n =>
{
    Task.Delay(500).Wait();
    Console.WriteLine(n); ;
});

for (int i = 0; i < 10; i++)
{
    actionBlock.Post(i);
    Console.WriteLine("Input count" + actionBlock.InputCount);
}
```



### TransformBlock
Transform block veri al�r ve veri d�ner. A�a��daki kodda int de�er al�p string de�er d�necektir. Burada di�erlerinden fakl� olarak parallelism eklenilerek multithread kullan�lm��t�r.

```csharp
ConcurrentBag<int> values = new ConcurrentBag<int>();
var transformBlock = new TransformBlock<int, string>(n =>
    {
        Task.Delay(500).Wait();
        values.Add(n);
        return n.ToString();
    },new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 8 });

for (int i = 0; i < 10; i++)
{
    transformBlock.Post(i);
    Console.WriteLine("Input count:"+transformBlock.InputCount);
}

for (int i = 0; i < 10; i++)
{
    Console.WriteLine("output count:"+transformBlock.OutputCount);
    var result = transformBlock.Receive();
    var listResult = 0;
    values.TryTake(out listResult);
    Console.WriteLine($"Result:{result}  Output count:{transformBlock.OutputCount} input count:{transformBlock.InputCount} list item: {listResult}");
}
```

<details>
<summary>Result</summary> 

```
Input count:1
Input count:1
Input count:2
Input count:2
Input count:1
Input count:2
Input count:2
Input count:2
Input count:2
Input count:3
output count:0
Result:0  Output count:7 input count:0 list item: 1
output count:7
Result:1  Output count:6 input count:0 list item: 5
output count:6
Result:2  Output count:5 input count:0 list item: 7
output count:5
Result:3  Output count:4 input count:0 list item: 6
output count:4
Result:4  Output count:3 input count:0 list item: 2
output count:3
Result:5  Output count:2 input count:0 list item: 3
output count:2
Result:6  Output count:1 input count:0 list item: 0
output count:1
Result:7  Output count:0 input count:0 list item: 4
output count:0
Result:8  Output count:1 input count:0 list item: 9
output count:1
Result:9  Output count:0 input count:0 list item: 8
Tamamland�
```
</details>

### BatchBlock
Verileri grup halinde almak i�in kullan�l�r.
A�a��daki �rnekte 3'erli olarak getirecektir ve son 9 de�eri son 3 l� sette tek de�er olaca��ndan gelmeyecektir ve uygulama receive k�s�mda beklemede kalacakt�r. Bu da program�n �al��mas�n� engelleyecektir. 
```csharp
var batchBlock= new BatchBlock<int>(3);

for (int i = 0; i < 10; i++)
{
    batchBlock.Post(i);
}

for (int i = 0; i < 5; i++)
{
    int[] result = batchBlock.Receive();
    Console.Write($"Received batch {i}:");
    foreach (var r in result)
    {
        Console.Write(r+" ");
    }
    Console.Write("\n");
}
```

```
Received batch 0:0 1 2
Received batch 1:3 4 5
Received batch 2:6 7 8
```

Bunun i�in batchBlock.**Complete();** ve TryReceive metotlar� kullan�l�r. Burada dikkat edilmesi gereken bir noktada complete edildikten sonra post edilenlerin ignore edilmesidir.

```csharp
var batchBlock = new BatchBlock<int>(3);

for (int i = 0; i < 10; i++)
{
    batchBlock.Post(i);
}

batchBlock.Complete();

batchBlock.Post(10);
for (int i = 0; i < 5; i++)
{
    int[] result;
    if (batchBlock.TryReceive(out result))
    {
        Console.Write($"Received batch {i}:");

        foreach (var r in result)
        {
            Console.Write(r + " ");
        }
        Console.Write("\n");
    }
    else
    {
        Console.WriteLine("The block finished");
        break;
    }
}
```

```
Received batch 0:0 1 2
Received batch 1:3 4 5
Received batch 2:6 7 8
Received batch 3:9
The block finished
```

### TransformManyBlock
Batch block'un tam tersidir. Tek bir mesaj al�r ve birden fazla item d�ner. 
Burada iki tane block var ancak bu ikisini birbirine ba�lamam�z gerekmektedir. Bunun i�in **LintTo** metodu ile source block'u consumer block'a iletebiliriz.


```csharp
public void Run()
{
    var transformManyBlock = new TransformManyBlock<int, string>(a => FindEvenNumbers(a));

    var printBlock = new ActionBlock<string>(a => Console.WriteLine($"Al�nan mesaj:{a}"));

    transformManyBlock.LinkTo(printBlock);

    for (int i = 0; i < 10; i++)
    {
        transformManyBlock.Post(i);
    }
    Console.WriteLine("Tamamland�.");
}

private IEnumerable<string> FindEvenNumbers(int number)
{
    for (int i = 0; i < number; i++)
    {
        if (i % 2 == 0)
        {
            yield return $"{number}:{i}";
        }

    }
}
```
<details>
<summary>Result</summary> 

```
Tamamland�.
Al�nan mesaj:1:0
Al�nan mesaj:2:0
Al�nan mesaj:3:0
Al�nan mesaj:3:2
Al�nan mesaj:4:0
Al�nan mesaj:4:2
Al�nan mesaj:5:0
Al�nan mesaj:5:2
Al�nan mesaj:5:4
Al�nan mesaj:6:0
Al�nan mesaj:6:2
Al�nan mesaj:6:4
Al�nan mesaj:7:0
Al�nan mesaj:7:2
Al�nan mesaj:7:4
Al�nan mesaj:7:6
Al�nan mesaj:8:0
Al�nan mesaj:8:2
Al�nan mesaj:8:4
Al�nan mesaj:8:6
Al�nan mesaj:9:0
Al�nan mesaj:9:2
Al�nan mesaj:9:4
Al�nan mesaj:9:6
Al�nan mesaj:9:8
```
</details>

Yine ayn� �ekilde multithread kullanmak i�in a�a��daki kod kullan�labilir.
```csharp
var transformManyBlock = new TransformManyBlock<int, string>(a => FindEvenNumbers(a),
    new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 5 });
```

### BufferBlock
Mesaj� t�ketici alas�ya kadar saklamada kullan�l�r. �rnek olarak birden fazla source block'lar (action, transformblock vs.) olsun. Bunlar�n iletilece�i consumer block'lar da tan�mlans�n. Bu iki block'lar aras�ndaki ili�ki buffer block �zerinde tan�mlan�r. Buffer block **BoundedCapacity=1** property de�eri ile al�nan mesajlar�n ka� consumer �zerine da��t�laca��n� ayarlar. Bir nevi load balancer g�revi g�r�r. Default olarak �ncelikle ilk consumer'a gider ve devam�nda consumer mesaj� kabul ederse di�erlerine ilk consumer mesaj� reddetmedi�i s�rece hi�bir zaman gitmez. Bu durumda t�m al�c�larda 'BoundedCapacity' de�erleri 1 olarak verildi�inde dengeli olarak mesajlar da��t�lacakt�r.
https://www.blinkingcaret.com/2019/06/05/tpl-dataflow-in-net-core-in-depth-part-2/

![Buffer Block](../Files/bufferBlock.png)


```csharp
var bufferBlock = new BufferBlock<int>();
for (int i = 0; i < 10; i++)
{
    bufferBlock.Post(i);
}
for (int i = 0; i < 10; i++)
{
    int result = bufferBlock.Receive();
    Console.WriteLine(result);
}
```
�lgili �rnekte iki action block mevcut ve buffer block ise gelen veriyi bu consumer block'lara aktaracakt�r.

```csharp
var bufferBlock = new BufferBlock<int>();

var a1 = new ActionBlock<int>(a =>
    {
        Console.WriteLine($"Mesaj {a} a1 taraf�ndan i�lenildi.");
        Task.Delay(300).Wait();
    });


var a2 = new ActionBlock<int>(a =>
{
    Console.WriteLine($"Mesaj {a} a2 taraf�ndan i�lenildi.");
    Task.Delay(300).Wait();
});

bufferBlock.LinkTo(a1);
bufferBlock.LinkTo(a2);

for (int i = 0; i < 10; i++)
{
    bufferBlock.Post(i);
}
```

<details>
<summary>Result</summary> 

```
Mesaj 0 a1 taraf�ndan i�lenildi.
Mesaj 1 a1 taraf�ndan i�lenildi.
Mesaj 2 a1 taraf�ndan i�lenildi.
Mesaj 3 a1 taraf�ndan i�lenildi.
Mesaj 4 a1 taraf�ndan i�lenildi.
Mesaj 5 a1 taraf�ndan i�lenildi.
Mesaj 6 a1 taraf�ndan i�lenildi.
Mesaj 7 a1 taraf�ndan i�lenildi.
Mesaj 8 a1 taraf�ndan i�lenildi.
Mesaj 9 a1 taraf�ndan i�lenildi.
```
</details>

Burada dikkat edilece�i �zere default ayarlar de�i�tirilmedi�i i�in her zaman ilk al�c� �al��t�r�lacakt�r. Bunun i�in consumer'lar �zerinden BoundedCapacity bilgisi set edilir.

```csharp
var a1 = new ActionBlock<int>(a =>
    {
        Console.WriteLine($"Mesaj {a} a1 taraf�ndan i�lenildi.");
        Task.Delay(300).Wait();
    },new ExecutionDataflowBlockOptions() { BoundedCapacity = 1});


var a2 = new ActionBlock<int>(a =>
{
    Console.WriteLine($"Mesaj {a} a2 taraf�ndan i�lenildi.");
    Task.Delay(300).Wait();
}, new ExecutionDataflowBlockOptions() { BoundedCapacity = 1 });
```

<details>
<summary>Result</summary> 

```
Mesaj 1 a2 taraf�ndan i�lenildi.
Mesaj 0 a1 taraf�ndan i�lenildi.
Mesaj 2 a2 taraf�ndan i�lenildi.
Mesaj 3 a1 taraf�ndan i�lenildi.
Mesaj 4 a2 taraf�ndan i�lenildi.
Mesaj 5 a1 taraf�ndan i�lenildi.
Mesaj 6 a2 taraf�ndan i�lenildi.
Mesaj 7 a1 taraf�ndan i�lenildi.
Mesaj 8 a2 taraf�ndan i�lenildi.
Mesaj 9 a1 taraf�ndan i�lenildi.
```
</details>

Burada a1 e 10 de�erin vermi� olsayd�k hepsini a1 den i�letmi� olacakt�. ��nk� g�nderilen mesaj kapasitesi ve alaca�� mesaj kapasitesi ayn�. �lk ba�ta s�rekli a1'e d��mesinin nedeni ise default de�erin -1 yani i�lemsel olarak sonsuz mesaj� alabiliyor olmas� anlam�na geliyordu. Bu y�zden hi�bir zaman a2'ye d��meyecekti.

Ayn� �ekilde buffer block i�in de BoundedCapacity ayarlayabilirdik burada BufferBlock sonsuz mesaj al�rsa request �ok olursa outofmemory hatas� al�nabilir. Ancak 1 olarak ayarlad���m�zda sadece bir mesaj� i�leyecekti. Bunun nedeni post metodu ile veri g�nderdi�imizde bu metot e�er kapasite dolmu�sa g�nderim reddedilecek ve false d�necektir. Bu durumda mesajlar� reject olmadan SendAsync() metodu arac�l���yla g�derebiliriz. Bu �ekilde BoundedCapacity 1 bile olsa bir mesaj g�nderildikten sonra bir di�erini g�nderebilir.

```csharp
var bufferBlock = new BufferBlock<int>(new DataflowBlockOptions() { BoundedCapacity = 1 });

var a1 = new ActionBlock<int>(a =>
    {
        Console.WriteLine($"Mesaj {a} a1 taraf�ndan i�lenildi.");
        Task.Delay(300).Wait();
    },new ExecutionDataflowBlockOptions() { BoundedCapacity = 1});


var a2 = new ActionBlock<int>(a =>
{
    Console.WriteLine($"Mesaj {a} a2 taraf�ndan i�lenildi.");
    Task.Delay(300).Wait();
}, new ExecutionDataflowBlockOptions() { BoundedCapacity = 1 });

bufferBlock.LinkTo(a1);
bufferBlock.LinkTo(a2);

for (int i = 0; i < 10; i++)
{
    bufferBlock.SendAsync(i).ContinueWith(a =>
    {
        if (a.Result)
        {
            Console.WriteLine($"{i} mesaj� kabul edildi.");
        }
        else
        {
            Console.WriteLine($"{i} mesaj� reddedildi.");
        }
    });
}
```

<details>
<summary>Result</summary> 

```
Tamamland�
1 mesaj� kabul edildi.
Mesaj 0 a1 taraf�ndan i�lenildi.
Mesaj 1 a2 taraf�ndan i�lenildi.
10 mesaj� kabul edildi.
10 mesaj� kabul edildi.
Mesaj 2 a1 taraf�ndan i�lenildi.
10 mesaj� kabul edildi.
10 mesaj� kabul edildi.
Mesaj 3 a2 taraf�ndan i�lenildi.
Mesaj 4 a1 taraf�ndan i�lenildi.
10 mesaj� kabul edildi.
Mesaj 5 a2 taraf�ndan i�lenildi.
10 mesaj� kabul edildi.
Mesaj 6 a1 taraf�ndan i�lenildi.
10 mesaj� kabul edildi.
Mesaj 7 a2 taraf�ndan i�lenildi.
10 mesaj� kabul edildi.
Mesaj 8 a1 taraf�ndan i�lenildi.
10 mesaj� kabul edildi.
Mesaj 9 a2 taraf�ndan i�lenildi.
```
</details>


### BroadcastBlock
Mesaj� birden fazla al�c�ya g�nderir. Burada mesajdan kas�t class instance'lar�d�r. G�nderilen mesaj birden fazla al�c�ya ula�t���nda her bir mesaj ayn� instance'� payla��r.

```csharp
var broadcastBlock = new BroadcastBlock<int>(a => a, new DataflowBlockOptions() { BoundedCapacity = 1 });

var a1 = new ActionBlock<int>(a =>
{
    Console.WriteLine($"Mesaj {a} a1 taraf�ndan i�lenildi.");
    Task.Delay(300).Wait();
}, new ExecutionDataflowBlockOptions() { BoundedCapacity = 1 });


var a2 = new ActionBlock<int>(a =>
{
    Console.WriteLine($"Mesaj {a} a2 taraf�ndan i�lenildi.");
    Task.Delay(300).Wait();
}, new ExecutionDataflowBlockOptions() { BoundedCapacity = 1 });

broadcastBlock.LinkTo(a1);
broadcastBlock.LinkTo(a2);

for (int i = 0; i < 10; i++)
{
    broadcastBlock.SendAsync(i).ContinueWith(a =>
    {
        if (a.Result)
        {
            Console.WriteLine($"{i} mesaj� kabul edildi.");
        }
        else
        {
            Console.WriteLine($"{i} mesaj� reddedildi.");
        }
    });
}
```
Burada t�m mesajlar�n ba�ar�l� bir �ekilde kabul edildi ancak sadece ikisi i�lenildir.

<details>
<summary>Result</summary> 

```
1 mesaj� kabul edildi.
Tamamland�
10 mesaj� kabul edildi.
10 mesaj� kabul edildi.
10 mesaj� kabul edildi.
10 mesaj� kabul edildi.
10 mesaj� kabul edildi.
10 mesaj� kabul edildi.
10 mesaj� kabul edildi.
10 mesaj� kabul edildi.
10 mesaj� kabul edildi.
Mesaj 0 a1 taraf�ndan i�lenildi.
Mesaj 0 a2 taraf�ndan i�lenildi.
Mesaj 9 a1 taraf�ndan i�lenildi.
Mesaj 9 a2 taraf�ndan i�lenildi.
```
</details>

Bir mesaj bir yerden reject ald���nda tekrar g�nderim yap�lmaz. E�er boundedcapacity de�erleri kald�r�l�rsa i�leyi� de de�i�ecektir.

```csharp
var broadcastBlock = new BroadcastBlock<int>(a => a);

var a1 = new ActionBlock<int>(a =>
{
    Console.WriteLine($"Mesaj {a} a1 taraf�ndan i�lenildi.");
    Task.Delay(300).Wait();
});


var a2 = new ActionBlock<int>(a =>
{
    Console.WriteLine($"Mesaj {a} a2 taraf�ndan i�lenildi.");
    Task.Delay(300).Wait();
});
```
<details>
<summary>Result</summary> 

```
10 mesaj� kabul edildi.
Tamamland�
8 mesaj� kabul edildi.
8 mesaj� kabul edildi.
8 mesaj� kabul edildi.
10 mesaj� kabul edildi.
9 mesaj� kabul edildi.
10 mesaj� kabul edildi.
10 mesaj� kabul edildi.
10 mesaj� kabul edildi.
10 mesaj� kabul edildi.
Mesaj 0 a2 taraf�ndan i�lenildi.
Mesaj 0 a1 taraf�ndan i�lenildi.
Mesaj 1 a1 taraf�ndan i�lenildi.
Mesaj 1 a2 taraf�ndan i�lenildi.
Mesaj 2 a1 taraf�ndan i�lenildi.
Mesaj 2 a2 taraf�ndan i�lenildi.
Mesaj 3 a1 taraf�ndan i�lenildi.
Mesaj 3 a2 taraf�ndan i�lenildi.
Mesaj 4 a1 taraf�ndan i�lenildi.
Mesaj 4 a2 taraf�ndan i�lenildi.
Mesaj 5 a2 taraf�ndan i�lenildi.
Mesaj 5 a1 taraf�ndan i�lenildi.
Mesaj 6 a2 taraf�ndan i�lenildi.
Mesaj 6 a1 taraf�ndan i�lenildi.
Mesaj 7 a1 taraf�ndan i�lenildi.
Mesaj 7 a2 taraf�ndan i�lenildi.
Mesaj 8 a1 taraf�ndan i�lenildi.
Mesaj 8 a2 taraf�ndan i�lenildi.
Mesaj 9 a1 taraf�ndan i�lenildi.
Mesaj 9 a2 taraf�ndan i�lenildi.
```
</details>

### JoinBlock

JoinBlock birden fazla blocklar� birle�tirerek ba�ka block'a aktar�m� m�mk�n k�lar.

```csharp
var broadcastBlock = new BroadcastBlock<int>(a => a);

var a1 = new TransformBlock<int,int>(a =>
{
    Console.WriteLine($"Mesaj {a} a1 taraf�ndan i�lenilmekte.");
    Task.Delay(300).Wait();
    return a;
});


var a2 = new TransformBlock<int,int>(a =>
{
    Console.WriteLine($"Mesaj {a} a2 taraf�ndan i�lenilmekte.");
    Task.Delay(300).Wait();
    return a;
});

broadcastBlock.LinkTo(a1);
broadcastBlock.LinkTo(a2);

var joinBlock = new JoinBlock<int, int>();
a1.LinkTo(joinBlock.Target1);
a2.LinkTo(joinBlock.Target2);
var printBlock = new ActionBlock<Tuple<int,int>>(a => Console.WriteLine($"{a} mesaj� print block taraf�ndan i�lenildi."));

joinBlock.LinkTo(printBlock);

for (int i = 0; i < 10; i++)
{
    broadcastBlock.SendAsync(i).ContinueWith(a =>
    {
        if (a.Result)
        {
            Console.WriteLine($"{i} mesaj� kabul edildi.");
        }
        else
        {
            Console.WriteLine($"{i} mesaj� reddedildi.");
        }
    });
}
```

<details>
<summary>Result</summary> 

```
Tamamland�
4 mesaj� kabul edildi.
4 mesaj� kabul edildi.
4 mesaj� kabul edildi.
4 mesaj� kabul edildi.
8 mesaj� kabul edildi.
10 mesaj� kabul edildi.
10 mesaj� kabul edildi.
10 mesaj� kabul edildi.
10 mesaj� kabul edildi.
10 mesaj� kabul edildi.
Mesaj 0 a1 taraf�ndan i�lenilmekte.
Mesaj 0 a2 taraf�ndan i�lenilmekte.
Mesaj 1 a2 taraf�ndan i�lenilmekte.
Mesaj 1 a1 taraf�ndan i�lenilmekte.
(0, 0) mesaj� print block taraf�ndan i�lenildi.
Mesaj 2 a2 taraf�ndan i�lenilmekte.
Mesaj 2 a1 taraf�ndan i�lenilmekte.
(1, 1) mesaj� print block taraf�ndan i�lenildi.
Mesaj 3 a2 taraf�ndan i�lenilmekte.
Mesaj 3 a1 taraf�ndan i�lenilmekte.
(2, 2) mesaj� print block taraf�ndan i�lenildi.
Mesaj 4 a2 taraf�ndan i�lenilmekte.
Mesaj 4 a1 taraf�ndan i�lenilmekte.
(3, 3) mesaj� print block taraf�ndan i�lenildi.
Mesaj 5 a1 taraf�ndan i�lenilmekte.
Mesaj 5 a2 taraf�ndan i�lenilmekte.
(4, 4) mesaj� print block taraf�ndan i�lenildi.
Mesaj 6 a2 taraf�ndan i�lenilmekte.
Mesaj 6 a1 taraf�ndan i�lenilmekte.
(5, 5) mesaj� print block taraf�ndan i�lenildi.
Mesaj 7 a1 taraf�ndan i�lenilmekte.
Mesaj 7 a2 taraf�ndan i�lenilmekte.
(6, 6) mesaj� print block taraf�ndan i�lenildi.
Mesaj 8 a1 taraf�ndan i�lenilmekte.
Mesaj 8 a2 taraf�ndan i�lenilmekte.
(7, 7) mesaj� print block taraf�ndan i�lenildi.
Mesaj 9 a1 taraf�ndan i�lenilmekte.
(8, 8) mesaj� print block taraf�ndan i�lenildi.
Mesaj 9 a2 taraf�ndan i�lenilmekte.
(9, 9) mesaj� print block taraf�ndan i�lenildi.
```
</details>

Multithread ve �al��ma s�relerinde biraz oynama yaparsak �al��ma s�ras� yine de�i�meyecektir.
```csharp
var broadcastBlock = new BroadcastBlock<int>(a => a);

var a1 = new TransformBlock<int,int>(a =>
{
    Console.WriteLine($"Mesaj {a} a1 taraf�ndan i�lenilmekte.");
    Task.Delay(300).Wait();
    if (a%2==0)
    {
        Task.Delay(300).Wait();
    }
    else
    {
        Task.Delay(50).Wait();
    }
    return -a;
},new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 3 });


var a2 = new TransformBlock<int,int>(a =>
{
    Console.WriteLine($"Mesaj {a} a2 taraf�ndan i�lenilmekte.");
    Task.Delay(150).Wait();
    return a;
}, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 8 });

broadcastBlock.LinkTo(a1);
broadcastBlock.LinkTo(a2);

var joinBlock = new JoinBlock<int, int>();
a1.LinkTo(joinBlock.Target1);
a2.LinkTo(joinBlock.Target2);
var printBlock = new ActionBlock<Tuple<int,int>>(a => Console.WriteLine($"{a} mesaj� print block taraf�ndan i�lenildi. De�erler toplam� her zaman 0 d�necektir: {a.Item1+a.Item2}"));

joinBlock.LinkTo(printBlock);

for (int i = 0; i < 10; i++)
{
    broadcastBlock.SendAsync(i).ContinueWith(a =>
    {
        if (a.Result)
        {
            Console.WriteLine($"{i} mesaj� kabul edildi.");
        }
        else
        {
            Console.WriteLine($"{i} mesaj� reddedildi.");
        }
    });
}

```

<details>
<summary>Result</summary> 

```
6 mesaj� kabul edildi.
Tamamland�
10 mesaj� kabul edildi.
4 mesaj� kabul edildi.
6 mesaj� kabul edildi.
6 mesaj� kabul edildi.
10 mesaj� kabul edildi.
10 mesaj� kabul edildi.
10 mesaj� kabul edildi.
10 mesaj� kabul edildi.
10 mesaj� kabul edildi.
Mesaj 0 a2 taraf�ndan i�lenilmekte.
Mesaj 2 a2 taraf�ndan i�lenilmekte.
Mesaj 1 a2 taraf�ndan i�lenilmekte.
Mesaj 3 a2 taraf�ndan i�lenilmekte.
Mesaj 4 a2 taraf�ndan i�lenilmekte.
Mesaj 0 a1 taraf�ndan i�lenilmekte.
Mesaj 1 a1 taraf�ndan i�lenilmekte.
Mesaj 2 a1 taraf�ndan i�lenilmekte.
Mesaj 5 a2 taraf�ndan i�lenilmekte.
Mesaj 6 a2 taraf�ndan i�lenilmekte.
Mesaj 7 a2 taraf�ndan i�lenilmekte.
Mesaj 8 a2 taraf�ndan i�lenilmekte.
Mesaj 9 a2 taraf�ndan i�lenilmekte.
Mesaj 3 a1 taraf�ndan i�lenilmekte.
Mesaj 4 a1 taraf�ndan i�lenilmekte.
Mesaj 5 a1 taraf�ndan i�lenilmekte.
(0, 0) mesaj� print block taraf�ndan i�lenildi. De�erler toplam� her zaman 0 d�necektir: 0
(-1, 1) mesaj� print block taraf�ndan i�lenildi. De�erler toplam� her zaman 0 d�necektir: 0
(-2, 2) mesaj� print block taraf�ndan i�lenildi. De�erler toplam� her zaman 0 d�necektir: 0
Mesaj 6 a1 taraf�ndan i�lenilmekte.
(-3, 3) mesaj� print block taraf�ndan i�lenildi. De�erler toplam� her zaman 0 d�necektir: 0
Mesaj 7 a1 taraf�ndan i�lenilmekte.
Mesaj 8 a1 taraf�ndan i�lenilmekte.
(-4, 4) mesaj� print block taraf�ndan i�lenildi. De�erler toplam� her zaman 0 d�necektir: 0
(-5, 5) mesaj� print block taraf�ndan i�lenildi. De�erler toplam� her zaman 0 d�necektir: 0
Mesaj 9 a1 taraf�ndan i�lenilmekte.
(-6, 6) mesaj� print block taraf�ndan i�lenildi. De�erler toplam� her zaman 0 d�necektir: 0
(-7, 7) mesaj� print block taraf�ndan i�lenildi. De�erler toplam� her zaman 0 d�necektir: 0
(-8, 8) mesaj� print block taraf�ndan i�lenildi. De�erler toplam� her zaman 0 d�necektir: 0
(-9, 9) mesaj� print block taraf�ndan i�lenildi. De�erler toplam� her zaman 0 d�necektir: 0
```
</details>

### BatchedJoinBlock
JoingBlock gibi �al���r ama BatchedBlock gibi birden fazla ��eyi toplu halde alarak i�lem yapar. Ancak burada s�ralama her �al��mada farkl� �al��acakt�r.
```csharp
var broadcastBlock = new BroadcastBlock<int>(a => a);

var a1 = new TransformBlock<int, int>(a =>
{
    Console.WriteLine($"Mesaj {a} a1 taraf�ndan i�lenilmekte.");
    Task.Delay(300).Wait();
    if (a % 2 == 0)
    {
        Task.Delay(300).Wait();
    }
    else
    {
        Task.Delay(50).Wait();
    }
    return -a;
}, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 3 });


var a2 = new TransformBlock<int, int>(a =>
{
    Console.WriteLine($"Mesaj {a} a2 taraf�ndan i�lenilmekte.");
    Task.Delay(150).Wait();
    return a;
}, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 3 });

broadcastBlock.LinkTo(a1);
broadcastBlock.LinkTo(a2);

var joinBlock = new BatchedJoinBlock<int, int>(3);
a1.LinkTo(joinBlock.Target1);
a2.LinkTo(joinBlock.Target2);
var printBlock = new ActionBlock<Tuple<IList<int>, IList<int>>>(a => Console.WriteLine($"{a} mesaj� print block taraf�ndan i�lenildi.[{string.Join(',', a.Item1)}] , [{string.Join(',', a.Item2)}]"));

joinBlock.LinkTo(printBlock);

for (int i = 0; i < 10; i++)
{
    broadcastBlock.SendAsync(i).ContinueWith(a =>
    {
        if (a.Result)
        {
            Console.WriteLine($"{i} mesaj� kabul edildi.");
        }
        else
        {
            Console.WriteLine($"{i} mesaj� reddedildi.");
        }
    });
}
```

<details>
<summary>Result</summary> 

```
2 mesaj� kabul edildi.
4 mesaj� kabul edildi.
3 mesaj� kabul edildi.
6 mesaj� kabul edildi.
6 mesaj� kabul edildi.
9 mesaj� kabul edildi.
Tamamland�
10 mesaj� kabul edildi.
10 mesaj� kabul edildi.
10 mesaj� kabul edildi.
10 mesaj� kabul edildi.
Mesaj 0 a2 taraf�ndan i�lenilmekte.
Mesaj 1 a2 taraf�ndan i�lenilmekte.
Mesaj 2 a2 taraf�ndan i�lenilmekte.
Mesaj 0 a1 taraf�ndan i�lenilmekte.
Mesaj 2 a1 taraf�ndan i�lenilmekte.
Mesaj 1 a1 taraf�ndan i�lenilmekte.
Mesaj 3 a2 taraf�ndan i�lenilmekte.
Mesaj 4 a2 taraf�ndan i�lenilmekte.
Mesaj 5 a2 taraf�ndan i�lenilmekte.
Mesaj: [] , [0,1,2]
Mesaj 6 a2 taraf�ndan i�lenilmekte.
Mesaj 7 a2 taraf�ndan i�lenilmekte.
Mesaj 8 a2 taraf�ndan i�lenilmekte.
Mesaj: [] , [3,4,5]
Mesaj 3 a1 taraf�ndan i�lenilmekte.
Mesaj 9 a2 taraf�ndan i�lenilmekte.
Mesaj: [] , [6,7,8]
Mesaj 4 a1 taraf�ndan i�lenilmekte.
Mesaj 5 a1 taraf�ndan i�lenilmekte.
Mesaj: [0,-1,-2] , []
Mesaj 6 a1 taraf�ndan i�lenilmekte.
Mesaj 7 a1 taraf�ndan i�lenilmekte.
Mesaj 8 a1 taraf�ndan i�lenilmekte.
Mesaj: [-3,-4] , [9]
Mesaj 9 a1 taraf�ndan i�lenilmekte.
Mesaj: [-5,-6,-7] , []
```
</details>

### WriteOnceBlock
Ad�ndan da anla��laca�� gibi tek bir mesaj� kabul eder ve di�erlerini reddeder ve ayn� yan�t� d�ner.

```csharp
var block = new WriteOnceBlock<int>(a => a);
for (int i = 0; i < 10; i++)
{
    if (block.Post(i))
    {
        Console.WriteLine($"Mesaj {i} kabul edildi");
    }
    else
    {
        Console.WriteLine($"Mesaj {i} reddedildi");
    }
}
for (int i = 0; i < 15; i++)
{
    if (block.TryReceive(out var ret))
    {
        Console.WriteLine($"Mesaj {ret} kabul edildi. Iteration {i}");
    }
    else
    {
        Console.WriteLine("Mesaj al�namad�.");
    }
}
```
<details>
<summary>Result</summary> 

```
Mesaj 0 kabul edildi
Mesaj 1 reddedildi
Mesaj 2 reddedildi
Mesaj 3 reddedildi
Mesaj 4 reddedildi
Mesaj 5 reddedildi
Mesaj 6 reddedildi
Mesaj 7 reddedildi
Mesaj 8 reddedildi
Mesaj 9 reddedildi
Mesaj 0 kabul edildi. Iteration 0
Mesaj 0 kabul edildi. Iteration 1
Mesaj 0 kabul edildi. Iteration 2
Mesaj 0 kabul edildi. Iteration 3
Mesaj 0 kabul edildi. Iteration 4
Mesaj 0 kabul edildi. Iteration 5
Mesaj 0 kabul edildi. Iteration 6
Mesaj 0 kabul edildi. Iteration 7
Mesaj 0 kabul edildi. Iteration 8
Mesaj 0 kabul edildi. Iteration 9
Mesaj 0 kabul edildi. Iteration 10
Mesaj 0 kabul edildi. Iteration 11
Mesaj 0 kabul edildi. Iteration 12
Mesaj 0 kabul edildi. Iteration 13
Mesaj 0 kabul edildi. Iteration 14
Tamamland�
```
</details>

E�er receive metodu yerine ba�ka bir block'a g�nderim yapacak olsayd�k sadece tek bir sefer �al��t���n� g�rebiliriz.
```csharp
var block = new WriteOnceBlock<int>(a => a);
var print = new ActionBlock<int>(a => Console.WriteLine($"Mesaj {a} kabul edildi."));
for (int i = 0; i < 10; i++)
{
    if (block.Post(i))
    {
        Console.WriteLine($"Mesaj {i} kabul edildi");
    }
    else
    {
        Console.WriteLine($"Mesaj {i} reddedildi");
    }
}
block.LinkTo(print);
```

<details>
<summary>Result</summary> 

```
Mesaj 0 kabul edildi
Mesaj 1 reddedildi
Mesaj 2 reddedildi
Mesaj 3 reddedildi
Mesaj 4 reddedildi
Mesaj 5 reddedildi
Mesaj 6 reddedildi
Mesaj 7 reddedildi
Mesaj 8 reddedildi
Mesaj 9 reddedildi
Tamamland�
Mesaj 0 kabul edildi.
```
</details>

### Completion
BroadcastBlock �rne�indeki sendasync metodunu a�a��daki gibi de�i�tirelim. Bu durumda Console.ReadLine kodunu kald�rd���m�zda ilgili mesaj bilgileri ekrana yaz�lmayacakt�r. Bunun nedeni ilgili kod blo�unun farkl� thread'de �al��mas�d�r ve ReadLine ile bir bekleme yapt���m�z i�in sonu�lar� g�r�nt�leyebiliyorduk ancak bu kodu kald�rd���m�zda kodu bekleten bir �ey olmad��� i�in sonu� ekrana yans�madan Main metodu tamamlanm�� olacakt�r. Bunu ��zmek i�in ilgili thread'de �al��an kodun tamamlanmas�n� beklememiz gerekmektedir.
```csharp
var broadcastBlock = new BroadcastBlock<int>(a => a);

var a1 = new ActionBlock<int>(a =>
{
    Console.WriteLine($"Mesaj {a} a1 taraf�ndan i�lenildi.");
    Task.Delay(300).Wait();
});


var a2 = new ActionBlock<int>(a =>
{
    Console.WriteLine($"Mesaj {a} a2 taraf�ndan i�lenildi.");
    Task.Delay(300).Wait();
});

broadcastBlock.LinkTo(a1);
broadcastBlock.LinkTo(a2);

for (int i = 0; i < 10; i++)
{
    await broadcastBlock.SendAsync(i);
}
Console.WriteLine("Tamamland�");
//Console.ReadLine();
```


```
Tamamland�

C:\Users\Nuri\...\TPL DataFlow\DataDlow\DataDlow\bin\Debug\netcoreapp3.1\DataDlow.exe (process 18700) exited with code 0.
To automatically close the console when debugging stops, enable Tools->Options->Debugging->Automatically close the console when debugging stops.
Press any key to close this window . .

```

Block'un s�re�lerinin tamamlan�p tamamlanmad���n� ��renmek i�in block i�erisinde mesaj�n olup olmad���na ya da completed eventi kontrol edilir. Bunun i�in linkto i�erisinde **PropagateCompletion** true olarak set edilmeli ve *broadcastBlock.Complete(); finalBlock.Completion.Wait();* metotlar� �a��r�lmal�d�r.
```csharp
public static class LinktoWithPropagationExtension
{
    public static IDisposable LinkToWithPropagation<T>(this ISourceBlock<T> source,ITargetBlock<T> target)
    {
        return source.LinkTo(target, new DataflowLinkOptions()
        {
            PropagateCompletion = true
        });
    }
}



var broadcastBlock = new BroadcastBlock<int>(a => a);

var a1 = new TransformBlock<int, int>(a =>
{
    Console.WriteLine($"Mesaj {a} a1 taraf�ndan i�lenildi.");
    Task.Delay(300).Wait();
    return -a;
});


var a2 = new TransformBlock<int, int>(a =>
{
    Console.WriteLine($"Mesaj {a} a2 taraf�ndan i�lenildi.");
    Task.Delay(300).Wait();
    return a;
});


var joinBlock = new JoinBlock<int, int>();
a1.LinkToWithPropagation(joinBlock.Target1);
a2.LinkToWithPropagation(joinBlock.Target2);

broadcastBlock.LinkToWithPropagation(a1);
broadcastBlock.LinkToWithPropagation(a2);

var finalBlock = new ActionBlock<Tuple<int, int>>(a =>
    {
        Console.WriteLine($"{a.Item1}: t�m consumer'lar taraf�ndan i�lenildi");
    });

joinBlock.LinkToWithPropagation(finalBlock);

for (int i = 0; i < 10; i++)
{
    await broadcastBlock.SendAsync(i);
}

broadcastBlock.Complete();
finalBlock.Completion.Wait();

```
### Append
Append ba�lanan consumer ili�kisini �nceliklendirmek ya da kald�rmak i�in kullan�l�r. A�a��daki bufferblock i�in g�r�lece�i �zere sadece a1 mesajlar� gelmektedir.

```csharp
var bufferBlock = new BufferBlock<int>();

var a1 = new ActionBlock<int>(a =>
{
    Console.WriteLine($"Mesaj {a} a1 taraf�ndan i�lenildi.");
});


var a2 = new ActionBlock<int>(a =>
{
    Console.WriteLine($"Mesaj {a} a2 taraf�ndan i�lenildi.");
});

bufferBlock.LinkTo(a1);
bufferBlock.LinkTo(a2);

for (int i = 0; i < 10; i++)
{
    await bufferBlock.SendAsync(i);
}
```

<details>
<summary>Result</summary> 

```
Tamamland�
Mesaj 0 a1 taraf�ndan i�lenildi.
Mesaj 1 a1 taraf�ndan i�lenildi.
Mesaj 2 a1 taraf�ndan i�lenildi.
Mesaj 3 a1 taraf�ndan i�lenildi.
Mesaj 4 a1 taraf�ndan i�lenildi.
Mesaj 5 a1 taraf�ndan i�lenildi.
Mesaj 6 a1 taraf�ndan i�lenildi.
Mesaj 7 a1 taraf�ndan i�lenildi.
Mesaj 8 a1 taraf�ndan i�lenildi.
Mesaj 9 a1 taraf�ndan i�lenildi.
```
</details>


Append false yap�l�rsa a1 den �nce a2 ili�kisi ba�lanacakt�r. Bu �ekilde sonu�lar�n hepsi a2 taraf�ndan �al��t�r�lacakt�r.

```csharp
bufferBlock.LinkTo(a2,new DataflowLinkOptions() { Append = false });
```
<details>
<summary>Result</summary> 

```
Tamamland�
Mesaj 0 a2 taraf�ndan i�lenildi.
Mesaj 1 a2 taraf�ndan i�lenildi.
Mesaj 2 a2 taraf�ndan i�lenildi.
Mesaj 3 a2 taraf�ndan i�lenildi.
Mesaj 4 a2 taraf�ndan i�lenildi.
Mesaj 5 a2 taraf�ndan i�lenildi.
Mesaj 6 a2 taraf�ndan i�lenildi.
Mesaj 7 a2 taraf�ndan i�lenildi.
Mesaj 8 a2 taraf�ndan i�lenildi.
Mesaj 9 a2 taraf�ndan i�lenildi.
```
</details>

### MaxMessages
Block taraf�ndan al�nacak mesajlar� s�n�rlamak i�in kullan�l�r. A�a��daki �rnekte a2 i�in maksimum 5 mesaj al�n�r.

```csharp
bufferBlock.LinkTo(a2, new DataflowLinkOptions()
{
    Append = false,
    MaxMessages = 5
});
```

<details>
<summary>Result</summary> 

```
Tamamland�
Mesaj 5 a1 taraf�ndan i�lenildi.
Mesaj 0 a2 taraf�ndan i�lenildi.
Mesaj 1 a2 taraf�ndan i�lenildi.
Mesaj 2 a2 taraf�ndan i�lenildi.
Mesaj 3 a2 taraf�ndan i�lenildi.
Mesaj 4 a2 taraf�ndan i�lenildi.
Mesaj 6 a1 taraf�ndan i�lenildi.
Mesaj 7 a1 taraf�ndan i�lenildi.
Mesaj 8 a1 taraf�ndan i�lenildi.
Mesaj 9 a1 taraf�ndan i�lenildi.
```

</details> 

### Mesaage Filtering

```csharp
bufferBlock.LinkTo(a1, a => a % 2 == 0);
```

```
Tamamland�
Mesaj 0 a2 taraf�ndan i�lenildi.
Mesaj 1 a2 taraf�ndan i�lenildi.
Mesaj 2 a2 taraf�ndan i�lenildi.
Mesaj 3 a2 taraf�ndan i�lenildi.
Mesaj 4 a2 taraf�ndan i�lenildi.
```
Sonu� ne yaz�kki istedi�imiz gibi olmad�. Bunun nedeni bir �nceki kodda da anla��laca�� �zere ilk 5 mesaj� a2 i�liyor. a1 taraf�na 5 msaj� geliyor ve filtreden ge�emiyor ve ge�emedi�i i�in beklemede kal�yor ve dolay�s�yla a1 tamamlanam�yor.

Bunun i�in a�a��daki gibi NullTarget DataFlowBlock eklenebilir. Bu bize reddidilmi� olan block'lar i�in tamamlanabilir bir block sa�lam�� olacakt�r. Bu block'u da a�a��daki gibi LinkTo ile ba�layal�m
```csharp
bufferBlock.LinkTo(a1, a => a % 2 == 0);
bufferBlock.LinkTo(a2, new DataflowLinkOptions()
{
    Append = false,
    MaxMessages = 4
});

bufferBlock.LinkTo(DataflowBlock.NullTarget<int>());
for (int i = 0; i < 10; i++)
{
    await bufferBlock.SendAsync(i);
}
```

```
Tamamland�
Mesaj 4 a1 taraf�ndan i�lenildi.
Mesaj 0 a2 taraf�ndan i�lenildi.
Mesaj 1 a2 taraf�ndan i�lenildi.
Mesaj 2 a2 taraf�ndan i�lenildi.
Mesaj 3 a2 taraf�ndan i�lenildi.
Mesaj 6 a1 taraf�ndan i�lenildi.
Mesaj 8 a1 taraf�ndan i�lenildi.
```
NullTarget yerine herhangi bir action block koydu�umuzda bu block reddedilen mesajlar� alacakt�r.

```csharp
bufferBlock.LinkTo(new ActionBlock<int>(a => Console.WriteLine($"{a} mesaj� reddedildi.")));
```

```
Tamamland�
Mesaj 4 a1 taraf�ndan i�lenildi.
5 mesaj� reddedildi.
7 mesaj� reddedildi.
9 mesaj� reddedildi.
Mesaj 0 a2 taraf�ndan i�lenildi.
Mesaj 1 a2 taraf�ndan i�lenildi.
Mesaj 2 a2 taraf�ndan i�lenildi.
Mesaj 3 a2 taraf�ndan i�lenildi.
Mesaj 6 a1 taraf�ndan i�lenildi.
Mesaj 8 a1 taraf�ndan i�lenildi.
```

### Multiple Producers
Daha �nceki �rneklerde birden fazla consumer ve bir producer vard�.
A�a��daki gibi LinkTo ile iki producer'� ayn� consumer'a aktarabiliriz.
```csharp
var producer1 = new TransformBlock<string,string>(a =>
{
    Task.Delay(150).Wait();
    return a;
});

var producer2 = new TransformBlock<string, string>(a =>
{
    Task.Delay(300).Wait();
    return a;
});

var printBlock = new ActionBlock<string>(n => Console.WriteLine(n));
producer1.LinkTo(printBlock);
producer2.LinkTo(printBlock);

for (int i = 0; i < 10; i++)
{
    producer1.Post($"Producer 1 mesaj�: {i}");
    producer2.Post($"Producer 2 mesaj�: {i}");
}
```

<details>
<summary>Result</summary> 

```
Tamamland�
Producer 1 mesaj�: 0
Producer 2 mesaj�: 0
Producer 1 mesaj�: 1
Producer 1 mesaj�: 2
Producer 2 mesaj�: 1
Producer 1 mesaj�: 3
Producer 1 mesaj�: 4
Producer 2 mesaj�: 2
Producer 1 mesaj�: 5
Producer 1 mesaj�: 6
Producer 2 mesaj�: 3
Producer 1 mesaj�: 7
Producer 1 mesaj�: 8
Producer 2 mesaj�: 4
Producer 1 mesaj�: 9
Producer 2 mesaj�: 5
Producer 2 mesaj�: 6
Producer 2 mesaj�: 7
Producer 2 mesaj�: 8
Producer 2 mesaj�: 9
```

</details>

Kod i�erisine completion metotlar�n� ekleyelim. Burada completion i�in olu�turdu�umuz extension metot ile PropagateCompletion de�erini g�ncelledi�imizi de unutmayal�m.

```csharp
var printBlock = new ActionBlock<string>(n => Console.WriteLine(n));
producer1.LinkToWithPropagation(printBlock);
producer2.LinkToWithPropagation(printBlock);

for (int i = 0; i < 10; i++)
{
    producer1.Post($"Producer 1 mesaj�: {i}");
    producer2.Post($"Producer 2 mesaj�: {i}");
}
producer1.Complete();
producer2.Complete();
printBlock.Completion.Wait();
```
Sonu� ne yaz�kki istedi�imiz gibi olmad�. Baz� mesajlar eksik kald�. Bunun nedeni producer1 in tamamland� komutunu g�ndermesiyle consumer'�n da tamamlanmas�. Burada s�re� tek y�nl� oldu�u i�in consumer producer2 ye tamamlan�p tamamlanmad���n� sormad���ndan tamamland� olarak kabul edip yoluna devam etmekte.

<details>
<summary>Result</summary> 

```
Producer 1 mesaj�: 0
Producer 1 mesaj�: 1
Producer 2 mesaj�: 0
Producer 1 mesaj�: 2
Producer 2 mesaj�: 1
Producer 1 mesaj�: 3
Producer 1 mesaj�: 4
Producer 2 mesaj�: 2
Producer 1 mesaj�: 5
Producer 1 mesaj�: 6
Producer 2 mesaj�: 3
Producer 1 mesaj�: 7
Producer 1 mesaj�: 8
Producer 2 mesaj�: 4
Producer 1 mesaj�: 9
Tamamland�
```
</details>

TPL Dataflow'u bir push mimarisidir. Producer consumer'� bilir ancak tersi m�mk�n de�ildir. Bu y�zden PropagateCompletion de�eri default olarak false'dur. Buradaki senaryo i�in TPL taraf�nda haz�r bir ��z�m olmasa da Task.WhenAll ile her iki completion'�n bitti�i an� bulabilir ve printBlock'un da complete eventi ile s�reci tamamlam�� oluruz. Burada LinkTo k�sm� eski haline getirilmi� yani propagation'lar kald�r�lm��t�r. 

```csharp
var printBlock = new ActionBlock<string>(n => Console.WriteLine(n));
producer1.LinkTo(printBlock);
producer2.LinkTo(printBlock);

for (int i = 0; i < 10; i++)
{
    producer1.Post($"Producer 1 mesaj�: {i}");
    producer2.Post($"Producer 2 mesaj�: {i}");
}
await Task.WhenAll(new[] { producer1.Completion, producer2.Completion }).ContinueWith(a=>printBlock.Complete());
printBlock.Completion.Wait();
```

<details>
<summary>Result</summary> 

```
Tamamland�
Producer 1 mesaj�: 0
Producer 2 mesaj�: 0
Producer 1 mesaj�: 1
Producer 1 mesaj�: 2
Producer 2 mesaj�: 1
Producer 1 mesaj�: 3
Producer 1 mesaj�: 4
Producer 2 mesaj�: 2
Producer 1 mesaj�: 5
Producer 1 mesaj�: 6
Producer 2 mesaj�: 3
Producer 1 mesaj�: 7
Producer 1 mesaj�: 8
Producer 2 mesaj�: 4
Producer 1 mesaj�: 9
Producer 2 mesaj�: 5
Producer 2 mesaj�: 6
Producer 2 mesaj�: 7
Producer 2 mesaj�: 8
Producer 2 mesaj�: 9
```

</details>

```csharp

```

<details>
<summary>Result</summary> 

```

```

</details>

```csharp

```

<details>
<summary>Result</summary> 

```

```

</details>

```csharp

```

<details>
<summary>Result</summary> 

```

```

</details>

```csharp

```

<details>
<summary>Result</summary> 

```

```

</details>

```csharp

```

<details>
<summary>Result</summary> 

```

```

</details>

```csharp

```

<details>
<summary>Result</summary> 

```

```

</details>

```csharp

```

<details>
<summary>Result</summary> 

```

```

</details>

```csharp

```

<details>
<summary>Result</summary> 

```

```

</details>

```csharp

```

<details>
<summary>Result</summary> 

```

```

</details>

```csharp

```

<details>
<summary>Result</summary> 

```

```

</details>

```csharp

```

<details>
<summary>Result</summary> 

```

```

</details>

```csharp

```

<details>
<summary>Result</summary> 

```

```

</details>

