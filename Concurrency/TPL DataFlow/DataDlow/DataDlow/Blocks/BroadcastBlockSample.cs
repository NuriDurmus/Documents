using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataDlow.Blocks
{
    public class BroadcastBlockSample
    {
        public void Run()
        {
            var broadcastBlock = new BroadcastBlock<int>(a => a);

            var a1 = new ActionBlock<int>(a =>
            {
                Console.WriteLine($"Mesaj {a} a1 tarafından işlenildi.");
                Task.Delay(300).Wait();
            });


            var a2 = new ActionBlock<int>(a =>
            {
                Console.WriteLine($"Mesaj {a} a2 tarafından işlenildi.");
                Task.Delay(300).Wait();
            });

            broadcastBlock.LinkTo(a1);
            broadcastBlock.LinkTo(a2);

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
