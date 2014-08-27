using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mogre;
using NewtMath.d;

namespace OrbMec2
{
    public class MogreGraphicsEngine
    {
        private Root mRoot;
        private RenderWindow mRenderWindow;
        private SceneManager mSceneMgr;
        private Viewport mViewport;
        protected SceneNode mNode_lines;
        private MOIS.Keyboard mKeyboard;
        private MOIS.Mouse mMouse;

        protected Camera mCamera;
        protected CameraMan mCameraMan;
        double distanceScailer = 25e4f;
        double bodySizeScailer = 25e3f;
        //Dictionary<string, SpaceObject> SpaceObjectsDict;
        GameEngine GameEng;

        public MogreGraphicsEngine(Dictionary<string, SpaceObject> spaceObjectsDict, List<GraphicObject> grapicObjects, GameEngine gameEngine)
		{

            //SpaceObjectsDict = spaceObjectsDict;
            GameEng = gameEngine;
			try
			{
				CreateRoot();
				DefineResources();
				CreateRenderSystem();
				CreateRenderWindow();
				InitializeResources();


				CreateScene();
				InitializeInput();
				CreateFrameListeners();

                Console.WriteLine("Setting up Rendering Entities");
                foreach (GraphicObject gObj in grapicObjects)
                {
                    CreateNewEntity(spaceObjectsDict[gObj.IDName], gObj);
                    Console.Write(".");
                }
                Console.WriteLine("Done");

			}
			catch (OperationCanceledException) { }
            EnterRenderLoop();

		}

        private void CreateNewEntity(SpaceObject sObj, GraphicObject gObj)
        {
            
            try
            {

                Entity objEnt = mSceneMgr.CreateEntity(gObj.IDName, gObj.MeshName);
              
                objEnt.CastShadows = true;

                Vector3 scaledsize = new Vector3((float)(gObj.MeshSize / bodySizeScailer), (float)(gObj.MeshSize / bodySizeScailer), (float)(gObj.MeshSize / bodySizeScailer));

                SceneNode objNode = mSceneMgr.RootSceneNode.CreateChildSceneNode(gObj.IDName);
                objNode.AttachObject(objEnt);
                objNode.Scale(scaledsize);

                Quaternion quat = new Quaternion(gObj.MeshAxis[0], gObj.MeshAxis[1], gObj.MeshAxis[2], gObj.MeshAxis[3]);
                objNode.Orientation = quat;

                Vector3 scaledpos = TranslateMogrePhys.smVector_mVector3(sObj.Position);
                scaledpos.x = (float)(scaledpos.x / distanceScailer);
                scaledpos.y = (float)(scaledpos.y / distanceScailer);
                scaledpos.z = (float)(scaledpos.z / distanceScailer);
                objNode.Position = scaledpos;

                SceneNode objNode_Orbitline = mSceneMgr.RootSceneNode.CreateChildSceneNode(gObj.IDName + "_OrbitLine");

                ManualObject orbitline = mSceneMgr.CreateManualObject(gObj.IDName + "_OrbitLine");
                ManualObject forceline = mSceneMgr.CreateManualObject(gObj.IDName + "_ForceLine");
                ManualObject accelline = mSceneMgr.CreateManualObject(gObj.IDName + "_AccelLine");
                mNode_lines.AttachObject(orbitline);
                mNode_lines.AttachObject(forceline);
                mNode_lines.AttachObject(accelline);

                Console.WriteLine("OgreEntity Created: " + gObj.IDName);

            }
            catch (Exception ex)
            {
                Console.WriteLine("OgreEntity creation failed for " + gObj.IDName);
                Console.Error.WriteLine(ex);
            }

        }

 

        #region mogresetup

        protected virtual void CreateCamera()
        {
            mCamera = mSceneMgr.CreateCamera("PlayerCam");
            mCamera.Position = new Vector3(0, 0, 1000000f);
            mCamera.LookAt(Vector3.ZERO);
            mCamera.NearClipDistance = 1;
            mCamera.FarClipDistance = float.MaxValue;
            mCameraMan = new CameraMan(mCamera);
        }

        private void CreateRoot()
        {
            mRoot = new Root(); //can change location/name of plugins.cfg ogre.cfg and Ogre.log files here using the Root parameters. 
        }

        private void DefineResources()
        {
            ConfigFile cf = new ConfigFile();
            cf.Load("resources.cfg", "\t:=", true);

            var section = cf.GetSectionIterator();
            while (section.MoveNext())
            {
                foreach (var line in section.Current)
                {
                    ResourceGroupManager.Singleton.AddResourceLocation(
                        line.Value, line.Key, section.CurrentKey);
                }
            }
        }

        private void CreateRenderSystem()
        {
            if (!mRoot.ShowConfigDialog())
                throw new OperationCanceledException();
        }

        private void CreateRenderWindow()
        {
            mRenderWindow = mRoot.Initialise(true, "OrbMec2");
        }

        private void InitializeResources()
        {
            TextureManager.Singleton.DefaultNumMipmaps = 5;
            ResourceGroupManager.Singleton.InitialiseAllResourceGroups();
        }

        protected void InitializeInput()
        {

            int windowHnd;
            mRenderWindow.GetCustomAttribute("WINDOW", out windowHnd);
            var inputMgr = MOIS.InputManager.CreateInputSystem((uint)windowHnd);

            //buffered input
            mKeyboard = (MOIS.Keyboard)inputMgr.CreateInputObject(MOIS.Type.OISKeyboard, true);
            mMouse = (MOIS.Mouse)inputMgr.CreateInputObject(MOIS.Type.OISMouse, true);

            mKeyboard.KeyPressed += new MOIS.KeyListener.KeyPressedHandler(OnKeyPressed);
            mKeyboard.KeyReleased += new MOIS.KeyListener.KeyReleasedHandler(OnKeyReleased);
            mMouse.MouseMoved += new MOIS.MouseListener.MouseMovedHandler(OnMouseMoved);
            mMouse.MousePressed += new MOIS.MouseListener.MousePressedHandler(OnMousePressed);
            mMouse.MouseReleased += new MOIS.MouseListener.MouseReleasedHandler(OnMouseReleased);
        }

        private void CreateScene()
        {
            mSceneMgr = mRoot.CreateSceneManager(SceneType.ST_GENERIC);

            CreateCamera();
            mViewport = mRenderWindow.AddViewport(mCamera);
            mViewport.BackgroundColour = ColourValue.Black;
            mCamera.AspectRatio = (float)mViewport.ActualWidth / mViewport.ActualHeight;


            String resourceGroupName = "lines";
            if (ResourceGroupManager.Singleton.ResourceGroupExists(resourceGroupName) == false)
                ResourceGroupManager.Singleton.CreateResourceGroup(resourceGroupName);

            MaterialPtr moMaterialblue = MaterialManager.Singleton.Create("line_blue", resourceGroupName);
            moMaterialblue.ReceiveShadows = false;
            moMaterialblue.GetTechnique(0).SetLightingEnabled(true);
            moMaterialblue.GetTechnique(0).GetPass(0).SetDiffuse(0, 0, 1, 0);
            moMaterialblue.GetTechnique(0).GetPass(0).SetAmbient(0, 0, 1);
            moMaterialblue.GetTechnique(0).GetPass(0).SetSelfIllumination(0, 0, 1);
            moMaterialblue.Dispose();  // dispose pointer, not the material
            MaterialPtr moMaterialred = MaterialManager.Singleton.Create("line_red", resourceGroupName);
            moMaterialred.ReceiveShadows = false;
            moMaterialred.GetTechnique(0).SetLightingEnabled(true);
            moMaterialred.GetTechnique(0).GetPass(0).SetDiffuse(1, 0, 0, 0);
            moMaterialred.GetTechnique(0).GetPass(0).SetAmbient(1, 0, 0);
            moMaterialred.GetTechnique(0).GetPass(0).SetSelfIllumination(1, 0, 0);
            moMaterialred.Dispose();  // dispose pointer, not the material
            MaterialPtr moMaterialgreen = MaterialManager.Singleton.Create("line_green", resourceGroupName);
            moMaterialgreen.ReceiveShadows = false;
            moMaterialgreen.GetTechnique(0).SetLightingEnabled(true);
            moMaterialgreen.GetTechnique(0).GetPass(0).SetDiffuse(0, 1, 0, 0);
            moMaterialgreen.GetTechnique(0).GetPass(0).SetAmbient(0, 1, 0);
            moMaterialgreen.GetTechnique(0).GetPass(0).SetSelfIllumination(0, 1, 0);
            moMaterialgreen.Dispose();  // dispose pointer, not the material
            MaterialPtr moMaterialpurple = MaterialManager.Singleton.Create("line_purple", resourceGroupName);
            moMaterialpurple.ReceiveShadows = false;
            moMaterialpurple.GetTechnique(0).SetLightingEnabled(true);
            moMaterialpurple.GetTechnique(0).GetPass(0).SetDiffuse(1, 0, 1, 0);
            moMaterialpurple.GetTechnique(0).GetPass(0).SetAmbient(1, 0, 1);
            moMaterialpurple.GetTechnique(0).GetPass(0).SetSelfIllumination(1, 0, 1);
            moMaterialpurple.Dispose();  // dispose pointer, not the material
            MaterialPtr moMaterialyellow = MaterialManager.Singleton.Create("line_yellow", resourceGroupName);
            moMaterialyellow.ReceiveShadows = false;
            moMaterialyellow.GetTechnique(0).SetLightingEnabled(true);
            moMaterialyellow.GetTechnique(0).GetPass(0).SetDiffuse(1, 1, 0, 0);
            moMaterialyellow.GetTechnique(0).GetPass(0).SetAmbient(1, 1, 0);
            moMaterialyellow.GetTechnique(0).GetPass(0).SetSelfIllumination(1, 1, 0);
            moMaterialyellow.Dispose();  // dispose pointer, not the material
            MaterialPtr moMaterialcyan = MaterialManager.Singleton.Create("line_cyan", resourceGroupName);
            moMaterialcyan.ReceiveShadows = false;
            moMaterialcyan.GetTechnique(0).SetLightingEnabled(true);
            moMaterialcyan.GetTechnique(0).GetPass(0).SetDiffuse(0, 1, 1, 0);
            moMaterialcyan.GetTechnique(0).GetPass(0).SetAmbient(0, 1, 1);
            moMaterialcyan.GetTechnique(0).GetPass(0).SetSelfIllumination(0, 1, 1);
            moMaterialcyan.Dispose();  // dispose pointer, not the material

            mNode_lines = mSceneMgr.RootSceneNode.CreateChildSceneNode("scenenode_orbline", Vector3.ZERO);


            Light l = mSceneMgr.CreateLight("MainLight");
            l.Position = new Vector3(0, 0, 5000);

            //ParticleSystem explosionParticle = mSceneMgr.CreateParticleSystem("Explosion", "Explosion");


            //SceneNode particleNode = mSceneMgr.RootSceneNode.CreateChildSceneNode("Particle");
            //particleNode.AttachObject(explosionParticle);

        }

        private void CreateFrameListeners()
        {
            mRoot.FrameRenderingQueued += new FrameListener.FrameRenderingQueuedHandler(ProcessBufferedInput);
            mRoot.FrameStarted += new FrameListener.FrameStartedHandler(DoGraphicsUpdate);
        }

        private bool ProcessBufferedInput(FrameEvent evt)
        {
            //mTimer -= evt.timeSinceLastFrame;
            //return (mTimer > 0);
            mKeyboard.Capture();
            mMouse.Capture();
            mCameraMan.UpdateCamera(evt.timeSinceLastFrame);
            return true;
        }

        private void EnterRenderLoop()
        {
            if (mRoot != null)
                mRoot.StartRendering();
        }

        protected void Shutdown()
        {
            if (mRoot != null)
                mRoot.Dispose();
            //throw new ShutdownException();
        }

        #region input control
        protected bool OnKeyPressed(MOIS.KeyEvent evt)
        {
            switch (evt.key)
            {
                case MOIS.KeyCode.KC_W:
                case MOIS.KeyCode.KC_UP:
                    //shiplist[0].Thrusting = 1;
                    //mCameraMan.GoingForward = true;
                    mCameraMan.GoingUp = true;
                    break;

                case MOIS.KeyCode.KC_S:
                case MOIS.KeyCode.KC_DOWN:
                    //shiplist[0].Thrusting = -1;
                    //mCameraMan.GoingBack = true;
                    mCameraMan.GoingDown = true;
                    break;

                case MOIS.KeyCode.KC_A:
                case MOIS.KeyCode.KC_LEFT:
                    //shiplist[0].Strafing = -1;
                    mCameraMan.GoingLeft = true;
                    break;

                case MOIS.KeyCode.KC_D:
                case MOIS.KeyCode.KC_RIGHT:
                    //shiplist[0].Strafing = 1;
                    mCameraMan.GoingRight = true;
                    break;

                case MOIS.KeyCode.KC_E:
                    break;

                case MOIS.KeyCode.KC_PGUP:
                    GameEng.PhysicsEngine.Ticklen_ms *= 2f;
                    Console.Out.WriteLine("Ticklen = " + GameEng.PhysicsEngine.Ticklen_ms);
                    break;

                case MOIS.KeyCode.KC_Q:
                    break;

                case MOIS.KeyCode.KC_PGDOWN:
                    GameEng.PhysicsEngine.Ticklen_ms *= 0.5f;
                    Console.Out.WriteLine("Ticklen = " + GameEng.PhysicsEngine.Ticklen_ms);
                    break;
                
                case MOIS.KeyCode.KC_HOME:
                    distanceScailer *= 0.5f;
                    break;
                case MOIS.KeyCode.KC_END:
                    distanceScailer *= 2f;
                    break;

                case MOIS.KeyCode.KC_SPACE:
                    //Console.Out.WriteLine("space was pushed.");
                    break;

                case MOIS.KeyCode.KC_LSHIFT:
                case MOIS.KeyCode.KC_RSHIFT:
                    mCameraMan.FastMove = true;
                    break;

                case MOIS.KeyCode.KC_LBRACKET:
                    //selectPrev();
                    break;

                case MOIS.KeyCode.KC_RBRACKET:
                    //selectNext();
                    break;

                case MOIS.KeyCode.KC_T:
                    //CycleTextureFilteringMode();
                    break;

                case MOIS.KeyCode.KC_R:
                    //CyclePolygonMode();
                    break;

                case MOIS.KeyCode.KC_F5:
                    //ReloadAllTextures();
                    break;

                case MOIS.KeyCode.KC_SYSRQ:
                    //TakeScreenshot();
                    break;

                case MOIS.KeyCode.KC_ESCAPE:
                    Shutdown();
                    break;
            }

            return true;
        }

        protected bool OnKeyReleased(MOIS.KeyEvent evt)
        {
            switch (evt.key)
            {
                case MOIS.KeyCode.KC_W:
                case MOIS.KeyCode.KC_UP:

                    //mCameraMan.GoingForward = false;
                    mCameraMan.GoingUp = false;
                    break;

                case MOIS.KeyCode.KC_S:
                case MOIS.KeyCode.KC_DOWN:

                    //mCameraMan.GoingBack = false;
                    mCameraMan.GoingDown = false;
                    break;

                case MOIS.KeyCode.KC_A:
                case MOIS.KeyCode.KC_LEFT:

                    mCameraMan.GoingLeft = false;
                    break;

                case MOIS.KeyCode.KC_D:
                case MOIS.KeyCode.KC_RIGHT:

                    mCameraMan.GoingRight = false;
                    break;

                case MOIS.KeyCode.KC_E:
                case MOIS.KeyCode.KC_PGUP:

                    mCameraMan.GoingUp = false;
                    break;

                case MOIS.KeyCode.KC_Q:
                case MOIS.KeyCode.KC_PGDOWN:

                    mCameraMan.GoingDown = false;
                    break;

                case MOIS.KeyCode.KC_LSHIFT:
                case MOIS.KeyCode.KC_RSHIFT:
                    mCameraMan.FastMove = false;
                    break;
            }

            return true;
        }

        protected virtual bool OnMouseMoved(MOIS.MouseEvent evt)
        {
            if (mCameraMan.MouseLook == true)
            {
                mCameraMan.MouseMovement(evt.state.X.rel, evt.state.Y.rel, evt.state.Z.rel);
            }
            else
                mCameraMan.MouseMovement(0, 0, evt.state.Z.rel);
            return true;
        }

        protected virtual bool OnMousePressed(MOIS.MouseEvent evt, MOIS.MouseButtonID id)
        {
            if (id == MOIS.MouseButtonID.MB_Right)
            {
                mCameraMan.MouseLook = true;
            }
            else if (id == MOIS.MouseButtonID.MB_Left)
            {

            }
            return true;
        }

        protected virtual bool OnMouseReleased(MOIS.MouseEvent evt, MOIS.MouseButtonID id)
        {
            if (id == MOIS.MouseButtonID.MB_Right)
            {
                mCameraMan.MouseLook = false;
            }
            return true;
        }

        #endregion


        public bool DoGraphicsUpdate(FrameEvent evt)
        {
            foreach(SpaceObject so in GameEng.PhysicsEngine.SpaceObjects)
            {
                
                SceneNode soNode = mSceneMgr.GetSceneNode(so.IDName);
                SceneNode orbitLineNode = mSceneMgr.GetSceneNode(so.IDName + "_OrbitLine");
                //Vector3 mvector3Position = (TranslateMogrePhys.smVector_mVector3(so.Position));
                Vector3 mvector3Position = new Vector3((float)(so.Position.X / distanceScailer), (float)(so.Position.Y / distanceScailer), (float)(so.Position.Z / distanceScailer));
                soNode.Position = mvector3Position;
                //Vector3 rotateaxis = new Vector3(0f, 0f, -1f);
                //Quaternion quat = new Quaternion((float)(so.Heading), rotateaxis);
                //node.Orientation = quat;
                //Vector3 mvector3OrblinePosition = TranslateMogrePhys.smVector_mVector3(so.OrbLinePoint);
                Vector3 mvector3OrblinePosition = new Vector3((float)(so.OrbLinePoint.X / distanceScailer), (float)(so.OrbLinePoint.Y / distanceScailer), (float)(so.OrbLinePoint.Z / distanceScailer));
                //mvector3OrblinePosition *= distanceScailer;
                if (Trig.distance(so.Position, so.OrbLinePoint) > 100000 / distanceScailer) 
                {
                    //Vector3 scaledOrbPoint = new Vector3((float)(so.OrbLinePoint.X / scaleing_factor), (float)(so.OrbLinePoint.Y / scaleing_factor), (float)(so.OrbLinePoint.Z / scaleing_factor));
                    ManualObject manObj = mSceneMgr.GetManualObject(so.IDName + "_OrbitLine");
                    manObj.Begin("line_green", RenderOperation.OperationTypes.OT_LINE_LIST);
                    manObj.Position(mvector3OrblinePosition);
                    manObj.Position(mvector3Position);
                    manObj.End();
                    so.OrbLinePoint = so.Position;
                }

                if (so.IDName == "Earth")
                {
                    Console.Out.Write("\r Earth " + so.Position + " distanceScale: " + distanceScailer);
                }
                
                Vector3 scaledforcevec = new Vector3((float)(so.getForce.X), (float)(so.getForce.Y), (float)(so.getForce.Z));
                
                Vector3 scaledaccelvec = new Vector3((float)(so.AccelMetersSecond.X), (float)(so.AccelMetersSecond.Y), (float)(so.AccelMetersSecond.Z));
                scaledforcevec *= 0.000000000000000001f;
                scaledaccelvec *= 1e5f;

                //ManualObject forceline = mSceneMgr.GetManualObject(so.IDName + "_ForceLine");
                mSceneMgr.DestroyManualObject(so.IDName + "_ForceLine");
                ManualObject forceline = mSceneMgr.CreateManualObject(so.IDName + "_ForceLine");
                mNode_lines.AttachObject(forceline);
                forceline.Begin("line_blue", RenderOperation.OperationTypes.OT_LINE_LIST);
                forceline.Position(mvector3Position);
                forceline.Position((mvector3Position) + (scaledforcevec));
                forceline.End();

                //ManualObject accelline = mSceneMgr.GetManualObject(so.IDName + "_AccelLine");
                mSceneMgr.DestroyManualObject(so.IDName + "_AccelLine");
                ManualObject accelline = mSceneMgr.CreateManualObject(so.IDName + "_AccelLine");
                mNode_lines.AttachObject(accelline);
                accelline.Begin("line_red", RenderOperation.OperationTypes.OT_LINE_LIST);
                accelline.Position(mvector3Position);
                accelline.Position(mvector3Position + (scaledaccelvec));
                accelline.End();
                
            }

            return true;
        }

        #endregion
    }

    public static class TranslateMogrePhys
    {
        
        public static Mogre.Vector3 smVector_mVector3(PointXd smVector)
        {
            return new Mogre.Vector3((float)smVector.X, (float)smVector.Y, (float)smVector.Z);
        }
        public static Mogre.Vector2 smVector_mVector2(PointXd smVector)
        {
            return new Mogre.Vector2((float)smVector.X, (float)smVector.Y);
        }
        public static Mogre.Vector3 smVector2_mVector3(PointXd smVector)
        {
            return new Mogre.Vector3((float)smVector.X, (float)smVector.Y, 0);
        }
        public static PointXd mVector3_smVector3(Mogre.Vector3 mVector)
        {
            return new PointXd(mVector.x, mVector.y, mVector.z);
        }
    }
}
