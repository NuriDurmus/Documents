
### Transaction
Tek bir iş birimi gibi çalışan operasyonlar dizisidir. Bu şekilde bir hata olması durumunda database'in stabil olmasını sağlar ve veriyi kurtarmayı sağlar. Bununla birlikte eşzamanlı erişim durumlarında işlemler arasındaki izolasyonu sağlar.  
#### ACID properties of Transactions
Atomicity: All or nothing
Consistency: Data Integrity when complete. Transaction sonunda veritabanındaki bir durumun bir sonraki geçerli duruma geçmesini garanti altına alır. Database'e yazılan herhangi bir verinin belirlenen kurallara göre geçerli olmalıdır. Bu kurallar constraint,cascade,trigger'lar olabilir.
Isolation: Modifications isolated from other transactions. Bir transaction tamamlanasıya kadar diğer transaction tarafından görülmemelidir.
Durability: Effects are permanent.Sistem çöktüğünde ya da restart edildiğinde data hala saklanmış olmalıdır.

### Concurrency Control
#### Optimistic 
Birçok transaction'ın genellikle bir diğerini etkilemediğini varsayar. Bunun için veri okumada lock olmaz. Sadece veri kayıt olduğunda çakışmaları kontrol eder.
Kaydetme sırasında veri okunduktan sonra sistem başka bir transaction tarafından verinin değişip değişmediğini kontrol eder. Bunu da genelde verinin versiyon numarası ile custom oluşturulan fieldlar ile kontrol eder. Bu durumda da bir conflict olduğunda da transaction roll back edilir. Optimistic concurrency control genellikle veri çakışmalarının az olduğu durumlarda kullanılır. Burada roll back yapmanın maliyeti veriyi lock'lamaktan daha azdır.
#### Pessimistic
Düzenlemede ve hatta okuma işlemlerinde veri bütünlüğü için locklama işlemi yapılır. Başka bir kullanıcı Lock kalkasıya kadar hiçbir işlem yapamaz. Bu şekilde sistemin eşzamanlı çalışmasını azaltmış olur. Veri çakışmalarının çok olması durumunda kullanılır ve locklama maliyeyi transaction roll back etme maliyetinden azdır.

#### Lost Update
İki kullanıcı aynı kaydı okur güncellemye başlar. User2 güncellemesini User1 güncelleme işini bitirmeden yapar. Bu durumnda User1 güncellenen veriden habersiz kendisi de güncellenmiş verinin üzerine yazacaktır.

#### Dirty Read (Uncommited Dependency)
User1 işlem yaparken commitlenmeden User2 ilgili veriyi okur.Örnek olarak User1 x değerini 5'ten 7 ye güncellesin. Bu durumda commitlenmeden okuyan User2 x değerini 7 gibi görecektir. Ancak burada User1 yaptığı işlem commitlenmediğinde ya da roll back olduğunda veri eski haline gelecektir. Ancak User2 bu durumdan haberdar olmadığı için verinin 7 olduğunu düşünecektir.

#### Nonrepeatable Read
User1 bir veri setini alır ve User2 bundan sonra veriyi günceller. User1 ikinci defa veriyi okuduğunda güncellenmiş veriyi görecektir.  Bu durum aynı transaction içerisinde veriyi iki defa okunmadığı durumda sorun olmayacaktır ancak transaction içerisinde veri iki defa okunuyorsa ilk okuma ile ikinci okuma arasında fark olacaktır.
![](file/NonrepeatableRead.png) 

#### Phantom Read
Aynı nonrepeatable read gibidir ancak birden fazla veri seti için geçerlidir. User1 bir veri çeker ve o sırada User2 o veri seti için bir ekleme yapar User1 transaction içerisinde yeni bir sorgu daha çektiğinde User2 nin eklediği veriyi de almış olacaktır.  

#### Missing or Double Reads
User1 büyük bir veri seti çeksin. Bu süreç içerisinde User2 veri setinde güncelleme yapmış olabilir ve burada index'te bir değişiklik yapmış olabilir. Bu durumda yeni indexte daha önceden okumuş olduğu veriyi tekrar okuma yapabilir. Ya da tam tersi okuma işleminin devamında olması gereken veri yer değiştirdiği için kayıp olabilir.
![](file/MissingOrDoubleReads.gif) 

Bu tür durumlar locking data ile ya da row versioning ile çözülebilir. 

### Isolation Level
Veritabanında transaction işlemi gerçekleştirdiğinizde bu transaction belirli bir isolation level altında çalışır. Isolation level bir transaction'ın başka bir transction tarafından hangi düzeyde yalıtılacağını tanımlar. Isolation level verinin okundığında lock'lanıp lock'lanmayacağını, hanti tip lock'un talep edildiğini kontrol eder. Ayrıca Lock'un ne kadar tutulacağını da kontrol eder ve read etmeye çalışıldığında bu veri başka bir transaction tarafından değiştirildiğinde nasıl davranacağına karar verir. 
En temel lock tipleri **read** ve **exclusive** locklar'dır. 
> Read locklar SQL server tarafından share edilirler. Yani birden fazla transaction veriyi okuyabilir.  Bu tip lock read işlemi sonrasında relase edilebilir ya da transaction sonuna kadar saklanabilir. 
> Exclusive locklar silinme ve güncellenme olasılığna karşın kullanılır. Bu tip lock için sadece bir transaction çalışır. Bu şekilde diğer lock'lar buradaki veriye erişemeyecektir. Ancak bu deadlocklar için önemli bir husustur. 
Bunlara ek olarak **Key Range** ve **Update Locks** da mevcuttur.

Isolation level'ı lower ve higher olarak ikiye ayırabiliriz.
**Lower Isolation Level:** Birden fazla kullanıcının aynı anda işlem yapmasına olanak tanır. Ancak bu da concurrency sorunlarını daha çok doğuracağı anlamına gelir.
**Higher Isolation Level:** Concurrency sorunlarını azaltır. Ancak daha fazla sistem kaynağı tüketecektir. Transactionların başkalarını engelleme olasılığını arttırır hatta deadlock'a neden olabilir.

### Isolation Levels
- Read Uncommitted