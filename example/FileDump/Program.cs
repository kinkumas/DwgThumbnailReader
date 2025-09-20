using System.IO;
using DwgThumbnailReader;

while(true)
{
    Console.WriteLine("select dwg file.");
    var dwgPath = Console.ReadLine();

    if (!File.Exists(dwgPath))
    {
        Console.WriteLine("file not found.");
        continue;
    }

    var thumbnail = DwgThumbnail.GetImage(dwgPath);

    if (thumbnail.ImageType == DwgThumbnailImageType.None)
    {
        Console.WriteLine("no thumbnail.");
        break;
    }

    Console.WriteLine($"\ntype:{thumbnail.ImageType}, size:{thumbnail.Length}");

    break;
} 
