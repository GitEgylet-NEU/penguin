using System.Collections;
using System.Linq;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform followTransform;
    public float offset;
    public float xLimit;
    public float smoothing = 5f;

    Coroutine lerpCoroutine;

    void LateUpdate()
    {
        if (followTransform != null)
        {
            // kövesse a followTransform pozícióját, miközben az xLimiten belül marad
            Vector3 moveTo = new Vector3(
                Mathf.Clamp(followTransform.position.x, -xLimit, xLimit),
                transform.position.y,
                followTransform.position.z - offset
                );
            transform.position = Vector3.Lerp(transform.position, moveTo, smoothing * Time.deltaTime);
        }
    }

    public void SetTransform(Transform newTransform, bool lerp)
    {
        if (lerp)
        {
            if (followTransform == null)
            {
                SetTransform(newTransform, false);
                return;
            }

            // fancy lerp cucc ami nem működik úgy ahogy kellene neki
            float startZ = followTransform.position.z;
            followTransform = null;

            IEnumerator Lerp()
            {
                float t = 0;
                while (t <= .5f)
                {
                    if (newTransform == null)
                    {
                        Penguin first = TeamManager.instance.penguins.FirstOrDefault();
                        followTransform = first == null ? null : first.transform;
                        yield break;
                    }
                    transform.position = new Vector3(transform.position.x, transform.position.y, Mathf.Lerp(startZ + t * TeamManager.instance.penguins[0].speed, newTransform.position.z, t * 2f) - offset);
                    t += Time.deltaTime;
                    yield return null;
                }
                if (newTransform == null)
                {
                    Penguin first = TeamManager.instance.penguins.FirstOrDefault();
                    followTransform = first == null ? null : first.transform;
                    yield break;
                }
                followTransform = newTransform;
            }

            if (lerpCoroutine != null) StopCoroutine(lerpCoroutine);
            lerpCoroutine = StartCoroutine(Lerp());
        }
        else
        {
            followTransform = newTransform;
        }
    }
}
