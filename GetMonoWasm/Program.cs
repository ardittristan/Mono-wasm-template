using System.IO;
using System.IO.Compression;
using System.Reflection;
using CommandLine;

namespace GetMonoWasm
{
    class Program
    {
        private static readonly string ExecutingAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

        private static readonly string WasmPath = Path.Combine(ExecutingAssemblyLocation, "mono-wasm");

        public class Options
        {
            [Option('z', "zip-path", Required = true, HelpText = "Path to mono-wasm zip.")]
            public string ZipPath { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
            {
                string[] files = Directory.GetFiles(o.ZipPath);

                foreach (string file in files)
                {
                    if (!file.Contains("mono-wasm")) continue;
                    if (Directory.Exists(WasmPath)) Directory.Delete(WasmPath, true);
                    ZipFile.ExtractToDirectory(file, WasmPath);
                    return;
                }
            });
        }
    }
}