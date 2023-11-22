using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Input KeyCodes")]
    [SerializeField] private KeyCode keyCodeRun = KeyCode.LeftShift; //���� ����ƮŰ�� �޸��� Ű
    [SerializeField] private KeyCode keyCodeJump = KeyCode.Space;    //�����̽��� ���� Ű
    [SerializeField] private KeyCode keyCodeReload = KeyCode.R;     //ź ������ Ű

    [Header("Audio Clips")]
    [SerializeField] private AudioClip audioClipWalk;
    [SerializeField] private AudioClip audioClipRun;

    private RotateToMouse rotateToMouse;            //���콺 �̵����� ī�޶� ȸ��
    private MovementCharacterController movement;   //Ű���� �Է����� �÷��̾� �̵�, ����
    private Status status;                          //�̵��ӵ� ���� �÷��̾� ����
    private PlayerAnimatorController animator;      //�ִϸ��̼� ��� ����
    private AudioSource audioSource;                //���� ��� ����
    private WeaponAssaultRifle weapon;              //���⸦ �̿��� ���� ����
    public EnemyMemoryPool enemyMemoryPool;

    private void Awake()
    {
        //���콺 Ŀ�� �Ⱥ��̰� ����, ���� ��ġ�� ����
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        //������Ʈ ������ �ҷ��� ������ ����
        rotateToMouse = GetComponent<RotateToMouse>();
        movement = GetComponent<MovementCharacterController>();
        status = GetComponent<Status>();
        animator = GetComponent<PlayerAnimatorController>();
        audioSource = GetComponent<AudioSource>();
        weapon = GetComponentInChildren<WeaponAssaultRifle>();
    }

    private void Update()
    {
        UpdateRotate();
        UpdateMove();
        UpdateJump();
        UpdateWeaponAction();
    }

    private void UpdateRotate()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        rotateToMouse.UpdateRotate(mouseX, mouseY);
    }

    private void UpdateMove()
    {
        float x = Input.GetAxisRaw("Horizontal"); //Ű���� �¿��Է�
        float z = Input.GetAxisRaw("Vertical");   //Ű���� �����Է�

        //�̵��� �� ��(�ȱ� or �ٱ�)
        if (x != 0 || z != 0)
        {
            bool isRun = false;

            //���̳� �ڷ� �̵��� ���� �޸� �� ����
            if (z > 0) isRun = Input.GetKey(keyCodeRun);

            movement.MoveSpeed = isRun == true ? status.RunSpeed : status.WalkSpeed;
            animator.MoveSpeed = isRun == true ? 1 : 0.5f;
            audioSource.clip = isRun == true ? audioClipRun : audioClipWalk;

            //����Ű �Է� ���δ� �� ������ Ȯ��
            //������ ����߿��� �ٽ� ������� �ʵ��� isPlaying���� üũ�Ͽ� ���
            if (audioSource.isPlaying == false)
            {
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else
        {
            movement.MoveSpeed = 0;
            animator.MoveSpeed = 0;

            if (audioSource.isPlaying == true)
            {
                audioSource.Stop();
            }
        }

        movement.MoveTo(new Vector3(x, 0, z)); //�޾ƿ� �Էµ��� moveTo�� �Ű������� ���
    }

    private void UpdateJump()
    {
        if (Input.GetKeyDown(keyCodeJump))
        {
            movement.Jump();
        }
    }

    private void UpdateWeaponAction()
    {
        if (Input.GetMouseButtonDown(0))
        {
            weapon.StartWeaponAction();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            weapon.StopWeaponAction();
        }

        if (Input.GetMouseButtonDown(1))
        {
            weapon.StartWeaponAction(1);
        }
        else if (Input.GetMouseButtonUp(1))
        {
            weapon.StopWeaponAction(1);
        }

        if (Input.GetKeyDown(keyCodeReload))
        {
            weapon.StartReload();
        }
    }

    public void TakeDamage(int damage)
    {
        bool isDie = status.DecreaseHP(damage);

        if (isDie)
        {
            Debug.Log("GameOver");
            Time.timeScale = 0; // ���� ����
            Cursor.visible = true; // ���콺 Ŀ�� ���̰� ��
            Cursor.lockState = CursorLockMode.None; // ȭ�� �߾ӿ��� ���콺 Ǯ����
            enemyMemoryPool.panelGameOver.SetActive(true);
        }
    }
}
