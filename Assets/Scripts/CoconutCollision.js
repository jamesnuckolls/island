#pragma strict

var targetRoot : GameObject;
var hitSound : AudioClip;
var resetSound : AudioClip;

private var beenHit : boolean = false;
private var timer : float = 0.0;

function Start () {

}

function Update () {
	if(beenHit) {
		timer += Time.deltaTime;
	}

	if(timer > 2.2) {
		audio.PlayOneShot(resetSound);
		targetRoot.animation.Play("up");
		beenHit = false;
		CoconutWin.targets--;
		timer = 0.0;
	}
}

function OnCollisionEnter(theObject : Collision) {
	if(!beenHit && theObject.gameObject.name == "coconut") {
		audio.PlayOneShot(hitSound);
		targetRoot.animation.Play("down");
		beenHit = true;
		CoconutWin.targets++;
		
		if(!CoconutWin.haveWon) {		
			TextHints.message = CoconutWin.targets.ToString();
			TextHints.textOn = true;
		}
		
	}
}

@script RequireComponent(AudioSource)