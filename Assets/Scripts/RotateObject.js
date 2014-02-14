#pragma strict

var rotationAmount : float = 5.0;

function Start () {

}

function Update () {
	transform.Rotate(Vector3(0, rotationAmount, 0));
}