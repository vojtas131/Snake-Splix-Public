using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;
using System;
using System.Globalization;

public class MainMenu : MonoBehaviour{

    //Customization
    static public int height;
    static public int width;
    static public int obstacles;
    static public int lives;
    static public double moveRate;
    static public int time;
    public GameObject warning;
    public GameObject saved;
    
    
    public InputField inputHeight;
    public InputField inputWidth;
    public InputField inputObstacles;
    public InputField inputLives;
    public InputField inputMoveRate;
    public InputField inputTime;

    //Audio
    public AudioMixer audioMixer;
    public Slider effectSlider;
    public Slider musicSlider;
    public Text leaderboardText;


    float musicVolume;
    float effectVolume;
    bool start=false;
    public static bool setValues;
    void Start(){
        Cursor.visible=true;
        musicVolume = PlayerPrefs.GetFloat("musicVolume",0);
        effectVolume = PlayerPrefs.GetFloat("effectVolume",0);
        musicSlider.value = musicVolume;
        effectSlider.value = effectVolume;
        audioMixer.SetFloat("volumeMusic",musicVolume);
        audioMixer.SetFloat("volumeEffect",effectVolume);
        string props = PlayerPrefs.GetString("props","25_30_0_5_0,2_60_");
        string[] propsList = props.Split('_');
        /* 
        Default params
        height = 25
        width = 30
        obstacles = 0
        lives = 5
        moveRate = 0.2f
        time = 60
        */
        if(!setValues){
            height = Int32.Parse(propsList[0]);
            width = Int32.Parse(propsList[1]);
            obstacles = Int32.Parse(propsList[2]);
            lives = Int32.Parse(propsList[3]);
            moveRate = Convert.ToDouble(propsList[4]);
            time = Int32.Parse(propsList[5]);
        }
        setValues=true;
        start=true;
        saved.SetActive(false);

        

        inputHeight.text=height.ToString();
        inputWidth.text=width.ToString();
        inputObstacles.text=obstacles.ToString();
        inputLives.text=lives.ToString();
        inputMoveRate.text=moveRate.ToString();
        inputTime.text=time.ToString();
    }

#region MainMenu
    public void P1Button(){
        SceneManager.LoadScene("Game1P");
        Cursor.visible=false;
    }

    public void P2Button(){
        SceneManager.LoadScene("Game2P");
        Cursor.visible=false;
    }

    public void SettingsButton(){
        SceneManager.LoadScene("Settings");
    }
    public void QuitGameButton(){
        Debug.Log("Bye");
        Application.Quit();
    }
#endregion
#region Settings
    public void SetVolumeMusic(float volume){
        audioMixer.SetFloat("volumeMusic",volume);
        PlayerPrefs.SetFloat("musicVolume",volume);
    }
    public void SetVolumeEffect(float volume){
        audioMixer.SetFloat("volumeEffect",volume);
        PlayerPrefs.SetFloat("effectVolume",volume);
        if(start)  FindObjectOfType<AudioManager>().Play("PlayerWin");
    }
    public void ResetHighscores(){
        PlayerPrefs.DeleteKey("highscoreNames");
        PlayerPrefs.DeleteKey("highscores");
    }
#endregion Settings
#region Customize
    public void ApplyChanges(){
        warning.SetActive(false);
        if(Int32.Parse(inputObstacles.text)>=(((Int32.Parse(inputHeight.text)-3)*(Int32.Parse(inputWidth.text)-3))-8)){
            warning.SetActive(true);
            return;
        }
        StartCoroutine(ApplyChangesCoroutine());
    }
    public IEnumerator ApplyChangesCoroutine(){
        saved.GetComponent<Text>().color = new Color(0,0,0,255);
        saved.SetActive(true);
        height = Int32.Parse(inputHeight.text);
        width = Int32.Parse(inputWidth.text);
        obstacles = Int32.Parse(inputObstacles.text);   //Dynamic blokování čísla? (25-3)*(30-3) = 586
        lives = Int32.Parse(inputLives.text);
        moveRate = Convert.ToDouble(inputMoveRate.text);
        time = Int32.Parse(inputTime.text);
        string[] propsList = new string[6];
        propsList[0] = inputHeight.text;
        propsList[1] = inputWidth.text;
        propsList[2] = inputObstacles.text;
        propsList[3] = inputLives.text;
        propsList[4] = inputMoveRate.text;
        propsList[5] = inputTime.text;
        string props = "";
        for(int i=0;i<propsList.Length;i++){
            props += propsList[i] + "_";
        }
        PlayerPrefs.SetString("props",props);
        Debug.Log("SET");
        saved.GetComponent<Text>().CrossFadeAlpha(0,1.5f,false);
        yield return new WaitForSeconds(1.5f);
        saved.SetActive(false);
        yield return null;
    }

    public void ResetProps(){
        height = 25;
        width = 30;
        obstacles = 0;
        lives = 5;
        moveRate = 0.2f;
        time = 60;
        inputHeight.text = height.ToString();
        inputWidth.text = width.ToString();
        inputObstacles.text = obstacles.ToString();
        inputLives.text = lives.ToString();
        inputMoveRate.text = moveRate.ToString();
        inputTime.text = time.ToString();

        StartCoroutine(ApplyChangesCoroutine());
    }
#endregion
#region Leaderboard
/* 
Highscores:
1.
2.
3.
4.
5.
6.
7.
8.
9.
10.
*/
    public void LoadLeaderboard(){
        leaderboardText.text = "";
        int i = 0;
        string scores = PlayerPrefs.GetString("highscores",";");
        string[] scoresAndNames = scores.Split(';');
        string[] scoresList = scoresAndNames[0].Split(',');
        //string scoreNames = PlayerPrefs.GetString("highscoreNames","");
        string[] namesList = scoresAndNames[1].Split(',');
        while(i<scoresList.Length-1){
            leaderboardText.text += (i+1).ToString() + ". " + namesList[i] + ": " + scoresList[i] + "\n";
            i++;
        }
        if(scoresList.Length<=10){
            while(i<10){
                leaderboardText.text += (i+1).ToString() + ".\n";
                i++;
            }
        }
    }
#endregion Leaderboard
}
