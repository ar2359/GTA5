/*
Authors: Artur Filipowicz
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

// Controls: 
// NumPad0 - spawns in a new test vehicle 
// I - mounts rendering camera on vehicle 
// O - restores the rendering camera to original control

public class Class2 : Script
{

    // camera used on the vehicle
    Camera camera = null;
    Vehicle vehicle;
    float x, y, z;

    public Class2()
    {
        UI.Notify("Loaded TestVehicle.cs");

        // create a new camera 
        World.DestroyAllCameras();
        camera = World.CreateCamera(new Vector3(), new Vector3(), 50);
        camera.IsActive = true;
        GTA.Native.Function.Call(Hash.RENDER_SCRIPT_CAMS, false, true, camera.Handle, true, true);

        x = 0;
        y = 2;
        z = 0.4f;
        // attach time methods 
        Tick += OnTick;
        KeyUp += onKeyUp;
    }

    //function to get a random location on the map from paths2.txt 
    void getrandpos()
    {
        Random rnd = new Random();
        float dist = 101;
        Vector3 st, ed, stop;

        for (int i = 0; i < 1; i++)
        {
            UI.Notify("Entering For loop");
            int count = 0;
            do
            {

                UI.Notify("Do while " + count.ToString());
                count = count + 1;

                int m1 = rnd.Next(1, 1408);


                string line1 = File.ReadLines("C:\\Users\\anirudhsr\\Desktop\\GTA Paths\\paths.txt").Skip((m1 - 1)).Take(1).First();
                string[] tk1 = line1.Split(',');
                st = new Vector3(float.Parse(tk1[0]), float.Parse(tk1[1]), float.Parse(tk1[2]));



                //int m2 = rnd.Next(1, 1408);




                string line3 = File.ReadLines("C:\\Users\\anirudhsr\\Desktop\\GTA Paths\\paths.txt").Skip((m1 + 10)).Take(1).First();
                string[] tk3 = line3.Split(',');
                stop = new Vector3(float.Parse(tk3[0]), float.Parse(tk3[1]), float.Parse(tk3[2]));


                string line2 = File.ReadLines("C:\\Users\\anirudhsr\\Desktop\\GTA Paths\\paths.txt").Skip((m1 + 8)).Take(1).First();
                string[] tk2 = line2.Split(',');
                ed = new Vector3(float.Parse(tk2[0]), float.Parse(tk2[1]), float.Parse(tk2[2]));

                dist = Vector3.Distance2D(st, stop);
                UI.Notify("Distance:" + dist.ToString());

            } while (dist > 300f); //to ensure that the two points obtained are on the same road

            UI.Notify("Exited do-while");




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


            Vector3 pos = new Vector3();

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
                pos = Vector3.Project(ed, stop); //projection is being used to ensure that the stop sign ends up on the side of the end point and not directly in front of it


                UI.Notify("Condition 1");

            }
            else if (((angle > 0) && (ed.Length() > stop.Length())) || ((angle < 0) && (stop.Length() > ed.Length())))
            //else
            {
                pos = Vector3.Project(stop, ed);


                UI.Notify("Condition 2");

            }

            float x3 = pos.X;
            float y3 = pos.Y;
            float z3 = pos.Z;


            string str = "True, " + x1.ToString() + ", " + y1.ToString() + ", " + z1.ToString() + ", " + x1.ToString() + ", " + y1.ToString() + ", " + z1.ToString() + ", " + x2.ToString() + ", " + y2.ToString() + ", " + z2.ToString() + ", " + x2.ToString() + ", " + y2.ToString() + ", " + z2.ToString() + ", " + x3.ToString() + ", " + y3.ToString() + ", " + z3.ToString() + ",S";


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

                file.WriteLine(str);
                //file.WriteLine("Hello");
                file.Close();

                //StreamWriter file;
                file = new StreamWriter(@"C:\\Users\\anirudhsr\\Desktop\\StopObj\\StopObject.txt", true);

                str = "-949234773" + " S " + x3.ToString() + " " + y3.ToString() + " " + z3.ToString() + " " + (x3 + 3f).ToString() + " " + (y3 + 3f).ToString() + " " + (z3 + 3f).ToString();
                file.WriteLine(str);
                file.Close();

            }
            catch (Exception ex)
            {
                UI.Notify(ex.ToString());
                UI.Notify("Error Writing to file");
                //file.Close();
            }


        }

        UI.Notify("Writing Done ");


    }

    
    

    
    private void onKeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.X)
        {
            UI.Notify("X pressed");
            getrandpos();

        }


    }

    void OnTick(object sender, EventArgs e)
    {
        //keepCameraOnVehicle();
    }
}