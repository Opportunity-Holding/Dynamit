_By Erik von Krusenstierna (erik.von.krusenstierna@mopedo.com)_

# What is Dynamit?

Dynamit is a free to use open-source Starcounter class library and .NET package developed by Mopedo, for creating and querying dynamic database tables in Starcounter applications. This article will cover the basics of Dynamit and how to set it up in a Visual Studio project.

Dynamit is distributed as a [package](https://www.nuget.org/packages/Dynamit) on the NuGet Gallery, and an easy way to install it in an active Visual Studio project is by entering the following into the NuGet Package Manager console:

```
Install-Package Dynamit
```

## Contents

- [Static vs. dynamic](#static-vs-dynamic)
- [How does it work?](#how-does-it-work)
- [Using Dynamit](#using-dynamit)
- [Casts, JSON and data types](#casts-json-and-data-types)
- [C# Dynamic Syntax](#c-dynamic-syntax)
- [See also](#see-also)

## Static vs. dynamic

To illustrate the difference between dynamic and non-dynamic – _static_ – tables, consider the object-oriented programming concept object, and its standard implementations in Java and JavaScript respectively. In Java – which is class-based – objects are static with regards to their members, and this structure is defined in compile-time in a class definition. We can change the contents of objects – that is, assign values to instance fields – during runtime, but we cannot change the structure of objects without first recompiling the application. In JavaScript – which is prototype-based – however, both the content and the structure of objects can change during runtime. As far as .NET developers working with Starcounter are concerned, rows in static Starcounter tables behave like objects in Java. We can create new rows in runtime, but the table schema is defined during compile-time. Rows in dynamic tables, however, behave like JavaScript objects. There is no common schema of property names and types that applies to all rows in a dynamic table, and their members can have any name and data type, regardless of members and types of other entities in the same table.

## How does it work?

Dynamit achieves dynamic Starcounter tables by means of normalization. All data types used for values are ordered in value object classes, and are assigned their own Starcounter database tables. A row in these tables can, for example, be the string `"Foo"` or the integer `42`. Since Starcounter makes no distinction between database objects and table rows, the term _value object_ is used to refer to these table rows. Value objects are then assigned to key-value pairs with Starcounter object references. These key-value pairs contain a string key, a reference to a value object and a stored value object hash. The key-value pair is dynamically typed, since its value object can be of any value object type. The dynamic nature of how key-value pairs reference the values of their value objects means that we cannot index or run queries against the actual values, which is a limitation we need to be aware of. We can, however, index and query the value hashes, which is how querying dynamic tables can still be fast (for most queries). The dynamic key-value pairs rows then contain many-to-one references to dictionary objects (also with Starcounter object references) that are essentially lists of key-value pairs. Consider the following JSON object:

```json
{
    "Label": "Double Espresso",
    "Product_ID": 42,
    "Product_date": "2017-01-05T00:00:00",
    "Price": 3.25,
    "Group": "A1"
}
```

If this object was to be stored in a dynamic table, it would result in five key-value pairs – each with a key corresponding to the property name, a value hash corresponding to the hash code of the value, and a reference to a value object of dynamic type, containing the actual value. There would also be a dictionary object, and all five key-value pairs would contain a reference to that dictionary.

## Using Dynamit

### A simple example

Think of Dynamit dynamic tables as being to regular static database classes what .NET `Dictionary` objects are to regular .NET classes. In fact, they are built as Starcounter database classes that implement the `IDictionary<string, object>` interface.

```csharp
using Dynamit;

public class App
{
    public static void Main()
    {
        DynamitConfig.Init(); // A call to Init() initializes Dynamit
        Db.Transact(() => new Product // This is our dynamic table row
        {
            ["Label"] = "Double Espresso",
            ["Product_ID"] = 42,
            ["Product_date"] = new DateTime(2017, 01, 05),
            ["Price"] = 3.25,
            ["Group"] = "A1"
        });
    }
}
```

We can query dynamic tables using the `Finder<T>` class.

```csharp
var product = Finder<Product>.All.Where(o => o["Product_ID"] == 42);
product["Price"] // 3.25
product["Label"].Length // 15
product["Product_date"].AddDays(2).ToString("O") // "2017-01-07T00:00:00.0000000"
product is IDictionary<string, dynamic> // true
Finder<Product>.All.Where(row => row["Product_ID"] > 10).First()["Label"] // "Double Espresso"
```

### `DDictionary` and `DKeyValuePair`

All dynamic tables inherit from the public abstract class `Dynamit.DDictionary`, which in turn implements `IDictionary<string, object>`. In the example above, `Product` is a subclass of `DDictionary`. Product also have a separate table that contains all its key-value pairs. This table is declared by creating a subclass of the public abstract class `Dynamit.DKeyValuePair`.

Below is the complete dynamic table declaration for `Product`. The structure is the same for all dynamic table declarations. A key-value pair class is declared, that inherits from `DKeyValuePair`, a dictionary class is declared that inherits from `DDictionary`, and the dictionary class also implements the generic `IDDictionary` interface, which Dynamit then uses to connect the key-value pair class with the dictionary class under the hood. The dictionary class also needs to implement the abstract method `NewKeyPair`, which returns a new key-value pair instance. We also declare a constructor in the key-value pair class that calls the base constructor.

```csharp
public class Product : DDictionary, IDDictionary<Product, ProductKVP>
{
    public ProductKVP NewKeyPair(Product dict, string key, object value = null)
    {
        return new ProductKVP(dict, key, value);
    }
}

public class ProductKVP : DKeyValuePair
{
    public ProductKVP(DDictionary dict, string key, object value = null)
        : base(dict, key, value) { }
}
```

**Q**: Why do we need to declare our own sub-classes of `DDictionary` and `DKeyValuePair`? Why not just have these classes be concrete classes, and work with them directly?

**A**: The reason for this is that we want to have as many separate tables as possible, to speed up queries if we have many dynamic tables. We also want to expose the constructor for the key-value pairs, so that we can add additional logic that checks keys and values before they are inserted.

### `DynamitConfig`

To enable fast queries against dynamic tables, we need certain indexes to be registered in Starcounter. The method `DynamitConfig.Init()` will set up the necessary indexes during app startup, which is recommended if you do not set up these indexes yourself. By calling `DynamitConfig.Init()` on app startup, the following is done:

1. All dynamic table declarations are checked for declaration errors, and exceptions are thrown if there are errors.
2. Indexes are set up for all key-value pair tables on columns: a. `Dictionary` b. `Dictionary`, `Key` c. `Key`, `ValueHash` By setting the parameters of the call to `DynamitConfig.Init()`, you can skip step 2\. In the call you can also set whether escaped strings such as `"\"Example\""` should be understood as `"Example"` when setting values in dynamic tables. This is necessary to be able to unambiguously identify values that should be parsed as strings rather than datetimes when using other solutions that write strings to Dynamit tables, for example `RESTar`.

### `Dynamit.Finder`

Querying dynamic tables using SQL is a bit trickier than querying regular tables. The `Finder` class gives access to some useful static methods for finding `DDictionary` entities.

```csharp
static class Finder<T> where T : DDictionary
{
    static IEnumerable<T> All;
    static T First(string key, Operator op, dynamic value);
    static T First(params (string key, Operator op, dynamic value)[] equalityConditions);
    static IEnumerable<T> Where(string key, Operator op, object value);
    static IEnumerable<T> Where(params (string key, Operator op, object value)[] equalityConditions);
}
```

To make queries as fast as possible, it is always recommended to use the `Finder<T>.Where()` methods (as opposed to `Finder<T>.All` + LINQ) whenever the query contains equality conditions, since equality conditions can be evaluated much faster than other conditions due to the way values are hashed in key-value pairs. Consider the two examples below as good practice:

#### Examples

Find all products with a product id larger than `5`:

```csharp
Finder<Product>.All.Where(d => d["ProductId"] > 5);
```

Here we must use `Finder.All` + LINQ, since the `>` operator generally cannot be applied to value hashes.

Find all products with a product group equal to `"A1"` and price higher than `3`:

```csharp
Finder<Product>
    .Where("Group", Operator.EQUALS, "A1")
    .Where(d => d.SafeGet("Price") > 3);
```

Here we can use `Finder.Where()` for the equality condition, but need LINQ for the non-equality condition. The `DDictionary.SafeGet()` method can be used to get the value of a dynamic object property, or `null` if there is no such property.

### `Dynamit.DValue`

The `Dynamit` namespace also contains a class `DValue` that can be used to store values of arbitrary data types in static Starcounter database class definitions. In the following example, we need a dynamic member to hold a value of an arbitrary type.

```csharp
[Database]
public class Condition : IEntity
{
    public string PropertyName;
    public OperatorsEnum Operator;
    public ConditionValue Value;  // This is our DValue member
    public void OnDelete() => Value?.Delete();
}

public class ConditionValue : DValue
{
    public ConditionValue(object value) => Value = value;
}
```

Example usage:

```csharp
Db.Transact() =>
{
    new Condition
    {
        PropertyName = "Name",
        Operator = OperatorsEnum.Equals,
        Value = new ConditionValue("Tony Wonder")
    };
});
```

## Casts, JSON and data types

Dynamit dynamic tables can contain values of the following .NET data types:

```
System.Boolean              System.Int32                System.Single
System.Byte                 System.Int64                System.Double
System.SByte                System.UInt16               System.DateTime
System.Decimal              System.UInt32               System.String
System.Int16                System.UInt64
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

## C# Dynamic Syntax

Since package version `0.1.7`, `DDictionary` objects have support for C# dynamic member binding, which gives an alternative syntax for object property access. Example:

```csharp
dynamic product = null;
Db.TransactAsync(() => {
    product = new Product();
    product.Label = "Double Espresso",
    product.Product_ID = 42,
    product.Product_date = new DateTime(2017, 01, 05),
    product.Price = 3.25,
    product.Group = "A1"
});

product.Label
// "Double Espresso"

product.Product_date.AddDays(1).ToString()
// "2017-01-06 12:00:00 AM"
```

## See also

Dynamit is used by [RESTar](https://github.com/Mopedo/Home/tree/master/Documentation/RESTar), a package developed by Mopedo to help create powerful RESTful web services on the Starcounter system. Using RESTar, REST clients can insert arbitrary data into `DDictionary` objects.
