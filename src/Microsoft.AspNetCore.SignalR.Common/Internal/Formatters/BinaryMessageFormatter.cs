﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Binary;
using System.Buffers;
using System.IO;

namespace Microsoft.AspNetCore.SignalR.Internal.Formatters
{
    public static class BinaryMessageFormatter
    {
        public static void WriteMessage(ReadOnlySpan<byte> payload, Stream output)
        {
            var lenBuffer = new byte[5];
            var lenNumBytes = 0;
            for (var length = payload.Length; length > 0; lenNumBytes++)
            {
                lenBuffer[lenNumBytes] = (byte)(length & 0x7f);
                length = length >> ((lenNumBytes + 1) * 7);
                if (length > 0)
                {
                    lenBuffer[lenNumBytes] = (byte)(lenBuffer[lenNumBytes] | 0x80);
                }
            }

            var buffer = ArrayPool<byte>.Shared.Rent(lenNumBytes + payload.Length);
            var bufferSpan = buffer.AsSpan();

            lenBuffer.AsSpan().Slice(0, lenNumBytes).CopyTo(bufferSpan);
            bufferSpan = bufferSpan.Slice(lenNumBytes);
            payload.CopyTo(bufferSpan);
            output.Write(buffer, 0, lenNumBytes + payload.Length);

            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}