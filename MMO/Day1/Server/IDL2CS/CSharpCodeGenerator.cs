using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class CSharpCodeGenerator
{
    private string GetSolutionName()
    {
        string projectPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        string projectDirectory = Path.GetDirectoryName(projectPath);

        // 상위 디렉토리들을 순회하면서 .sln 파일 찾기
        while (projectDirectory != null)
        {
            string[] solutionFiles = Directory.GetFiles(projectDirectory, "*.sln");
            if (solutionFiles.Length > 0)
            {
                // 첫 번째 발견된 .sln 파일의 이름을 반환
                return Path.GetFileNameWithoutExtension(solutionFiles[0]);
            }
            projectDirectory = Directory.GetParent(projectDirectory)?.FullName;
        }

        // .sln 파일을 찾지 못한 경우 기본값 반환
        return "DefaultSolutionName";
    }

    public string GenerateConst(IdlParser.ConstInfo constInfo)
    {
        return $"public const int {constInfo.Name} = {constInfo.Value};";
    }


    public string GenerateStruct(IdlParser.StructInfo structInfo)
    {
        var sb = new StringBuilder();


        Console.WriteLine($"public struct {structInfo.Name}");
        sb.AppendLine($"public struct {structInfo.Name} : IPacketSerializable");
        sb.AppendLine("{");

        foreach (var field in structInfo.Fields)
        {
            string fieldType = field.Type;
            if (fieldType.Contains('['))
            {
                var match = Regex.Match(fieldType, @"(\w+)\[(\w+)\]");
                if (match.Success)
                {
                    string baseType = match.Groups[1].Value;
                    string arraySize = match.Groups[2].Value;
                    sb.AppendLine($"    public {baseType}[] {field.Name};");
                    Console.WriteLine($"    public {baseType}[] {field.Name};");
                }
            }
            else if (field.IsList == false)
            {
                sb.AppendLine($"    public {ConvertType(field.Type)} {field.Name};");
                Console.WriteLine($"    public {ConvertType(field.Type)} {field.Name};");
            }
            else
            {
                sb.AppendLine($"    public List<{ConvertType(field.Type)}> {field.Name};");
                Console.WriteLine($"    public List<{ConvertType(field.Type)}> {field.Name};");
            }
        }


        // 파라미터가 있는 생성자 추가
        sb.AppendLine($"    public {structInfo.Name}(");
        for (int i = 0; i < structInfo.Fields.Count; i++)
        {
            var field = structInfo.Fields[i];
            string fieldType = ConvertType(field.Type);
            if (field.Type.Contains('['))
            {
                var match = Regex.Match(field.Type, @"(\w+)\[(\w+)\]");
                if (match.Success)
                {
                    string baseType = ConvertType(match.Groups[1].Value);
                    sb.Append($"        {baseType}[] {field.Name.ToLower()}");
                }
            }
            else
            {
                sb.Append($"        {fieldType} {field.Name.ToLower()}");
            }
            if (i < structInfo.Fields.Count - 1)
                sb.AppendLine(",");
        }
        sb.AppendLine(")");
        sb.AppendLine("    {");
        foreach (var field in structInfo.Fields)
        {
            if (field.Type.Contains('['))
            {
                sb.AppendLine($"        this.{field.Name} = new {ConvertType(field.Type.Split('[')[0])}[Constants.{field.Type.Split('[')[1].TrimEnd(']')}];");
                sb.AppendLine($"        Array.Copy({field.Name.ToLower()}, this.{field.Name}, Constants.{field.Type.Split('[')[1].TrimEnd(']')});");
            }
            else
            {
                sb.AppendLine($"        this.{field.Name} = {field.Name.ToLower()};");
            }
        }
        sb.AppendLine("    }");

        // 기본 생성자 추가
        sb.AppendLine($"    public static {structInfo.Name} Default => new {structInfo.Name}");
        sb.AppendLine("    {");
        for (int i = 0; i < structInfo.Fields.Count; i++)
        {
            var field = structInfo.Fields[i];
            bool isLastField = (i == structInfo.Fields.Count - 1);

            if (field.Type.Contains('['))
            {
                var match = Regex.Match(field.Type, @"(\w+)\[(\w+)\]");
                if (match.Success)
                {
                    string baseType = ConvertType(match.Groups[1].Value);
                    string arraySize = match.Groups[2].Value;
                    sb.AppendLine($"        {field.Name} = new {baseType}[Constants.{arraySize}]{(isLastField ? "" : ",")}");
                }
            }
            else if (field.IsList)
            {
                sb.AppendLine($"        {field.Name} = new List<{ConvertType(field.Type)}>(){(isLastField ? "" : ",")}");
            }
            else
            {
                string defaultValue = GetDefaultValue(field.Type);
                sb.AppendLine($"        {field.Name} = {defaultValue}{(isLastField ? "" : ",")}");
            }
        }
        sb.AppendLine("    };");


        sb.AppendLine($"    public void Serialize(NetBase.PacketBase PacketBase)");
        sb.AppendLine("    {");
        foreach (var field in structInfo.Fields)
        {
            if (field.Type.Contains('['))
            {
                var match = Regex.Match(field.Type, @"(\w+)\[(\w+)\]");
                if (match.Success)
                {
                    string baseType = match.Groups[1].Value;
                    sb.AppendLine($"        foreach (var item in {field.Name})");
                    sb.AppendLine("        {");
                    sb.AppendLine($"            PacketBase.Write(item);");
                    sb.AppendLine("        }");
                }
            }
            else
            {
                sb.AppendLine($"        PacketBase.Write({field.Name});");
            }
        }
        sb.AppendLine("    }");

        sb.AppendLine($"    public void Deserialize(NetBase.PacketBase PacketBase)");
        sb.AppendLine("    {");
        foreach (var field in structInfo.Fields)
        {
            if (field.Type.Contains('['))
            {
                var match = Regex.Match(field.Type, @"(\w+)\[(\w+)\]");
                if (match.Success)
                {
                    string baseType = match.Groups[1].Value;
                    string arraySize = match.Groups[2].Value;
                    sb.AppendLine($"        for (int i = 0; i < Constants.{arraySize}; i++)");
                    sb.AppendLine("        {");
                    sb.AppendLine($"            {field.Name}[i] = PacketBase.Read<{baseType}>();");
                    sb.AppendLine("        }");
                }
            }
            else
            {
                sb.AppendLine($"        {field.Name} = PacketBase.Read<{field.Type}>();");
            }
        }
        sb.AppendLine("    }");
        sb.AppendLine("}");

        Console.WriteLine("}");
        return sb.ToString();
    }
    public int CountInParameters(string ReturnTypeString, IdlParser.MethodInfo Methods)
    {
        int count = 0;
        foreach (var param in Methods.Parameters)
        {
            if (param.ParamModifier == ReturnTypeString)
            {
                count++;
            }
        }
        return count;
    }

    public string GenerateInterface(IdlParser.InterfaceInfo interfaceInfo)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"namespace {interfaceInfo.Name}");
        sb.AppendLine("{");

        foreach (var method in interfaceInfo.Methods)
        {
            bool hasInParams = method.Parameters.Any(p => p.ParamModifier == "in" && p.Type != "void");
            bool hasOutParams = method.Parameters.Any(p => p.ParamModifier == "out" && p.Type != "void");
            bool hasVoidInParam = method.Parameters.Any(p => p.ParamModifier == "in" && p.Type == "void");

            if (hasInParams || hasVoidInParam)
            {
                Console.WriteLine("struct " + method.Name + "Req");
                sb.AppendLine($"    public struct {method.Name}Req");
                sb.AppendLine("    {");
                if (hasInParams)
                {
                    AppendParameters(sb, method.Parameters.Where(p => p.ParamModifier == "in" && p.Type != "void"));
                }
                sb.AppendLine("    }");
            }

            if (hasOutParams)
            {
                Console.WriteLine("struct " + method.Name + "Ack");
                sb.AppendLine($"    public struct {method.Name}Ack");
                sb.AppendLine("    {");
                AppendParameters(sb, method.Parameters.Where(p => p.ParamModifier == "out" && p.Type != "void"));
                sb.AppendLine("    }");
            }
        }
        sb.AppendLine("}");
        return sb.ToString();
    }

    private string GenerateStructForMethod(IdlParser.MethodInfo method)
    {
        var sb = new StringBuilder();

        string suffix = method.Parameters.Any(p => p.ParamModifier == "in") ? "Req" : "Ack";
        Console.WriteLine($"    public struct {method.Name}{suffix}");
        sb.AppendLine($"    public struct {method.Name}{suffix} : IPacketSerializable");
        sb.AppendLine("    {");

        // 파라미터를 구조체의 필드로 추가
        foreach (var param in method.Parameters)
        {
            if ((suffix == "Req" && param.ParamModifier == "in") ||
                (suffix == "Ack" && param.ParamModifier == "out"))
            {
                if (param.Type != "void")
                {
                    sb.AppendLine($"        public {ConvertType(param.Type)} {param.Name};");
                }
            }
        }

        // Serialize 메서드 생성
        sb.AppendLine("        public void Serialize(NetBase.PacketBase PacketBase)");
        sb.AppendLine("        {");
        foreach (var param in method.Parameters)
        {
            if ((suffix == "Req" && param.ParamModifier == "in") ||
                (suffix == "Ack" && param.ParamModifier == "out"))
            {
                if (param.Type != "void")
                {
                    sb.AppendLine($"            PacketBase.Write({param.Name});");
                }
            }
        }
        sb.AppendLine("        }");

        // Deserialize 메서드 생성
        sb.AppendLine("        public void Deserialize(NetBase.PacketBase PacketBase)");
        sb.AppendLine("        {");
        foreach (var param in method.Parameters)
        {
            if ((suffix == "Req" && param.ParamModifier == "in") ||
                (suffix == "Ack" && param.ParamModifier == "out"))
            {
                if (param.Type != "void")
                {
                    sb.AppendLine($"            {param.Name} = PacketBase.Read<{param.Type}>();");
                }
            }
        }
        sb.AppendLine("        }");

        sb.AppendLine("    }");
        return sb.ToString();
    }

    private void AppendParameters(StringBuilder sb, IEnumerable<IdlParser.ArgInfo> parameters)
    {
        foreach (var param in parameters)
        {
            Console.WriteLine($"        public {ConvertType(param.Type)} {param.Name};");
            sb.AppendLine($"        public {ConvertType(param.Type)} {param.Name};");
        }

        // Serialize 메서드 생성
        sb.AppendLine("        public void Serialize(NetBase.PacketBase PacketBase)");
        sb.AppendLine("        {");
        foreach (var param in parameters)
        {
            if (param.Type != "void")
            {
                sb.AppendLine($"            PacketBase.Write({param.Name});");
            }
        }
        sb.AppendLine("        }");

        // Deserialize 메서드 생성
        sb.AppendLine("        public void Deserialize(NetBase.PacketBase PacketBase)");
        sb.AppendLine("        {");
        foreach (var param in parameters)
        {
            if (param.Type != "void")
            {
                sb.AppendLine($"            {param.Name} = PacketBase.Read<{param.Type}>();");
            }
        }
        sb.AppendLine("        }");


    }

    public string GenerateEnum(IdlParser.EnumInfo enumInfo)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"public enum {enumInfo.Name}");
        sb.AppendLine("{");

        for (int i = 0; i < enumInfo.Values.Count; i++)
        {
            sb.Append($"    {enumInfo.Values[i]}");
            if (i < enumInfo.Values.Count - 1)
            {
                sb.AppendLine(",");
            }
        }

        sb.AppendLine();
        sb.AppendLine("}");
        return sb.ToString();
    }

    public string GeneratePacketTypeEnum(List<IdlParser.InterfaceInfo> interfaces)
    {
        var sb = new StringBuilder();
        sb.AppendLine("public enum PacketType");
        sb.AppendLine("{");
        sb.AppendLine("    None,");

        foreach (var interfaceInfo in interfaces)
        {
            foreach (var method in interfaceInfo.Methods)
            {
                bool hasInParams = method.Parameters.Any(p => p.ParamModifier == "in");
                bool hasOutParams = method.Parameters.Any(p => p.ParamModifier == "out" && p.Type != "void");

                if (hasInParams)
                    sb.AppendLine($"    {method.Name}Req,");
                if (hasOutParams)
                    sb.AppendLine($"    {method.Name}Ack,");
            }
        }

        sb.AppendLine("    Max,");
        sb.AppendLine("}");
        return sb.ToString();
    }

    public string GenerateFileIncludes(IdlParser.IdlInfo idlInfo)
    {
        var sb = new StringBuilder();
        foreach (var include in idlInfo.Includes)
        {
            sb.AppendLine($"#include \"{include.FileName}\"");
        }
        return sb.ToString();
    }

    public void GenerateCSharpFiles(string extractedName, IdlParser.IdlInfo idlInfo, string outputDirectory)
    {
        foreach (var include in idlInfo.Includes)
        {
            var filePath = Path.Combine(outputDirectory, Path.ChangeExtension(include.FileName, ".cs"));
            using (var writer = new StreamWriter(filePath, false)) // Overwrite existing files
            {
                writer.WriteLine("using System;");
                writer.WriteLine("using System.Collections.Generic;");
                writer.WriteLine("using NetBase;");
                writer.WriteLine();

                var match = Regex.Match(include.FileName, @"^(.*)\.idl$");
                if (match.Success)
                {
                    string idlName = match.Groups[1].Value;
                    writer.WriteLine("namespace " + idlName);
                    writer.WriteLine("{");
                    writer.WriteLine(GenerateConstAndEnums(include.FileIdlInfo));
                    foreach (var structInfo in include.FileIdlInfo.Structs)
                    {
                        writer.WriteLine(GenerateStruct(structInfo));
                    }
                    foreach (var interfaceInfo in include.FileIdlInfo.Interfaces)
                    {
                        writer.WriteLine(GenerateInterface(interfaceInfo));
                    }
                    writer.WriteLine("}");
                }
            }
        }

        var mainFilePath = Path.Combine(outputDirectory, Path.ChangeExtension(extractedName, ".cs"));
        using (var writer = new StreamWriter(mainFilePath, false)) // Overwrite existing files
        {
            writer.WriteLine("using System;");
            writer.WriteLine("using System.Collections.Generic;");
            writer.WriteLine("using NetBase;");
            writer.WriteLine();

            foreach (var include in idlInfo.Includes)
            {
                var includeFileName = Path.ChangeExtension(include.FileName, null);
                writer.WriteLine($"using {includeFileName};");
            }

            writer.WriteLine($"namespace {GetSolutionName()}");
            writer.WriteLine("{");

            writer.WriteLine("    public interface IPacketSerializable");
            writer.WriteLine("    {");
            writer.WriteLine("        void Serialize(PacketBase packet);");
            writer.WriteLine("        void Deserialize(PacketBase packet);");
            writer.WriteLine("    }");

            writer.WriteLine(GenerateConstAndEnums(idlInfo));
            foreach (var structInfo in idlInfo.Structs)
            {
                writer.WriteLine(GenerateStruct(structInfo));
            }
            foreach (var interfaceInfo in idlInfo.Interfaces)
            {
                writer.WriteLine(GenerateInterface(interfaceInfo));
            }
            writer.WriteLine("}");
        }
    }

    private string GenerateConstAndEnums(IdlParser.IdlInfo idlInfo)
    {
        var sb = new StringBuilder();
        sb.AppendLine("public static class Constants");
        sb.AppendLine("{");
        foreach (var constInfo in idlInfo.Constants)
        {
            sb.AppendLine("\t" + GenerateConst(constInfo));
        }
        sb.AppendLine("}");
        foreach (var enumInfo in idlInfo.Enums)
        {
            sb.AppendLine(GenerateEnum(enumInfo));
        }

        sb.AppendLine(GeneratePacketTypeEnum(idlInfo.Interfaces));

        return sb.ToString();
    }

    private string ConvertType(string idlType)
    {
        // 기존의 타입 변환 로직을 유지하고, List<T>를 처리하는 로직 추가
        if (idlType.StartsWith("List<") && idlType.EndsWith(">"))
        {
            string innerType = idlType.Substring(5, idlType.Length - 6);
            return $"List<{ConvertType(innerType)}>";
        }

        switch (idlType)
        {
            case "int":
                return "int";
            case "float":
                return "float";
            case "bool":
                return "bool";
            case "char":
                return "char";
            case "string":
                return "string";
            case "void":
                return "void";
            default:
                return idlType;
        }
    }

    private string GetDefaultValue(string type)
    {
        switch (type)
        {
            case "int":
                return "0";
            case "float":
                return "0f";
            case "bool":
                return "false";
            case "char":
                return "'\0'";
            case "string":
                return "string.Empty";
            default:
                return "default";
        }
    }

}

