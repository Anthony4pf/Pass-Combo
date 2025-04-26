using NaughtyAttributes;
using UnityEngine;

 
[CreateAssetMenu(menuName = "Managers/Economy Manager")]
public class EconomyManager : ScriptableObject
{
    [FancyHeader("$  ECONOMY MANAGER  $", 1.5f, "lime", 5.5f, order = 0)]
  

    [Label("Money Balance")] [ReadOnly] [SerializeField] double m_Money;
    [Label("Total amount accumulated")] [ReadOnly] [SerializeField] double m_TotalMoney;
    
    public delegate void OnMoneyChanged(double newAmount);
    public static OnMoneyChanged m_onMoneyChanged;

   
    [BoxGroup("SO Events"), Required] public FloatEvent onMoneyChanged;
    
    [Space, BoxGroup("SO Events"), Required] public FloatEvent onTotalMoneyIncreased;
    
    


    [BoxGroup("Config")]
    [Tooltip("The amount of money the player starts the game with")] public float startingMoney = 80f;

    
    [BoxGroup("Level Amount")]
    [Tooltip("Money earned after the player has completed a level")] public float completedLevelReward = 200;


 
    


    // Private Variables
    public double Money
    {
        get { return m_Money; }
        set { m_Money = value; }
    }
    public double TotalMoney
    {
        get { return m_TotalMoney; }
       set { m_TotalMoney = value; }
    }
    
    
    public void InitializeValues()
    {
        Money = PlayerPrefs.GetFloat("Money");
       
        onMoneyChanged.Raise(((float)Money));
    }
    

    

    public void SetTotalMoney(double amount)
    {
        TotalMoney = amount;
        onTotalMoneyIncreased.Raise((float)TotalMoney);
        
        SaveData();
    }
    
    public void AddToTotalMoney(double amount)
    {
        TotalMoney += amount;
            PlayerPrefs.SetFloat("TotalMoney", (float)TotalMoney);
        
        

        onTotalMoneyIncreased.Raise((float)TotalMoney);
        
        
    }
   

    // Set our balance to a specific amount]
    public void SetMoney(double amount)
    {

        Money = amount;
            PlayerPrefs.SetFloat("Money", (float)Money);
       

        if (m_onMoneyChanged != null)
            m_onMoneyChanged.Invoke(Money);

        onMoneyChanged.Raise(((float)Money));

       
        
    }
    

    public void AddMoney(float a)
    {
        AddMoney((double)Mathf.Round(a));

    }
    public void SpendMoney(float a)
    {
        ReduceMoney((double)Mathf.Round(a));
    }

    
    
    public void AddMoney(double amount, bool addTotal = true)
    {

        Money += amount;
        PlayerPrefs.SetFloat("Money", (float)Money);
    
    }
    

    public void ReduceMoney(double amount)
    {
        Money -= amount;
        PlayerPrefs.SetFloat("Money", (float)Money);

        if (Money < 0)
            {
                Money = 0;
                PlayerPrefs.SetFloat("Money", (float)Money);
            }


        // Tell other scripts our balance has changed
        if (m_onMoneyChanged != null)
            m_onMoneyChanged(Money);
        onMoneyChanged?.Raise(((float)Money));

     
        
    }
    

    // Save money data when the player quits
    public void SaveData()
    {

        PlayerPrefs.SetString("Money", Money.ToString());
        PlayerPrefs.SetString("TotalMoney", TotalMoney.ToString());
            
        
    }
}


