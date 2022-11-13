using System.Collections;
using UnityEngine;
using OpenCvSharp;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System;

public class RecortarVideo : MonoBehaviour
{
    //Raw Image to Show Video Images [Assign from the Editor]
    public RawImage image;
    public RawImage imageDiametro;
    public RawImage imageVelocidad;
    //Video To Play [Assign from the Editor]
    public VideoClip videoToPlay;
    //Texto para mostrar tiempo 
    public Text status;
    public Text MBDatos;
    public Text VelocidadDatos;
    public Text DiametroDatos;
    public Text minDiTxt;
    public Text maxDiTxt;
    public Text minVelTxt;
    public Text maxVelTxt;
    public Text MBDatosVel;
    public Button btnHistograma;
    public Mat circulos = new Mat(700, 680, MatType.CV_8UC1, 1);
    public Mat diametro = new Mat(250, 1000, MatType.CV_8UC1, 1);
    public Mat velocidadGrafico = new Mat(170, 730, MatType.CV_8UC1, 1);
    //Video a reproducir
    private VideoPlayer videoPlayer;
    private VideoSource videoSource;
    private List<int[]> MBActual = new List<int[]>();
    private List<int[]> MBAnterior = new List<int[]>();
    private List<int[]> MBOrdenar = new List<int[]>();
    private List<Double> velocidadPromedioLote = new List<Double>();
    private List<Double> diametroPromedioLote = new List<Double>();
    private CircleSegment[] circles;
    List<Mat> lista_frames = new List<Mat>();
    [SerializeField] private GameObject Proces = null;
    [SerializeField] private GameObject Menu = null;
    [SerializeField] private GameObject diagDiametro = null;
    [SerializeField] private GameObject diagVelocidad = null;
    [SerializeField] private GameObject AcercaDe = null;
    //medicion Tiempo de ejecucion
    Stopwatch sw_total = new Stopwatch();
    Stopwatch sw_proceso = new Stopwatch();
    double conversion = 0.6 / (272.544);
    double t = 0.00104; //preguntar por wapp
    int numeroBurbuja = 0;
    int numeroBurbujaIF = 0;
    int posiconHisto = 0;
    int contador = 0;
    // Use this for initialization
    void Start()
    {
        mosMenu();
        //sw_total.Start();// Iniciar medicion de tiempo Total
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
            status.text = "Preparing Video";
            yield return null;
        }
        UnityEngine.Debug.Log("Done Preparing Video");
        status.text = "Done Preparing Video";

        //image.texture = videoPlayer.texture;

        //Play Video
        videoPlayer.Play();

        while (videoPlayer.isPlaying)
        {
            btnHistograma.interactable = false;
            UnityEngine.Debug.LogWarning("Video Time: " + Mathf.FloorToInt((float)videoPlayer.time));
            // extraer textura del video
            Texture mainTexture = videoPlayer.texture;
            //convertir de textura a textua2D
            Texture2D texture2D = toTexture2D(mainTexture);
            // convertir la textura a Mat para realizar procesamiento con opencv         
            Mat frame = OpenCvSharp.Unity.TextureToMat(texture2D);
            caracterizacion_MB(frame);
            // Texture original = OpenCvSharp.Unity.MatToTexture(frame);
            // image.texture = original;
            // lista_frames.Add(frame);
            // status.text = "Reproduciendo video";
            yield return null;
        }
        // UnityEngine.Debug.Log("total frames" + lista_frames.Count);
        // for (int i =0; i < 1044; i+=12 )
        // {
        //     if (i > lista_frames.Count)
        //     {
        //         break;
        //     }else
        //     {
        //         Mat frame = lista_frames[i];
        //         //Cv2.ImShow("original" + contador, frame);
                
        //     }
        // }
        UnityEngine.Debug.Log("numero" + contador);
        /*sw_total.Stop(); // Detener la medici n.
        tiempo_total.text = "Total " + sw_total.Elapsed.ToString("ss\\.fff") + " seg"; // Mostrar el tiempo total transcurriodo con un formato ss.000
        */
        btnHistograma.interactable = true;
        UnityEngine.Debug.Log("Done Playing Video");
    }

    private void caracterizacion_MB(Mat frame)
    {
        
        //Cv2.ImShow("Original " + contador, frame);
        Mat frameProcesado = procesamiento(frame);
        Texture original = OpenCvSharp.Unity.MatToTexture(frame);
            image.texture = original;
        //Cv2.ImShow("procesamiento "+ contador, frameProcesado);
        MBActual.Clear();
        //UnityEngine.Debug.Log("circulos llenos " + MBActual.Count);
        DeteccionMB(frameProcesado);
        //UnityEngine.Debug.Log("circulos vacios " + MBActual.Count);
        circulos = new Mat(700, 680, MatType.CV_8UC1, 1);
        //MBActual.ForEach(graficar);
        categorizacion();
      
        contador += 1;

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
        frame = calibracionCamara(frame);
        OpenCvSharp.Rect rectCrop = new OpenCvSharp.Rect(0, 0, 665, 680);
        frame = new Mat(frame, rectCrop);
        //convertir a escala de grises
        Cv2.CvtColor(frame, frame, ColorConversionCodes.BGR2GRAY);

        //Cv2.Threshold(frame, frame, 0, 255, ThresholdTypes.Otsu);


        //Filtro mediana //medfilt2 es propio de matlab
        Cv2.MedianBlur(frame, frame, 21);
        //Size ksize = new Size(7, 7);

        Cv2.Threshold(frame, frame, 120, 255, ThresholdTypes.Binary);

        //Cv2.FastNlMeansDenoising(frame, frame);

        //filtro Gause

        Size ksize1 = new Size(7, 7);
        Cv2.GaussianBlur(frame, frame, ksize1, 255);


        //////int ddepth = frame.Type().Depth;
        //////Cv2.Laplacian(frame, frame, ddepth, 7, 0.04);

        ////rellenar agujeros
        //Point inicio = new Point(0, 0);
        //Cv2.FloodFill(frame, inicio, 255);

        //////Histograma(frame);

        //Mat Kernel1 = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(5, 5));
        //Mat Kernel2 = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(6, 6));
        //Cv2.Dilate(frame, frame, Kernel1);
        //Cv2.Erode(frame, frame, Kernel2);
        //Cv2.Threshold(frame, frame, 120, 255, ThresholdTypes.Binary);


        ////filtro de canny
        //Cv2.Canny(frame, frame, 0, 255, 5, true);
        return frame;
    }

    private void DeteccionMB(Mat frame)
    {
        // implementacion Hough circles
        circles = Cv2.HoughCircles(frame, HoughMethods.Gradient, 0.5, 15, 12, 14, 5, 50);
        Mat burbujas_detetadas = new Mat(700, 680, MatType.CV_8UC1, 1);
        Mat burubja = new Mat(700, 680, MatType.CV_8UC1, 1);
        // UnityEngine.Debug.LogError(frame.Height);
        // recorrer cada circulo encontrado
        foreach (CircleSegment circle in circles)
        {
            //UnityEngine.Debug.LogError("cordenadas "+ (int)circle.Center.X+ " " +(int)circle.Center.Y);
            // validar si la burbuja llega desde abajo
            if ((int)circle.Center.Y >= 670)
            {
                numeroBurbujaIF = numeroBurbujaIF + 1;
                // crear arrelo con cordenadas de la burbuja
                int[] coordenadasDentro = { (int)circle.Center.X, (int)circle.Center.Y, (int)circle.Radius };
                Cv2.Circle(burbujas_detetadas, (int)circle.Center.X, (int)circle.Center.Y, (int)circle.Radius, new Scalar(255, 255, 255));
                // mostrar grafico de circulos
                //Cv2.ImShow("circulos detectados en if " + contador, burbujas_detetadas);
                // agregar a la lista de burbuja anterior para validar que lleguen desde abajo
                MBAnterior.Add(coordenadasDentro);
            }
            
            //Cv2.Circle(burubja, (int)circle.Center.X, (int)circle.Center.Y, (int)circle.Radius, new Scalar(255, 255, 255));
            //Cv2.ImShow("circulos detectados " + contador, burubja);
            // crear arrelo con cordenadas de la burbuja
            int[] coordenadas = { (int)circle.Center.X, (int)circle.Center.Y, (int)circle.Radius };
            // agregar a lista actual para validar el dezplazamiento por frame
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
            numeroBurbuja = 0;
            MBOrdenar.ForEach(graficar);// se recore Mb actual y por cada elemento se aplica la funcion grafica
            UnityEngine.Debug.Log("Cantidad  Ordenada" + MBOrdenar.Count );
            MBAnterior = new List<int[]>(MBOrdenar); // se colana la informacion de MBOrdenar a Mb anteriorS
            velocidad(MBOrdenar);// limpiar Mb actual para las burbujas del nuevo frame
            if (velocidadPromedioLote.Count > 0)
            {
                double velocidadLotes = velocidadPromedioLote.Average();
                double diametroLotes = diametroPromedioLote.Average();
                MBDatos.text = "" + numeroBurbujaIF + "";
                MBDatosVel.text = "" + numeroBurbujaIF + "";
                VelocidadDatos.text = "" + Math.Round(velocidadLotes, 2) + " mm/seg";
                DiametroDatos.text = "" + Math.Round(diametroLotes, 2) + " μm";
                List<Double> copia_velocidad = new List<Double>(velocidadPromedioLote);
                List<Double> copia_diametro = new List<Double>(diametroPromedioLote);
                copia_velocidad.Sort();
                Double minVel = copia_velocidad[0];
                Double maxVel = copia_velocidad[copia_velocidad.Count() - 1];
                minVelTxt.text = "" + Math.Round(minVel, 2) + " μm";
                maxVelTxt.text = "" + Math.Round(maxVel, 2) + " μm";
                copia_diametro.Sort();
                Double minDi = copia_diametro[0];
                Double maxDi = copia_diametro[copia_diametro.Count() - 1];
                minDiTxt.text = "" + Math.Round(minDi, 2) + " μm";
                maxDiTxt.text = "" + Math.Round(maxDi, 2) + " μm";
            }
            //sw_proceso.Stop(); // Detener la medici n.
            //tiempo_proceso.text = "Proceso " + sw_proceso.Elapsed.ToString("ss\\.fff") + " seg"; // Mostrar el tiempo total transcurriodo con un formato ss.000
            //sw_proceso.Reset();   // resetear el tiempo 
        }
        MBActual.Clear(); // limpiar Mb ordenara para las burbujas nuevas
        MBOrdenar.Clear();// Detener la medici n.
    }

    private void Histograma(Mat frame)
    {
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
        //Cv2.ImShow("histogram", result);
    }

    private void graficar(int[] burbuja_Actual)
    {
    //UnityEngine.Debug.Log("coor "+ (int)burbuja_Actual[0] + " "  +(int)burbuja_Actual[1]+ " " +(int)burbuja_Actual[2]);
    // funcion utilizada para grafifcar cirulos, Mat para dibujar, coordenada x, cordenada y, radio, escala
        Cv2.Circle(circulos, (int)burbuja_Actual[0], (int)burbuja_Actual[1], (int)burbuja_Actual[2], new Scalar(255, 255, 255));
        numeroBurbuja = numeroBurbuja + 1;
        Cv2.PutText(circulos, numeroBurbuja.ToString(), new Point(burbuja_Actual[0], burbuja_Actual[1]), HersheyFonts.HersheySimplex, 1, 255);
        // mostrar grafico de circulos
        //Cv2.ImShow("circulos " + contador, circulos);
        // convertir el Mat circulos a textura
        Texture proceso = OpenCvSharp.Unity.MatToTexture(circulos);
        // subir textura en unity
        //image.texture = proceso;
    }

    private void seguimiento(List<int[]> MB)
    {
        //tiempo_total.text = "" + MB.Count();
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
            // UnityEngine.Debug.Log("distancia "+ calculo_distancia + " X " +  dX + " Y "+ dY );
            // agregar la distancia a la lista
            distancias.Add(calculo_distancia);
        }
        // copia de los elementos de la lista distancia
        List<float> copia_distancias = new List<float>(distancias);
        // organizar lista
        copia_distancias.Sort();
        // almacenar y obtener el valor minimo
        float distancia_minima = copia_distancias[0];
        // UnityEngine.Debug.LogError("distancia minima " + distancia_minima);
        if (distancia_minima > 7) { return; }
        else
        {
            // obtener ubicacion orginal de la lista original de distancias
            var ubucacion_burbuja = distancias.IndexOf(distancia_minima);
            //distancias.Clear();
            //UnityEngine.Debug.Log("ubicacion distancia minima" + ubucacion_burbuja);
            // obtener ubicacion de las cordenadas en Mb actual
            int[] burbuja = MBActual[ubucacion_burbuja];
            // almacenar las cordenadas de la burbuja
            int[] coordenadas_burbuja_actual_ordenada = { burbuja[0], burbuja[1], burbuja[2] };
            //UnityEngine.Debug.Log("coordenadas" + burbuja[0] + " "+  burbuja[1]+ " " +burbuja[2]);
            // guardar la informacion en MbOrdenar
            MBOrdenar.Add(coordenadas_burbuja_actual_ordenada);
        }
    }
    public void velocidad(List<int[]> Burbujas_Vel_Ord)
    {
        List<Double> velocidadPromedio = new List<Double>();
        List<Double> diamtreoPromedio = new List<Double>();
    
        double velocidadProm = 0;
        double diametroProm = 0;
        int posicion = 0; //recorrer de forma parelela los dos arreglos
        double t = 0.0021; //preguntar por wapp
        //lista auxiliar para almacenar distancias
        List<float> Listdistancia = new List<float>();
        // se recorre la lista y se aplica el metodo calcular_velocidad
        Burbujas_Vel_Ord.ForEach(calcular_velocidad);
        // metodo inicializado localmente recibe el valor de cada elemento de la lista
        UnityEngine.Debug.Log("Num velocidad " + Burbujas_Vel_Ord.Count);
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
            //UnityEngine.Debug.Log("velocidad " + contador +" " + calculo_distancia);
            double Velocidad = (conversion * calculo_distancia) / t;
            velocidadPromedio.Add(Velocidad);
            diamtreoPromedio.Add(coordenadas_burbuja_anterior[2] * 2 * conversion * 1000);

            // Aumentar la posicion
            posicion = +1;
            //UnityEngine.Debug.Log("velocidad "  + posicion +" "+ Velocidad);
            //UnityEngine.Debug.Log("Tama o " + posicion + " " + (coordenadas_burbuja_ord[2]*2));
            //UnityEngine.Debug.Log("velocidad Promedio" + velocidadProm);
            //UnityEngine.Debug.Log("Tama o Promedio" + diametroProm);
        }
        if (diamtreoPromedio.Count > 0)
        {
            posiconHisto = posiconHisto + 1;
            diametroProm = diamtreoPromedio.Average();
            diametroPromedioLote.Add(diametroProm);
            Cv2.PutText(diametro, "o", new Point(contador * 12, diametro.Height - Math.Round(diametroProm, 2)), HersheyFonts.HersheySimplex, 1, 255);
            // Cv2.PutText(diametro, Math.Round(diametroProm, 2).ToString(), new Point(contador  * 12, diametro.Height- diametroProm - 10), HersheyFonts.HersheySimplex, 0.5, 173);

            // Cv2.ImShow("diametros ", diametro);
            Texture histoDiametro = OpenCvSharp.Unity.MatToTexture(diametro);
            imageDiametro.texture = histoDiametro;
        }
        if (velocidadPromedio.Count > 0)
        {
            velocidadProm = velocidadPromedio.Average();
            velocidadPromedioLote.Add(velocidadProm);
            Cv2.PutText(velocidadGrafico, "->", new Point(posiconHisto * 12, velocidadGrafico.Height - Math.Round(velocidadProm*8, 2)), HersheyFonts.HersheySimplex, 0.5, 255);
            // Cv2.PutText(velocidadGrafico, Math.Round(velocidadProm, 2).ToString(), new Point(posiconHisto * 12, velocidadGrafico.Height - Math.Round(velocidadProm * 10, 2) - 10), HersheyFonts.HersheySimplex, 0.2, 255);

            //Cv2.ImShow("velocidad", velocidadGrafico);
            Texture histoVelocidad = OpenCvSharp.Unity.MatToTexture(velocidadGrafico);
            imageVelocidad.texture = histoVelocidad;
        }
             

    }

    public void mosProc()
    {

        Proces.SetActive(true);
        Menu.SetActive(false);
        diagDiametro.SetActive(false);
        diagVelocidad.SetActive(false);
        AcercaDe.SetActive(false);
        Application.runInBackground = true;
        StartCoroutine(playVideo());
    }

    public void mosMenu()
    {

        Proces.SetActive(false);
        Menu.SetActive(true);
        AcercaDe.SetActive(false);
        diagDiametro.SetActive(false);
        diagVelocidad.SetActive(false);
    }

    
    public void mosDiametro()
    {

        Proces.SetActive(false);
        Menu.SetActive(false);
        diagDiametro.SetActive(true);
        diagVelocidad.SetActive(false);
        AcercaDe.SetActive(false);
        Application.runInBackground = true;
    }

    public void mosVelocidad()
    {

        Proces.SetActive(false);
        Menu.SetActive(false);
        diagDiametro.SetActive(false);
        diagVelocidad.SetActive(true);
        AcercaDe.SetActive(false);
        Application.runInBackground = true;
    }

    public void mosAcercaDe()
        {

            Proces.SetActive(false);
            Menu.SetActive(false);
            diagDiametro.SetActive(false);
            AcercaDe.SetActive(true);
            diagVelocidad.SetActive(false);
            Application.runInBackground = true;

        }

    private Mat calibracionCamara (Mat frame) {
        double[,] cameraMatrix =
        {
            { 3062.7055460605, 0, 1786.52621963178 },
            { 0, 3078.00290144012, 1311.79605688144 },
            { 0, 0, 1 }
        };

        double[] distCoeffs =
        { 0.0626007343290323, -0.166909764928163, 0, 0, 0 };

        Mat map2 = new Mat();
        Cv2
            .Undistort(frame,
            map2,
            InputArray.Create(cameraMatrix),
            InputArray.Create(distCoeffs));

        return map2;
    }

}