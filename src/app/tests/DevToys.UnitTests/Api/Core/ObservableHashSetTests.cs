using System.Collections.Specialized;
using System.ComponentModel;

namespace DevToys.UnitTests.Api.Core;

public class ObservableHashSetTests
{
    private static readonly Random random = new();

    [Fact]
    public void CanConstruct()
    {
        new ObservableHashSet<int>().Comparer.Should().BeSameAs(new HashSet<int>().Comparer);

        new ObservableHashSet<object>(ReferenceEqualityComparer.Instance).Comparer.Should().BeSameAs(ReferenceEqualityComparer.Instance);

        List<int> testData1 = CreateTestData();

        var rh1 = new HashSet<int>(testData1);
        var ohs1 = new ObservableHashSet<int>(testData1);
        ohs1.OrderBy(i => i).Should().BeEquivalentTo(rh1.OrderBy(i => i));
        ohs1.Comparer.Should().BeSameAs(rh1.Comparer);

        IEnumerable<object> testData2 = CreateTestData().Cast<object>();

        var rh2 = new HashSet<object>(testData2, ReferenceEqualityComparer.Instance);
        var ohs2 = new ObservableHashSet<object>(testData2, ReferenceEqualityComparer.Instance);
        ohs2.OrderBy(i => i).Should().BeEquivalentTo(rh2.OrderBy(i => i));
        ohs2.Comparer.Should().BeSameAs(rh2.Comparer);
    }

    [Fact]
    public void CanAdd()
    {
        var hashSet = new ObservableHashSet<string>();
        int countChanging = 0;
        int countChanged = 0;
        int collectionChanged = 0;
        int currentCount = 0;
        int countChange = 1;
        string[] adding = [];

        hashSet.PropertyChanging += (s, a) => AssertCountChanging(hashSet, s, a, currentCount, ref countChanging);
        hashSet.PropertyChanged += (s, a) => AssertCountChanged(hashSet, s, a, ref currentCount, countChange, ref countChanged);
        hashSet.CollectionChanged += (s, a) =>
        {
            a.Action.Should().Be(NotifyCollectionChangedAction.Add);
            a.OldItems.Should().BeNull();
            a.NewItems.OfType<string>().Should().BeEquivalentTo(adding);
            collectionChanged++;
        };

        adding = ["Palmer"];
        hashSet.Add("Palmer").Should().BeTrue();

        countChanging.Should().Be(1);
        countChanged.Should().Be(1);
        collectionChanged.Should().Be(1);
        hashSet.Should().BeEquivalentTo(["Palmer"]);

        adding = ["Carmack"];
        hashSet.Add("Carmack").Should().BeTrue();

        countChanging.Should().Be(2);
        countChanged.Should().Be(2);
        collectionChanged.Should().Be(2);
        hashSet.OrderBy(i => i).Should().BeEquivalentTo(["Carmack", "Palmer"]);

        hashSet.Add("Palmer").Should().BeFalse();

        countChanging.Should().Be(2);
        countChanged.Should().Be(2);
        collectionChanged.Should().Be(2);
        hashSet.OrderBy(i => i).Should().BeEquivalentTo(["Carmack", "Palmer"]);
    }

    [Fact]
    public void CanClear()
    {
        var testData = new HashSet<int>(CreateTestData());

        var hashSet = new ObservableHashSet<int>(testData);
        int countChanging = 0;
        int countChanged = 0;
        int collectionChanged = 0;
        int currentCount = testData.Count;
        int countChange = -testData.Count;

        hashSet.PropertyChanging += (s, a) => AssertCountChanging(hashSet, s, a, currentCount, ref countChanging);
        hashSet.PropertyChanged += (s, a) => AssertCountChanged(hashSet, s, a, ref currentCount, countChange, ref countChanged);
        hashSet.CollectionChanged += (s, a) =>
        {
            a.Action.Should().Be(NotifyCollectionChangedAction.Replace);
            a.OldItems.OfType<int>().OrderBy(i => i).Should().BeEquivalentTo(testData.OrderBy(i => i));
            a.NewItems.Count.Should().Be(0);
            collectionChanged++;
        };

        hashSet.Clear();

        countChanging.Should().Be(testData.Count == 0 ? 0 : 1);
        countChanged.Should().Be(testData.Count == 0 ? 0 : 1);
        collectionChanged.Should().Be(testData.Count == 0 ? 0 : 1);
        hashSet.Should().BeEmpty();

        hashSet.Clear();

        countChanging.Should().Be(testData.Count == 0 ? 0 : 1);
        countChanged.Should().Be(testData.Count == 0 ? 0 : 1);
        collectionChanged.Should().Be(testData.Count == 0 ? 0 : 1);
        hashSet.Should().BeEmpty();
    }

    [Fact]
    public void ContainsWorks()
    {
        List<int> testData = CreateTestData();
        var hashSet = new ObservableHashSet<int>(testData);

        foreach (int item in testData)
        {
            hashSet.Should().Contain(item);
        }

        foreach (int item in CreateTestData(1000, 10000).Except(testData))
        {
            hashSet.Should().NotContain(item);
        }
    }

    [Fact]
    public void CanCopyToArray()
    {
        List<int> testData = CreateTestData();
        var orderedDistinct = testData.Distinct().OrderBy(i => i).ToList();

        var hashSet = new ObservableHashSet<int>(testData);

        hashSet.Count.Should().Be(orderedDistinct.Count);

        int[] array = new int[hashSet.Count];
        hashSet.CopyTo(array);

        array.OrderBy(i => i).Should().BeEquivalentTo(orderedDistinct);

        array = new int[hashSet.Count + 100];
        hashSet.CopyTo(array, 100);

        array.Skip(100).OrderBy(i => i).Should().BeEquivalentTo(orderedDistinct);

        int toTake = Math.Min(10, hashSet.Count);
        array = new int[100 + toTake];
        hashSet.CopyTo(array, 100, toTake);

        foreach (int value in array.Skip(100).Take(toTake))
        {
            hashSet.Should().Contain(value);
        }
    }

    [Fact]
    public void CanRemove()
    {
        var hashSet = new ObservableHashSet<string> { "Palmer", "Carmack" };
        int countChanging = 0;
        int countChanged = 0;
        int collectionChanged = 0;
        int currentCount = 2;
        int countChange = -1;
        string[] removing = [];

        hashSet.PropertyChanging += (s, a) => AssertCountChanging(hashSet, s, a, currentCount, ref countChanging);
        hashSet.PropertyChanged += (s, a) => AssertCountChanged(hashSet, s, a, ref currentCount, countChange, ref countChanged);
        hashSet.CollectionChanged += (s, a) =>
        {
            a.Action.Should().Be(NotifyCollectionChangedAction.Remove);
            a.OldItems.OfType<string>().Should().BeEquivalentTo(removing);
            a.NewItems.Should().BeNull();
            collectionChanged++;
        };

        removing = ["Palmer"];
        hashSet.Remove("Palmer").Should().BeTrue();

        countChanging.Should().Be(1);
        countChanged.Should().Be(1);
        collectionChanged.Should().Be(1);
        hashSet.Should().BeEquivalentTo(["Carmack"]);

        removing = ["Carmack"];
        hashSet.Remove("Carmack").Should().BeTrue();

        countChanging.Should().Be(2);
        countChanged.Should().Be(2);
        collectionChanged.Should().Be(2);
        hashSet.Should().BeEmpty();

        hashSet.Remove("Palmer").Should().BeFalse();

        countChanging.Should().Be(2);
        countChanged.Should().Be(2);
        collectionChanged.Should().Be(2);
        hashSet.Should().BeEmpty();
    }

    [Fact]
    public void NotReadOnly()
        => Assert.False(new ObservableHashSet<Random>().IsReadOnly);

    [Fact]
    public void CanUnionWith()
    {
        var hashSet = new ObservableHashSet<string> { "Palmer", "Carmack" };
        int countChanging = 0;
        int countChanged = 0;
        int collectionChanged = 0;
        int currentCount = 2;
        int countChange = 2;
        string[] adding = ["Brendan", "Nate"];

        hashSet.PropertyChanging += (s, a) => AssertCountChanging(hashSet, s, a, currentCount, ref countChanging);
        hashSet.PropertyChanged += (s, a) => AssertCountChanged(hashSet, s, a, ref currentCount, countChange, ref countChanged);
        hashSet.CollectionChanged += (s, a) =>
        {
            a.Action.Should().Be(NotifyCollectionChangedAction.Replace);
            a.OldItems.Count.Should().Be(0);
            a.NewItems.OfType<string>().OrderBy(i => i).Should().BeEquivalentTo(adding);
            collectionChanged++;
        };

        hashSet.UnionWith(new[] { "Carmack", "Nate", "Brendan" });

        countChanging.Should().Be(1);
        countChanged.Should().Be(1);
        collectionChanged.Should().Be(1);
        hashSet.OrderBy(i => i).Should().BeEquivalentTo(["Brendan", "Carmack", "Nate", "Palmer"]);

        hashSet.UnionWith(new[] { "Brendan" });

        countChanging.Should().Be(1);
        countChanged.Should().Be(1);
        collectionChanged.Should().Be(1);
        hashSet.OrderBy(i => i).Should().BeEquivalentTo(["Brendan", "Carmack", "Nate", "Palmer"]);
    }

    [Fact]
    public void CanIntersectWith()
    {
        var hashSet = new ObservableHashSet<string>
    {
        "Brendan",
        "Carmack",
        "Nate",
        "Palmer"
    };
        int countChanging = 0;
        int countChanged = 0;
        int collectionChanged = 0;
        int currentCount = 4;
        int countChange = -2;
        string[] removing = ["Brendan", "Nate"];

        hashSet.PropertyChanging += (s, a) => AssertCountChanging(hashSet, s, a, currentCount, ref countChanging);
        hashSet.PropertyChanged += (s, a) => AssertCountChanged(hashSet, s, a, ref currentCount, countChange, ref countChanged);
        hashSet.CollectionChanged += (s, a) =>
        {
            a.Action.Should().Be(NotifyCollectionChangedAction.Replace);
            a.OldItems.OfType<string>().OrderBy(i => i).Should().BeEquivalentTo(removing);
            a.NewItems.Count.Should().Be(0);
            collectionChanged++;
        };

        hashSet.IntersectWith(new[] { "Carmack", "Palmer", "Abrash" });

        countChanging.Should().Be(1);
        countChanged.Should().Be(1);
        collectionChanged.Should().Be(1);
        hashSet.OrderBy(i => i).Should().BeEquivalentTo(["Carmack", "Palmer"]);

        hashSet.IntersectWith(new[] { "Carmack", "Palmer", "Abrash" });

        countChanging.Should().Be(1);
        countChanged.Should().Be(1);
        collectionChanged.Should().Be(1);
        hashSet.OrderBy(i => i).Should().BeEquivalentTo(["Carmack", "Palmer"]);
    }

    [Fact]
    public void CanExceptWith()
    {
        var hashSet = new ObservableHashSet<string>
    {
        "Brendan",
        "Carmack",
        "Nate",
        "Palmer"
    };
        int countChanging = 0;
        int countChanged = 0;
        int collectionChanged = 0;
        int currentCount = 4;
        int countChange = -2;
        string[] removing = ["Carmack", "Palmer"];

        hashSet.PropertyChanging += (s, a) => AssertCountChanging(hashSet, s, a, currentCount, ref countChanging);
        hashSet.PropertyChanged += (s, a) => AssertCountChanged(hashSet, s, a, ref currentCount, countChange, ref countChanged);
        hashSet.CollectionChanged += (s, a) =>
        {
            a.Action.Should().Be(NotifyCollectionChangedAction.Replace);
            a.OldItems.OfType<string>().OrderBy(i => i).Should().BeEquivalentTo(removing);
            a.NewItems.Count.Should().Be(0);
            collectionChanged++;
        };

        hashSet.ExceptWith(new[] { "Carmack", "Palmer", "Abrash" });

        countChanging.Should().Be(1);
        countChanged.Should().Be(1);
        collectionChanged.Should().Be(1);
        hashSet.OrderBy(i => i).Should().BeEquivalentTo(["Brendan", "Nate"]);

        hashSet.ExceptWith(new[] { "Abrash", "Carmack", "Palmer" });

        countChanging.Should().Be(1);
        countChanged.Should().Be(1);
        collectionChanged.Should().Be(1);
        hashSet.OrderBy(i => i).Should().BeEquivalentTo(["Brendan", "Nate"]);
    }

    [Fact]
    public void CanSymmetricalExceptWith()
    {
        var hashSet = new ObservableHashSet<string>
    {
        "Brendan",
        "Carmack",
        "Nate",
        "Palmer"
    };
        int countChanging = 0;
        int countChanged = 0;
        int collectionChanged = 0;
        int currentCount = 4;
        int countChange = -1;
        string[] removing = ["Carmack", "Palmer"];
        string[] adding = ["Abrash"];

        hashSet.PropertyChanging += (s, a) => AssertCountChanging(hashSet, s, a, currentCount, ref countChanging);
        hashSet.PropertyChanged += (s, a) => AssertCountChanged(hashSet, s, a, ref currentCount, countChange, ref countChanged);
        hashSet.CollectionChanged += (s, a) =>
        {
            a.Action.Should().Be(NotifyCollectionChangedAction.Replace);
            a.OldItems.OfType<string>().OrderBy(i => i).Should().BeEquivalentTo(removing);
            a.NewItems.OfType<string>().OrderBy(i => i).Should().BeEquivalentTo(adding);
            collectionChanged++;
        };

        hashSet.SymmetricExceptWith(new[] { "Carmack", "Palmer", "Abrash" });

        countChanging.Should().Be(1);
        countChanged.Should().Be(1);
        collectionChanged.Should().Be(1);
        hashSet.OrderBy(i => i).Should().BeEquivalentTo(["Abrash", "Brendan", "Nate"]);

        hashSet.SymmetricExceptWith(Array.Empty<string>());

        countChanging.Should().Be(1);
        countChanged.Should().Be(1);
        collectionChanged.Should().Be(1);
        hashSet.OrderBy(i => i).Should().BeEquivalentTo(["Abrash", "Brendan", "Nate"]);
    }

    [Fact]
    public void IsSubsetOfWorksLikeNormalHashSet()
    {
        List<int> bigData = CreateTestData();
        List<int> smallData = CreateTestData(10);

        new ObservableHashSet<int>(smallData).IsSubsetOf(bigData).Should().Be(new HashSet<int>(smallData).IsSubsetOf(bigData));
    }

    [Fact]
    public void IsProperSubsetOfWorksLikeNormalHashSet()
    {
        List<int> bigData = CreateTestData();
        List<int> smallData = CreateTestData(10);

        new ObservableHashSet<int>(smallData).IsProperSubsetOf(bigData).Should().Be(new HashSet<int>(smallData).IsProperSubsetOf(bigData));
    }

    [Fact]
    public void IsSupersetOfWorksLikeNormalHashSet()
    {
        List<int> bigData = CreateTestData();
        List<int> smallData = CreateTestData(10);

        new ObservableHashSet<int>(bigData).IsSupersetOf(smallData).Should().Be(new HashSet<int>(bigData).IsSupersetOf(smallData));
    }

    [Fact]
    public void IsProperSupersetOfWorksLikeNormalHashSet()
    {
        List<int> bigData = CreateTestData();
        List<int> smallData = CreateTestData(10);

        new ObservableHashSet<int>(bigData).IsProperSupersetOf(smallData).Should().Be(new HashSet<int>(bigData).IsProperSupersetOf(smallData));
    }

    [Fact]
    public void OverlapsWorksLikeNormalHashSet()
    {
        List<int> bigData = CreateTestData();
        List<int> smallData = CreateTestData(10);

        new ObservableHashSet<int>(bigData).Overlaps(smallData).Should().Be(new HashSet<int>(bigData).Overlaps(smallData));
    }

    [Fact]
    public void SetEqualsWorksLikeNormalHashSet()
    {
        List<int> data1 = CreateTestData(5);
        List<int> data2 = CreateTestData(5);

        new ObservableHashSet<int>(data1).SetEquals(data2).Should().Be(new HashSet<int>(data1).SetEquals(data2));
    }

    [Fact]
    public void TrimExcessDoesntThrow()
    {
        List<int> bigData = CreateTestData();
        List<int> smallData = CreateTestData(10);

        var hashSet = new ObservableHashSet<int>(bigData.Concat(smallData));
        foreach (int item in bigData)
        {
            hashSet.Remove(item);
        }

        hashSet.Invoking(h => h.TrimExcess()).Should().NotThrow();
    }

    [Fact]
    public void CanRemoveWithPredicate()
    {
        var hashSet = new ObservableHashSet<string>
        {
            "Brendan",
            "Carmack",
            "Nate",
            "Palmer"
        };
        int countChanging = 0;
        int countChanged = 0;
        int collectionChanged = 0;
        int currentCount = 4;
        int countChange = -2;
        string[] removing = ["Carmack", "Palmer"];

        hashSet.PropertyChanging += (s, a) => AssertCountChanging(hashSet, s, a, currentCount, ref countChanging);
        hashSet.PropertyChanged += (s, a) => AssertCountChanged(hashSet, s, a, ref currentCount, countChange, ref countChanged);
        hashSet.CollectionChanged += (s, a) =>
        {
            a.Action.Should().Be(NotifyCollectionChangedAction.Replace);
            a.OldItems.OfType<string>().OrderBy(i => i).Should().BeEquivalentTo(removing);
            a.NewItems.Count.Should().Be(0);
            collectionChanged++;
        };

        hashSet.RemoveWhere(i => i.Contains('m')).Should().Be(2);

        countChanging.Should().Be(1);
        countChanged.Should().Be(1);
        collectionChanged.Should().Be(1);
        hashSet.OrderBy(i => i).Should().BeEquivalentTo(["Brendan", "Nate"]);

        hashSet.RemoveWhere(i => i.Contains('m')).Should().Be(0);

        countChanging.Should().Be(1);
        countChanged.Should().Be(1);
        collectionChanged.Should().Be(1);
        hashSet.OrderBy(i => i).Should().BeEquivalentTo(["Brendan", "Nate"]);
    }

    private static void AssertCountChanging<T>(
    ObservableHashSet<T> hashSet,
    object sender,
    PropertyChangingEventArgs eventArgs,
    int expectedCount,
    ref int changingCount)
    {
        sender.Should().Be(hashSet);
        eventArgs.PropertyName.Should().Be("Count");
        hashSet.Count.Should().Be(expectedCount);
        changingCount++;
    }

    private static void AssertCountChanged<T>(
        ObservableHashSet<T> hashSet,
        object sender,
        PropertyChangedEventArgs eventArgs,
        ref int expectedCount,
        int countDelta,
        ref int changedCount)
    {
        sender.Should().Be(hashSet);
        eventArgs.PropertyName.Should().Be("Count");
        hashSet.Count.Should().Be(expectedCount + countDelta);
        changedCount++;
        expectedCount += countDelta;
    }

    private static List<int> CreateTestData(int minSize = 0, int maxLength = 1000)
    {
        int length = random.Next(minSize, maxLength);
        var data = new List<int>();
        for (int i = 0; i < length; i++)
        {
            data.Add(random.Next(int.MinValue, int.MaxValue));
        }

        return data;
    }
}
