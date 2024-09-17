using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Logo Placement is contained within the SMARTS_SDK namespace.
/// </summary>
namespace SMARTS_SDK
{
    public class LogoPlacement : MonoBehaviour
    {
        public GameObject mainCanvas;
        private GameObject logo;

        // Use this for initialization
        IEnumerator Start()
        {

            //load image
            //will need to modify path appropriately to location of image
            //currently it only accepts absolute paths but this will need to be changed to a relative path
            //image can be found here: https://github.com/DLizdas/SMARTS-SDK-build/blob/SB_dev/Documentation/Images/CSSALTLogo.jpg
            //image can also be found in images folder in DLL
            WWW www = new WWW("E:/CSSALT/CSSALT-Logo.jpg");
            while (!www.isDone)
                yield return null;


            //set canvas
            mainCanvas = new GameObject("Logo Canvas");
            Canvas logoCanvas = mainCanvas.AddComponent<Canvas>();

            //set render mode as screen overlay
            logoCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            logo = new GameObject();
            //add raw image 
            RawImage logoImage = logo.AddComponent<RawImage>();
            logoImage.name = "Logo Image";
            logo.transform.parent = logoCanvas.transform;

            
            //set raw image texture to logo
            logoImage.texture = (Texture)www.texture;


            //configure location of logo to be bottom right
            logoImage.rectTransform.anchoredPosition = new Vector3(0, 0, 0);
            logoImage.rectTransform.localScale = new Vector3(2.3f, 1, 1);
            logoImage.rectTransform.anchorMin = new Vector2(1, 0);
            logoImage.rectTransform.anchorMax = new Vector2(1, 0);
            logoImage.rectTransform.pivot = new Vector2(1, 0);

        }
    }
}