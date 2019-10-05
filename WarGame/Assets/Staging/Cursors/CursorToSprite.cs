using UnityEngine;
using UnityEngine.UI;

public class CursorToSprite : MonoBehaviour
{
	
    public Image handCursor, normalCursor;
    public Sprite handSprite, normalSprite;
    public float speed;
	
	void Start()
    {
		Cursor.visible = false;
	}
	
	void Update ()

    {
		handCursor.transform.position = Input.mousePosition;
        if (Cursor.visible)
            Cursor.visible = false;

        if (Input.GetButtonDown ("Fire1"))
            handCursor.sprite = normalSprite;
        if (Input.GetButtonUp ("Fire1"))
            handCursor.sprite = handSprite;
            
        

        //if (Input.GetKey(KeyCode.A))
           // handCursor.transform.RotateAround(Input.mousePosition, new Vector3(0, 0.2f, 1), speed * Time.deltaTime);
        //if (Input.GetKey(KeyCode.D))
           // handCursor.transform.RotateAround(Input.mousePosition, new Vector3(0, 0.2f, 1), -speed * Time.deltaTime);
	}
}