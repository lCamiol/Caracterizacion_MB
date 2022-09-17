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

    double desviacionEstandar = 0;

    void Start()
    {
        Mat inteto10 =
            matImageFile("Assets/Caracterizacion_MB/PruebaNylon_025.jpg");
        Debug.Log (inteto10);
        Cv2.ImShow("procesada", inteto10);
        Deteccion (inteto10);
    }

    // Update is called once per frame
    void Update()
    {
    }

    private Mat matImageFile(string filePath)
    {
        Mat matResult = null;
        if (File.Exists(filePath))
        {
            // load into Mat type. Hack: workaround for Cv2.ImRead() being broke.
            byte[] fileData = File.ReadAllBytes(filePath);
            var tex = new Texture2D(2, 2);
            tex.LoadImage (fileData); // this will auto-resize the 2,2 texture dimensions.
            Mat matResult1 = OpenCvSharp.Unity.TextureToMat(tex);
            matResult = calibracionCamara(matResult1);

            // Cv2
            //     .Resize(matResult,
            //     matResult,
            //     new Size((int)(tex.width / 10), (int)(tex.height / 10)));
            Cv2.CvtColor(matResult, matResult, ColorConversionCodes.BGR2GRAY);
            // Cv2.MedianBlur(matResult, matResult, 1);
            // Mat Kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(2, 1.5));
            // Cv2.Dilate(matResult, matResult, Kernel);
            // Mat Kernel2 = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(1.5, 2));
            // Cv2.Erode(matResult, matResult, Kernel2);
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
                50,
                12,
                14,
                50,
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
                (circle.Radius*2).ToString(),
                new Point((int) circle.Center.X, (int) circle.Center.Y + 30),
                HersheyFonts.HersheySimplex,
                0.5,
                255);
            Cv2.ImShow("circulos", burbujas_detetadas);

            // Agregar desviación estandar (cada valor - el promedio)
            radios.Add(circle.Radius);
        }
        double promedio = radios.Average();
        foreach (double r in radios)
        {
            double desviacion = r - promedio;
            desviacionEstandar += Math.Pow(desviacion, 2);
            // Debug.Log("desviación"+ desviacion);
        }
        float desviacionTotal =
            Mathf.Sqrt((float) desviacionEstandar / radios.Count);
        Debug.Log("desviación total" + desviacionTotal);
        Debug.Log (promedio*2);
    }

    private Mat calibracionCamara(Mat frame)
    {
        double[,] cameraMatrix =
        {
            { 2.83943297e+03, 0, 1.90306605e+03 },
            { 0, 2.83339169e+03, 8.95584317e+02 },
            { 0, 0, 1 }
        };

        double[] distCoeffs =
        { -0.01311819, 0.37176549, 0.01207837, 0.00885673, -0.688254 };

        Mat map2 = new Mat();
        Cv2
            .Undistort(frame,
            map2,
            InputArray.Create(cameraMatrix),
            InputArray.Create(distCoeffs));

        return map2;
    }
}
