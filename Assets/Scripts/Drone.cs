using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : MonoBehaviour {
    public float MAX_SPEED = 30;
    public float MAX_SPEED_HOR = 5;
    public float MAX_ANGLE = 20;
    public float APPROACH_INTENSITY= 1;
    public float HORIZONTAL_APPROACH_INTENSITY = 1;
    public float ANGLE_INTENSITY = 1;
    public float PROP_STRENGTH = 200;
    public float TURN_STRENGTH = 10;
    public float EXTERNAL_FORCE_INTENSITY = 2;
    public bool mouseControl = false;
    //-------------------------------------------
    //-------------------------------------------
    private GameObject[] props = new GameObject[4];
    private GameObject[] propsRef = new GameObject[4];
    private Vector3[] propsForce = new Vector3[4];
    private Vector3 velocity;
    private Vector3 position;
    private Vector3 deltaPosition = Vector3.zero;
    Vector3 targetPosition = Vector3.zero;
    Vector3 vt = Vector3.zero, vht = Vector3.zero,
        anglet = Vector3.zero, 
        resultingForce = Vector3.zero;
    private float counter = 0;
    
    // Use this for initialization
    void Start () {
        this.props[0] = gameObject.transform.Find("Propeller1").gameObject;
        this.props[1] = gameObject.transform.Find("Propeller2").gameObject;
        this.props[2] = gameObject.transform.Find("Propeller3").gameObject;
        this.props[3] = gameObject.transform.Find("Propeller4").gameObject;
        this.propsRef[0] = gameObject.transform.Find("p1_ref").gameObject;
        this.propsRef[1] = gameObject.transform.Find("p2_ref").gameObject;
        this.propsRef[2] = gameObject.transform.Find("p3_ref").gameObject;
        this.propsRef[3] = gameObject.transform.Find("p4_ref").gameObject;
        this.clearPropsForce();
        position = gameObject.transform.position;
        this.targetPosition.Set(0, 0, 0);
    }
	
	// Update is called once per frame
	void Update () {
        foreach (GameObject prop in props)
        {
            prop.transform.Rotate(Vector3.up * 1080 * Time.deltaTime);
        }
        this.position = this.position + gameObject.GetComponent<Rigidbody>().velocity * Time.deltaTime;
        this.adjustPosition();
        this.applyPropellers();
        this.clearPropsForce();
        Debug.DrawLine(Vector3.zero, Vector3.zero + this.targetPosition, Color.red);
    }

    void setPropeller(int number, float intensity)
    {
        Vector3 force = (propsRef[number].transform.position - props[number].transform.position) * intensity;
        propsForce[number] += force;
    }

    void adjustPosition()
    {
        vt = (this.targetPosition - this.position)*APPROACH_INTENSITY;
        vht = (this.targetPosition - this.position)*HORIZONTAL_APPROACH_INTENSITY;
        vt.Set(absoluteLimit(vt.x, MAX_SPEED), 
            absoluteLimit(vt.y, MAX_SPEED), 
            absoluteLimit(vt.z, MAX_SPEED));
        vht.Set(absoluteLimit(vht.x * Mathf.Cos(transform.eulerAngles.y * Mathf.Deg2Rad)
                            - vht.z * Mathf.Sin(transform.eulerAngles.y * Mathf.Deg2Rad), MAX_SPEED_HOR), 
            absoluteLimit(vht.y, MAX_SPEED_HOR), 
            absoluteLimit(vht.z * Mathf.Cos(transform.eulerAngles.y * Mathf.Deg2Rad)
                        + vht.x * Mathf.Sin(transform.eulerAngles.y * Mathf.Deg2Rad), MAX_SPEED_HOR));
        anglet = (vht - gameObject.GetComponent<Rigidbody>().velocity) * ANGLE_INTENSITY;
        resultingForce = (vt - gameObject.GetComponent<Rigidbody>().velocity) * PROP_STRENGTH;
        setPropeller(0, resultingForce.y / 4);
        setPropeller(1, resultingForce.y / 4);
        setPropeller(2, resultingForce.y / 4);
        setPropeller(3, resultingForce.y / 4);
        turnX(anglet.x);
        turnZ(anglet.z);
        getInput();
        if (mouseControl)
        {
            this.targetPosition.x = transform.position.x + deltaPosition.x;
            this.targetPosition.z = transform.position.z + deltaPosition.z;
        }
    }

    void turnX(float anglet)
    {
        anglet = absoluteLimit(anglet, MAX_ANGLE);
        float angVelT = Mathf.DeltaAngle(transform.eulerAngles.z, -anglet);
        float propellerStrength = angVelT - GetComponent<Rigidbody>().angularVelocity.z;
        if (propellerStrength < 0)
        {
            setPropeller(0, TURN_STRENGTH * Mathf.Abs(propellerStrength));
            setPropeller(3, TURN_STRENGTH * Mathf.Abs(propellerStrength));
            setPropeller(1, 0);
            setPropeller(2, 0);
        }
        else if (propellerStrength > 0)
        {
            setPropeller(1, TURN_STRENGTH * Mathf.Abs(propellerStrength));
            setPropeller(2, TURN_STRENGTH * Mathf.Abs(propellerStrength));
            setPropeller(0, 0);
            setPropeller(3, 0);
        }
    }

    void turnZ(float anglet)
    {
        anglet = absoluteLimit(anglet, MAX_ANGLE);
        float angVelT = Mathf.DeltaAngle(transform.eulerAngles.x, anglet);
        float propellerStrength = angVelT - GetComponent<Rigidbody>().angularVelocity.x;
        if (propellerStrength > 0)
        {
            setPropeller(0, TURN_STRENGTH * Mathf.Abs(propellerStrength));
            setPropeller(1, TURN_STRENGTH * Mathf.Abs(propellerStrength));
            setPropeller(2, 0);
            setPropeller(3, 0);
        }
        else if (propellerStrength < 0)
        {
            setPropeller(2, TURN_STRENGTH * Mathf.Abs(propellerStrength));
            setPropeller(3, TURN_STRENGTH * Mathf.Abs(propellerStrength));
            setPropeller(0, 0);
            setPropeller(1, 0);
        }
    }

    void applyPropellers()
    {
        for (int i = 0; i < 4; i++)
        {
            if(propsForce[i].y < 0)
            {
                propsForce[i] = Vector3.zero;
            }
            gameObject.GetComponent<Rigidbody>().AddForceAtPosition(propsForce[i], this.props[i].transform.position);
        }
    }

    bool count(float seconds)
    {
        this.counter += Time.deltaTime;
        if (this.counter >= seconds)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    float absoluteLimit(float current, float max)
    {
        if (current > max)
        {
            return max;
        }
        if (current < -max)
        {
            return -max;
        }
        return current;
    }

    void clearPropsForce()
    {
        for(int i = 0; i < 4; i++)
        {
            propsForce[i] = Vector3.zero;
        }
    }

    void getInput()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            mouseControl = !mouseControl;
            deltaPosition = Vector3.zero;
        }

        if (Input.GetKey("w"))
            GetComponent<Rigidbody>().AddForce(0, 0, EXTERNAL_FORCE_INTENSITY);

        if (Input.GetKey("s"))
            GetComponent<Rigidbody>().AddForce(0, 0, -EXTERNAL_FORCE_INTENSITY);

        if (Input.GetKey("a"))
            GetComponent<Rigidbody>().AddForce(-EXTERNAL_FORCE_INTENSITY, 0, 0);

        if (Input.GetKey("d"))
            GetComponent<Rigidbody>().AddForce(EXTERNAL_FORCE_INTENSITY, 0, 0);

        if (Input.GetKey("u"))
            this.targetPosition.y += 0.1f;

        if (Input.GetKey("j"))
            this.targetPosition.y -= 0.1f;

        if (!mouseControl)
        {
            if (Input.GetKey("up"))
                this.targetPosition.z += 0.1f;

            if (Input.GetKey("down"))
                this.targetPosition.z -= 0.1f;

            if (Input.GetKey("left"))
                this.targetPosition.x -= 0.1f;

            if (Input.GetKey("right"))
                this.targetPosition.x += 0.1f;
        }
        if (mouseControl)
        {
            this.deltaPosition.x += 1f * Input.GetAxis("Mouse X");
            this.deltaPosition.z += 1f * Input.GetAxis("Mouse Y");

            this.deltaPosition.x = absoluteLimit(this.deltaPosition.x, 15f);
            this.deltaPosition.z = absoluteLimit(this.deltaPosition.z, 15f);
        }
    }
}
