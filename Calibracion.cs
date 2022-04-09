using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using OpenCvSharp;
using System;
using System.Globalization;

public class Calibracion : MonoBehaviour
{
    // Start is called before the first frame update
    private List<decimal> DatosCamara = new List<decimal>();
    CultureInfo cultures = new CultureInfo("en-US");
    void Start()
    {
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

        
	    Mat cameraMatrix =  new Mat(3, 3, MatType.CV_64F);
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
	    distCoeffs.Set(4, 0, 0);

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
