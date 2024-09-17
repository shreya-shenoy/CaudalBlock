using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dynamic_Artery_Interaction : MonoBehaviour
{
    Dynamic_Artery OriginalDV;
    public GameObject DeformableVein { get; private set; }
    Dynamic_Artery deformableDV = null;
    [SerializeField]
    GameObject needleTip, needleHub;
    [SerializeField]
    float needleRadius_mm = 0.4f;
    int frame = 0;
    LayerMask veinBaseLayer = 0, veinLayer = 0;
    bool inBaseVein = false;
    bool inVein = false;
    bool deformationInProgress = false;
    bool procedureInProgress = false;
    float effectDistance = 5;//World Units (mm)
    float percentRadiusForMotion = 0.5f;//% as fraction [0,1]. Minimum distance from central axis required for motion to occur.
    float[] distances;
    float[] radii;
    GameObject[] deformationNodes;
    GameObject movingNode;
    int movingNodeIndex;
    Vector3[] axialDirections;
    //Node[] deformationNodess;
    struct Node
    {
        Vector3 position;
        float distance;
        float radius;
        Vector3 axialDirection;
        public Node(Vector3 Position, float Distance, float Radius, Vector3 AxialDirection)
        {
            position = Position;
            distance = Distance;
            radius = Radius;
            axialDirection = AxialDirection;
        }
        public Node(Vector3 Position)
        {
            position = Position;
            distance = 1;
            radius = 1;
            axialDirection = Vector3.zero;
        }
    }

    private void Start()
    {
        veinBaseLayer = 1 << LayerMask.NameToLayer("Vein Base");
        veinLayer = 1 << LayerMask.NameToLayer("Vein");
        OriginalDV = GetComponent<Dynamic_Artery>();
        DeformableVein = new GameObject("Deformable Vein " + name.Substring(name.Length - 1));
        DeformableVein.layer = LayerMask.NameToLayer("Vein");
        //if(IV_Manager.ARM_ORIENTATION == IV_Manager.ARM_ORIENTATION.NOT_SELECTED)

        if (IV_Manager.currentOrientation == (IV_Manager.ARM_ORIENTATION)1)
        {
            DeformableVein.tag = "handUp";
        }
        if (IV_Manager.currentOrientation == (IV_Manager.ARM_ORIENTATION)2)
        {
            DeformableVein.tag = "handDown";
        }
        DeformableVein.AddComponent<SMMARTS_SDK.Ultrasound.UltrasoundMaterial>();

        //DeformableVein.transform.parent = OriginalDV.transform.parent;
       // MeshRenderer mr = DeformableVein.AddComponent<MeshRenderer>();
       // mr.material = Resources.Load("Veinous") as Material;//new Material(Shader.Find("VR/SpatialMapping/Wireframe"));
    }
    private void Update()
    {
        frame++;
        if (frame < 3)
            return;
        if (frame == 3)
        {
            CloneDV();
            return;
        }
        RaycastHit hit = new RaycastHit();
        inBaseVein = InVein(veinBaseLayer, false, out hit);
        inBaseVein = inBaseVein && (hit.transform.GetInstanceID() == transform.GetInstanceID());
        if (inBaseVein)
            Debug.Log("Hit Name: " + hit.transform.name + " " + DeformableVein.name);
        inVein = InVein(veinLayer, true, out hit);
        inVein = inVein && (hit.transform.GetInstanceID() == transform.GetInstanceID());
        if (deformationInProgress && !inBaseVein)
        {
            //Snap Back
            AssignOriginal();
            deformationInProgress = false;
            return;
        }
        else if (deformationInProgress && inBaseVein)
        {
            //Update Deformation Vein
            UpdateDeformedVein();
            return;
        }
        else if (procedureInProgress && inBaseVein)
        {
            //In vein doing procedure
            return;
        }
        else if (procedureInProgress && !inBaseVein)
        {
            //was in vein, not in vein anymore
            procedureInProgress = false;
            return;
        }
        else if (inBaseVein)
        {
            deformationInProgress = IsIndirectContact(veinBaseLayer);
            procedureInProgress = !deformationInProgress;

            //Establish Deformation Vein
            if (deformationInProgress)
                EstablishDeformation();
            else
                return;
        }
    }
    void CloneDV()
    {
        deformableDV = DeformableVein.AddComponent<Dynamic_Artery>();
        AssignOriginal();
    }
    void AssignOriginal()
    {
        ClearDeformationNodes();
        deformableDV.Segments = new Dynamic_Artery.VesselSegment[OriginalDV.Segments.Length];
        deformationNodes = new GameObject[OriginalDV.Segments.Length];
        for (int x = 0; x < deformableDV.Segments.Length; x++)
        {
            deformableDV.Segments[x] = new Dynamic_Artery.VesselSegment(OriginalDV.Segments[x]);
            GameObject GO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GO.transform.position = deformableDV.Segments[x].CenterPoint;
            GO.transform.parent = DeformableVein.transform;
            GO.transform.GetComponent<MeshRenderer>().enabled = false;
            deformationNodes[x] = GO;
        }
    }
    bool InVein(LayerMask layer, bool noBackwallAllowed, out RaycastHit hit)
    {
        if (noBackwallAllowed)
            return Physics.Raycast(needleHub.transform.position,
                needleTip.transform.position - needleHub.transform.position, out hit,
                Vector3.Distance(needleTip.transform.position, needleHub.transform.position),
                layer) && !Physics.Raycast(needleTip.transform.position,
                needleHub.transform.position - needleTip.transform.position,
                Vector3.Distance(needleHub.transform.position, needleTip.transform.position),
                layer);
        else
            return Physics.Raycast(needleHub.transform.position,
                needleTip.transform.position - needleHub.transform.position, out hit,
                Vector3.Distance(needleTip.transform.position, needleHub.transform.position),
                layer);
    }
    bool IsIndirectContact(LayerMask layer)
    {
        distances = new float[OriginalDV.Segments.Length];
        radii = new float[OriginalDV.Segments.Length];
        distances[0] = 0;
        axialDirections = new Vector3[OriginalDV.Segments.Length];
        radii[0] = OriginalDV.Segments[0].Radius;
        for (int x = 1; x < OriginalDV.Segments.Length; x++)
        {
            distances[x] = distances[x - 1] + Vector3.Distance(OriginalDV.Segments[x].CenterPoint, OriginalDV.Segments[x - 1].CenterPoint);
            axialDirections[x - 1] = OriginalDV.Segments[x].CenterPoint - OriginalDV.Segments[x - 1].CenterPoint;
            axialDirections[x] = axialDirections[x - 1];
            radii[x] = OriginalDV.Segments[x].Radius;
        }
        Vector3 direction = needleTip.transform.position - needleHub.transform.position;
        RaycastHit hit = new RaycastHit();
        Physics.Raycast(needleHub.transform.position,
                direction, out hit,
                Vector3.Distance(needleTip.transform.position, needleHub.transform.position),
                layer);
        Vector3 hitPos = hit.point;
        int indexLowerBound = LowerBoundNode(hitPos);
        bool indirectContact = RadialDistancePercent(indexLowerBound, hitPos, direction) > percentRadiusForMotion;
        return indirectContact;
    }
    int LowerBoundNode(Vector3 hitPos)
    {
        int returnIndex = -1;
        Vector3 direction1, direction2;
        for (int x = 0; x < axialDirections.Length - 1; x++)
        {
            direction1 = hitPos - OriginalDV.Segments[x].CenterPoint;
            direction2 = hitPos - OriginalDV.Segments[x + 1].CenterPoint;
            Debug.Log(Vector3.Dot(direction1, axialDirections[x]) + " " + Vector3.Dot(direction2, -axialDirections[x + 1]));
            if (Vector3.Dot(direction1, axialDirections[x]) >= 0 && Vector3.Dot(direction2, -axialDirections[x + 1]) >= 0)
            {
                returnIndex = x;
                break;
            }
        }


        return returnIndex;
    }
    float RadialDistancePercent(int lowerIndex, Vector3 hitPos, Vector3 hitDir)
    {
        hitDir *= -1;
        Debug.Log("LI: " + lowerIndex + " " + OriginalDV.Segments.Length);
        Vector3 axialDirection = OriginalDV.Segments[lowerIndex + 1].CenterPoint - OriginalDV.Segments[lowerIndex].CenterPoint;
        axialDirection.Normalize();
        Vector3 needleProjection = Vector3.ProjectOnPlane(hitDir, axialDirection);
        needleProjection.Normalize();
        Vector3 cross = Vector3.Cross(axialDirection, needleProjection);
        cross.Normalize();
        Vector3 direction = Vector3.ProjectOnPlane(hitPos - OriginalDV.Segments[lowerIndex].CenterPoint, axialDirection);
        direction.Normalize();

        float percent = Mathf.Cos(Vector3.Angle(cross, direction) * Mathf.Deg2Rad);
        float percentFromCenter = Mathf.Abs(percent);

        return percentFromCenter;
    }
    void EstablishDeformation()
    {
        ClearDeformationNodes();
        Vector3 direction = needleTip.transform.position - needleHub.transform.position;
        RaycastHit hit = new RaycastHit();
        Physics.Raycast(needleHub.transform.position,
                direction, out hit,
                Vector3.Distance(needleTip.transform.position, needleHub.transform.position),
                veinBaseLayer);
        int lowerBound = LowerBoundNode(hit.point);
        float axialDistanceFromNode = AxialDistanceFromNode(lowerBound, hit.point);
        float cumulativePointDistance = axialDistanceFromNode + distances[lowerBound];
        CreateDeformationNodes(cumulativePointDistance, lowerBound);
        UpdateDeformedVein();
    }
    void ClearDeformationNodes(bool saveMovingNode = false)
    {
        if (deformationNodes != null)
            for (int x = 0; x < deformationNodes.Length; x++)
            {
                if (x != movingNodeIndex || !saveMovingNode)
                    Destroy(deformationNodes[x]);
            }
    }
    float AxialDistanceFromNode(int index, Vector3 hitPos)
    {
        Vector3 dir = OriginalDV.Segments[index + 1].CenterPoint - OriginalDV.Segments[index].CenterPoint;
        Vector3 d1 = hitPos - OriginalDV.Segments[index].CenterPoint;
        float theta = Vector3.Angle(dir, d1);
        return Vector3.Distance(hitPos, OriginalDV.Segments[index].CenterPoint) * Mathf.Cos(theta * Mathf.Deg2Rad);
    }
    void CreateDeformationNodes(float cumulativeDistance, int lowerBound, bool maintainMovingNode = false)
    {
        float lowerBoundDistance = cumulativeDistance - effectDistance;
        float upperBoundDistance = cumulativeDistance + effectDistance;
        if (lowerBoundDistance < 0)
            lowerBoundDistance = float.Epsilon * 2;
        if (upperBoundDistance > distances[distances.Length - 1])
            upperBoundDistance = distances[distances.Length - 1] - float.Epsilon * 2;
        int totalNodes = OriginalDV.Segments.Length + 3;
        bool ignoreLowerNode = false;
        bool ignoreUpperNode = false;
        if (lowerBoundDistance < distances[lowerBound])
        { totalNodes--; ignoreLowerNode = true; }
        if (upperBoundDistance > distances[lowerBound + 1])
        { totalNodes--; ignoreUpperNode = true; }
        deformationNodes = new GameObject[totalNodes];
        int lowerNode = ignoreLowerNode ? lowerBound - 1 : lowerBound;
        int upperNode = ignoreUpperNode ? lowerBound + 2 : lowerBound + 1;

        //Debug.Log(lowerNode + " " + lowerBound + " " + upperNode);
        //Debug.Log(lowerBoundDistance + " " + cumulativeDistance + " " + upperBoundDistance);
        //Debug.Log(ignoreLowerNode + " " + ignoreUpperNode);


        int constructedNode = -1;
        GameObject GO = null;
        float radius = 0;
        for (int x = 0; x <= lowerNode; x++)
        {
            constructedNode = x;
            GO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GO.transform.position = OriginalDV.Segments[x].CenterPoint;
            GO.transform.parent = DeformableVein.transform;
            GO.transform.GetComponent<MeshRenderer>().enabled = false;
            deformationNodes[constructedNode] = GO;
            radius = OriginalDV.Segments[x].Radius;
            GO.transform.localScale = new Vector3(radius, radius, radius);
        }
        constructedNode++;
        GO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        GO.transform.GetComponent<MeshRenderer>().enabled = false;
        Vector3 direction = OriginalDV.Segments[lowerNode + 1].CenterPoint - OriginalDV.Segments[lowerNode].CenterPoint;
        direction.Normalize();
        float distance = lowerBoundDistance - distances[lowerNode];
        GO.transform.position = OriginalDV.Segments[lowerNode].CenterPoint + direction * distance;
        GO.transform.parent = DeformableVein.transform;
        deformationNodes[constructedNode] = GO;

        float distancePercent = distance / (distances[lowerNode + 1] - distances[lowerNode]);
        radius = OriginalDV.Segments[lowerNode + 1].Radius * distancePercent + (1 - distancePercent) * OriginalDV.Segments[lowerNode].Radius;
        GO.transform.localScale = new Vector3(radius, radius, radius);


        constructedNode++;
        if (maintainMovingNode)
        {
            deformationNodes[constructedNode] = movingNode;
            movingNodeIndex = constructedNode;


            distance = cumulativeDistance - distances[lowerBound];
            distancePercent = distance / (distances[lowerBound + 1] - distances[lowerBound]);
            radius = OriginalDV.Segments[lowerBound + 1].Radius * distancePercent + (1 - distancePercent) * OriginalDV.Segments[lowerBound].Radius;
            movingNode.transform.localScale = new Vector3(radius, radius, radius);
            ///ADD RADIUS MOD
        }
        else
        {
            GO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GO.transform.GetComponent<MeshRenderer>().enabled = false;
            direction = OriginalDV.Segments[lowerBound + 1].CenterPoint - OriginalDV.Segments[lowerBound].CenterPoint;
            direction.Normalize();
            distance = cumulativeDistance - distances[lowerBound];
            GO.transform.position = OriginalDV.Segments[lowerBound].CenterPoint + direction * distance;
            GO.transform.parent = DeformableVein.transform;
            deformationNodes[constructedNode] = GO;
            MoveNodePerpendicularDistance(GO, lowerBound);
            movingNode = GO;
            movingNodeIndex = constructedNode;


            distancePercent = distance / (distances[lowerBound + 1] - distances[lowerBound]);
            radius = OriginalDV.Segments[lowerBound + 1].Radius * distancePercent + (1 - distancePercent) * OriginalDV.Segments[lowerBound].Radius;
            movingNode.transform.localScale = new Vector3(radius, radius, radius);
        }

        constructedNode++;
        GO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        GO.transform.GetComponent<MeshRenderer>().enabled = false;
        direction = OriginalDV.Segments[upperNode].CenterPoint - OriginalDV.Segments[upperNode - 1].CenterPoint;
        direction.Normalize();
        distance = upperBoundDistance - distances[upperNode - 1];
        GO.transform.position = OriginalDV.Segments[upperNode - 1].CenterPoint + direction * distance;
        GO.transform.parent = DeformableVein.transform;
        deformationNodes[constructedNode] = GO;


        distancePercent = distance / (distances[upperNode] - distances[upperNode - 1]);
        radius = OriginalDV.Segments[upperNode].Radius * distancePercent + (1 - distancePercent) * OriginalDV.Segments[upperNode - 1].Radius;
        GO.transform.localScale = new Vector3(radius, radius, radius);

        for (int x = upperNode; x < OriginalDV.Segments.Length; x++)
        {
            constructedNode++;
            GO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GO.transform.GetComponent<MeshRenderer>().enabled = false;
            GO.transform.position = OriginalDV.Segments[x].CenterPoint;
            GO.transform.parent = DeformableVein.transform;
            deformationNodes[constructedNode] = GO;



            radius = OriginalDV.Segments[x].Radius;
            GO.transform.localScale = new Vector3(radius, radius, radius);
        }
    }
    void MoveNodePerpendicularDistance(GameObject node, int lowerBound)
    {
        float distanceToMove = 0;
        Vector3 direction = needleTip.transform.position - needleHub.transform.position;
        RaycastHit hit = new RaycastHit();
        Physics.Raycast(needleHub.transform.position,
                direction, out hit,
                Vector3.Distance(needleTip.transform.position, needleHub.transform.position),
                veinBaseLayer);
        Vector3 axialDirection = OriginalDV.Segments[lowerBound + 1].CenterPoint - OriginalDV.Segments[lowerBound].CenterPoint;
        Vector3 A = needleTip.transform.position - OriginalDV.Segments[lowerBound].CenterPoint;
        Vector3 B = hit.point - OriginalDV.Segments[lowerBound].CenterPoint;
        A = Vector3.ProjectOnPlane(A, axialDirection);
        B = Vector3.ProjectOnPlane(B, axialDirection);

        Vector3 needleProjection = Vector3.ProjectOnPlane(needleHub.transform.position - needleTip.transform.position, axialDirection);
        needleProjection.Normalize();

        if (Vector3.Dot(needleProjection, A) < 0)
        {
            float theta = Vector3.Angle(needleProjection, B);
            float R = B.magnitude;
            float d2 = Mathf.Sin(theta * Mathf.Deg2Rad) * R;
            distanceToMove = (R - d2) + needleRadius_mm;
            //Debug.Log(distanceToMove + " " + needleRadius_mm);
            Vector3 cross = Vector3.Cross(needleProjection, axialDirection);
            cross.Normalize();
            if (Vector3.Dot(B, cross) < 0)
                distanceToMove *= -1;
            node.transform.position = node.transform.position - distanceToMove * cross * 1.1f;//1.1 factor of safety
            node.transform.parent = needleTip.transform.parent;
        }
        else
        {
            float theta = Vector3.Angle(needleProjection, A);
            float dt = A.magnitude;
            float d1 = Mathf.Cos(theta * Mathf.Deg2Rad) * dt;
            float d2 = Mathf.Sin(theta * Mathf.Deg2Rad) * dt;
            float R = B.magnitude;
            distanceToMove = Mathf.Sqrt(R * R - d1 * d1) - d2;
            Vector3 cross = Vector3.Cross(needleProjection, axialDirection);
            cross.Normalize();
            if (Vector3.Dot(A, cross) < 0)
                distanceToMove *= -1;
            node.transform.position = node.transform.position - distanceToMove * cross * 1.1f;//1.1 factor of safety
            node.transform.parent = needleTip.transform.parent;
        }
    }
    void UpdateDeformedVein()
    {
        int lowerBound = LowerBoundNode(movingNode.transform.position);
        float distance = AxialDistanceFromNode(lowerBound, movingNode.transform.position);
        distance = distance + distances[lowerBound];
        ClearDeformationNodes(true);
        CreateDeformationNodes(distance, lowerBound, true);

        deformableDV.Segments = new Dynamic_Artery.VesselSegment[deformationNodes.Length];
        for (int x = 0; x < deformableDV.Segments.Length; x++)
        {
            deformableDV.Segments[x] = new Dynamic_Artery.VesselSegment(deformationNodes[x].transform.localScale.x,
                deformationNodes[x].transform.position, 1, transform);
        }
    }
}
