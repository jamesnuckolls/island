#pragma strict

static var targets : int = 0;
static var haveWon : boolean = false;
var winSound : AudioClip;
var battery : GameObject;

function Start () {

}

function Update () {
	if(targets == 3 && !haveWon) {
		haveWon = true;
		audio.PlayOneShot(winSound);
		Instantiate(battery, Vector3(transform.position.x,
			transform.position.y + 2,
			transform.position.z),
			transform.rotation );
	}
}

@script RequireComponent(AudioSource)