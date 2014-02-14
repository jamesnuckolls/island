#pragma strict

private var doorIsOpen : boolean = false;
private var doorTimer : float = 0.0;
private var currentDoor : GameObject;

var coconutCollectSound : AudioClip;

var doorOpenTime : float = 3.0;
var doorOpenSound : AudioClip;
var doorShutSound : AudioClip;
var batteryCollectSound : AudioClip;

function Start () {

}

function Update () {

    var hit : RaycastHit;
    if(Physics.Raycast (transform.position, transform.forward, hit,5)) {
        if(hit.collider.gameObject.tag == "outpostDoor" && !doorIsOpen && BatteryCollect.charge >= 4) {
            currentDoor = hit.collider.gameObject;
            Door(doorOpenSound, true, "dooropen", currentDoor);
            GameObject.Find("batteryGUI").GetComponent(GUITexture).enabled = false;
            
        } else if(hit.collider.gameObject.tag == "outpostDoor" && doorIsOpen == false && BatteryCollect.charge < 4) {
        	GameObject.Find("batteryGUI").GetComponent(GUITexture).enabled = true;
        	TextHints.message = "The door seems to need more power...";
        	TextHints.textOn = true;
        }
    }
    
    if(doorIsOpen) {
        doorTimer += Time.deltaTime;
        
        if(doorTimer > doorOpenTime) {
            Door(doorShutSound, false, "doorshut", currentDoor);
            doorTimer = 0.0;
        }
    }
}


function OnControllerColliderHit(hit : ControllerColliderHit) {
    if(hit.gameObject.name == "coconutItem") {
    	audio.PlayOneShot(coconutCollectSound);
    
    	CoconutThrow.canThrow = true;
    	Destroy(hit.gameObject);
    	
    	TextHints.message = "Press left mouse to throw your new coconut!";
    	TextHints.textOn = true;
    }
    
    if(hit.gameObject.name == "mat"  && !CoconutWin.haveWon) {
    	TextHints.message = "Perhaps knocking down these targets is important...";
    	TextHints.textOn = true;
    }
}


function Door(aClip : AudioClip, openCheck : boolean, animName : String, thisDoor : GameObject) {
    audio.PlayOneShot(aClip);
    doorIsOpen = openCheck;
    
    thisDoor.transform.parent.animation.Play(animName);
}

function OnTriggerEnter(collisionInfo : Collider) {
	if(collisionInfo.gameObject.tag == "battery") {
		BatteryCollect.charge++;
		audio.PlayOneShot(batteryCollectSound);
		Destroy(collisionInfo.gameObject);
	}
	
}

@script RequireComponent(AudioSource)