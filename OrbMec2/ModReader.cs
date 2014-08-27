using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewtMath.d;
namespace OrbMec2
{
    public class ModReader
    {
        public static List<SpaceObject> ReadSpaceObjects(string folder = "DefaultMod")
        {
            folder = "Mods\\" + folder;
            List<SpaceObjectCereal> spaceObjectCereals = new List<SpaceObjectCereal>();
            string filestring = System.IO.File.ReadAllText(folder + "\\System.cfg");
            spaceObjectCereals = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SpaceObjectCereal>>(filestring);

            List<SpaceObject> spaceObjects = new List<SpaceObject>();
            foreach (SpaceObjectCereal soc in spaceObjectCereals)
            {
                SpaceObject so = new SpaceObject(soc);
                spaceObjects.Add(so);
            }
            return spaceObjects;
        }

        public static List<GraphicObject> ReadGraphicObjects(string folder = "DefaultMod")
        {
            folder = "Mods\\" + folder;
            List<GraphicObject> graphicObjects = new List<GraphicObject>();
            string filestring = System.IO.File.ReadAllText(folder + "\\SystemGraphics.cfg");
            graphicObjects = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GraphicObject>>(filestring);

            return graphicObjects;
        }   
    }

    public class SpaceObjectCereal
    {
        public string IDName { get; set; }
        public double Mass { get; set; }
        public double[] Position { get; set; }
        public double[] VelocityMetersSecond { get; set; }
    }

    public class GraphicObject
    {
        public string IDName { get; set; }
        public string MeshName { get; set; }
        public double MeshSize { get; set; }
        public float[] MeshAxis { get; set; }
    }
}
