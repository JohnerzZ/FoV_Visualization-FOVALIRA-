using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;

public class InstantiateFromFile : MonoBehaviour {

    public int currentPiece = 0;
    public GameObject sensorPrefab;
    public GameObject parent;    //the parent of the sensors in our case will be the laelaps object prefab.
    public StreamReader sr = new StreamReader(@"instantiation.txt");
    public float posX, posY, posZ, pieceRotationX, pieceRotationY, pieceRotationZ, viewRadius, viewAngle, verticalViewRadius, verticalViewAngle, meshResolution, horizontalOffsetResolution, verticalMeshResolution, verticalOffsetResolution, edgeResolveIterations, edgeDstThreshold;

    void Start() {
      string line = sr.ReadLine();
      line = sr.ReadLine(); //skip first line
      while (line != null) {
          var separator = ",";
          var theLine = line.Split(separator[0]);
          //print(line);
          //print(theLine[0]);

          //load sensor position and rotation in the scene.
          posX = (float.Parse(theLine[0]));
          posY = (float.Parse(theLine[1]));
          posZ = (float.Parse(theLine[2]));
          pieceRotationX = (float.Parse(theLine[3]));
          pieceRotationY = (float.Parse(theLine[4]));
          pieceRotationZ = (float.Parse(theLine[5]));
          //load sensor parameters.
          viewRadius = (float.Parse(theLine[6]));
          viewAngle = (float.Parse(theLine[7]));
          verticalViewRadius = (float.Parse(theLine[8]));
          verticalViewAngle = (float.Parse(theLine[9]));
          meshResolution = (float.Parse(theLine[10]));
          horizontalOffsetResolution = (float.Parse(theLine[11]));
          verticalMeshResolution = (float.Parse(theLine[12]));
          verticalOffsetResolution = (float.Parse(theLine[13]));
          edgeResolveIterations = (float.Parse(theLine[14]));
          edgeDstThreshold = (float.Parse(theLine[15]));

          //Instantiate sensor at given position
          var sensor = Instantiate(sensorPrefab, parent.transform.position + posX*parent.transform.right + posY*parent.transform.up + posZ*parent.transform.forward, parent.transform.rotation, parent.transform);
          sensor.transform.Rotate( new Vector3(pieceRotationX, pieceRotationY, pieceRotationZ) );
          sensor.SetActive(true);
          sensor.name = ("sensor" + currentPiece);

          //set sensor parameters for its field of view script
          FieldOfView sensorScript = sensor.GetComponent<FieldOfView>();
          sensorScript.viewRadius = viewRadius;
          sensorScript.viewAngle = viewAngle;
          sensorScript.verticalViewRadius = verticalViewRadius;
          sensorScript.verticalViewAngle = verticalViewAngle;
          sensorScript.meshResolution = meshResolution;
          sensorScript.horizontalOffsetResolution = horizontalOffsetResolution;
          sensorScript.verticalMeshResolution = verticalMeshResolution;
          sensorScript.verticalOffsetResolution = verticalOffsetResolution;
          sensorScript.edgeResolveIterations = (int)edgeResolveIterations;
          sensorScript.edgeDstThreshold = edgeDstThreshold;

          currentPiece ++;
          line = sr.ReadLine();
      }
      sr.Close();
    }
}
