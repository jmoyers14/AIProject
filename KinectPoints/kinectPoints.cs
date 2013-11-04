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
        public const int frameHeight = 858;
        public const int frameWidth = 1525;

    }
    class myKinect
    {
        KinectSensor kinect;
        Skeleton[] skeletonData;
        public SkeletonPoint[] configData = new SkeletonPoint[5];
        public int index = -1;
        public Joint hand;

        public void StartKinectSensor()
        {
            kinect = KinectSensor.KinectSensors[0];
            if (kinect.Status == KinectStatus.Connected)
            {
                kinect.SkeletonStream.Enable();
                kinect.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;

                TransformSmoothParameters smoothingParam = new TransformSmoothParameters();
                {
                    smoothingParam.Smoothing = 0.5f;
                    smoothingParam.Correction = 0.1f;
                    smoothingParam.Prediction = 0.5f;
                    smoothingParam.JitterRadius = 0.1f;
                    smoothingParam.MaxDeviationRadius = 0.1f;
                };

                kinect.SkeletonStream.Enable(smoothingParam); // Enable skeletal tracking

                skeletonData = new Skeleton[kinect.SkeletonStream.FrameSkeletonArrayLength];
                kinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinect_SkeletonFrameReady);
                kinect.Start();
            }
            else
                Console.WriteLine("No Kinect Found");

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
            Joint temp;
            foreach (Skeleton skel in skeletonData)
            {
                temp = skel.Joints[JointType.HandRight];
                if (skel.TrackingState == SkeletonTrackingState.Tracked && temp.TrackingState == JointTrackingState.Tracked && index < 3)
                {
                    hand = temp;

                    if (index == -1)
                    {
                        Console.WriteLine("Body Found");
                        Console.WriteLine("Place hand in the top left");
                    }
                    if (index == 0)
                    {
                        Console.WriteLine("X: {0:g2} Y: {1:g2} Z: {2:g2}", hand.Position.X, hand.Position.Y, hand.Position.Z);
                        Console.WriteLine("Place hand in the bottom left");
                        configData[index] = hand.Position;   
                    }
                    if (index == 1)
                    {
                        Console.WriteLine("X: {0:g2} Y: {1:g2} Z: {2:g2}", hand.Position.X, hand.Position.Y, hand.Position.Z);
                        Console.WriteLine("Place hand in the top right");
                        configData[index] = hand.Position;
                    }
                    if (index == 2)
                    {
                        Console.WriteLine("X: {0:g2} Y: {1:g2} Z: {2:g2}", hand.Position.X, hand.Position.Y, hand.Position.Z);
                        configData[index] = hand.Position;
                    }
                    
                        
                    //System.Threading.Thread.Sleep(5000);
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey(true);

                    index++;
                }
                else if (skel.TrackingState == SkeletonTrackingState.Tracked && temp.TrackingState == JointTrackingState.Tracked)
                    hand = temp;

            }
        }

    }

    public class Mouse
    {
        [DllImport("user32.dll")]
        private static extern void mouse_event(UInt32 dwFlags, UInt32 dx, UInt32 dy, UInt32 dwData, IntPtr dwExtraInfo);

        private const UInt32 MouseEventLeftDown = 0x0002;
        private const UInt32 MouseEventLeftUp = 0x0004;

        public void DownClick()
        {
            mouse_event(MouseEventLeftDown, 0, 0, 0, new System.IntPtr());
        }

        public void UpClick()
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

        //line1 is the line from the top-left corner to bottom-left corner
        //line2 is the line from the top-left corner to top-right corner
        //normalVector is a unit vector that is perpendicular to the screen
        //point is the top-left corner of the screen
        //d is a constant needed for the plane equation
        //height is the height of the screen in meters
        //width is the width of the screen in meters

        public myScreen(SkeletonPoint P1, SkeletonPoint P2, SkeletonPoint P3)
        {
            line1 = line(convert(P1), convert(P2));
            line2 = line(convert(P1), convert(P3));

            normalVector = cross(line1, line2);
            normalVector = normalize(normalVector);
            point = convert(P1);
            d = -(dot(normalVector, point));
            height = lineLength(line1);
            width = lineLength(line2);
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

        private double lineLength(Vector l)
        {
            return Math.Sqrt(dot(l, l));
        }

        private double dot(Vector P1, Vector P2)
        {
            return (P1.x * P2.x) + (P1.y * P2.y) + (P1.z * P2.z);
        }

        private Vector normalize(Vector v)
        {
            double[] temp = new double[3];
            temp[0] = v.x / lineLength(v);
            temp[1] = v.y / lineLength(v);
            temp[2] = v.z / lineLength(v);

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
          
            double distToPoint, theta;
            Vector pointOnPlane, lineToPoint;

            double[] temp = new double[3];
            temp[0] = p.X - (dot(normalVector, convert(p)) + d) * normalVector.x;
            temp[1] = p.Y - (dot(normalVector, convert(p)) + d) * normalVector.y;
            temp[2] = p.Z - (dot(normalVector, convert(p)) + d) * normalVector.z;

            pointOnPlane = new Vector(temp);

            lineToPoint = line(point, pointOnPlane);
            distToPoint = lineLength(lineToPoint);

            theta = Math.Acos(dot(lineToPoint, line1) / (distToPoint*height));

            return  (distToPoint*Math.Sin(theta)) / width;
            
        }

        public double getYOnScreen(SkeletonPoint p)
        {
            double distToPoint, theta;
            Vector pointOnPlane, lineToPoint;

            double[] temp = new double[3];
            temp[0] = p.X - (dot(normalVector, convert(p)) + d) * normalVector.x;
            temp[1] = p.Y - (dot(normalVector, convert(p)) + d) * normalVector.y;
            temp[2] = p.Z - (dot(normalVector, convert(p)) + d) * normalVector.z;

            pointOnPlane = new Vector(temp);

            lineToPoint = line(point, pointOnPlane);
            distToPoint = lineLength(lineToPoint);

            theta = Math.Acos(dot(lineToPoint, line1) / (distToPoint * height));

            return (distToPoint * Math.Cos(theta)) / height;
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

            Console.WriteLine("Screen plane qualities- Height: {3:g4} Width: {4:g4} Normal Vector: {0:g4}, {1:g4}, {2:g4}", scrn.normalVector.x, scrn.normalVector.y, scrn.normalVector.z, scrn.height, scrn.width);

            Console.WriteLine("Press any key to start application...");
            Console.ReadKey(true);
            //t.Start();
            
            while (true)
            {            
                xCoord = (int)(scrn.getXOnScreen(prgm.hand.Position) * Constants.frameWidth);
                yCoord = (int)(scrn.getYOnScreen(prgm.hand.Position) * Constants.frameHeight);

                if (xCoord > Constants.frameWidth)
                    xCoord = Constants.frameWidth;
                else if (xCoord < 0)
                    xCoord = 0;

                if (yCoord > Constants.frameWidth)
                    yCoord = Constants.frameWidth;
                else if (yCoord < 0)
                    yCoord = 0;

                Console.WriteLine("X: {0:g} Y: {1:g} Distance to Screen: {2:g}", scrn.getXOnScreen(prgm.hand.Position), scrn.getYOnScreen(prgm.hand.Position), scrn.getDistanceToScreen(prgm.hand.Position));
                Cursor.Position = new Point(xCoord, yCoord);
                if (scrn.getDistanceToScreen(prgm.hand.Position) < 0.01)
                {
                    //check x and y coord against puzzle object  
                    if(first)
                        myMouse.DownClick();
                    first = false;
                }
                else{
                    //while (scrn.getDistanceToScreen(prgm.leftHand.Position) < 0.1) ;
                    myMouse.UpClick();
                    first = true;
                }

            }
        }
    }
}
