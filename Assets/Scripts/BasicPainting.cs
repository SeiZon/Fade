using UnityEngine;
using System.Collections;

public class BasicPainting : MonoBehaviour
{

    // the alpha texture from the gameobject and the splash texture
    [SerializeField] private Texture2D splashTexture, alphaTexture;
    [SerializeField] int elementNo = 0;

    //temporary texture
    public Texture2D tmpTexture; 

    void Start()
    {
        //when starting set the temp texture as the alpha texture(completely black)

        //setting temp texture width and height 
        tmpTexture = new Texture2D(alphaTexture.width, alphaTexture.height);

        //fill the new texture with the original one (to avoid "empty" pixels)
        for (int y = 0; y < tmpTexture.height; y++)
        {
            for (int x = 0; x < tmpTexture.width; x++)
            {
                tmpTexture.SetPixel(x, y, alphaTexture.GetPixel(x, y));
            }
        }
        //filling a part of the temporary texture with the target texture 
    }

    public void paint(int x,int y)
    {
        int size = splashTexture.width; //always use power of 2
        int radius = size/2;

        for (int i = 0; i <= size; i++ )
        {
            for (int j = 0; j <= size; j++)
            {
                if (splashTexture.GetPixel(i, j).a != 0 && (x + i - radius) > 0 && (y + j - radius) > 0 && (x + i - radius) <= alphaTexture.width && (y + j - radius) <= alphaTexture.height)
                {
                    tmpTexture.SetPixel(x + i - radius, y + j - radius, splashTexture.GetPixel(i, j));
                }
            }
        }

        //Apply 
        tmpTexture.Apply();
        //change the object alpha texture to the new one
        GetComponent<Renderer>().materials[elementNo].SetTexture("_PathMask", tmpTexture);
    }

}