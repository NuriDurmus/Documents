using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataDlow.Parallelization_Filtering_Customization
{
    public class CustomBlockSample
    {
        public async Task Run()
        {
            var increasingBlock = CreateFilteringBlock<int>();
            var printBlock = new ActionBlock<int>(
                a => Console.WriteLine(a + " mesajı alındı."));
            increasingBlock.LinkToWithPropagation(printBlock);

            await increasingBlock.SendAsync(1);
            await increasingBlock.SendAsync(2);
            await increasingBlock.SendAsync(4);
            await increasingBlock.SendAsync(1);
            await increasingBlock.SendAsync(2);
            await increasingBlock.SendAsync(9);

            increasingBlock.Complete();
            printBlock.Completion.Wait();

        }

        public static IPropagatorBlock<T, T> CreateFilteringBlock<T>() where T : IComparable<T>, new()
        {
            T maxElement = default(T);
            var source = new BufferBlock<T>();
            var target = new ActionBlock<T>(async item =>
              {
                  if (item.CompareTo(maxElement) > 0)
                  {
                      await source.SendAsync(item);
                      maxElement = item;
                  }
              });
            target.Completion.ContinueWith(a =>
            {
                if (a.IsFaulted)
                {
                    ((ITargetBlock<T>)source).Fault(a.Exception);
                }
                else
                {
                    source.Complete();
                }
            });
            return DataflowBlock.Encapsulate(target, source);
        }
    }


}
