#pragma strict

var throwSound : AudioClip;
var coconutObject : Rigidbody;
var throwForce : float = 0;

static var canThrow = false;

function Start () {

}

function Update () {
	if(Input.GetButtonUp("Fire1") && canThrow) {
		audio.PlayOneShot(throwSound);
		var newCoconut : Rigidbody = Instantiate(coconutObject, transform.position, transform.rotation);
		newCoconut.rigidbody.velocity = transform.TransformDirection(Vector3(0,0,throwForce));
		Physics.IgnoreCollision(transform.root.collider, newCoconut.collider, true);
		newCoconut.name = "coconut";
	}
}

@script RequireComponent(AudioSource)