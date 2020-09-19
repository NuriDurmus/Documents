using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks.Dataflow;

namespace DataDlow.Blocks
{
    public class TransformManyBlockSample
    {
        public void Run()
        {
            var transformManyBlock = new TransformManyBlock<int, string>(a => FindEvenNumbers(a),
                new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 5 });

            var printBlock = new ActionBlock<string>(a => Console.WriteLine($"Alınan mesaj:{a}"));

            transformManyBlock.LinkTo(printBlock);

            for (int i = 0; i < 10; i++)
            {
                transformManyBlock.Post(i);
            }
            Console.WriteLine("Tamamlandı");
        }

        private IEnumerable<string> FindEvenNumbers(int number)
        {
            for (int i = 0; i < number; i++)
            {
                if (i % 2 == 0)
                {
                    yield return $"{number}:{i}";
                }

            }
        }


    }
}
