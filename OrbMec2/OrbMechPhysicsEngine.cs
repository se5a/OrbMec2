using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbMec2
{
    public class OrbMecPysicsEngine
    {
        System.Diagnostics.Stopwatch PhysicssTimer = new System.Diagnostics.Stopwatch();

        public List<SpaceObject> SpaceObjects { get; set; }

        public bool Threaded { get; set; }

        public bool Running { get; set; }

        /// <summary>
        /// physics frame rate
        /// </summary>
        public float Ticklen_ms { get; set; }
        /// <summary>
        /// length of tick
        /// </summary>
        public int Simticklen_s { get; set; }
       
        public OrbMecPysicsEngine()
        {
            Ticklen_ms = 0;
            Simticklen_s = 1;
            Threaded = true;
            
        }

        public void Run()
        {
            System.Diagnostics.Stopwatch PhysicssTimer2 = new System.Diagnostics.Stopwatch();
            PhysicssTimer.Start();
            Running = true;

            while(Running)
            {
                if (PhysicssTimer.ElapsedMilliseconds >= Ticklen_ms)
                {
                    //PhysicssTimer2.Start();
                    DoPhysics(Simticklen_s);                    
                    PhysicssTimer.Restart();
                    //long physics_timetaken = PhysicssTimer2.ElapsedMilliseconds;
                    //Console.Out.WriteLine("Physics Objects = " + SpaceObjects.Count + " and took " + physics_timetaken.ToString() + "ms.");
                    //PhysicssTimer2.Reset();
                }

            }
            Console.Out.WriteLine("Physics Stopped Running");
        }

        private void DoPhysics(int simticklen_s)
        {         
            //int threads = 8;
            //int index = 0;
            int count = SpaceObjects.Count;
            if (Threaded)
            {

                Task[] threadedTasks = new Task[count-1];
                for (int i = 0; i < count-1; i++)
                {
                    //update grav forces in parallel
                    threadedTasks[i] = (Task.Factory.StartNew(() => SpaceObjects[i].GravEffect(SpaceObjects, i)));
                }
                Task.WaitAll(threadedTasks); //wait till all the grav effects calcs have been done.               
                //now grav forces have been calulated we can figure out where the objecs are going to be. 

                for (int i = 0; i < count-1; i++)
                {
                    threadedTasks[i] = (Task.Factory.StartNew(() => SpaceObjects[i].Move(simticklen_s)));
                }
                Task.WaitAll(threadedTasks);
            }
            else //linier non threaded.
            {
                for (int i = 0; i < count-1; i++)
                {
                    //update grav forces.
                    SpaceObjects[i].GravEffect(SpaceObjects, i);
               
                }

                //now grav forces have been calulated we can figure out where the objecs are going to be. 
                for (int i = 0; i < count - 1; i++)
                {
                    SpaceObjects[i].Move(simticklen_s);//physics move
                }
            }
        }
    }
}
