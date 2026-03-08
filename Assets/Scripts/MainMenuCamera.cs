using UnityEngine;
using UnityEngine.UI;

public class MainMenuCamera: CameraControl
{
    public override void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * GameMaster.instance.mouseSensitivity * Time.deltaTime;

        float mouseY = Input.GetAxis("Mouse Y") * GameMaster.instance.mouseSensitivity * Time.deltaTime;

        XRot -= mouseY;

        XRot = Mathf.Clamp(XRot, -viewAngle, viewAngle);

        YRot += mouseX;

        YRot = Mathf.Clamp(YRot, -viewAngle, viewAngle);

        if (Input.GetMouseButton(1))
        {
            transform.localRotation = Quaternion.Euler(XRot, YRot, 0f);
        }
        else transform.localRotation = Quaternion.Slerp(transform.localRotation, new Quaternion(0.608761489f, 0, 0, 0.793353319f), 0.01f); //Change in future
    }
}
