## Using ZLinq

You can knwow more about ZLinq at https://github.com/Cysharp/ZLinq, basically is a high performance wrapper for the normal C# LINQ. To use, any time you want to use normal linq, use `.AsValueEnumerable()` method before, like this:

```C#
using ZLinq;

var seq = source
    .AsValueEnumerable() // only add this line
    .Where(x => x % 2 == 0)
    .Select(x => x * 3);

foreach (var item in seq)
{
    // DO SOMETHING...
}
```

## Using Humanizer Pt

Humanizer meets all your .NET needs for manipulating and displaying strings, enums, dates, times, timespans, numbers and quantities. Please refer to https://github.com/Humanizr/Humanizer
