using UnityEngine;
using TMPro;
using static Unity.Mathematics.math;

/// <summary>
/// 
/// Behavior: 
///     Priority: don't fall off -> don't collide -> go towards player (only if aggro)
/// Aggro description:
///     Sends variable # of rays of variable distance. When it reaches the target (player), aggro for all cars for variable direction.
///     
/// The reason why not falling off is a bigger priority than colliding is because falling off is irreversible but bumping into something doesn't really matter
/// </summary>
public class SelfDriveCarBehavior : MonoBehaviour
{
    [SerializeField] private int _numDetectorRays;
    [SerializeField] private float _detectDist;
    [SerializeField] private float _horMoveForceFactor = 1f;
    [SerializeField] private GameObject _canvas;
    [SerializeField] private TextMeshProUGUI _textPrefab;

    private GameObject target;
    private Rigidbody2D rb;

    
    public static float aggroDuration = 5f;
    private static float timeTargetWasSeen = -aggroDuration - 1;

    void Start()
    {
        target = GameObject.FindGameObjectsWithTag("Player")[0];

        rb = GetComponent<Rigidbody2D>();

        _canvas = GameObject.FindGameObjectsWithTag("Canvas")[0];
    }

    void Update()
    {
        #region detect target
        Vector2 rayOrigin = transform.position + 2.1f * ScaledRight;
        for (int i = 0; i < _numDetectorRays; i++)
        {
            Vector2 rayDirection = ScaledRight + (transform.up * remap(0, _numDetectorRays - 1, -0.5f, 2, i));
            RaycastHit2D raycast = Physics2D.Raycast(rayOrigin, rayDirection, _detectDist);

            if (raycast.collider != null && raycast.collider.gameObject.transform.root == target.transform.root)
            {
                // target acquired
                UpdateTimeTargetWasSeen();
            }
        }
        #endregion detect target


        #region set direction
        if (IsAggro())
        {
            if (Random.Range(0, 40) == 0)
            {
                if ((transform.position.x < target.transform.position.x) != (transform.localScale.x == 1))
                {
                    // if target right and face left
                    // if target left and face right
                    ChangeDirection();
                }
            }
        }

        if (IsThereObstacle()) ChangeDirection();

        // check if there is ground forwards
        if (!IsGroundOnRight())
        {
            ChangeDirection();
        }
        #endregion set direction

        // move right but actually forward 
        rb.AddForce(ScaledRight * _horMoveForceFactor);
    }

    Vector3 ScaledRight
    {
        // I think transform.right only takes rotation into consideration
        // this is needed because I flip horizontally by negating localScale.x 
        // I could've done y rot as well, but that didn't work
        get { return transform.right * transform.localScale.x; }
    }

    bool IsThereObstacle()
    {
        Vector3 rayOrigin = transform.position + ScaledRight * 2f; // Offset the ray from the object
        Vector3 rayDirection = ScaledRight * 0.5f;
        RaycastHit2D raycast = Physics2D.Raycast(rayOrigin, rayDirection, 0.5f);
        Debug.DrawRay(rayOrigin, rayDirection, Color.red);
        return (raycast.collider != null) && (raycast.collider != target);
    }

    bool IsGroundOnRight()
    {
        Vector3 rayOrigin = transform.position + ScaledRight * 1.5f + transform.up * (-1f); // Offset the ray from the object
        Vector3 rayDirection = Vector3.down * 0.1f;
        RaycastHit2D raycast = Physics2D.Raycast(rayOrigin, rayDirection, 0.1f);
        Debug.DrawRay(rayOrigin, rayDirection, Color.red);
        return raycast.collider != null;
    }

    void ChangeDirection()
    {
        Vector3 velocity = rb.velocity;
        velocity.x = 0f;
        rb.velocity = velocity;


        transform.localScale = new Vector3(-transform.localScale.x, 1f, 1f);
    }

    static bool IsAggro()
    {
        float currentTime = Time.time;
        float aggroEndTime = timeTargetWasSeen + aggroDuration;

        return currentTime <= aggroEndTime;
    }

    void UpdateTimeTargetWasSeen()
    {
        if (!IsAggro())
        {
            // previously out of aggro time
            TextMeshProUGUI newText = Instantiate(_textPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);

            newText.text = "?!";
            newText.color = Color.red;
            // newText.transform.position = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f);

            newText.transform.SetParent(_canvas.transform, false);

            Destroy(newText.gameObject, 1f);
        }

        timeTargetWasSeen = Time.time;
    }
}
