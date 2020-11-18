// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.Diagnostics.Runtime
{
    internal struct BundleManifest
    {
        internal BundleFileEntry[] Files;

        internal static BundleManifest? Read(IMemoryReader reader, ref ulong address, int filesCount)
        {
            BundleFileEntry[] files = new BundleFileEntry[filesCount];

            for (int i = 0; i < filesCount; i++)
            {
                switch (BundleFileEntry.Read(reader, ref address))
                {
                    case null:
                        return null;

                    case BundleFileEntry entry:
                        files[i] = entry;
                        break;
                }
            }

            return new BundleManifest
            {
                Files = files,
            };
        }
    }
}
