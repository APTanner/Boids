using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 10f;

    float xDir;
    float yDir;

    float dX;
    float dY;

    private void Start()
    {
        xDir = transform.rotation.x;
        yDir = transform.rotation.y;
    }

    void Update()
    { 
        //Rotation
        xDir += Input.GetAxis("Mouse X"); //represents rotation from side to side
        yDir += -Input.GetAxis("Mouse Y"); //represents rotation up and down
        yDir = Mathf.Clamp(yDir, -90, 90);
        transform.eulerAngles = new Vector3(yDir, xDir);

        //Translation
        dX = Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed;
        dY = Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed;
        transform.Translate(new Vector3(dX, 0, dY), Space.Self);



    }
}
