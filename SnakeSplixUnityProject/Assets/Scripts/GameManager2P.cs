using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager2P : MonoBehaviour
{
    public int maxHeight;
    public int maxWidth;
    private int scoreP1;
    private int scoreP2;
    private int finalScoreP1;
    private int finalScoreP2;
    public Text curScoreText;
    public Text curLivesText;
    public Text curTime;
    public Color colorEnd;
    public Color colorBg;
    public Color colorP1;
    public Color colorLandP1;
    public Color colorTailP1;
    public Color colorP2;
    public Color colorLandP2;
    public Color colorTailP2;
    public GameObject pauseMenu;
    public Transform cameraHolder;
    public Camera mainCamera;

    HelpNode[,] gridHelp;
    Node[,] grid;
    
    //P1
    GameObject tailParentP1;
    GameObject landParentP1;
    GameObject player1;
    Node player1Node;
    Sprite player1Sprite;
    List<LandNode> landP1;
    List<TailNode> tailP1;
    public int livesP1;
    bool killP1;
    
        
    //P2
    GameObject tailParentP2;
    GameObject landParentP2;
    GameObject player2;
    Node player2Node;
    Sprite player2Sprite;
    List<LandNode> landP2;
    List<TailNode> tailP2;
    public int livesP2;
    bool killP2;

    GameObject mapObject;
    public GameObject gemeoverMenu;
    public Text gameoverMessage;
    public Text matchesScoreP1;
    public Text matchesScoreP2;
    SpriteRenderer mapRenderer;

    bool upP1, leftP1, rightP1, downP1;
    bool upP2, leftP2, rightP2, downP2;
    public double moveRate;
    public int obstacles;
    float timer;
    float time;
    int matchesWonP1;
    int matchesWonP2;

    Direction targetDirectionP1;
    Direction curDirectionP1;
    Direction targetDirectionP2;
    Direction curDirectionP2;

    bool pause;
    bool gameover;

    private static System.Random rnd = new System.Random();



    public enum Direction{
        up, down, left, right
    }

    void Start(){
        maxHeight = MainMenu.height;
        maxWidth = MainMenu.width;
        time=MainMenu.time;
        obstacles=MainMenu.obstacles;
        moveRate=MainMenu.moveRate;
        livesP1 = MainMenu.lives;
        livesP2 = MainMenu.lives;

        pause = false;
        gameover=false;
        tailP1 = new List<TailNode>();
        landP1 = new List<LandNode>();
        tailP2 = new List<TailNode>();
        landP2 = new List<LandNode>();
        scoreP1 = 0;
        scoreP2 = 0;
        targetDirectionP1 = Direction.right;
        targetDirectionP2 = Direction.left;
        CreateMap();
        PlaceCamera();
        UpdateScore();
        UpdateLives();
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
                    curDirectionP2 = targetDirectionP2;
                    MovePlayer();
                }
                curTime.text="Time: " + Mathf.Round(time).ToString();
                if(time <= 0 || livesP2==0 || landP2.Count==0 || livesP1==0 || landP1.Count==0)  GameOver();
            }
            else if(pause){
                pauseMenu.SetActive(true);   
            }
        }
    }

#region Movement
    void GetInput(){
        upP1 = Input.GetButtonDown("UpP1");
        downP1 = Input.GetButtonDown("DownP1");
        leftP1 = Input.GetButtonDown("LeftP1");
        rightP1 = Input.GetButtonDown("RightP1");

        upP2 = Input.GetButtonDown("UpP2");
        downP2 = Input.GetButtonDown("DownP2");
        leftP2 = Input.GetButtonDown("LeftP2");
        rightP2 = Input.GetButtonDown("RightP2");
    }


    void SetPlayerDirection(){
        if (upP1){
            SetDirectionP1(Direction.up);
        }
        else if (downP1){
            SetDirectionP1(Direction.down);
        }
        else if (leftP1){
            SetDirectionP1(Direction.left);
        }
        else if (rightP1){
            SetDirectionP1(Direction.right);
        }

        if (upP2){
            SetDirectionP2(Direction.up);
        }
        else if (downP2){
            SetDirectionP2(Direction.down);
        }
        else if (leftP2){
            SetDirectionP2(Direction.left);
        }
        else if (rightP2){
            SetDirectionP2(Direction.right);
        }
    }

    void SetDirectionP1(Direction d){
        if(!isOpposite(d,false)){
            targetDirectionP1 = d;
        }
    }
    private void SetDirectionP2(Direction d){
        if(!isOpposite(d,true)){
            targetDirectionP2 = d;
        }
    }

    void MovePlayer(){


        int P1x = 0;
        int P1y = 0;
        Node player1PrevNode = player1Node;

        int P2x = 0;
        int P2y = 0;
        Node player2PrevNode = player2Node;

        switch (curDirectionP1){
            case Direction.up:
                P1y = 1;
                break;
            case Direction.down:
                P1y = -1;
                break;
            case Direction.left:
                P1x = -1;
                break;
            case Direction.right:
                P1x = 1;
                break;
        }

        switch (curDirectionP2){
            case Direction.up:
                P2y = 1;
                break;
            case Direction.down:
                P2y = -1;
                break;
            case Direction.left:
                P2x = -1;
                break;
            case Direction.right:
                P2x = 1;
                break;
        }

        
        Node targetNodeP1 = GetNode(player1Node.x + P1x, player1Node.y + P1y);
        Node targetNodeP2 = GetNode(player2Node.x + P2x, player2Node.y + P2y);


        //Přidání pole tail oboum hráčům následný test jestli někdo umře
        player1.transform.position = targetNodeP1.worldPosition;
        player1Node = targetNodeP1;
        if (player1PrevNode.land != 1) {
            tailP1.Add(CreateTailNodeP1(player1PrevNode.x, player1PrevNode.y));
            player1PrevNode.tail = 1;
        }

        player2.transform.position = targetNodeP2.worldPosition;
        player2Node = targetNodeP2;
        if (player2PrevNode.land != 3) {
            tailP2.Add(CreateTailNodeP2(player2PrevNode.x, player2PrevNode.y));
            player2PrevNode.tail = 2;
        }

        if(player1Node == player2Node){
            if(player2Node.land==3){
                killP1=true;
            }
            else if(player1Node.land==1){
                killP2=true;
            }
            else{
                killP1=true;
                killP2=true;
            }
        }

        if(player2Node.tail == 1)   killP1=true;
        if(player1Node.tail == 2)   killP2=true;

        //Eventy zabití nezaviněné druhým hráčem
        if(player1Node.tail == 1 || player1Node.land == -1 || player1Node.land == 2 || player1Node.land == 4){
            scoreP1-=5;
            ResetPlayer(false);
        }
        
        if(player2Node.tail == 2 || player2Node.land == -1 || player2Node.land == 2 || player2Node.land == 4){
            scoreP2-=5;
            ResetPlayer(true);
        }

        //P1

        if (killP1) {
            scoreP2+=5;
            ResetPlayer(false);
            killP1 = false;
        }

        if (targetNodeP1.land == 1 && tailP1.Count > 0) {
            int countTail = tailP1.Count;
            FindObjectOfType<AudioManager>().PlayOneShot("Fill");
            for (int i = 0; i < countTail; i++) {
                TailNode curr = tailP1[0];
                curr.node.tail=0;
                CreateLandNodeP1(curr.node.x, curr.node.y);
                RemoveTail(tailP1);
            }
            FillP1();
            UpdateScore();
        }

        //P2
        if (killP2) {
            scoreP1+=5;
            ResetPlayer(true);
            killP2 = false;
        }

        if (targetNodeP2.land == 3 && tailP2.Count > 0) {
            int countTail = tailP2.Count;
            FindObjectOfType<AudioManager>().PlayOneShot("Fill");
            for (int i = 0; i < countTail; i++) {
                TailNode curr = tailP2[0];
                curr.node.tail=0;
                CreateLandNodeP2(curr.node.x, curr.node.y);
                RemoveTail(tailP2);
            }
            FillP2();
            UpdateScore();
        }


    }
#endregion


#region Utils
    private void ResetPlayer(bool player){
        //false=hráč 1; true=hráč 2
        if(!player){
            Debug.Log("P1 DIED");
            livesP1--;
            int countTailP1 = tailP1.Count;
            for(int i=0;i<countTailP1;i++){
                tailP1[0].node.tail=0;
                RemoveTail(tailP1);
            }
            int listInt = rnd.Next(0,landP1.Count-1);
            player1Node = landP1[listInt].node;
            player1.transform.position = player1Node.worldPosition;
            Node player1NodeRight = GetNode(player1Node.x+1,player1Node.y);
            if(player1NodeRight.land==-1 
            || player1NodeRight.land==2 
            || player1NodeRight.land==4)   targetDirectionP1 = Direction.left;
            else    targetDirectionP1=Direction.right;
        }
        else{
            Debug.Log("P2 DIED");
            livesP2--;
            int countTailP2 = tailP2.Count;
            for(int i=0;i<countTailP2;i++){
                tailP2[0].node.tail=0;
                RemoveTail(tailP2);
            }
            int listInt = rnd.Next(0,landP2.Count-1);
            player2Node = landP2[listInt].node;
            player2.transform.position = player2Node.worldPosition;
            Node player2NodeLeft = GetNode(player2Node.x-1,player1Node.y);
            if(player2NodeLeft.land==-1
            || player2NodeLeft.land==4
            || player2NodeLeft.land==2)   targetDirectionP2 = Direction.right;
            else    targetDirectionP2=Direction.left;     
        }
        UpdateScore();
        UpdateLives();
        if(livesP2!=0 || landP2.Count!=0 || livesP1!=0 || landP1.Count!=0)  FindObjectOfType<AudioManager>().PlayOneShot("PlayerDeath");
    }

    void GameOver(){
        FindObjectOfType<AudioManager>().Play("PlayerWin");
        finalScoreP1+=livesP1*10;
        finalScoreP2+=livesP2*10;
        curScoreText.text = "ScoreP1: " + finalScoreP1.ToString() + "\nScoreP2: " + finalScoreP2.ToString();
        gemeoverMenu.SetActive(true);
        if(finalScoreP1==finalScoreP2){
            gameoverMessage.text = "TIE";
            Debug.Log("TIE");
        }
        else if(finalScoreP1<finalScoreP2){
            gameoverMessage.text ="P2 WIN";
            matchesWonP2++;
            Debug.Log("P2 WIN");
        }
        else if(finalScoreP1>finalScoreP2){
            gameoverMessage.text ="P1 WIN";   
            matchesWonP1++;
            Debug.Log("P1 WIN");
        }
        matchesScoreP1.text=matchesWonP1.ToString();
        matchesScoreP2.text=matchesWonP2.ToString();
        gameover=true;
        Cursor.visible=true;
    }

    public void ResetGame(){
        UnityEngine.Object.Destroy(tailParentP1);
        UnityEngine.Object.Destroy(landParentP1);
        UnityEngine.Object.Destroy(player1);
        UnityEngine.Object.Destroy(tailParentP2);
        UnityEngine.Object.Destroy(landParentP2);
        UnityEngine.Object.Destroy(player2);
        UnityEngine.Object.Destroy(mapObject);
        Cursor.visible=false;
        Start();
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

                Node n = new Node(){
                    x = x,
                    y = y,
                    worldPosition = tp,
                    land = 0,
                    tail = 0
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
        //Výpočet pozice a velikosti podle mapy
        Node n = GetNode(maxWidth / 2, maxHeight / 2);
        Vector3 p = n.worldPosition;
        p += Vector3.one * .5f;
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

        landParentP1 = new GameObject("landParentP1");

        //4 startovní pole
        CreateLandNodeP1(1, 1);
        CreateLandNodeP1(1, 2);
        CreateLandNodeP1(2, 1);
        CreateLandNodeP1(2, 2);

        tailParentP1 = new GameObject("tailParentP1");

        player2 = new GameObject("Player2");
        SpriteRenderer player2Render = player2.AddComponent<SpriteRenderer>();
        player2Sprite = CreateSprite(colorP2);
        player2Render.sprite = player2Sprite;
        player2Render.sortingOrder = 1;
        player2Node = GetNode(maxWidth-2, maxHeight-2);
        player2.transform.position = player2Node.worldPosition;        

        landParentP2 = new GameObject("landParentP2");

        CreateLandNodeP2(maxWidth-2, maxHeight-2);
        CreateLandNodeP2(maxWidth-3, maxHeight-2);
        CreateLandNodeP2(maxWidth-2, maxHeight-3);
        CreateLandNodeP2(maxWidth-3, maxHeight-3);

        tailParentP2 = new GameObject("tailParentP2");

        

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


    void CreateLandNodeP1(int x, int y){
        LandNode l = new LandNode();
        l.node = GetNode(x, y);
        if(l.node.land==1 || l.node.land==2)    return;
        LandNode testNode = GetLandNode(l);
        if(testNode!=null){
            if(testNode.own==2){    //Pokud je už zabrána oblast druhým hráčem tak ji smaže
                UnityEngine.Object.DestroyImmediate(testNode.obj);
                landP2.Remove(testNode);
            }
            else if(testNode.own==1){
                return;
            }
        }
        if(l.node==player2Node){
            ResetPlayer(true);
        }

        l.obj = new GameObject("LandP1");
        l.obj.transform.position = l.node.worldPosition;
        l.obj.transform.parent = landParentP1.transform;
        SpriteRenderer r = l.obj.AddComponent<SpriteRenderer>();
        r.sprite = CreateSprite(colorLandP1);
        l.own = 1;
        if(l.node.land==-1 || l.node.land==4){
            r.sortingOrder = -11;
            l.node.land=2;
            landP1.Add(l);
            return;
        }
        l.node.land = 1;
        r.sortingOrder = 0;
        landP1.Add(l);
    }

    void CreateLandNodeP2(int x, int y){
        LandNode l = new LandNode();
        l.node = GetNode(x, y);
        if(l.node.land==3 || l.node.land==4)    return;
        LandNode testNode = GetLandNode(l);
        if(testNode!=null){
            if(testNode.own==1){
                UnityEngine.Object.DestroyImmediate(testNode.obj);
                landP1.Remove(testNode);
            }
            else if(testNode.own==2){
                return;
            }
        }
        if(l.node==player1Node){
            ResetPlayer(false);
        }
        l.obj = new GameObject("LandP2");
        l.obj.transform.position = l.node.worldPosition;
        l.obj.transform.parent = landParentP2.transform;
        SpriteRenderer r = l.obj.AddComponent<SpriteRenderer>();
        r.sprite = CreateSprite(colorLandP2);
        l.own = 2;
        if(l.node.land==-1 || l.node.land==2){
            r.sortingOrder = -11;
            l.node.land=4;
            landP2.Add(l);
            return;
        }
        r.sortingOrder = 0;
        l.node.land = 3;
        landP2.Add(l);

    }


    TailNode CreateTailNodeP1(int x, int y){
        TailNode s = new TailNode();
        s.node = GetNode(x, y);
        s.obj = new GameObject("tailP1");
        s.obj.transform.parent = tailParentP1.transform;
        s.obj.transform.position = s.node.worldPosition;
        SpriteRenderer r = s.obj.AddComponent<SpriteRenderer>();
        r.sprite = CreateSprite(colorTailP1);
        r.sortingOrder = 1;

        return s;
    }

    TailNode CreateTailNodeP2(int x, int y){
        TailNode s = new TailNode();
        s.node = GetNode(x, y);
        s.obj = new GameObject("tailP2");
        s.obj.transform.parent = tailParentP2.transform;
        s.obj.transform.position = s.node.worldPosition;
        SpriteRenderer r = s.obj.AddComponent<SpriteRenderer>();
        r.sprite = CreateSprite(colorTailP2);
        r.sortingOrder = 1;

        return s;
    }

    void FillP1(){
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
        HelpFillP1(GetHelpNode(maxWidth - 1, maxHeight - 1));
        HelpFillP1(GetHelpNode(0, 0));
        HelpFillP1(GetHelpNode(0, maxHeight - 1));
        HelpFillP1(GetHelpNode(maxWidth - 1, 0));
        FillRestP1();
    }

    void HelpFillP1(HelpNode node){
        if (node == null) return;
        if (node.node.land == 1){
            node.own = 1;
            return;
        }
        if (node.own == -1) return;
        node.own = -1;
        HelpFillP1(GetHelpNode(node.node.x, node.node.y + 1)); //Sever
        HelpFillP1(GetHelpNode(node.node.x, node.node.y - 1)); //Jih
        HelpFillP1(GetHelpNode(node.node.x - 1, node.node.y)); //Západ
        HelpFillP1(GetHelpNode(node.node.x + 1, node.node.y)); //Východ

    }

    void FillRestP1(){
        for (int i = 0; i < maxWidth; i++){
            for (int j = 0; j < maxHeight; j++){
                if ((GetHelpNode(i, j)).own == 0){
                    //score +=1;
                    CreateLandNodeP1(i, j);
                }
            }
        }
    }

    void FillP2(){
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
        HelpFillP2(GetHelpNode(maxWidth - 1, maxHeight - 1));
        HelpFillP2(GetHelpNode(0, 0));
        HelpFillP2(GetHelpNode(0, maxHeight - 1));
        HelpFillP2(GetHelpNode(maxWidth - 1, 0));
        FillRestP2();
    }

    void HelpFillP2(HelpNode node){
        if (node == null) return;
        if (node.node.land == 3){
            node.own = 1;
            return;
        }
        if (node.own == -1) return;
        node.own = -1;
        HelpFillP2(GetHelpNode(node.node.x, node.node.y + 1)); //Sever
        HelpFillP2(GetHelpNode(node.node.x, node.node.y - 1)); //Jih
        HelpFillP2(GetHelpNode(node.node.x - 1, node.node.y)); //Západ
        HelpFillP2(GetHelpNode(node.node.x + 1, node.node.y)); //Východ

    }

    void FillRestP2(){
        for (int i = 0; i < maxWidth; i++){
            for (int j = 0; j < maxHeight; j++){
                if ((GetHelpNode(i, j)).own == 0){
                    CreateLandNodeP2(i, j);
                }
            }
        }
    }


    void RemoveTail(List<TailNode> tailDelete){
        UnityEngine.Object.Destroy(tailDelete[0].obj);
        tailDelete.RemoveAt(0);
    }

    bool isOpposite(Direction d,bool player){
        //Testuje se aby se hráči nemohli pohnout zpátky; false=hráč 1 true=hráč 2
        if (!player){
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
        else{
            switch (d){
                default:
                case Direction.up:
                    if (curDirectionP2 == Direction.down)
                        return true;
                    else
                        return false;
                case Direction.down:
                    if (curDirectionP2 == Direction.up)
                        return true;
                    else
                        return false;
                case Direction.left:
                    if (curDirectionP2 == Direction.right)
                        return true;
                    else
                        return false;
                case Direction.right:
                    if (curDirectionP2 == Direction.left)
                        return true;
                    else
                        return false;
            }
        }
    }


    LandNode GetLandNode(LandNode n){
        for(int i=0;i<landP1.Count;i++){
            if(landP1[i].node.worldPosition==n.node.worldPosition){
                return landP1[i];
            }    
        }
        for(int i=0;i<landP2.Count;i++){
            if(landP2[i].node.worldPosition==n.node.worldPosition){
                return landP2[i];
            }    
        }
        return null;
    }
    
    Sprite CreateSprite(Color targetColor){
        Texture2D txt = new Texture2D(1, 1);
        txt.SetPixel(0, 0, targetColor);
        txt.filterMode = FilterMode.Point;
        txt.Apply();
        Rect rect = new Rect(0, 0, 1, 1);
        return Sprite.Create(txt, rect, Vector2.zero, 1, 0, SpriteMeshType.FullRect);
    }

    public void UpdateScore(){
        finalScoreP1 = landP1.Count+scoreP1;
        finalScoreP2 = landP2.Count+scoreP2;
        curScoreText.text = "ScoreP1: " + finalScoreP1.ToString() + "\nScoreP2: " + finalScoreP2.ToString();
    }

    public void UpdateLives(){
        curLivesText.text = "LivesP1: "+livesP1.ToString() + "\nLivesP2: " + livesP2.ToString();
    }

    public void QuitGame(){
        SceneManager.LoadScene("MainMenu");
    }
#endregion
}
