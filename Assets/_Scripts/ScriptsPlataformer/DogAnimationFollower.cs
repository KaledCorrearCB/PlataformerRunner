using UnityEngine;

public class DogAnimationFollower : MonoBehaviour
{
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Animator dogAnimator;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int JumpHash = Animator.StringToHash("Jump");

    private bool wasJumpTriggered = false;

    void Update()
    {
        // Speed es lo que controla el Blend Tree del jugador
        dogAnimator.SetFloat(SpeedHash, playerAnimator.GetFloat(SpeedHash));
        dogAnimator.SetBool(IsGroundedHash, playerAnimator.GetBool(IsGroundedHash));

        SyncJumpTrigger();
    }

    void SyncJumpTrigger()
    {
        bool isPlayerJumping = playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Jump");

        if (isPlayerJumping && !wasJumpTriggered)
        {
            dogAnimator.SetTrigger(JumpHash);
            wasJumpTriggered = true;
        }
        else if (!isPlayerJumping)
        {
            wasJumpTriggered = false;
        }
    }
}