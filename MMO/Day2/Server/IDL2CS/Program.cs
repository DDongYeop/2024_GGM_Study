using System;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: Program <path to IDL file>");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            return;
        }

        string idlPath = args[0];

        if (!File.Exists(idlPath))
        {
            Console.WriteLine($"File not found: {idlPath}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            return;
        }

        try
        {
            string idlContent = File.ReadAllText(idlPath);
            string idlDirectory = Path.GetDirectoryName(idlPath);

            var parser = new IdlParser();
            var idlInfo = parser.Parse(idlContent, idlDirectory);

            var generator = new CSharpCodeGenerator();
            var match = Regex.Match(idlPath, @"^(.*)\.idl$");
            if (match.Success)
            {
                string extractedName = match.Groups[1].Value;
                var outputDirectory = Path.GetDirectoryName(idlPath);
                generator.GenerateCSharpFiles(extractedName, idlInfo, "../../CodeGenCShap/" + outputDirectory);
                Console.WriteLine($"C# code generated successfully in directory: {"./CodeGenCShap/" + outputDirectory}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
