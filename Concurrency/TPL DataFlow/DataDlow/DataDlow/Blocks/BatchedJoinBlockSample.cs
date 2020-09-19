using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataDlow.Blocks
{
    public class BatchedJoinBlockSample
    {
        public void Run()
        {
            var broadcastBlock = new BroadcastBlock<int>(a => a);

            var a1 = new TransformBlock<int, int>(a =>
            {
                Console.WriteLine($"Mesaj {a} a1 tarafından işlenilmekte.");
                Task.Delay(300).Wait();
                if (a % 2 == 0)
                {
                    Task.Delay(300).Wait();
                }
                else
                {
                    Task.Delay(50).Wait();
                }
                return -a;
            }, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 3 });


            var a2 = new TransformBlock<int, int>(a =>
            {
                Console.WriteLine($"Mesaj {a} a2 tarafından işlenilmekte.");
                Task.Delay(150).Wait();
                return a;
            }, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 3 });

            broadcastBlock.LinkTo(a1);
            broadcastBlock.LinkTo(a2);

            var joinBlock = new BatchedJoinBlock<int, int>(3);
            a1.LinkTo(joinBlock.Target1);
            a2.LinkTo(joinBlock.Target2);

            var printBlock = new ActionBlock<Tuple<IList<int>, IList<int>>>(a => Console.WriteLine($"Mesaj: [{string.Join(',', a.Item1)}] , [{string.Join(',', a.Item2)}]"));

            joinBlock.LinkTo(printBlock);

            for (int i = 0; i < 10; i++)
            {
                broadcastBlock.SendAsync(i).ContinueWith(a =>
                {
                    if (a.Result)
                    {
                        Console.WriteLine($"{i} mesajı kabul edildi.");
                    }
                    else
                    {
                        Console.WriteLine($"{i} mesajı reddedildi.");
                    }
                });
            }
        }
    }
}
