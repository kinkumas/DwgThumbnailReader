using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DwgThumbnailReader;

public class DwgThumbnail
{
    public static DwgThumbnailData GetImage(string fileName)
    {
        try
        {
            using var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return GetImage(fs);
        }
        catch (Exception e)
        {
            _ = e;
            return DwgThumbnailData.Empty();
        }
    }

    public static DwgThumbnailData GetImage(Stream s)
    {
        try
        {
            return GetThumbnail(s);
        }
        catch (Exception e)
        {
            _ = e;
            return DwgThumbnailData.Empty();
        }
    }

    private static DwgThumbnailData GetThumbnail(Stream s)
    {
        if (!IsDwg(s))
            return DwgThumbnailData.Empty();

        using BinaryReader br = new BinaryReader(s);
        s.Seek(0xD, SeekOrigin.Begin);

        var nextPos = 0x14 + br.ReadInt32();
        if (nextPos >= s.Length)
            return new DwgThumbnailData();
        s.Seek(nextPos, SeekOrigin.Begin);

        var bytCnt = br.ReadByte();
        if (bytCnt <= 1)
            return DwgThumbnailData.Empty();

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
                //bw.Write(new byte[] { 0x42, 0x4D });
                bw.Write((ushort)0x4D42);
                bw.Write(54U + colorTableSize + biSizeImage);
                bw.Write(new ushort());
                bw.Write(new ushort());
                bw.Write(54U + colorTableSize);
                bw.Write(bitmapBuffer);

                return new DwgThumbnailData
                {
                    ImageType = DwgThumbnailImageType.Bmp,
                    Bytes = [.. ms.GetBuffer()],
                    Length = ms.Length
                };
            }

            // unknown
            if (imageCode == 3)
            {
                return DwgThumbnailData.Empty();
            }

            // png
            if (imageCode == 6)
            {
                // png start
                s.Seek(imageHeaderStart, SeekOrigin.Begin);

                if (!IsPositionInFile(s, imageHeaderSize))
                    return DwgThumbnailData.Empty();

                var data = new DwgThumbnailData
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

        return DwgThumbnailData.Empty();
    }

    private static bool IsDwg(Stream s)
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

    private static bool IsPositionInFile(Stream s, Int64 pos)
    {
        return (s.Position + pos) < s.Length;
    }
}
