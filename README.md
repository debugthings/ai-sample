# ai-sample

## Memory Considerations
* Memory O(n) in all cases is the amount of `IItemWithKey<>` elements in the original list
* Size of the elements will vary with the Key type
* If a new object is needed to hold references it will be annotated with things like kp for Key Pair
* If possible I removed the allocation of the underlying copy to save on memory usage if possible. 

## Thread Safety
* Removing the copy can make the `First()` function unstable if another thread were to insert into List while enumerating.


## Size of Collections
The max element size of the collections in .NET is 2 billion with a specific GC flag. If we need larger datasets, custom list operations or nested pools of elements will need to be considered.

[List(T) Remarks](https://msdn.microsoft.com/en-us/library/6sh2ey19(v=vs.110).aspx#Anchor_7)
>For very large List<T> objects, you can increase the maximum capacity to 2 billion elements on a 64-bit system by setting the enabled attribute of the configuration element to true in the run-time environment.

## EntryLookupList (original analysis)
- General Observations
  - Allows more than 1 item with a specific Key
  - Will return the first item it finds in the list with the Key; other items will not be found
- Error prone `GetEntryByKey` method
  - No exception handling
- Memory Usage
  - O(2n) Memory usage due to copy operation Original [O(n)] + Copy [O(n)]
- Performance
  - O(n) Constructor time
  - O(n) Search time

## EntryLookupBinarySearch (original analysis)
- Allows more than 1 item with a specific Key
- Will any matching item it finds in the list with the Key; no guarantee of which one is found
- O(2n) Memory usage due to copy operation Original [O(n)] + Copy [O(n)]
- O(n) Constructor time
- Error prone `GetEntryByKey` method
    - Needs logic to check index out of range
    - No exception handling
- GetEntryByKey() causes an allocation
    - O(ki) Memory usage due to new TItem allocation (i) at an undetermined value (k)
    - This could end up being O(n) if total number of items searched 
    - Should use Clone() to ensure Key is properly compared
- GetEntryByKey() should sort List<>
    - BinarySearch requires a sorted list, each insert would require a sort 
    - Sort worst space usage is O(log n) (Quicksort Sort() worstcase)
    - Sort worst sort time is O(n^2) (Quicksort Sort() worstcase)
    - BinarySearch worst search time is O(log n)
    - Search is unstable and will return any item on a match with same value
- Worstcase Memory Usage
    - O(3n + log n)

## EntryLookupDictionary (original analysis)
- Does not allow more than 1 item, does not allow null
- O(2n) Memory Usage due to copy operation in constructor
- O(n) Constructor time
- GetEntryByKey is error prone
  - No exception handling
  - Does not account for duplicate key entries
  - Does not account for nulls
- GetEntryByKey O(n) lookup time due to creation of Dictionary each time
  - Will cause allocation of Dictionary<> object
  - This allocation ends up being O(n + (kvp*n)) size due to an allocation of IKeyValuePair<TKey, IItemWithKey<TKey>> * count of IItemWithKey
  - No allocation of Keys or Values due to reference types
- GetHashCode O(1) time
  - If the object does not override the default hashcode

## Thoughts
After modifying the methods to add error checking and to remove any unessecary allocations the memory footprint went down at the expense of thead safety. If immutability is a requirement, the copy operations should be added back, if not threading primatives should be used to control read/writes

### Low Memory Footprint Inconsistent Data
If you have a large list where you can sacrifice a bit of speed in the way of memory consumption you should consider the modified EntryLookupList. This list will ensure that you are not allocating any memory and will give a linear search time.

### Fast Search Time Medium Memory Consistent
With the requirement of no duplicates or no null key values I would recommend the use of the modified EntryLookupDictonary for the majority of the workloads. Per the MSDN documentation the Dictionary lookup approaches an O(1) timing. The modified method will end up with a O(2n + (kvp*n)) memory footprint.

### Fast Search Time Medium-Large Memory Inconsistent Data
Lastly if you have a large dataset to search and you require no consistency checks on the Keys you should consider the modified BinarySearch. The search time is O(log n) and total space will be O(2n + log n)

