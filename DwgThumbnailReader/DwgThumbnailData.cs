using System;

namespace DwgThumbnailReader;

public class DwgThumbnailData
{
    public DwgThumbnailImageType ImageType { get; set; } = DwgThumbnailImageType.None;

    public byte[] Bytes { get; set; } = [];

    public long Length { get; set; } = 0;

    public static DwgThumbnailData Empty()
    {
        return new DwgThumbnailData();
    }
}
