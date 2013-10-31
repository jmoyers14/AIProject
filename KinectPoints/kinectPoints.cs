using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;
using System.Runtime.InteropServices;

namespace KinectPoints
{
    static class Constants
    {
        public const int frameHeight = 768;
        public const int frameWidth = 1366;

    }
    class myKinect
    {
        KinectSensor kinect;
        Skeleton[] skeletonData;
        public SkeletonPoint[] configData = new SkeletonPoint[5];
        public int index = -1;
        public Joint leftHand;

        public void StartKinectSensor()
        {
            kinect = KinectSensor.KinectSensors[0];
            if (kinect.Status == KinectStatus.Connected)
            {
                kinect.SkeletonStream.Enable();
                skeletonData = new Skeleton[kinect.SkeletonStream.FrameSkeletonArrayLength];
                kinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinect_SkeletonFrameReady);
                Console.WriteLine("==Configuration==");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
                kinect.Start();
            }
            else
                Console.WriteLine("Error");
        }

        private void kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame()) // Open the Skeleton frame
            {
                if (skeletonFrame != null && this.skeletonData != null) // check that a frame is available
                {
                    skeletonFrame.CopySkeletonDataTo(this.skeletonData); // get the skeletal information in this frame
                    getSkeletonPoints();
                }
            }
        }

        private void getSkeletonPoints()
        {
            Joint templeft;
            foreach (Skeleton skel in skeletonData)
            {
                templeft = skel.Joints[JointType.HandLeft];
                if (skel.TrackingState == SkeletonTrackingState.Tracked && templeft.TrackingState == JointTrackingState.Tracked && index < 3)
                {
                    leftHand = templeft;

                    if (index == -1)
                    {
                        Console.WriteLine("Body Found");
                        Console.WriteLine("Place hand in the top left");
                    }
                    if (index == 0)
                    {
                        Console.WriteLine("X: {0:g2} Y: {1:g2} Z: {2:g2}", leftHand.Position.X, leftHand.Position.Y, leftHand.Position.Z);
                        Console.WriteLine("Place hand in the bottom left");
                        configData[index] = leftHand.Position;   
                    }
                    if (index == 1)
                    {
                        Console.WriteLine("X: {0:g2} Y: {1:g2} Z: {2:g2}", leftHand.Position.X, leftHand.Position.Y, leftHand.Position.Z);
                        Console.WriteLine("Place hand in the bottom right");
                        configData[index] = leftHand.Position;
                    }
                    if (index == 2)
                    {
                        Console.WriteLine("X: {0:g2} Y: {1:g2} Z: {2:g2}", leftHand.Position.X, leftHand.Position.Y, leftHand.Position.Z);
                        configData[index] = leftHand.Position;
                    }
                    
                        
                    //System.Threading.Thread.Sleep(5000);
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey(true);

                    index++;
                }
                else if (skel.TrackingState == SkeletonTrackingState.Tracked && templeft.TrackingState == JointTrackingState.Tracked)
                    leftHand = templeft;

            }
        }

    }

    public class Mouse
    {
        [DllImport("user32.dll")]
        private static extern void mouse_event(UInt32 dwFlags, UInt32 dx, UInt32 dy, UInt32 dwData, IntPtr dwExtraInfo);

        private const UInt32 MouseEventLeftDown = 0x0002;
        private const UInt32 MouseEventLeftUp = 0x0004;

        public void SendFirstClick()
        {
            mouse_event(MouseEventLeftDown, 0, 0, 0, new System.IntPtr());
        }

        public void SendSecondClick()
        {
            mouse_event(MouseEventLeftUp, 0, 0, 0, new System.IntPtr());
        }

    }


    class Vector
    {
        public double x, y, z;

        public Vector()
        { }

        public Vector(double[] p)
        {
            x = p[0];
            y = p[1];
            z = p[2];
        }
    }


    class myScreen
    {
        public Vector normalVector, point, line1, line2;
        public double d, height, width;

        public myScreen(SkeletonPoint P1, SkeletonPoint P2, SkeletonPoint P3)
        {
            line1 = line(convert(P1), convert(P2));
            line2 = line(convert(P1), convert(P3));

            normalVector = cross(line1, line2);
           // normalVector = normalize(normalVector);
            point = convert(P1);
            d = -(dot(normalVector, point));
            height = distance(convert(P1), convert(P2));
            width = distance(convert(P2), convert(P3));
        }

        private Vector convert(SkeletonPoint p)
        {
            double[] temp = new double[3];
            temp[0] = p.X;
            temp[1] = p.Y;
            temp[2] = p.Z;

            return new Vector(temp);
        }

        private Vector line(Vector P1, Vector P2)
        {
            double[] temp = new double[3];
            temp[0] = P2.x - P1.x;
            temp[1] = P2.y - P1.y;
            temp[2] = P2.z - P1.z;

            return new Vector(temp);
        }

        private double distance(Vector P1, Vector P2)
        {
            return Math.Sqrt(dot(P1, P2));
        }

        private double dot(Vector P1, Vector P2)
        {
            return (P1.x * P2.x) + (P1.y * P2.y) + (P1.z * P2.z);
        }

        private Vector normalize(Vector v)
        {
            double[] temp = new double[3];
            temp[0] = v.x / distance(v, v);
            temp[1] = v.y / distance(v, v);
            temp[2] = v.z / distance(v, v);

            return new Vector(temp);
        }

        private Vector cross(Vector P1, Vector P2)
        {
            double[] temp = new double[3];
            temp[0] = (P1.y * P2.z) - (P1.z * P2.y);
            temp[1] = (P1.z * P2.x) - (P1.x * P2.z);
            temp[2] = (P1.x * P2.y) - (P1.y * P2.x);

            return new Vector(temp);
        }

        public double getDistanceToScreen(SkeletonPoint p)
        {
            return dot(normalVector, convert(p)) + d;
        }

        public double getXOnScreen(SkeletonPoint p)
        {
            /*
            double[] temp = new double[3];
            temp[0] = (p.X - point.x) * normalVector.x;
            temp[1] = (p.Y - point.y) * normalVector.y;
            temp[2] = (p.Z - point.z) * normalVector.z;

            if (abs(temp[0] + temp[1] + temp[2]) <= 0.05)
            {

            }
            */

           
            double distToPoint, theta;
            Vector pointOnPlane, lineToPoint;

            double[] temp = new double[3];
            temp[0] = p.X - dot(normalVector, convert(p)) * normalVector.x;
            temp[1] = p.Y - dot(normalVector, convert(p)) * normalVector.y;
            temp[2] = p.Z - dot(normalVector, convert(p)) * normalVector.z;

            pointOnPlane = new Vector(temp);

            lineToPoint = line(point, pointOnPlane);
            distToPoint = distance(point, pointOnPlane);

            theta = Math.Acos(dot(lineToPoint, line1)/(distToPoint*height));

            return  (distToPoint*Math.Sin(theta))/width;
            
        }

        public double getYOnScreen(SkeletonPoint p)
        {
            double distToPoint, theta;
            Vector pointOnPlane, lineToPoint;

            double[] temp = new double[3];
            temp[0] = p.X - dot(normalVector, convert(p)) * normalVector.x;
            temp[1] = p.Y - dot(normalVector, convert(p)) * normalVector.y;
            temp[2] = p.Z - dot(normalVector, convert(p)) * normalVector.z;

            pointOnPlane = new Vector(temp);

            lineToPoint = line(point, pointOnPlane);
            distToPoint = distance(point, pointOnPlane);

            theta = Math.Acos(dot(lineToPoint, line1)/(distToPoint*height));

            return  (distToPoint*Math.Cos(theta))/height;
        }
    }

    class Program
    {
        static void startGame()
        {
            Application.Run(new ConsoleApplication1.MatchingGame());
        }

        static void Main(string[] args)
        {
            Mouse myMouse = new Mouse();
            Thread t = new Thread(startGame);
            Cursor myCursor = new Cursor(Cursor.Current.Handle);
            int xCoord, yCoord;
            myScreen scrn;
            Boolean first = true;

            myKinect prgm = new myKinect();
            prgm.StartKinectSensor();
            while (prgm.index < 3) ;
                        
            scrn = new myScreen(prgm.configData[0], prgm.configData[1], prgm.configData[2]);

            Console.WriteLine("Screen plane qualities: Normal Vector: {0:g2}, {1:g2}, {2:g2}", scrn.normalVector.x, scrn.normalVector.y, scrn.normalVector.z);

            Console.WriteLine("Press any key to start application...");
            Console.ReadKey(true);
            t.Start();
            
            while (true)
            {            
                xCoord = (int)(scrn.getXOnScreen(prgm.leftHand.Position) * Constants.frameWidth);
                yCoord = (int)(scrn.getYOnScreen(prgm.leftHand.Position) * Constants.frameHeight);

                //Console.WriteLine("X: {0:g} Y: {1:g}", scrn.getXOnScreen(prgm.leftHand.Position), scrn.getYOnScreen(prgm.leftHand.Position));
                Console.WriteLine("X: {0:g} Y: {1:g}", xCoord, yCoord);
                Cursor.Position = new Point(xCoord, yCoord);
                if (scrn.getDistanceToScreen(prgm.leftHand.Position) < 0.07)
                {
                    //check x and y coord against puzzle object  
                    if(first)
                        myMouse.SendFirstClick();
                    first = false;
                }
                else{
                    //while (scrn.getDistanceToScreen(prgm.leftHand.Position) < 0.1) ;
                    myMouse.SendSecondClick();
                    first = true;
                }

            }
        }
    }
}
