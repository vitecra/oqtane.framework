namespace Oqtane.Modules.Visualization3D.Models
{
    /// <summary>
    /// Represents a 3D camera for viewing the scene
    /// </summary>
    public class Camera
    {
        public Vector3 Position { get; set; }
        public Vector3 Target { get; set; }
        public Vector3 Up { get; set; }
        public double FieldOfView { get; set; }
        public double AspectRatio { get; set; }
        public double Near { get; set; }
        public double Far { get; set; }

        public Camera()
        {
            Position = new Vector3(0, 0, 5);
            Target = Vector3.Zero;
            Up = Vector3.Up;
            FieldOfView = 45.0;
            AspectRatio = 1.0;
            Near = 0.1;
            Far = 1000.0;
        }

        public void LookAt(Vector3 target)
        {
            Target = target;
        }
    }
}
