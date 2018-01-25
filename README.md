# Invio.Extensions.Linq

[![Build Status](https://ci.appveyor.com/api/projects/status/vjf55wpbc6tc0hkf/branch/master?svg=true)](https://ci.appveyor.com/project/invio/invio-extensions-linq/branch/master)
[![Travis CI](https://img.shields.io/travis/invio/Invio.Extensions.Linq.svg?maxAge=3600&label=travis)](https://travis-ci.org/invio/Invio.Extensions.Linq)
[![NuGet](https://img.shields.io/nuget/v/Invio.Extensions.Linq.svg)](https://www.nuget.org/packages/Invio.Extensions.Linq/)
[![Coverage](https://coveralls.io/repos/github/invio/Invio.Extensions.Linq/badge.svg?branch=master)](https://coveralls.io/github/invio/Invio.Extensions.Linq?branch=master)

A collection of extension methods and helper classes for working with Linq.

# Installation
The latest version of this package is available on NuGet. To install, run the following command:

```
PM> Install-Package Invio.Extensions.Linq
```

## [Enumerable Extensions](src/Invio.Extensions.Linq/EnumerableExtensions.cs)

### Infinite Enumerables & Enumerators

There are two extension methods on `IEnumerable<T>` that allow a caller to easily enumerate over the original enumerable ad infinitum. These are as follows:

1. **`GetInfiniteEnumerator()`** returns an `IEnumerator<T>` that, upon reaching the end of the parent `IEnumerable<T>`, will start again at the beginning.
2. **`AsInfiniteEnumerable()`** makes the `IEnumerable<T>` parent no longer

These can be especially useful in [generative or "model-based" testing scenarios](https://en.wikipedia.org/wiki/Model-based_testing) when you want to rotate through a collection of valid data as a parameter for as long as you have test cases.

```csharp
public static IEnumerable<Profile> ValidProfiles { get; } =
    ImmutableList
        .Create<Profile>(Profile.User, Profile.Admin)
        .AsInfiniteEnumerable();

public IEnumerable<TestCase> ToTestCases(IEnumerable<Guid> ids) {
   return ids.Zip(ValidProfiles, (id, profile) => new TestCase(id, profile));
}
```

In the above example, it does not matter how large `IEnumerable<Guid>` is, it will always get a valid `Profile` from the list of `ValidProfiles`.
