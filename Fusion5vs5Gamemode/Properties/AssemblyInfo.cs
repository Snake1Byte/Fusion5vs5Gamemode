using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using MelonLoader;
using Fusion5vs5Gamemode;
using LabFusion.SDK.Modules;

[assembly: MelonInfo(typeof(MainClass), "Fusion 5vs5 Gamemode", "1.0.0", "Snake1Byte")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle(MainClass.Name)]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct(MainClass.Name)]
[assembly: AssemblyCopyright("Copyright ©  2024 " + MainClass.Author)]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("d6c0fb27-e32d-457b-beaf-63549dbaa9b8")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion(MainClass.Version)]
[assembly: AssemblyFileVersion(MainClass.Version)]

[assembly: ModuleInfo(typeof(Fusion5vs5CustomModule), MainClass.Name, MainClass.Version, MainClass.Author, null, true, ConsoleColor.Magenta)]