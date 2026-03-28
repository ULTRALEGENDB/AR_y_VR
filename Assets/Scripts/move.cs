using UnityEngine;
using System.Collections;
using Vuforia;
using UnityEngine.SceneManagement;
public class move : MonoBehaviour
{
    public GameObject model; // El mago que se mueve entre targets
    public ObserverBehaviour[] ImageTargets;
    public int currentTarget;
    public float speed = 1.0f;
    public float rotationSpeed = 5.0f;
    public ObserverBehaviour magoTarget; // Target específico para el mago

    private bool[] targetsVisited;      // Para el movimiento del mago
    private bool[] objectsAssigned;     // Para asignar objetos una sola vez por target
    public GameObject[] targetObjects;  // Objetos ya existentes en escena, en el orden en que deben asignarse
    private int currentObjectIndex = 0;
    private GameObject[] objectForTarget; // Almacena el objeto asociado a cada target

    [Header("Animator Parameters")]
    private bool isMoving = false;
    private Animator animator;
    public int skipFirstTargets = 1;
    void Start()
    {
        animator = model.GetComponent<Animator>();
        targetsVisited = new bool[ImageTargets.Length];
        objectsAssigned = new bool[ImageTargets.Length];
        objectForTarget = new GameObject[ImageTargets.Length];
    }

    void Update()
    {
        // Revisar cada Image Target para asignar el siguiente objeto en el orden de detección
        for (int i = 0; i < ImageTargets.Length; i++)
        {
            ObserverBehaviour target = ImageTargets[i];
            if (target != null && !objectsAssigned[i] && IsTargetTracked(target))
            {
                // Asignar el siguiente objeto a este target
                if (currentObjectIndex < targetObjects.Length)
                {
                    GameObject obj = targetObjects[currentObjectIndex];
                    if (obj != null)
                    {
                        // Posicionar el objeto exactamente en el target y hacerlo hijo temporal
                        obj.transform.SetParent(target.transform, false);
                        obj.transform.localPosition = Vector3.zero;
                        obj.transform.localRotation = Quaternion.identity;
                        obj.SetActive(true); // Asegurar que esté visible

                        // Guardar referencia
                        objectForTarget[i] = obj;
                        objectsAssigned[i] = true;
                        currentObjectIndex++;
                        Debug.Log($"Objeto {obj.name} apareció en target {target.name}");
                    }
                    else
                    {
                        Debug.LogWarning($"El objeto en índice {currentObjectIndex} es null.");
                    }
                }
                else
                {
                    Debug.LogWarning("No hay más objetos disponibles en targetObjects.");
                }
            }
        }
    }

    private bool IsTargetTracked(ObserverBehaviour target)
    {
        return target.TargetStatus.Status == Status.TRACKED ||
               target.TargetStatus.Status == Status.EXTENDED_TRACKED;
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
        Quaternion targetRotation = Quaternion.LookRotation((targetPosition - startPosition).normalized);

        if (animator != null)
            animator.SetTrigger("walkTrigger");

        float journey = 0;

        while (journey <= 1)
        {
            journey += Time.deltaTime * speed;
            model.transform.position = Vector3.Lerp(startPosition, targetPosition, journey);
            model.transform.rotation = Quaternion.Slerp(model.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            yield return null;
        }

        model.transform.rotation = target.transform.rotation * Quaternion.Euler(0, 180, 0);
        model.transform.SetParent(target.transform, true);

        if (animator != null)
            animator.SetTrigger("idleTrigger");

        int targetIndex = System.Array.IndexOf(ImageTargets, target);
        if (targetIndex >= 0)
        {
            targetsVisited[targetIndex] = true;

            // Recoger el objeto asociado a este target: hacerlo hijo del modelo
            if (objectForTarget[targetIndex] != null)
            {
                GameObject obj = objectForTarget[targetIndex];
                obj.transform.SetParent(model.transform, true); // Mantener posición
                obj.transform.localPosition = new Vector3(-0.5f, 0f, 0f);  
                obj.SetActive(false); // Desaparece al ser recogido
                Debug.Log($"Objeto {obj.name} recogido por {model.name}");
            }
        }

        currentTarget = (currentTarget + 1) % ImageTargets.Length;
        isMoving = false;
    }

    private ObserverBehaviour GetNextDetectedTarget()
    {
        int start = currentTarget;
        int i = start;

        do
        {
            ObserverBehaviour target = ImageTargets[i];
            if (target != null && !targetsVisited[i] && IsTargetTracked(target))
            {
                return target;
            }
            i = (i + 1) % ImageTargets.Length;
        } while (i != start);

        return null;
    }
    public void RestartGame()
{
    SceneManager.LoadScene("escena1");
}
}