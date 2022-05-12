using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FieldOfView : MonoBehaviour {

	public float viewRadius;
	[Range(0,360)]
	public float viewAngle;

	public float verticalViewRadius;
	[Range(0,360)]
	public float verticalViewAngle;

	public LayerMask targetMask;
	public LayerMask obstacleMask;

	//[HideInInspector]
	public List<Transform> horizontallyVisibleTargets = new List<Transform>();
	//[HideInInspector]
	public List<Transform> verticallyVisibleTargets = new List<Transform>();

	public float meshResolution;
	public float horizontalOffsetResolution;
	public float verticalMeshResolution;
	public float verticalOffsetResolution;
	public int edgeResolveIterations;
	public float edgeDstThreshold;

	public MeshFilter[] viewMeshFilterArray = new MeshFilter[100];
	Mesh[] viewMeshArray = new Mesh[100];

	public MeshFilter[] verticalMeshFilterArray = new MeshFilter[100];
	Mesh[] verticalMeshArray = new Mesh[100];



	void Start() {

		for (int i = 0; i <= 99; i++) {
			viewMeshArray[i] = new Mesh ();																													//Initializing a Mesh for each DrawHorizontalFieldOfView function.
			viewMeshArray[i].name = "ViewMesh" + i;
			viewMeshFilterArray[i].mesh = viewMeshArray[i];


			verticalMeshArray[i] = new Mesh ();																											//Initializing a Mesh for each DrawVerticalFieldOfView function.
			verticalMeshArray[i].name = "View Mesh" + i;
			verticalMeshFilterArray[i].mesh = verticalMeshArray[i];
		}

		StartCoroutine ("FindHorizontallyVisibleTargetsWithDelay", .2f);
		StartCoroutine ("FindVerticallyVisibleTargetsWithDelay", .2f);
	}


	IEnumerator FindHorizontallyVisibleTargetsWithDelay(float delay) {
		while (true) {
			yield return new WaitForSeconds (delay);
			FindHorizontallyVisibleTargets ();
		}
	}

	IEnumerator FindVerticallyVisibleTargetsWithDelay(float delay) {
		while (true) {
			yield return new WaitForSeconds (delay);
			FindVerticallyVisibleTargets ();
		}
	}

	void LateUpdate() {

		int horizontalOffsetStepCount = Mathf.RoundToInt(verticalViewAngle * horizontalOffsetResolution);
		float horizontalOffsetStepAngleSize = verticalViewAngle / horizontalOffsetStepCount;
		for (int i = 0; i <= horizontalOffsetStepCount; i++) {																			//maybe add a +1 here. Check later.
			float horizontalOffsetAngle = - verticalViewAngle / 2 + horizontalOffsetStepAngleSize * i;
			DrawFieldOfView (horizontalOffsetAngle, i);																				//also pass index i of mesh.
		}


		int verticalOffsetStepCount = Mathf.RoundToInt(viewAngle * verticalOffsetResolution);
		float verticalOffsetStepAngleSize = viewAngle / verticalOffsetStepCount;
		for (int i = 0; i <= verticalOffsetStepCount; i++) {																			//maybe add a +1 here. Check later.
			float verticalOffsetAngle = - viewAngle / 2 + verticalOffsetStepAngleSize * i;
			DrawVerticalFieldOfView (verticalOffsetAngle, i);																				//also pass index i of mesh.
		}
	}

	void FindHorizontallyVisibleTargets() {
		horizontallyVisibleTargets.Clear ();
		Collider[] targetsInViewRadius = Physics.OverlapSphere (transform.position, viewRadius, targetMask);
		for (int i = 0; i < targetsInViewRadius.Length; i++) {
			Transform target = targetsInViewRadius [i].transform;
			Vector3 vectorToTarget = (target.position - transform.position);
			Vector3 horizontalProjection = Vector3.ProjectOnPlane(vectorToTarget, transform.up);
			if (Vector3.Angle (transform.forward, horizontalProjection) < viewAngle / 2) {
				float dstToTarget = Vector3.Distance (transform.position, target.position);
				if (!Physics.Raycast (transform.position, vectorToTarget.normalized, dstToTarget, obstacleMask)) {
					horizontallyVisibleTargets.Add (target);
				}
			}
		}
	}

	void FindVerticallyVisibleTargets() {
		verticallyVisibleTargets.Clear ();
		Collider[] targetsInViewRadius = Physics.OverlapSphere (transform.position, verticalViewRadius, targetMask);
		for (int i = 0; i < targetsInViewRadius.Length; i++) {
			Transform target = targetsInViewRadius [i].transform;
			Vector3 vectorToTarget = (target.position - transform.position);
			Vector3 verticalProjection = Vector3.ProjectOnPlane(vectorToTarget, transform.right);
			if (Vector3.Angle (transform.forward, verticalProjection) < verticalViewAngle / 2) {
				float dstToTarget = Vector3.Distance (transform.position, target.position);
				if (!Physics.Raycast (transform.position, vectorToTarget.normalized, dstToTarget, obstacleMask)) {
					verticallyVisibleTargets.Add (target);
				}
			}
		}
	}

	void DrawFieldOfView(float offsetAngle, int countmesh) {
		int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
		float stepAngleSize = viewAngle / stepCount;
		List<Vector3> viewPoints = new List<Vector3> ();
		ViewCastInfo oldViewCast = new ViewCastInfo ();
		for (int i = 0; i <= stepCount; i++) {
			float angle = - viewAngle / 2 + stepAngleSize * i;
			ViewCastInfo newViewCast = ViewCast (angle, offsetAngle);

			if (i > 0) {
				bool edgeDstThresholdExceeded = Mathf.Abs (oldViewCast.dst - newViewCast.dst) > edgeDstThreshold;
				if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded)) {
					EdgeInfo edge = FindEdge (oldViewCast, newViewCast);
					if (edge.pointA != Vector3.zero) {
						viewPoints.Add (edge.pointA);
					}
					if (edge.pointB != Vector3.zero) {
						viewPoints.Add (edge.pointB);
					}
				}

			}


			viewPoints.Add (newViewCast.point);
			oldViewCast = newViewCast;
		}

		int vertexCount = viewPoints.Count + 1;
		Vector3[] vertices = new Vector3[vertexCount];
		int[] triangles = new int[(vertexCount-2) * 3];

		vertices [0] = Vector3.zero;
		for (int i = 0; i < vertexCount - 1; i++) {
			vertices [i + 1] = transform.InverseTransformPoint(viewPoints [i]);

			if (i < vertexCount - 2) {
				triangles [i * 3] = 0;
				triangles [i * 3 + 1] = i + 1;
				triangles [i * 3 + 2] = i + 2;
			}
		}

		viewMeshArray[countmesh].Clear ();

		viewMeshArray[countmesh].vertices = vertices;
		viewMeshArray[countmesh].triangles = triangles;
		viewMeshArray[countmesh].RecalculateNormals ();
	}

	void DrawVerticalFieldOfView(float offsetAngle, int countmesh) {															//countmesh is the index that allows us to reference each mesh independently.
		int stepCount = Mathf.RoundToInt(verticalViewAngle * verticalMeshResolution);
		float stepAngleSize = verticalViewAngle / stepCount;
		List<Vector3> viewPoints = new List<Vector3> ();
		VerticalViewCastInfo oldViewCast = new VerticalViewCastInfo ();
		for (int i = 0; i <= stepCount; i++) {
			float angle = - verticalViewAngle / 2 + stepAngleSize * i;
			VerticalViewCastInfo newViewCast = VerticalViewCast (angle, offsetAngle);

			if (i > 0) {
				bool edgeDstThresholdExceeded = Mathf.Abs (oldViewCast.dst - newViewCast.dst) > edgeDstThreshold;
				if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded)) {
					EdgeInfo edge = VerticalFindEdge (oldViewCast, newViewCast);
					if (edge.pointA != Vector3.zero) {
						viewPoints.Add (edge.pointA);
					}
					if (edge.pointB != Vector3.zero) {
						viewPoints.Add (edge.pointB);
					}
				}

			}


			viewPoints.Add (newViewCast.point);
			oldViewCast = newViewCast;
		}

		int vertexCount = viewPoints.Count + 1;
		Vector3[] vertices = new Vector3[vertexCount];
		int[] triangles = new int[(vertexCount-2) * 3];

		vertices [0] = Vector3.zero;
		for (int i = 0; i < vertexCount - 1; i++) {
			vertices [i + 1] = transform.InverseTransformPoint(viewPoints [i]);

			if (i < vertexCount - 2) {
				triangles [i * 3] = 0;
				triangles [i * 3 + 1] = i + 1;
				triangles [i * 3 + 2] = i + 2;
			}
		}

		verticalMeshArray[countmesh].Clear ();

		verticalMeshArray[countmesh].vertices = vertices;
		verticalMeshArray[countmesh].triangles = triangles;
		verticalMeshArray[countmesh].RecalculateNormals ();
	}


	EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast) {
		float minAngle = minViewCast.angle;
		float maxAngle = maxViewCast.angle;
		float offsetAngle = minViewCast.offset;
		Vector3 minPoint = Vector3.zero;
		Vector3 maxPoint = Vector3.zero;

		for (int i = 0; i < edgeResolveIterations; i++) {
			float angle = (minAngle + maxAngle) / 2;
			ViewCastInfo newViewCast = ViewCast (angle, offsetAngle);

			bool edgeDstThresholdExceeded = Mathf.Abs (minViewCast.dst - newViewCast.dst) > edgeDstThreshold;
			if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded) {
				minAngle = angle;
				minPoint = newViewCast.point;
			} else {
				maxAngle = angle;
				maxPoint = newViewCast.point;
			}
		}

		return new EdgeInfo (minPoint, maxPoint);
	}

	EdgeInfo VerticalFindEdge(VerticalViewCastInfo minViewCast, VerticalViewCastInfo maxViewCast) {    //ftiakse auto. Na leitourgei me offset.
		float minAngle = minViewCast.angle;
		float maxAngle = maxViewCast.angle;
		float offsetAngle = minViewCast.offset;
		Vector3 minPoint = Vector3.zero;
		Vector3 maxPoint = Vector3.zero;

		for (int i = 0; i < edgeResolveIterations; i++) {
			float angle = (minAngle + maxAngle) / 2;
			VerticalViewCastInfo newViewCast = VerticalViewCast (angle, offsetAngle);

			bool edgeDstThresholdExceeded = Mathf.Abs (minViewCast.dst - newViewCast.dst) > edgeDstThreshold;
			if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded) {
				minAngle = angle;
				minPoint = newViewCast.point;
			} else {
				maxAngle = angle;
				maxPoint = newViewCast.point;
			}
		}

		return new EdgeInfo (minPoint, maxPoint);
	}



	ViewCastInfo ViewCast(float globalAngle, float offset) {
		Vector3 dir = HorizontalDirFromAngle (globalAngle, offset);
		RaycastHit hit;

		if (Physics.Raycast (transform.position, dir, out hit, viewRadius, obstacleMask)) {
			return new ViewCastInfo (true, hit.point, hit.distance, globalAngle, offset);
		} else {
			return new ViewCastInfo (false, transform.position + dir * (viewRadius - Mathf.Abs(offset)*Mathf.Deg2Rad*viewRadius/5), viewRadius, globalAngle, offset);
		}
	}

	VerticalViewCastInfo VerticalViewCast(float globalAngle, float offset) {
		Vector3 dir = VerticalDirFromAngle (globalAngle, offset);
		RaycastHit hit;

		if (Physics.Raycast (transform.position, dir, out hit, verticalViewRadius, obstacleMask)) {
			return new VerticalViewCastInfo (true, hit.point, hit.distance, globalAngle, offset);
		} else {
			return new VerticalViewCastInfo (false, transform.position + dir * (verticalViewRadius - Mathf.Abs(offset)*Mathf.Deg2Rad*verticalViewRadius/5), verticalViewRadius, globalAngle, offset);		//mageirema me meiwsh ths aktinas oso megalwnei to offset gia na doume ti paei lathos
		}
	}

	public Vector3 HorizontalDirFromAngle(float angleInDegrees, float verticalOffsetAngleInDegrees) {
		Vector3 direction = Mathf.Cos(angleInDegrees * Mathf.Deg2Rad)*transform.forward + Mathf.Sin(angleInDegrees * Mathf.Deg2Rad)*transform.right + Mathf.Sin(verticalOffsetAngleInDegrees * Mathf.Deg2Rad)*transform.up; //direction of horizontal fov line in relation to character's transform.
		return direction;
	}

	public Vector3 VerticalDirFromAngle(float angleInDegrees, float horizontalOffsetAngleInDegrees) {
		Vector3 direction = Mathf.Cos(angleInDegrees * Mathf.Deg2Rad)*transform.forward + Mathf.Sin(angleInDegrees * Mathf.Deg2Rad)*transform.up + Mathf.Sin(horizontalOffsetAngleInDegrees * Mathf.Deg2Rad)*transform.right; //direction of vertical fov line in relation to character's transform.
		return direction;
	}

	public struct ViewCastInfo {
		public bool hit;
		public Vector3 point;
		public float dst;
		public float angle;
		public float offset;

		public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle, float _offset) {
			hit = _hit;
			point = _point;
			dst = _dst;
			angle = _angle;
			offset = _offset;
		}
	}

	public struct VerticalViewCastInfo {
		public bool hit;
		public Vector3 point;
		public float dst;
		public float angle;
		public float offset;

		public VerticalViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle, float _offset) {
			hit = _hit;
			point = _point;
			dst = _dst;
			angle = _angle;
			offset = _offset;
		}
	}

	public struct EdgeInfo {
		public Vector3 pointA;
		public Vector3 pointB;

		public EdgeInfo(Vector3 _pointA, Vector3 _pointB) {
			pointA = _pointA;
			pointB = _pointB;
		}
	}

}
