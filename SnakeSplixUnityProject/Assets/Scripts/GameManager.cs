using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour{

    public int maxHeight;
    public int maxWidth;
    private int score;
    public Text curScoreText;
    public Text curTimeText;
    public Color colorEnd;
    public Color colorBg;
    public Color colorP1;
    public Color colorLandP1;
    public Color colorTailP1;
    public GameObject pauseMenu;
    public GameObject gameOverMenu;
    public GameObject leaderboardMenu;
    public Transform cameraHolder;
    public InputField inputName;
    public Text textScore;

    HelpNode[,] gridHelp;
    Node[,] grid;
    List<TailNode> tail;

    GameObject tailParent;
    GameObject landParent;

    GameObject player1;
    Node player1Node;
    Sprite player1Sprite;
    List<LandNode> landP1;

    GameObject mapObject;
    SpriteRenderer mapRenderer;
    public Camera mainCamera;

    bool upP1, leftP1, rightP1, downP1;

    public double moveRate;
    public int obstacles;
    float timer;
    float time;
    bool pause;
    bool gameover;
    Direction targetDirectionP1;
    Direction curDirectionP1;

    public enum Direction{
        up, down, left, right
    }

    private static System.Random rnd = new System.Random();

    void Start(){
        maxHeight = MainMenu.height;
        maxWidth = MainMenu.width;
        time=MainMenu.time;
        obstacles=MainMenu.obstacles;
        moveRate=MainMenu.moveRate;

        gameover = false;
        pause = false;
        tail = new List<TailNode>();
        landP1 = new List<LandNode>();
        score = 0;
        targetDirectionP1 = Direction.right;
        CreateMap();
        PlaceCamera();
        UpdateScore();
    }

    void Update(){
        if(!gameover){
            if(Input.GetButtonDown("Esc")){ 
                Cursor.visible ^= true;
                pause ^= true;
            }
            if(!pause){
                pauseMenu.SetActive(false);
                GetInput();
                SetPlayerDirection();
                timer += Time.deltaTime;
                time -= Time.deltaTime;
                if (timer > moveRate){
                    timer = 0;
                    curDirectionP1 = targetDirectionP1;
                    MovePlayer();
                }
                curTimeText.text = "Time: " + Mathf.Round(time).ToString();
                if(time<=0) ResetPlayer();
            }
            else if(pause){
                pauseMenu.SetActive(true);     
            }
        }
    }

    void GetInput(){
        upP1 = Input.GetButtonDown("UpP1");
        downP1 = Input.GetButtonDown("DownP1");
        leftP1 = Input.GetButtonDown("LeftP1");
        rightP1 = Input.GetButtonDown("RightP1");
    }


    void SetPlayerDirection(){
        if (upP1){
            SetDirection(Direction.up);
        }
        else if (downP1){
            SetDirection(Direction.down);
        }
        else if (leftP1){
            SetDirection(Direction.left);
        }
        else if (rightP1){
            SetDirection(Direction.right);
        }
    }

    void SetDirection(Direction d){
        if(!isOpposite(d)){
            targetDirectionP1 = d;
        }
    }

    void MovePlayer(){
        int x = 0;
        int y = 0;
        Node player1PrevNode = player1Node;

        switch (curDirectionP1){
            case Direction.up:
                y = 1;
                break;
            case Direction.down:
                y = -1;
                break;
            case Direction.left:
                x = -1;
                break;
            case Direction.right:
                x = 1;
                break;
        }



        Node targetNode = GetNode(player1Node.x + x, player1Node.y + y);
        
        if(targetNode.land == 2 || targetNode.land == -1){
            ResetPlayer();
            Debug.Log("Game over");
            return;
        }

        else{
            player1.transform.position = targetNode.worldPosition;
            player1Node = targetNode;
        }

        if (player1PrevNode.land != 1){
            tail.Add(CreateTailNode(player1PrevNode.x, player1PrevNode.y));
            player1PrevNode.tail = 1;

        }
        
        if (targetNode.land==1 && tail.Count > 0){
            int countTail = tail.Count;
            FindObjectOfType<AudioManager>().PlayOneShot("Fill");
            for (int i = 0; i < countTail; i++){
                TailNode curr = tail[0];
                CreateLandNode(curr.node.x, curr.node.y);
                RemoveTail(tail);
            }

            Fill();
            UpdateScore();

        }
    }

    public void ResetGame(){
        UnityEngine.Object.Destroy(tailParent);
        UnityEngine.Object.Destroy(landParent);
        UnityEngine.Object.Destroy(player1);
        UnityEngine.Object.Destroy(mapObject);
        inputName.text = "";
        Cursor.visible = false;
        Start();
    }

    private void ResetPlayer(){
        UpdateScore();
        gameover = true;
        Cursor.visible=true;
        string scores = PlayerPrefs.GetString("highscores","");
        string[] scoresList = scores.Split(',');
        if(scoresList.Length>10){
            Int32.TryParse(scoresList[9],out int x);
            if(score>x){
                leaderboardMenu.SetActive(true);
                FindObjectOfType<AudioManager>().Play("PlayerWin");
                textScore.text = "Score: " + score.ToString();
                Debug.Log(score);
            }
            else{    
                gameOverMenu.SetActive(true);
                FindObjectOfType<AudioManager>().Play("PlayerDeath");
            }
        }
        else{
            leaderboardMenu.SetActive(true);
            FindObjectOfType<AudioManager>().Play("PlayerWin");
            textScore.text = "Score: " + score.ToString();
        }
    }

    public void AddToLeaderboard(){
        string input = inputName.text;
        if(input.Contains(",") || input.Contains(";"))     return;
        string scores = PlayerPrefs.GetString("highscores",";");
        string[] scoresAndNames = new string[2];
        scoresAndNames = scores.Split(';');
        string[] scoresList = new string[11];
        scoresList = scoresAndNames[0].Split(',');
        string[] namesList = new string[11];
        namesList = scoresAndNames[1].Split(',');
        for(int i=0;i<scoresList.Length;i++){
            bool emptyField = Int32.TryParse(scoresList[i],out int x);
            if(!emptyField){    //Pokud není znak číslo automaticky ho přidá na konec leaderboardu
                namesList[i]=input;                
                scoresList[i]=score.ToString();
                break;
            }
            if(score>=x){
                for(int n=scoresList.Length-1;n>i;n--){     //Posun všech skóre o jedno výš
                    scoresList[n]=scoresList[n-1];
                    namesList[n]=namesList[n-1];    
                }
                scoresList[i]=score.ToString();
                namesList[i]=input;
                break;
            }
        }
        string listOut = "";
        int length;
        if(scoresList.Length>=10)    length = 10;
        else length = scoresList.Length;
        for(int i=0;i<length;i++){ 
            listOut += scoresList[i]+",";
        }
        listOut += ";";        
        for(int i=0;i<length;i++){
            listOut += namesList[i]+",";
        }
        PlayerPrefs.SetString("highscores",listOut);
        leaderboardMenu.SetActive(false);
        gameOverMenu.SetActive(true);
    }

    void CreateMap(){

        mapObject = new GameObject("Map");
        mapRenderer = mapObject.AddComponent<SpriteRenderer>();

        grid = new Node[maxWidth, maxHeight];

        Texture2D bg = new Texture2D(maxWidth, maxHeight);

        for (int x = 0; x < maxWidth; x++){
            for (int y = 0; y < maxHeight; y++){
                Vector3 tp = Vector3.zero;
                tp.x = x;
                tp.y = y;
                //Každé pole na mapě přestavuje Node
                Node n = new Node(){
                    x = x,
                    y = y,
                    worldPosition = tp,
                    land = 0
                };

                grid[x, y] = n;

                bg.SetPixel(x, y, colorBg);

            }
        }

        PlacePlayer();

        for(int x=0;x<maxWidth;x++){
            Node n = GetNode(x,0);
            Node n2 = GetNode(x,maxHeight-1);
            n.land = -1;
            n2.land = -1;
            bg.SetPixel(x,0,colorEnd);
            bg.SetPixel(x,maxHeight-1,colorEnd);
        }

        for(int y=0;y<maxHeight;y++){
            Node n3 = GetNode(0,y);
            Node n4 = GetNode(maxWidth-1,y);
            n3.land = -1;
            n4.land = -1;
            bg.SetPixel(0,y,colorEnd);
            bg.SetPixel(maxWidth-1,y,colorEnd);
        }
        //Náhodné spawnování překážek proti zacyklení je podmínka při setování počtu překážek
        int i=0;
        while(i<obstacles){
            int x = rnd.Next(1,maxWidth-1);
            int y = rnd.Next(1,maxHeight-1);
            Node n = GetNode(x,y);
            if(n.land==0){
                n.land=-1;
                bg.SetPixel(x,y,colorEnd);
                i++;
            }
        }

        bg.filterMode = FilterMode.Point;
        bg.Apply();
        Rect rect = new Rect(0, 0, maxWidth, maxHeight);
        Sprite sprite = Sprite.Create(bg, rect, Vector2.zero, 1, 0, SpriteMeshType.FullRect);
        mapRenderer.sprite = sprite;
        mapRenderer.sortingOrder = -10;


    }

    void PlaceCamera(){
        Node n = GetNode(maxWidth / 2, maxHeight / 2);
        Vector3 p = n.worldPosition;
        p += Vector3.one * .5f;
        //Aby kamera zabrala měnící se mapu
        int size=0;
        if(maxHeight>maxWidth){
            size= maxHeight/2;
        }
        else    size = maxWidth/2;
        cameraHolder.position = p;
        mainCamera.orthographicSize = size;
    }

    void PlacePlayer(){
        player1 = new GameObject("Player1");
        SpriteRenderer playerRender = player1.AddComponent<SpriteRenderer>();
        player1Sprite = CreateSprite(colorP1);
        playerRender.sprite = player1Sprite;
        playerRender.sortingOrder = 1;
        player1Node = GetNode(1, 1);
        player1.transform.position = player1Node.worldPosition;

        landParent = new GameObject("landParent");
        //4 startovací pole
        CreateLandNode(1, 1);
        CreateLandNode(1, 2);
        CreateLandNode(2, 1);
        CreateLandNode(2, 2);



        tailParent = new GameObject("tailParent");

    }

    void CreateLandNode(int x, int y){
        LandNode l = new LandNode();
        l.node = GetNode(x, y);
        if(l.node.land==1 || l.node.land==2)  return;
        l.obj = new GameObject("LandP1");
        l.obj.transform.parent = landParent.transform;
        l.obj.transform.position = l.node.worldPosition;
        SpriteRenderer r = l.obj.AddComponent<SpriteRenderer>();

        r.sprite = CreateSprite(colorLandP1);
        //Vytvoření překážky přes moje pole
        if(l.node.land==-1){
            r.sortingOrder = -11;
            l.node.land = 2;
            landP1.Add(l);
            return;
        }
        l.node.land = 1;
        r.sortingOrder = 0;

        landP1.Add(l);
    }

    Node GetNode(int x, int y){
        if (x < 0 || x > maxWidth - 1 || y < 0 || y > maxHeight - 1){
            return null;
        }

        return grid[x, y];
    }

    HelpNode GetHelpNode(int x, int y){
        if (x < 0 || x > maxWidth - 1 || y < 0 || y > maxHeight - 1){
            return null;
        }
        return gridHelp[x, y];
    }



    TailNode CreateTailNode(int x, int y){
        TailNode s = new TailNode();
        s.node = GetNode(x, y);
        s.obj = new GameObject("tailP1");
        s.obj.transform.parent = tailParent.transform;
        s.obj.transform.position = s.node.worldPosition;
        SpriteRenderer r = s.obj.AddComponent<SpriteRenderer>();
        r.sprite = CreateSprite(colorTailP1);
        r.sortingOrder = 1;

        return s;
    }

    void Fill(){
        gridHelp = new HelpNode[maxWidth, maxHeight];
        for (int x = 0; x < maxWidth; x++){
            for (int y = 0; y < maxHeight; y++){
                HelpNode n = new HelpNode(){
                    node = GetNode(x, y),
                    own = 0
                };

                gridHelp[x, y] = n;

            }
        }
        HelpFill(GetHelpNode(maxWidth - 1, maxHeight - 1));
        HelpFill(GetHelpNode(0, 0));
        HelpFill(GetHelpNode(0, maxHeight - 1));
        HelpFill(GetHelpNode(maxWidth - 1, 0));
        FillRest();
    }

    void HelpFill(HelpNode node){
        if (node == null) return;
        if (node.node.land == 1){
            node.own = 1;
            return;
        }
        if (node.own == -1) return;
        node.own = -1;
        HelpFill(GetHelpNode(node.node.x, node.node.y + 1)); //Sever
        HelpFill(GetHelpNode(node.node.x, node.node.y - 1)); //Jih
        HelpFill(GetHelpNode(node.node.x - 1, node.node.y)); //Západ
        HelpFill(GetHelpNode(node.node.x + 1, node.node.y)); //Východ

    }

    void FillRest(){
        for (int i = 0; i < maxWidth; i++){
            for (int j = 0; j < maxHeight; j++){
                if ((GetHelpNode(i, j)).own == 0){
                    CreateLandNode(i, j);
                }
            }
        }
    }

    void RemoveTail(List<TailNode> tailDelete){
        UnityEngine.Object.Destroy(tailDelete[0].obj);
        tailDelete.RemoveAt(0);
    }


    bool isOpposite(Direction d){
        //Testuje se aby se hráč nemohl pohnout zpátky
        switch (d){
            default:
            case Direction.up:
                if (curDirectionP1 == Direction.down)
                    return true;
                else
                    return false;
            case Direction.down:
                if (curDirectionP1 == Direction.up)
                    return true;
                else
                    return false;
            case Direction.left:
                if (curDirectionP1 == Direction.right)
                    return true;
                else
                    return false;
            case Direction.right:
                if (curDirectionP1 == Direction.left)
                    return true;
                else
                    return false;
        }
    }



    Sprite CreateSprite(Color targetColor){
        //Metoda na vytvoření textury z barvy
        Texture2D txt = new Texture2D(1, 1);
        txt.SetPixel(0, 0, targetColor);
        txt.filterMode = FilterMode.Point;
        txt.Apply();
        Rect rect = new Rect(0, 0, 1, 1);
        return Sprite.Create(txt, rect, Vector2.zero, 1, 0, SpriteMeshType.FullRect);
    }

    public void UpdateScore(){
        score = landP1.Count;
        curScoreText.text = "Score: " + score.ToString();
    }

    public void QuitGame(){
        SceneManager.LoadScene("MainMenu");
    }
}
