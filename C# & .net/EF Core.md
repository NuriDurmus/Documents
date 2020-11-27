### EF 5 ile gelen özellikler

Many to many tablolarımız bulunsun. 
Ancak burada ef5 ile gelen özellik sayesinde membership entity'mizi silebiliriz. Onun yerine ilişkileri User içerisinde Group listesi ve Group içerisinde User listesi verecek şekilde güncelleyebiliriz. DbContext tarafından kendisi Membership yerine kullanılacak olan tabloyu (UserGroups isminde) otomatik olarak oluşturur.

```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Membership> Memberships { get; set; }
}
public class Group
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Membership> Memberships { get; set; }
}

public class Membership
{
    public int Id { get; set; }
    public User User { get; set; }
    public Group Group { get; set; }
}
```

Burada ek olarak Membership'i kullanmak istiyoruz ve yine user'dan gruba yukarıdaki örnekteki gibi erişmek için. Entity'i aşağıdaki gibi güncelleyebiliriz.

```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Membership> Memberships { get; set; }
}
public class Group
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Membership> Memberships { get; set; }
}

public class Membership
{
    public int Id { get; set; }
    public User User { get; set; }
    public Group Group { get; set; }
}
```
Ancak burada dbcontext üzerinde many to many olacak kısmı set etmemiz gerekli.

```csharp
modelBuilder.Entity<User>()
.HasMany(u=>u.Groups)
.WithMany(g=>g.Users)
.UsingEntity<Membership>(
  j => j.HasOne(m => m.Group).WithMany(g => g.Memberships),
  j => j.HasOne(m => m.User).WithMany(g => g.Memberships));
```
### Inheritance Mapping
Entity'ler birbirini kalıtım aldıkça base tablo üzerine yeni kolonlar eklenilir. Burada veritabanından çekilen bilginin hangi Entity'e ait olduğu bilgisi ise otomatik olarak oluşturulan Discriminator kolonu ile bulunabilir. Yine de farklı tablolar'a kaydetmek istersek

```csharp
modelBuilder.Entity<ExternalUser>().ToTable("ExternalUsers");
modelBuilder.Entity<TimeRestrictedUser>().ToTable("TimeRestrictedUsers");
```

**Kaynak:** 
https://youtu.be/BIImyq8qaD4?list=WL 
https://docs.microsoft.com/tr-tr/ef/core/what-is-new/ef-core-5.0/whatsnew
