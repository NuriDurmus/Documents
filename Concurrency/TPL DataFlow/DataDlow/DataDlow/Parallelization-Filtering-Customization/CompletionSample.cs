using DataDlow.Parallelization_Filtering_Customization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataDlow.Blocks
{
    public static class CompletionSample
    {
        public static async Task Run()
        {
            var broadcastBlock = new BroadcastBlock<int>(a => a);

            var a1 = new TransformBlock<int, int>(a =>
            {
                Console.WriteLine($"Mesaj {a} a1 tarafından işlenildi.");
                Task.Delay(300).Wait();
                return -a;
            });


            var a2 = new TransformBlock<int, int>(a =>
            {
                Console.WriteLine($"Mesaj {a} a2 tarafından işlenildi.");
                Task.Delay(300).Wait();
                return a;
            });


            var joinBlock = new JoinBlock<int, int>();
            a1.LinkToWithPropagation(joinBlock.Target1);
            a2.LinkToWithPropagation(joinBlock.Target2);

            broadcastBlock.LinkToWithPropagation(a1);
            broadcastBlock.LinkToWithPropagation(a2);

            var finalBlock = new ActionBlock<Tuple<int, int>>(a =>
              {
                  Console.WriteLine($"{a.Item1}: tüm consumer'lar tarafından işlenildi");
              });

            joinBlock.LinkToWithPropagation(finalBlock);

            for (int i = 0; i < 10; i++)
            {
                await broadcastBlock.SendAsync(i);
            }

            broadcastBlock.Complete();
            finalBlock.Completion.Wait();
            Console.WriteLine("Tamamlandı");
        }
    }
}
