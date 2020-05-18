
### Transaction

Tek bir iş birimi gibi çalışan operasyonlar dizisidir. Bu şekilde bir hata olması durumunda database'in stabil olmasını sağlar ve veriyi kurtarmayı sağlar. Bununla birlikte eşzamanlı erişim durumlarında işlemler arasındaki izolasyonu sağlar.  
#### ACID properties of Transactions

Atomicity: All or nothing
Consistency: Data Integrity when complete. Transaction sonunda veritabanındaki bir durumun bir sonraki geçerli duruma geçmesini garanti altına alır. Database'e yazılan herhangi bir verinin belirlenen kurallara göre geçerli olmalıdır. Bu kurallar constraint,cascade,trigger'lar olabilir.
Isolation: Modifications isolated from other transactions. Bir transaction tamamlanasıya kadar diğer transaction tarafından görülmemelidir.
Durability: Effects are permanent. Sistem çöktüğünde ya da restart edildiğinde data hala saklanmış olmalıdır.

### Concurrency Control
#### Optimistic 
Birçok transaction'ın genellikle bir diğerini etkilemediğini varsayar. Bunun için veri okumada lock olmaz. Sadece veri kayıt olduğunda çakışmaları kontrol eder.
Kaydetme sırasında veri okunduktan sonra sistem başka bir transaction tarafından verinin değişip değişmediğini kontrol eder. Bunu da genelde verinin versiyon numarası ile custom oluşturulan fieldlar ile kontrol eder. Bu durumda da bir conflict olduğunda da transaction roll back edilir. Optimistic concurrency control genellikle veri çakışmalarının az olduğu durumlarda kullanılır. Burada roll back yapmanın maliyeti veriyi lock'lamaktan daha azdır.

#### Pessimistic
Düzenlemede ve hatta okuma işlemlerinde veri bütünlüğü için locklama işlemi yapılır. Başka bir kullanıcı Lock kaldırılıncaya kadar hiçbir işlem yapamaz. Bu şekilde sistemin eşzamanlı çalışmasını azaltmış olur. Veri çakışmalarının çok olması durumunda kullanılır ve locklama maliyeyi transaction roll back etme maliyetinden azdır.

#### Lost Update
İki kullanıcı aynı kaydı okur güncellemye başlar. User2 güncellemesini User1 güncelleme işini bitirmeden yapar. Bu durumnda User1 güncellenen veriden habersiz kendisi de güncellenmiş verinin üzerine yazacaktır.

#### Dirty Read (Uncommited Dependency)
User1 işlem yaparken commitlenmeden User2 ilgili veriyi okur. Örnek olarak User1 x değerini 5'ten 7 ye güncellesin. Bu durumda commitlenmeden okuyan User2 x değerini 7 gibi görecektir. Ancak burada User1 yaptığı işlem commitlenmediğinde ya da roll back olduğunda veri eski haline gelecektir. Ancak User2 bu durumdan haberdar olmadığı için verinin 7 olduğunu düşünecektir.

#### Nonrepeatable Read
User1 bir veri setini alır ve User2 bundan sonra veriyi günceller. User1 ikinci defa veriyi okuduğunda güncellenmiş veriyi görecektir. Bu durum aynı transaction içerisinde veriyi iki defa okunmadığı durumda sorun olmayacaktır ancak transaction içerisinde veri iki defa okunuyorsa ilk okuma ile ikinci okuma arasında fark olacaktır.
![ScreenShot](/files/NonrepeatableRead.png) 

#### Phantom Read
Aynı nonrepeatable read gibidir ancak birden fazla veri seti için geçerlidir. User1 bir veri çeker ve o sırada User2 o veri seti için bir ekleme yapar User1 transaction içerisinde yeni bir sorgu daha çektiğinde User2 nin eklediği veriyi de almış olacaktır.  

#### Missing or Double Reads
User1 büyük bir veri seti çeksin. Bu süreç içerisinde User2 veri setinde güncelleme yapmış olabilir ve burada index'te bir değişiklik yapmış olabilir. Bu durumda yeni indexte daha önceden okumuş olduğu veriyi tekrar okuma yapabilir. Ya da tam tersi okuma işleminin devamında olması gereken veri yer değiştirdiği için kayıp olabilir.
![ScreenShot](/files/MissingOrDoubleReads.gif) 

Bu tür durumlar locking data ile ya da row versioning ile çözülebilir. 

### Database Isolation Levels
Veritabanında transaction işlemi gerçekleştirdiğinizde bu transaction belirli bir isolation level altında çalışır. Isolation level bir transaction'ın başka bir transction tarafından hangi düzeyde yalıtılacağını tanımlar. Isolation level verinin okundığında lock'lanıp lock'lanmayacağını, hanti tip lock'un talep edildiğini kontrol eder. Ayrıca Lock'un ne kadar tutulacağını da kontrol eder ve read etmeye çalışıldığında bu veri başka bir transaction tarafından değiştirildiğinde nasıl davranacağına karar verir. 
En temel lock tipleri **read** ve **exclusive** locklar'dır. 
> Read locklar SQL server tarafından share edilirler. Yani birden fazla transaction veriyi okuyabilir.  Bu tip lock read işlemi sonrasında relase edilebilir ya da transaction sonuna kadar saklanabilir. 
> Exclusive locklar silinme ve güncellenme olasılığna karşın kullanılır. Bu tip lock için sadece bir transaction çalışır. Bu şekilde diğer lock'lar buradaki veriye erişemeyecektir. Ancak bu deadlocklar için önemli bir husustur. 
Bunlara ek olarak **Key Range** ve **Update Locks** da mevcuttur.

Isolation level'ı lower ve higher olarak ikiye ayırabiliriz.
**Lower Isolation Level:** Birden fazla kullanıcının aynı anda işlem yapmasına olanak tanır. Ancak bu da concurrency sorunlarını daha çok doğuracağı anlamına gelir.
**Higher Isolation Level:** Concurrency sorunlarını azaltır. Ancak daha fazla sistem kaynağı tüketecektir. Transactionların başkalarını engelleme olasılığını arttırır hatta deadlock'a neden olabilir.

### Isolation Levels
http://korayduzgun.blogspot.com/2012/05/sql-server-transaction-isolation-levels.html

- Read Uncommitted
    Bir veri üzerinde bir kullanıcı transaction yaparken diğer kullanıcılarında değişikliğe uğramış fakat commit edilmemiş verileri görebilmesini sağlayan level'dır. Dirty Read'e izin verir. 

```sql
USE TestData;
 
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; 
BEGIN TRANSACTION; 
SELECT ID, TestValue FROM dbo.TestItems WHERE ID=1;
UPDATE dbo.TestItems SET TestValue=100 WHERE ID=1;
WAITFOR DELAY '00:00:05';
COMMIT TRANSACTION;
SELECT ID, TestValue FROM dbo.TestItems WHERE ID=1;
```

İkinci transaction olarak
```sql
USE TestData; 
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; 
BEGIN TRANSACTION; 
SELECT ID, TestValue  FROM TestItems WHERE Id=1;
COMMIT TRANSACTION;
```
Bu durumda 2. transaction için hiç bekleme olmadan 100 değeri getirilmiş olacaktır. Ancak bu şekilde ilk transaction rollback olursa dirty read olmuş olacaktır.

- Read Committed
    Dirty read'e izin vermez. Diğer transaction'ın tamamlanmasını bekler(yani commit edilmesini) 
Yukarıdaki örnekte ikinci transaction için commited olarak ayarlansaydı 5 sn bekleme süresinden sonra okuma işlemi gerçekleşmiş olacaktı.
```sql
USE TestData; 
SET TRANSACTION ISOLATION LEVEL READ COMMITTED; 
BEGIN TRANSACTION; 
SELECT ID, TestValue  FROM TestItems WHERE Id=1;
COMMIT TRANSACTION;
```
Ancak bu durumda nonrepeatable read'e neden olabilir. SQL server'ın default isolation level'ıdır.

> Repeatable Read
    NonRepeatable Read'leri engeller. Mevcut transaction tamamlanasıya kadar update ya da delete olaylarını takip eder. Read lock'larını tutar ve deadlock'lara neden olabilir.
**Transaction1**
```sql
USE TestData;
SET TRANSACTION ISOLATION LEVEL READ COMMITTED; 
BEGIN TRANSACTION; 
SELECT ID, TestValue FROM TestItems WHERE Id=1;
WAITFOR DELAY '00:00:05';
SELECT ID, TestValue  FROM TestItems WHERE Id=1;
COMMIT TRANSACTION;
```
**Transaction2**
```sql
USE TestData;
SET TRANSACTION ISOLATION LEVEL READ COMMITTED; 
BEGIN TRANSACTION; 
UPDATE dbo.TestItems SET TestValue=200 WHERE Id=1;
COMMIT TRANSACTION;
```
Transaction 2 işlemi update işlemi yaptığı için ilk transaction işleminin tamamlanmasını beklemez. Bu şekilde ilk transaction sonucunda iki farklı yanıt dönülmüş olunur.

Ancak ilk transaction'ı aşağıdaki gibi değiştirirsek
```sql
USE TestData;
SET TRANSACTION ISOLATION LEVEL REPEATABLE READ; 
BEGIN TRANSACTION; 
SELECT ID, TestValue FROM TestItems WHERE Id=1;
WAITFOR DELAY '00:00:05';
SELECT ID, TestValue  FROM TestItems WHERE Id=1;
COMMIT TRANSACTION;
```
Bu şekilde ikinci işlem block'lanmış oluyor ve ikinci transaction(update işlemi) ilk transaction tamamlandıktan sonra çalışacaktır ve getireceği iki değer de eski veri olacaktır. Çünkü update işlemi daha sonra çalışmış olacaktır.

Range lock'lanmadığı için PhantomRead'e neden olabilir.

- Serializable
    Most isolated level. Tüm transaction'lar birbirinden tamamiyle ayrılmıştır.
    Range locks to prevent phantom reads.

**Transaction1**
```sql
USE TestData;
SET TRANSACTION ISOLATION LEVEL REPEATABLE READ; 
BEGIN TRANSACTION; 
SELECT COUNT(*) FROM TestItems
WAITFOR DELAY '00:00:05';
SELECT COUNT(*) FROM TestItems
COMMIT TRANSACTION;
```

**Transaction2**
```sql
USE TestData;
SET TRANSACTION ISOLATION LEVEL READ COMMITTED; 
BEGIN TRANSACTION; 
INSERT INTO DBO.TestItems (ID, TestValue)
VALUES (2, 50);
COMMIT TRANSACTION;
```
İlk sorguda iki count arasında farklılık olacaktır.

Ancak Transaction1 aşağıdaki gibi değiştirilirse.
```sql
USE TestData;
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE; 
BEGIN TRANSACTION; 
SELECT COUNT(*) FROM TestItems
WAITFOR DELAY '00:00:05';
SELECT COUNT(*) FROM TestItems
COMMIT TRANSACTION;
```
Bu şekilde ilk olarak Transaction1 tamamlanır ve devamında Transaction2 çalışacaktır.

### Sql Server Isolation Levels
Sql Server'da bu isolation level'lara ek olarak 2 tane daha isolation level vardır.
- Snapshot
    Lock yerine satır versiyonlama kullanır. Bu değerler tempdb'den okunur. Bu durumda repeatable read söz konusu olabilir. Optimistic Concurrency için kullanılır.

**Transaction1**
```sql
USE TestData;  
--ALTER DATABASE TestData SET ALLOW_SNAPSHOT_ISOLATION ON  
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE; 
BEGIN TRANSACTION; 
SELECT ID, TestValue FROM TestItems WHERE Id=1;
UPDATE dbo.TestItems SET TestValue=400 WHERE Id=1;
WAITFOR DELAY '00:00:05';
COMMIT TRANSACTION;
SELECT ID, TestValue  FROM TestItems WHERE Id=1;
```
**Transaction2**
```sql
USE TestData;
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE; 
BEGIN TRANSACTION; 
UPDATE dbo.TestItems SET TestValue=450 WHERE Id=1;
COMMIT TRANSACTION;
SELECT ID, TestValue  FROM TestItems WHERE Id=1;
```

Yukarıdaki iki transaction çalıştığında transction için önce eski değer devamında transaction tamamlandığı için Transaction2'nin güncellediği değer olan 450 değeri gelecektir. Transaction2 ise transaction1'in tamamlanmasnı bekleyip 450 değerini verecektir.
Ancak burada aşağıdaki gibi snapshot işlemi yapılsaydı. 
Bunun içn ilk önce snapshot özelliğini o database için aktif etmemiz gerekmektedir.
```sql
ALTER DATABASE TestData SET ALLOW_SNAPSHOT_ISOLATION ON  
```

**Transaction1**
```sql
USE TestData;  
SET TRANSACTION ISOLATION LEVEL SNAPSHOT; 
BEGIN TRANSACTION; 
SELECT ID, TestValue FROM TestItems WHERE Id=1;
UPDATE dbo.TestItems SET TestValue=460 WHERE Id=1;
WAITFOR DELAY '00:00:05';
COMMIT TRANSACTION;
SELECT ID, TestValue  FROM TestItems WHERE Id=1;
```

**Transaction2**
```sql
USE TestData;
SET TRANSACTION ISOLATION LEVEL SNAPSHOT; 
BEGIN TRANSACTION; 
UPDATE dbo.TestItems SET TestValue=470 WHERE Id=1;
COMMIT TRANSACTION;
SELECT ID, TestValue  FROM TestItems WHERE Id=1;
```

İkisinin aynı anda çalışması durumunda 2. transaction fail olacaktır(*Snapshot isolation transaction aborted due to update conflict*). Writing işlemi her zaman diğer writing işlemini engelleyecektir. Ancak read'ler write operasyonlarını ya da tam tersi olduğunda birbirini engellemeyecektir. 
Repeatable read tarafındaki kodu tekrar inceleyecek olursak ilk transaction 2 defa read işlemi yapıyor ve bu surada ikinci transaction güncelleme işlemi yapıyor. İlk transaction eski veriyi güncellenmeden aynı şekilde getirecektir. Ancak burada ikinci transaction ilk transaction'ın tamamlanmasını bekleyecektir. Çünkü read lock vardır.
**Transaction1**
```sql
SET TRANSACTION ISOLATION LEVEL REPEATABLE READ; 
BEGIN TRANSACTION; 
SELECT ID, TestValue FROM TestItems WHERE Id=1;
WAITFOR DELAY '00:00:05';
SELECT ID, TestValue  FROM TestItems WHERE Id=1;
COMMIT TRANSACTION;
```

**Transaction2**
```sql
SET TRANSACTION ISOLATION LEVEL READ COMMITTED; 
BEGIN TRANSACTION; 
UPDATE dbo.TestItems SET TestValue=250 WHERE Id=1;
COMMIT TRANSACTION;
```

Ancak burada **Transaction1**'de isolation level'ı aşağıdaki gibi değiştirdiğimizde
```sql
SET TRANSACTION ISOLATION LEVEL SNAPSHOT; 
```
İlk transaction eski değerleri yine aynı şeklde getirecektir ancak ikinci transaction lock olmadığı için 5 sn beklemeden direk güncelleme işlemini yapacaktır.
Query raporlama konusunda performans olarak Snapshot isolation level'ı önerilebilir.

- Read Committed Snapshot
    Isolation level değildir. Read Commited Isolation level'ın lock yerine  row versioning ile kullanılmasını sağlar. Bu işlem database ayarları konfigüre edilerek yapılır. Yine burada da row versioning için tempdb kullanılır. *Read işlemlerinde performans artışı için kullanılabilir.*


> Mevcut isolation level'ı öğrenmek için aşağıdaki kod kullanılabilir
```sql
SELECT CASE transaction_isolation_level 
WHEN 0 THEN 'Unspecified' 
WHEN 1 THEN 'ReadUncommitted' 
WHEN 2 THEN 'ReadCommitted' 
WHEN 3 THEN 'Repeatable' 
WHEN 4 THEN 'Serializable' 
WHEN 5 THEN 'Snapshot' END AS TRANSACTION_ISOLATION_LEVEL 
FROM sys.dm_exec_sessions 
where session_id = @@SPID

SELECT is_read_committed_snapshot_on FROM sys.databases 
WHERE name= 'TestData'
```

Burada ikinci selectten dönen yanıt 0 olduğunda bu isolation level'ın pasif olduğu anlamına gelir. Aktif etmek için aşağıdaki kod bloğu çalıştırılır ve devamında sql server restart edilir.

```sql
ALTER DATABASE TestData SET READ_COMMITTED_SNAPSHOT ON;
```
``` 
sqllocaldb stop "MSSQLLocalDB" -k
sqllocaldb start "MSSQLLocalDB" -k
```
