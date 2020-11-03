using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataDlow.Parallelization_Filtering_Customization
{
    public class ErrorSample
    {
        public async Task Run()
        {
            var block = new TransformBlock<int, int>(n =>
               {
                   if (n == 5)
                   {
                       throw new Exception("Hata mesajı!");
                   }
                   Console.WriteLine(n + " mesajı işlenildi");
                   return n;
               }, new ExecutionDataflowBlockOptions() { BoundedCapacity = 5 });
            for (int i = 0; i < 10; i++)
            {
                if (block.Post(i))
                {
                    Console.WriteLine(i + " mesajı kabul edildi");
                }
                else
                {
                    Console.WriteLine(i + " mesajı kabul reddedildi");
                }
            }
            try
            {
                block.Complete();
                block.Completion.Wait();
            }
            catch (AggregateException ae)
            {
                Console.WriteLine(ae.Flatten().InnerException);
            }

        }
    }
}
