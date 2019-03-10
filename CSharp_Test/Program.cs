using AForge.Math;
using ConsoleMorphVOXPro;
using ConsoleMorphVOXPro.FileSave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            test1();
            CaculateTimes caculate = new CaculateTimes();
            caculate.StartTime();
            //4 3 2 6 7 8 9 0
            int[] nums = { 4, 3 ,2, 6, 7, 8 ,9, 0 };
            int[] nums_2 = { 1, 2, 5, 4, 8, 6, 4, 5, 2, 3, 1 };
            Complex[] complices = new Complex[nums.Length];
            for (int i = 0; i < nums.Length; i++)
            {
                complices[i] = new Complex(nums[i], 0);
            }
            FourierTransform.DFT(complices, FourierTransform.Direction.Backward);
            for (int i = 0; i < nums.Length; i++)
            {
                Console.WriteLine(complices[i].Re + " " + complices[i].Im);
            }
            Console.WriteLine();
            Console.WriteLine("====================");
            Console.WriteLine();
            for (int i = 0; i < nums.Length; i++)
            {
                complices[i].Re = nums[i];
                complices[i].Im = 0;
            }
            FourierTransform.FFT(complices, FourierTransform.Direction.Backward);
            for (int i = 0; i < nums.Length; i++)
            {
                Console.WriteLine(complices[i].Re + " " + complices[i].Im);
            }
            Console.WriteLine(caculate.EndTime());
            Console.ReadKey();
        }
        static public void test1()
        {
            string file = "1.cvs";
            FileSave fileSave = new FileSave();
            fileSave.SaveMaxAveRageMin(file,1,2,3);
            fileSave.SaveMaxAveRageMin(file, 1, 2, 3);
            fileSave.SaveMaxAveRageMin(file, 1, 2, 3);
        }
    }
}
