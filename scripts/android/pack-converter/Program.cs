using System.Reflection;
using System.Runtime.Loader;
var managed = Path.GetFullPath(args[0]);
AssemblyLoadContext.Default.Resolving += (_, name) => {
 var path = Path.Combine(managed, name.Name + ".dll");
 return File.Exists(path) ? AssemblyLoadContext.Default.LoadFromAssemblyPath(path) : null;
};
var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(managed, "Assembly-CSharp.dll"));
var mapper = assembly.GetType("LitJson.JsonMapper", true)!;
var parse = mapper.GetMethods().Single(m => m.Name == "ToObject" && !m.IsGenericMethod && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(string));
var json = parse.Invoke(null, new object[]{File.ReadAllText(args[1])});
var encoder = assembly.GetType("JsonBin.NBinary.JsonToBinary", true)!.GetMethod("TryToJsonBinary")!;
object[] values = {json!, null!};
if (!(bool)encoder.Invoke(null, values)!) throw new Exception("JsonBin encoding failed");
File.WriteAllBytes(args[2], (byte[])values[1]);
Console.WriteLine($"Encoded {((byte[])values[1]).Length} bytes");
var decoder = assembly.GetType("JsonBin.NBinary.BinaryToJson",true)!.GetMethods().Single(m=>m.Name=="ToJson" && m.GetParameters().Length==1 && m.GetParameters()[0].ParameterType==typeof(byte[]));
var decoded=decoder.Invoke(null,new object[]{values[1]})!;
File.WriteAllText(args[2]+".roundtrip.json", (string)decoded.GetType().GetMethod("internalToJson",BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(decoded,new object[]{1024,false})!);

