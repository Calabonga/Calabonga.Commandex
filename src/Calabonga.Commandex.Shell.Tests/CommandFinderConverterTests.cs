using Calabonga.Commandex.Engine.Base;
using Calabonga.Commandex.Shell.Engine;
using Calabonga.Commandex.Shell.Models;
using Calabonga.Commandex.Shell.Tests.Commands;
using Moq;

namespace Calabonga.Commandex.Shell.Tests;

public class CommandFinderConverterTests
{
    [Fact]
    public void CommandFinder_CanFoundCommands()
    {
        var commands = GetCommands();
        Assert.NotEmpty(commands);
    }

    [Fact]
    public void CommandFinderShould_Process_Groups()
    {
        var groups = new TestGroupBuilder().GetGroups();
        Assert.NotEmpty(groups);
    }


    [Fact]
    public void CommandFinderShould_Process_Groups_TotalCount_6()
    {
        const int expected = 6;

        var groups = new TestGroupBuilder().GetGroups();
        var actual = groups.Count;

        Assert.NotEmpty(groups);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CommandFinderShould_Process_NestedGroups_TotalCount_1()
    {
        const int expected = 1;

        var groups = new TestGroupBuilder().GetGroups();
        var actual = groups.Count(x => x.SubGroups.Count == 1);

        Assert.NotEmpty(groups);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(0, "One")]
    [InlineData(0, "Two")]
    [InlineData(0, "Three")]
    [InlineData(0, "Four")]
    [InlineData(0, "Five")]
    [InlineData(1, "Six")]
    public void CommandFinderShould_Process_GroupsWithNested_AsExpected(int expected, string groupName)
    {
        var groups = new TestGroupBuilder().GetGroups();
        var group = groups.Find(x => x.Name.StartsWith(groupName));

        Assert.NotNull(group);

        var actual = group!.SubGroups.Count;

        Assert.NotEmpty(groups);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CommandFinder_CanConvert_ToList()
    {
        const int expected = 7;

        var commands = GetCommands();
        var reader = new Mock<ISettingsReaderConfiguration>();
        var items = CommandFinder.ConvertToItems(commands, reader.Object, string.Empty);
        var actual = items.Count;

        Assert.NotEmpty(items);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CommandFinder_CanConvert_ToHierarchy_WithTotalItems_5()
    {
        const int expected = 7;

        var commands = GetCommands();
        var reader = new Mock<ISettingsReaderConfiguration>();
        var items = CommandFinder.ConvertToGroupedItems(new TestGroupBuilder(), commands, reader.Object, string.Empty).ToList();
        var actual = items.Count;

        Assert.NotEmpty(items);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CommandFinder_CanConvert_ToHierarchy_WithUntagged_Equals_1()
    {
        const int expected = 1;

        var commands = GetCommands();
        var reader = new Mock<ISettingsReaderConfiguration>();
        var items = CommandFinder.ConvertToGroupedItems(new TestGroupBuilder(), commands, reader.Object, string.Empty).ToList();
        var actual = items.Count(x => x.Name.StartsWith("Untagged"));

        Assert.NotEmpty(items);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(3, "One")]
    [InlineData(2, "Two")]
    [InlineData(1, "Three")]
    [InlineData(4, "Four")]
    [InlineData(0, "Five")]
    public void CommandFinderShould_PutInGroup_CommandsTagged_ItemsCount(int expected, string groupName)
    {
        var commands = GetCommands();
        var reader = new Mock<ISettingsReaderConfiguration>();
        var items = CommandFinder.ConvertToGroupedItems(new TestGroupBuilder(), commands, reader.Object, string.Empty).ToList();
        var actual = items.Where(x => x.Name.StartsWith(groupName)).SelectMany(x => x.Items).Count();

        Assert.NotEmpty(items);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(1, "Three")]
    // [InlineData(3, "One")]
    // [InlineData(2, "Two")]
    // [InlineData(0, "Four")]
    // [InlineData(0, "Five")]
    public void CommandFinderShould_PutInGroup_CommandsTagged_Items_WithCorrectTagNames(int expected, string groupName)
    {
        var commands = GetCommands();
        var reader = new Mock<ISettingsReaderConfiguration>();
        var items = CommandFinder.ConvertToGroupedItems(new TestGroupBuilder(), commands, reader.Object, string.Empty).ToList();
        var actual = items.Where(x => x.Name.StartsWith(groupName)).SelectMany(x => x.Items).Count(x => x.Tags!.Contains(groupName.ToLower()));

        Assert.NotEmpty(items);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("One", "one", 3)]
    [InlineData("One", "two", 1)]
    [InlineData("Two", "one", 1)]
    [InlineData("Two", "two", 2)]
    [InlineData("Three", "three", 1)]
    [InlineData("Four", "three", 1)]
    [InlineData("Four", "one", 3)]
    public void CommandFinderShould_PutInGroup_CommandsTag_Equals_Expected(string groupName, string tagName, int expected)
    {
        var commands = GetCommands();
        var reader = new Mock<ISettingsReaderConfiguration>();
        var items = CommandFinder.ConvertToGroupedItems(new TestGroupBuilder(), commands, reader.Object, string.Empty).ToList();
        var actual = items.Where(x => x.Name.StartsWith(groupName)).SelectMany(x => x.Items).Count(x => x.Tags!.Contains(tagName));

        Assert.NotEmpty(items);
        Assert.Equal(expected, actual);
    }










    [Fact]
    public void CommandFinder_ConvertToGroupedItems_KeepsThirdLevelUnderItsParent()
    {
        var commands = GetCommands();
        var reader = new Mock<ISettingsReaderConfiguration>();

        var items = CommandFinder
            .ConvertToGroupedItems(new ThreeLevelGroupBuilder(), commands, reader.Object, string.Empty)
            .ToList();

        var level1 = Assert.Single(items, x => x.Name.StartsWith("L1"));
        var level2 = Assert.Single(level1.Items, x => x.Name.StartsWith("L2"));
        var level3 = Assert.Single(level2.Items, x => x.Name.StartsWith("L3"));

        Assert.NotNull(level3);
        // before the fix the third level was wrongly attached to the first-level group
        Assert.DoesNotContain(level1.Items, x => x.Name.StartsWith("L3"));
    }

    private sealed class ThreeLevelGroupBuilder : IGroupBuilder
    {
        public CommandGroup GetDefault()
            => new() { Name = "Untagged", Description = "Group for untagged commandex command.", Tags = [] };

        public List<CommandGroup> GetGroups() =>
        [
            new()
            {
                Name = "L1", Tags = ["l1"], Description = "Description",
                SubGroups =
                [
                    new()
                    {
                        Name = "L2", Tags = ["l2"], Description = "Description",
                        SubGroups = [new() { Name = "L3", Tags = ["l3"], Description = "Description" }]
                    }
                ]
            }
        ];
    }

    private IEnumerable<ICommandexCommand> GetCommands()
        => new List<ICommandexCommand>
        {
            new FakeCommandexCommand4(),
            new FakeCommandexCommand1(),
            new FakeCommandexCommand5(),
            new FakeCommandexCommand6(),
            new FakeCommandexCommand2(),
            new FakeCommandexCommand3(),
            new FakeCommandexCommand7(),
        };
}