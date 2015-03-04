using System;
using System.Numerics;
using System.Text;

namespace BaseStationTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Model m = new Model();
            Controller c = new Controller(m);
        }
    }
}
