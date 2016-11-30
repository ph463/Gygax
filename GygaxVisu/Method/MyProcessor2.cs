using System;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.XFeatures2D;
using GygaxCore.DataStructures;

namespace GygaxVisu
{
    public class MyProcessor2 : Processor
    {
        public MyProcessor2()
        {
            Image<Bgr, Byte> observedImage = new Image<Bgr, Byte>(@"C:\Users\Philipp\Desktop\Stitch AddTestelements\Test2\Picture043c.jpg");
            Image<Bgr, Byte> modelImage = new Image<Bgr, Byte>(@"C:\Users\Philipp\Desktop\Stitch AddTestelements\Test2\DSC07473.JPG");

            SURF surfCPU = new SURF(300, 20, 10);
            //SIFT surfCPU = new SIFT(100,5,0.1,5);
            //SURF surfCPU = new SURF(600, 20, 4);
            //FastDetector surfCPU = new FastDetector();

            //extract features from the object image
            UMat modelDescriptors = new UMat();
            VectorOfKeyPoint modelKeyPoints = new VectorOfKeyPoint();

            try
            {
                surfCPU.DetectAndCompute(modelImage, null, modelKeyPoints, modelDescriptors, false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

            Image<Bgr, Byte> outImage = new Image<Bgr, byte>(modelImage.Size);
            Features2DToolbox.DrawKeypoints(modelImage, modelKeyPoints, outImage,new Bgr(0,255,255),Features2DToolbox.KeypointDrawType.Default);
            outImage.Save(@"C:\Users\Philipp\Desktop\Stitch AddTestelements\out.JPG");

            VectorOfKeyPoint observedKeyPoints = new VectorOfKeyPoint();
            UMat observedDescriptors = new UMat();

            surfCPU.DetectAndCompute(observedImage, null, observedKeyPoints, observedDescriptors, false);

            outImage = new Image<Bgr, byte>(observedImage.Size);
            Features2DToolbox.DrawKeypoints(observedImage, observedKeyPoints, outImage, new Bgr(0, 255, 255), Features2DToolbox.KeypointDrawType.Default);
            outImage.Save(@"C:\Users\Philipp\Desktop\Stitch AddTestelements\out2.JPG");

            VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch();
            Mat mask;
            Mat homography;

            BFMatcher matcher = new BFMatcher(DistanceType.L2);
            matcher.Add(modelDescriptors);

            matcher.KnnMatch(observedDescriptors, matches, 2, null);
            mask = new Mat(matches.Size, 1, DepthType.Cv8U, 1);
            mask.SetTo(new MCvScalar(255));
            Features2DToolbox.VoteForUniqueness(matches, 0.8, mask);

            int nonZeroCount = CvInvoke.CountNonZero(mask);
            if (nonZeroCount >= 4)
            {
                nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(modelKeyPoints, observedKeyPoints,
                   matches, mask, 1.5, 20);
                if (nonZeroCount >= 4)
                    homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(modelKeyPoints,
                       observedKeyPoints, matches, mask, 2);
            }

            Mat result = new Mat();
            Features2DToolbox.DrawMatches(modelImage, modelKeyPoints, observedImage, observedKeyPoints,
               matches, result, new MCvScalar(255, 255, 255), new MCvScalar(255, 255, 255), mask);

            byte[] m = mask.GetData();
            
            MDMatch[][] test = matches.ToArrayOfArray();

            for (int i = 0; i < test.Length; i++)
            {
                if (m[i] != 0)
                {
                    var point1 = modelKeyPoints[test[i][0].TrainIdx];
                    var point2 = observedKeyPoints[test[i][0].QueryIdx];
                    //var point2 = observedKeyPoints[test[i][1].TrainIdx];
                }
            }



            result.Save(@"C:\Users\Philipp\Desktop\Stitch AddTestelements\out3.JPG");

            this.CvSource = result;


            //Emgu.CV.Matrix<int> indices;
            //BriefDescriptorExtractor descriptor = new BriefDescriptorExtractor();

            //Emgu.CV.Matrix<byte> mask;
            //int k = 2;
            //double uniquenessThreshold = 0.8;



            ////Image<Bgr, Byte> outImage = new Image<Bgr, byte>(image360.Size);

            ////Image<Bgr, Byte> result = 
            ////Features2DToolbox.DrawKeypoints(image360, modelKeyPoints, outImage,new Bgr(0,255,255),Features2DToolbox.KeypointDrawType.Default);

            ////outImage.Save(@"C:\Users\Philipp\Desktop\Stitch AddTestelements\out.JPG");


            ////extract features from the object image
            ////modelKeyPoints = fastCPU.DetectKeyPointsRaw(modelImage, null);
            //Mat modelDescriptors;
            //descriptor.DetectAndCompute(image, null, modelKeyPoints,modelDescriptors, false);

            //// extract features from the observed image
            //observedKeyPoints = fastCPU.DetectRaw(observedImage, null);
            //Emgu.CV.Matrix<Byte> observedDescriptors = descriptor.ComputeRaw(observedImage, null, observedKeyPoints);
            //BFMatcher<Byte> matcher = new BFMatcher<Byte>(DistanceType.L2);
            //matcher.Add(modelDescriptors);



            //indices = new Emgu.CV.Matrix<int>(observedDescriptors.Rows, k);
            //using (Emgu.CV.Matrix<float> dist = new Emgu.CV.Matrix<float>(observedDescriptors.Rows, k))
            //{
            //    matcher.KnnMatch(observedDescriptors, indices, dist, k, null);
            //    mask = new Emgu.CV.Matrix<byte>(dist.Rows, 1);
            //    mask.SetValue(255);
            //    Features2DToolbox.VoteForUniqueness(dist, uniquenessThreshold, mask);
            //}

            //int nonZeroCount = CvInvoke.cvCountNonZero(mask);
            //if (nonZeroCount >= 4)
            //{
            //    nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(modelKeyPoints, observedKeyPoints, indices, mask, 1.5, 20);
            //    if (nonZeroCount >= 4)
            //    {
            //        homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(
            //            modelKeyPoints, observedKeyPoints, indices, mask, 2);
            //    }
            //}

            ////Draw the matched keypoints
            //Image<Bgr, Byte> result = Features2DToolbox.DrawMatches(modelImage, modelKeyPoints, observedImage, observedKeyPoints,
            //indices, new Bgr(255, 255, 255), new Bgr(255, 255, 255), mask, Features2DToolbox.KeypointDrawType.DEFAULT);


            //if (homography != null)
            //{  //draw a rectangle along the projected model
            //    Rectangle rect = modelImage.ROI;
            //    PointF[] pts = new PointF[] {
            //    new PointF(rect.Left, rect.Bottom),
            //    new PointF(rect.Right, rect.Bottom),
            //    new PointF(rect.Right, rect.Top),
            //    new PointF(rect.Left, rect.Top)};
            //    homography.ProjectPoints(pts);

            //    result.DrawPolyline(Array.ConvertAll<PointF, Point>(pts, Point.Round), true, new Bgr(Color.Red), 5);
            //}
            //*/
        }

        public override void Initial()
        {
        }


        public override void Update()
        {
            Initial();
        }

    }
}