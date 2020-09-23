### Kaynak
https://www.dotnetcurry.com/patterns-practices/1412/dataflow-pattern-csharp-dotnet
https://www.blinkingcaret.com/2019/05/15/tpl-dataflow-in-net-core-in-depth-part-1/
https://csharppedia.com/en/tutorial/3110/task-parallel-library--tpl--dataflow-constructs

#### Ek kaynak
https://michaelscodingspot.com/pipeline-pattern-implementations-csharp/

### ActionBlock
Action gibi çalýþýr ve bunlarý bir veri seti halinde tutar. Geri dönüþ parametresi yoktur. Ýçerisindeki fonksiyonu asenkron olarak çalýþtýrýr.
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
Transform block veri alýr ve veri döner. Aþaðýdaki kodda int deðer alýp string deðer dönecektir. Burada diðerlerinden faklý olarak parallelism eklenilerek multithread kullanýlmýþtýr.

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
Tamamlandý
```
</details>

### BatchBlock
Verileri grup halinde almak için kullanýlýr.
Aþaðýdaki örnekte 3'erli olarak getirecektir ve son 9 deðeri son 3 lü sette tek deðer olacaðýndan gelmeyecektir ve uygulama receive kýsýmda beklemede kalacaktýr. Bu da programýn çalýþmasýný engelleyecektir. 
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

Bunun için batchBlock.**Complete();** ve TryReceive metotlarý kullanýlýr. Burada dikkat edilmesi gereken bir noktada complete edildikten sonra post edilenlerin ignore edilmesidir.

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
Batch block'un tam tersidir. Tek bir mesaj alýr ve birden fazla item döner. 
Burada iki tane block var ancak bu ikisini birbirine baðlamamýz gerekmektedir. Bunun için **LintTo** metodu ile source block'u consumer block'a iletebiliriz.


```csharp
public void Run()
{
    var transformManyBlock = new TransformManyBlock<int, string>(a => FindEvenNumbers(a));

    var printBlock = new ActionBlock<string>(a => Console.WriteLine($"Alýnan mesaj:{a}"));

    transformManyBlock.LinkTo(printBlock);

    for (int i = 0; i < 10; i++)
    {
        transformManyBlock.Post(i);
    }
    Console.WriteLine("Tamamlandý.");
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
Tamamlandý.
Alýnan mesaj:1:0
Alýnan mesaj:2:0
Alýnan mesaj:3:0
Alýnan mesaj:3:2
Alýnan mesaj:4:0
Alýnan mesaj:4:2
Alýnan mesaj:5:0
Alýnan mesaj:5:2
Alýnan mesaj:5:4
Alýnan mesaj:6:0
Alýnan mesaj:6:2
Alýnan mesaj:6:4
Alýnan mesaj:7:0
Alýnan mesaj:7:2
Alýnan mesaj:7:4
Alýnan mesaj:7:6
Alýnan mesaj:8:0
Alýnan mesaj:8:2
Alýnan mesaj:8:4
Alýnan mesaj:8:6
Alýnan mesaj:9:0
Alýnan mesaj:9:2
Alýnan mesaj:9:4
Alýnan mesaj:9:6
Alýnan mesaj:9:8
```
</details>

Yine ayný þekilde multithread kullanmak için aþaðýdaki kod kullanýlabilir.
```csharp
var transformManyBlock = new TransformManyBlock<int, string>(a => FindEvenNumbers(a),
    new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 5 });
```

### BufferBlock
Mesajý tüketici alasýya kadar saklamada kullanýlýr. Örnek olarak birden fazla source block'lar (action, transformblock vs.) olsun. Bunlarýn iletileceði consumer block'lar da tanýmlansýn. Bu iki block'lar arasýndaki iliþki buffer block üzerinde tanýmlanýr. Buffer block **BoundedCapacity=1** property deðeri ile alýnan mesajlarýn kaç consumer üzerine daðýtýlacaðýný ayarlar. Bir nevi load balancer görevi görür. Default olarak öncelikle ilk consumer'a gider ve devamýnda consumer mesajý kabul ederse diðerlerine ilk consumer mesajý reddetmediði sürece hiçbir zaman gitmez. Bu durumda tüm alýcýlarda 'BoundedCapacity' deðerleri 1 olarak verildiðinde dengeli olarak mesajlar daðýtýlacaktýr.
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
Ýlgili örnekte iki action block mevcut ve buffer block ise gelen veriyi bu consumer block'lara aktaracaktýr.

```csharp
var bufferBlock = new BufferBlock<int>();

var a1 = new ActionBlock<int>(a =>
    {
        Console.WriteLine($"Mesaj {a} a1 tarafýndan iþlenildi.");
        Task.Delay(300).Wait();
    });


var a2 = new ActionBlock<int>(a =>
{
    Console.WriteLine($"Mesaj {a} a2 tarafýndan iþlenildi.");
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
Mesaj 0 a1 tarafýndan iþlenildi.
Mesaj 1 a1 tarafýndan iþlenildi.
Mesaj 2 a1 tarafýndan iþlenildi.
Mesaj 3 a1 tarafýndan iþlenildi.
Mesaj 4 a1 tarafýndan iþlenildi.
Mesaj 5 a1 tarafýndan iþlenildi.
Mesaj 6 a1 tarafýndan iþlenildi.
Mesaj 7 a1 tarafýndan iþlenildi.
Mesaj 8 a1 tarafýndan iþlenildi.
Mesaj 9 a1 tarafýndan iþlenildi.
```
</details>

Burada dikkat edileceði üzere default ayarlar deðiþtirilmediði için her zaman ilk alýcý çalýþtýrýlacaktýr. Bunun için consumer'lar üzerinden BoundedCapacity bilgisi set edilir.

```csharp
var a1 = new ActionBlock<int>(a =>
    {
        Console.WriteLine($"Mesaj {a} a1 tarafýndan iþlenildi.");
        Task.Delay(300).Wait();
    },new ExecutionDataflowBlockOptions() { BoundedCapacity = 1});


var a2 = new ActionBlock<int>(a =>
{
    Console.WriteLine($"Mesaj {a} a2 tarafýndan iþlenildi.");
    Task.Delay(300).Wait();
}, new ExecutionDataflowBlockOptions() { BoundedCapacity = 1 });
```

<details>
<summary>Result</summary> 

```
Mesaj 1 a2 tarafýndan iþlenildi.
Mesaj 0 a1 tarafýndan iþlenildi.
Mesaj 2 a2 tarafýndan iþlenildi.
Mesaj 3 a1 tarafýndan iþlenildi.
Mesaj 4 a2 tarafýndan iþlenildi.
Mesaj 5 a1 tarafýndan iþlenildi.
Mesaj 6 a2 tarafýndan iþlenildi.
Mesaj 7 a1 tarafýndan iþlenildi.
Mesaj 8 a2 tarafýndan iþlenildi.
Mesaj 9 a1 tarafýndan iþlenildi.
```
</details>

Burada a1 e 10 deðerin vermiþ olsaydýk hepsini a1 den iþletmiþ olacaktý. Çünkü gönderilen mesaj kapasitesi ve alacaðý mesaj kapasitesi ayný. Ýlk baþta sürekli a1'e düþmesinin nedeni ise default deðerin -1 yani iþlemsel olarak sonsuz mesajý alabiliyor olmasý anlamýna geliyordu. Bu yüzden hiçbir zaman a2'ye düþmeyecekti.

Ayný þekilde buffer block için de BoundedCapacity ayarlayabilirdik burada BufferBlock sonsuz mesaj alýrsa request çok olursa outofmemory hatasý alýnabilir. Ancak 1 olarak ayarladýðýmýzda sadece bir mesajý iþleyecekti. Bunun nedeni post metodu ile veri gönderdiðimizde bu metot eðer kapasite dolmuþsa gönderim reddedilecek ve false dönecektir. Bu durumda mesajlarý reject olmadan SendAsync() metodu aracýlýðýyla göderebiliriz. Bu þekilde BoundedCapacity 1 bile olsa bir mesaj gönderildikten sonra bir diðerini gönderebilir.

```csharp
var bufferBlock = new BufferBlock<int>(new DataflowBlockOptions() { BoundedCapacity = 1 });

var a1 = new ActionBlock<int>(a =>
    {
        Console.WriteLine($"Mesaj {a} a1 tarafýndan iþlenildi.");
        Task.Delay(300).Wait();
    },new ExecutionDataflowBlockOptions() { BoundedCapacity = 1});


var a2 = new ActionBlock<int>(a =>
{
    Console.WriteLine($"Mesaj {a} a2 tarafýndan iþlenildi.");
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
            Console.WriteLine($"{i} mesajý kabul edildi.");
        }
        else
        {
            Console.WriteLine($"{i} mesajý reddedildi.");
        }
    });
}
```

<details>
<summary>Result</summary> 

```
Tamamlandý
1 mesajý kabul edildi.
Mesaj 0 a1 tarafýndan iþlenildi.
Mesaj 1 a2 tarafýndan iþlenildi.
10 mesajý kabul edildi.
10 mesajý kabul edildi.
Mesaj 2 a1 tarafýndan iþlenildi.
10 mesajý kabul edildi.
10 mesajý kabul edildi.
Mesaj 3 a2 tarafýndan iþlenildi.
Mesaj 4 a1 tarafýndan iþlenildi.
10 mesajý kabul edildi.
Mesaj 5 a2 tarafýndan iþlenildi.
10 mesajý kabul edildi.
Mesaj 6 a1 tarafýndan iþlenildi.
10 mesajý kabul edildi.
Mesaj 7 a2 tarafýndan iþlenildi.
10 mesajý kabul edildi.
Mesaj 8 a1 tarafýndan iþlenildi.
10 mesajý kabul edildi.
Mesaj 9 a2 tarafýndan iþlenildi.
```
</details>


### BroadcastBlock
Mesajý birden fazla alýcýya gönderir. Burada mesajdan kasýt class instance'larýdýr. Gönderilen mesaj birden fazla alýcýya ulaþtýðýnda her bir mesaj ayný instance'ý paylaþýr.

```csharp
var broadcastBlock = new BroadcastBlock<int>(a => a, new DataflowBlockOptions() { BoundedCapacity = 1 });

var a1 = new ActionBlock<int>(a =>
{
    Console.WriteLine($"Mesaj {a} a1 tarafýndan iþlenildi.");
    Task.Delay(300).Wait();
}, new ExecutionDataflowBlockOptions() { BoundedCapacity = 1 });


var a2 = new ActionBlock<int>(a =>
{
    Console.WriteLine($"Mesaj {a} a2 tarafýndan iþlenildi.");
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
            Console.WriteLine($"{i} mesajý kabul edildi.");
        }
        else
        {
            Console.WriteLine($"{i} mesajý reddedildi.");
        }
    });
}
```
Burada tüm mesajlarýn baþarýlý bir þekilde kabul edildi ancak sadece ikisi iþlenildir.

<details>
<summary>Result</summary> 

```
1 mesajý kabul edildi.
Tamamlandý
10 mesajý kabul edildi.
10 mesajý kabul edildi.
10 mesajý kabul edildi.
10 mesajý kabul edildi.
10 mesajý kabul edildi.
10 mesajý kabul edildi.
10 mesajý kabul edildi.
10 mesajý kabul edildi.
10 mesajý kabul edildi.
Mesaj 0 a1 tarafýndan iþlenildi.
Mesaj 0 a2 tarafýndan iþlenildi.
Mesaj 9 a1 tarafýndan iþlenildi.
Mesaj 9 a2 tarafýndan iþlenildi.
```
</details>

Bir mesaj bir yerden reject aldýðýnda tekrar gönderim yapýlmaz. Eðer boundedcapacity deðerleri kaldýrýlýrsa iþleyiþ de deðiþecektir.

```csharp
var broadcastBlock = new BroadcastBlock<int>(a => a);

var a1 = new ActionBlock<int>(a =>
{
    Console.WriteLine($"Mesaj {a} a1 tarafýndan iþlenildi.");
    Task.Delay(300).Wait();
});


var a2 = new ActionBlock<int>(a =>
{
    Console.WriteLine($"Mesaj {a} a2 tarafýndan iþlenildi.");
    Task.Delay(300).Wait();
});
```
<details>
<summary>Result</summary> 

```
10 mesajý kabul edildi.
Tamamlandý
8 mesajý kabul edildi.
8 mesajý kabul edildi.
8 mesajý kabul edildi.
10 mesajý kabul edildi.
9 mesajý kabul edildi.
10 mesajý kabul edildi.
10 mesajý kabul edildi.
10 mesajý kabul edildi.
10 mesajý kabul edildi.
Mesaj 0 a2 tarafýndan iþlenildi.
Mesaj 0 a1 tarafýndan iþlenildi.
Mesaj 1 a1 tarafýndan iþlenildi.
Mesaj 1 a2 tarafýndan iþlenildi.
Mesaj 2 a1 tarafýndan iþlenildi.
Mesaj 2 a2 tarafýndan iþlenildi.
Mesaj 3 a1 tarafýndan iþlenildi.
Mesaj 3 a2 tarafýndan iþlenildi.
Mesaj 4 a1 tarafýndan iþlenildi.
Mesaj 4 a2 tarafýndan iþlenildi.
Mesaj 5 a2 tarafýndan iþlenildi.
Mesaj 5 a1 tarafýndan iþlenildi.
Mesaj 6 a2 tarafýndan iþlenildi.
Mesaj 6 a1 tarafýndan iþlenildi.
Mesaj 7 a1 tarafýndan iþlenildi.
Mesaj 7 a2 tarafýndan iþlenildi.
Mesaj 8 a1 tarafýndan iþlenildi.
Mesaj 8 a2 tarafýndan iþlenildi.
Mesaj 9 a1 tarafýndan iþlenildi.
Mesaj 9 a2 tarafýndan iþlenildi.
```
</details>

### JoinBlock

JoinBlock birden fazla blocklarý birleþtirerek baþka block'a aktarýmý mümkün kýlar.

```csharp
var broadcastBlock = new BroadcastBlock<int>(a => a);

var a1 = new TransformBlock<int,int>(a =>
{
    Console.WriteLine($"Mesaj {a} a1 tarafýndan iþlenilmekte.");
    Task.Delay(300).Wait();
    return a;
});


var a2 = new TransformBlock<int,int>(a =>
{
    Console.WriteLine($"Mesaj {a} a2 tarafýndan iþlenilmekte.");
    Task.Delay(300).Wait();
    return a;
});

broadcastBlock.LinkTo(a1);
broadcastBlock.LinkTo(a2);

var joinBlock = new JoinBlock<int, int>();
a1.LinkTo(joinBlock.Target1);
a2.LinkTo(joinBlock.Target2);
var printBlock = new ActionBlock<Tuple<int,int>>(a => Console.WriteLine($"{a} mesajý print block tarafýndan iþlenildi."));

joinBlock.LinkTo(printBlock);

for (int i = 0; i < 10; i++)
{
    broadcastBlock.SendAsync(i).ContinueWith(a =>
    {
        if (a.Result)
        {
            Console.WriteLine($"{i} mesajý kabul edildi.");
        }
        else
        {
            Console.WriteLine($"{i} mesajý reddedildi.");
        }
    });
}
```

<details>
<summary>Result</summary> 

```
Tamamlandý
4 mesajý kabul edildi.
4 mesajý kabul edildi.
4 mesajý kabul edildi.
4 mesajý kabul edildi.
8 mesajý kabul edildi.
10 mesajý kabul edildi.
10 mesajý kabul edildi.
10 mesajý kabul edildi.
10 mesajý kabul edildi.
10 mesajý kabul edildi.
Mesaj 0 a1 tarafýndan iþlenilmekte.
Mesaj 0 a2 tarafýndan iþlenilmekte.
Mesaj 1 a2 tarafýndan iþlenilmekte.
Mesaj 1 a1 tarafýndan iþlenilmekte.
(0, 0) mesajý print block tarafýndan iþlenildi.
Mesaj 2 a2 tarafýndan iþlenilmekte.
Mesaj 2 a1 tarafýndan iþlenilmekte.
(1, 1) mesajý print block tarafýndan iþlenildi.
Mesaj 3 a2 tarafýndan iþlenilmekte.
Mesaj 3 a1 tarafýndan iþlenilmekte.
(2, 2) mesajý print block tarafýndan iþlenildi.
Mesaj 4 a2 tarafýndan iþlenilmekte.
Mesaj 4 a1 tarafýndan iþlenilmekte.
(3, 3) mesajý print block tarafýndan iþlenildi.
Mesaj 5 a1 tarafýndan iþlenilmekte.
Mesaj 5 a2 tarafýndan iþlenilmekte.
(4, 4) mesajý print block tarafýndan iþlenildi.
Mesaj 6 a2 tarafýndan iþlenilmekte.
Mesaj 6 a1 tarafýndan iþlenilmekte.
(5, 5) mesajý print block tarafýndan iþlenildi.
Mesaj 7 a1 tarafýndan iþlenilmekte.
Mesaj 7 a2 tarafýndan iþlenilmekte.
(6, 6) mesajý print block tarafýndan iþlenildi.
Mesaj 8 a1 tarafýndan iþlenilmekte.
Mesaj 8 a2 tarafýndan iþlenilmekte.
(7, 7) mesajý print block tarafýndan iþlenildi.
Mesaj 9 a1 tarafýndan iþlenilmekte.
(8, 8) mesajý print block tarafýndan iþlenildi.
Mesaj 9 a2 tarafýndan iþlenilmekte.
(9, 9) mesajý print block tarafýndan iþlenildi.
```
</details>

Multithread ve çalýþma sürelerinde biraz oynama yaparsak çalýþma sýrasý yine deðiþmeyecektir.
```csharp
var broadcastBlock = new BroadcastBlock<int>(a => a);

var a1 = new TransformBlock<int,int>(a =>
{
    Console.WriteLine($"Mesaj {a} a1 tarafýndan iþlenilmekte.");
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
    Console.WriteLine($"Mesaj {a} a2 tarafýndan iþlenilmekte.");
    Task.Delay(150).Wait();
    return a;
}, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 8 });

broadcastBlock.LinkTo(a1);
broadcastBlock.LinkTo(a2);

var joinBlock = new JoinBlock<int, int>();
a1.LinkTo(joinBlock.Target1);
a2.LinkTo(joinBlock.Target2);
var printBlock = new ActionBlock<Tuple<int,int>>(a => Console.WriteLine($"{a} mesajý print block tarafýndan iþlenildi. Deðerler toplamý her zaman 0 dönecektir: {a.Item1+a.Item2}"));

joinBlock.LinkTo(printBlock);

for (int i = 0; i < 10; i++)
{
    broadcastBlock.SendAsync(i).ContinueWith(a =>
    {
        if (a.Result)
        {
            Console.WriteLine($"{i} mesajý kabul edildi.");
        }
        else
        {
            Console.WriteLine($"{i} mesajý reddedildi.");
        }
    });
}

```

<details>
<summary>Result</summary> 

```
6 mesajý kabul edildi.
Tamamlandý
10 mesajý kabul edildi.
4 mesajý kabul edildi.
6 mesajý kabul edildi.
6 mesajý kabul edildi.
10 mesajý kabul edildi.
10 mesajý kabul edildi.
10 mesajý kabul edildi.
10 mesajý kabul edildi.
10 mesajý kabul edildi.
Mesaj 0 a2 tarafýndan iþlenilmekte.
Mesaj 2 a2 tarafýndan iþlenilmekte.
Mesaj 1 a2 tarafýndan iþlenilmekte.
Mesaj 3 a2 tarafýndan iþlenilmekte.
Mesaj 4 a2 tarafýndan iþlenilmekte.
Mesaj 0 a1 tarafýndan iþlenilmekte.
Mesaj 1 a1 tarafýndan iþlenilmekte.
Mesaj 2 a1 tarafýndan iþlenilmekte.
Mesaj 5 a2 tarafýndan iþlenilmekte.
Mesaj 6 a2 tarafýndan iþlenilmekte.
Mesaj 7 a2 tarafýndan iþlenilmekte.
Mesaj 8 a2 tarafýndan iþlenilmekte.
Mesaj 9 a2 tarafýndan iþlenilmekte.
Mesaj 3 a1 tarafýndan iþlenilmekte.
Mesaj 4 a1 tarafýndan iþlenilmekte.
Mesaj 5 a1 tarafýndan iþlenilmekte.
(0, 0) mesajý print block tarafýndan iþlenildi. Deðerler toplamý her zaman 0 dönecektir: 0
(-1, 1) mesajý print block tarafýndan iþlenildi. Deðerler toplamý her zaman 0 dönecektir: 0
(-2, 2) mesajý print block tarafýndan iþlenildi. Deðerler toplamý her zaman 0 dönecektir: 0
Mesaj 6 a1 tarafýndan iþlenilmekte.
(-3, 3) mesajý print block tarafýndan iþlenildi. Deðerler toplamý her zaman 0 dönecektir: 0
Mesaj 7 a1 tarafýndan iþlenilmekte.
Mesaj 8 a1 tarafýndan iþlenilmekte.
(-4, 4) mesajý print block tarafýndan iþlenildi. Deðerler toplamý her zaman 0 dönecektir: 0
(-5, 5) mesajý print block tarafýndan iþlenildi. Deðerler toplamý her zaman 0 dönecektir: 0
Mesaj 9 a1 tarafýndan iþlenilmekte.
(-6, 6) mesajý print block tarafýndan iþlenildi. Deðerler toplamý her zaman 0 dönecektir: 0
(-7, 7) mesajý print block tarafýndan iþlenildi. Deðerler toplamý her zaman 0 dönecektir: 0
(-8, 8) mesajý print block tarafýndan iþlenildi. Deðerler toplamý her zaman 0 dönecektir: 0
(-9, 9) mesajý print block tarafýndan iþlenildi. Deðerler toplamý her zaman 0 dönecektir: 0
```
</details>

### BatchedJoinBlock
JoingBlock gibi çalýþýr ama BatchedBlock gibi birden fazla öðeyi toplu halde alarak iþlem yapar. Ancak burada sýralama her çalýþmada farklý çalýþacaktýr.
```csharp
var broadcastBlock = new BroadcastBlock<int>(a => a);

var a1 = new TransformBlock<int, int>(a =>
{
    Console.WriteLine($"Mesaj {a} a1 tarafýndan iþlenilmekte.");
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
    Console.WriteLine($"Mesaj {a} a2 tarafýndan iþlenilmekte.");
    Task.Delay(150).Wait();
    return a;
}, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 3 });

broadcastBlock.LinkTo(a1);
broadcastBlock.LinkTo(a2);

var joinBlock = new BatchedJoinBlock<int, int>(3);
a1.LinkTo(joinBlock.Target1);
a2.LinkTo(joinBlock.Target2);
var printBlock = new ActionBlock<Tuple<IList<int>, IList<int>>>(a => Console.WriteLine($"{a} mesajý print block tarafýndan iþlenildi.[{string.Join(',', a.Item1)}] , [{string.Join(',', a.Item2)}]"));

joinBlock.LinkTo(printBlock);

for (int i = 0; i < 10; i++)
{
    broadcastBlock.SendAsync(i).ContinueWith(a =>
    {
        if (a.Result)
        {
            Console.WriteLine($"{i} mesajý kabul edildi.");
        }
        else
        {
            Console.WriteLine($"{i} mesajý reddedildi.");
        }
    });
}
```

<details>
<summary>Result</summary> 

```
2 mesajý kabul edildi.
4 mesajý kabul edildi.
3 mesajý kabul edildi.
6 mesajý kabul edildi.
6 mesajý kabul edildi.
9 mesajý kabul edildi.
Tamamlandý
10 mesajý kabul edildi.
10 mesajý kabul edildi.
10 mesajý kabul edildi.
10 mesajý kabul edildi.
Mesaj 0 a2 tarafýndan iþlenilmekte.
Mesaj 1 a2 tarafýndan iþlenilmekte.
Mesaj 2 a2 tarafýndan iþlenilmekte.
Mesaj 0 a1 tarafýndan iþlenilmekte.
Mesaj 2 a1 tarafýndan iþlenilmekte.
Mesaj 1 a1 tarafýndan iþlenilmekte.
Mesaj 3 a2 tarafýndan iþlenilmekte.
Mesaj 4 a2 tarafýndan iþlenilmekte.
Mesaj 5 a2 tarafýndan iþlenilmekte.
Mesaj: [] , [0,1,2]
Mesaj 6 a2 tarafýndan iþlenilmekte.
Mesaj 7 a2 tarafýndan iþlenilmekte.
Mesaj 8 a2 tarafýndan iþlenilmekte.
Mesaj: [] , [3,4,5]
Mesaj 3 a1 tarafýndan iþlenilmekte.
Mesaj 9 a2 tarafýndan iþlenilmekte.
Mesaj: [] , [6,7,8]
Mesaj 4 a1 tarafýndan iþlenilmekte.
Mesaj 5 a1 tarafýndan iþlenilmekte.
Mesaj: [0,-1,-2] , []
Mesaj 6 a1 tarafýndan iþlenilmekte.
Mesaj 7 a1 tarafýndan iþlenilmekte.
Mesaj 8 a1 tarafýndan iþlenilmekte.
Mesaj: [-3,-4] , [9]
Mesaj 9 a1 tarafýndan iþlenilmekte.
Mesaj: [-5,-6,-7] , []
```
</details>

### WriteOnceBlock
Adýndan da anlaþýlacaðý gibi tek bir mesajý kabul eder ve diðerlerini reddeder ve ayný yanýtý döner.

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
        Console.WriteLine("Mesaj alýnamadý.");
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
Tamamlandý
```
</details>

Eðer receive metodu yerine baþka bir block'a gönderim yapacak olsaydýk sadece tek bir sefer çalýþtýðýný görebiliriz.
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
Tamamlandý
Mesaj 0 kabul edildi.
```
</details>

### Completion
BroadcastBlock örneðindeki sendasync metodunu aþaðýdaki gibi deðiþtirelim. Bu durumda Console.ReadLine kodunu kaldýrdýðýmýzda ilgili mesaj bilgileri ekrana yazýlmayacaktýr. Bunun nedeni ilgili kod bloðunun farklý thread'de çalýþmasýdýr ve ReadLine ile bir bekleme yaptýðýmýz için sonuçlarý görüntüleyebiliyorduk ancak bu kodu kaldýrdýðýmýzda kodu bekleten bir þey olmadýðý için sonuç ekrana yansýmadan Main metodu tamamlanmýþ olacaktýr. Bunu çözmek için ilgili thread'de çalýþan kodun tamamlanmasýný beklememiz gerekmektedir.
```csharp
var broadcastBlock = new BroadcastBlock<int>(a => a);

var a1 = new ActionBlock<int>(a =>
{
    Console.WriteLine($"Mesaj {a} a1 tarafýndan iþlenildi.");
    Task.Delay(300).Wait();
});


var a2 = new ActionBlock<int>(a =>
{
    Console.WriteLine($"Mesaj {a} a2 tarafýndan iþlenildi.");
    Task.Delay(300).Wait();
});

broadcastBlock.LinkTo(a1);
broadcastBlock.LinkTo(a2);

for (int i = 0; i < 10; i++)
{
    await broadcastBlock.SendAsync(i);
}
Console.WriteLine("Tamamlandý");
//Console.ReadLine();
```


```
Tamamlandý

C:\Users\Nuri\...\TPL DataFlow\DataDlow\DataDlow\bin\Debug\netcoreapp3.1\DataDlow.exe (process 18700) exited with code 0.
To automatically close the console when debugging stops, enable Tools->Options->Debugging->Automatically close the console when debugging stops.
Press any key to close this window . .

```

Block'un süreçlerinin tamamlanýp tamamlanmadýðýný öðrenmek için block içerisinde mesajýn olup olmadýðýna ya da completed eventi kontrol edilir. Bunun için linkto içerisinde **PropagateCompletion** true olarak set edilmeli ve *broadcastBlock.Complete(); finalBlock.Completion.Wait();* metotlarý çaðýrýlmalýdýr.
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
    Console.WriteLine($"Mesaj {a} a1 tarafýndan iþlenildi.");
    Task.Delay(300).Wait();
    return -a;
});


var a2 = new TransformBlock<int, int>(a =>
{
    Console.WriteLine($"Mesaj {a} a2 tarafýndan iþlenildi.");
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
        Console.WriteLine($"{a.Item1}: tüm consumer'lar tarafýndan iþlenildi");
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
Append baðlanan consumer iliþkisini önceliklendirmek ya da kaldýrmak için kullanýlýr. Aþaðýdaki bufferblock için görüleceði üzere sadece a1 mesajlarý gelmektedir.

```csharp
var bufferBlock = new BufferBlock<int>();

var a1 = new ActionBlock<int>(a =>
{
    Console.WriteLine($"Mesaj {a} a1 tarafýndan iþlenildi.");
});


var a2 = new ActionBlock<int>(a =>
{
    Console.WriteLine($"Mesaj {a} a2 tarafýndan iþlenildi.");
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
Tamamlandý
Mesaj 0 a1 tarafýndan iþlenildi.
Mesaj 1 a1 tarafýndan iþlenildi.
Mesaj 2 a1 tarafýndan iþlenildi.
Mesaj 3 a1 tarafýndan iþlenildi.
Mesaj 4 a1 tarafýndan iþlenildi.
Mesaj 5 a1 tarafýndan iþlenildi.
Mesaj 6 a1 tarafýndan iþlenildi.
Mesaj 7 a1 tarafýndan iþlenildi.
Mesaj 8 a1 tarafýndan iþlenildi.
Mesaj 9 a1 tarafýndan iþlenildi.
```
</details>


Append false yapýlýrsa a1 den önce a2 iliþkisi baðlanacaktýr. Bu þekilde sonuçlarýn hepsi a2 tarafýndan çalýþtýrýlacaktýr.

```csharp
bufferBlock.LinkTo(a2,new DataflowLinkOptions() { Append = false });
```
<details>
<summary>Result</summary> 

```
Tamamlandý
Mesaj 0 a2 tarafýndan iþlenildi.
Mesaj 1 a2 tarafýndan iþlenildi.
Mesaj 2 a2 tarafýndan iþlenildi.
Mesaj 3 a2 tarafýndan iþlenildi.
Mesaj 4 a2 tarafýndan iþlenildi.
Mesaj 5 a2 tarafýndan iþlenildi.
Mesaj 6 a2 tarafýndan iþlenildi.
Mesaj 7 a2 tarafýndan iþlenildi.
Mesaj 8 a2 tarafýndan iþlenildi.
Mesaj 9 a2 tarafýndan iþlenildi.
```
</details>

### MaxMessages
Block tarafýndan alýnacak mesajlarý sýnýrlamak için kullanýlýr. Aþaðýdaki örnekte a2 için maksimum 5 mesaj alýnýr.

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
Tamamlandý
Mesaj 5 a1 tarafýndan iþlenildi.
Mesaj 0 a2 tarafýndan iþlenildi.
Mesaj 1 a2 tarafýndan iþlenildi.
Mesaj 2 a2 tarafýndan iþlenildi.
Mesaj 3 a2 tarafýndan iþlenildi.
Mesaj 4 a2 tarafýndan iþlenildi.
Mesaj 6 a1 tarafýndan iþlenildi.
Mesaj 7 a1 tarafýndan iþlenildi.
Mesaj 8 a1 tarafýndan iþlenildi.
Mesaj 9 a1 tarafýndan iþlenildi.
```

</details> 

### Mesaage Filtering

```csharp
bufferBlock.LinkTo(a1, a => a % 2 == 0);
```

```
Tamamlandý
Mesaj 0 a2 tarafýndan iþlenildi.
Mesaj 1 a2 tarafýndan iþlenildi.
Mesaj 2 a2 tarafýndan iþlenildi.
Mesaj 3 a2 tarafýndan iþlenildi.
Mesaj 4 a2 tarafýndan iþlenildi.
```
Sonuç ne yazýkki istediðimiz gibi olmadý. Bunun nedeni bir önceki kodda da anlaþýlacaðý üzere ilk 5 mesajý a2 iþliyor. a1 tarafýna 5 msajý geliyor ve filtreden geçemiyor ve geçemediði için beklemede kalýyor ve dolayýsýyla a1 tamamlanamýyor.

Bunun için aþaðýdaki gibi NullTarget DataFlowBlock eklenebilir. Bu bize reddidilmiþ olan block'lar için tamamlanabilir bir block saðlamýþ olacaktýr. Bu block'u da aþaðýdaki gibi LinkTo ile baðlayalým
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
Tamamlandý
Mesaj 4 a1 tarafýndan iþlenildi.
Mesaj 0 a2 tarafýndan iþlenildi.
Mesaj 1 a2 tarafýndan iþlenildi.
Mesaj 2 a2 tarafýndan iþlenildi.
Mesaj 3 a2 tarafýndan iþlenildi.
Mesaj 6 a1 tarafýndan iþlenildi.
Mesaj 8 a1 tarafýndan iþlenildi.
```
NullTarget yerine herhangi bir action block koyduðumuzda bu block reddedilen mesajlarý alacaktýr.

```csharp
bufferBlock.LinkTo(new ActionBlock<int>(a => Console.WriteLine($"{a} mesajý reddedildi.")));
```

```
Tamamlandý
Mesaj 4 a1 tarafýndan iþlenildi.
5 mesajý reddedildi.
7 mesajý reddedildi.
9 mesajý reddedildi.
Mesaj 0 a2 tarafýndan iþlenildi.
Mesaj 1 a2 tarafýndan iþlenildi.
Mesaj 2 a2 tarafýndan iþlenildi.
Mesaj 3 a2 tarafýndan iþlenildi.
Mesaj 6 a1 tarafýndan iþlenildi.
Mesaj 8 a1 tarafýndan iþlenildi.
```

### Multiple Producers
Daha önceki örneklerde birden fazla consumer ve bir producer vardý.
Aþaðýdaki gibi LinkTo ile iki producer'ý ayný consumer'a aktarabiliriz.
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
    producer1.Post($"Producer 1 mesajý: {i}");
    producer2.Post($"Producer 2 mesajý: {i}");
}
```

<details>
<summary>Result</summary> 

```
Tamamlandý
Producer 1 mesajý: 0
Producer 2 mesajý: 0
Producer 1 mesajý: 1
Producer 1 mesajý: 2
Producer 2 mesajý: 1
Producer 1 mesajý: 3
Producer 1 mesajý: 4
Producer 2 mesajý: 2
Producer 1 mesajý: 5
Producer 1 mesajý: 6
Producer 2 mesajý: 3
Producer 1 mesajý: 7
Producer 1 mesajý: 8
Producer 2 mesajý: 4
Producer 1 mesajý: 9
Producer 2 mesajý: 5
Producer 2 mesajý: 6
Producer 2 mesajý: 7
Producer 2 mesajý: 8
Producer 2 mesajý: 9
```

</details>

Kod içerisine completion metotlarýný ekleyelim. Burada completion için oluþturduðumuz extension metot ile PropagateCompletion deðerini güncellediðimizi de unutmayalým.

```csharp
var printBlock = new ActionBlock<string>(n => Console.WriteLine(n));
producer1.LinkToWithPropagation(printBlock);
producer2.LinkToWithPropagation(printBlock);

for (int i = 0; i < 10; i++)
{
    producer1.Post($"Producer 1 mesajý: {i}");
    producer2.Post($"Producer 2 mesajý: {i}");
}
producer1.Complete();
producer2.Complete();
printBlock.Completion.Wait();
```
Sonuç ne yazýkki istediðimiz gibi olmadý. Bazý mesajlar eksik kaldý. Bunun nedeni producer1 in tamamlandý komutunu göndermesiyle consumer'ýn da tamamlanmasý. Burada süreç tek yönlü olduðu için consumer producer2 ye tamamlanýp tamamlanmadýðýný sormadýðýndan tamamlandý olarak kabul edip yoluna devam etmekte.

<details>
<summary>Result</summary> 

```
Producer 1 mesajý: 0
Producer 1 mesajý: 1
Producer 2 mesajý: 0
Producer 1 mesajý: 2
Producer 2 mesajý: 1
Producer 1 mesajý: 3
Producer 1 mesajý: 4
Producer 2 mesajý: 2
Producer 1 mesajý: 5
Producer 1 mesajý: 6
Producer 2 mesajý: 3
Producer 1 mesajý: 7
Producer 1 mesajý: 8
Producer 2 mesajý: 4
Producer 1 mesajý: 9
Tamamlandý
```
</details>

TPL Dataflow'u bir push mimarisidir. Producer consumer'ý bilir ancak tersi mümkün deðildir. Bu yüzden PropagateCompletion deðeri default olarak false'dur. Buradaki senaryo için TPL tarafýnda hazýr bir çözüm olmasa da Task.WhenAll ile her iki completion'ýn bittiði aný bulabilir ve printBlock'un da complete eventi ile süreci tamamlamýþ oluruz. Burada LinkTo kýsmý eski haline getirilmiþ yani propagation'lar kaldýrýlmýþtýr. 

```csharp
var printBlock = new ActionBlock<string>(n => Console.WriteLine(n));
producer1.LinkTo(printBlock);
producer2.LinkTo(printBlock);

for (int i = 0; i < 10; i++)
{
    producer1.Post($"Producer 1 mesajý: {i}");
    producer2.Post($"Producer 2 mesajý: {i}");
}
await Task.WhenAll(new[] { producer1.Completion, producer2.Completion }).ContinueWith(a=>printBlock.Complete());
printBlock.Completion.Wait();
```

<details>
<summary>Result</summary> 

```
Tamamlandý
Producer 1 mesajý: 0
Producer 2 mesajý: 0
Producer 1 mesajý: 1
Producer 1 mesajý: 2
Producer 2 mesajý: 1
Producer 1 mesajý: 3
Producer 1 mesajý: 4
Producer 2 mesajý: 2
Producer 1 mesajý: 5
Producer 1 mesajý: 6
Producer 2 mesajý: 3
Producer 1 mesajý: 7
Producer 1 mesajý: 8
Producer 2 mesajý: 4
Producer 1 mesajý: 9
Producer 2 mesajý: 5
Producer 2 mesajý: 6
Producer 2 mesajý: 7
Producer 2 mesajý: 8
Producer 2 mesajý: 9
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

