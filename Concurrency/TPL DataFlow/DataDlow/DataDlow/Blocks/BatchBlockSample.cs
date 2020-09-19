using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataDlow.Blocks
{
    public class BatchBlockSample
    {
       public void Run()
        {
            var batchBlock = new BatchBlock<int>(10);

            for (int i = 0; i < 20; i++)
            {
                batchBlock.Post(i);
            }

            batchBlock.Complete();

            batchBlock.Post(10);
            for (int i = 0; i < 5; i++)
            {
                int[] result;
                if (batchBlock.TryReceive(out result))
                {
                    Console.Write($"Received batch {i}:");

                    foreach (var r in result)
                    {
                        Console.Write(r + " ");
                    }
                    Console.Write("\n");
                }
                else
                {
                    Console.WriteLine("The block finished");
                    break;
                }
            }
        }

    }
}
