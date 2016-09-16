using System;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using GygaxCore.DataStructures;

namespace GygaxVisu
{
    public class MyProcessor: Processor
    {
        private NViewMatch _nvm;
        private PlaneReconstructor _planeReconstructor;

        public override void Initial()
        {
            _nvm = (NViewMatch) Source.Data;

            var imageSideWidth = 1000;
            var imageSideHeight = imageSideWidth;

            _planeReconstructor = new PlaneReconstructor(_nvm.CameraPositions,
                PlaneReconstructor.ExtractProjectionPlane(_nvm.Patches, _nvm.CameraPositions), _nvm.Patches);

            Image<Bgr, Byte> image = new Image<Bgr, Byte>(imageSideWidth, imageSideHeight);

            _planeReconstructor.DrawGeometry(ref image);

            image.Save(@"C:\Users\Philipp\Desktop\geometry.bmp");

            Image<Bgr, byte> surfaceReconstruction = new Image<Bgr, byte>(imageSideWidth, imageSideHeight);
            _planeReconstructor.ReconstructRayTracer(ref surfaceReconstruction);
            surfaceReconstruction.Save(@"C:\Users\Philipp\Desktop\Reconstruction.bmp");

            Image<Bgr, byte> bufferImage;
            LUT.ApplyLut(ref _planeReconstructor.SurfacePatchId, out bufferImage);
            bufferImage.Save(@"C:\Users\Philipp\Desktop\ReconstructionPatchId.bmp");

            LUT.ApplyColormap(ref _planeReconstructor.SurfaceNoOfPixels, out bufferImage, ColorMapType.Jet);
            bufferImage.Save(@"C:\Users\Philipp\Desktop\ReconstructionNoOfPixel.bmp");

            LUT.ApplyColormap(ref _planeReconstructor.SurfaceAngle, out bufferImage, ColorMapType.Jet);
            bufferImage.Save(@"C:\Users\Philipp\Desktop\ReconstructionAngle.bmp");

            LUT.ApplyColormap(ref _planeReconstructor.SurfacePixelSize, out bufferImage, ColorMapType.Jet);
            bufferImage.Save(@"C:\Users\Philipp\Desktop\ReconstructionPixelSize.bmp");
        }

        public override void Update()
        {
            Initial();
        }

        public enum Direction { TopLeft, TopRight, BottomLeft, BottomRight };
    }
    
}
