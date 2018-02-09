# Casts, JSON and data types

Dynamit dynamic tables can contain values of the following .NET data types:

```
System.Boolean
System.Byte
System.DateTime
System.Decimal
System.Double
System.Int16
System.Int32
System.Int64
System.SByte
System.Single
System.String
System.UInt16
System.UInt32
System.UInt64
```

Since `DDictionary` implements `IDictionary<string, object>`, most proper JSON serializers can deserialize straight to `DDictionary`. The Jil serializer and Newtonsoft Json.net are two examples of such serializers. When deserializing to `DDictionary`, the serializer will try to insert dynamic values, that is, objects implementing `IDynamicMetaObjectProvider` into the `DDictionary`. Dynamit can handle this, and will automatically try to make the following casts, in order, when deserializing values of unknown dynamic type:

```
If item can be converted to System.DateTime, convert to System.DateTime
Else, if item can be converted to System.String, convert to System.String
Else, if item can be converted to System.Boolean, convert to System.Boolean
Else, if item can be converted to System.Int32, convert to System.Int32
Else, if item can be converted to System.Int64, convert to System.Int64
Else, try to convert to System.Decimal
If else, fail by throwing a Dynamit.InvalidValueTypeException
```

Most serializers also know how to serialize `DDictionary` entities, since they already know how to serialize `IDictionary<string, object>`.
