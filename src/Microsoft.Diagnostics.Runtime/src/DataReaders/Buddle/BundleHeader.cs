// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.Diagnostics.Runtime
{
    internal readonly struct BundleHeader
    {
        internal readonly uint MajorVersion;
        internal readonly uint MinorVersion;
        internal readonly int EmbeddedFilesCount;
        internal readonly sbyte BundleIdLengthByte1;

        internal unsafe static BundleHeader? Read(IMemoryReader reader, ref ulong address) =>
            !reader.ReadAndAdvance(ref address, out BundleHeader header) ? null :
                (Bundle.GetPathLength(reader, ref address, header.BundleIdLengthByte1) switch
                {
                    null => null,
                    int bundleIdLength => Bundle.ReadString(reader, ref address, bundleIdLength) switch
                    {
                        null => null,
                        string => header,
                    }
                });
    }
}
