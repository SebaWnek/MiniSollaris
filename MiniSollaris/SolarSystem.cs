using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace MiniSollaris
{
    class SolarSystem
    {
        public int TimeStep { get; set; }
        internal CelestialObject[] Objects { get => objects; set => objects = value; }

        private CelestialObject[] objects;

        public SolarSystem(CelestialObject[] obj, int timeStep)
        {
            TimeStep = timeStep;
            Objects = obj;
        }
        public SolarSystem(List<CelestialObject> obj, int timeStep)
        {
            TimeStep = timeStep;
            Objects = obj.ToArray();
        }

        public SolarSystem(string path, int timeStep)
        {
            TimeStep = timeStep;
            Objects = Deserialize(path);
        }

        public void CalculateStep()
        {
            foreach (CelestialObject obj in objects)
            {
                obj.CalculateNewPosition(objects, TimeStep);
            }
        }

        public void CalculateStepParallel()
        {
            Parallel.ForEach(Objects, (obj) => { (obj as CelestialObject).CalculateNewPosition(Objects, TimeStep); });
        }

        public void Serialize(string path)
        {
            string jsonString = JsonSerializer.Serialize(Objects, new JsonSerializerOptions() { WriteIndented = true } );
            File.WriteAllText(path, jsonString);
        }

        public static CelestialObject[] Deserialize(string path)
        {
            CelestialObject[] result;
            string jsonString = File.ReadAllText(path);
            result = JsonSerializer.Deserialize<CelestialObject[]>(jsonString);
            return result;
        }

        public CelestialObject SelectObject(string name)
        {
            CelestialObject result = objects.First(obj => obj.Name == name); //not First or default so we get exception if not found
            return result;
        }
    }
}
