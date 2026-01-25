using System.Collections.Generic;

namespace Oqtane.Modules.Visualization3D.Models
{
    /// <summary>
    /// Represents a 3D scene containing objects, camera, and lights
    /// </summary>
    public class Scene
    {
        public List<Object3D> Objects { get; set; }
        public Camera Camera { get; set; }
        public Color BackgroundColor { get; set; }

        public Scene()
        {
            Objects = new List<Object3D>();
            Camera = new Camera();
            BackgroundColor = Color.Black;
        }

        public void Add(Object3D obj)
        {
            Objects.Add(obj);
        }

        public void Remove(Object3D obj)
        {
            Objects.Remove(obj);
        }

        public void Clear()
        {
            Objects.Clear();
        }
    }
}
