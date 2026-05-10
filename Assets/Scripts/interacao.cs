using System.Collections;
using UnityEngine;

public class interacao : MonoBehaviour
{
   
    public float pushForce = 10f;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        GameObject obj = hit.gameObject;

        switch (obj.tag)
        {
            case "Cubro":
                StartCoroutine(FadeAndDestroy(obj));
                break;

            case "Esfera":
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 dir = hit.moveDirection.normalized;
                    rb.AddForce(dir * pushForce, ForceMode.Impulse);
                }
                break;
        }
    }

   
    IEnumerator FadeAndDestroy(GameObject obj)
    {
        Renderer rend = obj.GetComponent<Renderer>();
        if (rend == null)
        {
            Destroy(obj);
            yield break;
        }

        Material mat = rend.material;
        Color original = mat.color;

        float duration = 1.2f; 
        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, t / duration);

            mat.color = new Color(original.r, original.g, original.b, alpha);

            yield return null;
        }

        Destroy(obj);
    }
}
