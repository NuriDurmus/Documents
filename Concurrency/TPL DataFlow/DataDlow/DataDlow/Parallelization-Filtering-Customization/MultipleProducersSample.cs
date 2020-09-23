using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataDlow.Parallelization_Filtering_Customization
{
    public class MultipleProducersSample
    {
        public async Task Run()
        {
            var producer1 = new TransformBlock<string,string>(a =>
            {
                Task.Delay(150).Wait();
                return a;
            });

            var producer2 = new TransformBlock<string, string>(a =>
            {
                Task.Delay(300).Wait();
                return a;
            });
            var printBlock = new ActionBlock<string>(n => Console.WriteLine(n));
            producer1.LinkTo(printBlock);
            producer2.LinkTo(printBlock);

            for (int i = 0; i < 10; i++)
            {
                producer1.Post($"Producer 1 mesajı: {i}");
                producer2.Post($"Producer 2 mesajı: {i}");
            }
            await Task.WhenAll(new[] { producer1.Completion, producer2.Completion }).ContinueWith(a=>printBlock.Complete());
            printBlock.Completion.Wait();
        }
    }
}
