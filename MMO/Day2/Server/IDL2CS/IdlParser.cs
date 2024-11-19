using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static IdlParser;

public class IdlParser
{
    public class ArgInfo
    {
        public string ParamModifier { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
    }

    public class MethodInfo
    {
        public string ReturnType { get; set; }
        public string Name { get; set; }
        public List<ArgInfo> Parameters { get; set; } = new List<ArgInfo>();
    }

    public class InterfaceInfo
    {
        public string Name { get; set; }
        public List<MethodInfo> Methods { get; set; } = new List<MethodInfo>();
    }

    public class StructFieldInfo
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public bool IsList { get; set; }
    }

    public class StructInfo
    {
        public string Name { get; set; }
        public List<StructFieldInfo> Fields { get; set; } = new List<StructFieldInfo>();
    }

    public class EnumInfo
    {
        public string Name { get; set; }
        public List<string> Values { get; set; } = new List<string>();
    }

    public class ConstInfo
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class IdlInfo
    {
        public List<ConstInfo> Constants { get; set; } = new List<ConstInfo>();
        public List<EnumInfo> Enums { get; set; } = new List<EnumInfo>();
        public List<StructInfo> Structs { get; set; } = new List<StructInfo>();
        public List<InterfaceInfo> Interfaces { get; set; } = new List<InterfaceInfo>();
        public List<(string FileName, IdlInfo FileIdlInfo)> Includes { get; set; } = new List<(string FileName, IdlInfo FileIdlInfo)>();
    }

    public IdlInfo Parse(string idlContent, string idlDirectory)
    {
        var idlInfo = new IdlInfo();

        // Parse includes
        var includeMatches = Regex.Matches(idlContent, @"#include\s+""([^""]+)""");
        foreach (Match match in includeMatches)
        {
            string includePath = Path.Combine(idlDirectory, match.Groups[1].Value);
            if (File.Exists(includePath))
            {
                string includeContent = File.ReadAllText(includePath);
                var includeInfo = Parse(includeContent, idlDirectory);
                idlInfo.Includes.Add((match.Groups[1].Value, includeInfo));
            }
        }

        // Parse constants
        var constMatches = Regex.Matches(idlContent, @"#define\s+(\w+)\s+([^\s]+)");
        foreach (Match match in constMatches)
        {
            idlInfo.Constants.Add(new ConstInfo
            {
                Name = match.Groups[1].Value,
                Value = match.Groups[2].Value
            });
        }

        // Parse enums
        var enumMatches = Regex.Matches(idlContent, @"enum\s+(\w+)\s*{([^}]*)}");
        foreach (Match match in enumMatches)
        {
            var enumInfo = new EnumInfo { Name = match.Groups[1].Value };
            var values = match.Groups[2].Value.Split(',');

            foreach (var value in values)
            {
                enumInfo.Values.Add(value.Trim());
            }
            idlInfo.Enums.Add(enumInfo);
        }

        // Parse structs
        var structMatches = Regex.Matches(idlContent, @"struct\s+(\w+)\s*{([^}]*)}");
        foreach (Match match in structMatches)
        {
            var structInfo = new StructInfo { Name = match.Groups[1].Value };
            var fieldsBlock = match.Groups[2].Value;

            var fieldMatches = Regex.Matches(fieldsBlock, @"(?:\r?\n\s*)(\w+(?:<.*?>)?)\s+(\w+)(?:\[(\w+)\])?;");
            foreach (Match fieldMatch in fieldMatches)
            {
                string fieldType = fieldMatch.Groups[1].Value;
                string fieldName = fieldMatch.Groups[2].Value;
                string arraySize = fieldMatch.Groups[3].Value;

                if (!string.IsNullOrEmpty(arraySize))
                {
                    fieldType = $"{fieldType}[{arraySize}]";
                }

                Match fieldMatchType = Regex.Match(fieldType, @"^(List<(.+)>)$");
                if (fieldMatchType.Success)
                {
                    structInfo.Fields.Add(new StructFieldInfo
                    {
                        Type = fieldType.Substring(5, fieldType.Length - 6),
                        Name = fieldName,
                        IsList = true
                    });
                }
                else
                {
                    structInfo.Fields.Add(new StructFieldInfo
                    {
                        Type = fieldType,
                        Name = fieldName,
                        IsList = false
                    });
                }
            }

            idlInfo.Structs.Add(structInfo);
        }

        // Parse interfaces
        var interfaceMatches = Regex.Matches(idlContent, @"interface\s+(\w+)\s*{([^}]*)}");
        foreach (Match match in interfaceMatches)
        {
            var interfaceInfo = new InterfaceInfo { Name = match.Groups[1].Value };
            var methodsBlock = match.Groups[2].Value;

            var methodMatches = Regex.Matches(methodsBlock, @"(\w+)\s+(\w+)\s*\(([^)]*)\);");
            foreach (Match methodMatch in methodMatches)
            {
                var methodInfo = new MethodInfo
                {
                    ReturnType = methodMatch.Groups[1].Value,
                    Name = methodMatch.Groups[2].Value
                };

                var parameters = methodMatch.Groups[3].Value.Split(',');
                foreach (var param in parameters)
                {
                    var paramParts = param.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (paramParts.Length == 3)
                    {
                        var argInfo = new ArgInfo
                        {
                            ParamModifier = paramParts[0],
                            Type = paramParts[1],
                            Name = paramParts[2]
                        };

                        methodInfo.Parameters.Add(argInfo);
                    }
                    else if (paramParts.Length == 2)
                    {
                        var argInfo = new ArgInfo
                        {
                            ParamModifier = paramParts[0],
                            Type = paramParts[1],
                            Name = ""
                        };

                        methodInfo.Parameters.Add(argInfo);
                    }
                }

                interfaceInfo.Methods.Add(methodInfo);
            }

            idlInfo.Interfaces.Add(interfaceInfo);
        }

        return idlInfo;
    }
}
