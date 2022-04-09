using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Calibracion : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        string path = "Assets/Caracterizacion_MB/Params.txt"; 
        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(path);
        //Debug.Log(path);
        Debug.Log(reader.ReadToEnd());
        //reader.Close();
        
        while (!reader.EndOfStream)
        {
            string x = reader.ReadLine();
            Debug.Log(x);
        }
        Debug.Log(reader.EndOfStream);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
