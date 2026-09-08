using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Dialogue : MonoBehaviour
{
    public enum TipoNombre
    {
        NPC,
        PlayerActual
    }

    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(4, 6)]
        public string texto;
        public TipoNombre tipoNombre;
        public string nombreNPC;
    }

    [Header("Diálogo")]
    [SerializeField] private DialogueLine[] dialogueLines;


    [Header("Referencias")]
    [SerializeField] private GameObject dialogueMark;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    
    [Header("Comportamiento al terminar")]
    [SerializeField] private bool desaparecerAlTerminar = false;
    [SerializeField] private string npcID;

    [Header("Sonido")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] sonidosDialogo;

    public float typingTime = 0.0f;

    private bool isPlayerInRange;
    private bool didDialogueStart;
    private bool isTyping;
    private int lineIndex;

    private Animator dialogueMarkAnimator;
    private Animator npcAnimator;

    private Coroutine typingCoroutine;


    void Start()
    {
        dialogueMarkAnimator = dialogueMark.GetComponent<Animator>();
        npcAnimator = GetComponentInChildren<Animator>();

        // Si este NPC ya fue completado, eliminarlo inmediatamente
        if (desaparecerAlTerminar && GameManager.instance != null
            && GameManager.instance.NPCEstaCompletado(npcID))
        {
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (isPlayerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (!didDialogueStart)
            {
                StartDialogue();
                return;
            }

            if (isTyping)
            {
                TerminarLinea();
                return;
            }

            NextDialogueLine();
        }
    }

    public void StartDialogue(bool forzar = false)
    {
        if (forzar) isPlayerInRange = true;

        didDialogueStart = true;
        dialoguePanel.SetActive(true);
        dialogueMark.SetActive(true);
        lineIndex = 0;

        if (audioSource != null && sonidosDialogo != null)
            ReproducirSonido();

        typingCoroutine = StartCoroutine(ShowLine());
    }

    private void NextDialogueLine()
    {
        lineIndex++;

        if (lineIndex < dialogueLines.Length)
        {
            if (audioSource != null && sonidosDialogo != null)
                ReproducirSonido();

            typingCoroutine = StartCoroutine(ShowLine());
        }
        else
        {
            EndDialogue();
        }
    }

    private IEnumerator ShowLine()
    {
        isTyping = true;
        dialogueText.text = string.Empty;
        DialogueLine lineaActual = dialogueLines[lineIndex];

        string nombre = ObtenerNombre(lineaActual);
        string texto = lineaActual.texto.Replace("{nombre}", nombre);

        foreach (char ch in texto)
        {
            dialogueText.text += ch;
            yield return new WaitForSeconds(typingTime);
        }

        isTyping = false;
        typingCoroutine = null;
    }

    private void TerminarLinea()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        DialogueLine lineaActual = dialogueLines[lineIndex];

        string nombre = ObtenerNombre(lineaActual);
        string texto = lineaActual.texto.Replace("{nombre}", nombre);

        dialogueText.text = texto;
        isTyping = false;
    }

    private string ObtenerNombre(DialogueLine linea)
    {
        if (linea.tipoNombre == TipoNombre.NPC)
        {
            return linea.nombreNPC;
        }

        if (CharacterSelectionData.personajeSeleccionado == 0)
        {
            return "Alvin";
        }
        else
        {
            return "Juliana";
        }
    }

    private void EndDialogue()
    {
        didDialogueStart = false;
        dialoguePanel.SetActive(false);
        dialogueMark.SetActive(false);

        if (npcAnimator != null)
            npcAnimator.SetBool("isTalking", false);

        if (desaparecerAlTerminar)
        {
            // Registrar en GameManager
            if (GameManager.instance != null && !string.IsNullOrEmpty(npcID))
                GameManager.instance.MarcarNPCCompletado(npcID);

            if (npcAnimator != null)
                npcAnimator.SetTrigger("vanish");

            isPlayerInRange = false;
            GetComponent<Collider2D>().enabled = false;
        }
        else
        {
            dialogueMark.SetActive(true); // NPCs normales mantienen el ícono
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerInRange = true;

            dialogueMark.SetActive(true);
            dialogueMarkAnimator.Play("67");

            if (npcAnimator != null)
            {
                npcAnimator.SetBool("isTalking", true);
            }
        }
        
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerInRange = false;

            dialogueMark.SetActive(false);
            dialoguePanel.SetActive(false);

            didDialogueStart = false;
            isTyping = false;
            lineIndex = 0;

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }

            if (npcAnimator != null)
            {
                npcAnimator.SetBool("isTalking", false);
            }
        }
    }

    private void ReproducirSonido()
    {
        if (audioSource == null || sonidosDialogo.Length == 0) return;

        if (dialogueLines[lineIndex].tipoNombre != TipoNombre.NPC) return;

        Debug.Log("ReproducirSonido llamado desde: " + gameObject.name);

        AudioClip clip = sonidosDialogo[Random.Range(0, sonidosDialogo.Length)];
        audioSource.PlayOneShot(clip);
    }
}
