using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OpenCvSharp;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class CalibracionChina : MonoBehaviour
{
    static int BoardSize_Width = 9;

    static int BoardSize_Height = 30;

    static Size BoardSize = new Size(BoardSize_Width, BoardSize_Height);

    static int SquareSize = 5;

    static int winSize = 11;

    void Start()
    {
        List<string> imagesList =
            new List<string>()
            {
                "Assets/Caracterizacion_MB/2/IMG_20220311_170745.jpg",
                "Assets/Caracterizacion_MB/2/IMG_20220311_170747.jpg",
                "Assets/Caracterizacion_MB/2/IMG_20220311_170802.jpg",
                "Assets/Caracterizacion_MB/2/IMG_20220311_170806.jpg",
                "Assets/Caracterizacion_MB/2/IMG_20220311_170810.jpg"
            };
        List<Point2f[]> imagesPoints = new List<Point2f[]>();
        Mat
            cameraMatrix = new Mat(),
            distCoeffs = new Mat();
        Size imageSize = new Size();
        bool found = false;

        Mat[] imagesPointsM = new Mat[imagesList.Count];

        for (int i = 0; i < imagesList.Count; i++)
        {
            Debug.Log(imagesList)
            byte[] fileData =
                File.ReadAllBytes(imagesList[i]);
            var tex = new Texture2D(2, 2);
            tex.LoadImage (fileData);
            Mat view = OpenCvSharp.Unity.TextureToMat(tex);
            Cv2.ImShow("Image View", view);
            // if (!view.Empty())
            // {
            //     imageSize = view.Size();
            //     Point2f[] pointBuf;

            //     Cv2.ImShow("Image View", view);
            //     found =
            //         Cv2
            //             .FindChessboardCorners(view,
            //             BoardSize,
            //             out pointBuf,
            //             ChessboardFlags.AdaptiveThresh |
            //             ChessboardFlags.NormalizeImage);
            //     Debug.Log("Prueba " + pointBuf.Length);
            //     if (found == true)
            //     {
            //         var criteria =
            //             new TermCriteria(CriteriaType.Eps |
            //                 CriteriaType.MaxIter,
            //                 30,
            //                 0.001);
            //         Mat viewGray = new Mat();
            //         Cv2.CvtColor(view, viewGray, ColorConversionCodes.BGR2GRAY);
            //         Cv2
            //             .CornerSubPix(viewGray,
            //             pointBuf,
            //             new Size(winSize, winSize),
            //             new Size(-1, -1),
            //             criteria);
            //         imagesPoints.Add (pointBuf);
            //         Debug.Log("Intento" + pointBuf);
            //         Mat p =
            //             new Mat(BoardSize.Width, 1, MatType.CV_64F, pointBuf);
            //         imagesPointsM[i] = p;

            //         Cv2.DrawChessboardCorners (
            //             view,
            //             BoardSize,
            //             pointBuf,
            //             found
            //         );
            //         Mat temp = view.Clone();
            //         Cv2.ImShow("Image View", view);
            //         Cv2.WaitKey(500);
            //     }
            // }
        }
        Mat[] rvecs = new Mat[0];
        Mat[] tvecs = new Mat[0];
    }

    void Update()
    {
    }
}
