using System.Collections.Generic;
using UnityEngine;
//Forcing Git Commit
/// <summary>
/// The UltrasoundManager is contained within the SMARTS_SDK.Ultrasound namespace.
/// </summary>


/// <summary>
/// 
/// Class Overview:
/// The UltrasoundManager (USM) is in charge of generating the Ultrasound (US) image. It uses the ultrasound probe's position
/// in space, relative to specified anatomies to generate realistic ultrasound images. The ultrasound plane is rendered upon
/// rebuild, and numerous variables can be used to manipulate the size and shape of the ultrasound's scanning plane. As it 
/// sits, the values used to manipulate the scanning plane are only visible in the inspector, and the only functionality of 
/// the USM is the generation of ultrasound images. The USM operates as a singleton. There can only be one instance of the
/// class operating at any one time. All other instances are destroyed at Awake(). The references to the singleton instance
/// of the USM can be made through the static UltrasoundManager ME. It can be referenced by any script using the namespace
/// SMARTS_SDK.Ultrasound. The UltrasoundManager.cs class uses world units as the default measurement, and world units are
/// generally assumed to be mm by CSSALT.
/// 
/// Future Functionality:
/// 
///		Touch ID:
///		The Touch ID was already implemented in the CSSALT CVA-MI software, and will be thoughtfully integrated to the SDK
///		version of the USM.
///		
///		Ultrasound Screen to World Space Drawing:
///		The first CSSALT TRUS prototype featured the ability to draw on the ultrasound and create matching drawings within
///		the world space. This functionality will also be added to the SDK USM.
///		
///		Dual-Axis US Image Reflection:
///		In later updates, the ultrasound probe will have the ability to be reflected vertically and horizontally across its
///		central axis.
///	
/// Dependencies:
/// This class runs as a standalone class and has no dependencies.
/// 
/// Developer:
///	A version of the ultrasound existed since before 2016 with initial concept: Dave Lizdas.
///	For the SMARTS-SDK the UltrasoundManager.cs script was completely scrapped and rebuilt from the ground up. Although in
///	principle the software operates in a seemingly similar manner (and the end result is a rendered ultrasound screen very
///	similar to the initial concept design in use for years), all rendering operations, ultrasound plane generation techniques,
///	and the increased performance is courtesy of:
///	Andre Kazimierz Bigos
///	2018.08.13
///	YYYY.MM.DD
///	16:32 EST (24h)
///	
/// </summary>
public class UltrasoundManagerSource : MonoBehaviour, IUltrasound
{
    /// This static UltrasoundManager is used to ensure a single instance of the ultrasound manager exists in the scene and 
    /// that the single instance is easily accessible to outside scripts. In awake we destroy all other possible instances.
    public static UltrasoundManagerSource ME;

    [Header("-----User Interface Managment-----")]

    /// Rendering Ultrasound or not:
    /// To be fixed with custom editor to allow for more accurate management of inspector value change
    [SerializeField]
    bool rendering = true;
    public bool Rendering
    {
        get
        {
            return rendering;
        }
        set
        {
            rendering = value;
            if (!rendering)
                ScanBlank();
            //ultrasoundUICanvasObject.SetActive(value);
            //renderUltrasoundOntoScanningPlane = value && renderUltrasoundOntoScanningPlane;
        }
    }

    /// If true, begins using small ultrasound UI, if false, begins using large ultrasound UI dimensions.
    [SerializeField]
    bool beginInSmallScreen = true;

    /// Enum for the screen dimensions. Allows further expansion and default dimensions to be added at a different time.
    /// Used to quickly and simply toggle screen between two editable sizes.
    public enum SCREEN_DIMENSIONS
    {
        LARGE = 1,
        SMALL = 2
    }

    /// Hide ultrasoundScreenSize inspector editing functionality in custom inspector. 

    public SCREEN_DIMENSIONS ultrasoundScreenSize = SCREEN_DIMENSIONS.SMALL;
    public SCREEN_DIMENSIONS UltrasoundScreenSize
    {
        get
        {
            return ultrasoundScreenSize;
        }
        set
        {
            if (value != ultrasoundScreenSize)
            {
                ultrasoundScreenSize = value;
                ToggleScreenDimensions();
            }
        }
    }

    /// Enum for the screen orientation. Allows further expansion and default orientation states to be added at a different time.
    /// Used to quickly and simply toggle screen between different rotations.
    public enum SCREEN_ORIENTATION
    {
        DEFAULT_ORIENTATION = 1,
        ROTATED_90_CLOCKWISE = 2,
        ROTATED_180_CLOCKWISE = 4,
        ROTATED_270_CLOCKWISE = 8,
    }

    /// Hide ultrasoundScreenOrientation inspector editing functionality in custom inspector. 
    [SerializeField]
    SCREEN_ORIENTATION ultrasoundScreenOrientation = SCREEN_ORIENTATION.DEFAULT_ORIENTATION;
    public SCREEN_ORIENTATION UltrasoundScreenOrientation
    {
        get
        {
            return ultrasoundScreenOrientation;
        }
        set
        {
            if (value != ultrasoundScreenOrientation)
            {
                ultrasoundScreenOrientation = value;
                ToggleScreenOrientation();
            }
        }
    }

    /// Enum for the screen inversion. Allows further expansion and default inversion states to be added at a different time.
    /// Used to quickly and simply toggle screen between different rotations.
    public enum SCREEN_INVERSION
    {
        DEFAULT_INVERSION = 0,
        INVERTED_HORIZONTALLY = 1,
        INVERTED_VERTICALLY = 2,
        INVERTED_BOTH = 3,
    }

    /// Hide ultrasoundScreenInversion inspector editing functionality in custom inspector. 
    [SerializeField]
    SCREEN_INVERSION ultrasoundScreenInversion = SCREEN_INVERSION.DEFAULT_INVERSION;
    public SCREEN_INVERSION UltrasoundScreenInversion
    {
        get
        {
            return ultrasoundScreenInversion;
        }
        set
        {
            if (value != ultrasoundScreenInversion)
            {
                ultrasoundScreenInversion = value;
                ToggleScreenInversion();
            }
        }
    }

    [SerializeField]
    float ultrasoundZoomFactor = 1;
    public float UltrasoundZoomFactor
    {
        get
        {
            return ultrasoundZoomFactor;
        }
        set
        {
            if (value > 16 || value < 1)
                throw new System.ArgumentException("Zoom Factor must be within the range [1,16], with 1 representing a full view" +
                    "of the ultrasound screen and 16 representing a 16 times zoom.");
            ultrasoundZoomFactor = value;
            ZoomUltrasoundScreen();
        }
    }

    [Header("-----Rebuild Scanning Plane During Runtime-----")]

    /// The rebuild field is a toggle that can be used during runtime to rebuild the ultrasound probe's scanning plane. It is
    /// useful in allowing the dynamic editing of the ultrasound probe's scanning plane width, depth, and angle.
    [SerializeField]
    bool rebuild = false;
    public bool Rebuild { set { rebuild = value; } }

    [Header("-----Define Scanning Plane-----")]

    /// The width field defines the width of the top of the ultrasound probe's scanning plane area. This segment is centered
    /// at the probeCeneterPoint object and extends width/2 units to the positive and negative directions along the local x
    /// axis.
    [SerializeField]
    int width = 40;///world units (mm)
    public int Width { get { return width; } set { if (value <= 0) throw new System.ArgumentOutOfRangeException("Width must be greater than 0."); width = value; } }

    /// The depth field is used to define the depth of the ultrasound’s scanning plane. This is the maximum depth the 
    /// transducer can measure at. 
    [SerializeField]
    int depth = 50;///world units (mm)
    public int Depth { get { return depth; } set { if (value <= 0) throw new System.ArgumentOutOfRangeException("Depth must be greater than 0."); depth = value; } }

    /// The angle field defines the angle at which the scanning plane deviates from the direction perpendicular to the probe’s
    /// contacting surface. An angle of zero creates a perfectly normal scanning plane, and angles nearer to 90 create almost 
    /// completely fanned out ultrasound scanning planes. Illustrative documentation will be added later to document the
    /// mechanics of this and other values more thoroughly.
    [SerializeField]
    int angle = 5;///degrees
    public int Angle { get { return angle; } set { if (value < 0 || value > 90) throw new System.ArgumentOutOfRangeException("Angle must be in the range [0,90]."); angle = value; } }

    /// The resolution field defines the pixel resolution of the ultrasound image texture. At the moment only even parity, 
    /// square resolutions are allowed. This resolution may be expanded to allow for more easily scalable solutions.
    [SerializeField]
    int resolution = 256;///pixels x pixels
    public int Resoltion { get { return resolution; } set { resolution = value; } }

    /// The beamThickness field defines the tolerance within which the ultrasound beam can travel in the direction perpendicular
    /// to the scanning plane. This gives a more realistic "flicker" to the ultrasound image as the independent RayCasts vary
    /// randomly from frame to frame.
    [SerializeField]
    float beamThickness = 1;///world units (mm)
    public float BeamThickness { get { return beamThickness; } set { beamThickness = value; } }

    /// The background color is the color of all non-rendered elements. This color is typically black, although users can make
    /// it any color imaginable.
    [SerializeField]
    Color backgroundColor = Color.black;
    public Color BackGroundColor { get { return backgroundColor; } set { backgroundColor = value; } }

    /// The attenuation is an overlay. When enabled, the ultrasound image gradually degrades and approaches this color.
    [SerializeField]
    Color attenuationColor = Color.black;
    public Color AttenuationColor { get { return attenuationColor; } set { attenuationColor = value; } }

    /// This field defines the edge color. This is the color of the area outside of the rendered ultrasound image. In all but
    /// those ultrasound images where angle = 0 (where the ultrasound image takes up the entire texture) this area remains a
    /// clear, and its color can be defined here.
    [SerializeField]
    Color edgeColor = Color.black;
    public Color EdgeColor { get { return edgeColor; } set { edgeColor = value; } }

    /// This gradient is used to rendered simplex noise within the ultrasound image. This gradient takes the result of the
    /// simplex class (a value [0,1]) and returns a color based on that calculated value.
    [SerializeField]
    Gradient texture1ColorGradient;
    public Gradient Texture1ColorGradient { get { return texture1ColorGradient; } set { texture1ColorGradient = value; } }

    ///GOING TO MAKE BETTER LATER. NEEDS TO BE ADDED FOR BME STUDENTS
    /// These fields are used to distort the simplex noise (elongate it to lessen the effect of distance traveled in a specific
    /// axis by increasing the divisors) and dampen the extremes of the gradient (modify dampener).
    [SerializeField]
    float divisorX1 = 2, divisorY1 = 8, divisorZ1 = 2, dampener1 = .8f;
    public float DivisorX1 { get { return divisorX1; } set { divisorX1 = value; } }
    public float DivisorY1 { get { return divisorY1; } set { divisorY1 = value; } }
    public float DivisorZ1 { get { return divisorZ1; } set { divisorZ1 = value; } }
    public float Dampener1 { get { return dampener1; } set { dampener1 = value; } }

    ///END HACK QUICK ADDITIONS FOR BME STUDENTS (2018.08.01 YYYY.MM.DD)

    /// This gradient is used to rendered simplex noise within the ultrasound image. This gradient takes the result of the
    /// simplex class (a value [0,1]) and returns a color based on that calculated value.
    [SerializeField]
    Gradient texture2ColorGradient;
    public Gradient Texture2ColorGradient { get { return texture2ColorGradient; } set { texture2ColorGradient = value; } }

    ///GOING TO MAKE BETTER LATER. NEEDS TO BE ADDED FOR BME STUDENTS
    /// These fields are used to distort the simplex noise (elongate it to lessen the effect of distance traveled in a specific
    /// axis by increasing the divisors) and dampen the extremes of the gradient (modify dampener).
    [SerializeField]
    float divisorX2 = 2, divisorY2 = 3, divisorZ2 = 10, dampener2 = .3f;
    public float DivisorX2 { get { return divisorX2; } set { divisorX2 = value; } }
    public float DivisorY2 { get { return divisorY2; } set { divisorY2 = value; } }
    public float DivisorZ2 { get { return divisorZ2; } set { divisorZ2 = value; } }
    public float Dampener2 { get { return dampener2; } set { dampener2 = value; } }

    ///END HACK QUICK ADDITIONS FOR BME STUDENTS (2018.08.01 YYYY.MM.DD)

    /// This multidimensional array stores data on the points used to generate the ultrasound images. It is populated when the
    /// scanning plane is built and is used in conjunction with the ever-moving center point to create the theoretical scanned
    /// image.
    Vector4[,] points;

    /// The depths are used primarily to define the attenuation overlay texture. However, this depth information can become
    /// rather useful in the future as more functionality is added.
    float[,] depths;

    /// 2D Array of boolean values listing whether or not a texture pixel is being rendered or not.
    bool[,] renderedArea;

    /// This list has definite room for improvement. It is populated when creating the scanning plane. And it is used when
    /// setting attenuation depths and when scanning. However, its usefulness can be greatly improved.
    List<Vector3>[] pixelAnalyses;

    /// The curved option remains relatively unused. It will be added with later functionality. It is currently only used 
    /// in an if statement where it is always false. It is, therefore, functionally unused.
    bool curved = false;

    /// This option gives users the option to render a very high fidelity US scanning plane, or a simpler version with 1/10th
    /// the vertexes of the exact scanning plane.
    [SerializeField]
    bool renderSimpleScanningPlane = true;
    public bool RenderSimpleScanningPlane { get { return renderSimpleScanningPlane; } set { renderSimpleScanningPlane = value; } }

    [SerializeField]
    bool renderUltrasoundOntoScanningPlane = false;
    public bool RenderUltrasoundOntoScanningPlane { get { return renderUltrasoundOntoScanningPlane; } set { renderUltrasoundOntoScanningPlane = value; } }

    [Header("-----Dynamic Ultrasound Image Settings-----")]

    /// The blur field lets users toggle on/off the blurring of the ultrasound screen. The ultrasound image is blurred using
    /// a custom shader attached to the camera looking at the ultrasound texture.
    [SerializeField]
    bool blur = false;
    public bool Blur { get { return blur; } set { blur = value; } }

    /// This toggles on/off the depth attenuation field. The field is a texture overlay placed in front of the ultrasound
    /// texture. When toggled off, the overlay is disabled.
    [SerializeField]
    bool depthAttenuation;
    public bool DepthAttenuation { get { return depthAttenuation; } set { depthAttenuation = value; } }

    /// These transformation matrices are used to transform points between the ultrasound rendered texture and the corresponding
    /// point on the ultrasound scanning plane (in relation to the probe center point).
    Matrix4x4 transformationMatrixTToU1, transformationMatrixTToU2, transformationMatrixUToT1, transformationMatrixUToT2;

    [Header("-----Generated Textures and GameObjects-----")]

    /// The currentUltrasoundMaterial field is used internally. The UltrasoundMaterial scripts attached to 
    /// objects the ultrasound needs to render are used and the current rendered ultrasound segment uses this field to minimize
    /// the amount of times the class is passed around. Eventually, the UltrasoundMaterial script will contain either a string
    /// parameter, a custom struct, or some other convenient way of passing off critical data to the UltrasoundManager.
    SMMARTS_SDK.Ultrasound.UltrasoundMaterial currentUltrasoundMaterial;

    /// This field is currently exposed in the inspector. In the future, it will likely no longer be visible in the inspector.
    /// Users should not be concerned with this texture. This is the main ultrasound texture. It is what the ideal, non-blurred,
    /// non-attenuated, ultrasound image should be.
    [SerializeField]
    Texture2D ultrasoundTexture;
    public Texture2D RawUltrasoundTexture
    { get { return ultrasoundTexture; } }

    /// This field is currently exposed in the inspector. In the future, it will likely no longer be visible in the inspector.
    /// Users should not be concerned with this texture. This is the attenuation overlay. It is created when the scanning plane is
    /// rebuilt and is used to attenuate the ultrasound image.
    [SerializeField]
    Texture2D attenuationOverlayTexture;

    /// This overlay obscures the sides of the ultrasound image that are not rendered. Eventually, this overlay will become a 
    /// background to the ultrasound texture to ensure full 100% accurate coverage of the sides of the ultrasound image. This will
    /// involve changing the main ultrasound rendered texture's GameObject's material to one that allows transparency.
    [SerializeField]
    Texture2D edgeOverlayTexture;

    /// This is the scanning plane that is created whenever the scanning plane is built. Its mesh is created vertex by vertex at 
    /// the time of its build. Although currently only visible in the inspector, this GameObject will eventually be accessible by
    /// users. Because it is created at runtime, it is much easier to provide users with a reference to the GameObject than 
    /// forcing them find it each time it is recreated during a rebuild.
    [SerializeField]
    GameObject scanningPlane;
    public GameObject ScanningPlane
    { get { return scanningPlane; } }

    [SerializeField]
    Material defaultScanningPlaneMaterial, renderedScanningPlaneMaterial;
    public Material DefaultScanningPlaneMaterial { get { return defaultScanningPlaneMaterial; } set { defaultScanningPlaneMaterial = value; } }

    [Header("-----Scoring Elements-----")]
    [SerializeField]
    float angleNeedleProbe;
    public float AngleNeedleProbe { get { return angleNeedleProbe; } }
    [SerializeField]
    float angleSkinProbe;
    public float AngleSkinProbe { get { return angleSkinProbe; } }
    [SerializeField]
    bool needleTipInView;
    public bool NeedleTipInView { get { return needleTipInView; } }
    [SerializeField]
    bool needleShaftInView;
    public bool NeedleShaftInView { get { return needleShaftInView; } }
    [SerializeField]
    bool needleTipViewed;
    public bool NeedleTipViewed { get { return needleTipViewed; } }
    [SerializeField]
    bool needleShaftViewed;
    public bool NeedleShaftViewed { get { return needleShaftViewed; } }

    [Header("-----Initialized GameObjects-----")]

    /// This is the ultrasoundProbe GameObject. Its primary function is to assign the scanning plane as a child of this object's
    /// transform to ensure simplicity in the scene hierarchy. This GameObject is currently only visible in the inspector, but
    /// will most like be made more accessible in later updates.
    [SerializeField]
    GameObject ultrasoundProbe;
    public GameObject UltrasoundProbe { get { return ultrasoundProbe; } }

    /// This GameObject is absolutely crucial. It is used to align the probe's scanning plane. The scanning plane is centered at
    /// this important GameObject, and aligned with its orientation.
    [SerializeField]
    GameObject probeCenterPoint;
    public GameObject ProbeCenterPoint { get { return probeCenterPoint; } }

    /// The rendered ultrasound texture is visible in the inspector because of the ease of its assignment. This is the GameObject
    /// to which the ultrasound texture is added. The ultrasoundTextureCamera GameObject's camera component is pointed towards
    /// this GameObject. This is the basis for all ultrasound rendering.
    [SerializeField]
    GameObject renderedUltrasoundTexture;

    /// The attenuationOverlay GameObject is toggled on/off as directed by the depthAttenuation boolean. This overlay obscures the
    /// renderedUltrasoundTexture GameObject as it is between it and the ultrasoundTextureCamera. The non-unity alpha values of the
    /// texture's pixels ensure it that the rendered texture behind it is visible in the ultrasound screen, and is only partially
    /// obscured by the overlay.
    [SerializeField]
    GameObject attenuationOverlay;

    /// The edge overlay is used as another overlay. However, unlike the attenuation overlay, this overlay has either zero or unity
    /// alpha values. The edges of the rendered ultrasound screen are rendered the color edgeColor and the section of the texture
    /// that would normally obscure the rendered ultrasound texture's ultrasound area are assigned alpha values of zero.
    /// In the future, this overlay will become a background image, and the rendered ultrasound image will use alpha values of
    /// zero in the non-rendered area to have the edge colors come better.
    [SerializeField]
    GameObject edgeOverlay;

    /// This is the GameObject that contains the camera that captures all the rendered ultrasound textures, overlays, and applies
    /// all blur effects. It is visible in the inspector, however, this object should not be moved, modified, or changed in any
    /// way by  the user.
    [SerializeField]
    GameObject ultrasoundTextureCamera;

    /// The GameObject which holds the canvas upon which the ultrasound is rendered.
    [SerializeField]
    GameObject ultrasoundUICanvasObject = null;
    public GameObject UltrasoundUICanvasObject { get { return ultrasoundUICanvasObject; } }

    /// The GameObject whose canvas holds the large dimensions used for the increasing size of the ultrasound.
    [SerializeField]
    GameObject largeUltrasoundUICanvasObject = null;
    public GameObject LargeUltrasoundUICanvasObject { get { return largeUltrasoundUICanvasObject; } }

    /// The GameObject whose canvas holds the small dimensions use for the decreasing size of the ultrasound.
    [SerializeField]
    GameObject smallUltrasoundUICanvasObject = null;
    public GameObject SmallUltrasoundUICanvasObject { get { return smallUltrasoundUICanvasObject; } }

    [SerializeField]
    GameObject needleTip = null;
    public GameObject NeedleTip { get { return needleTip; } }

    [SerializeField]
    GameObject needleShaft;
    public GameObject NeedleShaft { get { return needleShaft; } }


    /// <summary>
    /// In Awake() we create the static singleton reference UltrasoundManager.ME. It also destroys every unnecessary instance of 
    /// the ultrasound ensuring the single instance.
    /// </summary>
    private void Awake()
    {
        if (ME != null)
            Destroy(ME);
        ME = this;
    }

    /// <summary>
    /// In start, we build the scanning plane as it is described in the inspector at the beginning of runtime.
    /// </summary>
    private void Start()
    {
        BuildUltrasoundScanningPlane();
        if (beginInSmallScreen)
            ultrasoundScreenSize = SCREEN_DIMENSIONS.SMALL;
        else
            ultrasoundScreenSize = SCREEN_DIMENSIONS.LARGE;
        ToggleScreenDimensions();
    }

    /// <summary>
    /// This is the method where all the creation occurs. This is where the ultrasound plane is created, where every texture
    /// is created and initialized, and where all overlays are built. It is called from within Start(), and can be called at any
    /// time during runtime using the rebuild toggle. This method destroys or zeros all elements and allows the new scanning
    /// plane to be built sans errors.
    /// 
    /// I'm going to leave the internal mechanics of this method uncommented for now. So many additions are expected for this
    /// class that it seems a tinge redundant to add documentation that will inevitably be modified within the next month.
    /// </summary>
    void BuildUltrasoundScanningPlane()
    {
        if (resolution < 32)
        {
            Debug.Log("Ultrasound texture resolution cannot be less than 32x32. The resolution has been forced to 32x32.");
            resolution = 2;
        }
        if (resolution > 2048)
        {
            Debug.Log("Ultrasound texture resolution is capped at 2048x2048. The resolution has been forced to 2048x2048.");
            resolution = 2048;
        }
        if (resolution % 2 != 0)
        {
            Debug.Log("Only even valued resolutions are accepted. The resolution " + resolution + "x" + resolution + " is not an acceptable resolution. " +
            "The resolution has been forced to " + (resolution + 1) + "x" + (resolution + 1) + ".");
            resolution += 1;
        }
        if (scanningPlane != null)
            Destroy(scanningPlane);
        //renderedUltrasoundTexture.GetComponent<MeshRenderer>().material = Instantiate(Resources.Load("Default Ultrasound Texture")) as Material;
        ///Special case angle = 0;
        if (angle == 0)
        {
            ultrasoundTexture = new Texture2D(resolution, resolution);
            for (int x = 0; x < resolution; x++)
            {
                for (int y = 0; y < resolution; y++)
                {
                    ultrasoundTexture.SetPixel(x, y, Color.clear);
                }
            }
            points = new Vector4[4, resolution];
            pixelAnalyses = new List<Vector3>[resolution];
            depths = new float[resolution, resolution];
            renderedArea = new bool[resolution, resolution];
            for (int x = 0; x < resolution; x++)
            {
                points[0, x] = new Vector4(x, 0, 1);
                points[1, x] = new Vector4(x, resolution - 1, 1);
                points[2, x] = new Vector4(-(width / 2) + width * x / resolution - 1, 0, 1);
                points[3, x] = new Vector4(-(width / 2) + width * x / resolution - 1, depth, 1);
                List<Vector3> currentAnalysis = new List<Vector3>(2);
                currentAnalysis.Add(new Vector3(0, x, 0));
                currentAnalysis.Add(new Vector3(1, x, resolution - 1));
                pixelAnalyses[x] = currentAnalysis;
                for (int y = 0; y < resolution; y++)
                {
                    depths[x, y] = depth * y / (resolution - 1);
                }
            }
            attenuationOverlayTexture = new Texture2D(resolution, resolution);
            attenuationOverlayTexture.filterMode = FilterMode.Point;
            edgeOverlayTexture = new Texture2D(resolution, resolution);
            edgeOverlayTexture.filterMode = FilterMode.Point;
            CreateScanningPlane();
            SetAttenuationDepths(); // Depths should not be set above if this is called. (Redundant)


            transformationMatrixTToU1 = new Matrix4x4(new Vector4(1, 0), new Vector4(0, 1), new Vector4(-(resolution - 1) / 2, 0, 1), Vector4.zero);
            transformationMatrixTToU2 = new Matrix4x4(new Vector4(1f * (width) / (resolution - 1), 0), new Vector4(0, 1f * depth / (resolution - 1)), Vector4.zero, Vector4.zero);
            transformationMatrixUToT1 = new Matrix4x4(new Vector4((resolution - 1) / (width), 0), new Vector4(0, (resolution - 1) / depth), new Vector4(0, 0, 1), Vector4.zero);
            transformationMatrixUToT2 = new Matrix4x4(new Vector4(1, 0), new Vector4(0, 1), new Vector4((resolution - 1) / 2, 0, 1), Vector4.zero);


            return;
        }
        float depthU = depth;
        float widthDEU = width;
        float thetaU = Mathf.Deg2Rad * angle;
        float widthDBU = depthU * Mathf.Sin(thetaU);
        float depthDBU = depthU * Mathf.Cos(thetaU);
        float widthBCU = widthDBU * 2 + widthDEU;
        float ratioDWU = depthU / widthBCU;
        Vector4 focalPointU = new Vector4(0, (-widthDEU / 2) / Mathf.Tan(thetaU), 1);
        //Vector4 aU = new Vector4(0, depthU, 1);
        Vector4 bU = new Vector4(-widthDEU / 2 - widthDBU, depthDBU, 1);
        //Vector4 cU = new Vector4(-bU.x, bU.y, 1);
        Vector4 dU = new Vector4(-widthDEU / 2, 0, 1);
        //Vector4 eU = new Vector4(-dU.x, 0, 1);

        ultrasoundTexture = new Texture2D(resolution, resolution);///REMOVE
        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                ultrasoundTexture.SetPixel(x, y, Color.clear);
            }
        }
        depths = new float[resolution, resolution];
        renderedArea = new bool[resolution, resolution];
        float resolutionT = resolution - 1;
        float depthT;
        float widthBCT;
        if (ratioDWU > 1) { depthT = resolutionT; widthBCT = resolutionT / ratioDWU; }
        else { widthBCT = resolutionT; depthT = resolutionT * ratioDWU; }
        float midXT = resolutionT / 2;
        transformationMatrixTToU1 = new Matrix4x4(new Vector4(1, 0), new Vector4(0, 1), new Vector4(-midXT, 0, 1), Vector4.zero);
        transformationMatrixTToU2 = new Matrix4x4(new Vector4(widthBCU / widthBCT, 0), new Vector4(0, depthU / depthT), Vector4.zero, Vector4.zero);
        transformationMatrixUToT1 = new Matrix4x4(new Vector4(widthBCT / widthBCU, 0), new Vector4(0, depthT / depthU), new Vector4(0, 0, 1), Vector4.zero);
        transformationMatrixUToT2 = new Matrix4x4(new Vector4(1, 0), new Vector4(0, 1), new Vector4(midXT, 0, 1), Vector4.zero);

        //Vector4 aT = transformationMatrixUToT2 * (transformationMatrixUToT1 * aU);
        Vector4 bT = transformationMatrixUToT2 * (transformationMatrixUToT1 * bU);
        //Vector4 cT = transformationMatrixUToT2 * (transformationMatrixUToT1 * cU);
        Vector4 dT = transformationMatrixUToT2 * (transformationMatrixUToT1 * dU);
        //Vector4 eT = transformationMatrixUToT2 * (transformationMatrixUToT1 * eU);
        Vector4 focalPointT = transformationMatrixUToT2 * (transformationMatrixUToT1 * focalPointU);
        float thetaT = Vector2.Angle(bT - dT, Vector2.up) * Mathf.Deg2Rad;
        //float widthDET = Vector2.Distance(dT, eT);

        List<Vector4> startPointsU = new List<Vector4>();
        List<Vector4> endPointsU = new List<Vector4>();
        List<Vector4> startPointsT = new List<Vector4>();
        List<Vector4> endPointsT = new List<Vector4>();
        Vector4 p0S = dT;
        Vector4 p0E = bT;
        float theta0 = thetaT;
        startPointsT.Add(p0S);
        endPointsT.Add(p0E);
        float distanceT = Vector4.Distance(p0S, p0E);
        while (true)
        {
            Vector4 p1E = p0E + new Vector4(Mathf.Cos(theta0), Mathf.Sin(theta0));
            Vector4 p1S;
            if (!curved)
                p1S = IntersectYEquals0(p1E, focalPointT);
            else
                p1S = p1E + Vector4.Normalize(focalPointT - p1E) * distanceT;
            float distance = Vector4.Distance(p1E, p1S);
            float lerpValue = depthT / distance;
            p1E = Vector4.Lerp(p1S, p1E, lerpValue);
            endPointsT.Add(p1E);
            startPointsT.Add(p1S);
            p0S = p1S;
            p0E = p1E;
            Vector2 dir = p1E - p1S;
            theta0 = Vector2.Angle(dir, Vector2.up) * Mathf.Deg2Rad;
            if (midXT - p1E.x <= 1)
            {
                if (midXT - p1E.x > .5)
                {
                    p1E = p0E + new Vector4(Mathf.Cos(theta0) / 2, Mathf.Sin(theta0) / 2);
                    p1S = IntersectYEquals0(p1E, focalPointT);
                    distance = Vector4.Distance(p1E, p1S);
                    lerpValue = depthT / distance;
                    p1E = Vector4.Lerp(p1S, p1E, lerpValue);
                    endPointsT.Add(p1E);
                    startPointsT.Add(p1S);
                }
                break;
            }
        }
        for (int x = startPointsT.Count - 1; x >= 0; x--)
        {
            Vector4 sPX = startPointsT[x];
            sPX.x = midXT + (midXT - sPX.x);
            startPointsT.Add(sPX);
            Vector4 ePX = endPointsT[x];
            ePX.x = midXT + (midXT - ePX.x);
            endPointsT.Add(ePX);
        }
        for (int x = 0; x < startPointsT.Count; x++)
        {
            startPointsU.Add(transformationMatrixTToU2 * (transformationMatrixTToU1 * startPointsT[x]));
            endPointsU.Add(transformationMatrixTToU2 * (transformationMatrixTToU1 * endPointsT[x]));
        }
        points = new Vector4[4, endPointsT.Count];
        for (int x = 0; x < startPointsT.Count; x++)
        {
            points[0, x] = startPointsT[x];
            points[1, x] = endPointsT[x];
            points[2, x] = startPointsU[x];
            points[3, x] = endPointsU[x];
        }

        pixelAnalyses = new List<Vector3>[endPointsT.Count];
        for (int x = 0; x < pixelAnalyses.Length / 2; x++)
        {
            List<Vector3> currentLineAnalysis = new List<Vector3>();
            float xPixS = startPointsT[x].x;
            float yPixS = startPointsT[x].y;
            float xPixE = endPointsT[x].x;
            float yPixE = endPointsT[x].y;
            float xPix = xPixS;
            float yPix = yPixS;
            currentLineAnalysis.Add(new Vector3(0, xPixS, yPixS));
            float dx = Mathf.Abs(xPixE - xPixS);
            float dy = yPixE - yPixS;
            float m = dy / dx;
            float dist = 0;
            if ((int)xPixS != (int)xPixE)
            {

                xPix = (int)xPixS;
                dist = Mathf.Abs((xPix - xPixS) / dx);
                yPix = dist * dy + yPix;
                currentLineAnalysis.Add(new Vector3(dist, xPix, yPix));
                while (true)
                {
                    xPix--;
                    yPix += m;
                    dist += 1 / dx;
                    if ((int)xPixE >= (int)(xPix))
                    {
                        break;
                    }
                    currentLineAnalysis.Add(new Vector3(dist, xPix, yPix));
                }
            }
            currentLineAnalysis.Add(new Vector3(1, xPixE, yPixE));
            pixelAnalyses[x] = currentLineAnalysis;
        }
        attenuationOverlayTexture = new Texture2D(resolution, resolution);
        attenuationOverlayTexture.filterMode = FilterMode.Point;
        edgeOverlayTexture = new Texture2D(resolution, resolution);
        edgeOverlayTexture.filterMode = FilterMode.Point;
        for (int x = pixelAnalyses.Length / 2; x < pixelAnalyses.Length; x++)
        {
            List<Vector3> oppositeAnalysis = new List<Vector3>();
            List<Vector3> leftAnalysis = pixelAnalyses[pixelAnalyses.Length - x - 1];
            for (int y = 0; y < leftAnalysis.Count; y++)
            {
                oppositeAnalysis.Add(new Vector3(leftAnalysis[y].x, midXT + (midXT - leftAnalysis[y].y), leftAnalysis[y].z));
            }
            pixelAnalyses[x] = oppositeAnalysis;
        }
        CreateScanningPlane();
        SetAttenuationDepths();
    }

    /// <summary>
    /// In update, if the ultrasound plane has been requested to be rebuilt, that is, the "rebuild" toggle in the inspector has
    /// been set to true, the ultrasound scanning plane will be rebuilt. In update, we also perform the scanning of the
    /// ultrasound.
    /// </summary>
    private void Update()
    {
        if (rebuild)
        {
            rebuild = false;
            BuildUltrasoundScanningPlane();
        }
        if (!rendering)
            return;
        Scan();
    }

    /// <summary>
    /// This method is a functional method that returns the intersection point of the line connecting two points, and the y=0
    /// axis. Although the input parameters are Vector4s, the only components that are used to find the intersection point
    /// are the x and y components of the Vector4s. Essentially, the Vector4 is treated as a Vector2 and the intersection 
    /// point is calculated accordingly, and returned as a Vector4 (with the z component = 1).
    /// </summary>
    /// <param name="point1">The first of two points to calculate the intersection of.</param>
    /// <param name="point2">The second of two points to calculate the intersection of.</param>
    /// <returns>The intersection of the line connecting the two points, and the y = 0 lines.</returns>
    Vector4 IntersectYEquals0(Vector4 point1, Vector4 point2)
    {
        float y1 = point1.y;
        float x1 = point1.x;
        float y2 = point2.y;
        float x2 = point2.x;
        float x = -y1 * (x2 - x1) / (y2 - y1) + x1;
        float y = 0;
        return new Vector4(x, y, 1);
    }

    /// <summary>
    /// The Scan method handles the actual ultrasound scan. Once per frame, this method is called, and it uses
    /// Physics.RaycastAll() between the pre-set points set within points, adjusted for the movement of the probeCenterPoint
    /// GameObject. This method takes the RaycastHit[] output and uses the data on hit distance in conjunction with the hit
    /// GameObject's UltrasoundMaterial to generate a representation of the ultrasound image.
    /// 
    /// I'm going to leave the internal mechanics of this method uncommented for now. So many additions are expected for this
    /// class that it seems a tinge redundant to add documentation that will inevitably be modified within the next month.
    /// </summary>
    void Scan()
    {
        for (int x = 0; x < points.GetLength(1); x++)
        {
            Vector3 startPointU = points[2, x];
            Vector3 endPointU = points[3, x];
            startPointU.z = Random.Range(-beamThickness / 2, beamThickness / 2);
            endPointU.z = startPointU.z;
            Vector3 globalStartPointU = probeCenterPoint.transform.TransformPoint(startPointU);
            Vector3 globalEndPointU = probeCenterPoint.transform.TransformPoint(endPointU);
            RaycastHit[] forward = Physics.RaycastAll(globalStartPointU, globalEndPointU - globalStartPointU, depth);
            RaycastHit[] backward = Physics.RaycastAll(globalEndPointU, globalStartPointU - globalEndPointU, depth);
            List<ScannedElement> elements = new List<ScannedElement>();
            for (int y = 0; y < forward.Length; y++)
            {
                RaycastHit hit = forward[y];
                SMMARTS_SDK.Ultrasound.UltrasoundMaterial uM = hit.transform.GetComponent<SMMARTS_SDK.Ultrasound.UltrasoundMaterial>();
                if (hit.collider.gameObject.name == needleTip.name)
                {
                    needleTipViewed = true;
                    needleTipInView = true;
                }
                else
                {
                    needleTipInView = false;
                }
                if (hit.collider.gameObject.name == needleShaft.name)
                {
                    needleShaftViewed = true;
                    needleShaftInView = true;
                }
                else
                {
                    needleShaftInView = false;
                }
                if (uM != null && uM.ShowOnUltrasound)
                {
                    float angle = Mathf.Abs(90f - Vector3.Angle(hit.normal, globalStartPointU - globalEndPointU));
                    if (angle >= uM.AnisoIndex)
                    {
                        ScannedElement sE = new ScannedElement(hit.distance / depth, true, uM, hit.transform.GetInstanceID());
                        elements.Add(sE);
                    }
                }
            }
            if (elements.Count == 0)
            {
                Render(x, 0, 1, null, 0);
            }
            else
            {
                for (int y = 0; y < backward.Length; y++)
                {
                    RaycastHit hit = backward[y];
                    if (hit.collider.gameObject.name == needleTip.name)
                    {
                        needleTipViewed = true;
                        needleTipInView = true;
                    }
                    else
                    {
                        needleTipInView = false;
                    }
                    if (hit.collider.gameObject.name == needleShaft.name)
                    {
                        needleShaftViewed = true;
                        needleShaftInView = true;
                    }
                    else
                    {
                        needleShaftInView = false;
                    }
                    SMMARTS_SDK.Ultrasound.UltrasoundMaterial uM = hit.transform.GetComponent<SMMARTS_SDK.Ultrasound.UltrasoundMaterial>();
                    if (uM != null && uM.ShowOnUltrasound)
                    {
                        float angle = Mathf.Abs(90f - Vector3.Angle(hit.normal, globalEndPointU - globalStartPointU));
                        if (angle >= uM.AnisoIndex)
                        {
                            ScannedElement sE = new ScannedElement(1 - (hit.distance / depth), false, uM, hit.transform.GetInstanceID());
                            elements.Add(sE);
                        }
                    }
                }
                for (int y = 0; y < elements.Count - 1; y++)
                {
                    ScannedElement sEY = elements[y];
                    for (int z = y + 1; z < elements.Count; z++)
                    {
                        ScannedElement sEZ = elements[z];
                        if (sEZ.distance < sEY.distance)
                        {
                            elements[y] = sEZ;
                            elements[z] = sEY;
                            sEY = sEZ;
                        }
                    }
                }
                List<ScannedElement> open = new List<ScannedElement>();
                List<int> openID = new List<int>();
                ScannedElement current = new ScannedElement(true);
                float lastRenderedDistance = 0;
                int lastRenderedIndex = 0;
                for (int y = 0; y < elements.Count; y++)
                {
                    current = elements[y];
                    if (current.direction)
                    {
                        if (open.Count == 0)
                        {
                            lastRenderedIndex = Render(x, lastRenderedDistance, current.distance, null, lastRenderedIndex);
                        }
                        else
                        {
                            lastRenderedIndex = Render(x, lastRenderedDistance, current.distance, open[open.Count - 1].uMaterial, lastRenderedIndex);
                        }
                        open.Add(current);
                        openID.Add(current.instanceID);
                        lastRenderedDistance = current.distance;


                        if (current.uMaterial.Type == SMMARTS_SDK.Ultrasound.UltrasoundMaterial.RenderType.Bone)
                        {
                            float f = 0.02f;
                            lastRenderedIndex = RenderBoneEdge(x, current.distance, current.distance + f, Color.white, lastRenderedIndex);
                            lastRenderedIndex = Render(x, current.distance + f, 1, current.uMaterial, lastRenderedIndex);
                            break;
                        }


                    }
                    else
                    {
                        if (open.Count > 0)
                        {
                            lastRenderedIndex = Render(x, lastRenderedDistance, current.distance, open[open.Count - 1].uMaterial, lastRenderedIndex);
                            if (openID.Contains(current.instanceID))
                            {
                                int index = openID.LastIndexOf(current.instanceID);
                                openID.RemoveAt(index);
                                open.RemoveAt(index);
                            }
                        }
                        else
                        {
                            lastRenderedIndex = Render(x, lastRenderedDistance, current.distance, null, lastRenderedIndex);
                        }
                        lastRenderedDistance = current.distance;
                    }
                }
                if (open.Count > 0)
                    lastRenderedIndex = Render(x, lastRenderedDistance, 1, open[open.Count - 1].uMaterial, lastRenderedIndex);
                else
                    lastRenderedIndex = Render(x, lastRenderedDistance, 1, null, lastRenderedIndex);
            }
        }
        ultrasoundTextureCamera.GetComponent<SMMARTS_SDK.Ultrasound.BlurEffect>().enabled = blur;
        attenuationOverlay.SetActive(depthAttenuation);
        ApplyTexture();
        renderedUltrasoundTexture.GetComponent<MeshRenderer>().material.mainTexture = ultrasoundTexture;
        if (renderUltrasoundOntoScanningPlane)
            renderedScanningPlaneMaterial.mainTexture = ultrasoundTexture;
    }

    void ScanBlank()
    {
        for (int x = 0; x < ultrasoundTexture.width; x++)
        {
            for (int y = 0; y < ultrasoundTexture.height; y++)
            {
                ultrasoundTexture.SetPixel(x, y, renderedArea[x, y] ? backgroundColor : Color.clear);
            }
        }
        ultrasoundTexture.Apply();
    }


    /// <summary>
    /// This method is perhaps a little redundant. It applies the ultrasoundTexture's new pixels.
    /// </summary>
    void ApplyTexture()
    {
        ultrasoundTexture.Apply();
    }

    /// <summary>
    /// Called from within the Scan() method, this method determines more specifically the exact parameters of the rendering
    /// process. It is an intermediary between the Scan() and RenderLine() methods. This class more closely analyses the
    /// information about what to render, and passes the task of actually setting the pixels to the RenderLine() method.
    /// 
    /// I'm going to leave the internal mechanics of this method uncommented for now. So many additions are expected for this
    /// class that it seems a tinge redundant to add documentation that will inevitably be modified within the next month.
    /// </summary>
    /// <param name="line">The line corresponding to the current Raycast</param>
    /// <param name="dist1">The fist distance of the given segment being rendered.</param>
    /// <param name="dist2">The second distance of the given segment being rendered.</param>
    /// <param name="USM">The UltrasoundMaterial of the segment being rendered.</param>
    /// <param name="lastRenderedIndex">The index corresponding to the last rendered pixelAnalysis for the current line.</param>
    /// <returns></returns>
    int Render(int line, float dist1, float dist2, SMMARTS_SDK.Ultrasound.UltrasoundMaterial USM, int lastRenderedIndex)
    {
        currentUltrasoundMaterial = USM;
        List<Vector3> analysis = pixelAnalyses[line];
        for (int x = lastRenderedIndex; x < analysis.Count - 1; x++)
        {
            Vector3 current = analysis[x];
            Vector3 next = analysis[x + 1];
            if (current.x < dist1)
            {
                if (next.x > dist2)
                {
                    RenderLine(current.y, Mathf.Lerp(current.z, next.z, ((dist1 - current.x) / (next.x - current.x))), Mathf.Lerp(current.z, next.z, ((dist2 - current.x) / (next.x - current.x))));
                    return x;
                }
                else if (next.x > dist1)
                {
                    RenderLine(current.y, Mathf.Lerp(current.z, next.z, ((dist1 - current.x) / (next.x - current.x))), next.z);
                }
            }
            else if (next.x > dist2)
            {
                RenderLine(current.y, current.z, Mathf.Lerp(current.z, next.z, ((dist2 - current.x) / (next.x - current.x))));
                return x;
            }
            else
            {
                RenderLine(current.y, current.z, next.z);
            }
        }
        return analysis.Count;
    }

    /// <summary>
    /// This method sets the pixels on the x line of the texture between [y1,y2].
    /// The actual setting of the pixels is performed in the SetPixel() method. However, this method dictates exactly
    /// which pixels to set.
    /// </summary>
    /// <param name="x">The x texture coordinate of the segment being rendered.</param>
    /// <param name="y1">The first y coordinate of the segment being rendered.</param>
    /// <param name="y2">The second y coordinate of the segment being rendered.</param>
    void RenderLine(float x, float y1, float y2)
    {
        float y = y1;
        while (true)
        {
            SetPixel(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
            y++;
            if (y > y2)
            {
                SetPixel(Mathf.RoundToInt(x), Mathf.RoundToInt(y2));
                break;
            }
        }
    }

    /// <summary>
    /// Here, the actual pixels of the ultrasoundTexture are set individually. The color of the rendered pixel is determined
    /// using the currentUltrasoundMaterial set within the Render() method. Depending on the UltrasoundMaterial.RenderType 
    /// of the specified UltrasoundMaterial, certain render criteria are met differently. This method leaves room for
    /// improvement and will be optimized in later updates.
    /// 
    /// I'm going to leave the internal mechanics of this method uncommented for now. So many additions are expected for this
    /// class that it seems a tinge redundant to add documentation that will inevitably be modified within the next month.
    /// </summary>
    /// <param name="x">X texture coordinate to set the color of.</param>
    /// <param name="y">Y texture coordinate to set the color of.</param>
    void SetPixel(int x, int y)
    {
        if (currentUltrasoundMaterial == null)
        {
            ultrasoundTexture.SetPixel(x, y, backgroundColor);
        }
        else if (currentUltrasoundMaterial.Type == SMMARTS_SDK.Ultrasound.UltrasoundMaterial.RenderType.Normal)
        {
            ultrasoundTexture.SetPixel(x, y, currentUltrasoundMaterial.Color);
        }
        else if (currentUltrasoundMaterial.Type == SMMARTS_SDK.Ultrasound.UltrasoundMaterial.RenderType.Bone)
        {
            ultrasoundTexture.SetPixel(x, y, currentUltrasoundMaterial.Color);
        }
        else if (currentUltrasoundMaterial.Type == SMMARTS_SDK.Ultrasound.UltrasoundMaterial.RenderType.Lung)
        {
            if (Random.value >= .98)
            {
                ultrasoundTexture.SetPixel(x, y, Color.white);
            }
            else
            {
                ultrasoundTexture.SetPixel(x, y, currentUltrasoundMaterial.Color);
            }
        }
        else if (currentUltrasoundMaterial.Type == SMMARTS_SDK.Ultrasound.UltrasoundMaterial.RenderType.Texture1)
        {
            Vector4 ultrasoundScreenPoint = new Vector4(x, y, 1);
            Vector4 ultrasoundWorldPoint4 = (transformationMatrixTToU2 * (transformationMatrixTToU1 * ultrasoundScreenPoint));
            Vector3 ultrasoundWorldPoint = ultrasoundWorldPoint4;
            ultrasoundWorldPoint = probeCenterPoint.transform.TransformPoint(ultrasoundWorldPoint);

            ///GOING TO MAKE BETTER LATER. NEEDS TO BE ADDED FOR BME STUDENTS
            ultrasoundWorldPoint.x /= divisorX1;
            ultrasoundWorldPoint.y /= divisorY1;
            ultrasoundWorldPoint.z /= divisorZ1;
            float noiseValue = SMMARTS_SDK.Ultrasound.SimplexNoise.Simplex3D(ultrasoundWorldPoint);
            noiseValue = noiseValue * (1 - dampener1) + dampener1;
            ///END HACK QUICK ADDITIONS FOR BME STUDENTS (2018.08.01 YYYY.MM.DD)

            ultrasoundTexture.SetPixel(x, y, texture1ColorGradient.Evaluate(noiseValue));
        }
        else if (currentUltrasoundMaterial.Type == SMMARTS_SDK.Ultrasound.UltrasoundMaterial.RenderType.Texture2)
        {
            Vector4 ultrasoundScreenPoint = new Vector4(x, y, 1);
            Vector4 ultrasoundWorldPoint4 = (transformationMatrixTToU2 * (transformationMatrixTToU1 * ultrasoundScreenPoint));
            Vector3 ultrasoundWorldPoint = ultrasoundWorldPoint4;
            ultrasoundWorldPoint = probeCenterPoint.transform.TransformPoint(ultrasoundWorldPoint);

            ///GOING TO MAKE BETTER LATER. NEEDS TO BE ADDED FOR BME STUDENTS
            ultrasoundWorldPoint.x /= divisorX2;
            ultrasoundWorldPoint.y /= divisorY2;
            ultrasoundWorldPoint.z /= divisorZ2;
            float noiseValue = SMMARTS_SDK.Ultrasound.SimplexNoise.Simplex3D(ultrasoundWorldPoint);
            noiseValue = noiseValue * (2 - dampener2) + dampener2;
            ///END HACK QUICK ADDITIONS FOR BME STUDENTS (2018.08.01 YYYY.MM.DD)

            ultrasoundTexture.SetPixel(x, y, texture2ColorGradient.Evaluate(noiseValue));
        }
    }

    /// <summary>
    /// Used just as the Render() Method is. However, this is a special rendering method for creating the bright edge effect on bones.
    /// In the future, this method will be either expanded, or otherwise modified to better incorporate it into other existing methods.
    /// </summary>
    /// <param name="line">The line corresponding to the current Raycast</param>
    /// <param name="dist1">The fist distance of the given segment being rendered.</param>
    /// <param name="dist2">The second distance of the given segment being rendered.</param>
    /// <param name="edgeColor">The color to which the pixels will be set.</param>
    /// <param name="lastRenderedIndex">The index corresponding to the last rendered pixelAnalysis for the current line.</param>
    /// <returns></returns>
    int RenderBoneEdge(int line, float dist1, float dist2, Color edgeColor, int lastRenderedIndex)
    {
        List<Vector3> analysis = pixelAnalyses[line];
        for (int x = lastRenderedIndex; x < analysis.Count - 1; x++)
        {
            Vector3 current = analysis[x];
            Vector3 next = analysis[x + 1];
            if (current.x < dist1)
            {
                if (next.x > dist2)
                {
                    RenderLine(current.y, Mathf.Lerp(current.z, next.z, ((dist1 - current.x) / (next.x - current.x))), Mathf.Lerp(current.z, next.z, ((dist2 - current.x) / (next.x - current.x))), edgeColor);
                    return x;
                }
                else if (next.x > dist1)
                {
                    RenderLine(current.y, Mathf.Lerp(current.z, next.z, ((dist1 - current.x) / (next.x - current.x))), next.z, edgeColor);
                }
            }
            else if (next.x > dist2)
            {
                RenderLine(current.y, current.z, Mathf.Lerp(current.z, next.z, ((dist2 - current.x) / (next.x - current.x))), edgeColor);
                return x;
            }
            else
            {
                RenderLine(current.y, current.z, next.z, edgeColor);
            }
        }
        return analysis.Count;
    }

    /// <summary>
    /// Exactly the same as the RenderLine() method above, however, a specific color is passed.
    /// This is used exclusively by the RenderBoneEdge() method to allow for special bone edge
    /// flares to be added.
    /// </summary>
    /// <param name="x">The x texture coordinate of the segment being rendered.</param>
    /// <param name="y1">The first y coordinate of the segment being rendered.</param>
    /// <param name="y2">The second y coordinate of the segment being rendered.</param>
    /// <param name="c">The color to set the pixels to.</param>
    void RenderLine(float x, float y1, float y2, Color c)
    {
        float y = y1;
        while (true)
        {
            ultrasoundTexture.SetPixel(Mathf.RoundToInt(x), Mathf.RoundToInt(y), c);
            y++;
            if (y > y2)
            {
                ultrasoundTexture.SetPixel(Mathf.RoundToInt(x), Mathf.RoundToInt(y2), c);
                break;
            }
        }
    }

    /// <summary>
    /// The ScannedElement is a struct used by the Scan() method to store critical data efficiently.
    /// </summary>
    struct ScannedElement
    {
        public float distance;
        public bool direction;
        public SMMARTS_SDK.Ultrasound.UltrasoundMaterial uMaterial;
        public int instanceID;
        public ScannedElement(float Distance, bool DirectionForward, SMMARTS_SDK.Ultrasound.UltrasoundMaterial UMaterial, int InstanceID)
        {
            distance = Distance;
            direction = DirectionForward;
            uMaterial = UMaterial;
            instanceID = InstanceID;
        }
        public ScannedElement(bool nullConstructor)
        {
            distance = 0;
            direction = true;
            uMaterial = null;
            instanceID = 0;
        }
    }

    /// <summary>
    /// This is the method that actually creates the Scanning plane. Depending on the state of renderSimpleScanningPlane,
    /// (when false) this method creates either an ideal ultrasound scanning plane that perfectly matches the raycast lines,
    /// or a lower quality version with 1/10th the vertexes of the ideal plane (when true).
    /// 
    /// I'm going to leave the internal mechanics of this method uncommented for now. So many additions are expected for this
    /// class that it seems a tinge redundant to add documentation that will inevitably be modified within the next month.
    /// </summary>
    void CreateScanningPlane()
    {
        defaultScanningPlaneMaterial = Resources.Load("Ultrasound Probe Scanning Plane Material") as Material;
        renderedScanningPlaneMaterial = new Material(Shader.Find("Unlit/Texture"));
        renderedScanningPlaneMaterial.mainTexture = ultrasoundTexture;
        if (angle == 0)
        {
            scanningPlane = new GameObject("Scanning Plane");
            MeshCollider meshColliders = scanningPlane.AddComponent<MeshCollider>();
            MeshRenderer meshRenderers = scanningPlane.AddComponent<MeshRenderer>();
            MeshFilter meshFilters = scanningPlane.AddComponent<MeshFilter>();
            scanningPlane.transform.position = probeCenterPoint.transform.position;
            scanningPlane.transform.rotation = probeCenterPoint.transform.rotation;
            scanningPlane.transform.parent = ultrasoundProbe.transform;
            Mesh meshs = new Mesh();
            Vector3[] verticess = new Vector3[8];
            verticess[0] = points[2, 0];
            verticess[1] = points[2, points.GetLength(1) - 1];
            verticess[2] = points[3, points.GetLength(1) - 1];
            verticess[3] = points[3, 0];
            for (int x = 0; x < verticess.Length / 2; x++)
            {
                Vector3 negativeVector = verticess[x];
                negativeVector.z = -beamThickness / 2;
                Vector3 positiveZVector = negativeVector;
                positiveZVector.z = -positiveZVector.z;
                verticess[x] = positiveZVector;
                verticess[x + verticess.Length / 2] = negativeVector;
            }
            int[] triangless = new int[] { 0, 2, 3, 0, 1, 2, 4, 6, 5, 4, 7, 6, 0, 5, 1, 0, 4, 5, 1, 6, 2, 1, 5, 6, 2, 6, 3, 6, 7, 3, 0, 7, 4, 0, 3, 7 };
            Vector2[] UVs = { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
            meshs.vertices = verticess;
            meshs.triangles = triangless;
            meshs.uv = UVs;
            meshs.RecalculateNormals();
            meshFilters.mesh = meshs;
            if (!renderUltrasoundOntoScanningPlane)
                meshRenderers.material = defaultScanningPlaneMaterial;
            else
                meshRenderers.material = renderedScanningPlaneMaterial;

            meshColliders.sharedMesh = meshs;
            return;
        }
        scanningPlane = new GameObject("Scanning Plane");
        MeshCollider meshCollider = scanningPlane.AddComponent<MeshCollider>();
        MeshRenderer meshRenderer = scanningPlane.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = scanningPlane.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        Vector3[] vertices;
        if (renderSimpleScanningPlane && points.GetLength(1) > 10)
            vertices = new Vector3[(3 + 10) * 2];
        else
            vertices = new Vector3[(3 + points.GetLength(1)) * 2];
        vertices[0] = Vector3.zero;
        vertices[1] = points[2, 0];
        vertices[vertices.Length / 2 - 1] = points[2, points.GetLength(1) - 1];
        if (renderSimpleScanningPlane && points.GetLength(1) > 10)
        {
            for (int x = 0; x < 10; x++)
            {
                float y = x;
                y /= 9f;
                vertices[x + 2] = points[3, (int)((points.GetLength(1) - 1) * y)];
            }
        }
        else
        {
            for (int x = 0; x < vertices.Length / 2 - 3; x++)
            {
                vertices[x + 2] = points[3, x];
            }
        }
        for (int x = 0; x < vertices.Length / 2; x++)
        {
            Vector3 negativeVector = vertices[x];
            negativeVector.z = -beamThickness / 2;
            Vector3 positiveZVector = negativeVector;
            positiveZVector.z = -positiveZVector.z;
            vertices[x] = positiveZVector;
            vertices[x + vertices.Length / 2] = negativeVector;
        }
        Vector2[] UV = new Vector2[vertices.Length];
        UV[0] = new Vector2(.5f, 0);
        UV[1] = new Vector2(points[0, 1].x / resolution, points[0, 1].y / resolution);
        UV[UV.Length / 2 - 1] = new Vector2(points[0, points.GetLength(1) - 1].x / resolution, points[0, points.GetLength(1) - 1].y / resolution);
        UV[UV.Length / 2] = UV[0];
        UV[UV.Length / 2 + 1] = UV[1];
        UV[UV.Length - 1] = UV[UV.Length / 2 - 1];
        if (!renderSimpleScanningPlane)
        {
            for (int x = 2; x < UV.Length / 2 - 1; x++)
            {
                UV[x] = new Vector2(points[1, x - 2].x / resolution, points[1, x - 2].y / resolution);
                UV[UV.Length / 2 + x] = UV[x];
            }
        }
        else
        {
            for (int x = 2; x < UV.Length / 2 - 1; x++)
            {
                float y = x - 2;
                y /= 9f;
                UV[x] = new Vector2(points[1, (int)((points.GetLength(1) - 1) * y)].x / resolution, points[1, (int)((points.GetLength(1) - 1) * y)].y / resolution);
                UV[UV.Length / 2 + x] = UV[x];
            }
        }
        List<int> triangleList = new List<int>();
        for (int x = 1; x < vertices.Length / 2 - 1; x++)
        {
            triangleList.Add(0);
            triangleList.Add((x + 1));
            triangleList.Add(x);
            triangleList.Add(vertices.Length / 2);
            triangleList.Add(x + vertices.Length / 2);
            triangleList.Add(vertices.Length / 2 + x + 1);
        }
        for (int x = 0; x < vertices.Length / 2 - 1; x++)
        {
            triangleList.Add(x);
            triangleList.Add(x + 1 + vertices.Length / 2);
            triangleList.Add(x + vertices.Length / 2);
            triangleList.Add(x);
            triangleList.Add(x + 1);
            triangleList.Add(x + vertices.Length / 2 + 1);
        }
        triangleList.Add(vertices.Length / 2 - 1);
        triangleList.Add(vertices.Length / 2);
        triangleList.Add(vertices.Length - 1);
        triangleList.Add(vertices.Length / 2 - 1);
        triangleList.Add(0);
        triangleList.Add(vertices.Length / 2);
        int[] triangles = new int[triangleList.Count];
        for (int x = 0; x < triangleList.Count; x++)
        {
            triangles[x] = triangleList[x];
        }
        mesh.vertices = vertices;
        mesh.uv = UV;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
        if (!renderUltrasoundOntoScanningPlane)
            meshRenderer.material = defaultScanningPlaneMaterial;
        else
            meshRenderer.material = renderedScanningPlaneMaterial;
        meshCollider.sharedMesh = mesh;
        scanningPlane.transform.position = probeCenterPoint.transform.position;
        scanningPlane.transform.rotation = probeCenterPoint.transform.rotation;
        scanningPlane.transform.parent = ultrasoundProbe.transform;
    }

    /// <summary>
    /// The attenuation overlay is integral in creating a drop-off in the quality of the ultrasound image with increased
    /// depth. This method creates the field used for overlaying the attenuation depth.
    /// 
    /// I'm going to leave the internal mechanics of this method uncommented for now. So many additions are expected for this
    /// class that it seems a tinge redundant to add documentation that will inevitably be modified within the next month.
    /// </summary>
    void SetAttenuationDepths()
    {
        for (int x = 0; x < depths.GetLength(0); x++)
        {
            for (int y = 0; y < depths.GetLength(1); y++)
            {
                depths[x, y] = 1;
                renderedArea[x, y] = false;
                attenuationOverlayTexture.SetPixel(x, y, new Color(0, 0, 0, 0));
                edgeOverlayTexture.SetPixel(x, y, edgeColor);
            }
        }
        for (int line = 0; line < pixelAnalyses.Length; line++)
        {
            List<Vector3> analysis = pixelAnalyses[line];
            for (int x = 0; x < analysis.Count - 1; x++)
            {
                Vector3 a = analysis[x];
                Vector3 b = analysis[x + 1];
                float dy = b.z - a.z;
                float ddist = b.x - a.x;
                Color c;
                for (float y = a.z; y < b.z; y++)
                {
                    float d = (a.x + ((y - a.z) / dy) * ddist);
                    depths[(int)a.y, Mathf.RoundToInt(y)] = d;
                    c = attenuationColor;
                    c.a = d;
                    attenuationOverlayTexture.SetPixel((int)a.y, Mathf.RoundToInt(y), c);
                    edgeOverlayTexture.SetPixel((int)a.y, Mathf.RoundToInt(y), Color.clear);
                    renderedArea[(int)a.y, Mathf.RoundToInt(y)] = true;
                }
                depths[(int)a.y, Mathf.RoundToInt(b.z)] = b.x;
                c = attenuationColor;
                c.a = b.x;
                attenuationOverlayTexture.SetPixel((int)a.y, Mathf.RoundToInt(b.z), c);
                edgeOverlayTexture.SetPixel((int)a.y, Mathf.RoundToInt(b.z), Color.clear);
                renderedArea[(int)a.y, Mathf.RoundToInt(b.z)] = true;
            }
        }
        attenuationOverlayTexture.Apply();
        attenuationOverlay.GetComponent<MeshRenderer>().material.mainTexture = attenuationOverlayTexture;
        edgeOverlayTexture.Apply();
        edgeOverlay.GetComponent<MeshRenderer>().material.mainTexture = edgeOverlayTexture;
    }

    /// <summary>
    /// Toggles the screen dimensions. Currently there are two default sizes, Large and Small.
    /// This functionality is expandable for futher sizes, or eventually to custom sizes.
    /// </summary>
    public void ToggleScreenDimensions()
    {

        RectTransform rect;
        RectTransform UIRect = ultrasoundUICanvasObject.GetComponent<RectTransform>();
        if (ultrasoundScreenSize == SCREEN_DIMENSIONS.SMALL)
            rect = smallUltrasoundUICanvasObject.GetComponent<RectTransform>();
        else
            rect = largeUltrasoundUICanvasObject.GetComponent<RectTransform>();

        UIRect.anchoredPosition = rect.anchoredPosition;
        UIRect.anchoredPosition3D = rect.anchoredPosition3D;
        // UIRect.anchorMax = rect.anchorMax; // Dave Lizdas removed this 12/13/21
        // UIRect.anchorMin = rect.anchorMin; // Dave Lizdas removed this 12/13/21
        // UIRect.sizeDelta = rect.sizeDelta; // Dave Lizdas removed this 12/13/21
        // UIRect.offsetMin = rect.offsetMin; // Dave Lizdas removed this 12/13/21
        // UIRect.offsetMax = rect.offsetMax; // Dave Lizdas removed this 12/13/21
        UIRect.localScale = rect.localScale; // Dave Lizdas addition 12/13/21. Reason: I couldn't make the US dot work with both large and small US screens..
                                             // I replaced the anchors and offsets from above with a simple local scale. 
                                             // This allows the US dot to render in the small US screen exactly like it does in the large US Screen.
                                             // It also fixes a similar problem with the US centerline.  So, I think this is a better way to do it. 
    }

    /// <summary>
    /// Toggles the screen orientation between different presets. Four 90 degree
    /// rotations are given.
    /// </summary>
    public void ToggleScreenOrientation()
    {
        if (ultrasoundScreenOrientation == SCREEN_ORIENTATION.DEFAULT_ORIENTATION)
            RotateCamera(0);
        else if (ultrasoundScreenOrientation == SCREEN_ORIENTATION.ROTATED_90_CLOCKWISE)
            RotateCamera(90);
        else if (ultrasoundScreenOrientation == SCREEN_ORIENTATION.ROTATED_180_CLOCKWISE)
            RotateCamera(180);
        else if (ultrasoundScreenOrientation == SCREEN_ORIENTATION.ROTATED_270_CLOCKWISE)
            RotateCamera(270);
    }

    /// <summary>
    /// Used by ToggleScreenOrientation to actually rotate the camera looking at the texture canvas
    /// to attain different orientations.
    /// </summary>
    /// <param name="rotationAngle">Angle by which to rotate the camera.</param>
    void RotateCamera(float rotationAngle)
    {
        ultrasoundTextureCamera.transform.localRotation = Quaternion.Euler(0, 0, rotationAngle);
    }

    /// <summary>
    /// Toggles the screen inversion between different presets. Four states 
    /// are given.
    /// </summary>
    public void ToggleScreenInversion()
    {
        InvertHorizontal(((uint)ultrasoundScreenInversion & 1) == 1 ? true : false);
        InvertVertical(((uint)ultrasoundScreenInversion & 2) == 2 ? true : false);
        /// When inverting the screen, it's imperative to ensure the ultrasound zoom is updated to reflect
        /// the texture inversion. Otherwise, the camera movement may not reflect the updated inversion.
        ZoomUltrasoundScreen();
    }

    void InvertVertical(bool invert)
    {
        Vector3 localScale = renderedUltrasoundTexture.transform.localScale;
        localScale.z = invert ? -1 : 1;
        renderedUltrasoundTexture.transform.localScale = localScale;
        edgeOverlay.transform.localScale = localScale;
        attenuationOverlay.transform.localScale = localScale;
    }

    void InvertHorizontal(bool invert)
    {
        Vector3 localScale = renderedUltrasoundTexture.transform.localScale;
        localScale.x = invert ? -1 : 1;
        renderedUltrasoundTexture.transform.localScale = localScale;
        edgeOverlay.transform.localScale = localScale;
        attenuationOverlay.transform.localScale = localScale;
    }

    /// <summary>
    /// The slope formula m = (y2-y1)/(x2-x1)
    /// </summary>
    /// <param name="point1">Point 1 (x1,y1)</param>
    /// <param name="point2">Point 1 (x2,y2)</param>
    /// <returns>Slope (m)</returns>
    float Slope(Vector2 point1, Vector2 point2)
    {
        return (point2.y - point1.y) / (point2.x - point1.x);
    }

    /// <summary>
    /// This method takes in a screen position (anywhere on the screen), converts it to a pixel position on the US screen texture, and returns the
    /// corresponding position in world space of the screen location.
    /// 
    /// If the screen is frozen, the method will convert the location on the screen to the location in world units of where that position given is
    /// if the screen were not frozen. It does not know where the screen was at the time it was frozen.
    /// 
    /// If the location is not on the US Screen, returns Vector3.zero.
    /// 
    /// If the location is on the screen, but not on the rendered US insonating plane, returns Vector3.zero
    /// </summary>
    /// <param name="screenPosition">Vector2 screen position. Must be in terms of entire screen</param>
    /// <returns>The Vector3 position of the point on the US Screen in world global units. Vector3.zero if screen not hit.</returns>
    public Vector3 ScreenPointToWorldCoordinate(Vector2 screenPosition)
    {
        if (!OnUltrasoundScreen(screenPosition))
            throw new System.ArgumentOutOfRangeException("Screen point not on ultrasound screen.");
        Vector4 texturePoint = USPanelXYToUSScreenTextureXY(MainScreenXYToUSPanelXY(screenPosition));
        texturePoint.z = 1;
        Vector4 ultrasoundWorldPoint4 = (transformationMatrixTToU2 * (transformationMatrixTToU1 * texturePoint));
        Vector3 ultrasoundWorldPoint = ultrasoundWorldPoint4;
        ultrasoundWorldPoint = probeCenterPoint.transform.TransformPoint(ultrasoundWorldPoint);
        return ultrasoundWorldPoint;
    }

    /// <summary>
    /// Given a position on the screen, this method determines whether or not the ultrasound screen is being touched.
    /// The option is given to determine whether or not to determine whether it is on the ultrasound screen, or on the
    /// rendered area of the ultrasound screen specifically. The boolean specificallyOnRenderedUltrasoundArea is true
    /// by default, but can be called with "false" to determine whether a screen position is on the entirity of the 
    /// ultrasound screen and not exclusively on the renderered area. 
    /// </summary>
    /// <param name="screenPosition">The 2D location on the screen.</param>
    /// <returns>True if on the panel, false otherwise</returns>
    public bool OnUltrasoundScreen(Vector2 screenPosition, bool specificallyOnRenderedUltrasoundArea = true)
    {
        float sPX = screenPosition.x;
        float sPY = screenPosition.y;
        float width = ultrasoundUICanvasObject.GetComponent<RectTransform>().rect.width * ultrasoundUICanvasObject.GetComponent<RectTransform>().localScale.x;
        float height = ultrasoundUICanvasObject.GetComponent<RectTransform>().rect.height * ultrasoundUICanvasObject.GetComponent<RectTransform>().localScale.y;
        float xMin = ultrasoundUICanvasObject.transform.position.x - width / 2;
        float xMax = ultrasoundUICanvasObject.transform.position.x + width / 2;
        float yMin = ultrasoundUICanvasObject.transform.position.y - height / 2;
        float yMax = ultrasoundUICanvasObject.transform.position.y + height / 2;
        //Debug.Log(sPX + " " + xMin + " " + xMax + " " + sPY + " " + yMin + " " + yMax);
        return (sPX <= xMax && sPX >= xMin && sPY <= yMax && sPY >= yMin) && (!specificallyOnRenderedUltrasoundArea || OnScreenAndOnRenderedPortion(screenPosition));
    }

    /// <summary>
    /// Returns whether or not a pixel on the screen is on the rendered area of the ultrasound screen. Should one be
    /// used as a helper method for OnUltrasoundScreen so it can be verified that the point on the screen is on the US Screen
    /// before calling this method. If this is not done, it will possibly throw an index out of bounds exception when checking
    /// the renderedArea array of booleans.
    /// </summary>
    /// <param name="screenPosition">The position on the main screen to check.</param>
    /// <returns></returns>
    private bool OnScreenAndOnRenderedPortion(Vector2 screenPosition)
    {
        Vector2 texturePosition = USPanelXYToUSScreenTextureXY(MainScreenXYToUSPanelXY(screenPosition));
        return renderedArea[Mathf.RoundToInt(texturePosition.x), Mathf.RoundToInt(texturePosition.y)];
    }

    /// <summary>
    /// This method returns the object ID of the object that is on the ultrasound screen at a given screen position.
    /// The screen position must be given in terms of the whole screen, as a Vector2 position where the x and y
    /// components correspond to the pixel width and height of the location. 0 is returned if the position given is
    /// not on the ultrasound screen, or if the position on the screen does not have a rendered object.
    /// </summary>
    /// <param name="screenPosition">Vector2 location of a pixel on the screen</param>
    /// <returns>Object ID of the object on the US screen at the given position, zero otherwise.</returns>
    /*public int ObjectOnUSScreen(Vector2 screenPosition)
    {
        if (!OnUltrasoundScreen(screenPosition))
            return 0;
        Vector2 xyPos = MainScreenXYToUSPanelXY(screenPosition);
        Vector2 newXYPos = USPanelXYToUSScreenTextureXY(xyPos);
        return imageObjectID[(int)newXYPos.x, (int)newXYPos.y];
    }*/

    /// <summary>
    /// This method takes a given point on the main screen and transposes it onto the US Panel.
    /// If a given point on the whole screen is at the bottom left corner of the US Panel, this
    /// method will recognize it as being at (0,0) on the US Panel, regardless of the coordinate
    /// given on the main screen.
    /// </summary>
    /// <param name="screenPosition">The location on the main screen as a Vector2.</param>
    /// <returns>Corresponding Vector2 position on the US Panel.</returns>
    private Vector2 MainScreenXYToUSPanelXY(Vector2 screenPosition)
    {
        float sPX = screenPosition.x;
        float sPY = screenPosition.y;
        float width = ultrasoundUICanvasObject.GetComponent<RectTransform>().rect.width * ultrasoundUICanvasObject.GetComponent<RectTransform>().localScale.x;
        float xMin = ultrasoundUICanvasObject.transform.position.x - width / 2;
        float height = ultrasoundUICanvasObject.GetComponent<RectTransform>().rect.height * ultrasoundUICanvasObject.GetComponent<RectTransform>().localScale.y;
        float yMin = ultrasoundUICanvasObject.transform.position.y - height / 2;
        int xPix = (int)((sPX - xMin) / width * (resolution - 1));
        int yPix = (int)((sPY - yMin) / height * (resolution - 1));
        //Debug.Log("X: " + xMin + " " + sPX + " " + width + " " + xPix + " ");
        //Debug.Log("Y: " + yMin + " " + sPY + " " + height + " " + yPix + " ");
        //if (alignmentDotRightSide)
        //    return new Vector2(xPix, yPix);
        return new Vector2((resolution - 1) - xPix, yPix);
    }

    /// <summary>
    /// This private method converts the pixel location on the US Panel to a pixel location on the US texture.
    /// The US texture is flipped and does not follow traditional XY coordinates where positive x is right and
    /// positive y is up.
    /// </summary>
    /// <param name="xyPos">Vector2 location on US Panel</param>
    /// <returns>Vector2 location of corresponding pixel on texture.</returns>
    private Vector2 USPanelXYToUSScreenTextureXY(Vector2 xyPos)
    {
        Vector2 newXYPos = new Vector2(xyPos.x, resolution - xyPos.y - 1);

        //If Horizontally Inverted
        if (((uint)ultrasoundScreenInversion & 1) == 1)
            newXYPos.x = resolution - 1 - newXYPos.x;

        //If Vertically Inverted
        if (((uint)ultrasoundScreenInversion & 2) == 2)
            newXYPos.y = resolution - 1 - newXYPos.y;

        //If Rotated 90 Clockwise
        if (ultrasoundScreenOrientation == SCREEN_ORIENTATION.ROTATED_90_CLOCKWISE)
            newXYPos = new Vector2(resolution - 1 - newXYPos.y, newXYPos.x);

        //If Rotated 180
        else if (ultrasoundScreenOrientation == SCREEN_ORIENTATION.ROTATED_180_CLOCKWISE)
            newXYPos = new Vector2(resolution - 1 - newXYPos.x, resolution - 1 - newXYPos.y);

        //If Rotated 270 Clockwise
        else if (ultrasoundScreenOrientation == SCREEN_ORIENTATION.ROTATED_270_CLOCKWISE)
            newXYPos = new Vector2(newXYPos.y, resolution - 1 - newXYPos.x);

        //If Zoomed
        newXYPos = new Vector2(1f / (2 * ultrasoundZoomFactor) * ((resolution - 1f) * (ultrasoundZoomFactor - 1) + 2 * newXYPos.x), newXYPos.y / ultrasoundZoomFactor);

        return newXYPos;
    }


    void ZoomUltrasoundScreen()
    {
        Vector3 cameraLocalPosition = ultrasoundTextureCamera.transform.localPosition;
        float yPos = 5 - 5 / ultrasoundZoomFactor;
        cameraLocalPosition.y = ((uint)ultrasoundScreenInversion & 2) == 2 ? -yPos : yPos;
        ultrasoundTextureCamera.transform.localPosition = cameraLocalPosition;
        ultrasoundTextureCamera.GetComponent<Camera>().orthographicSize = 5 / ultrasoundZoomFactor;
    }
}