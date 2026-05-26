using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultTeam : MonoBehaviour
{
    [SerializeField] private UIScoreBoard board;
    [SerializeField] private TextMeshProUGUI MyScore;
    [SerializeField] private TextMeshProUGUI OppScore;

    [SerializeField] private GameObject VictoryObj;
    [SerializeField] private GameObject DefeatedObj;
    [SerializeField] private GameObject DrawObj;

    [SerializeField] private ResultItem[] StatItem;

    [SerializeField] private Image MyLogo;
    [SerializeField] private Image OppLogo;
    [SerializeField] private TextMeshProUGUI MyTeamName;
    [SerializeField] private TextMeshProUGUI OppTeamName;



}
