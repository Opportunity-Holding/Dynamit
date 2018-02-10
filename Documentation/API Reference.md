# API Reference

## A simple example

Think of Dynamit dynamic tables as being to regular static database classes what .NET `Dictionary` objects are to regular .NET classes. In fact, they are built as Starcounter database classes that implement the `IDictionary<string, object>` interface.

```csharp
using Dynamit;

public class App
{
    public static void Main()
    {
        DynamitConfig.Init(); // A call to Init() initializes Dynamit
        Db.Transact(() =>
        {
            // This is our dynamic table row
            new Product
            {
                ["Label"] = "Double Espresso",
                ["Product_ID"] = 42,
                ["Product_date"] = new DateTime(2017, 01, 05),
                ["Price"] = 3.25,
                ["Group"] = "A1"
            };
    });
}
```

We can query dynamic tables using the `Finder<T>` class.

```csharp
var product = Finder<Product>.All.Where(o => o["Product_ID"] == 42);

product["Price"]
// 3.25

product["Label"].Length
// 15

product["Product_date"].AddDays(2).ToString("O")
// "2017-01-07T00:00:00.0000000"

product is IDictionary<string, dynamic>
// true

Finder<Product>.All.Where(row => row["Product_ID"] > 10).First()["Label"]
// "Double Espresso"
```

## `DDictionary` and `DKeyValuePair`

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
        : base(dict, key, value)
    {
    }
}
```

**Q**: Why do we need to declare our own sub-classes of `DDictionary` and `DKeyValuePair`? Why not just have these classes be concrete classes, and work with them directly?

**A**: The reason for this is that we want to have as many separate tables as possible, to speed up queries if we have many dynamic tables. We also want to expose the constructor for the key-value pairs, so that we can add additional logic that checks keys and values before they are inserted.

## `DynamitConfig`

To enable fast queries against dynamic tables, we need certain indexes to be registered in Starcounter. The method `DynamitConfig.Init()` will set up the necessary indexes during app startup, which is recommended if you do not set up these indexes yourself. By calling `DynamitConfig.Init()` on app startup, the following is done:

1. All dynamic table declarations are checked for declaration errors, and exceptions are thrown if there are errors.
2. Indexes are set up for all key-value pair tables on columns: a. `Dictionary` b. `Dictionary`, `Key` c. `Key`, `ValueHash` By setting the parameters of the call to `DynamitConfig.Init()`, you can skip step 2\. In the call you can also set whether escaped strings such as `"\"Example\""` should be understood as `"Example"` when setting values in dynamic tables. This is necessary to be able to unambiguously identify values that should be parsed as strings rather than datetimes when using other solutions that write strings to Dynamit tables, for example `RESTar`.

## `Dynamit.Finder`

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

### Examples

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

## `Dynamit.DValue`

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
