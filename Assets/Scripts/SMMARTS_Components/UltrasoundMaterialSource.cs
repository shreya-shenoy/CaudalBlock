using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SMARTS_SDK.Ultrasound
{
    public class UltrasoundMaterial : MonoBehaviour
    {

        // Default is to render the object on ultrasound.
        // there are times when this is not desireable, such as rendering a test object that accumulates hits if it is inside the insonating beam.
        public bool ShowOnUltrasound = true;

        // Normal = simply render the color on US screen.
        // Bone = Color on surface, with shadow.
        // Lung = Color on surface, with lung sliders and noise artifacts
        // Texture1 = Perlin-like noise using texture settings 1
        // Texture2 = Perlin-like noise using texture settings 2
        public enum RenderType { Normal, Bone, Lung, Texture1, Texture2 };
        public RenderType Type;

        // the color setting of this object
        public Color Color;

        // Anisotropy Index.  
        // Has to do with how visible the object is in relation to the incident angle of the ultrasound insonating ray.
        // 0 (default) no anisotropy - the angle doesnt matter.
        // 90 (practically invisible) means that the object will be shown only for ultrasound insonating rays that are 90 degrees to the object's surface.
        // 45 (invisible past 45 degrees) the object will begin to fade out at about 70 degrees, and disappear at about 45 degrees. pretty extreme.
        // 65 (invisible past 65 degrees) seems about accurate for needles and lungs on newer US machines. 
        [Range(0, 90)]
        public float AnisoIndex = 0;

        // The following methods provide the means to test if this object is visible in US.

        // This is an accumulator for ultrasound insonating beam hits.
        // if the material is not under the insonating beam, this accumulator stays zero.
        // It should be polled on Update, and is cleared in LateUpdate.
        private int hitCount;

        public int GetHitCount()
        {
            return hitCount;
        }

        public void NoteHit()
        {
            hitCount++;
        }

        void LateUpdate()
        {
            hitCount = 0;
        }

    }
}