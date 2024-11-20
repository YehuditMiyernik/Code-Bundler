using System.CommandLine;

//fib bundle --output "======/bundleFile.txt" 

var bundleOption = new Option<FileInfo>("--output", "file path and name");

var bundleCommand = new Command("bundle", "Bundle code files for single file");
bundleCommand.AddOption(bundleOption);
bundleCommand.SetHandler((output) => {
    try
    {
        File.Create(output.FullName);
        Console.WriteLine("file was created");
    }
    catch {
        Console.WriteLine("Error: file path is invalid");
    }
}, bundleOption);

var rootCommand = new RootCommand("Root command for file bundler CLI");
rootCommand.AddCommand(bundleCommand);
rootCommand.InvokeAsync(args);
