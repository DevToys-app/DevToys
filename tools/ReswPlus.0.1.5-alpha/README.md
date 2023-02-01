C# Source generator built from https://github.com/DotNetPlus/ReswPlus/tree/sourceGenerator
This binary contains a tiny change to the usings generated [here](https://github.com/DotNetPlus/ReswPlus/blob/c94e47bbc1deff1569345c680e1017984120da3c/src/ReswPlus.Shared/CodeGenerators/CsharpCodeGenerator.cs#L49-L52) that set the proper namespace for non-UWP apps.

In the end, it generates the following:

```
// File generated automatically by ReswPlus. https://github.com/DotNetPlus/ReswPlus
// The NuGet package ReswPlusLib is necessary to support Pluralization.
using System;
using Windows.ApplicationModel.Resources;
#if WINDOWS_UWP
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Data;
#else
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Data;
#endif
```