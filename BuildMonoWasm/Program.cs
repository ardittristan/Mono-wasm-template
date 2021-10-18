using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;
using CommandLine;

namespace BuildMonoWasm
{
    class Program
    {
        private static string _monoPath = string.Empty;

        private static Options _options;

        private static string _args = "packager.exe --aot --link-mode=all --copy=always";

        public class Options
        {
            [Option('e', "emscripten", Required = true, HelpText = "Path to emscripten bin.")]
            public string EmscriptenPath { get; set; }

            [Option('a', "assembly", Required = true, HelpText = "Path to input assembly.")]
            public string Assembly { get; set; }

            [Option('s', "search-path", Required = false, HelpText = "Path to compiled assemblies.")]
            public IEnumerable<string> SearchPath { get; set; }

            [Option('o', "out", Required = false, Default = "./", HelpText = "Output directory.")]
            public string Out { get; set; }
        }

        static void Main(string[] args)
        {
            new Parser(o => o.AllowMultiInstance = true).ParseArguments<Options>(args).WithParsed(o =>
            {
                _options = o;
            });

            _args += " --emscripten-sdkdir=" + Surround(_options.EmscriptenPath);
            foreach (string path in _options.SearchPath)
            {
                _args += " --search-path=" + Surround(path);
            }
            _args += " --out=" + Surround(_options.Out);

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"HKEY_LOCAL_MACHINE\SOFTWARE\Mono"))
            {
                _monoPath = Path.Combine((string)key?.GetValue("SdkInstallRoot") ?? @"C:\Program Files\Mono\", "bin");
            }

            _args += " --mono-sdkdir=" + Surround(_monoPath);

            _args += " --search-path=./wasm-bcl/wasm";
            _args += " --search-path=./wasm-bcl/wasm/Facades";

            _args += " " + Surround(_options.Assembly);

            using (Process process = new Process())
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = Path.Combine(_monoPath, "mono.exe");
                process.StartInfo.Arguments = _args;
                process.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();

                process.Start();
                process.WaitForExit();
            }
        }

        private static string Surround(string input) => '"' + input.Replace("\"", "\\\"") + '"';
    }
}
