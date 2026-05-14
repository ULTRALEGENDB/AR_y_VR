using System.Collections;
using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public GameObject model;
    public GameObject model2;
    public GameObject rod;
    public GameObject Planito;
    private Color color;
    private Color color2;
    public Material colorMaterial;
    public Material colorMaterial2;
    public Material noShaderMaterial;
    public Material noShaderMaterial2;
    public Material colorMaterialAccesories;
    private bool hasRod = false;
    private bool hasModel2 = false;
    private bool readySizeEvent = false;
    private Animator animator;
    private Animator animator2;
    private bool isDancing = false;
    private bool alreadyMoved = false;

    public ParticleSystem summonParticles;

    void Start()
    {
        animator = model.GetComponent<Animator>();
        animator2 = model2.GetComponent<Animator>();
        model2.SetActive(false);
    }

    // Cambiar color
    public void ChangeColor_BTN()
    {
            Color randomColor = Random.ColorHSV();
            colorMaterial.color = randomColor;
            colorMaterialAccesories.color = randomColor;
    }

    public void Original_Color_BTN()
    {
        colorMaterial.color = Color.white;
        colorMaterial2.color = Color.blueViolet;
        colorMaterialAccesories.color = Color.white;
    }

    // Animación de baile (esta sí puede quedarse con bool porque es continua)
    public void Bailesito_BTN()
    {
        if (animator != null)
        {
            isDancing = !isDancing;
            animator.SetBool("isDancing", isDancing);
            animator2.SetBool("isDancing", isDancing);
        }
    }

    // Animación de defensa (Trigger)
    public void Defender_BTN()
    {
        if (animator != null)
        {
            animator.SetTrigger("Defend");
        }
    }

    // Animación de varita (Trigger)
    public void Varita_BTN()
    {
        if (animator != null)
        {
            animator.SetTrigger("Rod");

            if(!hasRod)    // activar la varita
            {
                rod.SetActive(true);
                hasRod = true;
            }
            else    // desactivar la varita
            {
                rod.SetActive(false);
                hasRod = false;
            }
        }
    }
    public void Summon_BTN()
    {
        if (animator2 != null)
        {
            if(!hasModel2)    // activar el modelo 2
            {
                model2.SetActive(true);
                hasModel2 = true;
                animator2.SetTrigger("summon");
                if(summonParticles != null)
                {
                    summonParticles.Stop();
                    summonParticles.Clear();                    
                    summonParticles.Play();
                }
            }
            else    // desactivar el modelo 2
            {
                model2.SetActive(false);
                hasModel2 = false;
                if(summonParticles != null)
                {
                    summonParticles.Stop();
                    summonParticles.Clear();
                    summonParticles.Play();
                }                
            }
        }
    }
        public void ChangeColor2_BTN()
    {
            Color randomColor = Random.ColorHSV();
            colorMaterial2.color = randomColor;
    }
        public void mover()
    {
        if (!alreadyMoved)
        {
            StartCoroutine(MoveModel());
        }
    }
        IEnumerator MoveModel()
    {
        alreadyMoved = true;
        // Activar animación
        animator.SetTrigger("walkTrigger");

        Vector3 startPos = model.transform.position;
        Vector3 targetPos = startPos + new Vector3(-1f, 0f, 0f);
        Vector3 startSize = Planito.transform.localScale;
        Vector3 targetSize = startSize * 0; // Escalar a 0 para desaparecer planito de a poquito
        float duration = 1.0f;
        float elapsed = 0f;
        float durationSize = 0.5f; // Duración para escalar el planito
        float elapsedSize = 0f;
        //la escala de planito a 0 debe ser despues de que el modelo llegue a su destino, así que hacemos un segundo while para eso
        while (elapsed < duration)
        {
            model.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
             // Activar evento de tamaño cuando el modelo llegue a su destino
            yield return null;
        }
        readySizeEvent = true;
        model.transform.position = targetPos;
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
           foreach (Material m in rend.materials)
         {
             m.SetInt("_StencilComp",
             (int)UnityEngine.Rendering.CompareFunction.Always);
         }
        }
        if (readySizeEvent == true)
        animator.SetTrigger("idleTrigger");
        {
            while (elapsedSize < durationSize)
            {
                Planito.transform.localScale = Vector3.Lerp(startSize, targetSize, elapsedSize / durationSize);
                elapsedSize += Time.deltaTime;
                yield return null;
            }
        }
    }
}