using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using Papae.UnitySDK.Extensions;

public class LeaderboardManager : MonoBehaviour
{
    [SerializeField] private RankEntry leaderboardPrefab;
    [SerializeField] private GameObject content;
    public GameObject Loading;
    public GameObject CheckNetWorkPanel;
    private bool hasLoadedLeaderboard = false;

    private static readonly string[] FootballerNames = new string[]
    {
        "Messi", "Ronaldo", "Neymar", "Mbappe", "Salah", "Kane", "Haaland", "Benzema", "Modric", "DeBruyne",
        "Lewandowski", "Vinicius", "Son", "Mane", "Griezmann", "Pogba", "Sterling", "Sancho", "Foden", "Pedri", "Lamine"
    };


    private void OnEnable()
    {
        PlayFabManager.Instance.OnLeaderboardFetched += OnLeaderboardFetched;
    }

    private void OnDisable()
    {
        PlayFabManager.Instance.OnLeaderboardFetched -= OnLeaderboardFetched;
    }

    public void DisplayLeaderboard()
    {
        if (hasLoadedLeaderboard)
            return;
            
        //Turn off loading if leaderboard isn't displayed in 5 secs
        if (content.transform.childCount <= 0)
        {
            Loading.SetActive(true);
            CheckNetWorkPanel.SetActive(false);
            Timer.Register(5f, () => {
                if (content.transform.childCount <= 0)
                {
                    Loading.SetActive(false);
                    CheckNetWorkPanel.SetActive(true);
                }
                
            });
        }

        if (!PlayFabClientAPI.IsClientLoggedIn())
            return;

        if (PlayFabManager.Instance?.LatestLeaderboardEntries != null)
        {
            var leaderboardEntries = PlayFabManager.Instance.LatestLeaderboardEntries;
            OnLeaderboardFetched(leaderboardEntries);
        }
        
        PlayFabManager.Instance?.FetchLeaderboard();        
    }

    private void OnLeaderboardFetched(List<PlayerLeaderboardEntry> leaderboardEntries)
    {
        // Clear old entries
        foreach (Transform child in content.transform)
            Destroy(child.gameObject);

        Loading.SetActive(false);
        CheckNetWorkPanel.SetActive(false);

        foreach (var entry in leaderboardEntries)
        {
            var rankEntry = Instantiate(leaderboardPrefab, content.transform);
            string displayName = entry.Profile.DisplayName;
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = GenerateRandomName();
                rankEntry.playerNameText.text = displayName;

                if (entry.PlayFabId == PlayFabManager.Instance?.PlayFabID)
                {
                    PlayFabManager.Instance?.SetDisplayName(displayName);
                }
            }
            else
            {
                rankEntry.playerNameText.text = displayName;
            }
            rankEntry.scoreText.text = entry.StatValue.ToString();
            rankEntry.UpdateRank(entry.Position + 1);

            // Highlight player's own entry
            if (entry.PlayFabId == PlayFabManager.Instance.PlayFabID)
            {
                rankEntry.GetComponent<Image>().color = new Color(0.8f, 0.9f, 1f, 1f);
            }

            if (entry.Profile.Locations != null && entry.Profile.Locations.Count > 0)
            {
                string countryCode = entry.Profile.Locations[0].CountryCode.ToString();
                if (!string.IsNullOrEmpty(countryCode))
                {
                    Sprite flag = Resources.Load<Sprite>("Flags/" + countryCode);
                    if (flag != null)
                        rankEntry.flag.sprite = flag;
                }
            }
        }
        hasLoadedLeaderboard = true;
    }

    private string GenerateRandomName()
    {
        string name = FootballerNames[UnityEngine.Random.Range(0, FootballerNames.Length)];
        int digits = UnityEngine.Random.Range(100, 1000);
        return name + digits;
    }
}