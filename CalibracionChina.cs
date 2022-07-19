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

    static int winSize = 11;

    bool found = false;

    Size imageSize = new Size();

    List<Point2f[]> imagesPoints = new List<Point2f[]>();

    Mat cameraMatrix = new Mat();

    Mat distCoeffs = new Mat();

    void Start()
    {
        List<string> imagesList =
            new List<string>()
            {
                "Assets/Caracterizacion_MB/2/IMG_20220311_171340.jpg",
                "Assets/Caracterizacion_MB/2/IMG_20220311_171338.jpg",
                "Assets/Caracterizacion_MB/2/IMG_20220311_171336.jpg",
                "Assets/Caracterizacion_MB/2/IMG_20220311_171007.jpg",
                "Assets/Caracterizacion_MB/2/IMG_20220311_170810.jpg",
                "Assets/Caracterizacion_MB/2/IMG_20220311_171348.jpg",
                "Assets/Caracterizacion_MB/2/IMG_20220311_171435.jpg",
                "Assets/Caracterizacion_MB/2/IMG_20220311_171435_1.jpg",
                "Assets/Caracterizacion_MB/2/IMG_20220311_171436.jpg",
                "Assets/Caracterizacion_MB/2/IMG_20220311_171437.jpg"
            };

        for (int i = 0; i < imagesList.Count; i++)
        {
            Mat[] imagesPointsM = new Mat[imagesList.Count];

            byte[] fileData = File.ReadAllBytes(imagesList[i]);
            var tex = new Texture2D(2, 2);
            tex.LoadImage (fileData);
            Mat view = OpenCvSharp.Unity.TextureToMat(tex);
            if (!view.Empty())
            {
                imageSize = view.Size();
                Point2f[] pointBuf;
                found =
                    Cv2
                        .FindChessboardCorners(view,
                        BoardSize,
                        out pointBuf,
                        ChessboardFlags.AdaptiveThresh |
                        ChessboardFlags.NormalizeImage);
                if (found == true)
                {
                    var criteria =
                        new TermCriteria(CriteriaType.Eps |
                            CriteriaType.MaxIter,
                            30,
                            0.001);
                    Mat viewGray = new Mat();
                    Cv2.CvtColor(view, viewGray, ColorConversionCodes.BGR2GRAY);
                    Cv2
                        .CornerSubPix(viewGray,
                        pointBuf,
                        new Size(winSize, winSize),
                        new Size(-1, -1),
                        criteria);
                    imagesPoints.Add (pointBuf);
                    Debug.Log("Intento" + pointBuf);
                    Mat p =
                        new Mat(BoardSize.Width, 1, MatType.CV_64F, pointBuf);
                    imagesPointsM[i] = p;

                    Cv2.DrawChessboardCorners (
                        view,
                        BoardSize,
                        pointBuf,
                        found
                    );
                    Mat temp = view.Clone();
                    Cv2.WaitKey(500);
                }
            }
        }
        Mat[] rvecs = new Mat[0];
        Mat[] tvecs = new Mat[0];
    }

    void Update()
    {
    }
}
