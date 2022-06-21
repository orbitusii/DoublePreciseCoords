/*

CVW-5 Prototype 2: View-Condensed Object
Author: Dan Lodholm (github: orbitusii)
Copyright: 2022

*/

using UnityEngine;

[RequireComponent(typeof(WrappedBody))]
public class ViewCondObject : MonoBehaviour, ILargePosition
{
#pragma warning disable CS0108
    [SerializeField, Tooltip("This object's Rigidbody component, if it exists. Will be set automatically if null.")]
    protected Rigidbody rigidbody;
    [SerializeField, Tooltip("This object's Wrapped Body component, if it exists. Will be set automatically if null.")]
    protected WrappedBody WrappedBody;

    public bool InterpolateView = false;

    public Camera TargetCamera;
    protected ILargePosition CameraPositionRef;

    public Vector64 GetPosition ()
    {
        return WrappedBody.GetPosition();
    }

    void ILargePosition.MovePosition(Vector64 delta)
    {
        throw new System.NotSupportedException("Moving a ViewCondObject via MovePosition() is not allowed!");
    }

    // We do this here rather than in Update() because while in other games the CAMERA is moving,
    // in our case the camera is stationary and EVERYTHING ELSE is moving. Thus, we do Unity's
    // recommended order in reverse.
    protected virtual void LateUpdate()
    {
        if (TargetCamera == null) return;

        if (CameraPositionRef == null)
        {
            CameraPositionRef = TargetCamera.GetComponent(typeof(ILargePosition)) as ILargePosition;
        }

        PlaceRelativeToCamera();
    }

    public Vector64 GetInterpolatedPosition()
    {
        float dt = Time.time - Time.fixedTime;

        Vector3 positionDiff = rigidbody.velocity * dt;

        return WrappedBody.GetPosition() + positionDiff;
    }

    public virtual void PlaceRelativeToCamera()
    {
        Vector3 relativePosition;

        // We perform the subtraction and assignment in this way to avoid losing precision
        if (InterpolateView)
        {
            relativePosition = (Vector3)(GetInterpolatedPosition() - CameraPositionRef.GetPosition());
        }
        else
        {
            relativePosition = (Vector3)(WrappedBody.GetPosition() - CameraPositionRef.GetPosition());
        }

        var scalingResults = ViewCondenser.GetScaledPosition(relativePosition);

        if (scalingResults.visible)
        {
            transform.position = scalingResults.position;
            transform.localScale = Vector3.one * scalingResults.scale;
        }
        else
        {
            // Hide this object somehow.
            // TODO : this
        }

    }

    protected virtual void OnEnable ()
    {
        if(rigidbody == null)
        {
            rigidbody = GetComponent<Rigidbody>();
        }

        if(WrappedBody == null)
        {
            WrappedBody = GetComponent<WrappedBody>();
        }

        if (CameraPositionRef == null)
        {
            TargetCamera = Camera.main;
            CameraPositionRef = TargetCamera.GetComponent(typeof(ILargePosition)) as ILargePosition;
        }
    }
}
