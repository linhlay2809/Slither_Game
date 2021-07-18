using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Advertisements;

public class BackMenu: MonoBehaviour {
	public void back(){

		SceneManager.LoadScene ("Loading");
	}

}
