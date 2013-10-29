using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace KinectPoints
{
    class myKinect
    {
        KinectSensor kinect;
        Skeleton[] skeletonData;
        public SkeletonPoint[] configData = new SkeletonPoint[5];
        public int index = -1;

        public void StartKinectSensor()
        {
            kinect = KinectSensor.KinectSensors[0];
            if (kinect.Status == KinectStatus.Connected)
            {
                kinect.SkeletonStream.Enable();
                skeletonData = new Skeleton[kinect.SkeletonStream.FrameSkeletonArrayLength];
                kinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinect_SkeletonFrameReady);
                Console.WriteLine("==Configuration==");
                Console.WriteLine("Steps: Depth => Top => Right => Bottom => Left");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
                kinect.Start();
            }
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

            Joint rightHand;
            foreach(Skeleton skel in skeletonData) {
                rightHand = skel.Joints[JointType.HandRight];
                if (skel.TrackingState == SkeletonTrackingState.Tracked && rightHand.TrackingState == JointTrackingState.Tracked && index < 5)
                {
                    if (index != -1)
                    {
                        Console.WriteLine("X: {0:g2} Y: {1:g2} Z: {2:g2}", rightHand.Position.X, rightHand.Position.Y, rightHand.Position.Z);
                        configData[index] = rightHand.Position;
                    }
                    System.Threading.Thread.Sleep(5000);
                    index++;
                }
            }
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            myKinect prgm = new myKinect();
            prgm.StartKinectSensor();
            while (prgm.index < 5) ;
            for (int i = 0; i < 5; i++)
               Console.WriteLine("CONFIG: X: {0:g2} Y: {1:g2} Z: {2:g2}", prgm.configData[i].X, prgm.configData[i].Y, prgm.configData[i].Z);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }
    }
}
