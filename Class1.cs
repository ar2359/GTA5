/*
Authors: Artur Filipowicz, Jeremiah Liu
Code used for research for TRB in Summer 2016 
Version: 0.9
Copyright: 2016
MIT License
*/

using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using GTA;
using GTA.Native;
using GTA.Math;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Linq;

public class StopObjDataGen : Script
{
    

    bool draw = true;
    bool pauseGame = false;
    bool enabled = false;
    bool enabledTraffic = false;

    const int IMAGE_HEIGHT = 600;
    const int IMAGE_WIDTH = 800;
    float min = 99999f;
    int c = 0;

    bool lkey=false;
    bool strt = false;

    List<Vector3> pointsToDraw = new List<Vector3>();
    List<Vector3> pointsToDrawBlue = new List<Vector3>();

    Vector3 startLineLeft;
    Vector3 startLineRight;
    Vector3 endLineLeft;
    Vector3 endLineRight;
    Vector3 stopObjSpawnLoc;
    string stopObjType;
    bool hasStopObjContext;

    Vector3 rayEnd;

    Prop stopObj;

    string pathFileDir = "C:\\Users\\anirudhsr\\Desktop\\GTA Paths\\GTApaths4.txt"; //path of the start and stop points
    //string pathFileDir = "C:\\Users\\anirudhsr\\Desktop\\GTA Paths\\paths.txt";
    StreamReader pathFile;
    string stopObjSOIDir = "C:\\Users\\anirudhsr\\Desktop\\StopObj\\StopObject.txt"; //path of the stop signs

    int trialNum = 0;
    bool doneWithTrials = false;
    int weatherForTrial = 0;
    bool record = false;
    //bool record = true;

    int frame = 0;

    bool flag = false;

    string dataDir = "F:\\GTAImages8\\"; //destination path of the images
    string pathDataDir;
    string fmtFrame = "00000000";
    string fmtTrack = "000";

    //StreamWriter file;

    Vector3 startPos;
    Vector3 endPos;

    bool noStopObjTrail = false;
    //bool noStopObjTrail = true;


    int track = 0;
    int sc = 0;

    /*
 	Stop object types
 	N - no stop object 
 	R - red light 
 	Y - yellow light 
 	X - rail road crossing
 	S - stop sign 
 	*/
    string objectType = "S";

    /*
 	Weather types
 	E - extra sunny 
 	R - raining 
 	O - overcast 
 	F - foggy 
 	T - thunder 
 	C - cloudy 
 	*/
    string weather = "";

    float distToObj = 40.0f;
    float maxDistToObj = 40.0f;

    float minDistToObj = 17.0f;

    private struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("C:\\Windows\\System32\\user32.dll")]
    private static extern IntPtr GetForegroundWindow();
    [DllImport("C:\\Windows\\System32\\user32.dll")]
    private static extern IntPtr GetClientRect(IntPtr hWnd, ref Rect rect);
    [DllImport("C:\\Windows\\System32\\user32.dll")]
    private static extern IntPtr ClientToScreen(IntPtr hWnd, ref Point point);

    public struct SOI
    {
        public int modelHash;
        public string name;
        public Vector3 FUL;
        public Vector3 BLR;

        public override bool Equals(object ob)
        {
            if (ob is SOI)
            {
                SOI c = (SOI)ob;
                return c.modelHash == this.modelHash;
            }
            else
            {
                return false;
            }
        }
    }

    List<SOI> stopObjSOI = new List<SOI>();


    public StopObjDataGen()
    {
        UI.Notify("Loaded StopObjDataGen1.cs");
        try
        {
            stopObjSOI = loadSOI(stopObjSOIDir);

            //pathFile = new StreamReader(pathFileDir);
        }
        catch (Exception)
        {
            UI.Notify("Stop Sign not loaded");
        }

        // attach time methods 
        Tick += OnTick;
        KeyUp += onKeyUp;
    }

    private void onKeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.NumPad8) //starts trials-recording in .png
        {
            pathFile = new StreamReader(pathFileDir);
            UI.Notify("NumPad8 pressed");
            sc = 0;
            trialNum = 0;
            strt = true;
            //trial = 0;
            readNextPath();

        }

        if (e.KeyCode == Keys.NumPad3) //records in .bmp
        {
            UI.Notify("Numpad3 Pressed");
            sc = 1;
            readNextPath();
        }

        if(e.KeyCode==Keys.L) //loads the last position on the map
        {

            dataDir = "F:\\GTAImages5\\";
            pathDataDir = dataDir + track.ToString(fmtTrack) + "\\";
            string text = File.ReadAllText(@"C:\Users\anirudhsr\Desktop\GTA Paths\last.txt", System.Text.Encoding.UTF8);
            string[] p = text.Split(',');

            lkey = true;

            //str = trialNum.ToString() + "," + (World.CurrentDayTime.Hours).ToString() + "," + weather + "," + stopObjType + "," + stopObjSpawnLoc.X.ToString() + "," + stopObjSpawnLoc.Y.ToString() + "," + stopObjSpawnLoc.Z.ToString() + "," + startPos.X.ToString() + "," + startPos.Y.ToString() + "," + startPos.Z.ToString() + "," + endPos.X.ToString() + "," + endPos.Y.ToString() + "," + endPos.Z.ToString();

            trialNum = int.Parse(p[0]);
            UI.Notify("Starting Trial " + p[0]);
            trialNum++;

            World.CurrentDayTime = TimeSpan.Parse(p[1]);
            weatherForTrial = int.Parse(p[2]);

            
            Wait(2000);

            string stopObjTyp = p[3];

            Vector3 v_stop = new Vector3(float.Parse(p[4]), float.Parse(p[5]), float.Parse(p[6]));
            Vector3 v_start = new Vector3(float.Parse(p[7]), float.Parse(p[8]), float.Parse(p[9]));
            Vector3 v_end = new Vector3(float.Parse(p[10]), float.Parse(p[11]), float.Parse(p[12]));

            startPos = v_start;
            endPos = v_end;
            UI.Notify("Start postion :" + v_start.ToString());
            Game.Player.Character.CurrentVehicle.Position = v_start;
            

            stopObjSpawnLoc = v_stop;
            stopObjType = stopObjTyp;

            deleteStopObjs(v_stop, stopObjTyp, 5f);

            startLineLeft = startPos;
            startLineRight = startPos;

            endLineLeft = endPos;
            endLineRight = endPos;

            spawnStopObj(v_stop, stopObjTyp);
            stopObj.IsVisible = !(bool.Parse(p[14]));
            UI.Notify("End of L");
            sc = 0;
            
            strt = true;
            track = int.Parse(p[13]);
            
            runTrial(trialNum, startPos, endPos);

            //str = trialNum.ToString() + "," + (World.CurrentDayTime.Hours).ToString() + "," + weather;
            //file.WriteLine(str);
            //file.Close();
            //World.CurrentDayTime = new TimeSpan(World.CurrentDayTime.Hours + 1, 0, 0);


        }

        //if(e.KeyCode==Keys.X)
        //{
        //    UI.Notify("X pressed");
        //    getrandpos();

        //}
    }

    void OnTick(object sender, EventArgs e) //ontick is like a while(1) loop
    {

        if (draw)
        {
            int obj = 0;
            //World.DrawMarker(MarkerType.DebugSphere, endLineLeft, new Vector3(0,0,0), new Vector3(0,0,0), new Vector3(.3f,.3f,.3f), Color.Red);
            //World.DrawMarker(MarkerType.DebugSphere, endLineRight, new Vector3(0,0,0), new Vector3(0,0,0), new Vector3(.3f,.3f,.3f), Color.Red);
            //World.DrawMarker(MarkerType.DebugSphere, startLineLeft, new Vector3(0,0,0), new Vector3(0,0,0), new Vector3(.3f,.3f,.3f), Color.Red);
            //World.DrawMarker(MarkerType.DebugSphere, startLineRight, new Vector3(0,0,0), new Vector3(0,0,0), new Vector3(.3f,.3f,.3f), Color.Red);

            foreach (Vector3 v in pointsToDraw)
            {
                World.DrawMarker(MarkerType.DebugSphere, v, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(.05f, .05f, .05f), Color.Red);
            }

            foreach (Vector3 v in pointsToDrawBlue)
            {
                World.DrawMarker(MarkerType.DebugSphere, v, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(.3f, .3f, .3f), Color.Blue);
            }
        }

        if (Game.Player.Character.IsInVehicle())
        {
            Vehicle v = Game.Player.Character.CurrentVehicle;
            Vector3 vehicleFront = v.Position + v.Model.GetDimensions().Y / 2.0f * v.ForwardVector;
            vehicleFront.Z = vehicleFront.Z - (vehicleFront.Z - World.GetGroundHeight(vehicleFront));

            //float distToEnd = Vector3.Distance2D(Vector3.Project(endLineLeft - vehicleFront, endLineLeft - endLineRight), endLineLeft - vehicleFront);
            float distToEnd = Vector3.Distance2D(endLineLeft, vehicleFront);
            distToEnd = distToEnd * 10;

           
            
            //UI.Notify("Going to ontick");
            //if (distToEnd < 2f && !doneWithTrials)
            //if(!doneWithTrials)
            //if(distToEnd<2f)
            //UI.Notify("Dist to End: " + distToEnd.ToString());
            // UI.Notify((endLineLeft - vehicleFront).ToString());



            c = c + 1;

            if (min > distToEnd) // in some cases the trials were getting stuck a particular location beyond the end point. This condition ensures the next trial is started in such a case
            {
                min = distToEnd;
                flag = false;
            }
            else if (min < distToEnd)
            {
                flag = true;
            }

            //UI.Notify(min.ToString()+" " + c.ToString());
            //if (distToEnd > 19f && distToEnd < 21f)
            //    distToEnd = 19f;



            //if (min2 > distToEnd)
            //{
            //    min2 = distToEnd;
            //    flag = false;
            //}
            //else if (min2 < distToEnd)
            //{
            //    flag = true;
            //}


            if ((distToEnd < 20f && !doneWithTrials || flag) && strt)
            {
                //UI.Notify("Inside On Tick");

                record = false;

                noStopObjTrail = !noStopObjTrail;
                if (noStopObjTrail)
                //if(false)
                {
                    //UI.Notify("Stop Object Hidden");

                    hideStopObj();
                    trialNum = trialNum + 1; //move to next trial
                    frame = 0;
                    //UI.Notify("Trial Number:" + trialNum.ToString());
                    runTrial(trialNum, startPos, endPos);

                }
                else
                {
                    //UI.Notify("Stop Object Shown");

                    showStopObj();
                    startPos = getRandLoc(startLineLeft, startLineRight);
                    endPos = getRandLoc(endLineLeft, endLineRight);
                    trialNum = trialNum + 1;
                    frame = 0;
                    //UI.Notify("Trial Number:" + trialNum.ToString());
                    runTrial(trialNum, startPos, endPos);
                }
            }

            if (doneWithTrials == true)
            {
                trialNum = 0;
                doneWithTrials = false;
                weatherForTrial = 0;
                frame = 0;
                //UI.Notify("Done with trials");
                readNextPath();
            }
        }

        if (record)
        {
            record = false;

            if (weatherForTrial == 1)
                weather = "E";
            if (weatherForTrial == 2)
                weather = "F";
            if (weatherForTrial == 3)
                weather = "O";
            if (weatherForTrial == 4)
                weather = "R";
            if (weatherForTrial == 5)
                weather = "T";
            if (weatherForTrial == 6)
                weather = "C";

            //distToObj = Vector3.Distance(Vector3.Project(World.RenderingCamera.Position - endLineLeft, endLineLeft - endLineRight), World.RenderingCamera.Position - endLineLeft);

            //distToObj = Vector3.Distance(World.RenderingCamera.Position, stopObjSpawnLoc);

            Vector3 v=Game.Player.Character.CurrentVehicle.Position;

            distToObj = Vector3.Distance(v, stopObjSpawnLoc);
            
            int c = 0;
            if (hasStopObjContext)
            {
                c = 1;
            }


            //UI.Notify("Recording Mode");
            //if (distToObj > maxDistToObj || !stopObjVisible(stopObj) || objectType == "N" || noStopObjTrail)
            //if ((distToObj > maxDistToObj || objectType == "N" || noStopObjTrail) && !stopObjVisible(stopObj))
            //if(((distToObj>maxDistToObj) && !stopObjVisible(stopObj)) && objectType =="N" && noStopObjTrail && !stopObj.IsVisible)
            //if ((distToObj > maxDistToObj || !stopObjVisible(stopObj) || objectType == "N" || noStopObjTrail) && !stopObj.IsVisible)
            if ((distToObj > maxDistToObj || !stopObj.IsVisible || objectType == "N" || noStopObjTrail || distToObj<minDistToObj))
            {
                //UI.Notify("Recording N type");

                //distToObj = maxDistToObj;   --uncomment this


                try
                {
                    string st = pathDataDir + trialNum.ToString(fmtTrack) + "-" + frame.ToString(fmtFrame) + ".png,";

                    //screenshot2(st);
                    //String tnum = trialNum.ToString(fmtTrack) + "-" + frame.ToString(fmtFrame);
                    //String wrld = (World.CurrentDayTime.Hours).ToString();
                    st = st + trialNum.ToString() + "," + trialNum.ToString(fmtTrack) + "-" + frame.ToString(fmtFrame) + ",N," + distToObj.ToString() + "," + (World.CurrentDayTime.Hours).ToString() + "," + weather;

                    StreamWriter file;

                    file = new StreamWriter(@"C:\Users\anirudhsr\Desktop\GTA Paths\last.txt");
                    string str;
                    str = trialNum.ToString() + "," + (World.CurrentDayTime.Hours).ToString() + "," + weatherForTrial.ToString() + "," + stopObjType + "," + stopObjSpawnLoc.X.ToString() + "," + stopObjSpawnLoc.Y.ToString() + "," + stopObjSpawnLoc.Z.ToString() + "," + startPos.X.ToString() + "," + startPos.Y.ToString() + "," + startPos.Z.ToString() + "," + endPos.X.ToString() + "," + endPos.Y.ToString() + "," + endPos.Z.ToString()+ "," +track.ToString() + "," + stopObj.IsVisible;

                    file.WriteLine(str);
                    //UI.Notify("Updated last value");
                    file.Close();
                    

                    if (sc == 0)
                        screenshot2(st);

                    if (sc == 1)
                    {
                        screenshot(pathDataDir + trialNum.ToString(fmtTrack) + "-" + frame.ToString(fmtFrame) + ".bmp");
                    }


                    //screenshot2(st, trialNum.ToString(), tnum, objectType, distToObj.ToString(), wrld, weather);//, c.ToString());
                    //CaptureScreen(Screen.PrimaryScreen, @st);
                }

                catch (Exception)
                {
                    //UI.Notify("Error taking screenshot");
                }

                //file.WriteLine(trialNum.ToString() + "," + trialNum.ToString(fmtTrack) + "-" + frame.ToString(fmtFrame) + ",N," + distToObj.ToString() + "," + (World.CurrentDayTime.Hours).ToString() + "," + weather + "," + c.ToString() + ",0,0,0,0");

                frame = frame + 1;
            }
            else
            {
                //UI.Notify("Recording non N-type");
                //screenshot(pathDataDir + trialNum.ToString(fmtTrack) + "-" + frame.ToString(fmtFrame) + ".jpg");
                String wrld = (World.CurrentDayTime.Hours).ToString();
                string st = pathDataDir + trialNum.ToString(fmtTrack) + "-" + frame.ToString(fmtFrame) + ".png,";

                //st = st + trialNum.ToString() + "," + trialNum.ToString(fmtTrack) + "-" + frame.ToString(fmtFrame) + distToObj.ToString() + "," + (World.CurrentDayTime.Hours).ToString() + "," + weather;
                st = st + trialNum.ToString() + "," + trialNum.ToString(fmtTrack) + "-" + frame.ToString(fmtFrame) + ",S," + distToObj.ToString() + "," + (World.CurrentDayTime.Hours).ToString() + "," + weather;

                StreamWriter file;

                file = new StreamWriter(@"C:\Users\anirudhsr\Desktop\GTA Paths\last.txt");
                string str;
                str = trialNum.ToString() + "," + (World.CurrentDayTime.Hours).ToString() + "," + weatherForTrial.ToString() + "," + stopObjType + "," + stopObjSpawnLoc.X.ToString() + "," + stopObjSpawnLoc.Y.ToString() + "," + stopObjSpawnLoc.Z.ToString() + "," + startPos.X.ToString() + "," + startPos.Y.ToString() + "," + startPos.Z.ToString() + "," + endPos.X.ToString() + "," + endPos.Y.ToString() + "," + endPos.Z.ToString() + "," + track.ToString()+","+ stopObj.IsVisible;

                file.WriteLine(str);
                //UI.Notify("Updated last value");
                file.Close();


                if (sc == 0)
                    screenshot2(st);
                else if (sc == 1)
                    screenshot(pathDataDir + trialNum.ToString(fmtTrack) + "-" + frame.ToString(fmtFrame) + ".bmp");



                //screenshot2(st, trialNum.ToString(), tnum, objectType, distToObj.ToString(), wrld, weather);//, c.ToString());
                //CaptureScreen(Screen.PrimaryScreen, @st);
                //Image img = Screen.PrimaryScreen;
                int[] bb = getStopObjectBoundingBox(stopObj, stopObj.Model);
                //file.WriteLine(trialNum.ToString() + "," + trialNum.ToString(fmtTrack) + "-" + frame.ToString(fmtFrame) + "," + objectType + "," + distToObj.ToString() + "," + (World.CurrentDayTime.Hours).ToString() + "," + weather + "," + c.ToString() + "," + bb[0] + "," + bb[1] + "," + bb[2] + "," + bb[3]);
                frame = frame + 1;
            }
            //UI.Notify("Still Recording");
            record = true;

            if (sc == 0) //to ensure python gets time to take a screenshot
                Script.Wait(600);
            else
                Script.Wait(5);
        }

    }

    //////////////////////////////////////////////////////////////////////////////////
    //                        TRAIL FUNCTIONS 
    //////////////////////////////////////////////////////////////////////////////////
    void readNextPath()
    {
        try
        {
            string path;

            if ((path = pathFile.ReadLine()) != null)
            {
                string[] tokens = path.Split(',');
                
                // read in path info 
                hasStopObjContext = bool.Parse(tokens[0]);

                endLineLeft = new Vector3(float.Parse(tokens[1]), float.Parse(tokens[2]), float.Parse(tokens[3]));
                endLineRight = new Vector3(float.Parse(tokens[4]), float.Parse(tokens[5]), float.Parse(tokens[6]));

                startLineLeft = new Vector3(float.Parse(tokens[7]), float.Parse(tokens[8]), float.Parse(tokens[9]));
                startLineRight = new Vector3(float.Parse(tokens[10]), float.Parse(tokens[11]), float.Parse(tokens[12]));
                stopObjSpawnLoc = new Vector3(float.Parse(tokens[13]), float.Parse(tokens[14]), float.Parse(tokens[15]));
                stopObjType = tokens[16];

                trialNum = 0;
                doneWithTrials = false;
                DirectoryInfo di = Directory.CreateDirectory(dataDir + track.ToString(fmtTrack) + "\\");
                pathDataDir = dataDir + track.ToString(fmtTrack) + "\\";
                //file = new StreamWriter(pathDataDir + track.ToString(fmtTrack) + ".csv", true);
                startPos = getRandLoc(startLineLeft, startLineRight);
                endPos = getRandLoc(endLineLeft, endLineRight);

                // move to next path to give the game time to generate the location
                Game.Player.Character.CurrentVehicle.Position = getRandLoc(startLineLeft, startLineRight);
                Wait(2000);
                deleteStopObjs(stopObjSpawnLoc, stopObjType, 5f);

                spawnStopObj(stopObjSpawnLoc, stopObjType);
                runTrial(trialNum, startPos, endPos);

                track = track + 1;
                //UI.Notify("Spawned Stop Signs");
            }
        }
        catch (Exception)
        {
            UI.Notify("Error Reading Path");
        }

    }

    void runTrial(int trial, Vector3 start, Vector3 end)
    {
        min = 9999f;  //for the purpose explained in line#307
        flag = false;
        c = 0;


        if (trial == 0)
        {
            World.CurrentDayTime = new TimeSpan(0, 0, 0);
        }
        else if (trial % 2 == 0)
        {
            World.CurrentDayTime = new TimeSpan(World.CurrentDayTime.Hours + 1, 0, 0);
        }

        if (((trial % 48) == 0))
        {
            if (weatherForTrial == 6)
                weatherForTrial = 0;

            weatherForTrial = weatherForTrial + 1;
            //UI.Notify("Changing Weather");
            if (weatherForTrial == 1)
                World.TransitionToWeather(Weather.ExtraSunny, 10);
            if (weatherForTrial == 2)
                World.TransitionToWeather(Weather.Foggy, 10);
            if (weatherForTrial == 3)
                World.TransitionToWeather(Weather.Overcast, 10);
            if (weatherForTrial == 4)
                World.TransitionToWeather(Weather.Raining, 10);
            if (weatherForTrial == 5)
                World.TransitionToWeather(Weather.ThunderStorm, 10);
            if (weatherForTrial == 6)
                World.TransitionToWeather(Weather.Clearing, 10);
        }


        Game.Player.Character.Task.ClearAll();
        Vehicle v = Game.Player.Character.CurrentVehicle;
        v.Repair();
        v.Position = start;
        v.Heading = (endLineLeft - startLineLeft).ToHeading(); //make the vehicle face in the direction of the end line
        Wait(2000);
        Game.Player.Character.Task.DriveTo(v, end, 2f, 10f, 786603);
        //UI.Notify("Running Trial " + trial.ToString());

        if (trial == 288) // 288
        {
            record = false;
            doneWithTrials = true;
            //file.Close();
            UI.Notify("Trials Over for this set");
            return;
            
        }

        record = true;
        return;
    }

    Vector3 getRandLoc(Vector3 start, Vector3 end)
    {
        Random random = new Random();
        return (float)random.NextDouble() * (end - start) + start;
    }

    bool isSpaceFree(Vector3 point, Model carModel)
    {
        Vehicle[] v = World.GetNearbyVehicles(point, 1.3f * carModel.GetDimensions().Y);


        if (v.Length != 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    //////////////////////////////////////////////////////////////////////////////////
    //                        STOP OBJECT FUNCTIONS 
    //////////////////////////////////////////////////////////////////////////////////
    // Function used to create a new stop sign
    public void spawnStopObj(Vector3 pos, string stopObjType)
    {
        try
        {
            if (stopObjType.CompareTo("S") == 0)
            {
                int n = -949234773;
                Model m = new Model((int)n);
                Prop p = World.CreateProp(m, pos, Vector3.Zero, false, false);
                

                // approximately align the stop sign with the road  
                float yaw = Vector3.SignedAngle(p.ForwardVector, (startLineRight - endLineRight), Vector3.WorldUp);
                p.Rotation = p.Rotation - Vector3.WorldUp * (180f - yaw); //rotate the stop sign and make it face the startline coordinate


                stopObj = p;
                

                noStopObjTrail = true;


                stopObj.IsCollisionProof = true;
                stopObj.HasCollision = false;

                //UI.Notify("Stops Signs Spawned");
                //hideStopObj();
                showStopObj();
                   
                

            }
        }
        catch (Exception)
        {
            UI.Notify("Error loading stop sign");
        }
    }

    public void deleteStopObjs(Vector3 pos, string stopObjType, float radius)
    {
        if (stopObjType.CompareTo("S") == 0)
        {
            int n = -949234773;
            Model m = new Model((int)n);
            Prop[] props = World.GetNearbyProps(pos, radius, m);
            foreach (Prop p in props)
            {
                p.Delete();
            }
        }
    }

    public void hideStopObj()
    {
        stopObj.IsVisible = false;
        //if (trialNum > 1)
        
    }

    public void showStopObj()
    {
        //stopObj.Delete();
        stopObj.IsVisible = true;
    }

    public bool stopObjVisible(Prop stopObj)
    {
        bool visible = false;

        foreach (SOI s in stopObjSOI)
        {
            if (s.modelHash == stopObj.Model.Hash)
            {
                Vector3 FUL = s.FUL.X * stopObj.RightVector + s.FUL.Y * (Vector3.Cross(stopObj.UpVector, stopObj.RightVector)) + s.FUL.Z * stopObj.UpVector + stopObj.Position;
                Vector3 BLR = s.BLR.X * stopObj.RightVector + s.BLR.Y * (Vector3.Cross(stopObj.UpVector, stopObj.RightVector)) + s.BLR.Z * stopObj.UpVector + stopObj.Position;
                Vector3 dim = new Vector3();



                dim.X = -System.Math.Abs(s.FUL.X - s.BLR.X);
                dim.Y = System.Math.Abs(s.FUL.Y - s.BLR.Y);
                dim.Z = System.Math.Abs(s.FUL.Z - s.BLR.Z);


                //dim.X = 2.0f;
                //dim.Y = 2.0f;
                //dim.Z = 2.0f;
                //UI.Notify("Stop Sign Visible");

                if (visibleOnScreen(stopObj, dim, FUL, BLR))
                {

                    visible = true;

                    break;
                }
            }
        }

        return visible;
    }


    private bool visibleOnScreen(Entity e, Vector3 dim, Vector3 FUL, Vector3 BLR)
    {
        bool isOnScreen = false;

        Vector3[] vertices = new Vector3[8];

        vertices[0] = FUL;
        vertices[1] = FUL - dim.X * e.RightVector;
        vertices[2] = FUL - dim.Z * e.UpVector;
        vertices[3] = FUL - dim.Y * Vector3.Cross(e.UpVector, e.RightVector);

        vertices[4] = BLR;
        vertices[5] = BLR + dim.X * e.RightVector;
        vertices[6] = BLR + dim.Z * e.UpVector;
        vertices[7] = BLR + dim.Y * Vector3.Cross(e.UpVector, e.RightVector);

        // check if the object is very close to the edge of the screen 
        if ((int)(IMAGE_WIDTH / (1.0 * UI.WIDTH) * UI.WorldToScreen(BLR).X) <= 7)
        {
            return false;
        }

        if ((int)(IMAGE_WIDTH / (1.0 * UI.WIDTH) * UI.WorldToScreen(FUL).X) >= IMAGE_WIDTH - 7)
        {
            return false;
        }

        foreach (Vector3 v in vertices)
        {
            if (UI.WorldToScreen(v).X != 0 && UI.WorldToScreen(v).Y != 0)
            {
                // is if point is visiable on screen
                Vector3 f = World.RenderingCamera.Position;
                Vector3 h = World.Raycast(f, v, IntersectOptions.Everything).HitCoords;

                if ((h - f).Length() < (v - f).Length())
                {
                    break;
                }
                else
                {
                    isOnScreen = true;
                    break;
                }
            }
        }

        return isOnScreen;
    }

    private int[] getStopObjectBoundingBox(Entity e, Model m)
    {
        foreach (SOI s in stopObjSOI)
        {
            if (s.modelHash == m.Hash)
            {
                Vector3 FUL = s.FUL.X * e.RightVector + s.FUL.Y * (Vector3.Cross(e.UpVector, e.RightVector)) + s.FUL.Z * e.UpVector + e.Position;
                Vector3 BLR = s.BLR.X * e.RightVector + s.BLR.Y * (Vector3.Cross(e.UpVector, e.RightVector)) + s.BLR.Z * e.UpVector + e.Position;
                Vector3 dim = new Vector3();

                dim.X = -System.Math.Abs(s.FUL.X - s.BLR.X);
                dim.Y = System.Math.Abs(s.FUL.Y - s.BLR.Y);
                dim.Z = System.Math.Abs(s.FUL.Z - s.BLR.Z);

                Vector3[] vertices = new Vector3[8];

                vertices[0] = FUL;
                vertices[1] = FUL - dim.X * e.RightVector;
                vertices[2] = FUL - dim.Z * e.UpVector;
                vertices[3] = FUL - dim.Y * Vector3.Cross(e.UpVector, e.RightVector);

                vertices[4] = BLR;
                vertices[5] = BLR + dim.X * e.RightVector;
                vertices[6] = BLR + dim.Z * e.UpVector;
                vertices[7] = BLR + dim.Y * Vector3.Cross(e.UpVector, e.RightVector);

                int xMin = int.MaxValue;
                int yMin = int.MaxValue;
                int xMax = 0;
                int yMax = 0;

                xMin = (int)(IMAGE_WIDTH / (1.0 * UI.WIDTH) * UI.WorldToScreen(FUL).X);

                int y = UI.WorldToScreen(FUL).Y;
                if (y != 0)
                {
                    yMin = (int)(IMAGE_HEIGHT / (1.0 * UI.HEIGHT) * y);
                }
                else
                {
                    yMin = (int)(IMAGE_HEIGHT / (1.0 * UI.HEIGHT) * UI.WorldToScreen(vertices[1]).Y);
                }

                int x = UI.WorldToScreen(BLR).X;
                if (x != 0)
                {
                    xMax = (int)(IMAGE_WIDTH / (1.0 * UI.WIDTH) * x);
                }
                else
                {
                    xMax = IMAGE_WIDTH;
                }

                y = UI.WorldToScreen(BLR).Y;
                if (y != 0)
                {
                    yMax = (int)(IMAGE_HEIGHT / (1.0 * UI.HEIGHT) * y);
                }
                else
                {
                    yMax = (int)(IMAGE_HEIGHT / (1.0 * UI.HEIGHT) * UI.WorldToScreen(vertices[5]).Y);
                }

                int[] boundingBox = new int[4] { xMin, yMin, xMax, yMax };

                return boundingBox;
            }
        }

        return new int[4];
    }

    Vector2 get2Dfrom3D(Vector3 a)
    {
        // camera rotation 
        Vector3 theta = (float)(System.Math.PI / 180f) * World.RenderingCamera.Rotation;
        // camera direction, at 0 rotation the camera looks down the postive Y axis 
        Vector3 camDir = rotate(Vector3.WorldNorth, theta);

        //UI.Notify("camDir: " + camDir.X.ToString() + " " + camDir.Y.ToString() + " " + camDir.Z.ToString());


        // camera position 
        Vector3 c = World.RenderingCamera.Position + World.RenderingCamera.NearClip * camDir;
        // viewer position 
        Vector3 e = -World.RenderingCamera.NearClip * camDir;
        // point locatios with repect to camera coordinates 
        Vector3 d;

        float viewWindowHeight = 2 * World.RenderingCamera.NearClip * (float)System.Math.Tan((World.RenderingCamera.FieldOfView / 2f) * (System.Math.PI / 180f));
        float viewWindowWidth = (IMAGE_WIDTH / ((float)IMAGE_HEIGHT)) * viewWindowHeight;

        Vector3 camUp = rotate(Vector3.WorldUp, theta);
        Vector3 camEast = rotate(Vector3.WorldEast, theta);

        Vector3 del = a - c;

        pointsToDraw.Add(del + c);

        Vector3 viewerDist = del - e;
        Vector3 viewerDistNorm = viewerDist * (1 / viewerDist.Length());
        float dot = Vector3.Dot(camDir, viewerDistNorm);
        float ang = (float)System.Math.Acos((double)dot);
        float viewPlaneDist = World.RenderingCamera.NearClip / (float)System.Math.Cos((double)ang);
        Vector3 viewPlanePoint = viewPlaneDist * viewerDistNorm + e;

        Vector3 newOrigin = c + (viewWindowHeight / 2f) * camUp - (viewWindowWidth / 2f) * camEast;
        viewPlanePoint = (viewPlanePoint + c) - newOrigin;

        float viewPlaneX = Vector3.Dot(viewPlanePoint, camEast) / Vector3.Dot(camEast, camEast);
        float viewPlaneZ = Vector3.Dot(viewPlanePoint, camUp) / Vector3.Dot(camUp, camUp);

        float screenX = viewPlaneX / viewWindowWidth * UI.WIDTH;
        float screenY = -viewPlaneZ / viewWindowHeight * UI.HEIGHT;

        return new Vector2((int)screenX, (int)screenY);
    }

    Vector3 rotate(Vector3 a, Vector3 theta)
    {
        Vector3 d = new Vector3();

        d.X = (float)System.Math.Cos((double)theta.Z) * ((float)System.Math.Cos((double)theta.Y) * a.X + (float)System.Math.Sin((double)theta.Y) * ((float)System.Math.Sin((double)theta.X) * a.Y + (float)System.Math.Cos((double)theta.X) * a.Z)) - (float)System.Math.Sin((double)theta.Z) * ((float)System.Math.Cos((double)theta.X) * a.Y - (float)System.Math.Sin((double)theta.X) * a.Z);
        d.Y = (float)System.Math.Sin((double)theta.Z) * ((float)System.Math.Cos((double)theta.Y) * a.X + (float)System.Math.Sin((double)theta.Y) * ((float)System.Math.Sin((double)theta.X) * a.Y + (float)System.Math.Cos((double)theta.X) * a.Z)) + (float)System.Math.Cos((double)theta.Z) * ((float)System.Math.Cos((double)theta.X) * a.Y - (float)System.Math.Sin((double)theta.X) * a.Z);
        d.Z = -(float)System.Math.Sin((double)theta.Y) * a.X + (float)System.Math.Cos((double)theta.Y) * ((float)System.Math.Sin((double)theta.X) * a.Y + (float)System.Math.Cos((double)theta.X) * a.Z);

        return d;
    }

    private List<SOI> loadSOI(string filename)
    {
        StreamReader SOIfile = new StreamReader(filename);
        List<SOI> items = new List<SOI>();
        string line;

        while ((line = SOIfile.ReadLine()) != null)
        {
            SOI item = new SOI();
            string[] tokens = line.Split(' ');

            item.modelHash = Int32.Parse(tokens[0]);
            item.name = tokens[1];
            item.FUL = new Vector3(float.Parse(tokens[2]), float.Parse(tokens[3]), float.Parse(tokens[4]));
            item.BLR = new Vector3(float.Parse(tokens[5]), float.Parse(tokens[6]), float.Parse(tokens[7]));

            items.Add(item);
        }
        SOIfile.Close();
        return items;
    }


    //////////////////////////////////////////////////////////////////////////////////
    //                        TRAFFIC FUNCTIONS 
    //////////////////////////////////////////////////////////////////////////////////


    //////////////////////////////////////////////////////////////////////////////////
    //                        SCREENSCHOT FUNCTIONS 
    //////////////////////////////////////////////////////////////////////////////////
    void screenshot(String filename)
    {
        //UI.Notify("Taking screenshot?");

        var foregroundWindowsHandle = GetForegroundWindow();
        var rect = new Rect();
        GetClientRect(foregroundWindowsHandle, ref rect);

        var pTL = new Point();
        var pBR = new Point();
        pTL.X = rect.Left;
        pTL.Y = rect.Top;
        pBR.X = rect.Right;
        pBR.Y = rect.Bottom;

        ClientToScreen(foregroundWindowsHandle, ref pTL);
        ClientToScreen(foregroundWindowsHandle, ref pBR);

        Rectangle bounds = new Rectangle(pTL.X, pTL.Y, rect.Right - rect.Left, rect.Bottom - rect.Top);


        using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
        {
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.ScaleTransform(.2f, .2f);
                g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
            }
            Bitmap output = new Bitmap(IMAGE_WIDTH, IMAGE_HEIGHT);
            using (Graphics g = Graphics.FromImage(output))
            {
                g.DrawImage(bitmap, 0, 0, IMAGE_WIDTH, IMAGE_HEIGHT);
            }
            output.Save(filename, ImageFormat.Bmp);

            // delay needed to ensure that images are not duplicated
            //Script.Wait(5);
            output.Dispose();
            bitmap.Dispose();
        }
    }


    void screenshot2(String name)//,String c )
    {
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        startInfo.FileName = "cmd.exe";

        string arg = "/C python screenshot.py "; //calling the python code to take a screenshot. Space occupied is lesser than .bmp
        arg = arg + name;

        startInfo.Arguments = arg;
        process.StartInfo = startInfo;
        process.Start();
        //Script.Wait(5);
    }

    void getrandpos()
    {
        Random rnd = new Random();
        float dist=101;
        Vector3 st, ed, stop;

        for (int i = 0; i < 1; i++)
        {
            //UI.Notify("Entering For loop");
            int count = 0;
            do
            {
                
                    //UI.Notify("Do while " + count.ToString());
                    count = count + 1;

                //int m1 = rnd.Next(1, 1408);

                    int m1 = rnd.Next(1, 77814);

                    string line1 = File.ReadLines("C:\\Users\\anirudhsr\\Desktop\\GTA Paths\\paths2.txt").Skip((m1 - 1)).Take(1).First();
                    string[] tk1 = line1.Split(',');
                    st = new Vector3(float.Parse(tk1[0]), float.Parse(tk1[1]), float.Parse(tk1[2]));



                    
                    //this portion takes a random position in the list of paths and takes another point a few metres ahead as the end point



                    string line3 = File.ReadLines("C:\\Users\\anirudhsr\\Desktop\\GTA Paths\\paths2.txt").Skip((m1 + 10)).Take(1).First();
                    string[] tk3 = line3.Split(',');
                    stop = new Vector3(float.Parse(tk3[0]), float.Parse(tk3[1]), float.Parse(tk3[2]));


                    string line2 = File.ReadLines("C:\\Users\\anirudhsr\\Desktop\\GTA Paths\\paths2.txt").Skip((m1 + 8)).Take(1).First();
                    string[] tk2 = line2.Split(',');
                    ed = new Vector3(float.Parse(tk2[0]), float.Parse(tk2[1]), float.Parse(tk2[2]));

                    dist = Vector3.Distance2D(st, stop);
                    UI.Notify("Distance:" + dist.ToString());
                
            } while (dist > 300f);

            //UI.Notify("Exited do-while");

           


            float x1 = ed.X;
            //UI.Notify(x1.ToString());

            float y1 = ed.Y;
            float z1 = ed.Z;

            float x2 = st.X;
            float y2 = st.Y;
            float z2 = st.Z;

            //float x3 = stop.X;
            //float y3 = stop.Y;
            //float z3 = stop.Z;


            Vector3 pos=new Vector3();

            float angle = Vector3.Angle(ed, pos);
            Vector3 cross = Vector3.Cross(ed, pos);

            if (cross.Y < 0)
                angle = -angle;


            //if ((angle > 0 && stop.Length() > ed.Length()) || (angle < 0 && ed.Length() > stop.Length()))
            //{
            //    pos = Vector3.Project(stop, ed);
            //}
            //else if ((angle > 0 && ed.Length() > stop.Length()) || (angle < 0 && stop.Length() > ed.Length()))
            ////else
            //{
            //    pos = Vector3.Project(ed, stop);
            //}

            if (((angle > 0) && (stop.Length() > ed.Length())) || ((angle < 0) && (ed.Length() > stop.Length())))
            {
                pos = Vector3.Project(ed, stop);


                //UI.Notify("Condition 1");

            }
            else if (((angle > 0) && (ed.Length() > stop.Length())) || ((angle < 0) && (stop.Length() > ed.Length())))
            //else
            {
                pos = Vector3.Project(stop, ed);


               // UI.Notify("Condition 2");

            }

            float x3 = pos.X;
            float y3 = pos.Y;
            float z3 = pos.Z;
            

            string str = "True, " + x1.ToString() + ", " + y1.ToString() + ", " + z1.ToString() + ", " + x1.ToString() + ", " + y1.ToString() + ", " + z1.ToString() + ", " + x2.ToString() + ", " + y2.ToString() + ", " + z2.ToString() + ", " + x2.ToString() + ", " + y2.ToString() + ", " + z2.ToString() + ", " + x3.ToString() + ", " + y3.ToString() + ", " + z3.ToString() +",S";
            

            //string str = "True, " + ed.ToString() + ", " + ed.ToString() + ", " + st.ToString() + ", " + st.ToString() + ", " + stop.ToString() + ",S";

            UI.Notify(str);

            



            try
            {
                //System.IO.StreamWriter file = new System.IO.StreamWriter("C:\\Users\\anirudhsr\\Desktop\\GTA Paths\\GTApaths2.txt",true);
                //file = new StreamWriter(pathDataDir + track.ToString(fmtTrack) + ".csv", true);
                //StreamWriter file;
                //file = new StreamWriter("C:\\Users\\anirudhsr\\Desktop\\GTA Paths\\GTApaths2.txt", true);
                UI.Notify("Writing to file");
                StreamWriter file;
                file = new StreamWriter(@"C:\\Users\\anirudhsr\\Desktop\\GTA Paths\\GTApaths3.txt", true);
                //file = new StreamWriter(@"C:\\Users\\anirudhsr\\Desktop\\GTA Paths\\paths.txt", true);
                file.WriteLine(str);
                //file.WriteLine("Hello");
                file.Close();

                //StreamWriter file;
                file = new StreamWriter(@"C:\\Users\\anirudhsr\\Desktop\\StopObj\\StopObject.txt", true);

                str = "-949234773" + " S " + x3.ToString() + " " + y3.ToString() + " " + z3.ToString() +" "+ (x3 + 3f).ToString()+ " " + (y3 + 3f).ToString()+ " " + (z3 + 3f).ToString();
                file.WriteLine(str);
                file.Close();

            }
            catch(Exception ex)
            {
                UI.Notify(ex.ToString());
                UI.Notify("Error Writing to file");
                //file.Close();
            }


        }

        UI.Notify("Writing Done ");


    }


}