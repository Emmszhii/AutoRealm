/*===============================================================================
Copyright (c) 2022 PTC Inc. All Rights Reserved.

Confidential and Proprietary - Protected under copyright and other laws.
Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/

using UnityEngine;
using Vuforia;

/// <summary>
/// Draw an outline around the barcode
/// </summary>
public class BarcodeOutlineBehaviour : MonoBehaviour
{
    [SerializeField] public Material Material;

    [SerializeField] public float OutlineThickness = 5;

    BarcodeBehaviour mBarcodeBehaviour;

    GameObject[] mLines;

    GameObject[] mCorners;

    /// <summary>
    /// Called when the script is started
    /// </summary>
    void Start()
    {
        mBarcodeBehaviour = GetComponent<BarcodeBehaviour>();
        if (mBarcodeBehaviour != null)
        {
            mBarcodeBehaviour.OnBarcodeOutlineChanged += OnBarcodeOutlineChanged;
        }
    }

    /// <summary>
    /// Called when the Barcode vertices changed
    /// </summary>
    void OnBarcodeOutlineChanged(Vector3[] vertices)
    {
        UpdateMesh(vertices);
    }

    /// <summary>
    /// Update the outline of the barcode
    /// </summary>
    void UpdateMesh(Vector3[] vertices)
    {
        if (mLines == null)
        {
            mLines = new []
            {
                GameObject.CreatePrimitive(PrimitiveType.Cylinder),
                GameObject.CreatePrimitive(PrimitiveType.Cylinder),
                GameObject.CreatePrimitive(PrimitiveType.Cylinder),
                GameObject.CreatePrimitive(PrimitiveType.Cylinder),
            };

            foreach (var go in mLines)
            {
                go.transform.parent = transform;
                go.transform.localPosition = Vector3.zero;
                go.transform.localScale = Vector3.one;
                go.transform.localRotation = Quaternion.identity;
                go.GetComponent<MeshRenderer>().material = Material;

                Destroy(go.GetComponent<Collider>());
            }

            mCorners = new []
            {
                GameObject.CreatePrimitive(PrimitiveType.Sphere),
                GameObject.CreatePrimitive(PrimitiveType.Sphere),
                GameObject.CreatePrimitive(PrimitiveType.Sphere),
                GameObject.CreatePrimitive(PrimitiveType.Sphere),
            };

            foreach (var go in mCorners)
            {
                go.transform.parent = transform;
                go.transform.localPosition = Vector3.zero;
                go.transform.localScale = Vector3.one;
                go.transform.localRotation = Quaternion.identity;
                go.GetComponent<MeshRenderer>().material = Material;

                Destroy(go.GetComponent<Collider>());
            }
        }

        var offset = Mathf.Sqrt(2) * (OutlineThickness) * 0.5f;
        for (var i = 0; i < vertices.Length; ++i)
        {
            var v0 = vertices[i];
            var v1 = vertices[(i + 1) % vertices.Length];
            v0 *= (1.0f + offset / v0.magnitude);
            v1 *= (1.0f + offset / v1.magnitude);

            var dir = v1 - v0;
            var line = mLines[i];

            var c = Vector3.Lerp(v0, v1, 0.5f);

            line.transform.localPosition = c;
            line.transform.localRotation = Quaternion.LookRotation(dir, transform.up) *
                                           Quaternion.AngleAxis(90, Vector3.right);

            line.transform.localScale = new Vector3(OutlineThickness, dir.magnitude * 0.5f, OutlineThickness);

            var corner = mCorners[i];
            corner.transform.localPosition = v0;
            corner.transform.localRotation = Quaternion.identity;
            corner.transform.localScale = new Vector3(OutlineThickness, OutlineThickness, OutlineThickness);
        }
    }
}