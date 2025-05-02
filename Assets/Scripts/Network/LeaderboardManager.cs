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
    string playfabID;

    private static readonly string[] FootballerNames = new string[]
    {
        "Messi", "Ronaldo", "Neymar", "Mbappe", "Salah", "Kane", "Haaland", "Benzema", "Modric", "DeBruyne",
        "Lewandowski", "Vinicius", "Son", "Mane", "Griezmann", "Pogba", "Sterling", "Sancho", "Foden", "Pedri", "Lamine"
    };

    #region CountryCodeWrapper
    public static Dictionary<string, string> CountryCodesToNames = new Dictionary<string, string>
    {
        { "AF", "Afghanistan" },
        { "AX", "�land Islands" },
        { "AL", "Albania" },
        { "DZ", "Algeria" },
        { "AS", "American Samoa" },
        { "AD", "Andorra" },
        { "AO", "Angola" },
        { "AI", "Anguilla" },
        { "AQ", "Antarctica" },
        { "AG", "Antigua and Barbuda" },
        { "AR", "Argentina" },
        { "AM", "Armenia" },
        { "AW", "Aruba" },
        { "AU", "Australia" },
        { "AT", "Austria" },
        { "AZ", "Azerbaijan" },
        { "BS", "Bahamas" },
        { "BH", "Bahrain" },
        { "BD", "Bangladesh" },
        { "BB", "Barbados" },
        { "BY", "Belarus" },
        { "BE", "Belgium" },
        { "BZ", "Belize" },
        { "BJ", "Benin" },
        { "BM", "Bermuda" },
        { "BT", "Bhutan" },
        { "BO", "Bolivia" },
        { "BQ", "Bonaire, Sint Eustatius, and Saba" },
        { "BA", "Bosnia and Herzegovina" },
        { "BW", "Botswana" },
        { "BV", "Bouvet Island" },
        { "BR", "Brazil" },
        { "IO", "British Indian Ocean Territory" },
        { "BN", "Brunei Darussalam" },
        { "BG", "Bulgaria" },
        { "BF", "Burkina Faso" },
        { "BI", "Burundi" },
        { "KH", "Cambodia" },
        { "CM", "Cameroon" },
        { "CA", "Canada" },
        { "CV", "Cape Verde" },
        { "KY", "Cayman Islands" },
        { "CF", "Central African Republic" },
        { "TD", "Chad" },
        { "CL", "Chile" },
        { "CN", "China" },
        { "CX", "Christmas Island" },
        { "CC", "Cocos Islands" },
        { "CO", "Colombia" },
        { "KM", "Comoros" },
        { "CG", "Congo-Brazzaville" },
        { "CD", "DRC" },
        { "CK", "Cook Islands" },
        { "CR", "Costa Rica" },
        { "CI", "C�te d'Ivoire" },
        { "HR", "Croatia" },
        { "CU", "Cuba" },
        { "CW", "Cura�ao" },
        { "CY", "Cyprus" },
        { "CZ", "Czech Republic" },
        { "DK", "Denmark" },
        { "DJ", "Djibouti" },
        { "DM", "Dominica" },
        { "DO", "Dominican Republic" },
        { "EC", "Ecuador" },
        { "EG", "Egypt" },
        { "SV", "El Salvador" },
        { "GQ", "Equatorial Guinea" },
        { "ER", "Eritrea" },
        { "EE", "Estonia" },
        { "ET", "Ethiopia" },
        { "FK", "Falkland Islands" },
        { "FO", "Faroe Islands" },
        { "FJ", "Fiji" },
        { "FI", "Finland" },
        { "FR", "France" },
        { "GF", "French Guiana" },
        { "PF", "French Polynesia" },
        { "TF", "French Southern Territories" },
        { "GA", "Gabon" },
        { "GM", "Gambia" },
        { "GE", "Georgia" },
        { "DE", "Germany" },
        { "GH", "Ghana" },
        { "GI", "Gibraltar" },
        { "GR", "Greece" },
        { "GL", "Greenland" },
        { "GD", "Grenada" },
        { "GP", "Guadeloupe" },
        { "GU", "Guam" },
        { "GT", "Guatemala" },
        { "GG", "Guernsey" },
        { "GN", "Guinea" },
        { "GW", "Guinea-Bissau" },
        { "GY", "Guyana" },
        { "HT", "Haiti" },
        { "HM", "Heard Island and McDonald Islands" },
        { "VA", "Vatican City"},
        { "HN", "Honduras" },
        { "HK", "Hong Kong" },
        { "HU", "Hungary" },
        { "IS", "Iceland" },
        { "IN", "India" },
        { "ID", "Indonesia" },
        { "IR", "Iran" },
        { "IQ", "Iraq" },
        { "IE", "Ireland" },
        { "IM", "Isle of Man" },
        { "IL", "Israel" },
        { "IT", "Italy" },
        { "JM", "Jamaica" },
        { "JP", "Japan" },
        { "JE", "Jersey" },
        { "JO", "Jordan" },
        { "KZ", "Kazakhstan" },
        { "KE", "Kenya" },
        { "KI", "Kiribati" },
        { "KP", "Korea (North)" },
        { "KR", "Korea (South)" },
        { "KW", "Kuwait" },
        { "KG", "Kyrgyzstan" },
        { "LA", "Lao People's Democratic Republic" },
        { "LV", "Latvia" },
        { "LB", "Lebanon" },
        { "LS", "Lesotho" },
        { "LR", "Liberia" },
        { "LY", "Libya" },
        { "LI", "Liechtenstein" },
        { "LT", "Lithuania" },
        { "LU", "Luxembourg" },
        { "MO", "Macao" },
        { "MK", "Macedonia" },
        { "MG", "Madagascar" },
        { "MW", "Malawi" },
        { "MY", "Malaysia" },
        { "MV", "Maldives" },
        { "ML", "Mali" },
        { "MT", "Malta" },
        { "MH", "Marshall Islands" },
        { "MQ", "Martinique" },
        { "MR", "Mauritania" },
        { "MU", "Mauritius" },
        { "YT", "Mayotte" },
        { "MX", "Mexico" },
        { "FM", "Micronesia" },
        { "MD", "Moldova" },
        { "MC", "Monaco" },
        { "MN", "Mongolia" },
        { "ME", "Montenegro" },
        { "MS", "Montserrat" },
        { "MA", "Morocco" },
        { "MZ", "Mozambique" },
        { "MM", "Myanmar" },
        { "NA", "Namibia" },
        { "NR", "Nauru" },
        { "NP", "Nepal" },
        { "NL", "Netherlands" },
        { "NC", "New Caledonia" },
        { "NZ", "New Zealand" },
        { "NI", "Nicaragua" },
        { "NE", "Niger" },
        { "NG", "Nigeria" },
        { "NU", "Niue" },
        { "NF", "Norfolk Island" },
        { "MP", "Northern Mariana Islands" },
        { "NO", "Norway" },
        { "OM", "Oman" },
        { "PK", "Pakistan" },
        { "PW", "Palau" },
        { "PS", "Palestine" },
        { "PA", "Panama" },
        { "PG", "Papua New Guinea" },
        { "PY", "Paraguay" },
        { "PE", "Peru" },
        { "PH", "Philippines" },
        { "PN", "Pitcairn" },
        { "PL", "Poland" },
        { "PT", "Portugal" },
        { "PR", "Puerto Rico" },
        { "QA", "Qatar" },
        { "RE", "R�union" },
        { "RO", "Romania" },
        { "RU", "Russia" },
        { "RW", "Rwanda" },
        { "BL", "Saint Barth�lemy" },
        { "SH", "Saint Helena" },
        { "KN", "Saint Kitts and Nevis" },
        { "LC", "Saint Lucia" },
        { "MF", "Saint Martin" },
        { "PM", "Saint Pierre and Miquelon" },
        { "VC", "Saint Vincent and the Grenadines" },
        { "WS", "Samoa" },
        { "SM", "San Marino" },
        { "ST", "Sao Tome and Principe" },
        { "SA", "Saudi Arabia" },
        { "SN", "Senegal" },
        { "RS", "Serbia" },
        { "SC", "Seychelles" },
        { "SL", "Sierra Leone" },
        { "SG", "Singapore" },
        { "SX", "Sint Maarten" },
        { "SK", "Slovakia" },
        { "SI", "Slovenia" },
        { "SB", "Solomon Islands" },
        { "SO", "Somalia" },
        { "ZA", "South Africa" },
        { "GS", "South Georgia and the South Sandwich Islands" },
        { "SS", "South Sudan" },
        { "ES", "Spain" },
        { "LK", "Sri Lanka" },
        { "SD", "Sudan" },
        { "SR", "Suriname" },
        { "SJ", "Svalbard and Jan Mayen" },
        { "SZ", "Swaziland" },
        { "SE", "Sweden" },
        { "CH", "Switzerland" },
        { "SY", "Syrian Arab Republic" },
        { "TW", "Taiwan" },
        { "TJ", "Tajikistan" },
        { "TZ", "Tanzania" },
        { "TH", "Thailand" },
        { "TL", "Timor-Leste" },
        { "TG", "Togo" },
        { "TK", "Tokelau" },
        { "TO", "Tonga" },
        { "TT", "Trinidad and Tobago" },
        { "TN", "Tunisia" },
        { "TR", "Turkey" },
        { "TM", "Turkmenistan" },
        { "TC", "Turks and Caicos Islands" },
        { "TV", "Tuvalu" },
        { "UG", "Uganda" },
        { "UA", "Ukraine" },
        { "AE", "United Arab Emirates" },
        { "GB", "United Kingdom" },
        { "US", "United States" },
        { "UM", "United States Minor Outlying Islands" },
        { "UY", "Uruguay" },
        { "UZ", "Uzbekistan" },
        { "VU", "Vanuatu" },
        { "VE", "Venezuela" },
        { "VN", "Viet Nam" },
        { "VG", "Virgin Islands (British)" },
        { "VI", "Virgin Islands (U.S.)" },
        { "WF", "Wallis and Futuna" },
        { "EH", "Western Sahara" },
        { "YE", "Yemen" },
        { "ZM", "Zambia" },
        { "ZW", "Zimbabwe" }
    };
    #endregion

    public float retryInterval = 120f; // 2 minutes
    private bool isLoggedIn = false;

    public static LeaderboardManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        StartCoroutine(TryLoginRoutine());
    }

    private IEnumerator TryLoginRoutine()
    {
        while (!isLoggedIn)
        {
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                CustomLogin();
            }
            yield return new WaitForSeconds(retryInterval);
        }
    }
    
private void CustomLogin()
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true,            
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetUserData = true
            }
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        isLoggedIn = true;
        Debug.Log("PlayFab login successful!");

        playfabID = result.PlayFabId;
        // Get country code
        GetAccountInfo(result.PlayFabId);

        SubmitScore(GameStateManager.LevelManager.CurrentLevel);

        // Fetch leaderboard after login
        FetchLeaderboard();
    }

    private void OnLoginFailure(PlayFabError error)
    {
        isLoggedIn = false;
        Debug.LogWarning("PlayFab login failed: " + error.GenerateErrorReport());
    }

    public void GetAccountInfo(string playFabId)
    {
        var profileRequest = new GetPlayerProfileRequest
        {
            PlayFabId = playFabId,
            ProfileConstraints = new PlayerProfileViewConstraints
            {
                ShowDisplayName = true,
                ShowAvatarUrl = true,
                ShowCreated = true,
                ShowLocations = true,
                ShowLinkedAccounts = true
            }
            };

        PlayFabClientAPI.GetPlayerProfile(profileRequest, OnGetPlayerProfileSuccess, OnGetPlayerProfileFailure);
    }

    private void OnGetPlayerProfileSuccess(GetPlayerProfileResult result)
    {
        var location = result.PlayerProfile.Locations.FirstOrDefault();
        string countryCode = location.CountryCode.ToString();
        string countryName = GetCountryName(countryCode);
        Debug.Log("Country Code: " + countryCode + "Country Name: " + countryName);
        PlayerPrefs.SetString("CountryCode", countryCode);
        PlayerPrefs.SetString("Country", countryName);
    }
    public static string GetCountryName(string countryCode)
    {
        return CountryCodesToNames.TryGetValue(countryCode, out var countryName) ? countryName : "Unknown";
    }

    private void OnGetPlayerProfileFailure(PlayFabError error)
    {
        Debug.LogError("Failed to get DisplayName: " + error.GenerateErrorReport());
    }

    public void SubmitScore(int score)
    {
        Debug.Log("Score Submission");
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate { StatisticName = "HighScore", Value = score }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request, OnScoreSubmitted, OnScoreSubmitFailed);
    }

    private void OnScoreSubmitted(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Score submitted to PlayFab leaderboard!");
    }

    private void OnScoreSubmitFailed(PlayFabError error)
    {
        Debug.LogWarning("Failed to submit score: " + error.GenerateErrorReport());
    }

    public void FetchLeaderboard()
    {
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

        var request = new GetLeaderboardRequest
        {
            StatisticName = "HighScore",
            StartPosition = 0,
            MaxResultsCount = 100,
            ProfileConstraints = new PlayerProfileViewConstraints()
            {
                ShowDisplayName = true,
                ShowAvatarUrl = true,
                ShowLocations = true
            }
        };
        PlayFabClientAPI.GetLeaderboard(request, OnLeaderboardFetched, OnLeaderboardFetchFailed);
    }

    private void OnLeaderboardFetched(GetLeaderboardResult result)
    {
        // Clear old entries
        foreach (Transform child in content.transform)
            Destroy(child.gameObject);

        Debug.Log("Leaderboard Fetched");
        Loading.SetActive(false);
        CheckNetWorkPanel.SetActive(false);

        foreach (var entry in result.Leaderboard)
        {
            var rankEntry = Instantiate(leaderboardPrefab, content.transform);
            if (entry.Profile.DisplayName.IsNullOrEmpty())
            {
                rankEntry.playerNameText.text = entry.DisplayName;
            }
            else
            {
                GenerateRandomName();
            }
            rankEntry.scoreText.text = entry.StatValue.ToString();
            rankEntry.UpdateRank(entry.Position + 1);

            // Highlight player's own entry
            if (entry.PlayFabId == playfabID)
            {
                rankEntry.GetComponent<Image>().color = new Color(0.8f, 0.9f, 1f, 1f); // Example shade
            }


            string countryCode = entry.Profile.Locations[0].CountryCode.ToString();
            if (!string.IsNullOrEmpty(countryCode))
            {
                Sprite flag = Resources.Load<Sprite>("Flags/" + countryCode);
                if (flag != null)
                    rankEntry.flag.sprite = flag;
            }
            else
            {
                Debug.Log("Country Code: " + countryCode);
                Debug.LogError("No Country Code Found");
            }
        }
    }

    private void OnLeaderboardFetchFailed(PlayFabError error)
    {
        Debug.LogWarning("Failed to fetch leaderboard: " + error.GenerateErrorReport());
    }

private string GenerateRandomName()
{
    string name = FootballerNames[UnityEngine.Random.Range(0, FootballerNames.Length)];
    int digits = UnityEngine.Random.Range(100, 1000);
    return name + digits;
}
}
