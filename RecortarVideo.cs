using System.Collections;
using UnityEngine;
using OpenCvSharp;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class RecortarVideo : MonoBehaviour
{
    //Raw Image to Show Video Images [Assign from the Editor]
    public RawImage image;
    //Video To Play [Assign from the Editor]
    public VideoClip videoToPlay;
    public Mat circulos = new Mat(680, 700, MatType.CV_8UC1, 1);
    //Texto para mostrar tiempo 
    public Text tiempo_total;
    public Text tiempo_proceso;

    //Video a reproducir
    private VideoPlayer videoPlayer;
    private VideoSource videoSource;
   
    private List<int[]> MBActual = new List<int[]>();
    private List<int[]> MBAnterior = new List<int[]>();
    private List<int[]> MBOrdenar = new List<int[]>();

    //medicion Tiempo de ejecucion
    Stopwatch sw_total = new Stopwatch();
    Stopwatch sw_proceso = new Stopwatch();
    double conversion = 5.79 * Mathf.Pow(10,-6);
    int numeroBurbuja = 0;
    // Use this for initialization
    void Start()
    {
        //sw_total.Start();// Iniciar medicion de tiempo Total
        Application.runInBackground = true;
        StartCoroutine(playVideo());

    }

    private void Update()
    {

    }

    IEnumerator playVideo()
    {
        //Add VideoPlayer to the GameObject
        videoPlayer = gameObject.AddComponent<VideoPlayer>();

        //Disable Play on Awake for both Video and Audio
        videoPlayer.playOnAwake = false;

        //We want to play from video clip not from url
        videoPlayer.source = VideoSource.VideoClip;

        //Set video To Play then prepare Audio to prevent Buffering
        videoPlayer.clip = videoToPlay;
        videoPlayer.Prepare();

        //Wait until video is prepared
        while (!videoPlayer.isPrepared)
        {
            UnityEngine.Debug.Log("Preparing Video");
            yield return null;
        }
        UnityEngine.Debug.Log("Done Preparing Video");

        //image.texture = videoPlayer.texture;

        //Play Video
        videoPlayer.Play();


        UnityEngine.Debug.Log("Playing Video");
        while (videoPlayer.isPlaying)
        {

            UnityEngine.Debug.LogWarning("Video Time: " + Mathf.FloorToInt((float)videoPlayer.time));
            caracterizacion_MB();
            yield return null;
        }

        /*sw_total.Stop(); // Detener la medici�n.
        tiempo_total.text = "Total " + sw_total.Elapsed.ToString("ss\\.fff") + " seg"; // Mostrar el tiempo total transcurriodo con un formato ss.000
        UnityEngine.Debug.Log("Done Playing Video");*/
    }

    private void caracterizacion_MB()
    {
        // extraer textura del video
        Texture mainTexture = videoPlayer.texture;
        //convertir de textura a textua2D
        Texture2D texture2D = toTexture2D(mainTexture);
        // convertir la textura a Mat para realizar procesamiento con opencv  
        //Texture2D texture2D = (Texture2D) mainTexture;
        //Assign the Texture from Video to RawImage to be displayed       
        Mat frame = OpenCvSharp.Unity.TextureToMat(texture2D);
        Mat frameProcesado = procesamiento(frame);
        DeteccionMB(frameProcesado);
        categorizacion();


        /*Texture proceso = OpenCvSharp.Unity.MatToTexture(frameProcesado);
        image.texture = proceso;
        Cv2.ImShow("procesamiento", frameProcesado);*/
    }

    Texture2D toTexture2D(Texture texture)
    {
        //se crea una textura con las dimeciones de preferencia para guardar la conversion
        //Texture2D texture2D = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        //Recorte de la imagen en 680*700
        Texture2D texture2D = new Texture2D(680, 700, TextureFormat.RGBA32, false);
        //se utiliza para empezar el metodo de renderizacion
        RenderTexture currentRT = RenderTexture.active;
        //se toma el video que se envio por parametro y se renderiza

        RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 32);
        Graphics.Blit(texture, renderTexture);
        //se remplaza por la textura ya renderizada

        RenderTexture.active = renderTexture;
        //se lee los nuevos pixeles con cordenadas de inicio y fin
        //texture2D.ReadPixels(new UnityEngine.Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.ReadPixels(new UnityEngine.Rect(300, 1, 800, 700), 0, 0);
         
        texture2D.Apply();
        //finalmente se remplaza 
        RenderTexture.active = currentRT;
        RenderTexture.ReleaseTemporary(renderTexture);
        //se retorna la textura en 2D
        return texture2D;
    }


    private Mat procesamiento(Mat frame)
    {
        //convertir a escala de grises
        Cv2.CvtColor(frame, frame, ColorConversionCodes.BGR2GRAY);


        //Cv2.Threshold(frame, frame, 0, 255, ThresholdTypes.Otsu);


        //Filtro mediana //medfilt2 es propio de matlab
        Cv2.MedianBlur(frame, frame, 13);
        //Size ksize = new Size(7, 7);

        //Cv2.FastNlMeansDenoising(frame, frame);

        //filtro Gause

        Size ksize1 = new Size(3, 3);
        Cv2.GaussianBlur(frame, frame, ksize1, 180);
        //int ddepth = frame.Type().Depth;
        //Cv2.Laplacian(frame, frame, ddepth, 7, 0.04);

        //rellenar agujeros
        Point inicio = new Point(0, 0);
        Cv2.FloodFill(frame, inicio, 255);

        //Histograma(frame);

        Mat Kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(4, 4));
        Mat Kernel2 = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(6, 6));
        Cv2.Dilate(frame, frame, Kernel);
        Cv2.Erode(frame, frame, Kernel2);
        Cv2.Threshold(frame, frame, 110, 255, ThresholdTypes.Binary);

        //filtro de canny
        Cv2.Canny(frame, frame, 0, 255, 5, true);
        return frame;
    }

    private void DeteccionMB(Mat frame)
    {
        // implementacion Hough circles
        CircleSegment[] circles = Cv2.HoughCircles(frame, HoughMethods.Gradient, 0.5, 5, 11, 14, 5, 20);
        // UnityEngine.Debug.LogError(frame.Height);
        // recorrer cada circulo encontrado
        foreach (CircleSegment circle in circles)
        {
            // UnityEngine.Debug.LogError((int)circle.Center.Y);
            // validar si la burbuja llega desde abajo
            if ((int)circle.Center.Y >= 680)
            {
                UnityEngine.Debug.Log("Burbuja");
                // crear arrelo con cordenadas de la burbuja
                int[] coordenadasDentro = { (int)circle.Center.X, (int)circle.Center.Y, (int)circle.Radius };
                // agregar a la lista de burbuja anterior para validar que lleguen desde abajo
                MBAnterior.Add(coordenadasDentro);
                UnityEngine.Debug.Log("total" + MBActual.Count);
            }
            // crear arrelo con cordenadas de la burbuja
            int[] coordenadas = { (int)circle.Center.X, (int)circle.Center.Y, (int)circle.Radius };
            // agregar a lista actual para validar el dezplazamiento pro frame
            MBActual.Add(coordenadas);
            //Cv2.Circle(circulos, (int)circle.Center.X, (int)circle.Center.Y, (int)circle.Radius, new Scalar(255, 255, 255));
        }

    }

    private void categorizacion()
    {
        //MBOrdenar.Clear(); 
        UnityEngine.Debug.Log("Cantidad Actual " + MBActual.Count + " Anterior " + MBAnterior.Count);
        // verificar que las MB anterior y actual se hayan registrado
        if (MBActual.Count > 0 && MBAnterior.Count > 0)
        {
            //sw_proceso.Start();// iniciar medicion de tiempo
            seguimiento(MBAnterior); // metodo utilizado para realizar el seguimiento de las MB enviando como parametro Mb anterior
            circulos = new Mat(700, 680, MatType.CV_8UC1, 1);// Mat auxiliar utilizado para graficar
            MBOrdenar.ForEach(graficar);// se recore Mb actual y por cada elemento se aplica la funcion grafica
            UnityEngine.Debug.Log("Cantidad  Ordenada" + MBOrdenar.Count);
            MBAnterior = new List<int[]>(MBOrdenar); // se colana la informacion de MBOrdenar a Mb anteriorS
            velocidad(MBOrdenar);// limpiar Mb actual para las burbujas del nuevo frame
            //sw_proceso.Stop(); // Detener la medici�n.
            //tiempo_proceso.text = "Proceso " + sw_proceso.Elapsed.ToString("ss\\.fff") + " seg"; // Mostrar el tiempo total transcurriodo con un formato ss.000
            //sw_proceso.Reset();   // resetear el tiempo 
        }
        MBActual.Clear(); // limpiar Mb ordenara para las burbujas nuevas
        MBOrdenar.Clear();// Detener la medici�n

    }

    private void Histograma(Mat frame) {
        // vaiable auxliar para generar el histograma
        Mat hist = new Mat();
        // Mat para guardar resltado final en forma grafica
        Mat result = Mat.Ones(new Size(256, 500), MatType.CV_8UC1);
        // se llama la funcion paa encontrar los puentos del histograma
        Cv2.CalcHist(new Mat[] { frame }, new int[] { 0 }, null, hist, 1, new int[] { 256 }, new Rangef[] { new Rangef(0, 256) });
        // se normaliza a los valores en el rago que se necesita
        Cv2.Normalize(hist, hist, 0, 255, NormTypes.MinMax);
        // se recorre cada punto  para graficar
        for (int i = 0; i < hist.Rows; i++)
        {
            // graficar linea respecto a cada punto encontrdo            Cv2.Line(result, new Point(i, 500), new Point(i, 500 - hist.Get<float>(i)), Scalar.White);

            Cv2.Line(result, new Point(i, 500), new Point(i, 500 - hist.Get<float>(i)), Scalar.White);
        }
        // mostrar histograma
        Cv2.ImShow("histogram", result);
    }

    private void graficar(int[] burbuja_Actual)
    {
        //UnityEngine.Debug.Log("coor�"+ (int)burbuja_Actual[0] + " "  +(int)burbuja_Actual[1]+ " " +(int)burbuja_Actual[2]);
        // funcion utilizada para grafifcar cirulos, Mat para dibujar, coordenada x, cordenada y, radio, escala
        Cv2.Circle(circulos, (int)burbuja_Actual[0], (int)burbuja_Actual[1], (int)burbuja_Actual[2], new Scalar(255, 255, 255));
        numeroBurbuja = numeroBurbuja + 1;
        Cv2.PutText(circulos, numeroBurbuja.ToString(), new Point(burbuja_Actual[0], burbuja_Actual[1]), HersheyFonts.HersheySimplex, 0.5, 255);
        // mostrar grafico de circulos
        Cv2.ImShow("circulos", circulos);
        // convertir el Mat circulos a textura
        Texture proceso = OpenCvSharp.Unity.MatToTexture(circulos);
        // subir textura en unity
        image.texture = proceso;
    }

    private void seguimiento(List<int[]> MB)
    {
        tiempo_total.text = "" + MB.Count();
        // REcorrer lista y por cada elemento aplicar distancia minima
        MB.ForEach(distanciaMinima);
    }

    private void distanciaMinima(int[] coordenadas_burbuja_anterior)
    {
        //lista auxiliar para almacenar distancias
        List<float> distancias = new List<float>();
        // se recorre la lista y se aplica el metodo calcular_distancias
        MBActual.ForEach(calcular_distancia);
        // metodo inicializado localmente recibe el valor de cada elemento de la lista
        void calcular_distancia(int[] coordenadas_burbuja_actual)
        {
            // metodo de distancia euclidiana, basado en triangulo rectangulo
            // calculo de la distancia en la cordenada X
            int dX = Mathf.Abs(coordenadas_burbuja_actual[0] - coordenadas_burbuja_anterior[0]);
            // calculo de la distancia en la cordenada Y
            int dY = Mathf.Abs(coordenadas_burbuja_actual[1] - coordenadas_burbuja_anterior[1]);
            // implementar formula de distancia euclidiana
            float calculo_distancia = Mathf.Sqrt((dX ^ 2) + (dY ^ 2));
            // agregar la distancia a la lista
            distancias.Add(calculo_distancia);
        }
        // copia de los elementos de la lista distancia
        List<float> copia_distancias = new List<float>(distancias);
        // organizar lista
        copia_distancias.Sort();
        // almacenar y obtener el valor minimo
        float distancia_minima = copia_distancias[0];
        // obtener ubicacion orginal de la lista original de distancias
        var ubucacion_burbuja = distancias.IndexOf(distancia_minima);
        //distancias.Clear();
        // obtenr ubicacion de las cordenadas en Mb acutal
        int[] burbuja = MBActual[ubucacion_burbuja];
        // almacenar las cordenadas de la burbuja
        int[] coordenadas_burbuja_actual_ordenada = { burbuja[0], burbuja[1], burbuja[2] };
        // guardar la informacion en MbOrdenar
        MBOrdenar.Add(coordenadas_burbuja_actual_ordenada);
    }
    public void velocidad(List<int[]> Burbujas_Vel)
    {
        int posicion = 0; //recorrer de forma parelela los dos arreglos
        double t = 0.0021; //preguntar por wapp
        //lista auxiliar para almacenar distancias
        List<float> Listdistancia = new List<float>();
        // se recorre la lista y se aplica el metodo calcular_velocidad
        Burbujas_Vel.ForEach(calcular_velocidad);
        // metodo inicializado localmente recibe el valor de cada elemento de la lista
        void calcular_velocidad(int[] coordenadas_burbuja_ord)
        {
            int[] coordenadas_burbuja_anterior = MBAnterior[posicion]; // almacenar cordenadas de burbuja anterior para calculo de distancia
            // metodo de distancia euclidiana, basado en triangulo rectangulo
            // calculo de la distancia en la cordenada X
            int dX = Mathf.Abs(coordenadas_burbuja_ord[0] - coordenadas_burbuja_anterior[0]);
            // calculo de la distancia en la cordenada Y
            int dY = Mathf.Abs(coordenadas_burbuja_ord[1] - coordenadas_burbuja_anterior[1]);
            // implementar formula de distancia euclidiana
            float calculo_distancia = Mathf.Sqrt((dX ^ 2) + (dY ^ 2));
            // Calculo de la velocidad
            double Velocidad = (conversion * calculo_distancia) / t;
            UnityEngine.Debug.Log("velocidad "+ Velocidad);
            // Aumentar la posicion
            posicion = +1;
        }


    }
}
