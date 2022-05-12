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
	public float verticalMeshResolution;
	public float verticalOffsetResolution;
	public int edgeResolveIterations;
	public float edgeDstThreshold;

	public MeshFilter viewMeshFilter;
	Mesh viewMesh;

	void Start() {
		viewMesh = new Mesh ();
		viewMesh.name = "View Mesh";
		viewMeshFilter.mesh = viewMesh;

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

		DrawFieldOfView ();

		int verticalOffsetStepCount = Mathf.RoundToInt(viewAngle * verticalOffsetResolution);
		float verticalOffsetStepAngleSize = viewAngle / verticalOffsetStepCount;
		for (int i = 0; i <= verticalOffsetStepCount; i++) {
			float verticalOffsetAngle = - viewAngle / 2 + verticalOffsetStepAngleSize * i;
			DrawVerticalFieldOfView (verticalOffsetAngle);
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

	void DrawFieldOfView() {
		int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
		float stepAngleSize = viewAngle / stepCount;
		List<Vector3> viewPoints = new List<Vector3> ();
		ViewCastInfo oldViewCast = new ViewCastInfo ();
		for (int i = 0; i <= stepCount; i++) {
			float angle = - viewAngle / 2 + stepAngleSize * i;
			ViewCastInfo newViewCast = ViewCast (angle);

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

		viewMesh.Clear ();

		viewMesh.vertices = vertices;
		viewMesh.triangles = triangles;
		viewMesh.RecalculateNormals ();
	}

	void DrawVerticalFieldOfView(float offsetAngle) {
		int stepCount = Mathf.RoundToInt(verticalViewAngle * verticalMeshResolution);
		float stepAngleSize = verticalViewAngle / stepCount;
		for (int i = 0; i <= stepCount; i++) {
			float angle = - verticalViewAngle / 2 + stepAngleSize * i;
			Debug.DrawLine(transform.position, transform.position + VerticalDirFromAngle(angle, offsetAngle)*verticalViewRadius, Color.blue );
		}
	}



	EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast) {
		float minAngle = minViewCast.angle;
		float maxAngle = maxViewCast.angle;
		Vector3 minPoint = Vector3.zero;
		Vector3 maxPoint = Vector3.zero;

		for (int i = 0; i < edgeResolveIterations; i++) {
			float angle = (minAngle + maxAngle) / 2;
			ViewCastInfo newViewCast = ViewCast (angle);

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


	ViewCastInfo ViewCast(float globalAngle) {
		Vector3 dir = HorizontalDirFromAngle (globalAngle);
		RaycastHit hit;

		if (Physics.Raycast (transform.position, dir, out hit, viewRadius, obstacleMask)) {
			return new ViewCastInfo (true, hit.point, hit.distance, globalAngle);
		} else {
			return new ViewCastInfo (false, transform.position + dir * viewRadius, viewRadius, globalAngle);
		}
	}

	VerticalViewCastInfo VerticalViewCast(float globalAngle, float offset) {
		Vector3 dir = VerticalDirFromAngle (globalAngle, offset);
		RaycastHit hit;

		if (Physics.Raycast (transform.position, dir, out hit, viewRadius, obstacleMask)) {
			return new VerticalViewCastInfo (true, hit.point, hit.distance, globalAngle, offset);
		} else {
			return new VerticalViewCastInfo (false, transform.position + dir * viewRadius, viewRadius, globalAngle, offset);
		}
	}

	public Vector3 HorizontalDirFromAngle(float angleInDegrees) {
		Vector3 direction = Mathf.Cos(angleInDegrees * Mathf.Deg2Rad)*transform.forward + Mathf.Sin(angleInDegrees * Mathf.Deg2Rad)*transform.right; //direction of horizontal fov line in relation to character's transform.
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

		public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle) {
			hit = _hit;
			point = _point;
			dst = _dst;
			angle = _angle;
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
