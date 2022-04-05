using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Actualizar : MonoBehaviour
{
    public float Actualizar_start; //Los segundos por los quales comienza i la variable que utilizaremos para que vaya contando segundos.
    public float Actualizar_end; //Segundos que queremos que pasen para que cambie de escena
                                 // Update is called once per frame
    public string escena;
    void Update()
    {
        Actualizar_start += Time.deltaTime;//Función para que la variable tiempo_start vaya contando segundos.
        if (Actualizar_start >= Actualizar_end) //Si pasan los segundos que hemos puesto antes...
        {
            SceneManager.LoadScene(escena);
        }
    }

}
