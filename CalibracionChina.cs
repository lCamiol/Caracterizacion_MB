using System.Collections;
using UnityEngine;
using OpenCvSharp;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using System.Globalization;
using System.Threading.Tasks;


public class CalibracionChina : MonoBehaviour
{
    static int BoardSize_Width = 9;
    static int BoardSize_Height = 30;
    static Size BoardSize = new Size(BoardSize_Width, BoardSize_Height);
    static int SquareSize = 5;
    static int winSize = 11;
    

    void Start()
    {
        List<string> imagesList = new List<string>() {
                "Assets/Caracterizacion_MB/2/IMG_20220311_170745.jpg",
                "Assets/Caracterizacion_MB/2/IMG_20220311_170747.jpg",
                "Assets/Caracterizacion_MB/2/IMG_20220311_170802.jpg",
                "Assets/Caracterizacion_MB/2/IMG_20220311_170806.jpg",
                "Assets/Caracterizacion_MB/2/IMG_20220311_170810.jpg"
            };
        List<Point2f[]> imagesPoints = new List<Point2f[]>();
        Mat cameraMatrix = new Mat(), distCoeffs = new Mat();
        Size imageSize = new Size();
        bool found = false;

        Mat[] imagesPointsM = new Mat[imagesList.Count];

        for (int i = 0; i < imagesList.Count; i++)
        {
            Mat view = new Mat(imagesList[i]);
            if (!view.Empty())
            {
                imageSize = view.Size();
                Point2f[] pointBuf;

                found = Cv2.FindChessboardCorners(view, BoardSize, out pointBuf, ChessboardFlags.AdaptiveThresh | ChessboardFlags.NormalizeImage);
                if (found == true)
                {
                    var criteria = new TermCriteria(CriteriaType.Eps | CriteriaType.MaxIter, 30, 0.001);
                    Mat viewGray = new Mat();
                    Cv2.CvtColor(view, viewGray, ColorConversionCodes.BGR2GRAY);
                    Cv2.CornerSubPix(viewGray, pointBuf, new Size(winSize, winSize), new Size(-1, -1), criteria);
                    imagesPoints.Add(pointBuf);
                    Mat p = Mat.FromArray<Point2f>(pointBuf);
                    imagesPointsM[i] = p;

                    Cv2.DrawChessboardCorners(view, BoardSize, pointBuf, found);
                    Mat temp = view.Clone();
                    Cv2.ImShow("Image View", view);
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
