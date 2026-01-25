using System;

namespace Oqtane.Modules.Visualization3D.Models
{
    /// <summary>
    /// Base class for all 3D objects
    /// </summary>
    public abstract class Object3D
    {
        public string Id { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Scale { get; set; }
        public Color Color { get; set; }
        public bool Visible { get; set; }

        protected Object3D()
        {
            Id = Guid.NewGuid().ToString();
            Position = Vector3.Zero;
            Rotation = Vector3.Zero;
            Scale = Vector3.One;
            Color = Color.White;
            Visible = true;
        }

        public abstract string GetType();
    }

    /// <summary>
    /// Represents a 3D cube
    /// </summary>
    public class Cube : Object3D
    {
        public double Size { get; set; }

        public Cube() : this(1.0) { }

        public Cube(double size)
        {
            Size = size;
        }

        public override string GetType() => "Cube";
    }

    /// <summary>
    /// Represents a 3D sphere
    /// </summary>
    public class Sphere : Object3D
    {
        public double Radius { get; set; }
        public int Segments { get; set; }

        public Sphere() : this(1.0) { }

        public Sphere(double radius, int segments = 32)
        {
            Radius = radius;
            Segments = segments;
        }

        public override string GetType() => "Sphere";
    }

    /// <summary>
    /// Represents a 3D plane
    /// </summary>
    public class Plane : Object3D
    {
        public double Width { get; set; }
        public double Height { get; set; }

        public Plane() : this(1.0, 1.0) { }

        public Plane(double width, double height)
        {
            Width = width;
            Height = height;
        }

        public override string GetType() => "Plane";
    }
}
