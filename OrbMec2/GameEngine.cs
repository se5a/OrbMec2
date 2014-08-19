using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace OrbMec2
{
    public class GameEngine
    {
        public OrbMecPysicsEngine PhysicsEngine { get; set; }
        MogreGraphicsEngine graphicsEngine;
        List<SpaceObject> SpaceObjects;
        Dictionary<string, SpaceObject> SpaceObjectsDict;
        List<GraphicObject> GraphicObjects;
        public bool runningGraphics { get; set; }
        public GameEngine(List<SpaceObject> spaceObjects, List<GraphicObject> graphicObjects)
        {
            SpaceObjects = spaceObjects;
            GraphicObjects = graphicObjects;
            SpaceObjectsDict = new Dictionary<string, SpaceObject>();
            foreach (SpaceObject so in SpaceObjects)
            {
                SpaceObjectsDict.Add(so.IDName, so);
            }
            PhysicsEngine = new OrbMecPysicsEngine();
            PhysicsEngine.SpaceObjects = SpaceObjects;
            PhysicsEngine.Threaded = false;
            
            Thread physicsThread = new Thread(new ThreadStart(PhysicsEngine.Run));
            //Thread graphicsThread = new Thread(new ThreadStart(startGraphics));

            //graphicsThread.Start();
            physicsThread.Name = "PhysicsMainThread";
            physicsThread.Start();
            //PhysicsEngine.Run();
            startGraphics();

        }

        private void startGraphics()
        {
            runningGraphics = true;
            graphicsEngine = new MogreGraphicsEngine(SpaceObjectsDict, GraphicObjects, this);
        }
    }
}
