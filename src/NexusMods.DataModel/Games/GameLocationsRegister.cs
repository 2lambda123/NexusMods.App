﻿using System.Diagnostics;
using NexusMods.Paths;

namespace NexusMods.DataModel.Games;

/// <summary>
/// Installation specific register of game locations <see cref="GameFolderType"/> and their resolved paths.
/// <remarks>
/// This class provides information about the locations, such as resolved paths, nested locations and top level parents.
/// </remarks>
/// </summary>
public class GameLocationsRegister
{
    private readonly Dictionary<GameFolderType, GameLocationDescriptor> _locations = new();

    /// <summary>
    /// Obtain the resolved path for a <see cref="GameFolderType"/>.
    /// </summary>
    /// <param name="id">The <see cref="GameFolderType"/> to lookup</param>
    public AbsolutePath this[GameFolderType id] => _locations[id].ResolvedPath;

    /// <summary>
    /// Construct a new instance of <see cref="GameLocationsRegister"/>.
    /// </summary>
    /// <remarks>
    /// Computes the nested and top level parents relationships.
    /// </remarks>
    /// <param name="newLocations">A Dictionary of <see cref="GameFolderType"/>, <see cref="AbsolutePath"/> to register</param>
    public GameLocationsRegister(IReadOnlyDictionary<GameFolderType, AbsolutePath> newLocations)
    {
        foreach (var (newId, newPath) in newLocations)
        {
            var newLocation = new GameLocationDescriptor(newId, newPath);

            var isDuplicate = !_locations.TryAdd(newId, newLocation);

            Debug.Assert(isDuplicate == false,
                $"Duplicate location found for {newId} at {_locations[newId].ResolvedPath}: {newPath}");

            // Detect Locations that are relative to each other and update nested locations and top level parent
            foreach (var existingId in _locations.Keys)
            {
                if (existingId == newId) continue;

                // Check if new location is nested to the existing location
                if (this[existingId].GetFullPathLength() < newPath.GetFullPathLength())
                {
                    if (!newPath.InFolder(this[existingId])) continue;

                    _locations[existingId].AddNestedLocation(newLocation);
                    newLocation.SetTopLevelParent(ComputeTopLevelParent(newLocation, _locations[existingId]));
                }
                else
                {
                    // Check if existing location is nested to the new location
                    if (!this[existingId].InFolder(newPath)) continue;

                    newLocation.AddNestedLocation(_locations[existingId]);
                    _locations[existingId]
                        .SetTopLevelParent(ComputeTopLevelParent(_locations[existingId], newLocation));
                }
            }
        }
    }

    private GameFolderType ComputeTopLevelParent(GameLocationDescriptor child, GameLocationDescriptor newParent)
    {
        if (child.TopLevelParent == null)
        {
            return newParent.TopLevelParent ?? newParent.Id;
        }

        var previousToLevelParent = _locations[child.TopLevelParent.Value];
        var newParentTopParent = _locations[newParent.TopLevelParent ?? newParent.Id];

        // If the new parent path is shorter it means that it is the new top level parent
        return newParentTopParent.ResolvedPath.GetFullPathLength() <
               previousToLevelParent.ResolvedPath.GetFullPathLength()
            ? newParentTopParent.Id
            : child.TopLevelParent.Value;
    }

    /// <summary>
    /// Dictionary of <see cref="GameFolderType"/> and <see cref="GameLocationDescriptor"/>s.
    /// </summary>
    public IReadOnlyDictionary<GameFolderType, GameLocationDescriptor> LocationDescriptors => _locations;

    /// <summary>
    /// True if the <see cref="GameFolderType"/> is a top level location, as no other location contains it.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool IsTopLevel(GameFolderType id)
    {
        return _locations[id].IsTopLevel;
    }

    /// <summary>
    /// Obtain the top level parent of a <see cref="GameFolderType"/>.
    /// </summary>
    /// <remarks>
    /// If the <see cref="GameFolderType"/> is a top level location, it will return itself.
    /// If there are two top level parents with the same path, the first one in the order they were declared will be returned.
    /// </remarks>
    /// <param name="id"></param>
    /// <returns></returns>
    public GameFolderType GetTopLevelParent(GameFolderType id)
    {
        return _locations[id].TopLevelParent ?? id;
    }

    /// <summary>
    /// Returns the associated <see cref="AbsolutePath"/> for the current game installation.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public AbsolutePath GetResolvedPath(GameFolderType id)
    {
        return this[id];
    }

    /// <summary>
    /// Translates the <see cref="GamePath"/> to a resolved <see cref="AbsolutePath"/> for the current game installation.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public AbsolutePath GetResolvedPath(GamePath path)
    {
        return _locations[path.Type].ResolvedPath.Combine(path.Path);
    }

    /// <summary>
    /// Returns a collection of <see cref="GameFolderType"/>s that are nested directories of the passed location.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public IReadOnlyCollection<GameFolderType> GetNestedLocations(GameFolderType id)
    {
        return _locations[id].NestedLocations.ToArray();
    }

    /// <summary>
    /// Maps the <see cref="AbsolutePath"/> to a <see cref="GamePath"/> for the current game installation.
    /// </summary>
    /// <remarks>
    /// In case of multiple nested locations containing the path,
    /// this method will return a <see cref="GamePath"/> for the "closest" <see cref="GameFolderType"/> to the file.
    /// E.g. if "Data" location is nested to "Game", and the path is "Game/Data/foo.bar", the GamePath will be relative to "Data".
    /// </remarks>
    /// <param name="absolutePath"></param>
    /// <returns></returns>
    public GamePath ToGamePath(AbsolutePath absolutePath)
    {
        return _locations.Values.Where(location => absolutePath.StartsWith(location.ResolvedPath))
            .Select(desc => new GamePath(desc.Id, absolutePath.RelativeTo(desc.ResolvedPath)))
            .MinBy(gamePath => gamePath.Path.Path.Length);
    }

    /// <summary>
    /// Returns the collection of game locations that are not nested to any other,
    /// in the form of a collection of <see cref="KeyValuePair"/> of <see cref="GameFolderType"/>, <see cref="AbsolutePath"/> />
    /// </summary>
    /// <remarks>
    /// If there are two top level locations with the same path, the first one in the order they were declared will be returned.
    /// </remarks>
    /// <returns></returns>
    public IReadOnlyCollection<KeyValuePair<GameFolderType, AbsolutePath>> GetTopLevelLocations()
    {
        return LocationDescriptors
            .Where(x => x.Value.IsTopLevel)
            .GroupBy(kv => kv.Value.ResolvedPath)
            .Select(g => g.First())
            .Select(x => new KeyValuePair<GameFolderType, AbsolutePath>(x.Key, x.Value.ResolvedPath))
            .ToArray();
    }
}
