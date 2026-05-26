//This script is Just used for the Click Demo.


using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CreateShockWave_OnClick : MonoBehaviour {

    

    private float MaxRadius = 0.5f;
    private float Speed = 1f;
    private float Amp = 1f;

    public Slider MaxRadiusSlider;
    public Text MaxRadiusText;
    public Slider SpeedSlider;
    public Text SpeedText;
    public Slider AmplitudeSlider;
    public Text AmplitudeText;



    //setting variables
    void FixedUpdate()
    {
        MaxRadius = MaxRadiusSlider.value;
        Speed = SpeedSlider.value;
        Amp = AmplitudeSlider.value;

        MaxRadiusText.text = "MaxRadius: " + (Mathf.Round(MaxRadius * 1000f) / 1000f).ToString();
        SpeedText.text = "Speed: " + (Mathf.Round(Speed * 1000f) / 1000f).ToString();
        AmplitudeText.text = "Amplitude: " + (Mathf.Round(Amp * 1000) / 1000f).ToString();
    }
	
	// Update is called once per frame
	void Update () 
    {
        if (Input.GetMouseButtonDown(0))
        {
            
            ShockWave.Get().StartIt(Input.mousePosition,MaxRadius,Speed, Amp);

//            //a Raycast can be used to determine where the ShockWave is as well
//            Ray screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);
//            RaycastHit RCH;
//            if (Physics.Raycast(screenRay,out RCH))
//            {
//                ShockWave.Get().StartIt(RCH.point,10f,5f,1f);
//            }
        }
	
	}
}
