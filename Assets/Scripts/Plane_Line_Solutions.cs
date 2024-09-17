using UnityEngine;
using System.Collections;

public static class Plane_Line_Solutions
{
	public static Vector3 LinePlaneIntersection(Line line, Plane_ plane)
	{
		Vector3 intersectionPoint;
		if(Vector3.Angle(line.Direction,plane.Normal)==90f)
		{
			throw new System.Exception("LINE IS PARALLEL TO PLANE");
		}
		Vector3 normal = plane.Normal;
		float constant = plane.Constant;
		Vector3 direction = line.Direction;
		Vector3 point = line.Point;
		float t = (constant - normal.x * point.x - normal.y * point.y - normal.z * point.z) / (normal.x * direction.x + normal.y * direction.y + normal.z * direction.z);
		intersectionPoint = new Vector3(t * direction.x + point.x, t * direction.y + point.y, t * direction.z + point.z);
		return intersectionPoint;
	}
	public static float MinPointPlaneDistance(Vector3 point, Plane_ plane)
	{
		Vector3 normal = plane.Normal;
		float constant = plane.Constant;
		return Mathf.Abs(normal.x * point.x + normal.y * point.y + normal.z * point.z - constant) / Mathf.Sqrt(normal.x * normal.x + normal.y * normal.y + normal.z * normal.z);
	}
}
public struct Plane_
{
	Vector3 normal;
	Vector3 point;
	float constant;
	public Plane_(Vector3 Point1,Vector3 Point2, Vector3 Point3)
	{
		if (Vector3.Angle(Point2 - Point1, Point3 - Point1) == 0)
			throw new System.Exception("PLANE CANNOT BE MADE FROM 3 COLINEAR POINTS");
		normal = Vector3.Cross(Point2 - Point1, Point3 - Point1);
		point = Point1;
		constant = normal.x * point.x + normal.y * point.y + normal.z * point.z;
	}
	public Plane_(Vector3 Point1, Vector3 Normal)
	{
		normal = Normal;
		point = Point1;
		constant = normal.x * point.x + normal.y * point.y + normal.z * point.z;
	}
	public Vector3 Normal
	{
		get
		{
			return normal;
		}
	}
	public Vector3 Point
	{
		get
		{
			return point;
		}
	}
	public float Constant
	{
		get
		{
			return constant;
		}
	}
}
public struct Line
{
	Vector3 point;
	Vector3 direction;
	public Line(Vector3 Point1, Vector3 Point2)
	{
		if (Point1 == Point2)
			throw new System.Exception("CANNOT MAKE A LINE WITH POINT1 = POINT2");
		point = Point1;
		direction = Point2 - Point1;
	}
	public Line(Vector3 Point, Vector3 Direction, bool PointAndDirection)
	{
		if (Direction == Vector3.zero)
			throw new System.Exception("DIRECTION CANNOT BE ZERO");
		direction = Direction;
		point = Point;
	}
	public Vector3 Point
	{
		get
		{
			return point;
		}
	}
	public Vector3 Direction
	{
		get
		{
			return direction;
		}
	}
}