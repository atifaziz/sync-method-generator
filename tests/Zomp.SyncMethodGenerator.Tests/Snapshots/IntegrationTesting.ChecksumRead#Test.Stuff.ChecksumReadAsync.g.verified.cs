﻿//HintName: Test.Stuff.ChecksumReadAsync.g.cs
// <auto-generated/>
#nullable enable
namespace Test;
public partial class Stuff
{
    static int ChecksumRead(global::System.Span<byte> buffer, global::System.IO.Stream stream)
    {
        int bytesRead = stream.Read(buffer);
        return Checksum(buffer.Slice(0, bytesRead));
    }
}
