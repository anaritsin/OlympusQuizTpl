
int _lineCount = 0;
SemaphoreSlim _semaphoreAsyncLock = new SemaphoreSlim(1, 1);

try
{
    var filePath = InitializeOutFile();

    if (File.Exists(filePath))
    {
        // Launch 10 Tasks (there is no guarantee each task would launch a new thread)
        //The runtime attempts to optimize resource utilization by reusing threads, using asynchronous I/ O operations, and intelligently scheduling task continuations.
        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(WriteLinesAsync(filePath));
        }

        // Wait for all threads to complete
        await Task.WhenAll(tasks);
    }

    // Wait for user input before exiting
    Console.WriteLine("Press any key to exit...");
    Console.Read();
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}

string InitializeOutFile()
{
    try
    {
        // Output directory
        const string outputDirectory = "/log/";
        // File path
        string filePath = Path.Combine(outputDirectory, "out.txt");

        // Ensure output directory exists
        Directory.CreateDirectory(outputDirectory);

        // Create or overwrite the file and initialize with the first line
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine($"{_lineCount}, 0, {DateTime.Now:HH:mm:ss.fff}");
        }

        return filePath;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Unable to create output file: {ex.Message}");
        return string.Empty;
    }
}

async Task WriteLinesAsync(string filePath)
{
    for (int i = 0; i < 10; i++)
    {
        await WriteLineAsync(filePath);
    }
}

async Task WriteLineAsync(string filePath)
{
    await _semaphoreAsyncLock.WaitAsync();

    try
    {
        var line = $"{Interlocked.Increment(ref _lineCount)}, {Environment.CurrentManagedThreadId}, {DateTime.Now:HH:mm:ss.fff}";

        await File.AppendAllTextAsync(filePath, line + Environment.NewLine);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Thread {Environment.CurrentManagedThreadId} threw an exception: {ex.Message}");
    }
    finally
    {
        _semaphoreAsyncLock?.Release();
    }
}
