using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

// Agrega este script a la Main Camera para que siga el movimiento del headset.
// Funciona con Oculus Loader y cualquier proveedor XR sin necesidad de XR Rig.
// Si ya tienes un XR Origin / XR Rig con TrackedPoseDriver, NO agregues este script
// (sería redundante).
[RequireComponent(typeof(Camera))]
[DisallowMultipleComponent]
public class XRHeadTracking : MonoBehaviour
{
    [Header("Floor Offset")]
    [Tooltip("Altura del ojo sobre el suelo cuando el headset reporta posicion (0,0,0). " +
             "Valor tipico para Oculus en modo Device: 0. Para modo Floor: 0.")]
    public float floorOffset = 0f;

    private readonly List<InputDevice> _headDevices = new List<InputDevice>();

    void OnEnable()
    {
        InputDevices.deviceConnected    += OnDeviceChanged;
        InputDevices.deviceDisconnected += OnDeviceChanged;
        RefreshDevices();
    }

    void OnDisable()
    {
        InputDevices.deviceConnected    -= OnDeviceChanged;
        InputDevices.deviceDisconnected -= OnDeviceChanged;
    }

    void OnDeviceChanged(InputDevice _) => RefreshDevices();

    void RefreshDevices()
    {
        _headDevices.Clear();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeadMounted, _headDevices);

        if (_headDevices.Count == 0)
            Debug.LogWarning("[XRHeadTracking] No se encontró ningún dispositivo HMD.");
    }

    void Update()
    {
        if (_headDevices.Count == 0) return;

        InputDevice head = _headDevices[0];

        if (head.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 pos))
            transform.localPosition = new Vector3(pos.x, pos.y + floorOffset, pos.z);

        if (head.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rot))
            transform.localRotation = rot;
    }
}
