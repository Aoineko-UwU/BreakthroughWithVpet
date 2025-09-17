using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Block_Spring : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private bool isActive = false;
    [SerializeField]private float bounceForce = 13f;//������(�����)
    [SerializeField] private float resetTime = 10f; //����ʱ��

    private void Update()
    {
        animator.SetBool("isActive", isActive);
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (isActive) return;

        if (other.collider.CompareTag("Item") || other.collider.CompareTag("Vpet") || other.collider.CompareTag("Enemy"))
        {
            isActive = true;
            other.rigidbody.AddForce(transform.up.normalized * bounceForce * other.rigidbody.mass, ForceMode2D.Impulse);
            AudioManager.Instance.PlaySound3D("spring_active", transform.position);
            Invoke("ResetSpring", resetTime);
        }
    }

    private void ResetSpring()
    {
        isActive = false;
    }

}
