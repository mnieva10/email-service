using System;
using System.Collections.Generic;

namespace Sovos.SvcBus.Common.Model.Infrastructure.Pooling
{
    public class ItemStore<T> : LinkedList<TimeTrackableObject<T>> 
    {
        public T Fetch()
        {
            var item = First.Value;
            RemoveFirst();
            return item.Object;
        }

        public void Store(T item)
        {
            AddFirst(new TimeTrackableObject<T>(item, DateTime.Now.Ticks));
        }
    }

    public class TimeTrackableObject<T>
    {
        public long LastUsed { get; set; }
        public T Object { get; set; }

        public TimeTrackableObject(T obj, long ticks)
        {
            Object = obj;
            LastUsed = ticks;
        }
    }
}
