using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetfairConsole
{
  public class FixedSizedQueue<T> : Queue<T>
  {
    Queue<T> q = new Queue<T>();
    private int limit { get; set; }

    public FixedSizedQueue(int limit)
    {
      this.limit = limit;
    }

    public void Enqueue(T obj)
    {
      if (q.Count == limit)
      {
        q.Dequeue();
        q.Enqueue(obj);
      }
      else
      {
        q.Enqueue(obj);
      }

    }
  }
}
