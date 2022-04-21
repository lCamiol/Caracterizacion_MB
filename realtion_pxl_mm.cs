using System.Collections;
using UnityEngine;
using OpenCvSharp;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;

public class realtion_pxl_mm : MonoBehaviour
{
    // Start is called before the first frame update
    private List<Double> radios = new List<Double>();
    void Start()
    {
        Mat inteto10= matImageFile("Assets/Caracterizacion_MB/px6.jpg");
        Cv2.ImShow("procesada", inteto10);
        Deteccion(inteto10);
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
            tex.LoadImage(fileData); // this will auto-resize the 2,2 texture dimensions.
            matResult = OpenCvSharp.Unity.TextureToMat(tex);
            Cv2.Resize(matResult, matResult, new Size(400,394));
            Cv2.CvtColor(matResult, matResult, ColorConversionCodes.BGR2GRAY);
            Cv2.MedianBlur(matResult, matResult, 13);
            Size ksize1 = new Size(3, 3);
            Cv2.GaussianBlur(matResult, matResult, ksize1, 180);

            //rellenar agujeros
            Point inicio = new Point(0, 0);
            Cv2.FloodFill(matResult, inicio, 255);
            Mat Kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(8 , 8));
            Mat Kernel2 = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(6, 6));
            Cv2.Dilate(matResult, matResult, Kernel);  
            Cv2.Erode(matResult, matResult, Kernel2);
            // Cv2.Threshold(matResult, matResult, 70, 255, ThresholdTypes.Binary);
            }
        return matResult;
    }

    private void Deteccion (Mat frame){
        
        CircleSegment[] circles = Cv2.HoughCircles(frame, HoughMethods.Gradient, 0.5, 15, 12, 14, 10, 50);
        Mat burbujas_detetadas = new Mat(400, 394, MatType.CV_8UC1, 1);
        foreach (CircleSegment circle in circles)
        {
            Cv2.Circle(burbujas_detetadas, (int)circle.Center.X, (int)circle.Center.Y, (int)circle.Radius, new Scalar(255, 255, 255));
            Cv2.PutText(burbujas_detetadas, circle.Radius.ToString(), new Point((int)circle.Center.X-60,(int)circle.Center.Y+30), HersheyFonts.HersheySimplex, 1, 255);
            Cv2.ImShow("circulos", burbujas_detetadas);
            radios.Add(circle.Radius);
            
        }
        double promedio = radios.Average();
        Debug.Log(promedio);

    }

}
