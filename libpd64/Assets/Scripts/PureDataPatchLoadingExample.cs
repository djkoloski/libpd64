using UnityEngine;
using System.Collections;
using Magicolo;

public class PureDataPatchLoadingExample : MonoBehaviour
{
	void Start()
	{
		// Opening the patch will connect it up to the DSP
		PureData.OpenPatch("samplePatch");
	}

	void OnGUI()
	{
		GUILayout.Label("Use the number keys 0-9 to change the pitch of the note");

		KeyCode[] keyCodes = {
			KeyCode.Alpha1,
			KeyCode.Alpha2,
			KeyCode.Alpha3,
			KeyCode.Alpha4,
			KeyCode.Alpha5,
			KeyCode.Alpha6,
			KeyCode.Alpha7,
			KeyCode.Alpha8,
			KeyCode.Alpha9,
			KeyCode.Alpha0
		};
		for (int i = 0; i < keyCodes.Length; ++i)
		{
			if (Input.GetKeyDown(keyCodes[i]))
			{
				// Our patch listens for the sample_event message and plays the corresponding note
				PureData.Send<float>("sample_event", i);
			}
		}
	}
}
