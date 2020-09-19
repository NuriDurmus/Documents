using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataDlow.Blocks
{
    public class TransformBlockSample
    {
        public void Run()
        {
            ConcurrentBag<int> values = new ConcurrentBag<int>();
            var transformBlock = new TransformBlock<int, string>(n =>
              {
                  Task.Delay(500).Wait();
                  values.Add(n);
                  return n.ToString();
              }, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 4 });

            for (int i = 0; i < 10; i++)
            {
                transformBlock.Post(i);
                Console.WriteLine("Input count:" + transformBlock.InputCount);
            }

            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine("output count:" + transformBlock.OutputCount);
                var result = transformBlock.Receive();
                var listResult = 0;
                values.TryTake(out listResult);
                Console.WriteLine($"Result:{result}  Output count:{transformBlock.OutputCount} input count:{transformBlock.InputCount} list item: {listResult}");
            }
        }
    }
}
