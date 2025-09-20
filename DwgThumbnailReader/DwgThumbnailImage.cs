using System;

namespace DwgThumbnailReader;

public class DwgThumbnailImage
{
    public DwgThumbnailImageType ImageType { get; set; } = DwgThumbnailImageType.None;

    public byte[] Bytes { get; set; } = [];

    public long Length { get; set; } = 0;

    public static DwgThumbnailImage Empty()
    {
        return new DwgThumbnailImage();
    }
}
