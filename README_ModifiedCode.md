# ai-sample
Analysis in this document will be done on modified code base found here. 

## Overall Recommendation
The answer still remains *it depends*. We will need to look at the breakdown of the classes to determine what our requirements are.

## Threading Consideration
Since most of these operations are read operations I have foregone the use of thread safety. We can't guarantee that the underlying enumeration will be stable. This will affect the `EntryLookupList` the most as it is operating directly on the collection. The other classes make use of their own data structure.

## Collection Copy Consideration
In order to reduce the amount of memory overhead for the `EntryLookupList` class and the `EntryLookupDictionary` I have removed the initial copy of the collection on instantiation. This reduces the memory overhead by O(n) for these classes.

## Allocation Consideration
Before modifying these classes we would incur a number of allocation for temporary items. Some of these allocations would be in the order of O(n) in the case of the `EntryLookupDictionary` class.

## Creation and UsageConsideration
Both the `EntryLookupDictionary` and `EntryLookupBinarySearch` have a cost associated with the first use of the `GetEntryByKey()` method. Once this cost is incurred the time to search will approach the expected time of the respective algorithms. It is not recommended to create these classes multiple times but instead create once and use multiple times.

## Fast Search Requirement with Unique Keys
If the requirement is a fast search and the keys are going to be unique we should consider the `EntryLookupDictionary`. This class has the benefit of being near constant time for direct key lookups. This class does cause memory increase due to creating a `Dictionary<>` object. Total memory usage would be O(2n).

## Fast Search Requirement with Duplicates/Nulls
If the requirement is a fast search and the data could be duplicated `EntryLookupBinarySearch` would be an ideal class. The search time is O(log n). The unfortunate drawback here is the memory footprint. This class will have O(2n + log n + i), where i is the creation of the reusable `IItemWithKey<TKey>`. Even though the BinarySearch would be considered stable due to the sorting; we still have an issue with duplicates that could possibly return different results. This class should be used only when we are not concerned with the fact that duplicated data would be considered unstable.

## Low Memory Usage with Duplicates/Nulls
If the requirement is simply low memory and best effort search we should use the `EntryLookupList`. This class will be the fastest to stand up and the lowest memory usage. We also cannot guarantee we will return the correct item as the algorithm will simply stop once it gets the first item. This class can be created and used over and over again. 


