using UnityEngine;

public class InputTypeManager : Singleton<InputTypeManager>
{
	private void Start() => Input.simulateMouseWithTouches = false;

	private void Update()
	{
		if (Input.touchCount > 0 && !usingTouch)
		{
			usingTouch = true;
		}
		else if (Input.anyKeyDown)
		{
			usingTouch = false;
		}
	}

	public static bool usingTouch;
}
