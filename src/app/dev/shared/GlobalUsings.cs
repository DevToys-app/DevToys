global using global::CommunityToolkit.Diagnostics;
global using global::System;
global using global::System.Collections.Generic;
global using global::System.ComponentModel;
global using global::System.ComponentModel.Composition;
global using global::System.Diagnostics;
global using global::System.Diagnostics.CodeAnalysis;
global using global::System.Linq;
global using global::System.Threading;
global using global::System.Threading.Tasks;
global using ExportAttribute = global::System.ComponentModel.Composition.ExportAttribute;

#if __MACCATALYST__
[assembly: System.Runtime.Versioning.SupportedOSPlatform("ios14.0")] // MacCatalyst is a superset of iOS, therefore it's also supported.
#elif __WINDOWS__
[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows10.0.19041.0")]
#endif

