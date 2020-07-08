using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AquaMonitor.Data.Models
{
    /// <summary>
    /// Collection of latest readings
    /// </summary>
    public class ReadingCollection : IEnumerable<IReading>
    {
        private readonly List<IReading> entries = new List<IReading>();

        public ReadingCollection()
        {
        }

        public ReadingCollection(IEnumerable<IReading> readings)
        {
            this.entries.AddRange(readings);
        }

        public IReading this[ReadingType type]
        {
            get
            {
                return entries.FirstOrDefault(t => t.Type == type);
            }
            set
            {
                for (int x = 0; x < entries.Count; x++)
                {
                    if (entries[x].Type == type)
                    {
                        entries[x] = value;
                        if(value.Type == type)
                            break; // don't look any further
                    }
                }
            }
        }


        public void Add(IReading reading)
        {
            entries.Add(reading);
        }

        public void Remove(IReading reading)
        {
            entries.Remove(reading);
        }

        public void Remove(ReadingType type)
        {
            entries.RemoveAll(t => t.Type == type);
        }

        public void Clear()
        {
            entries.Clear();
        }
        
        public IEnumerator<IReading> GetEnumerator()
        {
            return entries.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return entries.GetEnumerator();
        }
    }
}
