using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DwgThumbnailReader;

public class DwgThumbnail
{
    public static DwgThumbnailImage GetImage(string fileName)
    {
        try
        {
            using var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return GetImage(fs);
        }
        catch (Exception e)
        {
            _ = e;
            return DwgThumbnailImage.Empty();
        }
    }

    public static DwgThumbnailImage GetImage(Stream s)
    {
        try
        {
            return new DwgThumbnail().GetThumbnail(s);
        }
        catch (Exception e)
        {
            _ = e;
            return DwgThumbnailImage.Empty();
        }
    }

    private DwgThumbnailImage GetThumbnail(Stream s)
    {
        if (!IsDwg(s))
            return DwgThumbnailImage.Empty();

        using BinaryReader br = new BinaryReader(s);
        s.Seek(0xD, SeekOrigin.Begin);

        var nextPos = 0x14 + br.ReadInt32();
        if (nextPos >= s.Length)
            return new DwgThumbnailImage();
        s.Seek(nextPos, SeekOrigin.Begin);

        var bytCnt = br.ReadByte();
        if (bytCnt <= 1)
            return DwgThumbnailImage.Empty();

        for (short i = 1; i <= bytCnt; i++)
        {
            // image type
            var imageCode = br.ReadByte();
            // image start pos
            var imageHeaderStart = br.ReadInt32();
            // image size
            var imageHeaderSize = br.ReadInt32();

            // bmp
            if (imageCode == 2)
            {
                s.Seek(imageHeaderStart, SeekOrigin.Begin);

                // BITMAPINFOHEADER (40 bytes)
                br.ReadBytes(0xE);
                ushort biBitCount = br.ReadUInt16();
                br.ReadBytes(4); //biCompression
                uint biSizeImage = br.ReadUInt32();

                s.Seek(imageHeaderStart, SeekOrigin.Begin);
                byte[] bitmapBuffer = br.ReadBytes(imageHeaderSize);
                uint colorTableSize = (uint)((biBitCount < 9) ? 4 * Math.Pow(2, biBitCount) : 0);
                using MemoryStream ms = new MemoryStream();
                using BinaryWriter bw = new BinaryWriter(ms);

                bw.Write((ushort)0x4D42);
                bw.Write(54U + colorTableSize + biSizeImage);
                bw.Write(new ushort());
                bw.Write(new ushort());
                bw.Write(54U + colorTableSize);
                bw.Write(bitmapBuffer);

                return new DwgThumbnailImage
                {
                    ImageType = DwgThumbnailImageType.Bmp,
                    Bytes = [.. ms.GetBuffer()],
                    Length = ms.Length
                };
            }

            // unknown
            if (imageCode == 3)
            {
                return DwgThumbnailImage.Empty();
            }

            // png
            if (imageCode == 6)
            {
                // png start
                s.Seek(imageHeaderStart, SeekOrigin.Begin);

                if (!IsPositionInFile(s, imageHeaderSize))
                    return DwgThumbnailImage.Empty();

                var data = new DwgThumbnailImage
                {
                    ImageType = DwgThumbnailImageType.Png
                };

                var pngSize = imageHeaderSize;

                data.Bytes = new byte[pngSize];
                data.Length = pngSize;
                s.Seek(imageHeaderStart, SeekOrigin.Begin);
                _ = s.Read(data.Bytes, 0, (int)pngSize);

                return data;
            }
        }

        return DwgThumbnailImage.Empty();
    }

    private bool IsDwg(Stream s)
    {
        const int acLen = 2;

        s.Seek(0, SeekOrigin.Begin);

        if (s.Length < acLen)
            return false;

        var ac = new byte[acLen];
        if (s.Read(ac, 0, acLen) != acLen)
            return false;

        return ac[0] == 0x41 && ac[1] == 0x43;
    }

    private bool IsPositionInFile(Stream s, Int64 pos)
    {
        return (s.Position + pos) < s.Length;
    }
}
