using BinArchiver.Version;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinArchiver
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello world! This is version: {0}", GitVersionRetreiver.getVersion());
            Console.ReadKey();
        }
    }
}
