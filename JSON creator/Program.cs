using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using MiniSollaris;

namespace JSON_creator
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            List<CelestialObject> objects = new List<CelestialObject>();
            string name;
            double mass;
            bool isCalc;
            long radius;
            string ephemeris;
            string[] vectors;
            long[] position = new long[2];
            double[] velocity = new double[2];
            int size;
            string color = "";
            var converter = new BrushConverter();

            bool isDataCorrect = true;
            do
            {
                Console.WriteLine("Name?");
                name = Console.ReadLine();
                Console.WriteLine("Mass? [kg]");
                while (!double.TryParse(Console.ReadLine(), out mass))
                {
                    Console.WriteLine("Incorrect value!");
                }
                Console.WriteLine("Is calculatable? y/n");
                if (Console.ReadLine() == "y") isCalc = true;
                else isCalc = false; 
                Console.WriteLine("Radius? [km]");
                while (!long.TryParse(Console.ReadLine(), out radius))
                {
                    Console.WriteLine("Incorrect value!");
                }
                radius *= 1000;
                Console.WriteLine("Position Data?");
                do
                {
                    try
                    {
                        isDataCorrect = true;
                        ephemeris = Console.ReadLine();
                        ephemeris += Console.ReadLine();
                        ephemeris = ephemeris.Replace('.', ',');
                        vectors = ephemeris.Split(new char[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                        position = new long[] { (long)(double.Parse(vectors[0], System.Globalization.NumberStyles.Float) * 1000),
                                        (long)(double.Parse(vectors[1], System.Globalization.NumberStyles.Float) * 1000) };
                        velocity = new double[] { double.Parse(vectors[3], System.Globalization.NumberStyles.Float),
                                          double.Parse(vectors[4], System.Globalization.NumberStyles.Float) };
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Incorrect value!");
                        isDataCorrect = false;
                    }
                } while (!isDataCorrect);
                Console.WriteLine("Image size?");
                while (!int.TryParse(Console.ReadLine(), out size))
                {
                    Console.WriteLine("Incorrect value!");
                }
                Console.WriteLine("Image color? #AARRGGBB");
                do
                {
                    isDataCorrect = true;
                    color = Console.ReadLine();
                    if(!Regex.IsMatch(color, "^#([A-Fa-f0-9]{8})$"))
                    {
                        Console.WriteLine("Incorrect value!");
                        isDataCorrect = false;
                    }
                } while (!isDataCorrect);
                    objects.Add(new CelestialObject(name, mass, isCalc, radius, position, velocity, size, (Brush)converter.ConvertFromString(color)));
                Console.WriteLine("Add next?");
            } while (Console.ReadLine() == "y");
            objects.Serialize("jsonResult.json");
        }
    }
}
