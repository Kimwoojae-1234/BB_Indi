/*
Creates and Manages the ShockWave
*/

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class ShockWave : MonoBehaviour {

    //this is the material that will store the ShockWave Shader
    public Material mat;

    public ShockWave() 
    {
        mat = new Material(Shader.Find("Custom/ShockWave"));//"Custom/ShockWave"));
    }

    //the Radius of the ShockWave
    protected float _radius;
    public float radius 
    {
      get { return _radius; }
      set { 
          _radius=value;
          mat.SetFloat("_Radius",_radius);
      }
    }

    //the MaxRadius of the ShockWave
    private float _maxRadius;
    public float maxRadius 
    {
        get { return _maxRadius; }
        set { 
            _maxRadius=value;
        }
    }

    //the Speed of the ShockWave
    private float _speed;
    public float speed 
    {
        get { return _speed; }
        set { 
            _speed=value;
        }
    }

    //the Amplitude of the ShockWave
    private float _amplitude;
    public float amplitude 
    {
        get { return _amplitude; }
        set { 
            _amplitude = value;
            mat.SetFloat("_Amplitude",_amplitude);
        }
    }

    //this if for the Vector2 Position ShockWave
    public void StartIt(Vector2 Position,float MaxRadius, float Speed, float Amplitude ) 
    {
        radius=  -0.2f;

        maxRadius = MaxRadius;
        speed = Speed;
        amplitude = Amplitude;

        Vector2 V2 = Camera.main.ScreenToViewportPoint(Position);

        mat.SetFloat("_CenterX",V2.x);
        mat.SetFloat("_CenterY",V2.y);
        mat.SetFloat("_ScreenRatio", (int)Screen.width/(float)Screen.height );

        StartCoroutine("Processing");

    }


    //this if for the Vector3 Position ShockWave
    public void StartIt(Vector3 Position,float MaxRadius, float Speed, float Amplitude ) 
    {
        //assign values to variables
        radius=  -0.2f;
        maxRadius = MaxRadius;
        speed = Speed;
        amplitude = Amplitude;

        Vector2 V2 = Camera.main.WorldToViewportPoint(Position);


        mat.SetFloat("_CenterX",V2.x);
        mat.SetFloat("_CenterY",V2.y);
        mat.SetFloat("_ScreenRatio", (int)Screen.height/(float)Screen.width );

        StartCoroutine("Processing");

    }


    public void StartItCustom(Camera camera, Vector3 Position, float MaxRadius, float Speed, float Amplitude)
    {
        //assign values to variables
        radius = -0.2f;
        maxRadius = MaxRadius;
        speed = Speed;
        amplitude = Amplitude;

        Vector2 V2 = camera.WorldToViewportPoint(Position);


        mat.SetFloat("_CenterX", V2.x);
        mat.SetFloat("_CenterY", V2.y);
        mat.SetFloat("_ScreenRatio", (int)Screen.height / (float)Screen.width);

        StartCoroutine("Processing");

    }

    //this if for the Vector3 Position ShockWave
    public void StartIt(Vector3 Position, bool IsScreenPosition,float MaxRadius, float Speed, float Amplitude ) 
    {
        //assign values to variables
        radius=  -0.2f;
        maxRadius = MaxRadius;
        speed = Speed;
        amplitude = Amplitude;

        Vector2 V2;
        if (IsScreenPosition)
        {
            V2 = Camera.main.ScreenToViewportPoint(Position);
        }
        else
        {
            V2 = Camera.main.WorldToViewportPoint(Position);
        }


        mat.SetFloat("_CenterX",V2.x);
        mat.SetFloat("_CenterY",V2.y);
        mat.SetFloat("_ScreenRatio", (int)Screen.height/(float)Screen.width );

        StartCoroutine("Processing");

    }



    //this processes the ShockWave over time
    private IEnumerator Processing()
    {
        while (radius < maxRadius * 0.995f)
        {
            //move the radius to the MaxRadius using lerp
            radius = Mathf.Lerp(radius,maxRadius,Time.deltaTime * speed);

            //die down the amplitude while the radius raises
            amplitude *= (1f - (Mathf.Clamp(radius,0f,100f)/maxRadius));

            yield return null;
        }

        //destory after processing
        Destroy(this);
    }


    //this attaches the script to the camera
    static public ShockWave Get() 
    {
        ShockWave SW=Camera.main.gameObject.AddComponent<ShockWave>(); 
        return SW;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest) 
    {
        if (mat == null)
        {
            return;
        }

        if(mat != null)
        {
            
        }

        Graphics.Blit(src, dest, mat);
    }
}
