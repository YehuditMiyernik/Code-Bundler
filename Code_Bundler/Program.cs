using System.CommandLine;

//catch errors
//catch this error - '-' was not matched. Did you mean one of the following?
//fib bundle -n -o "C:\Users\ymiyernx\OneDrive - Intel Corporation\Desktop\פרקטיקוד\Code-Bundler-master\Code_Bundler\File.txt" -a Yehudit-Holtzman -c

var bundleCommand = new Command("bundle", "Bundle code files for single file");

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
        using (StreamWriter writer = new StreamWriter(output.FullName))
        {
            if (author != null)
            {
                writer.WriteLine($"// Name: {author}");
                writer.WriteLine();
            }
            string extension = GetFileExtension(language);
            var query = Directory.GetFiles(Directory.GetCurrentDirectory(), $"*{extension}")
            .Where(filePath => filePath != output.FullName);

            query = sort
                ? query.OrderBy(filePath => Path.GetExtension(filePath))
                : query.OrderBy(filePath => Path.GetFileName(filePath));
            string[] files = query.ToArray();

            foreach (string filePath in files)
            {
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
        Console.WriteLine(ex.ToString());
    }
}, language, output, note, sort, removeEmptyLines, author);

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
    return extension ?? throw new ArgumentException($"Unsupported language or file extension: {language}");
}

var rootCommand = new RootCommand("Root command for file bundler CLI");
rootCommand.AddCommand(bundleCommand);
rootCommand.InvokeAsync(args);
