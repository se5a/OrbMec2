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
            int index = 0;
            if (Threaded)
            {
                List<Task> threadedGravEffectTasks = new List<Task>();
                foreach (SpaceObject so in SpaceObjects)
                {
                    //update grav forces in parallel
                    threadedGravEffectTasks.Add(Task.Factory.StartNew(() => so.GravEffect(SpaceObjects, index)));
                    index += 1;
                }
                Task.WaitAll(threadedGravEffectTasks.ToArray()); //wait till all the grav effects calcs have been done. 
                //now grav forces have been calulated we can figure out where the objecs are going to be. 
                List<Task> threadedMoveTasks = new List<Task>();
                foreach (SpaceObject so in SpaceObjects)
                {
                    threadedMoveTasks.Add(Task.Factory.StartNew(() => so.Move(simticklen_s)));
                }
                Task.WaitAll(threadedMoveTasks.ToArray());
            }
            else //linier non threaded.
            { 
                foreach (SpaceObject so in SpaceObjects)
                {
                    //update grav forces.
                    so.GravEffect(SpaceObjects, index);
                    index += 1;
                }

                //now grav forces have been calulated we can figure out where the objecs are going to be. 
                foreach (SpaceObject so in SpaceObjects)
                {
                    so.Move(simticklen_s);//physics move
                }
            }
        }
    }
}
