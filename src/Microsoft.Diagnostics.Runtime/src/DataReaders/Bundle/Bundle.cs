// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Diagnostics.Runtime
{
    internal static class Bundle
    {
        internal static int ReadAndAdvance(this IMemoryReader reader, ref ulong address, Span<byte> buffer)
        {
            int result = reader.Read(address, buffer);
            address += (uint)buffer.Length;
            return result;
        }

        internal static unsafe bool ReadAndAdvance<T>(this IMemoryReader reader, ref ulong address, out T value) where T : unmanaged
        {
            bool result = reader.Read(address, out value);
            address += (uint)sizeof(T);
            return result;
        }

        internal static IEnumerable<BundleFileEntry> EnumerateFiles(IMemoryReader reader, ulong address, int length) =>
            (address = FindHeader(reader, address, length)) == 0 ? Enumerable.Empty<BundleFileEntry>() :
                (BundleHeader.Read(reader, ref address) switch
                {
                    null => Enumerable.Empty<BundleFileEntry>(),
                    BundleHeader header => BundleManifest.Read(reader, ref address, header.EmbeddedFilesCount) switch
                    {
                        null => Enumerable.Empty<BundleFileEntry>(),
                        BundleManifest manifest => manifest.Files,
                    },
                });

        internal static ulong FindHeader(IMemoryReader reader, ulong address, int length)
        {
            // SHA-256 for ".net core bundle"
            byte[] signature =
            {
                0x8b, 0x12, 0x02, 0xb9, 0x6a, 0x61, 0x20, 0x38,
                0x72, 0x7b, 0x93, 0x02, 0x14, 0xd7, 0xa0, 0x32,
                0x13, 0xf5, 0xb9, 0xe6, 0xef, 0xae, 0x33, 0x18,
                0xee, 0x3b, 0x2d, 0xce, 0x24, 0xb3, 0x6a, 0xae,
            };

            address = reader.SearchMemory(address, length, signature);
            //return address == 0 ? 0 : !reader.ReadPointer(address - sizeof(ulong), out ulong header) ? 0 : header;
            var r = address == 0 ? 0 : !reader.ReadPointer(address - sizeof(ulong), out ulong header) ? 0 : header;
            Console.WriteLine(r);
            return r;
        }

        internal static int? GetPathLength(IMemoryReader reader, ref ulong address, sbyte firstByte)
        {
            int length;

            if ((firstByte & 0x80) == 0)
            {
                length = firstByte;
            }
            else
            {
                if (!reader.ReadAndAdvance(ref address, out sbyte secondByte))
                {
                    return null;
                }

                if ((secondByte & 0x80) == 0)
                {
                    return null;
                }

                length = (secondByte << 7) | (firstByte & 0x7f);
            }

            return length;

        }

        internal static string? ReadString(IMemoryReader reader, ref ulong address, int length)
        {
            byte[] buffer = ArrayPool<byte>.Shared.Rent(length);
            try
            {
                return reader.ReadAndAdvance(ref address, buffer) != length ? null : Encoding.UTF8.GetString(buffer);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}
