using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbMec2
{
    class Program
    {
        static void Main(string[] args)
        {
            
            List<SpaceObject> spaceobjects = ModReader.ReadSpaceObjects();
            List<GraphicObject> graphicobjects = ModReader.ReadGraphicObjects();
            GameEngine game = new GameEngine(spaceobjects, graphicobjects);
        }
    }
}
