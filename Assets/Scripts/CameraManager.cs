using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

  public Transform target;
  public float dampTime = 0.15f;
  private Vector3 velocity = Vector3.zero;

  void Update()
  {
    if (target)
    {
      Vector3 point = GetComponent<Camera>().WorldToViewportPoint(target.position);
      Vector3 delta = target.position - GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z));
      Vector3 destination = transform.position + delta;
      transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);
    }
  }

  internal void SetTarget(Transform playerTransform)
  {
    target = playerTransform;
  }
}
