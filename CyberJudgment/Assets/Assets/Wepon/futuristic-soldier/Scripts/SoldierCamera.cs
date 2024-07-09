using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierCamera : MonoBehaviour
{

	public Transform target;
	
	public PositionSettings position = new PositionSettings();
	public OrbitSettings orbit = new OrbitSettings();
	[HideInInspector]
	public InputSettings input = new InputSettings();
	public CollisionHandler collision = new CollisionHandler();

	Vector3 targetPos = Vector3.zero;
	Vector3 destination = Vector3.zero;
	Vector3 adjustedDestination = Vector3.zero;
	Vector3 camVel = Vector3.zero;
	float vOrbitInput, hOrbitInput, zoomInput, hOrbitSnapInput;
    
    void Start(){
    	Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
		position.applyScale(target.GetComponent<SoldierController>().Scale());
        MoveToTarget();
        collision.Initialize(Camera.main);
        collision.UpdateCameraClipPoints(transform.position, transform.rotation, ref collision.adjustedCameraClipPoints);
        collision.UpdateCameraClipPoints(destination, transform.rotation, ref collision.desidedCameraClipPoints);
    }


    void LateUpdate(){
    	MoveToTarget();
    	LookAtTarget();
    	collision.UpdateCameraClipPoints(transform.position, transform.rotation, ref collision.adjustedCameraClipPoints);
        collision.UpdateCameraClipPoints(destination, transform.rotation, ref collision.desidedCameraClipPoints);
        collision.CheckColliding(targetPos);
        position.adjustmentDistance = collision.GetAdjustedDistanceWithRayFrom(targetPos);
        
    }

    void GetInput(){
    	vOrbitInput = Input.GetAxisRaw(input.ORBIT_VERTICAL);
    	hOrbitInput = Input.GetAxisRaw(input.ORBIT_HORIZONTAL);
    	zoomInput = Input.GetAxisRaw(input.ZOOM);
    }

    void Update(){
    	GetInput();
    	OrbitTarget();
    	ZoomInOnTarget();
    }

    void MoveToTarget(){
    	targetPos = new Vector3(target.position.x, target.position.y, target.position.z) + position.targetPosOffset;
    	destination = Quaternion.Euler(orbit.xRotation, orbit.yRotation, 0) * -Vector3.forward * position.distanceFromTarget;
    	destination += targetPos;
    	
    	if(collision.colliding){
    		adjustedDestination = Quaternion.Euler(orbit.xRotation, orbit.yRotation, 0) * Vector3.forward * position.adjustmentDistance;
    		adjustedDestination += targetPos;
    		if(position.smoothFollow){
    			transform.position = Vector3.SmoothDamp(transform.position, adjustedDestination, ref camVel, position.smooth);
			}else{
				transform.position = adjustedDestination;
			}
		}else{
			if(position.smoothFollow){
				transform.position = Vector3.SmoothDamp(transform.position, destination, ref camVel, position.smooth);
			}else{
				transform.position = destination;
			}

		}
    }

    void LookAtTarget(){
    	Quaternion targetRotation = Quaternion.LookRotation((targetPos - transform.position));
    	transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, position.lookSmooth * Time.deltaTime);
    }

    void OrbitTarget(){
    	orbit.xRotation += vOrbitInput * orbit.vOrbitSmooth * 2f * Time.deltaTime;
    	orbit.yRotation += hOrbitInput * orbit.hOrbitSmooth * 2f * Time.deltaTime;
    	if(orbit.xRotation > orbit.maxXRotation){
    		orbit.xRotation = orbit.maxXRotation;
    	}
    	if(orbit.xRotation < orbit.minXRotation){
    		orbit.xRotation = orbit.minXRotation;
    	}
        target.rotation =  Quaternion.Euler(0, target.eulerAngles.y, target.eulerAngles.z);      
    }

    void ZoomInOnTarget(){
    	position.distanceFromTarget += zoomInput * position.zoomSmooth * Time.deltaTime;
    	if(position.distanceFromTarget > position.maxZoom){
    		position.distanceFromTarget = position.maxZoom;
    	}
    	if(position.distanceFromTarget < position.minZoom){
    		position.distanceFromTarget = position.minZoom;
    	}
    }

    [System.Serializable]
    public class CollisionHandler{

        public LayerMask collisionLayer;

        [HideInInspector]
        public bool colliding = false;
        [HideInInspector]
        public Vector3[] adjustedCameraClipPoints;
        [HideInInspector]
        public Vector3[] desidedCameraClipPoints;

        Camera camera;

        public void Initialize(Camera cam){
            camera = cam;
            adjustedCameraClipPoints = new Vector3[5];
            desidedCameraClipPoints = new Vector3[5];
        }

        public void UpdateCameraClipPoints(Vector3 cameraPosition, Quaternion atRotarion, ref Vector3[] intoArray){
            if(!camera)
                return;

            intoArray = new Vector3[5];

            float z = camera.nearClipPlane;
            float x = Mathf.Tan(camera.fieldOfView/3.41f)*z;
            float y = x / camera.aspect;

            intoArray[0] = (atRotarion * new Vector3(-x,y,z))+cameraPosition;
            intoArray[1] = (atRotarion * new Vector3(x,y,z))+cameraPosition;
            intoArray[2] = (atRotarion * new Vector3(-x,-y,z))+cameraPosition;
            intoArray[3] = (atRotarion * new Vector3(x,-y,z))+cameraPosition;
            intoArray[4] = cameraPosition - camera.transform.forward;
        }

        bool CollisionDetectedAtClipPoints(Vector3[] clipPoints, Vector3 fromPosition){
            for(int i = 0; i < clipPoints.Length;i++){
                Ray ray = new Ray(fromPosition, clipPoints[i] - fromPosition);
                float distance = Vector3.Distance(clipPoints[i],fromPosition);
                if(Physics.Raycast(ray, distance, collisionLayer)){
                    return true;
                }
            }
            return false;

        }

        public float GetAdjustedDistanceWithRayFrom(Vector3 from){
            float distance  = -1;

            for(int i=0;i< desidedCameraClipPoints.Length;i++){
                Ray ray = new Ray(from, desidedCameraClipPoints[i] - from);
                RaycastHit hit;
                if(Physics.Raycast(ray, out hit)){
                    if(distance == -1){
                        distance = hit.distance;
                    }else{
                        if(hit.distance < distance){
                            distance = hit.distance;
                        }
                    }
                }
            }
            if(distance == -1){
                return 0;
            }else{
                return distance;
            }

        }

        public void CheckColliding(Vector3 targetPosition){
            if(CollisionDetectedAtClipPoints(desidedCameraClipPoints,targetPosition)){
                colliding = true;
            }else{
                colliding = false;
            }
        }

    }

	[System.Serializable]
	public class PositionSettings{
		[HideInInspector]
		public Vector3 targetPosOffset = new Vector3(0, 15f, 0);
		[HideInInspector]
		public float lookSmooth = 100f;
		[HideInInspector]
		public float distanceFromTarget = -2;
		[HideInInspector]
		public float zoomSmooth = 1000f;
		public float maxZoom = -9;
		public float minZoom = -20;
		[HideInInspector]
		public bool smoothFollow = true;
		[HideInInspector]
		public float smooth = 0.05f;

		public void applyScale(float scale){
			targetPosOffset *= scale;
			minZoom *= scale;
			maxZoom *= scale;
			distanceFromTarget *= scale;
		}

		[HideInInspector]
		public float newDistance = -2;
		[HideInInspector]
		public float adjustmentDistance = -2;
	}

	[System.Serializable]
	public class OrbitSettings{
		[HideInInspector]
		public float xRotation = -20;
		[HideInInspector]
		public float yRotation = -180;
		public float maxXRotation = 60;
		public float minXRotation = -60;
		[HideInInspector]
		public float vOrbitSmooth = 150;
		[HideInInspector]
		public float hOrbitSmooth = 150;
	}

	[System.Serializable]
	public class InputSettings{
		[HideInInspector]
		public string ORBIT_HORIZONTAL = "Mouse X";
		[HideInInspector]
		public string ORBIT_VERTICAL = "Mouse Y";
		[HideInInspector]
		public string ZOOM = "Mouse ScrollWheel";
	}

}
