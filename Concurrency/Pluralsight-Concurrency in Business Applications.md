*Kitap önerisi: Patterns of Enterprise Application Architecture Martin Fowler*
*Kaynak: https://app.pluralsight.com/library/courses/e81319cc-a32a-4c42-af66-fd5d80aaa31c/table-of-contents*

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
![ScreenShot](/Concurrency/Files/NonrepeatableRead.png) 

#### Phantom Read
Aynı nonrepeatable read gibidir ancak birden fazla veri seti için geçerlidir. User1 bir veri çeker ve o sırada User2 o veri seti için bir ekleme yapar User1 transaction içerisinde yeni bir sorgu daha çektiğinde User2 nin eklediği veriyi de almış olacaktır.  

#### Missing or Double Reads
User1 büyük bir veri seti çeksin. Bu süreç içerisinde User2 veri setinde güncelleme yapmış olabilir ve burada index'te bir değişiklik yapmış olabilir. Bu durumda yeni indexte daha önceden okumuş olduğu veriyi tekrar okuma yapabilir. Ya da tam tersi okuma işleminin devamında olması gereken veri yer değiştirdiği için kayıp olabilir.
![ScreenShot](/Concurrency/Files/MissingOrDoubleReads.gif) 

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

- **Read Uncommitted**

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

- **Read Committed**

    Dirty read'e izin vermez. Diğer transaction'ın tamamlanmasını bekler (yani commit edilmesini). 
Yukarıdaki örnekte ikinci transaction için commited olarak ayarlansaydı 5 sn bekleme süresinden sonra okuma işlemi gerçekleşmiş olacaktı.
```sql
USE TestData; 
SET TRANSACTION ISOLATION LEVEL READ COMMITTED; 
BEGIN TRANSACTION; 
SELECT ID, TestValue  FROM TestItems WHERE Id=1;
COMMIT TRANSACTION;
```
- **Nonrepeatable read** Ancak bu durumda *Nonrepeatable read*'e neden olabilir. SQL server'ın default isolation level'ıdır.

**Repeatable Read:** NonRepeatable Read'leri engeller. Mevcut transaction tamamlanasıya kadar update ya da delete olaylarını takip eder. Read lock'larını tutar ve deadlock'lara neden olabilir.

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

- **Serializable**

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
- **Snapshot:** Lock yerine satır versiyonlama kullanır. Bu değerler tempdb'den okunur. Sql Server güncelleme ya da silme işlemi sırasında copy-on-write mekanizmasını kullanır. Bu durumda repeatable read söz konusu olabilir. Optimistic Concurrency için kullanılır.

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
Bunun için ilk önce snapshot özelliğini o database için aktif etmemiz gerekmektedir.
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
Repeatable read tarafındaki kodu tekrar inceleyecek olursak ilk transaction 2 defa read işlemi yapıyor ve bu sırada ikinci transaction güncelleme işlemi yapıyor. İlk transaction eski veriyi güncellenmeden aynı şekilde getirecektir. Ancak burada ikinci transaction ilk transaction'ın tamamlanmasını bekleyecektir. Çünkü read lock vardır.

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

- **Read Committed Snapshot (RCSI):**
    Isolation level değildir. Read Commited Isolation level'ın lock yerine  **row versioning** ile kullanılmasını sağlar. Bu işlem database ayarları konfigüre edilerek yapılır. Yine burada da row versioning için tempdb kullanılır. *Read işlemlerinde performans artışı için kullanılabilir.*


Mevcut isolation level'ı öğrenmek için aşağıdaki kod kullanılabilir.
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
``` sql
sqllocaldb stop "MSSQLLocalDB" -k
sqllocaldb start "MSSQLLocalDB" -k
```

Aktif snapshot transaction'ları görüntülemek için
```sql
SELECT DB_NAME(database_id) AS DatabaseName, t.*
FROM sys.dm_tran_active_snapshot_database_transactions t
    JOIN sys.dm_exec_sessions s
    ON t.session_id = s.session_id;
```
TempDb nin dolma olasılığına karşılık doluluk miktarını gösteren kod
```sql
SELECT DB_NAME(vsu.database_id) AS DatabaseName,
    vsu.reserved_page_count, 
    vsu.reserved_space_kb, 
    tu.total_page_count as tempdb_pages, 
    vsu.reserved_page_count * 100. / tu.total_page_count AS [Snapshot %],
    tu.allocated_extent_page_count * 100. / tu.total_page_count AS [tempdb % used]
FROM sys.dm_tran_version_store_space_usage vsu
    CROSS JOIN tempdb.sys.dm_db_file_space_usage tu
WHERE vsu.database_id = DB_ID(DB_NAME());
```
Mevcut version storedaki bileşenleri gösterir
```sql
-- Show the contents of the current version store (expensive)
SELECT DB_NAME(database_id) AS DatabaseName, *
FROM sys.dm_tran_version_store;
```

```sql
-- Show objects producing most versions (expensive)
SELECT DB_NAME(database_id) AS DatabaseName, *
FROM sys.dm_tran_top_version_generators;
```

#### Locking vs. Row Versioning
| Locking(Pessimistic)             | Row Versioning (Optimistic)           |
|----------------------------------|---------------------------------------|
| Read Uncommited                  | Read commited snapshot isolation      |
| Read Commited                    | Snapshot isolation level              |
| Repeatable read                  |                                       |
| Serializable                     |                                       |
| ANSI SQL-92 compliant            | Proprietary                           |
| Better for long-running updates  | Better for read-heavy operations      |
| Normal tempdb usage              | Extra usage of tempdb (version store) |
| More blocking = less concurrency | Less blocking = greater concurrency   |

### Deadlock

İki task'ın birbirini kalıcı olarak engellediğinde meydana gelir. Her bir taskın bir kaynak üzerinde bir lock'u vardır. Ancak database'ler bu durumda sadece bir transaction'ı seçecektir.

> Örnek olması açısından bir kullanıcı bir sayfada bir işlem yaparken başka sayfada da işlem yapmak istiyor. Ancak işlem yapacağı sayfada da başkası o sayfayı lock'lamış olsun. Bu süreçte ilgili lock'un kalkmasını bekleyecektir. Ve burada ilgili sayfadaki lock'u oluşturan kullanıcı da ilk kullanıcının lock'ladığı sayfada işlem yapmaya çalışsın. Aynı şekilde her iki kullanıcı da birbirinin lock'unu beklemek durumundadur. Burada Database bir kurban seçecektir ya da lock süresi bitene göre bir kurban seçilir. Bu seçimler çok zor olduğun için başka farklı yöntemler mevcuttur.

Deadlock'u engellemek için dbcontexten güncellenecek veri çekimi sırasında updlock komutu kullanılabilir. Bu sadece ilgili satır lock'lanmış olacaktır.

```csharp
return await _context.TestItems
                .FromSql("SELECT Id, Name, Value, Modified, ModifiedBy FROM TESTITEMS WITH (UPDLOCK) WHERE Id = " + id)
                .FirstAsync();
```
Bir ikinci metot ise deadlock olduğu zaman tekrarlı bir şekilde aynı transaction'ı çalıştırmak olabilir.
```csharp
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=ConcurrencyTestingEFCoreDb;Trusted_Connection=True;MultipleActiveResultSets=true",
                options => options.EnableRetryOnFailure());

```

```csharp
using (var _context = new AppDataContext())
            {
                var strategy = _context.Database.CreateExecutionStrategy();

                strategy.Execute(() =>
                {

                    using (var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
                    {
                        try
                        {
                            TestItem item = _context.TestItems.Find(testData.RecordId);
                            int originalValue = item.Value;
                            Console.WriteLine("{0} has read record {1}", testData.UserName, item.Id, item.Value);

                            item.Value += 2;
                            item.ModifiedBy = testData.UserName;
                            item.Modified = DateTime.Now;

                            Console.WriteLine("{0} is updating record {1}, setting Value:{2} + 2", testData.UserName, item.Id, originalValue);

                            // now try to update the read lock to an exclusive lock
                            _context.SaveChanges();

                            Console.WriteLine("{0} about to Commit Transaction", testData.UserName);

                            transaction.Commit();

                            Console.WriteLine("{0} has Committed Transaction", testData.UserName);

                        }
                        catch (System.Data.SqlClient.SqlException ex)
                        {
                            Console.WriteLine("{0} Transaction failed. - Error Info: {1}", testData.UserName, GetExceptionMessage(ex));
                            
                            throw (ex);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("{0} Transaction failed. Error Info: {1}", testData.UserName, GetExceptionMessage(ex));

                            throw (ex);
                        }
                    }
                });
            }
```

## Implementing the Optimistic Offline Lock Pattern
Çakışmalar kaydetmeden önce tespit edilir ve bu pattern'e göre çakışma olasılığının düşük olduğu varsayılır.
Çakışma olaylarının takibi için **Version** numarası tutulur. Bu bilgi kaydetme sırasında db dekiyle aynı olup olmadığı kontrol edilerek işleme devam edilir. Eğer farklı bir durum varsa kullanıcıya bilgi dönülür. Bu tür bilgileri elde etmek için update işleminde where sorgusu içerisinde versiyon numarası da eklenilir. Ya da where koşulu içerisine herhangi bir property'nin eski değeri de gönderilebilir.(Yani sadece adı değiştiğinde kullanıcı bu işlemi yapamasın gibi örnek verilebilir). Bu da yine iş kurallarına bağlıdır.

EF Core tarafında tablo create ederken sütun olarak *rowversion* tipinde ekleme yapılabilir. Bu sütun auto-incrementing'dir ve 8 byte veri tutar.Sql tarafında timestamp olarak geçer. Entity tarafında da aşağıdaki gibi property tanımlanır.
```csharp
        [Timestamp]
        public byte[] RowVersion { get; set; }
```
EF Core burada where sorguları içerisine otomatik olarak versiyon numarasını ekler. Eğer çakışma olduğunda DbConcurrencyException hatası fırlatır.
Bunun yanında modelcreating kısmında da .IsRowVersion() metoduyla da ilgili property'nin timestamp olduğu belirtilmiş olunur.
Ek olarak version numarası yerine ilgili alanın değişip değişmediği kontrolü yapılmak isteniyorsa aşağıdaki gibi **ConcurrencyCheck** attribute'u eklenilir.
```csharp
    [ConcurrencyCheck]
    public string FirstName { get; set; }
```
Controller kısmında yapılabilecek örnek kod aşağıdaki gibidir. Burada DbUpdateConcurrencyException ile conflict olmuş db yazılamamış veriye erişim mümkündür. Buradan çekilen entry ile o verinin database'de bulunan veriler ile farklılığına bakılarak ona göre validation hatası verilebilir.

Buna ek olarak view tarafında da hidden field'a RowVersion'u eklememiz gerekmektedir.
```html
    <div asp-validation-summary="ModelOnly" class="text-danger"></div>
    <input asp-for="ID" type="hidden" />
    <input asp-for="RowVersion" type="hidden" /> 
```

```csharp
try
            {
                _changeRequestRepository.UpdateChangeRequest(model, _userManager.GetUserName(this.User));
                return RedirectToAction("Index", "Home");
            }catch(DbUpdateConcurrencyException ex)
            {
                var exceptionEntry = ex.Entries.Single();
                if(exceptionEntry.Entity.GetType() == typeof(ChangeRequest))
                {
                    var clientValues = (ChangeRequest)exceptionEntry.Entity;
                    var dbEntry = exceptionEntry.GetDatabaseValues();
                    if(dbEntry == null)
                    {
                        ModelState.AddModelError(string.Empty, "Unable to save changes.  Change Request was deleted by another user.");
                    }
                    else
                    {
                        var dbValues = (ChangeRequest)dbEntry.ToObject();
                        if(dbValues.Name != clientValues.Name)
                        {
                            ModelState.AddModelError("Name", $"Current Value: {dbValues.Name}");
                        }
                        if (dbValues.Summary != clientValues.Summary)
                        {
                            ModelState.AddModelError("Summary", $"Current Value: {dbValues.Summary}");
                        }
                        if (dbValues.Status != clientValues.Status)
                        {
                            ModelState.AddModelError("Status", $"Current Value: {dbValues.Status}");
                        }
                        if (dbValues.Priority != clientValues.Priority)
                        {
                            ModelState.AddModelError("Priority", $"Current Value: {dbValues.Priority}");
                        }
                        if (dbValues.Urgency != clientValues.Urgency)
                        {
                            ModelState.AddModelError("Urgency", $"Current Value: {dbValues.Urgency}");
                        }
                        if (dbValues.TargetDate != clientValues.TargetDate)
                        {
                            ModelState.AddModelError("TargetDate", $"Current Value: {dbValues.TargetDate}");
                        }
                        if (dbValues.ActualDate != clientValues.ActualDate)
                        {
                            ModelState.AddModelError("ActualDate", $"Current Value: {dbValues.ActualDate}");
                        }
                        if (dbValues.Owner != clientValues.Owner)
                        {
                            ModelState.AddModelError("Owner", $"Current Value: {dbValues.Owner}");
                        }
                        if (dbValues.Modified != clientValues.Modified)
                        {
                            ModelState.AddModelError("Modified", $"Current Value: {dbValues.Modified}");
                        }
                        if (dbValues.ModifiedBy != clientValues.ModifiedBy)
                        {
                            ModelState.AddModelError("ModifiedBy", $"Current Value: {dbValues.ModifiedBy}");
                        }

                        ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                            + "was modified by another user after you got the original value.  The "
                            + "edit operation was cancelled and the current values in the database "
                            + "have been displayed.");

                        model.RowVersion = (byte[])dbValues.RowVersion;
                        ModelState.Remove("RowVersion");
                    }
                }
            }catch(Exception ex)
            {
                ModelState.AddModelError("", "An error occurred saving the record.");
            }
            return View(model);
```
![ScreenShot](/Concurrency/Files/OptimisticLockSample.png) 

Entity'nin ilgişkili olduğu nesneler için değişikliklerin kontrolünün yapılmaması için aşağıdaki gibi TrackGraph metodu kullanılabilir.
```csharp
        public void UpdateChangeRequest(ChangeRequest cr, string currentUser)
        {
            cr.ModifiedBy = currentUser;
            cr.Modified = DateTime.Now;

            //_appDataContext.Update(cr);
            _appDataContext.ChangeTracker.TrackGraph(cr, e => UpdateStateOfItems(e));

            _appDataContext.SaveChanges();
        }

        private void UpdateStateOfItems(EntityEntryGraphNode node)
        {
            node.Entry.State = EntityState.Modified;
            if(node.Entry.Entity.GetType() == typeof(ChangeRequestTask))
            {
                node.Entry.State = EntityState.Unchanged;
            }
        }
```

Aynı şekilde silme işlemi için de entity tarafında update lock yapılamadığı için sorgu içerisinde updatelock yapılır. Bu şekilde select işleminden sonra başka birisnin işlem yapması engellenmiş olacaktır.

```csharp
        public bool DeleteChangeRequest(int Id, byte[] rv)
        {
            using(var transaction = _appDataContext.Database.BeginTransaction())
            {

                var cr = _appDataContext.ChangeRequests
                .FromSql("SELECT * FROM ChangeRequests WITH (UPDLOCK) WHERE Id = " + Id)
                .FirstOrDefault();

                if (cr == null)
                {
                    // record has been deleted by another user
                    return true;
                }else if (!cr.RowVersion.SequenceEqual<byte>(rv))
                {
                    // record has been modified by another user
                    return false;
                }

                _appDataContext.ChangeRequests.Remove(cr);
                _appDataContext.SaveChanges();
                transaction.Commit();
                return true;
            }

        }
```

## Implementing the Pessimistic Offline Lock Pattern
Conflictlerin çok olduğu durumlarda, roll back maliyeti de artmış olacaktır. Bu durumda pessimistic pattern kullanılabilir. Kullanımı için öncelikle lock tipi belirlenir, lock manager ve protokoller tanımlanır.
3 çeşit lock vardır
**Exclusive Write lock**: *Writes block writers.* Bir kullanıcı bir kaydı düzenlediğinde başka bir kullanıcı onu düzenleyemez. Read işlemleriyle ilgilenmez.

**Exclusive Read lock**:*Reader blocks others.* Bir kullanıcı bir kaydı görüntülediğinde başka bir kullanıcı tarafından erişilemez. Bu da concurrency'i azaltacaktır.

**Exclusive Read/Write lock**: *Writers block readers. Readers block writers. Multiple readers ok.*

### Lock Manager
İlgili lock'u onaylar veya reddeder. Neyin lock'lanacağına (*primary key olabilir*) ve lock'un sahibinin kim olduğuna karar verir(*business transaction, session id, user id olabilir*). İmplementasyonu kod taradında olabilir(*store etmek için session memory ya da hashtable kullanılabilir*). Ancak kod tarafında store etmek için de load balancer kullanımı gibi senaryoda işler kompleksleşebilir. Bunun yanında implementasyon için **database table** kullanılabilir.
- Lock table: Maps locks to owners. Serialized access
- Exclusive Read and exclusive Write: Unique constraint on object id
- Read/Write Lock: Multiple read locks. Potential for inconsistent reads

### Locking Protocol
Locklama ve lock'u bırakma olaylarıyla ilgilenir.
- Acquire lock before loading data. 
- Lock and load in system transaction.
- Lock primary key id of object.
- Release when business transaction completes.
- Lost session
  - Release on web session timeout
  - Invalidate lock after timestamp expiry
Pessimistic lock patterni için bir lock clas'ı oluşturulur. Bu lock class'ı içerisinde neyi locklayacağımız bilgisi ve neye göre ya da kime göre lock'lama yapacağımız bilgisi tutulur. Bu lock objesi database'de tutulur. Kişi bir veri güncellemesi yaparken yeni bir lock objesi üretilir ve db ye kaydedilir. Kişi logout olasıya,session düşesiye kadar ya da işlemini tamamlayasıya kadar lock db de saklanılır. Bu süre içerisinde aynı kayıt ile işlem yapmaya çalışan kişi için giriş yapılmasına izin verilmez. Sonuç olarak bir kayıt aynı anda sadece bir kişi tarafından güncellenebilir olacaktır.
İlgili kodlar aşağıdaki gibidir.

```csharp
    public class Lock
    {

        // EntityId and EntityName will be the composite key
        // composite keys can only be configured using the fluent API
        public int EntityId { get; set; }
        public string EntityName { get; set; }
        public string OwnerId { get; set; }
        // Acquired DateTime will be used for releasing abandoned locks
        public DateTime AcquiredDateTime { get; set; }

    }
```
Dbcontext tarafında
```csharp
        public DbSet<Lock> Lock { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // composite keys can only be configured using the fluent API
            builder.Entity<Lock>().HasKey(table => new { table.EntityId, table.EntityName });

            base.OnModelCreating(builder);
        }
```

Lock tarafında meydana gelen hataları ayıklamak için 
```csharp
    public class ConcurrencyException : Exception
    {
        public ConcurrencyException()
        {
        }

        public ConcurrencyException(string message)
            : base(message)
        {
        }

    }
```

Lock manager ise mevcut lock'ların bb'ye eklenmesi ve silinmesi işlemlerini yürütür. RemoveExpiredLock kısmı ise  aşağıdaki sql scriptini çalıştıracaktır. Bu şekilde expire edilmiş lock'lar silinmiş olacaktır. Bunu da yeni lock talep edildiğinde çağırabiliriz(*AcquireLock metodunda*) Benzer şekilde lock'un yenilenmesi için de RenewLock metodu kullanılır.
```sql
CREATE FUNCTION GetLockWithUPDLock(@entityId int, @entityName nvarchar(450))
RETURNS TABLE 
AS 
RETURN 
SELECT Entityld, EntityName, AcquiredDateTime, Ownerld 
FROM dbo. Locks WITH (UPDLOCK) 
WHERE Entityld= @entityld AND EntityName=@entityName 
```

```csharp
    public class LockManager
    {
        private readonly int _lockExpirySeconds = 30;

        AppDataContext _appDataContext;
        public LockManager(AppDataContext appDataContext)
        {
            _appDataContext = appDataContext;
        }

        public void AcquireLock(int id, string entityName, string owner)
        {
            RemoveExpiredLock(id, entityName);

            if (!HasLock(id, entityName, owner))
            {
                try
                {
                    Lock _lock = new Lock
                    {
                        EntityId = id,
                        EntityName = entityName,
                        OwnerId = owner,
                        AcquiredDateTime = DateTime.Now
                    };

                    _appDataContext.Lock.Add(_lock);
                    _appDataContext.SaveChanges();

                }
                catch (DbUpdateException)
                {
                    // an error occured inserting the data
                    throw new ConcurrencyException("Entity is locked by another user and cannot be edited at this time.");
                }
            }
        }

        public void ReleaseLock(int id, string entityName, string owner)
        {
            if (HasLock(id, entityName, owner))
            {
                Lock _lock = _appDataContext.Lock
                    .FirstOrDefault(c => c.EntityId == id && c.EntityName == entityName && c.OwnerId == owner);

                if (_lock != null)
                {
                    try
                    {
                        _appDataContext.Lock.Remove(_lock);
                        _appDataContext.SaveChanges();

                    }
                    catch (Exception)
                    {
                        throw new ConcurrencyException("Unexpected error releasing lock on Entity");
                    }
                }
            }
        }

        public bool HasLock(int id, string entityName, string owner)
        {
            Lock _lock = _appDataContext.Lock
                .AsNoTracking()
                .FirstOrDefault(c => c.EntityId == id && c.EntityName == entityName && c.OwnerId == owner);
            return _lock != null ? true : false;
        }

        public void ReleaseAllLocks(string owner)
        {
            Lock[] _locks = _appDataContext.Lock
                .Where(c => c.OwnerId == owner).ToArray();
            if (_locks != null)
            {
                foreach (var _lock in _locks)
                {
                    _appDataContext.Lock.Remove(_lock);
                }
                _appDataContext.SaveChanges();
            }
        }

        private void RemoveExpiredLock(int id, string entityName)
        {

            Lock _lock = _appDataContext.Lock
                .FromSql("SELECT * FROM dbo.GetLockWithUPDLock({0},{1})", id, entityName)
                .AsNoTracking()
                .FirstOrDefault();

                if (_lock != null && _lock.AcquiredDateTime <= DateTime.Now.AddSeconds(-_lockExpirySeconds))
                {
                    // delete the expired lock for another user
                    _appDataContext.Lock.Remove(_lock);
                    _appDataContext.SaveChanges();

                }
        }

        public void RenewLock(int id, string entityName, string currentUser)
        {
            Lock _lock = _appDataContext.Lock
                .FromSql("SELECT * FROM dbo.GetLockWithUPDLock({0},{1})", id, entityName)
                .FirstOrDefault();

            if (_lock != null && _lock.OwnerId == currentUser)
            {
                if(_lock.AcquiredDateTime <= DateTime.Now.AddSeconds(-_lockExpirySeconds))
                {
                    throw new ConcurrencyException("Lock on Entity has expired.  Please restart Business Transaction.");
                }

                _lock.AcquiredDateTime = DateTime.Now;
                _appDataContext.SaveChanges();

            }
            else
            {
                throw new ConcurrencyException("Lock not found for user and Entity");
            }
        }
    }
```

Düzenlenecek öğe için ilgili repository katmanında aşağıdaki işlemler uygulanacak ilgili öğenin lock'a sahip olup olmadığı kontrol edilir
```csharp
        public ChangeRequestTask GetChangeRequestTaskbyIdForEdit(int id, string currentUser)
        {
            using (var transaction = _appDataContext.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                try
                {
                    // using Serializable because we want repeatable reads for the lock manager,
                    // but the change repository doesn't know if this is a read/write lock 
                    // - where phantom rows would be an issue

                    LockManager lockManager = new LockManager(_appDataContext);
                    lockManager.AcquireLock(id, "ChangeRequestTask", currentUser);

                    ChangeRequestTask cr = _appDataContext.ChangeRequestTasks.Where(c => c.ID == id)
                            .SingleOrDefault();

                    if (cr != null)
                    {
                        transaction.Commit();
                    }
                    else
                    {
                        throw new ConcurrencyException("Entity Not Found.  Entity may have been deleted by another user.");
                    }

                    return cr;

                }
                catch (ConcurrencyException ex)
                {
                    transaction.Rollback();
                    string newMessage = ex.Message.Replace("Entity", "Change Request Task " + id.ToString("D5"));
                    throw new ConcurrencyException(newMessage);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }

            }
        }
```

Düzenleme ve silme işlemi için
```csharp
        public void UpdateChangeRequestTask(ChangeRequestTask task, string currentUser)
        {
            task.ModifiedBy = currentUser;
            task.Modified = DateTime.Now;

            using (var transaction = _appDataContext.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                try
                {
                    LockManager lockManager = new LockManager(_appDataContext);

                    // prevent lost updates if lock expires
                    if(lockManager.HasLock(task.ID, "ChangeRequestTask", currentUser))
                    {
                        _appDataContext.ChangeRequestTasks.Update(task);
                        _appDataContext.SaveChanges();

                        lockManager.ReleaseLock(task.ID, "ChangeRequestTask", currentUser);

                        transaction.Commit();
                    }
                    else
                    {
                        // the user does not have a lock on the record
                        throw new ConcurrencyException("User does not have a lock on Entity.  "
                            + "This may be due to a timeout.  "
                            + "Please reload record and restart editing to prevent overwriting another user's changes.");
                    }
                }
                catch(ConcurrencyException ex)
                {
                    transaction.Rollback();
                    string newMessage = ex.Message.Replace("Entity", "Change Request Task " + task.ID.ToString("D5"));
                    throw new ConcurrencyException(newMessage);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }

                public bool DeleteChangeRequestTask(int id, string currentUser)
        {
            using (var transaction = _appDataContext.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                try
                {
                    LockManager lockManager = new LockManager(_appDataContext);

                    // prevent lost updates if lock expires
                    if (lockManager.HasLock(id, "ChangeRequestTask", currentUser))
                    {

                        var cr = _appDataContext.ChangeRequestTasks
                        .FromSql("SELECT * FROM ChangeRequestTasks WITH (UPDLOCK) WHERE Id = {0}", id)
                        .FirstOrDefault();

                        _appDataContext.ChangeRequestTasks.Remove(cr);
                        _appDataContext.SaveChanges();

                        lockManager.ReleaseLock(cr.ID, "ChangeRequestTask", currentUser);

                        transaction.Commit();
                        return true;
                    }
                    else
                    {
                        // the user does not have a lock on the record
                        throw new ConcurrencyException("User does not have a lock on Entity.  "
                            + "This may be due to a timeout.  "
                            + "Please reload record and restart editing to prevent overwriting another user's changes.");
                    }

                }
                catch (ConcurrencyException ex)
                {
                    transaction.Rollback();
                    string newMessage = ex.Message.Replace("Entity", "Change Request Task " + id.ToString("D5"));
                    throw new ConcurrencyException(newMessage);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }
```
Controller kısmında ise 
```csharp
        public IActionResult Edit(int id, int changeRequestId)
        {
            //var task = _changeRequestRepository.GetChangeRequestTaskbyId(id);
            //return View(task.Result);
            try
            {
                var task = _changeRequestRepository.GetChangeRequestTaskbyIdForEdit(id, _userManager.GetUserName(this.User));
                return View(task);
            }
            catch (ConcurrencyException ex)
            {
                // cannot open record for editing
                TempData["ConcurrencyError"] = ex.Message;
                return RedirectToAction("Edit", "ChangeRequest", new { id = changeRequestId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ChangeRequestTask task)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _changeRequestRepository.UpdateChangeRequestTask(task, _userManager.GetUserName(this.User));
                    return RedirectToAction("Edit", "ChangeRequest", new { id = task.ChangeRequestID });

                }catch(ConcurrencyException ex)
                {
                    // user's edit lock has expired
                    ModelState.AddModelError("", ex.Message);
                    return View("Edit", task);
                }
            }
            else
            {
                ModelState.AddModelError("", "Values are not valid");
                return View("Edit", task);
            }
        }

        public IActionResult Delete(int id, int changeRequestId)
        {
            try
            {
                var task = _changeRequestRepository.GetChangeRequestTaskbyIdForEdit(id, _userManager.GetUserName(this.User));
                return View(task);
            }
            catch (ConcurrencyException ex)
            {
                // cannot open record for editing
                TempData["ConcurrencyError"] = ex.Message;
                return RedirectToAction("Edit", "ChangeRequest", new { id = changeRequestId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(ChangeRequestTask task)
        {
            try
            {
                bool result = _changeRequestRepository.DeleteChangeRequestTask(task.ID, _userManager.GetUserName(this.User));
            }
            catch (ConcurrencyException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(task);
            }
            return RedirectToAction("Edit", "ChangeRequest", new { id = task.ChangeRequestID });

        }
```

### 5- Implementing the Coarse-grained Lock Pattern
**Coarse Grained Lock Pattern:** Bir öğenin ilişkili olduğu nesneler varsa onları da lock'lama işlemidir.
- **Shared Lock:** All objects reference the same lock

![ScreenShot](/Concurrency/Files/CearseGrained-SharedLock.png) 

Shared lock ile kullanım örneği olarak: Bir kullanıcı task girişi yapsın ve taska ait birden fazla not eklesin. Burada not ekleme kısımlarında hiçbir işlem task (*yani ana kayıt*) eklenesiye kadar db'ye işlem yapılmacaktır. Burada cancel etme işlemini db yi yormadan yapmak ve optimistic kullanımda başka birisi aynı kaydı güncelleme işlemi gerçekleştirdiğinde çakışma durumunda hangi kayıtların ne şekilde güncellediğine dair bilgi kullanıcıya gösterme gibi işlemler yapılabilir.

- **Root Lock:** Root object provides access and ownds lock
![ScreenShot](/Concurrency/Files/CearseGrained-RootLock.png) 

Root lock kullanımında lazy loading kısmında dikkatli olmak gerekli. Çünkü bu aşamada veri değiştirilmiş olabilir.

Coarse-grained lock patterni single lock kullanmaktan daha etkilidir ancak birkaç dezavantajı da vardır.
- Potentially many database joins
- Performance and freshness when navigating through object hierarchies

Örnek uygulama için session aktif edilmelidir. Bunun için de ilgili memory cache de eklenilmelidir.

```csharp
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(20);
                options.Cookie.HttpOnly = true;
            });

            
```
```csharp
// app.UseSession must be BEFORE UseMvcWithDefaultRoute
app.UseSession();
```
Session state btye array şeklinde tuttuğu için int ya da string tutabilmesi için aşağıdaki gibi bir extension class'ı eklenilir.

```csharp
    public static class SessionExtensions
    { 
        public static void Set<T>(this ISession session, string key, T value)
        {
            JsonSerializerSettings jss = new JsonSerializerSettings();
            jss.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            session.SetString(key, JsonConvert.SerializeObject(value, jss));
        }

        public static T Get<T>(this ISession session, string key)
        {
            JsonSerializerSettings jss = new JsonSerializerSettings();
            jss.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            var value = session.GetString(key);
            return value == null ? default(T) :
                                  JsonConvert.DeserializeObject<T>(value, jss);
        }
    }
```
Repository katmanında aşağıdaki gibi sessionkeyName eklenilir.
```csharp
        const string _sessionKeyName = nameof(ChangeRequest);
        private static string SessionKeyName => _sessionKeyName;
```
Yine repository kısmına aşağıdaki session yönetimi ile ilgili metotlar eklenilir
```csharp
        public void StartBusinessTransaction(ChangeRequest entity)
        {
            _context.HttpContext.Session.Set<ChangeRequest>(SessionKeyName, entity);
        }

        public void EndBusinessTransaction()
        {
            _context.HttpContext.Session.Set<ChangeRequest>(SessionKeyName, null);
        }

        private void UpdateBusinessEntity(ChangeRequest entity)
        {
            _context.HttpContext.Session.Set<ChangeRequest>(SessionKeyName, entity);
        }

        protected ChangeRequest GetCurrentEntityFromSession()
        {
            return _context.HttpContext.Session.Get<ChangeRequest>(SessionKeyName);
        }

        public void ContinueBusinessTransaction(ChangeRequest entity)
        {
            var changeRequest = ContinueBusinessTransaction(entity.ID);
            UpdateChangeRequestProperties(entity, changeRequest);
            UpdateBusinessEntity(changeRequest);
        }

        public ChangeRequest ContinueBusinessTransaction(int id)
        {
            // get ChangeRequest in session
            ChangeRequest cr = GetCurrentEntityFromSession();

            if (cr != null)
            {
                // verify the id passed from the controller
                if (cr.ID == id)
                {
                    return cr;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private void UpdateChangeRequestProperties(ChangeRequest newCr, ChangeRequest oldCr)
        {
            oldCr.Name = newCr.Name;
            oldCr.Owner = newCr.Owner;
            oldCr.Summary = newCr.Summary;
            oldCr.TargetDate = newCr.TargetDate;
            oldCr.Priority = newCr.Priority;
            oldCr.Urgency = newCr.Urgency;
            oldCr.Status = newCr.Status;
            oldCr.ActualDate = newCr.ActualDate;

            oldCr.SharedVersion.RowVersion = newCr.SharedVersion.RowVersion;
            oldCr.State = TrackedEntityState.Modified;

            oldCr.ModifiedBy = CurrentUser;
            oldCr.Modified = DateTime.Now;
        }
```

UpdateChangeRequest metodu aşağıdaki gibi değiştirilecektir.
```csharp
        /// <summary>
        /// Update existing ChangeRequest and graph records in database
        /// </summary>
        /// <param name="cr"></param>
        public void UpdateChangeRequest(ChangeRequest changeRequest)
        {
            var changeRequestToSave = ContinueBusinessTransaction(changeRequest.ID);
            UpdateChangeRequestProperties(changeRequest, changeRequestToSave);

            DbContext.ChangeTracker.TrackGraph(changeRequestToSave, e => UpdateStateOfItems(e));
            DbContext.SaveChanges();
        }
```
Burada yeni bir talep taskı eklenildiğinde mevcut transaction aşağıdaki gibi takip edilir
```csharp
        public ChangeRequestTask CreateNewChangeRequestTask(int changeRequestId)
        {
            var changeRequest = ContinueBusinessTransaction(changeRequestId);
            if(changeRequest != null)
            {
                // Create new task and associate to ChangeRequest
                var task = new ChangeRequestTask();
                task.ChangeRequestID = changeRequest.ID;
                task.ChangeRequest = changeRequest;

                // used later to help find within collection prior to primary key being generated
                task.Modified = DateTime.Now;

                return task;

            }
            else
            {
                throw new Exception("Error creating task.");
            }

        }
                public int CreateChangeRequestTask(ChangeRequestTask task)
        {
            task.ModifiedBy = CurrentUser;
            task.Modified = DateTime.Now;

            task.State = TrackedEntityState.Added;

            var changeRequest = ContinueBusinessTransaction(task.ChangeRequestID);

            if(changeRequest.ChangeRequestTasks == null)
            {
                changeRequest.ChangeRequestTasks = new List<ChangeRequestTask>();
            }

            task.SharedVersionId = changeRequest.SharedVersion.ID;
            task.SharedVersion = changeRequest.SharedVersion;

            changeRequest.ChangeRequestTasks.Add(task);

            UpdateBusinessEntity(changeRequest);

            return task.ID;
        }
```
Veri getirme,güncelleme ve silme kısmında da session kullanımı aşağıdaki gibi olacaktır.
```csharp
        public ChangeRequestTask GetChangeRequestTaskbyId(int taskId, string modified)
        {
            var changeRequest = this.GetCurrentEntityFromSession();
            ChangeRequestTask task;

            if(taskId == 0)
            {
                string dt = Convert.ToDateTime(modified).ToLongTimeString();
                task = changeRequest.ChangeRequestTasks.Find(t => t.ID == taskId && t.Modified.ToLongTimeString() == dt);
            }
            else
            {
                task = changeRequest.ChangeRequestTasks.Find(t => t.ID == taskId);
            }

            return task;
        }

        public void UpdateChangeRequestTask(ChangeRequestTask task)
        {
            var changeRequest = this.GetCurrentEntityFromSession();
            ChangeRequestTask retrievedTask;

            if (task.ID == 0)
            {
                string dt = Convert.ToDateTime(task.Modified).ToLongTimeString();
                retrievedTask = changeRequest.ChangeRequestTasks.Find(t => t.ID == task.ID && t.Modified.ToLongTimeString() == dt);
            }
            else
            {
                retrievedTask = changeRequest.ChangeRequestTasks.Find(t => t.ID == task.ID);
            }

            retrievedTask.CompletedDate = task.CompletedDate;
            retrievedTask.Name = task.Name;
            retrievedTask.Status = task.Status;
            retrievedTask.Summary = task.Summary;

            retrievedTask.State = TrackedEntityState.Modified;
            retrievedTask.ModifiedBy = currentUser;
            retrievedTask.Modified = DateTime.Now;

            UpdateBusinessEntity(changeRequest);
        }

        public bool DeleteChangeRequestTask(ChangeRequestTask task)
        {
            var changeRequest = ContinueBusinessTransaction(task.ChangeRequestID);

            ChangeRequestTask retrievedTask;

            if (task.ID == 0)
            {
                retrievedTask = changeRequest.ChangeRequestTasks.Find(t => t.ID == task.ID && t.Modified.ToLongTimeString() == task.Modified.ToLongTimeString());
            }
            else
            {
                retrievedTask = changeRequest.ChangeRequestTasks.Find(t => t.ID == task.ID);
            }

            retrievedTask.State = TrackedEntityState.Deleted;

            // in case we implement logging later
            retrievedTask.Modified = DateTime.Now;
            retrievedTask.ModifiedBy = CurrentUser;

            UpdateBusinessEntity(changeRequest);

            return true;
        }
```
Controller kısmında ise aşağıdaki eklemeler yapılır.
```csharp
        public IActionResult Edit(int id)
        {
            var changeRequest = _changeRequestRepository.ContinueBusinessTransaction(id);

            if(changeRequest == null)
            {
                changeRequest = _changeRequestRepository.GetChangeRequestbyId(id);
                _changeRequestRepository.StartBusinessTransaction(changeRequest);
            }

            ChangeRequestViewModel vm = new ChangeRequestViewModel
            {
                ChangeRequest = changeRequest
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ChangeRequestViewModel model)
        {
            try
            {
                _changeRequestRepository.UpdateChangeRequest(model.ChangeRequest);
                _changeRequestRepository.EndBusinessTransaction();
                return RedirectToAction("Index", "Home");
            }
        }
        [HttpPost]
        public IActionResult Cancel()
        {
            _changeRequestRepository.EndBusinessTransaction();
            return RedirectToAction("Index", "Home");
        }
```
Controller içerisinde veri getirme kısmı da aşağıdaki gibi değişecektir
```csharp
            var changeRequest = _changeRequestRepository.ContinueBusinessTransaction(id);
```
Edit summary kısmı da aynı transaction'ı kullanmalıdır
```csharp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditSummary(ChangeRequest model, string Save, string Cancel)
        {
            if(Save != null)
            {
                var changeRequest = _changeRequestRepository.ContinueBusinessTransaction(model.ID);
                changeRequest.Summary = model.Summary;
                _changeRequestRepository.ContinueBusinessTransaction(changeRequest);
            }
            return RedirectToAction("Edit", new { id = model.ID });
        }
```
ViewModel tarafına aşağıdaki eklemer yapılır
```csharp
        public int TaskId { get; set; }
        public DateTime TaskModifiedTime { get; set; }
```
View tarafında kullanmak için
```html
<script type="text/javascript">
    function SetTaskAction(id, modified) {
        document.getElementById('taskId').value = id;
        document.getElementById('taskModifiedTime').value = modified;
    }
</script>
    <input asp-for="ChangeRequest.ID" type="hidden" />
    <input asp-for="TaskId" id="taskId" type="hidden" />
    <input asp-for="ChangeRequest.SharedVersionId" type="hidden" />
    <input asp-for="ChangeRequest.SharedVersion.ID" type="hidden" />
    <input asp-for="ChangeRequest.SharedVersion.RowVersion" type="hidden" />
            @for (var i = 0; i < Model.ChangeRequest.ChangeRequestTasks.Count; i++)
            {
                <tr style="@(Model.ChangeRequest.ChangeRequestTasks[i].State==TrackedEntityState.Deleted?"display:none":"display:")">
                    <td><input type="submit" formaction="EditChangeRequestTask" formmethod="post" onclick="SetTaskAction(@Model.ChangeRequest.ChangeRequestTasks[i].ID, '@Model.ChangeRequest.ChangeRequestTasks[i].Modified')" id="edit@Model.ChangeRequest.ChangeRequestTasks[i].ID" value="Edit" /></td>
                    <td><input type="submit" formaction="DeleteChangeRequestTask" formmethod="post" onclick="SetTaskAction(@Model.ChangeRequest.ChangeRequestTasks[i].ID, '@Model.ChangeRequest.ChangeRequestTasks[i].Modified')" id="delete@Model.ChangeRequest.ChangeRequestTasks[i].ID" value="Delete" /></td>                    <td>
                        <input asp-for="@Model.ChangeRequest.ChangeRequestTasks[i].Summary" type="hidden" />
                        <input asp-for="@Model.ChangeRequest.ChangeRequestTasks[i].CompletedDate" type="hidden" />
                        <input asp-for="@Model.ChangeRequest.ChangeRequestTasks[i].ChangeRequestID" type="hidden" />
                        <input asp-for="@Model.ChangeRequest.ChangeRequestTasks[i].Modified" type="hidden" />
                    </td>
                </tr>
            }

```

Bununla birlikte dbcontext disconnected olabilir. Bu durumda entity'leri track graph ile izlememiz gerekecektir.Burada her bir entity için EntityBase Classı oluşturulur ve bu class aracılığıyla değişiklikler takip edilir.
```csharp
    public abstract class EntityBase : IEntityBase
    {
        public int ID { get; set; }
        public DateTime Modified { get; set; }
        public string ModifiedBy { get; set; }

        [NotMapped]
        public TrackedEntityState State { get; set; }
    }
```

Buradaki ilgili metot aşağıdaki gibi trackgraph eklenerek düzenlenecektir.
```csharp
        public bool DeleteChangeRequest(int id)
        {
            ChangeRequest changeRequestToDelete = GetChangeRequestbyId(id);
            changeRequestToDelete.State = TrackedEntityState.Deleted;
            DbContext.ChangeTracker.TrackGraph(changeRequestToDelete, e => UpdateStateOfItems(e));
            DbContext.SaveChanges();

            return true;
        }

        private void UpdateStateOfItems(EntityEntryGraphNode node)
        {
            if (node.Entry.Entity.GetType().BaseType == typeof(EntityBase))
            {
                if (((EntityBase)node.Entry.Entity).State == TrackedEntityState.Added)
                {
                    node.Entry.State = EntityState.Added;
                }
                else if (((EntityBase)node.Entry.Entity).State == TrackedEntityState.Modified)
                {
                    node.Entry.State = EntityState.Modified;
                }
                else if (((EntityBase)node.Entry.Entity).State == TrackedEntityState.Deleted)
                {
                    if (((EntityBase)node.Entry.Entity).ID == 0)
                    {
                        node.Entry.State = EntityState.Unchanged;
                    }
                    else
                    {
                        node.Entry.State = EntityState.Deleted;
                    }
                }
            }
            else
            {
                if (node.Entry.IsKeySet)
                {
                    node.Entry.State = EntityState.Modified;
                }
                else if (node.Entry.State != EntityState.Deleted)
                {
                    node.Entry.State = EntityState.Added;
                    ((EntityBase)node.Entry.Entity).ID = 0;
                }
            }
            if (node.Entry.Entity.GetType() == typeof(Version))
            {
                if (node.Entry.State != EntityState.Added)
                {
                    node.Entry.State = EntityState.Modified;
                }
                node.Entry.CurrentValues["Modified"] = DateTime.Now;
                node.Entry.CurrentValues["ModifiedBy"] = CurrentUser;
            }
        }
```

Coarse-grained lock patterni hem optimistic hem pessimistic olarak kullanılabilir.

### 6-Implementing the Implicist Lock Pattern
Locklamanın developer'lar tarafından değil uygulama tarafından olması gerektiğini söyler. Diğer türlü developer tarafından lock'lama gibi işlemler unutulabilir. Bu da tutarlılığı düşürür. 
- Ensure no gaps in use of locking strategy
- Framework
  - Base Classes
  - Plumbing code
  - Code generation

Buada bir önceki işlemlere ek olarak Repository katmanı generic hale getirlir ve ilişkiler de dinamik olarak kontrol edilir.
```csharp
    public class GenericRepository<TEntity> : IGenericRepository<TEntity>
        where TEntity : EntityBase
    {
        private readonly AppDataContext _dbContext;
        private readonly IHttpContextAccessor _context;
        private string currentUser;
        private IQueryable<TEntity> query;

        protected AppDataContext DbContext => _dbContext;
        protected IHttpContextAccessor Context => _context;
        protected string CurrentUser { get => currentUser; set => currentUser = value; }

        public GenericRepository(AppDataContext dbContext, IHttpContextAccessor contextAccessor)
        {
            _dbContext = dbContext;
            _context = contextAccessor;
            CurrentUser = _context.HttpContext.User.Identity.Name;
        } 

        public void Create(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public void Update(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public IQueryable<TEntity> GetAll()
        {
            return DbContext.Set<TEntity>().AsNoTracking();
        }

        public TEntity GetById(int id)
        {
            query = DbContext.Set<TEntity>().AsQueryable();

            return Query()
                .AsNoTracking()
                .FirstOrDefault(e => e.ID == id);
        }

        public virtual IQueryable<TEntity> Query()
        {
            IEntityType entity = DbContext.Model.FindEntityType(typeof(TEntity));
            BuildNavigationIncludes(entity, null, string.Empty);

            return query;
        }

        private void BuildNavigationIncludes(IEntityType entity, IEntityType parent, string parentPath)
        {
            foreach(var property in entity.GetNavigations())
            {
                if(property.GetTargetType() != parent)
                {
                    string includePropertyName = "";
                    if(parent != null && parentPath != string.Empty)
                    {
                        includePropertyName = parentPath + ".";
                    }
                    includePropertyName += property.Name;

                    query = query.Include(includePropertyName);
                    IEntityType child = property.GetTargetType();
                    BuildNavigationIncludes(child, entity, includePropertyName);
                }
            }
        }

        protected void UpdateStateOfItems(EntityEntryGraphNode node)
        {
            if (node.Entry.Entity.GetType().BaseType == typeof(EntityBase))
            {
                if (((EntityBase)node.Entry.Entity).State == TrackedEntityState.Added)
                {
                    node.Entry.State = EntityState.Added;
                }
                else if (((EntityBase)node.Entry.Entity).State == TrackedEntityState.Modified)
                {
                    node.Entry.State = EntityState.Modified;
                }
                else if (((EntityBase)node.Entry.Entity).State == TrackedEntityState.Deleted)
                {
                    if (((EntityBase)node.Entry.Entity).ID == 0)
                    {
                        node.Entry.State = EntityState.Unchanged;
                    }
                    else
                    {
                        node.Entry.State = EntityState.Deleted;
                    }
                }
            }
            else
            {
                if (node.Entry.IsKeySet)
                {
                    node.Entry.State = EntityState.Modified;
                }
                else if (node.Entry.State != EntityState.Deleted)
                {
                    node.Entry.State = EntityState.Added;
                    ((EntityBase)node.Entry.Entity).ID = 0;
                }
            }

            if (node.Entry.Entity.GetType().GetInterfaces().Contains(typeof(ISharedLockable)))
            {
                if(node.Entry.Reference("SharedVersion").CurrentValue == null)
                {
                    throw new Exception("Version not loaded for lockable entity.");
                }
            }

            if (node.Entry.Entity.GetType() == typeof(Version))
            {
                if (node.Entry.State != EntityState.Added)
                {
                    node.Entry.State = EntityState.Modified;
                }
                node.Entry.CurrentValues["Modified"] = DateTime.Now;
                node.Entry.CurrentValues["ModifiedBy"] = CurrentUser;
            }
        }
    }
```
