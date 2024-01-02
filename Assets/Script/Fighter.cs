using UnityEngine;
using UnityEngine.UI;

public enum ActiveDirection
{
    NONE = 0,
    Up = 1,
    Left = 2,
    Right = 3
}

public static class ScoreTable
{
    public static float attackHitted = 100.0f;
    public static float attackBlocked = 150.0f;
    public static float partialBlock = 25.0f;
    public static float defeatEnemy = 500.0f;
    public static float changeDir = 50.0f;

    public static float partialHitted = -25.0f;
    public static float oponentBlocked = -50.0f;
    public static float getHitted = -100.0f;
    public static float passive = - 0.0f;
    public static float dying = - 500.0f;
    public static float blockWrongWay = - 5.0f;
}

public class Fighter : MonoBehaviour
{
    public bool fightIsOn = false;

    public float score;

    private int life;
    public int Life { get { return life; } }

    [SerializeField] SpriteRenderer UpSprite;
    [SerializeField] SpriteRenderer LeftSprite;
    [SerializeField] SpriteRenderer RightSprite;
    [SerializeField] SpriteRenderer ShieldSprite;
    [SerializeField] SpriteRenderer SkullSprite;

    [SerializeField] Fighter Oponent;

    [SerializeField] Slider lifeSlider;

    [SerializeField] public float attackCoolDown = 3;
    [SerializeField] float attackTimer;
    public float attackCDRemain { get { return attackTimer; } }

    [SerializeField] float blockCoolDown = 0.3f;
    [SerializeField] float blockTimer;

    [SerializeField] float attackSwingTime = 0.8f;
    [SerializeField] float swingTimer;

    private bool defeated;

    public bool OponentDefeated { get { return Oponent.defeated; } }

    [SerializeField] private ActiveDirection activeDirection;
    public ActiveDirection ActiveDir { get { return activeDirection; } }

    private bool isBlocking;
    public bool IsBlocking {  get { return isBlocking; } }

    private bool isAttacking;
    public bool IsAttacking { get { return isAttacking; } }

    public Fighter GetOpponent()
    {
        return Oponent;
    }

    #region scoreFunctions

    private void GetHitted()
    {
        Oponent.score += ScoreTable.attackHitted;
        score += ScoreTable.getHitted;
    }

    private void AttackBlocked()
    {
        score += ScoreTable.attackBlocked;
        Oponent.score += ScoreTable.oponentBlocked;

    }
    private void PartialBlock()
    {
        score += ScoreTable.partialBlock;
        Oponent.score += ScoreTable.partialHitted;
    }

    private void KillOponent()
    {
        score += ScoreTable.defeatEnemy;
    }

    private void GetKilled()
    {
        score += ScoreTable.dying;
    }

    private void BlockMalus()
    {
        score += ScoreTable.passive;
        if (!isBlocking)
            return;
        
        if (Oponent.ActiveDir != activeDirection)
            score += ScoreTable.blockWrongWay;
    }

    #endregion

    // Return true if the attack kill the fighter
    public bool GetAttacked(ActiveDirection dir)
    {
        if (dir == activeDirection) 
        { 
            if (isBlocking)
            {
                AttackBlocked();
                return CheckDefeat();
            }

            life -= 3;
            PartialBlock();
            return CheckDefeat();
        }

        life -= 10;
        GetHitted();

        return CheckDefeat();
    }

    public void Attack()
    {
        if (IsBlocking || attackTimer > 0 || Oponent == null)
            return;

        isAttacking = true;
        swingTimer = attackSwingTime;
        attackTimer = attackCoolDown;
    }

    public void StartBlock()
    {
        ShieldSprite.enabled = true;
        isBlocking = true;
    }

    public void StopBlock()
    {
        if (!isBlocking)
            return;

        ShieldSprite.enabled = false;
        isBlocking = false;
        blockTimer = blockCoolDown;
    }

    public void ResetAttack()
    {
        isAttacking = false; 
        swingTimer = 0;
    }

    public void SetActiveDir(ActiveDirection dir)
    {
        if (dir == activeDirection)
            return;

        if (IsAttacking)
            return;

        activeDirection = dir;

        UpSprite.color = Color.black;
        LeftSprite.color = Color.black;
        RightSprite.color = Color.black;

        switch (activeDirection) 
        { 
            case ActiveDirection.Up:
                UpSprite.color = Color.white;
                break;
            case ActiveDirection.Left:
                LeftSprite.color = Color.white;
                break;
            case ActiveDirection.Right:
                RightSprite.color = Color.white;
                break;
        }
    }

    public void ForceActiveDir(ActiveDirection dir)
    {
        activeDirection = dir;

        UpSprite.color = Color.black;
        LeftSprite.color = Color.black;
        RightSprite.color = Color.black;

        switch (activeDirection)
        {
            case ActiveDirection.Up:
                UpSprite.color = Color.white;
                break;
            case ActiveDirection.Left:
                LeftSprite.color = Color.white;
                break;
            case ActiveDirection.Right:
                RightSprite.color = Color.white;
                break;
        }
    }

    private bool CheckDefeat()
    {
        if (life <= 0)
        {
            defeated = true;
            UpSprite.color = Color.black;
            LeftSprite.color = Color.black;
            RightSprite.color = Color.black;
            StopBlock();
            SkullSprite.enabled = true;
            ShieldSprite.enabled = false;
            GetKilled();

            return true;
        }    

        return false;
    }

    public void ResetFighter()
    {
        UpSprite.color = Color.black;
        LeftSprite.color = Color.black;
        RightSprite.color = Color.black;
        ShieldSprite.enabled = false;
        SkullSprite.enabled = false;
        isAttacking = false;
        StopBlock();

        life = 100;
        attackTimer = 0;
        defeated = false;

        ActiveDirection randDir = (ActiveDirection)UnityEngine.Random.Range(1, 4);
        ForceActiveDir(randDir);
    }

    public void ResetAsDummy()
    {
        UpSprite.color = Color.black;
        LeftSprite.color = Color.black;
        RightSprite.color = Color.black;
        ShieldSprite.enabled = false;
        SkullSprite.enabled = false;
        isAttacking = false;
        ShieldSprite.enabled = false;
        isBlocking = false;
        life = 100;
        attackTimer = 0;
        defeated = false;
        blockTimer = 0;
        swingTimer = 0;

        ActiveDirection randDir = (ActiveDirection)UnityEngine.Random.Range(1, 4);
        ForceActiveDir(randDir);
    }

    void Awake()
    {
        ResetFighter();
    }

    // Update is called once per frame
    void Update()
    {
        if (lifeSlider != null)
            lifeSlider.value = life;

        if (defeated || OponentDefeated || !fightIsOn)
            return;

        BlockMalus();

        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;

        if (blockTimer > 0 && !isBlocking)
            blockTimer -= Time.deltaTime;

        if (isAttacking)
        {
            swingTimer -= Time.deltaTime;

            if (swingTimer < 0)
            {
                if (Oponent.GetAttacked(activeDirection))
                    KillOponent();

                isAttacking = false;
            }
        }
    }
}
