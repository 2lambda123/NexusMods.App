using FluentAssertions;
using NexusMods.Games.AdvancedInstaller.UI.Content.Left;
using NexusMods.Games.AdvancedInstaller.UI.Content.Right.Results.SelectLocation;
using NexusMods.Games.AdvancedInstaller.UI.Tests.Helpers;
using NexusMods.Paths;
using NSubstitute;

namespace NexusMods.Games.AdvancedInstaller.UI.Tests.ModContent;

/// <summary>
///     Tests related to the linking of mod content nodes to output directories.
/// </summary>
public class NodeLinkingTests
{
    [Fact]
    public void CanLinkFolders()
    {
        // Arrange & Act
        var (node, data, target) = CommonSetup();
        var armorsDir = node.GetNode("Textures").GetNode("Armors");
        armorsDir.Link(data, target, false);

        // Assert
        data.ArchiveToOutputMap.Count.Should().Be(3);
        data.OutputToArchiveMap.Count.Should().Be(3);
        armorsDir.GetNode("greenArmor.dds").Status.Should().Be(ModContentNodeStatus.IncludedViaParent);
        armorsDir.GetNode("greenBlade.dds").Status.Should().Be(ModContentNodeStatus.IncludedViaParent);
        armorsDir.GetNode("greenHilt.dds").Status.Should().Be(ModContentNodeStatus.IncludedViaParent);
    }

    [Fact]
    public void CanLinkFoldersRecursively()
    {
        // Arrange & Act
        var (node, data, target) = CommonSetup();
        var texturesDir = node.GetNode("Textures");
        texturesDir.Link(data, target, false);

        // Assert
        data.ArchiveToOutputMap.Count.Should().Be(6);
        data.OutputToArchiveMap.Count.Should().Be(6);
        AssertArmorsLinked(data, "Armors");

        var armorsDir = texturesDir.GetNode("Armors");
        armorsDir.GetNode("greenArmor.dds").Status.Should().Be(ModContentNodeStatus.IncludedViaParent);
        armorsDir.GetNode("greenBlade.dds").Status.Should().Be(ModContentNodeStatus.IncludedViaParent);
        armorsDir.GetNode("greenHilt.dds").Status.Should().Be(ModContentNodeStatus.IncludedViaParent);
    }

    [Fact]
    public void CanLinkFiles()
    {
        // Arrange & Act
        var (node, data, target) = CommonSetup();
        var greenArmor = node.GetNode("Textures").GetNode("Armors").GetNode("greenArmor.dds");
        greenArmor.Link(data, target, false);

        // Assert
        greenArmor.Status.Should().Be(ModContentNodeStatus.IncludedExplicit);
        data.ArchiveToOutputMap.Count.Should().Be(1);
        data.ArchiveToOutputMap["Textures/Armors/greenArmor.dds"].Should()
            .Be(new GamePath(LocationId.Game, "greenArmor.dds"));
    }

    [Fact]
    public void CanUnlinkFolders()
    {
        // Arrange & Act
        var (node, data, target) = CommonSetup();
        var armorsDir = node.GetNode("Textures").GetNode("Armors");
        armorsDir.Link(data, target, false);

        // Unlink assert that everything is empty.
        armorsDir.Unlink(data);
        AssertUnlinkedArmorsFolder(armorsDir, data);
    }

    [Fact]
    public void CanUnlinkFolders_ViaUnlinkableItem()
    {
        // Arrange & Act
        var (node, data, target) = CommonSetup();
        var armorsDir = node.GetNode("Textures").GetNode("Armors");
        armorsDir.Link(data, target, false);

        // Unlink assert that everything is empty.
        node.Unlink(data); // unlinkable
        AssertUnlinkedArmorsFolder(armorsDir, data);
    }

    [Fact]
    public void CanUnlinkFiles()
    {
        // Arrange & Act
        var (node, data, target) = CommonSetup();
        var greenArmor = node.GetNode("Textures").GetNode("Armors").GetNode("greenArmor.dds");
        greenArmor.Link(data, target, false);

        // Assert
        greenArmor.Unlink(data);
        data.ArchiveToOutputMap.Count.Should().Be(0);
        greenArmor.Status.Should().Be(ModContentNodeStatus.Default);
    }

    private (ModContentNode<int> node, DeploymentData data, IModContentBindingTarget target)
        CommonSetup()
    {
        var node = ModContentNodeTestHelpers.CreateTestTreeNode();
        var data = new DeploymentData();
        var target = new TestBindingTarget();
        return (node, data, target);
    }

    private void AssertArmorsLinked(DeploymentData data, string baseDir = "")
    {
        data.ArchiveToOutputMap["Textures/Armors/greenArmor.dds"].Should()
            .Be(new GamePath(LocationId.Game, $"{baseDir}/greenArmor.dds"));
        data.ArchiveToOutputMap["Textures/Armors/greenBlade.dds"].Should()
            .Be(new GamePath(LocationId.Game, $"{baseDir}/greenBlade.dds"));
        data.ArchiveToOutputMap["Textures/Armors/greenHilt.dds"].Should()
            .Be(new GamePath(LocationId.Game, $"{baseDir}/greenHilt.dds"));
    }

    private static void AssertUnlinkedArmorsFolder(IModContentNode armorsDir, DeploymentData data)
    {
        data.ArchiveToOutputMap.Should().BeEmpty();
        data.OutputToArchiveMap.Should().BeEmpty();
        armorsDir.Status.Should().Be(ModContentNodeStatus.Default);
        armorsDir.GetNode("greenArmor.dds").Status.Should().Be(ModContentNodeStatus.Default);
        armorsDir.GetNode("greenBlade.dds").Status.Should().Be(ModContentNodeStatus.Default);
        armorsDir.GetNode("greenHilt.dds").Status.Should().Be(ModContentNodeStatus.Default);
    }

    // Stub providing minimum functionality for the tests.
    public class TestBindingTarget : IModContentBindingTarget
    {
        public GamePath Current = new(LocationId.Game, "");

        public IModContentBindingTarget GetOrCreateChild(string name, bool isDirectory)
        {
            return new TestBindingTarget()
            {
                Current = new GamePath(LocationId.Game, Current.Path.Join(name))
            };
        }

        public GamePath Bind(IUnlinkableItem unlinkable, bool previouslyExisted) => Current;
        public string DirectoryName => Current.FileName;
    }
}
