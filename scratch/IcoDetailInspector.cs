using System;
using System.IO;

class IcoDetailInspector
{
    static void Main(string[] args)
    {
        if (args.Length == 0) { Console.WriteLine("Usage: IcoDetailInspector <path>"); return; }
        string path = args[0];
        if (!File.Exists(path)) { Console.WriteLine("File not found: " + path); return; }

        byte[] data = File.ReadAllBytes(path);
        Console.WriteLine($"File size: {data.Length} bytes");

        if (data.Length < 6) { Console.WriteLine("Too small for ICO"); return; }

        short reserved = BitConverter.ToInt16(data, 0);
        short type = BitConverter.ToInt16(data, 2);
        short count = BitConverter.ToInt16(data, 4);
        Console.WriteLine($"Reserved: {reserved}, Type: {type} (1=ICO, 2=CUR), Count: {count}");

        byte[] pngSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        for (int i = 0; i < count; i++)
        {
            int dirOffset = 6 + i * 16;
            byte width = data[dirOffset];
            byte height = data[dirOffset + 1];
            byte colorCount = data[dirOffset + 2];
            byte reservedByte = data[dirOffset + 3];
            short planes = BitConverter.ToInt16(data, dirOffset + 4);
            short bitCount = BitConverter.ToInt16(data, dirOffset + 6);
            int dataSize = BitConverter.ToInt32(data, dirOffset + 8);
            int dataOffset = BitConverter.ToInt32(data, dirOffset + 12);

            int w = width == 0 ? 256 : width;
            int h = height == 0 ? 256 : height;

            Console.WriteLine();
            Console.WriteLine($"--- Entry {i} ---");
            Console.WriteLine($"  Directory: Width={w}, Height={h}, Colors={colorCount}, Reserved={reservedByte}");
            Console.WriteLine($"  Planes={planes}, BitCount={bitCount}");
            Console.WriteLine($"  DataSize={dataSize}, DataOffset={dataOffset}");

            // Check if data is PNG
            if (dataOffset + 8 <= data.Length)
            {
                bool isPng = true;
                for (int j = 0; j < 8; j++)
                {
                    if (data[dataOffset + j] != pngSignature[j]) { isPng = false; break; }
                }
                Console.WriteLine($"  Format: {(isPng ? "PNG" : "BMP/DIB")}");

                if (isPng && dataOffset + 24 <= data.Length)
                {
                    // PNG IHDR chunk - width at offset 16, height at offset 20
                    int pngWidth = (data[dataOffset + 16] << 24) | (data[dataOffset + 17] << 16) | 
                                   (data[dataOffset + 18] << 8) | data[dataOffset + 19];
                    int pngHeight = (data[dataOffset + 20] << 24) | (data[dataOffset + 21] << 16) | 
                                    (data[dataOffset + 22] << 8) | data[dataOffset + 23];
                    byte bitDepth = data[dataOffset + 24];
                    byte colorType = data[dataOffset + 25];
                    Console.WriteLine($"  PNG actual size: {pngWidth}x{pngHeight}, BitDepth={bitDepth}, ColorType={colorType}");
                }
                else if (!isPng)
                {
                    // BMP header
                    int bmpHeaderSize = BitConverter.ToInt32(data, dataOffset);
                    int bmpWidth = BitConverter.ToInt32(data, dataOffset + 4);
                    int bmpHeight = BitConverter.ToInt32(data, dataOffset + 8);
                    Console.WriteLine($"  BMP header size: {bmpHeaderSize}, Width={bmpWidth}, Height={bmpHeight}");
                }
            }

            // Verify data bounds
            if (dataOffset + dataSize > data.Length)
            {
                Console.WriteLine($"  WARNING: Data extends beyond file! (offset {dataOffset} + size {dataSize} = {dataOffset + dataSize} > filesize {data.Length})");
            }
        }
    }
}
