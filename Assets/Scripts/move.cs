using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Vuforia;  

public class move : MonoBehaviour
{
    public GameObject model;
    public ObserverBehaviour[] ImageTargets;
    public int currentTarget;
    public float speed = 1.0f;
    public float rotationSpeed = 5.0f;

    [Header("Animator Parameters")] 

    private bool isMoving = false;
    private Animator animator;

    void Start()
    {
        animator = model.GetComponent<Animator>();
    }

    public void moveToNextTarget()
    {
        if (!isMoving)
        {
            StartCoroutine(MoveModel());
        }
    }

    private IEnumerator MoveModel()
    {
        isMoving = true;
        ObserverBehaviour target = GetNextDetectedTarget();

        if (target == null)
        {
            isMoving = false;
            yield break;
        }

        Vector3 startPosition = model.transform.position;
        Vector3 targetPosition = target.transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(
            (targetPosition - startPosition).normalized
        );

        // Disparar el trigger al iniciar
        if (animator != null)
            animator.SetTrigger("walkTrigger");

        float journey = 0;

        while (journey <= 1)
        {
            journey += Time.deltaTime * speed;

            model.transform.position = Vector3.Lerp(startPosition, targetPosition, journey);

            // Rotar hacia el destino durante el recorrido
            model.transform.rotation = Quaternion.Slerp(
                model.transform.rotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );

            yield return null;
        }

        model.transform.rotation = target.transform.rotation * Quaternion.Euler(0, 180, 0); // Ajuste para que mire hacia el frente del target

        // Disparar idle UNA SOLA VEZ al terminar
        if (animator != null)
            animator.SetTrigger("idleTrigger");

        currentTarget = (currentTarget + 1) % ImageTargets.Length;
        isMoving = false;
    }

    private ObserverBehaviour GetNextDetectedTarget()
    {
        foreach (ObserverBehaviour target in ImageTargets)
        {
            if (target != null && (target.TargetStatus.Status == Status.TRACKED ||
                target.TargetStatus.Status == Status.EXTENDED_TRACKED))
            {
                return target;
            }
        }
        return null;
    }

   
}