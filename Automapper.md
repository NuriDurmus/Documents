# Ignore specific properties
 
```c#
ForPath(i => i.PropertyName, i => i.Ignore());
```

Ignore all unmatched properties

```c#
configuration.CreateMap<EntityName, ModelName>(MemberList.None);
```

You can also set property values from combination of destination and source properties.

```c#
configuration.CreateMap<EntityName2, ModelName2>(MemberList.None).BeforeMap((source,dest)=>dest.Url=dest.BaseUrl+ source.Id).AfterMap((source,dest)=>dest.Name+=" (CusomValue)").ReverseMap();
```

Using Extension Method

```c#
private static IMappingExpression<T1, T2> UnmapSpecificProperties<T1, T2>(this IMappingExpression<T1, T2> mappingExpression) where T2 : LookupBaseModel
        {
            return mappingExpression.ForPath(i => i.PropertyName, i => i.Ignore());
        }
```

All of that code and using in global.asax.cs

```c#

protected void Application_Start()
        {
          CustomDtoMapper.Configure();
        }

 public static class CustomDtoMapper
    {
        public static void Configure()
        {
            Mapper.Initialize(conf => CreateMappings(conf));
            Mapper.Configuration.AssertConfigurationIsValid();
        }
        private static void CreateMappings(IMapperConfigurationExpression configuration)
        {
           //
           configuration.CreateMap<EntityName, ModelName>(MemberList.None);
           configuration.CreateMap<EntityName2, ModelName2>(MemberList.None).BeforeMap((source,dest)=>dest.Url=dest.BaseUrl+ source.Id).AfterMap((source,dest)=>dest.Name+=" (CusomValue)").ReverseMap();
        }

        private static IMappingExpression<T1, T2> UnmapSpecificProperties<T1, T2>(this IMappingExpression<T1, T2> mappingExpression) where T2 : LookupBaseModel
        {
            return mappingExpression.ForPath(i => i.PropertyName, i => i.Ignore());
        }
    }
```


