#:package Mono.Cecil@0.11.6

using Mono.Cecil;

if (args is not [var managedPath, var modPath])
{
    Console.Error.WriteLine("Usage: VerifyHarmonyPatches <game Managed directory> <compiled mod DLL>");
    return 1;
}

using var resolver = new DefaultAssemblyResolver();
foreach (string directory in resolver.GetSearchDirectories())
    resolver.RemoveSearchDirectory(directory);
resolver.AddSearchDirectory(Path.GetFullPath(managedPath));

using var mod = AssemblyDefinition.ReadAssembly(modPath, new ReaderParameters { AssemblyResolver = resolver });
var patches = mod.MainModule.GetTypes().SelectMany(type => type.Methods).Where(method =>
    method.CustomAttributes.Any(attribute => attribute.AttributeType.FullName is
        "HarmonyLib.HarmonyPrefix" or "HarmonyLib.HarmonyPostfix" or "HarmonyLib.HarmonyReversePatch")).ToArray();

if (patches.Length == 0)
{
    Console.Error.WriteLine("No Harmony patch methods were found in the compiled mod.");
    return 1;
}

bool success = true;
foreach (var patch in patches)
{
    string? error = CheckPatch(patch);
    if (error is not null)
    {
        Console.Error.WriteLine($"{patch.DeclaringType.FullName}.{patch.Name}: {error}");
        success = false;
    }
}

if (success)
    Console.WriteLine($"Verified all {patches.Length} Harmony patch signatures against the game assemblies.");
return success ? 0 : 1;

static string? CheckPatch(MethodDefinition patch)
{
    var attributes = patch.CustomAttributes.Where(attribute => attribute.AttributeType.FullName == "HarmonyLib.HarmonyPatch").ToArray();
    if (!patch.IsStatic || attributes is not [var attribute] || attribute.ConstructorArguments.Count is not (3 or 4))
        return "Expected a static patch method with a single target attribute specifying the declaring type, method name, and parameter types.";

    var arguments = attribute.ConstructorArguments;
    if (arguments[0].Value is not TypeReference targetType || arguments[1].Value is not string targetName ||
        arguments[2].Value is not CustomAttributeArgument[] parameterTypes)
        return "The Harmony target must specify the declaring type, method name, and parameter types.";

    var variations = arguments.Count == 3 ? null : (CustomAttributeArgument[])arguments[3].Value;
    if (variations is not null && variations.Length != parameterTypes.Length)
        return "The Harmony argument variations must match the number of parameter types.";

    string[] signature = parameterTypes.Select((type, index) =>
        ((TypeReference)type.Value).FullName + (variations is null ? "" : ArgumentSuffix(variations[index]))).ToArray();

    var targets = targetType.Resolve().Methods.Where(method => method.Name == targetName && !method.HasGenericParameters &&
        method.Parameters.Select(parameter => parameter.ParameterType.FullName).SequenceEqual(signature)).ToArray();
    if (targets is not [var target])
        return $"Expected one game method matching {targetType.FullName}.{targetName}({string.Join(", ", signature)}), found {targets.Length}.";

    if (patch.CustomAttributes.Any(marker => marker.AttributeType.FullName == "HarmonyLib.HarmonyReversePatch"))
    {
        var expectedParameters = target.HasThis ? signature.Prepend(target.DeclaringType.FullName) : signature;
        return patch.ReturnType.FullName == target.ReturnType.FullName &&
            patch.Parameters.Select(parameter => parameter.ParameterType.FullName).SequenceEqual(expectedParameters)
            ? null : $"The reverse patch signature does not match {target.FullName}.";
    }

    foreach (var parameter in patch.Parameters)
    {
        TypeReference? expectedType = parameter.Name switch
        {
            "__instance" => target.HasThis ? target.DeclaringType : null,
            "__result" => target.ReturnType.MetadataType != MetadataType.Void ? target.ReturnType : null,
            _ => target.Parameters.FirstOrDefault(original => original.Name == parameter.Name)?.ParameterType
        };

        if (expectedType is null || ValueTypeName(expectedType) != ValueTypeName(parameter.ParameterType))
            return $"The injected parameter {parameter.ParameterType.FullName} {parameter.Name} does not match {target.FullName}.";
    }

    return null;
}

static string ArgumentSuffix(CustomAttributeArgument variation)
{
    string name = variation.Type.Resolve().Fields.Single(field => field.HasConstant && Equals(field.Constant, variation.Value)).Name;
    return name switch
    {
        "Normal" => "",
        "Ref" or "Out" => "&",
        "Pointer" => "*",
        _ => throw new InvalidOperationException($"Unknown Harmony argument variation {name}.")
    };
}

static string ValueTypeName(TypeReference type) => type is ByReferenceType reference ? reference.ElementType.FullName : type.FullName;
