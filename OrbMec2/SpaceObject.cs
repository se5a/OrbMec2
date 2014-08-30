using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewtMath.d;

namespace OrbMec2
{
    public class SpaceObject
    {
        public string IDName { get; private set; }
        public double Mass { get; private set; }
        public PointXd Position { get; private set; }
        public PointXd VelocityMetersSecond { get; private set; }
        public PointXd AccelMetersSecond { get; private set; }

        protected PointXd ForceNewtons { get; set; }
        private PointXd SavedForce;
        public PointXd OrbLinePoint { get; set; }

        public SpaceObject(string ID, double startMass, PointXd startPosition, PointXd startVelocity)
        {
            IDName = ID;
            Mass = startMass;
            Position = startPosition;
            VelocityMetersSecond = startVelocity;
            AccelMetersSecond = new PointXd(0, 0, 0);
            ForceNewtons = new PointXd(0, 0, 0);
            OrbLinePoint = new PointXd(startPosition);
        }
        public SpaceObject(string ID, double startMass, PointXd startPosition, PointXd startVelocity, PointXd startaccelvec )
        {
            IDName = ID;
            Mass = startMass;
            Position = startPosition;
            VelocityMetersSecond = startVelocity;
            AccelMetersSecond = startaccelvec;
            ForceNewtons = new PointXd(0, 0, 0);
        }

        public SpaceObject(SpaceObjectCereal breakfast)
        {
            IDName = breakfast.IDName;
            Mass = breakfast.Mass;
            Position = new PointXd(breakfast.Position);
            VelocityMetersSecond = new PointXd(breakfast.VelocityMetersSecond);
            AccelMetersSecond = new PointXd(0, 0, 0);
            ForceNewtons = new PointXd(0, 0, 0);
            SavedForce = new PointXd(0, 0, 0);
            OrbLinePoint = new PointXd(breakfast.Position);
        }

        public PointXd getForce { get { return SavedForce; } }

        /// <summary>
        /// Calculates the force between obj at myindex and all other objects in spaceobjs. 
        /// it then updates the ForceNewtons on both this, and the other objects.
        /// </summary>
        /// <param name="spaceobjs"></param>
        /// <param name="myindex"></param>
        public void GravEffect(List<SpaceObject> spaceobjs, int myindex)
        {
            //Vector3d tempaccelvec = new Vector3d();
            PointXd tempforcevec = new PointXd();
            int index = spaceobjs.Count - 1;
            while (index > myindex)
            {
                SpaceObject otherSO = spaceobjs[index];
                double distance = Trig.distance(Position, otherSO.Position);
                double force = NMath.gravForce(Mass, otherSO.Mass, distance);

                PointXd myforcevec = Trig.intermediatePoint(Position, otherSO.Position, force);
                PointXd otherforcevec = new PointXd(myforcevec.X * -1, myforcevec.Y * -1, myforcevec.Z * -1);

                lock (otherSO.ForceNewtons)
                {
                    otherSO.ForceNewtons += otherforcevec;
                }


                //tempaccelvec.add(myaccelvec);
                tempforcevec += myforcevec;
                // *trying to think threadsafe here, 
                //not really sure what I'm doing, but my thinking is I'm less likely to be 
                //trying to update the actual forcevec while others threads may be 
                //attempting to do so at the same time this way. 

                index -= 1; //counting towards zero
            }
            lock (ForceNewtons)
            {
                ForceNewtons += tempforcevec; // see * above
            }
        }

        /// <summary>
        /// This should be called only after all forces have been added up (ie GravEffect)
        /// </summary>
        /// <param name="secondsThisTick"></param>
        public void Move(int secondsThisTick)
        {

            string id = this.IDName;

            AccelMetersSecond = NMath.accelVector(Mass, ForceNewtons);

            PointXd movethistick_fromvelocity = new PointXd(VelocityMetersSecond);
            VelocityMetersSecond += (AccelMetersSecond * secondsThisTick);

            movethistick_fromvelocity *= secondsThisTick; //displacement = velocity * time

            PointXd movethistick_fromacceleration = new PointXd(AccelMetersSecond);
            movethistick_fromacceleration *= System.Math.Pow(secondsThisTick, 2) * 0.5; //displacement = acceleration * time^2 / 2

            Position += movethistick_fromacceleration + movethistick_fromvelocity;
            SavedForce = new PointXd(ForceNewtons);
            ForceNewtons.ZEROIZE();
        }
    }
}
