# ai-sample
Analysis in this document will be done on existing code base in the [AlexBulankou/ai-samples](https://github.com/AlexBulankou/ai-samples/blob/master/SortingHat1.cs) repository. 

## Overall Recommendation
None, of course, as all of them are error prone and do not properly implement the feature they suggest. A less rigid answer would be *it depends*. If we were to go off the name alone it would seem that the Dictionary is the best way to go for seek times. However, we can't guarantee the consistency of the data; a simple conjecture would be that there could be nulls and/or duplicates in the original data set.

## EntryLookupList Analysis
- General Observations
  - Allows more than 1 item with a specific Key
  - Will return the first item it finds in the list with the Key; other items will not be found
- `GetEntryByKey()` is error prone
  - No exception handling
- Memory Usage
  - O(2n) Memory usage due to copy operation Original [O(n)] + Copy [O(n)]
- Performance
  - O(n) Constructor time
  - O(n) Search time

## EntryLookupList Summary
If you need to find the first item in the list, don't need to guarantee consistency, and need a low memory footprint. This class will do just that. Considering that it's linear time it's performance will suffer as you add more items. An added benefit of this class is you can add a number of duplicate items. Unfortunately, we still need to consider the cases where nulls would be added as it can cause issues with the `First()` method. As well, we are also creating a copy of the incoming `IEnumerable<>`. This class would 

## EntryLookupList Recommendations
If the data set is immutable or considered stable we could skip the copy operation in the constructor and save allocations as well as time to create the class. This would make the class O(n) for memory and O(n) for search. We would need to handle cases where the input class could change and break the usage of `First()`, as well we'd need to handle cases where `TKey` is null or the `IItemWithKey<TKey>.Key` is null.

## EntryLookupBinarySearch Analysis
- General Observations
  - Allows more than 1 item with a specific Key
  - Will any matching item it finds in the list with the Key; no guarantee of which one is found (unstable)
- `GetEntryByKey()` is error prone
    - Needs logic to check index out of range
    - No exception handling
- `GetEntryByKey()` causes an allocation
    - O(ki) Memory usage due to new TItem allocation (i) at an undetermined value (k)
    - This could end up being O(n) if total number of items searched is the same number of items
    - Should use Clone() to ensure Key is properly compared
- `GetEntryByKey()` must sort List<>
    - BinarySearch requires a sorted list
    - BinarySearch worst search time is O(log n)
    - Sort worst space usage is O(log n) (Quicksort Sort() worst case)
    - Sort worst sort time is O(n^2) (Quicksort Sort() worst case)
    - Search is unstable and will return any item on a match with same value
- Memory Usage
  - O(2n) Memory usage due to copy operation Original [O(n)] + Copy [O(n)]
  - Quicksort worst memory usage is O(log n)
  - O(ki) Memory usage due to new TItem allocation up to O(n)
- Performance
  - O(n) Constructor time
  - O(n^2) Sort time *(not used in this implementation)*
  - O(log n) Search time

## EntryLookupBinarySearch Summary
Unfortunately this class will produce unpredictable results. As it stands this class also creates a copy of the incoming `IEnumerable<>` object. However, since this method modifies the list and requires it to stay sorted to perform the search we should keep this behavior. It's search time against a long list would be considerably faster than the `EntryLookupList` class but slower than the `EntryLookupDictionary` class. As with the List class if we are not concerned with consistent results and need to have some level of duplicates this would be a better option over the `EntryLookupDictionary`.

## EntryLookupBinarySearch Recommendations
First of all we need to sort the list for the BinarySearch to work. This operation can take some time-O(n^2) worst case-but will produce a semi-stable search depending on the data. This does make the memory usage O(2n) but searches will be O(log n) and would be ideal for inconsistent data sets. We would need to add in exception handling and override the Equality to not require the item allocation.

## EntryLookupDictionary Analysis
- General Observations
  - Does not allow more than 1 item, does not allow null
- `GetEntryByKey()` is error prone
  - No exception handling
  - Does not account for duplicate key entries
  - Does not account for nulls
- `GetEntryByKey()` 
  - O(n) lookup time due to creation of Dictionary each time
  - Will cause allocation of Dictionary<> object
  - This allocation ends up being O(n) size due to an allocation of `KeyValuePair<TKey, IItemWithKey<TKey>>` * count of `IItemWithKey<>`
  - No allocation of Keys or Values due to reference types
- `GetHashCode()` Consideration
  - Default implementation of this method is approaching O(1)
  - Poor implementation can put search time at O(n)
- Memory Usage
  - O(2n) Memory usage due to copy operation Original [O(n)] + Copy [O(n)]
  - O(n) Memory usage due to `Dictionary<>` allocation in `GetEntryByKey()`
- Performance
  - O(n) Constructor time
  - O(n + 1) Search time
  
## EntryLookupDictionary Summary
If your data set is unique the LookupDictionary will perform the best of all of the classes. The creation of the class takes O(n) time due to the copy. The searching of an item approaches O(1) time (per MSDN). However, this implementation creates a `Dictionary<>` each time which adds O(n) as well which will cause O(n) memory increase. Not recommended based on poor implementation of `GetEntryByKey()` method.

## EntryLookupDictionary Recommendations
We can safely not copy the `IEnumerable<>` object as we are deriving the `Dictionary<>` from it. This would reduce the time to construct as well it will remove the extra memory overhead. This would make the total memory overhead O(2n). It would also allow all searches after initial construct to be closer to the advertised O(1) time. Beyond the implementation we need to add consistency checks and handle the exceptions.

## Usage Considerations
In each of the classes in their current state we hold a field of the copied List<> class. If we constantly change the parent list we will need to construct a new class each time and inccur the cost of instantiating that class. Each class would be best used in cases where we need to store data once and perform a number of lookup operations. All equality or comparer methods should be mindful not to throw exceptions.

## Size of Collections and Memory
The max element size of the collections in .NET is 2 billion with a specific GC flag. If we need data sets larger than 2 billion, custom list operations or nested pools of elements will need to be considered.

[List(T) Remarks](https://msdn.microsoft.com/en-us/library/6sh2ey19(v=vs.110).aspx#Anchor_7)
>For very large List<T> objects, you can increase the maximum capacity to 2 billion elements on a 64-bit system by setting the enabled attribute of the configuration element to true in the run-time environment.

