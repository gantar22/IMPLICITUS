using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Allows you to move this gameObject between two points according to a spline.
 */
public class WalkerFreePoints : MonoBehaviour {

    // Private fields
    private BezierSpline spline;
    private float duration;
    private bool lookForward = false;

    private bool moving = false;
    private float progress;


    // Use a spline to move from point A to point B
    public void moveBySpline(BezierSpline _spline, Vector3 _pointA, Vector3 _pointB, float _duration) {
        spline = Instantiate(_spline.gameObject).GetComponent<BezierSpline>();
        duration = _duration;
        moving = true;
        progress = 0f;

        // Compute some necessary vectors
        Vector3 splineStart = spline.GetPoint(0f);
        Vector3 splineEnd = spline.GetPoint(1f);

        Vector3 desiredAtoB = _pointB - _pointA;
        Vector3 splineAtoB = splineEnd - splineStart;

        // Scale as necessary
        float scale = desiredAtoB.magnitude / splineAtoB.magnitude;
        spline.transform.localScale *= scale;

        // Rotate as necessary
        Vector3 orthogonalAxis = Vector3.Cross(desiredAtoB, splineAtoB);
        const float min_ortho_magnitude = 0.00001f;
        if (orthogonalAxis.magnitude < min_ortho_magnitude) {
            orthogonalAxis = Vector3.up; // TODO - is this the right default? And is this ever being called?
        }
        float angleBetween = Vector3.SignedAngle(splineAtoB, desiredAtoB, orthogonalAxis);

        // TODO - is this supposed to be around spline.transform.position? Need to move to origin first?
        spline.transform.RotateAround(spline.transform.position, orthogonalAxis, angleBetween);

        // Move starting position to pointA
        splineStart = spline.GetPoint(0f);
        spline.transform.position += transform.position - splineStart;
    }

    // Move every frame, if movement is enabled
    private void Update() {
        if (!moving) {
            return;
        }

        progress += Time.deltaTime / duration;
        if (progress > 1f) {
            progress = 1f;
            moving = false;
            Destroy(spline.gameObject);
        }

        Vector3 position = spline.GetPoint(progress);
        transform.localPosition = position;

        if (lookForward) {
            transform.LookAt(position + spline.GetDirection(progress));
        }
    }
}
