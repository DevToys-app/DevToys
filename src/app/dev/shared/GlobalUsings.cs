global using global::CommunityToolkit.Diagnostics;
global using global::System;
global using global::System.Collections.Generic;
global using global::System.ComponentModel;
global using global::System.ComponentModel.Composition;
global using global::System.Diagnostics;
global using global::System.Linq;
global using global::System.Threading;
global using global::System.Threading.Tasks;
global using ExportAttribute = global::System.ComponentModel.Composition.ExportAttribute;

#if __MAC__
[assembly: System.Runtime.Versioning.SupportedOSPlatform("ios16.0")] // MacCatalyst is a superset of iOS, therefore it's also supported.
#endif
