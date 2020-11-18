// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.Diagnostics.Runtime
{
    internal struct BundleFileEntry
    {
        internal BundleFileEntryData Data;
        internal string RelativePath;

        internal static BundleFileEntry? Read(IMemoryReader reader, ref ulong address) =>
            !reader.ReadAndAdvance(ref address, out BundleFileEntryData data) ? null :
                (Bundle.GetPathLength(reader, ref address, data.PathLengthByte1) switch
                {
                    null => null,
                    int pathLength => Bundle.ReadString(reader, ref address, pathLength) switch
                    {
                        null => null,
                        string path => new BundleFileEntry
                        {
                            Data = data,
                            RelativePath = path,
                        },
                    }
                });

        internal readonly struct BundleFileEntryData
        {
            internal readonly long Offset;
            internal readonly long Size;
            internal readonly BundleFileType Type;
            internal readonly sbyte PathLengthByte1;
        }
    }
}
