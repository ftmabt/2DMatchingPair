using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PictureManager : MonoBehaviour
{
    public enum GameState
    {
        NoAction,
        MovingOnPositions,
        DeletingPuzzles,
        FlipBack,
        Checking,
        GameEnd
    };
    public enum PuzzleState
    {
        PuzzleRotating,
        CanRotate
    };

    public enum RevealedState
    {
        NoRevealed,
        OneRevealed,
        TwoRevealed
    };

    public Text txt1;
    public Text txt2;

    public GameState curGameState;
    public PuzzleState curPuzState;
    public RevealedState puzRevealedNum;

    int pairNumber=8;

    public Picture picPrefab;
    public Transform picSpawnPos;

    public Vector2 startPos;
    public Vector2 offset;

    public List<Picture> picList;
    public List<Material> matList = new List<Material>();
    public List<string> texturePathList = new List<string>();
    
    Material firstMat;
    string firstTexturePath;

    int firstRevealedPic;
    int secondRevealedPic;
    int revealedPicNum=0;

    int picToDestroy1;
    int picToDestroy2;

    bool coroutinestarted = false;
    int counter = 0;
    int score1 = 0;
    int score2 = 0;

    public GameObject endPanel;
    public Text endText;
    private void Start()
    {
        counter = 0;
        score1 = 0;
        score2 = 0;

        curGameState = GameState.MovingOnPositions;
        curPuzState = PuzzleState.CanRotate;
        puzRevealedNum = RevealedState.NoRevealed;
        revealedPicNum = 0;
        firstRevealedPic=-1;
        secondRevealedPic=-1;

        LoadMaterials();

        SpawnPictureMesh(4, 4, startPos, offset, false);
        MovePicture(4, 4, startPos, offset);
    }

    

    public void CheckPicture()
    {
        curGameState = GameState.Checking;
        revealedPicNum = 0;

        for(int id = 0; id < picList.Count; id++)
        {
            if (picList[id].revealed && revealedPicNum < 2)
            {
                if (revealedPicNum == 0)
                {
                    firstRevealedPic = id;
                    revealedPicNum++;
                }
                else if (revealedPicNum == 1)
                {
                    secondRevealedPic = id;
                    revealedPicNum++;
                }
            }
        }
        if (revealedPicNum == 2)
        {
            counter++;
            
            if (picList[firstRevealedPic].GetIndex() == picList[secondRevealedPic].GetIndex() && firstRevealedPic != secondRevealedPic)
            {
                if (counter % 2 == 0)
                {
                    score2++;
                    txt2.text = score2.ToString(); 
                }
                else
                {
                    score1++;
                    txt1.text = score1.ToString();
                }
                if ((score1+score2) == pairNumber)
                {
                    StartCoroutine(EndGame());
                }
                curGameState = GameState.DeletingPuzzles;
                picToDestroy1 = firstRevealedPic;
                picToDestroy2 = secondRevealedPic;
            }
            else
            {
                curGameState = GameState.FlipBack;
            }
            
        }
        curPuzState = PictureManager.PuzzleState.CanRotate;
        if (curGameState == GameState.Checking)
        {
            curGameState = GameState.NoAction;
        }
    }

    IEnumerator EndGame()
    {
        
        yield return new WaitForSeconds(0.5f);
        endPanel.SetActive(true);
        if (score1 > score2)
        {
            endText.text = "team one won";
        }
        else if (score2 > score1)
        {
            endText.text = "team two won";
        }
        else
        {
            endText.text = "draw";
        }
    }
    void DestroyPicture()
    {
        puzRevealedNum = RevealedState.NoRevealed;
        
        picList[picToDestroy1].Deactivate();
        picList[picToDestroy2].Deactivate();

        revealedPicNum = 0;
        curGameState = GameState.NoAction;
        curPuzState = PuzzleState.CanRotate;
    }
    
     IEnumerator FlipBack()
    {
        coroutinestarted = true;
        yield return new WaitForSeconds(0.5f);

        picList[firstRevealedPic].FlipBack();
        picList[secondRevealedPic].FlipBack();

        picList[firstRevealedPic].revealed = false;
        picList[secondRevealedPic].revealed = false;

        puzRevealedNum = RevealedState.NoRevealed;
        curGameState = GameState.NoAction;

        coroutinestarted = false;
    }
    void LoadMaterials()
    {
        var materialFilePath = "Materials/";
        var textureFilePath = "Images/";
        var firstMatName = "back";

        for (var index = 1; index <= pairNumber; index++)
        {
            var currentFilePath = materialFilePath + "pic" + index;
            Material mat = Resources.Load(currentFilePath, typeof(Material)) as Material;
            matList.Add(mat);

            var currentTextureFilePath = textureFilePath+ "pic" + index;
            texturePathList.Add(currentTextureFilePath);
        }

        firstTexturePath = textureFilePath + firstMatName;
        firstMat = Resources.Load(materialFilePath+firstMatName,typeof(Material))as Material;
        
    }
    private void Update()
    {
        if (curGameState == GameState.DeletingPuzzles)
        {
            if(curPuzState == PuzzleState.CanRotate)
            {
                DestroyPicture();
            }
        }

        if (curGameState == GameState.FlipBack)
        {
            if (curPuzState == PuzzleState.CanRotate && coroutinestarted==false)
            {
                StartCoroutine(FlipBack());
            }
        }
        //exit game
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
    }

    void SpawnPictureMesh(int rows,int cols, Vector2 pos,Vector2 offset,bool scaleDown)
    {
        for(int col=0; col < cols; col++)
        {
            for (int row = 0; row < rows; row++)
            {
                var tempPic = (Picture)Instantiate(picPrefab, picSpawnPos.position, picPrefab.transform.rotation);

                tempPic.name = tempPic.name + 'c' + col + 'r' + row;
                picList.Add(tempPic);
            }
        }

        ApplyTextures();
    }

    public void ApplyTextures()
    {
        var randMatIndex = Random.Range(0, matList.Count);
        var appliedTimes = new int[matList.Count];

        for(int i = 0; i < matList.Count; i++)
        {
            appliedTimes[i] = 0;
            
        }

        foreach(var o in picList)
        {
            var randPrevious = randMatIndex;
            var counter = 0;
            var forceMat = false;
            while (appliedTimes[randMatIndex] >= 2 ||((randPrevious==randMatIndex)&& !forceMat))
            {
                randMatIndex = Random.Range(0, matList.Count);
                counter++;
                if (counter > 100)
                {
                    for(var j = 0; j < matList.Count; j++)
                    {
                        if (appliedTimes[j]<2)
                        {
                            randMatIndex = j;
                            forceMat = true;

                        }
                    }

                    if (forceMat == false)
                    {
                        return;
                    }
                }
            }

            o.SetFirstMaterial(firstMat, firstTexturePath);
            o.ApplyFirstMaterial();
            o.SetSecondMaterial(matList[randMatIndex], texturePathList[randMatIndex]);
            o.SetIndex(randMatIndex);
            o.revealed = false;
            appliedTimes[randMatIndex]+=1;
            forceMat = false;
        }
    }

    void MovePicture(int rows, int cols, Vector2 pos, Vector2 offset)
    {
        var index = 0;
        for (int col = 0; col < cols; col++)
        {
            for (int row = 0; row < rows; row++)
            {
                var targetPos = new Vector3(pos.x + (offset.x * row), pos.y - (offset.y * col), 0f);
                StartCoroutine(MoveToPosition(targetPos,picList[index]));
                index++;
            }
        }
    }

    IEnumerator MoveToPosition(Vector3 target,Picture obj)
    {
        var randomDis = 100;

        while (obj.transform.position != target)
        {
            obj.transform.position = Vector3.MoveTowards(obj.transform.position, target, randomDis * Time.deltaTime);
            yield return 0;
        }
    }

}
