using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Input KeyCodes")]
    [SerializeField] private KeyCode keyCodeRun = KeyCode.LeftShift; //왼쪽 쉬프트키는 달리기 키
    [SerializeField] private KeyCode keyCodeJump = KeyCode.Space;    //스페이스는 점프 키
    [SerializeField] private KeyCode keyCodeReload = KeyCode.R;     //탄 재장전 키

    [Header("Audio Clips")]
    [SerializeField] private AudioClip audioClipWalk;
    [SerializeField] private AudioClip audioClipRun;

    private RotateToMouse rotateToMouse;            //마우스 이동으로 카메라 회전
    private MovementCharacterController movement;   //키보드 입력으로 플레이어 이동, 점프
    private Status status;                          //이동속도 등의 플레이어 정보
    private PlayerAnimatorController animator;      //애니메이션 재생 제어
    private AudioSource audioSource;                //사운드 재생 제어
    private WeaponAssaultRifle weapon;              //무기를 이용한 공격 제어
    public EnemyMemoryPool enemyMemoryPool;

    private void Awake()
    {
        //마우스 커서 안보이게 설정, 현재 위치에 고정
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        //컴포넌트 정보를 불러와 변수에 저장
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
        float x = Input.GetAxisRaw("Horizontal"); //키보드 좌우입력
        float z = Input.GetAxisRaw("Vertical");   //키보드 상하입력

        //이동중 일 때(걷기 or 뛰기)
        if (x != 0 || z != 0)
        {
            bool isRun = false;

            //옆이나 뒤로 이동할 때는 달릴 수 없음
            if (z > 0) isRun = Input.GetKey(keyCodeRun);

            movement.MoveSpeed = isRun == true ? status.RunSpeed : status.WalkSpeed;
            animator.MoveSpeed = isRun == true ? 1 : 0.5f;
            audioSource.clip = isRun == true ? audioClipRun : audioClipWalk;

            //방향키 입력 여부는 매 프레임 확인
            //때문에 재생중에는 다시 재생하지 않도록 isPlaying으로 체크하여 재생
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

        movement.MoveTo(new Vector3(x, 0, z)); //받아온 입력들을 moveTo의 매개변수로 사용
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
            Time.timeScale = 0; // 게임 멈춤
            Cursor.visible = true; // 마우스 커서 보이게 함
            Cursor.lockState = CursorLockMode.None; // 화면 중앙에서 마우스 풀어줌
            enemyMemoryPool.panelGameOver.SetActive(true);
        }
    }
}
