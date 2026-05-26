using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIScoreBoard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] awayScore;
    [SerializeField] private TextMeshProUGUI[] homeScore;
    [SerializeField] private TextMeshProUGUI[] awayRBH;
    [SerializeField] private TextMeshProUGUI[] homeRBH;

    [SerializeField] private TextMeshProUGUI awayTeam;
    [SerializeField] private TextMeshProUGUI homeTeam;


    public void InitScoreBoard(string _awayTeam, string _homeTeam)
    {
        for(int i = 0; i < awayScore.Length; i++)
        {
            awayScore[i].text = "0";
            awayScore[i].gameObject.SetActive(i == 0);
        }

        for (int i = 0; i < homeScore.Length; i++)
        {
            homeScore[i].text = "0";
            homeScore[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < awayRBH.Length; i++)
        {
            awayRBH[i].text = "0";
            awayRBH[i].gameObject.SetActive(true);
        }

        for (int i = 0; i < homeRBH.Length; i++)
        {
            homeRBH[i].text = "0";
            homeRBH[i].gameObject.SetActive(true);
        }

        awayTeam.text = _awayTeam;
        homeTeam.text = _homeTeam;
    }


    public void BoardUpdate(int[] away, int[] home, int inning, bool bTopInning, bool bEnd)
    {

    }

    public void BoardUpdate(int away, int home)
    {
        
    }


    string GetBoardValue(GameObject obj, int value)
    {
        
        return value.ToString();
        
    }

}
