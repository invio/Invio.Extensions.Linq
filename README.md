# Invio.Extensions.Linq

[![Build Status](https://ci.appveyor.com/api/projects/status/vjf55wpbc6tc0hkf/branch/master?svg=true)](https://ci.appveyor.com/project/invio/invio-extensions-linq/branch/master)
[![Travis CI](https://img.shields.io/travis/invio/Invio.Extensions.Linq.svg?maxAge=3600&label=travis)](https://travis-ci.org/invio/Invio.Extensions.Linq)
[![NuGet](https://img.shields.io/nuget/v/Invio.Extensions.Linq.svg)](https://www.nuget.org/packages/Invio.Extensions.Linq/)
[![Coverage](https://coveralls.io/repos/github/invio/Invio.Extensions.Linq/badge.svg?branch=master)](https://coveralls.io/github/invio/Invio.Extensions.Linq?branch=master)

A collection of extension methods and helper classes for working with `System.Linq`.

# Installation
The latest version of this package is available on NuGet. To install, run the following command:

```
PM> Install-Package Invio.Extensions.Linq
```

## [Enumerable Extensions](src/Invio.Extensions.Linq/EnumerableExtensions.cs)

### Cycling Enumerables

There is an extension method, on both `IEnumerable` and `IEnumerable<T>`, that allow a caller to cycle over the original enumerable ad infinitum. Named **`Cycle()`**, this method makes it so that, upon reaching the end of the parent `IEnumerable<T>`, it will always start enumerating again from the beginning.

This can be especially useful in [generative or "model-based" testing scenarios](https://en.wikipedia.org/wiki/Model-based_testing) when you want to rotate through a collection of valid data as a parameter for a different collection of test cases whose length is unknown.

```csharp
public static IEnumerable<Profile> ValidProfiles { get; } =
    ImmutableList
        .Create<Profile>(Profile.User, Profile.Admin)
        .Cycle();

public IEnumerable<TestCase> ToTestCases(IEnumerable<Guid> ids) {
   return ids.Zip(ValidProfiles, (id, profile) => new TestCase(id, profile));
}
```

In the above example, it does not matter how large the `IEnumerable<Guid>` is, it will always get a valid `Profile` from the list of `ValidProfiles`.

### Enumerable Subsequences

There is an extension method on `IEnumerable<T>` that allows a caller to get all
potential subsequences of an initial sequence of elements. Named **`Subsequences()`**,
this method will include all combinations of elements in no particular order, including
the identity sequence and the empty sequence.

If there are duplicate elements, that will result in duplicate subsequences.

This can be useful when attempting to enumerate all potential subsets of a group of values,
like so:

```csharp
public static void EnumerateSubsets() {
    var values = new List<string> { "foo", "bar", "biz" };

    foreach (var subsequence in values.Subsequences()) {
        Console.WriteLine("[" + String.Join(", ") + "]");
    }
}

// Output
[]
[foo]
[bar]
[biz]
[foo, bar]
[foo, biz]
[bar, biz]
[foo, bar, biz]
```

### Batching

The `IEnumerable<T>.Batch(size)` extension method breaks the enumerable's items into batches of `IEnumerable<T>` where each batch contains `size` items, if possible.

```csharp
public static void EnumerateBatches() {
    var values = new List<string> { "foo", "bar", "biz" };

    foreach (var batch in values.Batch(size: 2)) {
        Console.WriteLine("[" + String.Join(", ") + "]");
    }
}
// Output
[foo, bar]
[biz]
```
