using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private Animator _animator;

    private Rigidbody2D rb;
    private Vector2 inputDirection;
    private PlantingScript plantingScript;
    private int collectedUpgradedCount = 0;
    private int targetPlantCount = 9; // default fallback

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputDirection = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        plantingScript = GetComponent<PlantingScript>();
    }

    void Start()
    {
    }

    void Update()
    {
        if (Keyboard.current == null)
        {
            inputDirection = Vector2.zero;
            return;
        }
        

        float x = 0f;
        float y = 0f;
        _animator.SetBool("isIdle", true);
        if (Keyboard.current.aKey.isPressed) {x -= 1f;
        _animator.SetBool("isIdle", false);
        _animator.SetFloat("xDirection", -1f);
        _animator.SetFloat("yDirection", 0f);
        Debug.Log("x: " + x);
        Debug.Log("y: " + y);
        Debug.Log("Button A Pressed");
        /* Flip the player horizontally */
          /* gameObject.GetComponent<SpriteRenderer>().flipX = true; */
        gameObject.GetComponent<SpriteRenderer>().flipX = true;
        }
        if (Keyboard.current.dKey.isPressed) {x += 1f;
        _animator.SetBool("isIdle", false);
        _animator.SetFloat("xDirection", 1f);
        _animator.SetFloat("yDirection", 0f);
        Debug.Log("x: " + x);
        Debug.Log("y: " + y);
        Debug.Log("Button D Pressed");
        gameObject.GetComponent<SpriteRenderer>().flipX = false;
        }
        if (Keyboard.current.sKey.isPressed) {y -= 1f;
        _animator.SetBool("isIdle", false);
        _animator.SetFloat("xDirection", 0f);
        _animator.SetFloat("yDirection", -1f);
        Debug.Log("x: " + x);
        Debug.Log("y: " + y);
        Debug.Log("Button S Pressed");
        gameObject.GetComponent<SpriteRenderer>().flipX = false;
        }
        if (Keyboard.current.wKey.isPressed) {y += 1f;
        _animator.SetBool("isIdle", false);
        _animator.SetFloat("xDirection", 0f);
        _animator.SetFloat("yDirection", 1f);
        Debug.Log("x: " + x);
        Debug.Log("y: " + y);
        Debug.Log("Button W Pressed");
        gameObject.GetComponent<SpriteRenderer>().flipX = false;
        }

        inputDirection = new Vector2(x, y);

        if (inputDirection.sqrMagnitude > 1f)
        {
            inputDirection = inputDirection.normalized;
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = inputDirection * moveSpeed;
        if (plantingScript != null && plantingScript.TryCollectUpgradedUnderPosition(transform.position))
        {
            collectedUpgradedCount++;
            // Refresh target from weather/timer at the moment of collection to avoid init race
            targetPlantCount = WeatherGameConfig.HasSelection ? WeatherGameConfig.TargetPlantCount : TimerScript.TargetPlantCount;
            if (collectedUpgradedCount >= Mathf.Max(1, targetPlantCount))
            {
                SceneManager.LoadSceneAsync(3);
            }
        }
    }
}
