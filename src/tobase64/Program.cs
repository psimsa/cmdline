using System.Text;

if (args.Length == 0)
{
    Console.WriteLine("Usage: tobase64 <string-to-convert>");
    return;
}
var sourceString = string.Join(" ", args);
var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(sourceString));
Console.WriteLine(base64);