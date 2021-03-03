template = '''#light

open System.Reflection;
open System.Runtime.CompilerServices;
open System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following set of attributes.
[<assembly: AssemblyTitle("astroclock-fs")>]
[<assembly: AssemblyDescription("")>]
[<assembly: AssemblyConfiguration("")>]
[<assembly: AssemblyCompany("Ravna & Tines.")>]
[<assembly: AssemblyProduct("Planetarium clock")>]
[<assembly: AssemblyCopyright("Copyright © Steve Gilham, 2009")>]
[<assembly: AssemblyTrademark("")>]
[<assembly: AssemblyCulture("")>]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[<assembly: ComVisible(false)>]

[<assembly: AssemblyVersion("1.0.%d.%d")>]
[<assembly: AssemblyFileVersion("1.0.0.0")>]
()'''

import clr
from System import *

