#light

open System
open System.IO

let format = "#light
namespace astroclock.fs.AssemblyInfo
open System.Reflection;
open System.Runtime.CompilerServices;
open System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following set of attributes.
[<assembly: AssemblyTitle(\"astroclock-fs\")>]
[<assembly: AssemblyDescription(\"\")>]
[<assembly: AssemblyConfiguration(\"\")>]
[<assembly: AssemblyCompany(\"Ravna & Tines.\")>]
[<assembly: AssemblyProduct(\"Planetarium clock\")>]
[<assembly: AssemblyCopyright(\"Copyright © Steve Gilham, 2009\")>]
[<assembly: AssemblyTrademark(\"\")>]
[<assembly: AssemblyCulture(\"\")>]

[<assembly: ComVisible(false)>]

[<assembly: AssemblyVersion(\"1.1.{0}\")>]
[<assembly: AssemblyFileVersion(\"1.1.{0}\")>]
()"

let now = DateTimeOffset.UtcNow
let epoch = new DateTimeOffset(2000, 1, 1, 0, 0, 0, new TimeSpan(int64 0))
let diff = now.Subtract(epoch)
let fraction = diff.Subtract(TimeSpan.FromDays(float diff.Days))
let revision= ((int fraction.TotalSeconds) / 3)
let version = String.Format("{0}.{1}", diff.Days, revision)
printfn "%s" version

File.WriteAllText ("AssemblyInfo.fs", String.Format (format, version))

