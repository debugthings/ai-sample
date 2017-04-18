namespace SortingHat1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public interface IItemWithKey<TKey>
    {
        TKey Key { get; set; }
    }

    
    public interface IEntryLookup<TKey>
    {
        IItemWithKey<TKey> GetEntryByKey(TKey key);
    }

    public class stringKey : IItemWithKey<string>, IEquatable<string>, IComparable<string>
    {
        public string Key
        {
            get; set;
        }

        public int CompareTo(string other)
        {
            return Key.CompareTo(other);
        }

        public bool Equals(string other)
        {
            return Key.Equals(other);
        }
    }

    public class EntryLookupList<TKey> : IEntryLookup<TKey>
           where TKey : IEquatable<TKey>
    {
        private IEnumerable<IItemWithKey<TKey>> entriesToSearch;
        public EntryLookupList(IEnumerable<IItemWithKey<TKey>> inputEntries)
        {
            // No copy required as we do not alter the existing list
            this.entriesToSearch = inputEntries;
        }

        public IItemWithKey<TKey> GetEntryByKey(TKey keyToLookup)
        {
            try
            {
                return entriesToSearch.First(entry => entry.Key.Equals(keyToLookup));
            }
            catch (NullReferenceException nullRefEx)
            {
                // entry.Key could be null
                // Logger.Log(nullRefEx)
                throw nullRefEx;
            }
            catch (Exception ex)
            {
                // entry.Key could throw and exception of an unknown type
                // Logger.Log(ex)
                throw ex;
            }

        }
    }

    public class EntryLookupBinarySearch<TKey, TItem> : IEntryLookup<TKey>, IComparer<TItem>
           where TKey : IEquatable<TKey>, IComparable<TKey>
           where TItem : IItemWithKey<TKey>, new()
    {
        private List<TItem> entriesToSearch;
        private bool isSorted;

        public EntryLookupBinarySearch(IEnumerable<TItem> inputEntries)
        {
            // Copy required as we might want to keep existing list
            // in exsiting order.
            this.entriesToSearch = new List<TItem>(inputEntries);
        }

        public int Compare(TItem leftItem, TItem rightItem)
        {
            return leftItem.Key.CompareTo(rightItem.Key);
        }

        public IItemWithKey<TKey> GetEntryByKey(TKey keyToLookup)
        {
            // Sort list for proper BinarySearch operation
            if (!isSorted)
            {
                entriesToSearch.Sort();
            }
            // If we have no entries to search return null
            if (entriesToSearch.Count == 0)
            {
                return null;
            }
            var entryToSearch = new TItem();
            entryToSearch.Key = keyToLookup;
            var index = entriesToSearch.BinarySearch(entryToSearch, this);
            // If the index is not out of range return the item
            if (index >= 0)
            {
                return this.entriesToSearch[index];
            }
            return null;
        }
    }

    public class EntryLookupDictionary<TKey> : IEntryLookup<TKey>, IEqualityComparer<TKey>
           where TKey : IEquatable<TKey>
    {

        private IEnumerable<IItemWithKey<TKey>> entriesToSearch;
        private Dictionary<TKey, IItemWithKey<TKey>> entriesDictionary;

        public EntryLookupDictionary(IEnumerable<IItemWithKey<TKey>> inputEntries)
        {
            // No copy required as we're generating a type from this list
            this.entriesToSearch = inputEntries;
        }

        public IItemWithKey<TKey> GetEntryByKey(TKey keyToLookup)
        {
            if (entriesDictionary == null)
            {
                try
                {
                    entriesDictionary = this.entriesToSearch.ToDictionary(entry => entry.Key, this);
                }
                catch (ArgumentNullException argumentNullException)
                {
                    // Per MSDN docs this can be thrown if the key selector returns null
                    // Logger.Log(argumentNullException);
                    throw argumentNullException;
                }
                catch (ArgumentException argumentException)
                {
                    // Per MSDN docs this can be thrown if there are duplicates
                    // Logger.Log(argumentException);
                    throw argumentException;
                }
                catch (Exception ex)
                {
                    // Some exception could be thrown by the getter of the Key property
                    // we need to handle that here.
                    // Logger.Log(ex);
                    throw ex;
                }
            }
            // If we do not have a null 
            if (keyToLookup != null)
            {
                return entriesDictionary[keyToLookup];
            }
            return null;

        }

        public bool Equals(TKey leftItem, TKey rightItem)
        {
            return leftItem.Equals(rightItem);
        }

        public int GetHashCode(TKey obj)
        {
            return obj.GetHashCode();
        }
    }
}
