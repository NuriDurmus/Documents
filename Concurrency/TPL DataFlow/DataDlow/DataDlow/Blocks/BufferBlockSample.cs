using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataDlow.Blocks
{
    public class BufferBlockSample
    {
        public void Run()
        {
            var bufferBlock = new BufferBlock<int>();
            for (int i = 0; i < 10; i++)
            {
                bufferBlock.Post(i);
            }
            for (int i = 0; i < 10; i++)
            {
                int result = bufferBlock.Receive();
                Console.WriteLine(result);
            }
        }

        public void Run1()
        {
            var bufferBlock = new BufferBlock<int>(new DataflowBlockOptions() { BoundedCapacity = 1 });

            var a1 = new ActionBlock<int>(a =>
              {
                  Console.WriteLine($"Mesaj {a} a1 tarafından işlenildi.");
                  Task.Delay(300).Wait();
              },new ExecutionDataflowBlockOptions() { BoundedCapacity = 1});


            var a2 = new ActionBlock<int>(a =>
            {
                Console.WriteLine($"Mesaj {a} a2 tarafından işlenildi.");
                Task.Delay(300).Wait();
            }, new ExecutionDataflowBlockOptions() { BoundedCapacity = 1 });

            bufferBlock.LinkTo(a1);
            bufferBlock.LinkTo(a2);

            for (int i = 0; i < 10; i++)
            {
                bufferBlock.SendAsync(i).ContinueWith(a =>
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
