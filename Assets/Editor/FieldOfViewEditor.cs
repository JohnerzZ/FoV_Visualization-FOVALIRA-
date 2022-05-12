using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor (typeof (FieldOfView))]
public class FieldOfViewEditor : Editor {

	void OnSceneGUI() {
		FieldOfView fow = (FieldOfView)target;
		Handles.color = Color.white;
		Handles.DrawWireArc (fow.transform.position, fow.transform.up, fow.transform.forward, 360, fow.viewRadius);
		Vector3 horizontalViewAngleA = fow.HorizontalDirFromAngle(-fow.viewAngle / 2, 0.0f);
		Vector3 horizontalViewAngleB = fow.HorizontalDirFromAngle(fow.viewAngle / 2, 0.0f);

		Handles.DrawLine (fow.transform.position, fow.transform.position + horizontalViewAngleA * fow.viewRadius);
		Handles.DrawLine (fow.transform.position, fow.transform.position + horizontalViewAngleB * fow.viewRadius);


		Handles.DrawWireArc (fow.transform.position, fow.transform.right, fow.transform.forward, 360, fow.verticalViewRadius);
		Vector3 verticalViewAngleA = fow.VerticalDirFromAngle (-fow.verticalViewAngle / 2, 0.0f);
		Vector3 verticalViewAngleB = fow.VerticalDirFromAngle (fow.verticalViewAngle / 2, 0.0f);

		Handles.DrawLine (fow.transform.position, fow.transform.position + verticalViewAngleA * fow.verticalViewRadius);
		Handles.DrawLine (fow.transform.position, fow.transform.position + verticalViewAngleB * fow.verticalViewRadius);

		Handles.color = Color.red;
		foreach (GameObject visibleTarget in fow.horizontallyVisibleTargets) {
			if (fow.verticallyVisibleTargets.Contains(visibleTarget)) {
				Handles.DrawLine (fow.transform.position, visibleTarget.transform.position);
			}
		}
	}
}
