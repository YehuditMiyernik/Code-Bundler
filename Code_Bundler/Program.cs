using System.CommandLine;

var bundleCommand = new Command("bundle", "Bundle code files for single file");
var create_rsp = new Command("create_rsp", "Create command for bundle command");

var language = new Option<string>(new string[] { "--language", "-l" }, "language")
{
    IsRequired = true
};
var output = new Option<FileInfo>(new string[] { "--output", "-o" }, "file path and name")
{
    IsRequired = true
};
var note = new Option<bool>(new string[] { "--note", "-n" }, "note file source");
var sort = new Option<bool>(new string[] { "--sort", "-s" }, "sort");
var removeEmptyLines = new Option<bool>(new string[] { "--clean", "-c" }, "remove empty lines");
var author = new Option<string>(new string[] { "--author", "-a" }, "author name");

bundleCommand.AddOption(language);
bundleCommand.AddOption(output);
bundleCommand.AddOption(note);
bundleCommand.AddOption(sort);
bundleCommand.AddOption(removeEmptyLines);
bundleCommand.AddOption(author);

bundleCommand.SetHandler((language, output, note, sort, removeEmptyLines, author) =>
{
    try
    {
        //Create file
        using (StreamWriter writer = new StreamWriter(output.FullName))
        {
            if (author != null)
            {
                writer.WriteLine($"// Name: {author}");
                writer.WriteLine();
            }
            //Find files with the requested extension
            string extension = GetFileExtension(language);
            var query = Directory.GetFiles(Directory.GetCurrentDirectory(), $"*{extension}")
            .Where(filePath => filePath != output.FullName);

            //Sort files by user request
            query = sort
                ? query.OrderBy(filePath => Path.GetExtension(filePath))
                : query.OrderBy(filePath => Path.GetFileName(filePath));
            string[] files = query.ToArray();

            //Add files content to bundle file
            foreach (string filePath in files)
            {
                //Note file path
                if (note)
                    writer.WriteLine($"// File path: {filePath}");
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    if (removeEmptyLines && string.IsNullOrWhiteSpace(line))
                        continue;
                    writer.WriteLine(line);
                }
                writer.WriteLine();
            }
        }
        Console.WriteLine("file was created");
    }
    catch(Exception ex) 
    {
        Console.WriteLine(ex.Message);
    }
}, language, output, note, sort, removeEmptyLines, author);

create_rsp.SetHandler(() =>
{
    string command = " ";
    Console.WriteLine("Enter language to bundle");
    command += $"--language {Console.ReadLine()} ";
    Console.WriteLine("Enter output file name and path");
    command += $"--output {Console.ReadLine()} ";
    Console.WriteLine("Enter author name");
    command += $"--author {Console.ReadLine()} ";
    Console.WriteLine("Sort files by language? [y/n]");
    command += Console.ReadLine().ToLower() == "y" ? "--sort " : "";
    Console.WriteLine("Note file path in output file? [y/n]");
    command += Console.ReadLine().ToLower() == "y" ? "--note " : "";
    Console.WriteLine("Remove empty lines? [y/n]");
    command += Console.ReadLine().ToLower() == "y" ? "--clean " : "";
    File.WriteAllText("bundle.rsp", command);
});

string GetFileExtension(string language)
{
    var languageExtensions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "CSharp", ".cs" },
        { "c#", ".cs" },
        { "cs", ".cs" },
        { "java", ".java" },
        { "python", ".py" },
        { "py", ".py" },
        { "html", ".html" },
        { "JavaScript", ".js" },
        { "js", ".js" },
        { "TypeScript", ".ts" },
        { "ts", ".ts" },
        { "Ruby", ".rb" },
        { "rb", ".rb" },
        { "php", ".php" },
        { "cpp", ".cpp" },
        { "C++", ".cpp" },
        { "h", ".h" },
        { "all", ".*" }
    };
    var extension = languageExtensions.GetValueOrDefault(language);
    return extension ?? throw new Exception($"Error: Unsupported language {language}");
}

var rootCommand = new RootCommand("Root command for file bundler CLI");
rootCommand.AddCommand(bundleCommand);
rootCommand.AddCommand(create_rsp);
rootCommand.InvokeAsync(args);
