using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataDlow.Blocks
{
    public class ActionBlockSample
    {
        public void Run()
        {
            var actionBlock = new ActionBlock<int>(n =>
            {
                Task.Delay(500).Wait();
                Console.WriteLine(n); ;
            });

            for (int i = 0; i < 10; i++)
            {
                actionBlock.Post(i);
                Console.WriteLine("Input count" + actionBlock.InputCount);
            }
        }
    }
}
