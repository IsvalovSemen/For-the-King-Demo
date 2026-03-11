using UnityEngine;
using UnityEngine.UI;

public class MainMenuCamera: CameraControl
{
    public override void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * GameMaster.instance.mouseSensitivity * Time.deltaTime;

        float mouseY = Input.GetAxis("Mouse Y") * GameMaster.instance.mouseSensitivity * Time.deltaTime;

        XRotation -= mouseY;

        XRotation = Mathf.Clamp(XRotation, -maxHorizontalAngle, maxHorizontalAngle);

        YRotation += mouseX;

        YRotation = Mathf.Clamp(YRotation, -maxVerticalAngle, maxVerticalAngle);

        if (Input.GetMouseButton(1))
        {
            transform.localRotation = Quaternion.Euler(XRotation, YRotation, 0f);
        }
        else transform.localRotation = Quaternion.Slerp(transform.localRotation, new Quaternion(0.608761489f, 0, 0, 0.793353319f), 0.01f); //Change in future
    }
}
