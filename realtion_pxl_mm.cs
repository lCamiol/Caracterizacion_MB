using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenCvSharp;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class realtion_pxl_mm : MonoBehaviour
{
    // Start is called before the first frame update
    private List<Double> radios = new List<Double>();

    void Start()
    {
        Mat inteto10 = matImageFile("Assets/Caracterizacion_MB/IM1.jpg");
        Cv2.ImShow("procesada", inteto10);
        Deteccion (inteto10);
    }

    // Update is called once per frame
    void Update()
    {
    }

    private static Mat matImageFile(string filePath)
    {
        Mat matResult = null;
        if (File.Exists(filePath))
        {
            // load into Mat type. Hack: workaround for Cv2.ImRead() being broke.
            byte[] fileData = File.ReadAllBytes(filePath);
            var tex = new Texture2D(2, 2);
            tex.LoadImage (fileData); // this will auto-resize the 2,2 texture dimensions.
            matResult = OpenCvSharp.Unity.TextureToMat(tex);
          /*  Cv2
                .Resize(matResult,
                matResult,
                new Size((int)(tex.width / 10), (int)(tex.height / 10)));*/
            Cv2.CvtColor(matResult, matResult, ColorConversionCodes.BGR2GRAY);
            /*Cv2.MedianBlur(matResult, matResult, 13);
            Size ksize1 = new Size(3, 3);
            Cv2.GaussianBlur(matResult, matResult, ksize1, 180);*/

            //rellenar agujeros
            Point inicio = new Point(0, 0);
            Cv2.FloodFill(matResult, inicio, 255);
            Mat Kernel =
                Cv2.GetStructuringElement(MorphShapes.Rect, new Size(11, 11));
           /* Mat Kernel2 =
                Cv2.GetStructuringElement(MorphShapes.Rect, new Size(6, 6));*/
            Cv2.Dilate(matResult, matResult, Kernel);
           /* Cv2.Erode(matResult, matResult, Kernel2);*/
            // Cv2.Threshold(matResult, matResult, 70, 255, ThresholdTypes.Binary);
        }
        return matResult;
    }

    private void Deteccion(Mat frame)
    {
        CircleSegment[] circles =
            Cv2
                .HoughCircles(frame,
                HoughMethods.Gradient,
                1,
                150,
                12,
                14,
                30,
                60);
        Mat burbujas_detetadas = new Mat(720, 1280, MatType.CV_8UC1, 1);
        foreach (CircleSegment circle in circles)
        {
            Cv2
                .Circle(burbujas_detetadas,
                (int) circle.Center.X,
                (int) circle.Center.Y,
                (int) circle.Radius,
                new Scalar(255, 255, 255));
            Cv2
                .PutText(burbujas_detetadas,
                (circle.Radius ).ToString(),
                new Point((int) circle.Center.X, (int) circle.Center.Y + 30),
                HersheyFonts.HersheySimplex,
                0.5,
                255);
            Cv2.ImShow("circulos", burbujas_detetadas);
            radios.Add(circle.Radius);
        }
        double promedio = radios.Average();
        Debug.Log(promedio*2);
    }
}
