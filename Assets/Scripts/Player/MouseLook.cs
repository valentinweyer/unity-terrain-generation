using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour 
{
	public float  mouseSensitivity = 100f;

	public Transform playerBody;
	public Transform playerHead;

	float xRotation = 0f;
	//float yRotation = 0f;
	
	void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
	}

	// Update is called once per frame
	void Update ()
	{
		float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
		float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

		xRotation -= mouseY;
		xRotation = Mathf.Clamp(xRotation, -90f, 90f);

		//yRotation += mouseY;
		//yRotation = Mathf.Clamp(yRotation, -90f, 90f);

		transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f); 
		playerBody.Rotate(Vector3.up * mouseX);

		//transform.localRotation = Quaternion.Euler(yRotation, 0f, 0f);
		//playerHead.Rotate(Vector3.up * mouseY);
	}
}