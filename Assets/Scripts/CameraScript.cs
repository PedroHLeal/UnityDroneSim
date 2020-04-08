using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {

    public GameObject drone;

    private Vector3 position = Vector3.zero;
    private Vector3 rotation = Vector3.zero;
    // Use this for initialization
    void Start () {
        rotation.Set(30, 0, 0);
        transform.Rotate(rotation);
	}
	
	// Update is called once per frame
	void Update () {
        position.Set(drone.transform.position.x, drone.transform.position.y + 2, drone.transform.position.z - 2);
        transform.position = position;
	}
}
