### Custom Serializer
Deserilize ettiğiniz veri tipi eğer Interface ya da Abstract class ise bu noktada sorunu çözmek için custom çözümler gerekebilir. Serialize etme noktasında ilgili tip bilgisini de yazabilirsiniz. Ancak burada yine de mevcut class'ın property'si List<IInterface> gibi bir tip ise burada bu tipi ilk oluşturma noktasında yine sorun yaşarsınız. Tam da bu noktada araya girerek ilgili tipin ne olması gerektiğini söylememiz gerekmetedir.

```csharp
 public abstract class JsonCreationConverter<T> : JsonConverter
    {
        protected abstract T Create(Type objectType, JObject jObject);

        public override bool CanConvert(Type objectType)
        {
            return typeof(T) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType,
            object existingValue, JsonSerializer serializer)
        {
            try
            {
                var jObject = JObject.Load(reader);
                var target = Create(objectType, jObject);
                return target;
            }
            catch (JsonReaderException ex)
            {
                return null;
            }
        }

        public override void WriteJson(JsonWriter writer, object value,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
    
    public class CustomModelConverter : JsonCreationConverter<RelatedClassType>
    {
        public CustomModelConverter()
        {

        }
        protected override CustomModelConverter Create(Type objectType, JObject jObject)
        {
            var objectValue = jObject["ProperyName"];
            jObject["PropertyName"] = "";
            var model = JsonConvert.DeserializeObject<TypeName>(jObject.ToString());
            model.ProperyName = new List<PropertyTypeName>();
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };
            var result = new List<PropertyTypeName>();
            foreach (var item in objectValue["$values"].Children())
            {
                result.Add((PropertyTypeName)JsonConvert.DeserializeObject(item.ToString(), settings));
            }
            model.ProperyName = result;
            return model;
        }
    }
```
Kullanım olarak aşağıdaki kod bloğu örnek gösterilebilir

```csharp
Newtonsoft.Json.JsonConvert.DeserializeObject<RelatedClassType>(json, new CustomModelConverter());
```
