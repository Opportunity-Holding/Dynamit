_By Erik von Krusenstierna (erik.von.krusenstierna@mopedo.com)_

# Dynamit specification

[• **Introduction**](#introduction)

[• **API Reference**](Documentation/API%20Reference.md)

[• **Casts, JSON and data types**](Documentation/Casts,%20JSON%20and%20data%20types.md)

[• **C# Dynamic Syntax**](Documentation/C%23%20Dynamic%20Syntax.md)

## Introduction

Dynamit is a free to use Starcounter class library and .NET package developed by Mopedo, for creating and querying dynamic database tables in Starcounter applications. This article will cover the basics of Dynamit and how to set it up in a Visual Studio project. 

Dynamit is distributed as a [package](https://www.nuget.org/packages/Dynamit) on the NuGet Gallery, and an easy way to install it in an active Visual Studio project is by entering the following into the NuGet Package Manager console:

```
Install-Package Dynamit
```

## Static vs. dynamic

To illustrate the difference between dynamic and non-dynamic – _static_ – tables, consider the object-oriented programming concept object, and its standard implementations in Java and JavaScript respectively. In Java – which is class-based – objects are static with regards to their members, and this structure is defined in compile-time in a class definition. We can change the contents of objects – that is, assign values to instance fields – during runtime, but we cannot change the structure of objects without first recompiling the application. In JavaScript – which is prototype-based – however, both the content and the structure of objects can change during runtime. As far as .NET developers working with Starcounter are concerned, rows in static Starcounter tables behave like objects in Java. We can create new rows in runtime, but the table schema is defined during compile-time. Rows in dynamic tables, however, behave like JavaScript objects. There is no common schema of property names and types that applies to all rows in a dynamic table, and their members can have any name and data type, regardless of members and types of other entities in the same table.

## How does it work?

Dynamit achieves dynamic Starcounter tables by means of normalization. All data types used for values are ordered in value object classes, and are assigned their own Starcounter database tables. A row in these tables can, for example, be the string `"Foo"` or the integer `42`. Since Starcounter makes no distinction between database objects and table rows, the term _value object_ is used to refer to these table rows. Value objects are then assigned to key-value pairs with Starcounter object references. These key-value pairs contain a string key, a reference to a value object and a stored value object hash code. The key-value pair is dynamically typed, since its value object can be of any value object class. The dynamic nature of how key-value pairs reference the values of their value objects means that we cannot index or run queries against the actual values, which is a limitation we need to be aware of. We can, however, index and query the value hash codes, which is how querying dynamic tables can still be fast (for most queries). The dynamic key-value pairs rows then contain many-to-one references to dictionary objects (also with Starcounter object references) that are essentially lists of key-value pairs. Consider the following JSON object:

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

## Distribution

Dynamit is free to use, and is distributed over the NuGet package manager. If you have any questions, comments or bug reports, or if you want to contribute to the Dynamit project, please contact develop@mopedo.com.


## See also

Dynamit is used by [RESTar](https://github.com/Mopedo/Home/tree/master/Documentation/RESTar), a package developed by Mopedo to help create powerful RESTful web services on the Starcounter system. Using RESTar, REST clients can insert arbitrary data into `DDictionary` objects.
