using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataDlow.Parallelization_Filtering_Customization
{
    public class AppendSample
    {
        public async Task Run()
        {
            var bufferBlock = new BufferBlock<int>();

            var a1 = new ActionBlock<int>(a =>
            {
                Console.WriteLine($"Mesaj {a} a1 tarafından işlenildi.");
            });


            var a2 = new ActionBlock<int>(a =>
            {
                Console.WriteLine($"Mesaj {a} a2 tarafından işlenildi.");
            });

            bufferBlock.LinkTo(a1, a => a % 2 == 0);
            bufferBlock.LinkTo(a2, new DataflowLinkOptions()
            {
                Append = false,
                MaxMessages = 4
            });

            bufferBlock.LinkTo(new ActionBlock<int>(a => Console.WriteLine($"{a} mesajı reddedildi.")));
            for (int i = 0; i < 10; i++)
            {
                await bufferBlock.SendAsync(i);
            }

        }
    }
}
