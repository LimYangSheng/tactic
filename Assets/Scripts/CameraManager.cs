using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraManager : MonoBehaviour
{
    public Camera mainCamera;

    // Update is called once per frame
    void Update()
	{
        float panSpeed = 5.0f;
        float edgePanSize = 5.0f;
        Transform cameraTransform = mainCamera.gameObject.transform;
        if (Input.mousePosition.x > Screen.width - edgePanSize) {
            cameraTransform.position += new Vector3(panSpeed * Time.deltaTime, 0, 0);
        }
        if (Input.mousePosition.x < edgePanSize) {
            cameraTransform.position -= new Vector3(panSpeed * Time.deltaTime, 0, 0);
        }
        if (Input.mousePosition.y > Screen.height - edgePanSize) {
            cameraTransform.position += new Vector3(0, panSpeed * Time.deltaTime, 0);
        }
        if (Input.mousePosition.y < edgePanSize) {
            cameraTransform.position -= new Vector3(0, panSpeed * Time.deltaTime, 0);
        }
    }
}
