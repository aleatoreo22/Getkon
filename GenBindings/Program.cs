using System.Reflection;
using System.Text;

var assemblyPath = "";
#if DEBUG
assemblyPath = "/home/anemonas/Projects/test/Getkon/bin/Debug/net9.0/";
#else
assemblyPath = args.FirstOrDefault() ?? throw new Exception("Informe o caminho da DLL.");
#endif

var bindings = new Dictionary<string, StringBuilder>();

var files = new DirectoryInfo(assemblyPath).GetFiles("*.dll");

foreach (var item in files)
{
    var assembly = Assembly.LoadFrom(item.FullName);
    foreach (var type in assembly.GetTypes())
    {
        foreach (var method in type.GetMethods())
        {
            var attr = method.GetCustomAttributes();

            if (!attr.ToList().Exists(x => x.ToString() == "ExposeAttribute"))
                continue;

            bindings.TryGetValue(type.Name, out StringBuilder? value);
            if (value is null)
            {
                value = new StringBuilder();
                bindings.Add(type.Name, value);
                bindings[type.Name].AppendLine($"// {type.Name}.ts - gerado automaticamente");
            }

            var methodName = method.Name;
            var className = type.Name;
            var namespaceName = type.Namespace;
            var fullNamespace = $"{namespaceName}.{className}";

            var parameters = method.GetParameters();
            var paramList = string.Join(
                ", ",
                parameters.Select(p => $"{p.Name}: {MapType(p.ParameterType)}")
            );
            var paramObj = string.Join(", ", parameters.Select(p => p.Name));
            var returnType = MapType(method.ReturnType);
            value.AppendLine(
                $"export async function {methodName}({paramList}): Promise<{returnType}> {{"
            );
            value.AppendLine(
                $"  return await callNative(\"{fullNamespace}\", \"{methodName}\", {{ {paramObj} }});"
            );
            value.AppendLine("}\n");
        }
    }
}

foreach (var item in bindings)
{
    File.WriteAllText(item.Key + ".ts", item.Value.ToString());
}

Console.WriteLine("✅ bindings gerado com sucesso!");

static string MapType(Type type)
{
    return type.Name switch
    {
        "Int32" => "number",
        "String" => "string",
        "Boolean" => "boolean",
        _ => "any",
    };
}
