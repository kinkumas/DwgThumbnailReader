## **Description**



A lightweight and efficient library for extracting thumbnail images embedded in AutoCAD's **`.dwg` files**.


------



## **Features**



- **Simple API:** Provides an intuitive and easy-to-use interface to effortlessly extract thumbnail images.
- **Cross-Platform:** Works on various environments, including Windows, macOS, and Linux.

------



## **Usage**



```c#
using DwgThumbnailReader;

var thumbnail = DwgThumbnail.GetImage("drawing.dwg");

if (thumbnail.ImageType != DwgThumbnailImageType.None)
  Console.WriteLine($"\ntype:{thumbnail.ImageType}, size:{thumbnail.Bytes.Length}");
```

------



## **License**



MIT License