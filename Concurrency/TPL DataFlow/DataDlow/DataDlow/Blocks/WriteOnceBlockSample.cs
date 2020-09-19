using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks.Dataflow;

namespace DataDlow
{
    public class WriteOnceBlockSample
    {
        public void Run()
        {
            var block = new WriteOnceBlock<int>(a => a);
            var print = new ActionBlock<int>(a => Console.WriteLine($"Mesaj {a} kabul edildi."));
            for (int i = 0; i < 10; i++)
            {
                if (block.Post(i))
                {
                    Console.WriteLine($"Mesaj {i} kabul edildi");
                }
                else
                {
                    Console.WriteLine($"Mesaj {i} reddedildi");
                }
            }
            block.LinkTo(print);
            
        }
    }
}
