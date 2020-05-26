using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace MiniSollaris
{
    class CelestialObject
    {
        [JsonIgnore]
        public static double G { get; set; } = 6.6743015E-11; //Gravitational constant
        public string Name { get; set; }
        public double Mass { get; set; } //kg
        [JsonIgnore]
        public double StdGravPar { get; set; } // Standard Gravitational Parameter = M * G
        public bool IsCalculatable { get; set; }
        public long Radius { get; set; } //m
        public long[] Position { get; set; } = { 0, 0 }; //m, m in X and Y directions
        public double[] Velocity { get; set; } = { 0, 0 };  //m/s, m/s in X and Y directions
        
        public Ellipse Picture { get; set; }
        protected double[] acceleration = { 0, 0 };
        [JsonIgnore]
        public int Number { get; set; }
        [JsonIgnore]
        public CelestialObject[] CalculatableObjects { get; set; }

        public CelestialObject()
        {
            StdGravPar = G * Mass;
        }

        /// <summary>
        /// New CelestialObject object.
        /// </summary>
        /// <remarks>
        /// For now it uses only one elipse to represent object. For future use should be expaanded to more advanced graphic representation, but for this project it is enough.
        /// </remarks>
        /// <param name="name">Name of the object</param>
        /// <param name="mass">Mass of the object</param>
        /// <param name="isCalculatable">Should this object exert forces on others, or only be affected by otherw one-way</param>
        /// <param name="radius">Radius of the object</param>
        /// <param name="position">Starting position vector of the object in world coordinates</param>
        /// <param name="velocity">Starting velocity vector of the object</param>
        /// <param name="imageSize">Elipse size to be drawn to represent object in UI</param>
        /// <param name="color">Elipse color to be drawn to represent object in UI</param>
        public CelestialObject(string name, double mass, bool isCalculatable, long radius, long[] position, double[] velocity, int imageSize = 2, Brush color = null)
        {
            Name = name;
            Mass = mass;
            IsCalculatable = isCalculatable;
            Radius = radius;
            Position = position;
            Velocity = velocity;
            StdGravPar = G * Mass;
            GenerateShape(imageSize, color);
        }

        /// <summary>
        /// New CelestialObject object.
        /// </summary>
        /// <remarks>
        /// For now it uses only one elipse to represent object. For future use should be expaanded to more advanced graphic representation, but for this project it is enough.
        /// </remarks>
        /// <param name="name">Name of the object</param>
        /// <param name="mass">Mass of the object</param>
        /// <param name="isCalculatable">Should this object exert forces on others, or only be affected by otherw one-way</param>
        /// <param name="radius">Radius of the object</param>
        /// <param name="distance">Polar distance of object from center of coordinate system</param>
        /// <param name="angle">Polar angle of object</param>
        /// <param name="andleDeviation">Angle between vector prependicular, counter clockwise to radius and speed vector of object in radians</param>
        /// <param name="speed">Scalar velocity of object</param>
        /// <param name="imageSize">Elipse size to be drawn to represent object in UI</param>
        /// <param name="color">Elipse color to be drawn to represent object in UI</param>
        public CelestialObject(string name, double mass, bool isCalculatable, long radius, long distance, double angle, double andleDeviation, double speed, int imageSize = 2, Brush color = null)
        {
            Name = name;
            Mass = mass;
            IsCalculatable = isCalculatable;
            Radius = radius;
            Position = MathHelper.PositionPolarToCartesian(distance, angle);
            Velocity = MathHelper.VelocityPolarToCartesian(speed, angle, andleDeviation);
            StdGravPar = G * Mass;
            GenerateShape(imageSize, color);
        }

        /// <summary>
        /// New CelestialObject object. Position relative to parent object.
        /// </summary>
        /// <remarks>
        /// For now it uses only one elipse to represent object. For future use should be expaanded to more advanced graphic representation, but for this project it is enough.
        /// </remarks>
        /// <param name="parent">Parent object to calculate position and velocity in relation to</param>
        /// <param name="name">Name of the object</param>
        /// <param name="mass">Mass of the object</param>
        /// <param name="isCalculatable">Should this object exert forces on others, or only be affected by otherw one-way</param>
        /// <param name="radius">Radius of the object</param>
        /// <param name="position">Starting position vector of the object in world coordinates</param>
        /// <param name="velocity">Starting velocity vector of the object</param>
        /// <param name="imageSize">Elipse size to be drawn to represent object in UI</param>
        /// <param name="color">Elipse color to be drawn to represent object in UI</param>
        public CelestialObject(CelestialObject parent, string name, double mass, bool isCalculatable, long radius, long[] position, double[] velocity, int imageSize = 2, Brush color = null)
        {
            Name = name;
            Mass = mass;
            IsCalculatable = isCalculatable;
            Radius = radius;
            Position[0] = position[0] + parent.Position[0];
            Position[1] = position[1] + parent.Position[1];
            Velocity[0] = velocity[0] + parent.Velocity[0];
            Velocity[1] = velocity[1] + parent.Velocity[1];
            StdGravPar = G * Mass;
            GenerateShape(imageSize, color);
        }

        /// <summary>
        /// New CelestialObject object. Position relative to parent object.
        /// </summary>
        /// <remarks>
        /// For now it uses only one elipse to represent object. For future use should be expaanded to more advanced graphic representation, but for this project it is enough.
        /// </remarks>
        /// <param name="parent">Parent object to calculate position and velocity in relation to</param>
        /// <param name="name">Name of the object</param>
        /// <param name="mass">Mass of the object</param>
        /// <param name="isCalculatable">Should this object exert forces on others, or only be affected by otherw one-way</param>
        /// <param name="radius">Radius of the object</param>
        /// <param name="distance">Polar distance of object from center of coordinate system</param>
        /// <param name="angle">Polar angle of object</param>
        /// <param name="andleDeviation">Angle between vector prependicular, counter clockwise to radius and speed vector of object in radians</param>
        /// <param name="speed">Scalar velocity of object</param>
        /// <param name="imageSize">Elipse size to be drawn to represent object in UI</param>
        /// <param name="color">Elipse color to be drawn to represent object in UI</param>
        public CelestialObject(CelestialObject parent, string name, double mass, bool isCalculatable, long radius, long distance, double angle, double andleDeviation, double speed, int imageSize = 2, Brush color = null)
        {
            Name = name;
            Mass = mass;
            IsCalculatable = isCalculatable;
            Radius = radius;
            Position = MathHelper.PositionPolarToCartesian(distance, angle);
            Velocity = MathHelper.VelocityPolarToCartesian(speed, angle, andleDeviation);
            Position[0] += parent.Position[0];
            Position[1] += parent.Position[1];
            Velocity[0] += parent.Velocity[0];
            Velocity[1] += parent.Velocity[1];
            StdGravPar = G * Mass;
            GenerateShape(imageSize, color);
        }

        /// <summary>
        /// Generates graphic representation of object. 
        /// </summary>
        /// <remarks>
        /// For future use should be expanded to contain more advanced representation and on-demmand displayed object parameters. For purpose of this project it is enough.
        /// </remarks>
        /// <param name="imageSize">Elipse size to be drawn to represent object in UI</param>
        /// <param name="color">Elipse color to be drawn to represent object in UI</param>
        private void GenerateShape(int imageSize, Brush color)
        {
            Picture = new Ellipse
            {
                Fill = color ?? Brushes.White,
                Width = imageSize,
                Height = imageSize
            };
        }

        /// <summary>
        /// Aggregate method to perform all steps of calculating new object position. 
        /// </summary>
        /// <remarks>
        /// To be used in algorithms that don't require dividing calculations into multiple steps.
        /// </remarks>
        /// <param name="timeStep">Time step of simulation</param>
        public void CalculateNewPosition(double timeStep)
        {
            CalculateAcceleration();
            UpdateVelocity(timeStep);
            UpdatePosition(timeStep);
        }

        /// <summary>
        /// Aggregate method to perform all steps of calculating new object velocity. 
        /// </summary>
        /// <remarks>
        /// To be used in algorithms dividing calculaitons in two steps. 
        /// This is first step that calculates only values not read by other object during calculaitons, allowing for thread-safe parallel calculaitons of multiple objects at once.
        /// </remarks>
        /// <param name="timeStep">Time step of simulation</param>
        public void CalculateNewVelocity(double timeStep)
        {
            CalculateAcceleration();
            UpdateVelocity(timeStep);
        }

        /// <summary>
        /// Updates object position based on current velocity ant time step.
        /// </summary>
        /// <remarks>
        /// To be used in algorithms dividing calculaitons in two steps. 
        /// In this step each object updates only it's own fields, allowing to thread safe parallel updating of multiple objects at one, as long as other objects are also in the same step.
        /// If another object will be calculating new acceleration it might read this object position in thread unsafe manner. 
        /// Thus must decide to either ensure that two steps are calculatted completely independently, or make sure that errors due to thread-unsafe reading are small enough to not cause siginficant problems.
        /// In theory extremely small differences in distance during force calculaiton in solar-system-scale shouldn't cause any significant errors, but if decided to use it that way must be aware of potential risk.
        /// </remarks>
        /// <param name="timeStep">Time step of simulaiton</param>
        public void UpdatePosition(double timeStep)
        {
            Position[0] += (long)Math.Round(Velocity[0] * timeStep);
            Position[1] += (long)Math.Round(Velocity[1] * timeStep);
        }

        /// <summary>
        /// Calculates new velocity from current acceleration and time step.
        /// </summary>
        /// <param name="timeStep">Time step of simulation</param>
        private void UpdateVelocity(double timeStep)
        {
            Velocity[0] += acceleration[0] * timeStep;
            Velocity[1] += acceleration[1] * timeStep;
        }

        /// <summary>
        /// Calculates new acceleration.
        /// </summary>
        /// <remarks>
        /// Must NOT be provided itself. If object will calculate force from itself <code>r2</code> will befome <code>0</code>, <code>a</code> will become <code>Double.PositiveInfinity</code>.
        /// It will cause <code>acceleration[]</code> to become <code>{ Double.NaN, Double.NaN }</code> and break simulation.
        /// I decided to not put try-catch here as I want to provide as much efficiency as possible and remove all unnecessary code here, so extra care must be taken when assigning calcullatable objects.
        /// </remarks>
        protected virtual void CalculateAcceleration()
        {
            acceleration = new double[] { 0, 0 };
            double dX, dY, r2, a;                                       //dX, dY - relative position vector components, r2 s- quare of distance, a - temporary value 

            foreach (CelestialObject obj in CalculatableObjects)
            {
                dX = obj.Position[0] - this.Position[0];
                dY = obj.Position[1] - this.Position[1];
                r2 = dX * dX + dY * dY;
                a = obj.StdGravPar / (Math.Sqrt(r2) * r2);
                acceleration[0] += a * dX;
                acceleration[1] += a * dY;
            }
        }
    }
}
