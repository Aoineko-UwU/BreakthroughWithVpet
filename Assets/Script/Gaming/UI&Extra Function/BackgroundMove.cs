using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMove : MonoBehaviour
{

    private float speed = 0.6f;
    Vector2 initPos;

    private void Start()
    {
        initPos = transform.localPosition;
    }

    private void Update()
    {
        if (transform.localPosition.x <= 0)
            transform.localPosition = initPos;

        transform.Translate(Vector2.left * speed * Time.deltaTime);
    }
}
