using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class Shoot : MonoBehaviour
{
    // Start is called before the first frame update
    private PlayerInput playerControls;
    private InputAction shoot;
    private InputAction doReload;
    private Movement playerMovementScript;
    [SerializeField] private Camera aimCam;
    [SerializeField] private Transform aimTransform;
    [SerializeField] private GameObject bullet;
    [SerializeField] private PlayerStats statsScript;
    [SerializeField] private PlayerManager pM;
    private int theBullet = 1;
    private Vector3 destination;
    private float projectileSpeed = 60;
    private float bulletYRotation = 0.0f;
    public float ammoCount = 15;
    private float reloadTime = 2.0f;
    private float reloadTimer = 0.0f;
    private bool reloading = false;
    private void Awake() => playerControls = new PlayerInput();

    private void Start()
    {
        playerMovementScript = GetComponent<Movement>();
    }
    private void OnEnable()
    {
       
            shoot = playerControls.Player.Fire;
            shoot.performed += FireBullet;
            shoot.Enable();

            doReload = playerControls.Player.Reload;
            doReload.performed += ActiveReload;
            doReload.Enable();
        
    }
    private void OnDisable()
    {
        doReload.Disable();
        shoot.Disable();
    }
    private void FireBullet(InputAction.CallbackContext context)
    {
        if(statsScript != null && !statsScript.isRespawning && !statsScript.gameOver)
        {
            Ray ray = aimCam.ViewportPointToRay(new Vector3(0.5f,0.5f,0f));
            RaycastHit hit;

            if(Physics.Raycast(ray, out hit)){
                destination = hit.point;
            }
            else{
                destination = ray.GetPoint(1000);
            }
            
            if(ammoCount > 0 && !reloading)
            {
                InstantiableBullet();
                ammoCount -= 1;
            }
            else 
            {
                reloading = true;
            }
        }
    }
    private void Reload()
    {
        reloadTimer += Time.deltaTime;
        if(reloadTimer >= reloadTime)
        {
            reloading = false;
            reloadTimer = 0;
            ammoCount = 15;
        }
    }
    private void ActiveReload(InputAction.CallbackContext context)
    {
        if(ammoCount < 15)
        {
            reloading = true;
        }
    }
    private void InstantiableBullet()
    {
        Quaternion R = Quaternion.identity;
        R = aimCam.transform.rotation;
        var projectileObj = Instantiate(bullet, aimTransform.position, R) as GameObject;  
        Vector3 target = (destination - aimTransform.position).normalized * projectileSpeed;
        projectileObj.GetComponent<Rigidbody>().velocity = target;
        projectileObj.GetComponent<BulletInfo>().playerId = pM.id;
        projectileObj.GetComponent<BulletInfo>().bulletId = theBullet;
        ClientSend.bulletSpawned(pM.id, theBullet, aimTransform.position, aimTransform.rotation, target);
        theBullet += 1;
    }
    private void Update()
    {
        if(statsScript.isRespawning)
        {
            ammoCount = 15;
        }
        if(reloading)
        {
            Reload();
        }
        if(theBullet >= 100)
        {
            theBullet = 1;
        }
    }
    public float GetAmmoCount()
    {
        return ammoCount;
    }
    public bool GetReloading()
    {
        return reloading;
    }
    public float GetReloadTimeRemaining()
    {
        return reloadTime - reloadTimer;
    }
}
