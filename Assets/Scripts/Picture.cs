using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Picture : MonoBehaviour
{
    Material fisrtMat;
    Material secondMat;

    Quaternion curRot;

    public bool revealed = false;
    public PictureManager pm;
    bool clicked = false;

    int index;

    public void SetIndex(int id)
    {
        index = id;
    }
    public int GetIndex()
    {
        return index;
    }
    private void Start()
    {
        revealed = false;
        clicked = false;
        pm = GameObject.Find("PictureManager").GetComponent<PictureManager>();
        curRot = gameObject.transform.rotation;
    }
    private void OnMouseDown()
    {
        if (clicked == false)
        {
            pm.curPuzState = PictureManager.PuzzleState.PuzzleRotating;
            StartCoroutine(LoopRotation(45, false));
            clicked = true;
        }
        
    }

    public void FlipBack()
    {
        pm.curPuzState = PictureManager.PuzzleState.PuzzleRotating;
        revealed = false;
        StartCoroutine(LoopRotation(45, true));
    }

    IEnumerator LoopRotation(float angle,bool firstMat)
    {
        var rot = 0f;
        const float dir = 1f;
        const float rotSpeed = 180f;
        var startAngle = angle;
        var assigned = false;

        if (firstMat)
        {
            while (rot < angle)
            {
                var step = Time.deltaTime * rotSpeed;
                gameObject.GetComponent<Transform>().Rotate(new Vector3(0, 2, 0) * step * dir);
                if(rot>=(startAngle-2)&& assigned == false)
                {
                    ApplyFirstMaterial();
                    assigned = true;
                }
                rot += (1 * step * dir);
                yield return null;
            }
        }
        else
        {
            while (angle > 0)
            {
                float step = Time.deltaTime * rotSpeed;
                gameObject.GetComponent<Transform>().Rotate(new Vector3(0, 2, 0) * step * dir);
                angle -= (1 * step * dir);
                yield return null;
            }
        }

        gameObject.GetComponent<Transform>().rotation = curRot;
        if (!firstMat)
        {
            revealed = true;
            ApplySecondMaterial();
            pm.CheckPicture();
        }
        else
        {
            pm.puzRevealedNum = PictureManager.RevealedState.NoRevealed;
            pm.curPuzState = PictureManager.PuzzleState.CanRotate;
        }

        clicked = false;
    }
    public void SetFirstMaterial(Material mat,string texturePath)
    {
        fisrtMat = mat;
        fisrtMat.mainTexture = Resources.Load(texturePath, typeof(Texture2D)) as Texture2D;
    }
    public void SetSecondMaterial(Material mat, string texturePath)
    {
        secondMat = mat;
        secondMat.mainTexture = Resources.Load(texturePath, typeof(Texture2D)) as Texture2D;
    }

    public void ApplyFirstMaterial()
    {
        gameObject.GetComponent<Renderer>().material = fisrtMat;
    }

    public void ApplySecondMaterial()
    {
        gameObject.GetComponent<Renderer>().material = secondMat;
    }

    public void Deactivate()
    {
        StartCoroutine(DeactivateCoroutine());
    }

    IEnumerator DeactivateCoroutine()
    {
        revealed = false;
        yield return new WaitForSeconds(0.5f);

        gameObject.SetActive(false);
    }
}
