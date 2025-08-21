using UnityEngine;

public class PlayerController : BaseController
{
    private Camera playerCamera;

    private GameManager gameManager;

    public void Init(GameManager gameManager)
    {
        this.gameManager = gameManager;
        playerCamera = Camera.main;
    }

    protected override void HandleAction()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertial = Input.GetAxisRaw("Vertical");
        movementDirection = new Vector2(horizontal, vertial).normalized;

        Vector2 mousePosition = Input.mousePosition;
        Vector2 worldPos = playerCamera.ScreenToWorldPoint(mousePosition);
        lookDirection = (worldPos - (Vector2)transform.position);

        if (lookDirection.magnitude < 0.9f)
        {
            lookDirection = Vector2.zero;
        }
        else
        {
            lookDirection = lookDirection.normalized;
        }

        isAttacking = Input.GetMouseButton(0);
    }
}
