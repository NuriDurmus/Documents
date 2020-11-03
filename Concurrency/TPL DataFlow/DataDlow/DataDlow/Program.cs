using DataDlow.Blocks;
using DataDlow.Parallelization_Filtering_Customization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataDlow
{
    class Program
    {
        static void Main(string[] args)
        {
            var test = new Test();
            test.number = 3;
            test.number = 0;
            var b = test.IsEmpty();
            List<Test> test1 = new List<Test>() { new Test() { number = new Random().Next(0, 1000), operationDate = DateTime.Now } };
            IList<Test> test2 = test1;
            if (test1[0].order.Value == 4)
            {

            }
            new CustomBlockSample().Run();
            var a = 10;
            Console.WriteLine("Tamamlandı");
            Console.ReadLine();
        }

    }

    public class Test
    {
        public int number { get; set; }

        public DateTime operationDate { get; set; }
        public int? order { get; set; }


        public override bool Equals(Object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Test p = (Test)obj;
                return (number == p.number) && (order == p.order);
            }
        }
        public override int GetHashCode()
        {
            return (base.GetHashCode() << 2) ^ number;
        }

        public bool IsEmpty()
        {
            return this.Equals(new Test());
        }
    }
}
