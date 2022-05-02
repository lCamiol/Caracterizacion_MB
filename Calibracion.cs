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

public class Calibracion : MonoBehaviour
 
{

   
    // Start is called before the first frame update
    private List<decimal> DatosCamara = new List<decimal>();
    CultureInfo cultures = new CultureInfo("en-US");
    void Start()
    {
        byte[] fileData = File.ReadAllBytes("Assets/Caracterizacion_MB/PruebaCalib.png");
        var tex = new Texture2D(2, 2);
        tex.LoadImage(fileData); // this will auto-resize the 2,2 texture dimensions.
        Mat map1 = OpenCvSharp.Unity.TextureToMat(tex);
        Cv2.Resize(map1, map1, new Size(1920, 1080));

        string path = "Assets/Caracterizacion_MB/Params.txt"; 
        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(path);
        //Debug.Log(path);
        // Debug.Log(reader.ReadToEnd());
        //reader.Close();


        while (!reader.EndOfStream)
        {
            string fila = reader.ReadLine();
            string[] arreglo = fila.Split(' ');
            for (int i = 0; i < arreglo.Length; i++)
                {
                    try{
                        decimal param= Convert.ToDecimal(arreglo[i], cultures);
                        DatosCamara.Add(param);
                    }catch{
                        break;
                    }
                }        
        }


        /*Mat cameraMatrix =  new Mat(3, 3, MatType.CV_64F);
        cameraMatrix.Set(0, 0, DatosCamara[4]);
        cameraMatrix.Set(0, 1, DatosCamara[5]);
        cameraMatrix.Set(0, 2, DatosCamara[6]);;
        cameraMatrix.Set(1, 0, DatosCamara[7]);
        cameraMatrix.Set(1, 1, DatosCamara[8]);
        cameraMatrix.Set(1, 2, DatosCamara[9]);
        cameraMatrix.Set(2, 0, DatosCamara[10]);
        cameraMatrix.Set(2, 1, DatosCamara[11]);
        cameraMatrix.Set(2, 2, DatosCamara[12]);

        Mat distCoeffs = new Mat(5, 1, MatType.CV_64F);
        distCoeffs.Set(0, 0, DatosCamara[0]);
        distCoeffs.Set(1, 0, DatosCamara[1]);
        distCoeffs.Set(2, 0, DatosCamara[2]);
        distCoeffs.Set(3, 0, DatosCamara[3]);
        distCoeffs.Set(4, 0, 0);*/

        Mat cameraMatrix = new Mat(3, 3, MatType.CV_64F);
        cameraMatrix.Set(0, 0, 467.420998429215);
        cameraMatrix.Set(0, 1, 0);
        cameraMatrix.Set(0, 2, 0);
        cameraMatrix.Set(1, 0, 0);
        cameraMatrix.Set(1, 1, 467.353862656172);
        cameraMatrix.Set(1, 2, 0);
        cameraMatrix.Set(2, 0, 320.721360122971);
        cameraMatrix.Set(2, 1, 180.458856691086);
        cameraMatrix.Set(2, 2, 0);

        Mat distCoeffs = new Mat(5, 1, MatType.CV_64F);
        distCoeffs.Set(0, 0, 0.0229184164434659);
        distCoeffs.Set(1, 0, -0.122241761248509);
        distCoeffs.Set(2, 0, 0);
        distCoeffs.Set(3, 0, 0);
        distCoeffs.Set(4, 0, 0);

        //Mat.ForeachFunctionDouble(cameraMatrix);

        /*double [,] cameraMatrix = new double[3, 3];
        cameraMatrix[0, 0] = 467.420998429215;
        cameraMatrix[0, 1] = 0;
        cameraMatrix[0, 2] = 0;
        cameraMatrix[1, 0] = 0;
        cameraMatrix[1, 1] = 467.353862656172;
        cameraMatrix[1, 2] = 0;
        cameraMatrix[2, 0] = 320.721360122971;
        cameraMatrix[2, 1] = 180.458856691086;
        cameraMatrix[2, 2] = 1;

        double[,] distCoeffs = new double[5, 1];
        distCoeffs[0, 0] = 0.0229184164434659;
        distCoeffs[1, 0] = -0.122241761248509;
        distCoeffs[2, 0] = 0;
        distCoeffs[3, 0] = 0;
        distCoeffs[4, 0] = 0;*/


        Mat map2 = map1.Clone();
        Mat newcam1 = new Mat();

        //newcam1 = Cv2.GetOptimalNewCameraMatrix(cameraMatrix, distCoeffs, map1.Size(),1, map1.Size(), 0);
        //Cv2.InitUndistortRectifyMap(cameraMatrix, distCoeffs, new Mat(), newcam1, Size(700, 680), map1, map2);
        Cv2.Undistort(map1,map2,cameraMatrix, distCoeffs);

        Cv2.ImShow("Imagen normal", map2);
        //Cv2.ImShow("imagen calibrada", map2);
        //UnityEngine.Debug.Log("Matriz de distorsion" + cameraMatrix.ToString());
        UnityEngine.Debug.Log("coeficiente de dst"+ distCoeffs);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
   
}
