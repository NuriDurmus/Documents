using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataDlow
{
    class Program
    {
        static void Main(string[] args)
        {
            new WriteOnceBlockSample().Run();
            Console.WriteLine("Tamamlandı");
            Console.ReadLine();
        }
    }
}
