# C# Dynamic Syntax

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
