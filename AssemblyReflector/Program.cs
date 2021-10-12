using System;
using System.IO;
using CommandLine;

namespace AssemblyReflector
{
    internal class Program
    {
        public class LaunchOptions
        {
            [Option('i', "input", Required = true, HelpText = "Path to input assembly.")]
            public string InputPath { get; set; } = null!;

            [Option('o', "output", Required = true, HelpText = "Path to output assembly.")]
            public string OutputPath { get; set; } = null!;
            
            [Option('f', "force", Default = false, HelpText = "If set then output files will be overwritten if they exist.")]
            public bool Force { get; set; }
        }
        
        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<LaunchOptions>(args).WithParsed(MainWithOptions);
        }

        private static void MainWithOptions(LaunchOptions options)
        {
            if (!ValidateOptions(options, out string? error))
            {
                Console.WriteLine(error);
                return;
            }

            var reflector = new Reflector(options.InputPath);
            reflector.CleanAssembly();

            if (Directory.Exists(options.OutputPath))
            {
                options.OutputPath = Path.Combine(options.OutputPath, Path.GetFileName(options.InputPath));
            }

            reflector.AssemblyDefinition.Write(options.OutputPath);
        }

        private static bool ValidateOptions(LaunchOptions options, out string? errorMessage)
        {
            try
            {
                try
                {
                    options.InputPath = Path.GetFullPath(options.InputPath);
                    options.OutputPath = Path.GetFullPath(options.OutputPath);
                }
                catch (Exception ex) when (ex is NotSupportedException or ArgumentException)
                {
                    errorMessage = $"Input or output path contains invalid characters: {ex.Message}";
                    return false;
                }

                if (!File.Exists(options.InputPath))
                {
                    errorMessage = "Input file does not exist";
                    return false;
                }

                if (File.GetAttributes(options.InputPath) == FileAttributes.Directory)
                {
                    errorMessage = "Input path is a directory";
                    return false;
                }

                FileAttributes? outputFileAttributes = null;
                
                try
                {
                    outputFileAttributes = File.GetAttributes(options.OutputPath);
                }
                catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
                {
                    // Create output directory if path does not have an extension
                    if (Path.GetExtension(options.OutputPath) == string.Empty)
                        Directory.CreateDirectory(options.OutputPath);
                    else
                    {
                        string? directoryName = Path.GetDirectoryName(options.OutputPath);

                        if (directoryName == null)
                        {
                            errorMessage = "Output path is invalid";
                            return false;
                        }
                        
                        // Create the directory/directories that the output is contained within
                        if (directoryName != options.OutputPath)
                            Directory.CreateDirectory(directoryName);
                    }

                    if (File.Exists(options.OutputPath) || Directory.Exists(options.OutputPath))
                        outputFileAttributes = File.GetAttributes(options.OutputPath);
                }
                
                if (outputFileAttributes != FileAttributes.Directory && !options.Force && File.Exists(options.OutputPath))
                {
                    errorMessage = "Output path file already exists. Specify --force to overwrite output files.";
                    return false;
                }

                errorMessage = null;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Could not verify arguments: {ex.Message}";
                return false;
            }
        }
    }
}