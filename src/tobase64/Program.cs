using System.Buffers;
using System.Text;

if (args.Length == 0)
{
    var pipeInput = Console.OpenStandardInput();

    var buffer = ArrayPool<byte>.Shared.Rent(3072);
    var pipeInputTextTask = pipeInput.ReadAsync(buffer, 0, 3);
    await Task.WhenAny(pipeInputTextTask, Task.Delay(TimeSpan.FromMilliseconds(100)));
    if (pipeInputTextTask.IsCompleted)
    {
        var text = Encoding.UTF8.GetString(buffer, 0, await pipeInputTextTask);
        Console.Write(Convert.ToBase64String(Encoding.UTF8.GetBytes(text)));
        var count = await pipeInput.ReadAsync(buffer.AsMemory(0, 3072));
        while (count > 0)
        {
            Console.Write(Convert.ToBase64String(buffer, 0, count));
            count = await pipeInput.ReadAsync(buffer.AsMemory(0, 3072));
        }
    }
    else
    {
        Console.WriteLine("Usage: tobase64 <string-to-convert>");
        Console.WriteLine("       or pipe string to stdin");
        return;
    }
    ArrayPool<byte>.Shared.Return(buffer);
}
else
{
    var sourceString = string.Join(" ", args);
    var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(sourceString));
    Console.WriteLine(base64);
}
