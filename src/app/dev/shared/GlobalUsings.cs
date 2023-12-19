global using CommunityToolkit.Diagnostics;
global using System;
global using System.Collections.Generic;
global using System.ComponentModel;
global using System.ComponentModel.Composition;
global using System.Diagnostics;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;
global using ExportAttribute = System.ComponentModel.Composition.ExportAttribute;

#if __MACCATALYST__
[assembly: System.Runtime.Versioning.SupportedOSPlatform("ios14.0")] // MacCatalyst is a superset of iOS, therefore it's also supported.
#elif __MACOS__
[assembly: System.Runtime.Versioning.SupportedOSPlatform("macos12.0")]
#elif __LINUX__
[assembly: System.Runtime.Versioning.SupportedOSPlatform("linux")]
#elif __WINDOWS__
[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows10.0.19041.0")]
#endif

