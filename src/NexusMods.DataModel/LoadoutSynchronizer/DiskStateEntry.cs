﻿using NexusMods.Hashing.xxHash64;
using NexusMods.Paths;

namespace NexusMods.DataModel.LoadoutSynchronizer;

/// <summary>
/// Metadata about a file on disk.
/// </summary>
public readonly struct DiskStateEntry
{
    /// <summary>
    /// The hash of the file.
    /// </summary>
    public required Hash Hash { get; init; }

    /// <summary>
    /// The size of the file.
    /// </summary>
    public required Size Size { get; init; }

    /// <summary>
    /// The last modified time of the file.
    /// </summary>
    public required DateTime LastModified { get; init; }

}
