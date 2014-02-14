#pragma strict

var hitSound : AudioClip;

function Start () {

}

function Update () {

}



function OnCollisionEnter(theObject : Collision)
{
	if(theObject.gameObject.name == "coconut")
	{
		audio.PlayOneShot(hitSound);
	}
	else
	{
		TextHints.message = theObject.gameObject.name;
		TextHints.textOn = true;
	}
}

@script RequireComponent(AudioSource)