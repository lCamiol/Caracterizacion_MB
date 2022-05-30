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

public class Calibracion : MonoBehaviour
 
{

   
    // Start is called before the first frame update
    private List<decimal> DatosCamara = new List<decimal>();
    CultureInfo cultures = new CultureInfo("en-US");
    void Start()
    {
        byte[] fileData = File.ReadAllBytes("Assets/Caracterizacion_MB/Cam0.jpg");
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
                try
                {
                    decimal param = Convert.ToDecimal(arreglo[i], cultures);
                    DatosCamara.Add(param);
                    Debug.Log(param);
                }
                catch
                {
                    break;
                }
            }
        }


        /*Mat cameraMatrix =  new Mat(3, 3, MatType.CV_64FC1);
        cameraMatrix.Set(0, 0, DatosCamara[4]);
        cameraMatrix.Set(0, 1, DatosCamara[5]);
        cameraMatrix.Set(0, 2, DatosCamara[6]);;
        cameraMatrix.Set(1, 0, DatosCamara[7]);
        cameraMatrix.Set(1, 1, DatosCamara[8]);
        cameraMatrix.Set(1, 2, DatosCamara[9]);
        cameraMatrix.Set(2, 0, DatosCamara[10]);
        cameraMatrix.Set(2, 1, DatosCamara[11]);
        cameraMatrix.Set(2, 2, DatosCamara[12]);

        Mat distCoeffs = new Mat(5, 1, MatType.CV_64FC1);
        distCoeffs.Set(0, 0, DatosCamara[0]);
        distCoeffs.Set(1, 0, DatosCamara[1]);
        distCoeffs.Set(2, 0, DatosCamara[2]);
        distCoeffs.Set(3, 0, DatosCamara[3]);
        distCoeffs.Set(4, 0, 0);*/

        Mat cameraMatrix = new Mat(3, 3, MatType.CV_8UC3);
        cameraMatrix.Set(0, 0, 3062.7055460605);
        cameraMatrix.Set(0, 1, 0);
        cameraMatrix.Set(0, 2, 0);
        cameraMatrix.Set(1, 0, 0);
        cameraMatrix.Set(1, 1, 3078.00290144012);
        cameraMatrix.Set(1, 2, 0);
        cameraMatrix.Set(2, 0, 1786.52621963178);
        cameraMatrix.Set(2, 1, 1311.79605688144);
        cameraMatrix.Set(2, 2, 1);

        Mat distCoeffs = new Mat(5, 1, MatType.CV_8UC3);
        distCoeffs.Set(0, 0, 0.0626007343290323);
        distCoeffs.Set(1, 0, -0.166909764928163);
        distCoeffs.Set(2, 0, 0);
        distCoeffs.Set(3, 0, 0);
        distCoeffs.Set(4, 0, 0);


        /*var size = new Size(9, 6);
        var rms = 0.0;
        var camera = new MatOfDouble(Mat.Eye(3, 3, MatType.CV_64FC1));
        Cv2.CvtColor(map1, map1, ColorConversionCodes.BGR2GRAY);
        var criteria = new TermCriteria(CriteriaType.Eps | CriteriaType.MaxIter, 30, 0.001);
        var imgPoints = new List<MatOfPoint2f>();
        var objPoints = new List<MatOfPoint3f>();
        var distortion = new Mat();
        var frameSize = Size.Zero;
       
        Mat newcam1 = new Mat();
        //var objp = MatOfPoint3f.FromArray(Create3DChessboardCorners(size, 1));
        Cv2.FindChessboardCorners(map1, size, out Point2f[] corners);
        //objPoints.Add(objp);
        UnityEngine.Debug.Log("Esquinas"+ corners.Length);
        imgPoints.Add(MatOfPoint2f.FromArray(corners.ToArray()));
        //var corners2 = Cv2.CornerSubPix(map1, corners, size, size, criteria);
        //Cv2.DrawChessboardCorners(map1, map1.Size(), corners2, true);
        rms = Cv2.CalibrateCamera(objPoints, imgPoints, map1.Size(), camera, distortion, out var rvectors, out var tvectors, CalibrationFlags.UseIntrinsicGuess | CalibrationFlags.FixK5);
        newcam1 = Cv2.GetOptimalNewCameraMatrix(camera, distortion, map1.Size(),1, map1.Size(),out var roi);*/
        //Cv2.InitUndistortRectifyMap(cameraMatrix, distCoeffs, new Mat(), newcam1, Size(700, 680), map1, map2);
        /*Mat map2 = map1.Clone();
        UnityEngine.Debug.Log("Tipo" + map2);
        Cv2.Undistort(map1,map2, cameraMatrix, distCoeffs);
        UnityEngine.Debug.Log("Tipo" + map2);
        Cv2.ImShow("Imagen normal", map1);
        Cv2.ImShow("imagen calibrada", map2);
        //UnityEngine.Debug.Log("Matriz de distorsion" + cameraMatrix.ToString());
        //UnityEngine.Debug.Log("coeficiente de dst");*/


    }
    // Update is called once per frame
    void Update()
    {
        
    }
   
}
