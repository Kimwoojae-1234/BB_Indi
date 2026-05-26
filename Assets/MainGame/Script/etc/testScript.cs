using UnityEngine;
using System.Collections;
using Spine.Unity;

public class testScript : MonoBehaviour {

    //public Camera camera;
    //public SpriteRenderer spr;

    public GameObject [] spineObj;

    void Awake()
    {
        //Application.targetFrameRate = 60;
        step = 0;
    }

    int step = 0;
    
    void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.Space) == true)
        {
            spr.sprite = MakeCaptureSprite();
        }*/

        if (Input.GetMouseButtonDown(0) == true)
        {
            if (step == 4)
            {
                for (int i = 0; i < 4; i++) spineObj[i].gameObject.SetActive(false);
                step = 0;
            }
            else
            {
                spineObj[step].gameObject.SetActive(true);
                step++;
            }

        }
    }

    /*
    private Sprite MakeCaptureSprite()
    {
        RenderTexture tempRT = new RenderTexture(1280, 720, 24);
        camera.targetTexture = tempRT;
        camera.Render();
                
        Texture2D virtualPhoto = new Texture2D(1280, 720, TextureFormat.RGB24, false);    
        RenderTexture.active = tempRT;
        virtualPhoto.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
        virtualPhoto.Apply();
        RenderTexture.active = null;
        camera.targetTexture = null;

        return Sprite.Create(virtualPhoto, new Rect(0, 0, 1280, 720), new Vector2(0.5f, 0.5f));

    }
    */


}
